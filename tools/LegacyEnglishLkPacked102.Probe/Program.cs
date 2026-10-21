using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("models-directory capture.json");
var models=args[0];byte[] ak=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.lite.res")),raw=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-lk-text.lite.res"));
var source=LegacySource247WireLayout.Read(ak);int guards=0;
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected bounded refusal.");}
Guard(()=>LegacyEnglishLkPackedData.Read(null!,raw));Guard(()=>LegacyEnglishLkPackedData.Read(source,raw,new(true)));Guard(()=>LegacyEnglishLkPackedData.Read(source,raw.AsSpan(0,raw.Length-1)));
var foreign=LegacySource247WireLayout.Read(File.ReadAllBytes(Path.Combine(models,"ru-RU/ru_RU-ak-cur.lite.res")));Guard(()=>LegacyEnglishLkPackedData.Read(foreign,raw));
var owner=LegacyEnglishLkPackedData.Read(source,raw);if(!ReferenceEquals(owner.Source,source)||!ReferenceEquals(owner.AcousticAlphabet,source.Alphabet))throw new Exception("Actual selected source identity lost.");
if(owner.Dictionary.StorageBytes!=2082964||owner.LexicalAlphabet.LabelCount!=104||owner.Prefix.AlphabetPayloadOffset!=110||owner.LexicalAlphabetAbsoluteOffset!=294)throw new Exception("Resource-relative and absolute AL domains conflated.");
string Nodes(LegacyPackedLexicon dictionary)
{
 using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);Span<byte> row=stackalloc byte[16];var children=new int[81];
 for(int node=0;node<dictionary.NodeCount;node++)
 {
  var range=dictionary.GetChildRange(node);int count=dictionary.CopyChildren(node,children);if(count!=range.End-range.Start||count!=dictionary.GetChildCount(node))throw new Exception("Child ownership mismatch.");
  for(int i=0;i<count;i++)if(children[i]!=range.Start+i||!dictionary.TryAdvance(node,dictionary.GetLabel(children[i]),out int found)||found!=children[i])throw new Exception("Actual packed transition mismatch.");
  row.Clear();BinaryPrimitives.WriteInt32LittleEndian(row,node);BinaryPrimitives.WriteUInt16LittleEndian(row[4..],dictionary.GetLabel(node));row[6]=dictionary.IsTerminal(node)?(byte)1:(byte)0;BinaryPrimitives.WriteInt32LittleEndian(row[8..],range.Start);BinaryPrimitives.WriteInt32LittleEndian(row[12..],range.End);hash.AppendData(row);
 }
 return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
}
var dict=owner.Dictionary;string nodeHash=Nodes(dict);
using var words=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);var path=new List<ushort>();var stack=new Stack<(int Node,int Child)>();stack.Push((0,0));int terminals=0;byte[] wordBuffer=new byte[88];
while(stack.Count!=0)
{
 var (node,offset)=stack.Pop();if(offset==0&&node!=0)
 {
  path.Add(dict.GetLabel(node));if(dict.IsTerminal(node))
  {
   if(!dict.Contains(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(path)))throw new Exception("Full actual terminal path lookup failed.");
   BinaryPrimitives.WriteUInt16LittleEndian(wordBuffer,(ushort)path.Count);for(int i=0;i<path.Count;i++)BinaryPrimitives.WriteUInt16LittleEndian(wordBuffer.AsSpan(2+i*2),path[i]);words.AppendData(wordBuffer.AsSpan(0,2+2*path.Count));terminals++;
  }
 }
 var range=dict.GetChildRange(node);if(offset<range.End-range.Start){stack.Push((node,offset+1));stack.Push((range.Start+offset,0));}else if(node!=0)path.RemoveAt(path.Count-1);
}
if(terminals!=260074||path.Count!=0)throw new Exception("Actual dictionary terminal traversal incomplete.");string wordHash=Convert.ToHexString(words.GetHashAndReset()).ToLowerInvariant();
raw[0]^=255;ak[0]^=255;string mutatedHash=Nodes(dict);if(mutatedHash!=nodeHash)throw new Exception("Borrowed model buffers changed owner.");raw[0]^=255;ak[0]^=255;
var childrenCopy=owner.CopySerializedRootChildren();childrenCopy[0]=default;if(owner.CopySerializedRootChildren()[0].Offset!=92)throw new Exception("Root inventory aliases caller copy.");
Guard(()=>dict.GetLabel(-1));Guard(()=>dict.GetChildRange(dict.NodeCount));Guard(()=>dict.CopyChildren(0,new int[1]));if(dict.Contains([]))throw new Exception("Empty word accepted.");
var faults=new List<(string Stage,int Calls)>();
foreach(string stage in new[]{"sha:input","copy:resource","sha:snapshot","alphabet:read","dictionary:read","dictionary:arrays","dictionary:traversal","dictionary:complete","publish"})
{
 using var cts=new CancellationTokenSource();int calls=0;Guard(()=>LegacyEnglishLkPackedData.ReadPrepared(source,raw,s=>{if(s==stage){calls++;cts.Cancel();}},cts.Token));if(calls!=1)throw new Exception("Cancellation stage not entered.");faults.Add((stage,calls));
}
using(var cts=new CancellationTokenSource())
{int visits=0;Guard(()=>LegacyEnglishLkPackedData.ReadPrepared(source,raw,s=>{if(s=="dictionary:traversal"&&++visits==2)cts.Cancel();},cts.Token));if(visits!=2)throw new Exception("Mid-traversal cancellation not exercised.");faults.Add(("dictionary:traversal-after65536",visits));}
foreach(string stage in new[]{"sha:input:chunk","sha:snapshot:chunk"})
{using var cts=new CancellationTokenSource();int calls=0;Guard(()=>LegacyEnglishLkPackedData.ReadPrepared(source,raw,s=>{if(s==stage&&++calls==2)cts.Cancel();},cts.Token));if(calls!=2)throw new Exception("Chunked hash cancellation not exercised.");faults.Add((stage+":after32768",calls));}
var tampered=new List<int>();
foreach(int at in new[]{0,14,184+14,2572+14,2588,2592,2596,2600,2604,2608,2609,2610,669165,669160,669161,669168,669172,669173,1252408})
{raw[at]^=1;try{Guard(()=>LegacyEnglishLkPackedData.Read(source,raw));}finally{raw[at]^=1;}tampered.Add(at);}
//Named PREPARED structural controls exercise parser failures beyond SHA.
foreach(int at in new[]{2572+14,2588,2592,2596,2600,2604,2608,2609,669160,669161,669168,669172,669173,1252408})
{raw[at]^=1;try{Guard(()=>LegacyPackedLexicon.ReadEnglish102(raw,default));}finally{raw[at]^=1;}}
//PREPARED native-alignment control: skipped padding has no data meaning.
raw[2610]^=1;try{var padding=LegacyPackedLexicon.ReadEnglish102(raw,default);if(padding.StorageBytes!=dict.StorageBytes)throw new Exception("Ignored alignment padding changed packed storage.");}finally{raw[2610]^=1;}
Guard(()=>LegacyPackedLexicon.Load(raw.AsSpan(2572,1252412-2572)));
long copyBytes=LegacyEnglishLkPackedCopyBudget.Preflight();Guard(()=>LegacyEnglishLkPackedCopyBudget.Preflight(copyBytes-1));Guard(()=>LegacyEnglishLkPackedCopyBudget.Preflight(-1));if(LegacyEnglishLkPackedCopyBudget.Preflight(copyBytes)!=copyBytes)throw new Exception("Boundary envelope changed.");
//Every stateless cancellation/refusal leaves subsequent real Read possible.
var retry=LegacyEnglishLkPackedData.Read(source,raw);if(retry.Dictionary.NodeCount!=dict.NodeCount)throw new Exception("Rejected load poisoned unrelated future load.");
var mapping=Enumerable.Range(0,104).Select(i=>{if(!owner.LexicalAlphabet.TryGetText((ushort)i,out string text))throw new Exception("Missing lexical text.");return new{Lexical=i,Text=text,ExactAcoustic=owner.AcousticAlphabet.TryGetLabel(text,out ushort id)?(int?)id:null};}).ToArray();
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("Scope","Actual English packed DATA only; no language/masks/controller/winner");w.WriteNumber("Nodes",dict.NodeCount);w.WriteNumber("Terminals",terminals);w.WriteNumber("MaximumWordLength",dict.MaximumWordLength);w.WriteNumber("StorageBytes",dict.StorageBytes);w.WriteString("NodeRecordSha256",nodeHash);w.WriteString("AfterCallerMutationSha256",mutatedHash);w.WriteString("WordLabelPreorderSha256",wordHash);w.WriteNumber("LogicalLoadCopyBytes",copyBytes);w.WriteNumber("Guards",guards);
w.WritePropertyName("RootChildren");w.WriteStartArray();foreach(var c in owner.CopySerializedRootChildren()){w.WriteStartObject();w.WriteNumber("Offset",c.Offset);w.WriteNumber("End",c.End);w.WriteNumber("Kind",c.Kind);w.WriteNumber("Version",c.Version);w.WriteEndObject();}w.WriteEndArray();
w.WritePropertyName("Prefix");w.WriteStartObject();w.WriteString("Identifier",owner.Prefix.Identifier);w.WriteNumber("Weight60",owner.Prefix.Weight60);w.WriteNumber("Weight64",owner.Prefix.Weight64);w.WriteNumber("Field68",owner.Prefix.Field68);w.WriteNumber("RuntimeLanguageScale",owner.Prefix.RuntimeLanguageScale);w.WriteNumber("AlphabetPayloadOffset",owner.Prefix.AlphabetPayloadOffset);w.WriteEndObject();w.WriteNumber("AlphabetAbsoluteOffset",owner.LexicalAlphabetAbsoluteOffset);
var language=owner.UnsupportedLanguageBoundary;w.WritePropertyName("UnsupportedLanguageBoundary");w.WriteStartObject();w.WriteNumber("Offset",language.Offset);w.WriteNumber("End",language.End);w.WriteNumber("Kind",language.Kind);w.WriteNumber("Version",language.Version);w.WriteEndObject();
w.WritePropertyName("ExactTextMapping");w.WriteStartArray();foreach(var m in mapping){w.WriteStartObject();w.WriteNumber("Lexical",m.Lexical);w.WriteString("Text",m.Text);if(m.ExactAcoustic.HasValue)w.WriteNumber("ExactAcoustic",m.ExactAcoustic.Value);else w.WriteNull("ExactAcoustic");w.WriteEndObject();}w.WriteEndArray();
w.WritePropertyName("CanceledLoads");w.WriteStartArray();foreach(var f in faults){w.WriteStartObject();w.WriteString("Stage",f.Stage);w.WriteNumber("Calls",f.Calls);w.WriteBoolean("Published",false);w.WriteEndObject();}w.WriteEndArray();w.WritePropertyName("TamperedByteOffsets");w.WriteStartArray();foreach(int n in tampered)w.WriteNumberValue(n);w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS actual English packed DATA {dict.NodeCount} nodes/{terminals} terminals/{guards} guards; all transitions+terminal Contains; no masks/language owner");
