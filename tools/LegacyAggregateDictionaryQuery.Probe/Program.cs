using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int traces=0,pages=0,negative=0;
foreach(var trace in controls.RootElement.EnumerateArray())
{
 var specs=trace.GetProperty("children").EnumerateArray().ToArray();var calls=new List<Call>();var eligibility=new List<int>();var children=new List<LegacyAggregateDictionaryChild>();
 for(int i=0;i<specs.Length;i++)
 {
  int childIndex=i;var spec=specs[i];
  children.Add(new((q,aux,clear,token)=>
  {
   token.ThrowIfCancellationRequested();calls.Add(new(childIndex,q.Node,q.Skip,aux,clear,q.Parameter1c));
   int count=Math.Min(q.Capacity,spec.GetProperty(aux?"aux":"normal")[q.Node].GetInt32()-q.Skip);
   byte[] records=new byte[Math.Max(0,count)*32];for(int j=0;j<count;j++)SyntheticRecord(spec.GetProperty("seed").GetUInt32(),q.Node,q.Skip+j).CopyTo(records,j*32);
   return new(count,records,0,q.Skip+Math.Max(0,count),q.Skip+Math.Max(0,count),clear?new byte[Math.Max(0,count)*12]:[]);
  },owner=>{eligibility.Add(childIndex);return spec.GetProperty("owners")[(int)owner].GetBoolean();}));
 }
 var aggregate=new LegacyPreparedAggregateDictionaryQuerySource(children);
 foreach(var op in trace.GetProperty("ops").EnumerateArray())
 {
  calls.Clear();eligibility.Clear();int node=op.GetProperty("node").GetInt32(),skip=op.GetProperty("skip").GetInt32();byte sid=op.GetProperty("sid").GetByte(),header=op.GetProperty("header").GetByte();
  var q=new LegacyPreparedDictionaryQuery{Node=node,Skip=skip,Capacity=op.GetProperty("capacity").GetInt32(),Parameter1c=op.GetProperty("owner").GetUInt32(),Parameter20=0xaabbccdd,Flags24=0x104,Flags26=0xabcd,Category18=13,Parameter19=17,ContextRoot=123,ParentContext=456};
  var p=aggregate.Read(Tag(node,sid,header),q,op.GetProperty("aux").GetBoolean(),op.GetProperty("clear").GetBoolean());
  Require(p.ReturnedCount==op.GetProperty("count").GetInt32()&&p.SelectedOrdinal==op.GetProperty("ordinal").GetInt32()&&p.RestoredSkip==skip&&q.Skip==skip&&p.Records.Span.SequenceEqual(Convert.FromHexString(op.GetProperty("records").GetString()!))&&p.ExtraCosts.Span.SequenceEqual(Convert.FromHexString(op.GetProperty("costs").GetString()!)),$"Aggregate page/stamp/restored-skip differs at trace{traces}/page{pages}: actual count={p.ReturnedCount},ordinal={p.SelectedOrdinal},skip={p.RestoredSkip},bytes={p.Records.Length},costs={p.ExtraCosts.Length}; expected{op.GetRawText()}.");
  Require(aggregate.CurrentSelectedOrdinal==op.GetProperty("current").GetInt32()&&aggregate.CopyAcceptedCountBases().SequenceEqual(op.GetProperty("bases").EnumerateArray().Select(x=>x.GetInt32()))&&eligibility.SequenceEqual(op.GetProperty("eligibility").EnumerateArray().Select(x=>x.GetInt32())),"Cursor/global accepted bases or eligibility differs.");
  var expectedCalls=op.GetProperty("calls").EnumerateArray().Select(x=>new Call(x.GetProperty("child").GetInt32(),x.GetProperty("node").GetInt32(),x.GetProperty("skip").GetInt32(),x.GetProperty("aux").GetBoolean(),x.GetProperty("clear").GetBoolean(),x.GetProperty("owner").GetUInt32())).ToArray();
  Require(calls.SequenceEqual(expectedCalls),"Wrong child/mode/relative skip forwarded.");
  if(p.ChildPage is {} raw)for(int j=0;j<Math.Max(0,p.ReturnedCount);j++){var original=raw.Records.Slice(j*32,32).ToArray();original[4]=(byte)(p.SelectedOrdinal+1);Require(p.Records.Span.Slice(j*32,32).SequenceEqual(original),"Stamp changed bytes outside record source ID.");}
  negative+=p.ReturnedCount<0?1:0;pages++;
 }
 traces++;
}
LegacyPreparedDictionaryQuery Query()=>new(){Node=0,Capacity=1};
LegacyAggregateDictionaryChild Good()=>new((q,a,c,t)=>new(1,new byte[32],0,1,1,c?new byte[12]:[]),_=>true);
var malformed=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>new(2,new byte[64],0,0,0,ReadOnlyMemory<byte>.Empty),_=>true)]);var badQuery=Query();Reject(()=>malformed.Read(Tag(0,0,8),badQuery));Require(badQuery.Skip==0,"Failed page did not restore skip.");Reject(()=>malformed.Read(Tag(0,0,8),badQuery));
var mutating=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>{q.Parameter20++;return new(0,ReadOnlyMemory<byte>.Empty,0,0,0,ReadOnlyMemory<byte>.Empty);},_=>true)]);Reject(()=>mutating.Read(Tag(0,0,8),Query()));Reject(()=>mutating.Read(Tag(0,0,8),Query()));
var mutatingByte19=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>{q.Parameter19++;return new(0,ReadOnlyMemory<byte>.Empty,0,0,0,ReadOnlyMemory<byte>.Empty);},_=>true)]);Reject(()=>mutatingByte19.Read(Tag(0,0,8),Query()));Reject(()=>mutatingByte19.Read(Tag(0,0,8),Query()));
var failCosts=new LegacyPreparedAggregateDictionaryQuerySource([Good()]);Reject(()=>failCosts.Read(Tag(0,1,0),Query(),clearExtraCosts:false,cancellationToken:new CancellationToken(true)));Require(failCosts.Read(Tag(0,0,8),Query()).ReturnedCount==1,"Pre-entry cancellation poisoned untouched session.");
var wrongCosts=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>new(1,new byte[32],0,0,0,new byte[11]),_=>true)]);Reject(()=>wrongCosts.Read(Tag(0,0,8),Query(),clearExtraCosts:true));
using(var cancel=new CancellationTokenSource())
{
 var canceled=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>{cancel.Cancel();return new(1,new byte[32],0,0,0,ReadOnlyMemory<byte>.Empty);},_=>true)]);Reject(()=>canceled.Read(Tag(0,0,8),Query(),cancellationToken:cancel.Token));Reject(()=>canceled.Read(Tag(0,0,8),Query()));
}
foreach(bool startup in new[]{true,false})using(var cancel=new CancellationTokenSource())
{
 var canceledEligibility=new LegacyPreparedAggregateDictionaryQuerySource([new(Good().Read,_=>{cancel.Cancel();return false;})]);Reject(()=>canceledEligibility.Read(Tag(0,startup?(byte)0:(byte)1,startup?(byte)8:(byte)0),Query(),cancellationToken:cancel.Token));Reject(()=>canceledEligibility.Read(Tag(0,0,8),Query()));
}
LegacyPreparedAggregateDictionaryQuerySource? nested=null;nested=new([new((q,a,c,t)=>{nested!.Read(Tag(0,0,8),Query());return new(0,ReadOnlyMemory<byte>.Empty,0,0,0,ReadOnlyMemory<byte>.Empty);},_=>true)]);Reject(()=>nested.Read(Tag(0,0,8),Query()));Reject(()=>nested.Read(Tag(0,0,8),Query()));
byte[] ownedRecords=new byte[32],ownedCosts=new byte[12];var snapshotSource=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>new(1,ownedRecords,0,1,1,ownedCosts),_=>true)]);var snapshot=snapshotSource.Read(Tag(0,0,8),Query(),clearExtraCosts:true);ownedRecords[0]=99;ownedCosts[0]=99;Require(snapshot.Records.Span[0]==0&&snapshot.ExtraCosts.Span[0]==0,"Child-owned record/cost arrays escaped into mutable output.");var copiedBases=snapshotSource.CopyAcceptedCountBases();copiedBases[0]=88;Require(snapshotSource.CopyAcceptedCountBases()[0]==0,"Public accepted-base copy mutated the session.");
var thrown=new LegacyPreparedAggregateDictionaryQuerySource([new((q,a,c,t)=>throw new InvalidDataException("own failure"),_=>true)]);Reject(()=>thrown.Read(Tag(0,0,8),Query()));Reject(()=>thrown.Read(Tag(0,0,8),Query()));
Reject(()=>new LegacyPreparedAggregateDictionaryQuerySource(Enumerable.Repeat(Good(),256).ToArray()));
var max=new LegacyPreparedAggregateDictionaryQuerySource(Enumerable.Repeat(Good(),255).ToArray());Require(max.Read(Tag(0,255,0),Query()).Records.Span[4]==255,"Last legal source ID narrowed incorrectly.");
foreach(ulong tag in new[]{Tag(0,0,0),Tag(0,2,0),Tag(1,1,0)}){var invalid=new LegacyPreparedAggregateDictionaryQuerySource([Good()]);Reject(()=>invalid.Read(tag,Query()));Reject(()=>invalid.Read(Tag(0,0,8),Query()));}
var rewind=new LegacyPreparedAggregateDictionaryQuerySource([Good()]);var rq=Query();rq.Skip=3;rewind.Read(Tag(0,0,8),rq);rq.Skip=2;Reject(()=>rewind.Read(Tag(0,0,8),rq));Require(rq.Skip==2,"Rejected negative-relative skip not restored.");
Console.WriteLine($"PASS {traces} independent aggregate stream traces / {pages} pages ({negative} negative nonzero stop pages); global/relative skip, cursor reset, eligibility, full32-byte source stamping, auxiliary/cost transport,255 IDs, guards/cancellation/invalidation.");
if(args.Length==1)return;
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var exactMap=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);var contexts=new LegacySymbolContextFactory();var initial=contexts.CreateInitialSourceMap(exactMap);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var permissions=Enumerable.Repeat((byte)7,model.AcousticAlphabet.LabelCount).ToArray();
LegacyPreparedDictionaryQuerySource Source(uint flags)=>new(model.Lexicon,new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,flags,exactMap,exactMap).Refresh(permissions),initial);
var source0=Source(descriptor.Flags);var source1=Source(descriptor.Flags);var selection=new LegacyPreparedDictionarySourceSelectionBuilder();selection.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor,model.LanguageModel,contexts.ContainerIdentity));selection.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor with{},model.LanguageModel,contexts.ContainerIdentity));
var selected=selection.Prepare();var graph=OwnOrdinaryGraph();var prepared=new[]{source0,source1};var real=new LegacyPreparedAggregateDictionaryQuerySource(selected.Select(s=>LegacyOrdinaryDictionarySourceOwner.BindDictionary(prepared[s.SourceIndex],contexts,graph)).ToArray());
var rootQuery=new LegacyPreparedDictionaryQuery{Node=0,Capacity=128,ContextRoot=contexts.ContainerIdentity};var realRecords=new List<byte[]>();int realPages=0;
while(true){var page=real.Read(Tag(0,0,8),rootQuery,clearExtraCosts:true);if(page.ReturnedCount==0)break;Require(page.ReturnedCount>0&&page.ExtraCosts.Length==page.ReturnedCount*12,"Actual root pagination returned an invalid extent.");for(int i=0;i<page.ReturnedCount;i++)realRecords.Add(page.Records.Slice(i*32,32).ToArray());rootQuery.Skip+=page.ReturnedCount;realPages++;}
Require(realRecords.Count==264&&realPages==4&&real.CopyAcceptedCountBases().SequenceEqual(new[]{0,132}),"Actual two-selected source pages/base offsets differ.");
for(int i=0;i<132;i++){byte[] a=realRecords[i],b=realRecords[i+132];Require(a[4]==1&&b[4]==2&&a.Take(4).SequenceEqual(b.Take(4))&&a.Skip(5).SequenceEqual(b.Skip(5)),"Actual shared context/source stamping differs across two sources.");ulong id=BinaryPrimitives.ReadUInt64LittleEndian(a.AsSpan(8));Require(contexts.ReadNode(id).Symbol==BinaryPrimitives.ReadUInt16LittleEndian(a.AsSpan(20)),"Actual source record lost acoustic-key context identity.");}
var branch=realRecords.First(r=>model.Lexicon.GetChildCount((int)BinaryPrimitives.ReadUInt32LittleEndian(r))>0);int node1=(int)BinaryPrimitives.ReadUInt32LittleEndian(branch);ulong parent=BinaryPrimitives.ReadUInt64LittleEndian(branch.AsSpan(8));var q1=new LegacyPreparedDictionaryQuery{Node=node1,Capacity=128,ContextRoot=contexts.ContainerIdentity,ParentContext=parent};var p1=real.Read(Tag(node1,1,0),q1);int contextsAfterFirst=contexts.NodeCount;var p2=real.Read(Tag(node1,2,0),q1);
Require(p1.ReturnedCount>0&&p1.ReturnedCount==p2.ReturnedCount&&contexts.NodeCount==contextsAfterFirst,"Actual shared parent-child contexts duplicated.");
for(int i=0;i<p1.ReturnedCount;i++){var a=p1.Records.Span.Slice(i*32,32);var b=p2.Records.Span.Slice(i*32,32);ulong id=BinaryPrimitives.ReadUInt64LittleEndian(a[8..]);Require(a[4]==1&&b[4]==2&&BinaryPrimitives.ReadUInt64LittleEndian(b[8..])==id&&contexts.ReadNode(id).ParentLink==parent,"Actual child contexts/source IDs lost owner link.");}
var normal800=LegacyOrdinaryDictionarySourceOwner.BindDictionary(Source(0x80a),contexts,graph);Require(normal800.IsEligible(123),"Proven ordinary21b virtual1C8 false not consumed by category800.");
var noGetter=LegacyAggregateDictionaryChild.BindDictionary(Source(0x80a),contexts);Reject(()=>noGetter.IsEligible(123));var blocked=LegacyAggregateDictionaryChild.BindDictionary(Source(0x80a),contexts,()=>true);Require(!blocked.IsEligible(0),"Explicit alternate frontend280 true not rejected.");
var auxAggregate=new LegacyPreparedAggregateDictionaryQuerySource([LegacyOrdinaryDictionarySourceOwner.BindDictionary(Source(descriptor.Flags),contexts,graph)]);var auxQuery=new LegacyPreparedDictionaryQuery{Node=0,Capacity=128,ContextRoot=contexts.ContainerIdentity};var auxPage=auxAggregate.Read(Tag(0,0,8),auxQuery,auxiliary:true,clearExtraCosts:true);Require(auxPage.ReturnedCount>=0&&Enumerable.Range(0,auxPage.ReturnedCount).All(i=>(auxPage.Records.Span[i*32+5]&2)!=0),"Actual auxiliary mode accepted a nonterminal.");
var foreignContexts=new LegacySymbolContextFactory();var foreign=new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(Source(descriptor.Flags),foreignContexts)]);Reject(()=>foreign.Read(Tag(0,0,8),new(){Node=0,ContextRoot=contexts.ContainerIdentity}));
Console.WriteLine($"PASS actual RU selection priority1024 -> two selected dictionary readers -> aggregate264records/{realPages}pages,bases0/132 -> shared139root contexts and{p1.ReturnedCount} per-parent child contexts; source IDs1/2, actual auxiliary terminal gate. Ordinary800 eligibility proven; alternate-wrapper construction/word startup/winner remain unresolved.");
static LegacyOrdinaryCursiveCandidateGraph OwnOrdinaryGraph(){var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);}
static ulong Tag(int node,byte source,byte header)=>(uint)node|((ulong)source<<32)|((ulong)header<<40);
static byte[] SyntheticRecord(uint seed,int node,int index){Span<byte> b=stackalloc byte[12];BinaryPrimitives.WriteUInt32LittleEndian(b,seed);BinaryPrimitives.WriteInt32LittleEndian(b[4..],node);BinaryPrimitives.WriteInt32LittleEndian(b[8..],index);return SHA256.HashData(b);}
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Malformed/canceled request was accepted.");}
sealed record Call(int Child,int Node,int Skip,bool Auxiliary,bool Clear,uint Owner);
