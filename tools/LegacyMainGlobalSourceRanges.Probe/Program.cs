using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
int cases=0,queries=0,guards=0,cross=0;using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var result=LegacyFreshMainGlobalSourceRanges.Prepare(graph);
    if(!ReferenceEquals(result.Graph,graph))throw new Exception("Compact graph identity changed.");
    var chronology=cuts.Chronology;var original=cuts.Preparation.Input.Input.Input.Owner.Controller.Source;
    var keys=new LegacySourcePointKey[original.Strokes.Sum(s=>s.Points.Count)];
    foreach(int s in Enumerable.Range(0,original.Strokes.Count))for(int p=0;p<original.Strokes[s].Points.Count;p++){var id=original.Chronology.Resolve(s,p);keys[id.KeyRecordIndex]=id.Key;}
    w.WriteStartObject();w.WriteStartArray("keys");foreach(var key in keys)Key(key);w.WriteEndArray();w.WriteStartArray("metadata");
    for(int s=0;s<chronology.StrokePointCounts.Length;s++){w.WriteStartArray();for(int p=0;p<chronology.StrokePointCounts.Span[s];p++){var id=chronology.Resolve(s,p);var end=original.ResolveEndingMetadata(id);w.WriteStartArray();Key(id.Key);Key(end.Key);w.WriteNumberValue(id.KeyRecordIndex);w.WriteNumberValue(id.RemainderByte);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");foreach(var group in graph.State.Groups){w.WriteStartArray();foreach(var r in group.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("nodes");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteNumberValue(arc.Span);w.WriteStartArray();foreach(int a in arc.CopySelector(1))w.WriteNumberValue(a);w.WriteEndArray();w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("queries");for(int n=0;n<graph.State.NodeCount;n++)for(int a=0;a<graph.State.GetNode(n).Count;a++)
    {
        var arc=graph.State.GetNode(n)[a];var refs=result.ResolveCandidateWithResiduals(n,a);if(refs.Length==0)throw new Exception("Actual range path missing.");
        if(graph.ResolveGroupSource((int)arc.GroupId).Secondary is not null)cross++;
        w.WriteStartObject();w.WriteNumber("first",arc.StartContextId);w.WriteNumber("last",arc.EndContextId);w.WriteStartArray("ranges");foreach(var r in refs){w.WriteStartArray();Key(r.First);Key(r.Last);w.WriteNumberValue(r.FirstResidual);w.WriteNumberValue(r.LastResidual);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();queries++;
    }w.WriteEndArray();w.WriteEndObject();cases++;
    Reject(()=>LegacyFreshMainGlobalSourceRanges.Prepare(graph,new(true)));Reject(()=>result.ResolveCandidate(-1,0));Reject(()=>result.ResolveForwardWithResiduals(uint.MaxValue,0));Reject(()=>result.ResolveCandidate(0,0,new(true)));
}
w.WriteEndArray();w.Flush();if(cross==0)throw new Exception("Actual cross-descriptor references not exercised.");Console.WriteLine($"PASS {cases} actual global owners/{queries} source queries/{cross} cross arcs/{guards} guards; no global context/NN claim.");
void Key(LegacySourcePointKey k){w.WriteStartArray();w.WriteNumberValue(k.First);w.WriteNumberValue(k.Second);w.WriteNumberValue(k.Third);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException or OperationCanceledException){guards++;return;}throw new Exception("Invalid global source request accepted.");}
