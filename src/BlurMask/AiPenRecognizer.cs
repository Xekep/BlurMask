using Avalonia;
using Avalonia.Threading;

namespace BlurMask;

internal sealed class AiPenRecognizer(InkDocument document, Func<string> recognitionMode,
    Func<InkMark[], string, CancellationToken, IReadOnlyList<AiInkLayout.Result>>? infer = null) : IDisposable
{
    private static readonly SemaphoreSlim Inference = new(1, 1);
    private readonly AiInkBatch _batch = new(document);
    private bool _disposed;
    private int _generation;
    private CancellationTokenSource? _work;
    internal Task Pending { get; private set; } = Task.CompletedTask;
    internal void Pause()
    {
        if (_disposed) return;
        _generation++; _work?.Cancel();
    }
    internal void Resume() => Flush();
    internal void Add(InkMark mark)
    {
        if (_disposed) return;
        _batch.Add(mark);
        // Add is called by CompleteStroke on pointer release. Start inference
        // now; a subsequent pen press cancels this generation, not its ink.
        Flush();
    }
    internal void Flush()
    {
        if (_disposed) return;
        var strokes = _batch.Snapshot();
        if (strokes.Length == 0) return;
        _work?.Cancel(); _work?.Dispose(); _work = new();
        Pending = ConvertAsync(strokes, ++_generation, recognitionMode(), _work.Token);
    }
    private async Task ConvertAsync(InkMark[] strokes, int generation, string mode, CancellationToken cancellationToken)
    {
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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_disposed || generation != _generation || document.HasActiveEdit) return;
                foreach (var result in results)
                {
                    var bounds = result.Strokes.Select(s => s.Bounds).Aggregate((a, b) => a.Union(b));
                    AiTypesetLayout.Fit(result.Text, bounds, result.SizingText, document.Marks);
                    _batch.Apply(result);
                }
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or InvalidOperationException)
        { System.Diagnostics.Trace.WriteLine($"AI pen: {ex.Message}"); }
    }
    public void Dispose() { _disposed = true; _work?.Cancel(); _work?.Dispose(); _batch.Clear(); }
}
