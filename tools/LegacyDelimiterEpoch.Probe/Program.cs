using System.Buffers.Binary;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("AK LK engine required");
var model = LegacyRussianModelBundle.Load(File.ReadAllBytes(args[0]), File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]));
int controls = 0;
foreach (bool auxiliary in new[] { false, true })
{
    for (int skip = 0; skip < 4; skip++)
    {
        var contexts = new LegacySymbolContextFactory();
        var log = new List<(int Skip, LegacyPreparedDictionaryQuery Request)>();
        LegacyAggregateDictionaryQueryPage Backend(ulong tag, LegacyPreparedDictionaryQuery query, CancellationToken token)
        {
            log.Add((query.Skip, query));
            int count = Math.Max(0, 4 - query.Skip);
            byte[] bytes = new byte[count * 32];
            for (int i = 0; i < count; i++)
            {
                var row = bytes.AsSpan(i * 32, 32);
                BinaryPrimitives.WriteUInt32LittleEndian(row, (uint)(query.Skip + i));
                row[5] = 2; row[23] = auxiliary ? (byte)0x10 : (byte)0;
                BinaryPrimitives.WriteUInt16LittleEndian(row[20..], 7);
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], contexts.ResolveInitial(7, token).Identity);
            }
            return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
        }
        var normal = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, contexts, Backend, 2);
        var aux = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, contexts, Backend, 4);
        var request = new LegacyPreparedDictionaryQuery { Capacity = 1, ContextRoot = contexts.ContainerIdentity };
        var query = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = request };
        LegacyNativeDelimiterQueryPage Read() => auxiliary ? aux.Read(0, request) : normal.Read(query);
        Require(Read().ReturnedCount == 1 && log.Count == 1, "First epoch read must fetch its backend block once.");
        var oldRequest = log[0].Request;
        request.Skip = 1;
        Require(Read().ReturnedCount == 1 && log.Count == (auxiliary ? 2 : 1),
            "Normal cache resume versus auxiliary one-time replay differs before reset.");
        if (auxiliary) aux.ResetForControllerNode(); else normal.ResetForControllerNode();
        request.Skip = skip;
        int oldCalls = log.Count;
        var page = Read();
        Require(page.ReturnedCount == 1 && BinaryPrimitives.ReadUInt32LittleEndian(page.Records.Span) == skip &&
            log.Count == oldCalls + 1 && log[^1].Skip == 0 && !ReferenceEquals(oldRequest, log[^1].Request),
            "V80 reset must replay accepted skip using a fresh underlying query-owner identity.");
        if (!auxiliary)
        {
            var firstNew = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = new() { ContextRoot = contexts.ContainerIdentity } };
            var secondNew = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = new() { ContextRoot = contexts.ContainerIdentity } };
            Require(normal.Read(firstNew).SlotIndex == 0 && normal.Read(secondNew).SlotIndex == 1,
                "237080 must reset outer normal nextSlot to zero, including an old query's slot0 resume.");
        }
        controls++;
    }
}
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Failed delimiter cache was revived by epoch reset.");
}
var failureContexts = new LegacySymbolContextFactory();
LegacyAggregateDictionaryQueryPage Fail(ulong tag, LegacyPreparedDictionaryQuery query, CancellationToken token)
    => new(-1, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
var failedNormal = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, failureContexts, Fail, 2);
var failedAux = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, failureContexts, Fail, 4);
var failedRequest = new LegacyPreparedDictionaryQuery { ContextRoot = failureContexts.ContainerIdentity };
Require(failedNormal.Read(new() { SourceTag = 0, Request = failedRequest }).ReturnedCount == -1 &&
    failedAux.Read(0, failedRequest).ReturnedCount == -1, "Native failed return channel differs.");
Reject(() => failedNormal.ResetForControllerNode());
Reject(() => failedAux.ResetForControllerNode());
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    Reject(() => failedNormal.ResetForControllerNode(canceled.Token));
    Reject(() => failedAux.ResetForControllerNode(canceled.Token));
}
Console.WriteLine($"PASS {controls} normal/auxiliary epoch replay+request-owner controls, nextSlot0/1 and {guards} lifecycle guards; no per-permission refresh reset.");

static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
