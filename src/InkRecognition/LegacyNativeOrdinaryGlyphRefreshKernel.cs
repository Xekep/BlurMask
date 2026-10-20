using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalOrdinaryRefresh.Probe")]
namespace InkRecognition;

internal readonly record struct LegacyOrdinaryGlyphRefreshCost(int Cost);
internal sealed record LegacyOrdinaryGlyphRefreshOperations(
    Func<ushort,CancellationToken,LegacyRussianMainReplacementResolution> ResolveSymbol,
    Func<ushort,CancellationToken,uint> ReadSymbolScoreBits,
    Func<ushort,bool,CancellationToken,LegacyOrdinaryGlyphRefreshCost> ReadPreparedSymbol);

//Shared217670 compact scan/update body. Actual old/global owners bind every
//callback; this internal kernel neither constructs source identities nor NN.
internal static class LegacyNativeOrdinaryGlyphRefreshKernel
{
    internal static int Apply(LegacyNativeGlyphRecordPool pool,ulong canonicalOwnerKey,
        ReadOnlySpan<byte> compact,int first,int end,ulong baseSourceKey,
        LegacyOrdinaryGlyphRefreshOperations operations,ReadOnlySpan<byte> allocatedRecordStorage40,
        CancellationToken token)
    {
        if(first<0||end<first||end>compact.Length/40)throw new InvalidDataException("Actual compact-node interval required.");
        int visited=0;
            for (int index = first; index < end; index++)
            {
                token.ThrowIfCancellationRequested(); ReadOnlySpan<byte> row = compact.Slice(index * 40, 40);
                byte traits = row[0x20];
                if ((traits & 0x20) == 0 || (traits & 7) != 0 ||
                    BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) != baseSourceKey) continue;
                var inserted = pool.AppendDescriptor(canonicalOwnerKey, index - first, false, allocatedRecordStorage40, token);
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[0x1c..]);
                var resolved = operations.ResolveSymbol(symbol, token);
                ushort emitted = resolved.EmittedSymbol, normalized = resolved.NormalizedOriginal;
                byte variant = resolved.ExternalVariant;
                if (!LegacyNativeGlyphSymbolUpdate.Apply(pool, inserted.Reference, emitted, 1, 0, variant != 0, token))
                    throw new NotSupportedException("Actual ordinary scalar symbol update did not complete.");
                byte[] record = pool.CopyRecord(inserted.Reference, token);
                uint symbolScore = operations.ReadSymbolScoreBits(normalized, token);
                BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(0x10), symbolScore);
                pool.ReplaceRecord(inserted.Reference, record, token);
                var scores = operations.ReadPreparedSymbol(normalized, (traits & 0x40) != 0, token);
                BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(0x14), scores.Cost);
                BinaryPrimitives.WriteSingleLittleEndian(record.AsSpan(0x0c), row[0x21] / 255f);
                ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4));
                BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4), (ushort)((flags & 0xfff7) | ((traits & 0x40) != 0 ? 8 : 0)));
                pool.ReplaceRecord(inserted.Reference, record, token); visited++;
            }
        token.ThrowIfCancellationRequested();return visited;
    }
}
