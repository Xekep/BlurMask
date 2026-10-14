using System.Text.Json;
using System.Buffers.Binary;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("controls.json required");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var root = document.RootElement;
var categories = LegacyCursiveCategoryProfile.Load(Convert.FromBase64String(root.GetProperty("profile").GetString()!), 0, out _);
void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException) { return; }
    throw new InvalidOperationException("Invalid source row accepted.");
}
int cases = 0, ranges = 0, labels = 0;
foreach (var item in root.GetProperty("cases").EnumerateArray())
{
    var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new InkStroke(s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    int[] mapping = item.GetProperty("mapping").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    LegacyFirstResultSourceRange[] ReadRanges(JsonElement values) => values.EnumerateArray().Select(r => new LegacyFirstResultSourceRange(new(r[0][0].GetInt32(),r[0][1].GetInt32(),r[0][2].GetInt32()),new(r[1][0].GetInt32(),r[1][1].GetInt32(),r[1][2].GetInt32()))).ToArray();
    var sourceRanges = ReadRanges(item.GetProperty("ranges"));
    byte[] alternatives = Convert.FromBase64String(item.GetProperty("alternatives").GetString()!);
    int selected = item.GetProperty("selected").GetInt32();
    var result = LegacyFirstResultSourceRecord.Create(alternatives, selected, sourceRanges, mapping, strokes, categories, 2);
    Require(result.CopyStorage().Span.SequenceEqual(Convert.FromBase64String(item.GetProperty("expected").GetString()!)),"Native720-byte label/score/category/geometry row differs from independent oracle.");
    Require(result.Ranges.SequenceEqual(ReadRanges(item.GetProperty("projected"))) && result.LabelCount == item.GetProperty("count").GetInt32(),"Filtered source membership or selected-label dedup differs.");
    var snapshot = result.CopyStorage().ToArray(); snapshot.AsSpan().Fill(0xcc);
    alternatives.AsSpan().Clear(); mapping.AsSpan().Fill(-1);
    Require(result.CopyStorage().Span.SequenceEqual(Convert.FromBase64String(item.GetProperty("expected").GetString()!)),"Source row exposes caller/storage mutation.");
    ranges += result.Ranges.Count; labels += result.LabelCount; cases++;
}
var stroke = new InkStroke([new InkPoint(0,0),new InkPoint(10,20)]);
byte[] one = new byte[40]; var valid = new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,1));
Reject(() => LegacyFirstResultSourceRecord.Create(one,1,[valid],[0],[stroke],categories,1));
Reject(() => LegacyFirstResultSourceRecord.Create(one,0,[valid],[0,0],[stroke],categories,1));
Reject(() => LegacyFirstResultSourceRecord.Create(one,0,[new(new(0,0,0),new(1,0,1))],[0],[stroke],categories,1));
Reject(() => LegacyFirstResultSourceRecord.Create(one,0,[valid],[0],[stroke],categories,1,new(true)));
Require(LegacyFirstResultGlyphKey.Decode(0x0080008000ffffffUL) == new LegacyFirstResultGlyphKey(-1,short.MinValue,short.MinValue),"Packed source key lost signed fields.");
for (int existing = 0; existing < 128; existing++)
{
    byte[] descriptor = Enumerable.Repeat((byte)0xcc,88).ToArray();
    BinaryPrimitives.WriteUInt16LittleEndian(descriptor, (ushort)(existing % 2 == 0 ? 0x10 : 0x16));
    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(52),-1);
    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),-1);
    byte[] published = LegacyFirstResultSourceRecord.PublishDescriptorInterval(descriptor,existing,17);
    Require(published.AsSpan(0,52).SequenceEqual(descriptor.AsSpan(0,52)) && published.AsSpan(60).SequenceEqual(descriptor.AsSpan(60)) &&
        BinaryPrimitives.ReadInt32LittleEndian(published.AsSpan(52)) == existing && BinaryPrimitives.ReadInt32LittleEndian(published.AsSpan(56)) == existing+17,
        "157c60 descriptor result interval lost source count or opaque fields.");
}
var resolved = new byte[88]; BinaryPrimitives.WriteInt32LittleEndian(resolved.AsSpan(56),1);
Reject(() => LegacyFirstResultSourceRecord.PublishDescriptorInterval(resolved,0,1));
Reject(() => LegacyFirstResultSourceRecord.PublishDescriptorInterval(new byte[88],4096,1));
for (int mask = 0; mask < 256; mask++)
{
    var strokes = Enumerable.Range(0,8).Select(i => (mask & (1<<i)) != 0
        ? new InkStroke([new InkPoint(0,0),new InkPoint(100,0)])
        : new InkStroke([new InkPoint(0,0),new InkPoint(100,10)])).ToArray();
    int[] nonflat = Enumerable.Range(0,8).Where(i => (mask & (1<<i)) == 0).ToArray();
    if (nonflat.Length == 0) nonflat = [7];
    Require(LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(strokes,0,7,0,10).SequenceEqual(nonflat),"Actual flat-stroke child submission differs from independent membership.");
    Require(LegacyFirstResultSourceRecord.SelectChildPhysicalIndices(strokes,0,7,6,10).SequenceEqual(Enumerable.Range(0,8)),"Kind6 incorrectly applied ordinary flat filter.");
}
Require(LegacyFirstResultSourceRecord.SelectChildPhysicalIndices([new InkStroke([new InkPoint(0,0),new InkPoint(30,0)]),stroke],0,1,0,10).SequenceEqual(new[]{0,1}),"Width equality should retain the first stroke.");
Require(LegacyFirstResultSourceRecord.SelectChildPhysicalIndices([new InkStroke([new InkPoint(0,0),new InkPoint(100,2)]),stroke],0,1,0,10).SequenceEqual(new[]{1}),"Float quotient .02f is below the native double .02 threshold.");
Require(LegacyFirstResultSourceRecord.SelectChildPhysicalIndices([new InkStroke([new InkPoint(0,0),new InkPoint(100,MathF.BitIncrement(2f))]),stroke],0,1,0,10).SequenceEqual(new[]{0,1}),"Float quotient above native double threshold should retain first stroke.");
Console.WriteLine($"PASS {cases} independent720-byte source rows / {ranges} projected filtered ranges / {labels} label alternatives; exact crop/gap membership, geometry, selected alternative, category channels, owned storage and guards. Prepared273 inputs; no glyph materializer or main frame substitution.");
//Small owned buffers, repeated valid spans: bounded input/range counts alone
//must not allow billions of point visits. The exact limit remains accepted.
var dense = new InkStroke(Enumerable.Range(0,4096).Select(i => new InkPoint(i,i*2)).ToArray());
var full = new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,4095));
var atWorkLimit = LegacyFirstResultSourceRecord.Create(one,0,Enumerable.Repeat(full,256).ToArray(),[0],[dense],categories,1);
Require(atWorkLimit.Ranges.Count == 256 &&
    BinaryPrimitives.ReadSingleLittleEndian(atWorkLimit.CopyStorage().Span[28..]) == 4095,
    "Exact1,048,576 point visits should remain valid.");
try
{
    LegacyFirstResultSourceRecord.Create(one,0,Enumerable.Repeat(full,257).ToArray(),[0],[dense],categories,1);
    throw new InvalidOperationException("Overlapping spans exceeded geometry work budget without rejection.");
}
catch (ArgumentException e) when (e.Message.Contains("point-visit work budget",StringComparison.Ordinal)) { }
var fragmented = new LegacyFirstResultSourceRange(new(0,0,0),new(0,1,0));
var tiny = new InkStroke([new InkPoint(1,2)]);
Require(LegacyFirstResultSourceRecord.Create(one,0,Enumerable.Repeat(fragmented,2048).ToArray(),[0,2],[tiny,tiny,tiny],categories,1).Ranges.Count == 4096,
    "Exactly4096 projected fragments should remain valid.");
try
{
    LegacyFirstResultSourceRecord.Create(one,0,Enumerable.Repeat(fragmented,2049).ToArray(),[0,2],[tiny,tiny,tiny],categories,1);
    throw new InvalidOperationException("Fragment cap failed before adding a4097th range.");
}
catch (ArgumentException e) when (e.Message.Contains("Projected source ranges",StringComparison.Ordinal)) { }
using var cancellation = new CancellationTokenSource();
var observable = new CancelOnFirstIndexedPoint(dense.Points.ToArray(),cancellation);
Reject(() => LegacyFirstResultSourceRecord.Create(one,0,[full],[0],[new InkStroke(observable)],categories,1,cancellation.Token));
Require(observable.IndexedReads == 1,"Cancellation inside bbox continued reading subsequent points.");
Console.WriteLine("PASS portable point-work budget exactlimit/overlap exhaustion, projected-fragment exactlimit/exhaustion and deterministic in-bbox cancellation controls.");

sealed class CancelOnFirstIndexedPoint(InkPoint[] points,CancellationTokenSource cancellation) : IReadOnlyList<InkPoint>
{
    public int IndexedReads {get;private set;}
    public int Count => points.Length;
    public InkPoint this[int index]
    {
        get { IndexedReads++; if (IndexedReads == 1) cancellation.Cancel(); return points[index]; }
    }
    //Validation enumerates directly; first indexed read occurs in the real
    //bounding-box loop, allowing a deterministic cancellation boundary.
    public IEnumerator<InkPoint> GetEnumerator() => ((IEnumerable<InkPoint>)points).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
