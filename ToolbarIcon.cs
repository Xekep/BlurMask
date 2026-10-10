using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace BlurMask;

internal enum ToolbarGlyph { Nib, Eye, ClosedEye, Cursor, Select, Pen, Highlighter, Text, Line, Arrow, Rectangle, Ellipse, Eraser, Size, Undo, Redo, Trash, Board, Camera, Menu, Mask, Shapes, Fading, MaskBlur, MaskPixels, MaskScramble, MaskGlass, MaskBlackout }

internal sealed class ToolbarIcon(ToolbarGlyph glyph, bool circle = false) : Control
{
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#333333"));
    private static readonly Pen Outline = new(Ink, 1.25, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly IReadOnlyDictionary<ToolbarGlyph, Geometry> Paths = new Dictionary<ToolbarGlyph, Geometry>
    {
        [ToolbarGlyph.Cursor] = Geometry.Parse("M3,2 L20,12 L12,14 L16,21 L12,23 L8,16 L3,21 Z"),
        [ToolbarGlyph.Select] = Geometry.Parse("M2,8 L2,2 L8,2 M16,2 L22,2 L22,8 M22,16 L22,22 L16,22 M8,22 L2,22 L2,16 M8,7 L18,14 L13,15 L11,20 Z"),
        [ToolbarGlyph.Pen] = Geometry.Parse("M3,17 L16,4 L20,8 L7,21 L2,22 Z M13,7 L17,11 M3,17 L7,21"),
        [ToolbarGlyph.Highlighter] = Geometry.Parse("M3,12 L10,5 L21,16 L14,23 Z M4,13 L1,17 L7,23 L11,20 M10,5 L12,3 L23,14 L21,16"),
        [ToolbarGlyph.Trash] = Geometry.Parse("M5,7 L19,7 L18,23 L6,23 Z M3,4 L21,4 M9,4 L9,1 L15,1 L15,4 M9,10 L9,20 M12,10 L12,20 M15,10 L15,20"),
        [ToolbarGlyph.Board] = Geometry.Parse("M3,3 L21,3 L21,17 L3,17 Z M1,1 L23,1 M12,17 L12,23 M7,23 L10,17 M17,23 L14,17"),
        [ToolbarGlyph.Camera] = Geometry.Parse("M2,7 L7,7 L9,3 L16,3 L18,7 L23,7 L23,21 L2,21 Z"),
        [ToolbarGlyph.Menu] = Geometry.Parse("M5,3 L19,3 L19,23 L5,23 Z M9,1 L15,1 L15,5 L9,5 Z M8,9 L16,9 M8,13 L16,13 M8,17 L16,17"),
        [ToolbarGlyph.Nib] = Geometry.Parse("M4,17 L16,3 C18,1 24,5 22,8 L10,23 C7,25 1,21 4,17 Z"),
        [ToolbarGlyph.Arrow] = Geometry.Parse("M3,22 L22,3 M12,3 L22,3 L22,13")
    };

    public override void Render(DrawingContext context)
    {
        using var transform = context.PushTransform(Matrix.CreateScale(Bounds.Width / 26, Bounds.Height / 26));
        using var inset = context.PushTransform(Matrix.CreateTranslation(1, 1));
        if (glyph == ToolbarGlyph.Nib)
        {
            if (circle) context.DrawEllipse(Brushes.Black, null, new Point(12, 12), 12, 12);
            context.DrawGeometry(new SolidColorBrush(Color.Parse("#F7F7F2")), new Pen(Ink, 1.5), Paths[glyph]);
            context.DrawGeometry(new SolidColorBrush(Color.Parse("#DDDCD4")), Outline,
                Geometry.Parse("M4,17 C7,15 13,20 10,23 C7,25 1,21 4,17 Z"));
            var grain = new Pen(new SolidColorBrush(Color.Parse("#C8C7BE")), .7, lineCap: PenLineCap.Round);
            context.DrawGeometry(null, grain, Geometry.Parse("M9,16 L16,7 M12,17 L18,10 M17,5 L19,6 M7,20 L9,21 M6,18 L7,19"));
            return;
        }
        if (Paths.TryGetValue(glyph, out var geometry))
        {
            using (context.PushTransform(Matrix.CreateTranslation(1, 1)))
                context.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromArgb(65, 0, 0, 0)), 2), geometry);
            context.DrawGeometry(glyph is ToolbarGlyph.Arrow ? null : Brushes.White, Outline, geometry);
            if (glyph == ToolbarGlyph.Camera)
            {
                context.DrawEllipse(Brushes.White, Outline, new Point(12.5, 13), 5, 5);
                context.DrawEllipse(Ink, null, new Point(12.5, 13), 2.5, 2.5);
                context.DrawRectangle(Ink, null, new Rect(4, 9, 3, 2));
            }
            return;
        }
        switch (glyph)
        {
            case ToolbarGlyph.MaskBlur:
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#C4D3DC")), Outline, new Rect(2, 3, 21, 19), 2, 2);
                foreach (var radius in new[] { 7d, 5d, 3d })
                    context.DrawEllipse(new SolidColorBrush(Color.FromArgb(65, 255, 255, 255)), null, new Point(12, 12), radius, radius * .75);
                break;
            case ToolbarGlyph.MaskPixels:
            case ToolbarGlyph.MaskScramble:
            case ToolbarGlyph.MaskGlass:
                context.DrawRectangle(Brushes.White, Outline, new Rect(2, 3, 21, 19), 1, 1);
                for (var y = 0; y < 3; y++)
                    for (var x = 0; x < 3; x++)
                    {
                        var shade = glyph == ToolbarGlyph.MaskScramble ? (byte)(35 + ((x * 11 + y * 7) % 6) * 25)
                            : (byte)(150 + ((x * 3 + y * 5) % 4) * 25);
                        var rect = new Rect(3 + x * 6.5, 4 + y * 5.7, 6, 5.2);
                        context.DrawRectangle(new SolidColorBrush(Color.FromRgb(shade, shade, shade)), null, rect);
                        if (glyph == ToolbarGlyph.MaskGlass)
                            context.DrawLine(new Pen(Brushes.White, .8), rect.TopLeft + new Vector(1, 4), rect.TopRight - new Vector(1, 0));
                    }
                break;
            case ToolbarGlyph.MaskBlackout:
                context.DrawRectangle(Ink, Outline, new Rect(2, 3, 21, 19), 1, 1);
                break;
            case ToolbarGlyph.ClosedEye:
                var lash = new Pen(Ink, 1.8, lineCap: PenLineCap.Round);
                context.DrawGeometry(null, lash, Geometry.Parse("M2,9 Q12,19 22,9 M4,12 L2,15 M8,14 L7,18 M12,15 L12,19 M16,14 L17,18 M20,12 L22,15"));
                break;
            case ToolbarGlyph.Eraser:
                context.DrawGeometry(new SolidColorBrush(Color.Parse("#DCE8ED")), Outline,
                    Geometry.Parse("M3,14 L13,4 Q14,3 15,4 L22,11 Q23,12 22,13 L13,22 L10,22 Z"));
                context.DrawGeometry(Brushes.White, Outline, Geometry.Parse("M3,14 L8,9 L17,18 L13,22 L10,22 Z"));
                context.DrawLine(Outline, new Point(9, 23), new Point(23, 23));
                break;
            case ToolbarGlyph.Undo:
            case ToolbarGlyph.Redo:
                using (context.PushTransform(glyph == ToolbarGlyph.Redo
                    ? new Matrix(-1, 0, 0, 1, 24, 0) : Matrix.Identity))
                {
                    var arrowPen = new Pen(Ink, 2.1, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
                    context.DrawGeometry(null, arrowPen, Geometry.Parse("M4,9 L13,9 C24,9 24,21 14,21 L10,21 M9,4 L4,9 L9,14"));
                }
                break;
            case ToolbarGlyph.Shapes:
                context.DrawRectangle(Brushes.White, Outline, new Rect(2, 2, 13, 13));
                context.DrawEllipse(Brushes.White, Outline, new Rect(9, 9, 14, 14));
                break;
            case ToolbarGlyph.Fading:
                context.DrawRectangle(Ink, null, new Rect(9, 1, 6, 2), .5, .5);
                context.DrawLine(new Pen(Ink, 1.5), new Point(12, 3), new Point(12, 5));
                context.DrawLine(new Pen(Ink, 2), new Point(18, 5), new Point(20, 7));
                context.DrawEllipse(Brushes.White, new Pen(Ink, 1.5), new Point(12, 14), 8.5, 8.5);
                context.DrawGeometry(null, Outline, Geometry.Parse("M12,7 L12,8 M12,20 L12,21 M5,14 L6,14 M18,14 L19,14"));
                context.DrawGeometry(null, new Pen(Ink, 1.7, lineCap: PenLineCap.Round), Geometry.Parse("M12,9 L12,14 L16,16"));
                context.DrawEllipse(Ink, null, new Point(12, 14), 1, 1);
                break;
            case ToolbarGlyph.Eye:
                context.DrawGeometry(Brushes.White, Outline, Geometry.Parse("M1,12 Q12,0 23,12 Q12,24 1,12 Z"));
                context.DrawEllipse(Brushes.White, Outline, new Point(12, 12), 5, 5);
                context.DrawEllipse(Ink, null, new Point(12, 12), 3, 3);
                break;
            case ToolbarGlyph.Size: context.DrawEllipse(Brushes.Black, null, new Point(12, 12), 3, 3); break;
            case ToolbarGlyph.Line: context.DrawLine(new Pen(Ink, 2), new Point(3, 3), new Point(22, 22)); break;
            case ToolbarGlyph.Rectangle: context.DrawRectangle(null, Outline, new Rect(2, 4, 21, 16)); break;
            case ToolbarGlyph.Ellipse: context.DrawEllipse(null, Outline, new Rect(2, 4, 21, 16)); break;
            case ToolbarGlyph.Text:
                context.DrawGeometry(null, new Pen(Ink, 2.4), Geometry.Parse("M3,22 L12,2 L21,22 M6,16 L18,16"));
                break;
            case ToolbarGlyph.Mask:
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#B7CAD8")), Outline, new Rect(2, 3, 21, 19), 2, 2);
                context.DrawLine(new Pen(Brushes.White, 3), new Point(5, 7), new Point(19, 7));
                context.DrawLine(new Pen(Brushes.White, 3), new Point(5, 13), new Point(15, 13));
                break;
        }
    }
}
