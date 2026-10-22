using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Decoded storage fields only. Their automaton meaning is not implied.</summary>
public readonly record struct LegacyFst23CWireDescriptor(
    uint RawField0, ushort RawLabel, byte RawField6, byte RawField7);

/// <summary>Bounded immutable inline23C/109 storage. This is not a query engine,
/// source admission, state traversal or a claim that RawField20 is an initial state.</summary>
public sealed class LegacyFst23CWireLayout
{
    public const int MaximumInlineBytes = 64 * 1024 * 1024;
    public const int MaximumStates = 1_048_576;
    public const int MaximumTransitionWords = 4_194_304;
    private const string RussianRootHash = "D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E";
    private readonly byte[] bytes;
    private readonly int descriptorsOffset, wordsOffset;
    public int SourceOffset { get; }
    public int SourceEnd => SourceOffset + bytes.Length;
    public ushort Version => 0x109;
    public ushort LabelCount { get; }
    public int StateCount { get; }
    public int DescriptorCount => StateCount + 1;
    public int TransitionWordCount { get; }
    public uint RawField20 { get; }
    public string Fingerprint { get; }
    // Non-null only for the exact retained, hash-pinned whole-root adapter.
    // These are origin receipts for storage, never query/configuration authority.
    public LegacyLinguisticRoot13FWireLayout? SourceRoot { get; }
    public LegacyLinguisticRoot13FWireChild? SourceChild { get; }

    private LegacyFst23CWireLayout(byte[] bytes, int origin, Layout layout,
        LegacyLinguisticRoot13FWireLayout? root, LegacyLinguisticRoot13FWireChild? child)
    {
        this.bytes = bytes; SourceOffset = origin;
        descriptorsOffset = layout.Descriptors; wordsOffset = layout.Words;
        LabelCount = layout.Labels; StateCount = layout.States;
        TransitionWordCount = layout.WordCount;
        RawField20 = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(wordsOffset - 4));
        Fingerprint = Convert.ToHexString(SHA256.HashData(bytes));
        SourceRoot = root; SourceChild = child;
    }

    /// <summary>Read one caller-supplied complete inline block, starting with its
    /// version word. absoluteOffset is the position of that word in its stream;
    /// it affects alignment but does not authenticate any native source.</summary>
    public static LegacyFst23CWireLayout ReadInline(ReadOnlySpan<byte> inline,
        int absoluteOffset, CancellationToken token = default) =>
        ReadOwned(inline, absoluteOffset, null, null, token);

    /// <summary>Read only the observed ordinary number13A from the exact pinned
    /// Russian root. No scanning for tags, source selection or grammar execution.</summary>
    public static LegacyFst23CWireLayout ReadActualRussianNumber(
        LegacyLinguisticRoot13FWireLayout root, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(root);
        if (root.Fingerprint != RussianRootHash)
            throw new ArgumentException("Exact retained Russian linguistic root required.", nameof(root));
        var child = root.Children.Single(c => c.Offset == 299000 && c.End == 301580 &&
            c.Kind == 0x13a && c.Version == 0x202);
        var envelope = root.CopyChildEnvelope(child);
        token.ThrowIfCancellationRequested();
        return ReadOwned(envelope.AsSpan(576), 299576, root, child, token);
    }

    private static LegacyFst23CWireLayout ReadOwned(ReadOnlySpan<byte> input, int origin,
        LegacyLinguisticRoot13FWireLayout? root, LegacyLinguisticRoot13FWireChild? child,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Preflight before allocating the owned buffer, then revalidate the
        // actual snapshot. A caller's writable backing array is not authority.
        _ = Inspect(input, origin, token);
        var owned = Copy(input, token);
        var layout = Inspect(owned, origin, token);
        var result = new LegacyFst23CWireLayout(owned, origin, layout, root, child);
        token.ThrowIfCancellationRequested();
        return result;
    }

    private readonly record struct Layout(ushort Labels, int States, int WordCount,
        int Descriptors, int Words);

    private static Layout Inspect(ReadOnlySpan<byte> input, int origin, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (origin < 0 || origin > int.MaxValue - input.Length)
            throw new ArgumentOutOfRangeException(nameof(origin));
        if (input.Length is < 12 or > MaximumInlineBytes)
            throw new InvalidDataException("Bounded complete inline23C block required.");
        if (BinaryPrimitives.ReadUInt16LittleEndian(input) != 0x109)
            throw new NotSupportedException("Only inline23C/109 storage is supported.");
        ushort labels = BinaryPrimitives.ReadUInt16LittleEndian(input[2..]);
        uint states = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);
        int declared = BinaryPrimitives.ReadInt32LittleEndian(input[8..]);
        if (states > MaximumStates || declared < 0 || declared > MaximumTransitionWords)
            throw new InvalidDataException("Inline state/word budget exceeded.");
        int descriptors = (int)((((long)origin + 15) & ~3L) - origin);
        long field20 = descriptors + ((long)states + 1) * 8;
        long words = field20 + 4;
        long end = words + (long)declared * 4;
        if (end != input.Length)
            throw new InvalidDataException("Truncated or unconsumed inline23C bytes.");
        uint sentinelCount = BinaryPrimitives.ReadUInt32LittleEndian(
            input.Slice(descriptors + (int)states * 8, 4));
        // Native allocates with 'declared' but reads using this last descriptor.
        // Strict agreement is our safe storage contract for malformed input.
        if (sentinelCount != (uint)declared)
            throw new InvalidDataException("Declared and sentinel word counts differ.");
        return new(labels, (int)states, declared, descriptors, (int)words);
    }

    public LegacyFst23CWireDescriptor GetDescriptor(int index)
    {
        if ((uint)index >= (uint)DescriptorCount) throw new ArgumentOutOfRangeException(nameof(index));
        var raw = bytes.AsSpan(descriptorsOffset + index * 8, 8);
        return LabelCount < 255
            ? new(BinaryPrimitives.ReadUInt32LittleEndian(raw), raw[4] == 255 ? ushort.MaxValue : raw[4], raw[5], raw[6])
            : new(BinaryPrimitives.ReadUInt32LittleEndian(raw), BinaryPrimitives.ReadUInt16LittleEndian(raw[4..]), raw[6], raw[7]);
    }

    public uint GetTransitionWord(int index)
    {
        if ((uint)index >= (uint)TransitionWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(wordsOffset + index * 4, 4));
    }

    public byte[] CopyInlineBytes(CancellationToken token = default) => Copy(bytes, token);
    public byte[] CopyAlignmentPadding(CancellationToken token = default) => Copy(bytes.AsSpan(12, descriptorsOffset - 12), token);
    public byte[] CopyDescriptorBytes(int index)
    {
        if ((uint)index >= (uint)DescriptorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return bytes.AsSpan(descriptorsOffset + index * 8, 8).ToArray();
    }
    public byte[] CopyTransitionBytes(CancellationToken token = default) => Copy(bytes.AsSpan(wordsOffset), token);

    private static byte[] Copy(ReadOnlySpan<byte> input, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var result = new byte[input.Length];
        for (int at = 0; at < result.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested();
            input.Slice(at, Math.Min(65536, input.Length - at)).CopyTo(result.AsSpan(at));
        }
        token.ThrowIfCancellationRequested();
        return result;
    }
}
