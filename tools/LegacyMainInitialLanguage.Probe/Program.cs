using System.Buffers.Binary;
using InkRecognition;

if (args.Length != 4) throw new ArgumentException("AK LK engine separation required");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]);
var fixture = MainFixture.Create(ak, lk, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, File.ReadAllBytes(args[2]), fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var configuration = policy.Configuration;
var graph = owner.Source.SourceRanges.Graph;
var dictionary = owner.DictionaryOwner;
var model = dictionary.Model;
var categories = model.CursiveCategoryProfiles[1];
uint[] headers = dictionary.CopySourceHeaders();
var preparedHeaps = LegacyNativeWordHeapPreparation.PrepareFresh(
    [new(0, 0, 1), LegacyNativeWordPreparedSource.FromDictionary(dictionary.Descriptor)],
    configuration, policy.SecondaryScaleEc);
Require(preparedHeaps.Limits.Count == 1 && preparedHeaps.Limits[0].PrimaryInitialCapacity20 == 250,
    "Actual1FBB90 source-header/weight capacities differ.");
var initial = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
var profile = model.LanguageProfiles[model.ResourceLanguageProfileIndex];
var auxiliary = model.AuxiliaryProfiles[model.ResourceLanguageProfileIndex];
float scale = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964)).RuntimeLanguageScale;
int totalQueries = 0, totalRecords = 0, punctuation = 0, guarded = 0;

//This consumes every ordinary whole-prefix startup page, not a complete
//2144F0 recurrence. Allocation bytes are explicit owned test residues.
for (int node = 0; node < graph.NodeCount; node++)
{
    if (owner.GetSpanExtent(node) <= node) continue;
    var span = owner.Prepare(node, (byte)node);
    var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 128,
        Parameter1c = (uint)node, Parameter20 = (uint)node, ContextRoot = owner.Contexts.ContainerIdentity };
    var page = owner.Read(span, 8UL << 40, query, clearExtraCosts: true);
    var initialHeap = new LegacyNativeWordRecordHeap(250);
    var initialRouter = new LegacyNativeWordHeapRouter([initialHeap], preparedHeaps.CopySourceBuckets().Span, -1, uint.MaxValue);
    initial.ProcessPreparedPage(span, page.Records.Span, 0, initialRouter, policy.SecondaryScaleEc,
        policy.Refresh.ConsecutiveBlank22f7 != 0, false);
    var assigned = LegacyNativeWordAlternativeOrdinals.AssignInitial(initialHeap.GetSlotStorageSnapshot().Span, initialHeap.Count);
    var pools = LegacyNativeRootWordAlternativeProducer.Create(assigned.SlotStorage.Span, initialHeap.Count,
        (uint)node, configuration, owner.Contexts, symbol => symbol,
        Enumerable.Repeat((byte)0xa7, 250 * 80).ToArray(), []);
    var ordered = LegacyNativeWordAlternativeOrdering.Prepare(assigned.SlotStorage.Span, initialHeap.Count,
        false, configuration, headers, false, new(2, pools.PrimaryStorage.Span), null);
    var rows = new List<LegacyNativeStoredWordRecord>();
    for (ushort ordinal = 0; ordinal < ordered.Heap.Count; ordinal++)
        rows.Add(LegacyNativeWordRowMaterializer.Materialize(ordered.Heap.GetSlotSnapshot(ordinal).Span,
            ordinal, configuration.PrimaryArcLimit230a, 2, 1, ordered.PrimaryAlternativeStorage.Span));

    byte[] allocation = Enumerable.Repeat((byte)0x6d, 250 * 128).ToArray();
    var output = new LegacyNativeWordRecordHeap(250, allocation);
    var router = new LegacyNativeWordHeapRouter([output], preparedHeaps.CopySourceBuckets().Span, -1, uint.MaxValue);
    var completion = NewCompletion();
    var auxiliaryPages = new LegacyNativeAuxiliaryWordQueryPages(dictionary.DictionaryQueries,
        owner.Contexts, 128, new byte[128 * 32]);
    for (ushort ordinal = 0; ordinal < rows.Count; ordinal++)
    {
        ushort actualOrdinal = ordinal;
        var parent = rows[ordinal].Record;
        var result = auxiliaryPages.Run(parent.Span, node, new(0, 0, false, false, policy.MaximumParentDistance54),
            0, false, false, (actualPage, token) =>
        {
            for (int index = 0; index < actualPage.Records32.Length / 32; index++)
            {
                byte[] terminal = actualPage.Records32.Slice(index * 32, 32).ToArray();
                byte[] residue = Enumerable.Repeat((byte)0xa7, 40).ToArray();
                var actual = completion.ProcessCandidate(terminal, node, actualOrdinal, parent.Span,
                    router, policy.KnowledgeF5, residue, Lookup, false, token);
                Require(actual.LanguageAccepted && actual.Allocation.Accepted, "Full actual first-word source query was rejected.");

                int wordId = model.WordIndex.TryGetWordId(unchecked((int)U32(terminal, 0)), out int id)
                    ? id : throw new InvalidDataException("Actual terminal has no owned vocabulary ID.");
                //Ordinary source70/094040 clears token+8.204DA5/205368
                //transport that zero into LM request20. Sampler geometry is
                //used separately below for the previous-category path penalty.
                int category = 0;
                int wordClass = model.LanguageModel.GetWordClass(wordId);
                int wordCost = Transform(model.LanguageModel.GetStoredWordCost(wordId), profile.WordSlope, profile.WordOffset, scale);
                int classCost = Transform(model.LanguageModel.GetStoredClassCost(wordClass), profile.UnigramSlope, profile.UnigramOffset, scale);
                int categoryCost = category switch { 3 => 1, 5 or 6 => 2, 10 or 11 or 12 => 3, _ => 0 };
                var token16 = dictionary.WordMetadata.ReadTerminal(U64(terminal, 0) & ~(255UL << 32), 0).Span;
                Require(BinaryPrimitives.ReadInt32LittleEndian(token16[8..]) == 0,
                    "Ordinary source70 must retain its independent zero metadata category.");
                if (token16[13] != 0) categoryCost = 0;
                int boundary = token16[12] != 0
                    ? (int)((scale * auxiliary.BoundaryOffset - scale * auxiliary.WordOffset) * -8192f) : 0;
                int subtotal = checked(wordCost + categoryCost + boundary + classCost);
                Require(actual.Language.Score.LexicalSubtotal == subtotal && actual.Language.Score.WordCost == wordCost &&
                    actual.Language.Score.ClassCost == classCost && actual.Language.Score.CategoryCost == categoryCost &&
                    actual.Language.Score.BoundaryCost == boundary, "Original actual-resource scalar LM formula differs.");
                byte[] expectedLanguage = residue.ToArray();
                I32(expectedLanguage, 0, subtotal); I32(expectedLanguage, 4, 1); expectedLanguage[8] = 1;
                W32(expectedLanguage, 12, (uint)wordClass | (token16[13] != 0 ? 0u : (uint)category << 24));
                W32(expectedLanguage, 24, uint.MaxValue); I32(expectedLanguage, 28, wordId);
                Require(actual.Language.Record40.Span.SequenceEqual(expectedLanguage), "Original full40 language payload differs.");

                int previousCategory = categories.CompositeCategories.Span[parent.Span[26] & 63].FirstId;
                int correction = previousCategory is 2 or 4 ? 8192 : 0;
                int expectedPath = checked(Read32(parent.Span, 16) + Read32(terminal, 16) + correction);
                Require(actual.PreLexicalCost == expectedPath && actual.RestartTag28 == 8UL << 40,
                    "Original category/path/restart source policy differs.");
                byte[] expected = Enumerable.Repeat((byte)0x6d, 128).ToArray();
                expected.AsSpan(0, 120).Clear(); //1F8D60 allocation zeroes the defined prefix.
                parent.Span.CopyTo(expected);
                terminal.AsSpan(0, 8).CopyTo(expected);
                terminal.AsSpan(8, 8).CopyTo(expected.AsSpan(8));
                I32(expected, 16, checked(expectedPath + subtotal));
                BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(30), actualOrdinal);
                expected[32] = 255; expected[33] &= 192; expected[39] &= 254;
                W64(expected, 40, 8UL << 40);
                BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(62), ushort.MaxValue);
                expectedLanguage.AsSpan(8, 20).CopyTo(expected.AsSpan(64));
                W32(expected, 88, uint.MaxValue); W64(expected, 92, (0x4000000UL << 32) | uint.MaxValue);
                I32(expected, 104, -1); W64(expected, 112, 0);
                bool found = Enumerable.Range(0, output.Count).Any(i => output.GetSlotSnapshot(i).Span.SequenceEqual(expected));
                Require(found, "Original complete128 continuation record/residue differs.");
                if (model.AcousticAlphabet.TryGetText(U16(terminal, 20), out string text) && text == "(") punctuation++;
                totalQueries++; totalRecords++;
            }
            return true;
        }, 100);
        Require(result.Completed, "Unfiltered source auxiliary loop failed its native zero-count stop.");

        ReadOnlyMemory<byte> Lookup(int actualNode, ushort row)
        {
            Require(actualNode == node && row < rows.Count, "First-word lookup lost the actual node/ordinal owner.");
            return rows[row].Record;
        }
    }

    //Replacement-force and virtual protocol ownership stay explicit.
    var candidate = rows.Select((row, ordinal) => (row, ordinal))
        .First(pair => LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(pair.row.Record.Span, new byte[32]).HasValue);
    byte[] validQuery = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(candidate.row.Record.Span, new byte[32])!.Value.ToArray();
    ushort virtualSymbol = Enumerable.Range(0, model.AcousticAlphabet.LabelCount)
        .Select(i => (ushort)i).First(model.AcousticAlphabet.IsVirtual);
    Require(model.AcousticAlphabet.TryGetLabel("\ufffd", out ushort replacement), "Actual replacement label absent.");
    foreach (ushort forbidden in new[] { virtualSymbol, replacement, ushort.MaxValue })
    {
        byte[] bad = validQuery.ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(bad.AsSpan(20), forbidden);
        Reject(() => NewCompletion().ProcessCandidate(bad, node, (ushort)candidate.ordinal, candidate.row.Record.Span,
            router, false, new byte[40], (_, i) => rows[i].Record, false)); guarded++;
    }
    byte[] forcedParent = candidate.row.Record.ToArray();
    BinaryPrimitives.WriteUInt16LittleEndian(forcedParent.AsSpan(36), replacement);
    Reject(() => NewCompletion().ProcessCandidate(validQuery, node, (ushort)candidate.ordinal, forcedParent,
        router, false, new byte[40], (_, i) => rows[i].Record, false)); guarded++;
}
Require(totalQueries == 33 && totalRecords == 33 && punctuation == 1, "Actual full source language regression changed.");
Console.WriteLine($"PASS {totalQueries} actual unfiltered auxiliary/terminal queries ->{totalRecords} independent numeric/full40/full128 LM records; actual '('={punctuation}; {guarded} virtual/FFFD/missing-label guards. Whole-prefix root bridge, not complete main scheduling/winner.");

LegacyNativeFirstDictionaryWordCompletion NewCompletion() => new(model, dictionary.WordMetadata,
    dictionary.LanguageRecords, dictionary.RestartTags, 1, headers[1], false, 0, false, false);
static int Transform(int stored, float slope, float offset, float scale)
{
    float multiplied = ((float)stored * (1f / 8192)) * (scale * slope);
    return (int)((multiplied - scale * offset) * 8192f);
}
static uint U32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
static int Read32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
static void I32(Span<byte> value, int offset, int number) => BinaryPrimitives.WriteInt32LittleEndian(value[offset..], number);
static void W32(Span<byte> value, int offset, uint number) => BinaryPrimitives.WriteUInt32LittleEndian(value[offset..], number);
static void W64(Span<byte> value, int offset, ulong number) => BinaryPrimitives.WriteUInt64LittleEndian(value[offset..], number);
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); } catch (NotSupportedException) { return; }
    throw new InvalidDataException("Unsupported source force/protocol policy accepted.");
}
