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
            throw new ArgumentException("Нужен HTTPS-адрес сервера; для localhost разрешён HTTP.");
        return uri.GetLeftPart(UriPartial.Authority);
    }
    public static BoardConnection ParseLink(string link)
    {
        if (!Uri.TryCreate(link.Trim(), UriKind.Absolute, out var uri)) throw new ArgumentException("Вставьте ссылку на полотно.");
        var values = uri.Fragment.TrimStart('#').Split('&').Select(part => part.Split('=', 2))
            .Where(part => part.Length == 2).ToDictionary(part => part[0], part => Uri.UnescapeDataString(part[1]));
        if (!values.TryGetValue("board", out var id) || !Guid.TryParse(id, out var boardId) ||
            !values.TryGetValue("key", out var key) || key.Length != 43 || !key.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            throw new ArgumentException("В ссылке отсутствует номер полотна или ключ доступа.");
        return new(Server(uri.GetLeftPart(UriPartial.Authority)), boardId, key);
    }
    public static string Link(BoardConnection connection) => $"{Server(connection.Server)}/#board={connection.Id}&key={connection.Key}";
    private static void Ensure(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        throw new HttpRequestException(response.StatusCode switch
        {
            HttpStatusCode.Forbidden => "По этой ссылке разрешён только просмотр.",
            HttpStatusCode.NotFound => "Полотно недоступно: проверьте ссылку.",
            HttpStatusCode.RequestEntityTooLarge => "Полотно слишком большое.",
            _ => "Сервер полотна временно недоступен."
        }, null, response.StatusCode);
    }
    public void Dispose() => _http.Dispose();
}
