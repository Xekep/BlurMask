namespace InkRecognition;

public readonly record struct InkPoint(float X, float Y);
public sealed record InkStroke(IReadOnlyList<InkPoint> Points);
public sealed record TextRecognition(string Text, float Confidence, int Frames)
{
    /// <summary>Experimental native scores are candidate scores,
    /// not calibrated recognition probabilities. Hosts must label this mode.</summary>
    public bool ExperimentalCandidateScore { get; init; }
    /// <summary>Optional ordered detached WORDs; multiple runs do not advertise flat Text.</summary>
    public IReadOnlyList<PositionedWordRun>? WordRuns { get; init; }
    public bool WordRunsCoverFullInput { get; init; }
    /// <summary>Optional actual recognized-range geometry for font matching.
    /// Heights and horizontal coordinates must use the same units as input ink.
    /// A decoder without prepared containing-line x-height leaves this absent;
    /// stroke boxes must not be passed off as prepared line measurements.</summary>
    public IReadOnlyList<LegacyFontSizeTextSample>? FontSizeSamples { get; init; }
}

public interface IStrokeTextRecognizer
{
    string Language { get; }
    TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default);
}

/// <summary>CPU inference with immutable model weights. No UI, native DLL or Python dependency.</summary>
public sealed class StrokeTextRecognizer : IStrokeTextRecognizer
{
    private readonly RecognitionModel _model;
    private StrokeTextRecognizer(RecognitionModel model) => _model = model;
    public string Language => _model.Language;
    /// <summary>Reads a portable model. The caller retains ownership of the stream.</summary>
    public static StrokeTextRecognizer Load(Stream model)
    {
        var parsed = ModelReader.Read(model);
        if (parsed.Means.Length != 13 || parsed.Scales.Length != 13 || parsed.LinkMode != 0 || parsed.MinPoints is < 1 or > 8)
            throw new InvalidDataException("Unsupported stroke feature configuration.");
        return new(parsed);
    }

    public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        cancellationToken.ThrowIfCancellationRequested();
        var features = StrokeFeatures.Create(strokes, _model.Means, _model.Scales, _model.Spacing, _model.MinPoints, cancellationToken);
        if (features.Length == 0) return new("", 0, 0);
        var frames = features.Length / 13;
        var probabilities = _model.Network.Run(features, frames, cancellationToken);
        return CtcDecoder.Decode(probabilities, frames, _model.Labels, _model.BlankIndex);
    }
}
