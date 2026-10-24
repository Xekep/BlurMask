using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=3)throw new ArgumentException("MODELS ORIGINAL14_FIXTURES FRESH_CAPTURE");
if(File.Exists(args[2]))throw new IOException("Fresh capture required.");
Type contextType=typeof(LegacyMathAkSourceGeometry).Assembly.GetType("InkRecognition.LegacyMathAkOriginalInputContext",true)!;
MethodInfo prepare=contextType.GetMethod("PrepareFreshThrough13A2Diagnostic",BindingFlags.Static|BindingFlags.NonPublic)
    ??throw new MissingMethodException("PrepareFreshThrough13A2Diagnostic");
var fields=prepare.ReturnType.GetProperties(BindingFlags.Instance|BindingFlags.Public);
string[] expected=["Version","HeightBits","BeforePointBits","BeforeOriginalIndices","BeforeEnds","PointBits","OriginalIndices","Ends","MarkerFields","RangeFields","MetricBits","SamplerChargedBytes","SamplerChargedWork","DiagnosticChargedBytes","DiagnosticChargedWork"];
if(!fields.Select(x=>x.Name).SequenceEqual(expected)||fields.Any(x=>x.PropertyType!=typeof(int)&&x.PropertyType!=typeof(long)&&x.PropertyType!=typeof(int[])))
    throw new InvalidOperationException("Detached diagnostic DATA schema mismatch.");
object Invoke(LegacyMathAkSourceGeometry source,CancellationToken token)
{
    try{return prepare.Invoke(null,[source,token])!;}
    catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}
}
T Get<T>(object data,string name)=>(T)fields.Single(x=>x.Name==name).GetValue(data)!;
Vector2[][] Original(JsonElement c)
{
    if(c.TryGetProperty("input",out var input))return input.EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var r=c.GetProperty("recipe");int n=r.GetProperty("count").GetInt32();float step=r.GetProperty("step").GetSingle();bool horizontal=r.GetProperty("kind").GetString()=="horizontal";float[] pattern=[0f,.001f,0f,-.001f];
    return [Enumerable.Range(0,n).Select(i=>new Vector2(i*step,horizontal?0f:pattern[i%4])).ToArray()];
}
byte[] raw=File.ReadAllBytes(Path.Combine(args[0],"math","math-ak.res"));
var profile=LegacyMathAkFeatureProfile.PrepareActual(raw);
using var fixture=JsonDocument.Parse(File.ReadAllBytes(args[1]));
if(fixture.RootElement.GetProperty("cases").GetArrayLength()!=14)throw new InvalidOperationException("Original fourteen fixtures required.");
var guards=new List<string>();var cases=new List<object>();long retained=0;
void Guard(string name,bool value){if(!value)throw new InvalidOperationException(name);guards.Add(name);}
foreach(var c in fixture.RootElement.GetProperty("cases").EnumerateArray())
{
    string name=c.GetProperty("name").GetString()!;var source=profile.CreateGeometry(Original(c));
    if(cases.Count==0)
    {
        using var cancel=new CancellationTokenSource();cancel.Cancel();bool exact=false;
        try{Invoke(source,cancel.Token);}catch(OperationCanceledException e){exact=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token&&cancel.IsCancellationRequested;}
        Guard("fresh-precancel-exact-token",exact);
        bool nil=false;try{Invoke(null!,CancellationToken.None);}catch(ArgumentNullException){nil=true;}Guard("null-source-refused",nil);
    }
    object data=Invoke(source,CancellationToken.None);
    var before=Get<int[]>(data,"BeforeOriginalIndices");var points=Get<int[]>(data,"PointBits");var map=Get<int[]>(data,"OriginalIndices");
    var ends=Get<int[]>(data,"Ends");var markers=Get<int[]>(data,"MarkerFields");var ranges=Get<int[]>(data,"RangeFields");
    Guard("version-"+name,Get<int>(data,"Version")==1);
    Guard("same-source-shape-"+name,Get<int[]>(data,"BeforePointBits").Length==2*before.Length&&Get<int[]>(data,"BeforeEnds").Length==source.StrokeCount&&Get<int[]>(data,"BeforeEnds")[^1]==before.Length);
    Guard("current-counts-"+name,points.Length==2*map.Length&&ends.Length==source.StrokeCount&&ends[^1]==map.Length);
    Guard("marker-range-counts-"+name,markers.Length==ranges.Length&&markers.Length%3==0&&Get<int[]>(data,"MetricBits").Length==6*source.StrokeCount);
    long bytes=Get<long>(data,"DiagnosticChargedBytes"),work=Get<long>(data,"DiagnosticChargedWork");
    Guard("diagnostic-ledger-"+name,bytes==256+12L*before.Length+12L*map.Length+8L*markers.Length+32L*source.StrokeCount&&bytes<=64L*1024*1024&&work<=64L*1024*1024);
    Guard("sampler-ledger-"+name,Get<long>(data,"SamplerChargedBytes")<=128L*1024*1024&&Get<long>(data,"SamplerChargedWork")<=1_000_000_000L);
    if(bytes>128L*1024*1024-retained)throw new NotSupportedException("Probe retained diagnostic DATA cap exhausted.");retained+=bytes;
    cases.Add(new{name,data});
}
File.WriteAllText(args[2],JsonSerializer.Serialize(new{resourceSHA=Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),fixtureSHA=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))).ToLowerInvariant(),cases,guards,retainedDiagnosticBytes=retained,networkForwards=0,processedGraphAuthority=false,matcherRequestAuthority=false},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {cases.Count} fresh scalar sampler cases/{guards.Count} guards/0NN");
