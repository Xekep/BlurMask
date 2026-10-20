namespace InkRecognition;

/// <summary>Actual global V140 ordinary references projected into the identical
/// pre-NN raw document. No sampled/neural identities replace original IDs.</summary>
public sealed class LegacyFreshMainGlobalGlyphDocumentPositions
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    internal LegacyFreshMainGlobalGlyphProjectionContext Projection {get;}
    public LegacyLibraryGlobalTextPublicationScope Scope=>Projection.Scope;
    public LegacyFreshMainGlobalGlyphSourceMetadataResult Source {get;}
    public LegacyOwnedRawInkDocument Document=>Scope.Document;
    internal LegacyFreshMainGlobalGlyphDocumentPositions(LegacyFreshMainGlobalGlyphProjectionContext projection,
        LegacyFreshMainGlobalGlyphSourceMetadataResult source,LegacyNativeInkPositionRange[] ranges)
    {Projection=projection;Source=source;this.ranges=ranges;}
    public LegacyNativeInkPositionRange[] CopyRanges(CancellationToken token=default)
    {Scope.Validate();token.ThrowIfCancellationRequested();return ranges.ToArray();}
}

//One original input map/root per actual choices owner, matching the native
//wrapper's shared mapping. Only per-source references and output ranges vary.
internal sealed class LegacyFreshMainGlobalGlyphProjectionContext
{
    private readonly LegacyFreshMainGlobalNoCandidateSourceCompletion completion;
    private readonly LegacyNativeTextSourceInputMap map;
    private readonly LegacyNativeInkPositionRange[] original;
    internal LegacyLibraryGlobalTextPublicationScope Scope {get;}
    internal LegacyFreshMainGlobalGlyphProjectionContext(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalNoCandidateSourceCompletion completion,CancellationToken token)
    {
        scope.Validate();completion.Validate(token);var document=scope.Document;
        if(!ReferenceEquals(scope.SourceCompletion,completion)||
            !ReferenceEquals(completion.Source.Owner.SourceDocument,document))
            throw new ArgumentException("Exact global scope, document and completion required.");
        Scope=scope;this.completion=completion;
        map=new LegacyNativeTextSourceInputMap(document.CopySelectedInputs(),[],0);
        original=[new(LegacyNativeInkPosition.Pack(0,0,3,document.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)document.ObjectCount-1,document.Strokes[^1].Points.Count-1,5,document.Strokes[^1].Points.Count))];
        scope.Validate();completion.Validate(token);
    }
    internal LegacyFreshMainGlobalGlyphDocumentPositions Project(
        LegacyFreshMainGlobalGlyphSourceMetadataResult actualSource,CancellationToken token)
    {
        Scope.Validate();completion.Validate(token);
        if(!ReferenceEquals(actualSource.Owner,completion.Source.Session!.Associations.Metadata))
            throw new ArgumentException("Exact original global V140 owner required.");
        var references=new LegacyNativeTextSourceReference[actualSource.ReferenceCount];
        for(int i=0;i<references.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=actualSource.References[i];
            if(r.First.First!=0||r.Last.First!=0)
                throw new NotSupportedException("Actual fresh ordinary group0 only; no cached/C25 source geometry.");
            references[i]=new(new(r.First.First,r.First.Second,r.First.Third),
                new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        var projected=LegacyNativeTextSourcePositionPublication.Project(map,references,true,original,token);
        completion.Validate(token);Scope.Validate();return new(this,actualSource,projected);
    }
}
