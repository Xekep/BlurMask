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
var good=B1();var b=LegacyFullEnglishFirstControllerCosts.Prepare(good,table);b.Run();
var primary=LegacyFullEnglishFirstPrimary.Prepare(b);Guard(()=>primary.Run(new CancellationToken(true)));if(primary.IsFaulted||primary.IsComplete)throw new Exception("Preentry cancellation consumed state.");primary.Run();Guard(()=>primary.Run());
foreach(var c in primary.CopyCompletedChildren())
{
 if(!ReferenceEquals(c.Owner,primary)||!ReferenceEquals(c.Initializer.Owner,b))throw new Exception("Detached owner.");
 var rq=c.CopyRequests(0);if(rq.Length>0){var old=rq[0];rq[0]=default;if(c.CopyRequests(0)[0]!=old)throw new Exception("Mutable request.");}
 var r=c.Primary.GetRowCopy(0);if(r.Count>0){byte original=r.CopyStorage()[0];var data=r.CopyStorage();data[0]^=255;if(c.Primary.GetRowCopy(0).CopyStorage()[0]!=original)throw new Exception("Mutable rows.");byte saved=r.GetLikelihoodByte(0);r.SetLikelihoodByte(0,(byte)(saved^255));if(c.Primary.GetRowCopy(0).GetLikelihoodByte(0)!=saved)throw new Exception("Mutable row object.");}
}
Guard(()=>LegacyFullEnglishFirstPrimary.Prepare(null!));Guard(()=>LegacyFullEnglishFirstPrimary.PrepareBounded(b,1,LegacyFullEnglishFirstPrimaryBudget.WorkLimit,LegacyFullEnglishFirstPrimaryBudget.VisitLimit));Guard(()=>LegacyFullEnglishFirstPrimary.PrepareBounded(b,LegacyFullEnglishFirstPrimaryBudget.PeakLimit,1,LegacyFullEnglishFirstPrimaryBudget.VisitLimit));Guard(()=>LegacyFullEnglishFirstPrimary.PrepareBounded(b,LegacyFullEnglishFirstPrimaryBudget.PeakLimit,LegacyFullEnglishFirstPrimaryBudget.WorkLimit,1));
foreach(var x in new[]{(4097,1,1,1),(1,4097,1,1),(1,1,4097,1),(1,1,1,1000001)})Guard(()=>LegacyFullEnglishFirstPrimaryBudget.Envelope(x.Item1,x.Item2,x.Item3,x.Item4));
var max=LegacyFullEnglishFirstPrimaryBudget.Envelope(4096,4096,4096,1000000);if(max.Peak!=60040192||max.Work!=195966464)throw new Exception("Maximum budget arithmetic drift.");
var unfinished=LegacyFullEnglishFirstControllerCosts.Prepare(good,table);Guard(()=>LegacyFullEnglishFirstPrimary.Prepare(unfinished));
foreach(string stage in new[]{"entry","child:entry","requests","primary","child:published"})
{
 var p=LegacyFullEnglishFirstPrimary.Prepare(b);using var cts=new CancellationTokenSource();Guard(()=>p.RunPrepared(s=>{if(s==stage)cts.Cancel();},null,cts.Token));cancels++;if(!p.IsFaulted||p.IsComplete)throw new Exception("Entered cancellation did not poison.");Guard(()=>p.Run());foreach(var child in p.CopyCompletedChildren())_=child.Primary.GetRowCopy(0);
}
Guard(()=>LegacyFullEnglishFirstPrimary.Prepare(b,new CancellationToken(true)));
var foreignB1=B1();var foreignCache=foreignB1.Parent.CopyCompletedChildren()[0];var ownChild=b.CopyCompletedChildren()[0];Guard(()=>ownChild.ReadGroup(foreignCache.ScoreGroup(ownChild.CopyGroupIds()[0])));
var callbackCancel=LegacyFullEnglishFirstPrimary.Prepare(b);using(var cts=new CancellationTokenSource()){Guard(()=>callbackCancel.RunPrepared(null,(_,_,_,_)=>cts.Cancel(),cts.Token));if(!callbackCancel.IsFaulted)throw new Exception("Callback cancel not poisoned");Guard(()=>callbackCancel.Run());cancels++;}
var observerFault=LegacyFullEnglishFirstPrimary.Prepare(b);Guard(()=>observerFault.RunPrepared(null,(_,_,_,_)=>throw new InvalidDataException("Injected observer failure")));if(!observerFault.IsFaulted)throw new Exception("Observer fault not poisoned.");Guard(()=>observerFault.Run());
var multiStrokes=ink.Concat(ink.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();var mb1=B1(multiStrokes);var mb2=LegacyFullEnglishFirstControllerCosts.Prepare(mb1,table);mb2.Run();var mp=LegacyFullEnglishFirstPrimary.Prepare(mb2);using(var cts=new CancellationTokenSource()){int count=0;Guard(()=>mp.RunPrepared(s=>{if(s=="child:entry"&&++count==2)cts.Cancel();},null,cts.Token));if(!mp.IsFaulted||mp.CopyCompletedChildren().Count!=1)throw new Exception("Nonempty history lost.");cancels++;}
var concurrent=LegacyFullEnglishFirstPrimary.Prepare(b);using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{var task=Task.Run(()=>concurrent.RunPrepared(s=>{if(s=="entry"){entered.Set();release.Wait();}},null));if(!entered.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Barrier timeout");try{Guard(()=>concurrent.Run());_=concurrent.CopyCompletedChildren();}finally{release.Set();}task.GetAwaiter().GetResult();}
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","Actual full-FIRST primary98 only; no context/terminal/WORD");w.WriteBoolean("TargetsRead",false);w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);
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
  var pc=producer.CopyCompletedChildren();int childIndex=0;
  w.WriteNumber("B3Peak",producer.AdditionalLogicalPeakBytes);w.WriteNumber("B3Work",producer.LogicalCopyWorkBytes);w.WriteNumber("B3Visits",producer.LogicalVisitWork);
  w.WriteString("Status","AcceptedPrimary");w.WriteNumber("NetworkCount",nn);w.WriteNumber("LogicalAdditional",c.AdditionalLogicalEnvelopeBytes);w.WriteNumber("LogicalCombined",c.CombinedLogicalReservationBytes);w.WriteStartArray("Children");
  foreach(var child in c.CopyCompletedChildren())
  {
   var primaryChild=pc[childIndex];var tr=trace[childIndex++];
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
   w.WriteStartArray("Primary");for(int n0=0;n0<primaryChild.Primary.NodeCount;n0++){var row0=primaryChild.Primary.GetRowCopy(n0);w.WriteStartObject();w.WriteNumber("Count",row0.Count);w.WriteBase64String("Bytes",row0.CopyStorage());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
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
