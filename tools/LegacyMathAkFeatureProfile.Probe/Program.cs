using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=2 || File.Exists(args[1]))throw new ArgumentException("REPOSITORY FRESH_CAPTURE");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var profile=LegacyMathAkFeatureProfile.PrepareActual(resource);
var models=LegacyMathAkSelectedModels.PrepareActual(resource);
const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
short[][] Min(LegacyMathAkFeatureProfile p)=>(short[][])typeof(LegacyMathAkFeatureProfile).GetField("minimum",flags)!.GetValue(p)!;
short[][] Max(LegacyMathAkFeatureProfile p)=>(short[][])typeof(LegacyMathAkFeatureProfile).GetField("maximum",flags)!.GetValue(p)!;
string Hash(short[] a)=>Convert.ToHexString(SHA256.HashData(a.SelectMany(v=>new[]{(byte)v,(byte)(v>>8)}).ToArray())).ToLowerInvariant();
var before=Min(profile).Concat(Max(profile)).ToArray();var guards=new List<string>();
void Require(bool c,string name){if(!c)throw new Exception(name);guards.Add(name);}
foreach(string phase in new[]{"captured","authenticated","calibrated","publish"})
{
 using var cancellation=new CancellationTokenSource();bool reached=false,rejected=false;
 try{LegacyMathAkFeatureProfile.PrepareWithObserver(resource,p=>{if(p==phase){reached=true;cancellation.Cancel();}},cancellation.Token);}
 catch(OperationCanceledException e){rejected=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancellation.Token&&e.CancellationToken.IsCancellationRequested;}
 Require(reached&&rejected,"cancel-"+phase+"-exact-token");
}
bool bad=false;try{LegacyMathAkFeatureProfile.PrepareActual(resource.AsSpan(0,resource.Length-1));}catch(ArgumentException){bad=true;}Require(bad,"truncated-resource-refused");
var damaged=(byte[])resource.Clone();damaged[1006085]^=1;bad=false;try{LegacyMathAkFeatureProfile.PrepareActual(damaged);}catch(InvalidDataException){bad=true;}Require(bad,"changed-calibration-resource-refused");
var callerClone=(byte[])resource.Clone();
var capturedProfile=LegacyMathAkFeatureProfile.PrepareWithObserver(callerClone,p=>{if(p=="captured")Array.Clear(callerClone);});
Require(Min(capturedProfile).Concat(Max(capturedProfile)).Select(Hash).SequenceEqual(before.Select(Hash)),"caller-mutation-after-private-capture");
var source=new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)};
var countList=new ChangingCount(source);var geometry=profile.CreateGeometry(countList);Require(countList.Reads==1,"stroke-count-read-once");
var snapshot=geometry.CopyOriginalStrokes();source[0]=new(100,100);Require(geometry.CopyOriginalStrokes()[0][0]==snapshot[0][0],"caller-points-detached");
snapshot[0][0]=new(999,999);Require(geometry.CopyOriginalStrokes()[0][0]!=snapshot[0][0],"returned-points-detached");
var others=Enumerable.Range(0,128).Select(_=>profile.CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}})).ToArray();
Require(others.All(g=>ReferenceEquals(g.FeatureProfile,profile)),"all128-geometries-same-profile");
Require(before.Zip(Min(profile).Concat(Max(profile))).All(p=>ReferenceEquals(p.First,p.Second)),"eight-calibration-arrays-not-cloned");
Require(models.CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}}).FeatureProfile==models.FeatureProfile,"models-create-geometry-same-profile");
var foreign=LegacyMathAkFeatureProfile.PrepareActual(resource);var equal=foreign.CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
Require(!ReferenceEquals(foreign,profile)&&!geometry.OwnsTemporalFeatures(equal.PrepareUnguidedSingleStrokeTemporal()),"equal-data-foreign-source-cohort-refused");
var pcopy=profile.Program;pcopy=pcopy with{NormalizationTarget=1};Require(profile.Program.NormalizationTarget==8192,"program-value-copy-detached");
using(var cancellation=new CancellationTokenSource())
{
 cancellation.Cancel();bad=false;try{profile.CreateGeometry(new[]{source},cancellation.Token);}catch(OperationCanceledException e){bad=e.CancellationToken==cancellation.Token;}Require(bad,"create-pre-cancel-exact-token");
}
Array.Clear(resource);Require(profile.CreateGeometry(new[]{new[]{Vector2.Zero,new Vector2(1,1)}}).PointCount==2,"caller-resource-cleared-before-create");
Parallel.For(0,32,new ParallelOptions{MaxDegreeOfParallelism=2},i=>{var g=profile.CreateGeometry(new[]{new[]{new Vector2(i,0),new Vector2(i+2,1)}});if(g.FeatureProfile!=profile||Hash(Min(profile)[i%4])!=Hash(before[i%4]))throw new Exception("Parallel profile mutation");});guards.Add("parallel-profile-create-inspect");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{profile.Program,profile.CalibrationPayloadBytes,calibrations=Min(profile).Concat(Max(profile)).Select(a=>new{length=a.Length,sha256=Hash(a)}).ToArray(),guards,geometries=128,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS profile8arrays / {guards.Count} guards /128 shared geometries /0NN");
sealed class ChangingCount : IReadOnlyList<Vector2[]>
{
 private readonly Vector2[] row;public int Reads{get;private set;}public ChangingCount(Vector2[] row)=>this.row=row;
 public int Count=>++Reads==1?1:int.MaxValue;
 public Vector2[] this[int i]=>i==0?row:throw new IndexOutOfRangeException();
 public IEnumerator<Vector2[]> GetEnumerator()=>throw new NotSupportedException();System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
