using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>Prepared0393D3..0394D0 row eligibility only. The owning caller must
/// separately establish successful last position and an empty completed vector.
/// This kernel cannot manufacture a Step, event lease or completed record.</summary>
internal static class LegacyPreparedMathLastPositionFallback
{
    internal static int[] EligibleRanks(ReadOnlySpan<byte> postEosSorted48, int width,
        IReadOnlyList<ushort[]> oldPrimary, LegacyMath2ObservableTokens actualGate,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(oldPrimary);
        ArgumentNullException.ThrowIfNull(actualGate);
        token.ThrowIfCancellationRequested();
        if (width < 1 || width > 16 || postEosSorted48.Length != width * 2 * 48 ||
            oldPrimary.Count < 1 || oldPrimary.Count > width ||
            oldPrimary.Any(row => row == null || row.Length > 1000))
            throw new ArgumentException("Complete prepared2*width pool and bounded old prefixes required.");
        var ranks = new List<int>();
        for (int rank = 0; rank < width; rank++)
        {
            token.ThrowIfCancellationRequested();
            var row = postEosSorted48.Slice(rank * 48, 48);
            float cost = BinaryPrimitives.ReadSingleLittleEndian(row);
            // Native UCOMISS first branches JP to the eligible-row path;
            // only ordered equality with FMAX skips the parent read.
            if (cost == float.MaxValue) continue;
            int parent = BinaryPrimitives.ReadInt32LittleEndian(row[40..]);
            if (parent < 0 || parent >= oldPrimary.Count)
                throw new ArgumentException("An eligible row must have an owned old-prefix parent.");
            if (oldPrimary[parent].Any(actualGate.IsObservable)) ranks.Add(rank);
        }
        token.ThrowIfCancellationRequested();
        return ranks.ToArray();
    }
}
