using InkMath.Solver;

namespace BlurMask;

// Recognition and calculation are independent. Giac is launched only for an
// explicit calculation; it consumes no idle CPU/RAM and has no Python runtime.
internal static class AiMathSolver
{
    private static string ConfigurationPath => Path.Combine(Path.GetDirectoryName(SettingsStore.PathName)!, "giac-path.txt");
    internal static string? Executable
    {
        get
        {
            try
            {
                if (File.Exists(ConfigurationPath) && new FileInfo(ConfigurationPath).Length <= 8192)
                {
                    var configured = File.ReadAllText(ConfigurationPath).Trim();
                    if (Path.IsPathFullyQualified(configured) && File.Exists(configured)) return configured;
                }
                var rid = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
                var bundled = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "solver", OperatingSystem.IsWindows() ? "icas.exe" : "icas");
                if (File.Exists(bundled)) return bundled;
                var candidates = OperatingSystem.IsWindows() ? new[] { @"C:\xcaswin\icas.exe", @"C:\xcas64\icas.exe" } :
                    new[] { "/usr/bin/icas", "/usr/bin/giac", "/usr/local/bin/icas", "/usr/local/bin/giac", "/Applications/usr/bin/icas", "/Applications/xcas.app/Contents/MacOS/icas", "/opt/homebrew/bin/icas", "/opt/homebrew/bin/giac" };
                return candidates.FirstOrDefault(File.Exists);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException) { return null; }
        }
    }
    internal static MathSolution? Calculate(string latex, CancellationToken token)
    {
        if (PolynomialIntegralSolver.Solve(latex, token) is { } exact) return exact;
        try { return Executable is { } path ? new SymbolicMathSolver(path).SolveAsync(latex, token).GetAwaiter().GetResult() : null; }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException) { return null; }
    }

    internal static async Task ConfigureAsync(string path, CancellationToken token = default)
    {
        path = Path.GetFullPath(path);
        var result = await new SymbolicMathSolver(path).SolveAsync("1+1", token);
        if (result?.LaTex != "2") throw new InvalidOperationException(UiText.T("Не удалось запустить решатель Giac"));
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationPath)!);
        var temporary = ConfigurationPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { await File.WriteAllTextAsync(temporary, path, token); File.Move(temporary, ConfigurationPath, overwrite: true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
