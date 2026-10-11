using System.Buffers.Binary;
using InkRecognition;
using System.Text.Json;
var ownNetwork = LegacyInkNetwork.Load(BuildNetwork());
var ownActivations = new LegacyInkActivations(new int[48], Enumerable.Repeat(512,128).ToArray());
var own = new LegacyContextWindowScorer(ownNetwork, ownActivations);
var line = Enumerable.Range(0,4).Select(i => new LegacyInkStroke([new LegacyInkPoint((short)(i*20),0),new((short)(i*20+10),10)])).ToArray();
var groups = own.ScoreLine(line, 2, 10, 10, [7,7,8,8]);
Require(groups.Length == 5 && groups.Select(g => (g.StartPhysicalIndex,g.EndPhysicalIndex)).SequenceEqual(new[] {(1,1),(1,2),(2,2),(2,3),(3,3)}), "Checked window enumeration must skip reference stroke0 and include prefixes up to2.");
// Equal EXP values25: sum700, reduced divisor87, valueShift26,
// ((25<<26)/87)>>14 =1177. Native quantization is not exact1/28.
Require(groups[0].CenterX == 25 && groups[1].CenterX == 35 && groups[0].Probabilities.Span[0] == 1177f/32768,
    $"Checked point-mean center or own network probabilities failed: centers{groups[0].CenterX},{groups[1].CenterX};probability{groups[0].Probabilities.Span[0]*32768}.");
Require(own.ScoreLine(line, 2, 10, 10, [7,7,8,8], checkScope:true).Length == 4, "Windows must stop at scope changes.");
Require(own.ScoreLine(line, 2, 10, 10, [7,7,8,8], nonblankCutoff:1).Length == 0, "Nonblank acceptance cutoff failed.");
float exactMass = 0; for(int i=0;i<27;i++) exactMass += 1177f/32768;
Require(own.ScoreLine(line, 2, 10, 10, [7,7,8,8], nonblankCutoff:exactMass).Length == 0, "Acceptance must be strict, not inclusive.");
Require(own.ScoreLine([line[0]], 2, 10, 10, [7]).Length == 0, "A sole reference stroke cannot manufacture a context window.");
var selected = LegacyPriorContextSelection.Select(groups, 2, [1], true, 7);
Require(selected.Length == 0, "Target must exclude its own accepted context windows.");
selected = LegacyPriorContextSelection.Select(groups, 2, [0]);
var fixedContext = LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(selected, 25, 10, 0, 1, 1));
Require(fixedContext.Length == 28 && fixedContext.All(p => p > -1024 && p < 0), "Geometry→network→selection→weighting→fixed context failed.");
var sensitiveNetwork=LegacyInkNetwork.Load(BuildNetwork(true));
var sensitiveActivations=new LegacyInkActivations(Enumerable.Range(0,48).Select(i=>i*20).ToArray(),Enumerable.Repeat(512,128).ToArray());
var sensitive=new LegacyContextWindowScorer(sensitiveNetwork,sensitiveActivations);
LegacyInkStroke[] backtracking = [new([new LegacyInkPoint(100,0),new(110,10)]),
    new([new LegacyInkPoint(-50,0),new(-40,10)]),new([new LegacyInkPoint(0,0),new(10,10)]),
    new([new LegacyInkPoint(20,0),new(30,10)])];
foreach(bool maximumMode in new[]{false,true})
{
    float reference=maximumMode?-50:110;
    var expected=LegacyClassScores.Convert(sensitiveNetwork.Evaluate(LegacyFloatInputQuantization.Quantize(
        LegacyContextGeometry.Extract([backtracking[3]],10,reference,10,maximumMode)),sensitiveActivations)).Probability32768.ToArray();
    var scored=sensitive.ScoreLine(backtracking,0,10,10,[7,8,9,10],maximumWindowStrokes:1,nonblankCutoff:0,checkScope:true,useMaximumX:maximumMode);
    Require(scored.Length==3 && scored[2].Probabilities.ToArray().Select(p=>(int)(p*32768)).SequenceEqual(expected),
        "Earlier X-reference must remain cumulative across backwards strokes in both modes.");
    var lastOnly=LegacyClassScores.Convert(sensitiveNetwork.Evaluate(LegacyFloatInputQuantization.Quantize(
        LegacyContextGeometry.Extract([backtracking[3]],10,maximumMode?0:10,10,maximumMode)),sensitiveActivations)).Probability32768.ToArray();
    Require(!expected.SequenceEqual(lastOnly),"Sensitive reference test must distinguish cumulative from last-stroke geometry.");
}
Throws<ArgumentException>(() => own.ScoreLine(line, 2, 10, 10, []));
Throws<ArgumentException>(() => own.ScoreLine(line, 2, 0, 10, [7,7,8,8]));
Throws<ArgumentException>(() => own.ScoreLine(line, 2, 10, 10, [7,7,8,8], maximumWindowStrokes:4));
Throws<ArgumentException>(() => own.ScoreLine(line, 2, 10, 10, [7,7,8,8], nonblankCutoff:float.NaN));
Throws<ArgumentException>(() => own.ScoreLine([new([new LegacyInkPoint(0,0)]),new([new LegacyInkPoint(10,10)])],2,10,10,[0,0]));
Throws<ArgumentOutOfRangeException>(() => new LegacyContextWindowScorer(ownNetwork,ownActivations,28));
Throws<OperationCanceledException>(() => own.ScoreLine(line, 2, 10, 10, [7,7,8,8], cancellationToken:new CancellationToken(true)));
Console.WriteLine("PASS own complete context-window chain, enumeration, centers, scope stops, cumulative X-reference in both modes, strict neural acceptance, target exclusion, weighting/fixed conversion, malformed/cancellation. Automatic lines and cursive text accuracy are not tested.");
if (args.Length == 0) return;
if (args.Length is < 2 or > 3) throw new ArgumentException("Usage: LegacyContextWindow.Probe [acoustic.res activations.json [calibration.json]]");
foreach(string path in args) if(new FileInfo(path).Length > 8*1024*1024) throw new ArgumentException("Caller asset exceeds8MiB.");
byte[] acoustic = File.ReadAllBytes(args[0]); using var tables = JsonDocument.Parse(File.ReadAllBytes(args[1]));
int[] Values(JsonElement e) => e.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
var activations = new LegacyInkActivations(Values(tables.RootElement.GetProperty("tanh").GetProperty("values")),Values(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
var actual = new LegacyContextWindowScorer(LegacyInkNetwork.Load(acoustic.AsSpan(189755,195746-189755)),activations);
LegacyInkStroke[] wordInk = [new([new LegacyInkPoint(0,40),new(0,0),new(12,30),new(24,0),new(24,40)]),
    new([new LegacyInkPoint(40,40),new(52,0),new(64,40)]),
    new([new LegacyInkPoint(46,20),new(58,20)]),
    new([new LegacyInkPoint(80,40),new(80,0),new(92,30),new(104,0),new(104,40)]),
    new([new LegacyInkPoint(120,40),new(132,0),new(144,40)]),
    new([new LegacyInkPoint(126,20),new(138,20)])];
int[][] glyphPhysicalIndices = [[0],[1,2],[3],[4,5]];
var actualGroups = actual.ScoreLine(wordInk,0,40,40,[0,0,0,0,0,0]);
Require(actualGroups.Length > 0,"Own printed МАМА produced no accepted initial context windows; inspect geometry/profile.");
short[]? minima=null,maxima=null;LegacyInkNetwork? directional=null;LegacyInkAlphabet? alphabet=null;
if(args.Length==3)
{
    using var calibration=JsonDocument.Parse(File.ReadAllBytes(args[2]));
    minima=calibration.RootElement.GetProperty("minima").EnumerateArray().Select(p=>p.GetInt16()).ToArray();
    maxima=calibration.RootElement.GetProperty("maxima").EnumerateArray().Select(p=>p.GetInt16()).ToArray();
    directional=LegacyInkNetwork.Load(acoustic.AsSpan(367899,467094-367899));alphabet=LegacyInkAlphabet.Load(acoustic.AsSpan(99),0x103);
}
for(int target=0;target<glyphPhysicalIndices.Length;target++)
{
    int[] physicalIndices = glyphPhysicalIndices[target];
    var glyph = physicalIndices.Select(index => wordInk[index]).ToArray();
    var observations = LegacyPriorContextSelection.Select(actualGroups,0,physicalIndices);
    float center = 0; int pointCount = 0;
    foreach(var stroke in glyph) foreach(var p in stroke.Points) {center += p.X; pointCount++;}
    center /= pointCount;
    var context = LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(observations,center,40,0,1,1));
    using var buffer = new MemoryStream();using(var writer=new Utf8JsonWriter(buffer))
    {
        writer.WriteStartObject();writer.WriteString("ownWord","МАМА");writer.WriteNumber("targetGlyphIndex",target);
        writer.WriteStartArray("physicalIndices");foreach(int index in physicalIndices)writer.WriteNumberValue(index);writer.WriteEndArray();
        writer.WriteNumber("acceptedWindows",actualGroups.Length);writer.WriteNumber("selectedWindows",observations.Length);
        writer.WriteStartArray("context");foreach(short value in context) writer.WriteNumberValue(value);writer.WriteEndArray();writer.WriteEndObject();
    }
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer.ToArray()));
    if(directional is not null)
        foreach(bool useContext in new[]{false,true})
        {
            var supplied=useContext?context:Enumerable.Repeat((short)-1024,28).ToArray();
            var features=LegacyTemporalDirectionalFeatures.Extract(glyph,new(40,40),minima!,maxima!,supplied,1024,1024);
            var scores=LegacyClassScores.Convert(directional.Evaluate(features,activations)).Probability32768.ToArray();
            using var scoreBuffer=new MemoryStream();using(var writer=new Utf8JsonWriter(scoreBuffer))
            {
                writer.WriteStartObject();writer.WriteNumber("targetGlyphIndex",target);writer.WriteBoolean("contextEnabled",useContext);
                writer.WriteStartArray("top");foreach(int index in Enumerable.Range(0,scores.Length).OrderByDescending(i=>scores[i]).ThenBy(i=>i).Take(3))
                {writer.WriteStartObject();writer.WriteNumber("class",index);if(alphabet!.TryGetText((ushort)index,out string text))writer.WriteString("text",text);writer.WriteNumber("probability32768",scores[index]);writer.WriteEndObject();}
                writer.WriteEndArray();writer.WriteEndObject();
            }
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(scoreBuffer.ToArray()));
        }
}
Console.WriteLine("Completed actual context-window diagnostics on own printed МАМА with explicit lineheight/Yreference40. Context variation does not establish text recognition accuracy.");
static byte[] BuildNetwork(bool sensitive=false)
{
    using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    writer.Write(0x0106fea0u);writer.Write(0u);writer.Write((byte)0x12);writer.Write((ushort)0x7001);
    writer.Write(30u);writer.Write(2u);writer.Write(0u);writer.Write(0u);writer.Write(29u);writer.Write((ushort)10);
    var hiddenWeights=new byte[30];if(sensitive)hiddenWeights[12]=127;
    var outputWeights=new byte[56];if(sensitive)outputWeights[1]=127;
    writer.Write(hiddenWeights);writer.Write(new byte[]{1,(byte)(sensitive?4:1)});writer.Write(outputWeights);
    writer.Write((byte)1);for(int i=1;i<29;i++)writer.Write((byte)(sensitive&&i==1?127:1));
    var data=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4),(uint)data.Length-8);return data;
}
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action) where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected{typeof(T).Name}.");}
