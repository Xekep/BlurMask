using System.Reflection;
using System.Text.Json;

namespace NovaPen;

internal static class AppUpdates
{
    internal const string Website = "https://pen.kiiko.ru/";
    // The download page uses this same stable release feed.
    private const string ReleaseFeed = "https://api.github.com/repos/Xekep/NovaPen/releases/latest";
    internal static string CurrentVersion => typeof(AppUpdates).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

    internal static async Task<string?> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8), MaxResponseContentBufferSize = 512 * 1024 };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NovaPen-update-check");
            using var response = await client.GetAsync(ReleaseFeed, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            return ReadNewVersion(data, CurrentVersion);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidDataException)
        { return null; }
    }

    internal static string? ReadNewVersion(ReadOnlyMemory<byte> json, string current)
    {
        if (json.Length > 512 * 1024) return null;
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("draft", out var draft) || draft.ValueKind != JsonValueKind.False ||
            !root.TryGetProperty("prerelease", out var prerelease) || prerelease.ValueKind != JsonValueKind.False ||
            !root.TryGetProperty("tag_name", out var tag) || tag.ValueKind != JsonValueKind.String) return null;
        var available = tag.GetString();
        if (!TryVersion(available, out var next, out var nextPreview) || nextPreview ||
            !TryVersion(current, out var installed, out var installedPreview)) return null;
        return next > installed || next == installed && installedPreview ? available!.TrimStart('v') : null;
    }

    private static bool TryVersion(string? text, out Version version, out bool preview)
    {
        version = new Version(0, 0, 0); preview = false;
        if (string.IsNullOrEmpty(text) || text.Length > 100) return false;
        text = text.Split('+', 2)[0].TrimStart('v');
        var parts = text.Split('-', 2); preview = parts.Length == 2;
        if (preview && (parts[1].Length == 0 || parts[1].Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('.' or '-')))) return false;
        if (parts[0].Count(c => c == '.') is not (2 or 3) || parts[0].Any(c => !char.IsAsciiDigit(c) && c != '.')) return false;
        if (!Version.TryParse(parts[0], out var parsed)) return false;
        version = new Version(parsed.Major, parsed.Minor, parsed.Build, Math.Max(0, parsed.Revision));
        return true;
    }
}
