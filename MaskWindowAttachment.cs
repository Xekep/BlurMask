using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Diagnostics;

namespace BlurMask;

internal sealed class MaskWindowAttachment : IDisposable
{
    private readonly BlurMaskWindow _mask;
    private readonly IWindowCatalog _catalog;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private TrackedWindow? _target;
    private PixelPoint _offset;
    private long _manualAt;
    private bool _applying, _hidden;
    internal MaskWindowAttachment(BlurMaskWindow mask, IWindowCatalog catalog)
    {
        _mask = mask; _catalog = catalog;
        mask.PositionChanged += OnMoved; mask.Resized += OnResized;
        _timer.Tick += (_, _) => Update();
        _manualAt = Stopwatch.GetTimestamp(); _timer.Start();
    }
    private void OnMoved(object? sender, PixelPointEventArgs e) { if (!_applying) _manualAt = Stopwatch.GetTimestamp(); }
    private void OnResized(object? sender, WindowResizedEventArgs e) { if (!_applying) _manualAt = Stopwatch.GetTimestamp(); }
    internal void Update()
    {
        try
        {
            if (_manualAt != 0)
            {
                if (Stopwatch.GetElapsedTime(_manualAt).TotalMilliseconds < 300) return;
                _manualAt = 0;
                var center = _mask.PointToScreen(new Point(_mask.Bounds.Width / 2, _mask.Bounds.Height / 2));
                _target = _catalog.FindAt(center);
                if (_target is not null) _offset = new PixelPoint(_mask.Position.X - _target.Bounds.X, _mask.Position.Y - _target.Bounds.Y);
            }
            if (_target is null) return;
            var target = _catalog.Read(_target.Id);
            if (target is null || target.ProcessId != _target.ProcessId) { _mask.Close(); return; }
            _applying = true;
            try
            {
                if (!target.Visible) { if (!_hidden) { _hidden = true; _mask.Hide(); } return; }
                _mask.Position = new PixelPoint(target.Bounds.X + _offset.X, target.Bounds.Y + _offset.Y);
                if (_hidden) { _hidden = false; _mask.Show(); }
            }
            finally { _applying = false; }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or ObjectDisposedException)
        { _timer.Stop(); _target = null; }
    }
    public void Dispose()
    {
        _timer.Stop(); _mask.PositionChanged -= OnMoved; _mask.Resized -= OnResized; _catalog.Dispose();
    }
    internal void Detach() { if (_hidden) { _hidden = false; _mask.Show(); } Dispose(); }
}
