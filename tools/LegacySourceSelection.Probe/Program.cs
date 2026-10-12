using System.Text.Json;
using InkRecognition;
if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int traces=0,cases=0;
foreach(var trace in controls.RootElement.EnumerateArray())
{
 var builder=new LegacyPreparedDictionarySourceSelectionBuilder();var originals=new List<LegacyPreparedDictionarySourceSelectionInput>();
 foreach(var op in trace.EnumerateArray())
 {
  if(op.TryGetProperty("remove",out var remove)){int index=remove.GetInt32();Require(ReferenceEquals(builder.Remove(index),originals[index]),"Removal changed original source identity.");originals.RemoveAt(index);}
  else
  {
   var s=op.GetProperty("add");var d=new LegacyPreparedDictionarySourceDescriptor(s.GetProperty("flags").GetUInt32(),s.GetProperty("weight").GetSingle(),s.GetProperty("other").GetSingle(),1,s.GetProperty("order").GetUInt32(),"own prepared source");
   uint? count=s.GetProperty("count").ValueKind==JsonValueKind.Null?null:s.GetProperty("count").GetUInt32();
   var input=new LegacyPreparedDictionarySourceSelectionInput(d,count,s.GetProperty("owner").GetUInt64());Require(builder.Add(input)==originals.Count,"Added source index differs.");originals.Add(input);
  }
  var inputWeightBits=originals.Select(x=>BitConverter.SingleToUInt32Bits(x.Descriptor.Weight24)).ToArray();
  var result=builder.Prepare();var expected=op.GetProperty("expected");Require(result.Count==expected.GetArrayLength()&&result.Count==builder.SourceCount,"Selected count differs.");
  for(int i=0;i<result.Count;i++)
  {
   var row=result[i];var e=expected[i];Require(row.SourceIndex==e.GetProperty("index").GetInt32()&&row.Priority==e.GetProperty("priority").GetUInt32()&&row.AcceptedSkipBase==uint.MaxValue&&row.ContextOwnerIdentity==e.GetProperty("owner").GetUInt64()&&row.Descriptor.Flags==e.GetProperty("flags").GetUInt32()&&row.Descriptor.LanguageOrder==e.GetProperty("order").GetUInt32()&&BitConverter.SingleToUInt32Bits(row.Descriptor.Weight24)==e.GetProperty("weightBits").GetUInt32()&&row.Descriptor.Weight28==e.GetProperty("other").GetSingle(),$"Native source priority/order/weight record differs in trace{traces},case{cases}.");
   Require(BitConverter.SingleToUInt32Bits(originals[row.SourceIndex].Descriptor.Weight24)==inputWeightBits[row.SourceIndex],"Input descriptor mutated.");
  }
  cases++;
 }
 traces++;
}
var aliases=new LegacyPreparedDictionarySourceSelectionBuilder();var d0=new LegacyPreparedDictionarySourceDescriptor(0xc00,1,1,1,0,"");var input0=new LegacyPreparedDictionarySourceSelectionInput(d0,null,0);aliases.Add(input0);Reject(()=>aliases.Add(input0));
Reject(()=>aliases.Add(new(d0 with{Flags=0xc08},null,0)));Reject(()=>aliases.Add(new(d0 with{Weight24=float.NaN},null,0)));Reject(()=>aliases.Add(new(d0 with{Weight24=32769},null,0)));Reject(()=>aliases.Add(new(d0 with{RuntimeLanguageScale=0},null,0)));Reject(()=>aliases.Remove(1));Reject(()=>aliases.Prepare(new CancellationToken(true)));
var limit=new LegacyPreparedDictionarySourceSelectionBuilder();for(int i=0;i<255;i++)limit.Add(new(d0 with{},null,(ulong)i));Reject(()=>limit.Add(new(d0 with{},null,0)));Require(limit.Prepare().Select(s=>s.SourceIndex).SequenceEqual(Enumerable.Range(0,255).Reverse()),"Unsigned equal-priority source-index tie order differs.");
var one=new LegacyPreparedDictionarySourceSelectionBuilder();one.Add(new(d0 with{Weight24=1},null,0));Require(one.Prepare()[0].Descriptor.Weight24==1,"Strict sum>1 normalization changed exact unity.");
Console.WriteLine($"PASS {traces} independent source traces / {cases} add-remove-prepare snapshots; unsigned priority/index order, binary32 normalization, skip reset, metadata/owner retention,255 sources, aliased-owner and malformed/canceled guards.");
if(args.Length==1)return;
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var contexts=new LegacySymbolContextFactory();var map=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);var initial=contexts.CreateInitialSourceMap(map);
var actual=new LegacyPreparedDictionarySourceSelectionBuilder();actual.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor,model.LanguageModel,contexts.ContainerIdentity));
var row0=actual.Prepare().Single();Require(row0.SourceIndex==0&&row0.Priority==1024&&row0.Descriptor.Flags==0xc0a&&row0.Descriptor.Weight24==0&&row0.Descriptor.Weight28==1&&row0.ContextOwnerIdentity==contexts.ContainerIdentity&&initial.All(x=>x!=0),"Observed RU source count/weight/context facts differ.");
actual.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor with{},model.LanguageModel,contexts.ContainerIdentity));Require(actual.Prepare().Select(s=>s.SourceIndex).SequenceEqual(new[]{1,0}),"Two distinct prepared RU instances lost native index tie order.");actual.Remove(0);Require(actual.Prepare().Single().SourceIndex==0&&actual.Prepare().Single().Priority==1024,"Native source removal did not reindex remaining instance.");
Console.WriteLine("PASS actual RU prepared source descriptorC0A, stored first n-gram count/ClassCount1024 (distinct from order2/WordCount439826), weight0/1 and real shared context owner -> native selected-source rows; two distinct-owner instances/removal identity. Child factory/preparation and aggregate scheduling remain separate.");
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Malformed source selection accepted.");}
