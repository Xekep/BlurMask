using System.Reflection;
using System.Text.Json;
using InkRecognition;
using System.Globalization;
if(args.Length==3 && args[0]=="--actual")
{
    if(File.Exists(args[2]))throw new IOException("Fresh output required");
    byte[] resource=File.ReadAllBytes(Path.Combine(args[1],"models/math/math-ak.res"));
    var xy=new[]{new System.Numerics.Vector2(0,0),new System.Numerics.Vector2(2,1),new System.Numerics.Vector2(4,0)};
    var source=LegacyMathAkSourceGeometry.CreateActual(resource,new[]{xy});
    var features=source.PrepareUnguidedSingleStrokeTemporal();
    var network=LegacyMathAkTemporalNetwork.PrepareActual(source,resource);
    var guards=new List<string>();
    using(var canceled=new CancellationTokenSource())
    {
        canceled.Cancel();
        try{network.EvaluateRaw(features,canceled.Token);throw new Exception("Missing pre-cancellation");}
        catch(OperationCanceledException e){if(e.CancellationToken!=canceled.Token)throw;guards.Add("pre-cancel-exact-token");}
    }
    var foreign=LegacyMathAkSourceGeometry.CreateActual(resource,new[]{xy});
    try{network.EvaluateRaw(foreign.PrepareUnguidedSingleStrokeTemporal());throw new Exception("Foreign source accepted");}
    catch(ArgumentException){guards.Add("equal-data-foreign-source-refused");}
    //The original caller resource can change after preparation; the numerical
    //owner must continue using its private authenticated weights.
    resource.AsSpan(1_007_739,1_151_090-1_007_739).Clear();
    var result=network.EvaluateNetworkCosts(features);
    guards.Add("caller-resource-detached-before-forward");
    var detachedRaw=result.CopyRawOutputs();int originalRaw=detachedRaw[0];detachedRaw[0]^=1;
    if(result.CopyRawOutputs()[0]!=originalRaw)throw new Exception("Raw output alias");guards.Add("raw-output-detached");
    var detachedProbability=result.CopyProbability32768();int originalProbability=detachedProbability[0];detachedProbability[0]^=1;
    if(result.CopyProbability32768()[0]!=originalProbability)throw new Exception("Probability output alias");guards.Add("probability-output-detached");
    File.WriteAllText(args[2],JsonSerializer.Serialize(new{scope="actual source-bound absent-guide1F only",modelExecutions=1,vendorExecution=false,
        guards,xy=xy.Select(p=>new[]{p.X,p.Y}).ToArray(),features=features.CopyValues(),result.ModelStart,
        hiddenDots=result.CopyHiddenDots(),hidden=result.CopyHiddenValues(),raw=result.CopyRawOutputs(),
        probability=result.CopyProbability32768(),costs=result.CopyNetworkCosts8192(),result.HasUnifiedAlphabetCosts,result.HasCandidateAuthority},new JsonSerializerOptions{WriteIndented=true}));
    Console.WriteLine("Actual1F numerical forward completed; no unifiedAL/candidate authority");return;
}
if(args.Length!=1||args[0].StartsWith("--",StringComparison.Ordinal))throw new ArgumentException("CAPTURE or explicit --actual REPOSITORY CAPTURE required.");
var output=args[0];if(File.Exists(output))throw new IOException("Fresh output required");
var t=typeof(LegacyMathAkTemporalNetwork);const BindingFlags flags=BindingFlags.Static|BindingFlags.NonPublic;
var dot16=t.GetMethod("Dot16",flags)!.CreateDelegate<Dot16>();
var dot8=t.GetMethod("Dot8",flags)!.CreateDelegate<Dot8>();
var scores=t.GetMethod("ConvertRaw",flags)!.CreateDelegate<Scores>();
var hidden=t.GetMethod("Hidden",flags)!.CreateDelegate<Func<int,short>>();
var rows=new List<object>();
void D16(string name,short[]x,short[]w,int at=0)=>rows.Add(new{name,mode=0,input=x,weights=w.Select(q=>(int)q).ToArray(),at,scale=1,result=dot16(x,w,at)});
void D8(string name,short[]x,sbyte[]w,byte scale,int at=0)=>rows.Add(new{name,mode=1,input=x,weights=w.Select(q=>(int)q).ToArray(),at,scale=(int)scale,result=dot8(x,w,at,scale)});
D16("signed",new short[]{-1024,1024,0,32767,-32768},new short[]{32767,-32768,2,32767,32767});
D16("pair-overflow",new short[]{-32768,-32768},new short[]{-32768,-32768});
D16("sum-wrap",Enumerable.Repeat((short)32767,222).ToArray(),Enumerable.Repeat((short)32767,222).ToArray());
D16("offset",new short[]{-1,2},new short[]{99,99,-3,4,99},2);
D8("signed-scale",new short[]{-32768,32767,1024},new sbyte[]{-128,127,-1},255);
D8("scale-wrap",Enumerable.Repeat((short)32767,201).ToArray(),Enumerable.Repeat((sbyte)127,201).ToArray(),255);
D8("offset",new short[]{-1,2},new sbyte[]{99,99,-3,4,99},17,2);
D8("scale-one",new short[]{1024,-1024},new sbyte[]{-128,127},1);
int[] points={int.MinValue+1,-(48<<16),-(47<<16)-1,-(47<<16),-65537,-65536,-65535,-1,0,1,65535,65536,65537,(47<<16),(47<<16)+1,(48<<16),int.MaxValue};
var h=points.Select(value=>new{value,result=(int)hidden(value)}).ToArray();bool refused=false;
try{hidden(int.MinValue);}catch(NotSupportedException e){refused=e.Message=="INT_MIN hidden lookup domain.";}
if(!refused)throw new Exception("INT_MIN refusal");
var scoreRows=new List<object>();
foreach(var input in new[]{new int[257],Enumerable.Range(0,257).Select(i=>i*1000000-128000000).ToArray(),Enumerable.Range(0,257).Select(i=>i%2==0?20000000:-20000000).ToArray()})
{var q=scores(input,default);scoreRows.Add(new{raw=input,probability=q.Probability,costs=q.Costs});}
File.WriteAllText(output,JsonSerializer.Serialize(new{rows,scoreRows,hidden=h,intMinRefused=refused,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS {rows.Count} synthetic dots / {h.Length} hidden controls / 0NN");
delegate int Dot16(ReadOnlySpan<short> input,ReadOnlySpan<short> weights,int at);
delegate int Dot8(ReadOnlySpan<short> input,ReadOnlySpan<sbyte> weights,int at,byte scale);

delegate (int[] Probability,int[] Costs) Scores(ReadOnlySpan<int> raw,CancellationToken token);
