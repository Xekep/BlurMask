using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=6)throw new ArgumentException("AK.res LK.res engine-profile case-data publication-data captures.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[2]));
using var cs=File.OpenRead(args[3]);var caseData=LegacyNativeCaseData.Load(cs);using var ps=File.OpenRead(args[4]);var publication=LegacyNativeTextPublicationData.Load(ps);
var preparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication),publication);
var contexts=new LegacySymbolContextFactory();var maps=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[]);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);
LegacyPreparedDictionaryQuerySource Source()=>maps.CreateQuerySource(model.Lexicon,descriptor.Flags);
LegacyTaggedDictionaryChild CaseChild()=>LegacyTaggedDictionaryChild.BindCase292(prefix,uint.MaxValue,model.LanguageModel,model.LexicalAlphabet,caseData,publication,preparation,contexts,Source(),graph,true);
var raw=Source().Read(new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity},false,clearExtraCosts:true);
if(raw.ReturnedCount!=132)throw new Exception("Actual raw dictionary root changed.");
using var file=File.Create(args[5]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("raw",Convert.ToHexString(raw.Records.Span));w.WriteStartArray("normal");int pages=0,rows=0;
for(int state=0;state<28;state++)
{
 var plain=LegacyTaggedDictionaryChild.BindOrdinaryDictionary(Source(),contexts,graph);var cased=CaseChild();var aggregate=new LegacyTaggedAggregateDictionaryQuerySource([plain,cased]);
 var query=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=77};
 ulong tag=(0x83UL<<56)|((ulong)state<<58)|(0xa5UL<<48)|(0x89UL<<40); //case bits distinct from category77, sourceID0.
 var first=aggregate.Read(tag,query,clearExtraCosts:true);pages++;
 if(first.SelectedOrdinal!=0||first.ReturnedCount!=132||plain.LastSourceTag!=tag||plain.LastMethodOffset!=0x40)throw new Exception("First selected source lost full request tag.");
 query.Skip=first.ReturnedCount;var second=aggregate.Read(tag,query,clearExtraCosts:true);pages++;
 if(second.SelectedOrdinal!=1||second.ReturnedCount<=0||query.Skip!=132||cased.LastSourceTag!=tag||cased.LastMethodOffset!=0x40||!aggregate.CopyAcceptedCountBases().SequenceEqual(new[]{0,132}))throw new Exception("Case child tag, relative skip or selected source base changed.");
 var direct=LegacyNativeCaseDictionaryFactory.Bind292(prefix,uint.MaxValue,model.LanguageModel,model.LexicalAlphabet,caseData,publication,preparation,contexts,Source(),graph,true);
 var directRequest=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=77};
 var expected=direct.Read(new(){Request=directRequest,SourceTag=tag});
 byte[] stamped=expected.Records.ToArray();for(int i=0;i<expected.ReturnedCount;i++)stamped[i*32+4]=2;
 if(second.ReturnedCount!=expected.ReturnedCount||!second.Records.Span.SequenceEqual(stamped)||!second.ExtraCosts.Span.SequenceEqual(expected.ExtraCosts.Span))throw new Exception("Aggregate case source differs from the exact full-tag direct producer.");
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("tag",tag);w.WriteNumber("forwarded",cased.LastSourceTag);w.WriteString("records",Convert.ToHexString(second.Records.Span));w.WriteEndObject();rows+=second.ReturnedCount;
}
w.WriteEndArray();w.WriteStartArray("auxiliary");
//A broad supplied aux map is explicitly a stress fixture, not native092990 map.
ushort[] exact=maps.CopyPrimary();LegacyPreparedDictionaryQuerySource StressSource()=>new(model.Lexicon,new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,exact,exact),maps.CopyInitialContexts());
var auxiliaryBase=StressSource().Read(new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=99},true,clearExtraCosts:true);
for(int state=0;state<32;state++)
{
 var child=LegacyTaggedDictionaryChild.BindCase292(prefix,uint.MaxValue,model.LanguageModel,model.LexicalAlphabet,caseData,publication,preparation,contexts,StressSource(),graph,true);
 var aggregate=new LegacyTaggedAggregateDictionaryQuerySource([child]);var q=new LegacyPreparedDictionaryQuery{Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=99};
 ulong tag=(uint)0|(1UL<<32)|(0x83UL<<56)|((ulong)state<<58)|(0x5aUL<<48); //continuation source ID1 must survive forwarding.
 var actual=aggregate.Read(tag,q,true,true);pages++;
 var direct=LegacyNativeCaseAuxiliaryQuerySource.BindOrdinaryDictionary(StressSource(),contexts,graph).Read(tag,new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=99},true);
 byte[] expected=direct.Records.ToArray();for(int i=0;i<direct.ReturnedCount;i++)expected[i*32+4]=1;
 if(actual.ReturnedCount<=0||actual.SelectedOrdinal!=0||child.LastSourceTag!=tag||child.LastMethodOffset!=0x48||!actual.Records.Span.SequenceEqual(expected))throw new Exception("Auxiliary source used expansion, guessed category, or lost source/case bits.");
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("tag",tag);w.WriteNumber("forwarded",child.LastSourceTag);w.WriteString("records",Convert.ToHexString(actual.Records.Span));w.WriteEndObject();rows+=actual.ReturnedCount;
}
w.WriteEndArray();w.WriteString("auxiliaryBase",Convert.ToHexString(auxiliaryBase.Records.Span));w.WriteString("auxiliaryStressScope","supplied broad map; NOT native092990auxiliary60");
for(int state=0;state<32;state++)
{
 var plain=LegacyTaggedDictionaryChild.BindOrdinaryDictionary(Source(),contexts,graph);var cased=CaseChild();var actual=new LegacyTaggedAggregateDictionaryQuerySource([plain,cased]);
 ulong tag=(0x83UL<<56)|((ulong)state<<58)|(0xa5UL<<48)|(8UL<<40);
 var result=actual.Read(tag,new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity},true,true);pages++;
 if(result.ReturnedCount!=0||result.SelectedOrdinal!=-1||actual.CurrentSelectedOrdinal!=2||plain.LastSourceTag!=tag||cased.LastSourceTag!=tag||plain.LastMethodOffset!=0x48||cased.LastMethodOffset!=0x48||!actual.CopyAcceptedCountBases().SequenceEqual(new[]{0,0}))throw new Exception("Native auxiliary-map empty aggregate cursor/tag flow changed.");
}
w.WriteNumber("nativeAuxiliaryEmptyStates",32);w.WriteStartArray("contexts");foreach(var context in contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(context.Identity);w.WriteNumberValue(context.ParentLink);w.WriteNumberValue(context.Symbol);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
var canceled=new LegacyTaggedAggregateDictionaryQuerySource([CaseChild()]);try{canceled.Read(8UL<<40,new(){Node=0,Capacity=512},false,true,new(true));throw new Exception("Canceled aggregate accepted.");}catch(OperationCanceledException){}
var wrong=new LegacyTaggedAggregateDictionaryQuerySource([CaseChild()]);try{wrong.Read(8UL<<40,new(){Node=0,Capacity=512},false,false);throw new Exception("Prepared side-cost mismatch accepted.");}catch(ArgumentException){}
try{wrong.Read(8UL<<40,new(){Node=0,Capacity=512},false,true);throw new Exception("Failed tagged owner remained usable.");}catch(InvalidOperationException){}
Console.WriteLine($"PASS actual owned132raw→28normal case states +32auxstress states/{rows} records/{pages} calls, exact64-bit tags, relative skip/stamp, V40vsV48 dispatch, context/category separation and guards. Prepared maps/options; not controller/winner.");
