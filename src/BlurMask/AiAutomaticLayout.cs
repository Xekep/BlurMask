using InkRecognition;
using InkRecognition.Math;
using InkRecognition.RawContent;

namespace BlurMask;

// An installed raw-content model partitions mixed ink before either decoder runs.
// Without a model, retain the previous conservative text-first fallback.
internal static class AiAutomaticLayout
{
    internal static IReadOnlyList<AiInkLayout.Result> Recognize(IReadOnlyList<InkMark> strokes,
        IStrokeTextRecognizer? textRecognizer = null,
        Func<IReadOnlyList<InkStroke>, CancellationToken, MathRecognitionResult?>? mathRecognizer = null,
        CancellationToken cancellationToken = default,
        Func<IReadOnlyList<InkStroke>, CancellationToken, RawContentResult?>? rawAnalyzer = null,
        Func<string, CancellationToken, InkMath.Solver.MathSolution?>? mathSolver = null)
    {
        if (strokes.Count == 0 || strokes.Count > 256 || strokes.Sum(s => (long)s.Points.Count) > 20000) return [];
        cancellationToken.ThrowIfCancellationRequested();
        // Explicit injected decoders keep tests/reusers independent of the user's
        // installed assets. Production uses the optional local analyzer automatically.
        if (rawAnalyzer is not null || textRecognizer is null && mathRecognizer is null && AiRawContentModels.Installed)
        {
            var ink = strokes.Select(s => new InkStroke(s.Points.Select(p => new InkPoint((float)p.X, (float)p.Y)).ToArray())).ToArray();
            var classified = (rawAnalyzer ?? AiRawContentModels.Analyze)(ink, cancellationToken);
            if (classified is not null) return RecognizeRegions(strokes, classified, textRecognizer, mathRecognizer, cancellationToken, mathSolver);
        }
        var text = AiInkLayout.Recognize(strokes, textRecognizer, cancellationToken).Where(r=>r.Admission==AiInkLayout.ResultAdmission.Scored).ToArray();
        var complete = text.SelectMany(r => r.Strokes).Distinct().Count() == strokes.Count;
        if (complete && text.Length > 0 && text.All(r => r.Confidence >= r.AcceptanceThreshold && (r.PositionedWords is{Length:>1} positioned?positioned.All(w=>NaturalText(w.Mark.Text)):NaturalText(r.Text.Text)))) return text;
        cancellationToken.ThrowIfCancellationRequested();
        if (mathRecognizer is not null || AiMathRecognitionModels.Installed)
        {
            var formulas = AiMathLayout.Recognize(strokes, mathRecognizer, cancellationToken, calculate: false, minimumConfidence: .85f);
            if (formulas.Count > 0 && formulas.All(r=>r.Admission==AiInkLayout.ResultAdmission.Scored) && formulas.All(r => r.Confidence >= .85f && Mathematical(r.Text.Text)))
            {
                foreach (var formula in formulas) AiMathLayout.ApplySolvedResult(formula, cancellationToken, mathSolver);
                return formulas;
            }
        }
        // Plain numeric input can still be computed with the text model alone.
        foreach (var result in text)
            if (result.PositionedWords is not{Length:>1} && result.Confidence >= result.AcceptanceThreshold && Mathematical(result.Text.Text) && MathTypesetting.Supported(result.Text.Text))
                AiMathLayout.ApplySolvedResult(result, cancellationToken, mathSolver);
        return text.Where(r => r.Confidence >= r.AcceptanceThreshold).ToArray();
    }

    private static IReadOnlyList<AiInkLayout.Result> RecognizeRegions(IReadOnlyList<InkMark> strokes, RawContentResult classification,
        IStrokeTextRecognizer? textRecognizer, Func<IReadOnlyList<InkStroke>, CancellationToken, MathRecognitionResult?>? mathRecognizer,
        CancellationToken token, Func<string, CancellationToken, InkMath.Solver.MathSolution?>? mathSolver)
    {
        var results = new List<AiInkLayout.Result>(); var used = new HashSet<int>();
        foreach (var region in classification.Regions)
        {
            token.ThrowIfCancellationRequested();
            if (region.Category is not (InkCategory.Text or InkCategory.Math) || !float.IsFinite(region.Confidence) || region.Confidence < .85f ||
                region.StrokeIndices.Count == 0 || region.StrokeIndices.Any(i => i < 0 || i >= strokes.Count || used.Contains(i)) ||
                region.StrokeIndices.Distinct().Count() != region.StrokeIndices.Count) continue;
            // Preserve chronology, colors and fading; neither decoder crosses a
            // classifier boundary, and no drawing/uncertain stroke is substituted.
            var members = region.StrokeIndices.Order().Select(i => strokes[i]).ToArray();
            var decoded = region.Category == InkCategory.Text ? AiInkLayout.Recognize(members, textRecognizer, token) :
                AiMathLayout.Recognize(members, mathRecognizer, token, calculate: false, minimumConfidence: .85f);
            if (decoded.Count == 0 || decoded.Any(r=>r.Admission!=AiInkLayout.ResultAdmission.Scored) || decoded.Any(r => !float.IsFinite(r.Confidence) || r.Confidence < r.AcceptanceThreshold) ||
                decoded.SelectMany(r => r.Strokes).Distinct().Count() != members.Length) continue;
            foreach (var result in decoded)
                if (result.PositionedWords is not{Length:>1} && Mathematical(result.Text.Text) && MathTypesetting.Supported(result.Text.Text))
                    AiMathLayout.ApplySolvedResult(result, token, mathSolver);
            results.AddRange(decoded); foreach (var i in region.StrokeIndices) used.Add(i);
        }
        return results;
    }

    private static bool NaturalText(string value) => value.Any(char.IsLetter) &&
        value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c is '\'' or '’' or '.' or ',' or '!' or '?');
    internal static bool Mathematical(string value) => value.Any(char.IsDigit) ||
        value.IndexOfAny(['+', '=', '*', '/', '^', '×', '÷', '√', '∫', '∑', 'π', '±', '\\']) >= 0;
}
