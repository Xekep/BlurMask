using InkRecognition;
using System.Text.Json;
if(args.Length is not(3 or 7))throw new ArgumentException("case-data publication-data query-controls [AK.res LK.res engine-profile captures.json]");
using var caseStream=File.OpenRead(args[0]);var caseData=LegacyNativeCaseData.Load(caseStream);
using var pubStream=File.OpenRead(args[1]);var publication=LegacyNativeTextPublicationData.Load(pubStream);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));int tests=0,emissions=0,unsupported=0;
foreach(var input in controls.RootElement.EnumerateArray())
{
 var a=input.GetProperty("alphabet");var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(a.GetProperty("payload").GetString()!),a.GetProperty("version").GetUInt16());
 var preparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(alphabet,caseData,publication),publication);
 var contexts=new LegacySymbolContextFactory();
 foreach(var item in input.GetProperty("tests").EnumerateArray())
 {
  var policy=LegacyNativeCasePathPolicy.Create(item.GetProperty("version").GetUInt16());var producer=new LegacyNativeCaseRecordExpansion(preparation,policy,contexts);
  byte[] raw=Convert.FromHexString(item.GetProperty("input").GetString()!);int state=item.GetProperty("state").GetInt32();
  if(producer.CountForSkip(raw,state)!=item.GetProperty("count").GetInt32())throw new Exception("Pre-skip expansion count mismatch.");
  var expected=item.GetProperty("expected");tests++;
  if(expected.ValueKind==JsonValueKind.Null)
  {
   try{producer.Expand(raw,state,0,0,false,out _);throw new Exception("Uninitialized native form read accepted.");}
   catch(NotSupportedException){unsupported++;}continue;
  }
  var actual=producer.Expand(raw,state,0,0,false,out int nextState);
  if(nextState!=state || actual.Count!=expected.GetArrayLength())throw new Exception("Expansion count/state mismatch.");
  for(int i=0;i<actual.Count;i++)
  {if(!actual[i].Record.Span.SequenceEqual(Convert.FromHexString(expected[i][0].GetString()!))||actual[i].CostDelta!=expected[i][1].GetInt32())throw new Exception("Native case ordered record/cost mismatch.");emissions++;}
  try{producer.Expand(raw,state,0,0,false,out _,new(true));throw new Exception("Canceled case expansion accepted.");}catch(OperationCanceledException){}
 }
}
//Restart preserves all61 other bits and ignores state without exact source-header4.
var random=new Random(0x233f70);
for(int n=0;n<1024;n++)
{
 ulong inner=(ulong)random.NextInt64(),request=(ulong)random.NextInt64();
 ulong actual=LegacyNativeCasePathPolicy.ApplyRestart(inner,request),mask=0x7cUL<<56;
 ulong expected=((request>>40)&4)==0?inner:inner^((inner^request)&mask);
 if(actual!=expected)throw new Exception("Restart state masked transport mismatch.");
}
Console.WriteLine($"PASS{tests} independent case state/category/flags records/{emissions} ordered emissions/{unsupported} exact missing-form refusals +1024 restart transports.");
if(args.Length==3)return;
byte[] ak=File.ReadAllBytes(args[3]),lk=File.ReadAllBytes(args[4]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[5]));
var prepared=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication),publication);
var nativePolicy=LegacyNativeCasePathPolicy.Create(model.LanguageModel.CasePathVersion);if(nativePolicy.Version!=0x200)throw new Exception("Actual RU TCM ownership changed.");
var shared=new LegacySymbolContextFactory();ushort[] exact=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);ulong[] initial=shared.CreateInitialSourceMap(exact);
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
bool dutch=prefix.Identifier.Contains("nl_",StringComparison.Ordinal);if(dutch)throw new Exception("Russian resource language metadata changed.");
LegacyPreparedDictionaryQuerySource Source()=>new(model.Lexicon,new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,exact,[]),initial);
var graph=OrdinaryGraph();var expand=new LegacyNativeCaseRecordExpansion(prepared,nativePolicy,shared);
if(model.LexicalAlphabet.HasStoredRelationMaps || !caseData.HasCaseAlternatives(model.LexicalAlphabet,publication))
 throw new Exception("Actual lexical fallback predicate was not exercised.");
var noCaseSource=VirtualLexicalCopy();
if(caseData.HasCaseAlternatives(noCaseSource,publication))throw new Exception("Virtual-only source retained a case predicate.");
try
{
 LegacyNativeCaseDictionaryFactory.Bind292(prefix,uint.MaxValue,model.LanguageModel,noCaseSource,caseData,publication,prepared,shared,Source(),graph,true);
 throw new Exception("Acoustic stored maps incorrectly selected wrapper for a case-free lexical source.");
}
catch(NotSupportedException){}
//A nonroot suffix rewrites every cased ancestor after the fixed start parent.
ushort Key(string text)=>model.AcousticAlphabet.TryGetLabel(text,out ushort symbol)?symbol:throw new Exception("Owned case fixture label absent.");
ulong Child(ulong parent,ushort symbol)
{var c=shared.ResolveChild(parent,symbol);if(c.Created)shared.LinkCreatedContext(c.Identity,parent);return c.Identity;}
ulong parentA=shared.ResolveInitial(Key("a")).Identity,childB=Child(parentA,Key("b")),leafC=Child(childB,Key("c"));
byte[] suffixRow=new byte[32];System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(suffixRow.AsSpan(20),Key("c"));System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(suffixRow.AsSpan(8),leafC);
var rewritten=expand.Expand(suffixRow,0,shared.ContainerIdentity,parentA,false,out _).First(r=>r.CostDelta!=0);
ulong rewrittenLeaf=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(rewritten.Record.Span[8..]);var last=shared.ReadNode(rewrittenLeaf);var middle=shared.ReadNode(last.ParentLink);
if(!prepared.TryReadForms(Key("b"),true,out var bForms)||!prepared.TryReadForms(Key("c"),true,out var cForms)||last.Symbol!=cForms.Second||middle.Symbol!=bForms.Second||middle.ParentLink!=parentA)throw new Exception("Owned nonroot suffix rewrite lost prefix/ancestor mapping.");
//Native Dutch first i chooses column14; state27 before a non-j consumes the
//same original form table but moves the current policy to state8.
byte[] dutchRow=suffixRow.ToArray();dutchRow[5]=1;System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(dutchRow.AsSpan(20),Key("i"));
var dutchVariants=expand.Expand(dutchRow,0,0,0,true,out _);if(!dutchVariants.Any(r=>(r.Record.Span[7]>>2&31)==27))throw new Exception("Native Dutch i state27 was not emitted.");
expand.Expand(suffixRow,27,0,0,true,out int moved);if(moved!=8)throw new Exception("Native Dutch state27 non-j transition lost.");
using var output=File.Create(args[6]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();int realPages=0,realRows=0;
var rawPage=Source().Read(new(){Node=0,Capacity=512,ContextRoot=shared.ContainerIdentity},false);
if(rawPage.ReturnedCount!=132)throw new Exception("Actual prepared root source extent changed.");
w.WriteString("source",Convert.ToHexString(rawPage.Records.Span));w.WriteStartArray("states");
foreach(int state in Enumerable.Range(0,nativePolicy.StateCount))
{
 var whole=ReadAll(512,state);var paged=ReadAll(5,state);
 if(!whole.AsSpan().SequenceEqual(paged))throw new Exception("Real TCM stream changed with page capacity5 vs512.");
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteString("records",Convert.ToHexString(whole));w.WriteEndObject();realRows+=whole.Length/32;
}
w.WriteEndArray();w.WriteStartArray("contexts");foreach(var c in shared.CopyNodes())
{w.WriteStartArray();w.WriteNumberValue(c.Identity);w.WriteNumberValue(c.ParentLink);w.WriteNumberValue(c.Symbol);w.WriteEndArray();}
w.WriteEndArray();
var metadata=new LegacyNativeCaseWordMetadata(new(model.Lexicon,model.WordIndex),expand);
byte[] terminal=Enumerable.Range(0,132).Select(i=>rawPage.Records.Slice(i*32,32).ToArray()).First(r=>(r[5]&2)!=0);
uint terminalNode=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(terminal);
w.WriteStartArray("metadata");
foreach(int state in Enumerable.Range(0,32))foreach(bool ignored in new[]{false,true})foreach(bool start in new[]{false,true})
foreach(string text in new[]{"a",",",".","-"})
{
 if(!model.AcousticAlphabet.TryGetLabel(text,out ushort label))throw new Exception("Metadata fixture symbol absent.");
 ulong tag=terminalNode|((ulong)state<<58)|(start?1UL<<40:0);var bytes=metadata.ReadTerminal(tag,label,7,ignored);
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("label",label);w.WriteBoolean("ignored",ignored);w.WriteBoolean("start",start);w.WriteString("bytes",Convert.ToHexString(bytes.Span));w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS actual Russian TCM200 dictionary wrapper,28 states/{realRows} case-emitted records/{realPages} pages; capacity5/512 stream invariant +512 terminal category records. Prepared dictionary maps; not controller/winner proof.");
byte[] ReadAll(int capacity,int state)
{
 var wrapper=LegacyNativeCaseDictionaryFactory.Bind292(prefix,uint.MaxValue,model.LanguageModel,model.LexicalAlphabet,caseData,publication,prepared,shared,Source(),graph,true);
 if(!wrapper.ReadAuxiliary24((ulong)state<<58).IsEmpty)throw new Exception("Actual2B7 emptyV58 continuation changed.");
 var request=new LegacyPreparedDictionaryQuery{Node=0,Capacity=capacity,ContextRoot=shared.ContainerIdentity};
 var query=new LegacyNativeCaseNormalQuery{Request=request,SourceTag=(ulong)state<<58};var records=new List<byte>();
 int count=0;
 while(true)
 {
  var page=wrapper.Read(query);realPages++;if(page.ReturnedCount==0)break;
  if(page.ReturnedCount<0||count++>4096||page.ExtraCosts.Length!=page.ReturnedCount*12)throw new Exception("Unexpected bounded wrapper page extent.");
  for(int i=0;i<page.ReturnedCount;i++)
  {
   var row=page.Records.Span.Slice(i*32,32);int cost=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
   int side=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(page.ExtraCosts.Span.Slice(i*12+4,4));if(cost!=side)throw new Exception("Real zero backend side-cost delta differs from record cost.");
  }
  records.AddRange(page.Records.ToArray());request.Skip+=page.ReturnedCount;
 }
 return records.ToArray();
}
LegacyInkAlphabet VirtualLexicalCopy()
{
 byte[] payload=lk.AsSpan(606964+prefix.AlphabetPayloadOffset,model.LexicalAlphabet.BytesConsumed).ToArray();
 int cursor=16;
 ushort U16(){ushort value=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(cursor));cursor+=2;return value;}
 int n=U16();
 foreach(int increment in new[]{32,64,1})
 {
  int last=-1;for(int i=0;i<n;i++){ushort v=U16();if(v!=65535)last=v;}
  if(last<0)throw new Exception("Missing native lookup directory.");n=last+increment;
 }
 int span=0;for(int i=0;i<=n;i++)span=U16();int entries=span+1;
 int stringBytes=checked((int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(cursor+(entries-1)*8)));
 cursor=checked(cursor+entries*8+stringBytes);
 payload.AsSpan(cursor,model.LexicalAlphabet.LabelCount*2).Fill(255);
 return LegacyInkAlphabet.Load(payload,0x103);
}
static LegacyOrdinaryCursiveCandidateGraph OrdinaryGraph()
{
 var ink=LegacyCursivePreparedInk.Create([new InkStroke([new InkPoint(0,0),new InkPoint(10,80)])]);var markers=new[]{new LegacyCursiveCutMarker(0,0,0)};
 var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),[0],[-1]);
 return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),1);
}
