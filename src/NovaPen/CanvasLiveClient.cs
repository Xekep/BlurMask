using System.Net.WebSockets;
using System.Text.Json;
using NovaPen.Shared;

namespace NovaPen;

// Uses the framework WebSocket client and source-generated JSON (Native AOT).
internal sealed class CanvasLiveClient : IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private readonly object _gate = new();
    private readonly SemaphoreSlim _wake = new(0, 1);
    private int _disposed;
    private InkMarkDto? _desired;
    private bool _clear;
    private readonly Action<LiveFrame?> _received;
    internal bool Connected { get; private set; }
    public CanvasLiveClient(BoardConnection connection, Action<LiveFrame?> received)
    {
        _received = received;
        _ = Task.Run(async () =>
        {
            try { await RunAsync(connection, _stop.Token); }
            finally { lock (_gate) { _disposed = 1; _stop.Dispose(); _wake.Dispose(); } }
        });
    }
    public void Preview(InkMarkDto? mark, bool canceled = false)
    {
        lock (_gate)
        {
            if (_disposed != 0) return;
            _desired = mark is null ? null : mark with { Points = new(mark.Points) }; _clear |= canceled;
            if (_wake.CurrentCount == 0) _wake.Release();
        }
    }
    public void PreviewInk(InkMark mark)
    {
        lock (_gate)
        {
            if (_disposed != 0) return;
            _desired = LivePreviewCapture.Capture(mark, _desired);
            if (_wake.CurrentCount == 0) _wake.Release();
        }
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
                socket.Options.AddSubProtocol(LiveWire.Protocol);
                socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                socket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(10);
                var server = new Uri(CanvasCloudClient.Server(connection.Server));
                var uri = new UriBuilder(server) { Scheme = server.Scheme == "https" ? "wss" : "ws", Path = $"/api/boards/{connection.Id}/live" }.Uri;
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
                { timeout.CancelAfter(TimeSpan.FromSeconds(8)); await socket.ConnectAsync(uri, timeout.Token); }
                if (socket.SubProtocol != LiveWire.Protocol) throw new InvalidDataException("Unsupported live protocol.");
                Connected = true; retry = 1;
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var sending = SendAsync(socket, linked.Token);
                var receiving = ReceiveAsync(socket, linked.Token);
                await Task.WhenAny(sending, receiving);
                linked.Cancel(); socket.Abort();
                try { await Task.WhenAll(sending, receiving); }
                catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or JsonException or InvalidDataException) { }
            }
            catch (Exception ex) when (ex is WebSocketException or HttpRequestException or OperationCanceledException or InvalidDataException) { }
            finally { Connected = false; _received(null); }
            try { await Task.Delay(TimeSpan.FromSeconds(retry), ct); }
            catch (OperationCanceledException) { break; }
            retry = Math.Min(10, retry * 2);
        }
    }
    private async Task SendAsync(ClientWebSocket socket, CancellationToken ct)
    {
        InkMarkDto? sent = null;
        var sentPoints = 0;
        var touched = DateTime.UtcNow;
        while (!ct.IsCancellationRequested)
        {
            bool active, clear;
            LiveFrame[] frames = [];
            lock (_gate)
            {
                clear = _clear; _clear = false;
                if (clear) { sent = null; sentPoints = 0; }
                var desired = _desired; active = desired is not null;
                if (desired is null) { sent = null; sentPoints = 0; }
                else if (desired.Tool is "Pen" or "Highlighter")
                {
                    var offset = sent?.Id == desired.Id && sent.Tool == desired.Tool && sent.Color == desired.Color && sent.Width == desired.Width &&
                        sent.FontSize == desired.FontSize && sent.Font == desired.Font && sent.Text == desired.Text && sent.Transform == desired.Transform && desired.Points.Count >= sentPoints ? sentPoints : 0;
                    if (offset < desired.Points.Count)
                    { frames = LiveProtocol.Chunks(desired, offset).ToArray(); sent = desired with { Points = [] }; sentPoints = desired.Points.Count; }
                }
                else if (sent?.Id != desired.Id || !BoardValidation.Same(sent, desired))
                { frames = LiveProtocol.Chunks(desired).ToArray(); sent = desired; sentPoints = desired.Points.Count; }
            }
            if (clear) await SendFrameAsync(socket, new() { Type = "clear" }, ct);
            foreach (var frame in frames) await SendFrameAsync(socket, frame, ct);
            if (frames.Length > 0) touched = DateTime.UtcNow;
            if (active && DateTime.UtcNow - touched > TimeSpan.FromSeconds(2))
            { await SendFrameAsync(socket, new() { Type = "touch" }, ct); touched = DateTime.UtcNow; }
            if (active) await Task.Delay(50, ct);
            else await _wake.WaitAsync(ct);
        }
    }
    private static async Task SendFrameAsync(ClientWebSocket socket, LiveFrame frame, CancellationToken ct)
    {
        var compact = LiveWire.IsDelta(frame);
        var bytes = compact ? LiveWire.Encode(frame) : JsonSerializer.SerializeToUtf8Bytes(frame, BoardJsonContext.Default.LiveFrame);
        if (bytes.Length >= LiveProtocol.MaxPacketBytes) throw new InvalidDataException("Live packet limit exceeded.");
        await socket.SendAsync(bytes.AsMemory(), compact ? WebSocketMessageType.Binary : WebSocketMessageType.Text, true, ct);
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
                if (result.MessageType is not (WebSocketMessageType.Text or WebSocketMessageType.Binary) || length + result.Count >= buffer.Length)
                    throw new InvalidDataException("Invalid live packet.");
                length += result.Count;
            } while (!result.EndOfMessage);
            var frame = result.MessageType == WebSocketMessageType.Binary ? LiveWire.Decode(buffer.AsSpan(0, length)) :
                JsonSerializer.Deserialize(buffer.AsSpan(0, length), BoardJsonContext.Default.LiveFrame);
            if (frame is not null) _received(frame);
        }
    }
    public void Dispose()
    {
        lock (_gate) if (Interlocked.Exchange(ref _disposed, 1) == 0) _stop.Cancel();
    }
}
