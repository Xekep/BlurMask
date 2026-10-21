using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("MODELS ACTUAL-FAILED-CAPTURE OUTPUT");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));
byte[] actual=File.ReadAllBytes(args[1]);
using var profile=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-engine-profile.json")));
var av=profile.RootElement.GetProperty("activations");
var activation=new LegacyInkActivations(av.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),av.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var model=LegacyFirstAcousticModel.ReadSource(resource,6120);
var changed=resource.ToArray();changed[19195+33]^=1;
var foreign=LegacyFirstAcousticModel.ReadSource(changed,6120);Array.Fill(changed,(byte)0);
using var input=JsonDocument.Parse(actual);var rows=new List<Row>();int checks=0;
void Check(bool ok){checks++;if(!ok)throw new Exception($"Check{checks} failed.");}
void Reject<T>(Action a)where T:Exception{try{a();}catch(T){checks++;return;}throw new Exception("Expected "+typeof(T).Name);}
foreach(var item in input.RootElement.GetProperty("samples").EnumerateArray())
{
 var diag=item.GetProperty("diagnostic");var points=diag.GetProperty("normalizedShorts").GetString()!.Split('|').Select(x=>x.Split(',').Select(short.Parse).ToArray()).Select(p=>new LegacyInkPoint(p[0],p[1])).ToArray();
 var strokes=new[]{new LegacyInkStroke(points)};
 Check(LegacyRussianFirstDegenerateFeatures.TryCreate(model,strokes,default,out var t,out var r));
 Check(t.Length==217&&r.Length==199);
 var context=diag.GetProperty("context").GetString()!.Split(',').Select(short.Parse).ToArray();var signs=diag.GetProperty("boundaries").GetString()!.Split(',').Select(short.Parse).ToArray();
 var ti=t.Concat(context).Concat(signs).ToArray();var ri=r.Concat(context).Concat(signs).ToArray();
 var scores=model.EvaluatePreparedInputs(ti,ri,activation);var mask=Enumerable.Range(0,180).Select(i=>(byte)(i%3==0?0:1)).ToArray();
 rows.Add(new(item.GetProperty("id").GetString()!,points.Select(p=>new[]{p.X,p.Y}).ToArray(),context,signs,t.ToArray(),r.ToArray(),ti,ri,scores.TrajectoryProbability32768.ToArray(),scores.RasterProbability32768.ToArray(),scores.GetFreshIntegerCosts(),mask,scores.GetFreshIntegerCosts(mask)));
 Check(!LegacyRussianFirstDegenerateFeatures.TryCreate(foreign,strokes,default,out var ft,out var fr)&&ft.Length==0&&fr.Length==0);
 t[0]=123;r[0]=456;
 Check(LegacyRussianFirstDegenerateFeatures.TryCreate(model,strokes,default,out var t2,out var r2)&&t2[0]==0&&r2[0]==1024);
 Reject<ArgumentException>(()=>LegacyStrokeFeatures.Extract(strokes));
 using var cancel=new CancellationTokenSource();cancel.Cancel();Reject<OperationCanceledException>(()=>LegacyRussianFirstDegenerateFeatures.TryCreate(model,strokes,cancel.Token,out _,out _));
 Parallel.For(0,16,_=>{if(!LegacyRussianFirstDegenerateFeatures.TryCreate(model,strokes,default,out var ct,out var cr)||!ct.SequenceEqual(t2)||!cr.SequenceEqual(r2))throw new Exception("Concurrent independent vector mismatch.");ct[0]=12;});checks+=16;
}
var distinct=new[]{new LegacyInkStroke(new LegacyInkPoint[]{new(0,0),new(1,0)})};
Check(!LegacyRussianFirstDegenerateFeatures.TryCreate(model,distinct,default,out var dt,out var dr)&&dt.Length==0&&dr.Length==0);
Reject<ArgumentException>(()=>LegacyRussianFirstDegenerateFeatures.TryCreate(model,Array.Empty<LegacyInkStroke>(),default,out _,out _));
var one=new LegacyInkStroke(new LegacyInkPoint[]{new(0,0)});
Reject<ArgumentException>(()=>LegacyRussianFirstDegenerateFeatures.TryCreate(model,Enumerable.Repeat(one,16).ToArray(),default,out _,out _));
var huge=new LegacyInkStroke(Enumerable.Repeat(new LegacyInkPoint(0,0),4096).ToArray());
Reject<ArgumentException>(()=>LegacyRussianFirstDegenerateFeatures.TryCreate(model,new[]{huge,one},default,out _,out _));
Check(LegacyRussianFirstDegenerateFeatures.TryCreate(model,new[]{huge},default,out _,out _));
var empty=(LegacyInkStroke)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(LegacyInkStroke));
Reject<ArgumentException>(()=>LegacyRussianFirstDegenerateFeatures.TryCreate(model,new[]{empty},default,out _,out _));
Check(rows.Count==4);
File.WriteAllText(args[2],JsonSerializer.Serialize(new Capture(model.SourceFingerprint,Convert.ToHexStringLower(SHA256.HashData(actual)),false,checks,rows),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{checks} native-degenerate exact-owner/vector/foreign/cancel/bounds/concurrent checks;4 actual candidate inputs, no labels.");

internal sealed record Row(string id,short[][] points,short[] context,short[] boundaries,short[] trajectory,short[] raster,short[] trajectoryInput,short[] rasterInput,int[] trajectoryProbabilities,int[] rasterProbabilities,int[] integerCosts,byte[] mask,int[] maskedCosts);
internal sealed record Capture(string sourceFingerprint,string actualInputSha256,bool expectedLabelsRead,int checks,List<Row> rows);
[System.Text.Json.Serialization.JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:System.Text.Json.Serialization.JsonSerializerContext{}
