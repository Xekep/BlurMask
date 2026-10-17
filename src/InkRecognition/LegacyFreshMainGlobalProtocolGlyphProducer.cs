using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Actual global216BC0 subtype3 FFFA/FFFB/FFFC request/caller binding.
/// Row72, compact neighbors and packetAC/B0 remain explicit input boundaries.
/// Pending metadata may exhaust the scratch without writing outside256labels;
/// unsafe negative-cursor finalization remains an explicit separate refusal.</summary>
public sealed class LegacyFreshMainGlobalProtocolGlyphProducer
{
    private readonly LegacyMainProtocolGlyphProductionOperations operations;
    public LegacyFreshMainGlobalSourceSymbolCallbacks SourceCallbacks{get;}
    public LegacyFreshMainGlobalSourceGeometry Geometry=>SourceCallbacks.Geometry;
    public LegacyFreshMainGlobalWordControllerPolicy Policy=>Geometry.Policy;
    private LegacyFreshMainGlobalProtocolGlyphProducer(LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,uint[] headers,ushort hyphen)
    {
        SourceCallbacks=callbacks;
        operations=new(headers,hyphen,(node,arc,token)=>{_=Geometry.ReadKind(node,arc,token);},
            (request,cursor,token)=>{var value=SourceCallbacks.ReadDelimiter(request,cursor,token);
                return new(value.RemainingCursor,value.NativeFlag0C,value.CopySymbols());});
    }
    public static LegacyFreshMainGlobalProtocolGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(callbacks);token.ThrowIfCancellationRequested();var policy=callbacks.Geometry.Policy;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(
            callbacks.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh nontracing global source required.");
        uint[] headers=policy.Owner.Symbols.CopySourceHeaders();
        if(headers.Any(value=>((value>>9)&15)==10))throw new NotSupportedException("Class10 source category rewrite is separate.");
        if(!policy.Owner.Model.AcousticAlphabet.TryGetLabel("-",out ushort hyphen))throw new InvalidDataException("Actual hyphen label required.");
        return new(callbacks,headers,hyphen);
    }
    public LegacyOwnedProtocolGlyphProduction Produce(ReadOnlySpan<byte> preparedRow72,
        ReadOnlySpan<byte> compact40,LegacyOrdinaryGlyphProductionScratch scratch,
        uint packetAcBits,uint packetB0Bits,ReadOnlySpan<byte> requestStackResidue64,
        CancellationToken token=default)
        =>LegacyMainProtocolGlyphProductionKernel.Produce(operations,preparedRow72,compact40,scratch,
            packetAcBits,packetB0Bits,requestStackResidue64,token);
}
