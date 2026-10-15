using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeWordSourceSpan(int First, int Last);

/// <summary>Owned WORD276120-byte linked-reference builder13f110/180/1f0/
///260/410/370/4a0/520/7b0. Pool identity remains the TOP273 owner. No string,
/// guessed Unicode or fabricated source endpoints replace its references.</summary>
public sealed class LegacyNativeWordBuilder
{
    private readonly byte[] data;
    public LegacyNativeGlyphRecordPool Pool { get; }
    public ulong HeadReference => BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(8));
    private LegacyNativeWordBuilder(LegacyNativeGlyphRecordPool pool, byte[] data) { Pool=pool; this.data=data; }

    /// <summary>Explicit empty allocated120-byte storage. Existing auxiliary
    /// vectors/pointer ownership require native reset/destruction separately.</summary>
    public static LegacyNativeWordBuilder CreateEmpty(LegacyNativeGlyphRecordPool actualPool,
        ReadOnlySpan<byte> allocatedEmptyStorage120, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualPool); cancellationToken.ThrowIfCancellationRequested();
        if(allocatedEmptyStorage120.Length!=120 || I32(allocatedEmptyStorage120,96)!=0 || I32(allocatedEmptyStorage120,112)!=0 ||
            U64(allocatedEmptyStorage120,32)!=0 || U64(allocatedEmptyStorage120,88)!=0 || U64(allocatedEmptyStorage120,104)!=0)
            throw new NotSupportedException("Owned empty WORD276 storage without native pointer vectors required.");
        byte[] d=allocatedEmptyStorage120.ToArray();
        W64(d,8,ulong.MaxValue);W32(d,16,0x3f800000);W32(d,20,0x3f800000);W32(d,24,0);W32(d,28,0);
        W32(d,40,0);W32(d,48,0xfffe);W16(d,53,0);d[55]=0;W32(d,60,-1);W32(d,84,-1);
        //13ebf0/13ef80 reset auxiliary vector counts; pointers are separate.
        W32(d,96,0);W32(d,112,0);
        return new(actualPool,d);
    }

    /// <summary>276.28/13f410 or276.30/13f370. Native append sets the word's
    /// first head only; the caller1f42d0 writes each previous record's20 link.</summary>
    public LegacyNativeGlyphRecordInsertion AppendGlyph(ulong canonicalOwnerKey,int descriptorId,
        bool forceDuplicate,ReadOnlySpan<byte> allocatedRow40,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result=Pool.AppendDescriptor(canonicalOwnerKey,descriptorId,forceDuplicate,allocatedRow40,cancellationToken);
        if(HeadReference==ulong.MaxValue)W64(data,8,result.Reference);
        return result;
    }

    public void LinkGlyph(ulong previousReference,ulong nextReference,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _=Pool.CopyRecord(nextReference,cancellationToken);
        byte[] previous=Pool.CopyRecord(previousReference,cancellationToken);
        W64(previous,32,nextReference);Pool.ReplaceRecord(previousReference,previous,cancellationToken);
    }

    //218560 publishes the completed reverse-built chain only after all rows
    //succeed. This internal operation cannot inject an unowned native pointer.
    internal void ReplaceHeadReference(ulong reference,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(reference!=ulong.MaxValue)_=Pool.CopyRecord(reference,token);
        W64(data,8,reference);
    }

    /// <summary>276.38/13f4a0 ordinary one-component publication on13f180's
    /// last linked glyph. Zero-count/macro rows can be supplied by their
    /// actual independent record producer through the pool's ReplaceRecord.</summary>
    public void SetLastOrdinarySymbol(ushort symbol,CancellationToken cancellationToken=default)
    {
        var chain=CopyChain(cancellationToken);
        if(chain.Length==0)throw new ArgumentException("WORD276 has no actual linked reference.");
        Pool.SetOrdinarySymbol(chain[^1],symbol,cancellationToken);
    }

    public int GetReferenceCount(CancellationToken cancellationToken=default)=>CopyChain(cancellationToken).Length;
    public int GetSymbolSpan(CancellationToken cancellationToken=default)
    {
        int total=0;
        foreach(ulong reference in CopyChain(cancellationToken))
        {
            byte[] row=Pool.CopyRecord(reference,cancellationToken);
            int kind=U16(row,4)&7;
            total=unchecked(total+((kind is 1 or 3)?(U16(row,8)>>2)&0x1fff:1));
        }
        return total;
    }

    /// <summary>276.48/13f7b0: ordinal0 follows Count-1 links from the head.
    /// Zero-span references are counted and remain accessible.</summary>
    public ulong GetReadingOrderReference(int ordinal,CancellationToken cancellationToken=default)
    {
        var chain=CopyChain(cancellationToken);
        if(ordinal<0 || ordinal>=chain.Length)throw new ArgumentOutOfRangeException(nameof(ordinal));
        return chain[chain.Length-1-ordinal];
    }

    /// <summary>Raw scalar offsets10/14/18/1c. Their confidence/log-mass/cost
    /// interpretation belongs to1f42d0 and later result publication.</summary>
    public void SetRawScalars(float value10,float value14,float value18,float value1c,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();WF(data,16,value10);WF(data,20,value14);WF(data,24,value18);WF(data,28,value1c);
    }
    public byte[] CopyStorage(CancellationToken cancellationToken=default)
    {cancellationToken.ThrowIfCancellationRequested();return data.ToArray();}

    /// <summary>276.40/13f520 source bounds. Supplied callbacks are actual
    /// frontend168/170 over native graph node/arc; no geometry fallback.</summary>
    public LegacyNativeWordSourceSpan GetSourceSpan(Func<int,ushort,int> actualFirstBoundary,
        Func<int,ushort,int> actualLastBoundary,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualFirstBoundary);ArgumentNullException.ThrowIfNull(actualLastBoundary);
        var chain=CopyChain(cancellationToken);
        if(chain.Length==0)throw new NotSupportedException("Empty source-boundary dispatch requires its actual frontend producer.");
        ulong firstHead=chain[0],lastNonnegative=firstHead;
        foreach(ulong reference in chain)if(Decode(reference).Arc>=0)lastNonnegative=reference;
        var left=Decode(lastNonnegative);var right=Decode(firstHead);
        int first;
        if(left.Arc>=0)
        {
            cancellationToken.ThrowIfCancellationRequested();first=actualFirstBoundary(left.Node,(ushort)left.Arc);
            cancellationToken.ThrowIfCancellationRequested();
        }
        else first=unchecked(left.Node+left.Arc);
        int last;
        if(right.Arc>=0)
        {
            cancellationToken.ThrowIfCancellationRequested();last=actualLastBoundary(right.Node,(ushort)right.Arc);
            cancellationToken.ThrowIfCancellationRequested();
        }
        else last=right.Node;
        if(left.Arc<0&&LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(lastNonnegative)==LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(firstHead))
            (first,last)=(last,first);
        return new(first,last);
    }

    internal ulong[] CopyChain(CancellationToken token)=>CopyChain(Pool,HeadReference,token);
    internal static ulong[] CopyChain(LegacyNativeGlyphRecordPool actualPool,ulong head,CancellationToken token)
    {
        var references=new List<ulong>();var seen=new HashSet<ulong>();
        for(ulong reference=head;reference!=ulong.MaxValue;)
        {
            token.ThrowIfCancellationRequested();
            if(references.Count==4096||!seen.Add(reference))throw new ArgumentException("Word reference chain is cyclic or exceeds the portable4096-reference budget.");
            references.Add(reference);reference=U64(actualPool.CopyRecord(reference,token),32);
        }
        return references.ToArray();
    }
    private static LegacyFirstResultGlyphKey Decode(ulong reference)=>LegacyFirstResultGlyphKey.Decode(reference);
    internal static ushort U16(ReadOnlySpan<byte>d,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(d[o..]);
    internal static int I32(ReadOnlySpan<byte>d,int o)=>BinaryPrimitives.ReadInt32LittleEndian(d[o..]);
    internal static ulong U64(ReadOnlySpan<byte>d,int o)=>BinaryPrimitives.ReadUInt64LittleEndian(d[o..]);
    internal static void W16(byte[]d,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(d.AsSpan(o),v);
    internal static void W32(byte[]d,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(o),v);
    internal static void W64(byte[]d,int o,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(d.AsSpan(o),v);
    private static void WF(byte[]d,int o,float v)=>W32(d,o,BitConverter.SingleToInt32Bits(v));
}
