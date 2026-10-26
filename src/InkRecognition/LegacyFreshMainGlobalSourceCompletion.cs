namespace InkRecognition;

/// <summary>Closed same-scope GLOBAL source continuation. Owns the actual variable
/// WORD/span batch, retaining unsupported V70/protocol branches as explicit errors.
/// This is not a NoCandidate completion or accepted transcription.</summary>
public sealed class LegacyFreshMainGlobalSourceCompletion
{
    private readonly LegacyLibraryGlobalTextPublicationScope scope;
    internal LegacyLibraryGlobalTextPublicationScope Scope=>scope;
    internal LegacyFreshMainGlobalSourceBatchCompletion Batch {get;}
    internal LegacyFreshMainGlobalNoCandidateSourceCompletion? Legacy=>null;
    private LegacyFreshMainGlobalSourceBatchCompletion.Result? output;
    private bool started;
    public LegacyFreshMainGlobalPlainRecognitionResult Source {get;}
    public LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot Snapshot=>Batch.TextSnapshot(default);
    public LegacyNativePublishedWordNode Word {get;}
    public bool IsTopV68Completed=>Batch.IsComplete;
    public bool IsFaulted=>Batch.IsFaulted;
    public bool QuoteCleanupCompleted=>Batch.QuoteCleanupCompleted;
    public bool SourcesMarkedComplete=>Batch.SourcesMarkedComplete;
    public bool RankingApplied=>Batch.RankingApplied;
    public bool SpanScoresApplied=>Batch.SpanScoresApplied;
    public bool CheckboxGuardVerified=>Batch.CheckboxGuardVerified;
    public int QuotaAfter=>output?.QuotaAfter??0;
    public int ReferenceCount=>CopyReferences().Length;
    private LegacyFreshMainGlobalSourceCompletion(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalPlainRecognitionResult source,LegacyFreshMainGlobalSourceBatchCompletion batch)
    {this.scope=scope;Source=source;Batch=batch;Word=source.Session!.Ownership.WordPublisher.Head!;}
    internal static LegacyFreshMainGlobalSourceCompletion Prepare(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalPlainRecognitionResult source,ReadOnlySpan<byte> acoustic,ReadOnlySpan<byte> allocation,CancellationToken token)
    {
        var batch=LegacyFreshMainGlobalSourceBatchCompletion.Prepare(scope,source,acoustic,allocation,token:token);
        batch.ReserveSourceFacade(816,token);
        return new(scope,source,batch);
    }
    internal void Run(LegacyGlobalSourceCallerResidue residue,Span<byte> leading,Span<byte> trailing,CancellationToken token)
    {
        if(started)throw new InvalidOperationException("Actual GLOBAL source completion cannot retry.");
        // Equality slots start with the caller's explicit scalar residues, before
        // quote normalization can write them; neither aliases quote scratch.
        byte[] left=leading.ToArray(),right=trailing.ToArray();started=true;
        output=Batch.Run(residue.Word,residue.Glyph,residue.Labels,residue.Request,residue.Backtrack,leading,trailing,left,right,token);
    }
    internal void Validate(CancellationToken token)
    {
        scope.Validate();Batch.RequireCompleted(token);
        if(!ReferenceEquals(scope.SourceCompletion,this))throw new InvalidOperationException("Exact same-scope GLOBAL completion required.");
    }
    internal LegacyGlobalCompletedTextSource Bind(LegacyLibraryGlobalTextPublicationScope actualScope,CancellationToken token)
    {
        Validate(token);
        if(!ReferenceEquals(scope,actualScope))throw new ArgumentException("Identical original GLOBAL publication scope required.");
        return Batch.PublishedWordCount==1&&Batch.UniqueSpanCount==1?
            scope.BindFacadeTextSource(this,token):scope.BindBatchTextSource(Batch,token);
    }
    public LegacyOrdinaryGraphSourceReference[] CopyReferences(){Validate(default);return Batch.CopyHeadReferences(default);}
    public LegacyNativeInkPositionRange[] CopyRanges(){Validate(default);return Batch.CopyHeadRanges(default);}
    public byte[] CopySourceStorage26(){Validate(default);return LegacyFreshMainGlobalWordAugmentation.EncodeSourceStorage26(Batch.CopyHeadReferences(default),default);}
}

// Explicit caller residues retained once from the SAME Run. No model/row import.
internal sealed class LegacyGlobalSourceCallerResidue
{
    internal readonly byte[] Word,Glyph,Request,Backtrack;
    internal readonly ushort[] Labels;
    internal LegacyGlobalSourceCallerResidue(ReadOnlySpan<byte> word,ReadOnlySpan<byte> glyph,
        ReadOnlySpan<ushort> labels,ReadOnlySpan<byte> request,ReadOnlySpan<byte> backtrack)
    {
        if(word.Length!=120||glyph.Length!=40||labels.Length!=256||request.Length!=64||backtrack.Length!=72)
            throw new ArgumentException("Complete actual GLOBAL caller residues required before snapshot.");
        Word=word.ToArray();Glyph=glyph.ToArray();Labels=labels.ToArray();Request=request.ToArray();Backtrack=backtrack.ToArray();
    }
}
