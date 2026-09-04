using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace BlurMask;

public enum PrivacyMode
{
    Blur,
    Acrylic,
    Frosted,
    PixelMosaic,
    Blackout
}

public sealed class BlurMaskWindow : Window
{
    private const double Grip = 10;
    private readonly Border _outline;
    private readonly Grid _root;
    private readonly UniformGrid _mosaic;

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

        _mosaic = CreateMosaic();
        _mosaic.IsHitTestVisible = false;
        _root.Children.Add(_mosaic);

        _outline = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            IsHitTestVisible = false,
            Opacity = 0.0
        };
        _root.Children.Add(_outline);

        var moveSurface = new Border
        {
            Margin = new Thickness(Grip),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.SizeAll)
        };
        moveSurface.PointerPressed += MoveSurfaceOnPointerPressed;
        _root.Children.Add(moveSurface);

        AddGrip(_root, WindowEdge.North, HorizontalAlignment.Stretch, VerticalAlignment.Top,
            new Thickness(Grip, 0, Grip, 0), double.NaN, Grip, StandardCursorType.TopSide);
        AddGrip(_root, WindowEdge.South, HorizontalAlignment.Stretch, VerticalAlignment.Bottom,
            new Thickness(Grip, 0, Grip, 0), double.NaN, Grip, StandardCursorType.BottomSide);
        AddGrip(_root, WindowEdge.West, HorizontalAlignment.Left, VerticalAlignment.Stretch,
            new Thickness(0, Grip, 0, Grip), Grip, double.NaN, StandardCursorType.LeftSide);
        AddGrip(_root, WindowEdge.East, HorizontalAlignment.Right, VerticalAlignment.Stretch,
            new Thickness(0, Grip, 0, Grip), Grip, double.NaN, StandardCursorType.RightSide);

        AddGrip(_root, WindowEdge.NorthWest, HorizontalAlignment.Left, VerticalAlignment.Top,
            default, Grip * 1.6, Grip * 1.6, StandardCursorType.TopLeftCorner);
        AddGrip(_root, WindowEdge.NorthEast, HorizontalAlignment.Right, VerticalAlignment.Top,
            default, Grip * 1.6, Grip * 1.6, StandardCursorType.TopRightCorner);
        AddGrip(_root, WindowEdge.SouthWest, HorizontalAlignment.Left, VerticalAlignment.Bottom,
            default, Grip * 1.6, Grip * 1.6, StandardCursorType.BottomLeftCorner);
        AddGrip(_root, WindowEdge.SouthEast, HorizontalAlignment.Right, VerticalAlignment.Bottom,
            default, Grip * 1.6, Grip * 1.6, StandardCursorType.BottomRightCorner);

        _root.PointerEntered += (_, _) => _outline.Opacity = 1;
        _root.PointerExited += (_, _) => _outline.Opacity = 0;

        Content = _root;

        // Tunnel handler catches middle/right click even when the pointer is over a resize grip.
        AddHandler(InputElement.PointerPressedEvent, OnAnyPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);

        ApplyMode();
        Opened += (_, _) => PlatformBlur.TryApplyNativeEnhancements(this);
    }

    private static UniformGrid CreateMosaic()
    {
        const int columns = 16;
        const int rows = 12;
        var grid = new UniformGrid
        {
            Columns = columns,
            Rows = rows,
            IsVisible = false
        };

        var brushes = new IBrush[]
        {
            new SolidColorBrush(Color.FromRgb(38, 38, 42)),
            new SolidColorBrush(Color.FromRgb(66, 66, 72)),
            new SolidColorBrush(Color.FromRgb(96, 96, 102)),
            new SolidColorBrush(Color.FromRgb(52, 52, 58))
        };

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                // Deterministic coarse privacy mosaic. It is intentionally opaque so text/details
                // beneath the mask cannot leak into the recording.
                var index = (x * 3 + y * 5 + ((x ^ y) & 1)) % brushes.Length;
                grid.Children.Add(new Border { Background = brushes[index] });
            }
        }

        return grid;
    }

    private void MoveSurfaceOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        BeginMoveDrag(e);
        e.Handled = true;
    }

    private void AddGrip(
        Grid root,
        WindowEdge edge,
        HorizontalAlignment horizontal,
        VerticalAlignment vertical,
        Thickness margin,
        double width,
        double height,
        StandardCursorType cursor)
    {
        var grip = new Border
        {
            HorizontalAlignment = horizontal,
            VerticalAlignment = vertical,
            Margin = margin,
            Background = Brushes.Transparent,
            Cursor = new Cursor(cursor)
        };

        if (!double.IsNaN(width))
            grip.Width = width;
        if (!double.IsNaN(height))
            grip.Height = height;

        grip.PointerPressed += (_, e) =>
        {
            var properties = e.GetCurrentPoint(this).Properties;
            if (properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
                return;

            BeginResizeDrag(edge, e);
            e.Handled = true;
        };

        root.Children.Add(grip);
    }

    private void OnAnyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var kind = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

        if (kind == PointerUpdateKind.MiddleButtonPressed)
        {
            e.Handled = true;
            Close();
            return;
        }

        if (kind == PointerUpdateKind.RightButtonPressed)
        {
            e.Handled = true;
            CycleMode();
        }
    }

    private void CycleMode()
    {
        Mode = Mode switch
        {
            PrivacyMode.Blur => PrivacyMode.Acrylic,
            PrivacyMode.Acrylic => PrivacyMode.Frosted,
            PrivacyMode.Frosted => PrivacyMode.PixelMosaic,
            PrivacyMode.PixelMosaic => PrivacyMode.Blackout,
            _ => PrivacyMode.Blur
        };

        ApplyMode();
        ModeChanged?.Invoke(Mode);
    }

    private void ApplyMode()
    {
        Background = Brushes.Transparent;
        _mosaic.IsVisible = false;

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

            case PrivacyMode.Acrylic:
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.AcrylicBlur,
                    WindowTransparencyLevel.Blur,
                    WindowTransparencyLevel.Transparent
                ];
                _root.Background = new SolidColorBrush(Color.FromArgb(48, 225, 225, 230));
                break;

            case PrivacyMode.Frosted:
                TransparencyLevelHint =
                [
                    WindowTransparencyLevel.AcrylicBlur,
                    WindowTransparencyLevel.Blur,
                    WindowTransparencyLevel.Transparent
                ];
                _root.Background = new SolidColorBrush(Color.FromArgb(118, 235, 238, 242));
                break;

            case PrivacyMode.PixelMosaic:
                TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                _root.Background = Brushes.Transparent;
                _mosaic.IsVisible = true;
                break;

            case PrivacyMode.Blackout:
                TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                Background = Brushes.Black;
                _root.Background = Brushes.Black;
                break;
        }
    }
}
