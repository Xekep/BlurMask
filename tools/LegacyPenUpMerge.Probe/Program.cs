using InkRecognition;
using System.Text.Json;
LegacyInkStroke[] dots=new short[]{0,1,11,13,33}.Select(x=>new LegacyInkStroke(new LegacyInkPoint[]{new(x,0)})).ToArray();
var merged=LegacyNormalizedPenUpMerge.Merge(dots,3);
Require(merged.RetainedBoundaryIndices.Span.SequenceEqual(new int[]{1,3})&&merged.Strokes.Count==3&&merged.Strokes[0].Points.SequenceEqual(new LegacyInkPoint[]{new(0,0),new(1,0)})&&merged.Strokes[1].Points.SequenceEqual(new LegacyInkPoint[]{new(11,0),new(13,0)}),"Largest-gap boundaries or source point grouping differ.");
var tie=LegacyNormalizedPenUpMerge.Merge(new LegacyInkStroke[]{new(new LegacyInkPoint[]{new(0,0)}),new(new LegacyInkPoint[]{new(1,0)}),new(new LegacyInkPoint[]{new(2,0)})},2);
Require(tie.RetainedBoundaryIndices.Span.SequenceEqual(new int[]{1}),"Native equal two-gap swap ordering differs.");
Require(LegacyNormalizedPenUpMerge.Merge(dots,5).RetainedBoundaryIndices.Span.SequenceEqual(new int[]{0,1,2,3}),"Below-threshold ink lost a boundary.");
Require(LegacyNormalizedPenUpMerge.Merge(dots,1).Strokes.Single().Points.SequenceEqual(dots.SelectMany(s=>s.Points.ToArray()).ToArray()),"One-stroke merge changed/discarded points.");
try{LegacyNormalizedPenUpMerge.Merge(dots,0);throw new Exception("Zero threshold accepted.");}catch(ArgumentException){}
try{LegacyNormalizedPenUpMerge.Merge(new LegacyInkStroke[]{new(new LegacyInkPoint[]{new(short.MinValue,0)}),new(new LegacyInkPoint[]{new(short.MaxValue,0)})},1);throw new Exception("Gap overflow accepted.");}catch(ArgumentException){}
try{LegacyNormalizedPenUpMerge.Merge(dots,3,new CancellationToken(true));throw new Exception("Cancel ignored.");}catch(OperationCanceledException){}
Console.WriteLine("PASS native pen-up merge: largest gap boundaries, exact two-gap tie swap, points/physical order retained, no merge below threshold, one-stroke reduction, overflow and cancellation.");
if(args.Length==0)return;
if(args.Length!=2||args[0]!="--controls")throw new ArgumentException("Usage: LegacyPenUpMerge.Probe [--controls own.json]");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[1]));int count=0;
foreach(var c in document.RootElement.GetProperty("cases").EnumerateArray())
{
 var strokes=c.GetProperty("strokes").EnumerateArray().Select(s=>new LegacyInkStroke(s.EnumerateArray().Select(p=>new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray())).ToArray();
 var result=LegacyNormalizedPenUpMerge.Merge(strokes,c.GetProperty("threshold").GetInt32());
 var retained=c.GetProperty("retained").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
 Require(result.RetainedBoundaryIndices.Span.SequenceEqual(retained)&&result.Strokes.Count==retained.Length+1&&result.Strokes.SelectMany(s=>s.Points.ToArray()).SequenceEqual(strokes.SelectMany(s=>s.Points.ToArray())),"Independent native gap-selection/point-order result differs.");count++;
}
Require(count>0,"Empty references.");Console.WriteLine($"PASS {count} independent unique-gap selection controls.");
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
