using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),engine=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[8]);
using var ed=JsonDocument.Parse(engine);var acts=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(acts.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),acts.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);int guards=0;long compared=0;int contexts=0;ulong identity=1;
var budget=new LegacyMainGlobalRepeatedPublicationBudget(512,128*1024*1024,4096*40);
Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(0,1,40));Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(50001,1,40));
Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(1,0,40));Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(1,long.MaxValue,40));
Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(1,1,39));Reject(()=>new LegacyMainGlobalRepeatedPublicationBudget(1,1,int.MaxValue));
using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("cases");
foreach(int physical in new[]{1,61,91,121})
{
 var raw=Enumerable.Range(0,physical).Select(n=>new InkStroke(new InkPoint[]{new(n*70,90),new(n*70,10),new(n*70+25,60),new(n*70+50,10),new(n*70+50,90)})).ToArray();
 var prepared=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
 var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,prepared.Smoothed);
 var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,LegacyFreshMainGlobalOrdinaryState.Prepare(ak,LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table),table));
 var ranges=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,LegacyFreshMainGlobalContext.Prepare(ak,ranges,activations));
 LegacyFreshMainGlobalWordControllerPolicy Fresh()
 {
  var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);
  return LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table),LegacyFreshMainGlobalWordTransitions.Prepare(ak,LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table)));
 }
 var policy=Fresh();var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,policy.Owner);policy.Owner.LiveReadLog=[];
 byte[] residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();int forwarded=0;
 LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
 {forwarded++;var bytes=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
 Reject(()=>LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue,new(Math.Max(1,graph.State.NodeCount-1),budget.MaximumRequestedControllerBytes,residue.Length)));
 Reject(()=>LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue,new(512,budget.MaximumRequestedControllerBytes,40)));
 Reject(()=>LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue.AsSpan(1),budget));
 Reject(()=>LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue,new(512,1,residue.Length)));
 using(var canceled=new CancellationTokenSource()){canceled.Cancel();Reject(()=>LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue,budget,token:canceled.Token));}
 if(forwarded!=0||policy.Owner.AcousticEvaluations!=0)throw new Exception("Preflight budget/entry guards allocated or evaluated NN.");
 if(physical>1)Reject(()=>LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak,policy,Allocate,confidence,residue));
 var observed=new List<LegacyFreshMainGlobalIntermediatePublication>();var ledgerAtGate=new List<long>();
 LegacyFreshMainGlobalRepeatedWordPass? pass=null;
 pass=LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,policy,Allocate,confidence,residue,budget,p=>{observed.Add(p);ledgerAtGate.Add(pass!.RequestedControllerPayloadBytes);});
 var repeated=pass.RunOwned();var actual=repeated.PassResult;int evaluations=policy.Owner.AcousticEvaluations;
 Reject(()=>LegacyFreshMainGlobalFinalPublication.AppendRepeated(new(pass,actual),confidence,ak));
 if(evaluations!=graph.State.Groups.Count)throw new Exception("Expected one cached NN evaluation per actual group.");
 Reject(()=>pass.RunOwned());
 var reference=Fresh();reference.Owner.LiveReadLog=[];var pubs=new List<OriginalGlobalLiveSchedulerOracle.Publication>();
 var expected=OriginalGlobalLiveSchedulerOracle.Run(ak,reference,residue,null,pubs);
 if(pubs.Count!=observed.Count||observed.Count!=(graph.State.NodeCount-1)/120||actual.IntermediatePublicationCount!=observed.Count||actual.NextCompactPublicationNode2490!=120*observed.Count)throw new Exception("Repeated native gate/cursor differs.");
 for(int n=0;n<actual.NodeCount;n++)
 {
  var a=actual.GetNodeSnapshot(n);var e=expected[n];
  if(a with{Records=default,PrimaryAlternatives=default,SecondaryAlternatives=default}!=e with{Records=default,PrimaryAlternatives=default,SecondaryAlternatives=default})throw new Exception($"Independent node{n} counters differ.");
  Compare(a.Records,e.Records,88,[8],policy.Owner.Contexts,reference.Owner.Contexts);
  Compare(a.PrimaryAlternatives,e.PrimaryAlternatives,40,[8,16],policy.Owner.Contexts,reference.Owner.Contexts);
 }
 for(int i=0;i<observed.Count;i++)
 {
  var a=observed[i];var e=pubs[i];
  if(a.StartingNode2490!=i*120||a.EndingNode!=(i+1)*120-1||a.AcousticEvaluationsBefore!=a.AcousticEvaluationsAfter||a.RecordCount2470!=e.Published.Count||a.PreviousRecordCount2470!=(i==0?0:observed[i-1].RecordCount2470)||!a.CopyCompactStorage().AsSpan().SequenceEqual(e.Published.Storage))throw new Exception("Independent repeated compact state differs.");
  if(!a.CopyNodeStarts2478().AsSpan().SequenceEqual(e.Published.Starts.AsSpan(0,a.EndingNode+2))||!a.CopyPrefixOwners2488().AsSpan().SequenceEqual(e.Published.PrefixOwners.AsSpan(0,a.EndingNode+2)))throw new Exception("Persistent source tables differ.");
  for(int n=0;n<=a.EndingNode;n++){Compare(a.GetInputNodeSnapshot(n).Records,e.Before[n].Records,88,[8],policy.Owner.Contexts,reference.Owner.Contexts);Compare(a.GetPublishedNodeSnapshot(n).Records,e.Published.Nodes[n].Records,88,[8],policy.Owner.Contexts,reference.Owner.Contexts);}
 }
 if(policy.Owner.LiveReadLog.Count!=reference.Owner.LiveReadLog.Count)throw new Exception("Retained backend call extent differs.");
 for(int i=0;i<policy.Owner.LiveReadLog.Count;i++)
 {
  var a=policy.Owner.LiveReadLog[i];var e=reference.Owner.LiveReadLog[i];
  if(a with{ParentContext=0}!=e with{ParentContext=0}||!Path(policy.Owner.Contexts,a.ParentContext).SequenceEqual(Path(reference.Owner.Contexts,e.ParentContext)))throw new Exception("Same dictionary/outer290 LIVE chronology differs.");
 }
 if(physical>1)Reject(()=>LegacyFreshMainGlobalFinalPublication.Append(actual,confidence,ak));
 var final=LegacyFreshMainGlobalFinalPublication.AppendRepeated(repeated,confidence,ak);var selected=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy).SelectPublished(final);
 Reject(()=>LegacyFreshMainGlobalFinalPublication.AppendRepeated(repeated,confidence,ak));
 if(policy.Owner.AcousticEvaluations!=evaluations)throw new Exception("Repeated/final publication reevaluated NN.");
 if(physical>1&&selected.Winner.HasWinner)throw new Exception("Long no-winner fixture changed; inspect rather than manufacture a winner.");
 if(physical==61)
 {
  //The first real LIVE succeeds, then the cumulative payload boundary rejects
  //the next allocation before forwarding it. Earlier snapshots stay owned.
  var limited=Fresh();var limitedConfidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,limited.Owner);int limitedCalls=0;var kept=new List<LegacyFreshMainGlobalIntermediatePublication>();
  var limitedPass=LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,limited,r=>{limitedCalls++;return Allocate(r);},limitedConfidence,residue,new(512,ledgerAtGate[0],residue.Length),kept.Add);
  Reject(()=>limitedPass.RunOwned());if(kept.Count!=1||limitedPass.RequestedControllerPayloadBytes!=ledgerAtGate[0])throw new Exception("Budget failure did not preserve exact first LIVE prefix.");Reject(()=>limitedPass.RunOwned());
  using var cancel=new CancellationTokenSource();var canceledPolicy=Fresh();var canceledConfidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,canceledPolicy.Owner);int callbacks=0;
  var canceledPass=LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,canceledPolicy,Allocate,canceledConfidence,residue,budget,p=>{if(++callbacks==2)cancel.Cancel();});
  Reject(()=>canceledPass.RunOwned(cancel.Token));if(callbacks!=2)throw new Exception("Cancellation failed to reach second LIVE.");Reject(()=>canceledPass.RunOwned());
  var throwPolicy=Fresh();int throwingCalls=0;var throwing=LegacyFreshMainGlobalRepeatedWordPass.Prepare(ak,throwPolicy,Allocate,LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,throwPolicy.Owner),residue,budget,p=>{if(++throwingCalls==2)throw new InvalidOperationException("prepared observer failure");});
  Reject(()=>throwing.RunOwned());if(throwingCalls!=2)throw new Exception("Observer failure did not preserve repeated prefix.");Reject(()=>throwing.RunOwned());
 }
 w.WriteStartObject();w.WriteNumber("physical",physical);w.WriteNumber("nodeCount",graph.State.NodeCount);w.WriteNumber("groups",evaluations);w.WriteNumber("requestedBytes",pass.RequestedControllerPayloadBytes);w.WriteNumber("cycles",actual.IntermediatePublicationCount);w.WriteNumber("nextNode",actual.NextCompactPublicationNode2490);w.WriteNumber("finalCount",final.RecordCount2470);w.WriteBoolean("hasWinner",selected.Winner.HasWinner);w.WriteNumber("backendReads",policy.Owner.LiveReadLog.Count);w.WriteNumber("primary",policy.Configuration.PrimaryArcLimit230a);
 w.WriteStartArray("gates");foreach(var p in observed){w.WriteStartArray();w.WriteNumberValue(p.StartingNode2490);w.WriteNumberValue(p.EndingNode);w.WriteNumberValue(p.PreviousRecordCount2470);w.WriteNumberValue(p.RecordCount2470);w.WriteNumberValue(p.QueryChecks);w.WriteNumberValue(p.BackendCalls);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("nodes");for(int n=0;n<actual.NodeCount;n++){var r=actual.GetNodeSnapshot(n);w.WriteStartObject();w.WriteNumber("node",n);w.WriteNumber("sourceEnd",r.SourceEnd23f0);w.WriteNumber("retainedEnd",r.RetainedEnd23f8);w.WriteNumber("offset",r.CumulativePathCost2520);w.WriteBase64String("records",r.Records.Span);w.WriteBase64String("alternatives",r.PrimaryAlternatives.Span);w.WriteEndObject();}w.WriteEndArray();
 w.WriteBase64String("compact",final.CopyCompactStorage());w.WriteStartArray("starts");foreach(int value in final.CopyNodeStarts2478())w.WriteNumberValue(value);w.WriteEndArray();w.WriteEndObject();w.Flush();
 Console.WriteLine($"PASS actual{physical}physical/{actual.NodeCount}nodes/{evaluations}NN/{observed.Count}LIVE/{pass.RequestedControllerPayloadBytes}requestedBytes winner={selected.Winner.HasWinner}");
}
w.WriteEndArray();
//The named raw route reaches the same real repeated controller and final
//no-winner boundary; it neither fabricates a terminal nor invokes glyphs.
var shared=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);
var rawLong=Enumerable.Range(0,61).Select(n=>new InkStroke(new InkPoint[]{new(n*70,90),new(n*70,10),new(n*70+25,60),new(n*70+50,10),new(n*70+50,90)})).ToArray();
var rawInvocation=shared.PrepareGlobal(rawLong,0,table,cd,pd);
Reject(()=>rawInvocation.RunWithRepeatedPublication(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],new(512,128*1024*1024,40)));
var rawResult=rawInvocation.RunWithRepeatedPublication(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],budget);
if(rawResult.HasWinner||rawResult.Session is not null||rawResult.Publications.Count!=0||rawResult.Selection.Publication.PassResult.NodeCount!=244||rawResult.Selection.Publication.PassResult.IntermediatePublicationCount!=2||rawResult.CachedMainGroupEvaluations!=1924)throw new Exception("Raw repeated publication did not retain actual no-winner source.");
Reject(()=>rawInvocation.RunWithRepeatedPublication(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],budget));
w.WriteNumber("rawNodes",244);w.WriteNumber("rawCycles",2);w.WriteNumber("rawNn",rawResult.CachedMainGroupEvaluations);w.WriteBoolean("rawHasWinner",rawResult.HasWinner);
w.WriteNumber("guards",guards);w.WriteNumber("comparedBytes",compared);w.WriteNumber("comparedContextPaths",contexts);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS repeated LIVE {guards}guards/{compared}wholeBytes/{contexts}contextPaths");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected explicit supported-domain/budget/lifecycle rejection.");}
static ushort[] Path(LegacySymbolContextFactory contexts,ulong id){var values=new List<ushort>();while(id!=0){if(values.Count>4096)throw new Exception("Context cycle.");var n=contexts.ReadNode(id);values.Add(n.Symbol);id=n.ParentLink;}values.Reverse();return values.ToArray();}
void Compare(ReadOnlyMemory<byte> a,ReadOnlyMemory<byte> e,int stride,int[] fields,LegacySymbolContextFactory actual,LegacySymbolContextFactory expected)
{byte[] x=a.ToArray(),y=e.ToArray();if(x.Length!=y.Length||x.Length%stride!=0)throw new Exception("Independent extent differs.");for(int p=0;p<x.Length;p+=stride)foreach(int field in fields){ulong av=BinaryPrimitives.ReadUInt64LittleEndian(x.AsSpan(p+field)),ev=BinaryPrimitives.ReadUInt64LittleEndian(y.AsSpan(p+field));if(!Path(actual,av).SequenceEqual(Path(expected,ev)))throw new Exception("Independent symbolic context differs.");x.AsSpan(p+field,8).Clear();y.AsSpan(p+field,8).Clear();contexts++;}if(!x.AsSpan().SequenceEqual(y))throw new Exception("Independent complete bytes differ.");compared+=x.Length;}
