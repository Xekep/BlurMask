using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Type21d runtime F0/F4/F8 class-cost gates, forwarded unchanged by1b0560.</summary>
public readonly record struct LegacyAcousticCandidateThresholds(int KindOne,int KindTwo,int KindThree)
{
    /// <summary>Confirmed single-profile type21d v110 prefix and bounded tail through F8.</summary>
    public static LegacyAcousticCandidateThresholds ReadV110Node(ReadOnlySpan<byte> serialized,
        out int consumedBytes,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(serialized,out int prefix,cancellationToken);
        if(BinaryPrimitives.ReadUInt16LittleEndian(serialized[12..])!=0)
            throw new InvalidDataException("Compressed acoustic objects are unsupported.");
        int length=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(serialized[4..])+8);
        var reader=new Reader(serialized[prefix..length]);
        reader.Skip(1+4+1+8); // raw78/7c/80/88/84.
        int lists=reader.U16();
        if(lists>512)throw new InvalidDataException("Too many acoustic tail lists.");
        for(int i=0;i<lists;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();int count=reader.U16();
            if(count>4096)throw new InvalidDataException("Acoustic tail list exceeds checked bounds.");
            reader.Skip(count*2);
        }
        reader.Skip(6*4+1+8); // raw a0..b4, b8,bc,c0.
        if(reader.Byte()!=0)throw new InvalidDataException("Nested obsolete acoustic conversion is unsupported.");
        int groups=reader.I16(); // raw c8.
        if(groups<0||groups>512)throw new InvalidDataException("Acoustic tail groups exceed checked bounds.");
        if(groups>0)
        {
            int entries=0;
            for(int i=0;i<=groups;i++)entries=reader.U16();
            if(entries>4096)throw new InvalidDataException("Acoustic group indices exceed checked bounds.");
            reader.Skip(entries*2+4); // raw d8 indices, common e0 value (v110).
        }
        reader.Skip(4+2+2); // raw e8/ec/ee, unrelated to this scorer.
        var result=new LegacyAcousticCandidateThresholds(reader.I32(),reader.I32(),reader.I32());
        try{result.Validate();}catch(ArgumentException exception){throw new InvalidDataException("Invalid acoustic class-cost thresholds.",exception);}
        consumedBytes=prefix+reader.Position;return result;
    }

    public void Validate()
    {
        if(KindOne is <0 or >0x4000000 || KindTwo is <0 or >0x4000000 || KindThree is <0 or >0x4000000)
            throw new ArgumentException("Class-cost thresholds must lie within the checked impossible-cost range.");
    }
    private ref struct Reader(ReadOnlySpan<byte> bytes)
    {
        private readonly ReadOnlySpan<byte> bytes=bytes;
        public int Position{get;private set;}
        private ReadOnlySpan<byte> Take(int n)
        {if(n<0||n>bytes.Length-Position)throw new InvalidDataException("Truncated acoustic candidate tail.");var value=bytes.Slice(Position,n);Position+=n;return value;}
        public void Skip(int n)=>Take(n);
        public byte Byte()=>Take(1)[0];
        public ushort U16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public short I16()=>BinaryPrimitives.ReadInt16LittleEndian(Take(2));
        public int I32()=>BinaryPrimitives.ReadInt32LittleEndian(Take(4));
    }
}
