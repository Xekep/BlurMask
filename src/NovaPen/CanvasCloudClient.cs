using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NovaPen.Shared;

namespace NovaPen;

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
        => (await ReadChangedAsync(connection, null))!;

    public async Task<BoardSnapshot?> ReadChangedAsync(BoardConnection connection, long? revision, bool readOnly = false)
    {
        using var request = Request(connection, HttpMethod.Get, "");
        if (revision is { } known) request.Headers.IfNoneMatch.Add(new($"\"{connection.Id:N}-{known}-{(readOnly ? "v" : "e")}\""));
        using var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotModified) return null;
        Ensure(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        return (await JsonSerializer.DeserializeAsync(stream, BoardJsonContext.Default.BoardSnapshot))!;
    }
    public async Task<BoardSnapshot> ApplyAsync(BoardConnection connection, BoardPatch patch, BoardSnapshot? baseSnapshot = null)
    {
        using var request = Request(connection, HttpMethod.Post, "/changes");
        if (baseSnapshot is { ReadOnly: false, Revision: >= 0 } && baseSnapshot.Id == connection.Id)
            request.Headers.Add(BoardWriteReceipt.BaseRevisionHeader, baseSnapshot.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Content = new StringContent(JsonSerializer.Serialize(patch, BoardJsonContext.Default.BoardPatch), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request);
        Ensure(response);
        await using var stream = await response.Content.ReadAsStreamAsync();
        if (response.Content.Headers.ContentType?.MediaType == BoardWriteReceipt.MediaType)
        {
            try
            {
                var receipt = await JsonSerializer.DeserializeAsync(stream, BoardWriteReceiptJsonContext.Default.BoardWriteReceipt);
                if (baseSnapshot is not null && receipt is not null && receipt.TryApply(baseSnapshot, patch.OperationId, out var acknowledged))
                    return acknowledged!;
            }
            catch (JsonException) { }
            // Do not re-post a committed write when an acknowledgement cannot be
            // reconstructed. Read the current authorized snapshot instead.
            return await ReadAsync(connection);
        }
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
