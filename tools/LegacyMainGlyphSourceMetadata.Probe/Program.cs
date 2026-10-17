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
var chronology=sourceRanges.PreparedChronology;var original=sourceRanges.OriginalSource;
int total=original.Strokes.Sum(s=>s.Points.Count);var keys=new LegacySourcePointKey[total];var ends=new LegacySourcePointKey[total];
for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
{var pointIdentity=original.Chronology.Resolve(s,p);keys[pointIdentity.KeyRecordIndex]=pointIdentity.Key;ends[pointIdentity.KeyRecordIndex]=original.ResolveEndingMetadata(pointIdentity).Key;}
int queries=0,higher=0,guards=0;var baseRefs=new List<ulong>();
for(int n=0;n<service.Graph.NodeCount;n++)for(int a=0;a<service.Graph.GetNode(n).Count;a++)baseRefs.Add((uint)n|((ulong)a<<24));
using(var stream=File.Create(args[6]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteNumber("factor",chronology.SourceFactor);w.WriteStartArray("map");foreach(uint tick in chronology.PreparedPointMap.Span)w.WriteNumberValue(tick);w.WriteEndArray();
    w.WriteStartArray("counts");foreach(int count in chronology.StrokePointCounts.Span)w.WriteNumberValue(count);w.WriteEndArray();
    w.WriteStartArray("keys");foreach(var key in keys)Key(key);w.WriteEndArray();w.WriteStartArray("endingKeys");foreach(var key in ends)Key(key);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<service.Graph.NodeCount;n++){w.WriteStartArray();foreach(var arc in service.Graph.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var group in service.Graph.Groups){w.WriteStartArray();foreach(var range in group.Ranges.Span)Range(range);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("queries");
    foreach(ulong reference in baseRefs)Inspect(reference,[]);
    //These are actual source-overlay merges over real graph references, not
    //claims that the selected recognition path contained these residuals.
    int bound=Math.Min(baseRefs.Count,24);
    for(int a=0;a<bound;a++)for(int b=0;b<bound;b+=3)
    {ulong[] input=[baseRefs[a],baseRefs[b]];var merged=overlay.MergeSources(input);if(!merged.Succeeded)throw new InvalidDataException("Owned source merge refused.");Inspect(merged.Reference,input);}
    w.WriteEndArray();
    Reject(()=>service.Read(1UL<<40));Reject(()=>service.Read(0xffffUL<<24));Reject(()=>service.Read(0x7fffff));
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();Reject(()=>service.Read(0,cancelled.Token));
    Reject(()=>LegacyOwnedMainGlyphSourceMetadata.Prepare(ownership,null!,sourceRanges));
    var foreignSource=MainFixture.Create(ak,engine,table).SourceRanges;
    Reject(()=>LegacyOwnedMainGlyphSourceMetadata.Prepare(ownership,overlay,foreignSource));
    var otherConfidence=lineContext is null?LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary):LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary,lineContext);
    var otherGeometry=lineContext is null?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,otherConfidence,owner.Source):LegacyOwnedRussianMainSourceGeometry.Prepare(ak,otherConfidence,owner.MainLineSource);
    var otherOverlay=LegacyOwnedRussianMainResidualSource.Prepare(otherGeometry,policy);
    Reject(()=>LegacyOwnedMainGlyphSourceMetadata.Prepare(ownership,otherOverlay,sourceRanges));
    bool invalidLastChecked=false;
    foreach(ulong sourceRef in baseRefs)
    {
        var physical=overlay.CopySourceRanges(sourceRef);
        if(physical.Length!=1||physical[0].EndPointExclusive-physical[0].StartPoint<2)continue;
        //The public lower-level overlay can retain arbitrary high bits of its
        //originalLast input. This owning metadata contract refuses a glyph
        //alternative ordinal masquerading as that canonical source key.
        ulong foreign=sourceRef|(1UL<<40);
        var invalid=overlay.MergeSources(new[]{foreign,foreign,foreign,foreign});
        ulong originalLast=overlay.ResolveBaseReference(invalid.Reference);
        if(originalLast==LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(originalLast))continue;
        Reject(()=>service.Read(invalid.Reference));invalidLastChecked=true;break;
    }
    if(!invalidLastChecked)throw new InvalidDataException("Noncanonical originalLast ownership guard was not exercised.");
    w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
    void Inspect(ulong reference,ulong[] mergeInputs)
    {
        var result=service.Read(reference);queries++;if(result.IsHigherSource)higher++;
        if(!ReferenceEquals(result.Owner,service)||result.BaseSourceKey!=overlay.ResolveBaseReference(reference)||
            result.AcousticGroupId!=service.Graph.GetNode(result.BaseNode)[result.BaseArc].GroupId)
            throw new InvalidDataException("Source target/base/group ownership changed.");
        var actualRanges=overlay.CopySourceRanges(reference);
        byte[] refs=result.CopyReferenceStorage24(),remainders=result.CopyResidualStorage2(),storage=result.CopySourceAllocation26();
        if(!storage.AsSpan(0,refs.Length).SequenceEqual(refs)||!storage.AsSpan(refs.Length).SequenceEqual(remainders))throw new InvalidDataException("Native26 allocation layout changed.");
        storage.AsSpan().Fill(0x55);if(result.CopyReferenceStorage24().AsSpan().SequenceEqual(storage.AsSpan(0,refs.Length)))throw new InvalidDataException("Source bytes alias caller storage.");
        w.WriteStartObject();w.WriteNumber("target",result.TargetSourceKey);w.WriteNumber("base",result.BaseSourceKey);w.WriteBoolean("higher",result.IsHigherSource);w.WriteNumber("group",result.AcousticGroupId);
        w.WriteStartArray("mergeInputs");foreach(ulong key in mergeInputs)w.WriteNumberValue(key);w.WriteEndArray();
        w.WriteStartArray("physicalRanges");foreach(var r in actualRanges)Range(r);w.WriteEndArray();
        w.WriteBase64String("references24",refs);w.WriteBase64String("residuals2",remainders);w.WriteEndObject();
    }
    void Key(LegacySourcePointKey key){w.WriteStartArray();w.WriteNumberValue(key.First);w.WriteNumberValue(key.Second);w.WriteNumberValue(key.Third);w.WriteEndArray();}
    void Range(LegacyInkRange range){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}
}
if(queries<5||higher==0)throw new InvalidDataException("Real higher-kind0 metadata path was not exercised.");
Console.WriteLine($"PASS actual owned base/higherV140 metadata {queries} queries/{higher} higher sources/{guards} guards; no protocol/lazy refresh/wrapper claim.");
void Reject(Action action){try{action();throw new InvalidDataException("Guard ignored.");}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;}}
