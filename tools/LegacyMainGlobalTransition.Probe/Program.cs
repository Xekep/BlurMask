using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

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
int owners=0,arcs=0,parity=0,cross=0,guards=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);
    LegacyFreshMainSingleLineSampled? old=null;LegacyMainWordTransitionGeometry? oldCache=null;LegacyMainBlankTransitionGeometry? oldBlank=null;
    if(local.Steps.Count==1){old=LegacyFreshMainSingleLineSampler.Prepare(ak,input.Smoothed).Sample(table);oldCache=old.CreatePrepared240TransitionGeometry(table);oldBlank=new(ak,old,table);}
    w.WriteStartObject();w.WriteNumber("sourceGap",cuts.Preparation.SourceGap);w.WriteStartArray("points");foreach(var s in cuts.CopyStrokes()){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");for(int g=0;g<graph.State.Groups.Count;g++)
    {
        var group=graph.State.Groups[g];var pair=graph.ResolveGroupSource(g);if(pair.Secondary is not null)cross++;
        w.WriteStartObject();w.WriteNumber("descriptor",pair.Primary.DescriptorOrdinal);w.WriteNumber("line",pair.Primary.LineId);w.WriteNumber("height",pair.Primary.Height);Floats("bounds",[group.Bounds.MinimumX,group.Bounds.MinimumY,group.Bounds.MaximumX,group.Bounds.MaximumY]);w.WriteStartArray("ranges");foreach(var r in group.CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteStartArray("nodes");
    for(int n=0;n<graph.State.NodeCount;n++)
    {
        w.WriteStartArray();var row=graph.State.GetNode(n);for(ushort a=0;a<row.Count;a++)
        {
            var arc=row[a];var state=cache.ReadState(n,a);w.WriteStartObject();w.WriteNumber("group",arc.GroupId);w.WriteNumber("span",arc.Span);Ints("previous",arc.CopySelector(0).Select(v=>(int)v));Ints("next",arc.CopySelector(1).Select(v=>(int)v));Floats("fold",[state.FutureMinimumX,state.PrefixMaximumX]);Ints("costs",[state.JoinCost,state.TripleJoinCost,state.BlankCost]);Ints("matrix",state.CategoryMatrix.ToArray());w.WriteEndObject();arcs++;
            if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(state.CategoryMatrix,out var exposed))throw new Exception("Cache mutation fixture missing.");int saved=exposed.Array![exposed.Offset];exposed.Array[exposed.Offset]^=7;if(cache.ReadState(n,a).CategoryMatrix.Span[0]!=saved)throw new Exception("Public cache state mutated owned rows.");
            if(old is not null)
            {
                var oldArc=old.SourceRanges.Graph.GetNode(n).Select((value,index)=>(value,index)).Single(p=>p.value.Span==arc.Span&&p.value.StartContextId==arc.StartContextId&&p.value.EndContextId==arc.EndContextId);
                var before=oldCache!.GetPreparedState(n,oldArc.index);
                if(state.FutureMinimumX!=before.FutureMinimumX||state.PrefixMaximumX!=before.PrefixMaximumX||state.JoinCost!=before.JoinCost||state.TripleJoinCost!=before.TripleJoinCost||state.BlankCost!=oldBlank!.ReadPreparedBlankCost(n,(ushort)oldArc.index))throw new Exception("Old owning single-domain transition cache parity failed.");parity++;
            }
        }w.WriteEndArray();
    }w.WriteEndArray();w.WriteEndObject();
    Reject(()=>cache.ReadState(-1,0));Reject(()=>cache.ReadState(0,ushort.MaxValue));Reject(()=>cache.ReadState(0,0,new(true)));Reject(()=>LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table,new(true)));var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalTransitionCache.Prepare(bad,owner,table));owners++;
}
w.WriteEndArray();w.Flush();if(cross==0||parity==0)throw new Exception("Actual cross/single cache domains not exercised.");Console.WriteLine($"PASS {owners} actual global1941B0 owners/{arcs} arc caches/{cross} cross groups/{parity} old single-domain cache parity/{guards} guards; line-aware selectors and primary heights.");
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported global transition cache accepted.");}
