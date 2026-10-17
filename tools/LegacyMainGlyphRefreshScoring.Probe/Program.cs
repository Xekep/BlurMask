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
string? inputTrackFingerprint=null;
if (args.Length == 7)
{
    var sampled = MainFixture.Create(ak, engine, table);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
}
else if(args[10]=="twoPhysical")
{
    var input=MainLineWordFixture.Create(ak,engine,File.ReadAllBytes(args[3]),File.ReadAllBytes(args[7]),
        File.ReadAllBytes(args[8]),cases,publication,2);
    dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,input.Source,cases,publication);
    owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,input.Source,input.Context,table);
    lineContext=input.Context;
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
    inputTrackFingerprint=Convert.ToHexStringLower(SHA256.HashData(bytes));
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
var geometry=lineContext is null?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.Source):
    LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.MainLineSource);
var interior=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,callbacks,geometry);
var ownership=LegacyFreshMainPlainResultOwnership.Create(compact,geometry);
var overlay=LegacyOwnedRussianMainResidualSource.Prepare(geometry,policy);
var sourceRanges=lineContext is null?owner.Source.SourceRanges:owner.MainLineSource.SourceRanges;
var service=LegacyOwnedMainGlyphSourceMetadata.Prepare(ownership,overlay,sourceRanges);
var scoring=LegacyOwnedRussianMainGlyphScoring.Prepare(ak,service,table);
var refs=new List<ulong>();
for(int n=0;n<owner.SourceGraph.NodeCount;n++)for(ushort a=0;a<owner.SourceGraph.GetNode(n).Count;a++)
    refs.Add((uint)n|((ulong)a<<24));
var prepared=refs.Select(r=>service.Read(r)).ToList();
for(int i=0;i<Math.Min(refs.Count,24);i++)for(int j=0;j<Math.Min(refs.Count,24);j+=3)
{
    var merged=overlay.MergeSources(new[]{refs[i],refs[j]});
    prepared.Add(service.Read(merged.Reference));
}
int controls=0,guards=0,higher=0,categoryRefusals=0;
using(var file=File.Create(args[6]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteNumber("height",owner.SourceHeight);
    w.WriteStartArray("points");foreach(var p in owner.SourceInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("counts");foreach(int c in owner.SourceInk.StrokePointCounts.Span)w.WriteNumberValue(c);w.WriteEndArray();
    w.WriteStartArray("groups");
    foreach(int group in prepared.Select(p=>p.AcousticGroupId).Distinct())
    {
        var scored=confidence.ScoreGroup(group);w.WriteStartObject();w.WriteNumber("group",group);
        w.WriteStartArray("ranges");foreach(var r in owner.SourceGraph.Groups[group].Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("probabilities");for(int f=0;f<4;f++){w.WriteStartArray();foreach(int p in scored.CopyFamilyProbabilities(f))w.WriteNumberValue(p);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("candidates");foreach(var c in scored.Candidates.Span){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToUInt32Bits(c.Confidence));w.WriteEndArray();}w.WriteEndArray();
        w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteStartArray("sources");
    foreach(var item in prepared)
    {
        if(item.IsHigherSource)higher++;
        uint first=scoring.ReadFirstCandidateScoreBits(item);
        w.WriteStartObject();w.WriteNumber("target",item.TargetSourceKey);w.WriteNumber("base",item.BaseSourceKey);
        w.WriteNumber("node",item.BaseNode);w.WriteNumber("arc",item.BaseArc);w.WriteNumber("group",item.AcousticGroupId);
        w.WriteBoolean("higher",item.IsHigherSource);w.WriteNumber("first",first);w.WriteStartArray("cases");
        //Labels here are prepared scalar controls drawn from actual candidates,
        //plus pinned resource labels. No expected transcription enters this path.
        foreach(ushort symbol in confidence.ScoreGroup(item.AcousticGroupId).Candidates.ToArray().Select(c=>c.Symbol)
            .Concat(new ushort[]{178,191,129,134,187,188}).Distinct())
        for(int mode=0;mode<2;mode++)
        {
            uint before=scoring.ReadSymbolScoreBits(item,symbol);
            LegacyOwnedMainGlyphRefreshScores read;
            try{read=scoring.ReadPreparedSymbol(item,symbol,mode!=0);}
            catch(NotSupportedException)
            {
                if(scoring.ReadSymbolScoreBits(item,symbol)!=before||scoring.ReadFirstCandidateScoreBits(item)!=first)
                    throw new InvalidDataException("Unsupported category corrupted the prefix scalars.");
                w.WriteStartObject();w.WriteNumber("symbol",symbol);w.WriteBoolean("end",mode!=0);
                w.WriteNumber("symbolBits",before);w.WriteBoolean("categoryRefused",true);w.WriteEndObject();categoryRefusals++;continue;
            }
            byte[] request=read.CopyRequest16();
            if(!ReferenceEquals(read.Owner,scoring)||!ReferenceEquals(read.Source,item)||
                read.SymbolScoreBits!=before||read.FirstCandidateScoreBits!=first)
                throw new InvalidDataException("Refresh scoring owner/channel changed.");
            request[0]^=0x55;if(read.CopyRequest16()[0]==request[0])throw new InvalidDataException("Request bytes alias caller storage.");
            w.WriteStartObject();w.WriteNumber("symbol",symbol);w.WriteBoolean("end",mode!=0);
            w.WriteNumber("symbolBits",read.SymbolScoreBits);w.WriteNumber("cost",read.Cost);
            w.WriteBase64String("request16",read.CopyRequest16());w.WriteEndObject();controls++;
        }
        w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();
    var firstItem=prepared[0];
    Reject(()=>LegacyOwnedRussianMainGlyphScoring.Prepare(ak,null!,table));
    Reject(()=>LegacyOwnedRussianMainGlyphScoring.Prepare(ak,service,null!));
    Reject(()=>LegacyOwnedRussianMainGlyphScoring.Prepare(ak,service,table,new(true)));
    byte[] bad=ak.ToArray();bad[566416]^=1;Reject(()=>LegacyOwnedRussianMainGlyphScoring.Prepare(bad,service,table));
    Reject(()=>scoring.ReadPreparedSymbol(null!,0,false));
    Reject(()=>scoring.ReadFirstCandidateScoreBits(firstItem,new(true)));
    Reject(()=>scoring.ReadSymbolScoreBits(firstItem,0,new(true)));
    Reject(()=>scoring.ReadPreparedSymbol(firstItem,0,false,new(true)));
    var foreignService=LegacyOwnedMainGlyphSourceMetadata.Prepare(ownership,overlay,sourceRanges);
    var foreign=foreignService.Read(firstItem.TargetSourceKey);
    Reject(()=>scoring.ReadPreparedSymbol(foreign,0,false));
    Reject(()=>scoring.ReadFirstCandidateScoreBits(foreign));
    Reject(()=>scoring.ReadSymbolScoreBits(foreign,0));
    //This native read precedes a category failure, and remains valid afterward.
    uint prefix=scoring.ReadSymbolScoreBits(firstItem,65535);
    if(prefix!=0)throw new InvalidDataException("Absent symbol vector is not zero.");
    Reject(()=>scoring.ReadPreparedSymbol(firstItem,65535,false));
    uint firstPrefix=scoring.ReadFirstCandidateScoreBits(firstItem);
    if(scoring.ReadFirstCandidateScoreBits(firstItem)!=firstPrefix||
        scoring.ReadSymbolScoreBits(firstItem,65535)!=prefix)throw new InvalidDataException("Cost refusal corrupted prior scalar reads.");
    w.WriteNumber("controls",controls);w.WriteNumber("categoryRefusals",categoryRefusals);w.WriteNumber("higherReads",higher);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS actual refresh scorer {prepared.Count} source reads/{higher} higher/{controls} prepared-symbol costs/{categoryRefusals} category refusals/{guards} guards; no pool refresh/symbol-production claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Scope guard ignored.");}
