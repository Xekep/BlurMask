using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Bounded216BC0 subtype3 protocol caller over the actual ordinary
/// DIRECT source. Row72 and packetAC/B0 remain prepared segment inputs; this
/// neither selects a segment nor synthesizes a word-boundary packet.</summary>
public sealed class LegacyOwnedRussianProtocolGlyphProducer
{
    public LegacyOwnedFreshMainWordControllerPolicy Policy {get;}
    public LegacyOwnedRussianMainSourceSymbolCallbacks SourceCallbacks {get;}
    private readonly uint[] headers;
    private readonly ushort hyphen;

    private LegacyOwnedRussianProtocolGlyphProducer(LegacyOwnedFreshMainWordControllerPolicy policy,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,uint[] headers,ushort hyphen)
    {Policy=policy;SourceCallbacks=callbacks;this.headers=headers;this.hyphen=hyphen;}

    public static LegacyOwnedRussianProtocolGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(callbacks);
        token.ThrowIfCancellationRequested();var owner=policy.Owner;
        if(owner.IsCaseAware||!ReferenceEquals(callbacks.Owner.DirectDictionaryOwner,owner.DictionaryOwner)||
            !ReferenceEquals(callbacks.Graph,owner.SourceGraph)||!ReferenceEquals(callbacks.Owner.Model,owner.CommonModel))
            throw new ArgumentException("Same actual DIRECT source, callbacks, graph and model required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),owner.SelectedSourceOwner.SourceFingerprint)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh nontracing ordinary main source required.");
        uint[] headers=owner.CopySourceHeaders();
        if(headers.Any(value=>((value>>9)&15)==10))
            throw new NotSupportedException("Class10 language/category rewrite is outside this bounded actual-source caller.");
        if(!owner.CommonModel.AcousticAlphabet.TryGetLabel("-",out ushort hyphen))
            throw new InvalidDataException("Actual ASCII hyphen marker required.");
        token.ThrowIfCancellationRequested();return new(policy,callbacks,headers,hyphen);
    }

    /// <summary>Build the full request64 preserving explicit stack padding,
    /// execute the actual source1A0 callback, then publish unit scores and zero
    /// ordinary publication flags. Unsupported callbacks retain entry flag
    /// clears. No rollback or fabricated successful callback is performed.</summary>
    public LegacyOwnedProtocolGlyphProduction Produce(ReadOnlySpan<byte> preparedRow72,
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
        var graph=Policy.Owner.SourceGraph;
        if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)
            throw new ArgumentException("Actual ordinary graph node/arc required.");
        scratch.ComponentFlag=0;scratch.ExternalVariant=0;
        byte[] request=requestStackResidue64.ToArray();int neighbor=I32(preparedRow72,32);
        int previous=-1;ushort previousArc=ushort.MaxValue;uint sourceClass=0;
        if(neighbor!=-1)
        {
            var adjacent=Row(compact40,neighbor);byte source=adjacent[4];
            if(source>=headers.Length)throw new InvalidDataException("Compact source header escapes its actual owner.");
            sourceClass=(headers[source]>>9)&15;
            if(U16(adjacent,28)==hyphen)
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
        var result=SourceCallbacks.ReadDelimiter(request,scratch.RemainingCursor,token);
        foreach(ushort emitted in result.CopySymbols())
        {scratch.RemainingCursor=unchecked(scratch.RemainingCursor-1);scratch.Write(emitted);}
        if(scratch.RemainingCursor!=result.RemainingCursor)
            throw new InvalidDataException("Actual delimiter cursor/output extent differs.");
        scratch.ComponentFlag=result.NativeFlag0C;
        token.ThrowIfCancellationRequested();
        scratch.Score0CBits=0x3f800000;scratch.Score10Bits=0x3f800000;scratch.Cost14=0;scratch.PublicationFlags06=0;
        return new(scratch.Snapshot(),request);
    }

    private static ReadOnlySpan<byte> Row(ReadOnlySpan<byte> storage,int index)
    {if((uint)index>=(uint)(storage.Length/40))throw new InvalidDataException("Compact neighbor escapes prepared pool.");return storage.Slice(index*40,40);}
    private static ushort U16(ReadOnlySpan<byte> value,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(value[at..]);
    private static int I32(ReadOnlySpan<byte> value,int at)=>BinaryPrimitives.ReadInt32LittleEndian(value[at..]);
    private static void W16(Span<byte> value,int at,ushort data)=>BinaryPrimitives.WriteUInt16LittleEndian(value[at..],data);
    private static void W32(Span<byte> value,int at,int data)=>BinaryPrimitives.WriteInt32LittleEndian(value[at..],data);
}

public sealed class LegacyOwnedProtocolGlyphProduction
{
    public LegacyPreparedGlyphFinalizationInput Finalization {get;}
    private readonly byte[] request;
    internal LegacyOwnedProtocolGlyphProduction(LegacyPreparedGlyphFinalizationInput finalization,byte[] request)
    {Finalization=finalization;this.request=request;}
    public byte[] CopyRequest()=>request.ToArray();
}
