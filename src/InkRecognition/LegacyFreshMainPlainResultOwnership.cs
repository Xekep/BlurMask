using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainWordPacket.Probe")]
namespace InkRecognition;

/// <summary>Fresh main TOP273 pool/publisher ownership. This restores the
/// result factory policies, not accepted alternatives or full2271F0 output.</summary>
public sealed class LegacyFreshMainPlainResultOwnership
{
    private readonly LegacyFreshMainPlainResultOwnership? originalTop;
    private readonly object widthGate=new();
    private uint? firstWidthBits;
    /// <summary>The identical TOP273 pool owner across independent279 lists.</summary>
    public LegacyFreshMainPlainResultOwnership TopOwner=>originalTop??this;
    internal LegacyMainSelectedCompactWordOwner SelectedOwner {get;}
    public LegacyOwnedMainCompactWordMaterialization Materialization => SelectedOwner.Single ??
        throw new InvalidOperationException("Configured secondary ownership has no count1 materialization.");
    public LegacyOwnedMainSecondaryCompactTerminal? SecondaryTerminal => SelectedOwner.Secondary;
    public LegacyOwnedRussianMainSourceGeometry Geometry {get;}
    public LegacyOwnedFreshMainWordControllerPolicy Controller=>SelectedOwner.Controller;
    public LegacyNativeGlyphRecordPool GlyphPool {get;}
    public LegacyNativePlainWordPublisher WordPublisher {get;}
    public bool CaptureGlyphGeometry=>false;
    public bool OwnerC4=>false;
    private LegacyFreshMainPlainResultOwnership(LegacyMainSelectedCompactWordOwner materialization,
        LegacyOwnedRussianMainSourceGeometry geometry,LegacyNativeGlyphRecordPool pool,LegacyNativePlainWordPublisher publisher,
        LegacyFreshMainPlainResultOwnership? originalTop=null)
    {SelectedOwner=materialization;Geometry=geometry;GlyphPool=pool;WordPublisher=publisher;this.originalTop=originalTop;}

    /// <summary>Independent fresh279 publisher over this actual TOP's pool,
    /// materialization, source and confidence. This does not select a new
    /// terminal or change the native fresh secondary-alternative limit.</summary>
    public LegacyFreshMainPlainResultOwnership ForkAlternative(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        var publisher=WordPublisher.ForkAlternative();
        return new(SelectedOwner,Geometry,GlyphPool,publisher,TopOwner);
    }

    public static LegacyFreshMainPlainResultOwnership Create(LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceGeometry geometry,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var policy=materialization.TerminalResult.PassResult.ControllerOwner;
        if(!ReferenceEquals(materialization.ConfidenceOwner,geometry.Owner)||
            !ReferenceEquals(policy.Owner.SourceGraph,geometry.Graph))
            throw new ArgumentException("Identical actual main compact/source geometry owner required.");
        if(policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||
            policy.Refresh.SourceFormat22f9!=0)
            throw new NotSupportedException("Fresh nontracing plain main result policies required.");
        //Fresh155DA0/155EF0 driver220=0; common157C60 passes it to
        //TOP273 constructor13B870 ->27341 ->274 capture10. Fresh00C860 C4=0.
        //SourceFormat22F9 was derived from the actual selected247 B9.
        var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);
        var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
        token.ThrowIfCancellationRequested();return new(LegacyMainSelectedCompactWordOwner.FromSingle(materialization),geometry,pool,publisher);
    }

    /// <summary>A separate actual configured2410 terminal in this same TOP
    /// retains the original pooled275 interning state and glyph pool, with an
    /// independent279 WORD list. A foreign compact pool is rejected.</summary>
    public LegacyFreshMainPlainResultOwnership ForkConfiguredAlternative(
        LegacyOwnedMainSecondaryCompactTerminal terminal,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(terminal);token.ThrowIfCancellationRequested();
        if(SecondaryTerminal is null || !ReferenceEquals(SecondaryTerminal.Materialization,terminal.Materialization))
            throw new ArgumentException("Configured alternatives must retain their actual same compact/source pool.");
        var publisher=WordPublisher.ForkAlternative();
        return new(LegacyMainSelectedCompactWordOwner.FromSecondary(terminal),Geometry,GlyphPool,publisher,TopOwner);
    }

    public static LegacyFreshMainPlainResultOwnership CreateConfigured(LegacyOwnedMainSecondaryCompactTerminal terminal,
        LegacyOwnedRussianMainSourceGeometry geometry,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(terminal);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var materialization=terminal.Materialization;
        var policy=materialization.PassResult.SourcePolicy.Controller;
        if(!ReferenceEquals(materialization.ConfidenceOwner,geometry.Owner)||
            !ReferenceEquals(policy.Owner.SourceGraph,geometry.Graph))
            throw new ArgumentException("Identical actual configured compact/source geometry owner required.");
        if(policy.SecondaryCountProperty12d0003!=2||policy.Configuration.SecondaryAlternativeCount2404!=2||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||
            policy.Refresh.SourceFormat22f9!=0)
            throw new NotSupportedException("Known configured2 nontracing plain main result policies required.");
        var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);
        var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
        token.ThrowIfCancellationRequested();
        return new(LegacyMainSelectedCompactWordOwner.FromSecondary(terminal),geometry,pool,publisher);
    }

    internal bool TryReadTopWidthStatistics(out uint bits)
    {
        var top=TopOwner;
        lock(top.widthGate)
        { bits=top.firstWidthBits??0;return top.firstWidthBits.HasValue; }
    }
    internal uint EstablishTopWidthStatistics(uint bits)
    {
        var top=TopOwner;
        lock(top.widthGate)
        {
            if(top.firstWidthBits is {} existing)return existing;
            if(!ReferenceEquals(top,this))
                throw new InvalidOperationException("The actual first alternative must establish TOP.B0 before its forks.");
            top.firstWidthBits=bits;return bits;
        }
    }

    /// <summary>Explicit complete allocator residue; native13F110 initializes
    /// only its own fields. The initial WORD44..53 bytes remain caller storage.</summary>
    public LegacyNativeWordBuilder CreateWorkingWord(ReadOnlySpan<byte> emptyAllocatorResidue120,CancellationToken token=default)
        =>LegacyNativeWordBuilder.CreateEmpty(GlyphPool,emptyAllocatorResidue120,token);
}
