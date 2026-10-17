using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
int cases=0,groups=0,guards=0,windowsTotal=0,accepted=0,cross=0,parity=0;using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var refs=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var result=LegacyFreshMainGlobalContext.Prepare(ak,refs,activations);
    if(!ReferenceEquals(result.Source,refs)||!ReferenceEquals(result.Graph,graph))throw new Exception("Global context graph/source ownership changed.");
    var sourcePoints=cuts.CopyStrokes();var windows=result.CopyWindows();windowsTotal+=windows.Length;accepted+=windows.Count(v=>v.Accepted);
    w.WriteStartObject();w.WriteStartArray("points");foreach(var s in sourcePoints){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("descriptors");foreach(var step in cuts.Steps.Where(s=>!s.IsPendingSeparator)){w.WriteStartObject();w.WriteNumber("ordinal",step.DescriptorOrdinal);w.WriteNumber("line",step.LineId);w.WriteNumber("first",step.FirstPhysical);w.WriteNumber("last",step.LastPhysical);w.WriteNumber("baseline",step.Baseline);w.WriteNumber("height",step.Height);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("windows");foreach(var v in windows){w.WriteStartObject();w.WriteNumber("descriptor",v.DescriptorOrdinal);w.WriteNumber("line",v.LineId);w.WriteNumber("start",v.StartPhysical);w.WriteNumber("end",v.EndPhysical);w.WriteNumber("center",v.CenterX);w.WriteBoolean("accepted",v.Accepted);Floats("features",v.Features.ToArray());Ints("fixed",v.FixedInput.ToArray().Select(v=>(int)v));Ints("probabilities",v.Probability32768.ToArray());w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("groups");for(int g=0;g<graph.State.Groups.Count;g++)
    {
        var group=graph.State.Groups[g];var context=result.GetContext(g);groups++;w.WriteStartObject();w.WriteNumber("descriptor",group.DescriptorOrdinal);w.WriteStartArray("ranges");foreach(var r in group.CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();Ints("context",context.Select(v=>(int)v));w.WriteEndObject();
        if(graph.ResolveGroupSource(g).Secondary is not null)cross++;
    }w.WriteEndArray();w.WriteEndObject();cases++;
    if(local.Steps.Count==1)
    {
        var single=LegacyFreshMainSingleLineSampler.Prepare(ak,input.Smoothed).Sample(table);var old=LegacyFreshMainSingleLineContext.Prepare(ak,single,activations);
        if(old.Windows.Count!=windows.Length)throw new Exception("Single-domain context window count changed.");
        for(int i=0;i<windows.Length;i++)if(!old.Windows[i].Features.Span.SequenceEqual(windows[i].Features.Span)||!old.Windows[i].FixedInput.Span.SequenceEqual(windows[i].FixedInput.Span)||!old.Windows[i].Probability32768.Span.SequenceEqual(windows[i].Probability32768.Span))throw new Exception("Single-domain NN/window parity failed.");
        for(int n=0;n<graph.State.NodeCount;n++)for(int a=0;a<graph.State.GetNode(n).Count;a++)
        {
            var arc=graph.State.GetNode(n)[a];var same=single.SourceRanges.Graph.GetNode(n).Single(v=>v.Span==arc.Span&&v.StartContextId==arc.StartContextId&&v.EndContextId==arc.EndContextId);
            if(!old.GetContext(same.GroupId).SequenceEqual(result.GetContext((int)arc.GroupId)))throw new Exception("Single-domain actual context parity failed.");parity++;
        }
    }
    Reject(()=>LegacyFreshMainGlobalContext.Prepare(ak,refs,activations,new(true)));Reject(()=>result.GetContext(-1));Reject(()=>result.GetContext(0,new(true)));var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalContext.Prepare(bad,refs,activations));
    if(result.CopyGroups().Length>0){var before=result.GetContext(0);var copy=result.CopyGroups();var numbers=copy[0].Probabilities.ToArray();Array.Fill(numbers,1f);copy[0]=copy[0]with{Probabilities=numbers};if(!before.SequenceEqual(result.GetContext(0)))throw new Exception("Exposed context copy altered production output.");}
}
w.WriteEndArray();w.Flush();if(accepted==0||cross==0||parity==0)throw new Exception("Actual context/descriptor/cross/single-parity paths not exercised.");Console.WriteLine($"PASS {cases} actual owners/{windowsTotal} real227 windows/{accepted} accepted/{groups} group contexts/{cross} cross groups/{parity} single-domain parity/{guards} guards.");
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported global context source accepted.");}
