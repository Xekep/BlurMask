using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 10) throw new ArgumentException("controls AK LK engine separation case publication line30 line50 capture");
int guards = 0, equality = 0;
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
using var output = File.Create(args[9]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject(); writer.WriteStartArray("comparisons");
foreach (var control in controls.RootElement.EnumerateArray())
{
    var pool = LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    int descriptor = 0;
    var left = ReadWords(control.GetProperty("a"));
    var right = ReadWords(control.GetProperty("b"));
    byte[] sa = ReadBytes(control.GetProperty("sa")), sb = ReadBytes(control.GetProperty("sb"));
    string result;
    try { result = LegacyNativeMainAlternativeEquality.Compare(pool, left, right, sa, sb, Lookup) ? "true" : "false"; }
    catch (NotSupportedException error) when (error.Message.StartsWith("Native all-active")) { result = "unsupported"; }
    string expected = control.GetProperty("expected").ValueKind == JsonValueKind.String
        ? control.GetProperty("expected").GetString()! : control.GetProperty("expected").GetBoolean() ? "true" : "false";
    Require(result == expected && sa.SequenceEqual(ReadBytes(control.GetProperty("ea"))) &&
        sb.SequenceEqual(ReadBytes(control.GetProperty("eb"))), "Independent skip/stream/auxiliary/scratch result differs.");
    writer.WriteStartObject(); writer.WriteString("result", result);
    writer.WriteString("left4", Convert.ToHexString(sa)); writer.WriteString("right4", Convert.ToHexString(sb)); writer.WriteEndObject();
    equality++;

    LegacyNativePlainWordRecord[] ReadWords(JsonElement list)
    {
        var words = new List<LegacyNativePlainWordRecord>();
        foreach (var entry in list.EnumerateArray())
        {
            var refs = new List<ulong>();
            foreach (var glyph in entry.GetProperty("g").EnumerateArray())
            {
                int id = ++descriptor;
                ulong reference = pool.AppendDescriptor((ulong)id, id, false, new byte[40]).Reference;
                byte[] row = pool.CopyRecord(reference);
                BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4), (ushort)glyph[0].GetInt32());
                row[6] = (byte)glyph[2].GetInt32();
                BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8), (ushort)glyph[1].GetInt32());
                pool.ReplaceRecord(reference, row); refs.Add(reference);
            }
            byte[] data = new byte[120];
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0x28), refs.Count);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0x3c), entry.GetProperty("f").GetInt32());
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0x60), entry.GetProperty("aux").GetBoolean() ? 1 : 0);
            words.Add(new(pool, data, refs.ToArray()));
        }
        return words.ToArray();
    }
}
writer.WriteEndArray();
for (int n = 0; n < 4090; n++) Require(LegacyOwnedMainAlternativeAssembly.NextCapacity(n) == ((n / 10) + 1) * 10,
    "Native ten-row capacity rounding differs.");

//Separate prepared275 identity controls, with exact shared heads rather than
//assuming that replayed chain production must reuse a64-bit glyph head.
var preparedPool = LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var preparedWord = LegacyNativeWordBuilder.CreateEmpty(preparedPool, new byte[120]);
preparedWord.AppendGlyph(0, 0, false, new byte[40]); preparedWord.SetLastOrdinarySymbol(7);
var preparedPublisher = new LegacyNativePlainWordPublisher(preparedPool, true, false, false);
var originalNode = preparedPublisher.Publish(preparedWord, (_, _) => 1, (_, _) => 9);
var childPublisher = preparedPublisher.ForkAlternative();
var sharedNode = childPublisher.Publish(preparedWord, (_, _) => 1, (_, _) => 9);
Require(ReferenceEquals(originalNode.Word, sharedNode.Word) && sharedNode.WordIndex == 0 &&
    !ReferenceEquals(originalNode, sharedNode) && sharedNode.Next is null,
    "Same native head/span did not intern one shared275 across independent279 lists.");
var otherWord = LegacyNativeWordBuilder.CreateEmpty(preparedPool, new byte[120]);
otherWord.AppendGlyph(1, 0, false, new byte[40]); otherWord.SetLastOrdinarySymbol(7);
var otherNode = childPublisher.Publish(otherWord, (_, _) => 1, (_, _) => 9);
Require(otherNode.WordIndex == 1 && !ReferenceEquals(otherNode.Word, sharedNode.Word), "Distinct head did not extend the shared span table.");
var grandchild = childPublisher.ForkAlternative();
var otherReuse = grandchild.Publish(otherWord, (_, _) => 1, (_, _) => 9);
Require(otherReuse.WordIndex == 1 && ReferenceEquals(otherReuse.Word, otherNode.Word), "Nested fork lost shared275 span indexes.");
Require((BinaryPrimitives.ReadUInt16LittleEndian(otherReuse.Word.CopyStorage().AsSpan(50)) & 0x400) != 0 &&
    BinaryPrimitives.ReadUInt16LittleEndian(otherReuse.Word.CopyStorage().AsSpan(50)) ==
    BinaryPrimitives.ReadUInt16LittleEndian(originalNode.Word.CopyStorage().AsSpan(50)), "Fork lost private C4 policy.");
childPublisher.ReleaseAlternativeList();
Require(childPublisher.Head is null && ReferenceEquals(preparedPublisher.Head, originalNode) &&
    ReferenceEquals(grandchild.Head!.Word, otherNode.Word), "Release cleared another279 list or shared275 storage.");
Reject(() => childPublisher.Publish(preparedWord, (_, _) => 1, (_, _) => 9));
Reject(() => childPublisher.ForkAlternative());
Reject(() => childPublisher.ReleaseAlternativeList());
Reject(() => LegacyNativeMainAlternativeEquality.Compare(preparedPool, [], [originalNode.Word], new byte[4], new byte[4]));
Reject(() => LegacyNativeMainAlternativeEquality.Compare(preparedPool, [originalNode.Word], [originalNode.Word], new byte[4], new byte[3]));
Reject(() => LegacyNativeMainAlternativeEquality.Compare(preparedPool, [originalNode.Word], [originalNode.Word], new byte[4], new byte[4], null, new CancellationToken(true)));
var foreignPool = LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
Reject(() => LegacyNativeMainAlternativeEquality.Compare(foreignPool, [originalNode.Word], [originalNode.Word], new byte[4], new byte[4]));
using (var cancel = new CancellationTokenSource())
{
    byte[] row = preparedPool.CopyRecord(preparedWord.HeadReference);
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4), 3);
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8), 4);
    preparedPool.ReplaceRecord(preparedWord.HeadReference, row);
    byte[] leftScratch = [1,2,3,4], rightScratch = [5,6,7,8];
    int calls = 0;
    Reject(() => LegacyNativeMainAlternativeEquality.Compare(preparedPool, [originalNode.Word], [originalNode.Word],
        leftScratch, rightScratch, _ => { if (++calls == 2) cancel.Cancel(); return 10; }, cancel.Token));
    Require(calls == 2 && leftScratch.SequenceEqual(new byte[] {10,0,0,4}) && rightScratch.SequenceEqual(new byte[] {10,0,0,8}),
        "Late lookup cancellation lost the actual scalar scratch prefix.");
}

byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]), engine = File.ReadAllBytes(args[3]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4]));
using var cf = File.OpenRead(args[5]); using var pf = File.OpenRead(args[6]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] spatial = File.ReadAllBytes(args[7]), physical = File.ReadAllBytes(args[8]);
var raw = LegacyFreshMainSingleLinePlainRecognition.Prepare(ak, lk, engine, spatial, physical,
    Word(0), 0, table, cases, publication).Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
byte[] residue = Enumerable.Range(0, 400).Select(i => (byte)(i * 37 + 19)).ToArray();
var first = LegacyFreshMainFirstCandidateCompletion.Complete(raw, residue, new byte[4], new byte[4]);
var completed = LegacyOwnedMainCompletedAlternative.FromFirstCandidate(first);
var top = raw.Session.Ownership;
int allocationCalls = 0;
var assembly = LegacyOwnedMainAlternativeAssembly.Prepare(top, request =>
{
    allocationCalls++;
    Require(request.PreviousCapacity == 0 && request.RequiredCapacity == 10, "Actual duplicate replay grew native count beyond one.");
    return residue.ToArray();
});
Reject(() => assembly.Consider(completed, new byte[3], new byte[4]));
Reject(() => assembly.Consider(completed, new byte[4], new byte[4], new CancellationToken(true)));
var initial = assembly.Consider(completed, new byte[4], new byte[4]);
Require(!initial.Duplicate && initial.ActiveCount == 1 && assembly.NativeFreshSecondaryLimit == 1,
    "Actual native fresh count or first insertion changed.");
var foreignRaw = LegacyFreshMainSingleLinePlainRecognition.Prepare(ak, lk, engine, spatial, physical,
    Word(0), 0, table, cases, publication).Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
var foreignDone = LegacyFreshMainFirstCandidateCompletion.Complete(foreignRaw, residue, new byte[4], new byte[4]);
var foreignCandidate = LegacyOwnedMainCompletedAlternative.FromFirstCandidate(foreignDone);
Reject(() => assembly.Consider(foreignCandidate, new byte[4], new byte[4]));
Require(!assembly.IsFaulted && assembly.ActiveCount == 1, "Pure foreign-TOP rejection damaged a valid assembly.");
using (var cancel = new CancellationTokenSource())
{
    var failed = LegacyOwnedMainAlternativeAssembly.Prepare(foreignRaw.Session.Ownership,
        _ => { cancel.Cancel(); return residue.ToArray(); });
    Reject(() => failed.Consider(foreignCandidate, new byte[4], new byte[4], cancel.Token));
    Require(failed.IsFaulted && failed.ActiveCount == 0 && failed.CopyRawStorage().Length == 0,
        "Allocation-callback cancellation did not retain the failed prefix and fault the assembly.");
    Reject(() => failed.Consider(foreignCandidate, new byte[4], new byte[4]));
    Reject(() => failed.FinalizeScores());
}
byte[] firstStorage = assembly.CopyRawStorage();
var originalWords = new List<(LegacyNativePlainWordRecord Word, byte[] Storage, ulong[] References, byte[][] Glyphs)>();
for (var node = top.WordPublisher.Head; node is not null; node = node.Next)
    originalWords.Add((node.Word, node.Word.CopyStorage(), node.Word.FlatReferences.ToArray(),
        node.Word.FlatReferences.Select(reference => top.GlyphPool.CopyRecord(reference)).ToArray()));
writer.WriteStartArray("actualSameTopReplays");
for (int iteration = 0; iteration < 4; iteration++)
{
    var fork = (iteration == 3 ? top.ForkAlternative() : top).ForkAlternative();
    var session = LegacyOwnedMainGlyphFlushSession.Prepare(fork, raw.Session.Plan, new byte[120]);
    var callbacks = LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak, top.Geometry.Owner, raw.Sampled);
    var interior = LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak, callbacks, top.Geometry);
    var publisher = LegacyOwnedMainPlainWordPublication.Prepare(ak, session, table, callbacks, interior);
    var publications = new List<LegacyOwnedMainWordPublicationResult>();
    while (!session.IsComplete)
    {
        var current = session.PrepareNext();
        var result = publisher.Publish(current, new byte[40], new ushort[256], new byte[64]);
        publications.Add(result); session.CompleteCurrent(result);
    }
    Require(ReferenceEquals(fork.TopOwner, top) && ReferenceEquals(fork.GlyphPool, top.GlyphPool) &&
        !ReferenceEquals(fork.WordPublisher, top.WordPublisher), "Fork changed actual TOP or reused independent279 publisher.");
    var a = top.WordPublisher.Head; var b = fork.WordPublisher.Head;
    while (a is not null && b is not null)
    {
        Require(!ReferenceEquals(a, b) && a.Span == b.Span,
            "Same TOP source span or independent279 node was lost.");
        if (a.Word.HeadReference == b.Word.HeadReference)
            Require(ReferenceEquals(a.Word, b.Word) && a.WordIndex == b.WordIndex,
                "Identical64-bit head did not reuse the shared TOP/275 WordIndex.");
        a = a.Next; b = b.Next;
    }
    Require(a is null && b is null, "Real callback replay omitted a WORD node.");
    var candidate = LegacyOwnedMainCompletedAlternative.CompleteActualCallbacks(raw.Terminal, session, publications,
        residue, new byte[4], new byte[4]);
    var result2 = assembly.Consider(candidate, new byte[4], new byte[4]);
    Require(result2.Duplicate && result2.DuplicateOf == 0 && result2.ActiveCount == 1 && candidate.IsReleased,
        "Actual same-TOP callback replay was not discarded as duplicate.");
    Require(fork.WordPublisher.Head is null, "Native1415B0 did not clear the discarded279 head.");
    Require(ReferenceEquals(top.WordPublisher.Head, first.Snapshot.WordHead18), "Discard changed original279 head.");
    foreach (var original in originalWords)
    {
        Require(original.Word.CopyStorage().SequenceEqual(original.Storage) &&
            original.Word.FlatReferences.SequenceEqual(original.References), "Discard changed a retained shared275 WORD.");
        for (int i = 0; i < original.References.Length; i++)
        {
            byte[] after = top.GlyphPool.CopyRecord(original.References[i]);
            Require(after.AsSpan(4, 6).SequenceEqual(original.Glyphs[i].AsSpan(4, 6)),
                "Duplicate callback replay changed an original WORD's raw glyph scalar/traits.");
        }
    }
    byte[] storage = assembly.CopyRawStorage();
    Require(storage.AsSpan(0, 40).SequenceEqual(firstStorage.AsSpan(0, 40)) &&
        storage.AsSpan(40, 40).SequenceEqual(firstStorage.AsSpan(0, 40)) &&
        storage.AsSpan(80).SequenceEqual(residue.AsSpan(80)), "Raw40 duplicate/inactive allocator bytes differ.");
    Reject(() => assembly.Consider(candidate, new byte[4], new byte[4]));
    writer.WriteStartObject(); writer.WriteString("storage", Convert.ToHexString(storage));
    writer.WriteNumber("words", candidate.Snapshot.PublishedWordCount); writer.WriteBoolean("shared275", true); writer.WriteEndObject();
}
writer.WriteEndArray();
Reject(() => assembly.Consider(completed, new byte[4], new byte[4]));
Reject(() => LegacyOwnedMainAlternativeAssembly.Prepare(top, _ => new byte[400]));
Reject(() => LegacyOwnedMainCompletedAlternative.FromFirstCandidate(first));
Require(allocationCalls == 1 && assembly.ActiveCount == 1 && !assembly.IsFaulted, "Duplicate lifecycle allocated or poisoned the TOP.");
var normalized = assembly.FinalizeScores();
Require(normalized.ActiveCount == 1 && normalized.Top80 == -1f, "Fresh normalization changed scalar policy.");
Reject(() => assembly.FinalizeScores());
Reject(() => assembly.Consider(completed, new byte[4], new byte[4]));
writer.WriteString("allocation", Convert.ToHexString(residue));
writer.WriteString("firstStorage", Convert.ToHexString(firstStorage));
writer.WriteString("finalStorage", Convert.ToHexString(normalized.CopyStorage()));
writer.WriteNumber("total", raw.Terminal.Winner.TotalCost); writer.WriteNumber("row14", raw.Terminal.Winner.RowCost14);
writer.WriteNumber("sourceNodes", raw.Sampled.SourceRanges.Graph.NodeCount);
writer.WriteNumber("guards", guards); writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"PASS{equality} independent comparison controls/4090 capacities/4 actual same-TOP callbacks/shared275/duplicate releases/{guards} guards. Fresh native alternative count remains1.");

void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid lifecycle or canceled input was admitted.");
}
static byte[] ReadBytes(JsonElement array) => array.EnumerateArray().Select(v => (byte)v.GetInt32()).ToArray();
static ushort Lookup(string value) => value switch { "\n" => 10, " " => 32, "\u00a0" => 160, _ => throw new Exception() };
static InkStroke[] Word(float x) => [Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),
    Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)), Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
static InkStroke Draw(params (float X,float Y)[] knots)
{
    var points = new List<InkPoint>();
    for (int i = 1; i < knots.Length; i++) for (int t = 0; t < 8; t++)
    { float f=t/8f; points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f)); }
    points.Add(new(knots[^1].X,knots[^1].Y)); return new(points.ToArray());
}
