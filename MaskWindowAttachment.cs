using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Diagnostics;

namespace BlurMask;

internal sealed class MaskWindowAttachment : IDisposable
{
    private readonly BlurMaskWindow _mask;
    private readonly IWindowCatalog _catalog;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private TrackedWindow? _target;
    private PixelPoint _offset;
    private PixelSize _originalMaskSize, _originalTargetSize;
    private WindowsWindowEvents? _events;
    private long _manualAt;
    private bool _applying, _hidden;
    private PixelPoint? _expectedPosition;
    private Size _lastSize;
    private Size? _expectedSize;
    private PixelPoint _lastPosition;
    private bool _disposed;
    internal MaskWindowAttachment(BlurMaskWindow mask, IWindowCatalog catalog)
    {
        _mask = mask; _catalog = catalog;
        _lastSize = mask.ClientSize; _lastPosition = mask.Position;
        mask.PositionChanged += OnMoved; mask.Resized += OnResized;
        _timer.Tick += (_, _) => Update();
        _manualAt = Stopwatch.GetTimestamp(); _timer.Start();
    }
    private void OnMoved(object? sender, PixelPointEventArgs e)
    {
        if (_lastPosition == _mask.Position) return;
        _lastPosition = _mask.Position;
        if (_expectedPosition == _mask.Position) { _expectedPosition = null; return; }
        if (!_applying) _manualAt = Stopwatch.GetTimestamp();
    }
    private void OnResized(object? sender, WindowResizedEventArgs e)
    {
        if (_lastSize == e.ClientSize) return;
        _lastSize = e.ClientSize;
        if (_expectedSize is { } expected && Math.Abs(expected.Width - e.ClientSize.Width) <= 1 / _mask.RenderScaling &&
            Math.Abs(expected.Height - e.ClientSize.Height) <= 1 / _mask.RenderScaling) { _expectedSize = null; return; }
        if (!_applying) _manualAt = Stopwatch.GetTimestamp();
    }
    internal void Update()
    {
        if (_disposed) return;
        try
        {
            if (_manualAt != 0)
            {
                if (Stopwatch.GetElapsedTime(_manualAt).TotalMilliseconds < 300) return;
                _manualAt = 0;
                var center = _mask.PointToScreen(new Point(_mask.Bounds.Width / 2, _mask.Bounds.Height / 2));
                _target = _catalog.FindAt(center);
                _events?.Dispose(); _events = null;
                _timer.Interval = TimeSpan.FromMilliseconds(16);
                if (_target is not null)
                {
                    _offset = new PixelPoint(_mask.Position.X - _target.Bounds.X, _mask.Position.Y - _target.Bounds.Y);
                    _originalTargetSize = new PixelSize(Math.Max(1, _target.Bounds.Width), Math.Max(1, _target.Bounds.Height));
                    _originalMaskSize = new PixelSize((int)Math.Round(_mask.ClientSize.Width * _mask.RenderScaling), (int)Math.Round(_mask.ClientSize.Height * _mask.RenderScaling));
                    if (_catalog is WindowsWindowCatalog)
                        _events = WindowsWindowEvents.TryCreate(_target, () =>
                        {
                            if (Dispatcher.UIThread.CheckAccess()) Update();
                            else Dispatcher.UIThread.Post(Update, DispatcherPriority.Render);
                        });
                    if (_events is not null) _timer.Interval = TimeSpan.FromMilliseconds(500);
                }
            }
            if (_target is null) return;
            var target = _catalog.Read(_target.Id);
            if (target is null || target.ProcessId != _target.ProcessId) { _mask.Close(); return; }
            _applying = true;
            try
            {
                if (!target.Visible) { if (!_hidden) { _hidden = true; _mask.Hide(); } return; }
                var scaleX = (double)target.Bounds.Width / _originalTargetSize.Width;
                var scaleY = (double)target.Bounds.Height / _originalTargetSize.Height;
                var position = new PixelPoint(target.Bounds.X + (int)Math.Round(_offset.X * scaleX), target.Bounds.Y + (int)Math.Round(_offset.Y * scaleY));
                var size = new Size(Math.Max(_mask.MinWidth, Math.Round(_originalMaskSize.Width * scaleX) / _mask.RenderScaling),
                    Math.Max(_mask.MinHeight, Math.Round(_originalMaskSize.Height * scaleY) / _mask.RenderScaling));
                if (Math.Abs(_mask.Width - size.Width) > .01 || Math.Abs(_mask.Height - size.Height) > .01)
                { _expectedSize = size; _mask.Width = size.Width; _mask.Height = size.Height; }
                if (_mask.Position != position) { _expectedPosition = position; _mask.Position = position; }
                if (_hidden) { _hidden = false; _mask.Show(); }
            }
            finally { _applying = false; }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or ObjectDisposedException)
        { _timer.Stop(); _events?.Dispose(); _events = null; _target = null; }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop(); _events?.Dispose(); _mask.PositionChanged -= OnMoved; _mask.Resized -= OnResized; _catalog.Dispose();
    }
    internal void Detach() { if (_hidden) { _hidden = false; _mask.Show(); } Dispose(); }
}
