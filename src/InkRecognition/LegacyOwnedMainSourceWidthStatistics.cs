using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainSourceWidth.Probe")]
namespace InkRecognition;

/// <summary>2188F0 accumulator fields;221840 converts the double mean to
/// TOP273.B0 only when the signed count is positive.</summary>
public readonly record struct LegacyMainSourceWidthStatistics(double WidthSum,int Count)
{
    public float AverageWidth=>Count>0?(float)(WidthSum/Count):0;
}

/// <summary>Actual source geometry and compact subtype owner for2188F0.
/// Each call still requires an explicit single native flush segment; the
/// complete backtrack trace must not be assumed to be one segment.</summary>
public sealed class LegacyOwnedMainSourceWidthStatistics
{
    private readonly byte[] compact;
    private readonly LegacyMainSelectedCompactWordOwner selected;
    public LegacyOwnedMainCompactWordMaterialization Materialization => selected.Single ??
        throw new InvalidOperationException("Configured width statistics have no count1 materialization.");
    public LegacyOwnedMainSecondaryCompactTerminal? SecondaryTerminal => selected.Secondary;
    public LegacyOwnedRussianMainSourceGeometry Geometry {get;}
    private LegacyOwnedMainSourceWidthStatistics(LegacyMainSelectedCompactWordOwner materialization,
        LegacyOwnedRussianMainSourceGeometry geometry)
    {selected=materialization;Geometry=geometry;compact=materialization.CopyCompactStorage();}

    public static LegacyOwnedMainSourceWidthStatistics Prepare(LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(geometry);
        if(!ReferenceEquals(materialization.ConfidenceOwner,geometry.Owner))
            throw new ArgumentException("Identical compact and actual source geometry owner required.");
        return new(LegacyMainSelectedCompactWordOwner.FromSingle(materialization),geometry);
    }

    public static LegacyOwnedMainSourceWidthStatistics PrepareConfigured(LegacyOwnedMainSecondaryCompactTerminal terminal,
        LegacyOwnedRussianMainSourceGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(terminal);ArgumentNullException.ThrowIfNull(geometry);
        if(!ReferenceEquals(terminal.Materialization.ConfidenceOwner,geometry.Owner))
            throw new ArgumentException("Identical configured compact and actual source geometry owner required.");
        return new(LegacyMainSelectedCompactWordOwner.FromSecondary(terminal),geometry);
    }

    public LegacyMainSourceWidthStatistics AccumulatePreparedSegment(ReadOnlySpan<byte> rows72,
        LegacyMainSourceWidthStatistics seed,CancellationToken token=default)
        =>Accumulate(rows72,compact,selected.RecordCount,seed,
            (node,arc)=>Geometry.ReadBounds(node,arc,token),token);

    internal static LegacyMainSourceWidthStatistics Accumulate(ReadOnlySpan<byte> rows72,ReadOnlySpan<byte> compact40,
        int compactCount,LegacyMainSourceWidthStatistics seed,Func<int,ushort,LegacyNativeResidualBounds> readBounds,
        CancellationToken token=default)
        =>LegacyMainSourceWidthKernel.Accumulate(rows72,compact40,compactCount,seed,readBounds,token);
}
