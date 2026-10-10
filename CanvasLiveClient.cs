using System.Net.WebSockets;
using System.Text.Json;
using BlurMask.Shared;

namespace BlurMask;

// Uses the framework WebSocket client and source-generated JSON (Native AOT).
internal sealed class CanvasLiveClient : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly object _gate = new();
    private InkMarkDto? _desired;
    private bool _clear;
    private long _version;
    private readonly Action<LiveFrame?> _received;
    internal bool Connected { get; private set; }
    public CanvasLiveClient(BoardConnection connection, Action<LiveFrame?> received)
    {
        _received = received;
        _ = RunAsync(connection, _stop.Token);
    }
    public void Preview(InkMarkDto? mark, bool canceled = false)
    {
        lock (_gate) { _desired = mark; _clear |= canceled; _version++; }
    }
    private async Task RunAsync(BoardConnection connection, CancellationToken ct)
    {
        var retry = 1;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var socket = new ClientWebSocket();
                socket.Options.SetRequestHeader("Authorization", "Bearer " + connection.Key);
                socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                socket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(10);
                var server = new Uri(CanvasCloudClient.Server(connection.Server));
                var uri = new UriBuilder(server) { Scheme = server.Scheme == "https" ? "wss" : "ws", Path = $"/api/boards/{connection.Id}/live" }.Uri;
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
                { timeout.CancelAfter(TimeSpan.FromSeconds(8)); await socket.ConnectAsync(uri, timeout.Token); }
                Connected = true; retry = 1;
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var sending = SendAsync(socket, linked.Token);
                var receiving = ReceiveAsync(socket, linked.Token);
                await Task.WhenAny(sending, receiving);
                linked.Cancel(); socket.Abort();
                try { await Task.WhenAll(sending, receiving); }
                catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or JsonException or InvalidDataException) { }
            }
            catch (Exception ex) when (ex is WebSocketException or HttpRequestException or OperationCanceledException) { }
            finally { Connected = false; _received(null); }
            try { await Task.Delay(TimeSpan.FromSeconds(retry), ct); }
            catch (OperationCanceledException) { break; }
            retry = Math.Min(10, retry * 2);
        }
    }
    private async Task SendAsync(ClientWebSocket socket, CancellationToken ct)
    {
        InkMarkDto? sent = null;
        long seen = -1;
        var touched = DateTime.UtcNow;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        while (await timer.WaitForNextTickAsync(ct))
        {
            InkMarkDto? desired; bool clear; long version;
            lock (_gate) { desired = _desired; clear = _clear; _clear = false; version = _version; }
            if (clear) { await SendFrameAsync(socket, new() { Type = "clear" }, ct); sent = null; }
            if (desired is null) { sent = null; seen = version; continue; }
            if (version != seen)
            {
                if (sent is null || sent.Id != desired.Id || !BoardValidation.Same(sent, desired))
                {
                    var offset = sent?.Id == desired.Id && desired.Tool is ("Pen" or "Highlighter") && desired.Points.Count >= sent.Points.Count
                        ? sent.Points.Count : 0;
                    foreach (var frame in LiveProtocol.Chunks(desired, offset)) await SendFrameAsync(socket, frame, ct);
                    sent = desired; touched = DateTime.UtcNow;
                }
                seen = version;
            }
            if (DateTime.UtcNow - touched > TimeSpan.FromSeconds(2))
            { await SendFrameAsync(socket, new() { Type = "touch" }, ct); touched = DateTime.UtcNow; }
        }
    }
    private static async Task SendFrameAsync(ClientWebSocket socket, LiveFrame frame, CancellationToken ct)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(frame, BoardJsonContext.Default.LiveFrame);
        if (bytes.Length >= LiveProtocol.MaxPacketBytes) throw new InvalidDataException("Live packet limit exceeded.");
        await socket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, true, ct);
    }
    private async Task ReceiveAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[LiveProtocol.MaxPacketBytes];
        while (!ct.IsCancellationRequested)
        {
            var length = 0; ValueWebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer.AsMemory(length), ct);
                if (result.MessageType == WebSocketMessageType.Close) return;
                if (result.MessageType != WebSocketMessageType.Text || length + result.Count >= buffer.Length)
                    throw new InvalidDataException("Invalid live packet.");
                length += result.Count;
            } while (!result.EndOfMessage);
            var frame = JsonSerializer.Deserialize(buffer.AsSpan(0, length), BoardJsonContext.Default.LiveFrame);
            if (frame is not null) _received(frame);
        }
    }
    public void Dispose() => _stop.Cancel();
}
