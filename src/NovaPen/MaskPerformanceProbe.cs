using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace NovaPen;

// Region equivalence and timings without creating windows, magnifiers or shell entries.
internal static partial class MaskPerformanceProbe
{
    internal static void Run()
    {
        if (!OperatingSystem.IsWindows()) return;
        var report = new Dictionary<string, PerformanceMetric>();
        foreach (var scramble in new[] { false, true })
            foreach (var (width, height) in new[] { (1280, 720), (1920, 1080) })
            {
                var columns = scramble ? width / 16 * 2 : (width + 31) / 32;
                var rows = scramble ? height / 16 * 2 : (height + 31) / 32;
                for (var plane = 0; plane < 4; plane++)
                {
                    var old = LegacyRegion(plane, columns, rows, scramble);
                    var current = WindowsMaskRegion.Create(plane, columns, rows, scramble);
                    try
                    {
                        if (old == 0 || current == 0 || EqualRgn(old, current) == 0)
                            throw new InvalidOperationException("Batched mask region differs from the original tiles.");
                    }
                    finally { WindowsMaskRegion.DeleteObject(old); WindowsMaskRegion.DeleteObject(current); }
                }
                var prefix = $"region_{(scramble ? "scramble" : "glass")}_{width}x{height}";
                report[prefix + "_before"] = Measure(() => BuildAll(true), 1);
                report[prefix + "_after"] = Measure(() => BuildAll(false), 5);
                void BuildAll(bool legacy)
                {
                    for (var plane = 0; plane < 4; plane++)
                    {
                        var region = legacy ? LegacyRegion(plane, columns, rows, scramble) : WindowsMaskRegion.Create(plane, columns, rows, scramble);
                        if (region == 0) throw new InvalidOperationException("GDI region creation failed.");
                        WindowsMaskRegion.DeleteObject(region);
                    }
                }
            }
        for (var plane = 0; plane < 4; plane++)
        {
            var empty = WindowsMaskRegion.Create(plane, 0, 0, true);
            if (empty == 0) throw new InvalidOperationException("Empty mask region creation failed.");
            WindowsMaskRegion.DeleteObject(empty);
        }
        Console.WriteLine(JsonSerializer.Serialize(report, PerformanceJsonContext.Default.DictionaryStringPerformanceMetric));
    }
    private static PerformanceMetric Measure(Action action, int iterations)
    {
        action(); GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var allocated = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
        for (var i = 0; i < iterations; i++) action();
        return new(Stopwatch.GetElapsedTime(start).TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - allocated, iterations);
    }
    private static nint LegacyRegion(int plane, int columns, int rows, bool scramble)
    {
        var region = CreateRectRgn(0, 0, 0, 0);
        for (var row = 0; row < rows; row++) for (var column = 0; column < columns; column++)
        {
            if (WindowsMaskRegion.PlaneForCell(column, row, scramble) != plane) continue;
            var left = scramble ? column * 8 : column * 32 + 1;
            var top = scramble ? row * 8 : row * 32 + 1;
            var side = scramble ? 8 : 30;
            var cell = CreateRectRgn(left, top, left + side, top + side);
            try { if (cell == 0 || CombineRgn(region, region, cell, 2) == 0) throw new InvalidOperationException("Baseline region failed."); }
            finally { WindowsMaskRegion.DeleteObject(cell); }
        }
        return region;
    }
    [LibraryImport("gdi32.dll")] private static partial nint CreateRectRgn(int left, int top, int right, int bottom);
    [LibraryImport("gdi32.dll")] private static partial int CombineRgn(nint dest, nint first, nint second, int mode);
    [LibraryImport("gdi32.dll")] private static partial int EqualRgn(nint first, nint second);
}
