using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace NovaPen;

internal static class OverlayPlatformProbe
{
    internal static void Run() => VerifyAsync().GetAwaiter().GetResult();
    private static async Task VerifyAsync()
    {
        MaskAttachmentChecks.Run();
        if (LinuxDisplayPolicy.UseWayland("auto", ":0", "wayland-1") ||
            !LinuxDisplayPolicy.UseWayland("auto", null, "wayland-1") ||
            !LinuxDisplayPolicy.UseWayland("wayland", ":0", "wayland-1") ||
            LinuxDisplayPolicy.UseWayland("x11", ":0", "wayland-1") ||
            LinuxDisplayPolicy.UseWayland("wayland", ":0", null) ||
            !LinuxDisplayPolicy.UseSoftware(false, false, true, false, Path.GetTempPath(), "valid-session") ||
            LinuxDisplayPolicy.UseSoftware(false, false, true, false, Path.GetTempPath(), "../bad") ||
            LinuxDisplayPolicy.UseSoftware(false, false, false, false, Path.GetTempPath(), "valid-session") ||
            LinuxDisplayPolicy.UseSoftware(true, false, true, true, Path.GetTempPath(), "valid-session") ||
            !LinuxDisplayPolicy.UseSoftware(true, false, false, false, null, null) ||
            !LinuxDisplayPolicy.UseSoftware(false, true, false, false, null, null))
            throw new InvalidOperationException("Linux backend/Hyprland software rendering policy failed.");
        const string title = "NovaPen — ink overlay";
        const string clients = """
            [{"pid":42,"mapped":true,"xwayland":true,"title":"NovaPen — ink overlay","address":"0xABC"},
             {"pid":43,"mapped":true,"xwayland":true,"title":"NovaPen — ink overlay","address":"0xDEF"},
             {"pid":42,"mapped":true,"xwayland":true,"title":"NovaPen — Canvas","address":"0x123"},
             {"pid":42,"mapped":false,"xwayland":true,"title":"NovaPen — ink overlay","address":"0x456"},
             {"pid":42,"mapped":true,"xwayland":true,"title":"NovaPen — ink overlay","address":"0xabc; exit"},
             {"pid":"42","mapped":true,"xwayland":true,"title":"NovaPen — ink overlay","address":"0x789"}]
            """;
        if (!HyprlandInkOverlay.OwnClients(clients, title, 42).SequenceEqual(["0xabc"]))
            throw new InvalidOperationException("Effects must target only mapped desktop ink owned by this process.");
        try { HyprlandInkOverlay.OwnClients("{}", title, 42); throw new InvalidOperationException("Malformed client list accepted."); }
        catch (JsonException) { }
        if (HyprlandInkOverlay.SocketPath(Path.GetTempPath(), "../other-session") is not null)
            throw new InvalidOperationException("Invalid compositor signature accepted.");
        var commands = new List<string>(); var lists = 0;
        await HyprlandInkOverlay.ConfigureAsync(title, 42, (command, _) =>
        {
            commands.Add(command);
            return Task.FromResult(command == "j/clients" ? ++lists == 1 ? "[]" : clients : "ok");
        }, CancellationToken.None);
        if (lists != 2 || commands.Count(c => c.StartsWith("/dispatch setprop address:0xabc ", StringComparison.Ordinal)) != 6 ||
            commands.Any(c => c.Contains("0xdef") || c.Contains("0x123") || c.Contains("keyword") || c.Contains("reload")))
            throw new InvalidOperationException("Overlay effects changed foreign windows or global configuration.");
        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            try
            {
                await HyprlandInkOverlay.ConfigureAsync(title, 42, (_, _) => throw new InvalidOperationException("Cancelled request was sent."), cancelled.Token);
                throw new InvalidOperationException("Overlay request ignored cancellation.");
            }
            catch (OperationCanceledException) { }
        }
        // Exercise the real bounded socket transport with fragmented replies.
        var path = Path.Combine(Path.GetTempPath(), "novapen-ipc-" + Guid.NewGuid().ToString("N"));
        using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(4));
        try
        {
            listener.Bind(new UnixDomainSocketEndPoint(path)); listener.Listen(1);
            var server = Task.Run(async () =>
            {
                using var peer = await listener.AcceptAsync(deadline.Token);
                var buffer = new byte[256];
                var read = await peer.ReceiveAsync(buffer, SocketFlags.None, deadline.Token);
                if (Encoding.UTF8.GetString(buffer, 0, read) != "j/clients") throw new InvalidOperationException("IPC request corrupted.");
                var bytes = Encoding.UTF8.GetBytes(clients);
                for (var offset = 0; offset < bytes.Length;)
                    offset += await peer.SendAsync(bytes.AsMemory(offset, Math.Min(17, bytes.Length - offset)), SocketFlags.None, deadline.Token);
            });
            var response = await HyprlandInkOverlay.RequestAsync(path, "j/clients", deadline.Token);
            await server;
            if (response != clients) throw new InvalidOperationException("Fragmented compositor reply truncated.");
        }
        finally { listener.Dispose(); if (File.Exists(path)) File.Delete(path); }
        Console.WriteLine("Overlay IPC: ownership, delayed mapping, cancellation and fragmented transport passed.");
    }
}
