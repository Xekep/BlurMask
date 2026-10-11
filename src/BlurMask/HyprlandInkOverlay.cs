using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BlurMask;

// Hyprland may blur every transparent normal window, even if the client requests
// plain transparency. Change only this process's desktop-ink clients, never rules
// or the user's configuration. IPC runs off the UI thread and is bounded.
internal sealed class HyprlandInkOverlay(string socketPath, string title) : IDisposable
{
    private CancellationTokenSource? _pending;
    internal static HyprlandInkOverlay? TryCreate(string title)
    {
        if (!OperatingSystem.IsLinux()) return null;
        var path = SocketPath(Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"),
            Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE"));
        return path is null ? null : new(path, title);
    }

    internal static string? SocketPath(string? runtime, string? signature)
    {
        if (string.IsNullOrEmpty(runtime) || !Path.IsPathFullyQualified(runtime) ||
            string.IsNullOrEmpty(signature) || signature.Length > 128 ||
            signature.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('_' or '-' or '.')) || signature is "." or "..") return null;
        var path = Path.Combine(runtime, "hypr", signature, ".socket.sock");
        return Encoding.UTF8.GetByteCount(path) <= 107 ? path : null;
    }

    internal void Apply()
    {
        Suspend();
        _pending = new CancellationTokenSource();
        var token = _pending.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(4));
                await ConfigureAsync(title, Environment.ProcessId,
                    (command, cancellation) => RequestAsync(socketPath, command, cancellation), timeout.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is SocketException or IOException or JsonException or ArgumentException)
            { Trace.WriteLine($"Hyprland ink effects: {ex.GetType().Name}"); }
        }, token);
    }

    internal void Suspend()
    {
        _pending?.Cancel();
        _pending?.Dispose();
        _pending = null;
    }
    public void Dispose() => Suspend();

    internal static async Task ConfigureAsync(string title, int pid,
        Func<string, CancellationToken, Task<string>> request, CancellationToken cancellationToken)
    {
        string[] addresses = [];
        // Mapping is asynchronous; no pointer events or focus changes are needed.
        for (var attempt = 0; attempt < 12; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            addresses = OwnClients(await request("j/clients", cancellationToken), title, pid);
            if (addresses.Length > 0) break;
            await Task.Delay(100, cancellationToken);
        }
        foreach (var address in addresses)
        {
            foreach (var (property, value) in new[] { ("no_blur", "1"), ("no_shadow", "1"), ("no_dim", "1"), ("no_anim", "1"), ("rounding", "0"), ("border_size", "0") })
            {
                cancellationToken.ThrowIfCancellationRequested();
                var response = await request($"/dispatch setprop address:{address} {property} {value}", cancellationToken);
                if (response.Trim() == "ok") continue;
                // Lua-based configurations expose dispatchers instead of legacy
                // dispatcher strings. Values and selectors here are fixed/hex.
                var lua = $"/eval local d = (hl.dsp and hl.dsp.window) or hl.window; return hl.dispatch(d.set_prop({{prop=\"{property}\",value=\"{value}\",window=\"address:{address}\"}}))";
                await request(lua, cancellationToken);
            }
        }
    }

    internal static string[] OwnClients(string json, string title, int pid)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 32 });
        if (document.RootElement.ValueKind != JsonValueKind.Array) throw new JsonException("Invalid Hyprland client list.");
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var client in document.RootElement.EnumerateArray())
        {
            if (client.ValueKind != JsonValueKind.Object ||
                !client.TryGetProperty("pid", out var owner) || owner.ValueKind != JsonValueKind.Number || !owner.TryGetInt32(out var number) || number != pid ||
                !client.TryGetProperty("mapped", out var mapped) || mapped.ValueKind != JsonValueKind.True ||
                !client.TryGetProperty("xwayland", out var xwayland) || xwayland.ValueKind != JsonValueKind.True ||
                !client.TryGetProperty("title", out var name) || name.ValueKind != JsonValueKind.String || name.GetString() != title ||
                !client.TryGetProperty("address", out var value) || value.ValueKind != JsonValueKind.String) continue;
            var address = value.GetString();
            if (address is null || address.Length is < 3 or > 18 || !address.StartsWith("0x", StringComparison.Ordinal) ||
                !ulong.TryParse(address.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var pointer) || pointer == 0) continue;
            result.Add($"0x{pointer:x}");
        }
        return result.ToArray();
    }

    internal static async Task<string> RequestAsync(string path, string command, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMilliseconds(750));
        var token = timeout.Token;
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        await socket.ConnectAsync(new UnixDomainSocketEndPoint(path), token);
        var bytes = Encoding.UTF8.GetBytes(command);
        for (var offset = 0; offset < bytes.Length;)
        {
            var sent = await socket.SendAsync(bytes.AsMemory(offset), SocketFlags.None, token);
            if (sent == 0) throw new IOException("Hyprland IPC closed while sending.");
            offset += sent;
        }
        socket.Shutdown(SocketShutdown.Send);
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int count;
        while ((count = await socket.ReceiveAsync(buffer, SocketFlags.None, token)) > 0)
        {
            if (output.Length + count > 8 * 1024 * 1024) throw new IOException("Hyprland IPC response exceeds the limit.");
            output.Write(buffer, 0, count);
        }
        return Encoding.UTF8.GetString(output.GetBuffer(), 0, checked((int)output.Length));
    }
}
