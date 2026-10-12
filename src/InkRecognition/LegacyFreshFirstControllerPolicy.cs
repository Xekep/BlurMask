using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal constructor/child249/source247 scalar policy. This is the
/// fresh no-linguistic-source class23f branch, not the main class240 word decoder.</summary>
public sealed record LegacyFreshFirstControllerPolicy(uint FrontendFlags,int SourceMode,int SourceVariant,
    int MaximumSourceArcs,int SourceAuxiliaryLimit,int SourceAlternativeLimit,bool PruneCandidates,
    int MaximumNormalizedStrokes)
{
    public static LegacyFreshFirstControllerPolicy ReadFirstSource(ReadOnlySpan<byte> resource,int sourceOffset,
        CancellationToken cancellationToken=default)
    {
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        int tail=model.OrdinaryNodeOffset+8+checked((int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(model.OrdinaryNodeOffset+4)..]));
        //Validated first source loader requires count0 +186 metadata bytes.
        var metadata=resource.Slice(tail+4,186);
        const int mode=13,variant=2; //00c860 +18=020DFFFF;156630 preserves1a/1b.
        uint mask=BinaryPrimitives.ReadUInt32LittleEndian(metadata[(mode*4)..]);
        uint flags=(0u|0x21703u)&~mask; //1503f0 flags0;00cb50->1d60d0->151b90.
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(64+mode*2)..]);
        int auxiliary=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(96+mode*2)..]);
        bool prune=metadata[128]>mode; //Fresh1c=-1 becomes0 iff raw1ec>source1a.
        int alternatives=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(162+variant*2)..]);
        //1c2940 serializes profile14,18,1c in this order. First model reader
        //already requires one active, non-grid profile. Initializer1b0760 sets
        //runtime profile248=1;1b4bd0 applies signed profile1c/3 if profile14!=0.
        int contextNetwork=model.OrdinaryNodeOffset+21;
        int profileStart=checked(contextNetwork+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(contextNetwork+4)..])+27);
        short gate=BinaryPrimitives.ReadInt16LittleEndian(resource[profileStart..]);
        int cap=gate==0?short.MaxValue:BinaryPrimitives.ReadInt16LittleEndian(resource[(profileStart+6)..])/3;
        if(cap<1)throw new NotSupportedException("Active pen-up profile must allow at least one normalized stroke.");
        return new(flags,mode,variant,arcs,auxiliary,alternatives,prune,cap);
    }

    /// <summary>Fresh21d1a4=0, containerf5=0. Primitive scope/probability and
    /// source-specific penalty labels are supplied by their separate producers.</summary>
    public LegacyOrdinaryAcousticCacheContext CreateContext(LegacySourcePrimitiveScope primitive,
        ReadOnlySpan<ushort> sourcePenaltyLabels)
        =>new(PruneCandidates,false,(FrontendFlags&0x10)!=0,
            primitive.BoundaryAlignedSingleNormalizedStroke,primitive.EndpointFieldsAreZero,
            primitive.SourceStrokeProbability,false,sourcePenaltyLabels.ToArray());
}
