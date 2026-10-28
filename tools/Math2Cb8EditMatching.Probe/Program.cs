using InkRecognition.Math;

int controls = 0;
string[] prefix = ["compare:tag:tag", "convert:old", "convert:new"];
string[] parsed = [.. prefix, "parse:old", "parse:new"];
string[] lookup = [.. parsed, "range:41", "name:41", "compare:MATH_SYMBOL:MATH_SYMBOL"];
void Check(bool expected, string[] trace, int kind = 7, string? name = "tag", long? oldId = 41,
    long? newId = -2, int[]? oldRange = null, bool missingRange = false, string? oldName = "MATH_SYMBOL",
    Math2Cb8PendingEdit[]? entries = null)
{
    var service = new Services(oldId, newId, missingRange ? null : oldRange ?? [1, 2], oldName);
    entries ??= [new(2, "MATH_SYMBOL", new[] { 1, 2 })];
    var before = entries.ToArray();
    bool actual = Math2Cb8EditMatching.Matches(kind, name, "tag", "old", "new", entries, service);
    if (actual != expected || !service.Trace.SequenceEqual(trace) || !before.SequenceEqual(entries))
        throw new InvalidDataException($"032800 branch/call-order mismatch: {controls}");
    controls++;
}

// Expected service prefixes were derived from the native branch instructions,
// also checked by the independent Python reference; no output-derived labels.
Check(false, [], kind: 6);
Check(false, [], name: null);
Check(false, ["compare:range:tag"], name: "range");
Check(false, [.. prefix, "parse:old"], oldId: null);
Check(false, parsed, newId: null);
Check(false, parsed, newId: 0);
Check(false, parsed, newId: 2);
Check(false, parsed, newId: -99);
Check(false, parsed, entries: []);
Check(false, [.. parsed, "range:41"], missingRange: true);
Check(false, [.. parsed, "range:41", "name:41"], oldName: null);
Check(false, [.. parsed, "range:41", "name:41", "compare:OTHER:MATH_SYMBOL"], oldName: "OTHER");
Check(true, [.. lookup, "equal"]);
Check(false, [.. lookup, "equal"], oldRange: [2, 1]);
Check(true, [.. lookup, "count"], oldRange: [], entries: [new(2, "MATH_SYMBOL", null)]);
Check(false, [.. lookup, "count"], entries: [new(2, "MATH_SYMBOL", null)]);
Check(false, [.. lookup, "equal"], entries: [new(2, "MATH_SYMBOL", new[] { 9 }), new(2, "MATH_SYMBOL", new[] { 1, 2 })]);
Check(true, [.. lookup, "equal"], entries: [new(8, "OTHER", null), new(2, "MATH_SYMBOL", new[] { 1, 2 })]);
Check(true, [.. parsed, "range:-7", "name:-7", "compare:MATH_SYMBOL:MATH_SYMBOL", "equal"], oldId: -7);
Check(true, [.. lookup, "equal"], newId: long.MinValue, entries: [new(long.MinValue, "MATH_SYMBOL", new[] { 1, 2 })]);

using var canceled = new CancellationTokenSource();
canceled.Cancel();
var untouched = new Services(41, -2, [1, 2], "MATH_SYMBOL");
try
{
    Math2Cb8EditMatching.Matches(7, "tag", "tag", "old", "new", [], untouched, canceled.Token);
    throw new InvalidDataException("Canceled callback entered services.");
}
catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
if (untouched.Trace.Count != 0) throw new InvalidDataException("Canceled callback touched services.");
Console.WriteLine($"PASS {controls} native-derived decision/call-order cases plus cancellation; prepared kernel only, no CB8 owner/admission.");

sealed class Services(long? oldId, long? newId, int[]? oldRange, string? oldName) : IMath2Cb8EditMatchServices
{
    public List<string> Trace { get; } = [];
    public int CompareName(object? left, object? right)
    { Trace.Add($"compare:{left}:{right}"); return Equals(left, right) ? 0 : 1; }
    public object? AttributeText(object? value)
    { Trace.Add("convert:" + value); return value; }
    public bool TryParseTag(object? text, out long value)
    { Trace.Add("parse:" + text); var parsed = Equals(text, "old") ? oldId : newId; value = parsed ?? 0; return parsed.HasValue; }
    public object? InkRange(long selectionId)
    { Trace.Add("range:" + selectionId); return oldRange; }
    public object? InkName(long selectionId)
    { Trace.Add("name:" + selectionId); return oldName; }
    public int RangeCount(object sourceRange)
    { Trace.Add("count"); return ((int[])sourceRange).Length; }
    public bool RangesEqual(object left, object right)
    { Trace.Add("equal"); return ((int[])left).SequenceEqual((int[])right); }
}
