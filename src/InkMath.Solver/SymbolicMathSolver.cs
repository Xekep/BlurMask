using System.Diagnostics;
using System.Text;

namespace InkMath.Solver;

public sealed record MathSolution(string LaTex, bool IsEquation);

/// <summary>Local Giac CAS, isolated from the caller and compatible with Native AOT.</summary>
public sealed class SymbolicMathSolver
{
    private readonly string _executable;
    // Headless diagnostic observer: verifies ownership/cleanup of a live OS
    // process without relying on a particular CAS expression being slow.
    internal Action<int>? ProcessStarted { get; init; }
    public SymbolicMathSolver(string executable)
    {
        _executable = Path.GetFullPath(executable);
        if (!Path.IsPathFullyQualified(executable) || !File.Exists(_executable)) throw new ArgumentException("An existing absolute Giac CLI path is required.", nameof(executable));
        if (Path.GetFileNameWithoutExtension(_executable) is not ("icas" or "giac")) throw new ArgumentException("Select the Giac command-line executable, not the Xcas GUI.", nameof(executable));
    }

    /// <summary>Returns null for unsupported/unsolved input or a resource limit. Cancellation propagates.</summary>
    public async Task<MathSolution?> SolveAsync(string latex, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Expression expression;
        try { expression = new MathExpressionParser(latex).Parse(); }
        catch (FormatException) { return null; }
        var variables = expression.Variables.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (variables.Length > 8) return null;
        bool equation = expression is Binary { Operator: "=" };
        Expression[]? equations = null;
        if (expression is Matrix matrix && matrix.Rows.SelectMany(r => r).All(e => e is Binary { Operator: "=" }))
        { equations = matrix.Rows.SelectMany(r => r).ToArray(); equation = true; }
        if (equation && variables.Length == 0) return null;
        bool checkedIntegral = ContainsDefiniteIntegral(expression);
        // Arithmetic/function forms containing infinity have not established
        // the explicit bound contract used by the whole-line split below.
        // Refuse rather than letting an unrecognized whole line become a PV.
        if (checkedIntegral && DefiniteIntegrals(expression).Any(c =>
            c.Arguments.Skip(2).Any(bound => ContainsInfinity(bound) && InfinitySign(bound) == 0))) return null;
        // This generated name cannot be produced by the admitted math grammar.
        const string integralValue = "blurmaskintegraltemporary";
        string operand = checkedIntegral ? integralValue : expression.Cas;
        var operation = equation ? "solve(" + (checkedIntegral ? integralValue : equations is null ? expression.Cas : "[" + string.Join(',', equations.Select(e => e.Cas)) + "]") +
            "," + (variables.Length == 1 ? variables[0] : "[" + string.Join(',', variables) + "]") + ")" :
            expression is Call ? operand : "simplify(" + operand + ")";
        var command = "latex(" + operation + ")";
        if (checkedIntegral)
        {
            // Pinned Giac enables its own interior singularity checks in
            // GeoGebra calculation mode. Reset before solve/LaTeX so roots and
            // complex-unit presentation retain the existing mode0 contract.
            var preparation = new StringBuilder();
            int temporaryIndex = 0;
            string source = equations is null ? IntegralCas(expression, preparation, ref temporaryIndex) :
                "[" + string.Join(',', equations.Select(e => IntegralCas(e, preparation, ref temporaryIndex))) + "]";
            if (temporaryIndex != 0 && !equation) command = "latex(simplify(" + integralValue + "))";
            string certificate = "try_catch(print(\"BLURMASK_INTEGRAL_DOMAIN=\"+string(when(" +
                string.Join(" && ", DefiniteIntegrals(expression).Select(IntegralCertificate)) +
                ",1,0))),blurmaskdiagnosticerror,print(\"BLURMASK_INTEGRAL_DOMAIN=0\"))\n";
            string endpoints = "try_catch(print(\"BLURMASK_INTEGRAL_ENDPOINTS=\"+string(" +
                string.Join("*", DefiniteIntegrals(expression).Select(IntegralEndpointCertificate)) +
                ")),blurmaskdiagnosticerror,print(\"BLURMASK_INTEGRAL_ENDPOINTS=0\"))\n";
            command = "calc_mode(0)\n" + EndpointFiniteHelper + endpoints + "calc_mode(1)\n" + certificate + preparation + integralValue + ":=(" + source + ")\ncalc_mode(0)\n" + command;
            if (command.Length > 16384) return null;
        }
        string? result = await ExecuteAsync(command, cancellationToken, checkedIntegral).ConfigureAwait(false);
        if (result is null || result.Length > 3500 || result.Contains("undef", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("operatorname", StringComparison.Ordinal) || result.Contains("\\int", StringComparison.Ordinal) || result.Contains("\\lim", StringComparison.Ordinal) ||
            result.Contains("\\mathrm", StringComparison.Ordinal) || result.Contains("\\infty", StringComparison.Ordinal) && expression is not Call { Name: "limit" }) return null;
        if (!equation && variables.Length > 0)
        {
            // An unchanged symbolic expression is not a computed answer (x= must
            // not become x=x). Compare generated math ASTs, not LaTeX formatting.
            try { if (new MathExpressionParser(result).Parse().Cas == expression.Cas) return null; }
            catch (FormatException) { }
        }
        if (equation)
        {
            if (variables.Length == 1)
            {
                var symbol = VariableLaTex(variables[0]).TrimEnd();
                if (result == "\\left[" + symbol + "\\right]" || result == "[" + symbol + "]") return null;
            }
            // Giac returns a list of roots. Use set membership, not an equality
            // between the original equation and a list (or an interval).
            result = RootSet(result);
            var names = variables.Length == 1 ? VariableLaTex(variables[0]) : "(" + string.Join(',', variables.Select(VariableLaTex)) + ")";
            result = names + "\\in " + result;
        }
        else if (expression is Call { Name: "integrate", Arguments.Length: 2 }) result += "+C";
        return new(result, equation);
    }

    private static bool ContainsDefiniteIntegral(Expression expression) => expression switch
    {
        Call { Name: "integrate", Arguments.Length: 4 } => true,
        Call call => call.Arguments.Any(ContainsDefiniteIntegral),
        Binary binary => ContainsDefiniteIntegral(binary.Left) || ContainsDefiniteIntegral(binary.Right),
        Matrix matrix => matrix.Rows.SelectMany(row => row).Any(ContainsDefiniteIntegral),
        _ => false
    };

    private static IEnumerable<Call> DefiniteIntegrals(Expression expression)
    {
        if (expression is Call { Name: "integrate", Arguments.Length: 4 } integral) yield return integral;
        IEnumerable<Expression> children = expression switch
        {
            Call call => call.Arguments,
            Binary binary => [binary.Left, binary.Right],
            Matrix matrix => matrix.Rows.SelectMany(row => row),
            _ => []
        };
        foreach (var child in children)
            foreach (var nested in DefiniteIntegrals(child)) yield return nested;
    }

    internal static bool RejectsUnverifiedIntegralDiagnostics(string diagnostics)
        => diagnostics.Contains("No checks were made for singular points of antiderivative", StringComparison.Ordinal) &&
            !diagnostics.Split('\n').Any(line => line.Trim() == "BLURMASK_INTEGRAL_DOMAIN=1");

    internal static bool RejectsUnverifiedEndpointDiagnostics(string diagnostics)
        => !diagnostics.Split('\n').Any(line => line.Trim() == "BLURMASK_INTEGRAL_ENDPOINTS=1");

    private const string EndpointFiniteHelper =
        "blurmaskendpointfinite:=blurmasklimittemporary->when(type(evalf(blurmasklimittemporary))==DOM_FLOAT," +
        "when(isfinite(blurmasklimittemporary),1,0),when(type(evalf(blurmasklimittemporary))==DOM_INT,1," +
        "when(type(evalf(blurmasklimittemporary))==DOM_COMPLEX,when(isfinite(blurmasklimittemporary),1,0),0)))\n";

    private static string IntegralEndpointCertificate(Call integral)
    {
        string body = integral.Arguments[0].Cas, variable = integral.Arguments[1].Cas;
        string Bound(Expression bound) => InfinitySign(bound) switch
        {
            1 => "+infinity", -1 => "-infinity", _ => bound.Cas
        };
        string lower = Bound(integral.Arguments[2]), upper = Bound(integral.Arguments[3]);
        string primitive = "integrate(" + body + "," + variable + ")";
        // Native finite-bound polynomial / nonsingular-primitive cases retain
        // symbolic parameters. Otherwise require finite numeric one-sided limits.
        string regular = "try_catch(when(isfinite(" + lower + ") && isfinite(" + upper + ")," +
            "when(is_polynomial(" + body + "," + variable + "),1,when(size(singular(" + primitive + "," + variable + "))==0,1,0)),0),blurmaskdiagnosticerror,0)";
        string Limit(string bound, int direction) => "blurmaskendpointfinite(limit(" + primitive + "," + variable + "," + bound + "," + direction + "))";
        // Checking both sides is conservative for reversed and unknown bounds;
        // no host-side symbolic ordering or antiderivative is introduced.
        return "when(" + regular + "==1,1,min(" + Limit(lower, 1) + "," + Limit(lower, -1) + "," + Limit(upper, 1) + "," + Limit(upper, -1) + "))";
    }

    private static bool ContainsInfinity(Expression expression) => expression switch
    {
        Variable { Name: "infinity" } => true,
        Call call => call.Arguments.Any(ContainsInfinity),
        Binary binary => ContainsInfinity(binary.Left) || ContainsInfinity(binary.Right),
        Matrix matrix => matrix.Rows.SelectMany(row => row).Any(ContainsInfinity),
        _ => false
    };

    private static string NativeNumericTruth(string condition)
        => "when(type(" + condition + ")==DOM_INT,when(" + condition + ",1,0),0)";

    private static string IntegralCertificate(Call integral)
    {
        const string pole = "blurmaskpoletemporary";
        string body = integral.Arguments[0].Cas, variable = integral.Arguments[1].Cas;
        string Bound(Expression bound) => InfinitySign(bound) switch
        {
            1 => "+infinity", -1 => "-infinity", _ => bound.Cas
        };
        string lower = Bound(integral.Arguments[2]), upper = Bound(integral.Arguments[3]);
        // Unknown symbolic inequalities are NOT native true. Reduce only
        // integer truth values to 0/1 before combining branches numerically.
        string nonreal = "when(type(im(" + pole + "))==DOM_INT,when(im(" + pole + ")!=0,1,0)," +
            "when(type(im(" + pole + "))==DOM_FLOAT,when(abs(im(" + pole + "))>0,1,0),0))";
        string below = NativeNumericTruth(pole + "<" + lower) + "*" + NativeNumericTruth(pole + "<" + upper);
        string above = NativeNumericTruth(pole + ">" + lower) + "*" + NativeNumericTruth(pole + ">" + upper);
        string outside = "product(map(" + pole + "->max(" + nonreal + "," + below + "," + above + "),singular(" + body + "," + variable + ")))";
        return "(is_polynomial(" + body + "," + variable + ") || " + outside + "==1)";
    }

    private static int InfinitySign(Expression expression) => expression switch
    {
        Variable { Name: "infinity" } => 1,
        Binary { Operator: "*", Left: Number { Value: "-1" }, Right: var right } => -InfinitySign(right),
        _ => 0
    };

    private static string IntegralCas(Expression expression, StringBuilder preparation, ref int temporaryIndex)
    {
        // Store each whole-line integrand once: nested rewrites grow linearly,
        // rather than recursively duplicating both copies of an inner integral.
        if (expression is Call { Name: "integrate", Arguments.Length: 4 } integral &&
            InfinitySign(integral.Arguments[2]) * InfinitySign(integral.Arguments[3]) == -1)
        {
            string body = IntegralCas(integral.Arguments[0], preparation, ref temporaryIndex);
            string variable = integral.Arguments[1].Cas;
            string temporary = "blurmaskintegrandtemporary" + temporaryIndex++;
            preparation.Append(temporary).Append(":=(").Append(body).Append(")\n");
            string lower = InfinitySign(integral.Arguments[2]) < 0 ? "-infinity" : "+infinity";
            string upper = InfinitySign(integral.Arguments[3]) < 0 ? "-infinity" : "+infinity";
            return "(integrate(" + temporary + "," + variable + "," + lower + ",0)+integrate(" + temporary + "," + variable + ",0," + upper + "))";
        }
        if (expression is Binary binary)
            return "(" + IntegralCas(binary.Left, preparation, ref temporaryIndex) + binary.Operator + IntegralCas(binary.Right, preparation, ref temporaryIndex) + ")";
        if (expression is Call call)
        {
            var arguments = new string[call.Arguments.Length];
            for (int i = 0; i < arguments.Length; i++) arguments[i] = IntegralCas(call.Arguments[i], preparation, ref temporaryIndex);
            return call.Name + "(" + string.Join(',', arguments) + ")";
        }
        if (expression is Matrix matrix)
        {
            var rows = new List<string>();
            foreach (var row in matrix.Rows)
            {
                var entries = new List<string>();
                foreach (var item in row) entries.Add(IntegralCas(item, preparation, ref temporaryIndex));
                rows.Add("[" + string.Join(',', entries) + "]");
            }
            return "[" + string.Join(',', rows) + "]";
        }
        return expression.Cas;
    }

    private static string RootSet(string latex)
    {
        if (latex is "[]" or "\\left[\\right]") return "\\varnothing";
        if (latex.StartsWith("\\left[", StringComparison.Ordinal) && latex.EndsWith("\\right]", StringComparison.Ordinal))
            return "\\left\\{" + latex[6..^7] + "\\right\\}";
        if (latex.StartsWith('[') && latex.EndsWith(']')) return "\\left\\{" + latex[1..^1] + "\\right\\}";
        // A matrix is a row per solution tuple; enclose the whole result as a set.
        return "\\left\\{" + latex + "\\right\\}";
    }

    private static string VariableLaTex(string variable)
    {
        int underscore = variable.IndexOf('_');
        var name = underscore < 0 ? variable : variable[..underscore];
        if (name.Length > 1) name = "\\" + name + " ";
        return underscore < 0 ? name : name + "_{" + variable[(underscore + 1)..] + "}";
    }

    private async Task<string?> ExecuteAsync(string command, CancellationToken token, bool guardIntegralWarnings = false)
    {
        var parent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "BlurMask-Giac"));
        var directory = Path.GetFullPath(Path.Combine(parent, Guid.NewGuid().ToString("N")));
        if (!directory.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new IOException("Invalid solver working directory.");
        Directory.CreateDirectory(directory);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(TimeSpan.FromSeconds(5));
        var start = new ProcessStartInfo(_executable)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
            RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = directory,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        start.ArgumentList.Add("--sage");
        start.Environment["XCAS_HOME"] = directory;
        start.Environment["GIAC_THREADS"] = "1";
        start.Environment["GIAC_NO_TIME"] = "1";
        // The pinned singularity diagnostic is translated by gettext. Keep
        // the child protocol stable independently of the desktop UI language.
        start.Environment["LC_ALL"] = "C";
        start.Environment["LANG"] = "C";
        start.Environment["LANGUAGE"] = "C";
        foreach (var name in new[] { "OMP_NUM_THREADS", "OPENBLAS_NUM_THREADS", "MKL_NUM_THREADS", "VECLIB_MAXIMUM_THREADS", "GOTO_NUM_THREADS" }) start.Environment[name] = "1";
        // No external display is needed by a command-line calculation.
        start.Environment.Remove("DISPLAY"); start.Environment.Remove("WAYLAND_DISPLAY");
        using var process = new Process { StartInfo = start };
        Task<string>? output = null, errors = null;
        Task? monitor = null;
        try
        {
            if (!process.Start()) return null;
            ProcessStarted?.Invoke(process.Id);
            output = ReadBounded(process.StandardOutput, deadline);
            errors = ReadBounded(process.StandardError, deadline);
            monitor = LimitMemory(process, deadline);
            await process.StandardInput.WriteLineAsync(command.AsMemory(), deadline.Token).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);
            var stdout = await output.ConfigureAwait(false); var stderr = await errors.ConfigureAwait(false);
            // A pinned native diagnostic reports unverified symbolic singularities.
            // Require a sufficient native diagnostic for EVERY checked integral;
            // do not publish an unconditional answer when Giac cannot check them.
            if (guardIntegralWarnings && (RejectsUnverifiedEndpointDiagnostics(stderr) || RejectsUnverifiedIntegralDiagnostics(stderr))) return null;
            if (process.ExitCode != 0) return null;
            string? value = null;
            foreach (var line in stdout.Split('\n'))
            {
                var candidate = line.Trim();
                // Giac's quoted LaTeX is not JSON: backslashes are literal.
                if (candidate.Length >= 2 && candidate[0] == '"' && candidate[^1] == '"') value = candidate[1..^1];
            }
            return value is { Length: > 0 } && !value.Any(char.IsControl) ? value : null;
        }
        catch (Exception error) when (error is OperationCanceledException or IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
        { token.ThrowIfCancellationRequested(); return null; }
        finally
        {
            deadline.Cancel();
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception) { }
            // Observe bounded reader/monitor tasks even when a timeout kills the child.
            foreach (var task in new Task?[] { output, errors, monitor })
                if (task is not null) try { await task.ConfigureAwait(false); } catch (Exception e) when (e is OperationCanceledException or IOException or InvalidOperationException) { }
            try { if (process.Id > 0) await process.WaitForExitAsync().ConfigureAwait(false); } catch (InvalidOperationException) { }
            try { Directory.Delete(directory, recursive: true); } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
    }

    private static async Task<string> ReadBounded(StreamReader reader, CancellationTokenSource deadline)
    {
        var result = new StringBuilder(); var buffer = new char[2048];
        while (true)
        {
            int count = await reader.ReadAsync(buffer.AsMemory(), deadline.Token).ConfigureAwait(false);
            if (count == 0) return result.ToString();
            if (result.Length + count > 65536) { deadline.Cancel(); throw new IOException("Solver output limit."); }
            result.Append(buffer, 0, count);
        }
    }
    private static async Task LimitMemory(Process process, CancellationTokenSource deadline)
    {
        while (!deadline.IsCancellationRequested && !process.HasExited)
        {
            process.Refresh();
            if (process.PrivateMemorySize64 > 256L * 1024 * 1024) { deadline.Cancel(); return; }
            await Task.Delay(25, deadline.Token).ConfigureAwait(false);
        }
    }
}
