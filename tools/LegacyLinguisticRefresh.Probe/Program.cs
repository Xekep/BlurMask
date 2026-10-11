using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("prepared-refresh-controls.json");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0;
foreach (var item in doc.RootElement.EnumerateArray())
{
    uint[] headers = item.GetProperty("headers").EnumerateArray().Select(x => x.GetUInt32()).ToArray();
    var actual = LegacyPreparedLinguisticRefreshPolicy.Summarize(headers, item.GetProperty("force").GetBoolean(),
        item.GetProperty("extra").GetBoolean(), item.GetProperty("previous800").GetInt32(), item.GetProperty("previousA00").GetInt32());
    Require(actual.Flags == item.GetProperty("flags").GetByte() &&
        actual.LastKind800Source == item.GetProperty("last800").GetInt32() &&
        actual.LastKindA00Source == item.GetProperty("lastA00").GetInt32() &&
        actual.ForceAlignmentAllowed == item.GetProperty("allowed").GetBoolean(), $"Prepared descriptor control {cases} differs.");
    cases++;
}

byte[] ordinaryBytes = Alphabet(["p", "\u0440", "-"]);
var ordinaryAlphabet = LegacyInkAlphabet.Load(ordinaryBytes, 0x103);
var specialAlphabet = LegacyInkAlphabet.Load(Alphabet(["p", "\U000f0009"]), 0x103);
foreach (var (sourceKind, special, alternate, expected) in new[]
    { (0x135, false, false, Array.Empty<ushort>()), (0x137, false, false, new ushort[] { 0x296 }),
      (0x135, true, false, new ushort[] { 0x296 }), (0x135, false, true, new ushort[] { 0x296, 0x28f }) })
{
    var plan = LegacyPreparedLinguisticRefreshPolicy.PlanDecorators(sourceKind, special ? specialAlphabet : ordinaryAlphabet, alternate, 0xfffb);
    Require(plan.Delimiter == 0xfffb && plan.OrderedWrapperClasses.Span.SequenceEqual(expected), "Native built-in decorator predicate/order differs.");
}

int budgetCases = 0;
foreach (byte budget in new byte[] { 0, 1, 49, 50, 51, 99, 100, 101, 149, 150, 151, 199, 200, 201, 249, 250, 251, 255 })
foreach (bool alternate in new[] { false, true })
foreach (bool active in new[] { false, true })
{
    byte expected = !alternate && !active ? (byte)0 : (byte)Math.Min(budget / 50, alternate ? 3 : 1);
    Require(LegacyPreparedLinguisticRefreshPolicy.InputMethodBudget(alternate, active, budget, 1, 3) == expected, "Native budget threshold differs.");
    budgetCases++;
}

// Verified FORCE_ALIGNMENT rejection goes through controller failure into
// ordinary fallback; accepted prepared descriptors keep the source restriction.
var binding = new LegacyPreparedOrdinaryLinguisticMaskBinding(ordinaryAlphabet, ordinaryBytes.AsSpan(0, 16),
    [new(new uint[] { 1 }, ReadOnlyMemory<ushort>.Empty)]);
var cache = new LegacyOrdinaryAcousticClassCache(new(0, 1, 1, 1, 1, 1, 65536, 1, 3), ordinaryAlphabet, ordinaryBytes.AsSpan(0, 16));
var context = new LegacyOrdinaryAcousticCacheContext(false, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
var glyph = new[] { new LegacyInkStroke([new LegacyInkPoint(0, 0), new LegacyInkPoint(10, 0)]) };
foreach (var (count, extra, prepared, bypass, outcome) in new[]
    { (2, false, true, false, 2), (1, false, true, false, 1), (3, false, true, false, 1),
      (2, true, true, false, 1), (2, false, false, false, 1), (3, true, false, true, 0) })
{
    var masks = LegacyPreparedLinguisticRefreshPolicy.BuildPreparedMasks(binding, new uint[count], true, extra, prepared, bypass, [], [], [], []);
    var final = cache.Build(new int[] { 0, 8192, 8192 }, glyph, masks.Masks.Primary.Span, context, []);
    Require((int)masks.ControllerOutcome == outcome && (final.Costs.Span[0] == 0x4000000) == (outcome == 2),
        "Prepared descriptor validation must reach final-cache restriction/fallback.");
}
//20d8f0 preserves absent-kind indices across refresh; constructor-only default-1 is explicit.
var retained = LegacyPreparedLinguisticRefreshPolicy.Summarize(new uint[] { 0xc00 }, false, false, 12, 17);
Require(retained.LastKind800Source == 12 && retained.LastKindA00Source == 17, "Missing source category must preserve explicit prior indices.");
Reject(() => LegacyPreparedLinguisticRefreshPolicy.Summarize([], false, false, -1, -1));
Reject(() => LegacyPreparedLinguisticRefreshPolicy.Summarize(new uint[258], false, false, -1, -1));
Reject(() => LegacyPreparedLinguisticRefreshPolicy.Summarize(new uint[] { 0 }, false, false, -2, -1));
Reject(() => LegacyPreparedLinguisticRefreshPolicy.Summarize(new uint[] { 0 }, false, false, -1, -1, new CancellationToken(true)));
Console.WriteLine($"PASS {cases} independent prepared source descriptor controls;4 decorator predicates;{budgetCases} budget threshold controls; retained source positions, FORCE_ALIGNMENT -> bound masks -> final cache, malformed/canceled inputs.");

static byte[] Alphabet(string[] labels)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) w.Write(ushort.MaxValue);
    w.Write((ushort)labels.Length); w.Write((ushort)1); w.Write((ushort)0);
    foreach (int n in new[] { 32, 64 }) for (int i = 0; i < n; i++) w.Write(i == n - 1 ? (ushort)0 : ushort.MaxValue);
    w.Write((ushort)0); w.Write((ushort)labels.Length); uint offset = 0;
    for (int i = 0; i < labels.Length; i++) { w.Write(offset); w.Write((uint)i); offset += (uint)Encoding.UTF8.GetByteCount(labels[i]); }
    w.Write(offset); w.Write(65535u); foreach (string label in labels) w.Write(Encoding.UTF8.GetBytes(label));
    for (int i = 0; i < labels.Length; i++) w.Write((ushort)i);
    w.Write((byte)0); w.Write((ushort)0); return stream.ToArray();
}
static void Require(bool ok, string message) { if (!ok) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled policy input accepted."); }
