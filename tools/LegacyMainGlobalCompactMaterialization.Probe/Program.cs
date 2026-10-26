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
var globalInputCapture=new GlobalPublicationInputCapture(ak,lk,args[3],args[4]);
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);ulong id=++identity;globalInputCapture.Observe(r,id,bytes);return new(bytes,id);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
    byte[] residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,null!,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,[]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,new byte[41]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(new byte[ak.Length],selected,confidence,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue,new(true)));
    using var allocatorFrame=globalInputCapture.Begin();
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue);
    var allocatorInputs=allocatorFrame.Complete();
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue));
    byte[] compact=materialized.CopyCompactStorage();int[] starts=materialized.CopyNodeStarts2478();
    w.WriteStartObject();globalInputCapture.Write(w,allocatorInputs,policy.Configuration.PrimaryAlternativeCount2402);w.WriteNumber("factory",0x292);w.WriteNumber("maximumAtoms",graph.State.MaximumAtoms);
    w.WriteNumber("intermediatePublicationDistance",policy.MaximumParentDistance54);
    w.WriteNumber("count",materialized.RecordCount);w.WriteNumber("selected",materialized.SelectedRecordIndex);
    w.WriteNumber("winner",selected.Winner.Ordinal);w.WriteNumber("primary",policy.Configuration.PrimaryArcLimit230a);
    w.WriteNumber("penalty",terminal.LanguagePenalty2534);w.WriteBase64String("compact",compact);
    w.WriteStartArray("starts");foreach(int v in starts)w.WriteNumberValue(v);w.WriteEndArray();
    w.WriteStartArray("primaryMap");foreach(ushort v in owner.Symbols.Symbols.Symbols.CopyPrimary())w.WriteNumberValue(v);w.WriteEndArray();
    w.WriteStartArray("inputNodes");for(int n=0;n<pass.NodeCount;n++)WriteNode(pass.GetNodeSnapshot(n));w.WriteEndArray();
    w.WriteStartArray("nodes");for(int n=0;n<pass.NodeCount;n++)WriteNode(materialized.GetNodeSnapshot(n));w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<pass.NodeCount;n++){
        w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){
            w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();
        }w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("contexts");foreach(var n in owner.Contexts.CopyNodes()){
        w.WriteStartArray();w.WriteNumberValue(n.Identity);w.WriteNumberValue(n.ParentLink);w.WriteNumberValue(n.Symbol);w.WriteEndArray();
    }w.WriteEndArray();
    w.WriteStartArray("scores");for(int g=0;g<graph.State.Groups.Count;g++){
        var score=confidence.ReadGroup(g);w.WriteStartObject();w.WriteStartArray("candidates");foreach(var c in score.Candidates.Span){
            w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteEndArray();
        }w.WriteEndArray();w.WriteNumber("scaleBits",BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
        w.WriteStartArray("costs");foreach(int v in score.CopyFinalClassCosts())w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteEndObject();
    Array.Clear(materialized.CopyCompactStorage());Array.Clear(materialized.CopyNodeStarts2478());
    if(!materialized.CopyCompactStorage().AsSpan().SequenceEqual(compact)||!materialized.CopyNodeStarts2478().AsSpan().SequenceEqual(starts))throw new InvalidDataException("Mutable compact snapshot escaped.");
    Reject(()=>materialized.GetNodeSnapshot(-1));Reject(()=>materialized.GetNodeSnapshot(pass.NodeCount));
    for(int n=0;n<pass.NodeCount;n++){
        var snapshot=materialized.GetNodeSnapshot(n);byte[] row=snapshot.Records.ToArray(),primary=snapshot.PrimaryAlternatives.ToArray(),secondary=snapshot.SecondaryAlternatives.ToArray();
        foreach(var memory in new[]{snapshot.Records,snapshot.PrimaryAlternatives,snapshot.SecondaryAlternatives})if(MemoryMarshal.TryGetArray(memory,out var segment))Array.Clear(segment.Array!);
        var repeated=materialized.GetNodeSnapshot(n);if(!repeated.Records.Span.SequenceEqual(row)||!repeated.PrimaryAlternatives.Span.SequenceEqual(primary)||!repeated.SecondaryAlternatives.Span.SequenceEqual(secondary))throw new InvalidDataException("Mutable node snapshot escaped.");
    }
    if(owner.AcousticEvaluations!=evaluations)throw new InvalidDataException("Compact publication recalculated neural networks.");
    Console.WriteLine($"PASS global compact owner{owners}/{pass.NodeCount} nodes/{materialized.RecordCount} records, selected{materialized.SelectedRecordIndex}, NN unchanged.");owners++;
}
w.WriteEndArray();Console.WriteLine($"PASS{owners} global compact owners/{guards} guards. Text publication remains separate.");
void WriteNode(LegacyMainDirectWordNodeResult n){w.WriteStartObject();w.WriteNumber("sourceEnd",n.SourceEnd23f0);w.WriteBase64String("rows",n.Records.Span);w.WriteBase64String("alternatives",n.PrimaryAlternatives.Span);w.WriteEndObject();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Global compact guard failed.");}
