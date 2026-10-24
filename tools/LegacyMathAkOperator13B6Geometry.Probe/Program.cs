using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("MODELS FIXTURES FRESH_CAPTURE");
if(File.Exists(args[2]))throw new IOException("Fresh capture required");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"math","math-ak.res"));
var profile=LegacyMathAkFeatureProfile.PrepareActual(resource);
var asm=typeof(LegacyMathAkSourceGeometry).Assembly;
Type pt=asm.GetType("InkRecognition.LegacyMathAkProcessedStrokePrefix",true)!,gt=asm.GetType("InkRecognition.LegacyMathAkOperator13B6Geometry",true)!;
object Invoke(MethodInfo m,object? owner,params object[] args){try{return m.Invoke(owner,args)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
MethodInfo pp=pt.GetMethod("Prepare",BindingFlags.Static|BindingFlags.NonPublic)!,gp=gt.GetMethod("Prepare",BindingFlags.Static|BindingFlags.NonPublic)!;
object Prefix(LegacyMathAkSourceGeometry o)=>Invoke(pp,null,o,CancellationToken.None);
object Prepare(object p,CancellationToken t=default)=>Invoke(gp,null,p,t);
T Get<T>(object g,string n)=>(T)gt.GetProperty(n,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(g)!;
T Copy<T>(object g,string n,CancellationToken t=default)=>(T)Invoke(gt.GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic)!,g,t);
Vector2[] Row(JsonElement x)=>x.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray();
float[][]?[] Coordinates(Vector2[]?[] rows)=>rows.Select(r=>r?.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));var cases=new List<object>();var joins=new List<object>();var guards=new List<string>();
void Guard(string n,bool ok){if(!ok)throw new InvalidOperationException(n);guards.Add(n);}
foreach(var c in input.RootElement.GetProperty("cases").EnumerateArray())
{
 var original=c.GetProperty("input").EnumerateArray().Select(Row).ToArray();var owner=profile.CreateGeometry(original);object prefix=Prefix(owner),g=Prepare(prefix);
 Guard("retains-genuine-prefix-"+(cases.Count+1),ReferenceEquals(Get<object>(g,"Prefix"),prefix));
 if(cases.Count==0)
 {
  var xy=Copy<Vector2[]?[]>(g,"CopyPolylines");var ids=Copy<int[]?[]>(g,"CopySelectedSourceIndices");
  xy[0]![0]=new(777,888);ids[0]![0]=999;
  Guard("actual-returned-copies-detached",Copy<Vector2[]?[]>(g,"CopyPolylines")[0]![0]!=xy[0]![0]&&Copy<int[]?[]>(g,"CopySelectedSourceIndices")[0]![0]!=ids[0]![0]);
  Guard("no-processed-graph-authority",!Get<bool>(g,"HasProcessedGraphAuthority"));
  using var ct=new CancellationTokenSource();ct.Cancel();bool cancel=false;
  try{_ = Prepare(prefix,ct.Token);}catch(OperationCanceledException e){cancel=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;}
  Guard("precancel-exact-type-token",cancel);
  cancel=false;try{_ = Copy<Vector2[]?[]>(g,"CopyPolylines",ct.Token);}catch(OperationCanceledException e){cancel=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;}
  Guard("copy-cancel-exact-type-token",cancel);
  var foreignProfile=LegacyMathAkFeatureProfile.PrepareActual(resource);var fo=foreignProfile.CreateGeometry(original);object fp=Prefix(fo),fg=Prepare(fp);
  Guard("foreign-profile-keeps-own-prefix",!ReferenceEquals(profile,foreignProfile)&&ReferenceEquals(Get<object>(fg,"Prefix"),fp)&&!ReferenceEquals(Get<object>(fg,"Prefix"),prefix));
  bool refused=false;try{Prepare(null!);}catch(ArgumentNullException){refused=true;}
  Guard("null-prefix-refused",refused);
 }
 cases.Add(new{name=c.GetProperty("name").GetString(),selectedSourceIndices=Copy<int[]?[]>(g,"CopySelectedSourceIndices"),polylines=Coordinates(Copy<Vector2[]?[]>(g,"CopyPolylines"))});
}
// Deliberately prepared NUMERICAL DATA only. Private Join creates no typed/source owner.
MethodInfo join=gt.GetMethod("Join",BindingFlags.Static|BindingFlags.NonPublic)!;
foreach(var c in input.RootElement.GetProperty("preparedJoinData").EnumerateArray())
{
 var segments=c.GetProperty("segments").EnumerateArray().Select(z=>new[]{Row(z)[0],Row(z)[1]}).ToArray();
 var output=(Vector2[])Invoke(join,null,segments.Select(z=>z[0]).ToArray(),segments.Select(z=>z[1]).ToArray(),CancellationToken.None);
 joins.Add(new{name=c.GetProperty("name").GetString(),points=output.Select(p=>new[]{p.X,p.Y}).ToArray(),numericDataOnly=true});
}
File.WriteAllText(args[2],JsonSerializer.Serialize(new{resourceSHA=Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant(),cases,preparedJoinData=joins,guards,portableDoubleTranscendentals=true,networkForwards=0},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {cases.Count} genuine-prefix scalar geometries/{joins.Count} numeric joins/{guards.Count} guards/0NN");
