using System.Numerics;
using System.Text.Json;
using System.Reflection;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("REPOSITORY CAPTURE");
byte[] ak=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var inputs=new (string Name,Vector2[] Points)[]
{
 ("horizontal-half",[new(100,20),new(100.5f,20),new(102,20)]),
 ("vertical",[new(-3,-8),new(-3,2)]),
 ("nonsquare",[new(-4,-2),new(4,2),new(0,0)]),
 ("fractional",[new(-.75f,-.125f),new(.125f,.875f),new(.5f,.25f)]),
 ("duplicates",[new(7,8),new(7,8),new(10,10)]),
 ("zigzag",[new(0,0),new(1,3),new(2,1),new(3,4),new(4,2),new(5,5)]),
 ("closed-loop",[new(0,0),new(2,0),new(2,2),new(0,2),new(0,0)])
};
var rows=new List<object>();var guards=new List<string>();
foreach(var(name,points)in inputs)
{
 var source=LegacyMathAkSourceGeometry.CreateActual(ak,new[]{points});
 var v=source.PrepareUnguidedSingleStrokeTemporal();
 if(!ReferenceEquals(v.Source,source)||!source.OwnsTemporalFeatures(v))throw new Exception("issuer");
 rows.Add(new{name,original=points.Select(p=>new[]{p.X,p.Y}).ToArray(),normalized=source.CopyNormalizedStrokes()[0].Select(p=>new[]{p.X,p.Y}).ToArray(),
  samples=v.CopyTemporalPoints(),values=v.CopyValues(),v.FeatureCount,v.ModelStart,v.HasCandidateAuthority,v.HasClassifierOutput,guidance="absent"});
}
void Refuse(string name,Action action,Type type){try{action();throw new Exception("did not refuse");}catch(Exception e){if(e.GetType()!=type)throw;guards.Add(name);}}
var own=LegacyMathAkSourceGeometry.CreateActual(ak,new[]{inputs[0].Points});var vector=own.PrepareUnguidedSingleStrokeTemporal();
var foreign=LegacyMathAkSourceGeometry.CreateActual(ak,new[]{inputs[0].Points});
if(foreign.OwnsTemporalFeatures(vector))throw new Exception("foreign issuer");guards.Add("equal-data-foreign-issuer");
var values=vector.CopyValues();values[0]^=123;if(vector.CopyValues()[0]==values[0])throw new Exception("values alias");guards.Add("values-detached");
var samples=vector.CopyTemporalPoints();samples[0][0]=123456;if(vector.CopyTemporalPoints()[0][0]==123456)throw new Exception("points alias");guards.Add("points-detached");
Refuse("point-degenerate",()=>LegacyMathAkSourceGeometry.CreateActual(ak,new[]{new[]{Vector2.Zero}}).PrepareUnguidedSingleStrokeTemporal(),typeof(NotSupportedException));
Refuse("multistroke-outside-subset",()=>LegacyMathAkSourceGeometry.CreateActual(ak,new[]{inputs[0].Points,inputs[1].Points}).PrepareUnguidedSingleStrokeTemporal(),typeof(NotSupportedException));
Refuse("feature-point-cap",()=>LegacyMathAkSourceGeometry.CreateActual(ak,new[]{Enumerable.Range(0,4097).Select(i=>new Vector2(i,0)).ToArray()}).PrepareUnguidedSingleStrokeTemporal(),typeof(NotSupportedException));
using var c=new CancellationTokenSource();c.Cancel();try{own.PrepareUnguidedSingleStrokeTemporal(c.Token);throw new Exception("cancel missing");}catch(OperationCanceledException e){if(e.CancellationToken!=c.Token)throw;guards.Add("cancel-exact-token");}
var normMethod=typeof(LegacyMathAkSourceGeometry).GetMethod("NativeNorm",BindingFlags.NonPublic|BindingFlags.Static)??throw new Exception("norm binding");
var numericNorm=new uint[]{16383,16384,32000,65535,uint.MaxValue}.Select(value=>new{value,result=(int)(normMethod.Invoke(null,new object[]{value})??throw new Exception("norm return"))}).ToArray();
if(File.Exists(args[1]))throw new IOException("fresh capture required");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{rows,guards,numericNorm,modelExecutions=0,vendorExecution=false})+"\n");
Console.WriteLine($"PASS {rows.Count} source-derived temporal vectors / {guards.Count} guards / 0NN");
