using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalProtocolGlyph.Probe")]
namespace InkRecognition;

//Only owned old/global factories bind this numeric request/caller kernel.
internal sealed record LegacyMainProtocolGlyphProductionOperations(uint[] Headers,ushort Hyphen,
    Action<int,ushort,CancellationToken> ValidateArc,
    Func<byte[],int,CancellationToken,LegacyMainProtocolDelimiterOutput> ReadDelimiter);
internal readonly record struct LegacyMainProtocolDelimiterOutput(int RemainingCursor,byte NativeFlag0C,ushort[] Symbols)
{internal ushort[] CopySymbols()=>Symbols;}

internal static class LegacyMainProtocolGlyphProductionKernel
{
    internal static LegacyOwnedProtocolGlyphProduction Produce(LegacyMainProtocolGlyphProductionOperations operations,ReadOnlySpan<byte> preparedRow72,
        ReadOnlySpan<byte> compact40,LegacyOrdinaryGlyphProductionScratch scratch,
        uint packetAcBits,uint packetB0Bits,ReadOnlySpan<byte> requestStackResidue64,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(scratch);token.ThrowIfCancellationRequested();
        if(preparedRow72.Length!=72||requestStackResidue64.Length!=64||compact40.Length%40!=0)
            throw new ArgumentException("Complete prepared row72, compact40 extent and explicit request64 residue required.");
        ushort symbol=U16(preparedRow72,58);int node=I32(preparedRow72,0);ushort arc=U16(preparedRow72,4);
        if(preparedRow72[60]!=3||unchecked((ushort)(symbol+6))>=3)
            throw new NotSupportedException("Only subtype3 FFFA/FFFB/FFFC protocol caller is restored.");
        operations.ValidateArc(node,arc,token);
        scratch.ComponentFlag=0;scratch.ExternalVariant=0;
        byte[] request=requestStackResidue64.ToArray();int neighbor=I32(preparedRow72,32);
        int previous=-1;ushort previousArc=ushort.MaxValue;uint sourceClass=0;
        if(neighbor!=-1)
        {
            var adjacent=Row(compact40,neighbor);byte source=adjacent[4];
            if(source>=operations.Headers.Length)throw new InvalidDataException("Compact source header escapes its actual owner.");
            sourceClass=(operations.Headers[source]>>9)&15;
            if(U16(adjacent,28)==operations.Hyphen)
            {
                int index=I32(adjacent,16);
                if(index!=-1)
                {
                    var predecessor=Row(compact40,index);
                    if(unchecked((ushort)(U16(predecessor,28)+7))<4)
                    {index=I32(predecessor,16);if(index!=-1)predecessor=Row(compact40,index);}
                    if(index!=-1)
                    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(predecessor[8..]);previous=(unchecked((int)key)<<8)>>8;previousArc=(ushort)(key>>24);}
                }
            }
        }
        W32(request,0,previous);W16(request,4,previousArc);
        W32(request,8,I32(preparedRow72,24));W16(request,12,U16(preparedRow72,28));
        W32(request,16,I32(preparedRow72,12));W16(request,20,U16(preparedRow72,16));
        W16(request,22,neighbor==-1?ushort.MaxValue:U16(Row(compact40,neighbor),28));
        W32(request,24,U16(preparedRow72,56)==ushort.MaxValue?unchecked(node+1):node);
        W16(request,28,arc);W16(request,30,U16(preparedRow72,56));
        W32(request,32,I32(preparedRow72,36));W16(request,36,U16(preparedRow72,40));
        W32(request,40,symbol==0xfffc?0:symbol==0xfffb?1:2);
        BinaryPrimitives.WriteUInt32LittleEndian(request.AsSpan(44),packetAcBits);
        BinaryPrimitives.WriteUInt32LittleEndian(request.AsSpan(48),packetB0Bits);
        BinaryPrimitives.WriteUInt32LittleEndian(request.AsSpan(52),sourceClass);W32(request,56,0);
        //Fresh knowledgeAA=0 (00C860 A4-qword zero), forwarded20C1C0 to2540;
        //no property/external controller path is accepted by this owning policy.
        request[60]=1;
        var result=operations.ReadDelimiter(request,scratch.RemainingCursor,token);
        ApplyDelimiter(scratch,result,token);
        token.ThrowIfCancellationRequested();
        scratch.Score0CBits=0x3f800000;scratch.Score10Bits=0x3f800000;scratch.Cost14=0;scratch.PublicationFlags06=0;
        return new(scratch.Snapshot(),request);
    }

    internal static void ApplyDelimiter(LegacyOrdinaryGlyphProductionScratch scratch,
        LegacyMainProtocolDelimiterOutput result,CancellationToken token)
    {
        foreach(ushort emitted in result.CopySymbols())
        {token.ThrowIfCancellationRequested();scratch.RemainingCursor=unchecked(scratch.RemainingCursor-1);scratch.Write(emitted);}
        //The native pending loop decrements even after exhaustion without
        //writing. Retain its actual negative cursor; never invent extra writes.
        scratch.RemainingCursor=result.RemainingCursor;scratch.ComponentFlag=result.NativeFlag0C;
    }

    private static ReadOnlySpan<byte> Row(ReadOnlySpan<byte> storage,int index)
    {if((uint)index>=(uint)(storage.Length/40))throw new InvalidDataException("Compact neighbor escapes prepared pool.");return storage.Slice(index*40,40);}
    private static ushort U16(ReadOnlySpan<byte> value,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(value[at..]);
    private static int I32(ReadOnlySpan<byte> value,int at)=>BinaryPrimitives.ReadInt32LittleEndian(value[at..]);
    private static void W16(Span<byte> value,int at,ushort data)=>BinaryPrimitives.WriteUInt16LittleEndian(value[at..],data);
    private static void W32(Span<byte> value,int at,int data)=>BinaryPrimitives.WriteInt32LittleEndian(value[at..],data);
}
