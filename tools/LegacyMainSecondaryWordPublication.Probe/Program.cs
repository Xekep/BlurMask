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

var geometry = LegacyOwnedRussianMainSourceGeometry.Prepare(ak, confidence, sampled);
var interior = LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak, callbacks, geometry);
var firstRows = LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, first, callbacks, new byte[72]);
var secondRows = LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(ak, second, callbacks, new byte[72]);
var firstPlan = LegacyOwnedMainGlyphFlushPlan.PrepareConfigured(firstRows);
var secondPlan = LegacyOwnedMainGlyphFlushPlan.PrepareConfigured(secondRows);
int guards = 0, words = 0, flushes = 0;
using var output = File.Create(args[6]);
using var w = new Utf8JsonWriter(output);
w.WriteStartObject();
w.WriteNumber("recordCount", compact.RecordCount); w.WriteBase64String("compact", compact.CopyCompactStorage());
w.WriteStartArray("starts"); foreach (int start in compact.CopyNodeStarts2478()) w.WriteNumberValue(start); w.WriteEndArray();
w.WriteStartArray("cumulative"); foreach (int value in result.CopyCumulativePathCosts()) w.WriteNumberValue(value); w.WriteEndArray();
w.WriteStartArray("graph"); for (int node = 0; node < owner.SourceGraph.NodeCount; node++)
{
    w.WriteStartArray(); foreach (var arc in owner.SourceGraph.GetNode(node))
    { w.WriteStartArray(); w.WriteNumberValue(arc.GroupId); w.WriteNumberValue(arc.Span); w.WriteNumberValue(arc.StartContextId); w.WriteNumberValue(arc.EndContextId); w.WriteEndArray(); }
    w.WriteEndArray();
}
w.WriteEndArray();
w.WriteStartArray("cases");
for (int n = 0;
n < 32;
n++)
{
    var top = LegacyFreshMainPlainResultOwnership.CreateConfigured(first, geometry);
    var fork = top.ForkConfiguredAlternative(second);
    if (!ReferenceEquals(top.TopOwner, fork.TopOwner) || !ReferenceEquals(top.GlyphPool, fork.GlyphPool))
        throw new Exception("Actual configured alternatives lost the same TOP/pool.");
    byte[] residue = Enumerable.Range(0, 120).Select(i => (byte)(i * 37 + n)).ToArray();
    foreach (var (offset, length) in new[] { (32, 8), (88, 12), (104, 12) }) residue.AsSpan(offset, length).Clear();
    if (n == 0)
    {
        Reject(() => _ = top.Materialization);
        Reject(() => LegacyOwnedMainGlyphFlushPlan.Prepare(policy, firstRows));
        Reject(() => LegacyOwnedMainGlyphFlushSession.Prepare(top, firstPlan, residue));
        var premature = LegacyOwnedMainGlyphFlushSession.PrepareConfigured(fork, secondPlan, residue);
        Reject(() => LegacyOwnedMainPlainWordPublication.PrepareConfigured(ak, premature, table, callbacks, interior));
    }
    uint? topWidth = null;
    foreach (var (ownership, plan) in new[] { (top, firstPlan), (fork, secondPlan) })
    {
        var session = LegacyOwnedMainGlyphFlushSession.PrepareConfigured(ownership, plan, residue);
        var publisher = LegacyOwnedMainPlainWordPublication.PrepareConfigured(ak, session, table, callbacks, interior);
        if (topWidth is { } established && publisher.TopB0Bits != established)
            throw new Exception("Second alternative replaced retained TOP.B0.");
        topWidth = publisher.TopB0Bits;
        byte[] record = Enumerable.Range(0, 40).Select(i => (byte)(i * 17 + n)).ToArray();
        ushort[] labels = Enumerable.Range(0, 256).Select(i => (ushort)(i * 13 + n)).ToArray();
        byte[] request = Enumerable.Range(0, 64).Select(i => (byte)(i * 19 + n)).ToArray();
        w.WriteStartObject();
        w.WriteNumber("ac", publisher.TopAcBits);
        w.WriteNumber("b0", publisher.TopB0Bits);
        w.WriteNumber("secondary", ownership.SecondaryTerminal!.Selection.SecondaryOrdinal);
        w.WriteNumber("identity", ownership.SecondaryTerminal.Selection.ExclusionIdentity);
        w.WriteNumber("selected", ownership.SecondaryTerminal.SelectedRecordIndex);
        w.WriteNumber("totalCost", ownership.SecondaryTerminal.Selection.Winner.TotalCost);
        w.WriteNumber("rowCost14", ownership.SecondaryTerminal.Selection.Winner.RowCost14);
        w.WriteNumber("alternative", 65535);
        w.WriteString("initial", Convert.ToHexString(session.CopyInitialWordStorage()));
        w.WriteStartArray("flushes");
        while (!session.IsComplete)
        {
            var associated = session.PrepareNext();
            byte[] before = associated.WorkingWord.CopyStorage();
            var published = publisher.Publish(associated, record, labels, request);
            if (!published.Succeeded) throw new Exception("Actual configured WORD callback failed.");
            byte[] after = associated.WorkingWord.CopyStorage();
            w.WriteStartObject();
            w.WriteString("packet", Convert.ToHexString(associated.Packet.CopyStorage()));
            w.WriteString("rows", Convert.ToHexString(associated.CopyRows72()));
            w.WriteString("before", Convert.ToHexString(before));
            w.WriteString("after", Convert.ToHexString(after));
            w.WriteBoolean("succeeded", published.Succeeded);
            if (published.PublishedWord is { } node)
            {
                words++;
                w.WriteString("word", Convert.ToHexString(node.Word.CopyStorage()));
                w.WriteNumber("wordIndex", node.WordIndex);
                w.WriteStartArray("span");
                w.WriteNumberValue(node.Span.First);
                w.WriteNumberValue(node.Span.Last);
                w.WriteEndArray();
                w.WriteStartArray("references");
                foreach (ulong reference in node.Word.FlatReferences) w.WriteNumberValue(reference);
                w.WriteEndArray();
                var projection = LegacyOwnedMainWordUtf8Projection.Project(published);
                w.WriteString("rawUtf8", Convert.ToHexString(projection.CopyUtf8()));
            }
            else w.WriteNull("word");
            w.WriteStartArray("pool");
            foreach (var poolNode in ownership.GlyphPool.CopyOwnerNodes())
            {
                w.WriteStartObject();
                w.WriteNumber("key", poolNode.Key);
                w.WriteNumber("packed", poolNode.PackedCount);
                w.WriteString("records", Convert.ToHexString(poolNode.Records.Span));
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
            Reject(() => publisher.Publish(associated, record, labels, request));
            session.CompleteCurrent(published);
            Reject(() => session.CompleteCurrent(published));
            flushes++;
        }
        if (ownership.WordPublisher.Head is null) throw new Exception("Real selected alternative published no WORD.");
        w.WriteEndArray();
        w.WriteEndObject();
    }
    if (ReferenceEquals(top.WordPublisher.Head!.Word, fork.WordPublisher.Head!.Word))
        throw new Exception("Distinct actual selected paths unexpectedly share the same275 WORD object.");
}
w.WriteEndArray();
w.WriteNumber("guards", guards);
w.WriteNumber("words", words);
w.WriteNumber("flushes", flushes);
w.WriteEndObject();
w.Flush();
Console.WriteLine($"PASS configured2 sameTOP {words} actual WORDs/{flushes} callbacks/{guards} guards; raw UTF8 only, no accepted273 scheduling.");
void Reject(Action action)
{
    try
    {
        action();
    }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException)
    {
        guards++;
        return;
    }
    throw new Exception("Invalid configured WORD ownership/lifecycle accepted.");
}
