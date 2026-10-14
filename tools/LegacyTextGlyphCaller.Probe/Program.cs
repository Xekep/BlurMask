using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 4) throw new ArgumentException("Pass original controls, publication model, CCC model and pinned RU resource.");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0])); var fixture = controls.RootElement;
byte[] alphabetBytes = Convert.FromHexString(fixture.GetProperty("alphabet").GetString()!);
var alphabet = LegacyInkAlphabet.Load(alphabetBytes, 0x103); byte[] storage = Convert.FromHexString(fixture.GetProperty("records").GetString()!);
ushort count = fixture.GetProperty("packedCount").GetUInt16();
const ulong key = 0x0100001234000042;
var owner = new LegacyNativeGlyphRecordOwner([new(key, count, storage)]);
storage.AsSpan().Clear(); //owner must copy actual record bytes
using var propertyStream = File.OpenRead(args[1]); var properties = LegacyNativeTextPublicationData.Load(propertyStream);
using var cccStream = File.OpenRead(args[2]); var ccc = LegacyNativeUnicodeNormalizationData.Load(cccStream);
byte[][] alt = fixture.GetProperty("alternate").EnumerateArray().Select(v => Convert.FromHexString(v.GetString()!)).ToArray();
var offsets = new List<int> { 0 }; foreach (byte[] b in alt) offsets.Add(offsets[^1] + b.Length);
var alternate = new LegacyNativeGlyphAlternateUtf8Table(alt.SelectMany(v=>v).ToArray(), offsets.ToArray());
int own = 0, prepared = 0;
foreach (var test in fixture.GetProperty("cases").EnumerateArray())
{
    bool full = test.GetProperty("prepared").GetBoolean(); using var pool = new LegacyNativeUtf8StringPool();
    var result = LegacyNativeTextGlyphCaller.Produce(owner, alphabet, alphabetBytes.AsSpan(0, 16),
        Convert.FromHexString(test.GetProperty("selection").GetString()!), key, test.GetProperty("filter").GetBoolean(),
        test.GetProperty("marker").GetInt32(), pool, properties, ccc, alternate, actualFullGlyphEmitter: full ? FullFixtureEmission : null);
    if (!result.RetainedSelectionRows.Span.SequenceEqual(Convert.FromHexString(test.GetProperty("retained").GetString()!)) ||
        result.AdjustedMarkerIndex != test.GetProperty("expectedMarker").GetInt32()) throw new Exception("Native selection removal/marker tracking differs.");
    var expected = test.GetProperty("expected").EnumerateArray().ToArray();
    if (result.Records.Count != expected.Length) throw new Exception("Caller result count differs.");
    for (int i=0;i<expected.Length;i++)
    {
        var r=result.Records[i]; byte[] bytes=Convert.FromHexString(expected[i][0].GetString()!);
        if (!r.Text.ReadBytes().AsSpan().SequenceEqual(bytes) || r.ByteCount!=bytes.Length || (r.Flags&0x1003ff)!=expected[i][1].GetUInt32())
            throw new Exception("Caller transformation/publication differs.");
    }
    if (full) prepared++; else own++;
}
//Exact signed index/key normalization; bits57..63 do not become native owner identity.
for (int i=0;i<17;i++)
{
    ulong reference=key|((ulong)i<<40)|0xfe00000000000000;
    if (owner.Resolve(reference).Reference!=reference || owner.Resolve(reference).ReadRecord().Length!=40) throw new Exception("Reference transport differs.");
}
Reject(()=>owner.Resolve(key|(0xffffUL<<40))); Reject(()=>owner.Resolve(key|(17UL<<40))); Reject(()=>owner.Resolve(key^1));
Reject(()=>new LegacyNativeGlyphRecordOwner([new(key,0x2000,new byte[0])]));
Reject(()=>new LegacyNativeGlyphRecordOwner([new(key,4,new byte[120])]));
Reject(()=>new LegacyNativeGlyphRecordOwner([new(key,0,ReadOnlyMemory<byte>.Empty),new(key,0,ReadOnlyMemory<byte>.Empty)]));
using(var pool=new LegacyNativeUtf8StringPool())
{
    byte[] selected=new byte[4]; BinaryPrimitives.WriteUInt16LittleEndian(selected,15);
    Reject(()=>LegacyNativeTextGlyphCaller.Produce(owner,alphabet,alphabetBytes.AsSpan(0,16),selected,key,false,0,pool,properties,ccc,alternate));
    Reject(()=>LegacyNativeTextGlyphCaller.Produce(owner,alphabet,alphabetBytes.AsSpan(0,15),[],key,false,0,pool,properties,ccc));
    using var cancel=new CancellationTokenSource();cancel.Cancel();
    try{LegacyNativeTextGlyphCaller.Produce(owner,alphabet,alphabetBytes.AsSpan(0,16),[],key,false,0,pool,properties,ccc,cancellationToken:cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
}
byte[] ruBytes=File.ReadAllBytes(args[3]);
if(Convert.ToHexString(SHA256.HashData(ruBytes)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned RU resource required.");
var ru=LegacyInkAlphabet.Load(ruBytes.AsSpan(99),0x103); var valid=new List<ushort>();
for(int i=0;i<ru.LabelCount;i++)if(ru.TryGetText((ushort)i,out string text)&&!text.Contains("\U000f0000",StringComparison.Ordinal))valid.Add((ushort)i);
byte[] ruRecords=new byte[valid.Count*40],ruSelected=new byte[valid.Count*4];
for(int i=0;i<valid.Count;i++){BinaryPrimitives.WriteUInt16LittleEndian(ruRecords.AsSpan(i*40+8),valid[i]);BinaryPrimitives.WriteUInt16LittleEndian(ruRecords.AsSpan(i*40+6),(ushort)i);BinaryPrimitives.WriteUInt16LittleEndian(ruSelected.AsSpan(i*4),(ushort)i);}
var ruOwner=new LegacyNativeGlyphRecordOwner([new(0x420001,(ushort)(valid.Count<<2),ruRecords)]);
using(var pool=new LegacyNativeUtf8StringPool())
{
    var result=LegacyNativeTextGlyphCaller.Produce(ruOwner,ru,ruBytes.AsSpan(99,16),ruSelected,0x420001,true,valid.Count,pool,properties,ccc);
    if(result.Records.Count!=188||result.AdjustedMarkerIndex!=188)throw new Exception("Actual RU HeaderE filter was mistaken for stored space.");
    for(int i=0;i<valid.Count;i++)
    {
        ru.TryGetText(valid[i],out string text);if(text=="\u2639")text="\U0001f641";else if(text=="\u263a")text="\U0001f642";
        if(!result.Records[i].Text.ReadBytes().AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(text)))throw new Exception("Actual RU caller bytes differ.");
    }
}
Console.WriteLine($"PASS {own} ordinary vector callers/{prepared} explicit full-emitter fixtures, signed owner/key/40-byte record guards, 188 actual RU primary emissions->caller->publication. Circle-start helper remains explicit, no accepted OCR winner/source geometry claim.");

byte[] FullFixtureEmission(LegacyNativeOwnedGlyphRecord glyph)
{
    if(!ReferenceEquals(glyph.Owner,owner))throw new Exception("Wrong emitter owner.");
    //Prepared full-emitter fixture only: no assertion that this executes0491A0's unfinished circle branch.
    if((glyph.Symbol.KindFlags&7)==4)return alt[glyph.Symbol.SymbolOrRunLength].ToArray();
    return LegacyNativeOrdinaryGlyphUtf8.Emit(glyph.Symbol,alphabet,alternate);
}
static void Reject(Action action){try{action();}catch(ArgumentException){return;}catch(InvalidDataException){return;}catch(NotSupportedException){return;}throw new Exception("Invalid owner/caller accepted.");}
