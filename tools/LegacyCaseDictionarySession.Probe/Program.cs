using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=6)throw new ArgumentException("AK LK engine-profile case-data publication-data capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[2]));
using var c=File.OpenRead(args[3]);var caseData=LegacyNativeCaseData.Load(c);using var p=File.OpenRead(args[4]);var publication=LegacyNativeTextPublicationData.Load(p);
var prep=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication),publication);
var contexts=new LegacySymbolContextFactory();var maps=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[]);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));
var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,2,true);
var binding=new LegacyPreparedCaseDictionarySymbolBinding(maps,prep,model.LanguageModel,descriptor.Flags);
var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};
var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);
var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);
var restart=LegacyPreparedDictionaryRestartTags.CreateFresh();
LegacyPreparedCaseDictionarySession Session(bool ignored=false,bool extraCosts=true)=>new(model,prefix,uint.MaxValue,ignored,caseData,publication,binding,graph,restart,extraCosts);
LegacyPreparedDictionaryQuery Q()=>new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity,Category18=77};
var raw=maps.CreateQuerySource(model.Lexicon,descriptor.Flags).Read(Q(),false,clearExtraCosts:true);
if(raw.ReturnedCount!=132)throw new Exception("Actual RU root changed.");
uint terminal=0;int word=0;
for(int i=0;i<raw.ReturnedCount;i++){uint node=BinaryPrimitives.ReadUInt32LittleEndian(raw.Records.Span[(i*32)..]);if(model.WordIndex.TryGetWordId((int)node,out word)){terminal=node;break;}}
if(terminal==0)throw new Exception("No actual terminal fixture found.");
using var output=File.Create(args[5]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("source",Convert.ToHexString(raw.Records.Span));
w.WriteStartArray("states");int emitted=0;
for(int state=0;state<28;state++)
{
 var session=Session();ulong tag=(0x83UL<<56)|((ulong)state<<58)|(0xa5UL<<48)|(0x89UL<<40);
 var actual=session.Read(tag,Q());
 var noCosts=Session(extraCosts:false).Read(tag,Q());
 if(noCosts.ReturnedCount!=actual.ReturnedCount||!noCosts.Records.Span.SequenceEqual(actual.Records.Span)||!noCosts.ExtraCosts.IsEmpty||actual.ExtraCosts.Length!=actual.ReturnedCount*12)throw new Exception("Side-cost presence changed normal records or extent.");
 if(actual.SelectedOrdinal!=0||actual.ReturnedCount<=0)throw new Exception("Owned normal source missing.");
 byte[] unstamped=actual.Records.ToArray();for(int i=0;i<actual.ReturnedCount;i++){if(unstamped[i*32+4]!=1)throw new Exception("Selected source stamp missing.");unstamped[i*32+4]=0;}
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteString("records",Convert.ToHexString(unstamped));w.WriteEndObject();emitted+=actual.ReturnedCount;
 var aux=session.Read((tag&~(255UL<<32))|(1UL<<32),Q(),true);
 if(aux.ReturnedCount!=0)throw new Exception("Genuine source60 must not become normal case expansion.");
}
w.WriteEndArray();w.WriteStartArray("metadata");
ushort[] ending={Label(","),Label("."),Label("a"),Label("A")};
foreach(bool ignored in new[]{false,true})foreach(bool start in new[]{false,true})for(int state=0;state<32;state++)foreach(ushort label in ending)
{
 var session=Session(ignored);ulong tag=terminal|(1UL<<32)|((ulong)state<<58)|(start?1UL<<40:0);
 byte[] bytes=session.ReadTerminal(tag,label,7).ToArray();if(BinaryPrimitives.ReadUInt32LittleEndian(bytes)!=1||BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(4))!=word)throw new Exception("Source/word identity conflated.");
 //Strip aggregate source ID for the independent existing234970 oracle.
 BinaryPrimitives.WriteUInt32LittleEndian(bytes,0);
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("label",label);w.WriteBoolean("ignored",ignored);w.WriteBoolean("start",start);w.WriteString("bytes",Convert.ToHexString(bytes));w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("restart");
for(int state=0;state<32;state++)for(int flag=0;flag<16;flag++)
{
 ulong incoming=0x123456UL|(1UL<<32)|((ulong)flag<<40)|(0xa5UL<<48)|(0x83UL<<56)|((ulong)state<<58);
 ulong actual=Session().ResolveRestart(incoming);w.WriteStartArray();w.WriteNumberValue(incoming);w.WriteNumberValue(actual);w.WriteEndArray();
}
w.WriteEndArray();w.WriteStartArray("refresh");
foreach(ushort allowed in new[]{Label("a"),Label("м"),ushort.MaxValue})
{
 var session=Session();byte[] input=new byte[model.AcousticAlphabet.LabelCount];if(allowed!=65535)input[allowed]=5;
 session.RefreshPermissions(input);
 var actual=session.Read(8UL<<40,Q());var direct=maps.CreateQuerySource(model.Lexicon,descriptor.Flags);
 direct.SetWorkingMap(binding.Refresh(input));var filtered=direct.Read(Q(),false,clearExtraCosts:true);
 var exact=LegacyNativeCaseDictionaryFactory.Bind292(prefix,uint.MaxValue,model.LanguageModel,model.LexicalAlphabet,caseData,publication,prep,contexts,direct,graph,true);
 var expected=exact.Read(new(){Request=Q(),SourceTag=8UL<<40});byte[] stamped=expected.Records.ToArray();for(int i=0;i<expected.ReturnedCount;i++)stamped[i*32+4]=1;
 if(actual.ReturnedCount!=expected.ReturnedCount||!actual.Records.Span.SequenceEqual(stamped))throw new Exception("Permissions did not reach actual normal backend.");
 var failed=session.BuildAndRefreshPermissions([new(new int[model.AcousticAlphabet.LabelCount],1,true,0,true,0,false)]);
 if(failed.Succeeded||failed.WorkingMap is not null||!failed.Permissions.IsEmpty)throw new Exception("Failed preparation forwarded partial map.");
 var after=session.Read(8UL<<40,Q());if(after.ReturnedCount!=actual.ReturnedCount||!after.Records.Span.SequenceEqual(actual.Records.Span))throw new Exception("Failed permission production changed selected backend.");
 w.WriteStartObject();w.WriteBase64String("input",input);w.WriteString("filteredRaw",Convert.ToHexString(filtered.Records.Span));w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("contexts");foreach(var node in contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(node.Identity);w.WriteNumberValue(node.ParentLink);w.WriteNumberValue(node.Symbol);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
try{Session().ResolveRestart(8UL<<40);throw new Exception("Implicit source0 accepted as selected1.");}catch(ArgumentException){}
try{Session().ReadTerminal(terminal|(2UL<<32),ending[0],0);throw new Exception("Foreign selected source accepted.");}catch(ArgumentException){}
try{Session().Read(8UL<<40,Q(),false,new(true));throw new Exception("Canceled accepted.");}catch(OperationCanceledException){}
var foreignModel=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[2]));
try{_=new LegacyPreparedCaseDictionarySession(foreignModel,prefix,uint.MaxValue,false,caseData,publication,binding,graph,restart,true);throw new Exception("Equal-sized foreign model owner accepted.");}catch(ArgumentException){}
Console.WriteLine($"PASS shared owned292 session/{emitted} normal records/28 nativeaux empties/512 metadata/512 restart tags/3 permission refreshes; source/word/context identities preserved. Prepared container options, not whole240 factory.");
ushort Label(string text)=>model.AcousticAlphabet.TryGetLabel(text,out ushort label)?label:throw new Exception("Missing actual symbol fixture.");
