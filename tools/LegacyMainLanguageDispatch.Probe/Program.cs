using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 8) throw new ArgumentException("capacity-controls AK LK engine separation case publication capture");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int scalarCount = 0, guards = 0;
foreach (var row in controls.RootElement.EnumerateArray())
{
    if (LegacyNativeLanguageContextCapacity.FromHeapCapacity(row[0].GetUInt32()) != row[1].GetInt32())
        throw new Exception("Native unsigned logical-capacity float conversion differs.");
    scalarCount++;
}
byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]), engine = File.ReadAllBytes(args[3]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4]));
using var c = File.OpenRead(args[5]); using var p = File.OpenRead(args[6]);
var cases = LegacyNativeCaseData.Load(c); var publication = LegacyNativeTextPublicationData.Load(p);
var sampled = MainFixture.Create(ak, engine, table);
var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
var selected = FindActualTerminal();
var span = selected.Span; byte[] actualQuery = selected.Query, referenceToken = selected.Token;
var empty = new LegacyNativeLanguageContextRecombinationResult(false, ReadOnlyMemory<byte>.Empty, new byte[80], 0, 0);
var router = new LegacyNativeWordHeapRouter([new LegacyNativeWordRecordHeap(31)], new byte[] { 0, 0 }, -1, uint.MaxValue);

LegacyNativeLaterDictionaryWordCompletion Completion() => LegacyNativeLaterDictionaryWordCompletion.ForFreshCaseSpan(policy, span);
LegacyNativeLaterDictionaryWordCompletion.PreparedTerminal Capture(LegacyNativeLaterDictionaryWordCompletion completion,
    byte[]? query = null, CancellationToken token = default)
    => completion.CaptureActualTerminal(query ?? actualQuery, span.EndingNode, owner.Contexts,
        false, owner.CommonSourceSummary.LastKind800Source, false, false,
        cancellation => owner.ValidateSpan(span, cancellation), token);
IReadOnlyList<LegacyNativeLaterDictionaryWordCompletionResult> Consume(LegacyNativeLaterDictionaryWordCompletion completion,
    LegacyNativeLaterDictionaryWordCompletion.PreparedTerminal snapshot,
    LegacyNativeLanguageContextRecombinationResult? contexts = null, CancellationToken token = default)
    => completion.ProcessActualQueryWithPreparedTerminal(snapshot, contexts ?? empty, 0, owner.SourceGraph.NodeCount,
        _ => throw new Exception("Prepared empty-context control must not read a source row."),
        _ => throw new Exception("Prepared empty-context control must not read a header."),
        (_, _) => throw new Exception("Prepared empty-context control must not call E8."),
        router, new byte[40], token);

var producer = Completion(); var captured = Capture(producer);
if (!captured.CopyQuery().SequenceEqual(actualQuery) || !captured.CopyTerminal().SequenceEqual(referenceToken))
    throw new Exception("Full source tag/query or source70 token changed.");
byte[] detachedQuery = captured.CopyQuery(), detachedToken = captured.CopyTerminal();
detachedQuery[4] ^= 1; detachedToken[4] ^= 255;
if (!captured.CopyQuery().SequenceEqual(actualQuery) || !captured.CopyTerminal().SequenceEqual(referenceToken))
    throw new Exception("Caller copy changed the captured terminal.");
if (Consume(producer, captured).Count != 0) throw new Exception("Empty prepared heap unexpectedly produced a language row.");
Reject(() => Consume(producer, captured));

var mutableQuery = actualQuery.ToArray();
var snapshotProducer = Completion();
var snapshotted = snapshotProducer.CaptureActualTerminal(mutableQuery, span.EndingNode, owner.Contexts,
    false, owner.CommonSourceSummary.LastKind800Source, false, false,
    token => { owner.ValidateSpan(span, token); Array.Fill(mutableQuery, (byte)255); }, default);
if (!snapshotted.CopyQuery().SequenceEqual(actualQuery) || !snapshotted.CopyTerminal().SequenceEqual(referenceToken))
    throw new Exception("Source callback changed the pre-callback query snapshot.");
if (Consume(snapshotProducer, snapshotted).Count != 0) throw new Exception("Snapshot empty heap changed.");

var original = Completion(); var foreignToken = Capture(original);
Reject(() => Consume(Completion(), foreignToken));
if (Consume(original, foreignToken).Count != 0) throw new Exception("Foreign consume destroyed the genuine producer's token.");
Reject(() => Completion().CaptureActualTerminal(actualQuery, span.EndingNode, new LegacySymbolContextFactory(),
    false, owner.CommonSourceSummary.LastKind800Source, false, false, _ => { }, default));
Reject(() => Completion().CaptureActualTerminal(actualQuery, span.EndingNode + 1, owner.Contexts,
    false, owner.CommonSourceSummary.LastKind800Source, false, false, _ => { }, default));
Reject(() => Capture(Completion(), token: new(true)));
Reject(() => Capture(Completion(), new byte[31]));
var wrong = actualQuery.ToArray(); wrong[4] = 0;
Reject(() => Capture(Completion(), wrong));
wrong = actualQuery.ToArray(); BinaryPrimitives.WriteUInt64LittleEndian(wrong.AsSpan(8), ulong.MaxValue);
Reject(() => Capture(Completion(), wrong));
wrong = actualQuery.ToArray(); BinaryPrimitives.WriteUInt64LittleEndian(wrong.AsSpan(24), 1);
Reject(() => Capture(Completion(), wrong));
wrong = actualQuery.ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(wrong.AsSpan(20), checked((ushort)model.AcousticAlphabet.LabelCount));
Reject(() => Capture(Completion(), wrong));
wrong = actualQuery.ToArray(); model.AcousticAlphabet.TryGetLabel("\ufffd", out ushort replacement);
BinaryPrimitives.WriteUInt16LittleEndian(wrong.AsSpan(20), replacement);
Reject(() => Capture(Completion(), wrong));
wrong = actualQuery.ToArray(); wrong[23] = (byte)((wrong[23] & ~7) | 1);
Reject(() => Capture(Completion(), wrong));
var shapeProducer = Completion(); var shapeToken = Capture(shapeProducer);
Reject(() => Consume(shapeProducer, shapeToken, empty with { Candidates16 = new byte[1] }));
var canceledProducer = Completion(); var canceledToken = Capture(canceledProducer);
Reject(() => Consume(canceledProducer, canceledToken, token: new(true)));
using (var cancel = new CancellationTokenSource())
{
    Reject(() => Completion().CaptureActualTerminal(actualQuery, span.EndingNode, owner.Contexts,
        false, owner.CommonSourceSummary.LastKind800Source, false, false, _ => cancel.Cancel(), cancel.Token));
}
using (var cancel = new CancellationTokenSource())
{
    int validations = 0;
    Reject(() => Completion().CaptureActualTerminal(actualQuery, span.EndingNode, owner.Contexts,
        false, owner.CommonSourceSummary.LastKind800Source, false, false,
        _ => { if (++validations == 2) cancel.Cancel(); }, cancel.Token));
}
int preparedHistoryRows = PreparedHistory.Verify(ak, lk, engine, model, table, cases, publication);
var staleProducer = Completion(); var staleToken = Capture(staleProducer);
owner.AdvanceNodeEpoch(span.EndingNode);
Reject(() => Consume(staleProducer, staleToken));

using (var output = File.Create(args[7]))
using (var json = new Utf8JsonWriter(output))
{
    json.WriteStartObject();
    json.WriteNumber("scalarCount", scalarCount);
    json.WriteNumber("guards", guards);
    json.WriteNumber("preparedHistoryRows", preparedHistoryRows);
    json.WriteNumber("endingNode", span.EndingNode);
    json.WriteNumber("span", span.Span);
    json.WriteString("actualQuery", Convert.ToHexString(actualQuery));
    json.WriteString("terminal", Convert.ToHexString(referenceToken));
    json.WriteString("capturedTerminal", Convert.ToHexString(captured.CopyTerminal()));
    json.WriteString("scope", "actual CASE source70 token lifecycle with explicitly prepared empty context heap; no inferred later anchor");
    json.WriteEndObject();
}
Console.WriteLine($"PASS {scalarCount} independent logical-capacity controls/{guards} source/query/epoch/single-consume/cancel guards; actual CASE query32/token16; {preparedHistoryRows} explicitly prepared reverse-history rows.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid prepared terminal ownership accepted.");
}

(LegacyOwnedMainWordSpan Span, byte[] Query, byte[] Token) FindActualTerminal()
{
    owner.AdvanceNodeEpoch(-1);
    for (int node = 0; node < owner.SourceGraph.NodeCount; node++)
    {
        for (byte selector = 0; selector < owner.GetSpanExtent(node); selector++)
        {
            var live = owner.Prepare(node, selector);
            var query = new LegacyPreparedDictionaryQuery
            { Capacity = 128, Parameter1c = (uint)node, Parameter20 = selector, ContextRoot = owner.Contexts.ContainerIdentity };
            for (int pageIndex = 0; pageIndex < 128; pageIndex++)
            {
                var page = owner.Read(live, 8UL << 40, query, false);
                if (page.ReturnedCount <= 0) break;
                for (int offset = 0; offset < page.Records.Length; offset += 32)
                {
                    byte[] record = page.Records.Slice(offset, 32).ToArray();
                    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(20));
                    if (record[4] != 1 || (record[23] & 7) is not (0 or 2) ||
                        BinaryPrimitives.ReadUInt64LittleEndian(record.AsSpan(24)) != 0 ||
                        symbol >= model.AcousticAlphabet.LabelCount || model.AcousticAlphabet.IsVirtual(symbol) ||
                        !model.AcousticAlphabet.TryGetText(symbol, out string text) || text == "\ufffd") continue;
                    try
                    {
                        var terminal = owner.ReadWordTerminal(live,
                            BinaryPrimitives.ReadUInt64LittleEndian(record), symbol, 0);
                        if (terminal.Length == 16) return (live, record, terminal.ToArray());
                    }
                    catch (NotSupportedException) { }
                }
                query.Skip += page.ReturnedCount;
            }
        }
        owner.AdvanceNodeEpoch(node);
    }
    throw new Exception("No eligible actual source query for the token lifecycle control.");
}
