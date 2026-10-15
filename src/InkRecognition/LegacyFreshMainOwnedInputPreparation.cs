namespace InkRecognition;

/// <summary>Actual fresh main input through2E3 memberships, descriptor/physical
/// reorder, same-factory owned first NN results, then720/24B/24D/29B/265.
/// Multiword first273 and empty-first24A fallback remain explicit refusals.
/// No caller masks, selected text, first results, boxes or guides are accepted.</summary>
public sealed class LegacyFreshMainOwnedInputPreparation
{
    private readonly byte[] resource;
    private readonly LegacyNativeCaseData caseData;
    private readonly LegacyNativeTextPublicationData publication;
    private readonly LegacyFreshMainOwnedFirstSubmission[] first;
    public LegacyFreshMainPhysicalGrouping Grouping {get;}
    public LegacyFreshMainDescriptorSplit Split {get;}
    public LegacyFreshMainPhysicalReordering Reordered {get;}
    public LegacyFreshMainFirstResultLines FirstSource {get;}
    public IReadOnlyList<LegacyFreshMainOwnedFirstSubmission> FirstSubmissions {get;}
    private LegacyFreshMainOwnedInputPreparation(byte[] resource,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,LegacyFreshMainPhysicalGrouping grouping,
        LegacyFreshMainDescriptorSplit split,LegacyFreshMainPhysicalReordering reordered,
        LegacyFreshMainFirstResultLines firstSource,LegacyFreshMainOwnedFirstSubmission[] first)
    {this.resource=resource;this.caseData=caseData;this.publication=publication;Grouping=grouping;Split=split;Reordered=reordered;
        FirstSource=firstSource;this.first=first;FirstSubmissions=Array.AsReadOnly(first);}

    public static LegacyFreshMainOwnedInputPreparation Prepare(ReadOnlySpan<byte> actualResource,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,float actualPhysicalResolution,
        LegacyInkActivations activations,LegacyCursiveSeparationTable separationTable,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(rawPhysicalStrokes);ArgumentNullException.ThrowIfNull(activations);
        ArgumentNullException.ThrowIfNull(separationTable);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();
        //The downstream grouping reader pins extents/hashes before model use.
        if(actualResource.Length!=572763||spatial30Resource.Length!=85812||physical50Resource.Length!=161332)
            throw new ArgumentException("Actual pinned main/embedded model extents required before snapshotting.");
        byte[] resource=actualResource.ToArray(),spatial=spatial30Resource.ToArray(),physical=physical50Resource.ToArray();
        var grouping=LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,rawPhysicalStrokes,actualPhysicalResolution,token);
        var split=LegacyFreshMainDescriptorSplit.Prepare(grouping,token);
        var reordered=LegacyFreshMainPhysicalReordering.Prepare(split,spatial,physical,token);
        var submissions=LegacyFreshMainFirstResultLines.Prepare(resource,reordered,token);
        var first=new LegacyFreshMainOwnedFirstSubmission[submissions.FirstSubmissions.Count];
        for(int i=0;i<first.Length;i++)
        {
            token.ThrowIfCancellationRequested();var submitted=submissions.FirstSubmissions[i];
            var recognition=LegacyFreshFirstOwnedRecognition.PrepareForOwnedFactory(resource,submitted.Factory,activations,separationTable,token);
            if(!ReferenceEquals(recognition.Factory,submitted.Factory))throw new InvalidDataException("Native first submission factory identity was replaced.");
            first[i]=new(submitted.DescriptorOrdinal,recognition);
        }
        token.ThrowIfCancellationRequested();return new(resource,caseData,publication,grouping,split,reordered,submissions,first);
    }

    public LegacyFreshMainOwnedFirstStage RunFirstStage(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var results=new LegacyFreshFirstOwnedRecognitionResult[first.Length];
        for(int i=0;i<results.Length;i++)
        {token.ThrowIfCancellationRequested();results[i]=first[i].Recognition.Run(token);}
        token.ThrowIfCancellationRequested();return new(this,results);
    }

    public LegacyFreshMainOwnedPreprocessedInput CompletePostprocessing(LegacyFreshMainOwnedFirstStage actualFirst,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualFirst);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualFirst.Owner,this)||actualFirst.Results.Count!=first.Length)
            throw new ArgumentException("Actual first-stage results must retain this exact input/submission owner.");
        var selected=new LegacyMainSelectedFirstWord[first.Length];
        for(int i=0;i<selected.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=actualFirst.Results[i];var m=r.Materialized;
            if(!ReferenceEquals(r.Owner,first[i].Recognition)||!ReferenceEquals(m.Result.Factory,first[i].Recognition.Factory))
                throw new ArgumentException("Actual first result belongs to a foreign descriptor submission.");
            if(m.PublishedWord is null)
                throw new NotSupportedException("Actual first result publishes no word; native main24A empty-result fallback is not yet owned here.");
            var word=new LegacyFirstMaterializedPlainWord(m.Result,m.Cuts,m.Graph,m.PublishedWord,m.GlyphReferences,
                m.DetachedTrailingReference,m.DetachedLeadingReference??throw new InvalidDataException("Published first word lacks its actual leading endpoint."),
                m.OwnerFirstScores1C,m.MinimumOwnerFirstScore80,m.Selection);
            selected[i]=new(first[i].DescriptorOrdinal,word,r.Owner.Prepared.SourceRanges);
        }
        var lines=FirstSource.ApplyFirstResults(selected,token);
        var shear=LegacyFreshMainShearPreparation.Prepare(resource,lines,caseData,publication,token);
        var gap=LegacyFreshMainActiveGapPreparation.Prepare(resource,shear,token);
        var smoothed=gap.Smooth(resource,token);
        token.ThrowIfCancellationRequested();return new(actualFirst,lines,shear,gap,smoothed);
    }

    public LegacyFreshMainOwnedPreprocessedInput Run(CancellationToken token=default)
        =>CompletePostprocessing(RunFirstStage(token),token);
}

public sealed record LegacyFreshMainOwnedFirstSubmission(int DescriptorOrdinal,LegacyFreshFirstOwnedRecognition Recognition);

public sealed class LegacyFreshMainOwnedFirstStage
{
    public LegacyFreshMainOwnedInputPreparation Owner {get;}
    public IReadOnlyList<LegacyFreshFirstOwnedRecognitionResult> Results {get;}
    internal LegacyFreshMainOwnedFirstStage(LegacyFreshMainOwnedInputPreparation owner,LegacyFreshFirstOwnedRecognitionResult[] results)
    {Owner=owner;Results=Array.AsReadOnly(results);}
}

public sealed record LegacyFreshMainOwnedPreprocessedInput(LegacyFreshMainOwnedFirstStage FirstStage,
    LegacyFreshMainFirstResultLineSet Lines,LegacyFreshMainShearPreparation Shear,
    LegacyFreshMainActiveGapPreparation Gap,LegacyFreshMainActiveGapSmoothed Smoothed);
