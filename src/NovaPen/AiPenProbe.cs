using Avalonia;
using Avalonia.Media;
using InkRecognition;
using InkRecognition.Math;
using InkRecognition.RawContent;

namespace NovaPen;

internal static class AiPenProbe
{
    internal static void Run()
    {
        DesktopFeatureChecks.Run();
        SelectionProbe.Run();
        StrokeGeometryChecks.Run();
        BoardWriteReceiptChecks.Run();
        AiInkBatchChecks.Run();
        InkRecognitionChecks.Run();
        var strokes = new[] { Stroke(10, 10), Stroke(32, 10), Stroke(10, 160), Stroke(32, 160) };
        var results = AiInkLayout.Recognize(strokes, new TestRecognizer());
        if (results.Count != 2 || results.Any(r => r.Text.Text != "hello" || r.Strokes.Length != 2))
            throw new InvalidOperationException("AI row layout split pen lifts or merged separate rows.");
        foreach (var result in results) VerifyReplacement(result);
        if (results.Any(r => r.SizingComponents is not { Length: 2 } || r.PreparedFontSizeSamples is not null))
            throw new InvalidOperationException("Component size hints were lost or mislabeled as native recognized character geometry.");
        LegacyFontSizeTextSample[] sizes = [new('h', 1, 18, 10, 20), new('e', 1, 20, 32, 42)];
        var sized = AiInkLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("hello", .99f, 6) { FontSizeSamples = sizes }));
        if (sized.Count != 2 || sized.Any(r => r.PreparedFontSizeSamples is not { Length: 2 } metadata || !metadata.AsSpan().SequenceEqual(sizes) || ReferenceEquals(metadata, sizes)))
            throw new InvalidOperationException("Prepared size metadata did not survive recognition as an owned row snapshot.");
        if (AiInkLayout.Recognize(strokes, new TestRecognizer(.2f)).Count != 0)
            throw new InvalidOperationException("Uncertain AI recognition replaced handwriting.");
        if (AiRecognitionModels.Current is null && AiInkLayout.Recognize(strokes).Count != 0)
            throw new InvalidOperationException("Missing model replaced handwriting.");
        using var invalid = new MemoryStream("not a model"u8.ToArray());
        try { AiRecognitionModels.Install(invalid); throw new InvalidOperationException("Invalid model installation succeeded."); }
        catch (InvalidDataException) { }
        VerifyLocalModel();
        VerifyExperimentalEngine();
        VerifyMath();
        AiMathCorpusProbe.RunIfRequested();
        VerifyAutomatic();
        VerifyAcceptedMathCalculation();
        VerifyRawContentDispatch();
        Console.WriteLine("AI row layout, uncertainty, model validation, history and peer ink: passed.");
    }

    private sealed class CompletedRecognizer(TextRecognition value):IPositionedStrokeTextRecognizer
    {
        public string Language=>"prepared-completed";public bool ProducesPositionedWordRuns=>true;
        public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken token=default){token.ThrowIfCancellationRequested();return value;}
    }
    private static void VerifyExperimentalEngine()
    {
        var original = Program.Settings;
        try
        {
            var missing = System.Text.Json.JsonSerializer.Deserialize("{}", SettingsJsonContext.Default.AppSettings)!;
            if (missing.ExperimentalTextEngine) throw new InvalidOperationException("Existing settings opted into an unfinished engine.");
            var enabled = missing with { ExperimentalTextEngine = true };
            var json = System.Text.Json.JsonSerializer.Serialize(enabled, SettingsJsonContext.Default.AppSettings);
            if (System.Text.Json.JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings)?.ExperimentalTextEngine != true)
                throw new InvalidOperationException("Experimental engine setting did not survive serialization.");
            Program.Settings = enabled;
            var recognizer = AiRecognitionModels.Current;
            if (recognizer is not ExperimentalGlobalTextRecognizer || !ReferenceEquals(recognizer, AiRecognitionModels.Current))
                throw new InvalidOperationException("Experimental mode did not select the bundled independent engine.");
            InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
            InkStroke[] ink = [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),
                Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
            // Verify algorithm output independently of the public two-second
            // wall-clock deadline on cold/shared CI runners. The host deadline
            // stays unchanged; layout below consumes this same captured result.
            var actual = ((ExperimentalGlobalTextRecognizer)recognizer).RecognizeDiagnostic(ink);
            if (actual.Text != "МИР" || !actual.ExperimentalCandidateScore || !actual.NativeSelectedText || !float.IsFinite(actual.Confidence) || actual.Confidence is <0f or >1f)
                throw new InvalidOperationException($"Experimental global dictionary integration changed its actual diagnostic result: '{actual.Text}', score {actual.Confidence}.");
            var marks = ink.Select(stroke =>
            {
                var mark = new InkMark(InkTool.Pen, Colors.Black, 3, new(stroke.Points[0].X, stroke.Points[0].Y));
                foreach (var p in stroke.Points.Skip(1)) mark.AddPoint(new(p.X, p.Y)); return mark;
            }).ToArray();
            var rows = AiAutomaticLayout.Recognize(marks, new CompletedRecognizer(actual));
            if (rows.Count != 1 || rows[0].Text.Text != actual.Text || !rows[0].ExperimentalCandidateScore)
                throw new InvalidOperationException("Actual experimental candidate scores were lost by the application layout: " +
                    string.Join(" / ", rows.Select(r => $"'{r.Text.Text}' score={r.Confidence} experimental={r.ExperimentalCandidateScore}")));
            if (recognizer.Recognize(Enumerable.Repeat(ink[0],33).ToArray()).Text.Length != 0 ||
                recognizer.Recognize([Draw((float.NaN,0))]).Text.Length != 0)
                throw new InvalidOperationException("Unsupported experimental input replaced handwriting.");
            using var cancel = new CancellationTokenSource(); cancel.Cancel();
            try { recognizer.Recognize(ink,cancel.Token); throw new InvalidOperationException("Canceled experimental request succeeded."); }
            catch (OperationCanceledException) { }
            Program.Settings = missing;
            if (AiRecognitionModels.Current is ExperimentalGlobalTextRecognizer)
                throw new InvalidOperationException("Disabling experimental mode retained the wrong engine.");
            var english=AiRecognitionModels.BundledEnglish;
            if(english.Language!="en-US"||!ReferenceEquals(english,AiRecognitionModels.BundledEnglish)||
                english.Recognize([ink[0]]).Text!="M")
                throw new InvalidOperationException("Bundled raw English resource did not reach the independent application decoder.");
            if(!File.Exists(AiRecognitionModels.PathName)&&!ReferenceEquals(AiRecognitionModels.Current,english))
                throw new InvalidOperationException("Missing optional pack did not use bundled English.");
            Console.WriteLine($"Experimental bundled global dictionary engine → application text layout: '{actual.Text}', native candidate score {actual.Confidence}; settings, rollback, bounds and cancellation passed. Russian cursive accuracy is not asserted.");
        }
        finally { Program.Settings = original; }
    }

    private static void VerifyLocalModel()
    {
        if (Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_INPUT") is not { Length: > 0 } path) return;
        if (AiRecognitionModels.Current is null) throw new InvalidOperationException("The local model verification requires an installed model.");
        var marks = ReadStrokes(path);
        var result = AiInkLayout.Recognize(marks);
        var expected = Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_TEXT") ?? "hello";
        if (result.Count != 1 || result[0].Text.Text != expected || result[0].Strokes.Length != marks.Count)
            throw new InvalidOperationException($"Local AI integration mismatch: expected {expected}, actual {string.Join(" / ", result.Select(r => r.Text.Text))}.");
        VerifyReplacement(result[0]);
        Console.WriteLine($"Local raw-stroke model → application layout/history: {result[0].Text.Text}; {marks.Count} original strokes preserved.");
    }

    internal static List<InkMark> ReadStrokes(string path)
    {
        using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(path));
        var marks = new List<InkMark>();
        foreach (var stroke in json.RootElement.EnumerateArray())
        {
            if (stroke.ValueKind == System.Text.Json.JsonValueKind.Object && stroke.GetProperty("type").GetString() != "stroke") continue;
            InkMark? mark = null;
            foreach (var point in (stroke.ValueKind == System.Text.Json.JsonValueKind.Array ? stroke : stroke.GetProperty("pointers")).EnumerateArray())
            {
                var position = point.ValueKind == System.Text.Json.JsonValueKind.Array ? new Point(point[0].GetDouble(), point[1].GetDouble()) :
                    new Point(point.GetProperty("x").GetDouble(), point.GetProperty("y").GetDouble());
                if (mark is null) mark = new(InkTool.Pen, Colors.Black, 2, position); else mark.AddPoint(position);
            }
            if (mark is not null) marks.Add(mark);
        }
        return marks;
    }

    private static void VerifyMath()
    {
        var latex = "\\left\\{\\begin{matrix}\\sqrt{3}\\\\\\sqrt{6}\\end{matrix}\\right.";
        var strokes = new[] { Stroke(10, 10), Stroke(32, 160) };
        var calls = 0;
        MathRecognitionResult Recognize(IReadOnlyList<InkStroke> ink, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); calls++;
            if (ink.Count != 2 || ink[1].Points[0].Y != 160) throw new InvalidOperationException("Formula was split into text rows.");
            return new("structural tokens", latex, [], .95f, 10, true);
        }
        var result = AiMathLayout.Recognize(strokes, Recognize);
        if (calls != 1 || result.Count != 1 || result[0].Text.Text != latex || result[0].Text.Font != "STIX Two Math")
            throw new InvalidOperationException("Formula layout lost structure or fonts.");
        VerifyReplacement(result[0]);
        foreach (var rejected in new[]
        {
            new MathRecognitionResult("", latex, [], .2f, 10, true),
            new MathRecognitionResult("", latex, [], .95f, 10, false),
            new MathRecognitionResult("", "\\unsupported{x}", [], .95f, 10, true),
            new MathRecognitionResult("", "\\frac{1}", [], .95f, 10, true),
            new MathRecognitionResult("", " ", [], .95f, 10, true)
        })
            if (AiMathLayout.Recognize(strokes, (_, _) => rejected).Count != 0) throw new InvalidOperationException("Uncertain/unsupported math replaced strokes.");
        foreach (var accepted in new[] { "y=3x+2", "x_{1}^{2}", "\\frac{1}{x}", latex, "\\int x\\,dx".Replace("\\,", " ") })
            if (!MathTypesetting.Supported(accepted)) throw new InvalidOperationException("Supported math cannot be typeset.");
        if (MathTypesetting.Supported(new string('{', 33) + "x" + new string('}', 33))) throw new InvalidOperationException("Unbounded formula nesting.");
        VerifySymbolicMath();
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { AiMathLayout.Recognize(strokes, Recognize, canceled.Token); throw new InvalidOperationException("Math cancellation ignored."); }
        catch (OperationCanceledException) { }
        var previous = File.Exists(AiMathRecognitionModels.PathName) ? File.ReadAllBytes(AiMathRecognitionModels.PathName) : null;
        using var invalid = new MemoryStream("not a math model"u8.ToArray());
        try { AiMathRecognitionModels.Install(invalid); throw new InvalidOperationException("Invalid math model installation succeeded."); }
        catch (InvalidDataException) { }
        if (previous is not null && !previous.AsSpan().SequenceEqual(File.ReadAllBytes(AiMathRecognitionModels.PathName))) throw new InvalidOperationException("Invalid math import replaced the installed model.");
        if (Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_INPUT") is { Length: > 0 } path)
        {
            var actual = AiMathLayout.Recognize(ReadStrokes(path));
            var expected = Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_LATEX") ?? "y=3x+2";
            if (actual.Count != 1 || actual[0].Text.Text != expected) throw new InvalidOperationException($"Raw math application mismatch: expected {expected}; actual {string.Join(" / ", actual.Select(r => r.Text.Text))}");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw math → application/history/persistence: {expected}");
        }
        Console.WriteLine("Math expression structure, typesetting syntax, confidence, cancellation and atomic installation: passed.");
    }

    private static void VerifySymbolicMath()
    {
        var saved = Program.Settings;
        try
        {
            Program.Settings = saved with { GiacEnabled = false };
            foreach (var input in new[] { "1+1=", @"\int_1^{10} x^2 dx=" })
                if (AiMathLayout.WithSolvedResult(input) != input)
                    throw new InvalidOperationException("Disabled Giac used a fallback calculator.");
        }
        finally { Program.Settings = saved; }
        var failure = new AiInkLayout.Result([], new InkMark(InkTool.Text, Avalonia.Media.Colors.Black, 1, new Avalonia.Point())
            { Text = @"\int_1^{10}dx^2=" });
        AiMathLayout.ApplySolvedResult(failure, default, (_, _) => null);
        if (!failure.MathCalculationFailed || failure.Text.Text != @"\int_1^{10}dx^2=")
            throw new InvalidOperationException("Ambiguous integral was replaced or failed without a notice.");
        int calls = 0;
        InkMath.Solver.MathSolution? Calculate(string source, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); calls++;
            if (source == "2+2") return new("4", false);
            if (source == @"\int x^2 dx") return new(@"\frac{x^{3}}{3}+C", false);
            if (source == "x^2-1=0") return new(@"x\in \left\{-1,1\right\}", true);
            return null;
        }
        foreach (var original in new[] { @"\int x^2 dx", @"\int x^2 dx=123", "x^2-1=0", "2+2=5" })
        {
            var result = AiMathLayout.WithSolvedResult(original, solve: Calculate);
            if (result != original) throw new InvalidOperationException("CAS evaluated unsolicited input/written RHS.");
        }
        if (calls != 0) throw new InvalidOperationException("CAS started without a symbolic calculation request.");
        foreach (var original in new[] { @"\int x^2 dx=", "x^2-1=0=", "2+2=" })
        {
            var result = AiMathLayout.WithSolvedResult(original, solve: Calculate);
            if (result == original || !MathTypesetting.Supported(result)) throw new InvalidOperationException("Explicit symbolic result lost/typesetting unsupported: " + original + " → " + result);
        }
        if (calls != 3) throw new InvalidOperationException("Symbolic requests were not dispatched exactly once.");
        if (AiMathLayout.WithSolvedResult("x^2=", solve: (_, _) => new(@"\unsupported{x}", false)) != "x^2=")
            throw new InvalidOperationException("Unsupported CAS result replaced the source.");
        if (Environment.GetEnvironmentVariable("BLURMASK_TEST_GIAC") is { Length: > 0 } path)
        {
            var engine = new InkMath.Solver.SymbolicMathSolver(System.IO.Path.GetFullPath(path));
            InkMath.Solver.MathSolution? Real(string text, CancellationToken token) => engine.SolveAsync(text, token).GetAwaiter().GetResult();
            foreach (var source in new[] { "2+2=", @"\int_1^{10} x^2 dx=", @"\int_0^1 x^2 dx=", @"\frac{d}{dx}\sin(x)=", @"\lim_{x\to 0}\frac{\sin x}{x}=", @"lim_{x\to 0}\frac{\sin x}{x}=", @"lim_{n\to \infty }\frac{1}{n}=", @"\det\begin{matrix}1&2\\3&4\end{matrix}=", "x^2-1=0=", @"\begin{cases}x+y=3\\x-y=1\end{cases}=", @"\inv\begin{matrix}1&2\\3&4\end{matrix}=", @"\left\{\begin{matrix}x+y=3\\x-y=1\end{matrix}\right.=", @"\left[\begin{matrix}1&2\\3&4\end{matrix}\right]^{-1}=", @"\log_{2}{8}=", "log_{2}(8)=", "sin^{2}(x)+cos^{2}(x)=", @"\frac{\partial^2}{\partial x \partial y}x^2y^3=", @"\int_0^\infty e^{-x} dx=" })
                if (AiMathLayout.WithSolvedResult(source, solve: Real) == source) throw new InvalidOperationException("Real CAS → application result unchanged: " + source);
            Console.WriteLine("Native Giac → trailing equals policy/typesetting: passed.");
        }
        Console.WriteLine("Symbolic math dispatch, equations vs answers, unsupported output and written RHS preservation: passed.");
    }

    private sealed class TestRecognizer(float confidence = .99f) : IStrokeTextRecognizer
    {
        public string Language => "en";
        public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
        {
            if (strokes.Count != 2 || strokes.Any(s => s.Points.Count != 3))
                throw new InvalidOperationException("Original stroke points were lost before inference.");
            return new("hello", confidence, 6);
        }
    }
    private sealed class DelegateRecognizer(Func<IReadOnlyList<InkStroke>, CancellationToken, TextRecognition> recognize) : IStrokeTextRecognizer
    {
        public string Language => "en";
        public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default) => recognize(strokes, cancellationToken);
    }
    private static void VerifyAutomatic()
    {
        InkMath.Solver.MathSolution? PreparedSolve(string source, CancellationToken token)
        { token.ThrowIfCancellationRequested(); return source switch { "2+2" => new("4", false), @"\frac{1}{2}" => new("0.5", false), @"\sqrt{9}" => new("3", false), "2^{3}" => new("8", false), "42" => new("42", false), _ => null }; }
        var strokes = new[] { Stroke(10, 10), Stroke(32, 10) };
        var original = strokes.Select(InkDocument.ExportMark).ToArray();
        void Preserved()
        {
            if (!strokes.Select(InkDocument.ExportMark).Zip(original).All(p => NovaPen.Shared.BoardValidation.Same(p.First, p.Second)))
                throw new InvalidOperationException("Auto dispatch mutated source ink before committing a replacement.");
        }
        var textCalls = 0; var mathCalls = 0;
        var natural = new DelegateRecognizer((input, token) =>
        {
            token.ThrowIfCancellationRequested(); textCalls++;
            if (input.Count != 2 || input.Any(s => s.Points.Count != 3)) throw new InvalidOperationException("Auto dispatch lost pen chronology/points.");
            return new("hello", .99f, 6);
        });
        var hello = AiAutomaticLayout.Recognize(strokes, natural, (_, _) =>
        { mathCalls++; throw new InvalidOperationException("Confident natural text loaded the Math engine."); });
        if (textCalls != 1 || mathCalls != 0 || hello.Count != 1 || hello[0].Text.Text != "hello" || hello[0].Text.Font != "Inter")
            throw new InvalidOperationException("Auto dispatch did not preserve the fast natural-text route.");
        Preserved(); VerifyReplacement(hello[0]);

        foreach (var (latex, expected, grows) in new[]
        {
            ("2+2", "2+2", false), ("2+2=", "2+2=4", true), ("2+2=4", "2+2=4", false),
            ("2+2=5", "2+2=5", false), ("2+2= ", "2+2=4", true),
            ("\\frac{1}{2}", "\\frac{1}{2}", false), ("\\frac{1}{2}=", "\\frac{1}{2}=0.5", true),
            ("\\sqrt{9}", "\\sqrt{9}", false), ("\\sqrt{9}=", "\\sqrt{9}=3", true),
            ("2^{3}", "2^{3}", false), ("2^{3}=", "2^{3}=8", true),
            ("42", "42", false), ("42=", "42=42", true), ("1/0=", "1/0=", false),
            ("\\frac{1}{0}=", "\\frac{1}{0}=", false), ("\\sqrt{-1}=", "\\sqrt{-1}=", false),
            ("0^{0}=", "0^{0}=", false), ("x=", "x=", false),
            ("y=3x+2", "y=3x+2", false), ("\\frac{1}{x}", "\\frac{1}{x}", false)
        })
        {
            textCalls = mathCalls = 0;
            var text = new DelegateRecognizer((_, _) => { textCalls++; return new(latex == "2+2" ? "2+2" : "unclear", .99f, 6); });
            // Structural math fixtures need uncertain text so that natural text cannot take first refusal.
            if (latex != "2+2") text = new DelegateRecognizer((_, _) => { textCalls++; return new("unclear", .2f, 6); });
            var result = AiAutomaticLayout.Recognize(strokes, text, (input, token) =>
            {
                token.ThrowIfCancellationRequested(); mathCalls++;
                if (input.Count != 2) throw new InvalidOperationException("Automatic formula split original strokes.");
                return new(latex, latex, [], .95f, 10, true);
            }, mathSolver: PreparedSolve);
            if (textCalls != 1 || mathCalls != 1 || result.Count != 1 || result[0].Text.Text != expected || result[0].Text.Font != "STIX Two Math")
                throw new InvalidOperationException($"Automatic formula/calculation mismatch for {latex}.");
            Preserved(); VerifyReplacement(result[0], grows);
        }

        var uncertain = new DelegateRecognizer((_, _) => new("hello", .8f, 6));
        var ambiguous = AiAutomaticLayout.Recognize(strokes, uncertain, (_, _) => new("2+2", "2+2", [], .8f, 10, true));
        if (ambiguous.Count != 0) throw new InvalidOperationException("Both uncertain auto candidates replaced handwriting.");
        Preserved();
        var fallback = AiAutomaticLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("2+2=", .99f, 6)), (_, _) => null, mathSolver: PreparedSolve);
        if (fallback.Count != 1 || fallback[0].Text.Text != "2+2=4") throw new InvalidOperationException("Explicit numeric text fallback required a Math model.");
        Preserved(); VerifyReplacement(fallback[0], true);
        var unfinished = AiAutomaticLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("2+2", .99f, 6)), (_, _) => null);
        if (unfinished.Count != 1 || unfinished[0].Text.Text != "2+2") throw new InvalidOperationException("Text fallback calculated without a requested answer.");
        Preserved(); VerifyReplacement(unfinished[0]);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try
        {
            AiAutomaticLayout.Recognize(strokes, natural, (_, _) => throw new InvalidOperationException("Canceled auto request invoked Math."), canceled.Token);
            throw new InvalidOperationException("Automatic dispatch ignored pre-cancellation.");
        }
        catch (OperationCanceledException) { }
        using var during = new CancellationTokenSource();
        try
        {
            AiAutomaticLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("unclear", .2f, 6)), (_, _) =>
            { during.Cancel(); return new("2+2", "2+2", [], .95f, 10, true); }, during.Token);
            throw new InvalidOperationException("Automatic dispatch published a result after Math cancellation.");
        }
        catch (OperationCanceledException) { }
        Preserved();
        if (Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_INPUT") is { Length: > 0 } rawText)
        {
            var source = ReadStrokes(rawText);
            var expected = Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_TEXT") ?? "hello";
            var actual = AiAutomaticLayout.Recognize(source, mathRecognizer: (_, _) => throw new InvalidOperationException("Real confident English unnecessarily loaded Math."));
            if (actual.Count != 1 || actual[0].Text.Text != expected) throw new InvalidOperationException("Raw English automatic route differed.");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw English automatic dispatch: {expected}; Math skipped.");
        }
        if (Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_INPUT") is { Length: > 0 } rawMath)
        {
            var actual = AiAutomaticLayout.Recognize(ReadStrokes(rawMath));
            var expected = Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_LATEX") ?? "y=3x+2";
            if (actual.Count != 1 || actual[0].Text.Text != expected || actual[0].Text.Font != "STIX Two Math") throw new InvalidOperationException("Raw Math automatic route differed.");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw Math automatic dispatch: {expected}; symbolic equation preserved.");
        }
        Console.WriteLine("Automatic text/math dispatch, numeric answers, uncertainty, cancellation, history and persistence: passed.");
    }
    private static void VerifyAcceptedMathCalculation()
    {
        var ink = new[] { Stroke(10, 10), Stroke(32, 10) };
        var uncertainText = new DelegateRecognizer((_, _) => new("unclear", .2f, 6));
        var localIntegral = AiAutomaticLayout.Recognize(ink, uncertainText,
            (_, _) => new(@"\int_{1}^{10}x^2dx=", @"\int_{1}^{10}x^2dx=", [], .99f, 10, true), mathSolver: (_, _) => new("333", false));
        if (localIntegral.Count != 1 || localIntegral[0].Text.Text != @"\int_{1}^{10}x^2dx=333" || localIntegral[0].MathCalculationFailed)
            throw new InvalidOperationException("Accepted definite integral did not calculate through the automatic route.");
        VerifyReplacement(localIntegral[0], grows: true);
        int calls = 0;
        InkMath.Solver.MathSolution? Solve(string source, CancellationToken token)
        { token.ThrowIfCancellationRequested(); calls++; return source switch { "x^2" => new("x^{2}", false), "2+2" => new("4", false), _ => null }; }
        foreach (var (latex, confidence, expectedCount, expectedCalls) in new[]
        {
            ("x^2=", .84f, 0, 0), ("2+2=", .84f, 0, 0), ("abc", .95f, 0, 0),
            ("x^2=3", .95f, 1, 0), ("x^2=", .95f, 1, 1), ("2+2=", .95f, 1, 1)
        })
        {
            calls = 0;
            var result = AiAutomaticLayout.Recognize(ink, uncertainText,
                (_, _) => new(latex, latex, [], confidence, 10, true), mathSolver: Solve);
            if (result.Count != expectedCount || calls != expectedCalls)
                throw new InvalidOperationException("Unaccepted/unsolicited OCR started a calculation: " + latex);
            if (expectedCount == 1 && latex == "x^2=" && result[0].Text.Text != "x^2=x^{2}")
                throw new InvalidOperationException("Accepted symbolic OCR did not dispatch its trailing equals.");
            if (expectedCount == 1 && latex == "x^2=3" && result[0].Text.Text != latex)
                throw new InvalidOperationException("Written symbolic RHS was replaced.");
        }
        // One raw-content region can decode into separate color groups. A region
        // rejected for incomplete coverage must not calculate its accepted subset.
        var second = ink[1];
        var differentlyColored = new InkMark(InkTool.Pen, Colors.Black, second.Width, second.Points[0]);
        foreach (var point in second.Points.Skip(1)) differentlyColored.AddPoint(point);
        ink[1] = differentlyColored;
        calls = 0;
        var classification = new RawContentResult([], [new(InkCategory.Math, new[] { 0, 1 }, .99f)]);
        int decode = 0;
        var partial = AiAutomaticLayout.Recognize(ink, uncertainText,
            (_, _) => new("x^2=", "x^2=", [], ++decode == 1 ? .95f : .8f, 10, true),
            rawAnalyzer: (_, _) => classification, mathSolver: Solve);
        if (partial.Count != 0 || calls != 0) throw new InvalidOperationException("Incomplete raw-content region started CAS.");
        ink[1] = second;
        calls = 0;
        var numeric = AiAutomaticLayout.Recognize(ink, new DelegateRecognizer((_, _) => new("2+2=", .99f, 6)),
            (_, _) => null, rawAnalyzer: (_, _) => new([], [new(InkCategory.Text, new[] { 0, 1 }, .99f)]), mathSolver: Solve);
        if (numeric.Count != 1 || numeric[0].Text.Text != "2+2=4" || calls != 1)
            throw new InvalidOperationException("Accepted raw-text numeric request did not dispatch to the sole CAS adapter.");
        Console.WriteLine("CAS only after accepted OCR/complete region; uncertainty, written RHS, numeric CAS dispatch: passed.");
    }

    private static void VerifyRawContentDispatch()
    {
        var ink = new[] { Stroke(10, 10), Stroke(32, 10), Stroke(10, 160), Stroke(32, 160), Stroke(200, 200) };
        var original = ink.Select(InkDocument.ExportMark).ToArray();
        var textCalls = 0; var mathCalls = 0;
        var text = new DelegateRecognizer((strokes, _) =>
        {
            textCalls++;
            if (strokes.Count != 2 || strokes.Any(s => s.Points[0].Y != 10)) throw new InvalidOperationException("Text decoder received formula/drawing ink.");
            return new("hello", .99f, 6);
        });
        MathRecognitionResult? Math(IReadOnlyList<InkStroke> strokes, CancellationToken token)
        {
            mathCalls++; token.ThrowIfCancellationRequested();
            if (strokes.Count != 2 || strokes.Any(s => s.Points[0].Y != 160)) throw new InvalidOperationException("Math decoder received text/drawing ink.");
            return new("2+2=", "2+2=", [], .99f, 6, true);
        }
        var classification = new RawContentResult([], [new(InkCategory.Text, new[] { 0, 1 }, .99f), new(InkCategory.Math, new[] { 2, 3 }, .99f)]);
        var results = AiAutomaticLayout.Recognize(ink, text, Math, rawAnalyzer: (input, token) =>
        {
            token.ThrowIfCancellationRequested();
            if (input.Count != 5 || input.Any(s => s.Points.Count != 3)) throw new InvalidOperationException("Raw analyzer lost original strokes.");
            return classification;
        }, mathSolver: (_, _) => new("4", false));
        if (results.Count != 2 || results[0].Text.Text != "hello" || results[1].Text.Text != "2+2=4" || textCalls != 1 || mathCalls != 1 ||
            results.SelectMany(r => r.Strokes).Contains(ink[4])) throw new InvalidOperationException("Mixed raw-content dispatch converted a drawing or merged text and math.");
        foreach (var result in results) VerifyReplacement(result, result.Text.Font == "STIX Two Math");
        if (!ink.Select(InkDocument.ExportMark).Zip(original).All(p => NovaPen.Shared.BoardValidation.Same(p.First, p.Second))) throw new InvalidOperationException("Raw dispatch mutated source ink.");
        textCalls = mathCalls = 0;
        var guarded = new RawContentResult([], [new(InkCategory.Shape, new[] { 4 }, .99f), new(InkCategory.Text, new[] { 0, 1 }, .8f),
            new(InkCategory.Math, new[] { 2, 99 }, .99f), new(InkCategory.Text, new[] { 0, 0 }, .99f)]);
        if (AiAutomaticLayout.Recognize(ink, text, Math, rawAnalyzer: (_, _) => guarded).Count != 0 || textCalls != 0 || mathCalls != 0)
            throw new InvalidOperationException("Invalid or uncertain regions invoked a decoder.");
        var uncertain = new DelegateRecognizer((_, _) => new("hello", .2f, 6));
        if (AiAutomaticLayout.Recognize(ink[..2], uncertain, Math, rawAnalyzer: (_, _) => new RawContentResult([], [classification.Regions[0]])).Count != 0)
            throw new InvalidOperationException("Classifier confidence bypassed decoder uncertainty.");
        // Optional real model verifies the app boundary without installing assets or
        // creating UI. Each category stays in its own decoder and undo transaction.
        if (Environment.GetEnvironmentVariable("BLURMASK_RAW_TEST_PACK") is { Length: > 0 } pack &&
            Environment.GetEnvironmentVariable("BLURMASK_RAW_TEST_INPUT") is { Length: > 0 } path)
        {
            using var stream = File.OpenRead(pack);
            using var analyzer = RawContentAnalyzer.Load(stream, Environment.GetEnvironmentVariable("BLURMASK_RAW_TEST_RUNTIME"));
            var actual = AiAutomaticLayout.Recognize(ReadStrokes(path), rawAnalyzer: analyzer.Analyze);
            var expected = (Environment.GetEnvironmentVariable("BLURMASK_RAW_TEST_TEXT") ?? "hello|y=3x+2").Split('|');
            if (!actual.Select(r => r.Text.Text).SequenceEqual(expected)) throw new InvalidOperationException($"Raw model → mixed app result differs: {string.Join('|', actual.Select(r => r.Text.Text))}");
            foreach (var result in actual) VerifyReplacement(result);
            Console.WriteLine("Actual Raw Content2 → mixed text/math decoders → replacement/history PASS");
        }
        Console.WriteLine("Mixed raw-content dispatch, drawing preservation, region guards, confidence and undo: passed.");
    }

    private static InkMark Stroke(double x, double y)
    {
        var mark = new InkMark(InkTool.Pen, Colors.Red, 3, new(x, y));
        mark.AddPoint(new(x + 10, y + 30)); mark.AddPoint(new(x + 20, y + 5)); return mark;
    }
    private static void VerifyReplacement(AiInkLayout.Result result, bool grows = false)
    {
        var strokes = result.Strokes; var text = result.Text;
        var bounds = strokes.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
        var document = new InkDocument(); foreach (var stroke in strokes) document.Add(stroke);
        var peer = new InkMark(InkTool.Pen, Colors.Blue, 3, new(200, 200)); document.Add(peer);
        var correctBounds = grows ? text.Bounds.TopLeft == bounds.TopLeft && text.Bounds.Height == bounds.Height && text.Bounds.Width > bounds.Width : text.Bounds == bounds;
        if (!document.ReplaceRecognized(strokes, text) || document.Marks.Count != 2 || document.Marks[0] != text ||
            document.Marks[1] != peer || !correctBounds || text.Id != strokes[0].Id || text.Color != strokes[0].Color)
            throw new InvalidOperationException($"AI conversion lost bounds, color, layers or peer ink: text={text.Text}, grows={grows}, source={bounds}, replacement={text.Bounds}, marks={document.Marks.Count}, identity={text.Id == strokes[0].Id}, color={text.Color == strokes[0].Color}.");
        document.Undo();
        if (!document.Marks.SequenceEqual(strokes.Append(peer))) throw new InvalidOperationException("AI undo did not restore handwriting.");
        document.Redo();
        if (!document.Marks.SequenceEqual(new[] { text, peer })) throw new InvalidOperationException("AI redo did not restore text.");
        document.Undo(); document.Undo(); document.Undo();
        if (document.ReplaceRecognized(strokes, text)) throw new InvalidOperationException("Stale AI result resurrected deleted handwriting.");
        var imported = new InkDocument(); imported.Import(new() { InkDocument.ExportMark(text) });
        if (imported.Marks.Single().Bounds != text.Bounds || imported.Marks.Single().Text != text.Text || imported.Marks.Single().Font != text.Font || !NovaPen.Shared.BoardValidation.Valid(InkDocument.ExportMark(text)))
            throw new InvalidOperationException("Saved/shared AI text lost its size.");
    }
}
