using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlurMask.Shared;

namespace BlurMask;

internal sealed class CanvasCloudClient : IDisposable
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(8) };
    public static string DefaultServer => "https://pen.kiiko.ru";

    public async Task<BoardAccess> CreateAsync(string server, BoardSnapshot board)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Server(server)}/api/boards")
        {
            Content = new StringContent(JsonSerializer.Serialize(new CreateBoardRequest(board.Title, board.Background),
                BoardJsonContext.Default.CreateBoardRequest), Encoding.UTF8, "application/json")
        };
        using var response = await _http.SendAsync(request);
        Ensure(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        return (await JsonSerializer.DeserializeAsync(stream, BoardJsonContext.Default.BoardAccess))!;
    }
    public async Task<BoardSnapshot> ReadAsync(BoardConnection connection)
    {
        using var request = Request(connection, HttpMethod.Get, "");
        using var response = await _http.SendAsync(request);
        Ensure(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        return (await JsonSerializer.DeserializeAsync(stream, BoardJsonContext.Default.BoardSnapshot))!;
    }
    public async Task<BoardSnapshot> ApplyAsync(BoardConnection connection, BoardPatch patch)
    {
        using var request = Request(connection, HttpMethod.Post, "/changes");
        request.Content = new StringContent(JsonSerializer.Serialize(patch, BoardJsonContext.Default.BoardPatch), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request);
        Ensure(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        return (await JsonSerializer.DeserializeAsync(stream, BoardJsonContext.Default.BoardSnapshot))!;
    }
    private static HttpRequestMessage Request(BoardConnection connection, HttpMethod method, string suffix)
    {
        var request = new HttpRequestMessage(method, $"{Server(connection.Server)}/api/boards/{connection.Id}{suffix}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.Key);
        return request;
    }
    public static string Server(string server)
    {
        if (!Uri.TryCreate(server.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException(UiText.T("Нужен HTTPS-адрес сервера; для localhost разрешён HTTP."));
        return uri.GetLeftPart(UriPartial.Authority);
    }
    public static BoardConnection ParseLink(string link)
    {
        link = link.Trim();
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            var invite = ParseInvite(link);
            return new(DefaultServer, invite.Id, invite.Key);
        }
        var server = Server(uri.GetLeftPart(UriPartial.Authority));
        if (server != DefaultServer && !uri.IsLoopback)
            throw new ArgumentException(UiText.T("Приглашение должно быть с pen.kiiko.ru."));
        var invitation = ParseInvite(uri.Fragment.TrimStart('#'));
        return new(server, invitation.Id, invitation.Key);
    }
    public static string Link(BoardConnection connection) => $"{Server(connection.Server)}/#{BoardInvite.Code(connection.Id, connection.Key)}";
    private static (Guid Id, string Key) ParseInvite(string code)
    {
        try { return BoardInvite.Parse(code); }
        catch (ArgumentException) { throw new ArgumentException(UiText.T("Вставьте корректный код приглашения или ссылку.")); }
    }
    private static void Ensure(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        throw new HttpRequestException(response.StatusCode switch
        {
            HttpStatusCode.Forbidden => UiText.T("По этой ссылке разрешён только просмотр."),
            HttpStatusCode.NotFound => UiText.T("Полотно недоступно: проверьте ссылку."),
            HttpStatusCode.RequestEntityTooLarge => UiText.T("Полотно слишком большое."),
            _ => UiText.T("Сервер полотна временно недоступен.")
        }, null, response.StatusCode);
    }
    public void Dispose() => _http.Dispose();
}
