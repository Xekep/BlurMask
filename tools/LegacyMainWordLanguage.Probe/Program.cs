using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
var model = LegacyWordLanguageModel.Load(OwnWordFixture.Create());
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int records = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    float wordSlope = test.GetProperty("wordSlope").GetSingle(), wordOffset = test.GetProperty("wordOffset").GetSingle();
    var profile = new LegacyWordLanguageCostProfile(wordSlope, wordOffset, wordSlope,
        test.GetProperty("unigramOffset").GetSingle(), test.GetProperty("bigramSlope").GetSingle(), test.GetProperty("bigramOffset").GetSingle());
    var auxiliary = new LegacyWordAuxiliaryProfile(test.GetProperty("flags").GetUInt16(), wordOffset,
        test.GetProperty("boundaryOffset").GetSingle(), test.GetProperty("categoryOffset").GetSingle(),
        test.GetProperty("categorySlope").GetSingle(), test.GetProperty("transitions").EnumerateArray().Select(x => x.GetSingle()).ToArray());
    var evaluator = new LegacyNativeKnownWordLanguageRecord(model, profile, auxiliary, test.GetProperty("scale").GetSingle());
    byte[] token = new byte[16];
    BinaryPrimitives.WriteInt32LittleEndian(token.AsSpan(4), test.GetProperty("wordId").GetInt32());
    token[12] = test.GetProperty("includeBoundary").GetBoolean() ? (byte)1 : (byte)0;
    token[13] = test.GetProperty("suppress").GetBoolean() ? (byte)1 : (byte)0;
    var result = evaluator.Evaluate(token, test.GetProperty("history").EnumerateArray().Select(x => x.GetUInt32()).ToArray(),
        test.GetProperty("category").GetInt32(), test.GetProperty("clear").GetBoolean(), test.GetProperty("lexicalLimit").GetInt32(),
        Hex(test, "residue"));
    Require(result.Score.WordCost == test.GetProperty("wordCost").GetInt32() &&
        result.Score.ClassCost == test.GetProperty("classCost").GetInt32() &&
        result.Score.CategoryCost == test.GetProperty("categoryCost").GetInt32() &&
        result.Score.BoundaryCost == test.GetProperty("boundaryCost").GetInt32() &&
        result.Score.LexicalSubtotal == test.GetProperty("subtotal").GetInt32(), "Independent actual flag-bound language fields differ.");
    bool accepted = test.GetProperty("accepted").GetBoolean();
    Require(result.Accepted == accepted && (accepted ? result.Record40.Span.SequenceEqual(Hex(test, "expected")) : result.Record40.IsEmpty),
        "Independent full40 history/residue/state or lexical budget differs.");
    records++;
}
Console.WriteLine($"PASS {records} independent numerical/full40 language outputs; source42 versus knowledgeF5 flags, history category, payload residue and budget verified.");

int injections = 0;
foreach (var test in document.RootElement.GetProperty("injections").EnumerateArray())
{
    var result = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(Hex(test, "parent"), Hex(test, "residue"));
    bool accepted = test.GetProperty("accepted").GetBoolean();
    Require(result.HasValue == accepted && (!accepted || result!.Value.Span.SequenceEqual(Hex(test, "expected"))),
        "Independent current-terminal injection/gate/extra residue differs.");
    injections++;
}
Console.WriteLine($"PASS {injections} independent current-terminal query-cache injections.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or OverflowException)
    { guards++; return; }
    throw new InvalidDataException("Malformed main language record accepted.");
}
var guardAux = new LegacyWordAuxiliaryProfile(0, 0, 0, 0, 0, new float[169]);
var guardService = new LegacyNativeKnownWordLanguageRecord(model, new(1, 0, 1, 0, 1, 0), guardAux, 1);
Reject(() => guardService.Evaluate(new byte[15], [], 0, false, 0x4000000, new byte[40]));
Reject(() => guardService.Evaluate(new byte[16], [], 0, false, 0x4000000, new byte[39]));
Reject(() => guardService.Evaluate(new byte[16], new uint[2], 0, false, 0x4000000, new byte[40]));
Reject(() => guardService.Evaluate(new byte[16], [], 13, false, 0x4000000, new byte[40]));
Reject(() => guardService.Evaluate(new byte[16], [], 0, false, 0x4000000, new byte[40], new CancellationToken(true)));
var invalidWord = new byte[16];
BinaryPrimitives.WriteInt32LittleEndian(invalidWord.AsSpan(4), 41);
Reject(() => guardService.Evaluate(invalidWord, [], 0, false, 0x4000000, new byte[40]));
Reject(() => LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(new byte[87], new byte[32]));
Reject(() => LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(new byte[88], new byte[31]));
Require(guardService.Evaluate(new byte[16], [], 0, false, int.MaxValue, new byte[40]).Accepted,
    "Valid widened lexical budget for a negative prior path was incorrectly limited to impossible sentinel.");
Console.WriteLine($"PASS {guards} language record/injection extent, category, history, token and cancellation guards.");
ActualMainWordLanguage.Run(args);

static byte[] Hex(JsonElement test, string key) => Convert.FromHexString(test.GetProperty(key).GetString()!);
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
