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
const BindingFlags PI=BindingFlags.Instance|BindingFlags.NonPublic;
MethodInfo method=contextType.GetMethod("PrepareFreshThrough13A0",BindingFlags.Static|BindingFlags.NonPublic)??throw new MissingMethodException();
foreach(string name in new[]{"Copy13A0Points","Copy13A0OriginalIndices","CopyProcessedWords","CopyRangeRecords"})
 if(contextType.GetMethod(name,PI) is null)throw new MissingMethodException(name);
object Invoke(MethodInfo m,object? owner,params object[] values){try{return m.Invoke(owner,values)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
object Prepare(LegacyMathAkSourceGeometry source,CancellationToken token=default)=>Invoke(method,null,source,token);
T Get<T>(object o,string n)=>(T)o.GetType().GetProperty(n,PI)!.GetValue(o)!;
T Copy<T>(object o,string n,params object[] a)=>(T)Invoke(o.GetType().GetMethod(n,PI)!,o,a);
int[][][] Bits(Vector2[][] rows)=>rows.Select(r=>r.Select(p=>new[]{BitConverter.SingleToInt32Bits(p.X),BitConverter.SingleToInt32Bits(p.Y)}).ToArray()).ToArray();
Vector2[][] Original(JsonElement c){
 if(c.TryGetProperty("input",out var input))return input.EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
 var r=c.GetProperty("recipe");int n=r.GetProperty("count").GetInt32();float step=r.GetProperty("step").GetSingle();bool horizontal=r.GetProperty("kind").GetString()=="horizontal";float[] pattern=[0f,.001f,0f,-.001f];
 return [Enumerable.Range(0,n).Select(i=>new Vector2(i*step,horizontal?0f:pattern[i%4])).ToArray()];
}
var guards=new List<string>();var cases=new List<object>();var exceptions=new List<object>();
void Guard(string name,bool ok){if(!ok)throw new InvalidOperationException(name);guards.Add(name);}
void CancelGuard(string name,Action<CancellationToken> action){using var ct=new CancellationTokenSource();ct.Cancel();bool reached=false;try{reached=true;action(ct.Token);throw new InvalidOperationException("Expected cancellation");}catch(OperationCanceledException e){bool exact=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;Guard(name,reached&&exact&&ct.Token.IsCancellationRequested);exceptions.Add(new{name,reached,exactType=e.GetType().FullName,tokenMatches=e.CancellationToken==ct.Token,requested=ct.Token.IsCancellationRequested});}}
//Neutral private quota object only: no source/row/frame/owner can be issued here.
Type budgetType=contextType.GetNestedType("Source13A0Budget",BindingFlags.NonPublic)??throw new MissingMemberException();
object NewBudget()=>Activator.CreateInstance(budgetType,true)!;
object B(object owner,string name,params object[] a)=>Invoke(budgetType.GetMethod(name,PI)!,owner,a);
void RefuseBudget(string name,Action action,string expected){bool yes=false;try{action();}catch(NotSupportedException e){yes=e.GetType()==typeof(NotSupportedException)&&e.Message==expected;}Guard(name,yes);}
var quota=NewBudget();long limit=32L*1024*1024;
RefuseBudget("quota-one-byte-over",()=>B(quota,"Reserve",limit+1),"Original-source logical operation reservation exhausted.");
B(quota,"Reserve",limit-12);B(quota,"BeginCounting");B(quota,"CountOutput");
Guard("quota-exact-one-output",Get<long>(quota,"CountedOutput")==1);
RefuseBudget("quota-one-output-over",()=>B(quota,"CountOutput"),"Original-source exact output exceeds remaining reservation.");
Guard("quota-refusal-count-unchanged",Get<long>(quota,"CountedOutput")==1);
for(int i=0;i<14;i++)B(quota,"CountNode",CancellationToken.None);
RefuseBudget("quota-one-node-over",()=>B(quota,"CountNode",CancellationToken.None),"Original-source count traversal reservation exhausted.");
B(quota,"ReserveOutput");for(int i=0;i<14;i++)B(quota,"FillNode",CancellationToken.None);B(quota,"CompleteFill");
Guard("quota-count-fill-exact",true);
using var fixture=JsonDocument.Parse(File.ReadAllBytes(args[1]));
foreach(var c in fixture.RootElement.GetProperty("cases").EnumerateArray()){
 string name=c.GetProperty("name").GetString()!;var original=Original(c);var source=profile.CreateGeometry(original);object context=Prepare(source);
 object prefix=Get<object>(context,"Prefix"),frame=Get<object>(context,"OriginalInput");
 Guard("same-frame-"+name,ReferenceEquals(Get<object>(prefix,"OriginalInput"),frame));
 Guard("same-profile-"+name,ReferenceEquals(source.FeatureProfile,profile));
 Guard("no-authority-"+name,!Get<bool>(context,"HasProcessedGraphAuthority")&&!Get<bool>(context,"HasMatcherRequestAuthority"));
 var prefixPoints=Copy<Vector2[][]>(prefix,"CopyPoints",CancellationToken.None);var prefixMap=Copy<int[][]>(prefix,"CopyOriginalPointIndices",CancellationToken.None);
 var points=Copy<Vector2[][]>(context,"Copy13A0Points",CancellationToken.None);var maps=Copy<int[][]>(context,"Copy13A0OriginalIndices",CancellationToken.None);
 float first=points[0][0].X;points[0][0]=new(float.NaN,float.NaN);int firstMap=maps[0][0];maps[0][0]=-1;
 Guard("detached-points-"+name,BitConverter.SingleToInt32Bits(Copy<Vector2[][]>(context,"Copy13A0Points",CancellationToken.None)[0][0].X)==BitConverter.SingleToInt32Bits(first));
 Guard("detached-map-"+name,Copy<int[][]>(context,"Copy13A0OriginalIndices",CancellationToken.None)[0][0]==firstMap);
 if(cases.Count==0){
  CancelGuard("prepare-precancel-exact-token",t=>Prepare(source,t));
  CancelGuard("point-copy-precancel-exact-token",t=>Copy<Vector2[][]>(context,"Copy13A0Points",t));
  CancelGuard("map-copy-precancel-exact-token",t=>Copy<int[][]>(context,"Copy13A0OriginalIndices",t));
  bool refused=false;try{Prepare(null!);}catch(ArgumentNullException){refused=true;}Guard("null-original-refused",refused);
  object other=Prepare(profile.CreateGeometry(Original(c)));
  Guard("equal-data-distinct-frame",!ReferenceEquals(frame,Get<object>(other,"OriginalInput")));
  Guard("equal-data-distinct-document",!ReferenceEquals(Get<object>(frame,"DocumentIdentity"),Get<object>(Get<object>(other,"OriginalInput"),"DocumentIdentity")));
 }
 int rangeCount=Get<int>(context,"RangeResultCount");var records=Enumerable.Range(0,rangeCount).Select(i=>Copy<byte[][]>(context,"CopyRangeRecords",i,CancellationToken.None).Select(Convert.ToHexString).Select(s=>s.ToLowerInvariant()).ToArray()).ToArray();
 //All exported data is re-read from genuine owners AFTER mutation controls.
 cases.Add(new{name,originalPointCounts=original.Select(r=>r.Length).ToArray(),currentPrefixPointCounts=prefixPoints.Select(r=>r.Length).ToArray(),prefixPointBits=Bits(prefixPoints),prefixOriginalIndices=prefixMap,pointBits=Bits(Copy<Vector2[][]>(context,"Copy13A0Points",CancellationToken.None)),originalIndices=Copy<int[][]>(context,"Copy13A0OriginalIndices",CancellationToken.None),processedWords=Copy<uint[]>(context,"CopyProcessedWords",CancellationToken.None),records,descriptorCount=Get<int>(context,"DescriptorCount"),rangeResultCount=rangeCount});
}
//Six separate genuine fresh owners; scalar only, no NN. The view never exposes Context.
var diagnosticResults=new List<object>();
MethodInfo diagnosticPrepare=contextType.GetMethod("PrepareFreshThrough13A0Diagnostic",BindingFlags.Static|BindingFlags.NonPublic)??throw new MissingMethodException();
Type stageType=contextType.GetNestedType("Source13A0Checkpoint",BindingFlags.NonPublic)!,viewType=contextType.GetNestedType("Source13A0Diagnostic",BindingFlags.NonPublic)!;
MethodInfo delegateBuilder=typeof(DiagnosticDelegateFactory).GetMethod("Create")!.MakeGenericMethod(stageType,viewType);
JsonElement diagnosticFixture=fixture.RootElement.GetProperty("cases").EnumerateArray().Single(c=>c.GetProperty("name").GetString()=="prior-range-owner-retained");
foreach(string wanted in new[]{"After13B6","AfterCountAndReservation","AfterFillBeforePublication"})foreach(string mode in new[]{"cancel","fault"}){
 using var ct=new CancellationTokenSource();object? view=null,firstView=null,before=null;bool reached=false,sameView=true;var stages=new List<string>();
 Action<object,object> callback=(stage,actualView)=>{
  firstView??=actualView;sameView&=ReferenceEquals(firstView,actualView);stages.Add(stage.ToString()!);
  if(stage.ToString()!=wanted)return;
  reached=true;view=actualView;before=Copy<object>(view,"Snapshot");
  if(mode=="cancel")ct.Cancel();else throw new InvalidOperationException("bounded diagnostic fault");
 };
 Delegate observer=(Delegate)delegateBuilder.Invoke(null,[callback])!;Exception? actual=null;
 try{Invoke(diagnosticPrepare,null,profile.CreateGeometry(Original(diagnosticFixture)),observer,ct.Token);}catch(Exception e){actual=e;}
 string suffix=wanted+"-"+mode;
 bool exact=mode=="cancel"?actual?.GetType()==typeof(OperationCanceledException)&&((OperationCanceledException)actual).CancellationToken==ct.Token&&ct.IsCancellationRequested:
  actual?.GetType()==typeof(InvalidOperationException)&&actual.Message=="bounded diagnostic fault";
 Guard("diagnostic-throw-"+suffix,reached&&exact&&view is not null&&before is not null&&sameView);
 object after=Copy<object>(view!,"Snapshot");
 int Scalar(object snap,string name)=>(int)snap.GetType().GetProperty(name)!.GetValue(snap)!;
 bool Published(object snap)=>(bool)snap.GetType().GetProperty("SourcePublished")!.GetValue(snap)!;
 bool prefixSame=new[]{"DescriptorCount","RangeResultCount","OriginalPointCount","PrefixPointCount","ProcessedWordCount","Word0","Word1","Word2"}.All(n=>Equals(before!.GetType().GetProperty(n)!.GetValue(before),after.GetType().GetProperty(n)!.GetValue(after)));
 Guard("diagnostic-poison-"+suffix,Scalar(before!,"Phase")==2&&Scalar(before!,"Claim")==1&&Scalar(after,"Phase")==-1&&Scalar(after,"Claim")==1&&!Published(before!)&&!Published(after)&&prefixSame);
 bool foreign=Task.Run(()=>{try{Copy<object>(view!,"Snapshot");return false;}catch(InvalidOperationException e){return e.GetType()==typeof(InvalidOperationException)&&e.Message=="Source13A0 diagnostic issuer thread mismatch.";}}).GetAwaiter().GetResult();
 Guard("diagnostic-foreign-thread-"+suffix,foreign);
 object third=Copy<object>(view!,"Snapshot"),fourth=Copy<object>(view!,"Snapshot");
 Guard("diagnostic-four-snapshots-"+suffix,JsonSerializer.Serialize(after)==JsonSerializer.Serialize(third)&&JsonSerializer.Serialize(after)==JsonSerializer.Serialize(fourth));
 bool exhausted=false;try{Copy<object>(view!,"Snapshot");}catch(NotSupportedException e){exhausted=e.GetType()==typeof(NotSupportedException)&&e.Message=="Source13A0 diagnostic snapshot reservation exhausted.";}
 Guard("diagnostic-fifth-refused-"+suffix,exhausted);
 diagnosticResults.Add(new{stage=wanted,mode,reached,sameView,stages,before,after,exceptionType=actual!.GetType().FullName,message=mode=="fault"?actual.Message:"",tokenMatches=actual is OperationCanceledException oce&&oce.CancellationToken==ct.Token,tokenRequested=ct.IsCancellationRequested});
}
File.WriteAllText(args[2],JsonSerializer.Serialize(new{resourceSHA=Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),cases,guards,exceptions,diagnosticResults,networkForwards=0},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {cases.Count} source13A0 scalar cases/{guards.Count} guards/0NN");

static class DiagnosticDelegateFactory
{
 public static Delegate Create<TStage,TView>(Action<object,object> callback)
  =>new Action<TStage,TView>((stage,view)=>callback(stage!,view!));
}
