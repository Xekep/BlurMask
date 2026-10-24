using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("MODELS FIXTURE_JSON FRESH_CAPTURE");
if (File.Exists(args[2])) throw new IOException("Fresh output required.");
byte[] resource = File.ReadAllBytes(Path.Combine(args[0], "math", "math-ak.res"));
var profile = LegacyMathAkFeatureProfile.PrepareActual(resource);
Type prefixType = typeof(LegacyMathAkSourceGeometry).Assembly.GetType("InkRecognition.LegacyMathAkProcessedStrokePrefix", true)!;
MethodInfo prepare = prefixType.GetMethod("Prepare", BindingFlags.Static|BindingFlags.NonPublic)!;
MethodInfo points = prefixType.GetMethod("CopyPoints", BindingFlags.Instance|BindingFlags.NonPublic)!;
MethodInfo indices = prefixType.GetMethod("CopyOriginalPointIndices", BindingFlags.Instance|BindingFlags.NonPublic)!;
MethodInfo strokeIds = prefixType.GetMethod("CopyOriginalStrokeIndices", BindingFlags.Instance|BindingFlags.NonPublic)!;
object Make(LegacyMathAkSourceGeometry owner, CancellationToken token = default)
{
    try { return prepare.Invoke(null, new object[]{owner, token})!; }
    catch (TargetInvocationException e) when(e.InnerException is not null)
    { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw(); throw; }
}
T Read<T>(object o, string name) => (T)prefixType.GetProperty(name, BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(o)!;
int[][] Bits(Vector2[][] rows) => rows.SelectMany(r=>r.Select(p=>new[]{BitConverter.SingleToInt32Bits(p.X),BitConverter.SingleToInt32Bits(p.Y)})).ToArray();
using var fixture = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var cases = new List<object>();var guards = new List<string>();
void Guard(string name, bool good) { if(!good)throw new InvalidOperationException(name); guards.Add(name); }
foreach(var item in fixture.RootElement.GetProperty("cases").EnumerateArray())
{
    Vector2[][] raw = item.GetProperty("input").EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var owner = profile.CreateGeometry(raw);
    var output = Make(owner);
    var actual = (Vector2[][])points.Invoke(output, new object[]{CancellationToken.None})!;
    var actualIndices = (int[][])indices.Invoke(output, new object[]{CancellationToken.None})!;
    var actualStrokeIds = (int[])strokeIds.Invoke(output, new object[]{CancellationToken.None})!;
    cases.Add(new { name=item.GetProperty("name").GetString(), scaleBits=BitConverter.SingleToInt32Bits(Read<float>(output,"EstimatedScale")), pointsBits=Bits(actual), ends=actual.Select(r=>r.Length).ToArray(), originalPointIndices=actualIndices.Select(r=>(int[])r.Clone()).ToArray(), originalStrokeIndices=(int[])actualStrokeIds.Clone(), expanded=Read<int>(output,"ExpandedStrokeCount"), pointCount=Read<int>(output,"PointCount") });
    Guard("same-original-owner-"+cases.Count, ReferenceEquals(Read<LegacyMathAkSourceGeometry>(output,"OriginalOwner"), owner));
    if(cases.Count==1)
    {
        actual[0][0]=new(777,888);actualIndices[0][0]=-99;actualStrokeIds[0]=-99;
        var again=(Vector2[][])points.Invoke(output,new object[]{CancellationToken.None})!;
        var againIndices=(int[][])indices.Invoke(output,new object[]{CancellationToken.None})!;
        var againIds=(int[])strokeIds.Invoke(output,new object[]{CancellationToken.None})!;
        Guard("detached-returned-arrays", again[0][0]!=actual[0][0]&&againIndices[0][0]==0&&againIds[0]==0);
        raw[0][0]=new(444,555);
        Guard("input-array-detached",owner.CopyOriginalStrokes()[0][0]==Vector2.Zero);
        using var cts = new CancellationTokenSource(); cts.Cancel();bool cancelled=false;
        try{_ = Make(owner,cts.Token);}catch(OperationCanceledException e){cancelled=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cts.Token;}
        Guard("precancel-exact-token",cancelled);
        var foreignProfile=LegacyMathAkFeatureProfile.PrepareActual(resource);
        var foreign=foreignProfile.CreateGeometry(new[]{new[]{Vector2.Zero}});var foreignOutput=Make(foreign);
        Guard("equal-data-distinct-profile-owner",!ReferenceEquals(profile,foreignProfile)&&ReferenceEquals(Read<LegacyMathAkSourceGeometry>(foreignOutput,"OriginalOwner"),foreign)&&!ReferenceEquals(Read<LegacyMathAkSourceGeometry>(foreignOutput,"OriginalOwner"),owner));
        Guard("no-graph-or-request-authority",!Read<bool>(output,"HasProcessedGraphAuthority")&&!Read<bool>(output,"HasMatcherRequestAuthority"));
    }
}
var maxRows=Enumerable.Range(0,64).Select(i=>new[]{new Vector2(i,0)}).ToArray();var maxOutput=Make(profile.CreateGeometry(maxRows));
Guard("maximum-strokes-expansion",Read<int>(maxOutput,"PointCount")==320&&Read<int>(maxOutput,"ExpandedStrokeCount")==64);
var maxPoints=Enumerable.Range(0,16384).Select(i=>new Vector2(i,0)).ToArray();var maxPointOutput=Make(profile.CreateGeometry(new[]{maxPoints}));
Guard("maximum-original-points",Read<int>(maxPointOutput,"PointCount")==16384);
bool refused=false;try{profile.CreateGeometry(new[]{new Vector2[16385]});}catch(ArgumentException){refused=true;}
Guard("over-original-point-bound",refused);
refused=false;try{profile.CreateGeometry(Enumerable.Range(0,65).Select(_=>new[]{Vector2.Zero}).ToArray());}catch(ArgumentException){refused=true;}
Guard("over-original-stroke-bound",refused);
// Reflection is diagnostic access only; all frames are constructed by the genuine private factory.
Type frameType=typeof(LegacyMathAkSourceGeometry).Assembly.GetType("InkRecognition.LegacyMathAkOriginalInputFrame",true)!;
MethodInfo createFrame=frameType.GetMethod("CreateFresh",BindingFlags.Static|BindingFlags.NonPublic)!;
object Frame(LegacyMathAkSourceGeometry o,CancellationToken t=default)
{
    try{return createFrame.Invoke(null,new object[]{o,t})!;}
    catch(TargetInvocationException e) when(e.InnerException is not null)
    {System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}
}
T F<T>(object f,string n)=>(T)frameType.GetProperty(n,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f)!;
T FC<T>(object f,string n,CancellationToken t=default)=>(T)frameType.GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{t})!;
var frames=new List<object>();var frameGuards=new List<string>();
void FG(string n,bool good){if(!good)throw new InvalidOperationException(n);frameGuards.Add(n);}
for(int n=1;n<=64;n++)
{
    var raw=Enumerable.Range(0,n).Select(i=>Enumerable.Range(0,(i*7+n)%3+1).Select(j=>new Vector2(i,j)).ToArray()).ToArray();
    var owner=profile.CreateGeometry(raw);object f=Frame(owner);
    int[] ends=FC<int[]>(f,"CopyEnds"), ids=FC<int[]>(f,"CopyStrokeIds"), reverse=FC<int[]>(f,"CopyStrokeById"), kinds=FC<int[]>(f,"CopyItemKinds");
    uint[] active=FC<uint[]>(f,"CopyActiveWords");
    bool scalar=true;for(int i=0;i<n;i++)
      scalar &= (int)frameType.GetMethod("ReadStrokeId",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{i})! == ids[i]
        && (int)frameType.GetMethod("ReadStrokeOrdinal",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{i})! == reverse[i]
        && (int)frameType.GetMethod("ReadItemKind",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{i})! == kinds[i]
        && (bool)frameType.GetMethod("IsActive",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{i})!;
    FG("scalar-readset-"+n,scalar&&ReferenceEquals(F<LegacyMathAkSourceGeometry>(f,"OriginalOwner"),owner));
    if(n==1)
    {
      // Mutate ACTUAL returned buffers, then check newly detached reads.
      ends[0]=999;ids[0]=999;reverse[0]=999;kinds[0]=999;active[0]=0;
      var xy=FC<Vector2[][]>(f,"CopyOriginalStrokes");xy[0][0]=new(999,999);
      FG("all-returned-buffers-detached",FC<int[]>(f,"CopyEnds")[0]==raw[0].Length&&FC<int[]>(f,"CopyStrokeIds")[0]==0&&FC<int[]>(f,"CopyStrokeById")[0]==0&&FC<int[]>(f,"CopyItemKinds")[0]==-1&&FC<uint[]>(f,"CopyActiveWords")[0]==1&&FC<Vector2[][]>(f,"CopyOriginalStrokes")[0][0]==raw[0][0]);
      object again=Frame(owner);
      FG("same-geometry-distinct-document",!ReferenceEquals(F<object>(f,"DocumentIdentity"),F<object>(again,"DocumentIdentity")));
      object prefix=Make(owner), retained=prefixType.GetProperty("OriginalInput",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(prefix)!;
      FG("prefix-retains-frame-document",ReferenceEquals(F<LegacyMathAkSourceGeometry>(retained,"OriginalOwner"),owner)&&ReferenceEquals(retained,prefixType.GetProperty("OriginalInput",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(prefix))&&!ReferenceEquals(retained,f));
      var foreignProfile=LegacyMathAkFeatureProfile.PrepareActual(resource);var foreignOwner=foreignProfile.CreateGeometry(raw);object foreign=Frame(foreignOwner);
      FG("equal-data-foreign-profile-document",!ReferenceEquals(F<object>(f,"DocumentIdentity"),F<object>(foreign,"DocumentIdentity"))&&ReferenceEquals(F<LegacyMathAkSourceGeometry>(foreign,"OriginalOwner"),foreignOwner)&&!ReferenceEquals(owner,foreignOwner));
      using var ct=new CancellationTokenSource();ct.Cancel();bool cancel=false;
      try{_ = Frame(owner,ct.Token);}catch(OperationCanceledException e){cancel=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==ct.Token;}
      FG("frame-precancel-exact-token",cancel);
      cancel=false;try{_ = FC<int[]>(f,"CopyStrokeIds",ct.Token);}catch(TargetInvocationException e)when(e.InnerException is OperationCanceledException oce){cancel=oce.GetType()==typeof(OperationCanceledException)&&oce.CancellationToken==ct.Token;}
      FG("copy-precancel-exact-token",cancel);
      bool bad=false;try{frameType.GetMethod("IsActive",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{-1});}catch(TargetInvocationException e)when(e.InnerException is ArgumentOutOfRangeException){bad=true;}
      FG("negative-id-refused",bad);
      bad=false;try{frameType.GetMethod("IsActive",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(f,new object[]{1});}catch(TargetInvocationException e)when(e.InnerException is ArgumentOutOfRangeException){bad=true;}
      FG("past-end-id-refused",bad);
    }
    // Publish only FRESH actual detached reads; mutation buffers are never patched for export.
    ends=FC<int[]>(f,"CopyEnds");ids=FC<int[]>(f,"CopyStrokeIds");reverse=FC<int[]>(f,"CopyStrokeById");kinds=FC<int[]>(f,"CopyItemKinds");active=FC<uint[]>(f,"CopyActiveWords");
    frames.Add(new{strokeCount=F<int>(f,"StrokeCount"),itemCount=F<int>(f,"ItemCount"),pointCount=F<int>(f,"PointCount"),activeWordCount=F<int>(f,"ActiveWordCount"),processedWordCount=F<int>(f,"ProcessedWordCount"),ends,ids,reverse,kinds,active});

}
var capture=new { resourceSHA=Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant(),raw139D=Convert.ToHexString(resource.AsSpan(7175,42)).ToLowerInvariant(),raw139F=Convert.ToHexString(resource.AsSpan(7217,20)).ToLowerInvariant(),cases,guards,frames,frameGuards,networkForwards=0,reflectionDiagnostics=true };
File.WriteAllText(args[2],JsonSerializer.Serialize(capture,new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"PASS {cases.Count} scalar cases/{guards.Count} old guards/{frames.Count} frames/{frameGuards.Count} frame guards/0 NN");
