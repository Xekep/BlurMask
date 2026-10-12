using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("Pass original source-line publication controls JSON.");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int scalarCount = 0, publicationCount = 0;
foreach (var c in controls.RootElement.GetProperty("scalar").EnumerateArray())
{
    var actual = LegacyNativeSourceLinePublication.ReadPreparedLineInfo(Guide(c.GetProperty("guide")), c.GetProperty("rotation").GetSingle());
    var e = c.GetProperty("expected");
    Close(actual.Angle, e[0].GetDouble()); FloatClose(actual.BaselineOffset, e[1].GetSingle());
    if (actual.Height != e[2].GetSingle()) throw new Exception("Mixed record height mismatch.");
    Close(actual.OriginX, e[3].GetDouble()); Close(actual.OriginY, e[4].GetDouble()); scalarCount++;
}
foreach (var c in controls.RootElement.GetProperty("publication").EnumerateArray())
{
    var existing = c.GetProperty("existing").EnumerateArray().Select(Line).ToArray();
    var inputs = c.GetProperty("inputs").EnumerateArray().Select(i => new LegacyNativeSourceLineInput(i[0].GetByte(), i[1].GetInt32())).ToArray();
    var guides = c.GetProperty("guides").EnumerateArray().Select(g => g.ValueKind == JsonValueKind.Null ? (LegacyNativeSourceLineGuide?)null : Guide(g)).ToArray();
    var publication = LegacyNativeSourceLinePublication.Append(existing, c.GetProperty("offset").GetInt32(), inputs, guides, c.GetProperty("rotation").GetSingle());
    var expected = c.GetProperty("expected");
    if (publication.NextWrapperLineOffset != expected[0].GetInt32() || publication.Lines.Length != expected[1].GetArrayLength())
        throw new Exception("Native descriptor count/append allocation/wrapper offset mismatch.");
    for (int i = 0; i < publication.Lines.Length; i++)
    {
        var a = publication.Lines[i]; var e = Line(expected[1][i]);
        if (a.IsPrepared != e.IsPrepared || a.OwnerTag != e.OwnerTag || a.XHeight != e.XHeight || a.UnadjustedBaseline != e.UnadjustedBaseline)
            throw new Exception("Source line ownership/height/base-baseline mismatch.");
        Close(a.OriginX, e.OriginX); Close(a.OriginY, e.OriginY); Close(a.Angle, e.Angle);
    }
    // The bridge narrows actual source doubles; no accepted character winner is inferred.
    var result = publication.CreateResultLineMetrics(publication.Lines.Length == 0 ? [] : [0]);
    for (int i = 0; i < result.LineCount; i++)
        if (result.ReadSourceLineXHeight(i) != (float)publication.Lines[i].XHeight) throw new Exception("2DA->2DB->SDK transport mismatch.");
    existing.AsSpan().Clear(); inputs.AsSpan().Clear(); guides.AsSpan().Clear();
    for (int i = 0; i < publication.Lines.Length; i++)
    {
        var e = Line(expected[1][i]); var a = publication.Lines[i];
        if (a.XHeight != e.XHeight || a.OwnerTag != e.OwnerTag || a.IsPrepared != e.IsPrepared)
            throw new Exception("Prepared publication aliases caller storage.");
    }
    publicationCount++;
}
for (int i = 0; i < 256; i++)
{
    float h = 4 + i * .5f;
    var publication = LegacyNativeSourceLinePublication.Append([], 0, [new(0, 0)], [new(0, 50, h, 999, 0)], 0);
    var result = publication.CreateResultLineMetrics([0]);
    var sample = result.CreateFontSizeSample(0, 0x430, 1, 10, 20);
    if (sample.SourceLineXHeight != h || LegacyHandwritingFontSizePolicy.MatchPrepared(16, 8, [sample]) != h * 2)
        throw new Exception("Actual prepared-guide publication->SDK->font-policy mismatch.");
}
// Deliberate angle-vs-tangent differentiator, corrected branch and threshold neighbors.
var angled = LegacyNativeSourceLinePublication.ReadPreparedLineInfo(new(.75f, 40, 12, 200, 0), .5f);
float tanOffset = (float)(angled.OriginY - angled.OriginX * Math.Tan(angled.Angle));
if (Math.Abs(angled.BaselineOffset - tanOffset) < 1) throw new Exception("Angle offset control failed to distinguish a tangent rewrite.");
//16e850 copies initial0/4 into guideC/10 BEFORE16e570 mutates0/4.
//The publisher must not silently use the deskewed acoustic-frame height.
var deskewed = LegacyLineDeskew.Apply([new InkPoint[] { new(0, 0), new(50, 60) }], new(40, 12), .75f);
if (deskewed.Line.Height == 12 || angled.Height != 12) throw new Exception("Source guide confused with the later acoustic frame.");
foreach (float adjusted in new[] { -float.Epsilon, -0f, 0f, float.Epsilon })
{
    var info = LegacyNativeSourceLinePublication.ReadPreparedLineInfo(new(.5f, 30, 12, 60, adjusted), 0);
    if (info.Height != (adjusted > 0 ? adjusted : 12) || info.OriginY != (adjusted > 0 ? 60 : 30))
        throw new Exception("CorrectedHeight>0 branch mismatch.");
}
var emptyGuide = LegacyNativeSourceLinePublication.Append([], 0, [new(0, 2)], [], 0);
if (emptyGuide.Lines.Length != 3 || emptyGuide.NextWrapperLineOffset != 3 || emptyGuide.Lines.ToArray().Any(x => x != default))
    throw new Exception("Missing native guide must leave zero-filled owned records.");
Reject(() => LegacyNativeSourceLinePublication.ReadPreparedLineInfo(new(float.NaN, 1, 1, 0, 0), 0));
Reject(() => LegacyNativeSourceLinePublication.ReadPreparedLineInfo(new(0, 1, 1, 0, 0), float.PositiveInfinity));
Reject(() => LegacyNativeSourceLinePublication.ReadPreparedLineInfo(new(4, float.MaxValue, 1, 0, 0), 1));
Reject(() => LegacyNativeSourceLinePublication.Append([], -1, [], [], 0));
Reject(() => LegacyNativeSourceLinePublication.Append([], 1, [], [], 0));
Reject(() => LegacyNativeSourceLinePublication.Append([], 0, [new(0, 4096)], [], 0));
Reject(() => LegacyNativeSourceLinePublication.Append(new LegacyNativePublishedSourceLine[4096], 0, [new(0, 0)], [], 0));
Reject(() => LegacyNativeSourceLinePublication.Append([], 0, new LegacyNativeSourceLineInput[65537], [], 0));
Reject(() => LegacyNativeSourceLinePublication.Append([], 0, [], new LegacyNativeSourceLineGuide?[4097], 0));
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
Cancelled(() => LegacyNativeSourceLinePublication.Append([], 0, [], [], 0, cancelled.Token));
Cancelled(() => emptyGuide.CreateResultLineMetrics([], cancelled.Token));
Console.WriteLine($"PASS {scalarCount} independent mixed float/double line-info controls +{publicationCount} descriptor-count/append/owner controls +256 guide->SDK->font cases; guards/cancellation. No word winner or raw OCR claim.");

static LegacyNativeSourceLineGuide Guide(JsonElement v) => new(v[0].GetSingle(), v[1].GetSingle(), v[2].GetSingle(), v[3].GetSingle(), v[4].GetSingle());
static LegacyNativePublishedSourceLine Line(JsonElement v) => new(v[0].GetBoolean(), v[1].GetDouble(), v[2].GetDouble(), v[3].GetDouble(), v[4].GetDouble(), v[5].GetInt64(), v[6].GetDouble());
static void Close(double a, double b) { if (!double.IsFinite(a) || Math.Abs(a - b) > 3e-14 * Math.Max(1, Math.Abs(b))) throw new Exception($"Double projection mismatch: {a:R}/{b:R}."); }
static void FloatClose(float a, float b) { if (!float.IsFinite(a) || Math.Abs((double)a - b) > 8e-7 * Math.Max(1, Math.Abs(b))) throw new Exception("Binary32 offset mismatch."); }
static void Reject(Action action) { try { action(); throw new Exception("Invalid prepared publication accepted."); } catch (ArgumentException) { } catch (InvalidDataException) { } }
static void Cancelled(Action action) { try { action(); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { } }
