using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 11) throw new ArgumentException("AK LK engine separation case publication line30 line50 normalization circles capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] line30 = File.ReadAllBytes(args[6]), line50 = File.ReadAllBytes(args[7]);
using var nf = File.OpenRead(args[8]); using var circleFile = File.OpenRead(args[9]);
var normalization = LegacyNativeUnicodeNormalizationData.Load(nf); var circles = LegacyNativeCircledPayloadData.Load(circleFile);
InkStroke[][] inputs = [[new(new InkPoint[] { new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90) })],
    Word(0).Concat(Word(300)).ToArray(), Word(450).Concat(Word(0)).ToArray()];
var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256],
    new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
int guards = 0, actualSpans = 0, preparedSpans = 0, refused = 0, emitted = 0, overlap = 0;
using var output = File.Create(args[10]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int input = 0; input < inputs.Length; input++)
{
    var result = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50,
        inputs[input], 0, table, cases, publication).Run(residue, r => new byte[r.RequiredBytes], new byte[20000 * 40]);
    var alternatives = result.Alternatives.CopyActiveAlternatives();
    var views = alternatives.Select(a => LegacyOwnedMainCandidateSource.FromConfigured(result, a)).ToArray();
    if (views.Length != 2) throw new Exception("Actual configured pair required for the sameTOP overlap control.");
    var pool = views[0].Ownership.GlyphPool;
    w.WriteStartObject(); w.WriteNumber("input", input); WriteSource(views[0]); w.WriteStartArray("candidates");
    for (int a = 0; a < views.Length; a++)
    {
        var view = views[a]; var refresh = LegacyOwnedMainSpanGlyphRefresh.PrepareConfigured(view);
        var repeated = LegacyOwnedMainCandidateSource.FromConfigured(result, alternatives[a]);
        if (!ReferenceEquals(refresh, LegacyOwnedMainSpanGlyphRefresh.PrepareConfigured(repeated)))
            throw new Exception("Repeated same candidate reset the span epoch.");
        var emission = LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(view, normalization, circles);
        if (!ReferenceEquals(emission.TopOwner, view.TopOwner) || emission.ControlSymbolCount != 0 || emission.AlternateOffsetCount != 0)
            throw new Exception("Configured emission changed actual TOP/control/alternate ownership.");
        Reject(() => _ = emission.RawResult); Reject(() => _ = refresh.RawResult); Reject(() => _ = refresh.Source.RawResult);
        Reject(() => LegacyOwnedMainSpanGlyphRefresh.PrepareConfigured(view, new(true)));
        Reject(() => LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(view, normalization, circles, new(true)));
        w.WriteStartObject(); w.WriteNumber("alternative", a); w.WriteStartArray("emission");
        foreach (var node in pool.CopyOwnerNodes())
        {
            var snapshot = new LegacyNativeGlyphRecordOwner([node]);
            for (int i = 0; i < node.Records.Length / 40; i++)
            {
                var glyph = snapshot.Resolve(node.Key | ((ulong)i << 40));
                w.WriteStartObject(); w.WriteNumber("reference", glyph.Reference); w.WriteBase64String("row40", glyph.ReadRecord());
                try { w.WriteBase64String("utf8", emission.Emit(glyph)); }
                catch (Exception e) when (e is InvalidDataException or NotSupportedException) { w.WriteBoolean("refused", true); }
                w.WriteEndObject(); emitted++;
                Reject(() => emission.Emit(glyph, new(true)));
            }
        }
        w.WriteEndArray();
        var firstNode = pool.CopyOwnerNodes().First(n => n.Records.Length != 0);
        byte[] altered = firstNode.Records.ToArray(); altered[6] ^= 1;
        var foreign = new LegacyNativeGlyphRecordOwner([new(firstNode.Key, firstNode.PackedCount, altered)]).Resolve(firstNode.Key);
        Reject(() => emission.Emit(foreign));
        w.WriteStartArray("actual");
        foreach (var node in pool.CopyOwnerNodes().Where(n => unchecked((short)(n.Key >> 24)) < 0))
        {
            var value = refresh.RefreshOwner(node.Key); var after = pool.CopyOwnerNode(node.Key);
            if (!ReferenceEquals(value, refresh.RefreshOwner(node.Key)) || value.SourceScoreBits != 0x3f800000 ||
                after.PackedCount != node.PackedCount || !after.Records.Span.SequenceEqual(node.Records.Span))
                throw new Exception("Span-only refresh mutated its actual shared pool.");
            WriteSpan(value.Source, false, node.Records.ToArray(), after.Records.ToArray()); actualSpans++;
        }
        w.WriteEndArray(); w.WriteStartArray("prepared");
        var contexts = Enumerable.Range(0, refresh.Source.Graph.NodeCount).SelectMany(n => refresh.Source.Graph.GetNode(n))
            .Where(arc => arc.Span == 0).SelectMany(arc => new[] { (int)arc.StartContextId, (int)arc.EndContextId }).Distinct().Order().ToArray();
        int completed = 0;
        foreach (int last in contexts) foreach (int first in contexts.Prepend(-1)) if (first < last && last - first <= 32768)
            for (int mode = 0; mode < 2; mode++)
            {
                ulong key = ((uint)last & 0xffffffUL) | ((ulong)unchecked((ushort)(first - last)) << 24) | ((ulong)mode << 56);
                try { var source = refresh.Source.Read(key); WriteSpan(source, true, null, null); preparedSpans++; completed++; }
                catch (NotSupportedException) { w.WriteStartObject(); w.WriteNumber("key", key); w.WriteBoolean("refused", true); w.WriteEndObject(); refused++; }
            }
        if (completed == 0) throw new Exception("Actual-source prepared span projection was not exercised.");
        w.WriteEndArray(); Reject(() => refresh.Source.Read(0)); Reject(() => refresh.RefreshOwner(0));
        w.WriteEndObject();
        void WriteSpan(LegacyOwnedMainSpanGlyphSourceResult source, bool prepared, byte[]? before, byte[]? after)
        {
            var positions = view.SourceDocument.ProjectSpanCandidate(repeated, source);
            if (!ReferenceEquals(positions.SelectedSession, view.Session) || !ReferenceEquals(positions.SpanSource, source))
                throw new Exception("Selected span source lost its true candidate/document ownership.");
            Reject(() => _ = positions.Result);
            Reject(() => view.SourceDocument.ProjectSpanCandidate(views[1 - a], source));
            Reject(() => view.SourceDocument.ProjectSpanCandidate(view, source, new(true)));
            w.WriteStartObject(); w.WriteNumber("key", source.SourceKey); w.WriteBoolean("prepared", prepared);
            w.WriteBase64String("source24", source.CopySourceStorage24());
            if (before is not null) { w.WriteBase64String("before", before); w.WriteBase64String("after", after!); w.WriteNumber("scoreBits", 0x3f800000); }
            w.WriteStartArray("positions"); foreach (var range in positions.Ranges)
            { w.WriteStartArray(); w.WriteNumberValue(range.First.Value); w.WriteNumberValue(range.Last.Value); w.WriteEndArray(); }
            w.WriteEndArray(); w.WriteEndObject();
        }
    }
    w.WriteEndArray();
    //Both refreshes use the same genuine raw source, TOP and base key, but
    //retain different selected candidate/overlay identities. This is not a
    //claim that an inferred higher source namespace can be shared.
    var baseGlyph = views[0].CopyRawWordsInNativeHeadOrder().SelectMany(word => word.Glyphs).First(g => !g.IsProtocolSource);
    var left = LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, views[0], table);
    var right = LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, views[1], table);
    var leftValue = left.RefreshOwner(baseGlyph.CanonicalSourceKey, new byte[40]);
    var beforePool = pool.CopyOwnerNodes();
    var rightValue = right.RefreshOwner(baseGlyph.CanonicalSourceKey, new byte[40]);
    var afterPool = pool.CopyOwnerNodes();
    if (beforePool.Count != afterPool.Count || leftValue.SourceScoreBits != rightValue.SourceScoreBits ||
        !leftValue.Source.CopySourceAllocation26().AsSpan().SequenceEqual(rightValue.Source.CopySourceAllocation26()))
        throw new Exception("Same native base source changed source metadata/score across alternatives.");
    for (int n = 0; n < beforePool.Count; n++)
        if (beforePool[n].Key != afterPool[n].Key || beforePool[n].PackedCount != afterPool[n].PackedCount ||
            !beforePool[n].Records.Span.SequenceEqual(afterPool[n].Records.Span))
            throw new Exception("Second candidate duplicated or rewrote a refreshed shared base pool record.");
    w.WriteStartObject("overlap"); w.WriteNumber("key", baseGlyph.CanonicalSourceKey);
    WritePool("before", beforePool); WritePool("after", afterPool); w.WriteEndObject(); overlap++;
    w.WriteEndObject();
}
w.WriteEndArray(); w.WriteNumber("actualSpans", actualSpans); w.WriteNumber("preparedSpans", preparedSpans);
w.WriteNumber("refused", refused); w.WriteNumber("emitted", emitted); w.WriteNumber("overlap", overlap);
w.WriteNumber("guards", guards); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS {inputs.Length} configured sources/{emitted} actual pool emissions/{actualSpans} inferred span owners/{preparedSpans} explicitly prepared spans/{refused} bounded span refusals/{overlap} sameTOP base overlap controls/{guards} guards.");

void WritePool(string name, IReadOnlyList<LegacyNativeGlyphRecordNode> nodes)
{
    w.WriteStartArray(name); foreach (var n in nodes)
    { w.WriteStartObject(); w.WriteNumber("key", n.Key); w.WriteNumber("packed", n.PackedCount); w.WriteBase64String("rows", n.Records.Span); w.WriteEndObject(); }
    w.WriteEndArray();
}
void WriteSource(LegacyOwnedMainCandidateSource view)
{
    var chronology = view.Sampled.Chronology; var original = view.Sampled.SourceRanges.OriginalSource;
    w.WriteNumber("factor", chronology.SourceFactor);
    w.WriteStartArray("rawCounts"); foreach (var stroke in view.SourceDocument.Strokes) w.WriteNumberValue(stroke.Points.Count); w.WriteEndArray();
    var keys = new LegacySourcePointKey[original.Strokes.Sum(s => s.Points.Count)]; var ends = new LegacySourcePointKey[keys.Length];
    for (int s = 0; s < original.Strokes.Count; s++) for (int p = 0; p < original.Strokes[s].Points.Count; p++)
    { var id = original.Chronology.Resolve(s, p); keys[id.KeyRecordIndex] = id.Key; ends[id.KeyRecordIndex] = original.ResolveEndingMetadata(id).Key; }
    w.WriteStartArray("keys"); foreach (var key in keys) Key(key); w.WriteEndArray();
    w.WriteStartArray("endingKeys"); foreach (var key in ends) Key(key); w.WriteEndArray();
    w.WriteStartArray("counts"); foreach (int count in chronology.StrokePointCounts.Span) w.WriteNumberValue(count); w.WriteEndArray();
    w.WriteStartArray("map"); foreach (uint tick in chronology.PreparedPointMap.Span) w.WriteNumberValue(tick); w.WriteEndArray();
    w.WriteStartArray("graph"); for (int node = 0; node < view.Sampled.SourceRanges.Graph.NodeCount; node++)
    {
        w.WriteStartArray(); foreach (var arc in view.Sampled.SourceRanges.Graph.GetNode(node))
        { w.WriteStartArray(); w.WriteNumberValue(arc.GroupId); w.WriteNumberValue(arc.Span); w.WriteNumberValue(arc.StartContextId); w.WriteNumberValue(arc.EndContextId); w.WriteEndArray(); }
        w.WriteEndArray();
    }
    w.WriteEndArray();
    w.WriteStartArray("groupRanges"); foreach (var group in view.Sampled.SourceRanges.Graph.Groups)
    {
        w.WriteStartArray(); foreach (var r in group.Ranges.Span)
        { w.WriteStartArray(); w.WriteNumberValue(r.StrokeIndex); w.WriteNumberValue(r.StartPoint); w.WriteNumberValue(r.EndPointExclusive); w.WriteEndArray(); }
        w.WriteEndArray();
    }
    w.WriteEndArray();
}
void Key(LegacySourcePointKey key) { w.WriteStartArray(); w.WriteNumberValue(key.First); w.WriteNumberValue(key.Second); w.WriteNumberValue(key.Third); w.WriteEndArray(); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException) { guards++; return; }
    throw new Exception("Foreign, stale or invalid configured span/emission accepted.");
}
InkStroke[] Word(float x) => [Draw((x, 80), (x, 0), (x + 30, 45), (x + 60, 0), (x + 60, 80)), Draw((x + 85, 80), (x + 85, 0), (x + 135, 0), (x + 135, 80)), Draw((x + 165, 80), (x + 165, 0), (x + 215, 0), (x + 215, 40), (x + 165, 40))];
InkStroke Draw(params (float X, float Y)[] knots)
{
    var points = new List<InkPoint>(); for (int i = 1; i < knots.Length; i++) for (int t = 0; t < 8; t++)
    { float f = t / 8f; points.Add(new(knots[i - 1].X + (knots[i].X - knots[i - 1].X) * f, knots[i - 1].Y + (knots[i].Y - knots[i - 1].Y) * f)); }
    points.Add(new(knots[^1].X, knots[^1].Y)); return new(points.ToArray());
}
