using System.Buffers.Binary;
using System.Text.Json;
using System.Security.Cryptography;
using InkRecognition;

if (args.Length is not (7 or 10 or 11)) throw new ArgumentException("AK LK engine separation case-data publication-data capture [line30 line50 physical-count | line30 line50 recorded-manifest word-index]");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var caseFile = File.OpenRead(args[4]);
using var publicationFile = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(caseFile);
var publication = LegacyNativeTextPublicationData.Load(publicationFile);
LegacyFreshCaseRussianDictionaryPreparation dictionary;
LegacyOwnedMainWordSpanPreparation owner;
LegacyFreshMainSingleLineContext? lineContext = null;
if (args.Length == 7)
{
    var sampled = MainFixture.Create(ak, engine, table);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
}
else if (args.Length == 10)
{
    var input = MainLineWordFixture.Create(ak, engine, File.ReadAllBytes(args[3]),
        File.ReadAllBytes(args[7]), File.ReadAllBytes(args[8]), cases, publication, int.Parse(args[9]));
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
    lineContext = input.Context;
}
else
{
    using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[9]));
    if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
        manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent")
        throw new InvalidDataException("Explicitly unsegmented pinned recorded coordinates required.");
    var sample = manifest.RootElement.GetProperty("samples").EnumerateArray()
        .Where(s => s.GetProperty("kind").GetString() == "word").ElementAt(int.Parse(args[10]));
    string name = sample.GetProperty("file").GetString()!;
    if (name != Path.GetFileName(name)) throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[9]))!, name));
    if (bytes.Length > 200000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != sample.GetProperty("trackSha256").GetString())
        throw new InvalidDataException("Recorded track hash changed.");
    using var parsed = JsonDocument.Parse(bytes);
    var points = parsed.RootElement.GetProperty("points").EnumerateArray()
        .Select(p => new InkPoint(p[0].GetSingle(), -p[1].GetSingle())).ToArray();
    if (points.Length is < 2 or > 4096) throw new InvalidDataException("Bounded recorded point vector required.");
    //Pen-up is absent in this dataset. One unsegmented path is an explicit
    //diagnostic input assumption; expected transcription is never read.
    var input = TrackedCaseInput.Create(ak, engine, table, File.ReadAllBytes(args[7]), File.ReadAllBytes(args[8]),
        [new InkStroke(points)], cases, publication);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, publication);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
    lineContext = input.Context;
}
var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
ulong identity = 0x1000;
var result = new LegacyOwnedMainCaseWordPass(ak, policy,
    lineContext is null ? null : owner.MainLineAcoustic, table, request =>
{
    byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, ++identity);
}).RunOwned();
var invocation = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy);
var terminal = invocation.SelectOwnedPass(result);
if (!ReferenceEquals(invocation.DictionaryOwner, dictionary) || invocation.LanguagePenalty2534 != 17153 ||
    invocation.Options.Filter4 || invocation.Options.ContextSentinels24b8 || invocation.NativeRescueGateActive)
    throw new InvalidDataException("Actual fresh case source penalty or invocation gates changed.");

//Independent native2257C0 scan for this actual Filter4=false/no-sentinel route:
//primary/flag4 rows cannot win. No source label or expected text is supplied.
var final = result.GetNodeSnapshot(result.NodeCount - 1);
int best = 0x4000000, expectedOrdinal = -1;
for (int ordinal = 0; ordinal < final.SourceEnd23f0; ordinal++)
{
    var row = final.Records.Span.Slice(ordinal * 88, 88);
    if (ordinal < controller.Configuration.PrimaryArcLimit230a || (row[5] & 4) != 0) continue;
    int cost = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
    if (cost < best) { best = cost; expectedOrdinal = ordinal; }
}
if (result.NodeCount <= controller.MaximumParentDistance54 &&
    (expectedOrdinal < 0 || !terminal.Winner.HasWinner || terminal.Winner.Ordinal != expectedOrdinal ||
    terminal.Winner.RelativeCost != best || terminal.Winner.TotalCost != unchecked(best + final.CumulativePathCost2520) ||
    !terminal.CopySelectedRecord().AsSpan().SequenceEqual(final.Records.Span.Slice(expectedOrdinal * 88, 88))))
    throw new InvalidDataException("Actual case owned rows and literal2257C0 numeric selection disagree.");
var sources = invocation.CopySources(); sources[1] = default;
if (invocation.CopySources()[1].Header != dictionary.Descriptor.Flags ||
    invocation.CopySources()[1].LanguagePenalty2534 != 17153)
    throw new InvalidDataException("Case selected source array leaked mutable storage.");
int guards = 0;
Reject(() => LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(null!));
Reject(() => LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy, new(true)));
Reject(() => invocation.SelectOwnedPass(null!));
Reject(() => invocation.SelectOwnedPass(result, new(true)));
var equivalentPolicy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
Reject(() => LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(equivalentPolicy).SelectOwnedPass(result));
var confidence = lineContext is null ? LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary) :
    LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary, lineContext);
//Explicit test budget, not an inferred native allocator/ring growth policy.
byte[] residue = Enumerable.Repeat((byte)0xa7, Math.Max(4096, result.NodeCount * 512) * 40).ToArray();
Reject(() => LegacyOwnedMainCompactWordReachability.Prepare(ak, terminal));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, residue));
Reject(() => LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, null!, terminal, confidence, residue));
Reject(() => LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, confidence, residue, new(true)));
var foreignDictionary = lineContext is null
    ? LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, owner.Source, cases, publication)
    : LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, owner.MainLineSource, cases, publication);
var foreignConfidence = lineContext is null
    ? LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, foreignDictionary)
    : LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, foreignDictionary, lineContext);
Reject(() => LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, foreignConfidence, residue));
if (result.NodeCount > controller.MaximumParentDistance54)
{
    try
    {
        _ = LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, confidence, residue);
        throw new InvalidDataException("Missing intermediate-publication lifecycle was silently accepted.");
    }
    catch (NotSupportedException error) when (error.Message.Contains("intermediate-publication boundary", StringComparison.Ordinal))
    {
        using var refusedFile = File.Create(args[6]);
        using var refusedWriter = new Utf8JsonWriter(refusedFile);
        refusedWriter.WriteStartObject(); refusedWriter.WriteNumber("nodeCount", result.NodeCount);
        refusedWriter.WriteNumber("intermediatePublicationDistance", controller.MaximumParentDistance54);
        refusedWriter.WriteString("stage", "prepared pass refused before intermediate-publication lifecycle");
        refusedWriter.WriteString("refusal", error.Message); refusedWriter.WriteBoolean("noTextClaim", true);
        refusedWriter.WriteEndObject(); refusedWriter.Flush();
        Console.WriteLine($"PASS explicit native intermediate-publication refusal for actual CASE {result.NodeCount} nodes/bound{controller.MaximumParentDistance54}; no filtered query or fabricated compact result.");
        return;
    }
}
var compact = LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, confidence, residue);
if (!ReferenceEquals(compact.CaseSourcePolicy, policy))
    throw new InvalidDataException("Published CASE records lost their exact typed factory policy.");
Reject(() => LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, confidence, residue));
Console.WriteLine($"CASE COMPACT {compact.RecordCount} records/index{compact.SelectedRecordIndex}, starts[{string.Join(',', compact.CopyNodeStarts2478())}], queries{compact.Reachability.QueryChecks}/{compact.Reachability.BackendCalls}.");
using var file = File.Create(args[6]);
using var writer = new Utf8JsonWriter(file);
writer.WriteStartObject(); writer.WriteNumber("nodeCount", result.NodeCount);
writer.WriteNumber("factory", 0x292); writer.WriteNumber("maximumAtoms", owner.SourceGraph.MaximumAtoms);
writer.WriteNumber("intermediatePublicationDistance", controller.MaximumParentDistance54);
writer.WriteNumber("winner", terminal.Winner.Ordinal); writer.WriteNumber("relative", terminal.Winner.RelativeCost);
writer.WriteNumber("total", terminal.Winner.TotalCost); writer.WriteNumber("cumulative", final.CumulativePathCost2520);
writer.WriteNumber("sourceEnd", final.SourceEnd23f0); writer.WriteNumber("primary", controller.Configuration.PrimaryArcLimit230a);
writer.WriteBase64String("finalRows", final.Records.Span); writer.WriteBase64String("selectedRow", terminal.CopySelectedRecord());
writer.WriteNumber("count", compact.RecordCount); writer.WriteNumber("selected", compact.SelectedRecordIndex);
writer.WriteBase64String("compact", compact.CopyCompactStorage());
writer.WriteStartArray("starts"); foreach (int value in compact.CopyNodeStarts2478()) writer.WriteNumberValue(value); writer.WriteEndArray();
writer.WriteNumber("penalty", invocation.LanguagePenalty2534);
writer.WriteStartArray("primaryMap"); foreach (ushort value in dictionary.Symbols.Symbols.CopyPrimary()) writer.WriteNumberValue(value); writer.WriteEndArray();
writer.WriteStartArray("inputNodes");
for (int node = 0; node < result.NodeCount; node++)
{
    var row = result.GetNodeSnapshot(node); writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
    writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteStartArray("nodes");
for (int node = 0; node < result.NodeCount; node++)
{
    var row = compact.GetNodeSnapshot(node); writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
    writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteStartArray("graph");
for (int node = 0; node < result.NodeCount; node++)
{
    writer.WriteStartArray(); foreach (var arc in owner.SourceGraph.GetNode(node))
    { writer.WriteStartArray(); writer.WriteNumberValue(arc.GroupId); writer.WriteNumberValue(arc.Span); writer.WriteNumberValue(arc.EndContextId); writer.WriteEndArray(); }
    writer.WriteEndArray();
}
writer.WriteEndArray(); writer.WriteStartArray("contexts");
foreach (var context in owner.Contexts.CopyNodes())
{ writer.WriteStartArray(); writer.WriteNumberValue(context.Identity); writer.WriteNumberValue(context.ParentLink); writer.WriteNumberValue(context.Symbol); writer.WriteEndArray(); }
writer.WriteEndArray(); writer.WriteStartArray("scores");
for (int group = 0; group < owner.SourceGraph.Groups.Count; group++)
{
    var score = confidence.ScoreGroup(group);
    var acoustic = lineContext is null ? owner.Acoustic.Score(group) : owner.MainLineAcoustic.Score(group);
    writer.WriteStartObject(); writer.WriteStartArray("candidates");
    foreach (var candidate in score.Candidates.Span)
    { writer.WriteStartArray(); writer.WriteNumberValue(candidate.Symbol); writer.WriteNumberValue(candidate.Cost); writer.WriteEndArray(); }
    writer.WriteEndArray(); writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
    writer.WriteStartArray("costs"); foreach (int value in acoustic.Cache.Costs.Span) writer.WriteNumberValue(value); writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray();
writer.WriteNumber("guards", guards); writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"PASS actual CASE owned terminal {result.NodeCount} nodes/{final.SourceEnd23f0} source rows, ordinal{expectedOrdinal}, cost{terminal.Winner.TotalCost}, {guards} guards; no text result.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Invalid case terminal ownership accepted.");
}
