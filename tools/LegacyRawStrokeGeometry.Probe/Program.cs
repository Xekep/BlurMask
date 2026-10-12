using System.Text.Json;
using InkRecognition;

if (args.Length != 2) throw new ArgumentException("Supply independent raw geometry controls and portable category model.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var root = document.RootElement;
if (root.GetProperty("format").GetString() != "original-native-raw-stroke-geometry-controls-v1")
    throw new InvalidDataException("Unexpected raw geometry controls.");
int views = 0, owners = 0, rangeChecks = 0;
foreach (var control in root.GetProperty("views").EnumerateArray())
{
    var points = ReadPoints(control.GetProperty("source"));
    var owner = new LegacyNativeRawStrokeGeometry([new(points)]);
    var s = control.GetProperty("selection");
    var selection = new LegacyNativeSelectedInkRange(s[0].GetUInt32(), s[1].GetInt32(), s[2].GetInt32(), s[3].GetInt32(),
        s[4].GetSingle(), s[5].GetSingle(), s[6].GetBoolean(), s[7].GetBoolean(), s[8].GetBoolean());
    var actual = owner.ReadSelectedPoints(selection);
    var expected = ReadPoints(control.GetProperty("points"));
    Equal(actual.Length, expected.Length, "view cardinality");
    for (int point = 0; point < actual.Length; point++) CheckPoint(actual[point], expected[point]);
    //Both captured source storage and returned view storage are owned values.
    points[0] = new(999, 999); actual[0] = new(999, 999);
    var again = owner.ReadSelectedPoints(selection);
    for (int point = 0; point < again.Length; point++) CheckPoint(again[point], expected[point]);
    views++;
}
foreach (var control in root.GetProperty("cases").EnumerateArray())
{
    var sources = control.GetProperty("sources").EnumerateArray().Select(value => new InkStroke(ReadPoints(value))).ToArray();
    var owner = new LegacyNativeRawStrokeGeometry(sources);
    var original = ReadRanges(control.GetProperty("originals"));
    var positions = owner.CreatePositionSnapshot(original);
    foreach (var query in control.GetProperty("queries").EnumerateArray())
    {
        var ranges = positions.ReadRange(query[0].GetInt32(), query[1].GetInt32());
        CheckBounds(owner.ReadRangeBounds(ranges), query[2]); rangeChecks++;
    }
    var endpoints = control.GetProperty("endpoints").EnumerateArray()
        .Select(value => new LegacyNativeDecodedItemEndpoint(value[0].GetInt32(), value[1].GetInt32())).ToArray();
    var itemBounds = owner.ReadDecodedItemBounds(positions, 2, endpoints);
    for (int item = 0; item < 2; item++) CheckBounds(itemBounds[item], control.GetProperty("itemBounds")[item]);
    Reject(() => new LegacyNativeRawStrokeGeometry(sources).ReadDecodedItemBounds(positions, 2, endpoints));
    owners++;
}

//One connected physical polyline, genuine source ranges and prepared decoded endpoints.
//Labels are explicit prepared decoder inputs; this is not an OCR corpus or a native-winner accuracy claim.
using var stream = File.OpenRead(args[1]); var model = LegacyCharacterLineCategoryModel.Load(stream);
var alphabet = model.Catalog.BindAlphabet(["a", "A", "p", "\u0430", "\u0410", "\u0440"]);
var profile = new LegacyCharacterLineFitProfile(2, 20, -.4f, .4f, 1e-6f, float.MaxValue, 3);
for (int control = 0; control < 256; control++)
{
    float height = 4 + control * .5f, baseline = 40 + control * .25f;
    var points = new List<InkPoint>();
    for (int item = 0; item < 6; item++)
    {
        float left = item * 20, right = left + 10;
        float top = baseline - height * (item % 3 == 1 ? 2 : 1);
        float bottom = baseline + (item % 3 == 2 ? height : 0);
        points.AddRange([new(left, baseline), new(left, top), new(right, top),
            new(right, bottom), new(left, bottom), new(left, baseline)]);
    }
    var raw = new LegacyNativeRawStrokeGeometry([new(points)]);
    var fullRange = new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(0, 0, 3, points.Count),
        LegacyNativeInkPosition.Pack(0, points.Count - 1, 5, points.Count));
    var positions = raw.CreatePositionSnapshot([fullRange]);
    var endpoints = Enumerable.Range(0, 6).Select(item => new LegacyNativeDecodedItemEndpoint(item, (item + 1) * 6)).ToArray();
    var line = LegacyNativeDecodedStrokeLine.Prepare(raw, positions, endpoints, [0, 1, 2, 3, 4, 5], alphabet, profile, true);
    if (line.Fit is null) throw new Exception("Connected source produced no primary fit.");
    var metrics = line.CreateAcceptedSourceLineMetrics([0, 0, 0, 0, 0, 0]);
    var sample = metrics.CreateFontSizeSample(0, 0x430, 1, line.ItemBounds[3].Left, line.ItemBounds[3].Right);
    Equal(sample.SourceLineXHeight, height, "connected source native x-height");
    Equal(LegacyHandwritingFontSizePolicy.MatchPrepared(16, 8, [sample]), height * 2, "connected source font policy");
    Equal(line.ItemBounds[1].Bottom - line.ItemBounds[1].Top, height * 2, "capital actual point extent");
    Equal(line.ItemBounds[0].Bottom - line.ItemBounds[0].Top, height, "lowercase actual point extent");
    Equal(line.ItemBounds[2].Bottom - line.ItemBounds[2].Top, height * 2, "descender actual point extent");
    if (control == 0)
    {
        Reject(() => line.CreateAcceptedSourceLineMetrics([1]));
        Reject(() => line.CreateAcceptedSourceLineMetrics(new int[65537]));
    }
    //Capital/descender double-height does not make the line's x-height double.
}

var safeOwner = new LegacyNativeRawStrokeGeometry([new([new(0, 0), new(1, 2)])]);
var safeRange = new LegacyNativeInkPositionRange(new(3), new(0x1ff05));
var safePosition = safeOwner.CreatePositionSnapshot([safeRange]);
var safeSelection = new LegacyNativeSelectedInkRange(0, 2, 0, -1, 0, 0, true, true);
Reject(() => new LegacyNativeRawStrokeGeometry([new([])]));
Reject(() => new LegacyNativeRawStrokeGeometry([new([new(float.NaN, 0)])]));
Reject(() => safeOwner.ReadSelectedPoints(safeSelection with { SourceIndex = 1 }));
Reject(() => safeOwner.ReadSelectedPoints(safeSelection with { IsKnownText = true }));
Reject(() => safeOwner.ReadSelectedPoints(safeSelection with { PointCount = 3 }));
Reject(() => safeOwner.ReadSelectedPoints(safeSelection with { LastFraction = .5f }));
Reject(() => safeOwner.ReadSelectedPoints(safeSelection with { FirstFraction = 1 }));
Reject(() => new LegacyNativeRawStrokeGeometry([new([new(-float.MaxValue, 0), new(float.MaxValue, 0)])])
    .ReadSelectedPoints(safeSelection with { FirstFraction = .5f }));
Reject(() => safeOwner.ReadDecodedItemBounds(LegacyNativeSelectedInkProducer.CreateSnapshot([safeRange], [new(2)]), 1, [new(0, 2)]));
var emptyLine = LegacyNativeDecodedStrokeLine.Prepare(safeOwner, safePosition, [], [0], alphabet, profile);
if (emptyLine.Fit is not null || emptyLine.ItemBounds[0].HasPoints) throw new Exception("Empty source invented a glyph fit.");
Reject(() => emptyLine.CreateAcceptedSourceLineMetrics([0]));
using var canceled = new CancellationTokenSource(); canceled.Cancel();
RejectCanceled(() => new LegacyNativeRawStrokeGeometry([], canceled.Token));
RejectCanceled(() => safeOwner.ReadSelectedPoints(safeSelection, canceled.Token));
RejectCanceled(() => safeOwner.ReadRangeBounds([], canceled.Token));
RejectCanceled(() => LegacyNativeDecodedStrokeLine.Prepare(safeOwner, safePosition, [], [], alphabet, profile, cancellationToken: canceled.Token));
Console.WriteLine($"PASS {views} float32 C4A/C4B point views / {owners} actual source owners / {rangeChecks} cropped bounds / 256 connected-source category->IRLS->SDK->font cases; ownership, guards, cancellation. No OCR winner claim.");

static InkPoint[] ReadPoints(JsonElement values) => values.EnumerateArray().Select(value => new InkPoint(value[0].GetSingle(), value[1].GetSingle())).ToArray();
static LegacyNativeInkPositionRange[] ReadRanges(JsonElement values) => values.EnumerateArray()
    .Select(value => new LegacyNativeInkPositionRange(new(value[0].GetUInt64()), new(value[1].GetUInt64()))).ToArray();
static void CheckPoint(InkPoint actual, InkPoint expected)
{ Bits(actual.X, expected.X, "point X"); Bits(actual.Y, expected.Y, "point Y"); }
static void CheckBounds(LegacyNativeStrokeBounds actual, JsonElement expected)
{ Bits(actual.Left, expected[0].GetSingle(), "left"); Bits(actual.Top, expected[1].GetSingle(), "top");
  Bits(actual.Right, expected[2].GetSingle(), "right"); Bits(actual.Bottom, expected[3].GetSingle(), "bottom"); }
static void Bits(float actual, float expected, string stage) => Equal(BitConverter.SingleToInt32Bits(actual), BitConverter.SingleToInt32Bits(expected), stage);
static void Equal<T>(T actual, T expected, string stage) where T : IEquatable<T>
{ if (!actual.Equals(expected)) throw new Exception($"{stage}: expected {expected}, got {actual}"); }
static void Reject(Action callback)
{ try { callback(); } catch (ArgumentException) { return; } catch (InvalidDataException) { return; } catch (InvalidOperationException) { return; }
  throw new Exception("Invalid geometry/owner accepted."); }
static void RejectCanceled(Action callback)
{ try { callback(); } catch (OperationCanceledException) { return; } throw new Exception("Cancellation ignored."); }
