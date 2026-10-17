using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory controls.json capture.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));var canceled=new CancellationToken(true);int guards=0,parity=0,positive=0;
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected bounded24C refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
float[][][] Points(IEnumerable<InkStroke> s)=>s.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
bool BitsEqual(IEnumerable<InkStroke>a,IEnumerable<InkStroke>b)=>Points(a).SelectMany(s=>s).SelectMany(p=>p).Select(BitConverter.SingleToInt32Bits).SequenceEqual(Points(b).SelectMany(s=>s).SelectMany(p=>p).Select(BitConverter.SingleToInt32Bits));
LegacyFirstAcoustic119WireModel Read(byte[] d){var s=LegacySource247WireLayout.Read(d);return LegacyFirstAcoustic119WireModel.ReadSelectedFirst(d,s,s.Children.First(c=>c.Kind==0x249));}
string path="en-US/en_US-ak-cur.lite.res";byte[] data=File.ReadAllBytes(Path.Combine(args[0],path));var model=Read(data);var source=model.Source;
var slantChild=source.Children.Single(c=>c.Kind==0x24c);var foreign=Read(data);
var cases=new List<CaseCapture>();int pointCount=0;
foreach(var item in input.RootElement.GetProperty("cases").EnumerateArray())
{
 var raw=Ink(item);var value=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);var prepared=value.CopyPreparedStrokes();
 if(!ReferenceEquals(value.Model,model)||!ReferenceEquals(value.Slant.Source,source)||!ReferenceEquals(value.Slant.Processor,slantChild)||value.RawDescriptor.FirstPhysicalStroke!=0||value.RawDescriptor.LastPhysicalStroke!=raw.Length-1||value.RawDescriptor.CurrentLineId!=-1)throw new Exception("Lost original packet/source provenance.");
 if(value.Slant.Applied)positive++;else
 {
  var old=LegacyOwnedEnglishLiteFirstPreprocessing.Prepare(model,raw);
  if(!BitsEqual(old.CopyPreparedStrokes(),prepared)||!BitsEqual(old.CopyDeskewedStrokes(),value.CopyDeskewedStrokes())||!old.CopyPointKeys200().SequenceEqual(value.CopyPointKeys200())||old.Line!=value.Line||old.Estimate!=value.Estimate)throw new Exception("Old short branch changed.");
  parity++;
 }
 var identities=prepared.Select((s,i)=>s.Points.Select((_,j)=>value.ResolvePreparedPoint(i,j)).ToArray()).ToArray();pointCount+=prepared.Sum(s=>s.Points.Count);
 cases.Add(new(Points(raw),Points(value.CopyOriginalStrokes()),Points(value.CopyDeskewedStrokes()),Points(value.Slant.CopyStrokes()),Points(prepared),value.Estimate,value.Line,value.SourceMetrics,value.SmoothingWeight,
  value.Slant.DirectionCount,value.Slant.AcceptedLength,value.Slant.WeightedAngleSum,value.Slant.Angle,value.CopyPhysicalPointCounts(),value.CopyPointKeys200(),identities));
 var copy=value.Slant.CopyStrokes();var before=copy[0].Points[0];((InkPoint[])copy[0].Points)[0]=new(900,900);
 if(value.Slant.CopyStrokes()[0].Points[0]!=before)throw new Exception("Mutable slant result.");
 var outCopy=value.CopyPreparedStrokes();before=outCopy[0].Points[0];((InkPoint[])outCopy[0].Points)[0]=new(900,900);
 if(value.CopyPreparedStrokes()[0].Points[0]!=before)throw new Exception("Mutable full preprocessing.");
 Guard(()=>value.ResolvePreparedPoint(-1,0));
}
var controls=new List<PreparedCapture>();
foreach(var item in input.RootElement.GetProperty("preparedControls").EnumerateArray())
{
 var raw=Ink(item.GetProperty("strokes"));float minimum=item.GetProperty("minimum").GetSingle(),maximum=item.GetProperty("maximum").GetSingle();
 var r=LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles(raw,minimum,maximum);
 controls.Add(new(Points(raw),minimum,maximum,r.DirectionCount,r.AcceptedLength,r.WeightedAngleSum,r.Angle,Points(r.CopyStrokes())));
}
InkStroke[] valid=[new InkStroke(new[]{new InkPoint(0,0),new InkPoint(1,4)})];
Guard(()=>LegacyPreparedEnglishFirstSlant.Apply(source,foreign.Source.Children.Single(c=>c.Kind==0x24c),valid));
Guard(()=>LegacyPreparedEnglishFirstSlant.Apply(source,source.Children.Single(c=>c.Kind==0x24a),valid));
Guard(()=>LegacyPreparedEnglishFirstSlant.Apply(source,slantChild,valid,canceled));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles(valid,float.NaN,1));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles(valid,1,-1));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles([],0,0));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles([new InkStroke([])],0,0));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles([new InkStroke(new[]{new InkPoint(float.PositiveInfinity,0)})],0,0));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles(Enumerable.Repeat(valid[0],257).ToArray(),0,0));
Guard(()=>LegacyPreparedEnglishFirstSlant.ApplyPreparedAngles([new InkStroke(Enumerable.Range(0,21).Select(i=>new InkPoint(0,i%2==0?0:float.MaxValue)).ToArray())],-.5f,.5f));
Guard(()=>LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,valid,canceled));
Guard(()=>LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,[]));
Guard(()=>LegacyOwnedEnglishLiteFullPreprocessing.Prepare(Read(File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.res"))),valid));
if(positive<20||parity<90)throw new Exception($"Insufficient positive/old-parity rows: {positive}/{parity}.");
var geometryChild=source.Children.Single(c=>c.Kind==0x24a);var smoothingChild=source.Children.Single(c=>c.Kind==0x265);byte[] angles=source.CopyChildEnvelopeBytes(slantChild);
var capture=new Capture(path,source.Fingerprint,source.Offset,source.End,
 new(geometryChild.Offset,geometryChild.End,LegacyAutomaticLineProfile.ReadResourceNode(source.CopyChildEnvelopeBytes(geometryChild),0)),
 new(slantChild.Offset,slantChild.End,BinaryPrimitives.ReadSingleLittleEndian(angles.AsSpan(16)),BinaryPrimitives.ReadSingleLittleEndian(angles.AsSpan(20))),
 new(smoothingChild.Offset,smoothingChild.End,LegacyOrientationSmoothingProfile.ReadV102Node(source.CopyChildEnvelopeBytes(smoothingChild),0)),
 positive,parity,pointCount,guards,cases,controls);
File.WriteAllText(args[2],JsonSerializer.Serialize(capture,CaptureJsonContext.Default.Capture));
Console.WriteLine($"ENlite full24A/24C/265: {cases.Count} actual rows/{positive} positive24C/{pointCount} prepared points; {parity} oldbit-parity; {controls.Count} prepared controls/{guards} guards.");
record CaseCapture(float[][][] Raw,float[][][] Original,float[][][] Deskewed,float[][][] Slanted,float[][][] Prepared,
 LegacyAutomaticLineEstimate Estimate,LegacyLinePair Line,LegacyOrientationStrokeMetrics SourceMetrics,float SmoothingWeight,
 int DirectionCount,double AcceptedLength,double WeightedAngleSum,double Angle,int[] Counts,int[] Keys200,LegacySourcePointIdentity[][] Identities);
record PreparedCapture(float[][][] Strokes,float Minimum,float Maximum,int DirectionCount,double AcceptedLength,double WeightedAngleSum,double Angle,float[][][] Output);
record GeometryCapture(int Offset,int End,LegacyAutomaticLineProfile Profile);
record SlantCapture(int Offset,int End,float MinimumSlantRadians,float MaximumSlantRadians);
record SmoothingCapture(int Offset,int End,LegacyOrientationSmoothingProfile Profile);
record Capture(string Path,string Fingerprint,int SourceOffset,int SourceEnd,GeometryCapture Geometry,SlantCapture Slant,SmoothingCapture Smoothing,
 int Positive,int OldParity,int Points,int Guards,List<CaseCapture> Cases,List<PreparedCapture> PreparedControls);
[JsonSerializable(typeof(Capture))]
partial class CaptureJsonContext:JsonSerializerContext;
