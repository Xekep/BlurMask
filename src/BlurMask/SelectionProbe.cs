using Avalonia;
using Avalonia.Media;
using BlurMask.Shared;
using System.Text.Json;

namespace BlurMask;

internal static class SelectionProbe
{
    internal static void Run()
    {
        foreach (var tool in new[] { InkTool.Pen, InkTool.Line, InkTool.Arrow, InkTool.Rectangle, InkTool.Ellipse, InkTool.Text, InkTool.Image })
        {
            var original = new InkMark(tool, Colors.Red, 4, new(100, 100)) { Text = "AaЖ1+", Image = tool == InkTool.Image ?
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR1cAAAAASUVORK5CYII=" : null };
            original.AddPoint(new(220, 180)); var doc = new InkDocument(); doc.Add(original); var select = new InkSelection(doc);
            select.Begin(tool == InkTool.Ellipse ? new Point(160, 100) : original.Points[0], 6); select.Complete();
            var opposite = InkSelection.HandlePoint(original, 0, .4);
            var handle = InkSelection.HandlePoint(original, 4, .4);
            select.Begin(handle, 6 / .4); select.Move(handle + new Vector(60, 40), true); select.Complete();
            var sized = doc.Marks.Single();
            Check(sized.Bounds.Width > original.Bounds.Width && sized.Bounds.Height > original.Bounds.Height, "Resize did not grow the object.");
            Check(((Vector)(InkSelection.HandlePoint(sized, 0, .4) - opposite)).Length < .00001, "Opposite corner moved.");
            doc.Undo(); Check(doc.Marks.Single().Same(original), "Resize undo failed."); doc.Redo(); Check(doc.Marks.Single().Same(sized), "Resize redo failed.");
            var center = sized.BaseBounds.Center.Transform(sized.Transform); var rotate = InkSelection.HandlePoint(sized, 8, 2);
            var vector = rotate - center; select.Begin(rotate, 3); select.Move(center + new Vector(-vector.Y, vector.X), true); select.Complete();
            var rotated = doc.Marks.Single();
            Check(((Vector)(rotated.BaseBounds.Center.Transform(rotated.Transform) - center)).Length < .00001, "Rotation moved the center.");
            var onInk = original.Points[0].Transform(rotated.Transform);
            if (tool == InkTool.Ellipse) onInk = new Point(160, 100).Transform(rotated.Transform);
            Check(rotated.Hit(onInk, 5), "Rotated ink cannot be selected/erased.");
            var saved = InkDocument.ExportMark(rotated); Check(BoardValidation.Valid(saved), "Transformed object rejected by server.");
            var payload = JsonSerializer.Serialize(new BoardSnapshot { Marks = [saved] }, BoardJsonContext.Default.BoardSnapshot);
            var imported = JsonSerializer.Deserialize(payload, BoardJsonContext.Default.BoardSnapshot)!;
            var peer = new InkDocument(); peer.Import(imported.Marks); Check(peer.Marks.Single().Same(rotated), "Peer/save round-trip lost transformation.");
            var edge = InkSelection.HandlePoint(rotated, 3, 2); select.Begin(edge, 3); select.Move(edge + new Vector(12, 40), true); select.Cancel();
            Check(doc.Marks.Single().Same(rotated), "Cancel did not restore rotated object.");
            var bad = saved with { Transform = new(double.NaN,0,0,1,0,0) }; Check(!BoardValidation.Valid(bad), "Nonfinite transform accepted.");
            Check(!BoardValidation.Valid(saved with { Transform = new(0,0,0,0,0,0) }), "Singular transform accepted.");
        }
        Console.WriteLine("Selection: 7 object types; zoom-aware resize/rotation, fixed anchors, hit testing, undo/redo, cancel and peer serialization passed.");
    }
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
