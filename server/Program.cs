using System.Threading.RateLimiting;
using System.Net;
using BlurMask.Server;
using BlurMask.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Postgres to a PostgreSQL connection string.");
var publicUrl = builder.Configuration["PublicUrl"] ?? "https://pen.kiiko.ru";
builder.Services.AddSingleton(NpgsqlDataSource.Create(connection));
builder.Services.AddSingleton<BoardStore>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, BoardJsonContext.Default));
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = BoardValidation.MaxDocumentBytes);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) || context.Request.Path == "/healthz"
            ? RateLimitPartition.GetConcurrencyLimiter("backend", _ => new ConcurrencyLimiterOptions
                { PermitLimit = 8, QueueLimit = 0 })
            : RateLimitPartition.GetNoLimiter("static"));
    options.AddPolicy("create", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
    options.AddPolicy("api", context => RateLimitPartition.GetTokenBucketLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new TokenBucketRateLimiterOptions { TokenLimit = 100, TokensPerPeriod = 50, ReplenishmentPeriod = TimeSpan.FromSeconds(1), QueueLimit = 0 }));
});
var app = builder.Build();
// The Docker reverse proxy is the only trusted forwarding peer (loopback in the
// host deployment). Do not trust arbitrary X-Forwarded-* values from the internet.
var forwarding = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto };
if (IPAddress.TryParse(builder.Configuration["TrustedProxy"], out var trustedProxy)) forwarding.KnownProxies.Add(trustedProxy);
app.UseForwardedHeaders(forwarding);
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; object-src 'none'";
    if (context.Request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
    try { await next(context); }
    catch (BoardLimitException) { context.Response.StatusCode = 413; await context.Response.WriteAsJsonAsync(new { error = "Полотно слишком большое." }); }
    catch (NpgsqlException ex)
    {
        app.Logger.LogError(ex, "PostgreSQL request failed");
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { error = "Хранилище временно недоступно." });
    }
});
app.UseRouting();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    // Reject malformed/missing credentials before JSON binding and database work.
    if (context.GetEndpoint()?.Metadata.GetMetadata<BoardKeyRequired>() is not null && !BoardStore.ValidKey(Key(context.Request)))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next(context);
});
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/healthz", async (BoardStore store, CancellationToken ct) =>
    await store.PingAsync(ct) ? Results.Ok(new { status = "ok" }) : Results.StatusCode(503));
var api = app.MapGroup("/api/boards").RequireRateLimiting("api");
api.MapPost("", async (CreateBoardRequest request, BoardStore store, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 120 || request.Background is not ("white" or "black"))
        return Results.BadRequest(new { error = "Проверьте название и фон полотна." });
    var access = await store.CreateAsync(request, publicUrl, ct);
    return Results.Created($"/api/boards/{access.Board.Id}", access);
}).RequireRateLimiting("create");
api.MapGet("/{id:guid}", async (Guid id, HttpRequest request, BoardStore store, CancellationToken ct) =>
{
    var board = await store.ReadAsync(id, Key(request), ct);
    return board is null ? Results.NotFound() : Results.Ok(board);
}).WithMetadata(new BoardKeyRequired());
api.MapPost("/{id:guid}/changes", async (Guid id, BoardPatch patch, HttpRequest request, BoardStore store, CancellationToken ct) =>
{
    if (!BoardValidation.Valid(patch)) return Results.BadRequest(new { error = "Некорректный рисунок." });
    var result = await store.UpdateAsync(id, Key(request), patch, ct);
    if (result.ReadOnly) return Results.StatusCode(403);
    return result.Board is null ? Results.NotFound() : Results.Ok(result.Board);
}).WithMetadata(new BoardKeyRequired());
await app.Services.GetRequiredService<BoardStore>().InitializeAsync();
await app.RunAsync();
static string Key(HttpRequest request)
{
    var header = request.Headers.Authorization.ToString();
    return header.StartsWith("Bearer ", StringComparison.Ordinal) ? header[7..] : "";
}
sealed class BoardKeyRequired;
