using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("models-directory capture.json");
byte[] ak=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.lite.res")),raw=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-lk-text.lite.res"));
var acoustic=LegacySource247WireLayout.Read(ak);var owner=LegacyEnglishLkPackedData.Read(acoustic,raw);int guards=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected bounded refusal.");}
Guard(()=>LegacyEnglishLkSparseLanguageData.Read(null!,raw));Guard(()=>LegacyEnglishLkSparseLanguageData.Read(owner,raw.AsSpan(0,raw.Length-1)));Guard(()=>LegacyEnglishLkSparseLanguageData.Read(owner,raw,new(true)));
var language=LegacyEnglishLkSparseLanguageData.Read(owner,raw);var binding=LegacyEnglishLkFreshNumericBinding.Create(language);
if(!ReferenceEquals(language.DataOwner,owner)||!ReferenceEquals(binding.DataOwner,owner)||!ReferenceEquals(binding.LanguageOwner,language)||!ReferenceEquals(owner.Source,acoustic))throw new Exception("Actual selected owner identity lost.");
if(language.RetainedLanguageWireBytes!=1580304||language.ReservedWordIndex!=260074||language.StoredDefault268C!=unchecked((int)0xfc000000)||binding.Context00!=0x4000000||binding.Profile!=2||binding.SourceScale!=1||binding.LiteralPacketByte0!=0)throw new Exception("Actual default/profile lineage lost.");
string Bytes(ReadOnlySpan<byte> bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
string Ints(int[] values){byte[] bytes=new byte[checked(values.Length*4)];for(int i=0;i<values.Length;i++)BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i*4),values[i]);return Bytes(bytes);}
var fields=language.CopyFields();var packed=new List<(LegacyEnglishSparsePackedField Field,int Count,int Width,string Sha)>();
foreach(var field in Enum.GetValues<LegacyEnglishSparsePackedField>())
{
 int[] values=language.CopyPackedValues(field);for(int i=0;i<values.Length;i++)if(language.GetPackedValue(field,i)!=values[i])throw new Exception("Packed scalar/snapshot mismatch.");packed.Add((field,values.Length,language.GetPackedWidth(field),Ints(values)));int old=language.GetPackedValue(field,0);values[0]^=1;if(language.GetPackedValue(field,0)!=old)throw new Exception("Packed snapshot aliases owner.");
 Guard(()=>language.GetPackedValue(field,-1));Guard(()=>language.GetPackedValue(field,language.GetPackedCount(field)));Guard(()=>language.CopyPackedValues(field,new(true)));
}
foreach(var field in fields)
{
 var copy=language.CopyFieldPayload(field.Name);if(Bytes(copy)!=field.Sha256)throw new Exception("Field copy/hash mismatch.");if(copy.Length!=field.End-field.Offset)throw new Exception("Field boundary mismatch.");if(copy.Length!=0){copy[0]^=1;if(Bytes(language.CopyFieldPayload(field.Name))!=field.Sha256)throw new Exception("Field snapshot aliases owner.");}
}
fields[0]=default;if(language.CopyFields()[0].Name!="2CD.version")throw new Exception("Field metadata aliases owner.");fields=language.CopyFields();
Guard(()=>language.GetPackedCount((LegacyEnglishSparsePackedField)(-1)));Guard(()=>language.CopyFieldPayload("foreign-profile"));Guard(()=>language.CopyFieldPayload(null!));Guard(()=>LegacyEnglishLkFreshNumericBinding.Create(null!));Guard(()=>LegacyEnglishLkFreshNumericBinding.Create(language,new(true)));Guard(()=>binding.CopyClassCosts(-1));Guard(()=>binding.CopyClassCosts(3));Guard(()=>binding.CopyBackoffCosts(-1));Guard(()=>binding.CopyBackoffCosts(2));
for(int i=0;i<=13;i++)for(int j=0;j<=13;j++)if(!(i==0&&j==0||i>=1&&i<=12&&j>=1&&j<=12))Guard(()=>binding.GetCaseCost(i,j));
foreach(var a in new[]{(-1,1,1),(13,1,1),(1,0,1),(1,1,0),(1,13,1),(1,1,13),(0,1,0),(0,0,1)})Guard(()=>binding.GetCaseCost(a.Item1,a.Item2,a.Item3));
if(binding.GetCaseCost(0,0)!=0||binding.GetCaseCost(0,0,0)!=0)throw new Exception("Explicit case zero lost.");
int[] words=binding.CopyWordCosts();string wordHash=Ints(words);words[0]^=1;if(Ints(binding.CopyWordCosts())!=wordHash)throw new Exception("Word costs alias snapshot.");
string[] classHash=new string[3],backHash=new string[2];for(int i=0;i<3;i++){var copy=binding.CopyClassCosts(i);classHash[i]=Ints(copy);copy[0]^=1;if(Ints(binding.CopyClassCosts(i))!=classHash[i])throw new Exception("Class costs alias snapshot.");if(i<2){copy=binding.CopyBackoffCosts(i);backHash[i]=Ints(copy);copy[0]^=1;if(Ints(binding.CopyBackoffCosts(i))!=backHash[i])throw new Exception("Backoff aliases snapshot.");}}
raw[1252428]^=1;ak[0]^=1;if(Ints(language.CopyPackedValues(LegacyEnglishSparsePackedField.WordClasses))!=packed[0].Sha||Ints(binding.CopyWordCosts())!=wordHash)throw new Exception("Borrowed raw buffers mutated owner.");raw[1252428]^=1;ak[0]^=1;
var faults=new List<(string Stage,int Calls)>();
foreach(string stage in new[]{"sha:input","copy:resource","sha:snapshot","copy:language","validate:packed","validate:wordClasses","validate:rows","publish"})
{
 using var cts=new CancellationTokenSource();int calls=0;Guard(()=>LegacyEnglishLkSparseLanguageData.ReadPrepared(owner,raw,s=>{if(s==stage){calls++;cts.Cancel();}},true,cts.Token));if(calls!=1)throw new Exception("Reader cancellation stage not reached.");faults.Add((stage,calls));
}
foreach(string stage in new[]{"sha:input:chunk","sha:snapshot:chunk","validate:wordClasses","validate:rows"})
{
 using var cts=new CancellationTokenSource();int calls=0;Guard(()=>LegacyEnglishLkSparseLanguageData.ReadPrepared(owner,raw,s=>{if(s==stage&&++calls==2)cts.Cancel();},true,cts.Token));if(calls!=2)throw new Exception("Mid-loop cancellation not reached.");faults.Add((stage+":second",calls));
}
foreach(string stage in new[]{"numeric:word","numeric:class","numeric:backoff","numeric:case","numeric:publish"})
{
 using var cts=new CancellationTokenSource();int calls=0;Guard(()=>LegacyEnglishLkFreshNumericBinding.CreatePrepared(language,s=>{if(s==stage){calls++;cts.Cancel();}},cts.Token));if(calls!=1)throw new Exception("Numeric cancellation stage not reached.");faults.Add((stage,calls));
}
using(var cts=new CancellationTokenSource()){int calls=0;Guard(()=>LegacyEnglishLkFreshNumericBinding.CreatePrepared(language,s=>{if(s=="numeric:case"&&++calls==6)cts.Cancel();},cts.Token));if(calls!=6)throw new Exception("Mid-case cancellation absent.");faults.Add(("numeric:case:sixth",calls));}
var tampered=new List<int>();foreach(int at in new[]{0,14,184,198,1252412,1252426,1252428,1252430,1252514,1252518,1252522,1252526,1252530,2832715})
{raw[at]^=1;try{Guard(()=>LegacyEnglishLkSparseLanguageData.Read(owner,raw));}finally{raw[at]^=1;}tampered.Add(at);}
//Explicit PREPARED controls bypass only resource SHA, never public acceptance.
int prepared=0;foreach(string name in new[]{"2CD.version","2CD.64,68,6C","26AC.maxOrder","wire.flags","70","74","78","88.version","88.count","88.width","88.metadata","A0","B0.1.version","B0.2.width","B8.0.count","B8.1.metadata","D0","E0","150.casePathVersion"})
{
 int at=fields.Single(f=>f.Name==name).Offset;raw[at]^=1;try{Guard(()=>LegacyEnglishLkSparseLanguageData.ReadPrepared(owner,raw,null,false));prepared++;}finally{raw[at]^=1;}
}
//Nonfinite coefficients refuse before publishing language; finite overflow
//refuses numeric construction. These do not become actual resource receipts.
int floatAt=fields.Single(f=>f.Name=="174").Offset+14*4;int saved=BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(floatAt));
try{BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(floatAt),0x7fc00000);Guard(()=>LegacyEnglishLkSparseLanguageData.ReadPrepared(owner,raw,null,false));prepared++;BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(floatAt),0x7f7fffff);var overflow=LegacyEnglishLkSparseLanguageData.ReadPrepared(owner,raw,null,false);Guard(()=>LegacyEnglishLkFreshNumericBinding.Create(overflow));prepared++;}finally{BinaryPrimitives.WriteInt32LittleEndian(raw.AsSpan(floatAt),saved);}
long envelope=LegacyEnglishSparseCopyBudget.Preflight();Guard(()=>LegacyEnglishSparseCopyBudget.Preflight(envelope-1));Guard(()=>LegacyEnglishSparseCopyBudget.Preflight(-1));Guard(()=>LegacyEnglishSparseCopyBudget.Snapshot(-1));Guard(()=>LegacyEnglishSparseCopyBudget.Snapshot(LegacyEnglishSparseCopyBudget.Maximum+1));if(LegacyEnglishSparseCopyBudget.Preflight(envelope)!=envelope)throw new Exception("Envelope equality changed.");
var retry=LegacyEnglishLkSparseLanguageData.Read(owner,raw);if(!ReferenceEquals(retry.DataOwner,owner)||Ints(LegacyEnglishLkFreshNumericBinding.Create(retry).CopyWordCosts())!=wordHash)throw new Exception("Failed load poisoned stateless retry.");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("Scope","Actual sparse English DATA +fresh numerical binding only; no query/masks/controller/inference");w.WriteNumber("RetainedLanguageWireBytes",language.RetainedLanguageWireBytes);w.WriteNumber("LogicalCopyBytes",envelope);w.WriteNumber("Guards",guards);w.WriteNumber("PreparedControls",prepared);w.WriteNumber("ReservedWordIndex",language.ReservedWordIndex);w.WriteNumber("StoredDefault268C",language.StoredDefault268C);
w.WritePropertyName("Fields");w.WriteStartArray();foreach(var f in fields){w.WriteStartObject();w.WriteString("Name",f.Name);w.WriteNumber("Offset",f.Offset);w.WriteNumber("End",f.End);w.WriteString("Sha256",f.Sha256);w.WriteEndObject();}w.WriteEndArray();
w.WritePropertyName("Packed");w.WriteStartArray();foreach(var p in packed){w.WriteStartObject();w.WriteString("Field",p.Field.ToString());w.WriteNumber("Count",p.Count);w.WriteNumber("Width",p.Width);w.WriteString("ValuesSha256",p.Sha);w.WriteEndObject();}w.WriteEndArray();
w.WritePropertyName("Compiled");w.WriteStartObject();w.WriteNumber("SourceKind",binding.SourceKind);w.WriteNumber("Profile",binding.Profile);w.WriteNumber("SourceScale",binding.SourceScale);w.WriteNumber("LiteralPacketByte0",binding.LiteralPacketByte0);w.WriteNumber("Context00",binding.Context00);w.WriteNumber("Boundary94C",binding.Boundary94C);w.WriteNumber("FinalUnigram94D",binding.FinalUnigram94D);w.WriteNumber("CaseBias",binding.CaseBias);w.WriteNumber("CaseMultiplier",binding.CaseMultiplier);w.WriteString("WordCostsSha256",wordHash);w.WritePropertyName("ClassCostsSha256");w.WriteStartArray();foreach(string s in classHash)w.WriteStringValue(s);w.WriteEndArray();w.WritePropertyName("BackoffCostsSha256");w.WriteStartArray();foreach(string s in backHash)w.WriteStringValue(s);w.WriteEndArray();
w.WritePropertyName("CaseRows");w.WriteStartArray();for(int i=1;i<=12;i++)for(int j=1;j<=12;j++){w.WriteStartArray();w.WriteNumberValue(i);w.WriteNumberValue(j);w.WriteNumberValue(binding.GetCaseCost(i,j));w.WriteEndArray();if(binding.GetCaseCost(0,i,j)!=binding.GetCaseCost(i,j))throw new Exception("Initial-history plane mismatch.");}w.WriteEndArray();w.WritePropertyName("CaseTriples");w.WriteStartArray();for(int i=1;i<=12;i++)for(int j=1;j<=12;j++)for(int k=1;k<=12;k++){w.WriteStartArray();w.WriteNumberValue(i);w.WriteNumberValue(j);w.WriteNumberValue(k);w.WriteNumberValue(binding.GetCaseCost(i,j,k));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
w.WritePropertyName("CanceledLoads");w.WriteStartArray();foreach(var f in faults){w.WriteStartObject();w.WriteString("Stage",f.Stage);w.WriteNumber("Calls",f.Calls);w.WriteBoolean("Published",false);w.WriteEndObject();}w.WriteEndArray();w.WritePropertyName("TamperedByteOffsets");w.WriteStartArray();foreach(int n in tampered)w.WriteNumberValue(n);w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS actual English sparse75fields/643871packed/1536costs/144+1728case; {guards}guards/{faults.Count}cancel; no masks/query/inference");
