using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Numerics;
using InkRecognition;
if(args.Length!=2 || File.Exists(args[1]))throw new ArgumentException("REPO FRESH_CAPTURE");
byte[] bytes=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var source=LegacyMathAkSourceGeometry.CreateActual(bytes,new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
var owner=LegacyMathAkSelectedModels.PrepareActual(bytes);
const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
var stored=(Array)typeof(LegacyMathAkSelectedModels).GetField("models",flags)!.GetValue(owner)!;
object[][] References()=>stored.Cast<object>().Select(m=>new[]{m.GetType().GetProperty("First")!.GetValue(m)!,m.GetType().GetProperty("Second")!.GetValue(m)!,m.GetType().GetProperty("Scales")!.GetValue(m)!}).ToArray();
var before=References();var guards=new List<string>();
void Require(bool condition,string name){if(!condition)throw new Exception(name);guards.Add(name);}
using(var cancel=new CancellationTokenSource())
{
 cancel.Cancel();bool ok=false;try{LegacyMathAkSelectedModels.PrepareActual(bytes,cancel.Token);}catch(OperationCanceledException e){ok=e.CancellationToken==cancel.Token;}Require(ok,"prepare-pre-cancel-token");
 ok=false;try{owner.BindTemporal(source,cancel.Token);}catch(OperationCanceledException e){ok=e.CancellationToken==cancel.Token;}Require(ok,"bind-pre-cancel-token");
}
bool nullRejected=false;try{owner.BindTemporal(null!);}catch(ArgumentNullException){nullRejected=true;}Require(nullRejected,"null-source-refused");
var bindings=Enumerable.Range(0,256).Select(_=>owner.BindTemporal(source)).ToArray();
Require(bindings.All(b=>ReferenceEquals(b.ModelOwner,owner)),"all-bindings-same-owner");
var foreign=LegacyMathAkSourceGeometry.CreateActual(bytes,new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
bool foreignRefused=false;try{bindings[0].EvaluateRaw(foreign.PrepareUnguidedSingleStrokeTemporal());}catch(ArgumentException){foreignRefused=true;}
Require(foreignRefused,"equal-data-foreign-features-refused-before-forward");
var after=References();Require(before.SelectMany(x=>x).Zip(after.SelectMany(x=>x)).All(p=>ReferenceEquals(p.First,p.Second)),"all-fifteen-weight-array-identities-stable");
Require(owner.Models is not LegacyMathAkSelectedModels.ModelDescriptor[],"descriptor-view-not-array");
bool readOnly=false;try{((IList<LegacyMathAkSelectedModels.ModelDescriptor>)owner.Models)[0]=default;}catch(NotSupportedException){readOnly=true;}Require(readOnly,"descriptor-view-readonly");
var bad=(byte[])bytes.Clone();bad[1007739]^=1;bool refused=false;try{LegacyMathAkSelectedModels.PrepareActual(bad);}catch(InvalidDataException){refused=true;}Require(refused,"foreign-resource-refused");
Array.Clear(bytes);
Require(References().SelectMany(x=>x).Zip(before.SelectMany(x=>x)).All(p=>ReferenceEquals(p.First,p.Second)),"caller-clear-does-not-replace-arrays");
string Hash(Array a)
{
 byte[] b=a switch{short[] s=>s.SelectMany(v=>new[]{(byte)v,(byte)(v>>8)}).ToArray(),sbyte[] s=>s.Select(v=>unchecked((byte)v)).ToArray(),byte[] s=>(byte[])s.Clone(),_=>throw new Exception()};
 return Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
}
var rows=owner.Models.Select((d,i)=>new{d.OriginalStart,d.Kind,d.InputWidth,d.FirstStride,d.SecondStride,hashes=before[i].Select(a=>Hash((Array)a)).ToArray(),lengths=before[i].Select(a=>((Array)a).Length).ToArray()}).ToArray();
var parallel=new string[32];Parallel.For(0,parallel.Length,new ParallelOptions{MaxDegreeOfParallelism=2},i=>parallel[i]=Hash((Array)References()[i%5][0]));
Require(parallel.Select((h,i)=>h==rows[i%5].hashes[0]).All(v=>v),"parallel-immutable-inspection");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{models=rows,owner.RetainedWeightBytes,guards,bindingCount=bindings.Length,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS five weight owners / {guards.Count} guards / 256 bindings / 0NN");
