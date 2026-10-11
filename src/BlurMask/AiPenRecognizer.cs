using Avalonia;
using Avalonia.Threading;

namespace BlurMask;

internal sealed class AiPenRecognizer(InkDocument document, Func<string> recognitionMode,
    Func<InkMark[], string, CancellationToken, IReadOnlyList<AiInkLayout.Result>>? infer = null,
    Func<TimeSpan, CancellationToken, Task>? settle = null) : IDisposable
{
    internal static readonly TimeSpan ConversionQuietPeriod = TimeSpan.FromMilliseconds(500);
    private static readonly SemaphoreSlim Inference = new(1, 1);
    private readonly AiInkBatch _batch = new(document);
    private bool _disposed;
    private int _generation;
    private CancellationTokenSource? _work;
    internal Task Pending { get; private set; } = Task.CompletedTask;
    internal event Action? CalculationFailed;
    internal void Pause()
    {
        if (_disposed) return;
        _generation++; _work?.Cancel();
    }
    internal void Resume() => Flush(deferConversion: true);
    internal void Add(InkMark mark)
    {
        if (_disposed) return;
        _batch.Add(mark);
        // Add is called by CompleteStroke on pointer release. Start inference
        // now; a subsequent pen press cancels this generation, not its ink.
        Flush(deferConversion: true);
    }
    internal void Flush(bool deferConversion = false)
    {
        if (_disposed) return;
        var strokes = _batch.Snapshot();
        if (strokes.Length == 0) return;
        _work?.Cancel(); _work?.Dispose(); _work = new();
        Pending = ConvertAsync(strokes, ++_generation, recognitionMode(), deferConversion, _work.Token);
    }
    internal static TimeSpan RemainingConversionDelay(TimeSpan elapsed)
        => elapsed >= ConversionQuietPeriod ? TimeSpan.Zero : ConversionQuietPeriod - elapsed;

    private async Task ConvertAsync(InkMark[] strokes, int generation, string mode, bool deferConversion, CancellationToken cancellationToken)
    {
        long releasedAt = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            await Inference.WaitAsync(cancellationToken);
            IReadOnlyList<AiInkLayout.Result> results;
            try { if (_disposed || generation != _generation) return; results = await Task.Run(() => infer is not null ? infer(strokes, mode, cancellationToken) : mode switch
                {
                    "math" => AiMathLayout.Recognize(strokes, cancellationToken: cancellationToken),
                    "text" => AiInkLayout.Recognize(strokes, cancellationToken: cancellationToken),
                    _ => AiAutomaticLayout.Recognize(strokes, cancellationToken: cancellationToken)
                }, cancellationToken); }
            finally { Inference.Release(); }
            if (results.Count == 0) return;
            // Inference and the quiet period overlap. Only publishing waits;
            // explicit tool/mode transitions bypass the quiet period.
            if (deferConversion)
            {
                var remaining = RemainingConversionDelay(System.Diagnostics.Stopwatch.GetElapsedTime(releasedAt));
                await (settle is null ? Task.Delay(remaining, cancellationToken) : settle(remaining, cancellationToken));
            }
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_disposed || generation != _generation || document.HasActiveEdit) return;
                foreach (var result in results)
                {
                    var bounds = result.Strokes.Select(s => s.Bounds).Aggregate((a, b) => a.Union(b));
                    AiTypesetLayout.Fit(result.Text, bounds, result.SizingText, document.Marks);
                    _batch.Apply(result);
                }
                if (results.Any(result => result.MathCalculationFailed)) CalculationFailed?.Invoke();
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or InvalidOperationException)
        { System.Diagnostics.Trace.WriteLine($"AI pen: {ex.Message}"); }
    }
    public void Dispose() { _disposed = true; _work?.Cancel(); _work?.Dispose(); _batch.Clear(); }
}
