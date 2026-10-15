using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("AK.res LK.res engine-profile.json captures.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[2]));
var contexts=new LegacySymbolContextFactory();ushort[] map=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);ulong[] initial=contexts.CreateInitialSourceMap(map);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
LegacyPreparedDictionaryQuerySource Source()=>new(model.Lexicon,new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,map,map),initial);
var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);
//The genuine092990 auxiliary60 map enables only joiner184/FFFB; its root
//terminal query is empty. The separate broad supplied map below stress-tests
//the copy kernel and is NOT presented as the native current controller map.
var nativeMaps=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[]);
if(!nativeMaps.CopyAuxiliary().Where(v=>v!=ushort.MaxValue).SequenceEqual(new ushort[]{184,0xfffb}))throw new Exception("Owned native auxiliary map changed.");
foreach(int state in Enumerable.Range(0,32))
{
 var native=LegacyNativeCaseAuxiliaryQuerySource.BindOrdinaryDictionary(nativeMaps.CreateQuerySource(model.Lexicon,descriptor.Flags),contexts,graph);
 var result=native.Read((ulong)state<<58,new(){Node=0,Capacity=7,ContextRoot=contexts.ContainerIdentity},true);
 if(result.ReturnedCount!=0||!result.Records.IsEmpty||!result.ClearedExtraCosts.IsEmpty)throw new Exception("Native auxiliary root unexpectedly admitted normal letters.");
}
var raw=Source().Read(new(){Node=0,Capacity=512,ContextRoot=contexts.ContainerIdentity},true,clearExtraCosts:true);
if(raw.ReturnedCount==0)throw new Exception("Actual prepared auxiliary32 fixture must contain real terminals.");
using var file=File.Create(args[3]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteNumber("nativeRootRecords",0);w.WriteString("stressAuxiliaryMap","explicit exactMap forcopy stress, NOT native092990auxiliary60");w.WriteString("source",Convert.ToHexString(raw.Records.Span));w.WriteStartArray("states");int count=0;
foreach(int state in Enumerable.Range(0,32))
{
 var source=LegacyNativeCaseAuxiliaryQuerySource.BindOrdinaryDictionary(Source(),contexts,graph);var query=new LegacyPreparedDictionaryQuery{Node=0,Capacity=7,ContextRoot=contexts.ContainerIdentity};
 var all=new List<byte>();var costs=new List<byte>();int pages=0;
 while(true)
 {
  var page=source.Read((ulong)state<<58,query,true);if(page.ReturnedCount==0)break;
  if(page.ReturnedCount<0||pages++>256)throw new Exception("Auxiliary pagination failed.");
  all.AddRange(page.Records.ToArray());costs.AddRange(page.ClearedExtraCosts.ToArray());query.Skip+=page.ReturnedCount;
 }
 byte[] bytes=all.ToArray();if(bytes.Length!=raw.Records.Length||costs.Count!=raw.ClearedExtraCosts.Length||costs.Any(v=>v!=0))throw new Exception("Native aux32 changed extent or untouched costs.");
 for(int i=0;i<raw.ReturnedCount;i++)
 {
  var expected=raw.Records.Slice(i*32,32).ToArray();expected[7]=(byte)((expected[7]&0x83)|(state<<2));
  if(!bytes.AsSpan(i*32,32).SequenceEqual(expected)||(bytes[i*32+5]&2)==0)throw new Exception("Aux32 transport changed noncase fields or terminal gate.");
  ulong id=BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(i*32+8));if(contexts.ReadNode(id).Symbol!=BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(i*32+20)))throw new Exception("Actual context owner changed.");
 }
 count+=raw.ReturnedCount;w.WriteStartObject();w.WriteNumber("state",state);w.WriteString("records",Convert.ToHexString(bytes));w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();
var bad=LegacyNativeCaseAuxiliaryQuerySource.BindOrdinaryDictionary(Source(),contexts,graph);
try{bad.Read(1,new(){Node=0});throw new Exception("Mismatched source tag accepted.");}catch(ArgumentException){}
try{bad.Read(0,new(){Node=0});throw new Exception("Failed owner remained usable.");}catch(InvalidOperationException){}
try{LegacyNativeCaseAuxiliaryQuerySource.BindOrdinaryDictionary(Source(),contexts,graph).Read(0,new(){Node=0},false,new(true));throw new Exception("Canceled aux accepted.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS actual owned092990auxiliary root0 for32states; supplied-map stress terminals={raw.ReturnedCount}/{count} records including31,7-record paging, unchanged contexts/costs, failure/cancellation guards. Not controller/winner.");
