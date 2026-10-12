using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("Pass independent prepared C1A control JSON.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int tested = 0, longFits = 0;
foreach (var c in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var pv = c.GetProperty("profile").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    var profile = new LegacyCharacterLineFitProfile((byte)pv[0], (byte)pv[1], pv[2], pv[3], pv[4], pv[5], pv[6]);
    var items = c.GetProperty("items").EnumerateArray().Select(ReadItem).ToArray();
    var actual = LegacyPreparedCharacterLineFit.Fit(items, profile, c.GetProperty("force").GetBoolean(), c.GetProperty("incoming").GetDouble())
        ?? throw new Exception($"Unexpected primary fit rejection: {tested}");
    var expected = c.GetProperty("expected");
    var values = expected.GetProperty("solution").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    Equal(actual.Solution.HeightScale, values[0]); Equal(actual.Solution.Slope, values[1]); Equal(actual.Solution.Intercept, values[2]);
    Equal(actual.RawProfileAffinity, expected.GetProperty("score").GetDouble());
    if (actual.Iterations != expected.GetProperty("iterations").GetInt32())
        throw new Exception($"Iteration count mismatch case {tested}: {actual.Iterations}/{expected.GetProperty("iterations")}");
    if (actual.Iterations == 251) longFits++;
    var output = actual.ToPreparedFitOutput();
    var ev = expected.GetProperty("output").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    Equal(output.BaselineY, ev[0], 2e-6); Equal(output.XHeight, ev[1], 2e-6); Equal(output.Slope, ev[2], 2e-6);
    tested++;
}

var profileFixture = new LegacyCharacterLineFitProfile(2, 20, -.4f, .4f, 1e-6f, 200, 3);
var fixture = new LegacyPreparedCharacterLineItem(false, false, false, 0, 10, 20, 100,
    [new(1, 77, 1, 1, 33, -1, 44)]);
var result = LegacyPreparedCharacterLineFit.Fit([fixture], profileFixture)!.Value;
Equal(result.ToPreparedFitOutput().BaselineY, 100); Equal(result.ToPreparedFitOutput().XHeight, 80);
if (result.Iterations != 10) throw new Exception("Native minimum ten-pass termination not preserved.");
if (LegacyPreparedCharacterLineFit.Fit([], profileFixture) is not null) throw new Exception("Empty fit must reject.");
var zero = new LegacyPreparedCharacterLineItem(false, false, false, 0, 10, 20, 100, []);
if (LegacyPreparedCharacterLineFit.Fit([zero], profileFixture) is not null) throw new Exception("Zero mass must reject.");
var raw = new byte[584]; raw[0] = 3; raw[1] = 1; raw[2] = 7;
BinaryPrimitives.WriteUInt32LittleEndian(raw.AsSpan(4), 20);
for (int i = 0; i < 20; i++)
{
    F(raw, 24+i*8, i+.25f); F(raw, 28+i*8, -i-.5f);
    BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(0xb8+i*4), i);
    F(raw, 0x108+i*8, i+1); F(raw, 0x10c+i*8, i+2);
    F(raw, 0x1a8+i*8, -i-3); F(raw, 0x1ac+i*8, -i-4);
}
F(raw, 8, 1); F(raw, 12, 2); F(raw, 16, 3); F(raw, 20, 4);
var decoded = LegacyPreparedCharacterLineItem.ReadPrepared(raw);
if (!decoded.KnownGlyph || !decoded.ClipVerticalToWidth || !decoded.DownweightMarker || decoded.CandidateCount != 20 || decoded.Bottom != 4)
    throw new Exception("Prepared item header mismatch.");
var copied = decoded.CopyPreparedSlots();
for (int i = 0; i < 20; i++)
    if (copied[i] != new LegacyCharacterLineCandidate(i+.25f, -i-.5f, i, i+1, i+2, -i-3, -i-4))
        throw new Exception("Prepared slot layout mismatch.");
copied[0] = default;
if (decoded.CopyPreparedSlots()[0].BaseWeight != .25f) throw new Exception("Prepared item aliases caller storage.");
var cfg = new byte[24]; cfg[0] = 2; cfg[1] = 20;
F(cfg, 4, -.4f); F(cfg, 8, .4f); F(cfg, 12, 1e-6f); F(cfg, 16, 200); F(cfg, 20, 3);
if (LegacyCharacterLineFitProfile.ReadPrepared(cfg) != profileFixture) throw new Exception("Prepared config layout mismatch.");
Reject(() => LegacyPreparedCharacterLineItem.ReadPrepared(new byte[248]));
Reject(() => LegacyCharacterLineFitProfile.ReadPrepared(new byte[18]));
BinaryPrimitives.WriteUInt32LittleEndian(raw.AsSpan(4), 21);
Reject(() => LegacyPreparedCharacterLineItem.ReadPrepared(raw));
Reject(() => LegacyPreparedCharacterLineFit.Fit([fixture], profileFixture with { MaximumItemCount = 2 }));
Reject(() => LegacyPreparedCharacterLineFit.Fit(new LegacyPreparedCharacterLineItem[257], profileFixture));
Reject(() => LegacyPreparedCharacterLineFit.Fit([fixture], profileFixture, incomingProfileAffinity: double.NaN));
Reject(() => new LegacyPreparedCharacterLineItem(false, false, false, 0, 1, 0, 1, [new(1, float.NaN, 0, 1, 0, -1, 0)]));
var overflow = new LegacyPreparedCharacterLineItem(false, false, false, float.MaxValue, float.MaxValue, 0, 1, [new(1, 0, 0, 1, 0, -1, 0)]);
Reject(() => LegacyPreparedCharacterLineFit.Fit([overflow], profileFixture));
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
try { LegacyPreparedCharacterLineFit.Fit([fixture], profileFixture, cancellationToken: cancelled.Token); throw new Exception("Cancellation ignored."); }
catch (OperationCanceledException) { }
Console.WriteLine($"PASS {tested} independent NumPy IRLS cases, {longFits} 251-pass fallbacks; prepared layouts, rejection, snapshot and cancellation controls.");

static LegacyPreparedCharacterLineItem ReadItem(JsonElement e)
{
    var b = e.GetProperty("bounds").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    var candidates = e.GetProperty("candidates").EnumerateArray().Select(c =>
    {
        var a = c.EnumerateArray().Select(x => x.GetSingle()).ToArray();
        return new LegacyCharacterLineCandidate(a[0], a[1], (int)a[2], a[3], a[4], a[5], a[6]);
    }).ToArray();
    return new(e.GetProperty("known").GetBoolean(), e.GetProperty("clip").GetBoolean(), e.GetProperty("marker").GetBoolean(), b[0], b[1], b[2], b[3], candidates);
}
static void Equal(double actual, double expected, double relative = 2e-8)
{
    if (!double.IsFinite(actual) || Math.Abs(actual - expected) > relative * Math.Max(1, Math.Abs(expected)))
        throw new Exception($"Numeric mismatch: actual {actual:R}, expected {expected:R}");
}
static void F(byte[] bytes, int offset, float value) => BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), BitConverter.SingleToInt32Bits(value));
static void Reject(Action action)
{
    try { action(); throw new Exception("Invalid prepared input accepted."); }
    catch (ArgumentException) { } catch (InvalidDataException) { }
}
