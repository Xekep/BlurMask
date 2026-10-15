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
int guards=0,rows=0;
using(var output=File.Create(args[6]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteNumber("recordCount",compact.RecordCount);w.WriteNumber("selected",compact.SelectedRecordIndex);
    w.WriteNumber("alternative",compact.SelectedAlternative);w.WriteBase64String("compact",compact.CopyCompactStorage());
    w.WriteStartArray("starts");foreach(int start in compact.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<owner.SourceGraph.NodeCount;node++)
    {w.WriteStartArray();foreach(var arc in owner.SourceGraph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<128;n++)
    {
        byte[] residue=Enumerable.Range(0,72).Select(i=>(byte)(n*73+i*31+11)).ToArray();
        var walk=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,residue);byte[] emitted=walk.CopyRows();
        if(walk.Count==0)throw new InvalidDataException("Actual case numeric winner contains no emitted row.");rows+=walk.Count;
        w.WriteStartObject();w.WriteBase64String("residue",residue);w.WriteBase64String("rows",emitted);
        w.WriteStartArray("visited");foreach(int index in walk.CopyVisitedCompactIndices())w.WriteNumberValue(index);w.WriteEndArray();
        w.WriteStartArray("residuals");foreach(ulong key in walk.CopyResidualSourceReferences())w.WriteNumberValue(key);w.WriteEndArray();w.WriteEndObject();
        byte[] copy=walk.CopyRows();copy[0]^=255;if(copy.AsSpan().SequenceEqual(walk.CopyRows()))throw new InvalidDataException("Case row snapshot escaped ownership.");
    }
    w.WriteEndArray();
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,(LegacyFreshCaseMainSourcePolicy)null!,compact,callbacks,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,null!,callbacks,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,null!,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,new byte[71]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,new byte[72],new(true)));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,equivalent(),compact,callbacks,new byte[72]));
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,new byte[72]));
    var corrupt=ak.ToArray();corrupt[566416]^=1;Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(corrupt,policy,compact,callbacks,new byte[72]));
    var freshConfidence=lineContext is null?LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary):LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary,lineContext);
    var foreignCallbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(freshConfidence);
    Reject(()=>LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,foreignCallbacks,new byte[72]));
    w.WriteNumber("guards",guards);w.WriteNumber("rowCount",rows);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS128 actualCASE compact-winner walks/{rows} full72 rows/{guards} owner/cancel/resource guards; no expected transcription or word segmentation.");
LegacyFreshCaseMainSourcePolicy equivalent()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Invalid CASE walker binding accepted.");}
