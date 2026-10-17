using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Prepared13A830 result, retaining the complete owner and active
/// selection allocation. This does not perform lazy217670 owner refresh.</summary>
public sealed class LegacyPreparedGlyphAlternativeSelection
{
    private readonly byte[] ownerStorage,selectionStorage;
    public ulong OwnerKey {get;}
    public ushort PackedOwnerCount {get;}
    public int NativeOwnerCount {get;}
    public bool Succeeded {get;}
    public int Count {get;}
    public int Capacity {get;}
    /// <summary>Native optional marker writeback: sorted position on success,
    /// unchanged incoming value on allocation failure, null when not supplied.</summary>
    public uint? SelectedMarker {get;}
    public byte[] CopyOwnerStorage40()=>ownerStorage.ToArray();
    public byte[] CopySelectionCapacityStorage4()=>selectionStorage.ToArray();
    public byte[] CopySelectionRows4()=>selectionStorage.AsSpan(0,Count*4).ToArray();

    private LegacyPreparedGlyphAlternativeSelection(ulong key,ushort packed,int nativeCount,
        byte[] owner,byte[] selection,int count,int capacity,bool succeeded,uint? marker)
    {OwnerKey=key;PackedOwnerCount=packed;NativeOwnerCount=nativeCount;ownerStorage=owner;
        selectionStorage=selection;Count=count;Capacity=capacity;Succeeded=succeeded;SelectedMarker=marker;}

    /// <summary>Exact bounded13A830 insertion/deduplication over prepared40-byte
    /// owner rows. The explicit allocation image supplies prior capacity bytes
    /// and each newly allocated tail. Growth beyond its length is an injected
    /// allocation refusal, returning native partial rows and unchanged marker.
    /// It is not evidence that a native allocator or217670 succeeded.</summary>
    public static LegacyPreparedGlyphAlternativeSelection Select(LegacyNativeGlyphRecordNode actualOwner,
        uint? selectedMarker,ReadOnlySpan<byte> allocationStorage4,int initialCapacity=0,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(actualOwner.Key!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(actualOwner.Key)||
            actualOwner.Records.Length%40!=0||actualOwner.Records.Length>65536*40||
            allocationStorage4.Length%4!=0||allocationStorage4.Length>65536*4||
            initialCapacity<0||initialCapacity>allocationStorage4.Length/4)
            throw new ArgumentException("Canonical owner, bounded complete rows and explicit allocation image required.");
        int nativeCount=unchecked((short)(actualOwner.PackedCount<<2))>>4;
        if(nativeCount>actualOwner.Records.Length/40)
            throw new ArgumentException("Packed owner count exceeds its actual row allocation.");
        byte[] owner=actualOwner.Records.ToArray(),storage=allocationStorage4.ToArray();
        int count=0,capacity=initialCapacity;uint representative=selectedMarker??0;
        uint originalMarker=representative;bool succeeded=true;
        for(int current=0;current<nativeCount;current++)
        {
            cancellationToken.ThrowIfCancellationRequested();int duplicate=0;
            while(duplicate<count&&!Same(current,Ordinal(duplicate)))
            {cancellationToken.ThrowIfCancellationRequested();duplicate++;}
            float score=Score(current);
            if(duplicate==count)
            {
                int position=0;
                //COMISS/JA: unordered and equal scores do not insert before.
                while(position<count&&!(score>Score(Ordinal(position))))position++;
                if(count==capacity)
                {
                    int requested=Math.Max(count+1,(int)((double)capacity*1.5));
                    if(requested>storage.Length/4){succeeded=false;break;}
                    capacity=requested;
                }
                storage.AsSpan(position*4,(count-position)*4).CopyTo(storage.AsSpan((position+1)*4));
                WriteOrdinal(position,current);count++;
            }
            else
            {
                int previous=Ordinal(duplicate);
                //COMISS/JBE also takes the ignore branch for unordered NaN.
                if(score>Score(previous))
                {
                    if((uint)previous==representative)representative=(uint)current;
                    int position=0;
                    while(position<duplicate&&!(score>Score(Ordinal(position))))position++;
                    storage.AsSpan(position*4,(duplicate-position)*4).CopyTo(storage.AsSpan((position+1)*4));
                    WriteOrdinal(position,current);
                }
                else if((uint)current==originalMarker)representative=(uint)previous;
            }
        }
        uint? writeback=selectedMarker;
        if(succeeded&&selectedMarker is not null)
        {
            writeback=uint.MaxValue;
            for(int position=0;position<count;position++)
            {cancellationToken.ThrowIfCancellationRequested();if((uint)Ordinal(position)==representative){writeback=(uint)position;break;}}
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(actualOwner.Key,actualOwner.PackedCount,nativeCount,owner,
            storage.AsSpan(0,capacity*4).ToArray(),count,capacity,succeeded,writeback);

        int Ordinal(int position)=>BinaryPrimitives.ReadInt32LittleEndian(storage.AsSpan(position*4));
        void WriteOrdinal(int position,int ordinal)=>BinaryPrimitives.WriteInt32LittleEndian(storage.AsSpan(position*4),ordinal);
        float Score(int ordinal)=>BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(owner.AsSpan(ordinal*40+12)));
        bool Same(int a,int b)
        {
            var first=owner.AsSpan(a*40,40);var second=owner.AsSpan(b*40,40);
            return (BinaryPrimitives.ReadUInt16LittleEndian(first[4..])&7)==(BinaryPrimitives.ReadUInt16LittleEndian(second[4..])&7)&&
                BinaryPrimitives.ReadUInt16LittleEndian(first[8..])==BinaryPrimitives.ReadUInt16LittleEndian(second[8..])&&
                (BinaryPrimitives.ReadUInt16LittleEndian(first[6..])&0xff00)==(BinaryPrimitives.ReadUInt16LittleEndian(second[6..])&0xff00);
        }
    }
}
