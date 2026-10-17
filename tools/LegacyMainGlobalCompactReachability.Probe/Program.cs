using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

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
int owners=0,guards=0;ulong identity=0x1000;
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
    Reject(()=>LegacyFreshMainGlobalCompactReachability.Prepare(new byte[ak.Length],selected));
    Reject(()=>LegacyFreshMainGlobalCompactReachability.Prepare(ak,selected,new(true)));
    var marked=LegacyFreshMainGlobalCompactReachability.Prepare(ak,selected);
    w.WriteStartObject();w.WriteNumber("primary",policy.Configuration.PrimaryArcLimit230a);
    w.WriteNumber("prefixEnd",marked.FinalizedPrefixEnd);w.WriteNumber("checks",marked.QueryChecks);w.WriteNumber("backendCalls",marked.BackendCalls);
    w.WriteBase64String("selection",marked.CopySelectedNodes());w.WriteStartArray("nodes");
    for(int node=0;node<result.NodeCount;node++)
    {
        var before=result.GetNodeSnapshot(node);var after=marked.GetNodeSnapshot(node);
        w.WriteStartObject();w.WriteNumber("sourceEnd",before.SourceEnd23f0);
        w.WriteBase64String("inputRows",before.Records.Span);w.WriteBase64String("inputAlternatives",before.PrimaryAlternatives.Span);
        w.WriteBase64String("rows",after.Records.Span);w.WriteBase64String("alternatives",after.PrimaryAlternatives.Span);w.WriteEndObject();
        byte[] expected=after.Records.ToArray();byte[] escaped=MemoryMarshal.TryGetArray(after.Records,out var segment)?segment.Array!:throw new InvalidDataException("Snapshot array absent.");
        escaped.AsSpan().Clear();if(!marked.GetNodeSnapshot(node).Records.Span.SequenceEqual(expected))
            throw new InvalidDataException("Mutable global marking snapshot escaped.");
    }
    w.WriteEndArray();w.WriteStartArray("continuations");int continuationRows=0,positives=0;
    for(int node=0;node<result.NodeCount;node++)for(ushort ordinal=0;ordinal<Math.Min(result.GetNodeSnapshot(node).SourceEnd23f0,policy.Configuration.PrimaryArcLimit230a);ordinal++)
    {
        var row=marked.GetNodeSnapshot(node).Records.Span.Slice(ordinal*88,88);
        if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)continue;
        bool answer=marked.HasLanguageContinuation(node,ordinal);if(answer)positives++;continuationRows++;
        w.WriteStartArray();w.WriteNumberValue(node);w.WriteNumberValue(ordinal);w.WriteBooleanValue(answer);w.WriteEndArray();
    }
    w.WriteEndArray();w.WriteNumber("continuationChecks",marked.QueryChecks);w.WriteNumber("continuationBackendCalls",marked.BackendCalls);
    if(owner.AcousticEvaluations!=evaluations)throw new InvalidDataException("Reachability recalculated acoustic networks.");
    Reject(()=>marked.GetNodeSnapshot(-1));Reject(()=>marked.GetNodeSnapshot(result.NodeCount));
    Reject(()=>marked.HasLanguageContinuation(-1,0));Reject(()=>marked.HasLanguageContinuation(0,0,new(true)));
    result.ValidateResultQueryLease(default);
    Reject(()=>owner.AdvanceNodeEpoch(result.NodeCount));Reject(()=>marked.HasLanguageContinuation(0,0));
    w.WriteEndObject();Console.WriteLine($"PASS global marking owner{owners}/{result.NodeCount} nodes/{continuationRows} actual207780 consumer rows/{positives} continuations; NN count unchanged.");owners++;
}
w.WriteEndArray();Console.WriteLine($"PASS{owners} global reachability owners/{guards} guards; compact allocation and text publication remain separate.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Global marking guard failed.");}
