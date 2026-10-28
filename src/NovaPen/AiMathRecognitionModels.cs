using InkRecognition;
using InkRecognition.Math;

namespace NovaPen;

internal static class AiMathRecognitionModels
{
    internal static string PathName => Path.Combine(Path.GetDirectoryName(SettingsStore.PathName)!, "models", "math2.ikrmath");
    internal static bool Installed => File.Exists(PathName);
    private static readonly object Gate = new();
    private static StrokeMathRecognizer? _recognizer;
    private static DateTime _modified;
    private static long _length = -1;

    internal static MathRecognitionResult? Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken token)
    {
        lock (Gate)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var file = new FileInfo(PathName);
                if (!file.Exists) { _recognizer?.Dispose(); _recognizer = null; _length = -1; return null; }
                if (_recognizer is null || _modified != file.LastWriteTimeUtc || _length != file.Length)
                {
                    using var stream = file.OpenRead();
                    var loaded = StrokeMathRecognizer.Load(stream);
                    _recognizer?.Dispose(); _recognizer = loaded;
                    _modified = file.LastWriteTimeUtc; _length = file.Length;
                }
                return _recognizer.Recognize(strokes, token);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or DllNotFoundException or BadImageFormatException or EntryPointNotFoundException or NotSupportedException)
            { System.Diagnostics.Trace.WriteLine($"Math model: {ex.Message}"); return null; }
        }
    }

    internal static void Install(Stream source)
    {
        lock (Gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
            var temporary = PathName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            StrokeMathRecognizer? loaded = null;
            try
            {
                using (var output = File.Create(temporary))
                {
                    var buffer = new byte[65536]; var total = 0; int read;
                    while ((read = source.Read(buffer)) > 0)
                    {
                        total = checked(total + read);
                        if (total > 64 * 1024 * 1024 + 48) throw new InvalidDataException("Model is too large.");
                        output.Write(buffer, 0, read);
                    }
                }
                using (var snapshot = File.OpenRead(temporary)) loaded = StrokeMathRecognizer.Load(snapshot);
                File.Move(temporary, PathName, true);
                _recognizer?.Dispose(); _recognizer = loaded; loaded = null;
                var file = new FileInfo(PathName); _modified = file.LastWriteTimeUtc; _length = file.Length;
            }
            finally { loaded?.Dispose(); if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
