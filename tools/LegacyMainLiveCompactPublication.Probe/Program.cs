using System.Text.Json;
using InkRecognition;

if (args.Length != 10)
    throw new ArgumentException("AK LK engine separation case-data publication-data line30 line50 physical-count capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var caseFile = File.OpenRead(args[4]);
using var publicationFile = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(caseFile);
var publication = LegacyNativeTextPublicationData.Load(publicationFile);
var input = MainLineWordFixture.Create(ak, engine, File.ReadAllBytes(args[3]),
    File.ReadAllBytes(args[6]), File.ReadAllBytes(args[7]), cases, publication, int.Parse(args[8]));
var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, publication);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary, input.Context);
ulong identity = 0x1000;
var pass = new LegacyOwnedMainCaseWordPass(ak, policy, owner.MainLineAcoustic, table, request =>
{
    byte[] bytes = new byte[request.RequiredBytes];
    request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, ++identity);
});
byte[] residue = Enumerable.Repeat((byte)0xa7, owner.SourceGraph.NodeCount * 1024 * 40).ToArray();
LegacyMainIntermediateCompactPublication? observed = null;
bool nativeGate = int.Parse(args[8]) != 1;
void Observe(LegacyMainIntermediateCompactPublication value)
{
    if (observed is not null) throw new InvalidDataException("This fixture expects exactly one actual native gate.");
    observed = value;
}
var result = nativeGate
    ? pass.RunOwnedWithIntermediatePublication(ak, confidence, residue, Observe)
    : new LegacyOwnedMainCaseWordPassResult(policy,
        new LegacyOwnedMainDirectWordPass(ak, controller, owner.MainLineAcoustic, table, request =>
        {
            byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
            return new(bytes, ++identity);
        }, policy, default).RunOwnedWithPreparedPublicationAtNode(ak, confidence, residue, 2, Observe));
var terminal = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy).SelectOwnedPass(result);
int boundary = nativeGate ? 120 : 3;
if (result.NodeCount != owner.SourceGraph.NodeCount || result.IntermediatePublicationCount != 1 ||
    result.NextUnpublishedNode2490 != boundary || observed is null || observed.StartingNode2490 != 0 || observed.EndingNode != boundary - 1)
    throw new InvalidDataException("Actual native120-node gate or source lifecycle changed.");
var starts = observed.CopyNodeStarts2478();
var prefixes = observed.CopyPrefixOwners2488();
if (starts.Length != boundary + 1 || starts[boundary] != observed.RecordCount2470 || prefixes.Length != boundary + 1 ||
    prefixes.Take(boundary).Any(value => value != -1) || prefixes[boundary] != boundary - 1)
    throw new InvalidDataException("Native2478/2488/2490 table lifecycle changed.");
int compared = 0;
for (int node = 0; node < boundary; node++)
{
    var actual = result.GetNodeSnapshot(node);
    var published = observed.GetPublishedNodeSnapshot(node);
    if (!actual.Records.Span.SequenceEqual(published.Records.Span) ||
        !actual.PrimaryAlternatives.Span.SequenceEqual(published.PrimaryAlternatives.Span))
        throw new InvalidDataException("LIVE publication was not written back into future parent storage.");
    compared += actual.Records.Length + actual.PrimaryAlternatives.Length;
}
//Copy getters cannot mutate future source/compact storage.
byte[] leaked = observed.CopyCompactStorage(); leaked[0] ^= 255;
if (observed.CopyCompactStorage()[0] == leaked[0]) throw new InvalidDataException("Compact capture leaked mutable storage.");
starts[0] = -123;
if (observed.CopyNodeStarts2478()[0] != 0) throw new InvalidDataException("2478 leaked mutable storage.");
using var output = File.Create(args[9]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject();
writer.WriteString("publicationKind", "live");
writer.WriteBoolean("nativeGate", nativeGate);
writer.WriteNumber("factory", 0x292);
writer.WriteNumber("nodeCount", observed.EndingNode + 1);
writer.WriteNumber("actualGraphNodeCount", result.NodeCount);
writer.WriteNumber("maximumAtoms", owner.SourceGraph.MaximumAtoms);
writer.WriteNumber("intermediatePublicationDistance", controller.MaximumParentDistance54);
writer.WriteNumber("primary", controller.Configuration.PrimaryArcLimit230a);
writer.WriteNumber("count", observed.RecordCount2470);
writer.WriteNumber("previousCount", observed.PreviousRecordCount2470);
writer.WriteNumber("startingNode", observed.StartingNode2490);
writer.WriteNumber("endingNode", observed.EndingNode);
writer.WriteNumber("nextNode2490", result.NextUnpublishedNode2490);
writer.WriteNumber("penalty", LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy).LanguagePenalty2534);
writer.WriteBoolean("hasTerminalWinner", terminal.Winner.HasWinner);
writer.WriteNumber("queries", observed.QueryChecks); writer.WriteNumber("backendCalls", observed.BackendCalls);
writer.WriteBase64String("compact", observed.CopyCompactStorage());
WriteInts("starts", observed.CopyNodeStarts2478());
WriteInts("prefixOwners2488", prefixes);
writer.WriteStartArray("primaryMap");
foreach (ushort value in dictionary.Symbols.Symbols.CopyPrimary()) writer.WriteNumberValue(value);
writer.WriteEndArray();
WriteNodes("inputNodes", observed.GetInputNodeSnapshot);
WriteNodes("nodes", observed.GetPublishedNodeSnapshot);
writer.WriteStartArray("graph");
for (int node = 0; node <= observed.EndingNode; node++)
{
    writer.WriteStartArray();
    foreach (var arc in owner.SourceGraph.GetNode(node))
    {
        writer.WriteStartArray(); writer.WriteNumberValue(arc.GroupId); writer.WriteNumberValue(arc.Span);
        writer.WriteNumberValue(arc.EndContextId); writer.WriteEndArray();
    }
    writer.WriteEndArray();
}
writer.WriteEndArray(); writer.WriteStartArray("contexts");
foreach (var context in owner.Contexts.CopyNodes())
{
    writer.WriteStartArray(); writer.WriteNumberValue(context.Identity); writer.WriteNumberValue(context.ParentLink);
    writer.WriteNumberValue(context.Symbol); writer.WriteEndArray();
}
writer.WriteEndArray(); writer.WriteStartArray("scores");
for (int group = 0; group < owner.SourceGraph.Groups.Count; group++)
{
    var score = confidence.ScoreGroup(group);
    var acoustic = owner.MainLineAcoustic.Score(group);
    writer.WriteStartObject(); writer.WriteStartArray("candidates");
    foreach (var candidate in score.Candidates.Span)
    {
        writer.WriteStartArray(); writer.WriteNumberValue(candidate.Symbol); writer.WriteNumberValue(candidate.Cost); writer.WriteEndArray();
    }
    writer.WriteEndArray(); writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
    writer.WriteStartArray("costs"); foreach (int value in acoustic.Cache.Costs.Span) writer.WriteNumberValue(value);
    writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"LIVE {result.NodeCount} nodes/{result.IntermediatePublicationCount} publications/2490={result.NextUnpublishedNode2490}; {observed.RecordCount2470} compact rows, {compared} live bytes; terminal={terminal.Winner.HasWinner}.");

void WriteInts(string name, int[] values)
{
    writer.WriteStartArray(name); foreach (int value in values) writer.WriteNumberValue(value); writer.WriteEndArray();
}
void WriteNodes(string name, Func<int, LegacyMainDirectWordNodeResult> read)
{
    writer.WriteStartArray(name);
    for (int node = 0; node <= observed.EndingNode; node++)
    {
        var row = read(node);
        writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
        writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span);
        writer.WriteEndObject();
    }
    writer.WriteEndArray();
}
