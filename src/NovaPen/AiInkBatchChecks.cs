using Avalonia;
using Avalonia.Media;

namespace NovaPen;

internal static class AiInkBatchChecks
{
    internal static void Run()
    {
        var document = new InkDocument(); var batch = new AiInkBatch(document);
        var first = Stroke(10, 10); var next = Stroke(35, 10);
        var peer = Stroke(300, 200); document.Add(peer);
        document.Add(first); batch.Add(first);
        Require(batch.Snapshot().SequenceEqual(new[] { first }), "First completed stroke is not immediately available.");
        var initial = Text([first], "l"); Require(batch.Apply(initial), "First live conversion failed.");
        document.Add(next); batch.Add(next);
        Require(batch.Snapshot().SequenceEqual(new[] { first, next }), "The next pen lift lost original ink behind live text.");
        var completed = Text([first, next], "hi"); Require(batch.Apply(completed), "Live word refinement failed.");
        Require(document.Marks.SequenceEqual(new[] { peer, completed.Text }), "Refinement left duplicate text or changed peer ink.");
        Require(!batch.Apply(initial), "A partial old result deleted a completed word.");
        Require(batch.Apply(Text([first, next], "hi")), "Identical live result was rejected.");
        document.Undo();
        Require(document.Marks.Contains(initial.Text) && document.Marks.Contains(next) &&
            batch.Snapshot().SequenceEqual(new[] { first, next }), "Live refinement undo lost the previous text/ink context.");
        document.Redo();
        Require(batch.Snapshot().SequenceEqual(new[] { first, next }) && document.Marks.Contains(completed.Text), "Redo lost the original recognition context.");
        completed.Text.Text = "edited";
        Require(batch.Snapshot().Length == 0 && !batch.Apply(Text([first, next], "hi")), "Recognition overwrote manually edited text.");

        var bars = new InkDocument(); var barBatch = new AiInkBatch(bars);
        var top = Horizontal(0); var bottom = Horizontal(15);
        bars.Add(top); barBatch.Add(top); Require(barBatch.Apply(Text([top], "-")), "First math bar conversion failed.");
        bars.Add(bottom); barBatch.Add(bottom);
        Require(barBatch.Snapshot().SequenceEqual(new[] { top, bottom }), "A second equals-sign bar lost the first bar's original ink.");
        var equals = Text([top, bottom], "="); Require(barBatch.Apply(equals), "Two-stroke equals refinement failed.");
        bars.Clear();
        Require(barBatch.Snapshot().Length == 0 && !barBatch.Apply(equals), "Stale live result resurrected cleared ink.");

        var separated = new InkDocument(); var separatedBatch = new AiInkBatch(separated);
        separated.Add(first); separatedBatch.Add(first); var word = Text([first], "l"); Require(separatedBatch.Apply(word), "Separate-row preparation failed.");
        var far = Stroke(1000, 1000); separated.Add(far); separatedBatch.Add(far);
        Require(separatedBatch.Snapshot().SequenceEqual(new[] { far }) && separated.Marks.Contains(word.Text), "A distant stroke retained unrelated recognition work or removed live text.");
        var moved = Text([far], "l"); Require(separatedBatch.Apply(moved), "Moved-text preparation failed.");
        separated.Marks[separated.Marks.IndexOf(moved.Text)] = moved.Text.Translate(new Vector(20, 0));
        Require(separatedBatch.Snapshot().Length == 0, "Selection movement left stale original coordinates active.");

        var capped = new InkDocument(); var cappedBatch = new AiInkBatch(capped);
        for (int i = 0; i < 257; i++) { var stroke = Stroke(0, 0); capped.Add(stroke); cappedBatch.Add(stroke); }
        Require(cappedBatch.Snapshot().Length == 1 && capped.Marks.Count == 257, "Live context work cap discarded document ink or grew unbounded.");
        var oversized = Stroke(0, 0); oversized.Points.AddRange(new Point[20000]); capped.Add(oversized); cappedBatch.Add(oversized);
        Require(cappedBatch.Snapshot().Length == 0 && capped.Marks.Contains(oversized), "Oversized ink was retained for unbounded inference or deleted.");
        VerifyReleaseScheduling();
        VerifyConversionGate();
        Console.WriteLine("Live AI ink context: pen-release availability, refinements, multi-stroke equals, undo/redo, edited/moved/deleted text, peer ink and work caps passed.");
    }

    private static void VerifyReleaseScheduling()
    {
        var document = new InkDocument(); var first = Stroke(10, 10); var next = Stroke(35, 10);
        document.Add(first);
        using var started = new ManualResetEventSlim();
        int calls = 0; InkMark[]? refined = null;
        using var recognizer = new AiPenRecognizer(document, () => "text", (ink, mode, token) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                started.Set();
                if (!token.WaitHandle.WaitOne(TimeSpan.FromSeconds(5))) throw new InvalidOperationException("New pen press did not cancel running inference.");
                token.ThrowIfCancellationRequested();
            }
            refined = ink; return [];
        });
        recognizer.Add(first);
        var previous = recognizer.Pending;
        Require(!previous.IsCompleted, "Pen release scheduled no immediate recognition work.");
        Require(started.Wait(TimeSpan.FromSeconds(5)), "Immediate recognition did not reach its worker.");
        recognizer.Pause(); document.Add(next); recognizer.Add(next);
        previous.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        recognizer.Pending.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        Require(calls == 2 && refined is not null && refined.SequenceEqual(new[] { first, next }),
            "The next release lost canceled ink or queued obsolete recognition work.");
    }

    private static void VerifyConversionGate()
    {
        Require(AiPenRecognizer.RemainingConversionDelay(TimeSpan.FromMilliseconds(300)) == TimeSpan.FromMilliseconds(200) &&
            AiPenRecognizer.RemainingConversionDelay(TimeSpan.FromMilliseconds(800)) == TimeSpan.Zero,
            "Model computation and the quiet period were added instead of overlapped.");
        var document = new InkDocument(); var first = Stroke(10, 10); document.Add(first);
        using var reached = new ManualResetEventSlim();
        var publish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        TimeSpan? remaining = null;
        using var recognizer = new AiPenRecognizer(document, () => "text", (ink, _, _) => [Text(ink, "l")], (delay, token) =>
        { remaining = delay; reached.Set(); return publish.Task.WaitAsync(token); });
        recognizer.Add(first);
        Require(reached.Wait(TimeSpan.FromSeconds(5)) && remaining is { } wait && wait >= TimeSpan.Zero && wait <= TimeSpan.FromMilliseconds(500),
            "Immediate inference did not reach the independent conversion gate.");
        Require(document.Marks.SequenceEqual(new[] { first }), "An intermediate result replaced ink before its conversion gate.");
        recognizer.Pause(); recognizer.Pending.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        Require(document.Marks.SequenceEqual(new[] { first }), "A canceled conversion gate changed document ink.");
    }

    private static InkMark Stroke(double x, double y)
    { var mark = new InkMark(InkTool.Pen, Colors.Red, 3, new(x, y)); mark.AddPoint(new(x + 10, y + 30)); return mark; }
    private static InkMark Horizontal(double y)
    { var mark = new InkMark(InkTool.Pen, Colors.Red, 3, new(0, y)); mark.AddPoint(new(50, y)); return mark; }
    private static AiInkLayout.Result Text(InkMark[] strokes, string value)
    {
        var bounds = strokes.Select(stroke => stroke.Bounds).Aggregate((left, right) => left.Union(right)); var first = strokes[0];
        var text = new InkMark(InkTool.Text, first.Color, first.Width, bounds.TopLeft)
            { Id = first.Id, Text = value, Font = "Inter", FontSize = 24 };
        text.AddPoint(bounds.BottomRight); return new(strokes, text);
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
