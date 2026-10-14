using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 3)) throw new ArgumentException("controls.json [actual-B8-capture.json ru-ak.res] required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, outputs = 0, infinities = 0, zeroIdentities = 0;
void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException) { return; }
    throw new InvalidOperationException("Invalid/cancelled terminal-grid request was accepted.");
}
uint Bits(float value) => BitConverter.SingleToUInt32Bits(value);
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var storage = test.GetProperty("nodes").EnumerateArray().Select(item => Convert.FromBase64String(item.GetString()!)).ToArray();
    int width = test.GetProperty("width").GetInt32(), height = test.GetProperty("height").GetInt32();
    var grid = new LegacyFirstMergedRecordGrid(width, height, storage.Select(x => (ReadOnlyMemory<byte>)x).ToArray());
    var policy = new LegacyFirstTerminalSelectionPolicy(test.GetProperty("skip").GetUInt16(), test.GetProperty("penalty").GetInt32(),
        test.GetProperty("cfg99").GetBoolean(), test.GetProperty("cfgA8").GetBoolean(), test.GetProperty("kind").GetInt32(), test.GetProperty("reference").GetInt64());
    // Mutating caller input after construction must not change the owned grid.
    foreach (var data in storage) data.AsSpan().Fill(0xcd);
    var result = LegacyFirstTerminalSelection.Select(grid, policy);
    var expected = test.GetProperty("expected");
    Require(result.ReferenceCost78 == test.GetProperty("finalReference").GetInt64() && result.Choices.Count == expected.GetArrayLength(), "Terminal count or signed64 reference differs.");
    for (int index = 0; index < result.Choices.Count; index++)
    {
        var actual = result.Choices[index]; var item = expected[index]; var trait = item.GetProperty("trait");
        Require(actual.Row == item.GetProperty("row").GetInt32() && actual.Column == item.GetProperty("column").GetInt32() &&
            actual.TotalCost == item.GetProperty("cost").GetInt32() && actual.AcousticCost == item.GetProperty("acoustic").GetInt32() &&
            actual.ContextIdentity == item.GetProperty("identity").GetUInt64() &&
            actual.TraitNode == trait[0].GetInt32() && actual.TraitRow == trait[1].GetInt32() && actual.TraitColumn == trait[2].GetInt32(), "Native terminal prefix/trailing trait/ranking/identity differs.");
        var score = actual.Score;
        Require(score.RelativeCost == item.GetProperty("relative").GetInt32() && Bits(score.NormalizedCost10) == item.GetProperty("normalized").GetUInt32() &&
            Bits(score.LogMass) == item.GetProperty("log").GetUInt32() && Bits(score.NormalizedAcousticCost14) == item.GetProperty("acousticNormalized").GetUInt32(), "Scalar native float32 ordering or wrap differs.");
        uint mass = Bits(score.Mass08), reference = item.GetProperty("mass").GetUInt32();
        // Platform exp implementations can differ slightly; all surrounding
        // float32 arithmetic is compared bitwise, including zero and infinity.
        Require(mass == reference || float.IsFinite(score.Mass08) && reference < 0x7f800000 && Math.Abs((long)mass - reference) <= 2, "Portable CRT-exp mass exceeds twofloat32 ULPs.");
        if (float.IsInfinity(score.Mass08)) infinities++;
        if (actual.ContextIdentity == 0) zeroIdentities++;
        var copied = grid.CopyRecord(grid.NodeCount - 1, actual.Row, actual.Column).ToArray();
        Require(BinaryPrimitives.ReadInt32LittleEndian(copied.AsSpan(4)) == actual.AcousticCost, "Winning record snapshot lost source acoustic cost.");
        copied.AsSpan().Clear();
        Require(BinaryPrimitives.ReadInt32LittleEndian(grid.CopyRecord(grid.NodeCount - 1, actual.Row, actual.Column).Span[4..]) == actual.AcousticCost, "Winning snapshot exposed grid mutation.");
        outputs++;
    }
    cases++;
}
var one = new byte[40]; BinaryPrimitives.WriteInt32LittleEndian(one.AsSpan(4), 7);
var policyDefault = new LegacyFirstTerminalSelectionPolicy(99, 40960, false, false, 0x135, 0);
Reject(() => new LegacyFirstMergedRecordGrid(1, 1, []));
Reject(() => new LegacyFirstMergedRecordGrid(257, 1, [one]));
Reject(() => new LegacyFirstMergedRecordGrid(1, 1, [new byte[39]]));
Reject(() => new LegacyFirstMergedRecordGrid(256, 256, Enumerable.Repeat<ReadOnlyMemory<byte>>(new byte[256*256*40], 17).ToArray()));
Reject(() => new LegacyFirstMergedRecordGrid(1, 1, [one], new CancellationToken(true)));
var guard = new LegacyFirstMergedRecordGrid(1, 1, [one]);
Reject(() => LegacyFirstTerminalSelection.Select(guard, policyDefault, new CancellationToken(true)));
Reject(() => guard.CopyRecord(1, 0, 0));
BinaryPrimitives.WriteUInt16LittleEndian(one.AsSpan(12), 99);
BinaryPrimitives.WriteUInt16LittleEndian(one.AsSpan(14), 14); // root cannot follow span7 to a negative table.
Reject(() => LegacyFirstTerminalSelection.Select(new(1, 1, [one]), policyDefault));
BinaryPrimitives.WriteUInt16LittleEndian(one.AsSpan(14), 0); one[9] = 255; one[10] = 255;
Require(LegacyFirstTerminalSelection.Select(new(1, 1, [one]), policyDefault).Choices.Count == 1, "Trailing root consumed unused predecessor bytes.");
var later = (byte[])one.Clone(); later[9] = 1; later[10] = 0;
Reject(() => LegacyFirstTerminalSelection.Select(new(1, 1, [one, later]), policyDefault));
Require(infinities > 0 && zeroIdentities > 0, "Controls omitted native overflow or zero-context behavior.");
Console.WriteLine($"PASS {cases} independent terminal grids / {outputs} choices / {zeroIdentities} zero identities / {infinities} nativeexp infinities; stable signed ranking, trailing metadata, reference flags, full storage ownership and guards.");

int glyphCases = 0, glyphCellsCount = 0;
foreach (var fixture in controls.RootElement.GetProperty("glyphCases").EnumerateArray())
{
    var grid = new LegacyFirstMergedRecordGrid(fixture.GetProperty("width").GetInt32(), fixture.GetProperty("height").GetInt32(),
        fixture.GetProperty("nodes").EnumerateArray().Select(n => (ReadOnlyMemory<byte>)Convert.FromBase64String(n.GetString()!)).ToArray());
    var choice = LegacyFirstTerminalSelection.Select(grid, new(ushort.MaxValue, 0, false, false, 0x135, 0)).Choices[0];
    var trace = LegacyFirstSelectedGlyphTrace.Read(grid, choice);
    var expected = fixture.GetProperty("expected").EnumerateArray().ToArray();
    Require(trace.Count == expected.Length, "Independent causal glyph ancestry length differs.");
    for (int i = 0; i < trace.Count; i++)
    {
        var cell = trace[i]; var e = expected[i];
        Require(cell.Node == e.GetProperty("node").GetInt32() && cell.Row == e.GetProperty("row").GetInt32() && cell.Column == e.GetProperty("column").GetInt32() &&
            cell.PreviousNode == e.GetProperty("previous").GetInt32() && cell.PreviousArc == e.GetProperty("previousArc").GetUInt16() &&
            cell.CopyRecord().Span.SequenceEqual(Convert.FromHexString(e.GetProperty("record").GetString()!)), "Independent glyph cell/source identity differs.");
    }
    Reject(() => LegacyFirstSelectedGlyphTrace.Read(grid, choice with { ContextIdentity = choice.ContextIdentity + 100000 }));
    Reject(() => LegacyFirstSelectedGlyphTrace.Read(grid, choice, new CancellationToken(true)));
    glyphCases++; glyphCellsCount += trace.Count;
}
Console.WriteLine($"PASS {glyphCases} causal ancestry fixtures/{glyphCellsCount} full selected glyph cells, source order, root sentinel, identity and cancellation. Glyph alternatives/source ranges remain separate.");

if (args.Length == 3)
{
    using var capture = JsonDocument.Parse(File.ReadAllBytes(args[1]));
    var root = capture.RootElement;
    var storage = root.GetProperty("nodes").EnumerateArray().Select(n => (ReadOnlyMemory<byte>)Convert.FromHexString(n.GetString()!)).ToArray();
    var actualGrid = new LegacyFirstMergedRecordGrid(root.GetProperty("columnCount").GetInt32(), root.GetProperty("beamWidth").GetInt32(), storage);
    var alphabet = LegacyFirstTransitionGeometryProfile.ReadSource(File.ReadAllBytes(args[2]), 6120).Alphabet;
    ushort trailing = alphabet.TryGetLabel("\u0651", out ushort label) ? label : ushort.MaxValue;
    for (int node = 0; node < actualGrid.NodeCount; node++)
        for (int row = 0; row < actualGrid.RowCountF8; row++)
            for (int column = 0; column < actualGrid.ColumnCountA4; column++)
            {
                var record = actualGrid.CopyRecord(node, row, column).Span;
                if (BinaryPrimitives.ReadInt32LittleEndian(record) >= 0x4000000) continue;
                Require(record[32] == 0 && BinaryPrimitives.ReadUInt16LittleEndian(record[12..]) != trailing,
                    "The actual fixture no longer satisfies policy-independent zero-trait/plain-label ranking.");
            }
    Require(root.GetProperty("modeE0").GetBoolean() == root.GetProperty("selectedChildE0").GetBoolean(),
        "This consumer requires the actual selected-child mode capture, not a controlled alternate mode.");
    var binding = LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(File.ReadAllBytes(args[2]), 6120);
    Require(root.GetProperty("sourceKind").GetInt32() == binding.SourceKind, "Capture source kind differs from its owned model.");
    Require(root.GetProperty("selectedChildE0").GetBoolean() == binding.NativeModeE0, "Capture mode differs from its owned new-child binding.");
    //99/A8 are captured from the proved new-direct source invocation. The zero
    //reference/unused penalty are explicit result fixture inputs, not generic defaults.
    var selected = LegacyFirstTerminalSelection.Select(actualGrid, new(trailing, 0,
        root.GetProperty("invocation99").GetBoolean(), root.GetProperty("invocationA8").GetBoolean(), binding.SourceKind, 0));
    var alternativePolicy = LegacyFirstTerminalSelection.Select(actualGrid, new(trailing, 40960, true, true, binding.SourceKind, 123));
    Require(selected.Choices.Select(c => (c.Row, c.Column, c.ContextIdentity, c.TotalCost)).SequenceEqual(
        alternativePolicy.Choices.Select(c => (c.Row, c.Column, c.ContextIdentity, c.TotalCost))), "Fixture policy changed native selected cells.");
    var contexts = root.GetProperty("contexts").EnumerateArray().ToDictionary(n => n.GetProperty("identity").GetUInt64(),
        n => (Parent: n.GetProperty("parent").GetUInt64(), Symbol: n.GetProperty("symbol").GetUInt16()));
    var text = new List<string>();
    foreach (var choice in selected.Choices)
    {
        var glyphCells = LegacyFirstSelectedGlyphTrace.Read(actualGrid, choice);
        Require(glyphCells.Count > 0 && glyphCells.Zip(glyphCells.Skip(1)).All(pair => pair.First.Node < pair.Second.Node),
            "Actual selected glyph cells are not in source order.");
        var reverse = new List<string>(); var seen = new HashSet<ulong>();
        var reversedSymbols = new List<ushort>();
        for (ulong id = choice.ContextIdentity; id != 0;)
        {
            Require(seen.Add(id) && contexts.ContainsKey(id), "Actual selected context is cyclic/unavailable.");
            var context = contexts[id];
            reversedSymbols.Add(context.Symbol);
            Require(context.Symbol == 0xfffb || alphabet.TryGetText(context.Symbol, out _), "Selected symbol is not in its owned alphabet.");
            reverse.Add(context.Symbol == 0xfffb ? " " : alphabet.TryGetText(context.Symbol, out string value) ? value : throw new Exception());
            id = context.Parent;
        }
        reversedSymbols.Reverse();
        Require(glyphCells.Select(cell => cell.Symbol).SequenceEqual(reversedSymbols),
            "Actual plain fixture's merged glyph walk differs from its independently owned symbol-context path.");
        reverse.Reverse(); text.Add(string.Concat(reverse) + ":" + choice.TotalCost);
    }
    Console.WriteLine($"PASS real raw/NN/B0/B8 -> terminal selected contexts [{string.Join(", ", text)}], proved new-direct invocation flags. Not273 glyph materialization or final Russian recognition.");
}
