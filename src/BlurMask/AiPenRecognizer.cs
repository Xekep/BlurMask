using Avalonia;
using Avalonia.Threading;

namespace BlurMask;

internal sealed class AiPenRecognizer(InkDocument document, Func<AppSettings> settings) : IDisposable
{
    private static readonly SemaphoreSlim Inference = new(1, 1);
    private readonly List<InkMark> _strokes = [];
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(800) };
    private bool _disposed;
    private bool _started;
    private int _generation;
    private InkMark[]? _processing;
    internal void Pause()
    {
        _timer.Stop(); _generation++;
        if (_processing is not { } previous) return;
        _processing = null;
        _strokes.InsertRange(0, previous.Where(m => document.Marks.Contains(m) || document.FadingMarks.Contains(m)));
    }
    internal void Resume() { if (!_disposed && _strokes.Count > 0) { _timer.Stop(); _timer.Start(); } }
    internal void Add(InkMark mark)
    {
        if (_disposed) return;
        if (!_started) { _timer.Tick += (_, _) => Flush(); _started = true; }
        if (_strokes.Count > 0)
        {
            if (_strokes.Count >= 256 || _strokes.Sum(m => m.Points.Count) + mark.Points.Count > 20000) Flush();
        }
        _strokes.Add(mark); _timer.Stop(); _timer.Start();
    }
    internal void Flush()
    {
        _timer.Stop();
        _strokes.RemoveAll(m => !document.Marks.Contains(m) && !document.FadingMarks.Contains(m));
        if (_disposed || _strokes.Count == 0) return;
        var strokes = _strokes.ToArray(); _strokes.Clear(); _processing = strokes;
        _ = ConvertAsync(strokes, ++_generation);
    }
    private async Task ConvertAsync(InkMark[] strokes, int generation)
    {
        try
        {
            await Inference.WaitAsync();
            IReadOnlyList<AiInkLayout.Result> results;
            try { if (_disposed || generation != _generation) return; var options = settings(); results = await Task.Run(() => AiInkLayout.Recognize(strokes, options.AiLanguage, options.AiFont)); }
            finally { Inference.Release(); }
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_disposed || generation != _generation || document.HasActiveEdit) return;
                foreach (var result in results) document.ReplaceRecognized(result.Strokes, result.Text);
            });
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or InvalidOperationException)
        { System.Diagnostics.Trace.WriteLine($"AI pen: {ex.Message}"); }
        finally { if (generation == _generation) _processing = null; }
    }
    internal static float[] Rasterize(IReadOnlyList<InkMark> strokes, Rect bounds)
    {
        var pixels = new float[1024]; var scale = 24 / Math.Max(bounds.Width, bounds.Height);
        var origin = new Point((32 - bounds.Width * scale) / 2, (32 - bounds.Height * scale) / 2);
        foreach (var mark in strokes)
        {
            var radius = Math.Clamp(mark.Width * scale / 2, .5, 5);
            for (var i = 0; i < mark.Points.Count; i++)
            {
                var a = origin + (mark.Points[Math.Max(0, i - 1)] - bounds.TopLeft) * scale;
                var b = origin + (mark.Points[i] - bounds.TopLeft) * scale;
                var minX = Math.Max(0, (int)Math.Floor(Math.Min(a.X, b.X) - radius - 1));
                var maxX = Math.Min(31, (int)Math.Ceiling(Math.Max(a.X, b.X) + radius + 1));
                var minY = Math.Max(0, (int)Math.Floor(Math.Min(a.Y, b.Y) - radius - 1));
                var maxY = Math.Min(31, (int)Math.Ceiling(Math.Max(a.Y, b.Y) + radius + 1));
                for (var y = minY; y <= maxY; y++) for (var x = minX; x <= maxX; x++)
                    pixels[y * 32 + x] = Math.Max(pixels[y * 32 + x], (float)Math.Clamp(radius + .5 - InkMark.Distance(new Point(x + .5, y + .5), a, b), 0, 1));
            }
        }
        return pixels;
    }
    internal static float[] RasterizeLine(IReadOnlyList<InkMark> strokes, Rect bounds, int height = 48, int width = 512)
    {
        var pixels = new float[height * width]; Array.Fill(pixels, 1);
        var sx = (width - 4) / bounds.Width; var sy = (height - 4) / bounds.Height;
        foreach (var stroke in strokes)
        {
            var radius = stroke.Width / 2; var antialias = .5 / Math.Max(sx, sy);
            for (var i = 0; i < stroke.Points.Count; i++)
            {
                var a = stroke.Points[Math.Max(0, i - 1)]; var b = stroke.Points[i];
                var left = Math.Max(0, (int)((Math.Min(a.X, b.X) - radius - bounds.X) * sx));
                var right = Math.Min(width - 1, (int)Math.Ceiling((Math.Max(a.X, b.X) + radius - bounds.X) * sx + 4));
                var top = Math.Max(0, (int)((Math.Min(a.Y, b.Y) - radius - bounds.Y) * sy));
                var bottom = Math.Min(height - 1, (int)Math.Ceiling((Math.Max(a.Y, b.Y) + radius - bounds.Y) * sy + 4));
                for (var y = top; y <= bottom; y++) for (var x = left; x <= right; x++)
                {
                    var point = new Point((x + .5 - 2) / sx + bounds.X, (y + .5 - 2) / sy + bounds.Y);
                    var coverage = Math.Clamp((radius + antialias - InkMark.Distance(point, a, b)) / (2 * antialias), 0, 1);
                    pixels[y * width + x] = Math.Min(pixels[y * width + x], (float)(1 - 2 * coverage));
                }
            }
        }
        return pixels;
    }
    public void Dispose() { _disposed = true; _timer.Stop(); _strokes.Clear(); _processing = null; }
}
