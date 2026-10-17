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

    var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
    byte[] residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,null!,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,[]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,new byte[41]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(new byte[ak.Length],selected,confidence,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue,new(true)));
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue);
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue));

    byte[] compact=materialized.CopyCompactStorage();var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);
    var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
    w.WriteStartObject();w.WriteNumber("recordCount",materialized.RecordCount);w.WriteNumber("selected",materialized.SelectedRecordIndex);
    w.WriteNumber("alternative",ushort.MaxValue);w.WriteBase64String("compact",compact);
    w.WriteStartArray("starts");foreach(int v in materialized.CopyNodeStarts2478())w.WriteNumberValue(v);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<pass.NodeCount;n++){
        w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){
            w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();
        }w.WriteEndArray();}w.WriteEndArray();
    int count=0;w.WriteStartArray("controls");for(int control=0;control<128;control++){
        byte[] residue72=Enumerable.Range(0,72).Select(i=>(byte)(control*73+i*31+11)).ToArray();
        var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,residue72);
        byte[] emitted=walk.CopyRows();count+=walk.Count;
        w.WriteStartObject();w.WriteBase64String("residue",residue72);w.WriteBase64String("rows",emitted);
        w.WriteStartArray("visited");foreach(int v in walk.CopyVisitedCompactIndices())w.WriteNumberValue(v);w.WriteEndArray();
        w.WriteStartArray("residuals");foreach(ulong v in walk.CopyResidualSourceReferences())w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();
        Array.Clear(walk.CopyRows());Array.Clear(walk.CopyVisitedCompactIndices());Array.Clear(walk.CopyResidualSourceReferences());
        if(!walk.CopyRows().AsSpan().SequenceEqual(emitted))throw new InvalidDataException("Backtrack snapshot escaped.");
    }w.WriteEndArray();int before=guards;
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,null!,callbacks,new byte[72]));
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,null!,new byte[72]));
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[71]));
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72],new(true)));
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(new byte[ak.Length],materialized,callbacks,new byte[72]));
    var foreignGeometry=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner));
    Reject(()=>LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,foreignGeometry),new byte[72]));
    w.WriteNumber("guards",guards-before);w.WriteNumber("rowCount",count);w.WriteEndObject();
    if(owner.AcousticEvaluations!=evaluations)throw new InvalidDataException("Backtracking reran neural networks.");
    Console.WriteLine($"PASS global backtrack owner{owners}/{pass.NodeCount} nodes/128 stack residues/{count} rows, NN unchanged.");owners++;
}
w.WriteEndArray();Console.WriteLine($"PASS{owners} global compact-to-backtrack owners/{guards} guards; glyph finalization remains separate.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Global backtrack guard failed.");}
