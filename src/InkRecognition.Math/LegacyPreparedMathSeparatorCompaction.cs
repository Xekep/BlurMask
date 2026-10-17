using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathSeparatorCompaction.Probe")]

namespace InkRecognition.Math;

/// <summary>Prepared041BA0 four-byte output units and original-unit assignments.
/// Explicit marker only: its actual request producer remains a separate join.</summary>
internal sealed class LegacyPreparedMathSeparatorCompaction
{
    private readonly uint[] units;
    private readonly int[] assignments;
    private readonly int[] oldToNew;
    internal uint[] CopyUnits() => units.ToArray();
    internal int[] CopyAssignments() => assignments.ToArray();
    internal int[] CopyOldToNew() => oldToNew.ToArray();
    private LegacyPreparedMathSeparatorCompaction(uint[] units, int[] assignments, int[] oldToNew)
    { this.units = units; this.assignments = assignments; this.oldToNew = oldToNew; }

    internal static LegacyPreparedMathSeparatorCompaction EvaluatePrepared(
        ReadOnlySpan<uint> sourceUnits, ushort separator, ReadOnlySpan<int> sourceAssignments,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (sourceUnits.Length > 1000 || sourceAssignments.Length > 6000)
            throw new ArgumentException("Bounded prepared output units and assignments required.");
        foreach (int index in sourceAssignments)
            if (index < -1 || index >= sourceUnits.Length)
                throw new ArgumentException("Assignments must be absent or refer to an original four-byte unit.");
        uint[] working = sourceUnits.ToArray();
        int[] assignments = sourceAssignments.ToArray();
        int[] origins = Enumerable.Range(0, working.Length).ToArray();
        int count = working.Length, cursor = 0;
        bool previousSeparator = true;
        void Remove(int index)
        {
            if (index + 1 < count)
            {
                Array.Copy(working, index + 1, working, index, count - index - 1);
                Array.Copy(origins, index + 1, origins, index, count - index - 1);
            }
            count--;
            for (int i = 0; i < assignments.Length; i++)
            {
                int value = assignments[i];
                if (value == index) assignments[i] = -1;
                else if (value > index) assignments[i] = value - 1;
            }
        }
        while (cursor < count)
        {
            token.ThrowIfCancellationRequested();
            bool currentSeparator = (ushort)working[cursor] == separator;
            if (previousSeparator && currentSeparator) Remove(cursor);
            else cursor++;
            previousSeparator = currentSeparator;
        }
        if (count > 0 && previousSeparator) Remove(count - 1);
        int[] map = Enumerable.Repeat(-1, sourceUnits.Length).ToArray();
        for (int i = 0; i < count; i++) map[origins[i]] = i;
        token.ThrowIfCancellationRequested();
        return new(working.AsSpan(0, count).ToArray(), assignments, map);
    }
}
