using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyOrdinaryLineScaleSnapshot(LegacyOrdinarySourceInk Source,
    LegacyLineScaleProfile Profile, ReadOnlyMemory<float> Samples, LegacyLineScales Scales)
{
    public float PrimaryScale => (float)Scales.Primary;
}

/// <summary>
/// Ordinary source219 ingress167210→unlimited/unweighted159e00 samples→resource
/// profile1d41e0/163970→15af40 scale selection. Input resolution remains supplied;
/// source-associated/custom/opaque records and later incremental scale hysteresis
/// are excluded. This does not select a geometry callback or enable recognition.
/// </summary>
public static class LegacyOrdinaryLineScaleSource
{
    public static LegacyLineScaleProfile ReadV10dResourceProfile(ReadOnlySpan<byte> acousticResource,
        int resourceOffset = 0)
    {
        var source = LegacyOrdinarySourceInk.ReadV10dResourceProfile(acousticResource, resourceOffset);
        return ReadValidatedProfile(acousticResource, source.ParametersOffset);
    }

    private static LegacyLineScaleProfile ReadValidatedProfile(ReadOnlySpan<byte> acousticResource, int at)
    {
        var profile = new LegacyLineScaleProfile(
            BinaryPrimitives.ReadSingleLittleEndian(acousticResource[at..]),
            BinaryPrimitives.ReadSingleLittleEndian(acousticResource[(at + 4)..]),
            BinaryPrimitives.ReadSingleLittleEndian(acousticResource[(at + 8)..]),
            BinaryPrimitives.ReadSingleLittleEndian(acousticResource[(at + 12)..]));
        Validate(profile); return profile;
    }

    public static LegacyOrdinaryLineScaleSnapshot Prepare(IReadOnlyList<InkStroke> rawSource,
        ReadOnlySpan<byte> acousticResource, float inputResolution, int resourceOffset = 0,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(acousticResource, resourceOffset);
        var scaleProfile = ReadValidatedProfile(acousticResource, sourceProfile.ParametersOffset);
        if (!float.IsFinite(inputResolution) || inputResolution < 0 || inputResolution > 1_000_000)
            throw new ArgumentException("Expected finite nonnegative input resolution.");
        var source = LegacyOrdinarySourceInk.Prepare(rawSource, sourceProfile, cancellationToken);
        return Extract(source, scaleProfile, inputResolution, cancellationToken);
    }

    /// <summary>
    /// Reuses the exact owned ingress snapshot; samples are extracted without a
    /// second duplicate filter. Ordinary source physical record+4 is−1, so the
    /// associated-record count is zero, as consumed by15af40.
    /// </summary>
    public static LegacyOrdinaryLineScaleSnapshot Extract(LegacyOrdinarySourceInk source,
        LegacyLineScaleProfile scaleProfile, float inputResolution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); cancellationToken.ThrowIfCancellationRequested(); Validate(scaleProfile);
        if (!float.IsFinite(inputResolution) || inputResolution < 0 || inputResolution > 1_000_000)
            throw new ArgumentException("Expected finite nonnegative input resolution.");
        var samples = new List<float>();
        foreach (var stroke in source.Strokes)
        {
            // 167210 explicitly passes halfX=0 and maximumTurns=−1. No guessed
            // known-record flag, truncation or per-stroke scalar averaging.
            samples.AddRange(LegacyLineScaleSamples.Extract(stroke.Points.ToArray(), false, cancellationToken));
        }
        float[] owned = samples.ToArray();
        var scales = LegacyLineScale.Select(owned, 0, inputResolution, scaleProfile, cancellationToken);
        if (!float.IsFinite((float)scales.Primary) || scales.Primary <= 0)
            throw new InvalidDataException("Ordinary primary scale is outside the downstream float profile.");
        return new(source, scaleProfile, owned, scales);
    }

    private static void Validate(LegacyLineScaleProfile profile)
    {
        if (!float.IsFinite(profile.Quantile) || profile.Quantile is < 0 or > 1 ||
            !float.IsFinite(profile.SparseQuantile) || profile.SparseQuantile is < 0 or > 1 ||
            !float.IsFinite(profile.BlendNumerator) || profile.BlendNumerator < 0 ||
            !float.IsFinite(profile.AssociatedRecordWeight) || profile.AssociatedRecordWeight <= 0 ||
            profile.AssociatedRecordWeight > 1_000_000)
            throw new InvalidDataException("Invalid source219 scalar scale profile.");
    }
}
