using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One actual group38's sorted candidate24 rows and separately owned
/// float vectors. TOP273 masses and normalized candidate24 field0 are not
/// replacements for these vectors. Serialized pointer10 stays opaque.</summary>
public sealed class LegacyNativeGlyphScoreCache
{
    private const int Impossible = 0x4000000;
    private readonly byte[] candidates;
    private readonly float[][] vectors;
    private readonly int[] geometry;
    private readonly ushort[] bypass;
    private readonly ushort blank;
    private readonly int blankCost;
    private readonly float scale;
    public int CandidateCount => vectors.Length;

    public LegacyNativeGlyphScoreCache(ReadOnlySpan<byte> actualCandidateRows24,
        IReadOnlyList<ReadOnlyMemory<float>> actualConfidenceVectors,
        ReadOnlySpan<int> actualGeometryCosts, ushort actualBlankLabel,
        int actualBlankCost, float actualGroupPowerScale,
        ReadOnlySpan<ushort> actualEightGeometryBypassLabels,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualConfidenceVectors);
        cancellationToken.ThrowIfCancellationRequested();
        if (actualCandidateRows24.Length % 24 != 0 || actualConfidenceVectors.Count > 150 ||
            actualCandidateRows24.Length / 24 != actualConfidenceVectors.Count ||
            actualGeometryCosts.Length > 128 || actualEightGeometryBypassLabels.Length != 8 ||
            !float.IsFinite(actualGroupPowerScale) || actualGroupPowerScale <= 0)
            throw new ArgumentException("Actual bounded candidate24/vector owners and group scalar fields required.");
        foreach (var vector in actualConfidenceVectors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (vector.Length is < 1 or > 255)
                throw new ArgumentException("Actual bounded nonempty confidence vectors required.");
        }
        candidates = actualCandidateRows24.ToArray(); vectors = new float[actualConfidenceVectors.Count][];
        for (int i = 0; i < vectors.Length; i++)
        { cancellationToken.ThrowIfCancellationRequested(); vectors[i] = actualConfidenceVectors[i].ToArray(); }
        geometry = actualGeometryCosts.ToArray(); bypass = actualEightGeometryBypassLabels.ToArray();
        blank = actualBlankLabel; blankCost = actualBlankCost; scale = actualGroupPowerScale;
    }

    /// <summary>Ordinary21d.slot80=1c1300. Returns the first matching symbol's
    /// actual vector[owner88], including its raw NaN/sign bits; no match returns0.
    /// An out-of-range native pointer read is refused explicitly.</summary>
    public float ReadSymbolScore(ushort symbol, byte actualOwnerScoreChannel88,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        for (int i = 0; i < vectors.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Label(i) != symbol) continue;
            if (actualOwnerScoreChannel88 >= vectors[i].Length)
                throw new ArgumentException("Owner88 channel escapes the actual confidence vector.");
            return vectors[i][actualOwnerScoreChannel88];
        }
        return 0;
    }

    /// <summary>Ordinary21d.slot90=1c1520. First candidate.vector10[0], not
    /// candidate24 field0. An empty group returns0.</summary>
    public float ReadFirstCandidateScore(CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); return vectors.Length == 0 ? 0 : vectors[0][0]; }

    /// <summary>Ordinary21d.slot88=1c13a0 after actual graph68 group resolution.
    /// Request16 contains node0/arc4, explicit candidate ordinal6, symbol8,
    /// signed geometryA, top20 gateB and scaled-fallback flagC. No owner or
    /// group identity is inferred from Unicode or source stroke ordinals.</summary>
    public int ReadRequestCost(ReadOnlySpan<byte> actualRequest16,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (actualRequest16.Length != 16) throw new ArgumentException("Complete actual16-byte glyph cost request required.");
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(actualRequest16[8..]);
        ushort ordinal = BinaryPrimitives.ReadUInt16LittleEndian(actualRequest16[6..]);
        int cost;
        if (actualRequest16[12] != 0)
        {
            // Native MULSS then CVTTSS2SI: finite overflow produces INT_MIN.
            float product = scale * 40000f;
            cost = !float.IsFinite(product) || product >= 2147483648f || product < -2147483648f
                ? int.MinValue : (int)product;
        }
        else if (ordinal == ushort.MaxValue && symbol == blank)
        {
            cost = blankCost;
            if (cost >= Impossible) return Impossible;
            return AddGeometry(cost, symbol, actualRequest16[10]);
        }
        else
        {
            int index = ordinal;
            if (ordinal == ushort.MaxValue)
            {
                index = 0;
                while (index < vectors.Length && Label(index) != symbol)
                { cancellationToken.ThrowIfCancellationRequested(); index++; }
            }
            cost = index < vectors.Length ? Cost(index) : Impossible;
            if (actualRequest16[11] != 0 && index > 19) return Impossible;
        }
        if (cost >= Impossible) return cost;
        cancellationToken.ThrowIfCancellationRequested();
        return AddGeometry(cost, symbol, actualRequest16[10]);
    }

    public byte[] CopyCandidateRecords() => candidates.ToArray();
    private ushort Label(int index) => BinaryPrimitives.ReadUInt16LittleEndian(candidates.AsSpan(index * 24 + 8));
    private int Cost(int index) => BinaryPrimitives.ReadInt32LittleEndian(candidates.AsSpan(index * 24 + 4));
    private int AddGeometry(int cost, ushort symbol, byte index)
    {
        if (bypass.AsSpan().Contains(symbol)) return cost;
        int signedIndex = (sbyte)index;
        if (signedIndex < 0 || signedIndex >= geometry.Length)
            throw new ArgumentException("Signed request geometryA escapes the actual group geometry cache.");
        return unchecked(cost + geometry[signedIndex]);
    }

    // Opt-in numerical acceleration only. No source/WORD authority and no new
    // instance field on the public/default cache. The caller prepays before this factory.
    internal FirstOrdinalIndex CreateFirstOrdinalIndex(int labelCount,CancellationToken token)
        =>new(this,labelCount,token);

    internal sealed class FirstOrdinalIndex
    {
        private readonly LegacyNativeGlyphScoreCache cache;
        private readonly int[] first;
        internal FirstOrdinalIndex(LegacyNativeGlyphScoreCache cache,int labelCount,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(labelCount is<1 or>256)throw new ArgumentOutOfRangeException(nameof(labelCount));
            this.cache=cache;int count=cache.vectors.Length;
            first=new int[labelCount];Array.Fill(first,count);
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();ushort label=cache.Label(i);
                if(label<first.Length&&first[label]==count)first[label]=i;
            }
        }
        internal float ReadSymbolScore(ushort symbol,byte channel,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(symbol>=first.Length)return cache.ReadSymbolScore(symbol,channel,token);
            int ordinal=first[symbol];
            if(ordinal==cache.vectors.Length)return 0;
            if(channel>=cache.vectors[ordinal].Length)
                throw new ArgumentException("Owner88 channel escapes the actual confidence vector.");
            return cache.vectors[ordinal][channel];
        }
        internal int ReadRequestCost(ReadOnlySpan<byte> request,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            // Preserve the public validator and all non-search branches verbatim.
            if(request.Length!=16)return cache.ReadRequestCost(request,token);
            ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(request[8..]);
            ushort ordinal=BinaryPrimitives.ReadUInt16LittleEndian(request[6..]);
            if(symbol>=first.Length||ordinal!=ushort.MaxValue||request[12]!=0||symbol==cache.blank)
                return cache.ReadRequestCost(request,token);
            Span<byte> indexed=stackalloc byte[16];request.CopyTo(indexed);
            BinaryPrimitives.WriteUInt16LittleEndian(indexed[6..],checked((ushort)first[symbol]));
            // Missing sentinel equals CandidateCount, exactly the old completed scan.
            // All cost/IMP/top20/bypass/signed-geometry/overflow arithmetic stays in
            // the actual existing public method; no English arithmetic is recopied.
            return cache.ReadRequestCost(indexed,token);
        }
    }
}
