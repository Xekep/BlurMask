using System.Text.Json;
using InkRecognition;

namespace NovaPen;

// Opt-in audit of installed real decoders/routing, separate from CAS goldens.
// Never starts Avalonia desktop lifetime, a GUI or a solver process.
internal static class AiMathCorpusProbe
{
    internal static void RunIfRequested()
    {
        if (Environment.GetEnvironmentVariable("BLURMASK_MATH_CORPUS") is not { Length: > 0 } path) return;
        if (!File.Exists(AiMathRecognitionModels.PathName)) throw new InvalidOperationException("Install a real Math2 pack before corpus audit.");
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(path));
        var samples = manifest.RootElement.GetProperty("samples");
        if (samples.GetArrayLength() is < 1 or > 128) throw new InvalidDataException("Invalid app corpus size.");
        int direct = 0, automatic = 0, math = 0;
        foreach (var sample in samples.EnumerateArray())
        {
            var file = sample.GetProperty("strokes").GetString()!;
            if (file != Path.GetFileName(file)) throw new InvalidDataException("Unexpected corpus path.");
            var strokes = AiPenProbe.ReadStrokes(Path.Combine(directory, file));
            var candidate = AiMathLayout.Recognize(strokes, calculate: false, minimumConfidence: .85f);
            var ink = strokes.Select(s => new InkStroke(s.Points.Select(p => new InkPoint((float)p.X, (float)p.Y)).ToArray())).ToArray();
            var classification = AiRawContentModels.Analyze(ink, CancellationToken.None);
            int calls = 0;
            var routed = AiAutomaticLayout.Recognize(strokes,
                rawAnalyzer: classification is null ? null : (_, _) => classification,
                mathSolver: (_, _) => { calls++; return null; });
            if (calls != 0) throw new InvalidOperationException("Corpus with written/no equals invoked CAS: " + sample.GetProperty("id").GetString());
            foreach (var result in candidate)
            {
                if (!MathTypesetting.Supported(result.Text.Text)) throw new InvalidOperationException("Accepted formula is not typesettable.");
                VerifyUndo(strokes, result);
            }
            foreach (var result in routed)
                if (result.Text.Font == "STIX Two Math" && !MathTypesetting.Supported(result.Text.Text))
                    throw new InvalidOperationException("Automatic formula is not typesettable.");
            if (candidate.Count > 0) direct++;
            if (routed.Count > 0) automatic++;
            if (routed.Any(r => r.Text.Font == "STIX Two Math")) math++;
            var regions = classification is null ? "none" : string.Join(", ", classification.Regions.Select(r => $"{r.Category}:{r.Confidence:F3}/{r.StrokeIndices.Count}"));
            if (classification is { Regions.Count: 0 })
                regions = "no eligible regions; strokes=" + string.Join(", ", classification.Strokes.Select(s => $"{s.Category}:{s.Confidence:F3}"));
            Console.WriteLine($"Math corpus app {sample.GetProperty("id").GetString()}: raw={regions}; direct={candidate.Count}, automatic={routed.Count}, math={routed.Count(r => r.Text.Font == "STIX Two Math")}; {string.Join(" / ", routed.Select(r => r.Text.Text))}");
        }
        Console.WriteLine($"Actual installed math corpus pipeline: direct candidates {direct}/{samples.GetArrayLength()}, automatic conversions {automatic}, math conversions {math}; supported syntax, written RHS/no-CAS and direct undo checked. This is routing, not OCR accuracy.");
    }

    private static void VerifyUndo(IReadOnlyList<InkMark> strokes, AiInkLayout.Result result)
    {
        var document = new InkDocument(); foreach (var stroke in strokes) document.Add(stroke);
        if (!document.ReplaceRecognized(result.Strokes, result.Text)) throw new InvalidOperationException("Actual formula replacement declined.");
        document.Undo();
        if (!document.Marks.SequenceEqual(strokes)) throw new InvalidOperationException("Actual formula undo did not restore ink.");
    }
}
