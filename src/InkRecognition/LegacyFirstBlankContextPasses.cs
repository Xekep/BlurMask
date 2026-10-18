using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyFirstContextGeometricAncestor(int OriginalNode,int OriginalPrimaryRow,int OriginalContextAlternative,
    LegacyFirstAcousticRecordFields OriginalRecord,int GeometryNode,int GeometryPrimaryRow,int GeometryContextAlternative,
    LegacyFirstAcousticRecordFields GeometryRecord,int SkippedCount);
public readonly record struct LegacyFirstBlankContextCorrection(int Stage,bool FinalPass,int CurrentNode,int CurrentPrimaryRow,
    LegacyFirstAncestorTransitionRequest Request,LegacyFirstContextGeometricAncestor Previous,int IncomingCost,
    LegacyFirstOrdinaryTransitionResult Result);

/// <summary>Owned fresh zero-trait first B0/B8 context walks including blank
/// and header ancestors. Actual ranked costs and sampler corrections come from
/// the same producing kernel/source graph; no unsupported rows are discarded.</summary>
public static class LegacyFirstBlankContextPasses
{
    public static LegacyFirstOrdinaryContextPasses Run(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
        =>RunCore(resource,factory,cuts,prepared,producer,table,null,null,null,cancellationToken);

    public static LegacyFirstOrdinaryContextPasses RunWithObserver(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        Action<LegacyFirstBlankContextCorrection> observer,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(observer);return RunCore(resource,factory,cuts,prepared,producer,table,null,null,observer,cancellationToken);}

    //Friend-probe-only prepared-row controls. Actual public entry points above
    //never accept a primary/cost override and always invoke the owning producer.
    internal static LegacyFirstOrdinaryContextPasses RunPreparedZeroTraitRows(ReadOnlySpan<byte> resource,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        LegacyFirstOrdinaryAcousticRecurrence primary,LegacyFirstOrdinaryGlyphCost glyphCost,
        Action<LegacyFirstBlankContextCorrection> observer,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(primary);ArgumentNullException.ThrowIfNull(glyphCost);ArgumentNullException.ThrowIfNull(observer);
        return RunCore(resource,factory,cuts,prepared,producer,table,primary,glyphCost,observer,cancellationToken);}

    private static LegacyFirstOrdinaryContextPasses RunCore(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyFreshFirstBlankPrimary producer,LegacyCursiveSeparationTable table,
        LegacyFirstOrdinaryAcousticRecurrence? preparedPrimary,LegacyFirstOrdinaryGlyphCost? preparedCost,
        Action<LegacyFirstBlankContextCorrection>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(producer);ArgumentNullException.ThrowIfNull(prepared);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(producer.SourceRanges,prepared.SourceRanges))throw new ArgumentException("Primary producer and context source-range owner differ.");
        var geometry=new LegacyFirstAncestorTransitionGeometry(resource,factory,cuts,prepared,table,token);
        var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,factory,token);
        var binding=literal.Binding;var primary=preparedPrimary??producer.Run(token);var graph=producer.SourceRanges.Graph;var connections=new LegacyOrdinaryCandidateConnections(graph,token);
        if(primary.NodeCount!=graph.NodeCount||primary.BeamWidth!=factory.PrimaryBeamWidthF8)throw new ArgumentException("Prepared primary dimensions differ from owned source graph/F8.");
        int width=primary.BeamWidth,cap0=binding.ContextColumnsA2,cap8=binding.MergedColumnsA4;
        if((long)primary.NodeCount*width*(cap0+cap8)>1_048_576)throw new ArgumentException("Portable context cell budget exceeded.");
        ushort blank=checked((ushort)binding.Alphabet.LabelCount),shadda=binding.TrailingSymbolE6;
        int p=checked(binding.SourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        ushort newline=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+8)..]),space=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+10)..]),nbsp=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+12)..]);
        return LegacyFirstBlankContextKernel.Run(primary,graph,connections,width,cap0,cap8,blank,shadda,newline,space,nbsp,
            literal.ClosedSymbolE4,binding.NativeModeE0,producer.NativeConsecutiveBlankExclusion,preparedCost??producer.ReadGroupCost,
            geometry.Correct118,geometry.Correct140,observer,token);
    }
}
