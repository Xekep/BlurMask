using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("AK LK engine separation capture.json");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var fixture = MainFixture.Create(ak, lk, engine, File.ReadAllBytes(args[3]));
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, owner.DictionaryOwner);
ulong identity = 1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] storage = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(storage);
    return new(storage, identity++);
}
var pass = new LegacyOwnedMainDirectWordPass(ak, policy, fixture.Table, Allocate).RunOwned();
var terminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
byte[] residue = Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray();
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Unsupported compact publication accepted.");
}
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, null!, confidence, residue));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, null!, residue));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, []));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, new byte[41]));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak.AsSpan(0, 100), terminal, confidence, residue));
byte[] corrupt = (byte[])ak.Clone(); corrupt[^1] ^= 1;
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(corrupt, terminal, confidence, residue));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, residue, new(true)));
var foreignOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var foreignConfidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, foreignOwner.DictionaryOwner);
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, foreignConfidence, residue));
var materialized = LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, residue);
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, terminal, confidence, residue));
Reject(() => materialized.GetNodeSnapshot(-1));
Reject(() => materialized.GetNodeSnapshot(pass.NodeCount));
byte[] compact = materialized.CopyCompactStorage();
var types = new Dictionary<byte, int>();
for (int i = 0; i < materialized.RecordCount; i++)
{
    byte kind = (byte)(compact[i * 40 + 32] & 7);
    types[kind] = types.GetValueOrDefault(kind) + 1;
    if (compact[i * 40 + 34] != 0xa7 || compact[i * 40 + 35] != 0xa7)
        throw new InvalidDataException("Compact allocator residue22/23 was overwritten.");
}
Console.WriteLine($"MATERIALIZED count{materialized.RecordCount} selected{materialized.SelectedRecordIndex} starts[{string.Join(',', materialized.CopyNodeStarts2478())}] types[{string.Join(',', types.Select(p => $"{p.Key}:{p.Value}"))}] queries{materialized.Reachability.QueryChecks}/{materialized.Reachability.BackendCalls}");
var starts = materialized.CopyNodeStarts2478();
var last = materialized.GetNodeSnapshot(pass.NodeCount - 1);
int relative = BinaryPrimitives.ReadUInt16LittleEndian(last.Records.Span[(terminal.Winner.Ordinal * 88 + 34)..]);
var cursor = LegacyPreparedCompactWordPredecessor.Begin(compact.AsSpan(0, materialized.RecordCount * 40), starts,
    pass.NodeCount - 1, relative);
var path = new List<int>();
while (!cursor.IsEnd)
{
    if (path.Count > materialized.RecordCount) throw new InvalidDataException("Backtrack cycle.");
    path.Add(cursor.RecordIndex);
    cursor = LegacyPreparedCompactWordPredecessor.Advance(compact.AsSpan(0, materialized.RecordCount * 40), starts, cursor, ushort.MaxValue);
}
Console.WriteLine($"PATH [{string.Join(',', path)}] labels[{string.Join(',', path.Select(index => BinaryPrimitives.ReadUInt16LittleEndian(compact.AsSpan(index * 40 + 28))))}]");
byte[] changed = materialized.CopyCompactStorage(); Array.Clear(changed);
int[] changedStarts = materialized.CopyNodeStarts2478(); Array.Clear(changedStarts);
if (!materialized.CopyCompactStorage().AsSpan().SequenceEqual(compact) ||
    !materialized.CopyNodeStarts2478().AsSpan().SequenceEqual(starts))
    throw new InvalidDataException("Public compact getters leaked private buffers.");
var changedNode = materialized.GetNodeSnapshot(0);
byte[] originalNodeRows = changedNode.Records.ToArray(), originalNodeAlternatives = changedNode.PrimaryAlternatives.ToArray();
if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(changedNode.Records, out var array)) Array.Clear(array.Array!);
if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(changedNode.PrimaryAlternatives, out array)) Array.Clear(array.Array!);
if (!materialized.GetNodeSnapshot(0).Records.Span.SequenceEqual(originalNodeRows) ||
    !materialized.GetNodeSnapshot(0).PrimaryAlternatives.Span.SequenceEqual(originalNodeAlternatives))
    throw new InvalidDataException("Public node getter leaked private row storage.");

//A failed allocation budget consumes the materialization lease, preventing
//unverified replay after a partial source-query/publication sequence.
var foreignPolicy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, foreignOwner);
var budgetPass = new LegacyOwnedMainDirectWordPass(ak, foreignPolicy, fixture.Table, Allocate).RunOwned();
var budgetTerminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(foreignPolicy).SelectOwnedPass(budgetPass);
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, budgetTerminal, foreignConfidence, new byte[40]));
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, budgetTerminal, foreignConfidence, residue));
var stalePass = new LegacyOwnedMainDirectWordPass(ak, foreignPolicy, fixture.Table, Allocate).RunOwned();
var staleTerminal = LegacyOwnedFreshMainTerminalInvocation.Prepare(foreignPolicy).SelectOwnedPass(stalePass);
foreignOwner.AdvanceNodeEpoch(stalePass.NodeCount - 1);
Reject(() => LegacyOwnedMainCompactWordMaterialization.Materialize(ak, staleTerminal, foreignConfidence, residue));
Console.WriteLine($"PASS {guards} null/resource/owner/budget/single-use/stale/cancel guards and deep copied compact/node buffers.");
using var file = File.Create(args[4]); using var writer = new Utf8JsonWriter(file);
writer.WriteStartObject(); writer.WriteNumber("count", materialized.RecordCount);
writer.WriteNumber("selected", materialized.SelectedRecordIndex); writer.WriteNumber("winner", terminal.Winner.Ordinal);
writer.WriteStartArray("starts"); foreach (int value in starts) writer.WriteNumberValue(value); writer.WriteEndArray();
writer.WriteBase64String("compact", compact); writer.WriteStartArray("nodes");
for (int node = 0; node < pass.NodeCount; node++)
{
    var row = materialized.GetNodeSnapshot(node);
    writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
    writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span);
    writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteNumber("primary", policy.Configuration.PrimaryArcLimit230a);
writer.WriteNumber("penalty", LegacyOwnedRussianTerminalSources.Prepare(owner.DictionaryOwner).LanguagePenalty2534);
writer.WriteStartArray("primaryMap");
foreach (ushort value in owner.DictionaryOwner.Symbols.CopyPrimary()) writer.WriteNumberValue(value);
writer.WriteEndArray(); writer.WriteStartArray("inputNodes");
for (int node = 0; node < pass.NodeCount; node++)
{
    var row = pass.GetNodeSnapshot(node);
    writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
    writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span);
    writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteStartArray("graph");
for (int node = 0; node < pass.NodeCount; node++)
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
foreach (var node in owner.Contexts.CopyNodes())
{
    writer.WriteStartArray(); writer.WriteNumberValue(node.Identity); writer.WriteNumberValue(node.ParentLink);
    writer.WriteNumberValue(node.Symbol); writer.WriteEndArray();
}
writer.WriteEndArray(); writer.WriteStartArray("scores");
for (int group = 0; group < owner.SourceGraph.Groups.Count; group++)
{
    var score = confidence.ScoreGroup(group); var acoustic = owner.Acoustic.Score(group);
    writer.WriteStartObject(); writer.WriteStartArray("candidates");
    foreach (var candidate in score.Candidates.Span)
    {
        writer.WriteStartArray(); writer.WriteNumberValue(candidate.Symbol);
        writer.WriteNumberValue(candidate.Cost); writer.WriteEndArray();
    }
    writer.WriteEndArray(); writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
    writer.WriteStartArray("costs");
    foreach (int value in acoustic.Cache.Costs.Span) writer.WriteNumberValue(value);
    writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteEndObject(); writer.Flush();
