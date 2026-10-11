using System.Globalization;
using InkRecognition.Math;

internal static class NumericEvaluatorChecks
{
    internal static void Verify()
    {
        var accepted = new (string Input, string Expected)[]
        {
            ("2+3*4", "14"), ("(2+3)*4", "20"), ("8/2/2", "2"),
            ("2^3^2", "512"), ("-2^2", "-4"), ("(-2)^2", "4"), ("2^-3", "0.125"),
            ("3**2+4**2", "25"), ("5−2×3", "-1"), ("9÷3+1", "4"),
            ("1,5+2,25", "3.75"), (".5+.25", "0.75"), ("0.1+0.2", "0.3"),
            ("2+2=", "4"), ("2+2=4", "4"), ("0.1+0.2=0.3", "0.3"), ("6/3=1+1", "2"),
            ("\\frac{1}{2}+\\frac{1}{4}", "0.75"), ("\\frac{1+2}{\\frac{3}{4}}", "4"),
            ("\\sqrt{9}+\\sqrt{16}", "7"), ("√(25)", "5"), ("\\sqrt[3]{-8}", "-2"),
            ("\\sqrt[4]{16}", "2"), ("\\sqrt{\\frac{1}{4}}", "0.5"), ("2^{3+1}", "16"),
            ("\\left(2+3\\right)\\times\\dfrac{6}{2}", "15"), ("[2+3]\\cdot 2", "10"),
            ("1e-8+1e-8", "0.00000002"), ("1e12*10", "10000000000000"), ("1e30*2", "2E+30"),
            ("-0", "0"), ("1e-13", "1E-13"), (" 2 + 3 = ", "5"),
            ("1" + string.Concat(Enumerable.Repeat("+1", 256)), "257")
        };
        var rejected = new string?[]
        {
            null, "", " ", "x+1", "y=3x+2", "2x=4", "2+2=5", "1=2=3", "=2", "2+=",
            "1/0", "\\frac{1}{0}", "\\sqrt{-1}", "(-2)^.5", "0^0", "0^-1", "1e309", "1e308*10",
            "\\sqrt[2.5]{16}", "\\sqrt[65]{1}", "\\sqrt[1]{9}", "\\sqrt{9", "\\frac{1}{2",
            "2/3(4)", "2(3+4)", "1 2", "1,2.3", "1e", "1e+", "NaN", "Infinity", "sin(0)",
            "\\sin{0}", "\\pi", "2;3", "2+3junk", "2+3=answer", "1**", "2^^3", "(2+3]",
            "\\left.2\\right)", "\\sqrt{}", "()", "--", "1^10001",
            new string('(', 33) + "1" + new string(')', 33),
            new string('-', 33) + "1", new string('1', 2049),
            "1" + string.Concat(Enumerable.Repeat("+1", 257))
        };
        foreach (var (input, expected) in accepted)
        {
            if (!NumericExpressionEvaluator.TryEvaluate(input, out var actual) || actual != expected)
                throw new InvalidDataException($"Numeric golden {input}: expected {expected}; got {actual}.");
        }
        foreach (var input in rejected)
        {
            if (NumericExpressionEvaluator.TryEvaluate(input, out var actual) || actual.Length != 0)
                throw new InvalidDataException($"Unsafe/unsupported numeric expression was accepted: {input} -> {actual}.");
        }
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            if (!NumericExpressionEvaluator.TryEvaluate("1,25+0.5", out var result) || result != "1.75")
                throw new InvalidDataException("Numeric output/input depended on the process culture.");
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        Console.WriteLine($"Bounded numeric arithmetic goldens/guards PASS ({accepted.Length + rejected.Length + 1} checks)");
    }
}
