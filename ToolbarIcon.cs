using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace BlurMask;

internal enum ToolbarGlyph { Nib, Eye, Cursor, Pen, Highlighter, Text, Line, Arrow, Rectangle, Ellipse, Eraser, Size, Undo, Redo, Trash, Board, Camera, Menu, Mask }

internal sealed class ToolbarIcon(ToolbarGlyph glyph) : Control
{
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#333333"));
    private static readonly Pen Outline = new(Ink, 1.25, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly IReadOnlyDictionary<ToolbarGlyph, Geometry> Paths = new Dictionary<ToolbarGlyph, Geometry>
    {
        [ToolbarGlyph.Cursor] = Geometry.Parse("M3,2 L20,12 L12,14 L16,21 L12,23 L8,16 L3,21 Z"),
        [ToolbarGlyph.Pen] = Geometry.Parse("M3,17 L16,4 L20,8 L7,21 L2,22 Z M13,7 L17,11 M3,17 L7,21"),
        [ToolbarGlyph.Highlighter] = Geometry.Parse("M3,12 L10,5 L21,16 L14,23 Z M4,13 L1,17 L7,23 L11,20 M10,5 L12,3 L23,14 L21,16"),
        [ToolbarGlyph.Eraser] = Geometry.Parse("M2,13 L11,3 L23,15 L15,23 L12,23 Z M7,8 L19,19 M5,16 L11,10"),
        [ToolbarGlyph.Undo] = Geometry.Parse("M10,3 L2,10 L10,16 L10,12 C21,10 23,21 12,22 L12,18 C19,18 18,14 10,16 Z"),
        [ToolbarGlyph.Redo] = Geometry.Parse("M14,3 L22,10 L14,16 L14,12 C3,10 1,21 12,22 L12,18 C5,18 6,14 14,16 Z"),
        [ToolbarGlyph.Trash] = Geometry.Parse("M5,7 L19,7 L18,23 L6,23 Z M3,4 L21,4 M9,4 L9,1 L15,1 L15,4 M9,10 L9,20 M12,10 L12,20 M15,10 L15,20"),
        [ToolbarGlyph.Board] = Geometry.Parse("M3,3 L21,3 L21,17 L3,17 Z M1,1 L23,1 M12,17 L12,23 M7,23 L10,17 M17,23 L14,17"),
        [ToolbarGlyph.Camera] = Geometry.Parse("M2,7 L7,7 L9,3 L16,3 L18,7 L23,7 L23,21 L2,21 Z"),
        [ToolbarGlyph.Menu] = Geometry.Parse("M5,3 L19,3 L19,23 L5,23 Z M9,1 L15,1 L15,5 L9,5 Z M8,9 L16,9 M8,13 L16,13 M8,17 L16,17"),
        [ToolbarGlyph.Nib] = Geometry.Parse("M6,3 L18,3 L21,13 L14,23 L10,23 L3,13 Z M12,12 L12,23"),
        [ToolbarGlyph.Arrow] = Geometry.Parse("M3,22 L22,3 M12,3 L22,3 L22,13")
    };

    public override void Render(DrawingContext context)
    {
        using var transform = context.PushTransform(Matrix.CreateScale(Bounds.Width / 26, Bounds.Height / 26));
        using var inset = context.PushTransform(Matrix.CreateTranslation(1, 1));
        if (glyph == ToolbarGlyph.Nib)
        {
            context.DrawEllipse(Brushes.Black, null, new Point(12, 12), 12, 12);
            context.DrawGeometry(Brushes.White, null, Paths[glyph]);
            context.DrawEllipse(Brushes.Black, null, new Point(12, 10), 2.5, 2.5);
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
