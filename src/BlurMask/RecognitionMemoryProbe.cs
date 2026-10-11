using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

namespace BlurMask;

internal sealed record RecognitionMemorySample(string Stage, long WorkingSetBytes, long PeakWorkingSetBytes,
    long PrivateCommitBytes, long PrivateResidentBytes, long ProportionalResidentBytes, long ManagedLiveBytes, long ManagedCommittedBytes);
[JsonSerializable(typeof(RecognitionMemorySample))]
internal partial class RecognitionMemoryJson : JsonSerializerContext;

// Headless process measurements. No desktop windows, capture, renderer or installed-model changes.
internal static class RecognitionMemoryProbe
{
    internal static void Run(string[] args)
    {
        string? Option(string name)
        {
            var index = Array.IndexOf(args, name);
            return index < 0 ? null : index + 1 < args.Length ? args[index + 1] : throw new ArgumentException($"Missing {name} value.");
        }
        Sample("process_start");
        Exercise(Option("--text-model"), Option("--math-model"), Option("--input"));
        Collect(); Sample("models_disposed_after_gc");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Exercise(string? textPath, string? mathPath, string? inputPath)
    {
        var strokes = inputPath is null ? new InkStroke[] { new(new InkPoint[] { new(0, 0), new(20, 30), new(40, 0) }) }
            : AiPenProbe.ReadStrokes(inputPath).Select(m => new InkStroke(m.Points.Select(p => new InkPoint((float)p.X, (float)p.Y)).ToArray())).ToArray();
        StrokeTextRecognizer? text = null;
        if (textPath is not null)
        {
            using (var source = File.OpenRead(textPath)) text = StrokeTextRecognizer.Load(source);
            Sample("text_loaded"); Collect(); Sample("text_loaded_after_gc");
            for (var i = 0; i < 30; i++) _ = text.Recognize(strokes);
            Sample("text_30_calls"); Collect(); Sample("text_30_calls_after_gc");
        }
        if (mathPath is not null)
        {
            using var source = File.OpenRead(mathPath);
            using var math = StrokeMathRecognizer.Load(source);
            Sample("math_loaded"); Collect(); Sample("math_loaded_after_gc");
            for (var i = 0; i < 30; i++) _ = math.Recognize(strokes);
            Sample("math_30_calls"); Collect(); Sample("math_30_calls_after_gc");
            GC.KeepAlive(text);
        }
    }

    private static void Collect() { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
    private static void Sample(string stage)
    {
        using var process = Process.GetCurrentProcess(); process.Refresh();
        long resident = 0, proportional = 0;
        if (OperatingSystem.IsLinux() && File.Exists("/proc/self/smaps_rollup"))
        {
            foreach (var line in File.ReadLines("/proc/self/smaps_rollup"))
            {
                var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 2 || !long.TryParse(words[1], out var kib)) continue;
                if (words[0] is "Private_Clean:" or "Private_Dirty:") resident += kib * 1024;
                if (words[0] == "Pss:") proportional = kib * 1024;
            }
        }
        var sample = new RecognitionMemorySample(stage, process.WorkingSet64, process.PeakWorkingSet64,
            process.PrivateMemorySize64, resident, proportional, GC.GetTotalMemory(false), GC.GetGCMemoryInfo().TotalCommittedBytes);
        Console.WriteLine(JsonSerializer.Serialize(sample, RecognitionMemoryJson.Default.RecognitionMemorySample));
    }
}
