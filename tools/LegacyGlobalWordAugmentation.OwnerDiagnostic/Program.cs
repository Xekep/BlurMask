using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length==3&&args[0]=="--topology-data")
{
 using var data=JsonDocument.Parse(File.ReadAllBytes(args[1]));var r=data.RootElement;
 byte[] compact=Convert.FromBase64String(r.GetProperty("compact").GetString()!);int count=r.GetProperty("count").GetInt32();compact=compact.AsSpan(0,count*40).ToArray();
 int[] starts=r.GetProperty("starts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();var lastArcs=r.GetProperty("graph")[starts.Length-2];int end=lastArcs[0][2].GetInt32();
 foreach(var arc in lastArcs.EnumerateArray())if(arc[2].GetInt32()!=end)throw new InvalidDataException("Single source end required for this DATA check");
 var envelope=LegacyGlobalWordAugmentationEnvelope.Prepare(compact,starts,starts.Length-1,4096,originalReferences:65536,actualLastContext:end);
 File.WriteAllText(args[2],JsonSerializer.Serialize(new{Scope="DATA-only topology from previous raw capture, NOT actual owner or source authority",NNCalls=0,Bounds=envelope.Bounds},new JsonSerializerOptions{WriteIndented=true}));return;
}
if(args.Length==2&&args[0]=="--prepared")
{
 var controls=new List<string>();
 byte[] Rows(int n,bool eligible){var b=new byte[n*40];for(int i=0;i<n;i++){BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(i*40+16),i-1);if(eligible)b[i*40+32]=8;}return b;}
 var zero=LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(131072,false),new[]{0,131072},1,4096);controls.Add("maximum-ineligible-geometry");
 var small=LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(2,true),new[]{0,2},1,2);controls.Add("two-eligible-admitted");
 Check(small.Bounds.Candidates==2&&small.Bounds.Rows==4&&small.Bounds.Lookahead==8&&small.Bounds.Equalities==6);
 Reject("large-eligible-beforeclaim",()=>LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(131072,true),new[]{0,131072},1,2));
 Reject("bad-starts-beforeclaim",()=>LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(2,false),new[]{1,2},1,2));
 Reject("bad-parent-beforeclaim",()=>{var b=Rows(2,false);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(16),2);LegacyGlobalWordAugmentationEnvelope.Prepare(b,new[]{0,2},1,2);});
 Reject("pre-cancel",()=>LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(2,false),new[]{0,2},1,2,new(true)),true);
 for(int i=0;i<small.Bounds.Rows;i++)small.Row();Reject("row-overrun-before-growth",small.Row);
 for(int i=0;i<small.Bounds.Lookahead;i++)small.Lookahead();Reject("lookahead-overrun-before-growth",small.Lookahead);
 for(int i=0;i<small.Bounds.Equalities;i++)small.Equality();Reject("equality-overrun-before-reader",small.Equality);
 var split=Rows(4,true);var sliced=LegacyGlobalWordAugmentationEnvelope.Prepare(split,new[]{0,2,4},2,2,actualLastContext:128);Check(sliced.Bounds.Candidates==2&&sliced.Bounds.MaximumParentDepth==4&&sliced.Bounds.CandidateStart==2);controls.Add("actual-last-node-scan-slice");
 Reject("cycle-beforeclaim",()=>{var b=Rows(2,true);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(16),1);LegacyGlobalWordAugmentationEnvelope.Prepare(b,new[]{0,2},1,2);});
 var forward=Rows(3,true);BinaryPrimitives.WriteInt32LittleEndian(forward.AsSpan(16),2);BinaryPrimitives.WriteInt32LittleEndian(forward.AsSpan(2*40+16),-1);var fd=LegacyGlobalWordAugmentationEnvelope.Prepare(forward,new[]{0,3},1,2);Check(fd.Bounds.MaximumParentDepth==3);controls.Add("forward-parent-acyclic");
 Reject("bad-span-last-node-beforeclaim",()=>LegacyGlobalWordAugmentationEnvelope.Prepare(Rows(2,true),new[]{0,2},1,2,actualLastContext:128));
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var before=pool.CopyOwnerNodes().Count;
 _=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);Check(pool.CopyOwnerNodes().Count==before);controls.Add("CreateEmpty-pool-unchanged");
 File.WriteAllText(args[1],JsonSerializer.Serialize(new{Scope="PREPARED structural envelope only; no actual owner/model/NN",NNCalls=0,Controls=controls},new JsonSerializerOptions{WriteIndented=true}));return;
 void Reject(string name,Action f,bool cancel=false){try{f();}catch(Exception e)when(cancel?e is OperationCanceledException:e is NotSupportedException or InvalidDataException){controls.Add(name);return;}throw new Exception("No expected refusal: "+name);}
}
if(args.Length!=4||args[0]!="--actual")throw new ArgumentException("--prepared report.json | --actual pinned-plan.json scenario capture.json");
// Parent must review/pin plan and authorize actual execution. One actual input/
// ONE NN source invocation per process. No default branch falls into inference.
using var plan=JsonDocument.Parse(File.ReadAllBytes(args[1]));var root=plan.RootElement;
var scenario=args[2];if(scenario is not("new-success" or "old-cross-thread" or "old-entered-failure" or "new-claim-protocol" or "new-entered-failure"))throw new ArgumentException("Exact reviewed scenario required.");
var files=new Dictionary<string,byte[]>();
foreach(var f in root.GetProperty("files").EnumerateArray())
{
 string key=f.GetProperty("name").GetString()!,path=f.GetProperty("path").GetString()!;long size=f.GetProperty("bytes").GetInt64();
 if(size<1||size>8000000||new FileInfo(path).Length!=size)throw new InvalidDataException("Pinned bounded file required.");
 byte[] bytes=File.ReadAllBytes(path);if(Convert.ToHexStringLower(SHA256.HashData(bytes))!=f.GetProperty("sha256").GetString())throw new InvalidDataException("Plan hash mismatch.");files.Add(key,bytes);
}
byte[] R(string key)=>files[key];
using var track=JsonDocument.Parse(R("track"));var points=track.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
if(points.Length is <2 or >4096)throw new InvalidDataException("Unchanged bounded one-physical-stroke recording required.");
var ak=R("ak");var models=LegacyFreshMainRecognitionContext.Load(ak,R("lk"),R("engine"),R("line30"),R("line50"));
var table=LegacyCursiveSeparationTable.Load(R("separation"));using var cs=new MemoryStream(R("case"));using var ps=new MemoryStream(R("publication"));var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);
var doc=LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(points)},default);
var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"diagnostic"});original.Run();
var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[0]);selected.Run();
using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(30));
var source=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],deadline.Token);
var session=source.Session??throw new InvalidDataException("Positive actual session required.");
var evidence=new List<object>();var sourceWord=session.Ownership.WordPublisher.Head??throw new InvalidDataException("One actual word required.");
byte[] beforeWord=sourceWord.Word.CopyStorage();int beforeNN=source.CachedMainGroupEvaluations;
void CheckActual(bool value,string name){if(!value)throw new Exception(name);evidence.Add(new{Name=name,Pass=true});}
void Refuse(string name,Action action,Type expected,CancellationToken? expectedToken=null,string? exactMessage=null)
{
 try{action();}catch(Exception e){if(exactMessage is not null&&e.Message!=exactMessage)throw; if(e.GetType()!=expected||expectedToken is{} t&&(e is not OperationCanceledException oce||oce.CancellationToken!=t))throw; evidence.Add(new{Name=name,Type=e.GetType().FullName,e.Message,ExactMessageChecked=exactMessage is not null,TokenMatches=expectedToken.HasValue});return;}throw new Exception("Expected refusal: "+name);
}
LegacyFreshMainGlobalWordAugmentation.Result Run(LegacyFreshMainGlobalWordAugmentation owner,CancellationToken token=default)=>owner.Run(new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],new byte[4],new byte[4],new byte[4],new byte[4],token);
var actualCompact=source.Selection.Publication.CopyCompactStorage().AsSpan(0,source.Selection.Publication.RecordCount2470*40).ToArray();
int actualBit8=0;for(int i=0;i<actualCompact.Length/40;i++)if((actualCompact[i*40+32]&8)!=0)actualBit8++;
var topology=LegacyGlobalWordAugmentationEnvelope.Prepare(actualCompact,source.Selection.Publication.CopyNodeStarts2478(),source.Selection.Publication.Policy.Owner.Graph.State.NodeCount,points.Length,deadline.Token,sourceWord.Word.FlatReferences.Count,sourceWord.Span.Last);
if(scenario=="new-success")CheckActual(actualBit8>0&&topology.Bounds.Candidates>0,"genuine-positive-preclaim-candidates");
object ReadActualState()
{
 var publisher=session.Ownership.WordPublisher;var snap=publisher.CaptureSpan(sourceWord,[sourceWord],deadline.Token);
 if(snap.Words.Count is <1 or >4096||snap.SelectedOrdinals.Count>4096)throw new NotSupportedException("Diagnostic actual275 export bounds");
 long flatCount=0;var words=new List<object>();foreach(var w in snap.Words){flatCount+=w.FlatReferences.Count;if(flatCount>65536)throw new NotSupportedException("Diagnostic total flatrefs bound");words.Add(new{Storage120=Convert.ToBase64String(w.CopyStorage()),FlatReferences=w.FlatReferences.ToArray()});}
 var nodes=session.Ownership.GlyphPool.CopyOwnerNodes(deadline.Token);if(nodes.Count>4096||nodes.Sum(n=>n.Records.Length)>65536*40)throw new NotSupportedException("Diagnostic pool export bound");
 return new{SpanFirst=snap.Span.First,SpanLast=snap.Span.Last,Words=words,SelectedOrdinals=snap.SelectedOrdinals.ToArray(),Pool=nodes.Select(n=>new{n.Key,n.PackedCount,Records=Convert.ToBase64String(n.Records.Span)}).ToArray()};
}
object beforeState=ReadActualState();object? afterState=null;object? result=null;
if(scenario=="new-success")
{
 var owner=LegacyFreshMainGlobalWordAugmentation.Prepare(ak,source,new byte[400]);using var canceled=new CancellationTokenSource();canceled.Cancel();
 Refuse("pre-cancel-exact-token",()=>Run(owner,canceled.Token),typeof(OperationCanceledException),canceled.Token);
 Refuse("invalid-residue-beforeclaim",()=>owner.Run(new byte[119],new byte[40],new ushort[256],new byte[64],new byte[72],new byte[4],new byte[4],new byte[4],new byte[4]),typeof(ArgumentException));
 CheckActual(!owner.IsFaulted&&!owner.IsComplete&&!session.IsStopped&&beforeWord.AsSpan().SequenceEqual(sourceWord.Word.CopyStorage()),"pre-entry-no-live-mutation");
 var completed=Run(owner,deadline.Token);CheckActual(completed.Candidates>0,"actual-selected-eligible-candidates");result=new{completed.Candidates,completed.Appended,completed.Duplicates,completed.UpdatedNewDuplicates,completed.QuotaAfter,Source26=Convert.ToBase64String(completed.CopySourceStorage26()),Words=completed.CopyRankedWords120().Select(Convert.ToBase64String).ToArray()};
 CheckActual(owner.IsComplete&&!owner.IsFaulted,"new-complete");afterState=ReadActualState();
 Refuse("new-repeat",()=>Run(owner),typeof(InvalidOperationException));
 Task.Run(()=>Refuse("new-thread-bound",()=>owner.RequireCompleted(default),typeof(InvalidOperationException))).GetAwaiter().GetResult();
 Refuse("old-after-new-eligibility-refusal",()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.Prepare(scope,source,new byte[400],default),typeof(NotSupportedException),exactMessage:"Active219580 bit8 augmentation is not restored.");
 Refuse("post-new-protocol-claim-collision",()=>session.ClaimSourceCompletion(new object(),false,default),typeof(InvalidOperationException),exactMessage:"Actual completed global TOP source stage already claimed or unavailable.");
 CheckActual(!session.IsStopped,"race-loser-does-not-poison-winner");
}
else if(scenario=="old-cross-thread"||scenario=="old-entered-failure")
{
 using var canceled=new CancellationTokenSource();canceled.Cancel();
 Refuse("old-prepare-pre-cancel",()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.Prepare(scope,source,new byte[400],canceled.Token),typeof(OperationCanceledException),canceled.Token);
 var owner=LegacyFreshMainGlobalNoCandidateSourceCompletion.Prepare(scope,source,new byte[400],default);
 if(scenario=="old-entered-failure")
 {
  Refuse("old-entered-bad-scratch",()=>owner.Run(Array.Empty<byte>(),new byte[4],default),typeof(ArgumentException));
  CheckActual(owner.IsFaulted&&session.IsStopped,"old-entered-failure-poisons-session");
  Refuse("old-faulted-reentry",()=>owner.Run(new byte[4],new byte[4],default),typeof(InvalidOperationException));
 }
 else
 {
  Task.Run(()=>owner.Run(new byte[4],new byte[4],default)).GetAwaiter().GetResult();
  CheckActual(owner.IsTopV68Completed&&!owner.IsFaulted&&!session.IsStopped,"old-bindThreadFalse-worker-run");
  Task.Run(()=>session.RequireSourceCompletion(owner,default)).GetAwaiter().GetResult();
  result=new{Source26=Convert.ToBase64String(owner.CopySourceStorage26()),After120=Convert.ToBase64String(sourceWord.Word.CopyStorage()),owner.QuotaAfter};
  Refuse("old-repeat",()=>owner.Run(new byte[4],new byte[4],default),typeof(InvalidOperationException));
  var competitor=LegacyFreshMainGlobalWordAugmentation.Prepare(ak,source,new byte[400]);
  Refuse("new-after-old-shared-claim",()=>Run(competitor),typeof(InvalidOperationException),exactMessage:"Actual completed global TOP source stage already claimed or unavailable.");CheckActual(!session.IsStopped,"new-claim-loser-no-poison");
 }
}
else if(scenario=="new-entered-failure")
{
 var owner=LegacyFreshMainGlobalWordAugmentation.Prepare(ak,source,new byte[400]);
 var genuine=LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(source.Selection,session,source.Publications,new byte[400]);
 LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(genuine,new byte[4],new byte[4]);
 Refuse("new-entered-real-quote-reentry",()=>Run(owner),typeof(InvalidOperationException),exactMessage:"This actual global callback session already entered quote cleanup.");
 CheckActual(owner.IsFaulted&&!owner.IsComplete&&session.IsStopped,"new-entered-catch-poisons-session");
 Refuse("new-faulted-repeat",()=>Run(owner),typeof(InvalidOperationException));
}
else
{
 var owner=LegacyFreshMainGlobalWordAugmentation.Prepare(ak,source,new byte[400]);session.ClaimSourceCompletion(owner,true,default);session.RequireSourceCompletion(owner,default);
 Task.Run(()=>Refuse("new-claim-thread-bound",()=>session.RequireSourceCompletion(owner,default),typeof(InvalidOperationException))).GetAwaiter().GetResult();
 // Protocol-level fault only: does NOT exercise the actual Run catch window.
 session.FaultSourceCompletion(owner);CheckActual(session.IsStopped,"new-claim-protocol-poison");Refuse("poisoned-claim-unavailable",()=>session.RequireSourceCompletion(owner,default),typeof(InvalidOperationException));
}
CheckActual(source.Selection.Publication.Policy.Owner.AcousticEvaluations==beforeNN,"augmentation-no-additional-NN");
File.WriteAllText(args[3],JsonSerializer.Serialize(new{Scenario=scenario,Scope="Actual source one inference; no accepted transcription/quality claim",NNSourceInvocations=1,ActualBit8=actualBit8,Topology=topology.Bounds,RawCompact=Convert.ToBase64String(actualCompact),RunCatchNewExercised=scenario=="new-entered-failure",Before120=Convert.ToBase64String(beforeWord),BeforeState=beforeState,AfterState=afterState,BeforeSource26Available=false,Result=result,Evidence=evidence},new JsonSerializerOptions{WriteIndented=true}));
static void Check(bool value){if(!value)throw new Exception("Prepared assertion");}
