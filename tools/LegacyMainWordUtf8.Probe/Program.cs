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
var backtrack=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,new byte[72]);
var plan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,backtrack);
int guards=0,words=0,emptyCallbacks=0;var readingOrderTexts=new List<byte[]>();
using(var output=File.Create(args[6]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();
    if(inputTrackFingerprint is not null){w.WriteNumber("recordedIndex",int.Parse(args[10]));w.WriteString("trackSha256",inputTrackFingerprint);w.WriteString("penUpMetadata","absent");}
    w.WriteStartArray("starts");foreach(int start in compact.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<owner.SourceGraph.NodeCount;node++){w.WriteStartArray();foreach(var arc in owner.SourceGraph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var group in owner.SourceGraph.Groups){w.WriteStartArray();foreach(var range in group.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<32;n++)
    {
        var ownership=LegacyFreshMainPlainResultOwnership.Create(compact,geometry);
        byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*37+n)).ToArray();
        foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
        var session=LegacyOwnedMainGlyphFlushSession.Prepare(ownership,plan,residue);
        var publisher=LegacyOwnedMainPlainWordPublication.Prepare(ak,session,table,callbacks,interior);
        byte[] record=Enumerable.Range(0,40).Select(i=>(byte)(i*17+n)).ToArray();
        ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(i*13+n)).ToArray();
        byte[] request=Enumerable.Range(0,64).Select(i=>(byte)(i*19+n)).ToArray();
        var textsByNode=new Dictionary<LegacyNativePublishedWordNode,byte[]>();
        w.WriteStartArray();
        while(!session.IsComplete)
        {
            var association=session.PrepareNext();var publicationResult=publisher.Publish(association,record,labels,request);
            if(!publicationResult.Succeeded)throw new Exception("Actual CASE word callback failed.");
            var projection=LegacyOwnedMainWordUtf8Projection.Project(publicationResult);
            if(!ReferenceEquals(projection.Publication,publicationResult))throw new Exception("Projection lost owning callback.");
            byte[] snapshot=projection.CopyUtf8();var copy=projection.CopyUtf8();
            if(copy.Length>0){copy[0]^=255;if(copy.AsSpan().SequenceEqual(projection.CopyUtf8()))throw new Exception("UTF8 escaped snapshot.");}
            if(!System.Text.Encoding.UTF8.GetBytes(projection.Text).AsSpan().SequenceEqual(snapshot))throw new Exception("Text changed raw UTF8.");
            w.WriteStartObject();w.WriteBase64String("associatedRows",association.CopyRows72());w.WriteBase64String("utf8",snapshot);w.WriteString("text",projection.Text);
            if(publicationResult.PublishedWord is {} node)
            {
                words++;textsByNode.Add(node,snapshot);w.WriteBase64String("word120",node.Word.CopyStorage());
                w.WriteStartArray("references");foreach(ulong reference in node.Word.FlatReferences)w.WriteNumberValue(reference);w.WriteEndArray();
                w.WriteStartArray("pool");foreach(ulong reference in node.Word.FlatReferences){w.WriteStartObject();w.WriteNumber("reference",reference);w.WriteBase64String("record",node.Word.Pool.CopyRecord(reference));w.WriteEndObject();}w.WriteEndArray();
            }
            else{emptyCallbacks++;w.WriteNull("word120");}
            w.WriteStartArray("glyphs");foreach(var glyph in projection.Glyphs)
            {
                w.WriteStartObject();w.WriteNumber("reference",glyph.Reference);w.WriteNumber("key",glyph.CanonicalSourceKey);w.WriteBoolean("protocol",glyph.IsProtocolSource);
                if(glyph.OriginalGraphSourceKey is ulong original)w.WriteNumber("original",original);else w.WriteNull("original");
                w.WriteNumber("kindFlags",glyph.Descriptor.KindFlags);w.WriteNumber("symbol",glyph.Descriptor.SymbolOrRunLength);
                w.WriteBase64String("record",glyph.CopyRecord40());w.WriteBase64String("utf8",glyph.CopyUtf8());
                w.WriteStartArray("rows");foreach(int row in glyph.CopyAssociatedRowOrdinals())w.WriteNumberValue(row);w.WriteEndArray();
                w.WriteStartArray("ranges");foreach(var range in glyph.CopySampledSourceRanges()){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
                byte[] mutation=glyph.CopyRecord40();mutation[8]^=255;if(mutation.AsSpan().SequenceEqual(glyph.CopyRecord40()))throw new Exception("Glyph record escaped snapshot.");
                var rows=glyph.CopyAssociatedRowOrdinals();rows[0]=-1;if(glyph.CopyAssociatedRowOrdinals()[0]==-1)throw new Exception("Glyph row map escaped.");
                var ranges=glyph.CopySampledSourceRanges();if(ranges.Length>0){ranges[0]=new(999,0,1);if(glyph.CopySampledSourceRanges()[0].StrokeIndex==999)throw new Exception("Glyph range escaped.");}
            }
            w.WriteEndArray();w.WriteEndObject();
            Reject(()=>LegacyOwnedMainWordUtf8Projection.Project(null!));Reject(()=>LegacyOwnedMainWordUtf8Projection.Project(publicationResult,new(true)));
            session.CompleteCurrent(publicationResult);
            //The typed published-word snapshot can be read after acknowledgement;
            //no new winning label or mutable working WORD is substituted.
            if(!LegacyOwnedMainWordUtf8Projection.Project(publicationResult).CopyUtf8().AsSpan().SequenceEqual(snapshot))throw new Exception("Completed callback projection changed.");
        }
        w.WriteEndArray();
        using var finalBytes=new MemoryStream();
        for(var node=ownership.WordPublisher.Head;node is not null;node=node.Next)finalBytes.Write(textsByNode[node]);
        readingOrderTexts.Add(finalBytes.ToArray());
    }
    w.WriteEndArray();w.WriteStartArray("readingOrderUtf8");foreach(byte[] text in readingOrderTexts)w.WriteBase64StringValue(text);w.WriteEndArray();
    w.WriteNumber("guards",guards);w.WriteNumber("words",words);w.WriteNumber("emptyCallbacks",emptyCallbacks);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS actualCASE WORD275->{words} raw UTF8/source-key projections/{emptyCallbacks} zero-span callbacks/{guards} guards; no expected transcription or document endpoints.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid UTF8 projection accepted.");}
