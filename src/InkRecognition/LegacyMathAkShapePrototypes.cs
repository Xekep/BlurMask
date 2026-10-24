using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Immutable actual13A6 prototype data for the selected299 window0
/// geometry source. It grants no classifier, grammar or candidate authority.</summary>
public sealed class LegacyMathAkShapePrototypes
{
    private readonly short[] values;
    private readonly ushort[] targets;
    private readonly float[] factors, weights;
    private readonly int[] labels, targetStarts;
    private LegacyMathAkShapePrototypes(short[] values, ushort[] targets, float[] factors,
        int[] labels, int[] targetStarts, float[] weights)
    { this.values = values; this.targets = targets; this.factors = factors;
      this.labels = labels; this.targetStarts = targetStarts; this.weights = weights; }
    public int LabelCount => 259;
    public int PrototypeCount => 39;
    public int Rows => 30;
    public int Channels => 7;
    public int Window => 0;
    public int RetainedPayloadBytes => 17896;
    public bool HasClassifierAuthority => false;
    //Only the owning profile calls this after authenticating its private snapshot.
    internal static LegacyMathAkShapePrototypes ReadAuthenticated(ReadOnlySpan<byte> snapshot,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (snapshot.Length != LegacyMathMixed107RawResource.ResourceBytes)
            throw new InvalidDataException("Actual shape source resource length differs.");
        int at = 1772528;
        static void Header(ReadOnlySpan<byte> b, int at, uint length, uint kind)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(b[at..]) != 0x0101fe90 ||
                BinaryPrimitives.ReadUInt32LittleEndian(b[(at + 4)..]) != length ||
                BinaryPrimitives.ReadUInt32LittleEndian(b[(at + 8)..]) != kind ||
                BinaryPrimitives.ReadUInt16LittleEndian(b[(at + 12)..]) != 0 ||
                BinaryPrimitives.ReadUInt16LittleEndian(b[(at + 14)..]) != 0x100)
                throw new InvalidDataException("Actual shape source header differs.");
        }
        Header(snapshot, at, 17003, 0x13a6); Header(snapshot, at + 16, 43, 0x299);
        at += 32;
        if (BinaryPrimitives.ReadUInt16LittleEndian(snapshot[at..]) != 30 ||
            BinaryPrimitives.ReadUInt16LittleEndian(snapshot[(at + 2)..]) != 0 ||
            snapshot[at + 4] != 4 || snapshot[at + 5] != 1 || snapshot[at + 6] != 127)
            throw new NotSupportedException("Only actual299 window0 source is supported.");
        var weights = new float[7]; at += 7;
        for (int c = 0; c < 7; c++, at += 4)
        {
            weights[c] = BinaryPrimitives.ReadSingleLittleEndian(snapshot[at..]);
            if (weights[c] != (c == 0 ? 1.5f : c < 3 ? 3f : 1f))
                throw new InvalidDataException("Actual shape component weights differ.");
        }
        if (at != 1772595 || BinaryPrimitives.ReadUInt16LittleEndian(snapshot[at..]) != 259)
            throw new InvalidDataException("Actual shape label count differs.");
        at += 2;
        var values = new short[39 * 210]; var targets = new ushort[66];
        var factors = new float[39]; var labels = new int[260]; var starts = new int[40];
        int next = 0, target = 0;
        for (int label = 0; label < 259; label++)
        {
            token.ThrowIfCancellationRequested(); labels[label] = next;
            int count = snapshot[at++];
            if (count > 10 || count > 39 - next)
                throw new InvalidDataException("Actual shape prototype bound differs.");
            for (int j = 0; j < count; j++, next++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < 210; c++, at += 2)
                    values[next * 210 + c] = BinaryPrimitives.ReadInt16LittleEndian(snapshot[at..]);
                int countTargets = snapshot[at++]; starts[next] = target;
                if (countTargets > 3 || countTargets > 66 - target)
                    throw new InvalidDataException("Actual shape target bound differs.");
                for (int k = 0; k < countTargets; k++, at += 2)
                {
                    ushort id = BinaryPrimitives.ReadUInt16LittleEndian(snapshot[at..]);
                    if (id >= 259 && id != ushort.MaxValue)
                        throw new InvalidDataException("Actual shape target label differs.");
                    targets[target++] = id;
                }
                if (countTargets != 0)
                {
                    factors[next] = BinaryPrimitives.ReadSingleLittleEndian(snapshot[at..]); at += 4;
                    if (factors[next] != 0.75f)
                        throw new InvalidDataException("Actual shape mixture factor differs.");
                }
            }
        }
        labels[259] = next; starts[39] = target;
        if (next != 39 || target != 66 || at != 1789539)
            throw new InvalidDataException("Actual complete shape source differs.");
        token.ThrowIfCancellationRequested();
        return new(values, targets, factors, labels, starts, weights);
    }
    public int GetAlternativeCount(int label)
    { ValidateLabel(label); return labels[label + 1] - labels[label]; }
    private void ValidateLabel(int label)
    { if ((uint)label >= 259) throw new ArgumentOutOfRangeException(nameof(label)); }
    private int Index(int label, int alternative)
    {
        ValidateLabel(label);
        if ((uint)alternative >= labels[label + 1] - labels[label])
            throw new ArgumentOutOfRangeException(nameof(alternative));
        return labels[label] + alternative;
    }
    public short[] CopyPrototypeValues(int label, int alternative)
        => values.AsSpan(Index(label, alternative) * 210, 210).ToArray();
    public ushort[] CopyTargets(int label, int alternative)
    { int index = Index(label, alternative); return targets.AsSpan(targetStarts[index], targetStarts[index + 1] - targetStarts[index]).ToArray(); }
    public float? GetFactor(int label, int alternative)
    { int i = Index(label, alternative); return targetStarts[i] == targetStarts[i + 1] ? null : factors[i]; }
    internal float ComponentWeight(int channel) => weights[channel];
    internal float Measure(int prototype, ReadOnlySpan<short> feature, CancellationToken token)
    {
        if ((uint)prototype >= 39 || feature.Length != 210) throw new ArgumentException("Actual shape dimensions required.");
        int sum = 0;
        for (int c = 0; c < 210; c++)
        { if ((c & 31) == 0) token.ThrowIfCancellationRequested();
          sum += Math.Abs((int)feature[c] - values[prototype * 210 + c]); }
        //03D1DE window0 branch. sum <=210*65535<2^24, denominator exact.
        return (float)sum / (30 << 13);
    }
    internal (int Start, int End) Alternatives(int label)
    { ValidateLabel(label); return (labels[label], labels[label + 1]); }
}
