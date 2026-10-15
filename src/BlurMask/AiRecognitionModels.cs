using InkRecognition;

namespace BlurMask;

internal static class AiRecognitionModels
{
    internal static string PathName => Path.Combine(Path.GetDirectoryName(SettingsStore.PathName)!, "models", "en-US.ikrpack");
    private static readonly object Gate = new();
    private static StrokeTextRecognizer? _recognizer;
    private static DateTime _modified;
    private static long _length = -1;
    private static readonly Lazy<ExperimentalFirstStageTextRecognizer> Experimental = new(() =>
        ExperimentalFirstStageTextRecognizer.Load(ReadEmbedded("acoustic"), ReadEmbedded("engine"), ReadEmbedded("separation")));
    private static byte[] ReadEmbedded(string name)
    {
        using var stream = typeof(AiRecognitionModels).Assembly.GetManifestResourceStream("BlurMask.ExperimentalInk." + name)
            ?? throw new InvalidDataException("Bundled experimental model is missing.");
        using var output = new MemoryStream(); stream.CopyTo(output); return output.ToArray();
    }

    internal static IStrokeTextRecognizer? Current
    {
        get
        {
            if (Program.Settings.ExperimentalTextEngine) return Experimental.Value;
            lock (Gate)
            {
                try
                {
                    var file = new FileInfo(PathName);
                    if (!file.Exists) { _recognizer = null; _length = -1; return null; }
                    if (_modified == file.LastWriteTimeUtc && _length == file.Length) return _recognizer;
                    _recognizer = null; _modified = file.LastWriteTimeUtc; _length = file.Length;
                    using var stream = file.OpenRead();
                    var model = StrokeTextRecognizer.Load(stream);
                    if (model.Language is not ("en" or "en-US")) throw new InvalidDataException("Unsupported recognition language.");
                    return _recognizer = model;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
                { System.Diagnostics.Trace.WriteLine($"AI model: {ex.Message}"); return null; }
            }
        }
    }

    internal static void Install(Stream source)
    {
        lock (Gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
            var temporary = PathName + "." + Guid.NewGuid().ToString("N") + ".tmp";
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
                // Validate the exact snapshot that will be installed; support nonseekable storage streams.
                StrokeTextRecognizer model;
                using (var snapshot = File.OpenRead(temporary)) model = StrokeTextRecognizer.Load(snapshot);
                if (model.Language is not ("en" or "en-US")) throw new InvalidDataException("Unsupported recognition language.");
                File.Move(temporary, PathName, true);
                _recognizer = model; var file = new FileInfo(PathName); _modified = file.LastWriteTimeUtc; _length = file.Length;
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
