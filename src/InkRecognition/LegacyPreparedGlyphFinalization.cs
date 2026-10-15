using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit successful/failed216BC0 output snapshot. This is not its
/// symbol/case/score producer. Score channels retain their raw IEEE754 bits.</summary>
public sealed class LegacyPreparedGlyphFinalizationInput
{
    private readonly ushort[] labels;
    public bool ProducerSucceeded { get; }
    public int RemainingCursor { get; }
    public byte ComponentFlag { get; }
    public byte ExternalVariant { get; }
    public uint Score0CBits { get; }
    public uint Score10Bits { get; }
    public int Cost14 { get; }
    public ushort PublicationFlags06 { get; }

    public LegacyPreparedGlyphFinalizationInput(bool producerSucceeded, int remainingCursor,
        ReadOnlySpan<ushort> actualLabels256, byte componentFlag, byte externalVariant,
        uint score0CBits, uint score10Bits, int cost14, ushort publicationFlags06)
    {
        if (producerSucceeded && actualLabels256.Length != 256)
            throw new ArgumentException("A successful216BC0 result requires its complete256-label scratch snapshot.");
        ProducerSucceeded = producerSucceeded; RemainingCursor = remainingCursor;
        labels = producerSucceeded ? actualLabels256.ToArray() : [];
        ComponentFlag = componentFlag; ExternalVariant = externalVariant;
        Score0CBits = score0CBits; Score10Bits = score10Bits; Cost14 = cost14; PublicationFlags06 = publicationFlags06;
    }

    internal ushort ResolveSymbol()
    {
        //2174EB: negative cursor with a nonzero component flag keeps scratch0.
        if (RemainingCursor < 0 && ComponentFlag != 0) return labels[0];
        if (RemainingCursor < 0)
            throw new NotSupportedException("Negative cursor without the native component flag would read before the supplied scratch buffer.");
        return RemainingCursor < 256 ? labels[RemainingCursor] : ushort.MaxValue;
    }
}

/// <summary>Complete bounded nontracing2173E0 finalizer after an explicitly
/// prepared216BC0 result. It updates one actual pool-owned40-byte glyph. The
///216BC0 producer,72-byte row construction,218560 chain lifecycle and21FFD0
/// backtracking are separate. This operation does not provide rollback.</summary>
public static class LegacyPreparedGlyphFinalization
{
    public static bool Apply(LegacyNativeGlyphRecordPool actualPool, ulong actualReference,
        ReadOnlySpan<byte> actualBacktrackRow72, LegacyPreparedGlyphFinalizationInput prepared,
        bool actualTrace2368, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualPool);
        ArgumentNullException.ThrowIfNull(prepared);
        cancellationToken.ThrowIfCancellationRequested();
        if (actualTrace2368)
            throw new NotSupportedException("Tracing requires its actual0x288-stride payload owner and640-byte copy branch; this endpoint is explicitly nontracing.");
        if (!prepared.ProducerSucceeded) return false;
        if (actualBacktrackRow72.Length != 72)
            throw new ArgumentException("Complete actual2498 backtrack row72 required.");
        bool mismatch = actualBacktrackRow72[0x3d] != 0;
        ushort symbol = prepared.ResolveSymbol();
        int componentCount = unchecked(256 - prepared.RemainingCursor);
        if (!LegacyNativeGlyphSymbolUpdate.Apply(actualPool, actualReference, symbol, componentCount,
                prepared.ComponentFlag, prepared.ExternalVariant != 0, cancellationToken))
            return false;
        //Native order: symbol/kind first, then scores, mismatch bit and flags.
        //Cancellation after the first update can therefore retain that update;
        //no transactional rollback or native cancellation behavior is claimed.
        byte[] row = actualPool.CopyRecord(actualReference, cancellationToken);
        BinaryPrimitives.WriteUInt32LittleEndian(row.AsSpan(12), prepared.Score0CBits);
        BinaryPrimitives.WriteUInt32LittleEndian(row.AsSpan(16), prepared.Score10Bits);
        BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(20), prepared.Cost14);
        ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4), (ushort)((flags & 0xfff7) | (mismatch ? 8 : 0)));
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(6), prepared.PublicationFlags06);
        actualPool.ReplaceRecord(actualReference, row, cancellationToken);
        return true;
    }
}
