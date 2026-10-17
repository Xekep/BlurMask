using InkRecognition;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;

if (args.Length != 9) throw new ArgumentException("AK LK engine separation case publication line30 line50 output");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
byte[] line30 = File.ReadAllBytes(args[6]), line50 = File.ReadAllBytes(args[7]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
InkStroke[] raw = [new(new InkPoint[] { new(0, 90), new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) })];
var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256],
    new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
var result = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50, raw, 0, table, cases, publication)
    .Run(residue, r => new byte[r.RequiredBytes], new byte[20000 * 40]);
var alternatives = result.Alternatives.CopyActiveAlternatives();
int guards = 0, refreshed = 0, visited = 0, added = 0, selectionFailures = 0;
var refreshOwners = new List<LegacyOwnedMainOrdinaryGlyphRefresh>();
using var output = File.Create(args[8]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int a = 0; a < alternatives.Length; a++)
{
    var view = LegacyOwnedMainCandidateSource.FromConfigured(result, alternatives[a]);
    var refresh = LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, view, table);
    var repeated = LegacyOwnedMainCandidateSource.FromConfigured(result, alternatives[a]);
    if (!ReferenceEquals(refresh, LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, repeated, table)))
        throw new Exception("Same genuine candidate reset its refresh cache.");
    if (refreshOwners.Any(r => ReferenceEquals(r, refresh))) throw new Exception("Two candidates shared a mutable overlay refresh epoch.");
    refreshOwners.Add(refresh);
    Reject(() => _ = refresh.RawResult);
    Reject(() => LegacyOwnedRussianMainGlyphScoring.Prepare(ak, refresh.SourceMetadata, table));
    Reject(() => LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(new byte[ak.Length], view, table));
    Reject(() => LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, view, table, new(true)));
    if (!MemoryMarshal.TryGetArray(table.Values, out ArraySegment<float> values)) throw new Exception("Expected table snapshot.");
    float saved = values.Array![values.Offset];
    try
    {
        values.Array[values.Offset] = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(saved) ^ 1);
        Reject(() => LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(ak, view, table));
    }
    finally { values.Array[values.Offset] = saved; }
    if (a == 1) Reject(() => LegacyOwnedRussianMainGlyphScoring.PrepareConfigured(ak, view, refreshOwners[0].SourceMetadata, table));
    var pool = view.Ownership.GlyphPool; byte[] compact = result.Compact.CopyCompactStorage(); int[] starts = result.Compact.CopyNodeStarts2478();
    w.WriteStartObject(); w.WriteString("compact", Convert.ToHexString(compact));
    w.WriteStartArray("starts"); foreach (int n in starts) w.WriteNumberValue(n); w.WriteEndArray();
    w.WriteStartArray("owners");
    var glyphs = view.CopyRawWordsInNativeHeadOrder().SelectMany(word => word.Glyphs).Where(g => !g.IsProtocolSource)
        .DistinctBy(g => g.CanonicalSourceKey).ToArray();
    foreach (var glyph in glyphs)
    {
        var before = pool.CopyOwnerNode(glyph.CanonicalSourceKey);
        byte[] allocated = Enumerable.Range(0, 40).Select(i => (byte)(i * 17 + a)).ToArray();
        Reject(() => refresh.RefreshOwner(glyph.CanonicalSourceKey, new byte[39]));
        var completed = refresh.RefreshOwner(glyph.CanonicalSourceKey, allocated); var after = pool.CopyOwnerNode(glyph.CanonicalSourceKey);
        if (!ReferenceEquals(completed, refresh.RefreshOwner(glyph.CanonicalSourceKey, new byte[40])))
            throw new Exception("Repeated configured refresh changed the completed source.");
        var failure = refresh.Select(glyph.Reference, allocated, []);
        if (failure.Selection.Succeeded) throw new Exception("Selection allocation refusal was hidden.");
        selectionFailures++;
        var selected = refresh.Select(glyph.Reference, allocated, new byte[2040 * 4]);
        if (!selected.Selection.Succeeded) throw new Exception("Actual configured refresh selection failed.");
        refreshed++; visited += completed.VisitedCompactCount; added += (after.Records.Length - before.Records.Length) / 40;
        w.WriteStartObject(); w.WriteNumber("key", glyph.CanonicalSourceKey); w.WriteNumber("base", completed.Source.BaseSourceKey);
        w.WriteBoolean("preparedHigher", false); w.WriteNumber("beforePacked", before.PackedCount); w.WriteNumber("afterPacked", after.PackedCount);
        w.WriteNumber("sourceScoreBits", completed.SourceScoreBits); w.WriteNumber("visited", completed.VisitedCompactCount);
        w.WriteString("residue", Convert.ToHexString(allocated)); w.WriteString("before", Convert.ToHexString(before.Records.Span));
        w.WriteString("after", Convert.ToHexString(after.Records.Span)); w.WriteString("source26", Convert.ToHexString(completed.Source.CopySourceAllocation26()));
        w.WriteNumber("inputMarker", (ushort)(glyph.Reference >> 40)); w.WriteNumber("marker", selected.Selection.SelectedMarker!.Value);
        w.WriteString("selection", Convert.ToHexString(selected.Selection.CopySelectionRows4())); w.WriteStartArray("scores");
        for (int index = starts[completed.Source.BaseNode]; index < starts[completed.Source.BaseNode + 1]; index++)
        {
            var row = compact.AsSpan(index * 40, 40);
            if ((row[32] & 0x27) != 0x20 || BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) != completed.Source.BaseSourceKey) continue;
            int descriptor = index - starts[completed.Source.BaseNode];
            byte[] actual = after.Records.Span.ToArray().Chunk(40).Single(r => BinaryPrimitives.ReadInt32LittleEndian(r) == descriptor);
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(actual.AsSpan(8));
            var score = refresh.Scoring.ReadPreparedSymbol(completed.Source, symbol, (row[32] & 0x40) != 0);
            w.WriteStartObject(); w.WriteNumber("compact", index); w.WriteNumber("descriptor", descriptor); w.WriteNumber("symbol", symbol);
            w.WriteNumber("symbolScoreBits", score.SymbolScoreBits); w.WriteNumber("firstScoreBits", score.FirstCandidateScoreBits);
            w.WriteNumber("cost", score.Cost); w.WriteString("request", Convert.ToHexString(score.CopyRequest16())); w.WriteEndObject();
        }
        w.WriteEndArray(); w.WriteEndObject();
    }
    w.WriteEndArray(); w.WriteEndObject();
}
w.WriteEndArray();
//Explicit diagnostic guard after the actual refresh captures. Allocate a real
//functional-overlay higher source from genuine base ranges; this is not an
//inferred residual glyph or evidence of cross-alternative namespace sharing.
var controlView = LegacyOwnedMainCandidateSource.FromConfigured(result, alternatives[0]);
var controlRefresh = refreshOwners[0]; var graph = controlView.Sampled.SourceRanges.Graph;
ulong[] bases = Enumerable.Range(0, graph.NodeCount).SelectMany(n => Enumerable.Range(0, graph.GetNode(n).Count)
    .Select(a => (uint)n | ((ulong)a << 24))).Take(24).ToArray();
LegacyOwnedMainGlyphSourceMetadataResult? higher = null;
foreach (ulong left in bases)
{
    foreach (ulong right in bases)
    {
        var merged = controlView.SourceOverlay.MergeSources([left, right]);
        var source = controlRefresh.SourceMetadata.Read(merged.Reference);
        if (source.IsHigherSource) { higher = source; break; }
    }
    if (higher is not null) break;
}
if (higher is null) throw new Exception("Prepared higher-source guard did not exercise an actual overlay source.");
controlView.Ownership.GlyphPool.AppendDescriptor(higher.TargetSourceKey, 0, false, new byte[40]);
byte[] higherBefore = controlView.Ownership.GlyphPool.CopyOwnerNode(higher.TargetSourceKey).Records.ToArray();
for (int i = 0; i < 2; i++)
{
    try { controlRefresh.RefreshOwner(higher.TargetSourceKey, new byte[40]); throw new Exception("Configured higher source was silently refreshed."); }
    catch (NotSupportedException) { guards++; }
}
if (!controlView.Ownership.GlyphPool.CopyOwnerNode(higher.TargetSourceKey).Records.Span.SequenceEqual(higherBefore))
    throw new Exception("Unsupported configured higher-source guard mutated the pool.");
w.WriteNumber("guards", guards); w.WriteNumber("refreshed", refreshed); w.WriteNumber("visited", visited);
w.WriteNumber("added", added); w.WriteNumber("selectionFailures", selectionFailures); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS two genuine configured candidate-local refreshes/{refreshed} owners/{visited} compact rows/{added} new glyphs/{guards} guards; no shared higher-source claim.");
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Foreign, invalid or incorrect-variant configured refresh accepted.");
}
