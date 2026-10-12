using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fields written by native23f insertion1ee030. Identity is an opaque
/// caller-owned symbol-context identity, not a managed/native address.</summary>
public readonly record struct LegacyFirstAcousticRecordFields(int TotalCost,int AcousticCost,
    byte PreviousAlternative,byte ContinuationType,ushort ArcOrdinal,byte Span,ushort Symbol,
    bool PathBit,ulong Identity,byte CompositeCategory,byte Traits);

/// <summary>Owned40-byte records for the native signed-cost insertion kernel.
/// It retains stable equal-cost ordering and native in-place identity updates.
/// Diagnostic640-byte payloads are outside this bounded API.</summary>
public sealed class LegacyFirstAcousticRecordTable
{
    public const int RecordSize=40;
    public const int ImpossibleCost=0x4000000;
    private readonly byte[] slots;
    public int Capacity {get;}
    public int Count {get;private set;}
    public LegacyFirstAcousticRecordTable(int capacity)
    {
        if(capacity is <1 or >256)throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity=capacity;slots=new byte[checked(capacity*RecordSize)];
        for(int i=0;i<capacity;i++)BinaryPrimitives.WriteInt32LittleEndian(slots.AsSpan(i*RecordSize),ImpossibleCost);
    }

    /// <summary>Explicit initial storage permits independent checks of native
    /// opaque-byte preservation. Active records need not already be sorted:
    /// an identity replacement can itself change native order.</summary>
    public LegacyFirstAcousticRecordTable(ReadOnlySpan<byte> initialSlots,int count)
    {
        if(initialSlots.Length is <RecordSize or >256*RecordSize||initialSlots.Length%RecordSize!=0||
            count<0||count>initialSlots.Length/RecordSize)throw new ArgumentException("Bounded40-byte storage/count required.");
        slots=initialSlots.ToArray();Capacity=slots.Length/RecordSize;Count=count;
    }

    /// <summary>Native1ee030 scans identities from the tail; a lower replacement
    /// stays at that position. Otherwise insert after equal costs, with capacity
    /// clipping. Upper packed bits and unwritten bytes retain destination data.</summary>
    public int Insert(LegacyFirstAcousticRecordFields fields,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(fields.Span>7||fields.CompositeCategory>63||fields.Traits>31)
            throw new ArgumentOutOfRangeException(nameof(fields));
        int index=-1;
        if(fields.Identity!=0)
            for(int i=Count-1;i>=0;i--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(BinaryPrimitives.ReadUInt64LittleEndian(slots.AsSpan(i*RecordSize+24))==fields.Identity)
                {
                    if(BinaryPrimitives.ReadInt32LittleEndian(slots.AsSpan(i*RecordSize))<=fields.TotalCost)return Count;
                    index=i;break;
                }
            }
        if(index<0)
        {
            index=Count;
            while(index>0&&BinaryPrimitives.ReadInt32LittleEndian(slots.AsSpan((index-1)*RecordSize))>fields.TotalCost)
            {cancellationToken.ThrowIfCancellationRequested();index--;}
            if(index>=Capacity)return Count;
            int nextCount=Math.Min(Capacity,Count+1);
            for(int i=nextCount-1;i>index;i--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                slots.AsSpan((i-1)*RecordSize,RecordSize).CopyTo(slots.AsSpan(i*RecordSize,RecordSize));
            }
            Count=nextCount;
        }
        var target=slots.AsSpan(index*RecordSize,RecordSize);
        BinaryPrimitives.WriteInt32LittleEndian(target,fields.TotalCost);
        BinaryPrimitives.WriteInt32LittleEndian(target[4..],fields.AcousticCost);
        target[9]=fields.PreviousAlternative;target[10]=fields.ContinuationType;
        BinaryPrimitives.WriteUInt16LittleEndian(target[12..],fields.Symbol);
        ushort packed=BinaryPrimitives.ReadUInt16LittleEndian(target[14..]);
        packed=(ushort)((packed&0xfc00)|(fields.Span<<1)|(fields.PathBit?1:0)|(fields.CompositeCategory<<4));
        BinaryPrimitives.WriteUInt16LittleEndian(target[14..],packed);
        BinaryPrimitives.WriteUInt16LittleEndian(target[16..],fields.ArcOrdinal);
        BinaryPrimitives.WriteUInt64LittleEndian(target[24..],fields.Identity);
        target[32]=(byte)((target[32]&0xe0)|fields.Traits);
        return Count;
    }

    public LegacyFirstAcousticRecordFields Get(int index)
    {
        if((uint)index>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(index));
        var s=slots.AsSpan(index*RecordSize,RecordSize);ushort p=BinaryPrimitives.ReadUInt16LittleEndian(s[14..]);
        return new(BinaryPrimitives.ReadInt32LittleEndian(s),BinaryPrimitives.ReadInt32LittleEndian(s[4..]),
            s[9],s[10],BinaryPrimitives.ReadUInt16LittleEndian(s[16..]),(byte)((p>>1)&7),
            BinaryPrimitives.ReadUInt16LittleEndian(s[12..]),(p&1)!=0,
            BinaryPrimitives.ReadUInt64LittleEndian(s[24..]),(byte)((p>>4)&63),(byte)(s[32]&31));
    }
    public byte GetLikelihoodByte(int index)
    {if((uint)index>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(index));return slots[index*RecordSize+8];}
    public void SetLikelihoodByte(int index,byte value)
    {if((uint)index>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(index));slots[index*RecordSize+8]=value;}
    public byte[] CopyStorage()=>slots.ToArray();
}
