using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.VisualTree;
using BlurMask.Shared;
using System.Text.Json;

namespace BlurMask;

internal static class DrawingSmokeTest
{
    public static void VerifyDocument(InkDocument document)
    {
        var invite = new BoardConnection(CanvasCloudClient.DefaultServer, Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"),
            "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8");
        const string code = "ABEiM0RVZneImaq7zN3u_wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4f";
        Check(BoardInvite.Code(invite.Id, invite.Key) == code, "Native invitation differs from browser format.");
        Check(CanvasCloudClient.ParseLink(code) == invite, "Invitation code roundtrip failed.");
        Check(CanvasCloudClient.ParseLink(CanvasCloudClient.Link(invite)) == invite, "Invitation link roundtrip failed.");
        Check(CanvasCloudClient.ParseLink($"{invite.Server}/#board={invite.Id}&key={invite.Key}") == invite,
            "Already issued invitation stopped working.");
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
    internal static async Task VerifyCanvasAsync(DrawingSession session)
    {
        var desktopMark = new InkMark(InkTool.Pen, Colors.Red, 4, new Point(100, 100));
        session.DesktopDocument.Add(desktopMark);
        session.SetBoard(BoardMode.Black);
        session.OpenCanvas();
        await Task.Delay(250);
        var window = session.Canvas!;
        Check(session.Target == DrawingTarget.Canvas, "Canvas mode did not activate.");
        Check(session.Overlays.All(o => !o.IsVisible), "Screen board obscures independent canvas.");
        if (OperatingSystem.IsWindows())
        {
            foreach (var overlay in session.Overlays) DesktopInkNative.VerifyClickThrough(overlay);
            session.SelectTool(InkTool.Pen);
            var a = window.Surface.TranslatePoint(new Point(220, 230), window)!.Value;
            var b = window.Surface.TranslatePoint(new Point(450, 330), window)!.Value;
            DesktopInkNative.TestMouseGesture(window, (int)a.X, (int)a.Y, (int)b.X, (int)b.Y);
            await Task.Delay(100);
            Check(session.CanvasDocument.Marks.Count == 1, "Canvas pointer gesture did not create a stroke.");
            Check(session.CanvasDocument.Marks[0].Points.Count > 1, "Canvas motion did not extend stroke.");
        }
        else session.CanvasDocument.Add(new InkMark(InkTool.Pen, Colors.Black, 4, new Point(100, 100)));
        Check(session.DesktopDocument.Marks.SequenceEqual(new[] { desktopMark }), "Canvas changed desktop ink.");
        session.Undo(); Check(session.CanvasDocument.Marks.Count == 0, "Canvas undo failed.");
        session.Redo(); Check(session.CanvasDocument.Marks.Count == 1, "Canvas redo failed.");
        var alpha = new InkDocument();
        alpha.Add(new InkMark(InkTool.Pen, Color.FromArgb(128, 255, 0, 17), 4, new Point(10, 10)));
        var imported = new InkDocument(); imported.Import(alpha.Export());
        Check(imported.Marks[0].Color == alpha.Marks[0].Color, "CSS alpha color roundtrip failed.");
        var artifacts = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
        if (!string.IsNullOrEmpty(artifacts))
        {
            SaveWindow(window, Path.Combine(artifacts, "native-canvas.png"));
            window.ExportPng(Path.Combine(artifacts, "native-canvas-export.png"));
        }
        if (Environment.GetEnvironmentVariable("BLURMASK_TEST_SERVER") is { Length: > 0 } server)
        {
            await window.CreateCloudAsync(server);
            var storage = Path.Combine(artifacts ?? Path.GetTempPath(), "canvas-smoke", "canvas.json");
            var state = JsonSerializer.Deserialize(File.ReadAllText(storage), CanvasJsonContext.Default.CanvasState)!;
            using var client = new CanvasCloudClient();
            var remote = await client.ReadAsync(state.Connection!);
            Check(remote.Marks.Count == 1, "Native cloud upload failed.");
            var peer = new InkMarkDto { Points = [new(500, 300), new(900, 500)], Tool = "Arrow", Color = "#00C9ED" };
            await client.ApplyAsync(state.Connection!, new BoardPatch { Add = [peer] });
            await window.SyncAsync();
            Check(session.CanvasDocument.Marks.Count == 2, "Native canvas did not receive peer stroke.");
            session.Undo();
            Check(session.CanvasDocument.Marks.Count == 1 && session.CanvasDocument.Marks[0].Id == peer.Id,
                "Undo removed peer stroke.");
            await window.SyncAsync();
            Check((await client.ReadAsync(state.Connection!)).Marks.Count == 1, "Native erasure did not synchronize.");
            var viewLink = CanvasCloudClient.Link(state.Connection! with { Key = state.ViewKey! });
            await window.ConnectAsync(viewLink);
            Check(window.ReadOnly, "Native viewer is editable.");
            session.Clear(); session.Undo();
            Check(session.CanvasDocument.Marks.Count == 1, "Read-only history changed canvas.");
        }
        session.SelectScreen();
        session.SetBoard(BoardMode.Desktop);
        session.Undo();
        Check(session.DesktopDocument.Marks.Count == 0 && session.CanvasDocument.Marks.Count > 0,
            "Screen undo changed independent canvas.");
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
