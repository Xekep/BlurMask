using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

bool observe=args.Length==3&&args[0]=="--observe-root";
if(!observe&&args.Length!=2)throw new ArgumentException("[--observe-root] MODELS CAPTURE");
var root=args[observe?1:0];var capturePath=args[observe?2:1];byte[] Read(string path)=>File.ReadAllBytes(Path.Combine(root,path));
byte[] ak=Read("ru-RU/ru_RU-ak-cur.lite.res"),lk=Read("ru-RU/ru_RU-lk-text.lite.res");
using var engine=JsonDocument.Parse(Read("ru-RU/legacy-engine-profile.json"));var ac=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(ac.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),ac.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var cf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-text-publication-data.json"));
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);var line30=Read("line-geometry/line-2e3-30.res");var line50=Read("line-geometry/line-2e3-50.res");
var linguistic=LegacyLinguisticRoot13FWireLayout.Read(lk);var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(linguistic);
var outgoing=LegacyFst13AOutgoingSource.CreateActualRussianNumber(linguistic,wire,ak);
var primary=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak);
var policy=LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary,ak);var initial=LegacyFst28CInitialState.CreateActualRussianNumberFresh(policy);
LegacyFreshMainCompactOrdinaryGraph Graph(InkStroke[] strokes)
{
 var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,strokes,0,activations,table,cd,pd).Run();
 var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);
 return LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table));
}
var patterns=new InkStroke[][]{
 [new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],
 [new(new InkPoint[]{new(0,10),new(30,90)}),new(new InkPoint[]{new(40,90),new(70,10)})],
 [new(new InkPoint[]{new(0,10),new(40,90),new(80,10)}),new(new InkPoint[]{new(0,90),new(40,10),new(80,90)})],
 [new(new InkPoint[]{new(0,90),new(0,10)}),new(new InkPoint[]{new(30,90),new(30,10)}),new(new InkPoint[]{new(0,230),new(0,150)})]};
var graphs=patterns.Select(Graph).ToArray();
var boots=graphs.Select(g=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,g,ak)).ToArray();
LegacyFst28CNumberSymbolMasks NewMasks(int i=0)=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boots[i],ak);
var masks=Enumerable.Range(0,4).Select(i=>NewMasks(i)).ToArray();
var owners=masks.Select(v=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(v,ak)).ToArray();

int ContextCount(LegacyFreshMainCompactOrdinaryGraph graph)
{
 int total=0;foreach(var s in graph.Source.Source.Steps)
 {if(s.IsPendingSeparator)continue;int k=s.LastPhysical-s.FirstPhysical;total+=k==0?0:k==1?1:3*k-3;}
 return total;
}
RootObservationDto RootInput(int i,LegacyFst28CNumberInitialConsumerContext context)
{
 var g=graphs[i];var state=g.State;var node=state.GetNode(0);var actual=state.Root.CopySelector(1);
 var before=g.Source.State.Root.CopySuccessors()[0];
 if(!actual.Select(x=>(int)x).SequenceEqual(before))throw new Exception("Same original root/compact selector differs.");
 var selected=owners[i].CopySelectedGroups();
 var rows=selected.Select((group,ord)=>new RootCostDto(group,owners[i].CopyGroup(ord).FinalCosts,state.Groups[group].CopyRanges().Select(r=>new[]{r.StrokeIndex,r.StartPoint,r.EndPointExclusive}).ToArray())).ToArray();
 var nodes=context.ReadActiveContextNodes();var ids=nodes.Select((n,index)=>(n.Identity,index)).ToDictionary(x=>x.Identity,x=>x.index);
 int Id(ulong id)=>id==0?-1:ids[id];
 return new(i,state.Root.CopyCumulativeEnds(),state.Root.CopyIndices(),actual,before,
  node.Select(a=>new RootArcDto(a.GroupId,a.Span,a.StartContextId,a.CopySelector(0))).ToArray(),selected,rows,owners[i].CopyEligibility(),
  Enumerable.Range(0,context.Count).Select(n=>{var row=context.ReadGraft(n).SourceRecord;return new long[]{unchecked((long)row.Tag),Id(row.ContextIdentity),row.Cost,row.Symbol,row.SourceCategory,row.CaseFlags,unchecked((long)row.Extra)};}).ToArray(),
  Enumerable.Range(0,context.Count).Select(n=>{var row=context.ReadGraft(n);return new[]{row.Ordinal,Id(row.SourceRecord.ContextIdentity),Id(row.GraftedContextIdentity)};}).ToArray(),
  context.SourceNamespace,context.SourceHeader,context.ActiveContextNodeCount);
}
if(observe)
{
 var contexts=owners.Select(d=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d)).ToArray();
 var input=Enumerable.Range(0,4).Select(i=>RootInput(i,contexts[i])).ToArray();
 int glyph=owners.Sum(d=>d.AcousticEvaluations)*4,windows=graphs.Sum(ContextCount);
 File.WriteAllText(capturePath,JsonSerializer.Serialize(new RootCaptureDto("number-genuine-root-observation-v1",false,glyph,windows,glyph+windows,input),ProbeJsonContext.Default.RootCaptureDto));
 Console.WriteLine($"PASS scoped genuine root observation4owners/{glyph}+{windows}NN; NEW consumer NOT executed");return;
}
// Everything below is COMPILED ONLY until root authorizes NEW runtime.
var guards=new List<string>();var exceptionEvidence=new List<ExceptionEvidenceDto>();
void RefuseCancellation(string name,CancellationToken expected,Action action,Func<bool>? reached=null)
{
 try{action();}
 catch(OperationCanceledException e)
 {
  bool hit=reached?.Invoke()??true;
  if(e.GetType()!=typeof(OperationCanceledException)||e.CancellationToken!=expected||!expected.IsCancellationRequested||!hit)
   throw new Exception("Wrong cancellation provenance "+name,e);
  exceptionEvidence.Add(new(name,e.GetType().FullName!,string.Empty,hit,true,true));guards.Add(name);return;
 }
 throw new Exception("Missing cancellation "+name);
}
void RefuseFault(string name,Action action,Func<bool> reached)
{
 try{action();}
 catch(IOException e)
 {
  if(e.GetType()!=typeof(IOException)||e.Message!="bounded diagnostic fault"||!reached())throw new Exception("Wrong fault "+name,e);
  exceptionEvidence.Add(new(name,e.GetType().FullName!,e.Message,true,false,false));guards.Add(name);return;
 }
 throw new Exception("Missing fault "+name);
}
void Require(bool value,string name){if(!value)throw new Exception(name);guards.Add(name);}
void Refuse<T>(string name,Action action)where T:Exception
{try{action();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
var inputs=owners.Select(d=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d)).ToArray();
LegacyFst28CNumberInitialConsumer Run(LegacyFst28CNumberInitialConsumerContext c,int bytes=754206,int work=16264293,CancellationToken token=default,Action<string>? cp=null,Action<LegacyFst28CNumberInitialAttempt>? trace=null)
 =>LegacyFst28CNumberInitialConsumer.CreateBounded(c,ak,bytes,work,token,cp,trace);
for(int i=0;i<4;i++)for(int j=i+1;j<4;j++)
 Require(!ReferenceEquals(inputs[i],inputs[j])&&!ReferenceEquals(owners[i],owners[j])&&
  !ReferenceEquals(graphs[i],graphs[j])&&!ReferenceEquals(boots[i],boots[j]),"four-independent-fixture-owners-"+i+"-"+j);
Refuse<ArgumentNullException>("null-context",()=>LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(null!,ak));
var cancelledAtEntry=new CancellationToken(true);RefuseCancellation("cancel-before-null",cancelledAtEntry,()=>LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(null!,ak,cancelledAtEntry));
foreach(int b in new[]{-1,754207,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+b,()=>Run(inputs[0],b));
foreach(int b in new[]{-1,16264294,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+b,()=>Run(inputs[0],754206,b));
Refuse<InvalidDataException>("one-byte-short",()=>Run(inputs[0],754205));Refuse<InvalidDataException>("one-work-short",()=>Run(inputs[0],754206,16264292));
foreach(int n in new[]{0,572762,572764})Refuse<ArgumentException>("foreign-resource-length-"+n,()=>LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(inputs[0],new byte[n]));
var altered=(byte[])ak.Clone();altered[188222]^=1;Refuse<ArgumentException>("mutated-same-size-resource",()=>LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(inputs[0],altered));
LegacyFst28CNumberInitialConsumer.ValidateAssociation(inputs[0],inputs[0]);guards.Add("same-typed-pool-no-claim");
Refuse<ArgumentException>("foreign-genuine-context-pool",()=>LegacyFst28CNumberInitialConsumer.ValidateAssociation(inputs[0],inputs[1]));
using(var cancel=new CancellationTokenSource())
{bool reached=false;RefuseCancellation("preclaim-cancel",cancel.Token,()=>Run(inputs[0],cp:s=>{if(s=="before-claim"){reached=true;cancel.Cancel();}},token:cancel.Token),()=>reached);Require(reached,"preclaim-cancel-reached");}
var results=new LegacyFst28CNumberInitialConsumer[4];var traces=new List<LegacyFst28CNumberInitialAttempt>[4];
for(int i=0;i<4;i++)
{
 int k=i;var trace=traces[i]=[];var context=inputs[i];var beforeNodes=context.ReadActiveContextNodes().ToArray();
 var r=results[i]=Run(context,trace:t=>trace.Add(t));
 Require(ReferenceEquals(r.Context,context)&&ReferenceEquals(r.D0,owners[i]),"same-context-d0-"+i);
 Require(!r.HasQueryAuthority&&!r.HasParentHistoryAuthority&&!r.HasAcceptedText&&!r.HasCompleteWordStartup,"initial-heap-only-"+i);
 Require(r.LogicalByteReservation==754206&&r.WorkReservation==16264293&&r.HeapCapacity==250,"fixed-bounds-"+i);
 Require(context.ReadActiveContextNodes().SequenceEqual(beforeNodes),"same-pool-unchanged-after-scoring-"+i);
 Require(r.CopyRootOrdinals().SequenceEqual(graphs[i].State.Root.CopySelector(1)),"genuine-root-order-"+i);
 Require(trace.Count==r.ScoringAttempts&&r.ScoringAttempts<=6144,"complete-attempt-trace-"+i);
 var diagnostic=r.ReadOwnerDiagnostic();Require(diagnostic[0]==1&&diagnostic[1]==1&&diagnostic[2]==1&&diagnostic[3]==1&&diagnostic[9]==r.CandidateCount,"same-private-owner-diagnostic-"+i);
 diagnostic[0]=0;Require(r.ReadOwnerDiagnostic()[0]==1,"detached-diagnostic-"+i);
 var roots=r.CopyRootOrdinals();if(roots.Length>0){roots[0]=65535;Require(r.CopyRootOrdinals()[0]!=65535,"defensive-root-copy-"+i);}
 var inspectionToken=new CancellationToken(true);RefuseCancellation("canceled-root-inspection-"+i,inspectionToken,()=>r.CopyRootOrdinals(inspectionToken));
 foreach(int n in new[]{-1,r.CandidateCount})Refuse<ArgumentOutOfRangeException>("candidate-index-"+i+"-"+n,()=>r.ReadCandidate(n));
 if(r.CandidateCount>0){var bytes=r.CopyInitializedHeapRecord(0);bytes[0]^=1;Require(!bytes.SequenceEqual(r.CopyInitializedHeapRecord(0)),"defensive-heap-copy-"+i);}
 Refuse<InvalidOperationException>("same-context-replay-"+i,()=>Run(context));
 var copied=owners[i].CopyInitialConsumerSingleRangeCosts(0,default);int originalCost=copied.ClassCosts.Span[0];System.Runtime.InteropServices.MemoryMarshal.TryGetArray(copied.ClassCosts,out ArraySegment<int> cells);cells.Array![cells.Offset]=originalCost^1;
 Require(owners[i].CopyInitialConsumerSingleRangeCosts(0,default).ClassCosts.Span[0]==originalCost,"projected-cache-copy-not-authority-"+i);
 Refuse<ArgumentException>("foreign-group-projection-"+i,()=>owners[i].CopyInitialConsumerSingleRangeCosts(-1,default));
 int good=0;Parallel.For(0,256,_=>{if(r.ReadOwnerDiagnostic()[0]==1&&r.CopyRootOrdinals().Length==r.ReadOwnerDiagnostic()[7])Interlocked.Increment(ref good);});Require(good==256,"parallel-readonly-inspection-"+i);
}
int extras=0,extraNN=0;var purposes=new List<string>();
LegacyFst28CNumberInitialConsumerContext Extra(string why)
{
 var boot=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graphs[0],ak);
 var mask=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boot,ak);var d=LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(mask,ak);
 extras++;extraNN+=d.AcousticEvaluations*4;purposes.Add(why);return LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d);
}
foreach(string stop in new[]{"after-claim","before-score-0","after-score-0","before-allocate-0","after-allocate-0","before-heap-snapshot","before-publish"})
{
 var c=Extra("cancel-"+stop);using var cancel=new CancellationTokenSource();bool reached=false;
 if(stop=="after-claim")
 {Refuse<ArgumentException>("equal-data-foreign-pool",()=>LegacyFst28CNumberInitialConsumer.ValidateAssociation(inputs[0],c));}
 RefuseCancellation("postclaim-cancel-"+stop,cancel.Token,()=>Run(c,token:cancel.Token,cp:s=>{if(s==stop){reached=true;cancel.Cancel();}}),()=>reached);
 Require(reached,"postclaim-checkpoint-"+stop);Refuse<InvalidOperationException>("poison-replay-"+stop,()=>Run(c));
 Require(c.ReadActiveContextNodes().Count==18+c.Count,"historical-context-retained-"+stop);
}
{
 var c=Extra("fault-after-allocate");bool reached=false;RefuseFault("injected-allocation-prefix-fault",()=>Run(c,cp:s=>{if(s=="after-allocate-0"){reached=true;throw new IOException("bounded diagnostic fault");}}),()=>reached);
 Refuse<InvalidOperationException>("fault-consumed",()=>Run(c));
}
{
 var c=Extra("same-context-barrier-race");using var arrived=new CountdownEvent(2);using var release=new ManualResetEventSlim(false);int won=0,lost=0;
 void Race(){try{Run(c,cp:s=>{if(s=="before-claim"){arrived.Signal();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new TimeoutException();}});Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}}
 var one=Task.Run(Race);var two=Task.Run(Race);if(!arrived.Wait(TimeSpan.FromSeconds(30)))throw new TimeoutException();release.Set();Task.WaitAll(one,two);
 Require(won==1&&lost==1,"one-shared-context-claim-winner");
}
// PREPARED neutral numerical controls cannot be redeemed as typed source/context.
var prepared=new List<PreparedHeapDto>();
foreach(int capacity in new[]{0,1,2,250})
{
 var heap=new LegacyNativeWordRecordHeap(capacity);int[] costs=[10,10,9,11,8,7,7,6];var accepted=new List<bool>();
 foreach(int cost in costs)accepted.Add(heap.TryAllocate(cost,slot=>BinaryPrimitives.WriteInt32LittleEndian(slot[16..],cost)));
 prepared.Add(new(capacity,costs,accepted.ToArray(),Enumerable.Range(0,heap.Count).Select(n=>Convert.ToHexString(heap.GetSlotSnapshot(n).Span[..120])).ToArray(),false));
}
ConsumerCaseDto DumpCase(int i)
{
 var r=results[i];var context=inputs[i];var ids=context.ReadActiveContextNodes().Select((n,index)=>(n.Identity,index)).ToDictionary(n=>n.Identity,n=>checked((ulong)(n.index+1)));
 ulong Id(ulong id)=>id==0?0:ids[id];
 string Record(int index)
 {var bytes=r.CopyInitializedHeapRecord(index);BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(8),Id(BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8))));return Convert.ToHexString(bytes);}
 var candidates=Enumerable.Range(0,r.CandidateCount).Select(n=>{var c=r.ReadCandidate(n);return c with{ContextIdentity=Id(c.ContextIdentity)};}).ToArray();
 var trace=traces[i].Select(t=>new AttemptDto(t.Row,t.Variant,t.Arc,t.Group,t.Category,t.Prototype,t.ClassCost,t.GeometryCost,
  Convert.ToHexString(t.RequestBefore),Convert.ToHexString(t.RequestAfter),t.CostAfter,t.AcousticAccepted,t.HeapAccepted,
  t.Fields is{}fields?fields with{Auxiliary08=Id(fields.Auxiliary08)}:null)).ToArray();
 return new(i,RootInput(i,context),r.CandidateCount,r.QualifiedRows,r.ScoringAttempts,r.AcousticAcceptances,r.HeapAcceptances,r.HeapRejections,
  r.MissingEndAlternates,r.ReadOwnerDiagnostic(),Enumerable.Range(0,r.CandidateCount).Select(Record).ToArray(),candidates,trace);
}
var cases=Enumerable.Range(0,4).Select(DumpCase).ToArray();
string FixtureSignature(InkStroke[] strokes)
{
 using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
 writer.Write(strokes.Length);foreach(var stroke in strokes){writer.Write(stroke.Points.Count);foreach(var point in stroke.Points){writer.Write(point.X);writer.Write(point.Y);}}
 writer.Flush();return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
}
var fixtureSignatures=patterns.Select(FixtureSignature).ToArray();
var capture=new ConsumerCaptureDto("number-owning-initial-consumer-v1",false,false,false,754206,16264293,16,graphs.Sum(ContextCount),extras,extraNN,0,purposes.ToArray(),guards.ToArray(),fixtureSignatures,exceptionEvidence.ToArray(),cases,prepared.ToArray());
File.WriteAllText(capturePath,JsonSerializer.Serialize(capture,ProbeJsonContext.Default.ConsumerCaptureDto));
Console.WriteLine($"PASS4 owning initial consumers/{guards.Count}guards;19canonicalNN and{extraNN}extra glyphNN separately; noacceptedtext/query/history");

sealed record RootArcDto(uint Group,byte Span,uint Auxiliary34,ushort[] Predecessors);
sealed record RootCostDto(int Group,int[] Costs,int[][] Ranges);
sealed record RootObservationDto(int Index,ushort[] RootEnds,ushort[] RootIndices,ushort[] ActualRoot,int[] SourceRootSuccessors0,
 RootArcDto[] Node0,int[] Selected,RootCostDto[] Rows,byte[] Eligibility,long[][] SourceRecords,int[][] Grafts,int SourceNamespace,uint SourceHeader,int NodeCount);
sealed record RootCaptureDto(string Schema,bool NewConsumerExecuted,int GlyphNN,int ContextNN,int TotalNN,RootObservationDto[] Actual);
sealed record PreparedHeapDto(int Capacity,int[] Costs,bool[] Accepted,string[] Final,bool Authority);
sealed record AttemptDto(int Row,int Variant,ushort Arc,int Group,int Category,int Prototype,int ClassCost,int GeometryCost,
 string Before,string After,int CostAfter,bool AcousticAccepted,bool HeapAccepted,LegacyInitialWordRecordFields? Fields);
sealed record ConsumerCaseDto(int Index,RootObservationDto Input,int Count,int Qualified,int Attempts,int Acoustic,int HeapAccepted,int HeapRejected,
 int Missing,int[] Diagnostic,string[] Records,LegacyFst28CNumberInitialCandidate[] Candidates,AttemptDto[] Trace);
sealed record ExceptionEvidenceDto(string Name,string Type,string Message,bool Reached,bool MatchingToken,bool Requested);
sealed record ConsumerCaptureDto(string Schema,bool Query,bool History,bool AcceptedText,int Bytes,int Work,int CanonicalGlyphNN,int CanonicalContextNN,
 int ExtraOwners,int ExtraGlyphNN,int ExtraContextNN,string[] Purposes,string[] Guards,string[] FixtureSignatures,ExceptionEvidenceDto[] Exceptions,ConsumerCaseDto[] Cases,PreparedHeapDto[] Prepared);
[JsonSourceGenerationOptions(PropertyNamingPolicy=JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RootCaptureDto))]
[JsonSerializable(typeof(ConsumerCaptureDto))]
partial class ProbeJsonContext:JsonSerializerContext {}
