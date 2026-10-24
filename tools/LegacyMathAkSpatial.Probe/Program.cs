using InkRecognition;
using System.Numerics;
using System.Text.Json;
string repo=args[0],output=args[1];if(File.Exists(output))throw new IOException("Fresh output required.");
byte[] bytes=File.ReadAllBytes(Path.Combine(repo,"models/math/math-ak.res"));
var cases=new List<(string Name,Vector2[] Points)>{
("horizontal",new[]{new Vector2(0,0),new Vector2(4,0)}),
("vertical",new[]{new Vector2(0,0),new Vector2(0,4)}),
("nonsquare",new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}),
("fractional",new[]{new Vector2(.25f,.5f),new Vector2(2.75f,1.25f),new Vector2(4.5f,.75f)}),
("duplicates",new[]{new Vector2(0,0),new Vector2(0,0),new Vector2(2,1),new Vector2(2,1),new Vector2(4,0)}),
("zigzag",new[]{new Vector2(0,0),new Vector2(1,3),new Vector2(2,0),new Vector2(3,3),new Vector2(4,0)}),
("closed-loop",new[]{new Vector2(0,0),new Vector2(3,0),new Vector2(3,3),new Vector2(0,3),new Vector2(0,0)}),
("reverse-slope",new[]{new Vector2(4,0),new Vector2(0,4)}),
("cross",new[]{new Vector2(0,0),new Vector2(4,4),new Vector2(0,4),new Vector2(4,0)}),
("triangle",new[]{new Vector2(0,0),new Vector2(2,4),new Vector2(4,0),new Vector2(0,0)}),
("diagonal-ties",new[]{new Vector2(0,0),new Vector2(4,2),new Vector2(4,0),new Vector2(0,2)})};
var dense=new List<Vector2>();for(int y=0;y<128;y++){dense.Add(new(y%2==0?0:127,y));dense.Add(new(y%2==0?127:0,y));}cases.Add(("full-raster",dense.ToArray()));
var bars=new List<Vector2>();for(int x=0;x<128;x+=2){bars.Add(new(x,(x/2)%2==0?0:127));bars.Add(new(x,(x/2)%2==0?127:0));}cases.Insert(cases.Count-1,("parallel-bars",bars.ToArray()));
var projection=typeof(LegacyMathAkSourceGeometry).GetMethod("SpatialCountProjection",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.CreateDelegate<Func<int,short>>();
var countControls=new[]{0,8,256,512,528,1024}.Select(value=>new{value,result=(int)projection(value)}).ToArray();
var rows=new List<object>();var guards=new List<string>();
foreach(var c in cases)
{
 var g=LegacyMathAkSourceGeometry.CreateActual(bytes,new[]{c.Points});var f=g.PrepareUnguidedSingleStrokeSpatial();
 rows.Add(new{name=c.Name,original=c.Points.Select(p=>new[]{p.X,p.Y}).ToArray(),normalized=g.CopyNormalizedStrokes()[0].Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray(),values=f.CopyValues(),f.FeatureCount,f.ModelStart,f.RasterPixelCount,f.HasCandidateAuthority,f.HasClassifierOutput});
 if(c.Name=="nonsquare")
 {
  var foreign=LegacyMathAkSourceGeometry.CreateActual(bytes,new[]{c.Points});if(foreign.OwnsSpatialFeatures(f)||!g.OwnsSpatialFeatures(f))throw new Exception("Issuer");guards.Add("equal-data-foreign-issuer");
  var v=f.CopyValues();v[0]++;if(v[0]==f.CopyValues()[0])throw new Exception("Aliased output");guards.Add("values-detached");
  using var cts=new CancellationTokenSource();cts.Cancel();bool good=false;try{g.PrepareUnguidedSingleStrokeSpatial(cts.Token);}catch(OperationCanceledException e){good=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cts.Token;}if(!good)throw new Exception("Cancellation");guards.Add("cancel-exact-token");
 }
}
bool refused=false;try{LegacyMathAkSourceGeometry.CreateActual(bytes,new[]{new[]{new Vector2(0,0),new Vector2(0,0)}}).PrepareUnguidedSingleStrokeSpatial();}catch(NotSupportedException){refused=true;}if(!refused)throw new Exception("Degenerate");guards.Add("degenerate-refused");
File.WriteAllText(output,JsonSerializer.Serialize(new{rows,guards,countControls,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine($"PASS {rows.Count} source-bound spatial204 vectors / {guards.Count} guards / 0NN");
