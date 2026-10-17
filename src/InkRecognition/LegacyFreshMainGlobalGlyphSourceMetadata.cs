namespace InkRecognition;

/// <summary>Actual ordinary global graph.V140/1AA470 target/base/source
/// projection. Higher source ranges never substitute a neural group or DFS.</summary>
public sealed class LegacyFreshMainGlobalGlyphSourceMetadata
{
    private readonly LegacyFreshMainGlobalSourceRanges ranges;
    public LegacyFreshMainGlobalResidualSource SourceOverlay{get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>SourceOverlay.Graph;
    private LegacyFreshMainGlobalGlyphSourceMetadata(LegacyFreshMainGlobalResidualSource overlay,
        LegacyFreshMainGlobalSourceRanges ranges){SourceOverlay=overlay;this.ranges=ranges;}
    public static LegacyFreshMainGlobalGlyphSourceMetadata Prepare(LegacyFreshMainGlobalResidualSource overlay,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(overlay);token.ThrowIfCancellationRequested();
        var source=overlay.Geometry.Policy.Owner.Acoustic.Geometry.Context.Source;
        if(!ReferenceEquals(source.Graph,overlay.Graph))throw new ArgumentException("Same actual global source chronology/graph required.");
        return new(overlay,source);
    }
    public LegacyFreshMainGlobalGlyphSourceMetadataResult Read(ulong targetKey,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(targetKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(targetKey))
            throw new ArgumentException("Canonical owner key without a glyph alternative ordinal required.");
        var target=LegacyNativeResidualSource.Decode(targetKey);
        if((uint)target.Node>=(uint)Graph.State.NodeCount||unchecked((short)target.Arc)<0)
            throw new NotSupportedException("Ordinary nonnegative global source arc required.");
        ulong baseKey=SourceOverlay.ResolveBaseReference(targetKey,token);var source=LegacyNativeResidualSource.Decode(baseKey);
        if(baseKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(baseKey))
            throw new NotSupportedException("Higher originalLast must be a canonical source key without a glyph-alternative ordinal.");
        if((uint)source.Node>=(uint)Graph.State.NodeCount||source.Arc>=Graph.State.GetNode(source.Node).Count)
            throw new InvalidDataException("Higher originalLast must retain a canonical owned base ordinary source.");
        bool higher=target.Arc>=Graph.State.GetNode(target.Node).Count;
        var references=higher?ranges.ResolveOwnedHigherRanges(SourceOverlay.CopySourceRanges(targetKey,token),token):
            ranges.ResolveCandidateWithResiduals(target.Node,target.Arc,token);
        if(references.Length==0)throw new NotSupportedException("Actual nonempty source membership required.");
        token.ThrowIfCancellationRequested();return new(this,targetKey,baseKey,higher,source.Node,source.Arc,
            checked((int)Graph.State.GetNode(source.Node)[source.Arc].GroupId),references);
    }
}

public sealed class LegacyFreshMainGlobalGlyphSourceMetadataResult
{
    private readonly LegacyOrdinaryGraphSourceReference[] references;
    public LegacyFreshMainGlobalGlyphSourceMetadata Owner{get;}
    public ulong TargetSourceKey{get;}
    public ulong BaseSourceKey{get;}
    public bool IsHigherSource{get;}
    public int BaseNode{get;}
    public ushort BaseArc{get;}
    public int AcousticGroupId{get;}
    public IReadOnlyList<LegacyOrdinaryGraphSourceReference> References{get;}
    public int ReferenceCount=>references.Length;
    internal LegacyFreshMainGlobalGlyphSourceMetadataResult(LegacyFreshMainGlobalGlyphSourceMetadata owner,ulong target,
        ulong original,bool higher,int node,ushort arc,int group,LegacyOrdinaryGraphSourceReference[] references)
    {Owner=owner;TargetSourceKey=target;BaseSourceKey=original;IsHigherSource=higher;BaseNode=node;BaseArc=arc;
        AcousticGroupId=group;this.references=references;References=Array.AsReadOnly(references);}
    public byte[] CopyReferenceStorage24()
    {byte[] bytes=new byte[checked(references.Length*24)];for(int i=0;i<references.Length;i++)references[i].CopyReferenceStorage24().CopyTo(bytes,i*24);return bytes;}
    public byte[] CopyResidualStorage2()
    {byte[] bytes=new byte[checked(references.Length*2)];for(int i=0;i<references.Length;i++){bytes[i*2]=references[i].FirstResidual;bytes[i*2+1]=references[i].LastResidual;}return bytes;}
    public byte[] CopySourceAllocation26()
    {byte[] bytes=new byte[checked(references.Length*26)];CopyReferenceStorage24().CopyTo(bytes,0);CopyResidualStorage2().CopyTo(bytes,references.Length*24);return bytes;}
}
