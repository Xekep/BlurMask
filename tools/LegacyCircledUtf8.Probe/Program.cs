using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using InkRecognition;

if(args.Length!=5)throw new ArgumentException("Pass literal model, CCC/normalization model, independent controls, pinned actualRU resource and publication model.");
using var literalStream=File.OpenRead(args[0]);var model=LegacyNativeCircledPayloadData.Load(literalStream);
using var normStream=File.OpenRead(args[1]);var norm=LegacyNativeUnicodeNormalizationData.Load(normStream);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));int cases=0;
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] input=Convert.FromHexString(test.GetProperty("input").GetString()!);
    var result=LegacyNativeCircledUtf8Writeback.Complete(input,norm,model);
    if(!result.Bytes.Span.SequenceEqual(Convert.FromHexString(test.GetProperty("output").GetString()!))||result.Changed!=test.GetProperty("changed").GetBoolean())
        throw new Exception($"Circled writeback mismatch {Convert.ToHexString(input)}.");
    cases++;
}
int publicMapped=0,publicUnmapped=0;
foreach(var row in controls.RootElement.GetProperty("publicRows").EnumerateArray())
{
    string text=row[1].GetString()!;byte[] payload=LegacyNativeCanonicalScratch.PrepareUtf8Scratch(Encoding.UTF8.GetBytes(text),norm);
    bool mapped=model.TryReadCircledCodePoint(payload,out int cp);
    int expected=row[3].GetInt32();
    if(mapped!=(expected>=0)||cp!=expected)throw new Exception($"Independent public circle decomposition differs for{row[0].GetInt32():X}: actual{mapped}/{cp:X}.");
    if(mapped)publicMapped++;else publicUnmapped++;
}
int aliases=0;
foreach(var row in controls.RootElement.GetProperty("aliases").EnumerateArray())
{if(!model.TryReadCircledCodePoint(Convert.FromHexString(row[0].GetString()!),out int cp)||cp!=row[1].GetInt32())throw new Exception("Static compatibilityJamo alias differs.");aliases++;}
//Exhaustive original ASCII arithmetic expectations, including unsigned/signed byte invalid matches and leadingzero/non-number cases.
int arithmetic=0;
for(int count=1;count<=2;count++)for(int first=0;first<256;first++)for(int second=0;second<(count==1?1:256);second++)
{
    byte[] bytes=count==1?[(byte)first]:[(byte)first,(byte)second];int expected=-1;
    if(count==1)
    {if(first is >=65 and <=90)expected=0x24b6+first-65;else if(first is >=97 and <=122)expected=0x24d0+first-97;else if(first==48)expected=0x24ea;else if(first is >=49 and <=57)expected=0x2460+first-49;}
    else if(first is >=49 and <=53&&second is >=48 and <=57)
    {int n=(first-48)*10+second-48;if(n is >=10 and <=20)expected=0x2460+n-1;else if(n is >=21 and <=35)expected=0x3251+n-21;else if(n is >=36 and <=50)expected=0x32b1+n-36;}
    bool actual=model.TryReadCircledCodePoint(bytes,out int cp);
    if(actual!=(expected>=0)||cp!=expected)throw new Exception("Exhaustive ASCII decision mismatch.");arithmetic++;
}
//Linear stress: this follows the same native resume semantics without repeated whole-buffer memmove allocations.
string many=string.Concat(Enumerable.Repeat("\U000f0000A\U000f0001",4096));
var stress=LegacyNativeCircledUtf8Writeback.Complete(Encoding.UTF8.GetBytes(many),norm,model);
if(!stress.Changed||Encoding.UTF8.GetString(stress.Bytes.Span)!=new string('\u24b6',4096))throw new Exception("Repeated updated-end scan differs.");
using var canceled=new CancellationTokenSource();canceled.Cancel();
try{LegacyNativeCircledUtf8Writeback.Complete([],norm,model,canceled.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Reject(()=>LegacyNativeCircledUtf8Writeback.Complete([0xc0,0x80],norm,model));
Reject(()=>LegacyNativeCircledUtf8Writeback.Complete(new byte[1_048_577],norm,model));
byte[] corrupt=Encoding.UTF8.GetBytes(File.ReadAllText(args[0]).Replace("12896","12897",StringComparison.Ordinal));
Reject(()=>LegacyNativeCircledPayloadData.Load(new MemoryStream(corrupt)));
byte[] ruBytes=File.ReadAllBytes(args[3]);
if(Convert.ToHexString(SHA256.HashData(ruBytes)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned RU resource required.");
var alphabet=LegacyInkAlphabet.Load(ruBytes.AsSpan(99),0x103);int actualLabels=0;
for(int i=0;i<alphabet.LabelCount;i++)
{
    if(!alphabet.TryGetText((ushort)i,out var text))continue;
    byte[]actual=LegacyNativeCircledUtf8Writeback.Emit(new(0,(ushort)i),alphabet,norm,model);
    if(!actual.AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(text)))throw new Exception("Actual RU glyph/no-success original bytes changed.");actualLabels++;
}
using var publicationStream=File.OpenRead(args[4]);var properties=LegacyNativeTextPublicationData.Load(publicationStream);
byte[][]segments=[Encoding.UTF8.GetBytes("\U000f0000A\U000f0001"),Encoding.UTF8.GetBytes("\U000f0000"),Encoding.UTF8.GetBytes("\U000f0001"),Encoding.UTF8.GetBytes("\u2639"),[]];
var offsets=new List<int>{0};foreach(var segment in segments)offsets.Add(offsets[^1]+segment.Length);
var alternate=new LegacyNativeGlyphAlternateUtf8Table(segments.SelectMany(v=>v).ToArray(),offsets.ToArray());
byte[] rows=new byte[200],selectedRows=new byte[20];
for(int i=0;i<5;i++){BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*40+4),4);BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*40+6),(ushort)i);BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*40+8),(ushort)i);BinaryPrimitives.WriteUInt16LittleEndian(selectedRows.AsSpan(i*4),(ushort)i);}
var owner=new LegacyNativeGlyphRecordOwner([new(0x42,20,rows)]);
using(var pool=new LegacyNativeUtf8StringPool())
{
    var composed=LegacyNativeTextGlyphCaller.Produce(owner,alphabet,ruBytes.AsSpan(99,16),selectedRows,0x42,false,5,pool,properties,norm,
        actualFullGlyphEmitter:g=>LegacyNativeCircledUtf8Writeback.Emit(g.Symbol,alphabet,norm,model,alternate));
    if(composed.Records.Count!=4||composed.AdjustedMarkerIndex!=4||composed.RetainedSelectionRows.Length!=16)throw new Exception("Full helper/caller retained vector differs.");
    string[] expected=["\u24b6","\U000f0000","\U000f0001","\U0001f641"];
    for(int i=0;i<4;i++)if(!composed.Records[i].Text.ReadBytes().AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(expected[i]))||
        ((composed.Records[i].Flags&0x100000)!=0)!=(i is 1 or 2))throw new Exception("Actual complete helper->caller->intern/classification differs.");
}
Console.WriteLine($"PASS {cases} independent complete marker/writeback cases, {publicMapped}/{publicUnmapped} mapped/unmapped publicUnicode9 circle entries, {aliases} native literal aliases, {arithmetic} exhaustive byte decisions,4096 repeated conversions, {actualLabels} actualRU glyphs including unpairedF0000, full helper->caller->publication composition. ValidUTF8/default services only; no handwriting winner claim.");
static void Reject(Action a){try{a();}catch(ArgumentException){return;}catch(InvalidDataException){return;}throw new Exception("Invalid circled operation accepted.");}
