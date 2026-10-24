using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("MODELS FIXTURES FRESH_CAPTURE");
if(File.Exists(args[2]))throw new IOException("Fresh capture required");
byte[] raw=File.ReadAllBytes(Path.Combine(args[0],"math","math-ak.res"));
var profile=LegacyMathAkFeatureProfile.PrepareActual(raw);
Type contextType=typeof(LegacyMathAkSourceGeometry).Assembly.GetType("InkRecognition.LegacyMathAkOriginalInputContext",true)!;
const BindingFlags PrivateInstance=BindingFlags.Instance|BindingFlags.NonPublic;
MethodInfo prepare=contextType.GetMethod("PrepareFresh",BindingFlags.Static|BindingFlags.NonPublic)!;
object Invoke(MethodInfo m,object? owner,params object[] values){try{return m.Invoke(owner,values)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
object Prepare(LegacyMathAkSourceGeometry source,CancellationToken token=default)=>Invoke(prepare,null,source,token);
T Get<T>(object c,string name)=>(T)contextType.GetProperty(name,PrivateInstance)!.GetValue(c)!;
T Copy<T>(object c,string name,params object[] args)=>(T)Invoke(contextType.GetMethod(name,PrivateInstance)!,c,args);
Vector2[] Row(JsonElement x)=>x.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray();
var guards=new List<string>();var captures=new List<object>();
void Guard(string name,bool ok){if(!ok)throw new InvalidOperationException(name);guards.Add(name);}
using var fixture=JsonDocument.Parse(File.ReadAllBytes(args[1]));
foreach(var c in fixture.RootElement.GetProperty("cases").EnumerateArray())
{
 var source=profile.CreateGeometry(c.GetProperty("input").EnumerateArray().Select(Row).ToArray());
 object context=Prepare(source);object frame=Get<object>(context,"OriginalInput");
 object prefix=Get<object>(context,"Prefix");
 Guard("same-prefix-frame-"+(captures.Count+1),ReferenceEquals(prefix.GetType().GetProperty("OriginalInput",PrivateInstance)!.GetValue(prefix),frame));
 Guard("no-authority-"+(captures.Count+1),!Get<bool>(context,"HasProcessedGraphAuthority")&&!Get<bool>(context,"HasMatcherRequestAuthority"));
 int n=Get<int>(context,"RangeResultCount");
 if(captures.Count==0)
 {
  using var ct=new CancellationTokenSource();ct.Cancel();bool cancelled=false;
  try{Prepare(source,ct.Token);}catch(OperationCanceledException e){cancelled=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;}
  Guard("precancel-exact-token",cancelled);cancelled=false;
  try{Copy<uint[]>(context,"CopyProcessedWords",ct.Token);}catch(OperationCanceledException e){cancelled=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;}
  Guard("copy-precancel-exact-token",cancelled);
  bool refused=false;try{Prepare(null!);}catch(ArgumentNullException){refused=true;}Guard("null-original-refused",refused);
 }
 var words=Copy<uint[]>(context,"CopyProcessedWords",CancellationToken.None);uint saved=words[0];words[0]^=uint.MaxValue;
 Guard("processed-copy-detached-"+(captures.Count+1),Copy<uint[]>(context,"CopyProcessedWords",CancellationToken.None)[0]==saved);
 for(int i=0;i<n;i++)
 {
  var ranges=Copy<byte[][]>(context,"CopyRangeRecords",i,CancellationToken.None);byte savedByte=ranges[0][0];ranges[0][0]^=255;
  Guard("ranges-detached-"+(captures.Count+1)+"-"+i,Copy<byte[][]>(context,"CopyRangeRecords",i,CancellationToken.None)[0][0]==savedByte);
  object foreign=Prepare(source);
  Array owners=(Array)contextType.GetField("results",PrivateInstance)!.GetValue(context)!;object range=owners.GetValue(i)!;
  bool refused=false;try{Invoke(range.GetType().GetMethod("Copy",PrivateInstance)!,range,foreign,CancellationToken.None);}catch(InvalidOperationException e){refused=e.Message=="Range owner is foreign or released.";}
  Guard("foreign-range-context-"+(captures.Count+1)+"-"+i,refused);
 }
 //Re-read genuine detached data after mutation controls; no expected-value writes.
 var records=Enumerable.Range(0,n).Select(i=>Copy<byte[][]>(context,"CopyRangeRecords",i,CancellationToken.None).Select(Convert.ToHexString).Select(s=>s.ToLowerInvariant()).ToArray()).ToArray();
 captures.Add(new{name=c.GetProperty("name").GetString(),processedWords=Copy<uint[]>(context,"CopyProcessedWords",CancellationToken.None),records,descriptorCount=Get<int>(context,"DescriptorCount"),rangeResultCount=n});
}
File.WriteAllText(args[2],JsonSerializer.Serialize(new{resourceSHA=Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),cases=captures,guards,networkForwards=0},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {captures.Count} connected source scalar cases/{guards.Count} guards/0NN");
