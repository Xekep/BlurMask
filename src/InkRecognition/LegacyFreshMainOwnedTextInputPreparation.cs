namespace InkRecognition;

/// <summary>Owning fresh raw main input through real first TEXT, source720 and
///24B/24A/24D/29B/265. Does not invoke main word decoding or invent first glyphs.</summary>
public sealed class LegacyFreshMainOwnedTextInputPreparation
{
    public LegacyFreshMainOwnedInputPreparation Input {get;}
    private readonly byte[] resource;
    private readonly LegacyNativeCaseData caseData;
    private readonly LegacyNativeTextPublicationData publication;
    private LegacyFreshMainOwnedTextInputPreparation(byte[] resource,LegacyFreshMainOwnedInputPreparation input,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication)
    {this.resource=resource;Input=input;this.caseData=caseData;this.publication=publication;}
    public static LegacyFreshMainOwnedTextInputPreparation Prepare(ReadOnlySpan<byte> resource,
        ReadOnlySpan<byte> spatial30,ReadOnlySpan<byte> physical50,IReadOnlyList<InkStroke> raw,float resolution,
        LegacyInkActivations activations,LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        var input=LegacyFreshMainOwnedInputPreparation.Prepare(resource,spatial30,physical50,raw,resolution,activations,table,caseData,publication,token);
        token.ThrowIfCancellationRequested();return new(resource.ToArray(),input,caseData,publication);
    }
    public LegacyFreshMainOwnedTextPreprocessedInput Run(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var runs=new LegacyFreshFirstOwnedTextRecognitionResult[Input.FirstSubmissions.Count];
        var sources=new LegacyMainMaterializedFirstTextSource[runs.Length];
        for(int i=0;i<runs.Length;i++)
        {
            token.ThrowIfCancellationRequested();var submission=Input.FirstSubmissions[i];runs[i]=submission.Recognition.RunText(token);
            sources[i]=LegacyFirstPlainTextSourceMaterializer.MaterializeForSubmission(Input.FirstSource,submission.DescriptorOrdinal,runs[i].Text,token);
        }
        var firstLines=LegacyFreshMainFirstTextResultLines.Apply(Input.FirstSource,sources,token);
        var fallback=LegacyFreshMainGeometricLineFallback.Apply(resource,firstLines,token);
        var lines=fallback.Lines;
        var shear=LegacyFreshMainShearPreparation.Prepare(resource,lines,caseData,publication,token);
        var gap=LegacyFreshMainActiveGapPreparation.Prepare(resource,shear,token);var smoothed=gap.Smooth(resource,token);
        token.ThrowIfCancellationRequested();return new(this,runs,sources,fallback,shear,gap,smoothed);
    }
}
public sealed class LegacyFreshMainOwnedTextPreprocessedInput
{
    public LegacyFreshMainOwnedTextInputPreparation Owner {get;}
    public IReadOnlyList<LegacyFreshFirstOwnedTextRecognitionResult> FirstResults {get;}
    public IReadOnlyList<LegacyMainMaterializedFirstTextSource> Sources {get;}
    public LegacyFreshMainFirstResultLineSet Lines {get;}
    public LegacyFreshMainGeometricLineFallback Fallback {get;}
    public LegacyFreshMainShearPreparation Shear {get;}
    public LegacyFreshMainActiveGapPreparation Gap {get;}
    public LegacyFreshMainActiveGapSmoothed Smoothed {get;}
    internal LegacyFreshMainOwnedTextPreprocessedInput(LegacyFreshMainOwnedTextInputPreparation owner,
        LegacyFreshFirstOwnedTextRecognitionResult[] results,LegacyMainMaterializedFirstTextSource[] sources,
        LegacyFreshMainGeometricLineFallback fallback,LegacyFreshMainShearPreparation shear,LegacyFreshMainActiveGapPreparation gap,LegacyFreshMainActiveGapSmoothed smoothed)
    {Owner=owner;FirstResults=Array.AsReadOnly(results);Sources=Array.AsReadOnly(sources);Fallback=fallback;Lines=fallback.Lines;Shear=shear;Gap=gap;Smoothed=smoothed;}
}
