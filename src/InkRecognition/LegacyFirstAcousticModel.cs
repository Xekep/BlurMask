using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Owned fresh no-dynamic type21d/v110 profile inside an already-selected row's
/// resource247. This is the first247/229 input model, not the baseline256/238 model.
/// Variable cuts, glyph normalization, final cache and word winner remain separate.
/// </summary>
public sealed class LegacyFirstAcousticModel
{
    public LegacyOrdinaryRowAcousticProfile RowProfile { get; }
    public int OrdinaryNodeOffset { get; }
    public int PrefixBytesConsumed { get; }
    public byte ContextSelector { get; }
    public short NormalizationTarget { get; }
    public LegacyInkNetwork ContextNetwork { get; }
    public LegacyInkNetwork TrajectoryNetwork { get; }
    public LegacyInkNetwork RasterNetwork { get; }
    public ReadOnlyMemory<LegacyAcousticWeightSlot> Slots { get; }

    private LegacyFirstAcousticModel(LegacyOrdinaryRowAcousticProfile row,int offset,int consumed,
        byte selector,short target,LegacyInkNetwork context,LegacyInkNetwork trajectory,
        LegacyInkNetwork raster,LegacyAcousticWeightSlot[] slots)
    {RowProfile=row;OrdinaryNodeOffset=offset;PrefixBytesConsumed=consumed;ContextSelector=selector;
        NormalizationTarget=target;ContextNetwork=context;TrajectoryNetwork=trajectory;RasterNetwork=raster;Slots=slots;}

    public static LegacyFirstAcousticModel ReadSource(ReadOnlySpan<byte> resource,int sourceOffset,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var row=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(resource,sourceOffset);
        int sourceEnd=NodeEnd(resource,sourceOffset,0x247,0x10d,resource.Length);
        int position=NodeEnd(resource,row.SmoothingNodeOffset,0x265,0x102,sourceEnd);
        if(position>=sourceEnd||resource[position++]!=1)throw new NotSupportedException("Current row source must own the graph/ordinary trio.");
        position=NodeEnd(resource,position,0x297,0x106,sourceEnd);
        position=NodeEnd(resource,position,0x21b,0x10a,sourceEnd);
        int offset=position,end=NodeEnd(resource,offset,0x21d,0x110,sourceEnd);
        //Reader1d4950 v10d: no optional knowledge field; extra-object count then
        //fixed metadata arrays. Metadata is inert data, not activation execution.
        if(sourceEnd-end!=190||BinaryPrimitives.ReadInt32LittleEndian(resource[end..])!=0)
            throw new NotSupportedException("Expected no extra source objects and the checked v10d metadata tail.");
        var r=new Reader(resource.Slice(offset,end-offset));r.Skip(16);r.Skip(2);
        short target=r.Short();if(target<0)throw new InvalidDataException("Negative normalization target.");if(target==0)target=100;
        if(!r.Flag())throw new NotSupportedException("Initial context network required.");
        var context=r.Network();r.Skip(3*4+2+2+4+1);byte selector=r.Byte();r.Skip(2);
        if(r.Flag())throw new NotSupportedException("This model owns the fresh no-dynamic branch only.");
        if(r.Byte()!=1||r.Flag())throw new NotSupportedException("Expected one non-grid acoustic profile.");
        r.Skip(2+4+5*2+7*4);r.EmptyShortList();r.EmptyShortList();r.EmptyShortList();
        r.Skip(2+2+4+4+2+2);
        if(r.Flag()||r.Flag())throw new NotSupportedException("Nested conversion/optional arrays unsupported in this profile.");
        var slots=new LegacyAcousticWeightSlot[7];var networks=new LegacyInkNetwork?[7];
        for(int i=0;i<7;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(r.Flag())throw new NotSupportedException("Per-slot conversion unsupported in this profile.");
            bool first=r.Flag(),second=r.Flag();byte mode=r.Byte();
            if(mode>2)throw new InvalidDataException("Invalid stored acoustic mode.");
            float weight=mode==0?r.Weight():0;byte mask=0;
            for(int n=0;n<3;n++)if(r.Flag())
            {var network=r.Network();mask|=(byte)(1<<n);if(n!=1)throw new NotSupportedException("Only second network per slot is supported.");networks[i]=network;}
            slots[i]=new(mode,weight,first,second,mask);
            if(i<2)
            {if(!first||!second||mode!=2||mask!=2)throw new NotSupportedException("First trajectory/raster slots must use fresh mode2.");}
            else if(first||second||mode!=0||weight!=0||mask!=0)throw new NotSupportedException("Only first two acoustic slots may be active.");
        }
        var trajectory=networks[0]!;var raster=networks[1]!;
        if(context.InputWidth!=30||context.Kind!=0x7001||trajectory.InputWidth!=247||trajectory.Kind!=0x1f||
            raster.InputWidth!=229||raster.Kind!=0x2f||trajectory.OutputWidth!=raster.OutputWidth||trajectory.OutputWidth>512)
            throw new InvalidDataException("Network widths/kinds differ from the first acoustic profile.");
        return new(row,offset,r.Position,selector,target,context,trajectory,raster,slots);
    }

    /// <summary>Constructor1b0330 initializes mode1=0/mode2=1; without dynamic1b0,
    /// callers1be930/1bf0a0 skip1b7440. This method applies only to a fresh owned model.</summary>
    public float[] GetFreshWeights()=>[1,1,0,0,0,0,0];

    /// <summary>Full caller-prepared fixed vectors. No baseline/context/clip policy is invented.</summary>
    public LegacyFirstAcousticScores EvaluatePreparedInputs(ReadOnlySpan<short> trajectory,
        ReadOnlySpan<short> raster,LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(activations);cancellationToken.ThrowIfCancellationRequested();
        var first=LegacyClassScores.Convert(TrajectoryNetwork.Evaluate(trajectory,activations,cancellationToken:cancellationToken),cancellationToken).Probability32768;
        var second=LegacyClassScores.Convert(RasterNetwork.Evaluate(raster,activations,cancellationToken:cancellationToken),cancellationToken).Probability32768;
        // No invented confidence calibration: this endpoint owns only the integer probability channel.
        return new(first,second);
    }

    private static int NodeEnd(ReadOnlySpan<byte> data,int offset,uint kind,ushort version,int end)
    {
        if(offset<0||offset>end-16||end>data.Length||BinaryPrimitives.ReadUInt32LittleEndian(data[offset..])!=0x0101fe90||
            BinaryPrimitives.ReadUInt32LittleEndian(data[(offset+8)..])!=kind||BinaryPrimitives.ReadUInt16LittleEndian(data[(offset+14)..])!=version)
            throw new InvalidDataException("Invalid typed source child.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(data[(offset+4)..]);
        if(length<8||length>(uint)(end-offset-8))throw new InvalidDataException("Source child escapes its parent.");
        return checked(offset+(int)length+8);
    }
    private ref struct Reader(ReadOnlySpan<byte> bytes)
    {
        private readonly ReadOnlySpan<byte> data=bytes;
        public int Position{get;private set;}
        private ReadOnlySpan<byte> Take(int count){if(count<0||count>data.Length-Position)throw new InvalidDataException("Truncated acoustic prefix.");var v=data.Slice(Position,count);Position+=count;return v;}
        public void Skip(int count)=>Take(count);
        public byte Byte()=>Take(1)[0];
        public bool Flag(){byte value=Byte();if(value>1)throw new InvalidDataException("Invalid acoustic boolean.");return value!=0;}
        public short Short()=>BinaryPrimitives.ReadInt16LittleEndian(Take(2));
        public float Weight(){float f=BinaryPrimitives.ReadSingleLittleEndian(Take(4));if(!float.IsFinite(f)||f<0)throw new InvalidDataException("Invalid stored weight.");return f;}
        public void EmptyShortList(){if(Short()!=0)throw new NotSupportedException("Only empty prefix lists supported.");}
        public LegacyInkNetwork Network()
        {
            var header=Take(8);uint length=BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
            if(length>1024*1024)throw new InvalidDataException("Network exceeds bounded profile.");
            int start=Position-8;Take((int)length);return LegacyInkNetwork.Load(data.Slice(start,(int)length+8));
        }
    }
}

/// <summary>Integer probabilities/costs only; no confidence or completed class cache implied.</summary>
public sealed class LegacyFirstAcousticScores
{
    public ReadOnlyMemory<int> TrajectoryProbability32768 {get;}
    public ReadOnlyMemory<int> RasterProbability32768 {get;}
    internal LegacyFirstAcousticScores(ReadOnlyMemory<int> trajectory,ReadOnlyMemory<int> raster)
    {TrajectoryProbability32768=trajectory;RasterProbability32768=raster;}
    public int[] GetFreshIntegerCosts(ReadOnlySpan<byte> enabledClasses=default,CancellationToken cancellationToken=default)
        =>LegacyAcousticCostEnsemble.Combine(new LegacyAcousticCostFamily?[]{
            new(TrajectoryProbability32768,1),new(RasterProbability32768,1),null,null,null,null,null},
            1,enabledClasses,cancellationToken);
}
