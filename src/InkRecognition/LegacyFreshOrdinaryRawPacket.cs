namespace InkRecognition;

/// <summary>Defined ordinary58-byte input-descriptor fields after167210.
/// These are separate from physical1c-byte stroke records and their chronology.</summary>
public readonly record struct LegacyOrdinaryRawInputDescriptor(ushort Flags,int FirstPhysicalStroke,
    int LastPhysicalStroke,int SourceItemId,int CurrentLineId,int PriorLineId,int ScopeId,
    int AuxiliaryId,int PhysicalBoundaryId,float FirstX,float FirstY,bool Horizontal,
    ushort FirstEndpointCategory,ushort LastEndpointCategory,ushort ThirdEndpointCategory,ushort FourthEndpointCategory);

/// <summary>Actual fresh, unscoped direct167210 float ingress. Constructor8e=0
/// and ordinary descriptorkind0 coalesce physical pen lifts into one descriptor.
/// It does not manufacture BeginPacket164a10 events or page/line assignment.</summary>
public sealed class LegacyFreshOrdinaryRawPacket
{
    public LegacyOrdinarySourceInk Source {get;}
    public LegacyOrdinaryRawInputDescriptor Descriptor {get;}
    public int DescriptorCount=>1;
    public int PhysicalStrokeCount=>Source.Strokes.Count;
    private LegacyFreshOrdinaryRawPacket(LegacyOrdinarySourceInk source,LegacyOrdinaryRawInputDescriptor descriptor)
    {Source=source;Descriptor=descriptor;}

    /// <summary>Fresh resource247/v10d source parameters are cleared by1d4950;
    /// reader1d41e0 never writes init68..6a, hence ingress pause/context flags
    /// 83/85/86 are zero. No optional callbacks, supplied endpoint tags or
    /// explicit semantic packet boundaries are invoked by this entry point.</summary>
    public static LegacyFreshOrdinaryRawPacket PrepareFirstSource(ReadOnlySpan<byte> resource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var owned=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        return PrepareDirect(rawPhysicalStrokes,owned.RowProfile.Source,cancellationToken);
    }

    /// <summary>The explicitly selected branch is fresh unscoped direct input,
    /// not a general events API. Geometry/chronology/probabilities come from
    /// the same checked ordinary ingress, not from caller descriptor IDs.</summary>
    public static LegacyFreshOrdinaryRawPacket PrepareDirect(IReadOnlyList<InkStroke> rawPhysicalStrokes,
        LegacyOrdinarySourceProfile sourceProfile,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source=LegacyOrdinarySourceInk.Prepare(rawPhysicalStrokes,sourceProfile,cancellationToken);
        //159810 initializes a new88-byte descriptor to zero.163970 sets source
        //88/94/98/a4/a8=-1,a0=int.MinValue,ac/b0=0.167210 writes kind0+bit4,
        //first physical count beforeappend and keeps that start while extending
        //last physical count. No custom direction callback means horizontal1.
        var first=source.Strokes[0].Points[0];
        var descriptor=new LegacyOrdinaryRawInputDescriptor(0x10,0,source.Strokes.Count-1,int.MinValue,
            -1,-1,-1,-1,-1,first.X,first.Y,true,ushort.MaxValue,ushort.MaxValue,ushort.MaxValue,ushort.MaxValue);
        cancellationToken.ThrowIfCancellationRequested();return new(source,descriptor);
    }

    /// <summary>218.virtual10/168ad0 reads descriptor18+ordinal*58. Fresh raw
    /// pen-up ordinal is not that descriptor ordinal: all retained strokes in
    /// this packet resolve to0, even though chronology.Second increments.</summary>
    public int GetDescriptorOrdinalForPhysicalStroke(int physicalStroke,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)physicalStroke>=(uint)PhysicalStrokeCount)throw new ArgumentOutOfRangeException(nameof(physicalStroke));
        return 0;
    }

    /// <summary>Resolve source descriptor ownership of a supplied native group.
    /// Range coordinates still address the caller-prepared physical stroke
    /// buffer; this only supplies proven descriptor identity and validates its
    /// physical membership. Group cuts/normalization remain separate producers.</summary>
    public int GetDescriptorOrdinalForGroup(ReadOnlySpan<LegacyInkRange> nativeRanges,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(nativeRanges.Length is <1 or >15)throw new ArgumentException("Bounded native group ranges required.");
        foreach(var range in nativeRanges)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if((uint)range.StrokeIndex>=(uint)PhysicalStrokeCount||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint)
                throw new ArgumentException("Range lacks owned ordinary physical membership.");
        }
        return 0;
    }
}
