using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BlurMask.Shared;

namespace BlurMask;

// Runs only inside the isolated native smoke host against its loopback API.
// Two real CanvasWindow render trees receive changes through the normal clients;
// models and recognition runtimes are unnecessary for testing canonical marks.
internal static class CollaborationVisualProbe
{
    private static readonly Point Origin = new(2000, 2000);
    private static double _zoom = .5;
    private static Vector Offset => new(20 - Origin.X * _zoom, 20 - Origin.Y * _zoom);
    private sealed record Fixture(string Name, InkMark Mark, Rect WorldRoi);
    private sealed record Pixels(int Width, int Height, byte[] Bgra);

    internal static async Task VerifyAsync(DrawingSession root)
    {
        var owner = root.Canvas ?? throw new InvalidOperationException("Shared canvas is missing.");
        _zoom = .5;
        var connection = owner.Connection ?? throw new InvalidOperationException("Shared connection is missing.");
        if (!Program.SmokeTest || !new Uri(connection.Server).IsLoopback)
            throw new InvalidOperationException("Visual collaboration probe requires isolated smoke mode and a loopback API.");
        if (owner.ReadOnly || owner.ViewKey is null || owner.BoardBackground != "white")
            throw new InvalidOperationException("Visual probe requires the editable white test board and its view key.");
        var directory = Path.Combine(Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") ?? Path.GetTempPath(), "collaboration-visual");
        Directory.CreateDirectory(directory);
        var viewerStateDirectory = Path.Combine(Path.GetTempPath(), "BlurMask-collaboration-viewer-" + Guid.NewGuid().ToString("N"));
        var log = new StringBuilder("phase,fixture,inkPixels,changedPixels,maxChannelError,meanChannelError\n");
        var ownedIds = new HashSet<Guid>();
        var oldZoom = owner.Surface.Zoom; var oldOffset = owner.Surface.Offset; var oldTool = root.Tool;
        using var viewerSession = new DrawingSession(() => { }, () => { }, _ => { }, () => { });
        using var cloud = new CanvasCloudClient();
        CanvasWindow? viewer = null;
        try
        {
            // Independent cache and session: neither client can see the other's document directly.
            viewerSession.OpenCanvas(Path.Combine(viewerStateDirectory, "canvas.json"));
            viewer = viewerSession.Canvas!;
            viewer.Width = owner.Width; viewer.Height = owner.Height;
            await viewer.ConnectAsync(CanvasCloudClient.Link(connection with { Key = owner.ViewKey }));
            await Wait(() => owner.LiveConnected && viewer.LiveConnected && viewer.ReadOnly &&
                owner.Surface.Bounds.Size == viewer.Surface.Bounds.Size, "Two native canvas clients did not connect/layout.");
            root.SelectTool(InkTool.Pen); viewerSession.SelectTool(InkTool.Cursor);
            SetViews(owner, viewer);

            foreach (var (name, mark) in PreviewFixtures())
            {
                ownedIds.Add(mark.Id);
                root.CanvasDocument.BeginEdit(); root.CanvasDocument.Preview = mark; root.CanvasDocument.Notify();
                await Wait(() => viewer.RemotePreviews.Any(m => m.Id == mark.Id && m.Points.Count == mark.Points.Count),
                    $"{name}: remote live preview missing before commit.");
                Check(!viewer.Snapshot().Marks.Any(m => m.Id == mark.Id), "Preview entered the persistent viewer document.");
                SetViews(owner, viewer);
                await CapturePair(owner, viewer, directory, "preview", name, Cell(0), log);
                if (mark.Tool == InkTool.Ellipse)
                {
                    mark.AddPoint(mark.Points[0] + new Vector(470, 280)); root.CanvasDocument.Notify();
                    await Wait(() => viewer.RemotePreviews.Any(m => m.Id == mark.Id && m.Points[^1] == mark.Points[^1]), "Live shape endpoint update was lost.");
                    await CapturePair(owner, viewer, directory, "preview-resized", name, Cell(0), log);
                }
                if (mark.Tool == InkTool.Pen)
                {
                    root.CanvasDocument.CompletePreview(); await owner.SyncAsync();
                    await Wait(() => viewer.Snapshot().Marks.Any(m => m.Id == mark.Id) && !viewer.RemotePreviews.Any(m => m.Id == mark.Id), "Preview-to-saved transition lost or duplicated the stroke.");
                    await CapturePair(owner, viewer, directory, "preview-committed", name, Cell(0), log);
                    root.CanvasDocument.BeginEdit(); root.CanvasDocument.Marks.RemoveAll(m => m.Id == mark.Id); root.CanvasDocument.Commit();
                    await owner.SyncAsync();
                    await Wait(() => !viewer.Snapshot().Marks.Any(m => m.Id == mark.Id), "Committed preview removal did not reach the viewer.");
                }
                else root.CanvasDocument.Cancel();
                await Wait(() => !viewer.RemotePreviews.Any(m => m.Id == mark.Id), $"{name}: canceled preview remained visible.");
                var cleared = Capture(viewer.Surface, Path.Combine(directory, $"preview-{name}-cleared.png"));
                Check(InkPixels(cleared, ToPixels(Cell(0), cleared)) == 0, $"{name}: canceled preview left painted pixels.");
            }

            var fixtures = SavedFixtures();
            foreach (var fixture in fixtures) ownedIds.Add(fixture.Mark.Id);
            root.CanvasDocument.BeginEdit(); root.CanvasDocument.Marks.AddRange(fixtures.Select(f => f.Mark)); root.CanvasDocument.Commit();
            await owner.SyncAsync();
            // Image forces notification-only WS -> authenticated HTTP fallback on the viewer.
            await Wait(() => fixtures.All(f => viewer.Snapshot().Marks.Any(m => m.Id == f.Mark.Id && BoardValidation.Same(m, InkDocument.ExportMark(f.Mark)))),
                "Viewer did not import all saved fixture fields through the live/HTTP paths.");
            SetViews(owner, viewer);
            foreach (var fixture in fixtures)
                await CapturePair(owner, viewer, directory, "saved", fixture.Name, fixture.WorldRoi, log);

            foreach (double zoom in new[] { .25, .1 })
            {
                _zoom = zoom; SetViews(owner, viewer);
                var phase = zoom == .25 ? "zoom25" : "zoom10";
                foreach (var fixture in fixtures)
                    await CapturePair(owner, viewer, directory, phase, fixture.Name, fixture.WorldRoi, log);
            }
            _zoom = .5; SetViews(owner, viewer);

            var viewerPixels = Capture(viewer.Surface, Path.Combine(directory, "saved-viewer-board.png"));
            var highlight = fixtures.Single(f => f.Name == "Highlighter").Mark;
            var sample = highlight.Points[0] + new Vector(220, 0);
            var local = new Point(sample.X * _zoom + Offset.X, sample.Y * _zoom + Offset.Y);
            var at = ((int)local.Y * viewerPixels.Width + (int)local.X) * 4;
            // Half-alpha cyan multiplied by the highlighter's 0.35 layer should be pale,
            // not fully opaque cyan or dark overlapping segment joints.
            Check(viewerPixels.Bgra[at + 2] is > 190 and < 230 && viewerPixels.Bgra[at + 1] > 235 && viewerPixels.Bgra[at] > 245,
                "Shared highlighter alpha/opacity produced the wrong center pixel.");

            var image = fixtures.Single(f => f.Name == "TransparentPng").Mark;
            var transparentPoint = image.Points[0] + (image.Points[1] - image.Points[0]) * .8;
            var imageLocal = new Point(transparentPoint.X * _zoom + Offset.X, transparentPoint.Y * _zoom + Offset.Y);
            var imageAt = ((int)imageLocal.Y * viewerPixels.Width + (int)imageLocal.X) * 4;
            Check(viewerPixels.Bgra.AsSpan(imageAt, 3).SequenceEqual(new byte[] { 255, 255, 255 }), "Transparent PNG obscured the board background.");

            var beforeView = viewer.Snapshot();
            var serverBeforeReadonly = await cloud.ReadAsync(connection);
            viewerSession.Clear(); viewerSession.Undo(); viewerSession.Redo();
            using (var bitmap = TransparentRaster()) viewer.InsertImage(bitmap);
            Check(SameMarks(beforeView, viewer.Snapshot()), "View-only native controls modified the document.");
            bool forbidden = false;
            try { await cloud.ApplyAsync(connection with { Key = owner.ViewKey }, new() { Remove = [fixtures[0].Mark.Id] }); }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden) { forbidden = true; }
            Check(forbidden, "Server accepted a view-only write.");
            var serverAfterReadonly = await cloud.ReadAsync(connection);
            Check(serverBeforeReadonly.Revision == serverAfterReadonly.Revision && SameMarks(serverBeforeReadonly, serverAfterReadonly), "Readonly attempts changed the server revision/document.");
            await viewer.SyncAsync();
            Check(SameMarks(beforeView, viewer.Snapshot()), "Readonly mutation changed shared marks.");
            SetViews(owner, viewer);
            await CapturePair(owner, viewer, directory, "readonly", "all-fixtures", MatrixRoi(), log);

            // A new connection must reconstruct the same pixels from the saved snapshot.
            await viewer.ConnectAsync(CanvasCloudClient.Link(connection with { Key = owner.ViewKey }));
            await Wait(() => viewer.LiveConnected && fixtures.All(f => viewer.Snapshot().Marks.Any(m => m.Id == f.Mark.Id)), "Viewer reconnect lost saved objects.");
            SetViews(owner, viewer);
            await CapturePair(owner, viewer, directory, "reconnect", "all-fixtures", MatrixRoi(), log);

            var fadingStart = Start(0) + new Vector(0, 320);
            var fading = new InkMark(InkTool.Pen, Colors.Magenta, 10, fadingStart) { FadeSeconds = 120 };
            fading.AddPoint(fadingStart + new Vector(180, 0)); ownedIds.Add(fading.Id);
            root.CanvasDocument.Add(fading); await owner.SyncAsync(); SetViews(owner, viewer);
            await Task.Delay(80);
            var fadingAuthor = Capture(owner.Surface, Path.Combine(directory, "local-fading-author.png"));
            var fadingViewer = Capture(viewer.Surface, Path.Combine(directory, "local-fading-viewer.png"));
            var fadingRoi = new Rect(fadingStart.X - 10, fadingStart.Y - 10, 200, 20);
            var authorInk = InkPixels(fadingAuthor, ToPixels(fadingRoi, fadingAuthor));
            var viewerInk = InkPixels(fadingViewer, ToPixels(fadingRoi, fadingViewer));
            Check(authorInk >= 40 && viewerInk == 0 && !viewer.Snapshot().Marks.Any(m => m.Id == fading.Id), "Fading ink was not isolated to its author.");
            log.AppendLine($"local-fading,expected-author-only,{authorInk},0,0,0");
            Console.WriteLine("Two native Canvas clients: live preview, saved visual parity, PNG/Math/transforms/readonly/reconnect PASS.");
        }
        finally
        {
            File.WriteAllText(Path.Combine(directory, "pixel-metrics.csv"), log.ToString());
            root.CanvasDocument.Cancel();
            if (ownedIds.Count > 0)
            {
                root.CanvasDocument.BeginEdit(); root.CanvasDocument.Marks.RemoveAll(m => ownedIds.Contains(m.Id));
                root.CanvasDocument.FadingMarks.RemoveAll(m => ownedIds.Contains(m.Id)); root.CanvasDocument.Commit();
                await owner.SyncAsync();
            }
            viewerSession.Dispose();
            // This uniquely named directory contains only this probe's flat cache files.
            // Keep invitation data out of uploaded visual artifacts.
            if (Directory.Exists(viewerStateDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(viewerStateDirectory)) File.Delete(file);
                Directory.Delete(viewerStateDirectory);
            }
            _zoom = .5; root.SelectTool(oldTool); owner.Surface.SetTestView(oldZoom, oldOffset);
        }
    }

    private static async Task Wait(Func<bool> ready, string message)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (!ready()) { Check(DateTime.UtcNow < deadline, message); await Task.Delay(20); }
        await Task.Delay(60); // Allow the invalidated render trees to settle.
    }
    private static void SetViews(CanvasWindow owner, CanvasWindow viewer)
    { owner.Surface.SetTestView(_zoom, Offset); viewer.Surface.SetTestView(_zoom, Offset); owner.Refresh(); viewer.Refresh(); }
    private static Rect Cell(int index) => new(Origin.X + index % 3 * 600 + 5, Origin.Y + index / 3 * 400 + 5, 580, 380);
    private static Rect MatrixRoi() => new(Origin.X + 5, Origin.Y + 5, 1780, 1180);
    private static Point Start(int index) => new(Origin.X + index % 3 * 600 + 40, Origin.Y + index / 3 * 400 + 50);

    private static InkMark Stroke(InkTool tool, Point start, Color color, double width, int points = 601, bool wave = true)
    {
        var mark = new InkMark(tool, color, width, start);
        for (int i = 1; i < points; i++) mark.AddPoint(start + new Vector(i * .75, wave ? System.Math.Sin(i * .03) * 35 : 0));
        return mark;
    }
    private static IEnumerable<(string, InkMark)> PreviewFixtures()
    {
        yield return ("Pen601", Stroke(InkTool.Pen, Start(0) + new Vector(0, 120), Colors.Red, 10));
        yield return ("Highlighter601", Stroke(InkTool.Highlighter, Start(0) + new Vector(0, 120), Color.FromArgb(128, 0, 201, 237), 32, wave: false));
        var shape = new InkMark(InkTool.Ellipse, Colors.Blue, 8, Start(0)); shape.AddPoint(Start(0) + new Vector(450, 250));
        yield return ("Ellipse", shape);
    }
    private static List<Fixture> SavedFixtures()
    {
        var fixtures = new List<Fixture>();
        var tools = new[] { InkTool.Pen, InkTool.Highlighter, InkTool.Line, InkTool.Arrow, InkTool.Rectangle, InkTool.Ellipse };
        for (int i = 0; i < tools.Length; i++)
        {
            var tool = tools[i]; var start = Start(i);
            var mark = tool is InkTool.Pen or InkTool.Highlighter
                ? Stroke(tool, start + new Vector(0, 120), tool == InkTool.Highlighter ? Color.FromArgb(128, 0, 201, 237) : Colors.Red, tool == InkTool.Highlighter ? 32 : 8, wave: tool != InkTool.Highlighter)
                : new InkMark(tool, Color.FromArgb(180, 20, 65, 220), 8, start);
            if (tool is not (InkTool.Pen or InkTool.Highlighter)) mark.AddPoint(start + new Vector(450, 250));
            if (tool == InkTool.Rectangle)
            {
                var center = mark.Bounds.Center;
                mark = mark.WithTransform(Matrix.CreateTranslation(-center.X, -center.Y) * Matrix.CreateScale(.85, 1.1) *
                    Matrix.CreateRotation(.18) * Matrix.CreateTranslation(center.X, center.Y));
            }
            fixtures.Add(new(tool.ToString(), mark, Cell(i)));
        }
        var text = new InkMark(InkTool.Text, Colors.Black, 1, Start(6)) { Font = "Inter", Text = "Hello русский Ω", FontSize = 38 };
        var textCell = Cell(6);
        fixtures.Add(new("TypedTextInter", text, new Rect(textCell.X, textCell.Y, textCell.Width, 120)));
        var fitted = new InkMark(InkTool.Text, Colors.DarkGreen, 1, Start(6) + new Vector(0, 150)) { Font = "Inter", Text = "AI fitted text", FontSize = 38 };
        fitted.AddPoint(Start(6) + new Vector(430, 260));
        fixtures.Add(new("AiTextInter", fitted, new Rect(textCell.X, textCell.Y + 145, textCell.Width, 230)));
        using var raster = TransparentRaster(); using var bytes = new MemoryStream(); raster.Save(bytes, PngBitmapEncoderOptions.Default);
        var image = new InkMark(InkTool.Image, Colors.Black, 1, Start(7)) { Image = Convert.ToBase64String(bytes.ToArray()) };
        image.AddPoint(Start(7) + new Vector(450, 280)); fixtures.Add(new("TransparentPng", image, Cell(7)));
        var math = new InkMark(InkTool.Text, Colors.Black, 1, Start(8)) { Font = "STIX Two Math", Text = "\\frac{1}{x}+\\left\\{\\begin{matrix}\\sqrt{3}\\\\\\sqrt{6}\\end{matrix}\\right.", FontSize = 48 };
        Check(MathTypesetting.Supported(math.Text), "Math fixture is unsupported by the native typesetter.");
        math.AddPoint(Start(8) + new Vector(450, 280)); fixtures.Add(new("MathFractionRadicalsTable", math, Cell(8)));
        return fixtures;
    }

    private sealed class RasterControl : Control
    {
        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)), null, new Rect(0, 0, 32, 32));
            context.DrawRectangle(Brushes.Blue, null, new Rect(32, 0, 32, 32));
        }
    }
    private static RenderTargetBitmap TransparentRaster()
    {
        var control = new RasterControl(); control.Measure(new Size(64, 64)); control.Arrange(new Rect(0, 0, 64, 64));
        var bitmap = new RenderTargetBitmap(new PixelSize(64, 64), new Vector(96, 96)); bitmap.Render(control); return bitmap;
    }
    private static Pixels Capture(Control control, string path)
    {
        var size = new PixelSize((int)System.Math.Ceiling(control.Bounds.Width), (int)System.Math.Ceiling(control.Bounds.Height));
        Check(size.Width > 0 && size.Height > 0, "Canvas has no capture size.");
        using var rendered = new RenderTargetBitmap(size, new Vector(96, 96)); rendered.Render(control); rendered.Save(path, PngBitmapEncoderOptions.Default);
        using var converted = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using var locked = converted.Lock(); rendered.CopyPixels(locked);
        var pixels = new byte[size.Width * size.Height * 4];
        for (int y = 0; y < size.Height; y++) Marshal.Copy(locked.Address + y * locked.RowBytes, pixels, y * size.Width * 4, size.Width * 4);
        return new(size.Width, size.Height, pixels);
    }
    private static PixelRect ToPixels(Rect world, Pixels pixels)
    {
        var rect = new Rect(world.X * _zoom + Offset.X, world.Y * _zoom + Offset.Y, world.Width * _zoom, world.Height * _zoom);
        var x = System.Math.Max(0, (int)rect.X); var y = System.Math.Max(0, (int)rect.Y);
        var right = System.Math.Min(pixels.Width, (int)System.Math.Ceiling(rect.Right)); var bottom = System.Math.Min(pixels.Height, (int)System.Math.Ceiling(rect.Bottom));
        Check(right > x && bottom > y, "Fixture ROI is outside the canvas viewport."); return new(x, y, right - x, bottom - y);
    }
    private static int InkPixels(Pixels pixels, PixelRect roi)
    {
        int count = 0;
        for (int y = roi.Y; y < roi.Bottom; y++) for (int x = roi.X; x < roi.Right; x++)
        { int p = (y * pixels.Width + x) * 4; if (pixels.Bgra[p] < 245 || pixels.Bgra[p + 1] < 245 || pixels.Bgra[p + 2] < 245) count++; }
        return count;
    }
    private static async Task CapturePair(CanvasWindow owner, CanvasWindow viewer, string directory, string phase, string name, Rect worldRoi, StringBuilder log)
    {
        await Task.Delay(80);
        var a = Capture(owner.Surface, Path.Combine(directory, $"{phase}-{name}-author.png"));
        var b = Capture(viewer.Surface, Path.Combine(directory, $"{phase}-{name}-viewer.png"));
        Check(a.Width == b.Width && a.Height == b.Height, "Client capture sizes differ.");
        var roi = ToPixels(worldRoi, a); int changed = 0, maximum = 0; long sum = 0;
        var heat = new byte[a.Bgra.Length];
        for (int y = roi.Y; y < roi.Bottom; y++) for (int x = roi.X; x < roi.Right; x++)
        {
            int p = (y * a.Width + x) * 4, error = 0;
            for (int c = 0; c < 3; c++) { int e = System.Math.Abs(a.Bgra[p + c] - b.Bgra[p + c]); sum += e; error = System.Math.Max(error, e); }
            maximum = System.Math.Max(maximum, error); if (error > 3) changed++;
            heat[p + 2] = (byte)System.Math.Min(255, error * 8); heat[p + 3] = 255;
        }
        using (var bitmap = new WriteableBitmap(new PixelSize(a.Width, a.Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque))
        {
            using (var locked = bitmap.Lock()) for (int y = 0; y < a.Height; y++) Marshal.Copy(heat, y * a.Width * 4, locked.Address + y * locked.RowBytes, a.Width * 4);
            bitmap.Save(Path.Combine(directory, $"{phase}-{name}-diff.png"), PngBitmapEncoderOptions.Default);
        }
        int ink = InkPixels(b, roi); double mean = (double)sum / (roi.Width * roi.Height * 3);
        log.AppendLine(FormattableString.Invariant($"{phase},{name},{ink},{changed},{maximum},{mean:F6}"));
        Check(ink >= (_zoom <= .1 ? 10 : 40), $"{phase}/{name}: remote content is blank.");
        Check(changed <= roi.Width * roi.Height * .002 && mean <= .25, $"{phase}/{name}: native clients paint different pixels (changed={changed}, max={maximum}, mean={mean:F4}).");
    }
    private static bool SameMarks(BoardSnapshot a, BoardSnapshot b) => a.Marks.Count == b.Marks.Count &&
        a.Marks.Zip(b.Marks).All(pair => pair.First.Id == pair.Second.Id && BoardValidation.Same(pair.First, pair.Second));
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
