using System.Buffers.Binary;
using InkRecognition;
var graph=LegacyPackedLexicon.Load(OwnGraph());
var model=LegacyWordLanguageModel.Load(OwnModel());
var decoder=new LegacyIntegerWordDecoder(graph,new(graph),new(new(model,new(1,0,1,0,1,0),new(0,0,-1,0,0,new float[169]),1)));
var mapping=new LegacyAcousticLexicalMapping(LegacyInkAlphabet.Load(BuildAlphabet("ABC"),0x103),LegacyInkAlphabet.Load(BuildAlphabet("CAB"),0x103),3);
var endpoint=new LegacyPreparedSequenceWordSearch(mapping,decoder);
var min=Enumerable.Repeat((short)-1024,274).ToArray();var max=Enumerable.Repeat((short)1024,274).ToArray();
int[] widths=[256,238,313,313];ushort[] kinds=[0x1f,0x2f,0x3f,0x4f];
var classifier=new LegacyDirectionalGlyphClassifier(widths.Select((width,i)=>new LegacyDirectionalAcousticFamily(LegacyInkNetwork.Load(BuildNetwork(width,3,kinds[i])),new float[]{.5f,.5f,.5f},new float[]{1,1,1})).ToArray(),new(new int[48],Enumerable.Repeat(512,128).ToArray()),new(min,max),new(min,max));
// Supplied normalized geometry is intentionally independent of the float
// prepared record: this endpoint does not assert a float-to-short normalizer.
var scores=classifier.Classify(new LegacyInkStroke[]{new(new LegacyInkPoint[]{new(0,0),new(0,40)})},new(40,40),[],1024,1024);
var prepared=LegacyCursivePreparedInk.Create(new InkStroke[]{new(new InkPoint[]{new(0,0),new(40,40)})}).Insert(0,1,new(.25f,.75f),2);
LegacyCursiveCutMarker[] markers=[new(0,0,0),new(1,0,1)];
LegacyPreparedScoredGlyph[] glyphs=[new(0,2,scores,new float[]{1,0,0,0,0,0,0},1)];
var result=endpoint.Search(prepared,markers,glyphs,[],0,false,false,false,7,10000,false,1);
int expectedAcoustic=-(int)(8192*Math.Log(683/2048d));
var word=result.Matches.Single();
Require(word.WordId==1&&word.Labels.Span.SequenceEqual(new ushort[]{2})&&word.AcousticCost==expectedAcoustic&&word.Score.PreLexicalPathCost==expectedAcoustic+7&&word.Score.TotalCost==expectedAcoustic+157,"Prepared scores -> reversed alphabet -> terminal language winner/components differ.");
Require(result.Glyphs.Single().Ranges.Span.SequenceEqual(new LegacyInkRange[]{new(0,0,3)})&&prepared.Points.Span[1].X==.25f&&prepared.Points.Span[1].Y==.75f,"Shared endpoint ownership or fractional prepared geometry changed.");
Require(endpoint.Search(prepared,markers,glyphs,[],0,false,false,false,7,expectedAcoustic+157,true,1).Matches.Count==0,"Full heap terminal tie accepted.");
Require(endpoint.Search(prepared,markers,new LegacyPreparedScoredGlyph[]{glyphs[0] with{Weights=new float[7]}},[],0,false,false,false,0,10000,false,1).Matches.Count==0,"Zero-weight impossible glyph became a word.");
Reject(()=>endpoint.Search(prepared,markers,new LegacyPreparedScoredGlyph[]{glyphs[0] with{StartAtom=1}},[],0,false,false,false,0,10000,false,1));
Reject(()=>endpoint.Search(prepared,markers,new LegacyPreparedScoredGlyph[]{glyphs[0] with{AtomCount=1}},[],0,false,false,false,0,10000,false,1));
Reject(()=>endpoint.Search(prepared,new LegacyCursiveCutMarker[]{new(0,0,3)},glyphs,[],0,false,false,false,0,10000,false,1));
Reject(()=>endpoint.Search(prepared,markers,new LegacyPreparedScoredGlyph[]{glyphs[0] with{Weights=new float[]{1,0,0,0,0,0}}},[],0,false,false,false,0,10000,false,1));
try{endpoint.Search(prepared,markers,glyphs,[],0,false,false,false,0,10000,false,1,new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS prepared fixed sequence: own uniform NN probabilities -> integer acoustic {expectedAcoustic} -> reversed alphabet -> dictionary terminal score, fractional ink preserved, shared endpoint merge, partition/point bounds, inactive weights, heap tie, cancellation. Supplied normalized glyph is not raw-ink recognition.");
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Malformed input accepted.");}catch(ArgumentException){}}

static byte[] OwnGraph()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2b4u); w.Write(0x01000000u); w.Write(0u); w.Write(4u); w.Write(1u);
    w.Write(0x01000000u); w.Write(4u); w.Write((byte)3); w.Write(0u | (5u << 3) | (6u << 6) | (7u << 9));
    w.Write((byte)3); w.Write(0x01000000u); w.Write(4u); w.Write((byte)2); w.Write(3u); w.Write(3u);
    var bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}
static byte[] OwnModel(int words = 3)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2d1u); w.Write(0x01000000u); w.Write((ushort)0x100);
    w.Write(0u); w.Write(new byte[56]); w.Write(uint.MaxValue); w.Write(0x01000000u); w.Write(0u);
    w.Write(2u); w.Write(3u); w.Write((uint)words); w.Write(2u); w.Write(2u); w.Write(4u);
    w.Write(0x01000000u); w.Write((uint)words); w.Write((byte)1); w.Write(2u);
    w.Write(new byte[] { 0, 1, 2 }.Take(words).ToArray()); w.Write(3u); foreach (int x in new[] { 1000, 100, 300 }) w.Write(x);
    w.Write(new byte[] { 0, 1 }); w.Write(new byte[] { 0, 1, 2, 3 });
    w.Write(2u); w.Write(4u); foreach (int x in new[] { 30, 50, 40, 80, 120, 160 }) w.Write(x);
    w.Write(0u); w.Write(0u); w.Write(new byte[128 + 2 + 16 + 16 + 0x2a4 + 0x2254]);
    var bytes = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 8); return bytes;
}

static byte[] BuildAlphabet(string text, bool withRelatedForms = false)
{
    var encoded = text.EnumerateRunes().Select(rune => System.Text.Encoding.UTF8.GetBytes(rune.ToString())).ToArray();
    int count = encoded.Length;
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) writer.Write((ushort)65535); writer.Write((ushort)count);
    writer.Write((ushort)1); writer.Write((ushort)0);
    foreach (int tableSize in new[] { 32, 64 })
        for (int i = 0; i < tableSize; i++) writer.Write(i == tableSize - 1 ? (ushort)0 : (ushort)65535);
    writer.Write((ushort)0); writer.Write((ushort)count);
    uint offset = 0;
    for (int i = 0; i < count; i++) { writer.Write(offset); writer.Write((uint)i); offset += (uint)encoded[i].Length; }
    writer.Write(offset); writer.Write(65535u);
    foreach (var bytes in encoded) writer.Write(bytes);
    for (int i = 0; i < count; i++) writer.Write((ushort)i);
    writer.Write((byte)0); writer.Write((ushort)(withRelatedForms ? 1 : 0));
    if (withRelatedForms)
    {
        Require(count == 3, "Original related-form fixture expects three Cyrillic labels.");
        writer.Write((ushort)0); writer.Write((ushort)2); writer.Write((ushort)0);
        writer.Write(1u); writer.Write(new byte[] { 0, 255, 0 });
    }
    return stream.ToArray();
}
static byte[] BuildNetwork(int inputs,int outputs,ushort kind)
{
    using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    writer.Write((ushort)0xfea0);writer.Write((ushort)0x106);writer.Write(0u);writer.Write((byte)0x12);writer.Write(kind);
    writer.Write((uint)inputs);writer.Write(2u);writer.Write(0u);writer.Write(0u);writer.Write((uint)(outputs+1));writer.Write((ushort)10);
    writer.Write(new byte[inputs]);writer.Write(new byte[]{1,1});writer.Write(new byte[2*outputs]);writer.Write(Enumerable.Repeat((byte)1,outputs+1).ToArray());
    var data=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4),(uint)data.Length-8);return data;
}
