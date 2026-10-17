using InkRecognition;
using System.Text.Json;

if (args.Length != 11) throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] line30 = File.ReadAllBytes(args[8]), line50 = File.ReadAllBytes(args[9]);
InkStroke[] m = [new(new InkPoint[] { new(0, 90), new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) })];
InkStroke[][] inputs = [m, Word(0).Concat(Word(300)).ToArray(), Word(450).Concat(Word(0)).ToArray(), Rotate(m, .18f)];
using var file = File.Create(args[10]); using var w = new Utf8JsonWriter(file); int guards = 0, actualObjects = 0, actualFragments = 0;
w.WriteStartObject(); w.WriteStartArray("actual");
foreach (var raw in inputs)
{
    var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
    var result = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50, raw, 0, table, cases, publication)
        .Run(residue, r => new byte[r.RequiredBytes], new byte[20000 * 40]);
    var doc = result.SourceDocument; var owner = LegacyFreshRawInkCurrentView.Prepare(doc);
    if (!ReferenceEquals(owner.Document, doc) || owner.Curves.Count != raw.Length || owner.Views.Count != raw.Length)
        throw new InvalidDataException("Actual raw document/current-view ownership differs.");
    w.WriteStartObject(); w.WriteStartArray("objects");
    for (int i = 0; i < owner.Curves.Count; i++)
    {
        var curve = owner.Curves[i]; var view = owner.Views[i]; var bytes = curve.CopyPointStorage();
        if (!ReferenceEquals(curve.Document, doc) || !ReferenceEquals(view.Curve, curve) || curve.OriginalSourceIndex != i ||
            curve.NativeConcreteType != 0xc49 || view.CurrentViewIndex != i || curve.PointCount != raw[i].Points.Count)
            throw new InvalidDataException("Raw C49 owner and original source ID changed.");
        w.WriteStartObject(); w.WriteNumber("source", i); w.WriteNumber("type", curve.NativeConcreteType);
        w.WriteBase64String("layout", curve.CopyChannelLayout()); w.WriteBase64String("xy", bytes);
        w.WriteBase64String("viewTail", view.CopyScalarStorage()); w.WriteStartArray("points");
        foreach (var p in raw[i].Points) { w.WriteStartArray(); w.WriteNumberValue(BitConverter.SingleToUInt32Bits(p.X)); w.WriteNumberValue(BitConverter.SingleToUInt32Bits(p.Y)); w.WriteEndArray(); }
        w.WriteEndArray(); WriteDescriptor("view", view.Descriptor); w.WriteEndObject();
        var copy = curve.CopyPointStorage(); copy[0] ^= 255;
        if (!bytes.AsSpan().SequenceEqual(curve.CopyPointStorage())) throw new InvalidDataException("Raw XY storage escaped.");
        copy = curve.CopyChannelLayout(); copy[0] ^= 255;
        if (curve.CopyChannelLayout()[0] != 2) throw new InvalidDataException("Layout storage escaped.");
    }
    w.WriteEndArray();
    var ranges = new List<LegacyNativeInkPositionRange>();
    for (int i = 0; i < raw.Length; i++)
    {
        int n = raw[i].Points.Count;
        ranges.Add(new(LegacyNativeInkPosition.Pack((uint)i, 0, 3, n), LegacyNativeInkPosition.Pack((uint)i, n - 1, 5, n)));
        if (n > 2) ranges.Add(new(LegacyNativeInkPosition.Pack((uint)i, .25f, 7, n), LegacyNativeInkPosition.Pack((uint)i, n - 1.5f, 1, n)));
    }
    var exported = owner.ExportIdentityTransforms(ranges.ToArray()); actualFragments += exported.MatricesResolved;
    if (!ReferenceEquals(exported.Owner, owner) || exported.NativeNonidentityTransformCount != 0) throw new InvalidDataException("Native identity exclusion differs.");
    w.WriteStartArray("fragments"); foreach (var f in exported.CopyFragments()) WriteDescriptor(null, f); w.WriteEndArray();
    w.WriteStartArray("matches"); foreach (int index in exported.CopyMatchedViewIndices()) w.WriteNumberValue(index); w.WriteEndArray();
    w.WriteBase64String("matrices", exported.CopyResolvedMatrices()); w.WriteNumber("transforms", exported.NativeNonidentityTransformCount);
    var matrices = exported.CopyResolvedMatrices(); if (matrices.Length != 0) matrices[0] ^= 255;
    if (matrices.AsSpan().SequenceEqual(exported.CopyResolvedMatrices())) throw new InvalidDataException("Matrix copy test failed.");
    if (owner.ExportIdentityTransforms([]).MatricesResolved != 0) throw new InvalidDataException("Empty source selection emitted matrices.");
    Reject(() => owner.ExportIdentityTransforms([new(new((ulong)(raw.Length + 1) << 32 | 3), new((ulong)(raw.Length + 1) << 32 | 0xff05))]));
    Reject(() => owner.ExportIdentityTransforms(ranges.ToArray(), new(true)));
    Reject(() => LegacyFreshRawInkCurrentView.Prepare(doc, new(true)));
    w.WriteEndObject(); actualObjects += raw.Length;
}
w.WriteEndArray();
// These are explicit endpoint/coverage controls, not inferred or SDK-created views.
w.WriteStartArray("preparedCoverage"); uint state = 0x13772449;
for (int i = 0; i < 4096; i++)
{
    int count = 4 + (int)(Next() % 64); int first = (int)(Next() % (uint)(count - 2)); int last = first + 1 + (int)(Next() % (uint)(count - first - 1));
    float ff = (Next() % 4) * .25f, lf = last == count - 1 ? 0 : (Next() % 4) * .25f;
    var view = new LegacyNativeSelectedInkRange((uint)(i % 3), count, first, last, ff, lf, (Next() & 1) != 0, (Next() & 1) != 0);
    int a = (int)(Next() % (uint)(count - 1)); int b = a + 1 + (int)(Next() % (uint)(count - a - 1));
    var request = new LegacyNativeSelectedInkRange((uint)(i % (i % 7 == 0 ? 4 : 3)), count, a, b, (Next() % 4) * .25f,
        b == count - 1 ? 0 : (Next() % 4) * .25f, (Next() & 1) != 0, (Next() & 1) != 0);
    if (i % 16 == 0) request = view;
    if (i % 16 == 1) request = view with { FirstFraction = view.FirstFraction + .0001f }; // native rounded equality
    if (i % 16 == 2) request = request with { LastPoint = -1, LastFraction = 0 };
    if (i % 16 == 3)
    {
        view = new((uint)(i % 3), count, 1, count - 1, .005f, 0, true, true);
        request = view with { FirstFraction = BitConverter.UInt32BitsToSingle(0x3b23d70a) };
    }
    bool matched = LegacyRawInkCurrentViewMatching.Covers(view, request);
    w.WriteStartObject(); WriteDescriptor("view", view); WriteDescriptor("request", request); w.WriteBoolean("matched", matched); w.WriteEndObject();
}
w.WriteEndArray();
var full = new LegacyNativeSelectedInkRange(0, 10, 0, 9, 0, 0, true, true);
if (LegacyRawInkCurrentViewMatching.Covers(full with { FirstPoint = 4 }, full) ||
    LegacyRawInkCurrentViewMatching.Covers(full, full with { SourceIndex = 1 }) ||
    LegacyRawInkCurrentViewMatching.Covers(full, full with { PointCount = 11 })) throw new InvalidDataException("Clipped or nonmatching view was accepted.");
Reject(() => LegacyRawInkCurrentViewMatching.Covers(full, full with { FirstFraction = float.NaN }));
Reject(() => LegacyRawInkCurrentViewMatching.Covers(full, full with { LastFraction = .1f }));
Reject(() => LegacyRawInkCurrentViewMatching.RequireCover([full with { FirstPoint = 4 }], full));
Reject(() => LegacyRawInkCurrentViewMatching.RequireCover([full with { SourceIndex = 1 }], full));
w.WriteNumber("guards", guards); w.WriteNumber("actualObjects", actualObjects); w.WriteNumber("actualFragments", actualFragments); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS4 genuine recognition-owned raw roots/{actualObjects} C49 objects/{actualFragments} checked identity fragments/4096 prepared coverage controls/{guards} guards; no SDK registry,037E80 caller,or acceptance claim.");
uint Next() { state = unchecked(state * 1664525 + 1013904223); return state; }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException or InvalidOperationException) { guards++; return; } throw new InvalidDataException("Missing raw/current view gate."); }
void WriteDescriptor(string? name, LegacyNativeSelectedInkRange d)
{
    if (name is null) w.WriteStartArray(); else w.WriteStartArray(name);
    w.WriteNumberValue(d.SourceIndex); w.WriteNumberValue(d.PointCount); w.WriteNumberValue(d.FirstPoint); w.WriteNumberValue(d.LastPoint);
    w.WriteNumberValue(BitConverter.SingleToUInt32Bits(d.FirstFraction)); w.WriteNumberValue(BitConverter.SingleToUInt32Bits(d.LastFraction));
    w.WriteBooleanValue(d.FirstClosed); w.WriteBooleanValue(d.LastClosed); w.WriteBooleanValue(d.IsKnownText); w.WriteEndArray();
}
InkStroke[] Rotate(InkStroke[] raw, float a) => raw.Select(s => new InkStroke(s.Points.Select(p => new InkPoint(p.X * MathF.Cos(a) - p.Y * MathF.Sin(a), p.X * MathF.Sin(a) + p.Y * MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x) => [Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)), Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)), Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params (float X,float Y)[] knots) { var p = new List<InkPoint>(); for(int i=1;i<knots.Length;i++) for(int t=0;t<8;t++){ float f=t/8f; p.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f)); } p.Add(new(knots[^1].X,knots[^1].Y)); return new(p.ToArray()); }
