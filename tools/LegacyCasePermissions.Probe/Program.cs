using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("LK controls");
byte[] lk=File.ReadAllBytes(args[0]);int length=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(lk.AsSpan(2513127))+8);
byte[] bytes=lk.AsSpan(2513123,length).ToArray();int at=bytes.Length-0x2a4-0x2254-34;
var modern=LegacyWordLanguageModel.Load(bytes);if(modern.CasePathVersion!=0x200)throw new Exception("Actual TCM version differs.");
BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at),0);var old=LegacyWordLanguageModel.Load(bytes);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[1]));int cases=0,rows=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var a=item.GetProperty("alphabet");var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(a.GetProperty("payload").GetString()!),a.GetProperty("version").GetUInt16());
 var p0=new LegacyNativeCasePermissions(alphabet,old);var p200=new LegacyNativeCasePermissions(alphabet,modern);
 foreach(var row in item.GetProperty("rows").EnumerateArray())
 {
  byte[] source=Convert.FromBase64String(row[0].GetString()!);byte[] snapshot=source.ToArray();
  foreach(var pair in new[]{(p0,1),(p200,2)})
  {
   byte[] expected=Convert.FromBase64String(row[pair.Item2].GetString()!);
   if(!pair.Item1.ExpandStoredPermissions(source).AsSpan().SequenceEqual(expected))throw new Exception("Ordered permission writes differ.");
   if(!source.AsSpan().SequenceEqual(snapshot))throw new Exception("Caller mask mutated.");
  }
  rows++;
 }
 try{p0.ExpandStoredPermissions(new byte[alphabet.LabelCount+1]);throw new Exception("Wrong extent accepted.");}catch(ArgumentException){}
 try{p200.ExpandStoredPermissions(new byte[alphabet.LabelCount],new(true));throw new Exception("Canceled accepted.");}catch(OperationCanceledException){}
 cases++;
}
BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at),0x300);
var unknown=LegacyWordLanguageModel.Load(bytes);
var first=json.RootElement[0].GetProperty("alphabet");var owner=LegacyInkAlphabet.Load(Convert.FromBase64String(first.GetProperty("payload").GetString()!),first.GetProperty("version").GetUInt16());
try{_ = new LegacyNativeCasePermissions(owner,unknown);throw new Exception("Unknown TCM accepted.");}catch(NotSupportedException){}
Console.WriteLine($"PASS {cases} raw-triplet alphabets/{rows} byte masks for TCM0/200; exact sequential writes, input snapshots, unknown-version/extent/cancellation guards. Inner source68 remains separate.");
