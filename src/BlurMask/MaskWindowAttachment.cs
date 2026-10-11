using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Diagnostics;

namespace BlurMask;

internal sealed class MaskWindowAttachment : IDisposable
{
    private readonly BlurMaskWindow _mask;
    private readonly IWindowCatalog _catalog;
    private readonly bool _fillRegion;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(32) };
    private TrackedWindow? _target;
    private MaskAttachmentCoverage? _coverage;
    private WindowsWindowEvents? _events;
    private long _manualAt;
    private bool _applying, _hidden, _disposed;
    private PixelPoint? _expectedPosition;
    private Size _lastSize;
    private Size? _expectedSize;
    private PixelPoint _lastPosition;
    internal bool IsRegionFill => _fillRegion;
    internal MaskWindowAttachment(BlurMaskWindow mask, IWindowCatalog catalog, TrackedWindow? region = null)
    {
        _mask = mask; _catalog = catalog; _fillRegion = region is not null;
        _lastSize = mask.ClientSize; _lastPosition = mask.Position;
        mask.PositionChanged += OnMoved; mask.Resized += OnResized;
        _timer.Tick += (_, _) => Update();
        if (region is null) _manualAt = Stopwatch.GetTimestamp();
        else Bind(region);
        _timer.Start(); Update();
    }
    private void OnMoved(object? sender, PixelPointEventArgs e)
    {
        if (_lastPosition == _mask.Position) return;
        _lastPosition = _mask.Position;
        if (_expectedPosition == _mask.Position) { _expectedPosition = null; return; }
        if (!_applying && !_fillRegion) _manualAt = Stopwatch.GetTimestamp();
    }
    private void OnResized(object? sender, WindowResizedEventArgs e)
    {
        if (_lastSize == e.ClientSize) return;
        _lastSize = e.ClientSize;
        if (_expectedSize is { } expected && Math.Abs(expected.Width - e.ClientSize.Width) <= 1 / _mask.RenderScaling &&
            Math.Abs(expected.Height - e.ClientSize.Height) <= 1 / _mask.RenderScaling) { _expectedSize = null; return; }
        if (!_applying && !_fillRegion) _manualAt = Stopwatch.GetTimestamp();
    }
    private void Bind(TrackedWindow? target)
    {
        _target = target; _events?.Dispose(); _events = null;
        _timer.Interval = TimeSpan.FromMilliseconds(32);
        _mask.Topmost = target is null;
        AttachedMaskWindows.Remove(_mask);
        if (target is null) { _coverage = null; _mask.SetPrivacyCover(false); return; }
        _mask.ShowActivated = false;
        AttachedMaskWindows.Register(_mask, target.StackingId);
        _coverage = new(target.Bounds, new PixelRect(_mask.Position,
            new PixelSize((int)Math.Ceiling(_mask.ClientSize.Width * _mask.RenderScaling), (int)Math.Ceiling(_mask.ClientSize.Height * _mask.RenderScaling))), _fillRegion);
        _mask.SetPrivacyCover(_fillRegion);
        if (_catalog is WindowsWindowCatalog)
        {
            var root = _catalog.Read(target.StackingId);
            if (root is not null) _events = WindowsWindowEvents.TryCreate(root, () =>
            {
                if (Dispatcher.UIThread.CheckAccess()) Update();
                else Dispatcher.UIThread.Post(Update, DispatcherPriority.Render);
            });
            if (_events is not null) _timer.Interval = TimeSpan.FromMilliseconds(500);
        }
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
                Bind(_catalog.FindAt(_mask.PointToScreen(new Point(_mask.Bounds.Width / 2, _mask.Bounds.Height / 2))));
            }
            if (_target is null || _coverage is null) return;
            var root = _catalog.Read(_target.StackingId);
            if (root is null || root.ProcessId != _target.ProcessId) { _mask.Close(); return; }
            var target = _target.ParentId == 0 ? root : _catalog.ReadRegion(_target);
            if (_fillRegion && (target is null || target.ProcessId != _target.ProcessId || !target.Visible && root.Visible))
            {
                // Removed/hidden children can be replaced by unrelated content.
                // Keep the parent opaque until the user explicitly removes it.
                Bind(root); target = root;
            }
            if (target is null || target.ProcessId != _target.ProcessId) { _mask.Close(); return; }
            _applying = true;
            try
            {
                if (!root.Visible || !target.Visible || target.Bounds.Width <= 0 || target.Bounds.Height <= 0)
                { if (!_hidden) { _hidden = true; _mask.Hide(); } return; }
                var rectangle = _coverage.Follow(target.Bounds);
                _mask.SetPrivacyCover(_coverage.RequiresOpaqueCover);
                var size = new Size(rectangle.Width / _mask.RenderScaling, rectangle.Height / _mask.RenderScaling);
                if (Math.Abs(_mask.Width - size.Width) > .01 || Math.Abs(_mask.Height - size.Height) > .01)
                { _expectedSize = size; _mask.Width = size.Width; _mask.Height = size.Height; }
                if (_mask.Position != rectangle.Position) { _expectedPosition = rectangle.Position; _mask.Position = rectangle.Position; }
                if (_hidden) { _hidden = false; _mask.Show(); }
                _ = _catalog.PlaceAbove(_mask, root);
            }
            finally { _applying = false; }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or ObjectDisposedException)
        { _timer.Stop(); _events?.Dispose(); _events = null; }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        AttachedMaskWindows.Remove(_mask);
        _timer.Stop(); _events?.Dispose(); _mask.PositionChanged -= OnMoved; _mask.Resized -= OnResized; _catalog.Dispose();
    }
    internal void Detach()
    {
        Dispose(); _mask.Topmost = true; _mask.ShowActivated = true; _mask.SetPrivacyCover(false);
        if (_hidden) { _hidden = false; _mask.Show(); }
    }
}
