using InkRecognition;
using InkRecognition.RawContent;

namespace NovaPen;

internal static class AiRawContentModels
{
    internal static string PathName => Path.Combine(Path.GetDirectoryName(SettingsStore.PathName)!, "models", "raw-content.ikrraw");
    internal static bool Installed => File.Exists(PathName);
    private static readonly object Gate = new();
    private static RawContentAnalyzer? _analyzer;
    private static DateTime _modified;
    private static long _length = -1;
    private static bool Failed;

    internal static RawContentResult? Analyze(IReadOnlyList<InkStroke> strokes, CancellationToken token)
    {
        lock (Gate)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var file = new FileInfo(PathName);
                if (!file.Exists) { _analyzer?.Dispose(); _analyzer = null; _length = -1; Failed = false; return null; }
                if (_modified != file.LastWriteTimeUtc || _length != file.Length)
                {
                    _analyzer?.Dispose(); _analyzer = null; Failed = false;
                    _modified = file.LastWriteTimeUtc; _length = file.Length;
                }
                if (Failed) return null;
                if (_analyzer is null) { using var stream = file.OpenRead(); _analyzer = RawContentAnalyzer.Load(stream); }
                return _analyzer.Analyze(strokes, token);
            }
            catch (Exception ex) when (Recoverable(ex))
            {
                // A broken/unsupported pack must not repeatedly warm up ORT on each
                // pen lift. Retry only when its timestamp/length changes or on install.
                _analyzer?.Dispose(); _analyzer = null; Failed = true;
                System.Diagnostics.Trace.WriteLine($"Raw-content model: {ex.Message}"); return null;
            }
        }
    }
    private static bool Recoverable(Exception ex) => ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException
        or DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or NotSupportedException;

    internal static void Install(Stream source)
    {
        lock (Gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
            var temporary = PathName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            RawContentAnalyzer? loaded = null;
            try
            {
                using (var output = File.Create(temporary))
                {
                    var buffer = new byte[65536]; var total = 0; int read;
                    while ((read = source.Read(buffer)) > 0)
                    {
                        total = checked(total + read);
                        if (total > 16 * 1024 * 1024 + 48) throw new InvalidDataException("Model is too large.");
                        output.Write(buffer, 0, read);
                    }
                }
                using (var snapshot = File.OpenRead(temporary)) loaded = RawContentAnalyzer.Load(snapshot);
                // Validate model ABI before replacing an existing working pack.
                _ = loaded.Analyze([new InkStroke([new InkPoint(0, 0), new InkPoint(10, 10)])]);
                File.Move(temporary, PathName, true);
                _analyzer?.Dispose(); _analyzer = loaded; loaded = null; Failed = false;
                var file = new FileInfo(PathName); _modified = file.LastWriteTimeUtc; _length = file.Length;
            }
            finally { loaded?.Dispose(); if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
