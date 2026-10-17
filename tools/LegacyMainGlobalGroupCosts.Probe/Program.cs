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
int owners=0,groups=0,arcs=0,guards=0,cross=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);
    var profile=LegacyCursiveSeparationProfile.ReadV106Node(ak,geometry.CategoryOwner.SamplerNodeOffset);
    w.WriteStartObject();Floats("profile",[profile.GapOffset,profile.Slope,profile.MaximumCost]);w.WriteStartArray("points");foreach(var s in cuts.CopyStrokes()){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
    //The public table memory is recoverable. An already owned cache retains a
    //checked copy; a later factory call must refuse this modified caller table.
    if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(table.Values,out var exposed))throw new Exception("Table mutation fixture unavailable.");
    var saved=table.Values.ToArray();Array.Fill(exposed.Array!,0f,exposed.Offset,exposed.Count);
    try
    {
        Reject(()=>LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table));
        w.WriteStartArray("groups");for(int g=0;g<graph.State.Groups.Count;g++)
        {
            var f=geometry.ReadFrame(g);var pair=graph.ResolveGroupSource(g);int value=costs.ReadGeometryCost(g);
            if(costs.ReadGeometryCost(g)!=value)throw new Exception("Repeated actual geometry cache changed.");
            if(pair.Secondary is not null)cross++;
            w.WriteStartObject();w.WriteNumber("height",f.Height);w.WriteNumber("primaryBaseline",pair.Primary.Baseline);w.WriteNumber("primaryHeight",pair.Primary.Height);
            if(pair.Secondary is{} secondary){w.WriteNumber("secondaryBaseline",secondary.Baseline);w.WriteNumber("secondaryHeight",secondary.Height);}
            w.WriteNumber("cost",value);w.WriteStartArray("ranges");foreach(var r in graph.State.Groups[g].CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();w.WriteEndObject();groups++;
        }w.WriteEndArray();
    }
    finally{saved.CopyTo(exposed.Array!,exposed.Offset);}
    owner.AdvanceNodeEpoch(-1);w.WriteStartArray("selected");
    for(int n=0;n<graph.State.NodeCount;n++)
    {
        for(byte s=0;s<owner.GetSpanExtent(n);s++)
        {
            var span=owner.PrepareSpan(n,s);var node=graph.State.GetNode(n);
            for(ushort a=0;a<node.Count;a++)if(node[a].Span==s)
            {
                var actual=costs.ReadCosts(span,n,a);int group=(int)node[a].GroupId;int expected=costs.ReadGeometryCost(group);
                if(actual.ClassCosts.Length!=195||actual.GeometryCosts.Length!=19||actual.GeometryCosts.Span.ContainsAnyExcept(expected)||!actual.ClassCosts.Span.SequenceEqual(owner.CopyClassCosts(span,n,a)))throw new Exception("Actual group/cache projection changed.");
                if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(actual.ClassCosts,out var memory))throw new Exception("Class mutation fixture unavailable.");
                int before=memory.Array![memory.Offset];memory.Array[memory.Offset]^=7;if(costs.ReadCosts(span,n,a).ClassCosts.Span[0]!=before)throw new Exception("Exposed class result mutated private cost cache.");
                w.WriteStartObject();w.WriteNumber("node",n);w.WriteNumber("arc",a);w.WriteNumber("group",group);w.WriteNumber("geometry",expected);w.WriteNumber("width",actual.GeometryCosts.Length);w.WriteEndObject();arcs++;
            }
        }
        owner.AdvanceNodeEpoch(n);
    }w.WriteEndArray();w.WriteEndObject();
    Reject(()=>costs.ReadGeometryCost(-1));Reject(()=>costs.ReadGeometryCost(0,new(true)));Reject(()=>LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table,new(true)));
    var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalGroupCosts.Prepare(bad,owner,table));owners++;
}
w.WriteEndArray();w.Flush();if(cross==0)throw new Exception("Actual cross-source geometry untested.");Console.WriteLine($"PASS {owners} actual global owners/{groups}193860 geometry groups/{cross} cross-source groups/{arcs} current D0 class+19geometry rows/{guards} guards; native stored ranges and private table/cache copies.");
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported global cost source accepted.");}
