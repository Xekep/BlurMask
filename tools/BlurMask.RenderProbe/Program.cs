using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BlurMask.Shared;

namespace BlurMask;

internal static class Program
{
    private static readonly Action<InkMark, DrawingContext, double> DrawMark = BindDraw();
    private static readonly List<object> Results = [];
    private static string _output = "";
    private static int _frames;

    private static void Main(string[] args)
    {
        var options = new Dictionary<string, string>();
        for (var i = 0; i < args.Length; i += 2)
        {
            if (i + 1 >= args.Length || args[i] is not ("--board" or "--output" or "--label" or "--frames" or "--typeset-only"))
                throw new ArgumentException("Use --board <private JSON> --output <local folder> --label <revision> --frames <count> --typeset-only true.");
            options.Add(args[i], args[i + 1]);
        }
        _output = Path.GetFullPath(options.GetValueOrDefault("--output", ".artifacts/render-probe"));
        _frames = int.Parse(options.GetValueOrDefault("--frames", "80"));
        if (_frames is < 10 or > 1000) throw new ArgumentOutOfRangeException("--frames");
        Directory.CreateDirectory(_output);
        // No App, Window, native platform detection, tray, compositor or main-app entry point.
        AppBuilder.Configure<Application>().UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .SetupWithoutStarting();
        TypesetChecks.Run();
        if (options.GetValueOrDefault("--typeset-only") == "true") return;
        if (options.TryGetValue("--board", out var boardPath))
        {
            var board = JsonSerializer.Deserialize(File.ReadAllText(boardPath), BoardJsonContext.Default.BoardSnapshot)
                ?? throw new InvalidDataException("Missing board.");
            if (!BoardValidation.Valid(board)) throw new InvalidDataException("Invalid board.");
            Profile("private-board", board, savePixels: false);
        }
        var image = CreateSyntheticImage();
        Profile("synthetic-4k-image", new BoardSnapshot
        {
            Width = 3840, Height = 2160,
            Marks = [new InkMarkDto { Tool = "Image", Color = "#111111", Image = image,
                                     Points = [new(0, 0), new(3840, 2160)] }]
        }, savePixels: true);
        Profile("synthetic-thin-pen", new BoardSnapshot
        {
            Width = 3840, Height = 2160,
            Marks = [new InkMarkDto { Width = 1, Points = [new(200, 400), new(3300, 1600)] }]
        }, savePixels: true);
        Profile("synthetic-smooth-pen", new BoardSnapshot
        {
            Width = 400, Height = 160,
            Marks = [new InkMarkDto { Width = 4, Points = [new(20, 20), new(200, 120), new(380, 20)] }]
        }, savePixels: true);
        var report = new
        {
            label = options.GetValueOrDefault("--label", "unspecified"), renderer = "Avalonia12.1.2 Headless + CPU Skia RenderTargetBitmap",
            windowsCreated = 0, acceptsViewScale = typeof(InkMark).GetMethod("Draw", [typeof(DrawingContext), typeof(double)]) is not null,
            frames = _frames, profiles = Results,
            limitations = new[] { "Offscreen CPU rendering; not GPU, compositor presentation FPS or physical pointer latency.",
                                 "Managed allocations exclude native Skia bitmap buffers. Private board images are never saved.",
                                 "Render loop matches canvas scale/culling and calls production InkMark.Draw; it excludes canvas chrome and collaboration timers." }
        };
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(_output, "report.json"), json);
        Console.WriteLine(json);
    }

    private static Action<InkMark, DrawingContext, double> BindDraw()
    {
        if (typeof(InkMark).GetMethod("Draw", [typeof(DrawingContext), typeof(double)]) is { } scaled)
            return scaled.CreateDelegate<Action<InkMark, DrawingContext, double>>();
        var plain = typeof(InkMark).GetMethod("Draw", [typeof(DrawingContext)])!
            .CreateDelegate<Action<InkMark, DrawingContext>>();
        return (mark, context, _) => plain(mark, context);
    }

    private static void Profile(string scenario, BoardSnapshot board, bool savePixels)
    {
        var document = new InkDocument();
        document.Import(board.Marks, resetHistory: true);
        var retained = document.Marks.ToArray();
        var same = Measure(30, () => document.Import(board.Marks));
        var reused = document.Marks.Zip(retained).Count(p => ReferenceEquals(p.First, p.Second));
        var changed = board.Marks.Select((m, i) => i == 0 ? m with { Color = "#334455" } : m).ToList();
        var alternating = false;
        var updates = Measure(30, () => { alternating = !alternating; document.Import(alternating ? changed : board.Marks); });
        document.Import(board.Marks);
        var views = new List<object>();
        foreach (var zoom in new[] { 1.0, .25, .10 })
        {
            // A fresh document makes the first image decode/LOD request occur at this zoom.
            // Otherwise the preceding 100% view would pre-populate every smaller view's cache.
            document = new InkDocument();
            document.Import(board.Marks, resetHistory: true);
            // Images are tested at their actual scaled target dimensions (960/384px).
            var size = savePixels ? new PixelSize((int)(board.Width * zoom), (int)(board.Height * zoom)) : new PixelSize(1600, 1000);
            var bounds = document.Marks.Select(m => m.Bounds).Aggregate(default(Rect), (a, b) => a == default ? b : a.Union(b));
            var offset = savePixels ? default(Vector) : new Vector(24 - bounds.X * zoom, 24 - bounds.Y * zoom);
            var control = new ProbeSurface(document, zoom, offset);
            control.Measure(size.ToSize(1)); control.Arrange(new Rect(size.ToSize(1)));
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            var cold = Measure(1, () => bitmap.Render(control));
            for (var i = 0; i < 5; i++) bitmap.Render(control);
            var warm = Measure(_frames, () => { control.InvalidateVisual(); bitmap.Render(control); });
            if (savePixels) bitmap.Save(Path.Combine(_output, $"{scenario}-{(int)Math.Round(zoom * 100)}.png"), PngBitmapEncoderOptions.Default);
            // Changed image metadata with the same PNG stresses cache preservation and re-decode.
            alternating = false;
            var importRender = Measure(8, () =>
            {
                alternating = !alternating; document.Import(alternating ? changed : board.Marks);
                control.InvalidateVisual(); bitmap.Render(control);
            });
            views.Add(new { zoom, width = size.Width, height = size.Height, cold, warm, changedSnapshotAndRender = importRender });
            document.Import(board.Marks);
        }
        Results.Add(new { scenario, marks = board.Marks.Count, points = board.Marks.Sum(m => m.Points.Count),
                          images = board.Marks.Count(m => m.Tool == "Image"), unchangedImport = same,
                          unchangedReferencesReused = reused, changedImport = updates, views });
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }

    private static object Measure(int iterations, Action action)
    {
        using var process = Process.GetCurrentProcess();
        var beforeCpu = process.TotalProcessorTime;
        var beforeBytes = GC.GetTotalAllocatedBytes(precise: true);
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++) action();
        watch.Stop(); process.Refresh();
        return new { iterations, wallMillisecondsPerIteration = watch.Elapsed.TotalMilliseconds / iterations,
                     cpuMillisecondsPerIteration = (process.TotalProcessorTime - beforeCpu).TotalMilliseconds / iterations,
                     managedBytesPerIteration = (GC.GetTotalAllocatedBytes(precise: true) - beforeBytes) / iterations,
                     workingSetMiB = process.WorkingSet64 / 1048576.0, privateMiB = process.PrivateMemorySize64 / 1048576.0 };
    }

    private static string CreateSyntheticImage()
    {
        using var bitmap = new WriteableBitmap(new PixelSize(3840, 2160), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using (var locked = bitmap.Lock())
        {
            var row = new byte[locked.RowBytes];
            for (var y = 0; y < 2160; y++)
            {
                for (var x = 0; x < 3840; x++)
                {
                    var checker = ((x / 4 + y / 4) & 1) == 0;
                    var line = x % 128 == 0 || y % 128 == 0 || (x - y * 2) % 311 == 0;
                    row[x * 4] = line ? (byte)5 : checker ? (byte)235 : (byte)70;
                    row[x * 4 + 1] = line ? (byte)5 : checker ? (byte)220 : (byte)90;
                    row[x * 4 + 2] = line ? (byte)5 : checker ? (byte)180 : (byte)130;
                    row[x * 4 + 3] = 255;
                }
                Marshal.Copy(row, 0, locked.Address + y * locked.RowBytes, row.Length);
            }
        }
        using var stream = new MemoryStream(); bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        var image = Convert.ToBase64String(stream.ToArray());
        if (!BoardValidation.ValidImage(image)) throw new InvalidDataException("Synthetic image violates the real image contract.");
        return image;
    }

    private sealed class ProbeSurface(InkDocument document, double zoom, Vector offset) : Control
    {
        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(Brushes.White, null, new Rect(Bounds.Size));
            using var clip = context.PushClip(new Rect(Bounds.Size));
            using var transform = context.PushTransform(Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(offset));
            var visible = new Rect(new Point(-offset.X / zoom, -offset.Y / zoom), new Size(Bounds.Width / zoom, Bounds.Height / zoom));
            foreach (var mark in document.Marks) if (mark.Bounds.Intersects(visible)) DrawMark(mark, context, zoom);
        }
    }
}
