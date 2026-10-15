using System.Buffers.Binary;
using System.Text.Json;
using System.Security.Cryptography;
using InkRecognition;

if (args.Length is not (7 or 11)) throw new ArgumentException("AK LK engine separation case-data publication-data capture [line30 line50 recorded-manifest word-index]");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var caseFile = File.OpenRead(args[4]);
using var publicationFile = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(caseFile);
var publication = LegacyNativeTextPublicationData.Load(publicationFile);
LegacyFreshCaseRussianDictionaryPreparation dictionary;
LegacyOwnedMainWordSpanPreparation owner;
LegacyFreshMainSingleLineContext? lineContext = null;
if (args.Length == 7)
{
    var sampled = MainFixture.Create(ak, engine, table);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
}
else
{
    using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[9]));
    if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
        manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent")
        throw new InvalidDataException("Explicitly unsegmented pinned recorded coordinates required.");
    var sample = manifest.RootElement.GetProperty("samples").EnumerateArray()
        .Where(s => s.GetProperty("kind").GetString() == "word").ElementAt(int.Parse(args[10]));
    string name = sample.GetProperty("file").GetString()!;
    if (name != Path.GetFileName(name)) throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[9]))!, name));
    if (bytes.Length > 200000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != sample.GetProperty("trackSha256").GetString())
        throw new InvalidDataException("Recorded track hash changed.");
    using var parsed = JsonDocument.Parse(bytes);
    var points = parsed.RootElement.GetProperty("points").EnumerateArray()
        .Select(p => new InkPoint(p[0].GetSingle(), -p[1].GetSingle())).ToArray();
    if (points.Length is < 2 or > 4096) throw new InvalidDataException("Bounded recorded point vector required.");
    //Pen-up is absent in this dataset. One unsegmented path is an explicit
    //diagnostic input assumption; expected transcription is never read.
    var input = TrackedCaseInput.Create(ak, engine, table, File.ReadAllBytes(args[7]), File.ReadAllBytes(args[8]),
        [new InkStroke(points)], cases, publication);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
    lineContext = input.Context;
}
var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
ulong identity = 0x1000;
var result = new LegacyOwnedMainCaseWordPass(ak, policy,
    lineContext is null ? null : owner.MainLineAcoustic, table, request =>
{
    byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, ++identity);
}).RunOwned();
var invocation = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy);
var terminal = invocation.SelectOwnedPass(result);
if (!ReferenceEquals(invocation.DictionaryOwner, dictionary) || invocation.LanguagePenalty2534 != 17153 ||
    invocation.Options.Filter4 || invocation.Options.ContextSentinels24b8 || invocation.NativeRescueGateActive)
    throw new InvalidDataException("Actual fresh case source penalty or invocation gates changed.");

//Independent native2257C0 scan for this actual Filter4=false/no-sentinel route:
//primary/flag4 rows cannot win. No source label or expected text is supplied.
var final = result.GetNodeSnapshot(result.NodeCount - 1);
int best = 0x4000000, expectedOrdinal = -1;
for (int ordinal = 0; ordinal < final.SourceEnd23f0; ordinal++)
{
    var row = final.Records.Span.Slice(ordinal * 88, 88);
    if (ordinal < controller.Configuration.PrimaryArcLimit230a || (row[5] & 4) != 0) continue;
    int cost = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
    if (cost < best) { best = cost; expectedOrdinal = ordinal; }
}
if (expectedOrdinal < 0 || !terminal.Winner.HasWinner || terminal.Winner.Ordinal != expectedOrdinal ||
    terminal.Winner.RelativeCost != best || terminal.Winner.TotalCost != unchecked(best + final.CumulativePathCost2520) ||
    !terminal.CopySelectedRecord().AsSpan().SequenceEqual(final.Records.Span.Slice(expectedOrdinal * 88, 88)))
    throw new InvalidDataException("Actual case owned rows and literal2257C0 numeric selection disagree.");
var sources = invocation.CopySources(); sources[1] = default;
if (invocation.CopySources()[1].Header != dictionary.Descriptor.Flags ||
    invocation.CopySources()[1].LanguagePenalty2534 != 17153)
    throw new InvalidDataException("Case selected source array leaked mutable storage.");
var confidence = lineContext is null ? LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary) :
    LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary, lineContext);
var compact=LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak,policy,terminal,confidence,Enumerable.Repeat((byte)0xa7,4096*40).ToArray());
var callbacks=lineContext is null?LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,owner.Source):
    LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,owner.MainLineSource);
int guards=0,flushedRows=0;
using(var output=File.Create(args[6]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteStartArray("scalarControls");
    for(int i=0;i<4096;i++)
    {
        int a=unchecked((int)(0x9e3779b9u*(uint)(i+1))),b=unchecked((int)(0x85ebca6bu*(uint)(i+3)));
        int c=unchecked(~a+(i<<21)),d=unchecked(b-(i<<19));
        int node=i*131071,previous=i%5==0?node:unchecked(node-(i%409+1));
        var costs=LegacyOwnedMainGlyphFlushPlan.CalculateCosts(a,b,c,d,node,previous);
        w.WriteStartArray();foreach(int input in new[]{a,b,c,d,node,previous})w.WriteNumberValue(input);
        w.WriteNumberValue(costs.LogMassBits);w.WriteNumberValue(costs.RelativeCostBits);w.WriteNumberValue(costs.RawField18DeltaBits);w.WriteEndArray();
    }
    w.WriteEndArray();w.WriteNumber("recordCount",compact.RecordCount);w.WriteNumber("selected",compact.SelectedRecordIndex);
    w.WriteNumber("alternative",compact.SelectedAlternative);w.WriteBase64String("compact",compact.CopyCompactStorage());
    w.WriteNumber("totalCost",terminal.Winner.TotalCost);w.WriteNumber("rowCost14",terminal.Winner.RowCost14);
    w.WriteStartArray("cumulative");foreach(int value in result.CopyCumulativePathCosts())w.WriteNumberValue(value);w.WriteEndArray();
    w.WriteStartArray("starts");foreach(int start in compact.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<owner.SourceGraph.NodeCount;node++)
    {w.WriteStartArray();foreach(var arc in owner.SourceGraph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<64;n++)
    {
        byte[] residue=Enumerable.Range(0,72).Select(i=>(byte)(n*73+i*31+11)).ToArray();
        var walk=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,residue);
        var plan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,walk);
        w.WriteStartObject();w.WriteBase64String("residue",residue);w.WriteBase64String("allRows",walk.CopyRows());
        w.WriteStartArray("visited");foreach(int index in walk.CopyVisitedCompactIndices())w.WriteNumberValue(index);w.WriteEndArray();
        w.WriteStartArray("flushes");
        foreach(var flush in plan.Flushes)
        {
            flushedRows+=flush.RowCount;w.WriteStartObject();w.WriteNumber("current",flush.CurrentCompactIndex);
            w.WriteNumber("filtered",flush.FilteredCompactIndex);w.WriteBoolean("protocol",flush.ProtocolFlush);
            w.WriteBase64String("rows",flush.CopyRows72());w.WriteBase64String("template",flush.CopyPacketWriteTemplate());
            w.WriteStartArray("origins");foreach(int value in flush.CopyInitialWordSeedOrigins())w.WriteNumberValue(value);w.WriteEndArray();
            w.WriteStartArray("residualAdditions");foreach(ulong key in flush.CopyResidualAdditions())w.WriteNumberValue(key);w.WriteEndArray();w.WriteEndObject();
            byte[] mutable=flush.CopyRows72();mutable[0]^=255;
            if(mutable.AsSpan().SequenceEqual(flush.CopyRows72()))throw new InvalidDataException("Flush rows escaped ownership.");
            var seed=flush.CopyInitialWordSeedOrigins();seed[0]=119;
            if(flush.CopyInitialWordSeedOrigins()[0]!=-1)throw new InvalidDataException("Flush origins escaped ownership.");
            mutable=flush.CopyPacketWriteTemplate();mutable[0]^=255;
            if(mutable.AsSpan().SequenceEqual(flush.CopyPacketWriteTemplate()))throw new InvalidDataException("Flush template escaped ownership.");
        }
        w.WriteEndArray();w.WriteStartArray("source70");
        foreach(var e in plan.SourceMetadataEvents)
        {w.WriteStartObject();w.WriteNumber("index",e.CompactIndex);w.WriteNumber("tag",e.FullSourceTag);w.WriteNumber("symbol",e.RawSymbol);w.WriteBase64String("metadata",e.CopyMetadata16());w.WriteEndObject();}
        w.WriteEndArray();w.WriteEndObject();
        if(plan.Flushes.Sum(f=>f.RowCount)!=walk.Count||plan.Flushes.Count==0)
            throw new InvalidDataException("Actual flushes did not partition the numeric append events.");
        if(n==0)
        {
            Reject(()=>LegacyOwnedMainGlyphFlushPlan.Prepare(null!,walk));
            Reject(()=>LegacyOwnedMainGlyphFlushPlan.Prepare(policy,null!));
            Reject(()=>LegacyOwnedMainGlyphFlushPlan.Prepare(policy,walk,new(true)));
            Reject(()=>LegacyOwnedMainGlyphFlushPlan.Prepare(LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller),walk));
        }
    }
    w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("flushedRows",flushedRows);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS64 actual CASE flush plans/{flushedRows} rows/{guards} owner/cancel guards; packet seed and source70 inactivity preserved.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Invalid flush owner accepted.");}
