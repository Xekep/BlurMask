using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 2))throw new ArgumentException("Usage: controls.json [acoustic.res]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int count=0;
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 int expected=test[1].GetInt32();bool found=LegacyWideSymbolPairs.TryGetWideBase(test[0].GetInt32(),out int actual);
 Require(found==(expected>=0)&&actual==expected,"Independent public Unicode wide mapping differs.");count++;
}
int wideCount=0;
foreach(var test in controls.RootElement.GetProperty("wide").EnumerateArray())
{Require(LegacyWideSymbolPairs.TryGetWideBase(test[0].GetInt32(),out int actual)&&actual==test[1].GetInt32(),"Exhaustive wide set differs.");wideCount++;}
Require(wideCount==104,"Wide set size differs.");
Require(!LegacyWideSymbolPairs.TryGetWideBase(-1,out _)&&!LegacyWideSymbolPairs.TryGetWideBase(0x110000,out _)&&!LegacyWideSymbolPairs.TryGetWideBase(0xd800,out _),"Invalid scalar unexpectedly decomposes.");
var alphabet=LegacyInkAlphabet.Load(BuildAlphabet(new[]{"Ａ","B","A","ｂ","b","　"," ","＄","$","（","(","ＡA","ｍ","m","р","p","Ｒ","Ｒx"}),0x103);
var pairs=LegacyWideSymbolPairs.CreateUnrestricted(alphabet);
Require(pairs.AsSpan().SequenceEqual(new LegacyAcousticCachePair[]{new(2,0),new(4,3),new(6,5),new(8,7),new(10,9),new(13,12)}),"Native unrestricted single-scalar/exact-existing-base/ascending-label pair construction differs.");
Require(!pairs.Any(p=>p.First==14||p.Second==14),"Latin p must not become Cyrillic р.");
try{LegacyWideSymbolPairs.CreateUnrestricted(alphabet,new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {wideCount} exhaustive public wide mappings +{count} scalar references; missing bases, multi-scalar text, no script/case folding, pair ordering and cancellation.");
if(args.Length==2)
{
 byte[] resource=File.ReadAllBytes(args[1]);var actualAlphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),0x103);
 Require(actualAlphabet.LabelCount==192&&LegacyWideSymbolPairs.CreateUnrestricted(actualAlphabet).Length==0,"Observed Russian acoustic alphabet native wide pair list must be empty.");
 Console.WriteLine("PASS actual RU192 alphabet: native wide-pair producer yields0; Cyrillic/Latin confusable classes do not inherit each other's score through C0/C8.");
}
static byte[] BuildAlphabet(string[] text)
{
 using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);int count=text.Length;
 for(int i=0;i<7;i++)writer.Write(ushort.MaxValue);writer.Write((ushort)count);writer.Write((ushort)1);writer.Write((ushort)0);
 foreach(int n in new[]{32,64})for(int i=0;i<n;i++)writer.Write(i==n-1?(ushort)0:ushort.MaxValue);
 writer.Write((ushort)0);writer.Write((ushort)count);uint offset=0;
 for(int i=0;i<count;i++){writer.Write(offset);writer.Write((uint)i);offset+=(uint)Encoding.UTF8.GetByteCount(text[i]);}
 writer.Write(offset);writer.Write(65535u);foreach(string value in text)writer.Write(Encoding.UTF8.GetBytes(value));
 for(int i=0;i<count;i++)writer.Write((ushort)i);writer.Write((byte)0);writer.Write((ushort)0);return stream.ToArray();
}
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
