using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathInsertedUnits.Probe")]
namespace InkRecognition.Math;

/// <summary>Prepared finite041240 insertion and initial mask. Arrays are explicit
/// numerical inputs, not evidence of an actual request or source selection.</summary>
internal sealed class LegacyPreparedMathInsertedUnits
{
    private readonly uint[] units;
    private readonly int[] assignments;
    private readonly byte[] insertionMask;
    internal uint[] CopyUnits() => units.ToArray();
    internal int[] CopyAssignments() => assignments.ToArray();
    internal byte[] CopyInsertionMask() => insertionMask.ToArray();
    private LegacyPreparedMathInsertedUnits(uint[] units, int[] assignments, byte[] mask)
    { this.units = units; this.assignments = assignments; insertionMask = mask; }

    internal static LegacyPreparedMathInsertedUnits EvaluatePrepared(ReadOnlySpan<uint> existing,
        ReadOnlySpan<uint> initialUnits, ReadOnlySpan<int> originalAssignments,
        ReadOnlySpan<int> observableOriginalRows, ReadOnlySpan<float> activated,
        ushort replacement, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        int s = originalAssignments.Length, n = observableOriginalRows.Length;
        if (s > 6000 || initialUnits.Length > 1000 || n > initialUnits.Length ||
            (existing.Length != 0 && existing.Length != s) ||
            (long)n * s > 1_000_000 || activated.Length != (long)n * s)
            throw new ArgumentException("Bounded prepared segmentation matrix and matching source columns required.");
        int previous = -1;
        foreach (int row in observableOriginalRows)
        {
            if (row <= previous || row >= initialUnits.Length)
                throw new ArgumentException("Increasing observable original rows required.");
            previous = row;
        }
        foreach (int value in originalAssignments)
            if (value < -1 || value >= initialUnits.Length)
                throw new ArgumentException("Assignments must be absent or refer to initial output units.");
        foreach (float value in activated)
            if (!float.IsFinite(value) || value < 0 || value > 1)
                throw new ArgumentException("Finite activated probabilities required.");
        // Preflight an upper bound. There cannot be more inserted units than
        // absent assignments. This explicit guard precedes copied payloads.
        int absent = 0;
        foreach (int value in originalAssignments) if (value == -1) absent++;
        if (initialUnits.Length + absent > 1000)
            throw new NotSupportedException("Prepared insertion envelope exceeds1000 output units.");
        int[] assignments = originalAssignments.ToArray();
        var output = new List<uint>(initialUnits.ToArray());
        var mask = Enumerable.Repeat((byte)0, initialUnits.Length).ToList();
        for (int current = 0; current < s; current++)
        {
            token.ThrowIfCancellationRequested();
            if (assignments[current] != -1) continue;
            int at = current == 0 ? 0 : assignments[current - 1] + 1;
            if ((uint)at > (uint)output.Count)
                throw new NotSupportedException("Insertion cursor must refer to the current output sequence.");
            uint unit;
            if (existing.Length != 0 && (existing[current] >> 24) == 0)
            {
                if (current + 1 >= s || (existing[current + 1] >> 24) != 0 || assignments[current + 1] != -1)
                    throw new NotSupportedException("A missing zero-flag source pair must contain two absent assignments.");
                unit = existing[current];
                for (int i = 0; i < s; i++) if (assignments[i] >= at) assignments[i]++;
                assignments[current] = at; assignments[current + 1] = at;
            }
            else
            {
                unit = 0x01000000u | replacement;
                if (existing.Length != 0 && n != 0)
                {
                    int selected = 0; float best = -float.MaxValue;
                    for (int row = 0; row < n; row++)
                    {
                        float sum = 0;
                        for (int column = 0; column < s; column++)
                            if ((existing[column] >> 24) != 0) sum += activated[row * s + column];
                        if (sum > best) { best = sum; selected = row; }
                    }
                    unit = output[observableOriginalRows[selected]];
                }
                //041240 fills ALL remaining absent assignments, including
                //nonadjacent positions, in this branch. It is not run-local.
                for (int i = 0; i < s; i++)
                    if (assignments[i] >= at) assignments[i]++;
                    else if (assignments[i] == -1) assignments[i] = at;
            }
            output.Insert(at, unit); mask.Insert(at, 1);
        }
        token.ThrowIfCancellationRequested();
        return new(output.ToArray(), assignments, mask.ToArray());
    }
}
