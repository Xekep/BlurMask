namespace InkRecognition;

/// <summary>
/// Restored bounded probability32768 to log8192 conversion from native 180249e90.
/// This is the integer acoustic-cost channel, separate from class confidence.
/// </summary>
public static class LegacyAcousticFixedLog
{
    public const int ImpossibleCost = 0x4000000;
    private static readonly int[] Table = BuildTable();

    /// <summary>Accepts the quantized probability domain 0..32768; zero has the native negative sentinel.</summary>
    public static int LogProbability(int probability32768)
    {
        if ((uint)probability32768 > 32768)
            throw new ArgumentOutOfRangeException(nameof(probability32768));
        int quotient = probability32768 >> 4, remainder = probability32768 & 15;
        if (quotient > 0) return Table[quotient + (remainder > 8 ? 1 : 0)];
        return remainder > 0 ? Table[remainder] - 0x58b9 : -ImpossibleCost;
    }

    /// <summary>Native scoring clamps zero probability to one before obtaining a positive cost.</summary>
    public static int ProbabilityCost(int probability32768)
    {
        if ((uint)probability32768 > 32768)
            throw new ArgumentOutOfRangeException(nameof(probability32768));
        return -LogProbability(Math.Max(1, probability32768));
    }

    private static int[] BuildTable()
    {
        // All 2048 positive entries match the inspected static integer table.
        // Mathematical construction avoids embedding copied engine data.
        var table = new int[2049]; table[0] = -ImpossibleCost;
        for (int i = 1; i < table.Length; i++) table[i] = (int)(8192 * Math.Log(i / 2048d));
        return table;
    }
}
