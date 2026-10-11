using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.VisualTree;
using BlurMask.Shared;
using System.Text.Json;
using System.Diagnostics;
using Avalonia.Automation;
using Avalonia.Input.Platform;
using Avalonia.Input;

namespace BlurMask;

internal static class DrawingSmokeTest
{
    internal static async Task VerifyWaylandCanvasAsync(DrawingSession session)
    {
        Check(Program.UseNativeWayland && session.Overlays.Count == 0, "Native Wayland created an unsupported desktop overlay.");
        session.OpenCanvas(); await Task.Delay(300);
        var canvas = session.Canvas!;
        Check(canvas.TryGetPlatformHandle() is null, "Wayland test unexpectedly received an X11 window handle.");
        session.CanvasDocument.Clear();
        var pen = new InkMark(InkTool.Pen, Colors.Red, 8, new Point(80, 100));
        for (var i = 1; i <= 120; i++) pen.AddPoint(new Point(80 + i * 3, 100 + Math.Sin(i / 8.0) * 35));
        session.CanvasDocument.Add(pen);
        session.CanvasDocument.Add(new InkMark(InkTool.Text, Colors.Black, 1, new Point(80, 180)) { Text = "Wayland · English · русский", Font = "Inter", FontSize = 32 });
        var math = new InkMark(InkTool.Text, Colors.Blue, 1, new Point(80, 250)) { Text = "\\frac{1}{x^{2}}", Font = "STIX Two Math", FontSize = 32 };
        math.AddPoint(new Point(170, 340)); session.CanvasDocument.Add(math);
        canvas.Surface.SetTestView(1, new Vector(24, 24));
        await Task.Delay(400);
        var size = new PixelSize((int)canvas.Surface.Bounds.Width, (int)canvas.Surface.Bounds.Height);
        Check(size.Width >= 600 && size.Height >= 400, "Wayland canvas did not receive a usable configure size.");
        using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96)); bitmap.Render(canvas.Surface);
        var pixels = new byte[size.Width * size.Height * 4];
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
        try { bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), pixels.Length, size.Width * 4); }
        finally { handle.Free(); }
        var red = 0;
        for (var i = 0; i < pixels.Length; i += 4)
            if (pixels[i + 2] > 180 && pixels[i + 1] < 100 && pixels[i] < 100) red++;
        Check(red > 500, "Native Wayland canvas did not render its red stroke.");
        session.Undo(); Check(session.CanvasDocument.Marks.Count == 2, "Wayland canvas undo failed.");
        session.Redo(); Check(session.CanvasDocument.Marks.Count == 3, "Wayland canvas redo failed.");
        if (Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } output)
        { Directory.CreateDirectory(output); bitmap.Save(Path.Combine(output, "wayland-native-canvas.png"), PngBitmapEncoderOptions.Default); }
        Console.WriteLine($"Native Wayland canvas: configured {size}, Inter/STIX, undo/redo, red stroke pixels={red}, no desktop overlay.");
        // Saving an offscreen bitmap does not mean Wayland has mapped/presented
        // the canvas yet. Yield the dispatcher until the isolated harness has
        // verified real compositor pixels; never synchronously block rendering.
        if (Environment.GetEnvironmentVariable("BLURMASK_WAYLAND_CAPTURE_ACK") is { Length: > 0 } captureAck)
        {
            var presentationWait = Stopwatch.StartNew();
            while (!File.Exists(captureAck))
            {
                Check(presentationWait.Elapsed < TimeSpan.FromSeconds(30), "Wayland compositor did not confirm presented canvas pixels.");
                await Task.Delay(50);
            }
        }
        else await Task.Delay(1800);
    }
    public static void VerifyDocument(InkDocument document)
    {
        VerifySelection();
        VerifyCanvasGeometry();
        VerifyPreviewOwnership();
        var invite = new BoardConnection(CanvasCloudClient.DefaultServer, Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"),
            "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8");
        const string code = "ABEiM0RVZneImaq7zN3u_wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4f";
        Check(BoardInvite.Code(invite.Id, invite.Key) == code, "Native invitation differs from browser format.");
        Check(CanvasCloudClient.ParseLink(code) == invite, "Invitation code roundtrip failed.");
        Check(CanvasCloudClient.ParseLink(CanvasCloudClient.Link(invite)) == invite, "Invitation link roundtrip failed.");
        var rejectedOldInvite = false;
        try { CanvasCloudClient.ParseLink($"{invite.Server}/#board={invite.Id}&key={invite.Key}"); }
        catch (ArgumentException) { rejectedOldInvite = true; }
        Check(rejectedOldInvite, "Parameter-based invitation was accepted.");
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
        VerifyFading();
        VerifyLocalization();
        VerifySettingsStorage();
    }
    private static void VerifyCanvasGeometry()
    {
        var mark = new InkMark(InkTool.Rectangle, Colors.Blue, 4, new Point(-3500, -2200));
        mark.AddPoint(new Point(8500, 6500));
        var original = mark.Points.ToArray();
        var bounds = CanvasGeometry.ForMarks(2400, 1600, [mark]);
        Check(bounds.Left < -3500 && bounds.Top < -2200 && bounds.Right > 8500 && bounds.Bottom > 6500,
            "Board did not grow on all four edges.");
        Check(mark.Points.SequenceEqual(original), "Growing a board moved existing ink.");
        var (size, scale) = CanvasGeometry.ExportSize(bounds);
        Check(size.Width <= 4096 && size.Height <= 4096 && scale < 1, "Large export exceeds raster allocation limits.");
        var doc = new InkDocument(); doc.Add(mark);
        var selection = new InkSelection(doc); selection.Begin(mark.Points[0], 4);
        selection.Move(new Point(999999, -999999), true); selection.Complete();
        Check(BoardValidation.Valid(new BoardSnapshot { Marks = doc.Export() }), "Moving beyond the canvas coordinate limit corrupts shared ink.");
        doc.Undo(); Check(doc.Marks[0].Same(mark), "Canvas boundary move cannot be undone.");
        var pen = new InkMark(InkTool.Pen, Colors.Red, 4, default);
        _ = pen.Bounds; pen.AddPoint(new Point(500, -300));
        Check(pen.Bounds.Contains(new Point(500, -300)), "Cached pen bounds lost appended ink.");
        var translated = pen.Translate(new Vector(20, 40));
        Check(translated.Bounds.Contains(new Point(520, -260)), "Moved ink retained old cached bounds.");
        _ = mark.Bounds; mark.AddPoint(new Point(10, 20));
        Check(mark.Bounds.Right < 100, "Resized shape retained old cached bounds.");
    }
    private static void VerifyPreviewOwnership()
    {
        var source = new InkMarkDto { Points = [new(1, 2)] };
        Check(LiveProtocol.TryMerge(new() { Mark = source }, null, out var merged), "Initial preview rejected.");
        var points = merged!.Points;
        Check(LiveProtocol.TryMerge(new() { Type = "points", MarkId = source.Id, Points = [new(3, 4)], Offset = 1 }, merged, out merged), "Preview append rejected.");
        Check(source.Points.Count == 1 && ReferenceEquals(points, merged!.Points) && merged.Points.Count == 2,
            "Preview accumulator copies every stroke or mutates a received packet.");
        Check(!LiveProtocol.TryMerge(new() { Mark = source, Offset = 5 }, merged, out _), "Out-of-order preview accepted.");
        var ink = new InkMark(InkTool.Pen, Colors.Red, 4, default);
        var captured = LivePreviewCapture.Capture(ink, null);
        ink.AddPoint(new Point(100, 200));
        captured = LivePreviewCapture.Capture(ink, captured);
        Check(captured.Points.Count == 2 && captured.Points[^1].X == 100, "Incremental capture lost points.");
        var before = new BoardSnapshot { Id = Guid.NewGuid(), ReadOnly = true, Marks = [source], Revision = 4 };
        var delta = new LiveFrame { Type = "changed", BaseRevision = 4, Revision = 5,
            Patch = new BoardPatch { Update = [source with { Points = [new(30, 40)] }], Title = " Updated " } };
        Check(LiveProtocol.TryApply(before, delta, out var after) && after!.ReadOnly && after.Title == "Updated" &&
            after.Marks[0].Points[0].X == 30 && before.Marks[0].Points[0].X == 1, "Saved delta corrupts canvas or access role.");
        Check(!LiveProtocol.TryApply(before, delta with { BaseRevision = 3 }, out _), "Missing revision applied without full resync.");
        var wire = LiveWire.Encode(new() { Mark = source with { Points = [new(-12345.67890123, 99999.987654321)] }, Offset = 1, Peer = Guid.NewGuid() });
        var decoded = LiveWire.Decode(wire);
        Check(wire.Length == 58 && decoded.MarkId == source.Id && decoded.Points![0] == new InkPointDto(-12345.67890123, 99999.987654321), "Binary ink lost coordinate precision/UUID.");
        Check(LiveProtocol.TryMerge(decoded, source with { Points = new(source.Points) }, out _), "Binary points did not merge with style metadata.");
        var saved = LiveWire.Decode(LiveWire.EncodeChange(delta));
        Check(LiveProtocol.TryApply(before, saved, out after) && after!.Marks[0].Points[0] == new InkPointDto(30, 40) &&
            after.Title == "Updated", "Binary saved patch lost updates/title.");
        var truncated = LiveWire.EncodeChange(delta)[..^1];
        try { LiveWire.Decode(truncated); throw new InvalidOperationException("Truncated saved patch accepted."); }
        catch (InvalidDataException) { }
    }
    private static void VerifySelection()
    {
        var document = new InkDocument();
        var mark = new InkMark(InkTool.Rectangle, Colors.Blue, 4, new Point(-200, 10)); mark.AddPoint(new Point(-100, 90));
        document.Add(mark);
        var selection = new InkSelection(document); selection.Begin(new Point(-200, 40), 3);
        selection.Move(new Point(-150, 70)); selection.Complete();
        var moved = document.Marks[0];
        Check(moved.Id == mark.Id && moved.Points[0] == mark.Points[0] + new Vector(50, 30), "Selection did not move the object.");
        document.Undo(); Check(document.Marks[0].Same(mark), "Object move undo failed.");
        document.Redo(); Check(document.Marks[0].Same(moved), "Object move redo failed.");
        selection.Begin(new Point(-150, 70), 3); selection.Move(new Point(-120, 80)); selection.Cancel();
        Check(document.Marks[0].Same(moved), "Cancelled move changed the document.");
        var before = new BoardSnapshot { Marks = [new InkMarkDto { Id = mark.Id, Points = [new(0, 0), new(10, 10)] }] };
        var changed = before with { Marks = [before.Marks[0] with { Points = [new(20, 30), new(30, 40)] }] };
        var peer = new InkMarkDto { Points = [new(10, 50)] };
        var patch = CanvasMerge.Difference(before, changed);
        Check(patch.Update.Count == 1 && patch.Add.Count == 0 && patch.Remove.Count == 0, "Moved object was replaced with a new layer.");
        Check(CanvasMerge.Rebase(before with { Marks = [.. before.Marks, peer] }, before, changed).Marks.SequenceEqual(new[] {changed.Marks[0], peer}), "Move merge loses peer ink or layer order.");
        Check(CanvasMerge.Rebase(before with { Marks = [] }, before, changed).Marks.Count == 0, "Move resurrected a peer deletion.");
    }

    private static void VerifyFading()
    {
        var document = new InkDocument();
        var permanent = new InkMark(InkTool.Pen, Colors.Black, 4, default);
        var transient = new InkMark(InkTool.Pen, Colors.Red, 4, default) { FadeSeconds = 2 };
        document.Add(permanent);
        document.BeginEdit();
        document.Preview = transient;
        Check(transient.FadeOpacity(Stopwatch.GetTimestamp()) == 1, "Active ink fades before pointer release.");
        document.CompletePreview();
        Check(document.Marks.SequenceEqual(new[] { permanent }) && document.FadingMarks.Count == 1,
            "Fading ink entered the permanent document.");
        Check(document.Export().Count == 1, "Fading ink leaked into saved/shared marks.");
        var fadingAt = transient.FadeStartedAt + (long)(1.7 * Stopwatch.Frequency);
        Check(transient.FadeOpacity(fadingAt) is > 0 and < 1, "Temporary ink does not fade smoothly.");
        document.ExpireFading(transient.FadeStartedAt + 3 * Stopwatch.Frequency);
        Check(document.FadingMarks.Count == 0 && document.Marks.Count == 1, "Expiration removed permanent ink.");
        document.Undo(); document.Redo();
        Check(document.Marks.SequenceEqual(new[] { permanent }) && document.FadingMarks.Count == 0,
            "History resurrected expired ink.");
        document.Add(new InkMark(InkTool.Text, Colors.Red, 1, default) { FadeSeconds = 5, Text = "Transient" });
        document.Undo();
        Check(document.FadingMarks.Count == 0 && document.Marks.Count == 1, "Undo of fading ink changed permanent ink.");
        document.Add(new InkMark(InkTool.Pen, Colors.Red, 4, default) { FadeSeconds = 10 });
        document.Clear(); document.Undo();
        Check(document.Marks.Count == 1 && document.FadingMarks.Count == 0, "Clear/undo restored temporary ink.");
    }

    public static async Task VerifyRenderingAsync(DrawingSession session)
    {
        foreach (var name in InkFonts.Names)
        {
            var face = InkFonts.Face(name).GlyphTypeface;
            Check(face.FamilyName == name || face.TypographicFamilyName == name, $"Embedded font did not load: {name} -> {face.FamilyName}.");
            Check(face.CharacterToGlyphMap.TryGetGlyph('A', out _) && face.CharacterToGlyphMap.TryGetGlyph('Ж', out _), $"Font lacks Latin/Cyrillic glyphs: {name}.");
        }
        var math = new Typeface(new FontFamily("avares://BlurMask/Assets/Fonts#STIX Two Math")).GlyphTypeface;
        Check(math.CharacterToGlyphMap.TryGetGlyph('∑', out _) && math.CharacterToGlyphMap.TryGetGlyph('∫', out _), "Embedded math font did not load.");
        var variable = InkFonts.MathVariable.GlyphTypeface;
        Check(variable.FamilyName == "STIX Two Text" && variable.Style == FontStyle.Italic && variable.CharacterToGlyphMap.TryGetGlyph('x', out _), "Embedded italic math variable font did not load.");
        var formulaPreview = new StackPanel { Spacing = 16 };
        formulaPreview.Children.Add(new TextBlock { Text = "English · русский · 123", FontFamily = InkFonts.Face("Inter").FontFamily, FontSize = 28 });
        foreach (var formula in new[] { "y=3x+2", "\\frac{1}{x^{2}}", "\\left\\{\\begin{matrix}\\sqrt{3}\\\\\\sqrt{6}\\end{matrix}\\right.", @"\int\limits_{1}^{10}x^2\mathrm{dx}=333" })
        {
            var geometry = MathTypesetting.Build(formula, 32);
            Check(geometry is not null && double.IsFinite(geometry.Bounds.Width) && double.IsFinite(geometry.Bounds.Height) && geometry.Bounds.Width > 0 && geometry.Bounds.Height > 0,
                "Formula geometry is empty or nonfinite.");
            formulaPreview.Children.Add(new Avalonia.Controls.Shapes.Path { Data = geometry, Fill = Brushes.Black, Width = geometry!.Bounds.Width, Height = geometry.Bounds.Height, Stretch = Stretch.Uniform, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left });
        }
        var preview = new Border { Width = 640, Height = 560, Padding = new Thickness(24), Background = Brushes.White, Child = formulaPreview };
        preview.Measure(new Size(640, 560)); preview.Arrange(new Rect(0, 0, 640, 560));
        using (var bitmap = new RenderTargetBitmap(new PixelSize(640, 560), new Vector(96, 96)))
        {
            bitmap.Render(preview);
            if (Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } output)
            { Directory.CreateDirectory(output); bitmap.Save(Path.Combine(output, "ai-fonts-formulas.png"), PngBitmapEncoderOptions.Default); }
        }
        await VerifySettingsWindowAsync(session);
        var originalOverlays = session.Overlays.ToArray();
        session.Toolbar.Hide();
        session.Toolbar.Show();
        Check(session.Overlays.SequenceEqual(originalOverlays), "Showing the toolbar recreated the ink overlay.");
        // Rebuilt native windows need their first layout before synthetic pointer input.
        await Task.Delay(150);
        if (OperatingSystem.IsLinux())
        {
            using var catalog = XcbWindowCatalog.TryCreate()
                ?? throw new InvalidOperationException("X11 diagnostic connection unavailable.");
            foreach (var overlay in session.Overlays)
                Check(catalog.Read((nuint)overlay.TryGetPlatformHandle()!.Handle) is { Visible: false },
                    "Cursor-mode desktop overlay is still mapped on X11.");
            var layer = session.Overlays.First();
            session.SelectTool(InkTool.Pen);
            await Task.Delay(150);
            Check(catalog.Read((nuint)layer.TryGetPlatformHandle()!.Handle) is { Visible: true },
                "Desktop drawing layer did not map on X11.");
            session.SelectTool(InkTool.Cursor);
            await Task.Delay(150);
            Check(catalog.Read((nuint)layer.TryGetPlatformHandle()!.Handle) is { Visible: false },
                "Desktop layer still intercepts X11 input after selecting cursor.");
            session.SelectTool(InkTool.Pen);
            await Task.Delay(150);
            Check(catalog.Read((nuint)layer.TryGetPlatformHandle()!.Handle) is { Visible: true },
                "Desktop drawing layer could not reopen after selecting cursor.");
            session.SelectTool(InkTool.Cursor);
        }
        if (OperatingSystem.IsWindows())
        {
            var overlay = session.Overlays.First();
            var count = session.Document.Marks.Count;
            session.SelectTool(InkTool.Pen);
            await Task.Delay(50);
            DesktopInkNative.VerifyInputTarget(overlay, true);
            DesktopInkNative.TestMouseGesture(overlay, 180, 120, 260, 180);
            await Task.Delay(120);
            Check(session.Document.Marks.Count == count + 1, "Mouse input did not create a desktop stroke.");
            Check(session.Document.Marks[^1].Points.Count > 1, "Mouse motion did not extend the stroke.");
            var originalStroke = session.Document.Marks[^1];
            session.SelectTool(InkTool.Select);
            DesktopInkNative.TestMouseGesture(overlay, 180, 120, 210, 145);
            await Task.Delay(80);
            Check(session.Document.Marks[^1].Id == originalStroke.Id && !session.Document.Marks[^1].Same(originalStroke), "Desktop selection pointer gesture did not move the stroke.");
            session.Undo();
            Check(session.Document.Marks[^1].Same(originalStroke), "Desktop pointer move undo failed.");
            var resize = InkSelection.HandlePoint(originalStroke, 4, 1 / overlay.RenderScaling);
            var resizeLocal = new Point((resize.X - overlay.Position.X) / overlay.RenderScaling, (resize.Y - overlay.Position.Y) / overlay.RenderScaling);
            DesktopInkNative.TestMouseGesture(overlay, (int)resizeLocal.X, (int)resizeLocal.Y, (int)resizeLocal.X + 40, (int)resizeLocal.Y + 30);
            await Task.Delay(80);
            Check(session.Document.Marks[^1].Transform != Matrix.Identity, "Desktop pointer resize handle did not transform the stroke.");
            session.Undo(); Check(session.Document.Marks[^1].Same(originalStroke), "Desktop resize undo failed.");
            DesktopInkNative.TestKey(overlay, 0x2E); await Task.Delay(80);
            Check(session.Document.Marks.Count == count, "Desktop Delete did not remove the selected object.");
            session.Undo(); Check(session.Document.Marks[^1].Same(originalStroke), "Desktop Delete undo failed.");
            session.SelectTool(InkTool.Eraser);
            DesktopInkNative.TestMouseGesture(overlay, 180, 120, 260, 180);
            await Task.Delay(120);
            Check(session.Document.Marks.Count == count, "Eraser mouse gesture did not remove the stroke.");
            session.Undo();
            Check(session.Document.Marks.Count == count + 1, "Eraser gesture undo failed.");
            session.SelectTool(InkTool.AiPen);
            var beforeAi = session.Document.Marks.Count;
            DesktopInkNative.TestMouseGesture(overlay, 510, 350, 590, 350);
            await Task.Delay(100);
            DesktopInkNative.TestMouseGesture(overlay, 550, 310, 550, 390);
            await Task.Delay(2200);
            Check(session.Document.Marks.Count == beforeAi + 2 && session.Document.Marks.TakeLast(2).All(m => m.Tool == InkTool.Pen),
                "AI pen without a model did not retain both original strokes.");
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
            session.SetFading(2);
            session.SelectTool(InkTool.Pen);
            DesktopInkNative.TestMouseGesture(overlay, 620, 220, 720, 270);
            await Task.Delay(90);
            Check(session.Document.FadingMarks.Count == 1 && session.Document.Marks.Count == 0, "Fading pointer gesture was saved as permanent ink.");
            await Task.Delay(1550);
            Check(session.Document.FadingMarks[0].FadeOpacity(Stopwatch.GetTimestamp()) is > 0 and < 1,
                "Live temporary ink did not begin fading.");
            await Task.Delay(650);
            for (var i = 0; i < 10 && session.Document.FadingMarks.Count != 0; i++) await Task.Delay(100);
            Check(session.Document.FadingMarks.Count == 0, "Fade timer did not remove the stroke.");
            session.SetFading(0);
            session.SelectTool(InkTool.Cursor);
        }
        await VerifyToolbarAsync(session);
        var directory = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        // Render the actual toolbar, not a separate HTML/mockup implementation.
        await Task.Delay(160);
        if (OperatingSystem.IsWindows())
            SaveWindow(session.Toolbar, Path.Combine(directory, "drawing-toolbar-desktop.png"));
        using (var bitmap = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(session.Toolbar.Bounds.Width), (int)Math.Ceiling(session.Toolbar.Bounds.Height)), new Vector(96, 96)))
        {
            bitmap.Render(session.Toolbar);
            bitmap.Save(Path.Combine(directory, "drawing-toolbar.png"), PngBitmapEncoderOptions.Default);
        }
        session.Toolbar.ShowTools();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-tools.png");
        session.Toolbar.ShowShapes();
        await Task.Delay(100);
        SaveFlyout(session, directory, "drawing-shapes.png");
        session.Toolbar.ShowSizes();
        await Task.Delay(100);
        SaveFlyout(session, directory, "drawing-sizes.png");
        session.Toolbar.ShowFading();
        await Task.Delay(100);
        SaveFlyout(session, directory, "drawing-fading.png");
        session.Toolbar.ShowMasks();
        await Task.Delay(100);
        SaveFlyout(session, directory, "drawing-masks.png");
        session.Toolbar.ShowPalette();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-palette.png");
        session.Toolbar.ShowMenu();
        await Task.Delay(150);
        SaveFlyout(session, directory, "drawing-menu.png");
        session.Toolbar.CloseFlyout();
        session.Toolbar.ToggleCollapsed();
        await Task.Delay(230);
        SaveWindow(session.Toolbar, Path.Combine(directory, "drawing-collapsed.png"));
        session.Toolbar.ToggleCollapsed();
        await Task.Delay(230);
        SaveChalkIcon(directory);
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
    internal static async Task VerifyScrambleAsync()
    {
        var minimum = new BlurMaskWindow();
        Check(minimum.MinWidth == 30 && minimum.MinHeight == 20, "Mask minimum dimensions were not halved.");
        minimum.SetPrivacyCover(true);
        Check(minimum.MinWidth * minimum.RenderScaling == 1 && minimum.MinHeight * minimum.RenderScaling == 1, "Filled region retained user-resize limits above one physical pixel.");
        minimum.SetPrivacyCover(false);
        Check(minimum.MinWidth == 30 && minimum.MinHeight == 20, "Ordinary mask resize limits were not restored.");
        minimum.Close();
        if (!OperatingSystem.IsWindows() || Program.SoftwareRendering) return;
        var source = new ScramblePattern();
        var pattern = new Window { Width = 200, Height = 200, WindowDecorations = WindowDecorations.None,
            ShowInTaskbar = false, Topmost = true, Position = new PixelPoint(400, 150), Content = source };
        pattern.Show(); await Task.Delay(150);
        var origin = pattern.PointToScreen(default);
        var before = DesktopInkNative.ReadScreenPixel(origin + new PixelPoint(4, 4));
        var mask = new BlurMaskWindow { Width = 128 / pattern.RenderScaling, Height = 128 / pattern.RenderScaling,
            MinWidth = 1, MinHeight = 1, WindowStartupLocation = WindowStartupLocation.Manual, Position = origin };
        try
        {
            mask.SetMode(PrivacyMode.Scramble); mask.Show(); await Task.Delay(350);
            var artifacts = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
            if (!string.IsNullOrEmpty(artifacts)) DesktopInkNative.SaveScreenshot(Path.Combine(artifacts, "native-scramble.png"), new PixelRect(origin, new PixelSize(128, 128)));
            if (mask.HasNativeOptics)
            {
                var after = DesktopInkNative.ReadScreenPixel(origin + new PixelPoint(4, 4));
                Check(ScreenColorNear(before, 0x000000FF) && ScreenColorNear(after, 0x0000FFFF), $"Native scramble did not swap live source tiles: {before:X8} -> {after:X8}.");
                source.Flip = true; source.InvalidateVisual(); await Task.Delay(300);
                Check(ScreenColorNear(DesktopInkNative.ReadScreenPixel(origin + new PixelPoint(4, 4)), 0x000000FF), "Scramble did not refresh live desktop content.");
            }
            mask.Width = 30; mask.Height = 20;
            foreach (var mode in Enum.GetValues<PrivacyMode>()) { mask.SetMode(mode); await Task.Delay(60); }
        }
        finally { mask.Close(); pattern.Close(); }
    }
    // GPU color conversion can differ by one channel unit from GDI's COLORREF.
    private static bool ScreenColorNear(uint actual, uint expected) =>
        Math.Abs((int)(actual & 255) - (int)(expected & 255)) <= 2 &&
        Math.Abs((int)((actual >> 8) & 255) - (int)((expected >> 8) & 255)) <= 2 &&
        Math.Abs((int)((actual >> 16) & 255) - (int)((expected >> 16) & 255)) <= 2;
    private sealed class ScramblePattern : Control
    {
        internal bool Flip;
        public override void Render(DrawingContext context)
        {
            var scale = TopLevel.GetTopLevel(this)!.RenderScaling;
            for (var y = 0; y < 32; y++) for (var x = 0; x < 32; x++)
                context.DrawRectangle((((x & 1) + (y & 1) * 2) ^ (Flip ? 3 : 0)) switch { 0 => Brushes.Red, 1 => Brushes.Green, 2 => Brushes.Blue, _ => Brushes.Yellow },
                    null, new Rect(x * 8 / scale, y * 8 / scale, 8 / scale, 8 / scale));
        }
    }
    internal static async Task VerifyWindowAttachmentAsync()
    {
        var mask = new BlurMaskWindow { Width = 100, Height = 80, WindowStartupLocation = WindowStartupLocation.Manual, Position = new PixelPoint(450, 250) };
        mask.SetMode(PrivacyMode.Blackout); mask.Show();
        var catalog = new TestWindowCatalog { Target = new(1, 123, new PixelRect(400, 200, 400, 300), true) };
        using var attachment = new MaskWindowAttachment(mask, catalog);
        try
        {
            await Task.Delay(450); attachment.Update();
            catalog.Target = catalog.Target with { Bounds = new PixelRect(500, 250, 400, 300) };
            attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(550, 300), "Mask did not follow its automatically detected window.");
            catalog.Target = catalog.Target with { Bounds = new PixelRect(500, 250, 800, 600) };
            attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(500, 250) && Math.Abs(mask.Width - 800) < 1 && Math.Abs(mask.Height - 600) < 1 && mask.HasPrivacyCover && !mask.Topmost,
                "Window reflow did not latch a full opaque cover at the target stack level.");
            await Task.Delay(350); attachment.Update();
            catalog.Target = catalog.Target with { Bounds = new PixelRect(500, 250, 400, 300) };
            attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(500, 250) && Math.Abs(mask.Width - 400) < 1 && mask.HasPrivacyCover,
                "Restoring target dimensions incorrectly removed resize protection.");
            catalog.Target = catalog.Target with { Visible = false }; attachment.Update();
            Check(!mask.IsVisible, "Mask did not hide with minimized target.");
            catalog.Target = catalog.Target with { Visible = true }; attachment.Update(); await Task.Delay(100);
            Check(mask.IsVisible && mask.Position == new PixelPoint(500, 250), "Mask did not return with restored target.");
            mask.Width = 100; mask.Height = 80; mask.Position = new PixelPoint(650, 350); await Task.Delay(450); attachment.Update();
            catalog.Target = catalog.Target with { Bounds = new PixelRect(600, 300, 400, 300) }; attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(750, 400) && !mask.HasPrivacyCover, "Manual mask placement did not reconfigure its protected region.");
            if (mask.NativeOptics is { } optics)
            {
                foreach (var mode in new[] { PrivacyMode.Scramble, PrivacyMode.GlassBlocks })
                {
                    mask.SetMode(mode); await Task.Delay(40);
                    var sources = optics.SourceUpdateCount; var regions = optics.RegionBuildCount;
                    var start = catalog.Target.Bounds.Position;
                    for (var move = 0; move < 120; move++)
                    {
                        catalog.Target = catalog.Target with { Bounds = new PixelRect(start.X + move + 1, start.Y + move / 2 + 1, 400, 300) };
                        attachment.Update();
                    }
                    Check(optics.SourceUpdateCount == sources, $"Optical capture blocked a synchronous attachment movement burst: {mode}, {sources} -> {optics.SourceUpdateCount}.");
                    await Task.Delay(150);
                    Check(optics.SourceUpdateCount > sources && optics.SourceUpdateCount - sources <= 12,
                        $"Optical movement burst was lost or replayed instead of coalesced: {mode}, {sources} -> {optics.SourceUpdateCount}.");
                    Check(optics.SourceBounds.Position == mask.PointToScreen(default) && optics.RegionBuildCount == regions,
                        $"Optical source lagged behind the final mask or rebuilt regions during a pure move: {mode}, {optics.SourceBounds.Position} != {mask.PointToScreen(default)}, regions {regions} -> {optics.RegionBuildCount}.");
                    catalog.Target = catalog.Target with { Visible = false }; attachment.Update();
                    sources = optics.SourceUpdateCount; await Task.Delay(150);
                    Check(optics.SourceUpdateCount == sources, "Hidden attached mask kept capturing the desktop.");
                    catalog.Target = catalog.Target with { Visible = true }; attachment.Update(); await Task.Delay(40);
                }
            }
            var closed = false; mask.Closed += (_, _) => closed = true;
            catalog.Target = null; attachment.Update(); Check(closed, "Mask outlived its closed target.");
        }
        finally { mask.Close(); }
        using var native = NativeWindowCatalog.TryCreate();
        if (native is not null)
        {
            // Exercise foreign-window enumeration, even on systems with no foreign windows.
            var found = native.FindAt(new PixelPoint(500, 300));
            Check(found is null || found.ProcessId != Environment.ProcessId, "Auto attachment picked one of its own overlays.");
            Check(native.Read(nuint.MaxValue) is null, "Native tracker accepted an invalid/recycled window ID.");
            var probe = new Window { Title = "BlurMask tracking probe", Width = 180, Height = 120, WindowDecorations = WindowDecorations.None,
                WindowStartupLocation = WindowStartupLocation.Manual, Position = new PixelPoint(400, 200), ShowInTaskbar = false };
            try
            {
                probe.Show(); await Task.Delay(150);
                var handle = probe.TryGetPlatformHandle()!;
                var id = OperatingSystem.IsMacOS() ? MacWindowCatalog.WindowNumber(handle.Handle) : (nuint)handle.Handle;
                var first = native.Read(id);
                Check(first is { Visible: true } && first.ProcessId == Environment.ProcessId, "Native tracker cannot read a real window.");
                var windowEvents = 0;
                using var events = WindowsWindowEvents.TryCreate(first!, () => windowEvents++);
                probe.Position = new PixelPoint(460, 230); await Task.Delay(150);
                if (OperatingSystem.IsWindows()) Check(events is not null && windowEvents > 0, "Windows geometry events did not report a real move.");
                var second = native.Read(id)!;
                Check(second.Bounds.X - first!.Bounds.X == 60 && second.Bounds.Y - first.Bounds.Y == 30, "Native tracker used incorrect screen coordinates.");
                probe.Hide(); await Task.Delay(100);
                Check(native.Read(id) is not { Visible: true }, "Native tracker reports a hidden window as visible.");
                probe.Close();
                // X11 queues destruction on Avalonia's separate connection. Let the UI
                // flush it before asking the catalog connection whether the ID survives.
                for (var attempt = 0; attempt < 20 && native.Read(id) is not null; attempt++) await Task.Delay(50);
                Check(native.Read(id) is null, "Native tracker retained a destroyed window.");
            }
            finally { probe.Close(); }
        }
    }
    private sealed class TestWindowCatalog : IWindowCatalog
    {
        internal TrackedWindow? Target;
        public TrackedWindow? FindAt(PixelPoint point) => Target is { Visible: true } && Target.Bounds.Contains(point) ? Target : null;
        public TrackedWindow? Read(nuint id) => Target?.Id == id ? Target : null;
        public bool PlaceAbove(Window mask, TrackedWindow target) => true;
        public void Dispose() { }
    }
    private static async Task VerifyToolbarAsync(DrawingSession session)
    {
        var toolbar = session.Toolbar;
        var originalPosition = toolbar.Position;
        var area = (toolbar.Screens.ScreenFromWindow(toolbar) ?? toolbar.Screens.Primary)!.WorkingArea;
        try
        {
            VerifyRememberedTools(session);
            foreach (var right in new[] { false, true })
            {
                toolbar.Position = new PixelPoint(right ? area.Right - (int)Math.Ceiling(toolbar.Bounds.Width * toolbar.RenderScaling) - 8 : area.X + 8, area.Y + 16);
                foreach (var (open, name) in new (Action, string)[]
                {
                    (toolbar.ShowSizes, "Толщина пера"), (toolbar.ShowMenu, "Меню BlurMask"),
                    (toolbar.ShowShapes, "Линии и фигуры"), (toolbar.ShowFading, "Исчезающие чернила"), (toolbar.ShowMasks, "Маски приватности")
                })
                {
                    open();
                    await Task.Delay(90);
                    var flyout = toolbar.OpenFlyout ?? throw new InvalidOperationException("Toolbar menu did not open.");
                    var anchor = toolbar.GetVisualDescendants().OfType<Button>().Single(b =>
                        AutomationProperties.GetName(b) == UiText.T(name) || AutomationProperties.GetName(b)?.StartsWith(UiText.T(name) + " · ", StringComparison.Ordinal) == true);
                    var anchorTop = anchor.PointToScreen(default).Y;
                    var height = (int)Math.Ceiling(flyout.Bounds.Height * flyout.RenderScaling);
                    Check(flyout.Position.Y >= area.Y && flyout.Position.Y + height <= area.Bottom + 1, "Toolbar menu is outside the working area.");
                    if (anchorTop + height <= area.Bottom)
                        Check(Math.Abs(flyout.Position.Y - anchorTop) <= 5 * toolbar.RenderScaling + 1,
                            $"Toolbar menu {name} is detached from its button: menu={flyout.Position}, anchorY={anchorTop}, height={height}, area={area}, scale={toolbar.RenderScaling}.");
                    Check(right ? flyout.Position.X < toolbar.Position.X : flyout.Position.X > toolbar.Position.X,
                        "Toolbar menu does not switch sides at the screen edge.");
                    toolbar.CloseFlyout();
                }
            }
            var expandedHeight = toolbar.Bounds.Height;
            toolbar.ToggleCollapsed();
            await Task.Delay(70);
            Check(toolbar.Bounds.Height > 50 && toolbar.Bounds.Height < expandedHeight, "Toolbar collapse is not animated.");
            await Task.Delay(180);
            Check(toolbar.Bounds.Height is > 75 and < 95, "Collapsed toolbar must retain the logo above the eye button.");
            toolbar.ToggleCollapsed();
            await Task.Delay(240);
            Check(Math.Abs(toolbar.Bounds.Height - expandedHeight) < 2, "Toolbar does not restore its full height.");
            var visible = session.InkVisible;
            session.MinimizeToTray(); Check(!toolbar.IsVisible && !session.InkVisible, "Minimize to tray left the toolbar or ink visible.");
            session.Show(); await Task.Delay(100);
            Check(toolbar.IsVisible && session.InkVisible == visible, "Tray restore did not restore toolbar and ink.");
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var previous = desktop.Windows.OfType<BlurMaskWindow>().Count();
                foreach (var label in new[] { "Размытие", "Чёрная маска" })
                {
                    toolbar.ShowMasks();
                    var button = toolbar.OpenFlyout!.GetVisualDescendants().OfType<Button>()
                        .Single(b => AutomationProperties.GetName(b) == UiText.T(label));
                    button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                    await Task.Delay(50);
                    Check(desktop.Windows.OfType<BlurMaskWindow>().Count() == ++previous, "Mask button did not create a separate mask.");
                }
                Check(desktop.Windows.OfType<BlurMaskWindow>().Last().Mode == PrivacyMode.Blackout, "New mask has the wrong mode.");
                toolbar.ShowMasks();
                toolbar.OpenFlyout!.GetVisualDescendants().OfType<Button>()
                    .Single(b => AutomationProperties.GetName(b) == UiText.T("Удалить все маски"))
                    .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                Check(!desktop.Windows.OfType<BlurMaskWindow>().Any(), "Delete-all left privacy masks open.");
                session.NewMask();
            }
        }
        finally { toolbar.CloseFlyout(); toolbar.Position = originalPosition; }
    }
    private static void VerifyRememberedTools(DrawingSession session)
    {
        var toolbar = session.Toolbar;
        var original = session.Tool;
        Button Find(string name) => toolbar.GetVisualDescendants().OfType<Button>().Single(b =>
            AutomationProperties.GetName(b) == name || AutomationProperties.GetName(b)?.StartsWith(name + " · ", StringComparison.Ordinal) == true);
        var drawing = Find(AppSettings.Label(ShortcutAction.Pen));
        var shapes = Find(UiText.T("Линии и фигуры"));
        var cursor = Find(AppSettings.Label(ShortcutAction.Cursor));
        void Click(Button button) => button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        try
        {
            foreach (var (tool, button) in new[]
            {
                (InkTool.Pen, drawing), (InkTool.AiPen, drawing), (InkTool.Highlighter, drawing), (InkTool.Text, drawing),
                (InkTool.Line, shapes), (InkTool.Arrow, shapes), (InkTool.Rectangle, shapes), (InkTool.Ellipse, shapes),
                (InkTool.Select, cursor)
            })
            {
                session.SelectTool(tool);
                toolbar.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
                Check(session.Tool == InkTool.Cursor, "Escape did not return to the ordinary cursor.");
                Click(button);
                Check(session.Tool == tool && toolbar.OpenFlyout is null, $"First click did not restore {tool} without opening its menu.");
                Click(button);
                Check(session.Tool == tool && toolbar.OpenFlyout is null, $"Repeated click did not retain {tool}; menus open on hover.");
                toolbar.CloseFlyout();
                session.SelectTool(InkTool.Eraser);
                session.ToggleInk(); session.ToggleInk();
                Click(button);
                Check(session.Tool == tool && toolbar.OpenFlyout is null, $"Eraser/visibility toggle lost remembered {tool}.");
            }
            session.SelectTool(InkTool.Cursor);
            session.SelectTool(InkTool.Pen);
            Click(cursor);
            Check(session.Tool == InkTool.Cursor && toolbar.OpenFlyout is null, "Explicit ordinary cursor choice was not remembered.");
        }
        finally { toolbar.CloseFlyout(); session.SelectTool(original); }
    }

    internal static async Task VerifyRegionScreenshotAsync(DrawingSession session)
    {
        if (!OperatingSystem.IsWindows()) return;
        var screen = session.Toolbar.Screens.Primary!;
        var start = screen.Bounds.Position + new PixelPoint(200, 160);
        var capture = session.CaptureScreenshotAsync();
        await Task.Delay(100);
        Check(session.PickingScreenshot && !session.Toolbar.IsVisible, "Screenshot tool did not enter region selection.");
        session.BeginScreenshot(start); session.MoveScreenshot(start + new PixelPoint(120, 90));
        Check(session.ScreenshotPreview is { Width: 120, Height: 90 }, "Selection dimensions were not physical screen pixels.");
        await Task.Delay(80);
        if (Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } directory)
        {
            Directory.CreateDirectory(directory);
            DesktopInkNative.SaveScreenshot(Path.Combine(directory, "screenshot-region-selection.png"), screen.Bounds);
        }
        session.CompleteScreenshot();
        await capture.WaitAsync(TimeSpan.FromSeconds(10));
        var actions = session.Toolbar.OpenFlyout ?? throw new InvalidOperationException("Screenshot actions did not open.");
        var buttons = actions.GetVisualDescendants().OfType<Button>().ToArray();
        Check(buttons.Length == 3 && buttons.All(b => b.Content is ToolbarIcon), "Screenshot actions must be a horizontal icon strip.");
        if (Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") is { Length: > 0 } actionsDirectory)
        {
            await Task.Delay(80);
            DesktopInkNative.SaveScreenshot(Path.Combine(actionsDirectory, "screenshot-region-actions.png"), screen.Bounds);
        }
        buttons.Single(b => AutomationProperties.GetName(b) == UiText.T("Копировать снимок"))
            .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        await session.Toolbar.ScreenshotActionTask.WaitAsync(TimeSpan.FromSeconds(5));
        using var bitmap = await session.Toolbar.Clipboard!.TryGetBitmapAsync();
        Check(bitmap?.PixelSize == new PixelSize(120, 90), $"Screenshot clipboard did not contain the selected area: {bitmap?.PixelSize.ToString() ?? "empty"}.");
        var cancel = session.CaptureScreenshotAsync(); await Task.Delay(60); session.CancelScreenshot();
        await cancel.WaitAsync(TimeSpan.FromSeconds(10));
        Check(!session.PickingScreenshot && session.Toolbar.IsVisible && session.Toolbar.OpenFlyout is null, "Screenshot cancel left an input layer or actions open.");
    }

    private static void SaveChalkIcon(string directory)
    {
        foreach (var size in new[] { 16, 24, 32, 48, 64, 128, 256 })
        {
            var icon = new ToolbarIcon(ToolbarGlyph.Nib) { Width = size, Height = size };
            icon.Measure(new Size(size, size)); icon.Arrange(new Rect(0, 0, size, size));
            using var bitmap = new RenderTargetBitmap(new PixelSize(size, size), new Vector(96, 96));
            bitmap.Render(icon); bitmap.Save(Path.Combine(directory, $"chalk-icon-{size}.png"), PngBitmapEncoderOptions.Default);
        }
    }
    private static void VerifySettingsStorage()
    {
        var settings = new AppSettings();
        Check(settings.Validate() is null, "Epic Pen default shortcuts conflict.");
        Check((settings with { LinuxDisplayBackend = "wayland" }).Validate() is null && (settings with { LinuxDisplayBackend = "invalid" }).Validate() is not null, "Linux backend settings validation failed.");
        Check((settings with { AiMode = "math" }).Validate() is null && (settings with { AiMode = "unknown" }).Validate() is not null, "AI mode settings validation failed.");
        Check(settings.Shortcuts[ShortcutAction.Cursor].Key == Key.D2 && settings.Shortcuts[ShortcutAction.Pen].Key == Key.D3 &&
            settings.Shortcuts[ShortcutAction.Undo].Key == Key.D6 && settings.Shortcuts[ShortcutAction.Screenshot].VirtualKey == 44,
            "Epic Pen shortcut layout was not retained.");
        var directory = Path.Combine(Path.GetTempPath(), "BlurMask-settings-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "settings.json");
        try
        {
            Check(!settings.GiacEnabled, "Giac must be disabled by default.");
            settings = settings with { Language = "de", HotkeysEnabled = false, GiacEnabled = true };
            settings.Shortcuts[ShortcutAction.Pen] = new(Key.F12, KeyModifiers.Control | KeyModifiers.Alt);
            settings.Shortcuts[ShortcutAction.Eraser] = ShortcutBinding.None;
            SettingsStore.Save(settings, path);
            var loaded = SettingsStore.Load(path);
            Check(loaded.Language == "de" && !loaded.HotkeysEnabled && loaded.GiacEnabled && loaded.Shortcuts[ShortcutAction.Pen] == settings.Shortcuts[ShortcutAction.Pen] &&
                !loaded.Shortcuts[ShortcutAction.Eraser].Enabled, "AOT settings serialization lost customized/disabled shortcuts.");
            loaded.Shortcuts[ShortcutAction.Cursor] = loaded.Shortcuts[ShortcutAction.Pen];
            Check(loaded.Validate() is not null, "Duplicate shortcuts were accepted.");
            File.WriteAllText(path, "{broken"); Check(SettingsStore.Load(path).Validate() is null, "Damaged settings prevent app startup.");
        }
        finally { if (File.Exists(path)) File.Delete(path); if (Directory.Exists(directory)) Directory.Delete(directory); }
    }
    private static async Task VerifySettingsWindowAsync(DrawingSession session)
    {
        var original = session.Settings.CloneSettings();
        session.ShowSettings();
        var window = session.OpenSettingsWindow!;
        try
        {
            await Task.Delay(100); window.ShowKeyboardTab(); await Task.Delay(100);
            var button = window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "ShortcutPen");
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.F12, KeyModifiers = KeyModifiers.Control | KeyModifiers.Alt });
            Check(window.Draft.Shortcuts[ShortcutAction.Pen] == new ShortcutBinding(Key.F12, KeyModifiers.Control | KeyModifiers.Alt), "Settings did not capture a new shortcut.");
            button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.D2, KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift });
            Check(window.Draft.Shortcuts[ShortcutAction.Pen].Key == Key.F12, "Conflicting shortcut overwrote the current binding.");
            window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
            await Task.Delay(100);
            var artifacts = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
            if (!string.IsNullOrEmpty(artifacts) && OperatingSystem.IsWindows())
            { Directory.CreateDirectory(artifacts); SaveWindow(window, Path.Combine(artifacts, "settings-keyboard.png")); }
            Check(window.Save(), "Settings window could not apply changes.");
            Check(session.ShortcutText(ShortcutAction.Pen) == "Ctrl+Alt+F12", "Saving settings did not change active shortcuts.");
            if (OperatingSystem.IsWindows())
            {
                Check(!session.UnavailableHotkeys.Contains(ShortcutAction.Pen), "Customized Windows hotkey did not register.");
                DesktopInkNative.TestShortcutMessage(session.Toolbar, ShortcutAction.Pen); await Task.Delay(50);
            }
            else
            {
                var key = new KeyEventArgs { Key = Key.F12, KeyModifiers = KeyModifiers.Control | KeyModifiers.Alt };
                Check(session.HandleLocalShortcut(key) && key.Handled, "Focused-window shortcut did not dispatch.");
            }
            Check(session.Tool == InkTool.Pen, "Configured pen action failed.");
            var size = session.PenSize;
            session.HandleShortcut(ShortcutAction.IncreaseSize); Check(session.PenSize == size + 1, "Pen-size shortcut failed.");
            session.PenSize = size;
            var inkVisible = session.InkVisible;
            session.HandleShortcut(ShortcutAction.ToggleToolbar);
            Check(!session.Toolbar.IsVisible && session.InkVisible == inkVisible, "Hiding the toolbar changed ink visibility.");
            session.HandleShortcut(ShortcutAction.ToggleToolbar); await Task.Delay(50);
            Check(session.Toolbar.IsVisible && session.InkVisible == inkVisible, "Toolbar shortcut did not restore the toolbar.");
            var fading = session.FadeSeconds;
            session.SetFading(30); session.HandleShortcut(ShortcutAction.ToggleFading); session.HandleShortcut(ShortcutAction.ToggleFading);
            Check(session.FadeSeconds == 30, "Fading shortcut forgot the selected duration."); session.SetFading(fading);
        }
        finally { window.Close(); session.SaveSettings(original); session.SelectTool(InkTool.Cursor); }
    }
    private static void VerifyLocalization()
    {
        foreach (var (culture, expected) in new[] { ("en-US", "en"), ("fr-CA", "fr"), ("de-AT", "de"), ("ru-RU", "ru"), ("ja-JP", "en"), ("es-ES", "en"), ("", "en") })
            Check(UiText.ResolveLanguage(culture) == expected, "System language selection/fallback failed.");
        foreach (var (key, values) in UiText.Translations)
        {
            Check(values.Length is 3 or 4 && values.All(v => !string.IsNullOrWhiteSpace(v)), "Missing UI translation.");
            foreach (var language in new[] { "en", "fr", "de", "ru" })
                _ = string.Format(System.Globalization.CultureInfo.InvariantCulture, UiText.Translate(key, language), "test");
        }
        Check(UiText.Translate("Перо", "en") == "Pen" && UiText.Translate("Ластик", "fr") == "Gomme" &&
            UiText.Translate("Новая маска", "de") == "Neue Maske", "UI translation lookup failed.");
    }
    internal static async Task VerifyCanvasAsync(DrawingSession session)
    {
        var desktopMark = new InkMark(InkTool.Pen, Colors.Red, 4, new Point(100, 100));
        session.DesktopDocument.Add(desktopMark);
        session.SetBoard(BoardMode.Black);
        session.OpenCanvas();
        await Task.Delay(250);
        var window = session.Canvas!;
        var anchor = new Point(330, 220); var world = window.Surface.ToBoard(anchor);
        window.Surface.ZoomAt(1.5, anchor);
        Check(((Vector)(window.Surface.ToBoard(anchor) - world)).Length < 0.000001, "Canvas zoom moved the point under the pointer.");
        window.Surface.Fit();
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
        byte[] RenderCanvas()
        {
            using var image = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(window.Surface.Bounds.Width), (int)Math.Ceiling(window.Surface.Bounds.Height)), new Vector(96, 96));
            image.Render(window.Surface);
            using var data = new MemoryStream(); image.Save(data, PngBitmapEncoderOptions.Default); return data.ToArray();
        }
        var beforeCollapse = RenderCanvas(); var activeCanvasTool = session.Tool;
        session.Toolbar.ToggleCollapsed(); await Task.Delay(220);
        Check(window.IsVisible && session.Tool == activeCanvasTool && RenderCanvas().SequenceEqual(beforeCollapse), "Eye collapse changed separate canvas ink or its selected tool.");
        session.Toolbar.ToggleCollapsed(); await Task.Delay(220);
        session.Undo(); Check(session.CanvasDocument.Marks.Count == 0, "Canvas undo failed.");
        session.Redo(); Check(session.CanvasDocument.Marks.Count == 1, "Canvas redo failed.");
        var fixedMark = session.CanvasDocument.Marks[0]; var fixedOffset = window.Surface.Offset; var fixedZoom = window.Surface.Zoom;
        var extension = new InkMark(InkTool.Rectangle, Colors.Blue, 4, new Point(-3500, -2200)); extension.AddPoint(new Point(8500, 6500));
        session.CanvasDocument.Add(extension);
        Check(window.Surface.BoardBounds.Contains(extension.Bounds) && session.CanvasDocument.Marks[0].Same(fixedMark) &&
            window.Surface.Offset == fixedOffset && window.Surface.Zoom == fixedZoom, "Auto-growing canvas shifted existing ink or the viewport.");
        var exportDirectory = Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS");
        if (!string.IsNullOrEmpty(exportDirectory))
        {
            var path = Path.Combine(exportDirectory, "native-canvas-extended.png"); window.ExportPng(path);
            using var large = new Bitmap(path); Check(large.PixelSize.Width <= 4096 && large.PixelSize.Height <= 4096, "Extended PNG allocation exceeded the limit.");
        }
        session.Undo(); Check(session.CanvasDocument.Marks.SequenceEqual(new[] { fixedMark }), "Undo after canvas growth moved old ink.");
        window.Surface.ResetBounds(); window.Surface.Fit();
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
        using (var source = new RenderTargetBitmap(new PixelSize(180, 120), new Vector(96, 96)))
        {
            var sample = new Border { Width = 180, Height = 120, Background = Brushes.Coral, Child = new TextBlock { Text = "Screenshot", FontSize = 20 } };
            sample.Measure(new Size(180, 120)); sample.Arrange(new Rect(0, 0, 180, 120)); source.Render(sample);
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" && window.Clipboard is not null)
            { await window.Clipboard.SetBitmapAsync(source); await window.PasteImageAsync(); }
            else window.InsertImage(source);
        }
        Check(session.CanvasDocument.Marks.Count == 2 && session.Tool == InkTool.Select, "Raster paste failed or did not activate selection.");
        var raster = session.CanvasDocument.Marks[^1];
        Check(BoardValidation.Valid(window.Snapshot()), "Pasted screenshot violates shared document limits.");
        if (OperatingSystem.IsWindows())
        {
            var center = (Point)((Vector)raster.Bounds.Center * window.Surface.Zoom + window.Surface.Offset);
            var a = window.Surface.TranslatePoint(center, window)!.Value;
            DesktopInkNative.TestMouseGesture(window, (int)a.X, (int)a.Y, (int)a.X + 35, (int)a.Y + 20);
            await Task.Delay(80);
            Check(!session.CanvasDocument.Marks[^1].Same(raster), "Canvas raster pointer gesture did not move the image.");
            session.Undo(); Check(session.CanvasDocument.Marks[^1].Same(raster), "Canvas raster pointer move undo failed.");
        }
        var rasterDocument = new InkDocument(); rasterDocument.Import(session.CanvasDocument.Export());
        Check(rasterDocument.Marks[^1].Same(raster), "Raster persistence roundtrip failed.");
        session.CanvasSelection.Begin(raster.Bounds.Center, 0); session.CanvasSelection.Move(raster.Bounds.Center + new Vector(120, 60)); session.CanvasSelection.Complete();
        Check(session.CanvasDocument.Marks[^1].Points[0] == raster.Points[0] + new Vector(120, 60), "Raster move failed.");
        session.Undo(); Check(session.CanvasDocument.Marks[^1].Same(raster), "Raster move undo failed.");
        window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.A, KeyModifiers = KeyModifiers.Control });
        Check(session.CanvasSelection.SelectedIds.Count == 2, "Canvas Ctrl+A did not select all objects.");
        window.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Delete });
        Check(session.CanvasDocument.Marks.Count == 0, "Canvas Delete did not remove the whole selection.");
        session.Undo(); Check(session.CanvasDocument.Marks.Count == 2 && session.CanvasDocument.Marks[^1].Same(raster), "Canvas group Delete undo failed.");
        if (!string.IsNullOrEmpty(artifacts))
        {
            await Task.Delay(100);
            SaveWindow(window, Path.Combine(artifacts, "native-canvas-raster.png"));
            window.ExportPng(Path.Combine(artifacts, "native-canvas-raster-export.png"));
            window.ToggleChrome(); await Task.Delay(50); SaveWindow(window, Path.Combine(artifacts, "native-canvas-focus.png")); window.ToggleChrome();
        }
        session.Undo(); Check(session.CanvasDocument.Marks.Count == 1, "Raster paste undo failed.");
        if (Environment.GetEnvironmentVariable("BLURMASK_TEST_SERVER") is { Length: > 0 } server)
        {
            await window.ShareAsync(server);
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime sharingDesktop)
            {
                var invitation = sharingDesktop.Windows.Single(w => w.Title == UiText.T("Поделиться полотном"));
                Check(invitation.GetVisualDescendants().OfType<TextBox>().Count(b => b.IsReadOnly) == 2,
                    "Collaboration did not show separate edit and view invitations.");
                Check(invitation.GetVisualDescendants().OfType<TextBox>().Where(b => b.IsReadOnly).All(b => b.Text is { } code && !code.Contains("://") && CanvasCloudClient.ParseLink(code).Id != Guid.Empty),
                    "Sharing form exposes links instead of invitation codes.");
                invitation.Close();
            }
            var storage = Path.Combine(artifacts ?? Path.GetTempPath(), "canvas-smoke", "canvas.json");
            var state = JsonSerializer.Deserialize(File.ReadAllText(storage), CanvasJsonContext.Default.CanvasState)!;
            using var client = new CanvasCloudClient();
            var remote = await client.ReadAsync(state.Connection!);
            Check(remote.Marks.Count == 1, "Native cloud upload failed.");
            await VerifyLiveAsync(window, session.CanvasDocument, state.Connection!, client);
            var peer = new InkMarkDto { Points = [new(500, 300), new(900, 500)], Tool = "Arrow", Color = "#00C9ED" };
            await client.ApplyAsync(state.Connection!, new BoardPatch { Add = [peer] });
            await window.SyncAsync();
            Check(session.CanvasDocument.Marks.Count == 2, "Native canvas did not receive peer stroke.");
            var own = session.CanvasDocument.Marks.First(m => m.Id != peer.Id);
            session.CanvasSelection.Begin(own.Points[0], 2);
            session.CanvasSelection.Move(own.Points[0] + new Vector(40, 20)); session.CanvasSelection.Complete();
            await window.SyncAsync();
            var movedRemote = await client.ReadAsync(state.Connection!);
            Check(movedRemote.Marks[0].Id == own.Id && movedRemote.Marks[0].Points[0].X == own.Points[0].X + 40 &&
                movedRemote.Marks[1].Id == peer.Id, "Cloud object move changed layer order or lost peer ink.");
            session.Undo(); await window.SyncAsync();
            Check(session.CanvasDocument.Marks[0].Same(own), "Cloud move undo failed.");
            using (var screenshot = new RenderTargetBitmap(new PixelSize(64, 32), new Vector(96, 96)))
            {
                var block = new Border { Width = 64, Height = 32, Background = Brushes.Coral };
                block.Measure(new Size(64, 32)); block.Arrange(new Rect(0, 0, 64, 32)); screenshot.Render(block); window.InsertImage(screenshot);
            }
            await window.SyncAsync();
            Check((await client.ReadAsync(state.Connection!)).Marks.Last().Tool == "Image", "Native screenshot upload failed.");
            session.Undo(); await window.SyncAsync();
            session.Undo();
            Check(session.CanvasDocument.Marks.Count == 1 && session.CanvasDocument.Marks[0].Id == peer.Id,
                "Undo removed peer stroke.");
            await window.SyncAsync();
            Check((await client.ReadAsync(state.Connection!)).Marks.Count == 1, "Native erasure did not synchronize.");
            await CollaborationVisualProbe.VerifyAsync(session);
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
    private static async Task VerifyLiveAsync(CanvasWindow window, InkDocument document, BoardConnection connection, CanvasCloudClient cloud)
    {
        var received = new System.Collections.Concurrent.ConcurrentQueue<LiveFrame>();
        using var peer = new CanvasLiveClient(connection, frame => { if (frame is not null) received.Enqueue(frame); });
        async Task Wait(Func<bool> predicate, string error)
        {
            var deadline = DateTime.UtcNow.AddSeconds(8);
            while (!predicate()) { Check(DateTime.UtcNow < deadline, error); await Task.Delay(15); }
        }
        await Wait(() => window.LiveConnected && peer.Connected && received.Any(f => f.Type == "ready"), "Native live channel did not connect.");
        var count = document.Marks.Count;
        document.BeginEdit();
        var own = new InkMark(InkTool.Pen, Colors.Red, 4, new Point(30, 30));
        document.Preview = own; document.Notify();
        await Wait(() => received.Any(f => f.Mark?.Id == own.Id), "Native stroke was invisible before pointer release.");
        for (var i = 1; i <= 600; i++) own.AddPoint(new Point(30 + i, 30 + i));
        document.Notify();
        await Wait(() => received.Where(f => (f.Mark?.Id ?? f.MarkId) == own.Id).Sum(f => f.Points?.Count ?? f.Mark!.Points.Count) == 601, "Native point deltas lost stroke detail.");
        var draft = new InkMarkDto { Tool = "Ellipse", Points = [new(120, 180), new(400, 320)], Color = "#00C9ED" };
        peer.Preview(draft);
        await Wait(() => window.RemotePreviewCount == 1, "Remote preview was hidden during a local drawing gesture.");
        Check(document.HasActiveEdit && document.Marks.Count == count && !window.Snapshot().Marks.Any(m => m.Id == draft.Id), "Live preview entered saved ink/history.");
        peer.Preview(draft with { Points = [new(120, 180), new(800, 600)] });
        await Wait(() => window.RemotePreviews.Single().Points[^1] == new Point(800, 600), "Native shape preview did not resize.");
        document.Cancel();
        await Wait(() => received.Any(f => f.Type == "clear"), "Cancelled native stroke remained visible to peers.");
        peer.Preview(null, true);
        await Wait(() => window.RemotePreviewCount == 0, "Cancelled peer preview remained on canvas.");
        peer.Preview(draft);
        await Wait(() => window.RemotePreviewCount == 1, "Peer preview did not restart.");
        peer.Preview(null);
        await cloud.ApplyAsync(connection, new BoardPatch { Add = [draft] });
        await window.SyncAsync();
        await Wait(() => document.Marks.Any(m => m.Id == draft.Id) && window.RemotePreviewCount == 0, "Preview-to-committed transition duplicated/lost ink.");
        await cloud.ApplyAsync(connection, new BoardPatch { Remove = [draft.Id] });
        await window.SyncAsync();
        Check(document.Marks.Count == count, "Live integration changed existing canvas ink.");
        peer.Preview(draft with { Id = Guid.NewGuid() });
        await Wait(() => window.RemotePreviewCount == 1, "Disconnect test preview did not appear.");
        peer.Dispose();
        await Wait(() => window.RemotePreviewCount == 0, "Disconnected peer left unfinished ink behind.");
        var board = await cloud.ReadAsync(connection);
        Check(await cloud.ReadChangedAsync(connection, board.Revision) is null, "Native conditional read downloaded unchanged canvas.");
        await window.SyncAsync(); await Task.Delay(150);
        var storage = Path.Combine(Environment.GetEnvironmentVariable("BLURMASK_SMOKE_ARTIFACTS") ?? Path.GetTempPath(), "canvas-smoke", "canvas.json");
        if (File.Exists(storage))
        {
            var modified = File.GetLastWriteTimeUtc(storage);
            await Task.Delay(2800);
            Check(File.GetLastWriteTimeUtc(storage) == modified, "Idle shared canvas rewrites its disk cache.");
        }
    }
    private static void SaveFlyout(DrawingSession session, string directory, string name)
    {
        var window = session.Toolbar.OpenFlyout ?? throw new InvalidOperationException("Flyout did not open.");
        if (OperatingSystem.IsWindows())
        {
            SaveWindow(window, Path.Combine(directory, name.Replace(".png", "-desktop.png")));
            var toolbar = session.Toolbar;
            var x = Math.Min(toolbar.Position.X, window.Position.X) - 8;
            var y = Math.Min(toolbar.Position.Y, window.Position.Y) - 8;
            var right = Math.Max(toolbar.Position.X + (int)Math.Ceiling(toolbar.Bounds.Width * toolbar.RenderScaling),
                window.Position.X + (int)Math.Ceiling(window.Bounds.Width * window.RenderScaling)) + 8;
            var bottom = Math.Max(toolbar.Position.Y + (int)Math.Ceiling(toolbar.Bounds.Height * toolbar.RenderScaling),
                window.Position.Y + (int)Math.Ceiling(window.Bounds.Height * window.RenderScaling)) + 8;
            DesktopInkNative.SaveScreenshot(Path.Combine(directory, name.Replace(".png", "-context.png")), new PixelRect(x, y, right - x, bottom - y));
        }
        using var bitmap = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(window.Bounds.Width),
            (int)Math.Ceiling(window.Bounds.Height)), new Vector(96, 96));
        bitmap.Render(window);
        bitmap.Save(Path.Combine(directory, name), PngBitmapEncoderOptions.Default);
    }
    private static void SaveWindow(Avalonia.Controls.Window window, string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            using var rendered = new RenderTargetBitmap(new PixelSize((int)Math.Ceiling(window.Bounds.Width), (int)Math.Ceiling(window.Bounds.Height)), new Vector(96, 96));
            rendered.Render(window); rendered.Save(path, PngBitmapEncoderOptions.Default);
            return;
        }
        using var catalog = new WindowsWindowCatalog();
        var bounds = catalog.Read((nuint)window.TryGetPlatformHandle()!.Handle)?.Bounds ?? new PixelRect(window.Position, new PixelSize(
            (int)Math.Ceiling(window.Bounds.Width * window.RenderScaling), (int)Math.Ceiling(window.Bounds.Height * window.RenderScaling)));
        DesktopInkNative.SaveScreenshot(path, bounds);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
