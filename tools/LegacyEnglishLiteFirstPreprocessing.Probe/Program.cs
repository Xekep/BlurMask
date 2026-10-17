using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if(args.Length!=3)throw new ArgumentException("models-directory controls.json capture.json");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[1]));int guards=0;var canceled=new CancellationToken(true);
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected scoped refusal.");}
InkStroke[] Strokes(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
float[][][] Points(IEnumerable<InkStroke> s)=>s.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
LegacyFirstAcoustic119WireModel Read(byte[] data){var p=LegacySource247WireLayout.Read(data);return LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,p,p.Children.First(c=>c.Kind==0x249));}
string relative="en-US/en_US-ak-cur.lite.res";byte[] resource=File.ReadAllBytes(Path.Combine(args[0],relative));var model=Read(resource);
var results=new List<CaseCapture>();int accepted=0,rejected=0,points=0;
foreach(var input in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=Strokes(input);LegacyOwnedEnglishLiteFirstPreprocessing value;
    try{value=LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,raw);}catch(NotSupportedException e)when(e.Message.StartsWith("Positive24C",StringComparison.Ordinal))
    {results.Add(new(Points(raw),true));rejected++;continue;}
    if(!ReferenceEquals(value.Model,model)||!ReferenceEquals(value.Source,model.Source)||
        !model.Source.Children.Contains(value.GeometryChild)||!model.Source.Children.Contains(value.SlantChild)||!model.Source.Children.Contains(value.SmoothingChild))throw new Exception("Lost selected source ownership.");
    var prepared=value.CopyPreparedStrokes();var identities=prepared.Select((s,i)=>s.Points.Select((_,j)=>value.ResolvePreparedPoint(i,j)).ToArray()).ToArray();
    points+=prepared.Sum(s=>s.Points.Count);accepted++;
    var original=value.CopyOriginalStrokes();var deskewed=value.CopyDeskewedStrokes();var counts=value.CopyPhysicalPointCounts();var keys=value.CopyPointKeys200();var probabilities=value.CopySourceProbabilities();
    results.Add(new(Points(raw),false,Points(original),Points(deskewed),Points(prepared),
        value.Estimate,value.Line,value.SourceMetrics,value.SmoothingWeight,value.SlantDirectionCount,counts.ToArray(),keys.ToArray(),identities,probabilities.ToArray()));
    //All recoverable public buffers are detached; mutations cannot rewrite
    //the source later consumed by features or alter its chronology.
    var first=value.CopyPreparedStrokes()[0].Points[0];((InkPoint[])prepared[0].Points)[0]=new(999,999);
    ((InkPoint[])original[0].Points)[0]=new(999,999);((InkPoint[])deskewed[0].Points)[0]=new(999,999);
    if(keys.Length!=0)keys[0]=-99;counts[0]=0;probabilities[0]=-1;
    if(value.CopyPreparedStrokes()[0].Points[0]!=first||value.CopyPointKeys200()[0]<0||value.CopyPhysicalPointCounts()[0]==0||value.CopySourceProbabilities()[0]<0)throw new Exception("Mutable owned preprocessing.");
    Guard(()=>value.ResolvePreparedPoint(-1,0));Guard(()=>value.ResolvePreparedPoint(0,value.CopyPhysicalPointCounts()[0]));
}
if(accepted<30||rejected<3)throw new Exception("Insufficient actual geometry/positive24C refusal coverage.");
var counter=new List<CounterCapture>();
foreach(var item in controls.RootElement.GetProperty("counterControls").EnumerateArray())
{
    var strokes=Strokes(item);int count=LegacyOwnedEnglishLiteFirstPreprocessing.CountFreshUnassignedDirections(strokes.Select(s=>s.Points.ToArray()).ToArray());
    counter.Add(new(Points(strokes),count));
}
var valid=new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(2,3)})};
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(null!,valid));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,valid,canceled));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,[]));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,[new InkStroke([])]));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,[new InkStroke(new[]{new InkPoint(float.NaN,1)})]));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,[new InkStroke(Enumerable.Repeat(new InkPoint(0,0),4097).ToArray())]));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,Enumerable.Repeat(valid[0],257).ToArray()));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(Read(File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.res"))),valid));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.CountFreshUnassignedDirections([],canceled));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.CountFreshUnassignedDirections([[]]));
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.CountFreshUnassignedDirections([new[]{new InkPoint(0,float.PositiveInfinity)}]));
foreach(string destination in new[]{"28","2C"})
{
    byte[] changed=resource.ToArray();int offset=model.Fields.Single(f=>f.Owner=="profile0"&&f.Destination==destination).Offset;
    if(destination=="28")BinaryPrimitives.WriteUInt16LittleEndian(changed.AsSpan(offset),1);else BinaryPrimitives.WriteSingleLittleEndian(changed.AsSpan(offset),.2f);
    Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(Read(changed),valid));
}
var slant=model.Source.Children.Single(c=>c.Kind==0x24c);byte[] badAngles=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(badAngles.AsSpan(slant.Offset+16),float.NaN);
Guard(()=>LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(Read(badAngles),valid));
var sample=LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,valid);
var output=new Capture(relative,model.Source.Fingerprint,model.Source.Offset,model.Source.End,
    new(sample.GeometryChild.Offset,sample.GeometryChild.End,sample.GeometryProfile),
    new(sample.SlantChild.Offset,sample.SlantChild.End,sample.MinimumSlantRadians,sample.MaximumSlantRadians),
    new(sample.SmoothingChild.Offset,sample.SmoothingChild.End,sample.SmoothingProfile),
    accepted,rejected,points,guards,results,counter);
File.WriteAllText(args[2],JsonSerializer.Serialize(output,CaptureJsonContext.Default.Capture));
Console.WriteLine($"ENlite actual source24A/24C<20/265: {accepted} accepted, {rejected} positive24C refusals, {points} prepared points, {counter.Count} counter controls, {guards} guards.");

record CaseCapture(float[][][] Raw,bool RejectedPositive24C,float[][][]? Original=null,float[][][]? Deskewed=null,
    float[][][]? Prepared=null,LegacyAutomaticLineEstimate Estimate=default,LegacyLinePair Line=default,
    LegacyOrientationStrokeMetrics SourceMetrics=default,float SmoothingWeight=0,int SlantDirectionCount=0,
    int[]? Counts=null,int[]? Keys200=null,LegacySourcePointIdentity[][]? Identities=null,float[]? Probabilities=null);
record CounterCapture(float[][][] Strokes,int Count);
record GeometryCapture(int Offset,int End,LegacyAutomaticLineProfile Profile);
record SlantCapture(int Offset,int End,float MinimumSlantRadians,float MaximumSlantRadians);
record SmoothingCapture(int Offset,int End,LegacyOrientationSmoothingProfile Profile);
record Capture(string Path,string Fingerprint,int SourceOffset,int SourceEnd,GeometryCapture Geometry,SlantCapture Slant,
    SmoothingCapture Smoothing,int Accepted,int Rejected,int Points,int Guards,List<CaseCapture> Cases,List<CounterCapture> CounterControls);
[JsonSerializable(typeof(Capture))]
partial class CaptureJsonContext:JsonSerializerContext;
