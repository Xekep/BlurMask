using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// One selected, processed ordinary cache row in native callback order. Graph
/// kind/record flags and cache recomputation are supplied upstream facts.
/// </summary>
public sealed record LegacyPreparedAcousticPermissionGroup(ReadOnlyMemory<int> FinalCosts,
    float CostScale, bool Recomputed, byte GraphKind, bool GraphRecordPresent,
    byte GraphRecordFlags, bool Succeeded);

/// <summary>Failed native preparation never forwards partial permissions to source68.</summary>
public sealed record LegacyPreparedAcousticPermissionResult(bool Succeeded,
    ReadOnlyMemory<byte> Permissions, LegacyPreparedDictionaryWorkingMap? WorkingMap);

/// <summary>
/// Bounded Text1c01b0 permission producer, composed with source68/093f80.
/// Group selection, graph lifecycle, source descriptors and acoustic cache
/// computation remain explicit; this class does not infer script aliases.
/// </summary>
public static class LegacyPreparedAcousticDictionaryPermissions
{
    /// <summary>
    /// Six native always-retained slots: alphabet header fields4/5/6, then exact
    /// lookups U+F0000, ZWNJ and ZWJ from Text1b0760. Missing slots remain FFFF.
    /// The serialized eight-field header must match the prepared alphabet.
    /// </summary>
    public static ushort[] ReadReservedSlots(LegacyInkAlphabet alphabet,
        ReadOnlySpan<byte> serializedAlphabetHeader)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        if (serializedAlphabetHeader.Length < 16 ||
            BinaryPrimitives.ReadUInt16LittleEndian(serializedAlphabetHeader[14..]) != alphabet.LabelCount)
            throw new ArgumentException("Matching serialized acoustic alphabet header required.");
        var result = new ushort[6];
        for (int i = 0; i < 3; i++)
        {
            result[i] = BinaryPrimitives.ReadUInt16LittleEndian(serializedAlphabetHeader[(8 + i * 2)..]);
            if (result[i] != ushort.MaxValue && result[i] >= alphabet.LabelCount)
                throw new InvalidDataException("Reserved acoustic header label outside alphabet.");
        }
        string[] symbols = ["\U000f0000", "\u200c", "\u200d"];
        for (int i = 0; i < symbols.Length; i++)
            result[i + 3] = alphabet.TryGetLabel(symbols[i], out ushort label) ? label : ushort.MaxValue;
        return result;
    }

    /// <summary>
    /// Text1bc900's exact disabled-power branch. The existing v110 profile
    /// reader verifies raw runtime100==0 before this function returns1f.
    /// No general exponent/scope default is assumed for other resources.
    /// </summary>
    public static float ReadDisabledPowerScale(ReadOnlySpan<byte> serializedOrdinaryV110Node,
        CancellationToken cancellationToken = default)
    {
        _ = LegacyOrdinaryAcousticCacheProfile.ReadV110Node(serializedOrdinaryV110Node, cancellationToken);
        return 1f;
    }

    /// <summary>
    /// Native resets2e8 and seeds six reserved slots with3, then ORs finite-cost
    /// flags across processed rows. The special one-bit state startsfalse and
    /// updates ONLY for recomputed kind1 rows; ordinary, kind2 and reused rows
    /// retain its previous value. Bit4 uses strict cost&lt;trunc(binary32 scale*40000).
    /// If any selected row fails, the native source callback is not performed.
    /// </summary>
    public static LegacyPreparedAcousticPermissionResult BuildAndRefresh(
        LegacyPreparedDictionaryWorkingMap source, ReadOnlySpan<ushort> reservedSlots,
        IReadOnlyList<LegacyPreparedAcousticPermissionGroup> processedGroups,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(processedGroups);
        var bytes = BuildBytes(source.AcousticSymbolCount,reservedSlots,processedGroups,cancellationToken);
        return bytes.Succeeded ? new(true,bytes.Permissions,source.Refresh(bytes.Permissions.Span,cancellationToken)) : new(false,ReadOnlyMemory<byte>.Empty,null);
    }
    internal static LegacyOrdinaryAcousticPermissionBytes BuildBytes(int acousticCount,ReadOnlySpan<ushort> reservedSlots,
        IReadOnlyList<LegacyPreparedAcousticPermissionGroup> processedGroups,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(processedGroups);
        cancellationToken.ThrowIfCancellationRequested();
        if (reservedSlots.Length != 6 || processedGroups.Count > 65535)
            throw new ArgumentException("Six prepared reserved slots and bounded callback-selected groups required.");
        var permission = new byte[acousticCount];
        foreach (ushort slot in reservedSlots)
        {
            if (slot == ushort.MaxValue) continue;
            if (slot >= permission.Length) throw new ArgumentException("Reserved symbol outside acoustic alphabet.");
            permission[slot] = 3;
        }
        bool specialOneBit = false;
        long totalCosts = 0;
        foreach (var group in processedGroups)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(group);
            if (!group.Succeeded) return new(false, ReadOnlyMemory<byte>.Empty);
            var costs = group.FinalCosts.Span;
            totalCosts += costs.Length;
            float thresholdValue = group.CostScale * 40000f;
            if ((costs.Length != permission.Length && costs.Length != permission.Length + 3) ||
                group.GraphKind > 2 || !float.IsFinite(group.CostScale) || group.CostScale < 0 ||
                !float.IsFinite(thresholdValue) || thresholdValue >= 2147483648f || totalCosts > 16_000_000)
                throw new ArgumentException("Bounded final acoustic costs, graph kind and finite permission threshold required.");
            int threshold = (int)thresholdValue;
            if (group.Recomputed && group.GraphKind == 1)
                specialOneBit = group.GraphRecordPresent && (group.GraphRecordFlags & 7) != 0;
            for (int symbol = 0; symbol < permission.Length; symbol++)
            {
                cancellationToken.ThrowIfCancellationRequested(); int cost = costs[symbol];
                byte flags = cost < LegacyAcousticFixedLog.ImpossibleCost ? (specialOneBit ? (byte)1 : (byte)3) : (byte)0;
                // Independent of the finite-cost branch, matching native ordering.
                if (cost < threshold) flags |= 4;
                permission[symbol] |= flags;
            }
        }
        return new(true,permission);
    }
}
