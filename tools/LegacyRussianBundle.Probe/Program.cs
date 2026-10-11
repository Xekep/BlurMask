using System.Text.Json;
using System.Text.Json.Nodes;
using InkRecognition;

void Reject(Action operation)
{
    try { operation(); throw new InvalidOperationException("Invalid model input accepted."); }
    catch (InvalidDataException) { }
}
Reject(() => LegacyRussianModelBundle.Load([], [], []));
try { LegacyRussianModelBundle.Load([], [], [], new CancellationToken(true)); throw new InvalidOperationException("Cancellation ignored."); }
catch (OperationCanceledException) { }
Console.WriteLine("PASS unsupported resource bounds and cancellation before model parsing.");
if (args.Length == 0) return;
if (args.Length != 3) throw new ArgumentException("Usage: LegacyRussianBundle.Probe acoustic.res lexical.res engine-profile.json");
foreach (var path in args) if (new FileInfo(path).Length > 8 * 1024 * 1024) throw new ArgumentException("Asset outside bounds.");
var acoustic = File.ReadAllBytes(args[0]); var lexical = File.ReadAllBytes(args[1]); var profile = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(acoustic, lexical, profile);
var recognizer = model.CreateWordRecognizer(relatedFormMode: 0);
if (model.Lexicon.StoredTerminalCount != 439826 || model.WordIndex.WordCount != model.LanguageModel.WordCount ||
    model.LanguageProfiles.Count != 4) throw new InvalidOperationException("Stored model components differ.");
ushort[] mir = "\u043c\u0438\u0440".Select(letter =>
    model.LexicalAlphabet.TryGetLabel(letter.ToString(), out ushort id) ? id : throw new InvalidOperationException("Missing label.")).ToArray();
if (!model.WordIndex.TryGetWordId(mir, out int wordId) || wordId != 198893 ||
    model.LanguageModel.GetStoredWordCost(wordId) != 9851 || model.LanguageModel.GetWordClass(wordId) != 435)
    throw new InvalidOperationException("Independent stored dictionary/language correspondence failed.");
if (model.SourceKind != 0x135 || model.ResourceLanguageProfileIndex != 2)
    throw new InvalidOperationException("Stored acoustic source profile forwarding differs.");
var ordinary=model.CreateResourceLanguageScorer(runtimeScale:1).Score(wordId,[],0,false,true,false);
var explicitProfile=model.CreateLanguageScorer(2,1).Score(wordId,[],0,false,true,false);
if (ordinary != explicitProfile || ordinary.WordCost != -9096)
    throw new InvalidOperationException("Stored source kind did not select the verified lexical profile.");
var integerPositions=mir.Select(label=>(ReadOnlyMemory<LegacyIntegerLexicalCandidate>)new LegacyIntegerLexicalCandidate[]{new(label,1)}).ToArray();
var integerWord=model.CreateIntegerWordDecoder(1).Decode(integerPositions,[],0,false,true,false,7,100000,false,64).Single();
if (integerWord.WordId!=198893 || integerWord.AcousticCost!=3 || integerWord.Score.Language.LexicalSubtotal!=4537 || integerWord.Score.TotalCost!=4547)
    throw new InvalidOperationException("Portable bundle integer terminal pipeline differs from independent stored costs.");

// Each malformed profile is validated against the same unchanged real resource pair.
Reject(() => LegacyRussianModelBundle.Load(acoustic, lexical, "{"u8));
foreach (Action<JsonObject> mutation in new Action<JsonObject>[] {
    root => root["schemaVersion"] = 2,
    root => root["acousticSha256"] = new string('0',64),
    root => root["slopes"]![0]![0] = 0,
    root => root["temporal"]!["minima"]![0] = 32768,
    root => root["spatial"]!["minima"]![0] = 32767,
    root => root["activations"]!["hidden"] = new JsonArray(0) })
{
    var changed = JsonNode.Parse(profile)!.AsObject(); mutation(changed);
    using var data = new MemoryStream();
    using (var writer = new Utf8JsonWriter(data)) changed.WriteTo(writer);
    Reject(() => LegacyRussianModelBundle.Load(acoustic, lexical, data.ToArray()));
}
// Caller buffers are not retained, including source weights and calibration JSON.
acoustic[200000] ^= 1;
Reject(() => LegacyRussianModelBundle.Load(acoustic, lexical, profile));
Array.Fill(acoustic, (byte)0); Array.Fill(lexical, (byte)0); Array.Fill(profile, (byte)0);
Console.WriteLine("PASS actual resource integrity, stored word identities, malformed JSON/profiles and caller buffer ownership.");
foreach (bool allFour in new[] { false, true })
foreach (var sample in Samples())
{
    var result = recognizer.Recognize(sample.Strokes, new(sample.Bottom, sample.Height), includeBaselineFamilies: allFour);
    if (result.GlyphCount != sample.Glyphs || result.ContextWindowCount == 0 || result.Words.Count == 0)
        throw new InvalidOperationException("Portable model path failed after caller buffer mutation.");
    using var stream = new MemoryStream();
    using (var writer = new Utf8JsonWriter(stream))
    {
        writer.WriteStartObject(); writer.WriteString("sample", sample.Name); writer.WriteNumber("scale", sample.Scale);
        writer.WriteBoolean("allFour", allFour); writer.WriteNumber("glyphs", result.GlyphCount);
        writer.WriteNumber("expectedRank", result.Words.ToList().FindIndex(word => word.Text == sample.Expected) + 1);
        writer.WriteStartArray("matches");
        foreach (var word in result.Words.Take(5))
        { writer.WriteStartObject(); writer.WriteString("text", word.Text); writer.WriteNumber("cost", word.LogScore); writer.WriteEndObject(); }
        writer.WriteEndArray(); writer.WriteEndObject();
    }
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(stream.ToArray()));
}
Console.WriteLine("PASS16 portable resource/profile word diagnostic paths. This is not general Russian or joined-handwriting accuracy.");

static IEnumerable<Sample> Samples()
{
    LegacyInkStroke[] Letter(params short[][][] strokes) => strokes.Select(stroke =>
        new LegacyInkStroke(stroke.Select(p => new LegacyInkPoint(p[0], p[1])).ToArray())).ToArray();
    var m = Letter([[0,90],[0,10],[25,60],[50,10],[50,90]]);
    var a = Letter([[0,90],[25,10],[50,90]], [[10,60],[40,60]]);
    var i = Letter([[0,10],[0,90],[50,10],[50,90]]);
    var r = Letter([[0,90],[0,10],[40,10],[50,20],[50,40],[40,50],[0,50]]);
    foreach (var (name, expected, glyphs) in new (string,string,LegacyInkStroke[][])[] {
        ("MAMA", "\u043c\u0430\u043c\u0430", [m,a,m,a]), ("MIR", "\u043c\u0438\u0440", [m,i,r]) })
    foreach (var (scale,x,y) in new[] { (1,0,0), (1,-100,300), (2,0,0), (3,200,-100) })
        yield return new(name, expected, scale, checked((short)(90*scale+y)), checked((short)(80*scale)), glyphs.Length,
            glyphs.SelectMany((glyph,index) => glyph.Select(stroke => new LegacyInkStroke(stroke.Points.ToArray().Select(point =>
                new LegacyInkPoint(checked((short)((point.X+index*70)*scale+x)), checked((short)(point.Y*scale+y)))).ToArray()))).ToArray());
}
sealed record Sample(string Name, string Expected, int Scale, short Bottom, short Height, int Glyphs, LegacyInkStroke[] Strokes);
