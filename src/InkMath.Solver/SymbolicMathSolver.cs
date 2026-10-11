using System.Diagnostics;
using System.Text;

namespace InkMath.Solver;

public sealed record MathSolution(string LaTex, bool IsEquation);

/// <summary>Local Giac CAS, isolated from the caller and compatible with Native AOT.</summary>
public sealed class SymbolicMathSolver
{
    private readonly string _executable;
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
        var operation = equation ? "solve(" + (equations is null ? expression.Cas : "[" + string.Join(',', equations.Select(e => e.Cas)) + "]") +
            "," + (variables.Length == 1 ? variables[0] : "[" + string.Join(',', variables) + "]") + ")" :
            expression is Call ? expression.Cas : "simplify(" + expression.Cas + ")";
        var command = "latex(" + operation + ")";
        string? result = await ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
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

    private async Task<string?> ExecuteAsync(string command, CancellationToken token)
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
        foreach (var name in new[] { "OMP_NUM_THREADS", "OPENBLAS_NUM_THREADS", "MKL_NUM_THREADS", "VECLIB_MAXIMUM_THREADS", "GOTO_NUM_THREADS" }) start.Environment[name] = "1";
        // No external display is needed by a command-line calculation.
        start.Environment.Remove("DISPLAY"); start.Environment.Remove("WAYLAND_DISPLAY");
        using var process = new Process { StartInfo = start };
        Task<string>? output = null, errors = null;
        Task? monitor = null;
        try
        {
            if (!process.Start()) return null;
            output = ReadBounded(process.StandardOutput, deadline);
            errors = ReadBounded(process.StandardError, deadline);
            monitor = LimitMemory(process, deadline);
            await process.StandardInput.WriteLineAsync(command.AsMemory(), deadline.Token).ConfigureAwait(false);
            process.StandardInput.Close();
            await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);
            var stdout = await output.ConfigureAwait(false); await errors.ConfigureAwait(false);
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
