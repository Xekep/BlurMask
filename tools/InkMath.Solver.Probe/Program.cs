using InkMath.Solver;
using System.Diagnostics;

var fixtures = new (string Input, string Cas)[]
{
    (@"\int x^2 dx", "integrate((x^2),x)"),
    (@"\int_0^1 x^2 dx", "integrate((x^2),x,0,1)"),
    (@"\int_{1}^{10} x^2 dx", "integrate((x^2),x,1,10)"),
    (@"\int\limits_{1}^{10} x^2 \,\mathrm{dx}", "integrate((x^2),x,1,10)"),
    (@"\int^ {10}_{1} x^2 \mathrm{d}x", "integrate((x^2),x,1,10)"),
    (@"\int_1^{10} dx", "integrate(1,x,1,10)"),
    (@"\int_1^{10} d{x}", "integrate(1,x,1,10)"),
    (@"\int_1^{10} x^2 \text{dx}", "integrate((x^2),x,1,10)"),
    (@"\int_1^{10} \theta^2 d\theta", "integrate((theta^2),theta,1,10)"),
    (@"\frac{d}{dx}\sin(x)", "diff(sin(x),x,1)"),
    (@"\frac{d^2}{dx^2}x^3", "diff((x^3),x,2)"),
    (@"\lim_{x\to 0}\frac{\sin x}{x}", "limit((sin(x)/x),x,0)"),
    (@"\sum_{k=1}^{10}k", "sum(k,k,1,10)"),
    (@"\prod_{k=1}^{4}k", "product(k,k,1,4)"),
    (@"x^2-1=0", "(((x^2)-1)=0)"),
    (@"\det\begin{matrix}1&2\\3&4\end{matrix}", "det([[1,2],[3,4]])"),
    (@"\frac{1}{2}+\sqrt{4}", "((1/2)+sqrt(4))"),
    ("-x^2", "(-1*(x^2))"), ("2x(x+1)", "((2*x)*(x+1))"),
    (@"\sqrt[3]{8}", "(8^(1/3))"),
    (@"\begin{cases}x+y=3\\x-y=1\end{cases}", "[[((x+y)=3)],[((x-y)=1)]]"),
    (@"\left\{\begin{matrix}x+y=3\\x-y=1\end{matrix}\right.", "[[((x+y)=3)],[((x-y)=1)]]"),
    (@"\left|\begin{matrix}1&2\\3&4\end{matrix}\right|", "det([[1,2],[3,4]])"),
    (@"\left[\begin{matrix}1&2\\3&4\end{matrix}\right]^{-1}", "([[1,2],[3,4]]^(-1*1))"),
    (@"\sin\left(x\right)", "sin(x)"),
    (@"\log_{2}{8}", "(ln(8)/ln(2))"),
    ("log_{2}(8)", "(ln(8)/ln(2))"),
    ("sin^{2}(x)", "(sin(x)^2)"),
    (@"\log_{x+1}{8}", "(ln(8)/ln((x+1)))"),
    (@"\frac{\partial^2}{\partial x \partial y}x^2y^3", "diff(diff(((x^2)*(y^3)),x,1),y,1)"),
    (@"\frac{\partial^3}{\partial x^2 \partial y}x^3y^2", "diff(diff(((x^3)*(y^2)),x,2),y,1)")
};
foreach (var (input, expected) in fixtures)
{
    var actual = new MathExpressionParser(input).Parse().Cas;
    if (actual != expected) throw new Exception($"Math grammar differs for {input}: {actual}");
}
foreach (var forbidden in new[] { @"\int_1^{10} dx^2", @"\int_1^{10} x dx_2", @"\int_1 x^2 dx", @"\system{whoami}", "read(\"secret\")", "x;system(1)", "x:=1", "__import__('os')", "x\0", new string('-', 4096) + "x", new string('-', 200) + "x", string.Join('^', Enumerable.Repeat("x", 200)), new string('(', 100) + "x" + new string(')', 100), "2+", @"\begin{matrix}1&2\\3\end{matrix}" })
{
    try { new MathExpressionParser(forbidden).Parse(); throw new Exception("Unsafe/invalid input accepted: " + forbidden); }
    catch (FormatException) { }
}
Console.WriteLine($"PASS math grammar: {fixtures.Length} translations, command injection and complexity guards");
var polynomialFixtures = new (string Input, string Expected)[]
{
    (@"\int_{1}^{10} x^2 dx", "333"),
    (@"\int\limits_{1}^{10} x^2 \mathrm{dx}", "333"),
    (@"∫_1^{10} x^2 dx", "333"),
    (@"\int^{10}_{1} x^2 \text{d}x", "333"),
    (@"\int_1^{10} dx", "9"),
    (@"\int_1^{10} d{x}", "9"),
    (@"\int_1^{10} \theta^2 d\theta", "333"),
    (@"\int_1^{10} 5 dx", "45"),
    (@"\int_1^{10} (x+1)^2 dx", "441"),
    (@"\int_1^{10} (x^2-2x+1) dx", "243"),
    (@"\int_{-0.5}^{0.5} x^2 dx", @"\frac{1}{12}"),
    (@"\int_{10}^{1} x^2 dx", "-333"),
    (@"\int_2^2 x^2 dx", "0"),
    (@"\int_0^2 (2x^3-x+1) dx", "8"),
    (@"\int_1^{10} \frac{x^2}{2} dx", @"\frac{333}{2}"),
    (@"\int_0^1 (x+1)^4 dx", @"\frac{31}{5}"),
    (@"\int_0^3 2 dx", "6"),
    (@"\int_0^3 0 dx", "0"),
    (@"\int_1^{10} x^0 dx", "9"),
    (@"\int_1^2 0.123x dx", @"\frac{369}{2000}")
};
foreach (var (input, expected) in polynomialFixtures)
    if (PolynomialIntegralSolver.Solve(input)?.LaTex != expected) throw new Exception("Built-in exact integral differs: " + input);
foreach (var input in new[] { @"\int_0^1 \sin x dx", @"\int_0^1 \frac{1}{x}dx", @"\int_0^a x dx", @"\int_0^1 xy dx", @"\int_0^1 0^0 dx", @"\int_0^1 x^{33} dx", @"\int_0^1 \frac{1}{0}dx", @"\int_1^{10} dx^2", @"\int_0^1 e^2 de", "1+1", @"\system{whoami}" })
    if (PolynomialIntegralSolver.Solve(input) is not null) throw new Exception("Unsupported/ambiguous local integral computed: " + input);
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try { PolynomialIntegralSolver.Solve(polynomialFixtures[0].Input, canceled.Token); throw new Exception("Local integral cancellation lost."); }
    catch (OperationCanceledException) { }
}
Console.WriteLine($"PASS built-in exact rational integrals: {polynomialFixtures.Length} fixtures, unsupported/ambiguous notation and cancellation");
if (new MathExpressionParser("plot(x)").Parse().Cas.Contains("plot(", StringComparison.Ordinal)) throw new Exception("Arbitrary function name passed to CAS.");
if (args.Length == 0) return;
var solver = new SymbolicMathSolver(Path.GetFullPath(args[0]));
if (args.Length > 1) { Console.WriteLine((await solver.SolveAsync(args[1]))?.LaTex ?? "unsolved"); return; }
var watch = Stopwatch.StartNew();
var actualFixtures = new (string Input, string Expected)[]
{
    (@"\int x^2 dx", @"\frac{x^{3}}{3}+C"),
    (@"\int_0^1 x^2 dx", @"\frac{1}{3}"),
    (@"\int_{1}^{10} x^2 dx", "333"),
    (@"\int\limits_1^{10} x^2 \mathrm{dx}", "333"),
    (@"\int_1^{10} dx", "9"),
    (@"\int_1^{10} \theta^2 d\theta", "333"),
    (@"\frac{d}{dx}\sin(x)", @"\cos x"),
    (@"\frac{d^2}{dx^2}x^3", "6 x"),
    (@"\lim_{x\to 0}\frac{\sin x}{x}", "1"),
    (@"\sum_{k=1}^{10}k", "55"),
    (@"\prod_{k=1}^{4}k", "24"),
    (@"\det\begin{matrix}1&2\\3&4\end{matrix}", "-2"),
    (@"x^2-1=0", @"x\in \left\{-1,1\right\}"),
    (@"\frac{1}{2}+\sqrt{4}", @"\frac{5}{2}"),
    (@"\begin{cases}x+y=3\\x-y=1\end{cases}", @"(x,y)\in \left\{\left[2,1\right]\right\}"),
    (@"\inv\begin{matrix}1&2\\3&4\end{matrix}", @"\left[\begin{array}{cc}-2&1\\\frac{3}{2}&-\frac{1}{2}\end{array}\right]"),
    (@"\sin^2 x+\cos^2 x", "1"),
    (@"\operatorname{factor}(x^2+2x+1)", @"\left(x+1\right)^{2}"),
    (@"\operatorname{expand}((x+1)^2)", "x^{2}+2 x+1"),
    (@"\left\{\begin{matrix}x+y=3\\x-y=1\end{matrix}\right.", @"(x,y)\in \left\{\left[2,1\right]\right\}"),
    (@"\left|\begin{matrix}1&2\\3&4\end{matrix}\right|", "-2"),
    (@"\left[\begin{matrix}1&2\\3&4\end{matrix}\right]^{-1}", @"\left[\begin{array}{cc}-2&1\\\frac{3}{2}&-\frac{1}{2}\end{array}\right]"),
    (@"\log_{2}{8}", "3"),
    ("log_{2}(8)", "3"),
    ("sin^{2}(x)+cos^{2}(x)", "1"),
    (@"\int_0^\infty e^{-x} dx", "1"),
    (@"\frac{\partial^2}{\partial x \partial y}x^2y^3", "6 x y^{2}"),
    (@"\frac{\partial^3}{\partial x^2 \partial y}x^3y^2", "12 x y"),
    (@"\operatorname{rank}\begin{matrix}1&2\\2&4\end{matrix}", "1")
};
foreach (var (input, expected) in actualFixtures)
{
    var value = await solver.SolveAsync(input);
    if (!SameResult(value?.LaTex, expected, value?.IsEquation == false)) throw new Exception($"Native CAS differs: {input} → {value?.LaTex ?? "null"}; expected {expected}");
}
foreach (var (input, expected) in polynomialFixtures)
{
    // Giac treats decimal literals as floating point. Compare exact equivalent
    // rational inputs against our decimal-as-rational integration path.
    var exactInput = input.Replace("-0.5", "-1/2", StringComparison.Ordinal).Replace("0.5", "1/2", StringComparison.Ordinal)
        .Replace("0.123", @"\frac{123}{1000}", StringComparison.Ordinal);
    var actual = (await solver.SolveAsync(exactInput))?.LaTex;
    if (actual != expected) throw new Exception($"Independent Giac/local polynomial comparison differs: {input} → {actual}; expected {expected}");
}
using var cancel = new CancellationTokenSource(); cancel.Cancel();
try { await solver.SolveAsync("1+1", cancel.Token); throw new Exception("Cancellation lost."); } catch (OperationCanceledException) { }
if (await solver.SolveAsync(@"\system{whoami}") is not null) throw new Exception("Unsafe input executed.");
if (await solver.SolveAsync("1/0") is not null) throw new Exception("Undefined arithmetic replaced the input.");
if (await solver.SolveAsync("x=x") is not null) throw new Exception("Unresolved/identity root echoed as its own solution.");
if (await solver.SolveAsync("x") is not null) throw new Exception("Unchanged variable treated as an answer.");
using var activeCancellation = new CancellationTokenSource();
Process? child = null;
var cancellableSolver = new SymbolicMathSolver(Path.GetFullPath(args[0]))
{
    ProcessStarted = id => { child = Process.GetProcessById(id); activeCancellation.Cancel(); }
};
var cancellationWatch = Stopwatch.StartNew();
try { await cancellableSolver.SolveAsync("1+1", activeCancellation.Token); throw new Exception("Running calculation did not cancel."); } catch (OperationCanceledException) { }
if (child is null || !child.HasExited) throw new Exception("Canceled CAS child was not reaped.");
child.Dispose();
if (cancellationWatch.Elapsed > TimeSpan.FromSeconds(2)) throw new Exception("Native child cancellation was slow.");
Console.WriteLine($"PASS native Giac: {actualFixtures.Length} calculus/algebra fixtures, cancellation and undefined/injection guards; {watch.ElapsedMilliseconds} ms");

static bool SameResult(string? actual, string expected, bool allowStructural)
{
    if (actual == expected) return true;
    if (actual is null || !allowStructural) return false;
    // Keep numerical/variable/operator goldens unchanged. Different Giac builds
    // add function parentheses or explicit multiplication in otherwise identical
    // expressions. Root sets and matrices still require their exact notation.
    try { return new MathExpressionParser(actual).Parse().Cas == new MathExpressionParser(expected).Parse().Cas; }
    catch (FormatException) { return false; }
}
