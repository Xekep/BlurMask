using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] ak = File.ReadAllBytes(args[1]);
var categories = LegacyCursiveCategoryProfile.Load(ak.AsSpan(188222), 188222, out _);
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 55);
    var request = LegacyNativeWordParentRequestFactory.Create(test.GetProperty("current").GetUInt32(), test.GetProperty("span").GetUInt32(),
        test.GetProperty("previous").GetInt32(), Hex(test, "parent"), test.GetProperty("arc").GetUInt16(),
        test.GetProperty("symbol").GetUInt16(), test.GetProperty("category").GetInt32(), test.GetProperty("tag").GetUInt64(),
        test.GetProperty("length").GetByte(), test.GetProperty("flags").GetByte(), test.GetProperty("header").GetUInt32(),
        test.GetProperty("priorHeader").GetUInt32(), test.GetProperty("permission").GetByte(), categories, cell,
        test.GetProperty("queryCost").GetInt32(), Hex(test, "residue"));
    Require(request.GetSnapshot().Span.SequenceEqual(Hex(test, "expected")) && cell.Value == test.GetProperty("expectedCost").GetInt32(),
        "Independent full80 parent request/cost differs.");
    cases++;
}
Console.WriteLine($"PASS {cases} independent201270 parent88→request80 controls; source/ending separation, zero FirstIds, inherited accumulator and signed wrap verified.");
int guards = 0;
byte[] parentGuard = new byte[88], residueGuard = new byte[80];
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Malformed native parent request accepted.");
}
LegacyNativeWordRecombinationRequest CreateGuard(byte[] parent, byte[] residue, int previous = 0, int category = 0,
    CancellationToken token = default) => LegacyNativeWordParentRequestFactory.Create(1, 0, previous, parent, 0, 0, category,
        0, 0, 0, 0, 0, 0, categories, new(0x1122334455667788, 0), 0, residue, token);
Reject(() => CreateGuard(new byte[87], residueGuard));
Reject(() => CreateGuard(parentGuard, new byte[79]));
Reject(() => CreateGuard(parentGuard, residueGuard, previous: -1));
Reject(() => CreateGuard(parentGuard, residueGuard, category: categories.CompositeCategories.Length));
var badCategory = parentGuard.ToArray();
badCategory[0x1a] = 63;
Reject(() => CreateGuard(badCategory, residueGuard));
Reject(() => CreateGuard(parentGuard, residueGuard, token: new CancellationToken(true)));
Console.WriteLine($"PASS {guards} request extent/category/cancellation guards.");
ActualParentCandidates.Run(args);

static byte[] Hex(JsonElement test, string key) => Convert.FromHexString(test.GetProperty(key).GetString()!);
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
