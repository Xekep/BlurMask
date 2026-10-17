using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if (args.Length != 7) throw new ArgumentException("AK LK engine separation case publication capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var c = File.OpenRead(args[4]);
using var p = File.OpenRead(args[5]);
var caseData = LegacyNativeCaseData.Load(c);
var publication = LegacyNativeTextPublicationData.Load(p);
var sampled = MainFixture.Create(ak, engine, table);
var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, caseData, publication);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);

var controller = LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 2);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
ulong identity = 0;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] storage = new byte[request.RequiredBytes];
    request.PreviousStorage.Span.CopyTo(storage);
    return new(storage, ++identity);
}
var result = new LegacyOwnedMainCaseWordPass(ak, policy, table, Allocate).RunOwned();
var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary);
int guards = 0;
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, null!, confidence, new byte[40]));
var foreignDictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, caseData, publication);
var foreignConfidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, foreignDictionary);
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, foreignConfidence, new byte[40]));
byte[] residue = Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray();
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(new byte[ak.Length], result, confidence, residue));
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak.AsSpan(0, 100), result, confidence, residue));
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, confidence, new byte[39]));
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, confidence, residue, new(true)));
var compact = LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, confidence, Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray());
Reject(() => LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, confidence, residue));
var ownedFirst = compact.SelectOwned([]) ?? throw new Exception("Actual first finite secondary missing.");
var ownedSecond = compact.SelectOwned([ownedFirst.Selection.ExclusionIdentity]) ?? throw new Exception("Actual second finite secondary missing.");
var first = ownedFirst.Selection;
var second = ownedSecond.Selection;
if (first.ExclusionIdentity == second.ExclusionIdentity || ownedFirst.SelectedRecordIndex == ownedSecond.SelectedRecordIndex ||
    !ReferenceEquals(ownedFirst.Materialization, compact) || !ReferenceEquals(ownedSecond.Materialization, compact))
    throw new Exception("Published alternatives lost distinct actual identity/indices or shared owner.");
Reject(() => compact.SelectOwned(new ulong[result.GetNodeSnapshot(result.NodeCount - 1).SourceEnd23f0 + 1]));
Reject(() => compact.SelectOwned([], new(true)));
Console.WriteLine($"Actual configured compact {compact.RecordCount}, first {first.Winner.Ordinal}/{first.SecondaryOrdinal} index{first.Winner.RowField22}, second {second.Winner.Ordinal}/{second.SecondaryOrdinal} index{second.Winner.RowField22}.");
byte[] escaped = compact.CopyCompactStorage();
byte original = escaped[0];
escaped[0] ^= 255;
if (compact.CopyCompactStorage()[0] != original) throw new Exception("Compact storage escaped through a getter.");
byte[] selectedEscape = ownedFirst.CopySecondaryAlternative40();
byte originalSelected = selectedEscape[0];
selectedEscape[0] ^= 255;
if (ownedFirst.CopySecondaryAlternative40()[0] != originalSelected) throw new Exception("Selected2410 snapshot escaped.");
using var output = File.Create(args[6]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject();
writer.WriteNumber("factory", 0x292);
writer.WriteNumber("nodeCount", result.NodeCount);
writer.WriteNumber("maximumAtoms", owner.SourceGraph.MaximumAtoms);
writer.WriteNumber("primary", controller.Configuration.PrimaryArcLimit230a);
writer.WriteNumber("count", compact.RecordCount);
writer.WriteBase64String("compact", compact.CopyCompactStorage());
writer.WriteStartArray("starts");
foreach (int n in compact.CopyNodeStarts2478()) writer.WriteNumberValue(n);
writer.WriteEndArray();
writer.WriteStartArray("inputNodes");
for (int n = 0;
n < result.NodeCount;
n++) WriteNode(result.GetNodeSnapshot(n));
writer.WriteEndArray();
writer.WriteStartArray("nodes");
for (int n = 0;
n < result.NodeCount;
n++) WriteNode(compact.GetNodeSnapshot(n));
writer.WriteEndArray();
writer.WriteStartArray("contexts");
foreach (var item in owner.Contexts.CopyNodes())
{
    writer.WriteStartArray();
    writer.WriteNumberValue(item.Identity);
    writer.WriteNumberValue(item.ParentLink);
    writer.WriteNumberValue(item.Symbol);
    writer.WriteEndArray();
}
writer.WriteEndArray();
writer.WriteStartArray("graph");
for (int n = 0;
n < result.NodeCount;
n++)
{
    writer.WriteStartArray();
    foreach (var arc in owner.SourceGraph.GetNode(n))
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(arc.GroupId);
        writer.WriteNumberValue(arc.Span);
        writer.WriteNumberValue(arc.EndContextId);
        writer.WriteEndArray();
    }
    writer.WriteEndArray();
}
writer.WriteEndArray();
writer.WriteStartArray("scores");
for (int group = 0;
group < owner.SourceGraph.Groups.Count;
group++)
{
    var score = confidence.ScoreGroup(group);
    var acoustic = owner.Acoustic.Score(group);
    writer.WriteStartObject();
    writer.WriteStartArray("candidates");
    foreach (var candidate in score.Candidates.Span)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(candidate.Symbol);
        writer.WriteNumberValue(candidate.Cost);
        writer.WriteEndArray();
    }
    writer.WriteEndArray();
    writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
    writer.WriteStartArray("costs");
    foreach (int value in acoustic.Cache.Costs.Span) writer.WriteNumberValue(value);
    writer.WriteEndArray();
    writer.WriteEndObject();
}
writer.WriteEndArray();
writer.WriteStartArray("primaryMap");
foreach (ushort value in dictionary.Symbols.Symbols.CopyPrimary()) writer.WriteNumberValue(value);
writer.WriteEndArray();
int penalty = LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel, model.LanguageProfiles[model.ResourceLanguageProfileIndex], dictionary.Descriptor.RuntimeLanguageScale);
writer.WriteNumber("penalty", penalty);
writer.WriteNumber("descriptorFlags", dictionary.Descriptor.Flags);
writer.WriteStartArray("selected");
foreach (var item in new[] { ownedFirst, ownedSecond })
{
    writer.WriteStartObject();
    writer.WriteNumber("ordinal", item.Selection.Winner.Ordinal);
    writer.WriteNumber("secondary", item.Selection.SecondaryOrdinal);
    writer.WriteNumber("identity", item.Selection.ExclusionIdentity);
    writer.WriteNumber("index", item.SelectedRecordIndex);
    writer.WriteBase64String("row88", item.CopyStoredRow88());
    writer.WriteBase64String("entry40", item.CopySecondaryAlternative40());
    writer.WriteEndObject();
}
writer.WriteEndArray();
owner.AdvanceNodeEpoch(-1);
Reject(() => compact.SelectOwned([]));
writer.WriteNumber("guards", guards);
writer.WriteEndObject();
writer.Flush();
void WriteNode(LegacyMainDirectWordNodeResult node)
{
    writer.WriteStartObject();
    writer.WriteNumber("sourceEnd", node.SourceEnd23f0);
    writer.WriteBase64String("rows", node.Records.Span);
    writer.WriteBase64String("alternatives", node.PrimaryAlternatives.Span);
    writer.WriteBase64String("secondary", node.SecondaryAlternatives.Span);
    writer.WriteEndObject();
}

void Reject(Action action)
{
    try
    {
        action();
    }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException)
    {
        guards++;
        return;
    }
    throw new Exception("Invalid configured publication ownership/lifecycle accepted.");
}
