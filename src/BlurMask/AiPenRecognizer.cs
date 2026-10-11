using Avalonia;
using Avalonia.Threading;

namespace BlurMask;

internal sealed class AiPenRecognizer(InkDocument document, Func<bool> mathMode) : IDisposable
{
    private static readonly SemaphoreSlim Inference = new(1, 1);
    private readonly List<InkMark> _strokes = [];
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(800) };
    private bool _disposed;
    private bool _started;
    private int _generation;
    private InkMark[]? _processing;
    private CancellationTokenSource? _work;
    internal void Pause()
    {
        _timer.Stop(); _generation++; _work?.Cancel();
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
        _work?.Cancel(); _work?.Dispose(); _work = new();
        _ = ConvertAsync(strokes, ++_generation, mathMode(), _work.Token);
    }
    private async Task ConvertAsync(InkMark[] strokes, int generation, bool math, CancellationToken cancellationToken)
    {
        try
        {
            await Inference.WaitAsync(cancellationToken);
            IReadOnlyList<AiInkLayout.Result> results;
            try { if (_disposed || generation != _generation) return; results = await Task.Run(() => math ? AiMathLayout.Recognize(strokes, cancellationToken: cancellationToken) : AiInkLayout.Recognize(strokes, cancellationToken: cancellationToken), cancellationToken); }
            finally { Inference.Release(); }
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_disposed || generation != _generation || document.HasActiveEdit) return;
                foreach (var result in results) document.ReplaceRecognized(result.Strokes, result.Text);
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or InvalidOperationException)
        { System.Diagnostics.Trace.WriteLine($"AI pen: {ex.Message}"); }
        finally { if (generation == _generation) _processing = null; }
    }
    public void Dispose() { _disposed = true; _timer.Stop(); _work?.Cancel(); _work?.Dispose(); _strokes.Clear(); _processing = null; }
}
