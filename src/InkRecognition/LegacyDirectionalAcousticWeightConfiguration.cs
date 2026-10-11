using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyAcousticWeightSlot(byte Mode,float StoredWeight,
    bool FirstFlag,bool SecondFlag,byte NetworkMask);

/// <summary>
/// Bounded type21d version110 prefix reader1c2940/1c25b0. It reads stored
/// dynamic type257 configuration and seven weight modes; remaining resource
/// properties are unconsumed. This supports the checked no-grid/no-nested-
/// conversion profile, not arbitrary legacy versions or global SDK defaults.
/// </summary>
public sealed class LegacyDirectionalAcousticWeightConfiguration
{
    public LegacyDynamicAcousticWeightProfile DynamicProfile {get;}
    public float ResourceCostMultiplier {get;}
    public ReadOnlyMemory<LegacyAcousticWeightSlot> Slots {get;}
    /// <summary>Reader raw+60, forwarded to type227 runtime+28.</summary>
    public byte StoredContextSelector {get;}
    public LegacyNativePriorContextMode ContextSelectionMode=>StoredContextSelector switch
        {0=>LegacyNativePriorContextMode.AllContained,1=>LegacyNativePriorContextMode.Disjoint,_=>LegacyNativePriorContextMode.SameLineAll};
    private LegacyDirectionalAcousticWeightConfiguration(LegacyDynamicAcousticWeightProfile profile,
        float multiplier,LegacyAcousticWeightSlot[] slots,byte contextSelector)
    {DynamicProfile=profile;ResourceCostMultiplier=multiplier;Slots=slots;StoredContextSelector=contextSelector;}

    public static LegacyDirectionalAcousticWeightConfiguration LoadV110Prefix(ReadOnlySpan<byte> serialized,
        out int consumedBytes,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(serialized.Length<16||BinaryPrimitives.ReadUInt32LittleEndian(serialized)!=0x0101fe90||
            BinaryPrimitives.ReadUInt32LittleEndian(serialized[8..])!=0x21d||
            BinaryPrimitives.ReadUInt16LittleEndian(serialized[14..])!=0x110)
            throw new InvalidDataException("Expected confirmed type21d version110 object.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(serialized[4..]);
        if(length<8||length>serialized.Length-8)throw new InvalidDataException("Type21d object length is outside its input.");
        var r=new Reader(serialized[..checked((int)length+8)]);r.Skip(16);
        r.Skip(4); // flags8/9, point target10; v110 implicit cost multiplier1.
        if(!r.Flag())throw new InvalidDataException("Checked profile requires an initial network.");
        r.Network();
        r.Skip(3*4+2+2+4+1); // initial-network context parameters48..5c.
        byte contextSelector=r.Byte(); // native raw+60, zero-extended from serializedbyte.
        r.Skip(2); // raw64/65.
        if(!r.Flag())throw new InvalidDataException("Checked profile requires dynamic type257 weights.");
        int count=r.UInt16();
        var profile=new LegacyDynamicAcousticWeightProfile(count,r.Float(),r.Float(),r.Float(),r.Float(),
            r.Float(),r.Float(),r.Float(),r.Float());
        r.Float(); // separate recognizer property1c4; not part of type257.
        if(r.Byte()!=1)throw new InvalidDataException("Only the checked single acoustic profile is supported.");
        if(r.Flag())throw new InvalidDataException("Grid profile prefix is outside the checked schema.");
        r.Skip(2+4+5*2+7*4); // profile14..44 (opaque geometry fields).
        r.Shorts(1);r.Shorts(2);r.Shorts(2); // lists48,58,70.
        r.Skip(2+2+4+4+2+2); // profile88..9e.
        if(r.Flag())throw new InvalidDataException("Nested a0 conversion is outside the checked schema.");
        if(r.Flag())r.Shorts(2); // optional two arrays at a8.
        var slots=new LegacyAcousticWeightSlot[7];
        for(int i=0;i<7;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(r.Flag())r.Shorts(2);
            bool first=r.Flag(),second=r.Flag();byte mode=r.Byte();
            if(mode>2)throw new InvalidDataException("Unknown acoustic weight mode.");
            float weight=mode==0?r.Float():0;
            byte mask=0;
            for(int n=0;n<3;n++)if(r.Flag()){r.Network();mask|=(byte)(1<<n);}
            slots[i]=new(mode,weight,first,second,mask);
        }
        // Reuse evaluator validation without inventing a second config policy.
        _=LegacyDynamicAcousticWeights.Evaluate(new InkStroke[]{new(new InkPoint[]{new(0,0)})},profile,cancellationToken);
        consumedBytes=r.Position;
        return new(profile,1f,slots,contextSelector);
    }

    /// <summary>Native mode0/1/2 resolution from explicit runtime geometry results.
    /// Runtime enabled slots, multiplier flag and family selection remain separate.</summary>
    public float[] ResolveWeights(LegacyDynamicAcousticWeightResult geometry,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!float.IsFinite(geometry.Mode1Weight)||!float.IsFinite(geometry.Mode2Weight)||
            geometry.Mode1Weight<0||geometry.Mode2Weight<0)
            throw new ArgumentException("Explicit finite nonnegative dynamic weights required.",nameof(geometry));
        var weights=new float[7];
        for(int i=0;i<weights.Length;i++)
        {
            var slot=Slots.Span[i];weights[i]=slot.Mode switch{0=>slot.StoredWeight,1=>geometry.Mode1Weight,2=>geometry.Mode2Weight,_=>throw new InvalidDataException("Unknown stored mode.")};
        }
        return weights;
    }

    private ref struct Reader(ReadOnlySpan<byte> source)
    {
        private readonly ReadOnlySpan<byte> source=source;
        public int Position{get;private set;}
        public ReadOnlySpan<byte> Take(int length)
        {if(length<0||length>source.Length-Position)throw new InvalidDataException("Truncated acoustic prefix.");var b=source.Slice(Position,length);Position+=length;return b;}
        public void Skip(int length)=>Take(length);
        public byte Byte()=>Take(1)[0];
        public bool Flag()=>Byte()!=0;
        public ushort UInt16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public float Float(){float f=BinaryPrimitives.ReadSingleLittleEndian(Take(4));if(!float.IsFinite(f)||f<0)throw new InvalidDataException("Expected finite nonnegative stored weight/configuration.");return f;}
        public void Shorts(int copies){int n=UInt16();if(n>512)throw new InvalidDataException("Prefix array exceeds512 entries.");Skip(n*2*copies);}
        public void Network()
        {
            var header=Take(8);
            if(BinaryPrimitives.ReadUInt16LittleEndian(header)!=0xfea0||BinaryPrimitives.ReadUInt16LittleEndian(header[2..])!=0x106)
                throw new InvalidDataException("Only confirmed network version106 is supported.");
            uint length=BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
            if(length is < 3 or > 1024*1024)throw new InvalidDataException("Network byte length exceeds checked bounds.");
            Skip((int)length);
        }
    }
}
