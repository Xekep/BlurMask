using InkRecognition;
using InkRecognition.Math;

namespace BlurMask;

internal static class AiMathLayout
{
    internal static IReadOnlyList<AiInkLayout.Result> Recognize(IReadOnlyList<InkMark> strokes,
        Func<IReadOnlyList<InkStroke>, CancellationToken, MathRecognitionResult?>? recognize = null, CancellationToken cancellationToken = default)
    {
        if (strokes.Count == 0 || strokes.Count > 256 || strokes.Sum(s => s.Points.Count) > 20000) return [];
        cancellationToken.ThrowIfCancellationRequested();
        recognize ??= AiMathRecognitionModels.Recognize;
        var results = new List<AiInkLayout.Result>();
        // Keep each expression intact: row/word splitting destroys fractions, roots and matrices.
        foreach (var group in strokes.GroupBy(s => (s.Color, s.FadeSeconds)))
        {
            var sources = group.ToArray(); var first = sources[0];
            var bounds = sources.Select(s => s.Bounds).Aggregate((a, b) => a.Union(b));
            if (!double.IsFinite(bounds.Width) || !double.IsFinite(bounds.Height) || bounds.Width < 2 || bounds.Height < 2 ||
                bounds.Left < -100000 || bounds.Top < -100000 || bounds.Right > 100000 || bounds.Bottom > 100000) continue;
            var input = sources.Select(s => new InkStroke(s.Points.Select(p => new InkPoint((float)p.X, (float)p.Y)).ToArray())).ToArray();
            var result = recognize(input, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (result is not { Completed: true, LaTex: { Length: > 0 } latex } || !float.IsFinite(result.Confidence) ||
                result.Confidence < .75f || !MathTypesetting.Supported(latex)) continue;
            var calculated = WithSolvedResult(latex, cancellationToken);
            // Re-read geometry after numeric parsing. Undefined arithmetic is
            // handled inside the parser; avoid carrying the Rect's floating
            // locals across that exception path in the ARM64 AOT compiler.
            bounds = sources.Select(s => s.Bounds).Aggregate((a, b) => a.Union(b));
            var mark = new InkMark(InkTool.Text, first.Color, first.Width, bounds.TopLeft)
            { Id = first.Id, Text = calculated, Font = "STIX Two Math", FontSize = Math.Clamp(bounds.Height, 8, 300), FadeSeconds = first.FadeSeconds };
            mark.AddPoint(bounds.BottomRight + new Avalonia.Vector(Math.Min(100000 - bounds.Right, ResultWidth(latex, calculated, bounds.Height)), 0));
            results.Add(new(sources, mark, result.Confidence) { SizingText = latex });
        }
        return results;
    }

    internal static string WithNumericResult(string expression)
    {
        var equal = expression.IndexOf('=');
        // An empty right-hand side explicitly requests a result. Recognition
        // alone must not evaluate expressions or replace a written answer.
        if (equal < 0 || expression[(equal + 1)..].Trim().Length > 0 ||
            !NumericExpressionEvaluator.TryEvaluate(expression, out var value)) return expression;
        return expression[..equal].TrimEnd() + "=" + value;
    }

    internal static double ResultWidth(string original, string calculated, double height)
        => original == calculated ? 0 : Math.Max(1, calculated.Length - original.Length) * height * .55;

    internal static string WithSolvedResult(string expression, CancellationToken token = default,
        Func<string, CancellationToken, InkMath.Solver.MathSolution?>? solve = null)
    {
        token.ThrowIfCancellationRequested();
        var numeric = WithNumericResult(expression);
        if (numeric != expression) return numeric;
        var equal = expression.LastIndexOf('=');
        if (equal < 0 || expression[(equal + 1)..].Trim().Length != 0) return expression;
        var source = expression[..equal].TrimEnd();
        if (source.Length == 0) return expression;
        var solution = (solve ?? AiMathSolver.Calculate)(source, token);
        token.ThrowIfCancellationRequested();
        if (solution is null) return expression;
        var calculated = source + (solution.IsEquation ? "\\Rightarrow " : "=") + solution.LaTex;
        return MathTypesetting.Supported(calculated) ? calculated : expression;
    }
}
