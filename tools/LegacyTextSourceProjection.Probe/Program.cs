using System.Globalization;
using System.Text.Json;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("Supply original source projection controls and portable character category model.");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int projections = 0, refused = 0, lookups = 0, traversals = 0;
foreach (var c in doc.RootElement.GetProperty("projection").EnumerateArray())
{
    var expected = c.GetProperty("expected");
    var row = Row(c.GetProperty("row")); float? start = Nullable(c.GetProperty("start")), end = Nullable(c.GetProperty("end"));
    if (expected.ValueKind == JsonValueKind.Null)
    {
        Refuse(() => LegacyNativeTextSourceRangeProjection.Project(row, start, end)); refused++; continue;
    }
    Check(LegacyNativeTextSourceRangeProjection.Project(row, start, end), expected); projections++;
}
foreach (var c in doc.RootElement.GetProperty("maps").EnumerateArray())
{
    var rows = c.GetProperty("rows").EnumerateArray().Select(Row).ToArray();
    var offsets = c.GetProperty("prefixes").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    var map = new LegacyNativeTextSourceInputMap(rows, offsets, c.GetProperty("offset").GetInt32());
    foreach (var q in c.GetProperty("lookups").EnumerateArray())
    {
        bool found = map.TryResolve(q[0].GetInt32(), q[1].GetInt32(), out var actual);
        if (found != (q[2].ValueKind != JsonValueKind.Null) || found && actual != rows[q[2].GetInt32()])
            throw new Exception("Native flat/group-prefix source lookup mismatch.");
        lookups++;
    }
    var saved = map.TryResolve(0, 0, out var preserved); rows.AsSpan().Clear(); offsets.AsSpan().Clear();
    if (map.TryResolve(0, 0, out var after) != saved || saved && after != preserved) throw new Exception("Source map aliases caller storage.");
}
foreach (var c in doc.RootElement.GetProperty("traversal").EnumerateArray())
{
    var map = new LegacyNativeTextSourceInputMap(c.GetProperty("rows").EnumerateArray().Select(Row).ToArray(),
        c.GetProperty("prefixes").EnumerateArray().Select(x => x.GetInt32()).ToArray(), c.GetProperty("offset").GetInt32());
    var references = c.GetProperty("references").EnumerateArray().Select(Reference).ToArray();
    var actual = map.ProjectNormalReferences(references); var expected = c.GetProperty("expected");
    if (actual.Length != expected.GetArrayLength()) throw new Exception("Normal source-reference row traversal mismatch.");
    for (int i = 0; i < actual.Length; i++) Check(actual[i], expected[i]);
    traversals++;
}
using var stream = File.OpenRead(args[1]); var catalog = LegacyCharacterLineCategoryModel.Load(stream).Catalog;
var alphabet = catalog.BindAlphabet(["a", "A", "p"]);
var profile = new LegacyCharacterLineFitProfile(2, 20, -.4f, .4f, 1e-6f, float.MaxValue, 3);
for (int i = 0; i < 128; i++)
{
    float h = 4 + i * .5f, baseline = 50 + i * .25f;
    //Object order differs from decoded input ordinal. Actual arrays produce every glyph bound.
    var raw = new LegacyNativeRawStrokeGeometry([Stroke(20, baseline - h * 2, baseline),
        Stroke(40, baseline - h, baseline + h), Stroke(0, baseline - h, baseline)]);
    var selected = new LegacyNativeSelectedInkRange[] { new(2, 6, 0, -1, 0, 0, true, true),
        new(0, 6, 0, -1, 0, 0, true, true), new(1, 6, 0, -1, 0, 0, true, true) };
    var map = new LegacyNativeTextSourceInputMap(selected, [3], 0);
    var refs = Enumerable.Range(0, 3).Select(n => new LegacyNativeTextSourceReference(new(0, n, 0), new(0, n, 5))).ToArray();
    var ranges = map.ProjectNormalReferences(refs);
    if (!ranges.Select(r => r.First.SourceIndex).SequenceEqual(new uint[] { 2, 0, 1 })) throw new Exception("Input ordinal substituted for actual object identity.");
    var positions = raw.CreatePositionSnapshot(ranges);
    var line = LegacyNativeDecodedStrokeLine.Prepare(raw, positions, [new(0, 6), new(1, 12), new(2, 18)],
        [0, 1, 2], alphabet, profile, true);
    if (line.Fit is null) throw new Exception("Prepared source reference geometry failed primary line fit.");
    var metrics = line.CreateAcceptedSourceLineMetrics([0, 0, 0]);
    if (metrics.ReadSourceLineXHeight(0) != h || line.ItemBounds[0].Bottom - line.ItemBounds[0].Top != h ||
        line.ItemBounds[1].Bottom - line.ItemBounds[1].Top != h * 2 || line.ItemBounds[2].Bottom - line.ItemBounds[2].Top != h * 2)
        throw new Exception("Actual source refs->cropped points->category->IRLS composition mismatch.");
}
var safe = new LegacyNativeSelectedInkRange(0, 6, 0, -1, 0, 0, true, true);
var safeMap = new LegacyNativeTextSourceInputMap([safe], [1], 0);
var emptyMiddle = new LegacyNativeTextSourceInputMap(Enumerable.Range(0, 4).Select(n => safe with { SourceIndex = (uint)n }).ToArray(), [2, 2, 4], 0);
var acrossEmpty = emptyMiddle.ProjectNormalReferences([new(new(0, 0, 0), new(2, 1, 5))]);
if (emptyMiddle.TryResolve(1, 0, out _) || !acrossEmpty.Select(r => r.First.SourceIndex).SequenceEqual(new uint[] { 0, 1, 2, 3 }))
    throw new Exception("Empty intermediate prefix group acquired a fabricated row.");
if (!emptyMiddle.TryResolve(0, 3, out var flatTail) || flatTail.SourceIndex != 3)
    throw new Exception("Native adjusted-group-zero branch was incorrectly bounded by first prefix.");
var overflowLookup = new LegacyNativeTextSourceInputMap([safe], [1], 1);
if (!overflowLookup.TryResolve(int.MaxValue, 0, out var overflowRow) || overflowRow != safe)
    throw new Exception("Signed32 native lookup addition semantics changed.");
var sentinelRange = safeMap.ProjectNormalReferences([new(new(0, 0, 0), new(0, 0, 5), 200)])[0];
if (sentinelRange != LegacyNativeTextSourceRangeProjection.Project(safe, -1f, 5f))
    throw new Exception("Residual-to-minus-one sentinel was not passed through.");
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe with { IsKnownText = true }));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe with { SourceIndex = uint.MaxValue }));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe with { LastFraction = .25f }));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe, -.25f));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe, 1, 0));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe, 0, 6));
Refuse(() => LegacyNativeTextSourceRangeProjection.Project(safe, float.NaN));
Refuse(() => new LegacyNativeTextSourceInputMap([safe], [2], 0));
Refuse(() => new LegacyNativeTextSourceInputMap([safe], [1, 0], 0));
Refuse(() => new LegacyNativeTextSourceInputMap([safe with { FirstFraction = 1 }], [1], 0));
Refuse(() => new LegacyNativeTextSourceInputMap([safe with { SourceIndex = uint.MaxValue }], [1], 0));
Refuse(() => safeMap.ProjectNormalReferences([new(new(-1, 0, 0), new(0, 0, 1))]));
Refuse(() => safeMap.ProjectNormalReferences([new(new(0, 1, 0), new(0, 1, 1))]));
Refuse(() => safeMap.ProjectNormalReferences([new(new(0, 0, 1), new(0, 0, 2), 255)]));
Refuse(() => safeMap.ProjectNormalReferences(new LegacyNativeTextSourceReference[65537]));
var sparsePrefixes = new int[1024]; sparsePrefixes[^1] = 1;
var sparseMap = new LegacyNativeTextSourceInputMap([safe], sparsePrefixes, 0);
var repeatedSparse = Enumerable.Repeat(new LegacyNativeTextSourceReference(new(0, 0, 0), new(1023, 0, 5)), 1025).ToArray();
Refuse(() => sparseMap.ProjectNormalReferences(repeatedSparse));
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
try { safeMap.ProjectNormalReferences([], cancelled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
Console.WriteLine($"PASS {projections} float32 projections/{refused} native-order refusals, {lookups} source-map lookups, {traversals} residual/group traversals +128 actual-point category/IRLS compositions. No OCR winner or implicit source mapping claim.");

static LegacyNativeSelectedInkRange Row(JsonElement r) => new(r[0].GetUInt32(), r[1].GetInt32(), r[2].GetInt32(), r[3].GetInt32(), r[4].GetSingle(), r[5].GetSingle(), r[6].GetBoolean(), r[7].GetBoolean());
static float? Nullable(JsonElement e) => e.ValueKind == JsonValueKind.Null ? null : e.GetSingle();
static LegacyNativeTextSourceKey Key(JsonElement k) => new(k[0].GetInt32(), k[1].GetInt32(), k[2].GetInt32());
static LegacyNativeTextSourceReference Reference(JsonElement r) => new(Key(r[0]), Key(r[1]), r[2].GetByte(), r[3].GetByte());
static void Check(LegacyNativeInkPositionRange a, JsonElement e)
{
    if (a.First.Value != ulong.Parse(e[0].GetString()!, CultureInfo.InvariantCulture) || a.Last.Value != ulong.Parse(e[1].GetString()!, CultureInfo.InvariantCulture))
        throw new Exception($"Source projection bits differ: {a.First.Value}/{a.Last.Value} vs {e}.");
}
static InkStroke Stroke(float x, float top, float bottom) => new(new InkPoint[] {
    new(x, bottom), new(x, top), new(x + 10, top), new(x + 10, bottom), new(x, bottom), new(x, bottom) });
static void Refuse(Action action) { try { action(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; }
    throw new Exception("Invalid/out-of-scope source projection accepted."); }
