using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Runtime.InteropServices;

namespace NovaPen;

// Reproducible screenshot of the real app, with a small sketch drawn using its ink tools.
// Run --website-preview --software-rendering with BLURMASK_SITE_PREVIEW pointing to a PNG.
internal static partial class WebsitePreview
{
    internal static async Task SaveAsync(DrawingSession session)
    {
        var path = Environment.GetEnvironmentVariable("BLURMASK_SITE_PREVIEW")
            ?? throw new InvalidOperationException("Set BLURMASK_SITE_PREVIEW to the output PNG.");
        session.OpenCanvas();
        var window = session.Canvas!;
        window.Width = 1100; window.Height = 760;
        await Task.Delay(250);
        var doc = session.CanvasDocument;
        var navy = Color.Parse("#253B56"); var green = Color.Parse("#719B78"); var coral = Color.Parse("#E67D64");
        Point Map(Point p) => new(p.X, p.Y * .8 + 110);
        void Stroke(Color color, double width, params Point[] points)
        {
            var ink = new InkMark(InkTool.Pen, color, width, Map(points[0]));
            foreach (var point in points.Skip(1)) ink.AddPoint(Map(point));
            doc.Add(ink);
        }
        void Shape(InkTool tool, Color color, double width, Point a, Point b)
        { var ink = new InkMark(tool, color, width, Map(a)); ink.AddPoint(Map(b)); doc.Add(ink); }
        void Text(string text, Point point, double size, Color color)
        { doc.Add(new InkMark(InkTool.Text, color, 4, Map(point)) { Text = text, FontSize = size }); }
        Text("A little idea.", new(400, 220), 100, navy);
        Text("A long way to go.", new(405, 350), 54, green);
        // Rocket nose, body, fins and round porthole.
        Stroke(navy, 10, new(1060, 990), new(1080, 800), new(1140, 635), new(1220, 535),
            new(1300, 638), new(1358, 808), new(1380, 990), new(1060, 990));
        Stroke(navy, 10, new(1072, 865), new(965, 962), new(943, 1100), new(1060, 1020));
        Stroke(navy, 10, new(1367, 865), new(1474, 962), new(1495, 1100), new(1380, 1020));
        Shape(InkTool.Ellipse, navy, 10, new(1140, 715), new(1300, 875));
        Shape(InkTool.Ellipse, green, 5, new(1158, 733), new(1282, 857));
        Stroke(coral, 12, new(1120, 1030), new(1128, 1120), new(1220, 1270), new(1305, 1130), new(1320, 1030));
        Stroke(coral, 9, new(1180, 1060), new(1220, 1180), new(1260, 1060));
        // A quiet landscape under the launch, stars above it.
        Stroke(green, 7, new(350, 1320), new(480, 1230), new(610, 1280), new(760, 1160), new(945, 1315));
        Stroke(green, 7, new(1490, 1315), new(1700, 1190), new(1840, 1270), new(1990, 1220), new(2110, 1315));
        Shape(InkTool.Ellipse, Color.Parse("#E6B74E"), 9, new(1880, 370), new(2020, 510));
        foreach (var p in new[] { new Point(920, 510), new Point(1590, 480), new Point(1760, 800), new Point(740, 880) })
        { Stroke(navy, 6, p + new Vector(-18, 0), p + new Vector(18, 0)); Stroke(navy, 6, p + new Vector(0, -18), p + new Vector(0, 18)); }
        Text("Start here", new(1570, 970), 52, coral);
        Shape(InkTool.Arrow, coral, 7, new(1660, 1070), new(1410, 1130));
        window.Surface.ResetBounds(); window.Surface.Fit();
        session.Color = coral; session.SelectTool(InkTool.Pen);
        var original = session.Toolbar.Position;
        session.Toolbar.Position = window.Position + new PixelPoint(40, 110);
        window.Activate(); DesktopInkNative.Raise(session.Toolbar);
        await Task.Delay(350);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        if (OperatingSystem.IsWindows())
        {
            using var catalog = new WindowsWindowCatalog();
            var handle = window.TryGetPlatformHandle()!.Handle;
            var bounds = catalog.Read((nuint)handle)!.Bounds;
            // GetWindowRect includes invisible resize borders: capture only the visible frame.
            if (DwmGetWindowAttribute(handle, 9, out var frame, 16) == 0)
                bounds = new PixelRect(frame.Left, frame.Top, frame.Right - frame.Left, frame.Bottom - frame.Top);
            DesktopInkNative.SaveScreenshot(path, bounds);
        }
        else
        {
            var size = new PixelSize((int)Math.Ceiling(window.Bounds.Width), (int)Math.Ceiling(window.Bounds.Height));
            using var canvas = new RenderTargetBitmap(size, new Vector(96, 96));
            canvas.Render(window);
            // The drawing tools are a separate top-level window. Rendering the
            // canvas alone omits them, even when they are visibly above it.
            var toolbar = session.Toolbar;
            var origin = window.PointToClient(toolbar.PointToScreen(default));
            using var tools = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(toolbar.Bounds.Width),
                (int)Math.Ceiling(toolbar.Bounds.Height)), new Vector(96, 96));
            tools.Render(toolbar);
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            using (var drawing = bitmap.CreateDrawingContext())
            {
                drawing.DrawImage(canvas, new Rect(window.Bounds.Size));
                drawing.DrawImage(tools, new Rect(origin, toolbar.Bounds.Size));
            }
            bitmap.Save(path, new PngBitmapEncoderOptions());
        }
        session.Toolbar.Position = original;
    }
    [StructLayout(LayoutKind.Sequential)] private struct FrameRect { public int Left, Top, Right, Bottom; }
    [LibraryImport("dwmapi.dll")] private static partial int DwmGetWindowAttribute(nint window, uint attribute, out FrameRect rect, uint size);
}
