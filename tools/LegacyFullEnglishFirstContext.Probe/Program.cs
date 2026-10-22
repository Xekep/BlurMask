using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("models blind-inputs capture");
var models=args[0];var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));int guards=0,cancels=0;
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal.");}
var ink=new[]{new InkStroke(Enumerable.Range(0,32).Select(i=>new InkPoint(10*i,(float)(40*Math.Sin(i*1.3)))).ToArray())};
LegacyFullEnglishFirstController B1(IReadOnlyList<InkStroke>? strokes=null){var p=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes??ink,0,m30,m50);var c=LegacyFullEnglishFirstController.Prepare(p);c.Run(table);return c;}
var b1=B1();var b2=LegacyFullEnglishFirstControllerCosts.Prepare(b1,table);b2.Run();
LegacyFullEnglishFirstPrimary B3(LegacyFullEnglishFirstControllerCosts? source=null){var p=LegacyFullEnglishFirstPrimary.Prepare(source??b2);p.Run();return p;}
var good=B3();var owner=LegacyFullEnglishFirstContextPasses.Prepare(good);Guard(()=>owner.Run(new CancellationToken(true)));if(owner.IsFaulted||owner.IsComplete)throw new Exception("Preentry consumed owner");owner.Run();Guard(()=>owner.Run());
Guard(()=>LegacyFullEnglishFirstContextPasses.Prepare(null!));Guard(()=>LegacyFullEnglishFirstContextPasses.Prepare(good,new CancellationToken(true)));
var exact=LegacyFullEnglishFirstContextPasses.PrepareBounded(good,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork);
Guard(()=>LegacyFullEnglishFirstContextPasses.PrepareBounded(good,owner.AdditionalLogicalPeakBytes-1,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork));Guard(()=>LegacyFullEnglishFirstContextPasses.PrepareBounded(good,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes-1,owner.LogicalVisitWork));Guard(()=>LegacyFullEnglishFirstContextPasses.PrepareBounded(good,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork-1));
foreach(var x in new[]{(4097,1,1,1,1),(1,4097,1,1,1),(1,1,4097,1,1),(1,1,1,1000001,1),(1,1,1,1,32769),(-1,1,1,1,1)})Guard(()=>LegacyFullEnglishFirstContextBudget.Envelope(x.Item1,x.Item2,x.Item3,x.Item4,x.Item5));
var maximum=LegacyFullEnglishFirstContextBudget.Envelope(4096,4096,4096,1000000,32768);if(maximum.Peak!=66331648||maximum.Work!=250492416)throw new Exception("Maximum envelope arithmetic drift");
Guard(()=>LegacyFullEnglishFirstContextPasses.Prepare(LegacyFullEnglishFirstPrimary.Prepare(b2)));
var incomplete=LegacyFullEnglishFirstPrimary.Prepare(b2);Guard(()=>incomplete.RunPrepared(s=>{if(s=="child:published")throw new InvalidDataException("Injected");},null));if(!incomplete.IsFaulted||incomplete.CopyCompletedChildren().Count==0)throw new Exception("No upstream historical prefix");Guard(()=>LegacyFullEnglishFirstContextPasses.Prepare(incomplete));
var policy=new LegacyFullEnglishFirstContextPolicy(good,good.CopyCompletedChildren()[0],default);var foreign=B3();Guard(()=>policy.Bind(foreign.CopyCompletedChildren()[0],default));Guard(()=>new LegacyFullEnglishFirstContextPolicy(good,foreign.CopyCompletedChildren()[0],default));
foreach(var c in owner.CopyCompletedChildren())
{
 if(!ReferenceEquals(c.Owner,owner)||!ReferenceEquals(c.Primary.Owner,good))throw new Exception("Detached context owner");var a=c.GetA8(0);var data=a.CopyStorage();byte old=data[0];data[0]^=255;if(a.CopyStorage()[0]!=old)throw new Exception("Mutable A8");
 for(int final=0;final<2;final++)for(int n=0;n<c.NodeCount;n++)for(int ri=0;ri<5;ri++){var row=final==0?c.GetB0RowCopy(n,ri):c.GetB8RowCopy(n,ri);if(row.Count==0)continue;byte before=row.GetLikelihoodByte(0);row.SetLikelihoodByte(0,(byte)(before^255));if((final==0?c.GetB0RowCopy(n,ri):c.GetB8RowCopy(n,ri)).GetLikelihoodByte(0)!=before)throw new Exception("Mutable context row");}
 var nodes=c.CopySymbolContexts().ToArray();if(nodes.Length>0){var oldNode=nodes[0];nodes[0]=oldNode with{Symbol=65535};if(c.CopySymbolContexts()[0]!=oldNode)throw new Exception("Mutable context node");}var pages=c.CopyContextPageUsedCounts();if(pages.Length>0){int saved=pages[0];pages[0]++;if(c.CopyContextPageUsedCounts()[0]!=saved)throw new Exception("Mutable page counts");}
 Guard(()=>c.GetA8(-1));Guard(()=>c.GetB0RowCopy(-1,0));
}
foreach(string stage in new[]{"entry","child:entry","a8","context","child:published"})
{var p=LegacyFullEnglishFirstContextPasses.Prepare(good);using var cts=new CancellationTokenSource();Guard(()=>p.RunPrepared(s=>{if(s==stage)cts.Cancel();},null,cts.Token));if(!p.IsFaulted||p.IsComplete)throw new Exception("Entered cancel not poisoned");Guard(()=>p.Run());cancels++;foreach(var c in p.CopyCompletedChildren())_=c.CopySymbolContexts();}
var callbackFault=LegacyFullEnglishFirstContextPasses.Prepare(good);Guard(()=>callbackFault.RunPrepared(null,_=>throw new InvalidDataException("Injected")));if(!callbackFault.IsFaulted)throw new Exception("Callback failure not poisoned");Guard(()=>callbackFault.Run());
var callbackCancel=LegacyFullEnglishFirstContextPasses.Prepare(good);using(var cts=new CancellationTokenSource()){Guard(()=>callbackCancel.RunPrepared(null,_=>cts.Cancel(),cts.Token));if(!callbackCancel.IsFaulted)throw new Exception("Callback cancel not poisoned");Guard(()=>callbackCancel.Run());cancels++;}
var multiInk=ink.Concat(ink.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();var multiB2=LegacyFullEnglishFirstControllerCosts.Prepare(B1(multiInk),table);multiB2.Run();var multi=LegacyFullEnglishFirstContextPasses.Prepare(B3(multiB2));using(var cts=new CancellationTokenSource()){int count=0;Guard(()=>multi.RunPrepared(s=>{if(s=="child:entry"&&++count==2)cts.Cancel();},null,cts.Token));if(!multi.IsFaulted||multi.CopyCompletedChildren().Count!=1)throw new Exception("No historical child prefix");cancels++;}
var concurrent=LegacyFullEnglishFirstContextPasses.Prepare(good);using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{var task=Task.Run(()=>concurrent.RunPrepared(s=>{if(s=="entry"){entered.Set();release.Wait();}},null));if(!entered.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Barrier timeout");try{Guard(()=>concurrent.Run());_=concurrent.CopyCompletedChildren();}finally{release.Set();}task.GetAwaiter().GetResult();}
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","Actual full-FIRST A8/B0/B8 only; no terminal/WORD");w.WriteBoolean("TargetsRead",false);w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);
void Ints(IEnumerable<int> values){w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Floats(IEnumerable<float> values){w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();int accepted=0,refused=0,groups=0,arcs=0;w.WriteStartArray("Actual");
foreach(var item in items)
{
 var raw=item.TryGetProperty("Raw",out var r)?r:item.GetProperty("strokes");string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;float resolution=item.TryGetProperty("Resolution",out var rr)?rr.GetSingle():0;var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();w.WriteStartObject();w.WriteString("Name",name);
 try
 {
  var parent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);var confidence=LegacyFullEnglishFirstController.Prepare(parent);confidence.Run(table);int nn=parent.NetworkEvaluationCount;var c=LegacyFullEnglishFirstControllerCosts.Prepare(confidence,table);c.Run();if(!c.IsComplete||c.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("B2 completion or NN replay drift.");
  var trace=new List<(string Hash,long Count,List<(long Index,int[] Data)> Witness)>();using var accumulator=new TraceAccumulator();
  var producer=LegacyFullEnglishFirstPrimary.Prepare(c);producer.RunPrepared(stage=>{if(stage=="child:published"){trace.Add(accumulator.Finish());}},accumulator.Add);
  if(!producer.IsComplete||producer.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("B3 completion or NN replay drift.");
  var contextTraces=new List<(string Hash,long Count,List<(long Index,int[] Data)> Witness)>();using var contextAccumulator=new ContextTraceAccumulator();
  var costTraces=new List<List<int[]>>();var currentCosts=new List<int[]>();int costQueries=0;
  var context=LegacyFullEnglishFirstContextPasses.Prepare(producer);context.RunPrepared(stage=>{if(stage=="child:published"){contextTraces.Add(contextAccumulator.Finish());costTraces.Add(currentCosts);currentCosts=new();}},contextAccumulator.Add,default,q=>{if(++costQueries>40960)throw new Exception("Diagnostic class-query bound exceeded");currentCosts.Add(new[]{q.Node,(int)q.Arc,q.Symbol,q.Composite,q.Prototype,q.Ordinal,q.TopTwenty?1:0,q.Primitive?1:0,q.Cost});});
  if(!context.IsComplete||context.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("B4 completion/NN replay drift");
  var contextChildren=context.CopyCompletedChildren();
  w.WriteNumber("B4Peak",context.AdditionalLogicalPeakBytes);w.WriteNumber("B4Work",context.LogicalCopyWorkBytes);w.WriteNumber("B4Visits",context.LogicalVisitWork);w.WriteNumber("B4Resolves",context.MaximumResolveAttempts);
  var pc=producer.CopyCompletedChildren();int childIndex=0;
  w.WriteNumber("B3Peak",producer.AdditionalLogicalPeakBytes);w.WriteNumber("B3Work",producer.LogicalCopyWorkBytes);w.WriteNumber("B3Visits",producer.LogicalVisitWork);
  w.WriteString("Status","AcceptedContext");w.WriteNumber("NetworkCount",nn);w.WriteNumber("LogicalAdditional",c.AdditionalLogicalEnvelopeBytes);w.WriteNumber("LogicalCombined",c.CombinedLogicalReservationBytes);w.WriteStartArray("Children");
  foreach(var child in c.CopyCompletedChildren())
  {
   var primaryChild=pc[childIndex];var contextChild=contextChildren[childIndex];var contextTrace=contextTraces[childIndex];var contextCosts=costTraces[childIndex];var tr=trace[childIndex++];
   var ca=child.ActualCache.Owner;w.WriteStartObject();w.WriteNumber("Descriptor",ca.Child.DescriptorOrdinal);w.WriteNumber("Height",ca.Preparation.Line.Height);w.WriteStartArray("Raw");foreach(var stroke in ca.CopyCutStrokes()){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("Groups");foreach(int g in child.CopyGroupIds())
   {
    var actual=child.ActualCache.ScoreGroup(g);var row=child.ReadGroup(actual);w.WriteStartObject();w.WriteNumber("Group",g);w.WriteNumber("Geometry",row.GeometryCost);w.WriteNumber("Fallback",actual.FallbackCost);w.WritePropertyName("Costs");Ints(actual.CopyFinalCosts());w.WritePropertyName("Confidence");Floats(row.CopyConfidence());w.WriteStartArray("Stored");foreach(var range in ca.CopyGroupRanges(g)){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("Entries");foreach(var e in row.CopyEntries()){w.WriteStartArray();w.WriteNumberValue(e.Symbol);w.WriteNumberValue(e.Cost);w.WriteNumberValue(e.NormalizedLikelihood);w.WriteNumberValue(row.PreparedLikelihood(e.Symbol));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();groups++;
   }w.WriteEndArray();
   w.WriteStartArray("Nodes");for(int node=0;node<ca.NodeCount;node++)
   {
    w.WriteStartArray();var ns=ca.CopyNode(node);var gs=child.Geometry.CopyNode(node);for(int a=0;a<ns.Length;a++)
    {
     var aa=ns[a];var state=gs[a];w.WriteStartObject();w.WriteNumber("Group",aa.GroupId);w.WriteNumber("Span",aa.Span);w.WritePropertyName("Pred");Ints(aa.PredecessorArcIndices.ToArray());w.WriteStartArray("Successors");foreach(var edge in aa.Successors){w.WriteStartArray();w.WriteNumberValue(edge.NodeOffset);w.WriteNumberValue(edge.ArcIndex);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("Bounds");w.WriteNumberValue(state.Bounds.MinimumX);w.WriteNumberValue(state.Bounds.MinimumY);w.WriteNumberValue(state.Bounds.MaximumX);w.WriteNumberValue(state.Bounds.MaximumY);w.WriteEndArray();w.WriteNumber("Future",state.FutureMinimumX);w.WriteNumber("Prefix",state.PrefixMaximumX);w.WriteNumber("Join",state.JoinCost);w.WriteNumber("Blank",state.BlankCost);w.WritePropertyName("Matrix");Ints(state.CopyMarginCosts());w.WriteEndObject();arcs++;
    }w.WriteEndArray();
   }w.WriteEndArray();
   w.WriteNumber("RequestCount",primaryChild.RequestCount);w.WriteNumber("FallbackProposals",primaryChild.FallbackProposalCount);w.WriteString("CallbackSha256",tr.Hash);w.WriteNumber("CallbackCount",tr.Count);
   w.WriteStartArray("Witnesses");foreach(var witness in tr.Witness){w.WriteStartObject();w.WriteNumber("Index",witness.Index);w.WritePropertyName("Data");Ints(witness.Data);w.WriteEndObject();}w.WriteEndArray();
   w.WriteStartArray("Requests");for(int n0=0;n0<primaryChild.Primary.NodeCount;n0++){var requests=primaryChild.CopyRequests(n0);var ord=primaryChild.CopyShortlistOrdinals(n0);w.WriteStartArray();for(int i=0;i<requests.Length;i++){var q=requests[i];Ints(new[]{(int)q.ArcOrdinal,q.Span,q.Symbol,q.CompositeCategory,q.GroupCost,q.LikelihoodByte,ord[i]});}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("Primary");for(int n0=0;n0<primaryChild.Primary.NodeCount;n0++){var row0=primaryChild.Primary.GetRowCopy(n0);w.WriteStartObject();w.WriteNumber("Count",row0.Count);w.WriteBase64String("Bytes",row0.CopyStorage());w.WriteEndObject();}w.WriteEndArray();

   w.WriteStartArray("ContextCosts");foreach(var query in contextCosts)Ints(query);w.WriteEndArray();
   w.WriteString("ContextCallbackSha256",contextTrace.Hash);w.WriteNumber("ContextCallbackCount",contextTrace.Count);
   w.WriteStartArray("ContextWitnesses");foreach(var witness in contextTrace.Witness){w.WriteStartObject();w.WriteNumber("Index",witness.Index);w.WritePropertyName("Data");Ints(witness.Data);w.WriteEndObject();}w.WriteEndArray();
   w.WriteStartArray("A8");for(int node=0;node<contextChild.NodeCount;node++){var a8=contextChild.GetA8(node);w.WriteStartObject();w.WriteNumber("Groups",a8.GroupCount);w.WritePropertyName("PrimaryOrdinals");Ints(Enumerable.Range(0,a8.GroupCount).Select(a8.GetPrimaryOrdinal));w.WritePropertyName("Counts");Ints(Enumerable.Range(0,5).Select(a8.GetFiniteAlternativeCount));w.WriteBase64String("Bytes",a8.CopyStorage());w.WriteEndObject();}w.WriteEndArray();
   for(int final=0;final<2;final++){w.WriteStartArray(final==0?"B0":"B8");for(int node=0;node<contextChild.NodeCount;node++){w.WriteStartArray();for(int ri=0;ri<5;ri++){var row=final==0?contextChild.GetB0RowCopy(node,ri):contextChild.GetB8RowCopy(node,ri);w.WriteStartObject();w.WriteNumber("Count",row.Count);w.WriteBase64String("Bytes",row.CopyStorage());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();}
   w.WriteNumber("ContextRoot",contextChild.ContextRoot);w.WriteNumber("ContextCount",contextChild.ContextCount);w.WriteNumber("ContextPageCount",contextChild.ContextPageCount);w.WriteNumber("ContextPageCapacity",contextChild.ContextPageDescriptorCapacity);w.WriteNumber("ContextCurrentPage",contextChild.CurrentContextPage);w.WritePropertyName("ContextPages");Ints(contextChild.CopyContextPageUsedCounts());
   w.WriteStartArray("Contexts");foreach(var node in contextChild.CopySymbolContexts()){w.WriteStartArray();foreach(ulong id in new[]{node.Identity,node.ParentLink,node.ChildTreeRoot,node.Left,node.Right})w.WriteNumberValue(id);w.WriteNumberValue(node.Symbol);w.WriteNumberValue(node.Balance);w.WriteNumberValue(node.PageIndex);w.WriteNumberValue(node.PageSlot);w.WriteEndArray();}w.WriteEndArray();
   w.WriteEndObject();
  }w.WriteEndArray();accepted++;
 }
 catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){w.WriteString("Status","Refused");w.WriteString("Reason",e.GetType().Name+":"+e.Message);refused++;}
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteNumber("Groups",groups);w.WriteNumber("Arcs",arcs);w.WriteEndObject();w.Flush();Console.WriteLine($"actual{items.Length}/accepted{accepted}/refused{refused}/groups{groups}/arcs{arcs}/guards{guards}/cancel{cancels}");

sealed class TraceAccumulator:IDisposable
{
 private readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);private long count;private readonly List<(long Index,int[] Data)> witness=[];
 public void Add(int stage,LegacyFirstAncestorTransitionRequest r,int incoming,LegacyFirstOrdinaryTransitionResult result)
 {
  var p=r.Previous;var o=p.OriginalRecord;var g=p.GeometryRecord;
  Span<int> values=stackalloc int[]{r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalAlternative,o.ArcOrdinal,o.Symbol,o.CompositeCategory,o.Span,o.PreviousAlternative,o.TotalCost,o.AcousticCost,o.PathBit?1:0,p.GeometryNode,p.GeometryAlternative,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,g.PreviousAlternative,g.TotalCost,g.AcousticCost,g.PathBit?1:0,p.SkippedCount,incoming,result.Cost,result.Accepted?1:0,stage};
  Span<byte> bytes=stackalloc byte[124];for(int i=0;i<31;i++)BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(i*4),values[i]);hash.AppendData(bytes);
  if(count<8||p.SkippedCount>0&&witness.Count<16)witness.Add((count,values.ToArray()));count++;
 }
 public (string Hash,long Count,List<(long Index,int[] Data)> Witness) Finish(){var result=(Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(),count,witness.ToList());count=0;witness.Clear();return result;}
 public void Dispose()=>hash.Dispose();
}

sealed class ContextTraceAccumulator:IDisposable
{
 private readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);private long count;private readonly List<(long Index,int[] Data)> witness=[];
 public void Add(LegacyFirstBlankContextCorrection e)
 {
  var r=e.Request;var p=e.Previous;var o=p.OriginalRecord;var g=p.GeometryRecord;
  Span<int> values=stackalloc int[]{e.Stage,e.FinalPass?1:0,e.CurrentNode,e.CurrentPrimaryRow,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalPrimaryRow,p.OriginalContextAlternative,o.ArcOrdinal,o.Symbol,o.CompositeCategory,o.Span,o.PreviousAlternative,o.ContinuationType,o.TotalCost,o.AcousticCost,o.PathBit?1:0,p.GeometryNode,p.GeometryPrimaryRow,p.GeometryContextAlternative,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,g.PreviousAlternative,g.ContinuationType,g.TotalCost,g.AcousticCost,g.PathBit?1:0,p.SkippedCount,e.IncomingCost,e.Result.Cost,e.Result.Accepted?1:0};
  Span<byte> bytes=stackalloc byte[148];for(int i=0;i<37;i++)BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(i*4),values[i]);hash.AppendData(bytes);
  if(count<8||p.SkippedCount>0&&witness.Count<16)witness.Add((count,values.ToArray()));count++;
 }
 public (string Hash,long Count,List<(long Index,int[] Data)> Witness) Finish(){var result=(Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(),count,witness.ToList());count=0;witness.Clear();return result;}
 public void Dispose()=>hash.Dispose();
}
