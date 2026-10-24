namespace InkRecognition;

// CLOSED live completion union. No public constructor, byte/Result ingress,
// replacement query/pool or new global CWT. Issued only by the same scope.
internal sealed class LegacyGlobalCompletedTextSource
{
    private readonly LegacyFreshMainGlobalNoCandidateSourceCompletion? legacy;
    private readonly LegacyFreshMainGlobalWordAugmentation? augmented;
    private readonly LegacyNativeInkPositionRange[]? augmentedRanges;
    internal readonly object Gate=new();
    internal LegacyFreshMainGlobalOrdinaryGlyphRefresh? RefreshOwner;
    internal LegacyFreshMainGlobalTextGlyphChoices? ChoicesOwner;
    internal LegacyLibraryGlobalTextPublicationScope Scope{get;}
    internal LegacyFreshMainGlobalPlainRecognitionResult Source{get;}
    internal LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot Snapshot{get;}
    internal LegacyNativePublishedWordNode Word{get;}
    internal LegacyFreshMainGlobalNoCandidateSourceCompletion? LegacyCompletion=>legacy;
    internal bool IsTopV68Completed=>legacy?.IsTopV68Completed??augmented!.IsComplete;
    internal bool IsFaulted=>legacy?.IsFaulted??augmented!.IsFaulted;
    private LegacyGlobalCompletedTextSource(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion legacy)
    {Scope=scope;this.legacy=legacy;Source=legacy.Source;Snapshot=legacy.Snapshot;Word=legacy.Word;}
    private LegacyGlobalCompletedTextSource(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
    {
        Scope=scope;augmented=actual;actual.RequireCompleted(token);
        Source=actual.TextSource(token);Snapshot=actual.TextSnapshot(token);Word=actual.TextWord(token);
        augmentedRanges=actual.ProjectCompletedTextRanges(scope,token);
    }
    internal static LegacyGlobalCompletedTextSource FromLegacy(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion actual,CancellationToken token)
    {actual.Validate(token);if(!actual.IsTopV68Completed||!ReferenceEquals(actual.TextScope,scope)||!ReferenceEquals(scope.SourceCompletion,actual))throw new ArgumentException("Exact completed legacy source and original scope required.");return new(scope,actual);}
    internal static LegacyGlobalCompletedTextSource FromAugmentation(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)=>new(scope,actual,token);
    internal static void ValidateAugmentationResource(LegacyLibraryGlobalTextPublicationScope scope,LegacyFreshMainGlobalWordAugmentation actual,ReadOnlySpan<byte> ownedResource,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(scope);ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();actual.RequireCompleted(token);
        scope.RequireActualAugmentationSource(actual.TextSource(token),token);
        if(ownedResource.Length!=572763||!Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ownedResource)).Equals(actual.TextSnapshot(token).SourcePolicy.Owner.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Same actual acoustic source resource required before TEXT binding.");
    }
    internal bool IsIssuedFor(object owner)=>ReferenceEquals(legacy,owner)||ReferenceEquals(augmented,owner);
    internal void Validate(CancellationToken token)
    {
        Scope.Validate();token.ThrowIfCancellationRequested();
        if(legacy is not null)legacy.Validate(token);else augmented!.RequireCompleted(token);
        if(!ReferenceEquals(Scope.ActualTextCompletion,this)||!IsTopV68Completed||IsFaulted||
            !ReferenceEquals(Source.Owner.SourceDocument,Scope.Document)||!ReferenceEquals(Scope.SharedState.GlobalTopOwner,Source.Session!.Ownership)||
            !ReferenceEquals(Word,Source.Session.Ownership.WordPublisher.Head))
            throw new InvalidOperationException("Exact same completed source/scope/current selected279 required.");
    }
    internal LegacyNativeInkPositionRange[] CopyRanges(){Validate(default);return legacy?.CopyRanges()??augmentedRanges!.ToArray();}
    internal int Actual275PayloadCount(CancellationToken token)
    {Validate(token);return Source.Session!.Ownership.WordPublisher.CaptureSpan(Word,[Word],token).Words.Count;}
    internal byte[][] Copy275Payloads(CancellationToken token)
    {Validate(token);return Source.Session!.Ownership.WordPublisher.CaptureSpan(Word,[Word],token).Words.Select(w=>w.CopyStorage()).ToArray();}
}

public sealed partial class LegacyLibraryGlobalTextPublicationScope
{
    private LegacyGlobalCompletedTextSource? textCompletion;
    internal LegacyGlobalCompletedTextSource ActualTextCompletion=>textCompletion??throw new InvalidOperationException("No completed TEXT source binding.");
    internal void RequireActualAugmentationSource(LegacyFreshMainGlobalPlainRecognitionResult actual,CancellationToken token)
    {lock(gate){Validate();token.ThrowIfCancellationRequested();if(!ReferenceEquals(actual,result)||!ReferenceEquals(actual.Owner.SourceDocument,Document)||!actual.PublicationCompleted)throw new ArgumentException("Original same-scope actual source required.");}}
    internal LegacyGlobalCompletedTextSource BindLegacyTextSource(LegacyFreshMainGlobalNoCandidateSourceCompletion actual,CancellationToken token)
    {
        lock(gate)
        {
            Validate();actual.Validate(token);
            if(textCompletion is null)textCompletion=LegacyGlobalCompletedTextSource.FromLegacy(this,actual,token);
            if(!textCompletion.IsIssuedFor(actual))throw new InvalidOperationException("Scope already bound to another completion.");
            textCompletion.Validate(token);return textCompletion;
        }
    }
    internal LegacyGlobalCompletedTextSource BindAugmentedTextSource(LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token)
    {
        lock(gate)
        {
            Validate();actual.RequireCompleted(token);var source=actual.TextSource(token);
            if(!ReferenceEquals(source,result)||!ReferenceEquals(source.Owner.SourceDocument,Document)||!source.PublicationCompleted)
                throw new ArgumentException("Original same-scope actual Run and completed augmentation required.");
            if(textCompletion is null)
            {
                if(enteredCompletion)throw new InvalidOperationException("Global source completion already entered.");
                // Adopt the already-successful same-session source completion;
                // never claim again or create a fake NoCandidate completion.
                var binding=LegacyGlobalCompletedTextSource.FromAugmentation(this,actual,token);
                enteredCompletion=true;
                try{shared.BindActualGlobalTop(source.Session!.Ownership);textCompletion=binding;}
                catch{faulted=true;throw;}
            }
            if(!textCompletion.IsIssuedFor(actual))throw new InvalidOperationException("Scope already bound to another completion.");
            textCompletion.Validate(token);return textCompletion;
        }
    }
}

public sealed partial class LegacyFreshMainGlobalWordAugmentation
{
    internal LegacyFreshMainGlobalPlainRecognitionResult TextSource(CancellationToken token){RequireCompleted(token);return source;}
    internal LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot TextSnapshot(CancellationToken token){RequireCompleted(token);return snapshot;}
    internal LegacyNativePublishedWordNode TextWord(CancellationToken token){RequireCompleted(token);return span;}
    internal LegacyNativeInkPositionRange[] ProjectCompletedTextRanges(LegacyLibraryGlobalTextPublicationScope scope,CancellationToken token)
    {
        RequireCompleted(token);scope.Validate();var doc=scope.Document;
        if(!ReferenceEquals(doc,source.Owner.SourceDocument)||sourceReferences.Length>4096)throw new ArgumentException("Same original document and bounded actual source references required.");
        var projection=new LegacyNativeTextSourceReference[sourceReferences.Length];
        for(int i=0;i<projection.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=sourceReferences[i];
            if(r.First.First!=0||r.Last.First!=0)throw new NotSupportedException("Fresh original group0 source only.");
            projection[i]=new(new(r.First.First,r.First.Second,r.First.Third),new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        var map=new LegacyNativeTextSourceInputMap(doc.CopySelectedInputs(),[],0);
        LegacyNativeInkPositionRange[] original=[new(LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),LegacyNativeInkPosition.Pack((uint)doc.ObjectCount-1,doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))];
        var ranges=LegacyNativeTextSourcePositionPublication.Project(map,projection,true,original,token);RequireCompleted(token);return ranges;
    }
}
