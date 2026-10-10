using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using BlurMask.Shared;

namespace BlurMask.Server;

// One process owns the transient channel; PostgreSQL remains the durable source.
public sealed class BoardLiveHub(LiveLimits limits) : BackgroundService
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Room> _rooms = [];
    private readonly Dictionary<string, int> _addresses = [];
    private int _previewPoints;
    private sealed class Room
    {
        public readonly object Gate = new();
        public readonly List<Peer> Peers = [];
        public HashSet<Guid> Committed = [];
        public long Revision;
        public int PreviewPoints;
    }
    private sealed class Peer(string address)
    {
        public readonly Guid Id = Guid.NewGuid();
        public readonly string Address = address;
        public readonly Channel<byte[]> Outgoing = Channel.CreateBounded<byte[]>(64);
        public readonly CancellationTokenSource Stop = new();
        public InkMarkDto? Preview;
        public long Touched = Stopwatch.GetTimestamp();
        public int QueuedBytes;
        public void Enqueue(byte[] bytes)
        {
            // Run cancellation callbacks outside room locks (registry -> room is the lock order).
            if (Interlocked.Add(ref QueuedBytes, bytes.Length) > 128 * 1024 || !Outgoing.Writer.TryWrite(bytes)) _ = Stop.CancelAsync();
        }
    }
    private static byte[] Encode(LiveFrame frame) => JsonSerializer.SerializeToUtf8Bytes(frame, BoardJsonContext.Default.LiveFrame);
    private bool ReservePoints(int delta)
    {
        while (true)
        {
            var previous = Volatile.Read(ref _previewPoints);
            if (previous + delta > limits.MaxPreviewPoints) return false;
            if (Interlocked.CompareExchange(ref _previewPoints, previous + delta, previous) == previous) return true;
        }
    }
    private void ClearPreview(Room room, Peer peer)
    {
        if (peer.Preview is not { } preview) return;
        room.PreviewPoints -= preview.Points.Count;
        Interlocked.Add(ref _previewPoints, -preview.Points.Count); peer.Preview = null;
    }
    private static void Broadcast(Room room, LiveFrame frame, Peer? except = null)
    {
        var bytes = Encode(frame);
        foreach (var peer in room.Peers) if (peer != except) peer.Enqueue(bytes);
    }
    public void Changed(BoardSnapshot board, BoardPatch patch)
    {
        Room? room;
        lock (_gate) _rooms.TryGetValue(board.Id, out room);
        if (room is null) return;
        lock (room.Gate)
        {
            if (board.Revision < room.Revision) return;
            room.Revision = board.Revision;
            room.Committed = board.Marks.Select(m => m.Id).ToHashSet();
            foreach (var peer in room.Peers)
                if (peer.Preview is { } preview && room.Committed.Contains(preview.Id))
                {
                    ClearPreview(room, peer);
                    Broadcast(room, new() { Type = "clear", Peer = peer.Id, Revision = board.Revision });
                }
            var notification = new LiveFrame { Type = "changed", Revision = board.Revision, BaseRevision = board.Revision - 1, Patch = patch };
            if (patch.Add.Concat(patch.Update).Any(m => m.Image is not null) ||
                patch.Add.Concat(patch.Update).Sum(m => m.Points.Count + m.Text.Length) > 1024) notification = notification with { Patch = null };
            var bytes = Encode(notification);
            // Large image/batch changes use the existing authenticated HTTP fallback.
            if (bytes.Length >= LiveProtocol.MaxPacketBytes) bytes = Encode(notification with { Patch = null });
            foreach (var peer in room.Peers) peer.Enqueue(bytes);
        }
    }
    public async Task HandleAsync(Guid id, HttpContext context, BoardStore store, string key)
    {
        if (!context.WebSockets.IsWebSocketRequest) { context.Response.StatusCode = 400; return; }
        // Native clients authenticate with a header, never cookies/query strings.
        if (context.Request.Headers.ContainsKey("Origin")) { context.Response.StatusCode = 403; return; }
        var peer = new Peer(context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        Room room;
        lock (_gate)
        {
            if (_addresses.GetValueOrDefault(peer.Address) >= limits.MaxPerAddress)
            { peer.Stop.Dispose(); context.Response.StatusCode = 429; return; }
            if (!_rooms.TryGetValue(id, out room!)) _rooms.Add(id, room = new());
            lock (room.Gate)
            {
                if (room.Peers.Count >= limits.MaxPerBoard)
                { peer.Stop.Dispose(); context.Response.StatusCode = 429; return; }
                room.Peers.Add(peer);
            }
            _addresses[peer.Address] = _addresses.GetValueOrDefault(peer.Address) + 1;
        }
        try
        {
            var board = await store.ReadAsync(id, key, context.RequestAborted);
            if (board is null) { context.Response.StatusCode = 404; return; }
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            using var stop = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, peer.Stop.Token);
            lock (room.Gate)
            {
                if (board.Revision >= room.Revision) { room.Revision = board.Revision; room.Committed = board.Marks.Select(m => m.Id).ToHashSet(); }
                peer.Enqueue(Encode(new() { Type = "ready", Peer = peer.Id, Revision = room.Revision }));
                foreach (var other in room.Peers)
                    if (other != peer && other.Preview is { } preview)
                        foreach (var frame in LiveProtocol.Chunks(preview)) peer.Enqueue(Encode(frame with { Peer = other.Id }));
            }
            var sending = SendAsync(socket, peer, stop.Token);
            var receiving = ReceiveAsync(socket, room, peer, board.ReadOnly, stop.Token);
            await Task.WhenAny(sending, receiving);
            stop.Cancel(); socket.Abort();
            try { await Task.WhenAll(sending, receiving); }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException or JsonException) { }
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested || peer.Stop.IsCancellationRequested) { }
        catch (WebSocketException) { }
        finally
        {
            lock (_gate)
            {
                lock (room.Gate)
                {
                    room.Peers.Remove(peer);
                    if (peer.Preview is not null)
                    { ClearPreview(room, peer); Broadcast(room, new() { Type = "clear", Peer = peer.Id }); }
                    if (room.Peers.Count == 0) _rooms.Remove(id);
                    peer.Stop.Dispose();
                }
                if (_addresses[peer.Address] == 1) _addresses.Remove(peer.Address);
                else _addresses[peer.Address]--;
            }
        }
    }
    private static async Task SendAsync(WebSocket socket, Peer peer, CancellationToken ct)
    {
        await foreach (var bytes in peer.Outgoing.Reader.ReadAllAsync(ct))
        {
            Interlocked.Add(ref peer.QueuedBytes, -bytes.Length);
            await socket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, true, ct);
        }
    }
    private async Task ReceiveAsync(WebSocket socket, Room room, Peer peer, bool readOnly, CancellationToken ct)
    {
        // Most deltas fit in 4 KiB; idle connections do not each reserve 64 KiB.
        var buffer = new byte[4096];
        var tokens = 80d; var byteTokens = 2 * 1024 * 1024d; var previousTime = Stopwatch.GetTimestamp();
        while (!ct.IsCancellationRequested)
        {
            var length = 0;
            ValueWebSocketReceiveResult result;
            do
            {
                if (length == buffer.Length)
                {
                    if (buffer.Length == LiveProtocol.MaxPacketBytes) return;
                    Array.Resize(ref buffer, Math.Min(buffer.Length * 2, LiveProtocol.MaxPacketBytes));
                }
                result = await socket.ReceiveAsync(buffer.AsMemory(length), ct);
                if (result.MessageType != WebSocketMessageType.Text || length + result.Count >= LiveProtocol.MaxPacketBytes) return;
                length += result.Count;
            } while (!result.EndOfMessage);
            var now = Stopwatch.GetTimestamp(); var seconds = Stopwatch.GetElapsedTime(previousTime, now).TotalSeconds;
            tokens = Math.Min(80, tokens + seconds * 40) - 1;
            byteTokens = Math.Min(2 * 1024 * 1024, byteTokens + seconds * 1024 * 1024) - length;
            previousTime = now;
            if (readOnly || tokens < 0 || byteTokens < 0) return;
            var frame = JsonSerializer.Deserialize(buffer.AsSpan(0, length), BoardJsonContext.Default.LiveFrame);
            if (frame is null) return;
            lock (room.Gate)
            {
                if (frame.Type == "touch" && frame.Mark is null) { peer.Touched = now; continue; }
                if (frame.Type == "clear" && frame.Mark is null)
                {
                    ClearPreview(room, peer);
                    Broadcast(room, new() { Type = "clear", Peer = peer.Id }, peer); continue;
                }
                var previousCount = peer.Preview?.Points.Count ?? 0;
                var count = (frame.Offset == 0 ? 0 : previousCount) + (frame.Mark?.Points?.Count ?? 0);
                var delta = count - previousCount;
                if (room.PreviewPoints + delta > 40000 || !ReservePoints(delta)) return;
                if (!LiveProtocol.TryMerge(frame, peer.Preview, out var merged))
                { Interlocked.Add(ref _previewPoints, -delta); return; }
                // An HTTP commit may win the race with the final preview packet.
                if (room.Committed.Contains(merged!.Id)) { Interlocked.Add(ref _previewPoints, -delta); continue; }
                room.PreviewPoints += delta; peer.Preview = merged; peer.Touched = now;
                Broadcast(room, frame with { Peer = peer.Id, Revision = 0 }, peer);
            }
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Room[] rooms; lock (_gate) rooms = _rooms.Values.ToArray();
                foreach (var room in rooms)
                    lock (room.Gate)
                        foreach (var peer in room.Peers)
                            if (peer.Preview is not null && Stopwatch.GetElapsedTime(peer.Touched).TotalSeconds > 30)
                            { ClearPreview(room, peer); Broadcast(room, new() { Type = "clear", Peer = peer.Id }); }
            }
        }
        finally
        {
            lock (_gate) foreach (var room in _rooms.Values) lock (room.Gate) foreach (var peer in room.Peers) _ = peer.Stop.CancelAsync();
        }
    }
}
