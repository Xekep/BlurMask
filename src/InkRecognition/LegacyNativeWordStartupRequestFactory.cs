using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal nontracing ordinary1fd240 scoring-request initializer.
/// Candidate enumeration, category alternative selection and context grafting
/// remain caller producers; known initial fields are restored without fake
/// previous rows. Unwritten padding is explicit caller residue.</summary>
public static class LegacyNativeWordStartupRequestFactory
{
    /// <summary>1fd240 ordinary single-symbol initial source context:
    ///1f6fb0 with equal null endpoints returns the FFFA sentinel prefix;
    ///0918d0 then creates/resolves its current-symbol child.
    ///The already grafted FFFA(root)-to-one-AL-leaf form from290 is also
    ///accepted:1f6fb0's equal endpoints preserve that same prefix/leaf.
    ///Other paths, control leaves and enabled case rewrites remain separate.</summary>
    public static ulong GraftOrdinaryInitialContext(LegacySymbolContextFactory contexts,
        ulong queryContextIdentity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        if (queryContextIdentity == 0) throw new ArgumentException("Actual ordinary initial source context required.");
        var query = contexts.ReadNode(queryContextIdentity);
        if (query.Symbol >= 0xfff7)
            throw new NotSupportedException("Only ordinary single-symbol initial dictionary contexts are supported.");
        if (query.ParentLink != 0)
        {
            var prefix = contexts.ReadNode(query.ParentLink);
            if (prefix.Symbol == 0xfffa && prefix.ParentLink == 0)
                return query.Identity;
            throw new NotSupportedException("Only an existing FFFA root before the single initial AL leaf is supported.");
        }
        var sentinel = contexts.ResolveInitial(0xfffa, cancellationToken);
        var result = contexts.ResolveChild(sentinel.Identity, query.Symbol, cancellationToken);
        if (result.Created) contexts.LinkCreatedContext(result.Identity, sentinel.Identity);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Identity;
    }

    public static LegacyNativeWordRecombinationRequest Create(uint endingNode, uint sourceSpan,
        ushort returnedSourceArc, ushort currentSymbol, int compositeCategory,
        ulong queryTag, byte queryLength, byte queryFlags, uint preparedSourceHeader,
        byte permission2309, LegacyCursiveCategoryProfile categories,
        LegacyNativeWordTransitionCostCell costCell, int queryBaseCost,
        ReadOnlySpan<byte> stackResidue80, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories); ArgumentNullException.ThrowIfNull(costCell);
        cancellationToken.ThrowIfCancellationRequested();
        if (endingNode > int.MaxValue || stackResidue80.Length != 80 ||
            (uint)compositeCategory >= (uint)categories.CompositeCategories.Length)
            throw new ArgumentException("Actual bounded node, category and full80 residue required.");
        var bytes = stackResidue80.ToArray();
        Write32(bytes, 0, endingNode); Write32(bytes, 4, uint.MaxValue); Write32(bytes, 8, uint.MaxValue);
        Write16(bytes, 12, returnedSourceArc); Write16(bytes, 14, ushort.MaxValue); Write16(bytes, 16, ushort.MaxValue);
        bytes[18] = checked((byte)categories.CompositeCategories.Span[compositeCategory].GeometryIndex);
        bytes[19] = byte.MaxValue; Write32(bytes, 20, uint.MaxValue); Write32(bytes, 24, uint.MaxValue);
        bytes[28] = (byte)(queryFlags & 7); Write32(bytes, 32, sourceSpan);
        Write16(bytes, 36, currentSymbol); Write16(bytes, 38, ushort.MaxValue);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), costCell.Identity);
        Write32(bytes, 48, 0); Write32(bytes, 52, 0x4000000);
        bytes[56] = queryLength; bytes[57] = 0; bytes[58] = permission2309;
        Write32(bytes, 64, (preparedSourceHeader >> 9) & 15); Write32(bytes, 68, 0);
        bytes[72] = (byte)(queryTag >> 56 & 1); bytes[73] = 0;
        costCell.Value = queryBaseCost;
        cancellationToken.ThrowIfCancellationRequested();
        return new(bytes, costCell);
    }

    private static void Write16(Span<byte> bytes, int at, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..], value);
    private static void Write32(Span<byte> bytes, int at, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes[at..], value);
}
