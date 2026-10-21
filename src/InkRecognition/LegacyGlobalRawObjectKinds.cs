using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Same-scope original-slot provenance for this library's sealed
/// ordinary C49 ingress and its kind-preserving selected clone. Portable
/// references are not native original/clone pointer equality.</summary>
internal sealed class LegacyGlobalRawObjectKinds
{
    private readonly LegacyLibraryOriginalSelectedFieldContext selected;
    private readonly LegacyFreshRawInkCurrentView view;
    private readonly LegacyOwnedRawInkCurve[] curves;
    private readonly LegacyOwnedRawInkCurrentViewEntry[] entries;
    internal LegacyFreshMainGlobalDocumentCoverage Coverage{get;}
    internal int Count=>curves.Length;
    private LegacyGlobalRawObjectKinds(LegacyFreshMainGlobalDocumentCoverage coverage,
        LegacyLibraryOriginalSelectedFieldContext selected,LegacyFreshRawInkCurrentView view,
        LegacyOwnedRawInkCurve[] curves,LegacyOwnedRawInkCurrentViewEntry[] entries)
    {Coverage=coverage;this.selected=selected;this.view=view;this.curves=curves;this.entries=entries;}
    internal static LegacyGlobalRawObjectKinds Prepare(LegacyFreshMainGlobalDocumentCoverage coverage,CancellationToken token=default)
        =>PrepareCore(coverage,null,token);
    internal static LegacyGlobalRawObjectKinds PrepareDiagnostic(LegacyFreshMainGlobalDocumentCoverage coverage,
        Action<int> checkpoint,CancellationToken token=default)=>PrepareCore(coverage,checkpoint,token);
    private static LegacyGlobalRawObjectKinds PrepareCore(LegacyFreshMainGlobalDocumentCoverage coverage,
        Action<int>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(coverage);coverage.Validate(token);
        var original=coverage.Publication.Scope.Context.OriginalSelectedInput??
            throw new NotSupportedException("Actual original-selected C49 ingress required for raw-slot kind receipt.");
        var current=original.CurrentView;ValidateBinding(coverage,original,current,token);
        if(current.Curves.Count is<1 or>256||current.Views.Count!=current.Curves.Count)
            throw new NotSupportedException("Bounded complete raw/current-view count required before snapshot.");
        int points=0;foreach(var curve in current.Curves){token.ThrowIfCancellationRequested();points=checked(points+curve.PointCount);}
        _=Preflight(current.Curves.Count,points,current.Views.Count);
        var curves=current.Curves.ToArray();var entries=current.Views.ToArray();
        var result=new LegacyGlobalRawObjectKinds(coverage,original,current,curves,entries);
        for(int i=0;i<curves.Length;i++)
        {token.ThrowIfCancellationRequested();result.ValidateSlot(i);checkpoint?.Invoke(i+1);token.ThrowIfCancellationRequested();}
        result.Validate(token);return result;
    }
    internal static long Preflight(int objects,int points,int views)
    {
        if(objects is<1 or>256||views!=objects||points<objects||points>4096)
            throw new NotSupportedException("Whole bounded full raw/current-view inventory required.");
        return (long)objects*64; // reference/scalar allowance; no XY buffers copied.
    }
    internal static void ValidateBinding(LegacyFreshMainGlobalDocumentCoverage coverage,
        LegacyLibraryOriginalSelectedFieldContext actualSelected,LegacyFreshRawInkCurrentView actualView,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(coverage);ArgumentNullException.ThrowIfNull(actualSelected);ArgumentNullException.ThrowIfNull(actualView);
        coverage.Validate(token);var scope=coverage.Publication.Scope;
        if(!ReferenceEquals(scope.Context.OriginalSelectedInput,actualSelected)||!actualSelected.State.Succeeded||actualSelected.State.IsFaulted||
            !ReferenceEquals(actualSelected.CurrentView,actualView)||!ReferenceEquals(actualSelected.Original.CurrentView,actualView)||
            !ReferenceEquals(scope.Document,actualSelected.Document)||!ReferenceEquals(scope.Document,actualView.Document)||
            scope.Document.ObjectCount!=actualView.Curves.Count)
            throw new ArgumentException("Exact retained original-selected CurrentView/Document required; same bounds or Document alone are insufficient.");
    }
    private void ValidateSlot(int i)
    {
        var document=Coverage.Publication.Scope.Document;var curve=curves[i];var entry=entries[i];var descriptor=entry.Descriptor;
        if(!ReferenceEquals(view.Curves[i],curve)||!ReferenceEquals(view.Views[i],entry)||
            !ReferenceEquals(curve.Document,document)||curve.OriginalSourceIndex!=i||curve.NativeConcreteType!=0xc49||
            !ReferenceEquals(entry.Curve,curve)||entry.CurrentViewIndex!=i||descriptor.SourceIndex!=(uint)i||
            descriptor.PointCount!=curve.PointCount||curve.PointCount!=document.Strokes[i].Points.Count||
            descriptor.IsKnownText||descriptor.FirstPoint!=0||descriptor.LastPoint!=curve.PointCount-1||
            descriptor.FirstFraction!=0||descriptor.LastFraction!=0||!descriptor.FirstClosed||!descriptor.LastClosed)
            throw new NotSupportedException("Exact complete ordinary typed C49 original-slot lineage required.");
    }
    internal void Validate(CancellationToken token=default)
    {
        ValidateBinding(Coverage,selected,view,token);
        if(view.Curves.Count!=curves.Length||view.Views.Count!=entries.Length||Coverage.Publication.Scope.Document.ObjectCount!=curves.Length)
            throw new InvalidOperationException("Original raw-slot inventory changed.");
        //All slots were checked before publication. View collections, entries,
        //curve identities and coordinates are sealed immutable owned storage.
    }
    internal LegacyGlobalRawObjectKind Lookup(uint sourceIndex,CancellationToken token=default)
    {
        Validate(token);if(sourceIndex>=(uint)curves.Length)throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        ValidateSlot((int)sourceIndex);var curve=curves[(int)sourceIndex];
        //Default C1E.V40=094940 explicitly maps actual concrete C49 to C1E.
        //No caller-supplied class, shape geometry or wrapper reinterpretation.
        return new(curve,(int)sourceIndex,curve.NativeConcreteType,0xc1e);
    }
    internal static uint ProjectMetadata(uint flags,int reportedKind)
        =>(flags&0xfeffffffu)|(reportedKind==0xc25?0x01000000u:0);
    internal static uint ComputeMetadata(LegacyFreshMainGlobalDocumentCoverage coverage,
        LegacyFreshMainGlobalTextSourcePayload source,LegacyGlobalAssignedPayloadScalars scalars,
        ReadOnlySpan<byte> key,ref LegacyGlobalRawObjectKinds? receipt,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(coverage);ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
        var parent=coverage.Publication;
        if(!source.IsComplete||!ReferenceEquals(source.Owner,parent.Wrapper)||!ReferenceEquals(source.Node.Owner,parent.Wrapper.CharacterInterner)||
            !ReferenceEquals(source.Node.FirstGlobalSource,source.Source)||source.CopyAssignedScalars()!=scalars||
            !source.Node.CopyRanges16().AsSpan().SequenceEqual(key))
            throw new ArgumentException("Exact same-wrapper source/scalar/key snapshot required for actual metadata.");
        uint flags=scalars.Flags??throw new ArgumentException("Assigned source flags required.");
        int count=source.Node.RangeCount;
        bool needsKind=(flags&0x30600000)==0&&count==1&&key.Length==16&&
            BinaryPrimitives.ReadUInt32LittleEndian(key[4..])==BinaryPrimitives.ReadUInt32LittleEndian(key[12..])&&
            BinaryPrimitives.ReadUInt16LittleEndian(key[2..])==0&&key[1]==0&&
            BinaryPrimitives.ReadUInt16LittleEndian(key[10..])==3&&key[9]==255;
        if(!needsKind)
        {LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(flags,scalars.SelectionId!.Value,count,key);return flags;}
        receipt??=Prepare(coverage,token);
        if(!ReferenceEquals(receipt.Coverage,coverage))throw new ArgumentException("Foreign raw-slot receipt.");
        var kind=receipt.Lookup(BinaryPrimitives.ReadUInt32LittleEndian(key[4..]),token);
        return ProjectMetadata(flags,kind.ReportedKind);
    }
}

internal sealed record LegacyGlobalRawObjectKind(LegacyOwnedRawInkCurve OriginalCurve,
    int OriginalSourceIndex,int ConcreteType,int ReportedKind);
