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
        VerifyGroups();
        VerifyMaskEraser();
        Console.WriteLine("Selection: 7 object types, Shift toggles, marquee/all, group move/resize/rotation/delete, atomic undo/cancel, peer serialization and mask eraser geometry passed.");
    }
    private static void VerifyGroups()
    {
        InkMark Mark(InkTool tool, Point first, Point last)
        {
            var mark = new InkMark(tool, Colors.Red, 4, first) { Text = "selected" };
            mark.AddPoint(last); return mark;
        }
        Rect Bounds(IEnumerable<InkMark> marks) => marks.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
        InkMark Frame(IEnumerable<InkMark> marks)
        {
            var bounds = Bounds(marks); var frame = new InkMark(InkTool.Rectangle, Colors.Transparent, 0, bounds.TopLeft);
            frame.AddPoint(bounds.BottomRight); return frame;
        }
        var a = Mark(InkTool.Line, new(100, 100), new(160, 130));
        var b = Mark(InkTool.Rectangle, new(240, 100), new(300, 160));
        var peer = Mark(InkTool.Rectangle, new(500, 500), new(540, 540));
        var doc = new InkDocument(); doc.Add(a); doc.Add(b); doc.Add(peer);
        var select = new InkSelection(doc);
        void Selected(params InkMark[] marks) => Check(select.SelectedIds.ToHashSet().SetEquals(marks.Select(m => m.Id)), "Wrong selected group.");
        select.Begin(a.Points[0], 6); select.Complete(); Selected(a);
        select.Begin(b.Points[0], 6, true); select.Complete(); Selected(a, b);
        select.Begin(a.Points[0], 6, true); select.Complete(); Selected(b);
        select.Begin(a.Points[0], 6, true); select.Complete(); Selected(a, b);
        select.Begin(new(400, 300), 6); select.Complete(); Selected();
        select.Begin(new(50, 50), 6); select.Move(new(350, 200)); select.Complete(); Selected(a, b);
        Check(!doc.HasActiveEdit, "Marquee opened a document edit.");
        select.Begin(new(450, 450), 6, true); select.Move(new(550, 550)); select.Complete(); Selected(a, b, peer);
        select.Begin(peer.Points[0], 6, true); select.Complete(); Selected(a, b);
        select.Begin(new(50, 50), 6); select.Move(new(200, 200)); Selected(a); select.Cancel(); Selected(a, b);

        var start = new Point(130, 115); var delta = new Vector(80, 60);
        select.Begin(start, 6); select.Move(start + delta, true); select.Complete(); Selected(a, b);
        var moved = doc.Marks.Take(2).ToArray();
        Check(moved[0].Bounds == a.Bounds.Translate(delta) && moved[1].Bounds == b.Bounds.Translate(delta) &&
            ReferenceEquals(doc.Marks[2], peer), "Group drag changed relative positions or unselected peer.");
        doc.Undo(); Check(doc.Marks[0].Same(a) && doc.Marks[1].Same(b), "Group move undo was not atomic.");
        doc.Redo(); Check(doc.Marks.Take(2).Zip(moved).All(p => p.First.Same(p.Second)), "Group move redo failed.");

        var frame = Frame(moved); var anchor = InkSelection.HandlePoint(frame, 0, .4);
        var corner = InkSelection.HandlePoint(frame, 4, .4);
        select.Begin(corner, 6 / .4); select.Move(corner + new Vector(60, 40), true); select.Complete();
        var resized = doc.Marks.Take(2).ToArray();
        Check(Bounds(resized).Width > Bounds(moved).Width && Bounds(resized).Height > Bounds(moved).Height &&
            ((Vector)(Bounds(resized).TopLeft - anchor)).Length < .00001, "Group resize lost the shared fixed anchor.");
        doc.Undo(); Check(doc.Marks.Take(2).Zip(moved).All(p => p.First.Same(p.Second)), "Group resize undo failed.");
        doc.Redo();

        frame = Frame(resized); var center = frame.Bounds.Center;
        var rotate = InkSelection.HandlePoint(frame, 8, 2); var vector = rotate - center;
        select.Begin(rotate, 3); select.Move(center + new Vector(-vector.Y, vector.X), true, true); select.Complete();
        var rotated = doc.Marks.Take(2).ToArray();
        for (var i = 0; i < 2; i++)
        {
            var offset = resized[i].Bounds.Center - center;
            Check(((Vector)(rotated[i].Bounds.Center - (center + new Vector(-offset.Y, offset.X)))).Length < .00001,
                "Group rotation lost relative positions.");
            Check(BoardValidation.Valid(InkDocument.ExportMark(rotated[i])), "Server rejected group transform.");
        }
        var imported = new InkDocument(); imported.Import(doc.Export());
        Check(imported.Marks.Zip(doc.Marks).All(p => p.First.Same(p.Second)), "Peer lost group transforms.");
        select.Delete(); Check(doc.Marks.Count == 1 && ReferenceEquals(doc.Marks[0], peer), "Delete removed an unselected peer or left group members.");
        Selected(); doc.Undo(); Check(doc.Marks.Take(2).Zip(rotated).All(p => p.First.Same(p.Second)), "Delete undo was not atomic.");
        doc.Undo(); Check(doc.Marks.Take(2).Zip(resized).All(p => p.First.Same(p.Second)), "Selection-only operations polluted undo history.");
        select.SelectAll(); Selected(a, b, peer); select.Delete(); Check(doc.Marks.Count == 0, "Select all/delete left objects.");
        doc.Undo(); Check(doc.Marks.Count == 3, "Delete all undo failed.");

        var nearA = Mark(InkTool.Text, new(99900, 100), new(99920, 120));
        var nearB = Mark(InkTool.Text, new(99950, 100), new(99970, 120));
        var limited = new InkDocument(); limited.Add(nearA); limited.Add(nearB); var edge = new InkSelection(limited); edge.SelectAll();
        edge.Begin(new(99910, 110), 6); edge.Move(new(100910, 110), true);
        Check(Bounds(limited.Marks).Right <= 100000 && limited.Marks[0].Bounds.X - nearA.Bounds.X ==
            limited.Marks[1].Bounds.X - nearB.Bounds.X, "Canvas boundary clamping split the group.");
        edge.Cancel(); Check(limited.Marks[0].Same(nearA) && limited.Marks[1].Same(nearB), "Group cancellation did not restore all objects.");
        corner = InkSelection.HandlePoint(Frame(limited.Marks), 4, 1);
        edge.Begin(corner, 6); edge.Move(corner + new Vector(1000, 1000), true); edge.Complete();
        Check(limited.Marks[0].Same(nearA) && limited.Marks[1].Same(nearB), "Invalid resize partially changed the group.");
    }
    private static void VerifyMaskEraser()
    {
        var mask = new Rect(-300, -200, 200, 100);
        Check(CanvasGeometry.IntersectsEraser(mask, new(-200, -150), 0), "Eraser missed mask interior.");
        Check(CanvasGeometry.IntersectsEraser(mask, new(-310, -150), 10), "Eraser missed touching mask edge.");
        Check(!CanvasGeometry.IntersectsEraser(mask, new(-308, -208), 10), "Eraser used square bounds at a mask corner.");
        Check(!CanvasGeometry.IntersectsEraser(mask, new(-311, -150), 10), "Eraser removed a distant mask.");
        Check(!CanvasGeometry.IntersectsEraser(default, default, 10) && !CanvasGeometry.IntersectsEraser(mask, default, double.NaN),
            "Invalid mask/eraser geometry was accepted.");
    }
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
