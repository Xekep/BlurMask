using System.Buffers.Binary;
using InkRecognition;
using System.Text.Json;
var samples=Samples().ToArray();
var neutralMin=Enumerable.Repeat((short)-1024,274).ToArray();var neutralMax=Enumerable.Repeat((short)1024,274).ToArray();
var emptyContext=Enumerable.Repeat((short)-1024,28).ToArray();
foreach(var sample in samples)
{
    Require(LegacySeparatedInkSegmenter.TrySegment(sample.Strokes,out var glyphs)&&glyphs.Length==sample.GlyphCount,"Own separated word fixture grouping failed.");
    foreach(var glyph in glyphs)
    {
        var baseline=new LegacyInkBaseline(checked((short)sample.Bottom),checked((short)sample.Height));
        Require(LegacySpatialDirectionalFeatures.Extract(glyph,baseline,neutralMin,neutralMax,emptyContext,1024,1024).Length==313&&
            LegacyTemporalDirectionalFeatures.Extract(glyph,baseline,neutralMin,neutralMax,emptyContext,1024,1024).Length==313&&
            LegacyStrokeFeatures.ExtractInputWithBaseline(glyph,baseline,emptyContext,1024,1024).Length==256&&
            LegacyRasterFeatures.ExtractInputWithBaseline(glyph,baseline,emptyContext,1024,1024).Length==238,
            "Scaled/translated word glyph cannot compose all four feature families.");
    }
}
// Own resource-free roundtrip deliberately reverses acoustic/dictionary IDs.
// Nonuniform quantized scores calibrate to 0.5, so bypassing calibration or
// treating IDs as interchangeable cannot satisfy the expected path and cost.
var ownAcoustic=LegacyInkAlphabet.Load(BuildAlphabet("\u0418\u041f"),0x103);
var ownLexical=LegacyInkAlphabet.Load(BuildAlphabet("\u041f\u0418"),0x103);
var ownMapping=new LegacyAcousticLexicalMapping(ownAcoustic,ownLexical,2);
var ownFamilies=new LegacyAcousticFamilyScores?[7];
ownFamilies[2]=new(new int[]{8192,24576},new float[]{.25f,.75f},new float[]{4,4});
ownFamilies[3]=new(new int[]{8192,24576},new float[]{.25f,.75f},new float[]{4,4});
var ownConfidence=LegacyAcousticEnsemble.Combine(ownFamilies);
Require(ownConfidence.AsSpan().SequenceEqual(new float[]{.5f,.5f}),"Own mode1 confidence calibration changed.");
var ownChoices=ownMapping.Map(ownConfidence.Select(value=>(int)(value*32768)).ToArray());
var ownGraph=LegacyPackedLexicon.Load(BuildGraph());
var ownMatches=LegacyLexicalDecoder.Decode(ownGraph,new ReadOnlyMemory<LegacyLexicalCandidate>[]{ownChoices,ownChoices});
Require(ownMatches.Count==1&&ownMatches[0].Labels.Span.SequenceEqual(new ushort[]{1,0})&&
    Math.Abs(ownMatches[0].LogScore+2*Math.Log(2))<1e-6,"Calibrated confidence -> reversed alphabet mapping -> terminal word roundtrip failed.");
Require(LegacyLexicalDecoder.Decode(ownGraph,new ReadOnlyMemory<LegacyLexicalCandidate>[]{ownChoices}).Count==0,"Incomplete word accepted.");
using(var canceled=new CancellationTokenSource())
{
    canceled.Cancel();
    try { LegacyAcousticEnsemble.Combine(ownFamilies,canceled.Token);throw new InvalidOperationException("Calibration cancellation ignored."); } catch(OperationCanceledException){}
    try { ownMapping.Map([16384,16384],canceled.Token);throw new InvalidOperationException("Mapping cancellation ignored."); } catch(OperationCanceledException){}
}
Console.WriteLine("PASS own score -> mode1 ensemble calibration -> original bounded quantization -> reversed acoustic/lexical mapping -> terminal dictionary beam roundtrip, incomplete path and cancellation.");
// Exercise the reusable model/context/dictionary module with own zero-weight
// networks. The reversed alphabets and sole terminal word constrain the result;
// this validates transport and ownership rather than recognition accuracy.
var ownActivations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
var ownCenters=new float[]{.5f,.5f};var ownSlopes=new float[]{1,1};
int[] ownWidths=[256,238,313,313];ushort[] ownKinds=[0x1f,0x2f,0x3f,0x4f];
var ownModels=ownWidths.Select((width,index)=>new LegacyDirectionalAcousticFamily(
    LegacyInkNetwork.Load(BuildNetwork(width,2,ownKinds[index])),ownCenters,ownSlopes)).ToArray();
var ownInitial=LegacyInkNetwork.Load(BuildNetwork(30,28,0x7001));
var reusable=new LegacyDirectionalWordRecognizer(ownModels,ownInitial,ownActivations,
    ownAcoustic,ownLexical,ownGraph,new(neutralMin,neutralMax),new(neutralMin,neutralMax));
LegacyInkStroke[] ownInk=[new([new LegacyInkPoint(0,0),new(0,40),new(24,0),new(24,40)]),
    new([new LegacyInkPoint(40,0),new(40,40),new(64,0),new(64,40)])];
ownCenters[0]=float.NaN;ownSlopes[0]=0; // The module must retain its own immutable coefficient snapshots.
var reusableResult=reusable.Recognize(ownInk,new(40,40));
Require(reusableResult.Words.Count==1&&reusableResult.Words[0].Text=="\u0418\u041f"&&
    Math.Abs(reusableResult.Words[0].LogScore+2*Math.Log(2))<1e-6&&reusableResult.GlyphCount==2&&reusableResult.ContextWindowCount>0,
    "Reusable four-family model/context/dictionary path or coefficient ownership failed.");
Require(reusable.Recognize(ownInk,new(40,40),includeBaselineFamilies:false).Words[0]==reusableResult.Words[0],
    "Two/four-family synthetic equal-score transport differs.");
foreach(Action invalid in new Action[]{()=>reusable.Recognize(ownInk,new(40,0)),
    ()=>reusable.Recognize([ownInk[0],ownInk[0]],new(40,40)),()=>reusable.Recognize(ownInk,new(40,40),beamWidth:257)})
{
    try{invalid();throw new InvalidOperationException("Invalid reusable word request accepted.");}catch(ArgumentException){}
}
try{reusable.Recognize(ownInk,new(40,40),cancellationToken:new CancellationToken(true));throw new InvalidOperationException("Reusable word cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine("PASS reusable four-family word module: own model forward/context/dictionary, coefficient snapshots, explicit subset, malformed requests and cancellation.");
Console.WriteLine("PASS eight own separated printed word fixtures through grouping, caller scaled/translated source baseline and all four 256/238/313/313 composers. Vendor-free controls assert no word accuracy.");
if(args.Length==0)return;
if(args.Length!=6)throw new ArgumentException("Usage: LegacyDirectionalWord.Probe [acoustic.res lexical.res activations.json temporal-calibration.json spatial-calibration.json mode1-score-calibration.json]");
foreach(string path in args)if(new FileInfo(path).Length>8*1024*1024)throw new ArgumentException("Caller asset exceeds8MiB.");
byte[] acoustic=File.ReadAllBytes(args[0]),lexical=File.ReadAllBytes(args[1]);using var tables=JsonDocument.Parse(File.ReadAllBytes(args[2]));
int[] Integers(JsonElement element)=>element.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
float[] Floats(JsonElement element)=>element.EnumerateArray().Select(p=>p.GetSingle()).ToArray();
var activations=new LegacyInkActivations(Integers(tables.RootElement.GetProperty("tanh").GetProperty("values")),Integers(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
(short[],short[]) Calibration(string path)
{
    using var document=JsonDocument.Parse(File.ReadAllBytes(path));return(document.RootElement.GetProperty("minima").EnumerateArray().Select(p=>p.GetInt16()).ToArray(),document.RootElement.GetProperty("maxima").EnumerateArray().Select(p=>p.GetInt16()).ToArray());
}
var(temporalMin,temporalMax)=Calibration(args[3]);var(spatialMin,spatialMax)=Calibration(args[4]);
using var calibration=JsonDocument.Parse(File.ReadAllBytes(args[5]));
var centers=calibration.RootElement.GetProperty("centers").EnumerateArray().Select(Floats).ToArray();
var slopes=calibration.RootElement.GetProperty("slopes").EnumerateArray().Select(Floats).ToArray();
if(centers.Length!=4||slopes.Length!=4||centers.Any(row=>row.Length!=180)||slopes.Any(row=>row.Length!=180))throw new ArgumentException("Explicit inspected mode1 rows4×180 required.");
var trajectoryNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(195895,283690-195895));var rasterNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(283697,367892-283697));
var temporalNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(367899,467094-367899));var spatialNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(467101,566296-467101));
var initialNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(189755,195746-189755));
var acousticAlphabet=LegacyInkAlphabet.Load(acoustic.AsSpan(99),0x103);var lexicalAlphabet=LegacyInkAlphabet.Load(lexical.AsSpan(607074),0x103);
int graphLength=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(lexical.AsSpan(609984,4))+8);var lexicon=LegacyPackedLexicon.Load(lexical.AsSpan(609980,graphLength));
LegacyInkNetwork[] networks=[trajectoryNetwork,rasterNetwork,temporalNetwork,spatialNetwork];
var recognizer=new LegacyDirectionalWordRecognizer(networks.Select((network,index)=>new LegacyDirectionalAcousticFamily(network,centers[index],slopes[index])).ToArray(),
    initialNetwork,activations,acousticAlphabet,lexicalAlphabet,lexicon,new(temporalMin,temporalMax),new(spatialMin,spatialMax),relatedFormMode:0);
int firstMatches=0;
foreach(bool allFour in new[]{false,true})
{
firstMatches=0;
foreach(var sample in samples)
{
    var result=recognizer.Recognize(sample.Strokes,new(checked((short)sample.Bottom),checked((short)sample.Height)),includeBaselineFamilies:allFour);
    var decoded=result.Words;int expectedRank=decoded.ToList().FindIndex(match=>match.Text==sample.Expected)+1;var matches=decoded.Take(5).ToArray();if(expectedRank==1)firstMatches++;
    using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))
    {
        writer.WriteStartObject();writer.WriteString("ownPrintedSample",sample.Name);writer.WriteNumber("scale",sample.Scale);writer.WriteNumber("x",sample.X);writer.WriteNumber("y",sample.Y);
        writer.WriteNumber("sourceBaselineBottom",sample.Bottom);writer.WriteNumber("sourceLineHeight",sample.Height);writer.WriteNumber("contextWindows",result.ContextWindowCount);writer.WriteString("families",allFour?"1f+2f+3f+4f, caller mode1 rows0..3":"3f+4f only, caller mode1 rows2/3");
        writer.WriteString("expected",sample.Expected);writer.WriteNumber("expectedRankInReturnedBeam",expectedRank);writer.WriteStartArray("matches");foreach(var match in matches){writer.WriteStartObject();writer.WriteString("text",match.Text);writer.WriteNumber("originalLogCost",match.LogScore);writer.WriteEndObject();}writer.WriteEndArray();writer.WriteEndObject();
    }
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
}
Console.WriteLine($"DIAGNOSTIC {(allFour?"four":"two")}-family expected first dictionary match{firstMatches}/{samples.Length} own fixtures. Original confidence quantization/alias/beam policies; not SDK word decoder or cursive accuracy.");
}
static IEnumerable<Sample> Samples()
{
    LegacyInkStroke[] Letter(params short[][][] strokes)=>strokes.Select(stroke=>new LegacyInkStroke(stroke.Select(p=>new LegacyInkPoint(p[0],p[1])).ToArray())).ToArray();
    var m=Letter([[0,90],[0,10],[25,60],[50,10],[50,90]]);var a=Letter([[0,90],[25,10],[50,90]],[[10,60],[40,60]]);
    var i=Letter([[0,10],[0,90],[50,10],[50,90]]);var r=Letter([[0,90],[0,10],[40,10],[50,20],[50,40],[40,50],[0,50]]);
    foreach(var(name,expected,glyphs)in new(string,string,LegacyInkStroke[][])[]{("МАМА","мама",[m,a,m,a]),("МИР","мир",[m,i,r])})
        foreach(var(scale,x,y)in new[]{(1,0,0),(1,-100,300),(2,0,0),(3,200,-100)})
            yield return new(name,expected,glyphs.Length,scale,x,y,90*scale+y,80*scale,glyphs.SelectMany((glyph,index)=>glyph.Select(stroke=>new LegacyInkStroke(stroke.Points.ToArray().Select(point=>new LegacyInkPoint(checked((short)((point.X+index*70)*scale+x)),checked((short)(point.Y*scale+y)))).ToArray()))).ToArray());
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
static byte[] BuildGraph(uint lastLabel = 0)
{
    using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
    writer.Write(0x0101fe90u); writer.Write(0u); writer.Write(0x2b4u); writer.Write(0x01000000u);
    writer.Write(0u); writer.Write(3u); writer.Write(2u);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)2); writer.Write(0b100100u | (lastLabel << 4)); // 0,1,terminal|lastLabel.
    writer.Write((byte)2);
    writer.Write(0x01000000u); writer.Write(3u); writer.Write((byte)1); writer.Write(0b011u); // Child counts1,1,0.
    writer.Write(1u); var data = stream.ToArray(); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), (uint)data.Length - 8); return data;
}
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
sealed record Sample(string Name,string Expected,int GlyphCount,int Scale,int X,int Y,int Bottom,int Height,LegacyInkStroke[] Strokes);
