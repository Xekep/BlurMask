using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Bounded, read-only legacy type2b4 dictionary graph. This is a lexical backend,
/// not a handwriting recognizer. Labels and acoustic scores belong to the caller.
/// </summary>
public sealed class LegacyPackedLexicon
{
    private const int MaximumNodes = 2_000_000;
    private readonly PackedArray _labels, _offsets;
    private readonly uint _labelMask, _terminalBit;
    public int NodeCount { get; }
    public int MaximumWordLength { get; }
    public uint StoredTerminalCount { get; }
    /// <summary>Native packed label masks used by the prepared source query materializer.</summary>
    public uint PackedLabelMask => _labelMask;
    public uint PackedTerminalBit => _terminalBit;
    public int StorageBytes => _labels.Data.Length + _offsets.Data.Length;

    private LegacyPackedLexicon(int nodes, int maximumWordLength, uint terminalCount,
        PackedArray labels, PackedArray offsets)
    {
        NodeCount = nodes; MaximumWordLength = maximumWordLength;
        StoredTerminalCount = terminalCount; _labels = labels; _offsets = offsets;
        _terminalBit = 1u << (labels.Width - 1); _labelMask = _terminalBit - 1;
    }

    /// <summary>
    /// Load a complete type2b4/version100 object with flags0, inline labels and
    /// packed child counts. Other versions and resource encodings fail closed.
    /// No activation, authentication or SDK loading takes place here.
    /// </summary>
    public static LegacyPackedLexicon Load(ReadOnlySpan<byte> data)
    {
        if (data.Length is < 51 or > 32 * 1024 * 1024 || U32(data, 0) != 0x0101fe90 ||
            U32(data, 4) != data.Length - 8 || U32(data, 8) != 0x2b4 ||
            U32(data, 12) != 0x01000000 || U32(data, 16) != 0)
            throw Invalid("Unsupported packed dictionary header or encoding.");
        int count = checked((int)U32(data, 20)), maximumLength = checked((int)U32(data, 24));
        if (count is < 1 or > MaximumNodes || maximumLength is < 1 or > 256)
            throw Invalid("Invalid dictionary bounds.");
        int cursor = 28;
        var labels = ReadPacked(data, ref cursor, count, 17);
        if (labels.Width < 2 || cursor >= data.Length) throw Invalid("Invalid label width.");
        int offsetWidth = data[cursor++];
        if (offsetWidth is < 1 or > 31) throw Invalid("Invalid child-offset width.");
        var counts = ReadPacked(data, ref cursor, count, 31);
        if (data.Length - cursor != 4) throw Invalid("Unconsumed dictionary bytes.");
        uint terminalCount = U32(data, cursor);
        if (terminalCount > count) throw Invalid("Invalid stored terminal count.");
        var offsets = new PackedArray(count + 1, offsetWidth,
            new byte[checked((int)(((long)(count + 1) * offsetWidth + 31) / 32 * 4))]);
        uint next = 1, observedTerminals = 0, terminalBit = 1u << (labels.Width - 1), labelMask = terminalBit - 1;
        for (int i = 0; i < count; i++)
        {
            if (next <= i || next > count) throw Invalid("Invalid or unreachable breadth-first child range.");
            uint delta = next - (uint)i;
            if (delta >= 1u << offsetWidth) throw Invalid("Child offset exceeds declared width.");
            offsets.Set(i, delta);
            uint children = counts.Get(i);
            if (children > count - next) throw Invalid("Child range exceeds graph.");
            uint previousLabel = 0;
            for (uint child = next; child < next + children; child++)
            {
                uint label = labels.Get((int)child) & labelMask;
                if (child != next && label <= previousLabel) throw Invalid("Children must have distinct increasing labels.");
                previousLabel = label;
            }
            // Native enumeration reads a tree: nodes0..N-1, children1..N-1.
            // Root is a sentinel and cannot mark a nonempty word by itself.
            if (i == 0 && labels.Get(i) != 0) throw Invalid("Invalid root label.");
            if ((labels.Get(i) & terminalBit) != 0) observedTerminals++;
            next += children;
        }
        if (next != count) throw Invalid("Child counts do not cover the dictionary tree.");
        if (observedTerminals != terminalCount) throw Invalid("Terminal count disagrees with graph flags.");
        offsets.Set(count, 0);
        return new(count, maximumLength, terminalCount, labels, offsets);
    }

    public ushort GetLabel(int state)
    {
        ValidateState(state); return (ushort)(_labels.Get(state) & _labelMask);
    }

    public uint GetPackedLabel(int state)
    {
        ValidateState(state); return _labels.Get(state);
    }

    public (int Start, int End) GetChildRange(int state)
    {
        ValidateState(state); return (ChildStart(state), ChildStart(state + 1));
    }

    public bool IsTerminal(int state)
    {
        ValidateState(state); return state != 0 && (_labels.Get(state) & _terminalBit) != 0;
    }

    /// <summary>Find an immediate child by its caller-supplied alphabet label.</summary>
    public bool TryAdvance(int state, ushort label, out int nextState)
    {
        ValidateState(state);
        int first = ChildStart(state), end = ChildStart(state + 1);
        // Both checked LK dictionaries have strictly increasing sibling labels;
        // load validates this before binary search is enabled.
        while (first < end)
        {
            int middle = first + (end - first) / 2;
            uint value = _labels.Get(middle) & _labelMask;
            if (value == label) { nextState = middle; return true; }
            if (value < label) first = middle + 1; else end = middle;
        }
        nextState = -1; return false;
    }

    public bool Contains(ReadOnlySpan<ushort> labels)
    {
        if (labels.Length is 0 || labels.Length > MaximumWordLength) return false;
        int state = 0;
        foreach (ushort label in labels)
            if (!TryAdvance(state, label, out state)) return false;
        return IsTerminal(state);
    }

    /// <summary>Copy immediate transitions without allocating or exposing backing arrays.</summary>
    public int CopyChildren(int state, Span<int> destination)
    {
        ValidateState(state);
        int first = ChildStart(state), count = ChildStart(state + 1) - first;
        if (destination.Length < count) throw new ArgumentException("Child buffer is too small.", nameof(destination));
        for (int i = 0; i < count; i++) destination[i] = first + i;
        return count;
    }

    public int GetChildCount(int state)
    {
        ValidateState(state); return ChildStart(state + 1) - ChildStart(state);
    }

    private int ChildStart(int state) => checked(state + (int)_offsets.Get(state));
    private void ValidateState(int state)
    {
        if ((uint)state >= (uint)NodeCount) throw new ArgumentOutOfRangeException(nameof(state));
    }

    private static PackedArray ReadPacked(ReadOnlySpan<byte> data, ref int cursor, int count, int maximumWidth)
    {
        if (data.Length - cursor < 9 || U32(data, cursor) != 0x01000000 || U32(data, cursor + 4) != count)
            throw Invalid("Unsupported packed array header.");
        int width = data[cursor + 8]; cursor += 9;
        if (width < 1 || width > maximumWidth) throw Invalid("Unsupported packed array width.");
        int bytes = checked((int)(((long)count * width + 31) / 32 * 4));
        if (bytes > data.Length - cursor) throw Invalid("Truncated packed array.");
        var result = new PackedArray(count, width, data.Slice(cursor, bytes).ToArray());
        cursor += bytes; return result;
    }

    private static uint U32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4));
    private static InvalidDataException Invalid(string message) => new(message);

    private sealed record PackedArray(int Count, int Width, byte[] Data)
    {
        public uint Get(int index)
        {
            long bit = (long)index * Width;
            int offset = (int)(bit >> 3), shift = (int)(bit & 7);
            ulong value = 0;
            for (int j = 0, bytes = (shift + Width + 7) / 8; j < bytes; j++)
                value |= (ulong)Data[offset + j] << (j * 8);
            return (uint)(value >> shift) & ((1u << Width) - 1);
        }

        public void Set(int index, uint value)
        {
            long bit = (long)index * Width;
            int offset = (int)(bit >> 3), shift = (int)(bit & 7);
            ulong shifted = (ulong)value << shift;
            for (int j = 0, bytes = (shift + Width + 7) / 8; j < bytes; j++)
                Data[offset + j] |= (byte)(shifted >> (j * 8));
        }
    }
}
