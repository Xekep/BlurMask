using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;
using BlurMask.Shared;

namespace BlurMask;

internal sealed record PerformanceMetric(double Milliseconds, long AllocatedBytes, int Iterations);
[JsonSerializable(typeof(Dictionary<string, PerformanceMetric>))]
internal partial class PerformanceJsonContext : JsonSerializerContext;

// Deterministic hot-path probes, independent of the desktop/GPU/network.
internal static class PerformanceProbe
{
    public static void Run()
    {
        var report = new Dictionary<string, PerformanceMetric>();
        PerformanceMetric Measure(Action action, int iterations)
        {
            action(); // JIT/type initialization is outside the measurement.
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            var bytes = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
            for (var i = 0; i < iterations; i++) action();
            return new(Stopwatch.GetElapsedTime(start).TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - bytes, iterations);
        }
        var points = Enumerable.Range(0, 20000).Select(i => new InkPointDto(i, i % 1000)).ToList();
        var mark = new InkMarkDto { Points = points };
        var frames = Enumerable.Range(0, 400).Select(i => new LiveFrame
            { Mark = mark with { Points = points.GetRange(i * 50, 50) }, Offset = i * 50 }).ToArray();
        report["merge_20000_points"] = Measure(() =>
        {
            InkMarkDto? preview = null;
            foreach (var frame in frames)
                if (!LiveProtocol.TryMerge(frame, preview, out preview)) throw new InvalidOperationException("Invalid benchmark stroke.");
            if (preview!.Points.Count != 20000) throw new InvalidOperationException("Lost benchmark points.");
        }, 10);
        var ink = new InkMark(InkTool.Pen, Colors.Red, 4, new Point(0, 0));
        foreach (var p in points.Skip(1)) ink.AddPoint(new Point(p.X, p.Y));
        report["bounds_20000_points_400_reads"] = Measure(() => { for (var i = 0; i < 400; i++) _ = ink.Bounds; }, 10);
        report["remote_preview_20000_points"] = Measure(() =>
        {
            var preview = new RemoteInkPreview();
            foreach (var frame in frames) if (!preview.Apply(frame)) throw new InvalidOperationException("Lost remote points.");
        }, 3);
        report["export_20000_points_400_updates"] = Measure(() =>
        { InkMarkDto? preview = null; for (var i = 0; i < 400; i++) preview = LivePreviewCapture.Capture(ink, preview); }, 3);
        var packet = JsonSerializer.SerializeToUtf8Bytes(new LiveFrame { Mark = mark with { Points = points.GetRange(0, 6) } }, BoardJsonContext.Default.LiveFrame);
        report["live_packet_6_points"] = new(0, packet.Length, 1);
        Console.WriteLine(JsonSerializer.Serialize(report, PerformanceJsonContext.Default.DictionaryStringPerformanceMetric));
    }
}
