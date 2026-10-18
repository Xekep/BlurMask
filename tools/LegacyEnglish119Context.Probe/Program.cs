using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=4)throw new ArgumentException("models-directory controller-costs.json primary-controls.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));
var canceled=new CancellationToken(true);int guards=0,actualCalls=0,actualRows=0,actualSkips=0,preparedCalls=0,preparedRows=0,preparedSkips=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException or ProbeException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] bytes=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
LegacyOwnedEnglish119FirstPrimary Make(InkStroke[] raw)
{
 var candidates=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw),table);
 var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);
 return LegacyOwnedEnglish119FirstPrimary.Prepare(LegacyOwnedEnglish119ControllerCosts.Prepare(LegacyOwnedEnglish119GlyphConfidence.Prepare(cache),table),table);
}
int[] Snapshot(LegacyFirstBlankContextCorrection e)
{
 var r=e.Request;var p=e.Previous;var o=p.OriginalRecord;var g=p.GeometryRecord;
 return new[]{e.Stage,e.FinalPass?1:0,e.CurrentNode,e.CurrentPrimaryRow,(int)r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,
  p.OriginalNode,p.OriginalPrimaryRow,p.OriginalContextAlternative,o.ArcOrdinal,o.Symbol,o.CompositeCategory,o.Span,o.PreviousAlternative,o.ContinuationType,o.TotalCost,o.AcousticCost,o.PathBit?1:0,
  p.GeometryNode,p.GeometryPrimaryRow,p.GeometryContextAlternative,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,g.PreviousAlternative,g.ContinuationType,g.TotalCost,g.AcousticCost,g.PathBit?1:0,
  p.SkippedCount,e.IncomingCost,e.Result.Cost,e.Result.Accepted?1:0};
}
Records[] PrimaryRows(LegacyFirstOrdinaryAcousticRecurrence primary)=>Enumerable.Range(0,primary.NodeCount).Select(n=>new Records(primary.GetRowCopy(n).Count,primary.GetRowCopy(n).CopyStorage())).ToArray();
Records[][] Rows(LegacyFirstOrdinaryContextPasses pass,bool final)=>Enumerable.Range(0,pass.NodeCount).Select(n=>Enumerable.Range(0,pass.BeamWidth).Select(r=>
 {var row=final?pass.GetB8RowCopy(n,r):pass.GetB0RowCopy(n,r);return new Records(row.Count,row.CopyStorage());}).ToArray()).ToArray();
string Canonical(LegacyFirstOrdinaryContextPasses pass)
{
 var nodes=pass.Contexts.CopyNodes().ToDictionary(n=>n.Identity);string Seq(ulong id)=>id==0?"":Seq(nodes[id].ParentLink)+","+nodes[id].Symbol;
 var output=new List<string>();for(int final=0;final<2;final++)for(int n=0;n<pass.NodeCount;n++)for(int r=0;r<pass.BeamWidth;r++)
 {var row=final==0?pass.GetB0RowCopy(n,r):pass.GetB8RowCopy(n,r);for(int a=0;a<row.Count;a++){var f=row.Get(a);output.Add($"{final}:{n}:{r}:{a}:{f.TotalCost}:{f.AcousticCost}:{f.PreviousAlternative}:{f.ContinuationType}:{f.ArcOrdinal}:{f.Span}:{f.Symbol}:{f.PathBit}:{f.CompositeCategory}:{f.Traits}:{Seq(f.Identity)}");}}
 return string.Join("|",output);
}
var actual=new List<Actual>();bool failedPrefix=false;LegacyOwnedEnglish119ContextPasses? first=null;
foreach(var reference in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(reference.GetProperty("Raw"));var primary=Make(raw);var ownedPrimary=primary.Run();
 var live=primary.Owner.Owner.Owner.Owner.CopyLiveGroupIds();var cacheObjects=live.Select(g=>primary.Owner.ScoreGroup(g).ActualCache).ToArray();
 var owner=LegacyOwnedEnglish119ContextPasses.Prepare(ownedPrimary);first??=owner;
 int scored=primary.Owner.Owner.Owner.ScoredGroupCount;var calls=new List<int[]>();var observed=owner.RunWithObserver(e=>calls.Add(Snapshot(e)));var result=owner.Run();
 if(!ReferenceEquals(result,owner.Run())||!ReferenceEquals(result.Owner,owner)||!ReferenceEquals(owner.Primary,ownedPrimary)||scored!=primary.Owner.Owner.Owner.ScoredGroupCount||Canonical(observed.Passes)!=Canonical(result.Passes))throw new Exception("Context owner/reuse/numeric result changed.");
 if(live.Where((g,i)=>!ReferenceEquals(cacheObjects[i],primary.Owner.ScoreGroup(g).ActualCache)).Any())throw new Exception("Same actual NN/cache objects changed.");
 var b0=Rows(observed.Passes,false);var b8=Rows(observed.Passes,true);var a8=new List<Shortlist>();var costs=new List<int[][]>();
 for(int n=0;n<primary.NodeCount;n++)
 {
  var a=owner.GetA8(n);a8.Add(new(a.GroupCount,Enumerable.Range(0,a.GroupCount).Select(a.GetPrimaryOrdinal).ToArray(),Enumerable.Range(0,a.BeamWidth).Select(a.GetFiniteAlternativeCount).ToArray(),a.CopyStorage()));
  var row=ownedPrimary.Primary.GetRowCopy(n);var queries=new List<int[]>();for(int r=0;r<row.Count;r++)
  {var f=row.Get(r);if(n-f.Span-1<0)continue;queries.Add(new[]{(int)f.ArcOrdinal,f.Symbol,f.CompositeCategory,primary.Policy.Categories.CompositeCategories.Span[f.CompositeCategory].GeometryIndex,owner.ReadGroupCost(n,f.ArcOrdinal,f.Symbol,f.CompositeCategory,default)});}
  costs.Add(queries.ToArray());
 }
 actual.Add(new(raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),PrimaryRows(ownedPrimary.Primary),a8.ToArray(),b0,b8,observed.CopySymbolContexts().ToArray(),calls,costs.ToArray()));
 actualCalls+=calls.Count;actualRows+=b0.Sum(n=>n.Sum(r=>r.Count))+b8.Sum(n=>n.Sum(r=>r.Count));actualSkips+=calls.Count(c=>c[33]>0);
 Guard(()=>owner.Run(canceled));Guard(()=>owner.RunWithObserver(_=>{},canceled));Guard(()=>owner.RunWithObserver(null!));Guard(()=>LegacyOwnedEnglish119ContextPasses.Prepare(ownedPrimary,canceled));Guard(()=>owner.GetA8(-1));
 if(!failedPrefix&&calls.Count>20)
 {
  int seen=0;Guard(()=>owner.RunWithObserver(_=>{if(++seen==20)throw new ProbeException();}));if(seen!=20)throw new Exception("No actual failure prefix.");
  using var cts=new CancellationTokenSource();seen=0;Guard(()=>owner.RunWithObserver(_=>{if(++seen==20)cts.Cancel();},cts.Token));
  var recovery=owner.RunWithObserver(_=>{});if(Canonical(recovery.Passes)!=Canonical(result.Passes)||scored!=primary.Owner.Owner.Owner.ScoredGroupCount)throw new Exception("Failure changed valid result or repeated NN.");failedPrefix=true;
 }
 //Public row, shortlist and context-node snapshots do not expose mutable data.
 var storage=owner.GetA8(0).CopyStorage();if(storage.Length>0){storage[0]^=255;if(storage[0]==owner.GetA8(0).CopyStorage()[0])throw new Exception("Mutable A8 escaped.");}
 var copy=result.GetB8RowCopy(0,0);var original=copy.CopyStorage();if(copy.Count>0){copy.SetLikelihoodByte(0,17);if(result.GetB8RowCopy(0,0).CopyStorage()[8]!=original[8])throw new Exception("Mutable B8 escaped.");}
}
InkStroke V(float x)=>new(new InkPoint[]{new(x,10),new(x,30),new(x,60),new(x,90)});
var preparedOwner=Make(new[]{V(0),V(40),V(80),V(120)});var graph=preparedOwner.Policy.SourceRanges.Graph;var prepared=new List<Prepared>();
foreach(var control in controls.RootElement.EnumerateArray())
{
 bool header=control.GetProperty("rootHeader").GetBoolean(),blank1=control.GetProperty("blank1").GetBoolean(),blank2=control.GetProperty("blank2").GetBoolean();int[] values=control.GetProperty("costs").EnumerateArray().Select(e=>e.GetInt32()).ToArray();var req=new LegacyFirstOrdinaryGlyphRequest[4][];
 for(int n=0;n<4;n++)
 {ushort symbol=n==0&&header?(ushort)182:n==1&&blank1||n==2&&blank2?(ushort)217:(ushort)10;int c=preparedOwner.Policy.Categories.StartCompositeMap.Span[symbol==182?10:symbol];ushort a=(ushort)graph.GetNode(n).Select((a,i)=>(a,i)).First(x=>x.a.Span==0).i;req[n]=[new(a,0,symbol,(byte)c,values[n],1)];}
 var p=preparedOwner.Policy;var geo=preparedOwner.Geometry;var connections=new LegacyOrdinaryCandidateConnections(graph);
 var pri=LegacyFirstPrimaryRecurrenceKernel.Run(req,p.PrimaryBeamWidth,p.Blank,p.Replacement,p.Closed,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,p.ExcludeConsecutiveBlank,connections,geo.Correct118,geo.Correct140,null,default);
 var calls=new List<int[]>();var pass=LegacyFirstBlankContextKernel.Run(pri,graph,connections,p.PrimaryBeamWidth,1,1,p.Blank,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,p.Closed,p.NativeModeE0,p.ExcludeConsecutiveBlank,
  (n,_,_,_,_)=>values[n],geo.Correct118,geo.Correct140,e=>calls.Add(Snapshot(e)),default);
 var b0=Rows(pass,false);var b8=Rows(pass,true);prepared.Add(new(PrimaryRows(pri),b0,b8,pass.Contexts.CopyNodes().ToArray(),calls,values));
 preparedCalls+=calls.Count;preparedRows+=b0.Sum(n=>n.Sum(r=>r.Count))+b8.Sum(n=>n.Sum(r=>r.Count));preparedSkips+=calls.Count(c=>c[33]>0);
}
Guard(()=>LegacyOwnedEnglish119ContextPasses.Prepare(null!));if(!failedPrefix||actual.Count!=124||prepared.Count!=512||preparedSkips==0)throw new Exception("Missing actual/prepared coverage.");
File.WriteAllText(args[3],JsonSerializer.Serialize(new Output(path,model.Source.Fingerprint,actual,prepared,actualCalls,actualRows,actualSkips,preparedCalls,preparedRows,preparedSkips,guards),CaptureJson.Default.Output));
Console.WriteLine($"Owned119 context{actual.Count}/{actualRows}records/{actualCalls}calls/{actualSkips}ancestor; prepared{prepared.Count}/{preparedRows}records/{preparedCalls}calls/{preparedSkips}ancestor/{guards}guards; no terminal/text.");
record Records(int Count,byte[] Bytes);
record Shortlist(int Groups,int[] PrimaryOrdinals,int[] Counts,byte[] Bytes);
record Actual(float[][][] Raw,Records[] Primary,Shortlist[] A8,Records[][] B0,Records[][] B8,LegacySymbolContextNode[] Contexts,List<int[]> Calls,int[][][] Costs);
record Prepared(Records[] Primary,Records[][] B0,Records[][] B8,LegacySymbolContextNode[] Contexts,List<int[]> Calls,int[] Costs);
record Output(string Path,string Fingerprint,List<Actual> Actual,List<Prepared> Prepared,int ActualCalls,int ActualRows,int ActualSkips,int PreparedCalls,int PreparedRows,int PreparedSkips,int Guards);
sealed class ProbeException:Exception;
[JsonSerializable(typeof(Output))]partial class CaptureJson:JsonSerializerContext;
