using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Bounded216BC0 subtype3 protocol caller over the actual ordinary
/// DIRECT/CASE source. Row72 and TOP273.AC/B0 remain prepared inputs; this
/// neither selects a segment nor synthesizes a word-boundary packet.</summary>
public sealed class LegacyOwnedRussianProtocolGlyphProducer
{
    public LegacyOwnedFreshMainWordControllerPolicy Policy {get;}
    public LegacyFreshCaseMainSourcePolicy? CasePolicy {get;}
    public LegacyOwnedRussianMainSourceSymbolCallbacks SourceCallbacks {get;}
    public LegacyOwnedRussianMainInteriorDelimiter? InteriorDelimiter {get;}
    private readonly uint[] headers;
    private readonly LegacyMainProtocolGlyphProductionOperations operations;
    private readonly ushort hyphen;

    private LegacyOwnedRussianProtocolGlyphProducer(LegacyOwnedFreshMainWordControllerPolicy policy,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,uint[] headers,ushort hyphen,
        LegacyOwnedRussianMainInteriorDelimiter? interior=null,LegacyFreshCaseMainSourcePolicy? casePolicy=null)
    {
        Policy=policy;SourceCallbacks=callbacks;this.headers=headers;this.hyphen=hyphen;InteriorDelimiter=interior;CasePolicy=casePolicy;
        operations=new(headers,hyphen,(node,arc,token)=>
        {token.ThrowIfCancellationRequested();var graph=Policy.Owner.SourceGraph;if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)throw new ArgumentException("Actual ordinary graph node/arc required.");},
        (request,cursor,token)=>
        {var value=BinaryPrimitives.ReadInt32LittleEndian(request.AsSpan(40))==1&&InteriorDelimiter is not null?
            InteriorDelimiter.ReadDelimiter(request,cursor,token):SourceCallbacks.ReadDelimiter(request,cursor,token);
            return new(value.RemainingCursor,value.NativeFlag0C,value.CopySymbols());});
    }

    public static LegacyOwnedRussianProtocolGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(callbacks);
        return PrepareCore(resource,policy,callbacks,null,token);
    }

    public static LegacyOwnedRussianProtocolGlyphProducer PrepareCase(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(callbacks);
        return PrepareCore(resource,policy.Controller,callbacks,policy,token);
    }

    private static LegacyOwnedRussianProtocolGlyphProducer PrepareCore(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyFreshCaseMainSourcePolicy? casePolicy,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var owner=policy.Owner;
        if((casePolicy is null
            ? owner.IsCaseAware||!ReferenceEquals(callbacks.Owner.DirectDictionaryOwner,owner.DictionaryOwner)
            : !owner.IsCaseAware||!ReferenceEquals(casePolicy.Controller,policy)||
                !ReferenceEquals(callbacks.Owner.CaseDictionaryOwner,casePolicy.DictionaryOwner))||
            !ReferenceEquals(callbacks.Graph,owner.SourceGraph)||!ReferenceEquals(callbacks.Owner.Model,owner.CommonModel))
            throw new ArgumentException("Same typed source, callbacks, graph and model required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)),owner.SelectedSourceOwner.SourceFingerprint)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh nontracing ordinary main source required.");
        uint[] headers=owner.CopySourceHeaders();
        if(headers.Any(value=>((value>>9)&15)==10))
            throw new NotSupportedException("Class10 language/category rewrite is outside this bounded actual-source caller.");
        if(!owner.CommonModel.AcousticAlphabet.TryGetLabel("-",out ushort hyphen))
            throw new InvalidDataException("Actual ASCII hyphen marker required.");
        token.ThrowIfCancellationRequested();return new(policy,callbacks,headers,hyphen,casePolicy:casePolicy);
    }

    /// <summary>Add the actual same-owner kind1 geometry service. Its packet
    /// remains prepared; this does not manufacture packetAC2 or a delimiter.</summary>
    public static LegacyOwnedRussianProtocolGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyOwnedRussianMainInteriorDelimiter interior,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(interior);
        var ordinary=Prepare(resource,policy,callbacks,token);
        if(!ReferenceEquals(interior.Callbacks,callbacks)||!ReferenceEquals(interior.Geometry.Owner,callbacks.Owner))
            throw new ArgumentException("The actual interior service must retain these exact source callbacks and geometry owner.");
        token.ThrowIfCancellationRequested();return new(policy,callbacks,ordinary.headers,ordinary.hyphen,interior);
    }

    public static LegacyOwnedRussianProtocolGlyphProducer PrepareCase(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy policy,LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyOwnedRussianMainInteriorDelimiter interior,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(interior);var ordinary=PrepareCase(resource,policy,callbacks,token);
        if(!ReferenceEquals(interior.Callbacks,callbacks)||!ReferenceEquals(interior.Geometry.Owner,callbacks.Owner))
            throw new ArgumentException("The actual interior service must retain these exact source callbacks and geometry owner.");
        return new(policy.Controller,callbacks,ordinary.headers,ordinary.hyphen,interior,policy);
    }

    /// <summary>Build the full request64 preserving explicit stack padding,
    /// execute the actual source1A0 callback, then publish unit scores and zero
    /// ordinary publication flags. Unsupported callbacks retain entry flag
    /// clears. No rollback or fabricated successful callback is performed.</summary>
    public LegacyOwnedProtocolGlyphProduction Produce(ReadOnlySpan<byte> preparedRow72,
        ReadOnlySpan<byte> compact40,LegacyOrdinaryGlyphProductionScratch scratch,
        uint packetAcBits,uint packetB0Bits,ReadOnlySpan<byte> requestStackResidue64,
        CancellationToken token=default)
        =>LegacyMainProtocolGlyphProductionKernel.Produce(operations,preparedRow72,compact40,scratch,
            packetAcBits,packetB0Bits,requestStackResidue64,token);

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
