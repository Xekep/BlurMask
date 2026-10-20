using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Global ordinary subtype0 of216BC0 with real owned confidence,
/// geometry and replacement callbacks. Input72 is prepared; this does not pick
/// a winning row, implement protocol delimiters or publish complete text.</summary>
public sealed class LegacyFreshMainGlobalOrdinaryGlyphProducer
{
    private readonly LegacyMainOrdinaryGlyphProductionOperations operations;
    private readonly ushort apostrophe,quote,replacement;
    public LegacyFreshMainGlobalGlyphScoring Scoring{get;}
    public LegacyFreshMainGlobalSourceSymbolCallbacks Callbacks=>Scoring.Callbacks;
    public LegacyFreshMainGlobalSourceGeometry Geometry=>Callbacks.Geometry;
    private LegacyFreshMainGlobalOrdinaryGlyphProducer(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalGlyphScoring scoring,CancellationToken token)
    {
        Scoring=scoring;var owner=Geometry.Policy.Owner;var alphabet=owner.Model.AcousticAlphabet;
        if(!ReferenceEquals(owner.Symbols.Symbols.Preparation.Alphabet,alphabet)||
            LegacyWideSymbolPairs.CreateUnrestricted(alphabet,token).Length!=0||
            Geometry.Policy.Configuration.Trace2368!=0||Geometry.Policy.Configuration.Mode22f6!=0||
            Geometry.Policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh nontracing/no-wide/non-Arabic preparation required.");
        //The successful owned CasePreparation scanned every actual AL scalar
        //and refused script5. NEW container99 is zero in this scoped factory;
        //external2458B0 temporary trace invocations are not accepted here.
        if(!alphabet.TryGetLabel("'",out apostrophe)||!alphabet.TryGetLabel("\"",out quote)||
            !alphabet.TryGetLabel("\ufffd",out replacement))throw new InvalidDataException("Actual quote/replacement labels required.");
        ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(resource[105..]);
        operations=new(alphabet.LabelCount,unit,apostrophe,quote,Geometry.Group,ResolveOwnedSymbol,
            scoring.GetCache,(node,arc,symbol,end)=>scoring.CreateRequest(node,arc,symbol,end));
    }
    public static LegacyFreshMainGlobalOrdinaryGlyphProducer Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalSourceSymbolCallbacks callbacks,CancellationToken token=default)
    {var scoring=LegacyFreshMainGlobalGlyphScoring.Prepare(resource,callbacks,token);return new(resource,scoring,token);}
    public LegacyPreparedGlyphFinalizationInput Produce(ReadOnlySpan<byte> preparedRow72,
        LegacyOrdinaryGlyphProductionScratch scratch,ulong conversionArgument=0,
        ReadOnlySpan<byte> preparedCompact40=default,CancellationToken token=default)
        =>LegacyMainOrdinaryGlyphProductionKernel.Produce(operations,preparedRow72,scratch,conversionArgument,preparedCompact40,token);
    internal LegacyRussianMainReplacementResolution ResolveForRefresh(int node,ushort arc,ushort symbol,CancellationToken token)
    {
        _=Geometry.Group(node,arc,token);
        return ResolveOwnedSymbol(symbol,0,node,arc,token);
    }
    private LegacyRussianMainReplacementResolution ResolveOwnedSymbol(ushort symbol,ulong conversion,int node,ushort arc,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();int count=Geometry.Policy.Owner.Model.AcousticAlphabet.LabelCount;
        if(unchecked((byte)conversion)!=0&&symbol==count+1)return new(apostrophe,apostrophe,0);
        if(unchecked((byte)conversion)!=0&&symbol==count+2)return new(quote,quote,0);
        if(symbol==replacement)return Callbacks.ResolveReplacement(node,arc,token);
        return new(symbol,symbol,0);
    }
}
