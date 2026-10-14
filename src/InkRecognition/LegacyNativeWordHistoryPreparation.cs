using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativePreparedWordHistory(bool UsesPreviousHistory, bool ClearsFirstCategoryTag,
    bool IncludesBoundaryCost, ReadOnlyMemory<uint> History, ReadOnlyMemory<byte> SourceToken16);

/// <summary>204290's same-source/fresh-history selector and its request01,
///04..14,24/25 producers. Both later-word requests have mode00=0;
///the non-reuse branch has literal zero history, not a first-word default.
///E8 reads input descriptor line ID at hex10;
///it is not a physical stroke ID. Runtime source/marker flags stay explicit.</summary>
public static class LegacyNativeWordHistoryPreparation
{
    public static LegacyNativePreparedWordHistory Prepare(ReadOnlySpan<byte> sourceToken16,
        byte currentSourceId, ReadOnlySpan<byte> previousRow88, int previousEndingNode,
        ReadOnlySpan<byte> currentAnchor88, int currentLineDifference,
        int subwordSourceIndex22ec, bool extendedMarkers22f8, bool knowledgeF5,
        ushort dashSymbol24c0, int availableSourceNodes, LegacySymbolContextFactory contexts,
        LegacyNativeWordParentRowLookup owningRowLookup, Func<byte, uint> sourceHeader,
        Func<int, int, int> descriptorLineE8, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(sourceHeader);
        ArgumentNullException.ThrowIfNull(descriptorLineE8);
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceToken16.Length != 16 || previousRow88.Length != 88 || currentAnchor88.Length != 88 ||
            availableSourceNodes is < 1 or > 1_000_000 || (uint)previousEndingNode >= (uint)availableSourceNodes ||
            subwordSourceIndex22ec is < -1 or > 255 || sourceToken16[13] > 1)
            throw new ArgumentException("Actual token16, stored88 rows, source extents and runtime source index required.");
        byte[] token = sourceToken16.ToArray(), previous = previousRow88.ToArray(), anchor = currentAnchor88.ToArray();
        bool clear = false;
        if (knowledgeF5)
        {
            uint header = sourceHeader(previous[4]);
            cancellationToken.ThrowIfCancellationRequested();
            if ((header & 0x1e00) == 0x1400 || U16(previous, 36) == dashSymbol24c0)
            {
                int ending = I32(previous, 56);
                ushort ordinal = U16(previous, 60);
                if ((uint)ending >= (uint)availableSourceNodes || ordinal == ushort.MaxValue)
                    throw new NotSupportedException("F5 needs the actual previous word's materialized anchor.");
                var owned = owningRowLookup(ending, ordinal);
                cancellationToken.ThrowIfCancellationRequested();
                if (owned is null || owned.Record.Length != 88) throw new ArgumentException("Previous anchor must own a complete stored88 row.");
                int start = unchecked(ending - (sbyte)owned.Record.Span[32]);
                if (start < 0) throw new NotSupportedException("Ordinary previous word starts at a nonnegative source node.");
                if (start == 0) clear = true;
                else if (start < availableSourceNodes)
                {
                    int before = descriptorLineE8(start - 1, 0);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (before != -1)
                    {
                        int after = descriptorLineE8(start, 0);
                        cancellationToken.ThrowIfCancellationRequested();
                        clear = before != after;
                    }
                }
            }
        }
        bool useHistory = previous[4] == currentSourceId && currentLineDifference <= 1;
        if (useHistory && currentSourceId == subwordSourceIndex22ec)
        {
            ulong context = U64(anchor, 8);
            if (context == 0) throw new NotSupportedException("Selected subword source needs its actual nonnull suffix context.");
            var oldest = contexts.ReadNode(context);
            int visits = 0;
            while (oldest.ParentLink != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++visits > contexts.NodeCount) throw new InvalidDataException("Owned suffix path is cyclic.");
                oldest = contexts.ReadNode(oldest.ParentLink);
            }
            ushort symbol = oldest.Symbol;
            bool bypass = symbol is 0xfffb or 0xfff9 or 0xfff5 ||
                extendedMarkers22f8 && unchecked((ushort)(symbol + 0x15)) <= 8;
            if (bypass) useHistory = false;
        }
        uint[] history = [];
        if (useHistory)
        {
            //Observed order2 output history has zero/one slot; higher orders
            //or malformed same-source cache rows are not silently truncated.
            int count = previous[64];
            if (count > 1) throw new NotSupportedException("Checked order2 source history permits at most one class slot.");
            history = count == 0 ? [] : new[] { BinaryPrimitives.ReadUInt32LittleEndian(previous.AsSpan(68)) };
            //Native request24 overwrites source70's byte0C with prev05.bit2.
            token[12] = (byte)(previous[5] >> 2 & 1);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(useHistory, clear, token[12] != 0, history, token);
    }

    private static int I32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static ulong U64(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[offset..]);
}
