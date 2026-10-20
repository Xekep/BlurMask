using InkRecognition;
using System.Buffers.Binary;
if(args.Length!=1)throw new ArgumentException("MODELS");
byte[] R(string n)=>File.ReadAllBytes(Path.Combine(args[0],n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res"),lk=R("ru-RU/ru_RU-lk-text.lite.res");
var model=LegacyRussianModelBundle.Load(ak,lk,R("ru-RU/legacy-engine-profile.json"));
using var cf=new MemoryStream(R("text-unicode/legacy-case-data.json"));using var pf=new MemoryStream(R("text-unicode/legacy-text-publication-data.json"));
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var prepared=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication),publication);
var contexts=new LegacySymbolContextFactory();var exact=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);var initial=contexts.CreateInitialSourceMap(exact);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var map=new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,exact,[]);
LegacyPreparedDictionaryQuerySource Source()=>new(model.Lexicon,map,initial);
int checks=0;var saved=new List<(ReadOnlyMemory<byte> Page,byte[] Original)>();
void Save(ReadOnlyMemory<byte> page){if(!page.IsEmpty)saved.Add((page,page.ToArray()));}
void Verify(){foreach(var item in saved)if(!item.Page.Span.SequenceEqual(item.Original))throw new Exception("Returned page aliases later scratch writes.");checks+=saved.Count;}
void Require(bool condition,string message){if(!condition)throw new Exception(message);checks++;}
void AllocationBound(string name,Action read)
{
 for(int i=0;i<64;i++)read();long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<256;i++)read();long amount=GC.GetAllocatedBytesForCurrentThread()-before;
 Require(amount<=256*2048,$"{name} empty pages allocate {amount} bytes after warmup; maximum-capacity buffers must be reused.");Console.WriteLine($"{name}: 256 empty pages allocated {amount} bytes.");
}
var source=Source();var request=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity};
foreach(int cap in new[]{512,1,128,5,512,2,16}){request.Capacity=cap;request.Skip=0;var page=source.Read(request,false);Save(page.Records);Require(page.Records.Length==Math.Max(0,page.ReturnedCount)*32,"Prepared record prefix extent.");}
Require(saved.Count>0,"Real nonempty prepared pages required.");
foreach(var item in saved)for(int row=0;row<item.Page.Length;row+=32)
 foreach(int offset in new[]{4,6,7,16,17,18,19,23,24,25,26,27,28,29,30,31})
  Require(item.Page.Span[row+offset]==0,"Prepared record padding must remain fresh-zero after scratch reuse.");
var copy=map.CopySelectedMap(false,0);copy[0]^=1;Require(map.CopySelectedMap(false,0).AsSpan().SequenceEqual(exact),"Public map snapshots must remain independent.");
int leaf=checked((int)model.Lexicon.NodeCount-1);var emptyRequest=new LegacyPreparedDictionaryQuery{Node=leaf,Capacity=512,ContextRoot=contexts.ContainerIdentity};
AllocationBound("Prepared dictionary",()=>{var p=source.Read(emptyRequest,false);if(p.ReturnedCount!=0||!p.Records.IsEmpty)throw new Exception("Leaf must produce an empty page.");});Verify();

var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);
var expansion=new LegacyNativeCaseRecordExpansion(prepared,LegacyNativeCasePathPolicy.Create(model.LanguageModel.CasePathVersion),contexts);
var wrapper=LegacyNativeCaseNormalQuerySource.BindOrdinaryDictionary(expansion,Source(),graph,false,true);
var caseRequest=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity};var query=new LegacyNativeCaseNormalQuery{Request=caseRequest,SourceTag=0};
foreach(int cap in new[]{512,5,128,6,512}){caseRequest.Capacity=cap;caseRequest.Skip=0;var p=wrapper.Read(query);Save(p.Records);Save(p.ExtraCosts);Require(p.ExtraCosts.Length==Math.Max(0,p.ReturnedCount)*12,"Actual cost side-table extent.");}
var emptyCaseRequest=new LegacyPreparedDictionaryQuery{Node=leaf,Capacity=512,ContextRoot=contexts.ContainerIdentity};var emptyCase=new LegacyNativeCaseNormalQuery{Request=emptyCaseRequest,SourceTag=(uint)leaf};
AllocationBound("Case wrapper",()=>{var p=wrapper.Read(emptyCase);if(p.ReturnedCount!=0||!p.Records.IsEmpty||!p.ExtraCosts.IsEmpty)throw new Exception("Case leaf must remain empty.");});Verify();

var delimiterContexts=new LegacySymbolContextFactory();byte[] backend=new byte[5*32];Require(model.AcousticAlphabet.TryGetLabel("a",out ushort symbol),"Actual ordinary symbol.");
for(int i=0;i<5;i++){BinaryPrimitives.WriteUInt32LittleEndian(backend.AsSpan(i*32),(uint)(i+1));BinaryPrimitives.WriteUInt16LittleEndian(backend.AsSpan(i*32+20),symbol);}
LegacyAggregateDictionaryQueryPage Backend(ulong tag,LegacyPreparedDictionaryQuery q,CancellationToken token)
{int count=q.Skip<5?Math.Min(q.Capacity,5-q.Skip):0;return new(count,backend.AsMemory(q.Skip<5?q.Skip*32:0,count*32),ReadOnlyMemory<byte>.Empty,0,q.Skip,null);}
var delimiter=new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet,delimiterContexts,Backend,2);var dq=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=delimiterContexts.ContainerIdentity};var outer=new LegacyNativeDelimiterQuery{Request=dq,SourceTag=0};
foreach(int cap in new[]{512,1,4,512,2}){dq.Capacity=cap;dq.Skip=0;var p=delimiter.Read(outer);Save(p.Records);Require(p.ReturnedCount==Math.Min(5,cap),"Delimiter pagination.");}
var emptyDelimiter=new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet,new LegacySymbolContextFactory(),(tag,q,token)=>new(0,ReadOnlyMemory<byte>.Empty,ReadOnlyMemory<byte>.Empty,0,q.Skip,null),2);var emptyOuter=new LegacyNativeDelimiterQuery{Request=new(){Node=0,Capacity=512},SourceTag=0};
AllocationBound("Delimiter",()=>{var p=emptyDelimiter.Read(emptyOuter);if(p.ReturnedCount!=0||!p.Records.IsEmpty)throw new Exception("Empty delimiter page changed.");});Verify();
Console.WriteLine($"PASS query scratch: {checks} ownership/padding/capacity/map checks; saved pages and cost tables survive later reuse; bounded warm empty-page allocations.");
