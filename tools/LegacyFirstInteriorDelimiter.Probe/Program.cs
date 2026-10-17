using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("AK engine separation capture");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
var raw=new[]{Word(0).Concat(Word(450)).ToArray(),Word(0),Hi(0).Concat(Hi(220)).ToArray()};
int guards=0,calls=0;
using var output=File.Create(args[3]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("sources");
for(int sample=0;sample<raw.Length;sample++)
{
    var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw[sample],activations,table);
    var delimiter=LegacyFirstOwnedInteriorDelimiter.Prepare(resource,owner);var graph=owner.Prepared.SourceRanges.Graph;
    var alphabet=owner.Factory.InvocationPolicy.Binding.Alphabet;
    var symbols=new[]{"M","И","P",":",";","!","?","#","@","-","/"}.Select(t=>alphabet.TryGetLabel(t,out ushort value)?value:throw new Exception("Own symbol absent")).ToArray();
    var arcs=Enumerable.Range(0,graph.NodeCount).SelectMany(n=>Enumerable.Range(0,graph.GetNode(n).Count).Select(i=>(N:n,A:(ushort)i))).ToArray();
    w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteNumber("heightBits",BitConverter.SingleToInt32Bits(owner.Factory.Frame.Row.Frame.Line.Height));
    w.WriteStartArray("counts");foreach(int n in owner.Cuts.Cuts.PreparedInk.StrokePointCounts.Span)w.WriteNumberValue(n);w.WriteEndArray();
    w.WriteStartArray("points");foreach(var p in owner.Cuts.Cuts.PreparedInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.X));w.WriteNumberValue(BitConverter.SingleToInt32Bits(p.Y));w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");for(int g=0;g<graph.Groups.Count;g++)
    {
        var b=delimiter.ReadBounds(g);w.WriteStartObject();w.WriteStartArray("ranges");foreach(var r in graph.Groups[g].Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("box");foreach(float v in new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY})w.WriteNumberValue(BitConverter.SingleToInt32Bits(v));w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();
    w.WriteStartArray("requests");
    for(int n=0;n<2048;n++)
    {
        var p=arcs[n%arcs.Length];var c=arcs[(n*17+3)%arcs.Length];ushort ps=n%13==0?(ushort)65535:symbols[n%symbols.Length],cs=symbols[n/11%symbols.Length];
        var result=delimiter.Read(p.N,p.A,ps,c.N,c.A,cs);calls++;
        w.WriteStartArray();w.WriteNumberValue(graph.GetNode(p.N)[p.A].GroupId);w.WriteNumberValue(graph.GetNode(c.N)[c.A].GroupId);
        w.WriteNumberValue(ps);w.WriteNumberValue(cs);w.WriteNumberValue(graph.GetNode(p.N)[p.A].StartContextId);w.WriteNumberValue(graph.GetNode(c.N)[c.A].Flags);
        w.WriteNumberValue(result.Count);w.WriteNumberValue(result.Symbol);w.WriteNumberValue(result.ComponentFlag);w.WriteNumberValue(result.ExternalVariant);w.WriteEndArray();
    }w.WriteEndArray();w.WriteEndObject();
    Reject(()=>delimiter.Read(-1,0,0,0,0,0));Reject(()=>delimiter.Read(0,ushort.MaxValue,0,0,0,0));
    Reject(()=>delimiter.Read(0,0,0,graph.NodeCount,0,0));Reject(()=>delimiter.Read(0,0,0,0,0,0,new CancellationToken(true)));
    Reject(()=>delimiter.ReadBounds(-1));Reject(()=>delimiter.ReadBounds(0,new CancellationToken(true)));
    Reject(()=>LegacyFirstOwnedInteriorDelimiter.Prepare(resource,owner,new CancellationToken(true)));
    var bad=resource.ToArray();bad[owner.Factory.AcousticModel.OrdinaryNodeOffset+100]^=1;
    Reject(()=>LegacyFirstOwnedInteriorDelimiter.Prepare(bad,owner));
    Reject(()=>delimiter.Read(0,0,0,0,0,ushort.MaxValue-1));
}
w.WriteEndArray();w.WriteNumber("calls",calls);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {calls} prepared first interior requests over3 genuine first-owned NN/cut graphs; {guards} guards. No text/materialized multiword claim.");
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported input accepted");}
static InkStroke[] Word(float x)=>[
 new(new InkPoint[]{new(x,90),new(x,10),new(x+25,60),new(x+50,10),new(x+50,90)}),
 new(new InkPoint[]{new(x+65,90),new(x+65,10)}),new(new InkPoint[]{new(x+65,90),new(x+115,10)}),new(new InkPoint[]{new(x+115,90),new(x+115,10)}),
 new(new InkPoint[]{new(x+130,90),new(x+130,10),new(x+175,10),new(x+185,20),new(x+185,45),new(x+175,55),new(x+130,55)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),
 new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
