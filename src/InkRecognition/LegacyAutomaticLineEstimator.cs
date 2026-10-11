using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Original Text class-24a v102 scalar profile; slope bounds are already tangents.</summary>
public readonly record struct LegacyAutomaticLineProfile(int NativeFirstField, float NativeSecondField,
    bool IncludeEndpoints, float MinimumSlope, float MaximumSlope, byte SparseThreshold, byte DenseThreshold,
    byte ReservedByte)
{
    public static LegacyAutomaticLineProfile ReadResourceNode(ReadOnlySpan<byte> resource, int nodeOffset)
    {
        if (resource.Length > 32 * 1024 * 1024 || nodeOffset < 0 || nodeOffset > resource.Length - 36)
            throw new InvalidDataException("Missing or oversized class-24a resource node.");
        var node = resource.Slice(nodeOffset, 36);
        if (BinaryPrimitives.ReadUInt32LittleEndian(node) != 0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[4..]) != 28 ||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..]) != 0x24a ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[12..]) != 0 ||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..]) != 0x102 || node[24] > 1)
            throw new InvalidDataException("Expected an uncompressed class-24a v102 node.");
        var profile = new LegacyAutomaticLineProfile(BinaryPrimitives.ReadInt32LittleEndian(node[16..]),
            BinaryPrimitives.ReadSingleLittleEndian(node[20..]), node[24] != 0,
            BinaryPrimitives.ReadSingleLittleEndian(node[25..]), BinaryPrimitives.ReadSingleLittleEndian(node[29..]),
            node[33], node[34], node[35]);
        profile.Validate(); return profile;
    }

    internal void Validate()
    {
        if (!float.IsFinite(NativeSecondField) || !float.IsFinite(MinimumSlope) || !float.IsFinite(MaximumSlope) ||
            MinimumSlope > MaximumSlope || SparseThreshold == 0 || DenseThreshold < SparseThreshold)
            throw new InvalidDataException("Invalid class-24a scalar profile.");
    }
}

public readonly record struct LegacyAutomaticLineEstimate(double Baseline, double UpperGuideline,
    double Slope, int VerticalRunCount, bool UsedDenseFit, bool BlendedSparseStatistics, int FitIterations)
{
    public LegacyLinePair Line => new((float)Baseline, (float)(Baseline - UpperGuideline));
}

/// <summary>
/// Text 18016e0c0 sparse/dense class-24a estimate for an explicit prepared line.
/// Excludes line grouping and subsequent 18016e850 callback adjustments.
/// </summary>
public static class LegacyAutomaticLineEstimator
{
    public static LegacyAutomaticLineEstimate Estimate(IReadOnlyList<InkPoint[]> preparedStrokes,
        LegacyAutomaticLineProfile profile, CancellationToken cancellationToken = default)
    {
        profile.Validate(); cancellationToken.ThrowIfCancellationRequested();
        if (LegacySparseLineEstimator.TryEstimate(preparedStrokes, out var sparse, profile.SparseThreshold, cancellationToken))
        {
            // Original 18016e0c0 adds/subtracts in float before storing as double.
            double upper = (float)(sparse.MeanY - sparse.HalfBand);
            return new(sparse.Line.Baseline, upper, 0, sparse.VerticalRunCount, false, false, 0);
        }
        // Sparse branch declined: there must be at least one strict vertical run,
        // so this endpoint-inclusive population cannot use the no-extrema fallback.
        var runExtrema = LegacyLineExtrema.Extract(preparedStrokes, true, cancellationToken);
        int runs = runExtrema.Lower.Length + runExtrema.Upper.Length;
        var fit = profile.IncludeEndpoints ? LegacyGaussianLineFit.FitExtrema(runExtrema.Lower, runExtrema.Upper, cancellationToken) :
            LegacyGaussianLineFit.Fit(preparedStrokes, false, cancellationToken);
        double blend = 1;
        if (profile.SparseThreshold < profile.DenseThreshold)
            blend = Math.Clamp(((float)runs - (float)profile.SparseThreshold) /
                ((float)profile.DenseThreshold - (float)profile.SparseThreshold), 0f, 1f);
        double slope = Math.Clamp(fit.Slope, (double)profile.MinimumSlope * blend, (double)profile.MaximumSlope * blend);
        double baseline = fit.Guideline1, upperGuideline = fit.Guideline2; bool blended = runs < 20;
        if (blended)
        {
            // Native 18016b050 uses float differences/squares, then double population moments.
            float origin = preparedStrokes[0][0].Y; int count = 0; double sum = 0, squares = 0;
            foreach (var stroke in preparedStrokes) foreach (var point in stroke)
            {
                cancellationToken.ThrowIfCancellationRequested(); float difference = point.Y - origin;
                sum += difference; squares += (double)(difference * difference); count++;
            }
            double variance = (squares * count - sum * sum) / ((double)count * count);
            float mean = (float)(sum / count + (double)origin), std = (float)Math.Sqrt(variance);
            if (!double.IsFinite(variance) || variance < 0 || !float.IsFinite(mean) || !float.IsFinite(std))
                throw new InvalidDataException("Invalid dense/sparse blend statistics.");
            slope *= .5; baseline = ((double)mean + baseline + (double)std) * .5;
            upperGuideline = (((double)mean + upperGuideline) - (double)std) * .5;
        }
        if (!double.IsFinite(slope) || !double.IsFinite(baseline) || !double.IsFinite(upperGuideline) ||
            !float.IsFinite((float)baseline) || !float.IsFinite((float)(baseline - upperGuideline)))
            throw new InvalidDataException("Class-24a guideline overflow.");
        return new(baseline, upperGuideline, slope, runs, true, blended, fit.Iterations);
    }
}
