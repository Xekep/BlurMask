using System.Text.Json;
using InkRecognition;
if(args.Length!=2||File.Exists(args[1]))throw new ArgumentException("prepared-cases fresh-capture");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[0]));
var output=new List<object>();
foreach(var c in input.RootElement.GetProperty("Cases").EnumerateArray())
{
 int[] counts=c.GetProperty("PointCounts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 var strokes=counts.Select(n=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(i,i+1)).ToArray())).ToArray();
 var document=LegacyOwnedRawInkDocument.Create(strokes,default);
 var map=new LegacyNativeTextSourceInputMap(document.CopySelectedInputs(),[],0);
 LegacyNativeInkPositionRange[] root=[new(LegacyNativeInkPosition.Pack(0,0,3,counts[0]),LegacyNativeInkPosition.Pack((uint)counts.Length-1,counts[^1]-1,5,counts[^1]))];
 LegacyOrdinaryGraphSourceReference[] Refs(string name)=>c.GetProperty(name).EnumerateArray().Select(r=>{var a=r.EnumerateArray().Select(v=>v.GetInt32()).ToArray();return new LegacyOrdinaryGraphSourceReference(new(a[0],a[1],a[2]),new(a[3],a[4],a[5]),checked((byte)a[6]),checked((byte)a[7]));}).ToArray();
 var word=LegacyEnglishCaseNode0Completion.ProjectCore(document,map,root,Refs("WordReferences"),default);
 var actual=Refs("GlyphReferences");var projected=LegacyEnglishCaseNode0Completion.ProjectCore(document,map,root,actual,default);
 var clipped=LegacyOwnedRawInkRangeOperations.Prepare(document).Intersect(projected,word);
 ulong[][] Pairs(LegacyNativeInkPositionRange[] v)=>v.Select(r=>new[]{r.First.Value,r.Last.Value}).ToArray();
 bool exactCancel=false;using var cancel=new CancellationTokenSource();cancel.Cancel();
 try{_=LegacyEnglishCaseNode0Completion.ProjectCore(document,map,root,actual,cancel.Token);}catch(OperationCanceledException e){exactCancel=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token;}
 if(!exactCancel)throw new Exception("Exact prepared cancellation required.");
 bool badGroup=false;try{_=LegacyEnglishCaseNode0Completion.ProjectCore(document,map,root,[new(new(1,0,0),new(1,0,0),0,0)],default);}catch(NotSupportedException){badGroup=true;}
 bool badPoint=false;try{_=LegacyEnglishCaseNode0Completion.ProjectCore(document,map,root,[new(new(0,0,counts[0]),new(0,0,counts[0]),0,0)],default);}catch(ArgumentException){badPoint=true;}
 if(!badGroup||!badPoint)throw new Exception("Prepared source limits not enforced.");
 output.Add(new{Id=c.GetProperty("Id").GetString(),OriginalRoot=Pairs(root),WordClip=Pairs(word),Projected=Pairs(projected),Clipped=Pairs(clipped),ExactCancel=exactCancel,BadGroup=badGroup,BadPoint=badPoint});
}
using var file=new FileStream(args[1],FileMode.CreateNew);JsonSerializer.Serialize(file,new{Scope="PREPARED_NEUTRAL_GEOMETRY_NOT_ACTUAL_EN_OWNER",Cases=output});
