using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory raw-controls.json capture.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));int guards=0;var canceled=new CancellationToken(true);
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected prepared-feature refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
float[][][] Points(IEnumerable<InkStroke> s)=>s.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
int[][][] Shorts(IEnumerable<LegacyInkStroke> s)=>s.Select(s=>s.Points.ToArray().Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray()).ToArray();
LegacyFirstAcoustic119WireModel Read(byte[]d){var p=LegacySource247WireLayout.Read(d);return LegacyFirstAcoustic119WireModel.ReadSelectedFirst(d,p,p.Children.First(c=>c.Kind==0x249));}
string path="en-US/en_US-ak-cur.lite.res";byte[] data=File.ReadAllBytes(Path.Combine(args[0],path));var model=Read(data);
var activations=LegacyOwnedFirst119Activations.Prepare(model);var evaluators=new[]{activations.CreatePreparedEvaluator(0),activations.CreatePreparedEvaluator(1)};
var cases=new List<CaseCapture>();int degenerate=0,merged=0,orderDifference=0;
void Capture(InkStroke[]raw,LegacyInkRange[]?selected=null)
{
 var owner=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);var counts=owner.CopyPhysicalPointCounts();
 var ranges=selected??counts.Select((n,s)=>new LegacyInkRange(s,0,n)).ToArray();
 var value=LegacyPreparedEnglishNoInitialGlyph.Prepare(owner,ranges);
 if(!ReferenceEquals(value.SourceOwner,owner)||!ReferenceEquals(value.Model,activations.Model)||value.MaximumMergedStrokes!=10)throw new Exception("Wrong source/profile/activation owner.");
 short[] t=value.CopyTrajectoryInput(),r=value.CopyRasterInput();
 if(t.Length!=219||r.Length!=201||t[^2]!=value.Boundary.Start||t[^1]!=value.Boundary.End||r[^2]!=t[^2]||r[^1]!=t[^1])throw new Exception("Wrong no-initial append layout.");
 var results=new[]{evaluators[0].EvaluatePreparedSourceUnmasked(t),evaluators[1].EvaluatePreparedSourceUnmasked(r)};
 var normalized=value.CopyNormalizedStrokes();var reduced=value.CopyMergedStrokes();
 degenerate+=value.DegenerateIntegerGeometry?1:0;merged+=normalized.Length>reduced.Length?1:0;
 orderDifference+=value.CopyNormalizationOrder().Where((v,i)=>v!=i).Any()?1:0;
 cases.Add(new(Points(raw),Points(owner.CopyPreparedStrokes()),owner.Line,
  ranges.Select(x=>new[]{x.StrokeIndex,x.StartPoint,x.EndPointExclusive}).ToArray(),value.CopyNormalizationOrder(),
  value.CopyStoredRanges().Select(x=>owner.ResolvePreparedPoint(x.StrokeIndex,x.StartPoint).Key).ToArray(),
  value.Boundary,value.DegenerateIntegerGeometry,Shorts(normalized),Shorts(reduced),value.CopyRetainedPenUpBoundaries(),t.ToArray(),r.ToArray(),
  results.Select(x=>new NetworkCapture(x.Owner.Slot,x.CopyHiddenDots(),x.CopyHiddenValues(),x.CopyOutputDots(),x.CopyProbabilities())).ToArray()));
 t[0]=12345;r[0]=12345;var order=value.CopyNormalizationOrder();order[0]=99;
 if(value.CopyTrajectoryInput()[0]==12345||value.CopyRasterInput()[0]==12345||value.CopyNormalizationOrder()[0]==99)throw new Exception("Mutable feature snapshot.");
 Guard(()=>LegacyPreparedEnglishNoInitialGlyph.Prepare(owner,[],canceled));Guard(()=>LegacyPreparedEnglishNoInitialGlyph.Prepare(owner,[new(-1,0,1)]));
}
foreach(var item in input.RootElement.GetProperty("cases").EnumerateArray())Capture(Ink(item));
InkStroke[] line=[new InkStroke(new[]{new InkPoint(0,50),new InkPoint(10,90),new InkPoint(30,30),new InkPoint(50,70)})];
var controlOwner=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,line);int n=controlOwner.CopyPhysicalPointCounts()[0];
Capture(line,[new(0,1,n),new(0,0,n-1)]);Capture(line,[new(0,0,n),new(0,0,n)]);
var many=Enumerable.Range(0,15).Select(s=>new InkStroke(new[]{new InkPoint(s*s*7,80),new InkPoint(s*s*7+2,0)})).ToArray();Capture(many);
Guard(()=>LegacyPreparedEnglishNoInitialGlyph.Prepare(controlOwner,Enumerable.Repeat(new LegacyInkRange(0,0,n),16).ToArray()));
Guard(()=>LegacyPreparedEnglishNoInitialGlyph.Prepare(controlOwner,[new(0,0,n+1)]));
Guard(()=>LegacyOwnedFirst119Activations.Prepare(null!));Guard(()=>LegacyOwnedFirst119Activations.Prepare(model,canceled));
Guard(()=>activations.CreatePreparedEvaluator(6));Guard(()=>activations.CreatePreparedEvaluator(-1));
Guard(()=>evaluators[0].EvaluatePreparedSourceUnmasked(new short[247]));
var h=activations.CopyHiddenTable();h[0]=999;var o=activations.CopyOutputTable();o[0]=999;
if(activations.CopyHiddenTable()[0]!=0||activations.CopyOutputTable()[0]!=512)throw new Exception("Mutable activation owner.");
var result=new CaptureData(path,model.Source.Offset,model.Source.End,model.Source.Fingerprint,
 activations.CopyHiddenTable(),activations.CopyOutputTable(),cases,degenerate,merged,orderDifference,guards);
File.WriteAllText(args[2],JsonSerializer.Serialize(result,CaptureJsonContext.Default.CaptureData));
Console.WriteLine($"PreparedEN noInitial: {cases.Count} source/range controls/{degenerate} degenerate/{merged} merged/{orderDifference} reordered;219/201 input+actual107diagnostics/{guards} guards. No native candidate membership claim.");
record NetworkCapture(int Slot,int[] HiddenDots,short[] HiddenActivations,int[] RawDots,int[] Probability32768);
record CaseCapture(float[][][] Raw,float[][][] Prepared,LegacyLinePair Line,int[][] Ranges,int[] Order,LegacySourcePointKey[] StartKeys,
 LegacyCursiveGlyphBoundarySigns Boundary,bool Degenerate,int[][][] Normalized,int[][][] Merged,int[] Retained,short[] Trajectory,short[] Raster,NetworkCapture[] Networks);
record CaptureData(string Path,int SourceOffset,int SourceEnd,string Fingerprint,int[] HiddenTable,int[] OutputTable,List<CaseCapture> Cases,int Degenerate,int Merged,int Reordered,int Guards);
[JsonSerializable(typeof(CaptureData))]
partial class CaptureJsonContext:JsonSerializerContext;
