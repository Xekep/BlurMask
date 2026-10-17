using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if (args.Length != 7) throw new ArgumentException("AK LK engine separation case publication capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var c = File.OpenRead(args[4]); using var p = File.OpenRead(args[5]);
var caseData = LegacyNativeCaseData.Load(c); var publication = LegacyNativeTextPublicationData.Load(p);
var sampled = MainFixture.Create(ak, engine, table);
var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, caseData, publication);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
int guards = 0;
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 0));
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 21));
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, uint.MaxValue));
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 1));
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 20));
Reject(() => LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 2, new CancellationToken(true)));
var controller = LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 2);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
ulong identity = 0;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] storage = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(storage);
    return new(storage, ++identity);
}
var pass = new LegacyOwnedMainCaseWordPass(ak, policy, table, Allocate);
var result = pass.RunOwned();
Reject(() => pass.Run());
Reject(() => new LegacyOwnedMainDirectWordPass(ak, controller, table, Allocate));
Reject(() => LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy));
Reject(() => LegacyFreshMainResultGlyphNormalization.Prepare(policy));
int extent = result.GetNodeSnapshot(0).Records.Length / 88;
Reject(() => new LegacyOwnedFreshMainWordNodeBuffer(controller, 0, new byte[extent*88], new byte[125*80]));
Reject(() => new LegacyOwnedFreshMainWordNodeBuffer(controller, 0, new byte[extent*88], new byte[125*80], new byte[extent*80-1], default));
var escaped = result.GetNodeSnapshot(0).SecondaryAlternatives;
if (!MemoryMarshal.TryGetArray(escaped, out var backing)) throw new Exception("Portable snapshot array expected.");
byte retained = result.GetNodeSnapshot(0).SecondaryAlternatives.Span[0];
backing.Array![backing.Offset] ^= 255;
if (result.GetNodeSnapshot(0).SecondaryAlternatives.Span[0] != retained) throw new Exception("Owned2410 storage escaped through public snapshot.");
int finite = 0, second = 0;
using var file = File.Create(args[6]); using var writer = new Utf8JsonWriter(file);
writer.WriteStartObject(); writer.WriteStartArray("nodes");
for (int node = 0; node < result.NodeCount; node++)
{
    var snapshot = result.GetNodeSnapshot(node);
    if (snapshot.SecondaryAlternatives.Length != snapshot.Records.Length / 88 * 80)
        throw new Exception("Actual2410 extent not coupled to its own source row allocation.");
    for (int row = 0; row < snapshot.SourceEnd23f0; row++)
        for (int k = 0; k < 2; k++)
        {
            var alternative = snapshot.SecondaryAlternatives.Span.Slice((row * 2 + k) * 40, 40);
            if (BinaryPrimitives.ReadInt32LittleEndian(alternative) >= 0x4000000) break;
            finite++; if (k == 1) second++;
        }
    writer.WriteStartObject(); writer.WriteNumber("node", node);
    writer.WriteNumber("sourceEnd", snapshot.SourceEnd23f0); writer.WriteNumber("retainedEnd", snapshot.RetainedEnd23f8);
    writer.WriteNumber("rootQueries", snapshot.RootQueries); writer.WriteNumber("parentQueries", snapshot.ParentQueries);
    writer.WriteNumber("language", snapshot.LanguageAllocations);
    writer.WriteBase64String("records", snapshot.Records.Span);
    writer.WriteBase64String("primary", snapshot.PrimaryAlternatives.Span);
    writer.WriteBase64String("secondary", snapshot.SecondaryAlternatives.Span); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteStartArray("contexts");
foreach (var context in owner.Contexts.CopyNodes())
{
    writer.WriteStartArray(); writer.WriteNumberValue(context.Identity); writer.WriteNumberValue(context.ParentLink);
    writer.WriteNumberValue(context.Symbol); writer.WriteEndArray();
}
writer.WriteEndArray();
writer.WriteStartArray("preparedPeriodic");
int[] costs = [int.MinValue,-1,0,12345,0x3ffffff,0x4000000,int.MaxValue];
for (int control = 0; control < 64; control++)
{
    byte[] recordAllocation = Enumerable.Range(0, extent*88).Select(i => (byte)(i*13+control)).ToArray();
    byte[] primaryAllocation = Enumerable.Range(0, 125*80).Select(i => (byte)(i*17+control)).ToArray();
    byte[] secondaryAllocation = Enumerable.Range(0, extent*80).Select(i => (byte)(i*23+control)).ToArray();
    var buffer = new LegacyOwnedFreshMainWordNodeBuffer(controller, 0, recordAllocation, primaryAllocation, secondaryAllocation, default);
    byte[] slot = new byte[128]; result.GetNodeSnapshot(0).Records.Span[..88].CopyTo(slot);
    BinaryPrimitives.WriteInt32LittleEndian(slot.AsSpan(16), costs[control%costs.Length]);
    byte[] source = result.GetNodeSnapshot(0).SecondaryAlternatives.ToArray();
    BinaryPrimitives.WriteInt32LittleEndian(source, costs[(control/2)%costs.Length]);
    BinaryPrimitives.WriteInt32LittleEndian(source.AsSpan(40), costs[(control/3)%costs.Length]);
    buffer.WritePrimary(slot, result.GetNodeSnapshot(0).PrimaryAlternatives.Span, source, default);
    buffer.BeginLanguage(); buffer.FinishWithoutLanguage();
    byte[] beforeRecords = buffer.CopyRecordStorage().ToArray(), beforeSecondary = buffer.CopySecondaryStorage().ToArray();
    int minimum = control%3 == 0 ? 0x3ffffff : control+1;
    buffer.ApplyPeriodicCostOffset(minimum, default);
    writer.WriteStartObject(); writer.WriteNumber("minimum", minimum); writer.WriteNumber("sourceEnd", buffer.SourceEnd23f0);
    writer.WriteBase64String("beforeRecords", beforeRecords); writer.WriteBase64String("beforeSecondary", beforeSecondary);
    writer.WriteBase64String("afterRecords", buffer.CopyRecordStorage().Span);
    writer.WriteBase64String("afterSecondary", buffer.CopySecondaryStorage().Span); writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteNumber("guards", guards);
writer.WriteNumber("finite", finite); writer.WriteNumber("second", second); writer.WriteEndObject(); writer.Flush();
if (result.NodeCount != 4 || finite == 0) throw new Exception("Actual configured2 inference did not produce secondary storage.");
Console.WriteLine($"PASS actual configured CASE inference4nodes/{finite} finite secondary40/{second} second entries; no WORD/terminal output claim.");
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or OperationCanceledException or InvalidOperationException)
    { guards++; return; }
    throw new Exception("Unsupported or malformed secondary ownership was accepted.");
}
