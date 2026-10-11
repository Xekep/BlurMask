using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using InkRecognition;
using InkRecognition.Math;

internal static class CorpusChecks
{
    // Formatting equivalence only, never algebraic simplification or OCR hints.
    private static string Canonical(string text)
    {
        // Exact control words only: stripping a prefix would corrupt \rightarrow.
        text = Regex.Replace(text, @"\\(?:left|right)(?![A-Za-z])", "");
        return Regex.Replace(text, @"\s+", "")
            .Replace(@"\dfrac", @"\frac").Replace(@"\tfrac", @"\frac")
            .Replace(@"\longrightarrow", @"\to").Replace(@"\rightarrow", @"\to")
            .Replace("Λ", @"\Lambda").Replace("||", @"\Vert")
            .Replace(@"\\\end{matrix}", @"\end{matrix}");
    }

    internal static void Run(string pack, string runtime, string manifestPath, string reportPath)
    {
        if (Canonical(@"lim_{n\longrightarrow\infty}x") != Canonical(@"lim_{n\to \infty }x") ||
            Canonical(@"||Λ||") != Canonical(@"\left\Vert \Lambda \right\Vert ") ||
            Canonical(@"\rightarrow0") != Canonical(@"\to0") ||
            Canonical(@"\int_{\gamma}x") == Canonical(@"\int_{y}x") || Canonical("F(x)") == Canonical("f(x)"))
            throw new InvalidDataException("Formatting-only comparison self-check differs.");
        string directory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
        var samples = manifest.RootElement.GetProperty("samples");
        if (samples.GetArrayLength() is < 1 or > 128) throw new InvalidDataException("Invalid corpus size.");
        var timer = Stopwatch.StartNew();
        using var model = File.OpenRead(pack);
        using var recognizer = StrokeMathRecognizer.Load(model, Path.GetFullPath(runtime));
        double loadMs = timer.Elapsed.TotalMilliseconds;
        int matched = 0, eligible = 0, wrongEligible = 0;
        using var report = File.Create(reportPath);
        using var writer = new Utf8JsonWriter(report, new() { Indented = true });
        writer.WriteStartObject(); writer.WriteString("runtime", recognizer.RuntimeVersion);
        writer.WriteString("modelSha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(pack))).ToLowerInvariant());
        writer.WriteString("manifestSha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))).ToLowerInvariant());
        writer.WriteNumber("loadMilliseconds", loadMs);
        writer.WriteString("policy", "One loaded recognizer; independent labels compared after recognition. Auto candidate gate: completed, supported LaTeX, confidence >= 0.85. Gate is not proof of correctness or full UI acceptance.");
        writer.WriteString("comparison", "Whitespace, dfrac/tfrac/frac, left/right fence sizing, final matrix row separator, longrightarrow/rightarrow/to, literal Lambda and ||/Vert aliases only. No variable substitution or algebraic equivalence.");
        writer.WriteStartArray("samples");
        foreach (var sample in samples.EnumerateArray())
        {
            var id = sample.GetProperty("id").GetString()!;
            var name = sample.GetProperty("strokes").GetString()!;
            if (name != Path.GetFileName(name)) throw new InvalidDataException("Corpus stroke path must be a file name.");
            string path = Path.Combine(directory, name);
            if (new FileInfo(path).Length > 8_000_000) throw new InvalidDataException("Corpus ink too large.");
            using var json = JsonDocument.Parse(File.ReadAllBytes(path));
            var strokes = new List<InkStroke>(); int points = 0;
            foreach (var stroke in json.RootElement.EnumerateArray())
            {
                var xy = new List<InkPoint>();
                foreach (var point in stroke.EnumerateArray())
                {
                    if (point.GetArrayLength() != 2) throw new InvalidDataException("Expected XY.");
                    float x = point[0].GetSingle(), y = point[1].GetSingle();
                    if (!float.IsFinite(x) || !float.IsFinite(y)) throw new InvalidDataException("Nonfinite XY.");
                    xy.Add(new(x, y));
                }
                if (xy.Count == 0) throw new InvalidDataException("Empty corpus stroke.");
                points += xy.Count; strokes.Add(new(xy));
            }
            if (strokes.Count is < 1 or > 256 || points > 20000) throw new InvalidDataException("Corpus limits exceeded.");
            timer.Restart(); var result = recognizer.Recognize(strokes); double elapsed = timer.Elapsed.TotalMilliseconds;
            timer.Restart(); var repeat = recognizer.Recognize(strokes); double repeatMs = timer.Elapsed.TotalMilliseconds;
            if (result.Text != repeat.Text || result.LaTex != repeat.LaTex || result.Confidence != repeat.Confidence)
                throw new InvalidDataException("Same-session recognition is not deterministic.");
            string expected = sample.GetProperty("expected").GetString()!;
            bool equal = result.LaTex != null && Canonical(result.LaTex) == Canonical(expected);
            bool gate = result.Completed && !string.IsNullOrWhiteSpace(result.LaTex) && float.IsFinite(result.Confidence) && result.Confidence >= .85f;
            if (equal) matched++; if (gate) eligible++; if (gate && !equal) wrongEligible++;
            writer.WriteStartObject(); writer.WriteString("id", id); writer.WriteString("category", sample.GetProperty("category").GetString());
            writer.WriteString("expected", expected); writer.WriteString("actual", result.LaTex); writer.WriteString("rawText", result.Text);
            writer.WriteStartArray("tokens"); foreach (var token in result.Tokens) writer.WriteStringValue(token); writer.WriteEndArray();
            writer.WriteBoolean("formatNormalizedMatch", equal); writer.WriteBoolean("autoConfidenceCandidate", gate);
            writer.WriteBoolean("completed", result.Completed); writer.WriteNumber("confidence", result.Confidence);
            writer.WriteNumber("frames", result.Frames); writer.WriteNumber("strokes", strokes.Count); writer.WriteNumber("points", points);
            writer.WriteNumber("milliseconds", elapsed); writer.WriteNumber("repeatMilliseconds", repeatMs); writer.WriteEndObject();
            Console.WriteLine($"{id} {sample.GetProperty("category").GetString()}: match={equal}, autoCandidate={gate}, confidence={result.Confidence:F3}, {elapsed:F1}/{repeatMs:F1} ms; {result.LaTex ?? "[unsupported]"}");
        }
        writer.WriteEndArray(); writer.WriteNumber("sampleCount", samples.GetArrayLength()); writer.WriteNumber("formatNormalizedMatches", matched);
        writer.WriteNumber("autoConfidenceCandidates", eligible); writer.WriteNumber("mismatchedAutoConfidenceCandidates", wrongEligible);
        writer.WriteEndObject();
        Console.WriteLine($"Corpus audit: {matched}/{samples.GetArrayLength()} formatting-normalized matches; {wrongEligible}/{eligible} auto-confidence candidates differ from independent labels. Model load {loadMs:F1} ms. Report: {reportPath}");
    }
}
