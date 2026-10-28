using InkMath.Solver;

namespace NovaPen;

// Recognition and calculation are independent. Giac is launched only for an
// explicit calculation; it consumes no idle CPU/RAM and has no Python runtime.
internal static class AiMathSolver
{
    private static string ConfigurationPath => Path.Combine(Path.GetDirectoryName(SettingsStore.PathName)!, "giac-path.txt");
    internal static string BundledExecutable => Path.Combine(AppContext.BaseDirectory, "runtimes",
        System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier, "solver", OperatingSystem.IsWindows() ? "icas.exe" : "icas");
    internal static string? Executable
    {
        get
        {
            try
            {
                if (File.Exists(BundledExecutable)) return BundledExecutable;
                if (File.Exists(ConfigurationPath) && new FileInfo(ConfigurationPath).Length <= 8192)
                {
                    var configured = File.ReadAllText(ConfigurationPath).Trim();
                    if (Path.IsPathFullyQualified(configured) && File.Exists(configured)) return configured;
                }
                var candidates = OperatingSystem.IsWindows() ? new[] { @"C:\xcaswin\icas.exe", @"C:\xcas64\icas.exe" } :
                    new[] { "/usr/bin/icas", "/usr/bin/giac", "/usr/local/bin/icas", "/usr/local/bin/giac", "/Applications/usr/bin/icas", "/Applications/xcas.app/Contents/MacOS/icas", "/opt/homebrew/bin/icas", "/opt/homebrew/bin/giac" };
                return candidates.FirstOrDefault(File.Exists);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException) { return null; }
        }
    }

    // Installer verification runs before any GUI, and deliberately refuses an
    // external installation so a developer's configured CAS cannot mask a broken payload.
    internal static async Task VerifyBundledAsync()
    {
        if (!File.Exists(BundledExecutable) || Executable != BundledExecutable)
            throw new InvalidOperationException("The bundled Giac solver was not discovered automatically.");
        var savedOff = System.Text.Json.JsonSerializer.Serialize(new AppSettings { GiacEnabled = false }, SettingsJsonContext.Default.AppSettings);
        if (System.Text.Json.JsonSerializer.Deserialize(savedOff, SettingsJsonContext.Default.AppSettings)?.GiacEnabled != false ||
            System.Text.Json.JsonSerializer.Deserialize("{}", SettingsJsonContext.Default.AppSettings)?.GiacEnabled != true)
            throw new InvalidOperationException("Giac settings must preserve saved off and enable new settings.");
        var originalSettings = Program.Settings;
        try
        {
            Program.Settings = originalSettings with { GiacEnabled = false };
            if (Calculate("1+1", default) is not null || Calculate(@"\int_1^{10} x^2 dx", default) is not null)
                throw new InvalidOperationException("Disabled Giac must not use a fallback calculator.");
            Program.Settings = new();
            if (!Program.Settings.GiacEnabled || Calculate("1+1", default)?.LaTex != "2" ||
                Calculate(@"\int_1^{10} x^2 dx", default)?.LaTex != "333")
                throw new InvalidOperationException("New settings must route all calculations to bundled Giac.");
            if (!Equivalent(Calculate(@"\frac{d}{dx}\sin(x)", default)?.LaTex, @"\cos x"))
                throw new InvalidOperationException("The Giac toggle did not enable the automatically discovered engine.");
        }
        finally { Program.Settings = originalSettings; }
        var solver = new SymbolicMathSolver(BundledExecutable);
        foreach (var (formula, expected) in new (string, string)[]
        {
            (@"\int_{1}^{10} x^2 dx", "333"),
            (@"\int_0^\infty e^{-x} dx", "1"),
            (@"\frac{d}{dx}\sin(x)", @"\cos x"),
            (@"\lim_{x\to 0}\frac{\sin x}{x}", "1")
        })
        {
            if (!Equivalent((await solver.SolveAsync(formula))?.LaTex, expected))
                throw new InvalidOperationException("Installed Giac calculation failed: " + formula);
        }
        Console.WriteLine("PASS bundled Giac: default-on, disabled means no calculation, automatic discovery, arithmetic, definite/improper integrals, derivative and limit.");
        static bool Equivalent(string? actual, string expected)
        {
            if (actual == expected) return true;
            if (actual is null) return false;
            // Giac releases print this same derivative with either implicit or
            // parenthesized arguments; all numerical results remain exact.
            return expected == @"\cos x" && actual == @"\cos\left(x\right)";
        }
    }
    internal static MathSolution? Calculate(string latex, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!Program.Settings.GiacEnabled) return null;
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
