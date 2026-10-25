using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if (args.Length != 4 || args[0] is not ("--validate-plan" or "--actual"))
    throw new ArgumentException("--validate-plan|--actual RESOURCE ORIGINAL14_FIXTURES FRESH_CAPTURE");
if (File.Exists(args[3])) throw new IOException("Fresh capture required.");
const string resourceSHA = "8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
const string fixtureSHA = "ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb";
const int maximumCaptureBytes = 262144;
string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
if (new FileInfo(args[1]).Length != 3685881 || new FileInfo(args[2]).Length != 4242)
    throw new InvalidDataException("Original input extents required before read.");
byte[] raw = File.ReadAllBytes(args[1]), fixtureRaw = File.ReadAllBytes(args[2]);
if (Hash(raw) != resourceSHA || Hash(fixtureRaw) != fixtureSHA)
    throw new InvalidDataException("Original input hash differs.");
using var fixture = JsonDocument.Parse(fixtureRaw);
var cases = fixture.RootElement.GetProperty("cases");
if (cases.GetArrayLength() != 14 || cases.EnumerateArray().Select(c => c.GetProperty("name").GetString()).Distinct().Count() != 14)
    throw new InvalidDataException("Ordered original14 fixtures required.");
if (args[0] == "--validate-plan")
{
    Console.WriteLine(JsonSerializer.Serialize(new { mode = "validate-plan", resourceSHA, fixtureSHA, cases = 14, profileCalls = 0, owningSourceCalls = 0, normalizationCalls = 0 }));
    return;
}
Vector2[][] Original(JsonElement c)
{
    if (c.TryGetProperty("input", out var source))
        return source.EnumerateArray().Select(row => row.EnumerateArray().Select(p => new Vector2(p[0].GetSingle(), p[1].GetSingle())).ToArray()).ToArray();
    var recipe = c.GetProperty("recipe");
    int n = recipe.GetProperty("count").GetInt32();
    float step = recipe.GetProperty("step").GetSingle();
    bool horizontal = recipe.GetProperty("kind").GetString() == "horizontal";
    float[] pattern = [0f, .001f, 0f, -.001f];
    return [Enumerable.Range(0, n).Select(i => new Vector2(i * step, horizontal ? 0f : pattern[i % 4])).ToArray()];
}
var profile = LegacyMathAkFeatureProfile.PrepareActual(raw);
Array.Fill(raw, (byte)0xA5);
var rows = new List<object>(14);
int owningCalls = 0, complete = 0, earlier = 0;
foreach (var c in cases.EnumerateArray())
{
    string name = c.GetProperty("name").GetString()!;
    var original = Original(c);
    var source = profile.CreateGeometry(original);
    owningCalls++;
    try
    {
        LegacyMathAkOriginalInputContext.PrepareFreshFirstNormalizedBoundary(source);
        throw new InvalidOperationException("Unported AL boundary unexpectedly returned.");
    }
    catch (LegacyMathAkOriginalInputContext.First276NormalizedBoundaryException e)
    {
        var d = e.Prefix;
        var n = e.Normalized;
        if (d.EndNode != 0 || d.SuffixCount != 1 || d.SelectedCount != 1 || d.RangeCount != 1 ||
            d.CacheEntries != 1 || d.Descriptor108Count != 1 || d.InitializedInputCount != 276 ||
            !d.HasInitializedFeatures || d.HasEvaluatorOutput || d.InitializedInput is not null ||
            n.NativeC0 < 0 ||
            (n.HasNormalizedOutputs && (n.ZeroMaximumNoOp || n.NativeC4 is null || n.Quantized90 is not { Length: 257 } || n.ProbabilityA0 is not { Length: 257 })) ||
            (!n.HasNormalizedOutputs && (!n.ZeroMaximumNoOp || n.NativeC4 is not null || n.Quantized90 is not null || n.ProbabilityA0 is not null)))
            throw new InvalidDataException("Normalized knownness or same-owner prefix differs.");
        foreach (var stroke in original) Array.Fill(stroke, new Vector2(float.NaN, float.NaN));
        complete++;
        rows.Add(new { name, outcome = "normalized-missing-shortlist", exceptionType = e.GetType().FullName, message = e.Message, data = d, normalized = n });
    }
    catch (NotSupportedException e) when (e.GetType() == typeof(NotSupportedException))
    {
        if (e.Message.Length > 1024) throw new InvalidDataException("Refusal text cap exceeded.");
        earlier++;
        rows.Add(new { name, outcome = "earlier-refusal-no-input", exceptionType = e.GetType().FullName, message = e.Message, data = (object?)null, normalized = (object?)null });
    }
}
if (owningCalls != 14 || complete + earlier != 14) throw new InvalidDataException("Original cardinality differs.");
var result = new { schema = "first276-normalization14-partial-v1", resourceSHA, fixtureSHA, cases = rows,
    successfulProfiles = 1, owningSourceCalls = owningCalls, normalizationBoundaries = complete, earlierRefusals = earlier,
    fullEvaluatorSuccesses = 0, shortlistSuccesses = 0, physicalPaddingExported = false, liveMultiRangeCoverage = false,
    maximumCaptureBytes, capMeaning = "serialized UTF8 extent; not allocator or RSS bound" };
byte[] output = JsonSerializer.SerializeToUtf8Bytes(result, new JsonSerializerOptions { WriteIndented = true });
if (output.Length > maximumCaptureBytes) throw new NotSupportedException("Diagnostic UTF8 cap exceeded.");
using (var file = new FileStream(args[3], FileMode.CreateNew, FileAccess.Write, FileShare.None)) file.Write(output);
Console.WriteLine($"CAPTURE14 normalization-boundary={complete} earlier-refusal={earlier} shortlist-success=0");
