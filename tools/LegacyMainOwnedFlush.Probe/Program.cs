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
var geometry=lineContext is null?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.Source):
    LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.MainLineSource);
var resultOwnership=LegacyFreshMainPlainResultOwnership.Create(compact,geometry);
int guards=0,associated=0;
var baseWalk=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,new byte[72]);
var basePlan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,baseWalk);
using(var output=File.Create(args[6]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteNumber("recordCount",compact.RecordCount);w.WriteNumber("selected",compact.SelectedRecordIndex);
    w.WriteNumber("alternative",compact.SelectedAlternative);w.WriteBase64String("compact",compact.CopyCompactStorage());
    w.WriteNumber("totalCost",terminal.Winner.TotalCost);w.WriteNumber("rowCost14",terminal.Winner.RowCost14);
    w.WriteStartArray("cumulative");foreach(int value in result.CopyCumulativePathCosts())w.WriteNumberValue(value);w.WriteEndArray();
    w.WriteStartArray("starts");foreach(int start in compact.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<owner.SourceGraph.NodeCount;node++)
    {w.WriteStartArray();foreach(var arc in owner.SourceGraph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("facts");for(int node=0;node<geometry.Graph.NodeCount;node++)
    {w.WriteStartArray();for(ushort arc=0;arc<geometry.Graph.GetNode(node).Count;arc++)
        {var b=geometry.ReadBounds(node,arc);w.WriteStartArray();w.WriteNumberValue(b.MinimumX);w.WriteNumberValue(b.MinimumY);w.WriteNumberValue(b.MaximumX);w.WriteNumberValue(b.MaximumY);
            w.WriteNumberValue(geometry.ReadLine(node,arc));w.WriteBooleanValue(geometry.ReadTopMark(node,arc));w.WriteNumberValue(geometry.ReadKind(node,arc));w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    var actualGuide=geometry.ReadGuide(geometry.ReadLine(0,0));w.WriteStartArray("guide");w.WriteNumberValue(actualGuide.Baseline);w.WriteNumberValue(actualGuide.Height);w.WriteEndArray();
    w.WriteStartArray("alphabetFlags");for(ushort label=0;label<model.AcousticAlphabet.LabelCount;label++)w.WriteBooleanValue(geometry.ReadAlphabetFlag(label));w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var group in geometry.Graph.Groups){w.WriteStartArray();foreach(var range in group.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<64;n++)
    {
        byte[] residue72=Enumerable.Range(0,72).Select(i=>(byte)(n*73+i*31+11)).ToArray();
        var walk=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,residue72);
        var plan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,walk);
        byte[] residue=Residue(n);
        var session=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,residue);
        byte[] initial=session.CopyInitialWordStorage();
        byte[] copy=session.CopyInitialWordStorage();copy[68]^=255;
        if(copy.AsSpan().SequenceEqual(session.CopyInitialWordStorage()))throw new Exception("WORD seed escaped ownership.");
        if(n==0)
        {
            Reject(()=>LegacyOwnedMainGlyphFlushSession.Prepare(null!,plan,residue));
            Reject(()=>LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,null!,residue));
            Reject(()=>LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,new byte[119]));
            Reject(()=>LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,Enumerable.Repeat((byte)255,120).ToArray()));
            Reject(()=>LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,residue,new(true)));
            Reject(()=>session.PrepareNext(new(true)));
            Reject(()=>session.CompleteCurrent(null!));
            Reject(()=>session.CompletePrepared(true));
        }
        w.WriteStartObject();w.WriteBase64String("residue72",residue72);w.WriteBase64String("residue120",residue);
        w.WriteBase64String("initialWord",initial);w.WriteStartArray("flushes");
        LegacyOwnedMainAssociatedGlyphFlush? previous=null;
        while(!session.IsComplete)
        {
            byte[] before=session.WorkingWord.CopyStorage();
            var association=session.PrepareNext();associated++;
            if(!ReferenceEquals(association.Owner,session)||!ReferenceEquals(association.Flush,plan.Flushes[association.Ordinal])||
                !ReferenceEquals(association.WorkingWord.Pool,resultOwnership.GlyphPool)||!association.Succeeded)
                throw new Exception("Actual flush/pool/WORD source binding changed.");
            w.WriteStartObject();w.WriteNumber("ordinal",association.Ordinal);w.WriteBase64String("beforeWord",before);
            w.WriteBase64String("packet",association.Packet.CopyStorage());w.WriteBase64String("rows",association.CopyRows72());
            w.WriteBase64String("appliedWord",association.WorkingWord.CopyStorage());w.WriteNumber("remaining",association.RemainingCount);
            w.WriteStartArray("residualStorage");foreach(ulong value in association.CopyResidualStorage())w.WriteNumberValue(value);w.WriteEndArray();
            if(n==0)
            {
                Reject(()=>session.PrepareNext());
                var foreign=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,residue);var foreignAssociation=foreign.PrepareNext();
                Reject(()=>session.CompleteCurrent(new LegacyOwnedMainWordPublicationResult(foreignAssociation,true,null)));
                Reject(()=>session.ValidateCurrent(foreignAssociation));Reject(()=>session.AbortPublication(foreignAssociation));
                if(previous is not null)Reject(()=>session.CompleteCurrent(new LegacyOwnedMainWordPublicationResult(previous,true,null)));
                association.BeginPublication();Reject(()=>association.BeginPublication());
            }
            byte[] rowsCopy=association.CopyRows72();rowsCopy[0]^=255;
            if(rowsCopy.AsSpan().SequenceEqual(association.CopyRows72()))throw new Exception("Associated rows escaped.");
            session.CompletePrepared(true);
            w.WriteBase64String("resetWord",session.WorkingWord.CopyStorage());w.WriteEndObject();previous=association;
        }
        if(session.IsStopped||session.CompletedFlushCount!=plan.Flushes.Count)throw new Exception("Successful prepared controls failed to advance.");
        if(n==0)
        {
            Reject(()=>session.PrepareNext());Reject(()=>session.CompletePrepared(true));
            var failed=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,residue);var prefix=failed.PrepareNext();
            byte[] committed=prefix.WorkingWord.CopyStorage();failed.CompletePrepared(false);
            if(!failed.IsStopped||!committed.AsSpan().SequenceEqual(prefix.WorkingWord.CopyStorage()))throw new Exception("Prepared callback failure rolled back packet writes.");
            Reject(()=>failed.PrepareNext());Reject(()=>failed.CompletePrepared(true));
            if(!committed.AsSpan().SequenceEqual(prefix.WorkingWord.CopyStorage()))throw new Exception("Rejected retry changed a committed failure prefix.");
            var aborted=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,plan,residue);var abortedPrefix=aborted.PrepareNext();
            aborted.AbortPublication(abortedPrefix);Reject(()=>aborted.PrepareNext());Reject(()=>aborted.ValidateCurrent(abortedPrefix));
        }
        w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteStartArray("preparedResidualControls");
    var allRefs=Enumerable.Range(0,geometry.Graph.NodeCount).SelectMany(node=>Enumerable.Range(0,geometry.Graph.GetNode(node).Count).Select(arc=>(uint)node|((ulong)arc<<24))).ToArray();
    for(int n=0;n<256;n++)
    {
        var preparedSession=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,basePlan,Residue(n));
        byte[] preparedRows=basePlan.Flushes[n%5==0?basePlan.Flushes.Count-1:0].CopyRows72();
        ulong[] references=Enumerable.Range(0,1+n%7).Select(i=>allRefs[(n*13+i*11)%allRefs.Length]).ToArray();
        var outcome=preparedSession.AssociatePreparedForControls(preparedRows,references);
        w.WriteStartObject();w.WriteBase64String("before",preparedRows);w.WriteBase64String("after",outcome.CopyRows());
        w.WriteBoolean("success",outcome.Succeeded);w.WriteNumber("remaining",outcome.RemainingCount);
        w.WriteStartArray("references");foreach(ulong value in references)w.WriteNumberValue(value);w.WriteEndArray();
        w.WriteStartArray("storage");foreach(ulong value in outcome.CopyResidualStorage())w.WriteNumberValue(value);w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();
    var guardSession=LegacyOwnedMainGlyphFlushSession.Prepare(resultOwnership,basePlan,Residue(0));
    Reject(()=>guardSession.AssociatePreparedForControls(basePlan.Flushes[0].CopyRows72(),new[]{ulong.MaxValue}));
    Reject(()=>guardSession.AssociatePreparedForControls(basePlan.Flushes[0].CopyRows72(),new[]{allRefs[0]},new(true)));
    w.WriteNumber("guards",guards);w.WriteNumber("associations",associated);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS64 actual owned WORD seeds/{associated} per-flush217DB0 calls/{guards} ownership, callback and cancellation guards. Callback outcomes explicitly prepared; no accepted-word claim.");
byte[] Residue(int n)
{
    byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(n*13+i*29+7)).ToArray();
    foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
    return residue;
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new Exception("Invalid owned flush transition accepted.");}
