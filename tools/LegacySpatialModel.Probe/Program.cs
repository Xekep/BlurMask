using InkRecognition;
using System.Text.Json;
var minima=Enumerable.Repeat((short)-1024,274).ToArray();var maxima=Enumerable.Repeat((short)1024,274).ToArray();
var empty=Enumerable.Repeat((short)-1024,28).ToArray();
LegacyInkStroke[] ownGlyph=[new([new LegacyInkPoint(0,40),new(0,0),new(24,40),new(24,0)])];
var spatial=LegacySpatialDirectionalFeatures.Extract(ownGlyph,new(40,40),minima,maxima,empty,123,-456);
var temporal=LegacyTemporalDirectionalFeatures.Extract(ownGlyph,new(40,40),minima,maxima,empty,123,-456);
Require(spatial.Length==313&&spatial[0]==1024&&spatial.AsSpan(283,28).SequenceEqual(empty)&&spatial[311]==123&&spatial[312]==-456,"Full spatial313 composition/order failed.");
Require(spatial.AsSpan(0,18).SequenceEqual(temporal.AsSpan(0,18))&&spatial.AsSpan(274).SequenceEqual(temporal.AsSpan(274))&&!spatial.AsSpan(18,256).SequenceEqual(temporal.AsSpan(18,256)),"Temporal/spatial shared globals/append and differing local layout failed.");
Throws<ArgumentException>(()=>LegacySpatialDirectionalFeatures.Extract(ownGlyph,new(40,40),minima.AsSpan(1),maxima,empty,0,0));
Throws<OperationCanceledException>(()=>LegacySpatialDirectionalFeatures.Extract(ownGlyph,new(40,40),minima,maxima,empty,0,0,new CancellationToken(true)));
Console.WriteLine("PASS own full spatial4f313 composition, shared global/baseline/context/boundary and differing local channels, malformed/cancellation. No handwriting accuracy assertion.");
if(args.Length==0)return;
if(args.Length is <3 or >4)throw new ArgumentException("Usage: LegacySpatialModel.Probe [acoustic.res activations.json spatial-calibration.json [temporal-calibration.json]]");
foreach(string path in args)if(new FileInfo(path).Length>8*1024*1024)throw new ArgumentException("Caller asset exceeds8MiB.");
byte[] acoustic=File.ReadAllBytes(args[0]);using var tables=JsonDocument.Parse(File.ReadAllBytes(args[1]));
int[] Integers(JsonElement element)=>element.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
var activations=new LegacyInkActivations(Integers(tables.RootElement.GetProperty("tanh").GetProperty("values")),Integers(tables.RootElement.GetProperty("sigmoid").GetProperty("values")));
(short[],short[]) Calibration(string path)
{
    using var document=JsonDocument.Parse(File.ReadAllBytes(path));
    return(document.RootElement.GetProperty("minima").EnumerateArray().Select(p=>p.GetInt16()).ToArray(),document.RootElement.GetProperty("maxima").EnumerateArray().Select(p=>p.GetInt16()).ToArray());
}
(minima,maxima)=Calibration(args[2]);
var network=LegacyInkNetwork.Load(acoustic.AsSpan(467101,566296-467101));Require(network.Kind==0x4f,"Caller model must match inspected4f profile.");
var alphabet=LegacyInkAlphabet.Load(acoustic.AsSpan(99),0x103);
var initial=new LegacyContextWindowScorer(LegacyInkNetwork.Load(acoustic.AsSpan(189755,195746-189755)),activations);
LegacyInkStroke[] wordInk=[new([new LegacyInkPoint(0,40),new(0,0),new(12,30),new(24,0),new(24,40)]),
    new([new LegacyInkPoint(40,40),new(52,0),new(64,40)]),new([new LegacyInkPoint(46,20),new(58,20)]),
    new([new LegacyInkPoint(80,40),new(80,0),new(92,30),new(104,0),new(104,40)]),
    new([new LegacyInkPoint(120,40),new(132,0),new(144,40)]),new([new LegacyInkPoint(126,20),new(138,20)])];
int[][] glyphIndices=[[0],[1,2],[3],[4,5]];
var accepted=initial.ScoreLine(wordInk,0,40,40,[0,0,0,0,0,0]);
LegacyInkNetwork? temporalNetwork=null;short[]? temporalMin=null,temporalMax=null;
if(args.Length==4){(temporalMin,temporalMax)=Calibration(args[3]);temporalNetwork=LegacyInkNetwork.Load(acoustic.AsSpan(367899,467094-367899));}
for(int target=0;target<glyphIndices.Length;target++)
{
    var glyph=glyphIndices[target].Select(index=>wordInk[index]).ToArray();
    var selected=LegacyPriorContextSelection.Select(accepted,0,glyphIndices[target]);
    float center=0;int points=0;foreach(var stroke in glyph)foreach(var point in stroke.Points){center+=point.X;points++;}center/=points;
    var context=LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(selected,center,40,0,1,1));
    foreach(bool useContext in new[]{false,true})
    {
        var supplied=useContext?context:empty;
        var input=LegacySpatialDirectionalFeatures.Extract(glyph,new(40,40),minima,maxima,supplied,1024,1024);
        Print("4f",target,useContext,LegacyClassScores.Convert(network.Evaluate(input,activations)).Probability32768.Span,alphabet);
        if(temporalNetwork is not null)
        {
            input=LegacyTemporalDirectionalFeatures.Extract(glyph,new(40,40),temporalMin!,temporalMax!,supplied,1024,1024);
            Print("3f",target,useContext,LegacyClassScores.Convert(temporalNetwork.Evaluate(input,activations)).Probability32768.Span,alphabet);
        }
    }
}
Console.WriteLine("Completed own printed МАМА family/context diagnostics. Caller line/baseline/calibration/boundaries are explicit; no ensemble or word/cursive accuracy claim.");
static void Print(string family,int target,bool context,ReadOnlySpan<int> scores,LegacyInkAlphabet alphabet)
{
    var owned=scores.ToArray();using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))
    {
        writer.WriteStartObject();writer.WriteString("family",family);writer.WriteNumber("targetGlyphIndex",target);writer.WriteBoolean("contextEnabled",context);
        writer.WriteStartArray("top");foreach(int index in Enumerable.Range(0,owned.Length).OrderByDescending(i=>owned[i]).ThenBy(i=>i).Take(5))
        {writer.WriteStartObject();writer.WriteNumber("class",index);if(alphabet.TryGetText((ushort)index,out string text))writer.WriteString("text",text);writer.WriteNumber("probability32768",owned[index]);writer.WriteEndObject();}
        writer.WriteEndArray();writer.WriteEndObject();
    }
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
}
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected{typeof(T).Name}.");}
