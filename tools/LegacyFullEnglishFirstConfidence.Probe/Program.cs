using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("models blind-inputs capture");
var models=args[0];var bytes=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res"));var knowledge=LegacyFullEnglishFirstKnowledge.Load(bytes);
var m30=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));int guards=0,cancels=0;
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal.");}
var ink=new[]{new InkStroke(Enumerable.Range(0,32).Select(i=>new InkPoint(10*i,(float)(40*Math.Sin(i*1.3)))).ToArray())};
LegacyFullEnglishFirstParentInput Parent(LegacyFullEnglishFirstBounds? bounds=null)=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,ink,0,m30,m50,bounds);
var pre=Parent();var good=LegacyFullEnglishFirstController.Prepare(pre);Guard(()=>good.Run(table,new CancellationToken(true)));if(pre.IsFaulted||good.IsFaulted)throw new Exception("Preentry consumed state.");good.Run(table);Guard(()=>good.Run(table));Guard(()=>LegacyFullEnglishFirstController.Prepare(pre));Guard(()=>pre.Run(table));
if(!good.IsComplete||good.IsFaulted||!pre.IsComplete)throw new Exception("Complete state drift.");
var controlCache=pre.CopyCompletedChildren()[0];int controlId=controlCache.Owner.CopyLiveGroupIds()[0];var controlGroup=controlCache.ScoreGroup(controlId);var snapshot=good.ReadGroup(controlGroup);var vals=snapshot.CopyValues();if(vals.Length>0){float old=vals[0];vals[0]=-1;if(snapshot.CopyValues()[0]!=old)throw new Exception("Mutable receipt.");}
var foreign=Parent();var foreignOwner=LegacyFullEnglishFirstController.Prepare(foreign);foreignOwner.Run(table);Guard(()=>good.ReadGroup(foreign.CopyCompletedChildren()[0].ScoreGroup(controlId)));
var inspectedParent=Parent();var inspected=LegacyFullEnglishFirstController.Prepare(inspectedParent);LegacyFullEnglishFirstConfidenceAccumulator? previousAccumulator=null;LegacyFullEnglishFirstAcousticCache? previousCache=null;int previousGroup=-1,doublePublishGuards=0;
inspected.RunInspected(table,(cache,id,accumulator)=>
{
 Guard(()=>inspected.Begin(cache,id,default));
 Guard(()=>inspected.Publish(controlGroup,accumulator,default));
 Guard(()=>foreignOwner.Publish(controlGroup,accumulator,default));
 var fabricated=new LegacyFullEnglishFirstCachedGroup(cache,id,new LegacyOrdinaryAcousticClassCacheResult(new int[220],Array.Empty<LegacyAcousticCacheCandidate>(),0),false,false,null);
 Guard(()=>inspected.Publish(fabricated,new LegacyFullEnglishFirstConfidenceAccumulator(inspected,cache,id),default));
 Guard(()=>inspected.Publish(fabricated,accumulator,default));
 if(previousAccumulator is not null){Guard(()=>inspected.Publish(previousCache!.ScoreGroup(previousGroup),previousAccumulator,default));doublePublishGuards++;}
 previousAccumulator=accumulator;previousCache=cache;previousGroup=id;
});
if(doublePublishGuards<1||!inspected.IsComplete)throw new Exception("Expected active duplicate-publication controls.");
Guard(()=>inspected.Publish(previousCache!.ScoreGroup(previousGroup),previousAccumulator!,default));
Guard(()=>LegacyFullEnglishFirstController.Envelope(4097,1));Guard(()=>LegacyFullEnglishFirstController.Envelope(4096,257));Guard(()=>LegacyFullEnglishFirstController.Envelope(1,0));
if(LegacyFullEnglishFirstController.Envelope(4096,256)!=17760256||pre.LogicalSourcePayloadEnvelopeBytes>15L*1024*1024)throw new Exception("Envelope boundary drift.");
var limited=Parent(new(4096,4));var limitOwner=LegacyFullEnglishFirstController.Prepare(limited);Guard(()=>limitOwner.Run(table));if(!limitOwner.IsFaulted||!limited.IsFaulted||limited.NetworkEvaluationCount!=4)throw new Exception("Entered NN admission poison drift.");Guard(()=>limitOwner.Run(table));
Guard(()=>limitOwner.Publish(controlGroup,new LegacyFullEnglishFirstConfidenceAccumulator(good,controlCache,controlId),default));Guard(()=>limitOwner.Begin(controlCache,controlId,default));
foreach(string stage in new[]{"run:entry","run:candidates","score:features","score:network0","score:network1","score:network2","score:network3","score:publish","run:child"})
{
 var p=Parent();var c=LegacyFullEnglishFirstController.Prepare(p);using var cts=new CancellationTokenSource();Guard(()=>c.RunPrepared(table,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;if(!p.IsFaulted||!c.IsFaulted||c.IsComplete)throw new Exception("Entered cancellation drift.");Guard(()=>c.Run(table));
 foreach(var cache in p.CopyCompletedChildren())foreach(int id in cache.Owner.CopyLiveGroupIds())_ = c.ReadGroup(cache.ScoreGroup(id)).CopyValues();
}
var mutable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));System.Runtime.InteropServices.MemoryMarshal.TryGetArray(mutable.Values,out var segment);float saved=segment.Array![segment.Offset];segment.Array[segment.Offset]=saved+1;var tp=Parent();var tc=LegacyFullEnglishFirstController.Prepare(tp);Guard(()=>tc.Run(mutable));if(tc.IsFaulted||tp.IsFaulted)throw new Exception("Table preflight consumed owner.");segment.Array[segment.Offset]=saved;tc.Run(mutable);
var twoLines=ink.Concat(ink.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();var multi=LegacyFullEnglishFirstParentInput.Prepare(knowledge,twoLines,0,m30,m50);if(multi.Children.Count<2)throw new Exception("Expected actual multiple child control.");var multiOwner=LegacyFullEnglishFirstController.Prepare(multi);using(var cts=new CancellationTokenSource())
{
 int seen=0;Guard(()=>multiOwner.RunPrepared(table,s=>{if(s=="run:candidates"&&++seen==2)cts.Cancel();},cts.Token));cancels++;
 if(!multi.IsFaulted||!multiOwner.IsFaulted||multiOwner.IsComplete||multi.CopyCompletedChildren().Count!=1||multiOwner.CompletedGroupCount<1)throw new Exception("Expected nonempty failed historical prefix.");
 var historical=multi.CopyCompletedChildren()[0];foreach(int id in historical.Owner.CopyLiveGroupIds())if(multiOwner.ReadGroup(historical.ScoreGroup(id)).CopyValues().Length!=historical.ScoreGroup(id).CopyCandidates().Length)throw new Exception("Historical confidence lost after fault.");Guard(()=>multiOwner.Run(table));
}
var concurrentParent=Parent();var concurrent=LegacyFullEnglishFirstController.Prepare(concurrentParent);using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{
 var task=Task.Run(()=>concurrent.RunPrepared(table,s=>{if(s=="run:entry"){entered.Set();release.Wait();}}));if(!entered.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Barrier timeout.");Guard(()=>concurrent.Run(table));Guard(()=>concurrentParent.Run(table));release.Set();task.GetAwaiter().GetResult();
}
Guard(()=>LegacyFullEnglishFirstConfidenceAccumulator.Calibrate(-1));Guard(()=>LegacyFullEnglishFirstConfidenceAccumulator.Calibrate(32769));var prepared=new LegacyFullEnglishFirstConfidenceAccumulator(good,controlCache,controlId);Guard(()=>prepared.Feed(1,new int[182],default));Guard(()=>prepared.Feed(0,new int[182],default));Guard(()=>prepared.Feed(0,new int[181],default));Guard(()=>prepared.ReadMean(0));Guard(()=>prepared.Feed(0,new int[182],new CancellationToken(true)));
var partialFailure=new LegacyFullEnglishFirstConfidenceAccumulator(good,controlCache,controlId);partialFailure.Feed(0,new int[182],default);var badHalf=new int[182];badHalf[91]=-1;Guard(()=>partialFailure.Feed(1,badHalf,default));Guard(()=>partialFailure.Feed(1,new int[182],default));
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","Actual full-FIRST scalar confidence only; no controller winner/720/frame");w.WriteBoolean("TargetsRead",false);w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);
w.WriteStartArray("Calibration");for(int p=0;p<=32768;p++)w.WriteNumberValue(LegacyFullEnglishFirstConfidenceAccumulator.Calibrate(p));w.WriteEndArray();
int before=pre.NetworkEvaluationCount;w.WriteStartArray("PreparedStreaming");
for(int p=0;p<=32768;p++)
{
 int[] probabilities=[p,32768-p,p/2,(p*17)%32769];var acc=new LegacyFullEnglishFirstConfidenceAccumulator(good,controlCache,controlId);
 for(int family=0;family<4;family++)acc.Feed(family,Enumerable.Repeat(probabilities[family],182).ToArray(),default);w.WriteNumberValue(acc.ReadMean(0));
}
w.WriteEndArray();if(before!=pre.NetworkEvaluationCount)throw new Exception("Prepared control evaluated network.");
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();int accepted=0,refused=0,groups=0,values=0;
w.WriteStartArray("Actual");
foreach(var item in items)
{
 var raw=item.TryGetProperty("Raw",out var r)?r:item.GetProperty("strokes");string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;float resolution=item.TryGetProperty("Resolution",out var rr)?rr.GetSingle():0;
 var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();w.WriteStartObject();w.WriteString("Name",name);
 try
 {
  var a=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);a.Run(table);
  var b=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);var c=LegacyFullEnglishFirstController.Prepare(b);c.Run(table);
  if(!c.IsComplete||c.IsFaulted||!b.IsComplete||a.NetworkEvaluationCount!=b.NetworkEvaluationCount)throw new Exception("B1 whole completion/NN replay drift.");
  var ac=a.CopyCompletedChildren();var bc=b.CopyCompletedChildren();if(ac.Count!=bc.Count)throw new Exception("Child count drift.");
  w.WriteString("Status","AcceptedConfidence");w.WriteNumber("NetworkCount",b.NetworkEvaluationCount);w.WriteStartArray("Children");
  for(int ci=0;ci<bc.Count;ci++)
  {
   var ca=ac[ci];var cb=bc[ci];var ids=cb.Owner.CopyLiveGroupIds();if(!ids.SequenceEqual(ca.Owner.CopyLiveGroupIds())||!cb.Owner.CopyCutRecords().SequenceEqual(ca.Owner.CopyCutRecords()))throw new Exception("Actual group/source lineage drift.");
   w.WriteStartObject();w.WriteNumber("Descriptor",cb.Owner.Child.DescriptorOrdinal);w.WriteStartArray("Groups");
   foreach(int id in ids)
   {
    var aa=ca.ScoreGroup(id);var bb=cb.ScoreGroup(id);if(!aa.CopyFinalCosts().SequenceEqual(bb.CopyFinalCosts())||!aa.CopyCandidates().SequenceEqual(bb.CopyCandidates()))throw new Exception("A output changed in confidence run.");
    var receipt=c.ReadGroup(bb);var vs=receipt.CopyValues();var candidates=bb.CopyCandidates();if(vs.Length!=candidates.Length)throw new Exception("Confidence shortlist drift.");
    for(int j=0;j<vs.Length;j++)if(receipt.ReadSymbolScore(candidates[j].Symbol)!=vs[j]||!float.IsFinite(vs[j])||vs[j]<0||vs[j]>1)throw new Exception("Invalid confidence scalar.");
    w.WriteStartObject();w.WriteNumber("Group",id);w.WriteNumber("StoredProbabilityInspection",LegacyFullEnglishFirstConfidenceAccumulator.ReadStoredProbability(cb,id,default));w.WriteStartArray("Values");foreach(float v in vs)w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();groups++;values+=vs.Length;
   }
   w.WriteEndArray();w.WriteEndObject();
  }
  w.WriteEndArray();accepted++;
 }
 catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){w.WriteString("Status","Refused");w.WriteString("Reason",e.GetType().Name+":"+e.Message);refused++;}
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteNumber("Groups",groups);w.WriteNumber("Values",values);w.WriteEndObject();w.Flush();Console.WriteLine($"actual{items.Length}/accepted{accepted}/refused{refused}/groups{groups}/values{values}/guards{guards}/cancel{cancels}");
