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

namespace BlurMask;

internal static class DrawingSmokeTest
{
    public static void VerifyDocument(InkDocument document)
    {
        VerifySelection();
        VerifyCanvasGeometry();
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
        var originalOverlays = session.Overlays.ToArray();
        session.Toolbar.Hide();
        session.Toolbar.Show();
        Check(session.Overlays.SequenceEqual(originalOverlays), "Showing the toolbar recreated the ink overlay.");
        // Rebuilt native windows need their first layout before synthetic pointer input.
        await Task.Delay(150);
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
        Check(minimum.MinWidth == 60 && minimum.MinHeight == 40, "Mask minimum dimensions were not halved.");
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
            mask.Width = 60; mask.Height = 40;
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
            catalog.Target = catalog.Target with { Bounds = new PixelRect(500, 250, 600, 400) };
            attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(550, 300), "Mask did not follow its automatically detected window.");
            catalog.Target = catalog.Target with { Visible = false }; attachment.Update();
            Check(!mask.IsVisible, "Mask did not hide with minimized target.");
            catalog.Target = catalog.Target with { Visible = true }; attachment.Update(); await Task.Delay(100);
            Check(mask.IsVisible && mask.Position == new PixelPoint(550, 300), "Mask did not return with restored target.");
            mask.Position = new PixelPoint(650, 350); await Task.Delay(450); attachment.Update();
            catalog.Target = catalog.Target with { Bounds = new PixelRect(600, 300, 600, 400) }; attachment.Update(); await Task.Delay(100);
            Check(mask.Position == new PixelPoint(750, 400), "Manual mask placement did not update its attachment offset.");
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
                probe.Position = new PixelPoint(460, 230); await Task.Delay(150);
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
        public void Dispose() { }
    }
    private static async Task VerifyToolbarAsync(DrawingSession session)
    {
        var toolbar = session.Toolbar;
        var originalPosition = toolbar.Position;
        var area = (toolbar.Screens.ScreenFromWindow(toolbar) ?? toolbar.Screens.Primary)!.WorkingArea;
        try
        {
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
                    var anchor = toolbar.GetVisualDescendants().OfType<Button>().Single(b => AutomationProperties.GetName(b) == UiText.T(name));
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
                var invitation = sharingDesktop.Windows.Single(w => w.Title == UiText.T("Ссылка на полотно"));
                Check(invitation.GetVisualDescendants().OfType<TextBox>().Count(b => b.IsReadOnly) == 2,
                    "Collaboration did not show separate edit and view invitations.");
                invitation.Close();
            }
            var storage = Path.Combine(artifacts ?? Path.GetTempPath(), "canvas-smoke", "canvas.json");
            var state = JsonSerializer.Deserialize(File.ReadAllText(storage), CanvasJsonContext.Default.CanvasState)!;
            using var client = new CanvasCloudClient();
            var remote = await client.ReadAsync(state.Connection!);
            Check(remote.Marks.Count == 1, "Native cloud upload failed.");
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
    private static void SaveWindow(Avalonia.Controls.Window window, string path) =>
        DesktopInkNative.SaveScreenshot(path, new PixelRect(window.Position, new PixelSize(
            (int)Math.Ceiling(window.Bounds.Width * window.RenderScaling),
            (int)Math.Ceiling(window.Bounds.Height * window.RenderScaling))));
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
