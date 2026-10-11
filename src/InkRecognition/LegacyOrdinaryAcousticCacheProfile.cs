using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Checked single-profile v110 fields used by ordinary final-cache production.</summary>
public sealed record LegacyOrdinaryAcousticCacheProfile(float FallbackPenalty,float LongDashWeight,
    float CjkDashWeight,float NegativeDirectionDivisor,float PositiveDirectionDivisor,
    float ClosedPrimitiveWeight,int CandidateThreshold,int MinimumCandidates,int CandidateLimit)
{
    public static LegacyOrdinaryAcousticCacheProfile ReadV110Node(ReadOnlySpan<byte> serialized,
        CancellationToken cancellationToken=default)
    {
        _=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(serialized,out int prefix,cancellationToken);
        if(BinaryPrimitives.ReadUInt16LittleEndian(serialized[12..])!=0)throw new InvalidDataException("Compressed acoustic objects unsupported.");
        int length=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(serialized[4..])+8);
        var r=new Reader(serialized[prefix..length]);
        r.Skip(1);float fallback=r.Float();r.Skip(1+8);
        int lists=r.U16();if(lists>512)throw new InvalidDataException("Too many candidate lists.");
        for(int i=0;i<lists;i++){cancellationToken.ThrowIfCancellationRequested();int n=r.U16();if(n>4096)throw new InvalidDataException("Candidate list too long.");r.Skip(n*2);}
        r.Skip(4);float longDash=r.Float(),cjkDash=r.Float(),negative=r.Float(),positive=r.Float(),closed=r.Float();
        if(r.Byte()!=0)throw new InvalidDataException("This ordinary profile excludes native size penalties.");r.Skip(8);
        if(r.Byte()!=0)throw new InvalidDataException("Obsolete nested conversion unsupported.");
        if(r.I16()!=0)throw new InvalidDataException("This ordinary profile excludes grouped probability smoothing.");
        int threshold=r.I32(),minimum=r.U16(),limit=r.U16();r.Skip(12+1);
        if(r.Float()!=0)throw new InvalidDataException("This ordinary profile excludes per-group power scaling.");
        var profile=new LegacyOrdinaryAcousticCacheProfile(fallback,longDash,cjkDash,negative,positive,closed,threshold,minimum,limit);
        profile.Validate();return profile;
    }
    internal void Validate()
    {
        foreach(float value in new[]{FallbackPenalty,LongDashWeight,CjkDashWeight,ClosedPrimitiveWeight})
            if(!float.IsFinite(value)||value<0)throw new InvalidDataException("Invalid ordinary cache coefficient.");
        if(!float.IsFinite(NegativeDirectionDivisor)||NegativeDirectionDivisor<=0||!float.IsFinite(PositiveDirectionDivisor)||PositiveDirectionDivisor<=0 ||
            CandidateThreshold is <0 or >0x4000000 || CandidateLimit is <1 or >150 || MinimumCandidates<1 || MinimumCandidates>CandidateLimit)
            throw new InvalidDataException("Invalid ordinary cache gates.");
    }
    private ref struct Reader(ReadOnlySpan<byte> bytes)
    {
        private readonly ReadOnlySpan<byte> bytes=bytes;private int offset;
        private ReadOnlySpan<byte> Take(int n){if(n<0||n>bytes.Length-offset)throw new InvalidDataException("Truncated final-cache tail.");var s=bytes.Slice(offset,n);offset+=n;return s;}
        public void Skip(int n)=>Take(n);public byte Byte()=>Take(1)[0];public ushort U16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));public short I16()=>BinaryPrimitives.ReadInt16LittleEndian(Take(2));public int I32()=>BinaryPrimitives.ReadInt32LittleEndian(Take(4));
        public float Float(){float value=BinaryPrimitives.ReadSingleLittleEndian(Take(4));if(!float.IsFinite(value))throw new InvalidDataException("Nonfinite final-cache profile.");return value;}
    }
}
