using System.Buffers.Binary;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("MODELS ORIGINAL14_FIXTURES FRESH_CAPTURE");
if(File.Exists(args[2]))throw new IOException("Fresh capture required.");
var context=typeof(LegacyMathAkSourceGeometry).Assembly.GetType("InkRecognition.LegacyMathAkOriginalInputContext",true)!;
var prepare=context.GetMethod("PrepareFreshThrough13A2SegmentsDiagnostic",BindingFlags.Static|BindingFlags.NonPublic)??throw new MissingMethodException();
var fields=prepare.ReturnType.GetProperties(BindingFlags.Instance|BindingFlags.Public);
string[] schema=["Version","Sampler","RecordBytes","DefinedMasks","RangeTriples","OriginalOffsets","SegmentCount","TotalMembership","RequiredWorkWords","BridgeChargedBytes","BridgeChargedWork","DiagnosticChargedBytes","DiagnosticChargedWork"];
if(!fields.Select(x=>x.Name).SequenceEqual(schema))throw new InvalidOperationException("Segment DATA schema mismatch.");
var samplerType=fields.Single(x=>x.Name=="Sampler").PropertyType;
var samplerFields=samplerType.GetProperties(BindingFlags.Instance|BindingFlags.Public);
string[] samplerSchema=["Version","HeightBits","BeforePointBits","BeforeOriginalIndices","BeforeEnds","PointBits","OriginalIndices","Ends","MarkerFields","RangeFields","MetricBits","SamplerChargedBytes","SamplerChargedWork","DiagnosticChargedBytes","DiagnosticChargedWork"];
if(!samplerFields.Select(x=>x.Name).SequenceEqual(samplerSchema)||
    samplerFields.Any(x=>x.PropertyType!=typeof(int)&&x.PropertyType!=typeof(long)&&x.PropertyType!=typeof(int[]))||
    fields.Where(x=>x.Name!="Sampler").Any(x=>x.PropertyType!=typeof(int)&&x.PropertyType!=typeof(long)&&x.PropertyType!=typeof(int[])&&x.PropertyType!=typeof(byte[])))
    throw new InvalidOperationException("Detached DATA property types mismatch.");
object Invoke(LegacyMathAkSourceGeometry source,CancellationToken token)
{
    try{return prepare.Invoke(null,[source,token])!;}
    catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}
}
T Read<T>(object data,string name)=>(T)fields.Single(x=>x.Name==name).GetValue(data)!;
T Sampler<T>(object data,string name)=>(T)samplerFields.Single(x=>x.Name==name).GetValue(data)!;
Vector2[][] Original(JsonElement c)
{
    if(c.TryGetProperty("input",out var input))return input.EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var r=c.GetProperty("recipe");int n=r.GetProperty("count").GetInt32();float step=r.GetProperty("step").GetSingle();bool horizontal=r.GetProperty("kind").GetString()=="horizontal";float[] pattern=[0f,.001f,0f,-.001f];
    return [Enumerable.Range(0,n).Select(i=>new Vector2(i*step,horizontal?0f:pattern[i%4])).ToArray()];
}
var raw=File.ReadAllBytes(Path.Combine(args[0],"math","math-ak.res"));var profile=LegacyMathAkFeatureProfile.PrepareActual(raw);
using var fixture=JsonDocument.Parse(File.ReadAllBytes(args[1]));if(fixture.RootElement.GetProperty("cases").GetArrayLength()!=14)throw new InvalidOperationException("Original14 fixtures required.");
var cases=new List<object>();var guards=new List<string>();long retained=0;
void Guard(string name,bool ok){if(!ok)throw new InvalidOperationException(name);guards.Add(name);}
byte[] expectedMask=[255,255,0,255,241,255,255,15,0,0,0,0];
foreach(var c in fixture.RootElement.GetProperty("cases").EnumerateArray())
{
    string name=c.GetProperty("name").GetString()!;var source=profile.CreateGeometry(Original(c));
    if(cases.Count==0)
    {
        using var cancel=new CancellationTokenSource();cancel.Cancel();bool exact=false;
        try{Invoke(source,cancel.Token);}catch(OperationCanceledException e){exact=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token&&cancel.IsCancellationRequested;}
        Guard("fresh-precancel-exact-token",exact);bool nil=false;try{Invoke(null!,CancellationToken.None);}catch(ArgumentNullException){nil=true;}Guard("null-source-refused",nil);
    }
    object data=Invoke(source,CancellationToken.None),sampler=Read<object>(data,"Sampler");int m=Read<int>(data,"SegmentCount"),s=source.StrokeCount;
    var bytes=Read<byte[]>(data,"RecordBytes");var masks=Read<byte[]>(data,"DefinedMasks");var ranges=Read<int[]>(data,"RangeTriples");var starts=Read<int[]>(data,"OriginalOffsets");
    Guard("version-"+name,Read<int>(data,"Version")==1&&Sampler<int>(sampler,"Version")==1);
    Guard("segment-shape-"+name,m>=0&&bytes.Length==96*m&&masks.Length==12*m&&ranges.Length==3*m);
    Guard("original-membership-"+name,starts.Length==s+1&&starts[0]==0&&starts[^1]==m&&Read<int>(data,"TotalMembership")==m);
    bool membership=true;for(int originalId=0;originalId<s;originalId++)
    {
        if(starts[originalId]<0||starts[originalId]>starts[originalId+1]||starts[originalId+1]>m){membership=false;break;}
        for(int i=starts[originalId];i<starts[originalId+1];i++)membership&=ranges[3*i]==originalId;
    }
    Guard("membership-order-"+name,membership);Guard("required-word-count-"+name,Read<int>(data,"RequiredWorkWords")==((m>>5)+1));
    bool known=true,fieldsFinite=true;long visits=0;var ends=Sampler<int[]>(sampler,"Ends");
    for(int i=0;i<m;i++)
    {
        known&=masks.AsSpan(12*i,12).SequenceEqual(expectedMask);var row=bytes.AsSpan(96*i,96);known&=row[..16].IndexOfAnyExcept((byte)0)<0;
        known&=BinaryPrimitives.ReadInt32LittleEndian(row[0x18..])==1&&BinaryPrimitives.ReadInt32LittleEndian(row[0x1c..])==1&&row[0x20]<=1;
        for(int k=0;k<96;k++)if((expectedMask[k>>3]&(1<<(k&7)))==0)known&=row[k]==0;
        for(int k=0;k<6;k++)fieldsFinite&=float.IsFinite(BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(row[(0x24+4*k)..])));
        int originalId=ranges[3*i],start=ranges[3*i+1],end=ranges[3*i+2];int count=ends[originalId]-(originalId==0?0:ends[originalId-1]);
        fieldsFinite&=0<=start&&start<=end&&end<count;visits=checked(visits+end-start+1);
    }
    Guard("known-mask-and-header-"+name,known);Guard("finite-range-metrics-"+name,fieldsFinite);
    Guard("bridge-ledger-"+name,Read<long>(data,"BridgeChargedBytes")==152+48L*m+4L*(s+1)&&Read<long>(data,"BridgeChargedWork")==14L*m+5L*s+2*visits+21&&Read<long>(data,"BridgeChargedBytes")<=64L*1024*1024&&Read<long>(data,"BridgeChargedWork")<=64L*1024*1024);
    long paid=Read<long>(data,"DiagnosticChargedBytes");Guard("diagnostic-ledger-"+name,paid==128+120L*m+4L*(s+1)&&Read<long>(data,"DiagnosticChargedWork")==320L*m+16L*s+128&&paid<=64L*1024*1024&&Read<long>(data,"DiagnosticChargedWork")<=64L*1024*1024);
    long combined=checked(paid+Sampler<long>(sampler,"DiagnosticChargedBytes"));if(combined>128L*1024*1024-retained)throw new NotSupportedException("Probe detached DATA reservation exhausted.");retained+=combined;
    cases.Add(new{name,data});
}
File.WriteAllText(args[2],JsonSerializer.Serialize(new{resourceSHA=Convert.ToHexString(SHA256.HashData(raw)).ToLowerInvariant(),fixtureSHA=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))).ToLowerInvariant(),cases,guards,retainedDiagnosticBytes=retained,networkForwards=0,processedGraphAuthority=false,matcherRequestAuthority=false,workspaceAuthority=false},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {cases.Count} fresh segment scalar cases/{guards.Count} guards/0NN");
