using InkRecognition;
using System.Text.Json;
using static InkRecognition.LegacyEnglishCaseInitializationNumericTables;
if(args.Length!=2||File.Exists(args[1]))throw new ArgumentException("MODELS FRESH_OUTPUT");
var source=LegacySource247WireLayout.Read(File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.lite.res")));
var raw=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-lk-text.lite.res"));
var language=LegacyEnglishLkSparseLanguageData.Read(LegacyEnglishLkPackedData.Read(source,raw),raw);
var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var tables=LegacyEnglishCaseInitializationNumericTables.Prepare(numeric,default);
var holder=TaggedWordTables.Prepare(tables);
if(!holder.IsFor(tables))throw new Exception("Same numeric owner lost.");
using var stream=File.Create(args[1]);using var json=new Utf8JsonWriter(stream);
json.WriteStartObject();json.WriteString("scope","PURE tagged known-word numeric records; no graph/source authority/NN");json.WriteNumber("actualNetworkForwards",0);json.WriteStartArray("cases");
int count=0;var matched=new int[4];
foreach(int word in new[]{0,17,260073})foreach(int category in Enumerable.Range(0,13))foreach(int flags in Enumerable.Range(0,8))foreach(int length in Enumerable.Range(0,3))
foreach(int firstTag in Enumerable.Range(0,length>0?13:1))foreach(int secondTag in Enumerable.Range(0,length>1?13:1))
{
 uint first=(uint)((firstTag*73+flags*17)%1024)|(uint)firstTag<<24|0xa0000000;
 uint second=(uint)((secondTag*91+category)%1024)|(uint)secondTag<<24|0x50000000;
 var input=new TaggedWordInput(word,category,length,first,second,(flags&1)!=0,(flags&2)!=0,(flags&4)!=0,0x4000000);
 var result=holder.EvaluateTaggedCore(input)??throw new Exception("Bounded genuine resource cost rejected.");matched[result.MatchedOrder]++;
 var before=Enumerable.Range(0,40).Select(i=>(byte)(i*37+count*11)).ToArray();var after=before.ToArray();result.WriteRecord(after);
 json.WriteStartObject();json.WritePropertyName("input");JsonSerializer.Serialize(json,input);
 json.WriteNumber("cost",result.Cost);json.WriteNumber("matched",result.MatchedOrder);json.WriteNumber("count",result.HistoryCount);json.WriteNumber("mask",result.WrittenByteMask);
 json.WriteString("before",Convert.ToHexString(before));json.WriteString("after",Convert.ToHexString(after));json.WriteEndObject();count++;
}
json.WriteEndArray();json.WriteNumber("caseCount",count);json.WritePropertyName("matchedCounts");JsonSerializer.Serialize(json,matched);
int guards=0;var valid=new TaggedWordInput(17,3,2,1u<<24|19,2u<<24|20,false,false,false,0x4000000);
void Refuse(TaggedWordInput x){try{holder.EvaluateTaggedCore(x);}catch(ArgumentException){guards++;return;}throw new Exception("Malformed input admitted.");}
Refuse(valid with{Word=-1});Refuse(valid with{Word=260074});Refuse(valid with{Category=13});Refuse(valid with{HistoryCount=3});Refuse(valid with{History0=1024});Refuse(valid with{History1=13u<<24});
var accepted=holder.EvaluateTaggedCore(valid)!.Value;
if(holder.EvaluateTaggedCore(valid with{LexicalLimit=accepted.Cost-1}) is not null||holder.EvaluateTaggedCore(valid with{LexicalLimit=accepted.Cost}) is null)throw new Exception("Exact lexical budget boundary lost.");guards+=2;
using var cancelled=new CancellationTokenSource();cancelled.Cancel();try{holder.EvaluateTaggedCore(valid,cancelled.Token);throw new Exception("Cancel ignored.");}catch(OperationCanceledException){guards++;}
json.WriteNumber("guards",guards);json.WriteEndObject();json.Flush();Console.WriteLine($"PASS {count} numeric cases/{guards} guards; no NN");
