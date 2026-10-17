using InkRecognition;
using System.Text.Json;

if (args.Length != 11) throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] line30 = File.ReadAllBytes(args[8]), line50 = File.ReadAllBytes(args[9]);
InkStroke[] m = [new(new InkPoint[] { new(0, 90), new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) })];
InkStroke[][] actualInputs = [m, Word(0).Concat(Word(300)).ToArray(), Word(450).Concat(Word(0)).ToArray(), Rotate(m, .18f)];
using var file = File.Create(args[10]); using var w = new Utf8JsonWriter(file);
int guards = 0, roots = 0, copies = 0, objectCount = 0;
w.WriteStartObject(); w.WriteStartArray("actualRecognitionRoots");
foreach (var raw in actualInputs)
{
    var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
    var result = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50, raw, 0, table, cases, publication)
        .Run(residue, r => new byte[r.RequiredBytes], new byte[20000 * 40]);
    Check(result.SourceDocument, 3);
}
w.WriteEndArray(); w.WriteStartArray("freshLibraryRoots");
// These roots check the context/registry producer without running recognition.
// In particular rawCount75/76 crosses the native history-pruning threshold.
for (int n = 1; n <= 256; n++)
{
    var strokes = new InkStroke[n];
    for (int i = 0; i < n; i++)
    {
        int count = 1 + (i + n) % 7;
        strokes[i] = new(Enumerable.Range(0, count).Select(j => new InkPoint(i * 8 + j * .25f,
            j == 0 && (i & 1) == 0 ? BitConverter.UInt32BitsToSingle(0x80000000) : n - i * .5f + j)).ToArray());
    }
    Check(LegacyOwnedRawInkDocument.Create(strokes, default), n % 4);
}
w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteNumber("roots", roots); w.WriteNumber("copies", copies);
w.WriteNumber("objects", objectCount); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS4 recognition-owned +256 fresh library roots/{copies} selected copies/{objectCount} raw objects/{guards} guards; typed INPUT/C22 registry, no SDK page or document acceptance.");

void Check(LegacyOwnedRawInkDocument doc, int depth)
{
    var view = LegacyFreshRawInkCurrentView.Prepare(doc); var owner = LegacyFreshTextFieldInputContext.Prepare(view);
    w.WriteStartObject(); w.WriteStartArray("rawPointBits");
    foreach (var stroke in doc.Strokes)
    {
        w.WriteStartArray(); foreach (var p in stroke.Points) { w.WriteNumberValue(BitConverter.SingleToUInt32Bits(p.X)); w.WriteNumberValue(BitConverter.SingleToUInt32Bits(p.Y)); } w.WriteEndArray();
    }
    w.WriteEndArray(); w.WriteStartArray("contexts");
    for (int copy = 0; copy <= depth; copy++)
    {
        if (!ReferenceEquals(owner.Document, doc) || !ReferenceEquals(owner.CurrentView, view) || !ReferenceEquals(owner.Input.Owner, owner) ||
            owner.Input.RegistryId != 0 || owner.NextRegistryId != 1 || owner.MaximumRegistryId != 0 || owner.Input.Category != 100 ||
            owner.NativeElementKind != 0 || owner.ElementName != "textField" || owner.RawObjectCount != doc.ObjectCount ||
            owner.Input.InputSpanCount != 0 || owner.Input.CopyDafInputRows32().Length != 0)
            throw new InvalidDataException("Fresh derived INPUT or retained C22 ownership changed.");
        var exported = owner.ExportInputIdentityTransforms(owner.Input);
        if (!ReferenceEquals(exported.Owner, owner) || !ReferenceEquals(exported.Input, owner.Input) ||
            !ReferenceEquals(exported.Geometry.Owner, view) || exported.Geometry.MatricesResolved != doc.ObjectCount ||
            exported.Geometry.NativeNonidentityTransformCount != 0)
            throw new InvalidDataException("INPUT source30/tag -> C22 -> identity export differs.");
        w.WriteStartObject(); w.WriteNumber("depth", copy); w.WriteNumber("registryId", owner.Input.RegistryId);
        w.WriteNumber("category", owner.Input.Category); w.WriteString("name", owner.Input.Name);
        w.WriteBase64String("counter", owner.CopyRegistryCounterStorage()); w.WriteBase64String("ranges", owner.Input.CopyRangeStorage());
        w.WriteBase64String("daf32", owner.Input.CopyDafInputRows32()); w.WriteBase64String("matrices", exported.Geometry.CopyResolvedMatrices());
        w.WriteStartArray("rawCopies"); for (int i = 0; i < doc.ObjectCount; i++) w.WriteBase64StringValue(owner.CopyRawPointStorage(i)); w.WriteEndArray();
        w.WriteStartArray("matchedViews"); foreach (int match in exported.Geometry.CopyMatchedViewIndices()) w.WriteNumberValue(match); w.WriteEndArray();
        w.WriteEndObject();
        var counter = owner.CopyRegistryCounterStorage(); counter[0] ^= 255;
        if (owner.CopyRegistryCounterStorage()[0] != 1) throw new InvalidDataException("Counter storage escaped.");
        var range = owner.Input.CopyRanges(); range[0] = default;
        if (owner.Input.CopyRanges()[0] == default) throw new InvalidDataException("C22 range storage escaped.");
        var rawCopy = owner.CopyRawPointStorage(0); rawCopy[0] ^= 255;
        if (rawCopy.AsSpan().SequenceEqual(owner.CopyRawPointStorage(0))) throw new InvalidDataException("Raw curve bytes escaped.");
        Reject(() => owner.CopyRawPointStorage(-1)); Reject(() => owner.CopyRawPointStorage(doc.ObjectCount));
        Reject(() => owner.ExportInputIdentityTransforms(owner.Input, new(true)));
        Reject(() => owner.CreateSelectedCopy(new(true)));
        if (copy == depth) break;
        var parent = owner; owner = owner.CreateSelectedCopy(); copies++;
        if (!ReferenceEquals(owner.Parent, parent) || ReferenceEquals(parent.Input, owner.Input)) throw new InvalidDataException("Selected C22 failed to rebind.");
        Reject(() => owner.ExportInputIdentityTransforms(parent.Input));
        Reject(() => parent.ExportInputIdentityTransforms(owner.Input));
    }
    w.WriteEndArray(); w.WriteEndObject(); roots++; objectCount += doc.ObjectCount;
    Reject(() => LegacyFreshTextFieldInputContext.Prepare(view, new(true)));
    Reject(() => LegacyFreshTextFieldInputContext.Prepare(null!));
}
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or OperationCanceledException) { guards++; return; } throw new InvalidDataException("Missing context/registry owner guard."); }
InkStroke[] Rotate(InkStroke[] raw, float a) => raw.Select(s => new InkStroke(s.Points.Select(p => new InkPoint(p.X * MathF.Cos(a) - p.Y * MathF.Sin(a), p.X * MathF.Sin(a) + p.Y * MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x) => [Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)), Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)), Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params (float X,float Y)[] knots) { var p = new List<InkPoint>(); for(int i=1;i<knots.Length;i++) for(int t=0;t<8;t++){ float f=t/8f; p.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f)); } p.Add(new(knots[^1].X,knots[^1].Y)); return new(p.ToArray()); }
