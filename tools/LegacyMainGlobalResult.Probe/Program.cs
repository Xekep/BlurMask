using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0,groups=0,candidates=0,guards=0,queryRows=0;ulong identity=0x1000;
LegacyFreshMainGlobalWordPassResult? previousResult=null;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);
    Reject(()=>confidence.ReadGroup(0));Reject(()=>owner.CaptureResultQueryLease(default));
    var result=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(result);
    if(previousResult is not null)Reject(()=>terminal.Select(previousResult));
    result.ValidateResultQueryLease(default);
    if(!ReferenceEquals(selected.PassResult,result)||!ReferenceEquals(selected.Invocation,terminal))throw new Exception("Global terminal owner changed.");
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("nodes",result.NodeCount);w.WriteNumber("primaryLimit",policy.Configuration.PrimaryArcLimit230a);
    var resultLease=ProbeSnapshots.ReadLease(result);
    var retainedMap=resultLease.DictionaryLease.Map;
    Ints("retainedWorkingMap",retainedMap.CopySelectedMap(false,0).Select(v=>(int)v));
    Ints("retainedPreservedMap",retainedMap.CopySelectedMap(false,0x40).Select(v=>(int)v));
    Ints("acousticMask",owner.Symbols.CopyMasks().Primary.ToArray().Select(v=>(int)v));
    w.WriteStartArray("initialContexts");foreach(ulong v in owner.Symbols.Symbols.Symbols.CopyInitialContexts())w.WriteNumberValue(v);w.WriteEndArray();
    w.WriteNumber("caseVersion",model.LanguageModel.CasePathVersion);
    var preparation=owner.Symbols.Symbols.Preparation;w.WriteNumber("caseBoundary",preparation.Ranges.LastCasedBoundary);
    w.WriteStartArray("caseRows");foreach(ushort symbol in Enumerable.Range(0,192).Concat(Enumerable.Range(65507,29)).Select(v=>(ushort)v))
    {w.WriteStartArray();w.WriteNumberValue(symbol);w.WriteNumberValue(preparation.ReadClassification(symbol));w.WriteNumberValue(preparation.ReadClassification(symbol,true));Forms(false);Forms(true);w.WriteEndArray();
     void Forms(bool alternate){if(!preparation.TryReadForms(symbol,alternate,out var forms)){w.WriteNullValue();return;}w.WriteStartArray();w.WriteNumberValue(forms.First);w.WriteNumberValue(forms.Second);w.WriteNumberValue(forms.Third);w.WriteEndArray();}}
    w.WriteEndArray();
    w.WriteNumber("penalty",terminal.LanguagePenalty2534);w.WriteNumber("cumulative",result.FinalCumulativeCost);w.WriteString("finalRecords",Convert.ToHexString(result.FinalRecords));w.WriteNumber("sourceEnd",result.FinalSourceEnd);
    w.WriteStartArray("sourceHeaders");foreach(var header in terminal.CopySources())w.WriteNumberValue(header.Header);w.WriteEndArray();
    var winner=selected.Winner;w.WriteStartArray("winner");w.WriteBooleanValue(winner.HasWinner);w.WriteNumberValue(winner.Ordinal);w.WriteNumberValue(winner.RelativeCost);w.WriteNumberValue(winner.TotalCost);w.WriteNumberValue(winner.RowCost14);w.WriteNumberValue(winner.RowField22);w.WriteNumberValue(winner.FallbackFlag);w.WriteNumberValue(winner.ResultFlag8);w.WriteEndArray();
    byte[] copy=selected.CopySelectedRecord();if(copy.Length>0){copy[0]^=1;if(copy.AsSpan().SequenceEqual(selected.CopySelectedRecord()))throw new Exception("Terminal selected row escaped.");}
    w.WriteStartArray("groups");
    foreach(int group in Enumerable.Range(0,graph.State.NodeCount).SelectMany(n=>graph.State.GetNode(n)).Select(a=>checked((int)a.GroupId)).Distinct().Order())
    {
        var value=confidence.ReadGroup(group);if(!ReferenceEquals(value,confidence.ReadGroup(group)))throw new Exception("Confidence pool recalculated a group.");
        if(!ReferenceEquals(value.Owner,confidence)||value.GroupIndex!=group||!ReferenceEquals(confidence.Graph,graph)||confidence.OwnerScoreChannel88!=0)throw new Exception("Actual global confidence ownership mismatch.");
        var entries=value.Candidates.Span;groups++;candidates+=entries.Length;
        if(MemoryMarshal.TryGetArray(value.Candidates,out var escaped)&&entries.Length>0){var c=entries[0];escaped.Array![escaped.Offset]=new(c.Symbol,c.Cost,-100);if(value.ReadSymbolScore(c.Symbol)!=c.Confidence)throw new Exception("Candidate public memory escaped.");}
        w.WriteStartObject();w.WriteNumber("group",group);w.WriteNumber("sourceProbabilityBits",BitConverter.SingleToInt32Bits(value.FirstStoredSourceProbability));
        var normalized=geometry.NormalizeGroup(group);var primitive=geometry.ReadPrimitiveScope(group);
        w.WriteStartArray("normalizedCacheInput");foreach(var stroke in normalized.Strokes){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
        w.WriteBoolean("whole",primitive.BoundaryAlignedSingleNormalizedStroke);w.WriteBoolean("zero",primitive.EndpointFieldsAreZero);
        w.WriteStartArray("probabilities");for(int family=0;family<4;family++){var vector=value.CopyFamilyProbabilities(family);w.WriteStartArray();foreach(int p in vector)w.WriteNumberValue(p);w.WriteEndArray();vector[0]^=1;if(vector[0]==value.CopyFamilyProbabilities(family)[0])throw new Exception("Private family vector escaped.");}w.WriteEndArray();
        w.WriteStartArray("rawCosts");foreach(int cost in value.CopyRawIntegerCosts())w.WriteNumberValue(cost);w.WriteEndArray();
        w.WriteStartArray("finalCosts");foreach(int cost in value.CopyFinalClassCosts())w.WriteNumberValue(cost);w.WriteEndArray();
        w.WriteStartArray("candidates");foreach(var c in entries){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToInt32Bits(c.Confidence));w.WriteNumberValue(BitConverter.SingleToInt32Bits(value.ReadNormalizedCandidateWeight(c.Symbol)));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
        if(value.ReadFirstCandidateScore()!=(entries.IsEmpty?0:entries[0].Confidence)||value.ReadSymbolScore(65535)!=0||value.ReadNormalizedCandidateWeight(65535)!=0)throw new Exception("Native score channels changed.");
        var rawCopy=value.CopyRawIntegerCosts();if(rawCopy.Length>0){rawCopy[0]^=1;if(rawCopy[0]==value.CopyRawIntegerCosts()[0])throw new Exception("Raw integer cost snapshot escaped.");}
    }
    w.WriteEndArray();
    //These queries are explicit prepared publication-consumer controls on
    //the actual retained map, not a restored207780 request producer.
    byte[] first=ReadAll(),second=ReadAll();if(!first.AsSpan().SequenceEqual(second))throw new Exception("Retained-map repeated result query changed output.");
    w.WriteString("queryRows",Convert.ToHexString(first));queryRows+=first.Length/32;
    w.WriteStartArray("contextNodes");foreach(var n in owner.Contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(n.Identity);w.WriteNumberValue(n.ParentLink);w.WriteNumberValue(n.Symbol);w.WriteEndArray();}w.WriteEndArray();
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Publication/confidence evaluated replacement networks.");
    w.WriteNumber("acousticEvaluations",evaluations);w.WriteEndObject();
    Reject(()=>confidence.ReadGroup(-1));Reject(()=>confidence.ReadGroup(graph.State.Groups.Count));Reject(()=>confidence.ReadGroup(0,new(true)));
    Reject(()=>LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner,new(true)));Reject(()=>terminal.Select(result,new(true)));
    Reject(()=>result.ReadResultQuery(8UL<<40,new(){Capacity=1,ContextRoot=owner.Contexts.ContainerIdentity+1},false,default));
    result.ValidateResultQueryLease(default);
    //After a result query changed slot revision, a second capture must not
    //pretend that the native final V80 reset occurred again.
    Reject(()=>owner.CaptureResultQueryLease(default));result.ValidateResultQueryLease(default);
    Reject(()=>owner.AdvanceNodeEpoch(result.NodeCount));Reject(()=>result.ValidateResultQueryLease(default));
    Reject(()=>terminal.Select(result));Reject(()=>confidence.ReadGroup(0));
    previousResult=result;
    Console.WriteLine($"PASS global result owner{owners}: {result.NodeCount} nodes/{evaluations} unique NN, numeric winner={winner.Ordinal}, prepared retained-map rows={first.Length/32}; no replacementNN/text claim.");owners++;
    byte[] ReadAll()
    {
        var q=new LegacyPreparedDictionaryQuery{Capacity=7,ContextRoot=owner.Contexts.ContainerIdentity,Parameter1c=(uint)(result.NodeCount-1),Parameter20=0,Flags24=0x100};
        using var bytes=new MemoryStream();
        for(int page=0;page<256;page++){var value=result.ReadResultQuery(8UL<<40,q,false,default);if(value.Records.Length!=value.ReturnedCount*32)throw new Exception("Result query32 extent changed.");if(value.ReturnedCount==0)return bytes.ToArray();bytes.Write(value.Records.Span);q.Skip+=value.ReturnedCount;}
        throw new Exception("Prepared publication query budget exceeded.");
    }
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {owners} actual global owners/{groups} confidence groups/{candidates} candidate scores/{queryRows} prepared result-query rows/{guards} guards; no text/publication/accuracy claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid global result scope accepted.");}
void Ints(string name,IEnumerable<int> values){w.WriteStartArray(name);foreach(int value in values)w.WriteNumberValue(value);w.WriteEndArray();}
static class ProbeSnapshots
{
    // Read-only diagnostic access to the SAME immutable installed map retained
    // by the completed result. Never recapture/refresh/replace authorization.
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="resultQueries")]
    internal static extern ref LegacyFreshMainGlobalCaseSession.ResultQueryLease ReadLease(LegacyFreshMainGlobalWordPassResult result);
}
