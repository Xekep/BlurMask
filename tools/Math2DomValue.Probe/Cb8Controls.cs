using System.Text.Json;
using InkRecognition.Math;

internal static class Cb8Controls
{
    private sealed record Range(int Key, int Count);
    private sealed class Interrupted : Exception { }
    private sealed class Services(JsonElement control, int interrupt = -1) : IMath2DomCb8Services
    {
        internal readonly List<string> Trace = [];
        internal void Note(string text)
        {
            Trace.Add(text);
            if (Trace.Count - 1 == interrupt) throw new Interrupted();
        }
        public int CompareName(object left, object right)
        { Note($"compare:{left}:{right}"); return Equals(left, right) ? 0 : 1; }
        public object? FindOldSelection(long id)
        { Note("range:" + id); return control.GetProperty("hasRange").GetBoolean() ? new Range(1, control.GetProperty("rangeCount").GetInt32()) : null; }
        public object? FindOldName(long id)
        { Note("name:" + id); return control.GetProperty("hasName").GetBoolean() ? control.GetProperty("oldName").GetString() : null; }
        public int CountRanges(object selection) { Note("count"); return ((Range)selection).Count; }
        public bool EqualRanges(object left, object right) { Note("equal"); return ((Range)left).Key == ((Range)right).Key; }
    }

    private static Math2DomValueView Value(JsonElement data)
    {
        var heap = data.GetProperty("heap");
        return new(data.GetProperty("tag").GetByte(), Convert.FromHexString(data.GetProperty("storage").GetString()!),
            heap.ValueKind == JsonValueKind.Null ? default : Convert.FromHexString(heap.GetString()!), heap.ValueKind != JsonValueKind.Null);
    }

    private static bool Invoke(JsonElement control, Math2DomCb8Pending[] pending, Services services, CancellationToken token = default)
        => Math2DomCb8EditMatcher.Matches(control.GetProperty("operation").GetByte(), control.GetProperty("envelope").GetByte(),
            control.GetProperty("name10").GetString(), control.GetProperty("name30").GetString(), "tag",
            Value(control.GetProperty("old")), Value(control.GetProperty("new")), pending, services,
            stage => services.Note(stage.ToString()), token: token);

    internal static int Run(string path)
    {
        using var controls = JsonDocument.Parse(File.ReadAllBytes(path));
        int count = 0;
        foreach (var control in controls.RootElement.GetProperty("cases").EnumerateArray())
        {
            var expected = control.GetProperty("trace").EnumerateArray().Select(x => x.GetString()!).ToArray();
            var pending = control.GetProperty("pending").EnumerateArray().Select(x => new Math2DomCb8Pending(x.GetProperty("id").GetInt64(),
                x.GetProperty("name").GetString()!, x.GetProperty("selection").ValueKind == JsonValueKind.Null ? null : new Range(x.GetProperty("selection").GetInt32(), 0))).ToArray();
            var before = pending.ToArray();
            var services = new Services(control);
            bool rejected = false, result = false;
            try { result = Invoke(control, pending, services); }
            catch (NotSupportedException) { rejected = true; }
            if (rejected != control.GetProperty("throws").GetBoolean() || (!rejected && result != control.GetProperty("result").GetBoolean())
                || !services.Trace.SequenceEqual(expected) || !pending.SequenceEqual(before))
                throw new InvalidDataException("CB8 decision/call prefix/mutation: " + control.GetProperty("label").GetString());
            count++;
            // Every observed service/conversion/parser prefix is interrupted.
            // The kernel must neither swallow the exception nor perform later work.
            for (int at = 0; at < expected.Length; at++)
            {
                var interrupted = new Services(control, at);
                try { Invoke(control, pending, interrupted); throw new InvalidDataException("CB8 interrupt swallowed"); }
                catch (Interrupted) { }
                if (!interrupted.Trace.SequenceEqual(expected.Take(at + 1)) || !pending.SequenceEqual(before))
                    throw new InvalidDataException("CB8 failure prefix/order");
                count++;
            }
            using var cancel = new CancellationTokenSource(); cancel.Cancel();
            var canceled = new Services(control);
            try { Invoke(control, pending, canceled, cancel.Token); throw new InvalidDataException("CB8 cancellation ignored"); }
            catch (OperationCanceledException) { }
            if (canceled.Trace.Count != 0) throw new InvalidDataException("CB8 cancellation performed work");
            count++;
        }
        return count;
    }
}
