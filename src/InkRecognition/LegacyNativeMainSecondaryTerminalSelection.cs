using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyPreparedMainSecondaryTerminalWinner(
    LegacyPreparedMainTerminalWinner Winner, byte SecondaryOrdinal, ulong ExclusionIdentity);

/// <summary>2257C0's nontracing multi-secondary scan and identity exclusions.
/// Rescue and accepted-alternative/WORD publication scheduling are separate.</summary>
public static class LegacyNativeMainSecondaryTerminalSelection
{
    public static LegacyPreparedMainSecondaryTerminalWinner SelectWithoutRescue(
        ReadOnlySpan<byte> records, ushort sourceEnd23f0, ReadOnlySpan<byte> secondary2410,
        ushort secondaryCount2404, ReadOnlySpan<LegacyPreparedTerminalSource> sources,
        LegacyPreparedMainTerminalOptions options, int cumulative2520,
        LegacySymbolContextFactory contexts, ReadOnlySpan<ulong> excludedIdentities,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(contexts); token.ThrowIfCancellationRequested();
        if (records.Length % 88 != 0 || records.Length / 88 > 4096 || sourceEnd23f0 > records.Length / 88 ||
            secondaryCount2404 is < 2 or > 20 || sources.Length is < 1 or > 256 ||
            secondary2410.Length != records.Length / 88 * secondaryCount2404 * 40 || excludedIdentities.Length > sourceEnd23f0)
            throw new ArgumentException("Bounded complete stored88/2410 allocation, actual count and exclusion extent required.");
        const int impossible = 0x4000000;
        int selected = -1, best = impossible, row14 = impossible;
        byte secondary = 0, fallback = 0;
        ulong identity = 0;
        ushort compactIndex = ushort.MaxValue;
        for (int ordinal = 0; ordinal < sourceEnd23f0; ordinal++)
        {
            token.ThrowIfCancellationRequested(); var row = records.Slice(ordinal * 88, 88);
            if (I32(row, 16) >= impossible || options.RequireFlag27Bit0_6 && (row[0x27] & 1) == 0) continue;
            if (row[4] >= sources.Length) throw new ArgumentException("Finite terminal row refers to an absent source.");
            var source = sources[row[4]];
            int penalty = (source.Header & 0x1c) == 0 ? 0 : RoundPenalty(source.LanguagePenalty2534);
            bool inspect = ordinal < options.PrimaryLimit230a || (row[5] & 4) != 0;
            if (inspect && options.ContextSentinels24b8)
            {
                ulong context = U64(row, 8);
                if (context != 0)
                {
                    var node = contexts.ReadNode(context);
                    //Unlike the single-secondary branch, only root controls
                    //FFF9/FFFB/FFFC bypass this inspection in the matrix path.
                    if (node.ParentLink == 0 && (node.Symbol == 0xfff9 || unchecked((ushort)(node.Symbol + 5)) <= 1))
                        inspect = false;
                }
            }
            for (int k = 0; k < secondaryCount2404; k++)
            {
                token.ThrowIfCancellationRequested();
                var alternative = secondary2410.Slice((ordinal * secondaryCount2404 + k) * 40, 40);
                int cost = I32(alternative, 0);
                if (cost >= impossible) break; //A later finite entry is unreachable.
                byte candidateFallback = 0;
                if (inspect)
                {
                    uint category = source.Header & 0x1e00;
                    if (!options.Filter4 || (row[5] & 2) != 0 || I32(row, 0x30) != -1 ||
                        category == 0xc00 && options.AlternateSourcePresent5 ||
                        unchecked(category - 0x200) < 0x801 || category is 0xe00 or 0x1000)
                        cost = impossible;
                    else
                    {
                        cost = unchecked(cost + penalty);
                        candidateFallback = (byte)(~(row[5] >> 1) & 1);
                    }
                }
                if (cost >= best) continue;
                ulong candidateIdentity = U64(alternative, 8);
                bool excluded = false;
                foreach (ulong prior in excludedIdentities) if (prior == candidateIdentity) { excluded = true; break; }
                if (excluded) continue;
                selected = ordinal; secondary = (byte)k; best = cost; row14 = I32(alternative, 4);
                identity = candidateIdentity; compactIndex = BinaryPrimitives.ReadUInt16LittleEndian(alternative[0x1c..]);
                fallback = candidateFallback;
            }
        }
        token.ThrowIfCancellationRequested();
        if (selected < 0)
            return new(new(false, -1, impossible, impossible, impossible, uint.MaxValue, 0, options.InputResultFlag8), 0, 0);
        byte resultFlag = (records[selected * 88 + 0x27] & 1) != 0 ? (byte)0 : options.InputResultFlag8;
        return new(new(true, selected, best, unchecked(best + cumulative2520), row14, compactIndex, fallback, resultFlag), secondary, identity);
    }

    private static int RoundPenalty(int raw)
    {
        float value = (float)raw * .8f;
        return (int)(value <= 0 ? value - .5f : value + .5f);
    }
    private static int I32(ReadOnlySpan<byte> data, int at) => BinaryPrimitives.ReadInt32LittleEndian(data[at..]);
    private static ulong U64(ReadOnlySpan<byte> data, int at) => BinaryPrimitives.ReadUInt64LittleEndian(data[at..]);
}
