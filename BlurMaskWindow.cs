using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace BlurMask;

public enum PrivacyMode
{
    Blur,
    BigPixels,
    Scramble,
    Loupes,
    Blackout
}

public sealed class BlurMaskWindow : Window
{
    private const double Grip = 12;

    private static readonly Cursor MoveCursor = new(StandardCursorType.SizeAll);
    private static readonly Cursor HorizontalCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor VerticalCursor = new(StandardCursorType.SizeNorthSouth);
    private static readonly Cursor NorthWestSouthEastCursor = new(StandardCursorType.TopLeftCorner);
    private static readonly Cursor NorthEastSouthWestCursor = new(StandardCursorType.TopRightCorner);

    private readonly Border _outline;
    private readonly Grid _root;
    private readonly Border _inputSurface;
    private readonly UniformGrid _bigPixels;
    private readonly UniformGrid _scramble;
    private readonly UniformGrid _loupes;

    public PrivacyMode Mode { get; private set; } = PrivacyMode.Blur;

    public event Action<PrivacyMode>? ModeChanged;

    public BlurMaskWindow()
    {
        Title = "BlurMask";
        Width = 360;
        Height = 360;
        MinWidth = 120;
        MinHeight = 80;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = true;
        CanMinimize = false;
        CanMaximize = false;
        WindowDecorations = WindowDecorations.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Transparent;
        TransparencyBackgroundFallback = new SolidColorBrush(Color.FromArgb(240, 36, 36, 36));

        _root = new Grid();

        _bigPixels = CreateBigPixels();
        _bigPixels.IsHitTestVisible = false;
        _root.Children.Add(_bigPixels);

        _scramble = CreateScramble();
        _scramble.IsHitTestVisible = false;
        _root.Children.Add(_scramble);

        _loupes = CreateLoupes();
        _loupes.IsHitTestVisible = false;
        _root.Children.Add(_loupes);

        // One full-window interaction surface is deliberately used instead of separate
        // move/resize controls. It makes all mouse buttons deterministic regardless of
        // which visual privacy layer is currently visible.
        _inputSurface = new Border
        {
            Background = Brushes.Transparent,
            Cursor = MoveCursor
        };
        _inputSurface.PointerPressed += InputSurfaceOnPointerPressed;
        _inputSurface.PointerMoved += InputSurfaceOnPointerMoved;
        _inputSurface.PointerEntered += (_, _) => _outline.Opacity = 1;
        _inputSurface.PointerExited += (_, _) => _outline.Opacity = 0;
        _root.Children.Add(_inputSurface);

        _outline = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            IsHitTestVisible = false,
            Opacity = 0.0
        };
        _root.Children.Add(_outline);

        Content = _root;

        ApplyMode();
        Opened += (_, _) => PlatformBlur.TryApplyNativeEnhancements(this);
    }

    private void InputSurfaceOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(_inputSurface);
        var properties = point.Properties;

        if (properties.IsMiddleButtonPressed)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (properties.IsRightButtonPressed)
        {
            e.Handled = true;
            CycleMode();
            return;
        }

        if (!properties.IsLeftButtonPressed)
            return;

        e.Handled = true;

        var edge = GetResizeEdge(point.Position);
        if (edge is { } resizeEdge)
            BeginResizeDrag(resizeEdge, e);
        else
            BeginMoveDrag(e);
    }

    private void InputSurfaceOnPointerMoved(object? sender, PointerEventArgs e)
    {
        var edge = GetResizeEdge(e.GetPosition(_inputSurface));
        _inputSurface.Cursor = CursorForEdge(edge);
    }

    private WindowEdge? GetResizeEdge(Point p)
    {
        var width = _inputSurface.Bounds.Width;
        var height = _inputSurface.Bounds.Height;

        var left = p.X <= Grip;
        var right = p.X >= width - Grip;
        var top = p.Y <= Grip;
        var bottom = p.Y >= height - Grip;

        if (top && left) return WindowEdge.NorthWest;
        if (top && right) return WindowEdge.NorthEast;
        if (bottom && left) return WindowEdge.SouthWest;
        if (bottom && right) return WindowEdge.SouthEast;
        if (top) return WindowEdge.North;
        if (bottom) return WindowEdge.South;
        if (left) return WindowEdge.West;
        if (right) return WindowEdge.East;
        return null;
    }

    private static Cursor CursorForEdge(WindowEdge? edge) => edge switch
    {
        WindowEdge.North or WindowEdge.South => VerticalCursor,
        WindowEdge.West or WindowEdge.East => HorizontalCursor,
        WindowEdge.NorthWest or WindowEdge.SouthEast => NorthWestSouthEastCursor,
        WindowEdge.NorthEast or WindowEdge.SouthWest => NorthEastSouthWestCursor,
        _ => MoveCursor
    };

    private static UniformGrid CreateBigPixels()
    {
        const int columns = 7;
        const int rows = 5;
        var grid = NewPrivacyGrid(columns, rows);
        var brushes = CreatePrivacyBrushes();

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                var index = (x * 5 + y * 3 + ((x + y) & 1)) % brushes.Length;
                grid.Children.Add(new Border { Background = brushes[index] });
            }
        }

        return grid;
    }

    private static UniformGrid CreateScramble()
    {
        const int columns = 9;
        const int rows = 7;
        var grid = NewPrivacyGrid(columns, rows);
        var brushes = CreatePrivacyBrushes();

        for (var source = 0; source < columns * rows; source++)
        {
            var shuffled = (source * 37 + 17) % (columns * rows);
            var x = shuffled % columns;
            var y = shuffled / columns;
            var index = (x * 11 + y * 7 + source) % brushes.Length;

            grid.Children.Add(new Border
            {
                Background = brushes[index],
                Margin = new Thickness((source % 3) == 0 ? 1 : 0)
            });
        }

        return grid;
    }

    private static UniformGrid CreateLoupes()
    {
        const int columns = 5;
        const int rows = 4;
        var grid = NewPrivacyGrid(columns, rows);
        var brushes = CreatePrivacyBrushes();

        for (var i = 0; i < columns * rows; i++)
        {
            grid.Children.Add(new Border
            {
                Margin = new Thickness(4 + (i % 2)),
                CornerRadius = new CornerRadius(999),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(140, 220, 220, 225)),
                Background = brushes[(i * 3 + i / columns) % brushes.Length]
            });
        }

        return grid;
    }

    private static UniformGrid NewPrivacyGrid(int columns, int rows) => new()
    {
        Columns = columns,
        Rows = rows,
        IsVisible = false
    };

    private static IBrush[] CreatePrivacyBrushes() =>
    [
        new SolidColorBrush(Color.FromRgb(30, 30, 34)),
        new SolidColorBrush(Color.FromRgb(58, 58, 64)),
        new SolidColorBrush(Color.FromRgb(90, 90, 98)),
        new SolidColorBrush(Color.FromRgb(44, 44, 50)),
        new SolidColorBrush(Color.FromRgb(112, 112, 120)),
        new SolidColorBrush(Color.FromRgb(70, 70, 78))
    ];

    internal void CycleModeForSmokeTest() => CycleMode();

    private void CycleMode()
    {
        Mode = Mode switch
        {
            PrivacyMode.Blur => PrivacyMode.BigPixels,
            PrivacyMode.BigPixels => PrivacyMode.Scramble,
            PrivacyMode.Scramble => PrivacyMode.Loupes,
            PrivacyMode.Loupes => PrivacyMode.Blackout,
            _ => PrivacyMode.Blur
        };

        ApplyMode();
        ModeChanged?.Invoke(Mode);
    }

    private void ApplyMode()
    {
        Background = Brushes.Transparent;
        _root.Background = Brushes.Transparent;
        _bigPixels.IsVisible = false;
        _scramble.IsVisible = false;
        _loupes.IsVisible = false;

        switch (Mode)
        {
            case PrivacyMode.Blur:
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.Blur,
                    WindowTransparencyLevel.AcrylicBlur,
                    WindowTransparencyLevel.Transparent
                ];
                _root.Background = new SolidColorBrush(Color.FromArgb(18, 245, 245, 245));
                break;

            case PrivacyMode.BigPixels:
                // Do not make the window click-through. The opaque pattern is rendered inside
                // the same top-level so the input layer remains active on every privacy mode.
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.Transparent,
                    WindowTransparencyLevel.None
                ];
                _root.Background = new SolidColorBrush(Color.FromRgb(30, 30, 34));
                _bigPixels.IsVisible = true;
                break;

            case PrivacyMode.Scramble:
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.Transparent,
                    WindowTransparencyLevel.None
                ];
                _root.Background = new SolidColorBrush(Color.FromRgb(24, 24, 28));
                _scramble.IsVisible = true;
                break;

            case PrivacyMode.Loupes:
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.Transparent,
                    WindowTransparencyLevel.None
                ];
                _root.Background = new SolidColorBrush(Color.FromRgb(22, 22, 26));
                _loupes.IsVisible = true;
                break;

            case PrivacyMode.Blackout:
                TransparencyLevelHint = [WindowTransparencyLevel.None];
                Background = Brushes.Black;
                _root.Background = Brushes.Black;
                break;
        }
    }
}
