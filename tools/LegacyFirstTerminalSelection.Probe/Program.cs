using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("controls.json required");
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
