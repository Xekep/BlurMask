using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyPreparedTerminalSource(uint Header, int LanguagePenalty2534);
public readonly record struct LegacyPreparedMainTerminalOptions(ushort PrimaryLimit230a,
    bool ContextSentinels24b8, bool Filter4, bool AlternateSourcePresent5,
    bool RequireFlag27Bit0_6, byte InputResultFlag8);
public readonly record struct LegacyPreparedMainTerminalWinner(bool HasWinner, int Ordinal,
    int RelativeCost, int TotalCost, int RowCost14, uint RowField22, byte FallbackFlag,
    byte ResultFlag8);

/// <summary>2257C0's prepared secondary-alternative-count0/1 terminal scan.
/// Source descriptors/penalties, cumulative path cost and invocation flags are
/// explicit. Multi-alternative exclusion lists and the optional source rescue
/// pass are separate operations; this entry does not choose live SDK options.</summary>
public static class LegacyNativeMainTerminalSelection
{
    public const int Impossible = 0x4000000;

    public static LegacyPreparedMainTerminalWinner SelectSingleAlternativeWithoutRescue(ReadOnlySpan<byte> records,
        ushort sourceEnd23f0, ReadOnlySpan<LegacyPreparedTerminalSource> sources,
        LegacyPreparedMainTerminalOptions options, int cumulativePathCost,
        LegacySymbolContextFactory contexts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        cancellationToken.ThrowIfCancellationRequested();
        if (records.Length % 88 != 0 || sourceEnd23f0 > records.Length / 88 ||
            sources.Length is < 1 or > 256)
            throw new ArgumentException("Actual source-end extent, complete88-byte rows and selected-source table required.");
        int winner = -1, best = Impossible, rowCost14 = Impossible;
        byte fallback = 0;
        for (int ordinal = 0; ordinal < sourceEnd23f0; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = records.Slice(ordinal * 88, 88);
            int cost = I32(row, 0x10);
            if (cost >= Impossible || options.RequireFlag27Bit0_6 && (row[0x27] & 1) == 0) continue;
            int sourceIndex = row[4];
            if (sourceIndex >= sources.Length) throw new ArgumentException("Finite terminal row refers to an absent selected source.");
            var source = sources[sourceIndex];
            byte candidateFallback = 0;
            bool inspect = ordinal < options.PrimaryLimit230a || (row[5] & 4) != 0;
            if (inspect && options.ContextSentinels24b8)
            {
                ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if (context != 0 && contexts.ReadNode(context).Symbol is 0xfff9 or 0xfffb) inspect = false;
            }
            if (inspect)
            {
                uint category = source.Header & 0x1e00;
                if (!options.Filter4 || (row[5] & 2) != 0 || I32(row, 0x30) != -1 ||
                    category == 0xc00 && options.AlternateSourcePresent5 ||
                    unchecked(category - 0x200) < 0x801 || category is 0xe00 or 0x1000)
                    cost = Impossible;
                else
                {
                    int penalty = (source.Header & 0x1c) == 0 ? 0 : RoundPenalty(source.LanguagePenalty2534);
                    cost = unchecked(cost + penalty);
                    candidateFallback = (byte)(~(row[5] >> 1) & 1);
                }
            }
            //Strict comparison retains the first original row on a tie.
            if (cost < best) { winner = ordinal; best = cost; rowCost14 = I32(row, 0x14); fallback = candidateFallback; }
        }
        if (winner < 0)
            return new(false, -1, Impossible, Impossible, Impossible, uint.MaxValue, 0, options.InputResultFlag8);
        var selected = records.Slice(winner * 88, 88);
        byte resultFlag = (selected[0x27] & 1) != 0 ? (byte)0 : options.InputResultFlag8;
        return new(true, winner, best, unchecked(best + cumulativePathCost), rowCost14,
            BinaryPrimitives.ReadUInt16LittleEndian(selected[0x22..]), fallback, resultFlag);
    }

    private static int RoundPenalty(int raw)
    {
        //22590E CVTDQ2PS;22591B MULSS;225937 ADDSS or22594B SUBSS;
        //225961 CVTTSS2SI. The getter itself is a pure field2534 read.
        float product = (float)raw * .8f;
        float rounded = product <= 0 ? product - .5f : product + .5f;
        return (int)rounded;
    }
    private static int I32(ReadOnlySpan<byte> row, int at) => BinaryPrimitives.ReadInt32LittleEndian(row[at..]);
}
