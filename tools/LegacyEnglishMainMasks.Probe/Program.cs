using System.Text.Json;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-raw capture");
var m=args[0];var bytes=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var packed=LegacyEnglishLkPackedData.Read(layout,lk);var language=LegacyEnglishLkSparseLanguageData.Read(packed,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var foreign=LegacySource247WireLayout.Read(bytes);var foreignNumeric=LegacyEnglishLkFreshNumericBinding.Create(LegacyEnglishLkSparseLanguageData.Read(LegacyEnglishLkPackedData.Read(foreign,lk),lk));
var m30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0,cancels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
LegacyEnglish119ParentPostprocessing Post(JsonElement item)
{var p=LegacyEnglish119ParentSubmission.Prepare(model,Ink(item.GetProperty("Raw")),item.GetProperty("Resolution").GetSingle(),m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication).Run();}
string SourceDigest(LegacyEnglish119FinalizedSingleSource f)
{using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);foreach(var r in f.CopyRows()){hash.AppendData(r.CopyStorage().Span);foreach(var x in r.Ranges)foreach(int v in new[]{x.First.First,x.First.Second,x.First.Third,x.Last.First,x.Last.Second,x.Last.Third})hash.AppendData(BitConverter.GetBytes(v));}return Convert.ToHexString(hash.GetHashAndReset());}
int[] ContextIdentity(ulong[] ids){var seen=new Dictionary<ulong,int>();return ids.Select(id=>{if(id==0)return 0;if(!seen.TryGetValue(id,out int v))seen[id]=v=seen.Count+1;return v;}).ToArray();}
object Capture(LegacyEnglishMainOnlySourceMasks s)=>D(("Scope",s.Policy),("DescriptorFlags",s.DescriptorFlags),("LanguageOrder",s.LanguageOrder),("SelectedPriority",s.SelectedPriority),("SelectedSourceIndex",s.SelectedSourceIndex),("ContextCount",s.ContextCount),("AcousticSymbolCount",s.AcousticSymbolCount),("NeuralClassCount",s.NeuralClassCount),("CacheWidth",s.CacheWidth),("HasContextualForms",s.HasContextualForms),("WidePairCount",s.WidePairCount),("LogicalCopyWorkBytes",s.LogicalCopyWorkBytes),("PrimaryMap",s.CopyPrimaryMap()),("AuxiliaryMap",s.CopyAuxiliaryMap()),("ContextMap",ContextIdentity(s.CopyInitialContextMap())),("Seed",s.CopyControllerSeed().Select(x=>(int)x).ToArray()),("Primary",s.CopyPrimaryMask().Select(x=>(int)x).ToArray()),("Secondary",s.CopySecondaryMask().Select(x=>(int)x).ToArray()),("Groups",s.Knowledge.CopyGroups().Select(g=>D(("Weight",g.Weight),("Symbols",g.Symbols.ToArray()))).ToArray()),("RelatedCaseRelaxationEnabled",s.Knowledge.RelatedCaseRelaxationEnabled),("FirstRelatedCaseMultiplier",s.Knowledge.FirstRelatedCaseMultiplier),("SecondRelatedCaseMultiplier",s.Knowledge.SecondRelatedCaseMultiplier),("SizeCostsEnabled",s.Knowledge.SizeCostsEnabled),("DotSizeDivisor",s.Knowledge.DotSizeDivisor),("PunctuationSizeDivisor",s.Knowledge.PunctuationSizeDivisor),("CachePower",s.Knowledge.CachePower));
bool first=true;
foreach(var item in inputs.RootElement.EnumerateArray())
{
 var post=Post(item);string name=item.GetProperty("Name").GetString()!;
 if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post));refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table);
 Guard(()=>LegacyEnglish119MainKnowledge.Create(local));local.Run();string before=SourceDigest(final);
 Guard(()=>LegacyEnglish119MainKnowledge.Create(local,new(true)));
 var knowledge=LegacyEnglish119MainKnowledge.Create(local);
 if(!ReferenceEquals(knowledge.MainWire,final.MainWire)||!ReferenceEquals(knowledge.Source,layout))throw new Exception("Main ownership lost.");
 Guard(()=>LegacyEnglishMainOnlySourceMasks.Create(knowledge,foreignNumeric));Guard(()=>LegacyEnglishMainOnlySourceMasks.Create(knowledge,numeric,new(true)));
 var masks=LegacyEnglishMainOnlySourceMasks.Create(knowledge,numeric);string stable=Encode(Capture(masks));
 if(!ReferenceEquals(masks.Source,numeric.DataOwner.Source)||!ReferenceEquals(masks.Knowledge.Owner,local)||masks.CacheWidth!=220||masks.NeuralClassCount!=182)throw new Exception("Actual source/channel lineage lost.");
 var repeated=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);if(stable!=Encode(Capture(repeated)))throw new Exception("Repeated completed reads changed output.");
 var leaked=masks.CopyPrimaryMask();leaked[0]^=1;var leakedMap=masks.CopyPrimaryMap();leakedMap[0]^=1;var leakedContexts=masks.CopyInitialContextMap();leakedContexts[0]^=1;
 var leakedGroups=knowledge.CopyGroups();if(!MemoryMarshal.TryGetArray(leakedGroups[0].Symbols,out ArraySegment<ushort> leakedLabels))throw new Exception("Mutation control missing.");leakedLabels.Array![leakedLabels.Offset]^=1;
 if(stable!=Encode(Capture(masks))||before!=SourceDigest(final))throw new Exception("Snapshot alias or source720 mutation.");
 if(first)
 {
  foreach(string stage in new[]{"knowledge:preflight","knowledge:complete"}){using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglish119MainKnowledge.CreatePrepared(local,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;if(stable!=Encode(Capture(masks)))throw new Exception("Canceled knowledge mutated old receipt.");}
  for(int boundary=1;boundary<=8;boundary++){using var cts=new CancellationTokenSource();int seen=0;Guard(()=>LegacyEnglish119MainKnowledge.CreatePrepared(local,s=>{if(s=="knowledge:group-boundary"&&++seen==boundary)cts.Cancel();},cts.Token));cancels++;if(stable!=Encode(Capture(masks)))throw new Exception("Canceled boundary mutated old receipt.");}
  foreach(string stage in new[]{"masks:preflight","masks:source-maps","masks:selected-main","masks:enumerated","masks:produced","masks:complete"}){using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishMainOnlySourceMasks.CreatePrepared(knowledge,numeric,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;if(stable!=Encode(Capture(masks)))throw new Exception("Canceled masks mutated old receipt.");}
  foreach(int bad in new[]{-1,0,257,int.MaxValue})Guard(()=>LegacyEnglish119MainKnowledge.Preflight(bad,7,25,4,104,217));
  Guard(()=>LegacyEnglish119MainKnowledge.Preflight(256,8,25,4,104,217));Guard(()=>LegacyEnglish119MainKnowledge.Preflight(256,7,26,4,104,217));Guard(()=>LegacyEnglish119MainKnowledge.Preflight(256,7,25,5,104,217));Guard(()=>LegacyEnglish119MainKnowledge.Preflight(256,7,25,4,105,217));Guard(()=>LegacyEnglish119MainKnowledge.Preflight(256,7,25,4,104,218));
  var raw=layout.CopyChildEnvelopeBytes(final.MainWire.Acoustic);Guard(()=>LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(raw,217,out _));
  first=false;
 }
 rows.Add(D(("Name",name),("Source720Before",before),("Source720After",SourceDigest(final)),("Masks",Capture(masks))));
}
if(rows.Count!=134||refused.Count!=7||cancels!=16)throw new Exception("Actual owner denominators changed.");
File.WriteAllText(args[2],Encode(D(("Scope","Actual completed EN main119 knowledge +explicit independent main13B-only masks; no mainNN/winner/fullroot"),("Rows",rows),("MultilineRefused",refused),("Guards",guards),("Cancellations",cancels),("MaximumLogicalCopyBytes",LegacyEnglish119MainKnowledge.Preflight(256,7,25,4,104,217)))));Console.WriteLine($"PASS actual134/7 masks guards{guards} cancel{cancels}");

static string Encode(object value){using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,value);return System.Text.Encoding.UTF8.GetString(stream.ToArray());}
static Dictionary<string,object?> D(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
static void Write(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
 case null:w.WriteNullValue();break;
 case string s:w.WriteStringValue(s);break;
 case bool b:w.WriteBooleanValue(b);break;
 case int n:w.WriteNumberValue(n);break;
 case uint n:w.WriteNumberValue(n);break;
 case long n:w.WriteNumberValue(n);break;
 case ulong n:w.WriteNumberValue(n);break;
 case byte n:w.WriteNumberValue(n);break;
 case short n:w.WriteNumberValue(n);break;
 case ushort n:w.WriteNumberValue(n);break;
 case float n:w.WriteNumberValue(n);break;
 case double n:w.WriteNumberValue(n);break;
 case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}



