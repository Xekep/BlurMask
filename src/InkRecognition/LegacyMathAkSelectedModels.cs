namespace InkRecognition;

/// <summary>Immutable, authenticated weights for the five selected base-tail
/// Math models. The sixth raw catalog entry belongs to another 13A5 owner and
/// is excluded. This object supplies no grammar, candidate or AL-combination authority.</summary>
public sealed class LegacyMathAkSelectedModels
{
    private static readonly (int Start, ushort Kind, int Inputs)[] Selected =
    [(1007739,0x1f,222),(1151097,0x2f,204),(1289175,0x3f,279),
     (1459255,0x4f,279),(1629111,0x5f,223)];
    private readonly ModelData[] models;
    private readonly LegacyMathAkFeatureProfile profile;
    public LegacyMathAkFeatureProfile FeatureProfile => profile;
    private readonly IReadOnlyList<ModelDescriptor> descriptors;
    private LegacyMathAkSelectedModels(ModelData[] models, ModelDescriptor[] descriptors, LegacyMathAkFeatureProfile profile)
    { this.profile = profile; this.models = models; this.descriptors = Array.AsReadOnly(descriptors); }
    public IReadOnlyList<ModelDescriptor> Models => descriptors;
    public int RetainedWeightBytes => 761370;
    public bool HasCandidateAuthority => false;
    public readonly record struct ModelDescriptor(int OriginalStart, ushort Kind,
        int InputWidth, int FirstStride, int SecondStride);
    private sealed record ModelData(short[] First, sbyte[] Second, byte[] Scales,
        int FirstStride, int SecondStride);

    /// <summary>Copies the authenticated selected weights once. Temporary raw
    /// catalog ownership is disposed before publication; no resource bytes or
    /// mutable matrix views escape. Binding additional geometry copies no weights.</summary>
    public static LegacyMathAkSelectedModels PrepareActual(ReadOnlySpan<byte> resource,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        using var raw = LegacyMathMixed107RawResource.LoadActualResource(resource, token);
        var data = new ModelData[5]; var description = new ModelDescriptor[5];
        for (int i = 0; i < Selected.Length; i++)
        {
            token.ThrowIfCancellationRequested(); var expected = Selected[i];
            var model = raw.Models[i + 1];
            if (model.OriginalStart != expected.Start || model.Kind != expected.Kind ||
                model.InputWidth != expected.Inputs || model.HiddenRows != 200 ||
                model.OutputRows != 257 || model.Quantizer != 10 ||
                model.Containing13A5Start != 1005980 || model.Containing13A5End != 1789568)
                throw new InvalidDataException("Actual selected-five model cohort differs.");
            var snapshot = model.Capture(token);
            short[] first = snapshot.CopyFirstWeights(token);
            sbyte[] second = snapshot.CopySecondWeights(token);
            byte[] scales = snapshot.CopyScales(token);
            for (int row = 0; row < 200; row++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = expected.Inputs; c < snapshot.FirstStride; c++)
                    if (first[row * snapshot.FirstStride + c] != 0)
                        throw new NotSupportedException("Nonzero first-layer padding.");
            }
            for (int row = 0; row < 257; row++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 201; c < snapshot.SecondStride; c++)
                    if (second[row * snapshot.SecondStride + c] != 0)
                        throw new NotSupportedException("Nonzero second-layer padding.");
            }
            data[i] = new(first, second, scales, snapshot.FirstStride, snapshot.SecondStride);
            description[i] = new(expected.Start, expected.Kind, expected.Inputs,
                snapshot.FirstStride, snapshot.SecondStride);
        }
        //Independently authenticated private startup snapshot, never post-hash
        //reads from a caller array. Both owners require the exact same fingerprint.
        var profile = LegacyMathAkFeatureProfile.PrepareActual(resource, token);
        token.ThrowIfCancellationRequested(); return new(data, description, profile);
    }
    public LegacyMathAkSourceGeometry CreateGeometry(IReadOnlyList<System.Numerics.Vector2[]> strokes,
        CancellationToken token = default) => profile.CreateGeometry(strokes, token);
    public LegacyMathAkTemporalNetwork BindTemporal(LegacyMathAkSourceGeometry source,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        return new(source, this);
    }
    public LegacyMathAkSpatialNetwork BindSpatial(LegacyMathAkSourceGeometry source,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        return new(source, this);
    }
    internal (int[] Dots, short[] Hidden, int[] Raw) EvaluateSpatial(
        LegacyMathAkSourceGeometry source, LegacyMathAkSourceGeometry.SpatialFeatures features,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!source.OwnsSpatialFeatures(features))
            throw new ArgumentException("Exact privately issued source feature owner required.", nameof(features));
        ModelData model = models[1];
        return LegacyMathAkTemporalNetwork.RunModel(features.CopyValues(), model.First,
            model.Second, model.Scales, model.FirstStride, model.SecondStride, token);
    }
    internal (int[] Dots, short[] Hidden, int[] Raw) EvaluateTemporal(
        LegacyMathAkSourceGeometry source, LegacyMathAkSourceGeometry.TemporalFeatures features,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!source.OwnsTemporalFeatures(features))
            throw new ArgumentException("Exact privately issued source feature owner required.", nameof(features));
        ModelData model = models[0];
        return LegacyMathAkTemporalNetwork.RunModel(features.CopyValues(), model.First,
            model.Second, model.Scales, model.FirstStride, model.SecondStride, token);
    }
}
