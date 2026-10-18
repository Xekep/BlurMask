using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathExistingUnitCorrection.Probe")]
namespace InkRecognition.Math;

/// <summary>Prepared040CD0/0409F0 correction over explicit existing four-byte
/// units and activated observable rows. No actual C25/record owner is claimed.</summary>
internal sealed class LegacyPreparedMathExistingUnitCorrection
{
    private readonly uint[] units, associations;
    private readonly float[] averaged;
    private readonly int[] matchingRows, matchingColumns;
    internal uint[] CopyUnits() => units.ToArray();
    internal uint[] CopyAssociations() => associations.ToArray();
    internal float[] CopyAveraged() => averaged.ToArray();
    internal int[] CopyMatchingRows() => matchingRows.ToArray();
    internal int[] CopyMatchingColumns() => matchingColumns.ToArray();
    private LegacyPreparedMathExistingUnitCorrection(uint[] units, uint[] associations, float[] averaged,
        int[] matchingRows, int[] matchingColumns)
    { this.units = units; this.associations = associations; this.averaged = averaged;
      this.matchingRows = matchingRows; this.matchingColumns = matchingColumns; }

    internal static LegacyPreparedMathExistingUnitCorrection EvaluatePrepared(ReadOnlySpan<uint> existing,
        ReadOnlySpan<uint> initialUnits, ReadOnlySpan<int> observableOriginalRows,
        ReadOnlySpan<float> activated, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        int n = observableOriginalRows.Length, s = existing.Length;
        if (s > 6000 || initialUnits.Length is < 1 or > 1000 || n > initialUnits.Length ||
            (long)n * s > 1_000_000 || activated.Length != (long)n * s)
            throw new ArgumentException("Bounded prepared existing units, output units and observable matrix required.");
        int previous = -1;
        foreach (int row in observableOriginalRows)
        {
            if (row <= previous || row >= initialUnits.Length)
                throw new ArgumentException("Observable rows must retain increasing original output indices.");
            previous = row;
        }
        foreach (float value in activated)
            if (!float.IsFinite(value) || value < 0 || value > 1)
                throw new ArgumentException("Activated finite segmentation probabilities required.");
        var starts = new List<int>();
        for (int i = 0; i < s; i++)
        {
            if ((existing[i] >> 24) != 0) continue;
            if (i + 1 >= s || (existing[i + 1] >> 24) != 0)
                throw new NotSupportedException("Prepared zero-flag source units must be complete adjacent pairs.");
            starts.Add(i); i++;
        }
        int pairs = starts.Count;
        if (pairs > 3000) throw new ArgumentException("Prepared pair count exceeds matching bound.");
        float[] average = new float[checked(n * pairs)];
        for (int row = 0; row < n; row++)
        {
            token.ThrowIfCancellationRequested();
            for (int pair = 0; pair < pairs; pair++)
            {
                int start = row * s + starts[pair];
                float sum = activated[start] + activated[start + 1]; //040E3D ADDSS
                average[row * pairs + pair] = sum * .5f; //040E43 MULSS, separate rounding
            }
        }
        var matching = LegacyPreparedMathColumnMatching.EvaluatePrepared(average, n, pairs, token);
        int[] rows = matching.CopyRows(), columns = matching.CopyColumns();
        int[] rowForPair = Enumerable.Repeat(-1, pairs).ToArray();
        uint[] output = initialUnits.ToArray();
        for (int i = 0; i < rows.Length; i++)
        {
            rowForPair[columns[i]] = rows[i];
            output[observableOriginalRows[rows[i]]] = existing[starts[columns[i]]];
        }
        uint[] association = new uint[checked(pairs * 2)];
        for (int pair = 0; pair < pairs; pair++)
        {
            int row = rowForPair[pair];
            //040F18 uses040CD0 param7, the observable original-row map.
            //Caller0426F0 binds it separately from param6/output units.
            uint value = row < 0 ? uint.MaxValue : (uint)observableOriginalRows[row];
            association[pair * 2] = value; association[pair * 2 + 1] = value;
        }
        token.ThrowIfCancellationRequested();
        return new(output, association, average, rows, columns);
    }
}
