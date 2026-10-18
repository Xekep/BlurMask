using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainDocumentInputTransforms.Probe")]

namespace InkRecognition;

/// <summary>Owned post023260 ->037E80(writeDomfalse) ->026EF0 fresh branch.
/// Retained INPUT ranges export through this same selected current-view owner.
/// Exact identity exclusion produces no transform80 records, and actual INPUT
/// span children produce no DAF32 rows. No accepted document is created.</summary>
public sealed class LegacyFreshMainDocumentInputTransforms
{
    private sealed class Entry {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainTextDocumentSeed,Entry> lifetime=new();
    private readonly LegacyDocumentTransformVector transforms;
    public LegacyFreshMainDocumentControlCleanup Cleanup {get;}
    public LegacyFreshMainTextDocumentSeed Seed {get;}
    public LegacyOwnedTextFieldInputIdentityExport SourceExport {get;}
    public int DocumentAlternativeCountD8 {get;}
    public int TransformCount110=>transforms.Count;
    public int DafInputCount {get;}
    public bool Succeeded {get;}
    public bool VectorWasSwapped=>true;
    public bool FollowingStage026EF0Executed {get;}
    internal LegacyDocumentTransformVector VectorOwner=>transforms;
    internal LegacyDocumentTransformVector ReleasedVectorOwner {get;}
    private LegacyFreshMainDocumentInputTransforms(LegacyFreshMainDocumentControlCleanup cleanup,
        LegacyFreshMainTextDocumentSeed seed,LegacyOwnedTextFieldInputIdentityExport export,int alternatives,
        LegacyDocumentTransformVector transforms,int dafCount,LegacyDocumentTransformSwap swap)
    {Cleanup=cleanup;Seed=seed;SourceExport=export;DocumentAlternativeCountD8=alternatives;
     this.transforms=transforms;DafInputCount=dafCount;ReleasedVectorOwner=swap.Released;
     Succeeded=swap.ExportSucceeded;FollowingStage026EF0Executed=swap.ExportSucceeded;}

    public static LegacyFreshMainDocumentInputTransforms Apply(
        LegacyFreshMainDocumentControlCleanup actualCleanup,CancellationToken token=default)
        =>ApplyCore(actualCleanup,null,token);

    private sealed record PreparedExport(LegacyDocumentTransformVector Vector,bool Succeeded);
    //Fault controls enter a genuine separate seed lifetime but provide an
    //explicit prepared export prefix/status. No public caller can select it.
    internal static LegacyFreshMainDocumentInputTransforms ApplyPreparedExport(
        LegacyFreshMainDocumentControlCleanup actualCleanup,LegacyDocumentTransformVector preparedPrefix,
        bool preparedSucceeded,CancellationToken token=default)
        =>ApplyCore(actualCleanup,new(preparedPrefix,preparedSucceeded),token);

    private static LegacyFreshMainDocumentInputTransforms ApplyCore(
        LegacyFreshMainDocumentControlCleanup actualCleanup,PreparedExport? prepared,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualCleanup);actualCleanup.ValidateCurrent(token);
        var words=actualCleanup.State.Text.WordChoices;var owner=words.Owner;
        var seed=owner.DocumentSeed??throw new NotSupportedException("The old OriginalID selection route has no native selected-input result seed.");
        seed.Validate(owner,token);
        if(seed.OriginalSelectedInputContext is not null)
            throw new NotSupportedException("Original-bootstrap selected transform/transport ownership is a separate adapter boundary.");
        int alternatives=words.Alternatives.Count;
        if(alternatives!=2)throw new NotSupportedException("Actual complete two-alternative document result required; source-line count is notD8.");
        var entry=lifetime.GetValue(seed,_=>new());
        lock(entry)
        {
            token.ThrowIfCancellationRequested();
            if(entry.Claimed)throw new InvalidOperationException("This fresh result transform-vector lifetime has already been entered.");
            entry.Claimed=true;
            //All entered failures retain the claimed lifetime. No retry can
            //reset the native output prefix through a newly prepared object.
            var export=seed.SelectedInputContext.ExportInputIdentityTransforms(seed.Input,token);
            if(!ReferenceEquals(export.Owner,seed.SelectedInputContext)||!ReferenceEquals(export.Input,seed.Input)||
                export.Geometry.NativeNonidentityTransformCount!=0)
                throw new NotSupportedException("The actual ordinary full-current-view identity-excluded export is required; nonidentity80B codec is unsupported.");
            //The old108 owner was constructed on this exact seed before any
            //WORD publication. Replace it even if an explicit prepared fault
            //returns false; keep that real seed output prefix on failure.
            var swapped=seed.SwapTransforms(prepared?.Vector??LegacyDocumentTransformVector.Fresh(),prepared?.Succeeded??true);
            var current=seed.CurrentTransforms;
            if(!swapped.ExportSucceeded)
                return new(actualCleanup,seed,export,alternatives,current,seed.CopyDafInputRows32().Length/32,swapped);
            //029861 tests export status before026EF0. DAF32 is read from the
            //actual retained INPUT's children, not inferred from empty108.
            byte[] daf=seed.CopyDafInputRows32();
            if(daf.Length!=0)throw new NotSupportedException("Nonempty DAF input matching is unsupported.");
            actualCleanup.ValidateCurrent(token);seed.Validate(owner,token);
            return new(actualCleanup,seed,export,alternatives,current,daf.Length/32,swapped);
        }
    }
    /// <summary>No row exists in the supported identity-excluded branch.
    /// No native pointer header or matrix bytes are synthesized.</summary>
    public byte[] CopyTransformScalarStorage64()=>transforms.CopyScalars();
}

//The typed vector sidecars model037E80 header ownership, not fabricated native
//addresses. Prepared fault controls exercise swap/status independently from
//the strictly owned successful current-view exporter above.
internal sealed class LegacyDocumentTransformVector
{
    private readonly byte[] scalars;
    private readonly LegacyNativeInkPositionRange[][] ranges;
    internal int Count=>ranges.Length;
    private LegacyDocumentTransformVector(byte[] scalars,LegacyNativeInkPositionRange[][] ranges)
    {this.scalars=scalars;this.ranges=ranges;}
    internal static LegacyDocumentTransformVector Fresh()=>new([],[]);
    internal static LegacyDocumentTransformVector Prepared(ReadOnlySpan<byte> scalars64,
        LegacyNativeInkPositionRange[][] ranges)
    {
        if(ranges.Length>4096||scalars64.Length!=checked(ranges.Length*64))throw new ArgumentException("Complete typed80B vector sidecars required.");
        return new(scalars64.ToArray(),ranges.Select(r=>r.ToArray()).ToArray());
    }
    internal byte[] CopyScalars()=>scalars.ToArray();
    internal LegacyNativeInkPositionRange[][] CopyRanges()=>ranges.Select(r=>r.ToArray()).ToArray();
}
internal sealed record LegacyDocumentTransformSwap(bool ExportSucceeded,LegacyDocumentTransformVector Released)
{
    internal static LegacyDocumentTransformSwap Apply(ref LegacyDocumentTransformVector current,
        LegacyDocumentTransformVector exported,bool exportSucceeded)
    {
        var old=current;current=exported;
        //Native037F1C saves AL,037F2D swaps unconditionally, and only the
        //outer029861 gate decides whether026EF0 may execute.
        return new(exportSucceeded,old);
    }
}
