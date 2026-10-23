using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("REPOSITORY CAPTURE");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var inputs=new (string Name,Vector2[][] Strokes)[]
{
 ("point", [[new(5,-7)]]),
 ("horizontal-half", [[new(100,20),new(100.5f,20),new(102,20)]]),
 ("vertical", [[new(-3,-8),new(-3,2)]]),
 ("nonsquare", [[new(-4,-2),new(4,2),new(0,0)]]),
 ("fractional", [[new(-.75f,-.125f),new(.125f,.875f),new(.5f,.25f)]]),
 ("two-strokes", [[new(0,0),new(10,20)],[new(5,5),new(15,25)]]),
 ("duplicates-retained", [[new(7,8),new(7,8),new(10,10)]])
};
var cases=new List<object>();
foreach(var (name,strokes) in inputs)
{
 var o=LegacyMathAkSourceGeometry.CreateActual(resource,strokes);
 cases.Add(new {name,original=strokes.Select(s=>s.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),
  normalized=o.CopyNormalizedStrokes().Select(s=>s.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),
  ends=o.CopyCumulativeEnds(),scale=o.AppliedScale,min=new[]{o.OriginalMinimum.X,o.OriginalMinimum.Y},max=new[]{o.OriginalMaximum.X,o.OriginalMaximum.Y},
  o.PointCount,o.StrokeCount,o.NormalizationTarget,o.HasCandidateAuthority,o.HasClassifierOutput});
}
var guards=new List<string>();
var changing=new ChangingCountInput();
var countSnapshot=LegacyMathAkSourceGeometry.CreateActual(resource,changing);
if(changing.Calls!=1||countSnapshot.StrokeCount!=1)throw new Exception("unstable count admission");
guards.Add("count-read-once-before-allocation");
void Guard(string n,Action a,Type t){try{a();throw new Exception("did not refuse "+n);}catch(Exception e){if(e.GetType()!=t)throw;guards.Add(n);}}
var original=new[]{new[]{new Vector2(1,2),new Vector2(3,4)}};
var owned=LegacyMathAkSourceGeometry.CreateActual(resource,original);original[0][0]=new(999,999);
if(owned.CopyOriginalStrokes()[0][0]!=new Vector2(1,2))throw new Exception("caller mutation");guards.Add("caller-input-detached");
var detached=owned.CopyNormalizedStrokes();detached[0][0]=new(123,456);
if(owned.CopyNormalizedStrokes()[0][0]==new LegacyInkPoint(123,456))throw new Exception("output mutation");guards.Add("returned-output-detached");
var ends=owned.CopyCumulativeEnds();ends[0]=0;if(owned.CopyCumulativeEnds()[0]!=2)throw new Exception("ends mutation");guards.Add("ends-detached");
var changed=(byte[])resource.Clone();changed[1005996]=0;
Guard("resource-mutation",()=>LegacyMathAkSourceGeometry.CreateActual(changed,original),typeof(InvalidDataException));
Guard("resource-truncated",()=>LegacyMathAkSourceGeometry.CreateActual(resource.AsSpan(1),original),typeof(ArgumentException));
Guard("empty-source",()=>LegacyMathAkSourceGeometry.CreateActual(resource,Array.Empty<Vector2[]>()),typeof(ArgumentException));
Guard("empty-stroke",()=>LegacyMathAkSourceGeometry.CreateActual(resource,new[]{Array.Empty<Vector2>()}),typeof(ArgumentException));
Guard("nan-source",()=>LegacyMathAkSourceGeometry.CreateActual(resource,new[]{new[]{new Vector2(float.NaN,0)}}),typeof(ArgumentException));
Guard("point-over-cap",()=>LegacyMathAkSourceGeometry.CreateActual(resource,new[]{new Vector2[16385]}),typeof(ArgumentException));
Guard("stroke-over-cap",()=>LegacyMathAkSourceGeometry.CreateActual(resource,Enumerable.Range(0,65).Select(_=>new[]{Vector2.Zero}).ToArray()),typeof(ArgumentException));
Guard("line-short-overflow",()=>LegacyMathAkSourceGeometry.CreateActual(resource,new[]{new[]{Vector2.Zero,new Vector2(40000,0)}}),typeof(NotSupportedException));
using var c=new CancellationTokenSource();c.Cancel();
try{LegacyMathAkSourceGeometry.CreateActual(resource,original,c.Token);throw new Exception("cancel missing");}
catch(OperationCanceledException e){if(e.CancellationToken!=c.Token)throw;guards.Add("cancel-exact-token");}
var maximum=LegacyMathAkSourceGeometry.CreateActual(resource,new[]{Enumerable.Range(0,16384).Select(i=>new Vector2(i,0)).ToArray()});
if(maximum.PointCount!=16384||maximum.CopyCumulativeEnds()[0]!=16384)throw new Exception("maximum");guards.Add("point-cap-positive");
var text=JsonSerializer.Serialize(new{resourceSHA256=Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant(),cases,guards,modelExecutions=0,vendorExecution=false});
if(File.Exists(args[1]))throw new IOException("fresh capture required");File.WriteAllText(args[1],text+"\n");Console.WriteLine($"PASS {cases.Count} geometry cases / {guards.Count} guards / 0 model executions");

sealed class ChangingCountInput : IReadOnlyList<Vector2[]>
{
    public int Calls { get; private set; }
    public int Count => ++Calls == 1 ? 1 : 65;
    public Vector2[] this[int index] => [new(0,0),new(1,1)];
    public IEnumerator<Vector2[]> GetEnumerator() => throw new InvalidOperationException("Enumeration is not the bounded input protocol.");
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
