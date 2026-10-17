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

var compact = LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, result, confidence,
    Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray());
var first = compact.SelectOwned([]) ?? throw new Exception("Actual first missing.");
var second = compact.SelectOwned([first.Selection.ExclusionIdentity]) ?? throw new Exception("Actual second missing.");
var callbacks = LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak, confidence, sampled);
int guards = 0, rowCount = 0;
using var output = File.Create(args[6]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject();
writer.WriteNumber("recordCount", compact.RecordCount);
writer.WriteBase64String("compact", compact.CopyCompactStorage());
writer.WriteStartArray("starts");
foreach (int start in compact.CopyNodeStarts2478()) writer.WriteNumberValue(start);
writer.WriteEndArray();
writer.WriteStartArray("graph");
for (int node = 0;
node < owner.SourceGraph.NodeCount;
node++)
{
    writer.WriteStartArray();
    foreach (var arc in owner.SourceGraph.GetNode(node))
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(arc.GroupId);
        writer.WriteNumberValue(arc.Span);
        writer.WriteNumberValue(arc.StartContextId);
        writer.WriteNumberValue(arc.EndContextId);
        writer.WriteEndArray();
    }
    writer.WriteEndArray();
}
writer.WriteEndArray();
writer.WriteStartArray("terminals");
foreach (var terminal in new[] { first, second })
{
    writer.WriteStartObject();
    writer.WriteNumber("selected", terminal.SelectedRecordIndex);
    writer.WriteNumber("secondary", terminal.Selection.SecondaryOrdinal);
    writer.WriteNumber("alternative", 65535);
    //Actual2271F0->221840 argument, not secondary slot.
    writer.WriteStartArray("controls");
    for (int n = 0;
n < 128;
n++)
    {
        byte[] residue = Enumerable.Range(0, 72).Select(i => (byte)(n * 73 + i * 31 + 11)).ToArray();
        var walk = LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, terminal, callbacks, residue);
        if (!ReferenceEquals(walk.SecondaryTerminal, terminal) || !ReferenceEquals(walk.ControllerOwner, controller) ||
            !ReferenceEquals(walk.ConfidenceOwner, confidence) || !ReferenceEquals(walk.CaseSourcePolicy, policy))
            throw new Exception("Configured selected ownership changed.");
        if (n == 0) Reject(() => _ = walk.Materialization);
        rowCount += walk.Count;
        writer.WriteStartObject();
        writer.WriteBase64String("residue", residue);
        writer.WriteBase64String("rows", walk.CopyRows());
        writer.WriteStartArray("visited");
        foreach (int index in walk.CopyVisitedCompactIndices()) writer.WriteNumberValue(index);
        writer.WriteEndArray();
        writer.WriteStartArray("residuals");
        foreach (ulong key in walk.CopyResidualSourceReferences()) writer.WriteNumberValue(key);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
    writer.WriteEndArray();
    writer.WriteEndObject();
}
writer.WriteEndArray();
Reject(() => LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, null!, callbacks, new byte[72]));
Reject(() => LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, first, null!, new byte[72]));
Reject(() => LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, first, callbacks, new byte[71]));
Reject(() => LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, first, callbacks, new byte[72], new(true)));
Reject(() => LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(new byte[ak.Length], first, callbacks, new byte[72]));
writer.WriteNumber("guards", guards);
writer.WriteNumber("rowCount", rowCount);
writer.WriteEndObject();
writer.Flush();
Console.WriteLine($"PASS configured2 actual distinct selected terminals/256 walks/{rowCount} rows/{guards} guards; no count1 terminal substitution.");
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
    throw new Exception("Malformed configured backtrack ownership accepted.");
}
