using InkRecognition;
using System.Text.Json;
if(args.Length!=6)throw new ArgumentException("AK LK engine-profile case-data publication-data capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[2]));
using var c=File.OpenRead(args[3]);var caseData=LegacyNativeCaseData.Load(c);
using var p=File.OpenRead(args[4]);var publication=LegacyNativeTextPublicationData.Load(p);
var ranges=LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication);
var prep=LegacyNativeCasePreparation.Prepare(ranges,publication);
var contexts=new LegacySymbolContextFactory();var symbols=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[]);
if(!ReferenceEquals(symbols.AcousticAlphabet,model.AcousticAlphabet)||!ReferenceEquals(symbols.LexicalAlphabet,model.LexicalAlphabet))throw new Exception("Map alphabet ownership differs.");
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));
var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,2,true);
var binding=new LegacyPreparedCaseDictionarySymbolBinding(symbols,prep,model.LanguageModel,descriptor.Flags);
var source=binding.CreateSymbolSource();if(!source.DictionaryMap.IsEmpty)throw new Exception("Mapped callbacks must not be mapped twice.");
var seed=LegacyLinguisticSymbolMaskController.Build(model.AcousticAlphabet.LabelCount,[source],true);
if(seed.Outcome!=LegacySymbolMaskControllerOutcome.Succeeded)throw new Exception("Prepared source enumeration failed.");
byte[] header=model.AcousticAlphabet.CopySerializedHeader();if(!header.AsSpan().SequenceEqual(ak.AsSpan(99,16)))throw new Exception("Serialized header snapshot differs.");header[0]^=255;
if(!model.AcousticAlphabet.CopySerializedHeader().AsSpan().SequenceEqual(ak.AsSpan(99,16)))throw new Exception("Header snapshot aliased owner.");
try{new LegacyPreparedCaseDictionarySymbolBinding(symbols,prep,model.LanguageModel,descriptor.Flags,new(true));throw new Exception("Canceled accepted.");}catch(OperationCanceledException){}
var foreign=LegacyInkAlphabet.Load(ak.AsSpan(99,model.AcousticAlphabet.BytesConsumed),0x103);
var foreignPrep=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(foreign,caseData,publication),publication);
try{new LegacyPreparedCaseDictionarySymbolBinding(symbols,foreignPrep,model.LanguageModel,descriptor.Flags);throw new Exception("Foreign equal alphabet accepted.");}catch(ArgumentException){}
using var output=File.Create(args[5]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();WriteU16("map",symbols.CopyPrimary());w.WriteStartArray("callbacks");foreach(uint t in source.Tokens.Span)w.WriteNumberValue(t);w.WriteEndArray();w.WriteBase64String("seed",seed.Mask.Span);
var random=new Random(0x234840);int refreshes=0;
w.WriteStartArray("refresh");
foreach(uint flags in new[]{descriptor.Flags,0x800u})
{
 var owner=new LegacyPreparedCaseDictionarySymbolBinding(symbols,prep,model.LanguageModel,flags);
 for(int at=0;at<model.AcousticAlphabet.LabelCount+16;at++)
 {
  byte[] values=new byte[model.AcousticAlphabet.LabelCount];
  if(at<values.Length)values[at]=(byte)(at%2==0?1:5);else random.NextBytes(values);
  var result=owner.Refresh(values);var again=owner.Refresh(new byte[values.Length]);
  var zero=symbols.CreateWorkingMap(flags).Refresh(new byte[values.Length]);
  if(!again.CopySelectedMap(false,0).AsSpan().SequenceEqual(zero.CopySelectedMap(false,0)))throw new Exception("Refresh retained previous filter.");
  w.WriteStartObject();w.WriteNumber("flags",flags);w.WriteBase64String("input",values);WriteU16("working",result.CopySelectedMap(false,0));WriteU16("preserved",result.CopySelectedMap(false,0x40));w.WriteEndObject();refreshes++;
 }
}
w.WriteEndArray();int produced=0;w.WriteStartArray("produced");
for(int test=0;test<24;test++)
{
 var groups=new List<LegacyPreparedAcousticPermissionGroup>();
 for(int row=0;row<3;row++)
 {
  int[] costs=new int[model.AcousticAlphabet.LabelCount];for(int i=0;i<costs.Length;i++)costs[i]=random.Next(4)==0?0x4000000:random.Next(60001);
  groups.Add(new(costs,row%2==0?1f:.5f,true,(byte)(row%3),true,(byte)(test%8),!(test%7==0&&row==1)));
 }
 var result=binding.BuildAndRefresh(groups);w.WriteStartObject();w.WriteStartArray("groups");
 foreach(var g in groups){w.WriteStartObject();w.WriteStartArray("costs");foreach(int v in g.FinalCosts.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteNumber("scale",g.CostScale);w.WriteBoolean("recomputed",g.Recomputed);w.WriteNumber("kind",g.GraphKind);w.WriteBoolean("present",g.GraphRecordPresent);w.WriteNumber("flags",g.GraphRecordFlags);w.WriteBoolean("ok",g.Succeeded);w.WriteEndObject();}
 w.WriteEndArray();w.WriteBoolean("ok",result.Succeeded);w.WriteBase64String("permissions",result.Permissions.Span);WriteU16("working",result.WorkingMap?.CopySelectedMap(false,0)??[]);w.WriteEndObject();produced++;
}
w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS owned lexical→case callback source/{source.Tokens.Length} callbacks, {refreshes} ordered permission→working maps and {produced} acoustic permission paths; equal-looking foreign AL and cancellation guards. Live source factory remains separate.");
void WriteU16(string key,ushort[] values){w.WriteStartArray(key);foreach(ushort value in values)w.WriteNumberValue(value);w.WriteEndArray();}
