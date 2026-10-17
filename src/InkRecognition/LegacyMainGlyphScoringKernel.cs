using System.Buffers.Binary;
namespace InkRecognition;

//Shared native1C13A0 request/cache layout; old and global factories supply
//their own genuine source ownership and native193860 geometry scalar.
internal static class LegacyMainGlyphScoringKernel
{
    internal static byte[] CreateRequest(LegacyCursiveCategoryProfile categories,ushort closed,
        int node,ushort arc,ushort symbol,bool useEndCategory)
    {
        byte geometry=255;
        if(symbol!=closed)
        {
            if(symbol>=categories.StartCompositeMap.Length)throw new NotSupportedException("Requested label escapes actual category maps.");
            int composite=categories.EndCompositeMap.Span[symbol];
            if(!useEndCategory||composite==-1)composite=categories.StartCompositeMap.Span[symbol];
            if((uint)composite>=(uint)categories.CompositeCategories.Length)
                throw new NotSupportedException("Native category access lacks a supported composite owner.");
            geometry=checked((byte)categories.CompositeCategories.Span[composite].GeometryIndex);
        }
        byte[] request=new byte[16];BinaryPrimitives.WriteInt32LittleEndian(request,node);
        W16(request,4,arc);W16(request,6,ushort.MaxValue);W16(request,8,symbol);request[10]=geometry;return request;
    }

    internal static LegacyNativeGlyphScoreCache CreateCache(ReadOnlySpan<LegacyMainGlyphConfidenceCandidate> candidates,
        int geometry,int prototypeCount,ushort blank,int fallback,float scale,ReadOnlySpan<ushort> bypass,CancellationToken token)
    {
        byte[] rows=new byte[candidates.Length*24];var vectors=new ReadOnlyMemory<float>[candidates.Length];
        for(int i=0;i<candidates.Length;i++)
        {token.ThrowIfCancellationRequested();W16(rows,i*24+8,candidates[i].Symbol);
            BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*24+4),candidates[i].Cost);
            vectors[i]=new float[]{candidates[i].Confidence};}
        return new(rows,vectors,Enumerable.Repeat(geometry,prototypeCount).ToArray(),blank,fallback,scale,bypass,token);
    }
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
}
