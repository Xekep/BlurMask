using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeAlternativeWordQueryGroup(ushort FirstRowOrdinal,
    ReadOnlyMemory<ushort> EligibleRowOrdinals, ulong RestartTag28, byte SmallState21);

/// <summary>
/// Ordinary1fe740 secondary-row grouping and query producer. Consecutive eligible
/// rows share actual record28 and low-six-bit21. Invalid/limited rows are skipped
/// within a group; no global set, sort, fake restart tag or initial-row substitution.
/// Candidate reduction/allocation is a separate consumer of these groups.
/// </summary>
public static class LegacyNativeAlternativeWordQueryPreparation
{
    public static IReadOnlyList<LegacyNativeAlternativeWordQueryGroup> GroupSecondaryRows(
        IReadOnlyList<ReadOnlyMemory<byte>> storedRows, ushort primaryLimit230a,
        uint sourceSpan, bool limited2601, byte limit2602, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(storedRows);
        cancellationToken.ThrowIfCancellationRequested();
        if (storedRows.Count > ushort.MaxValue || primaryLimit230a > storedRows.Count)
            throw new ArgumentException("Actual bounded primary-limit and complete stored-row extent required.");
        foreach (var row in storedRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (row.Length != 88) throw new ArgumentException("Every actual stored word row must contain88 bytes.");
        }
        var groups = new List<LegacyNativeAlternativeWordQueryGroup>();
        var ordinals = new List<ushort>();
        ulong key = 0;
        byte state = 0;
        ushort first = 0;
        for (int ordinal = primaryLimit230a; ordinal < storedRows.Count; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = storedRows[ordinal].Span;
            if ((row[0x21] & 128) != 0 || limited2601 && unchecked((int)((uint)row[0x26] + sourceSpan)) > limit2602)
                continue;
            ulong current = BinaryPrimitives.ReadUInt64LittleEndian(row[0x28..]);
            byte currentState = (byte)(row[0x21] & 63);
            if (current == ulong.MaxValue)
                throw new NotSupportedException("Native alternative queries require the computed source restart tag28, not an initial uninitialized row.");
            if (ordinals.Count == 0 || current != key || currentState != state)
            {
                if (ordinals.Count != 0) groups.Add(new(first, ordinals.ToArray(), key, state));
                ordinals.Clear();
                key = current;
                state = currentState;
                first = (ushort)ordinal;
            }
            ordinals.Add((ushort)ordinal);
        }
        if (ordinals.Count != 0) groups.Add(new(first, ordinals.ToArray(), key, state));
        cancellationToken.ThrowIfCancellationRequested();
        return Array.AsReadOnly(groups.ToArray());
    }

    public static ulong? Apply(LegacyPreparedDictionaryQuery query, LegacyOrdinaryDictionaryQueryPolicy policy,
        LegacyNativeAlternativeWordQueryGroup group, uint ownerNode, uint sourceSpan, ushort inputFlags,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(group);
        cancellationToken.ThrowIfCancellationRequested();
        if (policy.Mode2601 != 0 && unchecked((int)sourceSpan) > policy.Limit2602) return null;
        if (group.RestartTag28 == ulong.MaxValue || group.EligibleRowOrdinals.IsEmpty || group.SmallState21 > 63 ||
            group.EligibleRowOrdinals.Span[0] != group.FirstRowOrdinal)
            throw new ArgumentException("Actual computed nonempty secondary-row restart group required.");
        query.Node = unchecked((int)(uint)group.RestartTag28);
        query.ParentContext = 0;
        query.Category18 = group.SmallState21;
        query.Parameter19 = policy.Mode2601 == 0 ? (byte)0 : policy.Limit2602;
        query.Parameter1c = ownerNode;
        query.Parameter20 = sourceSpan;
        query.Flags24 = (ushort)(0x100 | (policy.Boundary254c && (inputFlags & 0x10) != 0 ? 4 : 0));
        query.Flags26 = inputFlags;
        query.Skip = 0;
        return group.RestartTag28;
    }
}
