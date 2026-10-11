using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if (args.Length != 2)
    throw new ArgumentException("controls.json acoustic.res");

using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int checkedPrefixes = 0;
LegacyInitialWordRecordFields? firstFields = null;

// Compare every initialized byte against independent bit-field ownership.
foreach (var test in controls.RootElement.EnumerateArray())
{
    var fields = JsonSerializer.Deserialize(test.GetProperty("fields").GetRawText(),
        ProbeJsonContext.Default.LegacyInitialWordRecordFields)!;
    var expected = Convert.FromBase64String(test.GetProperty("expected").GetString()!);
    var actual = LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields,
        test.GetProperty("scale").GetSingle());
    Require(actual.AsSpan().SequenceEqual(expected), "Native initial prefix bit ownership differs.");
    var decoded = LegacyPreparedWordPathReference.Read(actual, fields.SourceNode18);
    ushort expectedCategory = (ushort)((fields.CompositeCategory & 63) |
        (fields.PackedSourceArcIndex & 1023) << 6);
    Require(decoded.CategoryBits == expectedCategory && decoded.SourceSpan == fields.SourceSpan,
        "Initialized prefix does not round-trip through the native reference reader.");
    firstFields ??= fields;
    checkedPrefixes++;
}

// Use the actual resource's first startup map. No synthetic previous word or
// winning source arc is chosen: the return from source.virtualF8 is explicit.
var resource = File.ReadAllBytes(args[1]);
var profile = LegacyCursiveCategoryProfile.Load(resource.AsSpan(188222), 188222, out _);
var categories = new LegacyCursiveCategoryMapper(profile);
var template = firstFields! with { RequestedSecondaryFlag = 1 };
int emitted = 0, missing = 0, boundaryCorrections = 0;

// Primary ordinary graph ->fresh input descriptor ->native boundary callback.
var raw = Enumerable.Range(0, 8).Select(index => new InkStroke(new[]
{
    new InkPoint(index * 100, 10),
    new InkPoint(index * 100 + 20, 90)
})).ToArray();
var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource);
var source = LegacyOrdinaryCursiveCutSource.Generate(raw, sourceProfile, 90, 80);
var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource, 189379);
var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, 80, seedProfile,
    new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
var seedGraph = LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph, 8);
var sourceBoundaries = LegacyPreparedSourceBoundarySnapshot.CreateFreshOrdinary(source, graph);

for (int symbol = 0; symbol < categories.SymbolEntryCount; symbol++)
{
    int expectedCategory = profile.StartCompositeMap.Span[symbol];
    ushort returnedArc = (ushort)(symbol * 347);
    uint sourceSpan = symbol % 2 == 0 ? 7u : (uint)(symbol * 571);
    bool created = LegacyInitialWordRecordProducer.TryCreateOrdinaryStartupPrefix(template,
        categories, symbol, 7, returnedArc, sourceSpan, 0, out var prefix);
    Require(created == (expectedCategory >= 0), "Actual startup symbol map missing-entry behavior differs.");
    if (!created)
    {
        Require(prefix.Length == 0, "Missing startup category emitted an invented native record.");
        missing++;
        continue;
    }

    var decoded = LegacyPreparedWordPathReference.Read(prefix, 7);
    Require((decoded.CategoryBits & 63) == expectedCategory &&
        (decoded.CategoryBits >> 6) == (returnedArc & 1023) &&
        decoded.SourceSpan == unchecked((sbyte)(byte)sourceSpan),
        "Native startup map/span/arc packing differs.");
    Require(BinaryPrimitives.ReadUInt16LittleEndian(prefix.AsSpan(0x1c)) == returnedArc &&
        BinaryPrimitives.ReadUInt16LittleEndian(prefix.AsSpan(0x1e)) == ushort.MaxValue &&
        BinaryPrimitives.ReadInt32LittleEndian(prefix.AsSpan(0x38)) == 7 &&
        BinaryPrimitives.ReadUInt16LittleEndian(prefix.AsSpan(0x3c)) == ushort.MaxValue &&
        (prefix[0x27] & 1) == 0,
        "Startup source reference fields or fresh secondary-scale gate differ.");

    // Feed the produced category/span to the previously restored204290 block.
    var projection = LegacyWordPathBoundaryCost.Evaluate(decoded, categories, graph.NodeCount,
        false, 8, sourceBoundaries.GetBoundary);
    int start = 7 - unchecked((sbyte)(byte)sourceSpan);
    int expectedPenalty = profile.CompositeCategories.Span[expectedCategory].FirstId is 2 or 4 &&
        start == 0 ? 8192 : 0;
    Require(projection.AddedCost == expectedPenalty, "Startup producer/category-boundary consumer differs.");
    boundaryCorrections += projection.AddedCost != 0 ? 1 : 0;
    emitted++;
}

// Guards are separate from native heap selection and allocation policy.
try
{
    LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(template, float.NaN);
    throw new Exception("Unsupported secondary scale accepted.");
}
catch (ArgumentException) { }
try
{
    LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(template, 0, new CancellationToken(true));
    throw new Exception("Cancellation ignored.");
}
catch (OperationCanceledException) { }
try
{
    LegacyInitialWordRecordProducer.TryCreateOrdinaryStartupPrefix(template, categories,
        categories.SymbolEntryCount, 0, 0, 0, 0, out _);
    throw new Exception("Unavailable startup symbol accepted.");
}
catch (ArgumentOutOfRangeException) { }

Console.WriteLine($"PASS {checkedPrefixes} independent complete0x58 initial-prefix controls, " +
    $"actual RU startup map {emitted} emitted/{missing} missing entries, " +
    $"{boundaryCorrections} source-boundary corrections, signed-span/packed-arc round-trip " +
    "and explicit scale/bounds/cancel guards. Heap allocation, returned-source-arc production, " +
    "previous-reference selection and winner/confidence remain separate.");

static void Require(bool value, string message)
{
    if (!value)
        throw new InvalidDataException(message);
}

[JsonSerializable(typeof(LegacyInitialWordRecordFields))]
internal partial class ProbeJsonContext : JsonSerializerContext;
