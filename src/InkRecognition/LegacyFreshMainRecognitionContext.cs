using System.Text.Json;

namespace InkRecognition;

/// <summary>Reusable owned model/resources for independent raw main invocations.
/// Only immutable model data is shared: source graphs, dictionary contexts,
/// glyph pools, publication state and raw documents remain per invocation.
/// This is not an accepted-transcription or application-recognizer adapter.</summary>
public sealed class LegacyFreshMainRecognitionContext
{
    private readonly byte[] acoustic,linguistic,spatial30,physical50;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyInkActivations activations;
    private LegacyFreshMainRecognitionContext(byte[] acoustic,byte[] linguistic,byte[] spatial30,byte[] physical50,
        LegacyRussianModelBundle model,LegacyInkActivations activations)
    {this.acoustic=acoustic;this.linguistic=linguistic;this.spatial30=spatial30;this.physical50=physical50;
     this.model=model;this.activations=activations;}

    public static LegacyFreshMainRecognitionContext Load(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> linguisticResource,ReadOnlySpan<byte> engineProfile,
        ReadOnlySpan<byte> spatial30,ReadOnlySpan<byte> physical50,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(acousticResource.Length!=572763||linguisticResource.Length!=6200992)
            throw new InvalidDataException("The complete pinned acoustic/linguistic resource pair is required.");
        if(spatial30.Length!=85812||physical50.Length!=161332)
            throw new ArgumentException("Complete owned line30/line50 resources required.");
        //Bounded snapshots precede parsing. Mutating caller buffers after Load
        //cannot change this context's model or a later input preparation.
        byte[] acoustic=acousticResource.ToArray(),linguistic=linguisticResource.ToArray();
        var model=LegacyRussianModelBundle.Load(acoustic,linguistic,engineProfile,token);
        using var parsed=JsonDocument.Parse(engineProfile.ToArray());
        var values=parsed.RootElement.GetProperty("activations");
        var activations=new LegacyInkActivations(
            values.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),
            values.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
        token.ThrowIfCancellationRequested();
        return new(acoustic,linguistic,spatial30.ToArray(),physical50.ToArray(),model,activations);
    }

    public LegacyFreshMainSingleLinePlainRecognition Prepare(IReadOnlyList<InkStroke> raw,float resolution,
        LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default) =>
        LegacyFreshMainSingleLinePlainRecognition.PrepareOwnedModel(acoustic,linguistic,model,activations,
            spatial30,physical50,raw,resolution,table,caseData,publication,token);

    /// <summary>Prepare a separate bounded shared-global main invocation.
    /// Its raw WORD output is not accepted transcription or app replacement.</summary>
    public LegacyFreshMainGlobalPlainRecognition PrepareGlobal(IReadOnlyList<InkStroke> raw,float resolution,
        LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default) =>
        LegacyFreshMainGlobalPlainRecognition.PrepareOwnedModel(acoustic,linguistic,model,activations,
            spatial30,physical50,raw,resolution,table,caseData,publication,token);

    public LegacyFreshMainTextGlyphChoices PrepareTextChoices(LegacyFreshMainCompletedFirstCandidate candidate,
        LegacyCursiveSeparationTable table,LegacyNativeTextPublicationData publication,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(candidate);token.ThrowIfCancellationRequested();
        if(!candidate.RawResult.Owner.UsesOwnedModel(model,acoustic,linguistic))
            throw new ArgumentException("Completed candidate must use this identical owned model/resource context.");
        return LegacyFreshMainTextGlyphChoices.Prepare(acoustic,candidate,table,publication,normalization,circles,token);
    }

    internal LegacyFreshMainConfiguredSingleLinePlainRecognition PrepareConfiguredDocument(
        LegacyOwnedRawInkDocument document,float resolution,LegacyCursiveSeparationTable table,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token) =>
        LegacyFreshMainConfiguredSingleLinePlainRecognition.PrepareOwnedDocument(acoustic,linguistic,model,
            activations,spatial30,physical50,document,resolution,table,caseData,publication,token);

    internal ReadOnlySpan<byte> AcousticResource => acoustic;
}
