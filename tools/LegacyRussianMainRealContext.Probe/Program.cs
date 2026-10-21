using InkRecognition;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 INPUTS capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
using var data=JsonDocument.Parse(File.ReadAllBytes(args[7]));
using var file=File.Create(args[8]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
int index=0,windowsTotal=0,accepted=0;
foreach(var sample in data.RootElement.GetProperty("samples").EnumerateArray())
{
    var raw=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd,cancel.Token).Run(cancel.Token);
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed,cancel.Token);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table,cancel.Token);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table,cancel.Token);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global,cancel.Token);var refs=LegacyFreshMainGlobalSourceRanges.Prepare(graph,cancel.Token);var result=LegacyFreshMainGlobalContext.Prepare(ak,refs,activations,cancel.Token);
    if(!ReferenceEquals(result.Source,refs)||!ReferenceEquals(result.Graph,graph))throw new Exception("Global context graph/source ownership changed.");
    var sourcePoints=cuts.CopyStrokes();var windows=result.CopyWindows();windowsTotal+=windows.Length;accepted+=windows.Count(v=>v.Accepted);
    w.WriteStartObject();w.WriteStartArray("points");foreach(var s in sourcePoints){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("descriptors");foreach(var step in cuts.Steps.Where(s=>!s.IsPendingSeparator)){w.WriteStartObject();w.WriteNumber("ordinal",step.DescriptorOrdinal);w.WriteNumber("line",step.LineId);w.WriteNumber("first",step.FirstPhysical);w.WriteNumber("last",step.LastPhysical);w.WriteNumber("baseline",step.Baseline);w.WriteNumber("height",step.Height);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("windows");foreach(var v in windows){w.WriteStartObject();w.WriteNumber("descriptor",v.DescriptorOrdinal);w.WriteNumber("line",v.LineId);w.WriteNumber("start",v.StartPhysical);w.WriteNumber("end",v.EndPhysical);w.WriteNumber("center",v.CenterX);w.WriteBoolean("accepted",v.Accepted);Floats("features",v.Features.ToArray());Ints("fixed",v.FixedInput.ToArray().Select(v=>(int)v));Ints("probabilities",v.Probability32768.ToArray());w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("groups");for(int g=0;g<graph.State.Groups.Count;g++)
    {
        var group=graph.State.Groups[g];var context=result.GetContext(g,cancel.Token);var pair=graph.ResolveGroupSource(g,cancel.Token);w.WriteStartObject();w.WriteNumber("descriptor",group.DescriptorOrdinal);w.WriteNumber("secondary",pair.Secondary?.DescriptorOrdinal??-1);w.WriteStartArray("ranges");foreach(var r in group.CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();Ints("context",context.Select(v=>(int)v));w.WriteEndObject();

    }w.WriteEndArray();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteBoolean("expectedLabelsRead",false);w.WriteEndObject();w.Flush();Console.WriteLine("Captured "+(++index));
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {index} actual trajectory sources/{windowsTotal} context windows/{accepted} accepted; no target labels or transcription/quality claim.");
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
