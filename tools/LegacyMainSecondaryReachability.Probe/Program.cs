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
var controller = LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 2);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
ulong identity = 0;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] storage = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(storage);
    return new(storage, ++identity);
}
var result = new LegacyOwnedMainCaseWordPass(ak, policy, table, Allocate).RunOwned();
Reject(() => LegacyOwnedMainSecondaryCompactReachability.Prepare(ak, null!));
Reject(() => LegacyOwnedMainSecondaryCompactReachability.Prepare(new byte[ak.Length], result));
Reject(() => LegacyOwnedMainSecondaryCompactReachability.Prepare(ak.AsSpan(0, 100), result));
Reject(() => LegacyOwnedMainSecondaryCompactReachability.Prepare(ak, result, new(true)));
var marked = LegacyOwnedMainSecondaryCompactReachability.Prepare(ak, result);
if (!ReferenceEquals(marked.PassResult, result) || marked.FinalizedPrefixEnd != -1 || marked.QueryChecks != 0)
    throw new Exception("Actual four-node ownership/prefix changed.");
using var output = File.Create(args[6]); using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject(); writer.WriteNumber("primaryLimit", controller.Configuration.PrimaryArcLimit230a);
writer.WriteBase64String("selected", marked.CopySelectedNodes());
writer.WriteStartArray("actual");
for (int node = 0; node < result.NodeCount; node++)
{
    writer.WriteStartObject();
    WriteSnapshot("before", result.GetNodeSnapshot(node));
    WriteSnapshot("after", marked.GetNodeSnapshot(node));
    writer.WriteEndObject();
}
writer.WriteEndArray();
var escaped = marked.GetNodeSnapshot(0).SecondaryAlternatives;
if (!MemoryMarshal.TryGetArray(escaped, out var array)) throw new Exception("Owned snapshot expected.");
byte retained = marked.GetNodeSnapshot(0).SecondaryAlternatives.Span[0]; array.Array![array.Offset] ^= 255;
if (marked.GetNodeSnapshot(0).SecondaryAlternatives.Span[0] != retained) throw new Exception("Private2410 escaped.");
writer.WriteStartArray("prepared");
for (int control = 0; control < 256; control++)
{
    var nodes = new LegacyMainDirectWordNodeResult[4];
    for (int node = 0; node < 4; node++)
    {
        byte[] rows = Enumerable.Range(0, 4*88).Select(i => (byte)(i*19+control)).ToArray();
        byte[] secondary = Enumerable.Range(0, 4*80).Select(i => (byte)(i*31+control)).ToArray();
        for (int row = 0; row < 4; row++)
            for (int k = 0; k < 2; k++)
            {
                int at = (row*2+k)*40;
                BinaryPrimitives.WriteInt32LittleEndian(secondary.AsSpan(at), (k == 1 && control%3 == 0) ? 0x4000000 : control);
                BinaryPrimitives.WriteUInt16LittleEndian(secondary.AsSpan(at+0x18), (ushort)((row+control+k)%4));
                secondary[at+0x1a] = 0;
                secondary[at+0x1b] = (byte)((control+row+k)%2 | ((control%4 == 0 && row < 2) ? 0x80 : 0));
                BinaryPrimitives.WriteUInt16LittleEndian(secondary.AsSpan(at+0x20), control%2 == 0 ? (ushort)0xffff : (ushort)(control%4));
                secondary[at+0x22] = (byte)(control%2);
            }
        nodes[node] = result.GetNodeSnapshot(0) with
        { EndingNode = node, SourceEnd23f0 = 4, Records = rows, SecondaryAlternatives = secondary };
    }
    writer.WriteStartObject(); writer.WriteStartArray("before");
    foreach (var node in nodes) writer.WriteBase64StringValue(node.SecondaryAlternatives.Span);
    writer.WriteEndArray();
    byte[] selected = [0,0,0,0];
    LegacyNativeSecondaryChainReachability.Mark(nodes, selected, 0, 2, 3, control%4, default);
    writer.WriteNumber("ordinal", control%4); writer.WriteBase64String("selected", selected);
    writer.WriteStartArray("after");
    foreach (var node in nodes) writer.WriteBase64StringValue(node.SecondaryAlternatives.Span);
    writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray();
owner.AdvanceNodeEpoch(-1);
Reject(() => LegacyOwnedMainSecondaryCompactReachability.Prepare(ak, result));
writer.WriteNumber("guards", guards); writer.WriteEndObject(); writer.Flush();
Console.WriteLine("PASS actual configured2410 reachability plus256 prepared replacement-chain controls; compact indices remain unproduced.");
void WriteSnapshot(string property, LegacyMainDirectWordNodeResult node)
{
    writer.WriteStartObject(property); writer.WriteNumber("node", node.EndingNode);
    writer.WriteNumber("sourceEnd", node.SourceEnd23f0);
    writer.WriteBase64String("records", node.Records.Span);
    writer.WriteBase64String("primary", node.PrimaryAlternatives.Span);
    writer.WriteBase64String("secondary", node.SecondaryAlternatives.Span); writer.WriteEndObject();
}
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or OperationCanceledException or InvalidOperationException)
    { guards++; return; }
    throw new Exception("Malformed or stale configured ownership accepted.");
}
