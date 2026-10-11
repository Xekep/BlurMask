using InkRecognition;
using InkRecognition.Math;

namespace BlurMask;

// Text is cheap and gets first refusal. Load the formula engine only for
// mathematical notation or uncertain text; never compare unrelated model scores.
internal static class AiAutomaticLayout
{
    internal static IReadOnlyList<AiInkLayout.Result> Recognize(IReadOnlyList<InkMark> strokes,
        IStrokeTextRecognizer? textRecognizer = null,
        Func<IReadOnlyList<InkStroke>, CancellationToken, MathRecognitionResult?>? mathRecognizer = null,
        CancellationToken cancellationToken = default)
    {
        var text = AiInkLayout.Recognize(strokes, textRecognizer, cancellationToken);
        var complete = text.SelectMany(r => r.Strokes).Distinct().Count() == strokes.Count;
        if (complete && text.Count > 0 && text.All(r => r.Confidence >= .85f && NaturalText(r.Text.Text))) return text;
        cancellationToken.ThrowIfCancellationRequested();
        if (mathRecognizer is not null || AiMathRecognitionModels.Installed)
        {
            var formulas = AiMathLayout.Recognize(strokes, mathRecognizer, cancellationToken);
            if (formulas.Count > 0 && formulas.All(r => r.Confidence >= .85f && Mathematical(r.Text.Text))) return formulas;
        }
        // Plain numeric input can still be computed with the text model alone.
        foreach (var result in text)
            if (result.Confidence >= .85f && Mathematical(result.Text.Text) && MathTypesetting.Supported(result.Text.Text))
            {
                var original = result.Text.Text;
                var calculated = AiMathLayout.WithNumericResult(original);
                if (calculated == original) continue;
                result.SizingText = original;
                var bounds = result.Text.Bounds;
                result.Text.Text = calculated;
                result.Text.AddPoint(bounds.BottomRight + new Avalonia.Vector(Math.Min(100000 - bounds.Right, AiMathLayout.ResultWidth(original, calculated, bounds.Height)), 0));
            }
        return text.Where(r => r.Confidence >= .85f).ToArray();
    }

    private static bool NaturalText(string value) => value.Any(char.IsLetter) &&
        value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c is '\'' or '’' or '.' or ',' or '!' or '?');
    internal static bool Mathematical(string value) => value.Any(char.IsDigit) ||
        value.IndexOfAny(['+', '=', '*', '/', '^', '×', '÷', '√', '∫', '∑', 'π', '±', '\\']) >= 0;
}
