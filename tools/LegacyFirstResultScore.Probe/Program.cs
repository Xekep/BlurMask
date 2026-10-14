using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("Independent score publication controls required.");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, rows = 0, nan = 0;
foreach (var control in controls.RootElement.EnumerateArray())
{
    byte[] source = Convert.FromHexString(control.GetProperty("input").GetString()!);
    byte[] expected = Convert.FromHexString(control.GetProperty("expected").GetString()!);
    int prior = control.GetProperty("prior").GetInt32();
    byte[] actual = LegacyFirstResultScorePublication.Publish(source, prior);
    int count = source.Length / 40;
    for (int row = 0; row < count; row++)
        for (int offset = 0; offset < 40; offset++)
        {
            bool mass = offset is >= 8 and < 12;
            bool acoustic = offset is >= 12 and < 16;
            bool changed = row >= prior && prior < count && (mass || acoustic);
            if (!changed)
            {
                if (actual[row * 40 + offset] != source[row * 40 + offset]) throw new Exception("An opaque or prior-row byte changed.");
                continue;
            }
            if (offset is not (8 or 12)) continue;
            uint a = BinaryPrimitives.ReadUInt32LittleEndian(actual.AsSpan(row * 40 + offset));
            uint e = BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(row * 40 + offset));
            float af = BitConverter.UInt32BitsToSingle(a), ef = BitConverter.UInt32BitsToSingle(e);
            if (float.IsNaN(ef)) { if (!float.IsNaN(af)) throw new Exception("Native IEEE unordered result changed."); nan++; }
            else if (a != e && !(offset == 12 && float.IsFinite(af) && float.IsFinite(ef) && af > 0 && ef > 0 && Math.Abs((long)a - e) <= 2))
                throw new Exception($"Scalar mismatch row{row}/offset{offset}: {a:x8}!={e:x8}");
        }
    if (!actual.AsSpan().SequenceEqual(LegacyFirstResultScorePublication.Publish(source, prior))) throw new Exception("Publication mutates caller storage.");
    cases++; rows += count;
}
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or OperationCanceledException) { return; }
    throw new Exception("Invalid publication request accepted.");
}
Reject(() => LegacyFirstResultScorePublication.Publish(new byte[41], 0));
Reject(() => LegacyFirstResultScorePublication.Publish(new byte[40], 2));
Reject(() => LegacyFirstResultScorePublication.Publish(new byte[40], -1));
Reject(() => LegacyFirstResultScorePublication.Publish(new byte[4097 * 40], 0));
Reject(() => LegacyFirstResultScorePublication.Publish(new byte[40], 0, new CancellationToken(true)));
Console.WriteLine($"PASS {cases} independent score publication batches/{rows} full rows/{nan} IEEE NaNs; pairwise anchor, acoustic expf, prior/opaque preservation and guards. Not final OCR confidence.");
