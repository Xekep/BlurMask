using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace BlurMask;

internal static class DrawingSmokeTest
{
    public static void VerifyDocument(InkDocument document)
    {
        var pen = new InkMark(InkTool.Pen, Colors.Red, 4, new Point(-100, 100));
        pen.AddPoint(new Point(100, 100));
        Check(pen.Hit(new Point(0, 101), 2), "Eraser misses line crossing a negative monitor origin.");
        Check(!pen.Hit(new Point(0, 120), 2), "Eraser deletes an unrelated stroke.");
        document.Add(pen);
        var box = new InkMark(InkTool.Rectangle, Colors.Blue, 4, new Point(200, 200));
        box.AddPoint(new Point(100, 100));
        Check(box.Hit(new Point(100, 150), 2), "Reverse-dragged rectangle is not erasable.");
        Check(!box.Hit(new Point(150, 150), 2), "Rectangle interior erases its outline.");
        document.Add(box);
        document.BeginEdit();
        document.Marks.Remove(pen);
        document.Commit();
        document.Undo();
        Check(document.Marks.SequenceEqual(new[] { pen, box }), "Eraser undo changes stroke order.");
        document.Redo();
        Check(document.Marks.SequenceEqual(new[] { box }), "Eraser redo failed.");
        document.Clear();
        document.Undo();
        Check(document.Marks.Count == 1, "Clear cannot be undone.");
        document.BeginEdit();
        document.Marks.Clear();
        document.Cancel();
        Check(document.Marks.Count == 1, "Cancelled gesture destroys document.");
        document.Undo();
        document.Add(pen);
        Check(!document.CanRedo, "Drawing after undo retains invalid redo history.");
        document.Clear();
    }

    public static async Task VerifyRenderingAsync(DrawingSession session)
    {
        var originalOverlays = session.Overlays.ToArray();
        session.Toolbar.Hide();
        session.Toolbar.Show();
        Check(session.Overlays.SequenceEqual(originalOverlays), "Showing the toolbar recreated the ink overlay.");
        if (OperatingSystem.IsWindows())
        {
            var overlay = session.Overlays.First();
            var count = session.Document.Marks.Count;
            session.SelectTool(InkTool.Pen);
            DesktopInkNative.VerifyInputTarget(overlay, true);
            DesktopInkNative.TestMouseGesture(overlay, 180, 120, 260, 180);
            await Task.Delay(120);
            Check(session.Document.Marks.Count == count + 1, "Mouse input did not create a desktop stroke.");
            Check(session.Document.Marks[^1].Points.Count > 1, "Mouse motion did not extend the stroke.");
            session.SelectTool(InkTool.Eraser);
            DesktopInkNative.TestMouseGesture(overlay, 180, 120, 260, 180);
            await Task.Delay(120);
            Check(session.Document.Marks.Count == count, "Eraser mouse gesture did not remove the stroke.");
            session.Undo();
            Check(session.Document.Marks.Count == count + 1, "Eraser gesture undo failed.");
            foreach (var tool in new[] { InkTool.Highlighter, InkTool.Line, InkTool.Arrow, InkTool.Rectangle, InkTool.Ellipse })
            {
                var previousCount = session.Document.Marks.Count;
                session.SelectTool(tool);
                DesktopInkNative.TestMouseGesture(overlay, 350, 220, 250, 160);
                await Task.Delay(50);
                Check(session.Document.Marks.Count == previousCount + 1 && session.Document.Marks[^1].Tool == tool,
                    $"Mouse drawing failed for {tool}.");
                Check(session.Document.Marks[^1].Points.Count > 1, $"Reverse drag failed for {tool}.");
            }
            session.SelectTool(InkTool.Text);
            DesktopInkNative.TestMouseGesture(overlay, 380, 260, 380, 260);
            await Task.Delay(80);
            var editor = overlay.GetVisualDescendants().OfType<TextBox>().Single();
            editor.Text = "Пример\nТекст";
            DesktopInkNative.TestKey(overlay, 0x0D);
            await Task.Delay(80);
            Check(!overlay.GetVisualDescendants().OfType<TextBox>().Any(), "Enter did not close the text editor.");
            Check(session.Document.Marks[^1].Tool == InkTool.Text && session.Document.Marks[^1].Text == "Пример\nТекст",
                "Enter did not commit multiline text.");
            var beforeCancel = session.Document.Marks.Count;
            session.SelectTool(InkTool.Text);
            DesktopInkNative.TestMouseGesture(overlay, 380, 260, 380, 260);
            await Task.Delay(80);
            overlay.GetVisualDescendants().OfType<TextBox>().Single().Text = "Отмена";
            DesktopInkNative.TestKey(overlay, 0x1B);
            await Task.Delay(80);
            Check(session.Document.Marks.Count == beforeCancel && !overlay.GetVisualDescendants().OfType<TextBox>().Any(),
                "Escape did not cancel the text editor.");
            session.Clear();
            session.SelectTool(InkTool.Cursor);
        }
        var directory = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        // Render the actual toolbar, not a separate HTML/mockup implementation.
        await Task.Delay(160);
        if (OperatingSystem.IsWindows())
            SaveWindow(session.Toolbar, Path.Combine(directory, "drawing-toolbar-desktop.png"));
        using (var bitmap = new RenderTargetBitmap(new PixelSize(42, (int)Math.Ceiling(session.Toolbar.Bounds.Height)), new Vector(96, 96)))
        {
            bitmap.Render(session.Toolbar);
            bitmap.Save(Path.Combine(directory, "drawing-toolbar.png"), PngBitmapEncoderOptions.Default);
        }
        session.Toolbar.ShowTools();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-tools.png");
        session.Toolbar.ShowPalette();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-palette.png");
        session.Toolbar.ShowMenu();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-menu.png");
        session.Toolbar.CloseFlyout();
        if (OperatingSystem.IsWindows())
        {
            session.SelectTool(InkTool.Pen);
            session.Document.Add(new InkMark(InkTool.Pen, Color.Parse("#FF0099"), 6, new Point(200, 200)));
            var line = new InkMark(InkTool.Arrow, Color.Parse("#00C9ED"), 5, new Point(250, 250));
            line.AddPoint(new Point(500, 320));
            session.Document.Add(line);
            await Task.Delay(180);
            DesktopInkNative.SaveScreenshot(Path.Combine(directory, "drawing-desktop-active.png"));
            session.SelectTool(InkTool.Cursor);
            await Task.Delay(180);
            DesktopInkNative.SaveScreenshot(Path.Combine(directory, "drawing-desktop-cursor.png"));
            session.Clear();
        }
    }
    private static void SaveFlyout(DrawingSession session, string directory, string name)
    {
        var window = session.Toolbar.OpenFlyout ?? throw new InvalidOperationException("Flyout did not open.");
        if (OperatingSystem.IsWindows()) SaveWindow(window, Path.Combine(directory, name.Replace(".png", "-desktop.png")));
        using var bitmap = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(window.Bounds.Width),
            (int)Math.Ceiling(window.Bounds.Height)), new Vector(96, 96));
        bitmap.Render(window);
        bitmap.Save(Path.Combine(directory, name), PngBitmapEncoderOptions.Default);
    }
    private static void SaveWindow(Avalonia.Controls.Window window, string path) =>
        DesktopInkNative.SaveScreenshot(path, new PixelRect(window.Position, new PixelSize(
            (int)Math.Ceiling(window.Bounds.Width * window.RenderScaling),
            (int)Math.Ceiling(window.Bounds.Height * window.RenderScaling))));
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
