using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 9) throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] l30 = File.ReadAllBytes(args[6]), l50 = File.ReadAllBytes(args[7]);
int guards = 0, refreshed = 0, scanned = 0, added = 0, selectionFailures = 0, preparedHigher = 0;
var samples = new[] { Word(0), Word(0).Concat(Word(230)).ToArray(), Word(0).Concat(Word(300)).ToArray(),
    Word(0).Concat(Word(450)).ToArray(), Word(0).Concat(Word(700)).ToArray() };
using var output = File.Create(args[8]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int input = 0; input < samples.Length; input++)
{
    var raw = LegacyFreshMainSingleLinePlainRecognition.Prepare(ak, lk, engine, l30, l50,
        samples[input], 0, table, cases, publication).Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
    var refresh = LegacyOwnedMainOrdinaryGlyphRefresh.Prepare(ak, raw, table);
    if (!ReferenceEquals(refresh, LegacyOwnedMainOrdinaryGlyphRefresh.Prepare(ak, raw, table)))
        throw new Exception("Repeated Prepare reset the same result refresh epoch.");
    guards++;
    byte[] bad = ak.ToArray(); bad[^1] ^= 1;
    Reject(() => LegacyOwnedMainOrdinaryGlyphRefresh.Prepare(bad, raw, table));
    var pool = raw.Session.Ownership.GlyphPool;
    var nodes = pool.CopyOwnerNodes().ToList(); byte[] compact = raw.Session.Ownership.Materialization.CopyCompactStorage();
    //Explicit diagnostic control AFTER inference: use real overlay ranges to
    //create a higher source, then seed its pool owner with one real compact
    //descriptor. This is not evidence of a selected-trace higher glyph.
    var baseReferences = Enumerable.Range(0, refresh.SourceMetadata.Graph.NodeCount)
        .SelectMany(n => Enumerable.Range(0, refresh.SourceMetadata.Graph.GetNode(n).Count).Select(a => (uint)n | ((ulong)a << 24))).Take(24).ToArray();
    int[] starts = raw.Session.Ownership.Materialization.CopyNodeStarts2478();
    LegacyOwnedMainGlyphSourceMetadataResult? higherSource = null;
    foreach (ulong left in baseReferences)
    {
        foreach (ulong right in baseReferences)
        {
            var merged = refresh.SourceMetadata.SourceOverlay.MergeSources([left, right]);
            if (!merged.Succeeded) throw new Exception("Actual diagnostic overlay merge refused.");
            var source = refresh.SourceMetadata.Read(merged.Reference);
            if (source.IsHigherSource && Enumerable.Range(starts[source.BaseNode], starts[source.BaseNode+1] - starts[source.BaseNode])
                .Any(index => (compact[index*40+32] & 0x27) == 0x20 && BinaryPrimitives.ReadUInt64LittleEndian(compact.AsSpan(index*40+8)) == source.BaseSourceKey))
            { higherSource = source; break; }
        }
        if (higherSource is not null) break;
    }
    if (higherSource is null) throw new Exception("Prepared higher-source control did not produce an actual overlay.");
    int seed = Enumerable.Range(starts[higherSource.BaseNode], starts[higherSource.BaseNode + 1] - starts[higherSource.BaseNode])
        .First(index => (compact[index*40+32] & 0x27) == 0x20 && BinaryPrimitives.ReadUInt64LittleEndian(compact.AsSpan(index*40+8)) == higherSource.BaseSourceKey);
    pool.AppendDescriptor(higherSource.TargetSourceKey, seed - starts[higherSource.BaseNode], false, new byte[40]);
    nodes.Add(pool.CopyOwnerNode(higherSource.TargetSourceKey)); preparedHigher++;
    w.WriteStartObject(); w.WriteNumber("input", input); w.WriteString("compact", Convert.ToHexString(compact));
    w.WriteStartArray("starts"); foreach (int n in raw.Session.Ownership.Materialization.CopyNodeStarts2478()) w.WriteNumberValue(n); w.WriteEndArray();
    w.WriteStartArray("owners");
    foreach (var node in nodes)
    {
        if (unchecked((short)(node.Key >> 24)) < 0) continue;
        byte[] residue = Enumerable.Range(0, 40).Select(n => (byte)(n * 17 + input)).ToArray();
        Reject(() => refresh.RefreshOwner(node.Key, new byte[39]));
        Reject(() => refresh.RefreshOwner(node.Key | (1UL << 40), residue));
        Cancel(() => refresh.RefreshOwner(node.Key, residue, new CancellationToken(true)));
        var completed = refresh.RefreshOwner(node.Key, residue);
        var after = pool.CopyOwnerNode(node.Key);
        if (!ReferenceEquals(completed, refresh.RefreshOwner(node.Key, new byte[40])) ||
            !pool.CopyOwnerNode(node.Key).Records.Span.SequenceEqual(after.Records.Span))
            throw new Exception("A repeated refresh rewrote the source owner.");
        guards++; refreshed++; scanned += completed.VisitedCompactCount;
        added += (after.Records.Length - node.Records.Length) / 40;
        var refused = refresh.Select(node.Key, residue, []);
        if (refused.Selection.Succeeded || refused.Selection.SelectedMarker != 0) throw new Exception("Selection allocation failure was hidden.");
        selectionFailures++;
        var selected = refresh.Select(node.Key, residue, new byte[2040 * 4]);
        if (!selected.Selection.Succeeded || selected.Selection.Count == 0 || !ReferenceEquals(selected.RefreshedOwner, completed))
            throw new Exception("Actual refreshed owner did not reach alternative selection.");
        w.WriteStartObject(); w.WriteNumber("key", node.Key); w.WriteNumber("base", completed.Source.BaseSourceKey);
        w.WriteBoolean("preparedHigher", node.Key == higherSource.TargetSourceKey);
        w.WriteNumber("beforePacked", node.PackedCount); w.WriteNumber("afterPacked", after.PackedCount);
        w.WriteNumber("sourceScoreBits", completed.SourceScoreBits); w.WriteNumber("visited", completed.VisitedCompactCount);
        w.WriteString("residue", Convert.ToHexString(residue)); w.WriteString("before", Convert.ToHexString(node.Records.Span));
        w.WriteString("after", Convert.ToHexString(after.Records.Span));
        w.WriteString("source26", Convert.ToHexString(completed.Source.CopySourceAllocation26()));
        w.WriteNumber("marker", selected.Selection.SelectedMarker!.Value); w.WriteString("selection", Convert.ToHexString(selected.Selection.CopySelectionRows4()));
        w.WriteStartArray("scores");
        int first = raw.Session.Ownership.Materialization.CopyNodeStarts2478()[completed.Source.BaseNode];
        int end = raw.Session.Ownership.Materialization.CopyNodeStarts2478()[completed.Source.BaseNode + 1];
        for (int index = first; index < end; index++)
        {
            ReadOnlySpan<byte> row = compact.AsSpan(index * 40, 40);
            if ((row[32] & 0x20) == 0 || (row[32] & 7) != 0 || BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) != completed.Source.BaseSourceKey) continue;
            int descriptor = index - first;
            byte[] actualRecord = after.Records.Span.ToArray().Chunk(40).Single(r => BinaryPrimitives.ReadInt32LittleEndian(r) == descriptor);
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(actualRecord.AsSpan(8));
            var score = refresh.Scoring.ReadPreparedSymbol(completed.Source, symbol, (row[32] & 0x40) != 0);
            w.WriteStartObject(); w.WriteNumber("compact", index); w.WriteNumber("descriptor", descriptor); w.WriteNumber("symbol", symbol);
            w.WriteNumber("symbolScoreBits", score.SymbolScoreBits); w.WriteNumber("firstScoreBits", score.FirstCandidateScoreBits);
            w.WriteNumber("cost", score.Cost); w.WriteString("request", Convert.ToHexString(score.CopyRequest16())); w.WriteEndObject();
        }
        w.WriteEndArray(); w.WriteEndObject();
    }
    //Explicit diagnostic failure control: the adapter refuses span sources and
    //retains its single-use failed epoch; no accepted result is manufactured.
    ulong spanKey = (0xffffUL << 24) | 1;
    pool.AppendDescriptor(spanKey, -7, false, new byte[40]);
    try { refresh.RefreshOwner(spanKey, new byte[40]); throw new Exception("Span source was treated as an ordinary arc."); }
    catch (NotSupportedException) { guards++; }
    try { refresh.RefreshOwner(spanKey, new byte[40]); throw new Exception("Failed refresh epoch was retried."); }
    catch (InvalidOperationException error) when (error.Message.StartsWith("This owner retains a failed refresh prefix")) { guards++; }
    w.WriteEndArray(); w.WriteEndObject();
}
w.WriteEndArray(); w.WriteNumber("refreshed", refreshed); w.WriteNumber("visited", scanned); w.WriteNumber("added", added);
w.WriteNumber("guards", guards); w.WriteNumber("selectionFailures", selectionFailures); w.WriteEndObject(); w.Flush();
if (refreshed == 0 || scanned == 0 || added == 0 || selectionFailures != refreshed || preparedHigher != 5) throw new Exception("Actual refresh growth/selection coverage missing.");
Console.WriteLine($"PASS5 raw CASE results/{refreshed} actual owner refreshes/{scanned} compact rows/{added} added glyphs/{guards} guards; no string-wrapper acceptance claim.");

void Reject(Action action) { try { action(); } catch (ArgumentException) { guards++; return; } catch (InvalidDataException) { guards++; return; } throw new Exception("Invalid input accepted."); }
void Cancel(Action action) { try { action(); } catch (OperationCanceledException) { guards++; return; } throw new Exception("Cancellation ignored."); }
InkStroke[] Word(float x) => [Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)), Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)), Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params (float X,float Y)[] knots)
{
    var points = new List<InkPoint>(); for(int i=1;i<knots.Length;i++) for(int t=0;t<8;t++)
    { float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f)); }
    points.Add(new(knots[^1].X,knots[^1].Y)); return new(points.ToArray());
}
