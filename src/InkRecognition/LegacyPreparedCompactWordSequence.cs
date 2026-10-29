using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>2067C0 context difference and206960 symbol normalization on the
/// explicit944-byte2075E0 frame. Coordinate descriptors and207420 context
/// surgery must already be produced; this does not reconstruct207CB0 paths.</summary>
public static class LegacyPreparedCompactWordSequence
{
    public const int FrameBytes = 0x3b0;
    public const int MaximumSymbols = 50;
    public const int MaximumCoordinates = 20;
    //PE1802E7B20, rows/columns FFFC..FFF7; SHA256 ad45c41c...e71ac9a6.
    private static ReadOnlySpan<int> MergeTable =>
    [
        -1, -2, -1, -2, 3, 3,
        -1, -1, -1, -2, 3, 3,
        -2, -2, -1, -2, 3, 3,
        -1, -1, -1, -1, -1, -1,
        3, 3, 3, -2, 3, 3,
        3, 3, 3, -2, 3, 3
    ];

    /// <summary>Append exactly the context difference selected by2067C0.
    /// Both context IDs belong to the supplied factory. Source row and runtime
    /// fields are prepared inputs, not inferred source ownership or defaults.</summary>
    public static byte[] AppendContextDifference(ReadOnlySpan<byte> preparedFrame,
        LegacySymbolContextFactory contexts, ulong currentContext, ulong previousContext,
        ReadOnlySpan<byte> sourceRow88, uint sourceNode, ushort knowledgeSymbol0e,
        bool contextSentinels24b8, ushort specialSymbol24de,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFrame(preparedFrame);
        if (sourceRow88.Length != 88) throw new ArgumentException("Complete explicit88-byte source row required.");
        byte[] frame = preparedFrame.ToArray();
        return AppendContextDifferenceCore(frame, contexts, currentContext, previousContext, sourceRow88, sourceNode, knowledgeSymbol0e, contextSentinels24b8, specialSymbol24de, cancellationToken);
    }
    //Caller owns this fresh temporary; no array view escapes before completion.
    internal static byte[] AppendContextDifferenceOwned(byte[] frame,
        LegacySymbolContextFactory contexts, ulong currentContext, ulong previousContext,
        ReadOnlySpan<byte> sourceRow88, uint sourceNode, ushort knowledgeSymbol0e,
        bool contextSentinels24b8, ushort specialSymbol24de, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFrame(frame);
        if (sourceRow88.Length != 88) throw new ArgumentException("Complete explicit88-byte source row required.");
        return AppendContextDifferenceCore(frame, contexts, currentContext, previousContext, sourceRow88, sourceNode, knowledgeSymbol0e, contextSentinels24b8, specialSymbol24de, cancellationToken);
    }
    private static byte[] AppendContextDifferenceCore(byte[] frame,
        LegacySymbolContextFactory contexts, ulong currentContext, ulong previousContext,
        ReadOnlySpan<byte> sourceRow88, uint sourceNode, ushort knowledgeSymbol0e,
        bool contextSentinels24b8, ushort specialSymbol24de, CancellationToken cancellationToken)
    {
        int count = I32(frame, 0);
        if (U16(sourceRow88, 0x18) != sourceNode && (sbyte)sourceRow88[5] < 0)
        {
            if (count == MaximumSymbols) throw new ArgumentException("Prepared symbol frame capacity exceeded.");
            int at = Item(count);
            W16(frame, at, knowledgeSymbol0e);
            frame[at + 2] = 0;
            W32(frame, at + 4, -1);
            W32(frame, 0, count + 1);
            cancellationToken.ThrowIfCancellationRequested();
            return frame;
        }
        //Native scans the current chain for the previous chain's first shared
        //identity, then tries each ancestor of previous. Equal labels alone
        //are deliberately not considered a common context.
        var current = ReadChain(currentContext);
        var previous = ReadChain(previousContext);
        int difference = current.Count;
        foreach (var prior in previous)
        {
            int index = current.FindIndex(node => node.Identity == prior.Identity);
            if (index >= 0) { difference = index; break; }
        }
        if (difference > MaximumSymbols - count)
            throw new ArgumentException("Prepared symbol frame capacity exceeded.");
        for (int i = 0; i < difference; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ushort symbol = current[i].Symbol;
            int at = Item(count + difference - i - 1);
            W16(frame, at, symbol);
            bool sentinel = symbol == 0xfff9 ||
                (unchecked((uint)(short)symbol + 9) & 0xfffa) == 0;
            byte kind = 0;
            if (!contextSentinels24b8 || !sentinel)
            {
                if (unchecked((ushort)(symbol + 0x15)) < 0x13 ||
                    symbol == specialSymbol24de || symbol == ushort.MaxValue)
                {
                    kind = 3;
                    W32(frame, 0x3ac, unchecked(I32(frame, 0x3ac) + 1));
                }
            }
            frame[at + 2] = kind;
            W32(frame, at + 4, -1);
        }
        W32(frame, 0, count + difference);
        cancellationToken.ThrowIfCancellationRequested();
        return frame;

        List<LegacySymbolContextNode> ReadChain(ulong identity)
        {
            var chain = new List<LegacySymbolContextNode>();
            while (identity != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (chain.Count == MaximumSymbols)
                    throw new ArgumentException("Prepared context depth exceeds the bounded fifty-symbol frame.");
                var node = contexts.ReadNode(identity);
                chain.Add(node);
                identity = node.ParentLink;
            }
            return chain;
        }
    }


    /// <summary>206960 over prepared symbol/coordinate/counter fields. Every
    /// unchanged byte, including inactive descriptors and padding, is retained.
    /// Native coordinate qwords are packed graph references, not XY positions.</summary>
    public static byte[] Normalize(ReadOnlySpan<byte> preparedFrame, bool contextSentinels24b8,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFrame(preparedFrame);
        byte[] frame = preparedFrame.ToArray();
        return NormalizeCore(frame,contextSentinels24b8,cancellationToken);
    }
    internal static byte[] NormalizeOwned(byte[] frame,bool contextSentinels24b8,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFrame(frame);
        return NormalizeCore(frame,contextSentinels24b8,cancellationToken);
    }
    private static byte[] NormalizeCore(byte[] frame,bool contextSentinels24b8,CancellationToken cancellationToken)
    {
        int count = I32(frame, 0);
        int ordinary = I32(frame, 0x3a0), extra = I32(frame, 0x3a8);
        if (ordinary < 0 || extra < 0 || ordinary > MaximumCoordinates - 1 - extra)
            throw new ArgumentException("Prepared coordinate prefix counters must fit nineteen entries.");
        int coordinateCount = ordinary + extra;
        int leftover = I32(frame, 0x3a4);
        for (int i = count - 1; i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (U16(frame, Item(i)) == 0xfffd) Remove(i);
        }
        for (int i = count - 2; i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int left = Item(i), right = Item(i + 1);
            uint a = unchecked((uint)(0xfffc - U16(frame, left)));
            uint b = unchecked((uint)(0xfffc - U16(frame, right)));
            if (a >= 6 || b >= 6) continue;
            int action = MergeTable[(int)(a * 6 + b)];
            if (action == -1) Remove(i + 1);
            else if (action == -2) Remove(i);
            else
            {
                ushort merged = unchecked((ushort)(-action - 4));
                W16(frame, left, merged);
                if (contextSentinels24b8 && (unchecked((ushort)(merged + 9)) <= 2 ||
                    unchecked((ushort)(merged + 5)) <= 1)) frame[left + 2] = 0;
                Remove(i + 1);
            }
        }
        for (int i = count - 1; i >= 0; i--)
        {
            int at = Item(i);
            if (unchecked((ushort)(U16(frame, at) + 9)) < 2) W16(frame, at, 0xfff9);
        }
        int position = 0, coordinate = 0;
        for (int supplied = 0; supplied < coordinateCount; supplied++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (position < count - 1 && frame[Item(position) + 2] >= 3)
            {
                W32(frame, Item(position) + 4, coordinate);
                position++;
            }
            int at = Item(position);
            if ((frame[0x268 + supplied * 16] & 1) == 0)
            {
                ordinary--;
                frame[at + 2] = 1;
            }
            else
            {
                if (count == MaximumSymbols) throw new ArgumentException("Native coordinate insertion exceeds prepared symbol capacity.");
                if (position < count)
                    frame.AsSpan(Item(position), (count - position) * 12)
                        .CopyTo(frame.AsSpan(Boundary(position + 1)));
                count++;
                W16(frame, at, 0);
                frame[at + 2] = 2;
                extra--;
            }
            W32(frame, at + 4, coordinate);
            position++;
            coordinate++;
        }
        while (position < count - 1)
        {
            int at = Item(position);
            if (leftover > 0 && frame[at + 2] < 3)
            { leftover--; frame[at + 2] = 4; }
            W32(frame, at + 4, coordinate);
            position++;
        }
        //Native writes this index even when the active symbol count is zero.
        W32(frame, Item(position) + 4, coordinate);
        for (int i = position - 1; leftover > 0 && i >= 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int at = Item(i);
            ushort symbol = U16(frame, at);
            if ((unchecked((uint)symbol + 7) & 0xfffc) == 0 && symbol != 0xfffa)
            { frame[at + 2] = 4; leftover--; }
        }
        W32(frame, 0, count);
        cancellationToken.ThrowIfCancellationRequested();
        return frame;

        void Remove(int index)
        {
            frame.AsSpan(Boundary(index + 1), (count - index - 1) * 12)
                .CopyTo(frame.AsSpan(Item(index)));
            count--;
        }
    }


    private static void ValidateFrame(ReadOnlySpan<byte> frame)
    {
        if (frame.Length != FrameBytes || I32(frame, 0) is < 0 or > MaximumSymbols)
            throw new ArgumentException("Complete944-byte prepared sequence frame and bounded symbol count required.");
    }
    private static int Item(int index)
    {
        if ((uint)index >= MaximumSymbols) throw new ArgumentException("Prepared descriptor index exceeds native capacity.");
        return 4 + index * 12;
    }
    private static int Boundary(int index)
    {
        //A one-past address is permitted only for a zero-length descriptor move.
        if ((uint)index > MaximumSymbols) throw new ArgumentException("Prepared descriptor boundary exceeds native capacity.");
        return 4 + index * 12;
    }
    private static int I32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static void W32(Span<byte> data, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(data[offset..], value);
    private static void W16(Span<byte> data, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(data[offset..], value);
}
