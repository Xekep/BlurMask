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
var liveLimits = new LiveLimits();
builder.Configuration.GetSection("Live").Bind(liveLimits); liveLimits.Validate();
builder.Services.AddSingleton(liveLimits);
builder.Services.AddSingleton(NpgsqlDataSource.Create(connection));
builder.Services.AddSingleton<BoardStore>();
builder.Services.AddSingleton<BoardLiveHub>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<BoardLiveHub>());
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, BoardJsonContext.Default));
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = BoardValidation.MaxDocumentBytes);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        context.GetEndpoint()?.Metadata.GetMetadata<BoardLiveEndpoint>() is not null
            ? RateLimitPartition.GetConcurrencyLimiter("live", _ => new ConcurrencyLimiterOptions
                { PermitLimit = liveLimits.MaxConnections, QueueLimit = 0 }) :
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
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; connect-src 'self' https://api.github.com; frame-ancestors 'none'; base-uri 'none'; object-src 'none'";
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
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(10), KeepAliveTimeout = TimeSpan.FromSeconds(10) });
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
app.UseStaticFiles(new StaticFileOptions
{
    // Revalidate mutable page assets instead of keeping an old download list.
    // ETag/Last-Modified still allow lightweight 304 responses.
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-cache"
});
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
api.MapGet("/{id:guid}", async (Guid id, HttpContext context, BoardStore store, CancellationToken ct) =>
{
    var tag = context.Request.Headers.IfNoneMatch.ToString();
    // GET uses weak comparison: compression proxies can prefix our revision tag with W/.
    if (tag.StartsWith("W/", StringComparison.Ordinal)) tag = tag[2..];
    var prefix = $"\"{id:N}-";
    var readOnly = tag.EndsWith("-v\"", StringComparison.Ordinal);
    long? known = tag.StartsWith(prefix, StringComparison.Ordinal) && tag.Length > prefix.Length + 3 &&
        (readOnly || tag.EndsWith("-e\"", StringComparison.Ordinal)) &&
        long.TryParse(tag.AsSpan(prefix.Length, tag.Length - prefix.Length - 3), System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out var revision) && revision >= 0 ? revision : null;
    var result = await store.ReadChangedAsync(id, Key(context.Request), known, ct, readOnly);
    if (result.Revision is not { } current) return Results.NotFound();
    context.Response.Headers.ETag = $"\"{id:N}-{current}-{(result.ReadOnly ? "v" : "e")}\"";
    return result.Board is null ? Results.StatusCode(304) : Results.Ok(result.Board);
}).WithMetadata(new BoardKeyRequired());
api.MapGet("/{id:guid}/live", async (Guid id, HttpContext context, BoardStore store, BoardLiveHub live) =>
    await live.HandleAsync(id, context, store, Key(context.Request)))
    .WithMetadata(new BoardKeyRequired(), new BoardLiveEndpoint());
api.MapPost("/{id:guid}/changes", async (Guid id, BoardPatch patch, HttpRequest request, BoardStore store, BoardLiveHub live, CancellationToken ct) =>
{
    if (!BoardValidation.Valid(patch)) return Results.BadRequest(new { error = "Некорректный рисунок." });
    var result = await store.UpdateAsync(id, Key(request), patch, ct);
    if (result.ReadOnly) return Results.StatusCode(403);
    if (result.Board is not null && result.Change is not null) live.Changed(result.Board, result.Change);
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
sealed class BoardLiveEndpoint;
