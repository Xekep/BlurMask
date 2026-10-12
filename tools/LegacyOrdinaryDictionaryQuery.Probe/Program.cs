using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

if (args.Length is not (1 or 4)) throw new ArgumentException("controls.json [acoustic.res lexical.res engine-profile.json]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int initial = 0, continuation = 0, accepted = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var p = test.GetProperty("policy");
    var policy = new LegacyOrdinaryDictionaryQueryPolicy(p[0].GetByte(), p[1].GetByte(), p[2].GetBoolean(), p[3].GetBoolean(), p[4].GetInt32());
    var before = test.GetProperty("before");
    var query = new LegacyPreparedDictionaryQuery
    {
        Node = before[0].GetInt32(), Skip = before[1].GetInt32(), Capacity = before[2].GetInt32(),
        Parameter1c = before[3].GetUInt32(), Parameter20 = before[4].GetUInt32(),
        Flags24 = before[5].GetUInt16(), Flags26 = before[6].GetUInt16(),
        Category18 = before[7].GetByte(), Parameter19 = before[8].GetByte(),
        ContextRoot = before[9].GetUInt64(), ParentContext = before[10].GetUInt64()
    };
    byte[] parent = Convert.FromBase64String(test.GetProperty("parent").GetString()!);
    var ownedParent = parent.ToArray();
    uint owner = test.GetProperty("owner").GetUInt32(), span = test.GetProperty("span").GetUInt32();
    ushort flags = test.GetProperty("flags").GetUInt16(); bool frontend = test.GetProperty("frontend").GetBoolean();
    ulong? tag;
    if (test.GetProperty("kind").GetString() == "initial")
    { tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query, policy, owner, span, flags, frontend); initial++; }
    else
    { tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(query, policy, parent, owner, span, flags, frontend); continuation++; }
    var expectedTag = test.GetProperty("tag");
    if (tag != (expectedTag.ValueKind == JsonValueKind.Null ? (ulong?)null : expectedTag.GetUInt64()))
        throw new InvalidDataException("Ordinary query gate/tag differs from the independent signed-width control.");
    if (tag is not null) accepted++;
    var expected = test.GetProperty("expected");
    if (query.Node != expected[0].GetInt32() || query.Skip != expected[1].GetInt32() || query.Capacity != expected[2].GetInt32() ||
        query.Parameter1c != expected[3].GetUInt32() || query.Parameter20 != expected[4].GetUInt32() ||
        query.Flags24 != expected[5].GetUInt16() || query.Flags26 != expected[6].GetUInt16() ||
        query.Category18 != expected[7].GetByte() || query.Parameter19 != expected[8].GetByte() ||
        query.ContextRoot != expected[9].GetUInt64() || query.ParentContext != expected[10].GetUInt64() ||
        !parent.AsSpan().SequenceEqual(ownedParent))
        throw new InvalidDataException("Ordinary query mutation/preserved-owner fields differ.");
}
var fresh = new LegacyPreparedDictionaryQuery { Node = 7, Parameter19 = 42 };
try { LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(fresh, default, new byte[87], 0, 0, 0, false); throw new Exception("Truncated parent accepted."); }
catch (ArgumentException) { }
foreach (bool parent in new[] { false, true })
{
    try
    {
        if (parent) LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(fresh, default, new byte[88], 0, 0, 0, false, new CancellationToken(true));
        else LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(fresh, default, 0, 0, 0, false, new CancellationToken(true));
        throw new Exception("Canceled preparation succeeded.");
    }
    catch (OperationCanceledException) { }
}
if (fresh.Node != 7 || fresh.Parameter19 != 42) throw new Exception("Rejected preparation mutated reused query.");
Console.WriteLine($"PASS {initial} initial / {continuation} continuation controls, {accepted} accepted field mutations; signed32 wrap, owner distance, parent state/count, live frontend override, tag transport, preserved container/capacity, unchanged parent and cancellation guards.");
if (args.Length == 1) return;
byte[] acousticResource = File.ReadAllBytes(args[1]), lexicalResource = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(acousticResource, lexicalResource, File.ReadAllBytes(args[3]));
var exactMap = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet, model.LexicalAlphabet);
var contexts = new LegacySymbolContextFactory(); var initialContexts = contexts.CreateInitialSourceMap(exactMap);
var prefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexicalResource.AsSpan(606964, 4565174 - 606964));
uint languageOrder = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexicalResource.AsSpan(2513123, 4565174 - 2513123));
var descriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix, model.Lexicon, languageOrder, true);
var permissions = Enumerable.Repeat((byte)7, model.AcousticAlphabet.LabelCount).ToArray();
var source = new LegacyPreparedDictionaryQuerySource(model.Lexicon,
    new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount, descriptor.Flags, exactMap, exactMap).Refresh(permissions), initialContexts);
var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(source, contexts)]);
var reused = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity, ParentContext = 123 };
ulong startupTag = LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(reused, default, 3, 1, 0, false)!.Value;
var records = new List<byte[]>(); int realPages = 0;
while (true)
{
    var page = aggregate.Read(startupTag, reused, clearExtraCosts: true);
    if (page.ReturnedCount == 0) break;
    if (page.ReturnedCount < 0 || page.ExtraCosts.Length != page.ReturnedCount * 12) throw new Exception("Actual initial query page failed.");
    for (int i = 0; i < page.ReturnedCount; i++) records.Add(page.Records.Slice(i * 32, 32).ToArray());
    reused.Skip += page.ReturnedCount; realPages++;
}
if (records.Count != 132 || realPages != 2 || reused.ParentContext != 0 || reused.ContextRoot != contexts.ContainerIdentity)
    throw new Exception("Initial field producer did not feed the real dictionary/session correctly.");
var branch = records.First(r => model.Lexicon.GetChildCount((int)BinaryPrimitives.ReadUInt32LittleEndian(r)) > 0);
byte[] parentRow = new byte[88]; branch.AsSpan(0, 8).CopyTo(parentRow); parentRow[0x26] = 1;
BinaryPrimitives.WriteUInt32LittleEndian(parentRow.AsSpan(0x38), 3);
ulong nextTag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(reused, default, parentRow, 3, 1, 0, false)!.Value;
int beforeContextCount = contexts.NodeCount;
var continuationPage = aggregate.Read(nextTag, reused);
if (continuationPage.ReturnedCount <= 0 || reused.Skip != 0 || reused.ContextRoot != contexts.ContainerIdentity || contexts.NodeCount != beforeContextCount)
    throw new Exception("Continuation did not retain ordinary query owner/initial per-symbol contexts.");
for (int i = 0; i < continuationPage.ReturnedCount; i++)
{
    var record = continuationPage.Records.Span.Slice(i * 32, 32);
    ulong identity = BinaryPrimitives.ReadUInt64LittleEndian(record[8..]);
    if (record[4] != 1 || contexts.ReadNode(identity).Symbol != BinaryPrimitives.ReadUInt16LittleEndian(record[20..]))
        throw new Exception("Real ordinary continuation tag/source/context transport failed.");
}
Console.WriteLine($"PASS actual RU initial field producer -> aggregate/source132 records/{realPages} pages -> reused ordinary parent query -> {continuationPage.ReturnedCount} next records; exact full tag and per-symbol initial context semantics. Graph owner/span and parent row are explicit diagnostics; no native word winner claim.");
