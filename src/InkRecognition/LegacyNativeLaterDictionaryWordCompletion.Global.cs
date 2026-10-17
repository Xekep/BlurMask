namespace InkRecognition;

public sealed partial class LegacyNativeLaterDictionaryWordCompletion
{
    internal static LegacyNativeLaterDictionaryWordCompletion ForGlobalSpan(
        LegacyFreshMainGlobalWordControllerPolicy policy,LegacyOwnedMainWordSourceServices.SpanLease span,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(span);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(policy.Services,span.Services))throw new ArgumentException("Actual global controller/source/span lease required.");
        policy.Services.ValidateSpan(span,token);
        if(policy.Configuration.Mode22f6!=0||policy.Configuration.Trace2368!=0||policy.SecondaryScaleEc>0||policy.HasAssociatedSource)
            throw new NotSupportedException("Fresh ordinary nontracing global case source required.");
        return new(policy,span);
    }
    private LegacyNativeLaterDictionaryWordCompletion(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyOwnedMainWordSourceServices.SpanLease span)
    {
        var source=policy.Services;alphabet=source.Model.AcousticAlphabet;categories=source.Model.CreateCursiveCategoryMapper(1);
        categoryPolicy=new(categories);language=source.LanguageRecords;sourceId=1;sourceHeader=source.Descriptor.Flags;
        if(!language.IsForModel(source.Model)||(sourceHeader&0x1e00)!=0xc00||(sourceHeader&0x1c)<=7)
            throw new ArgumentException("Actual order2/C00 global case language owner required.");
        if(!alphabet.TryGetLabel("\ufffd",out replacement)||!alphabet.TryGetLabel("-",out dash))
            throw new NotSupportedException("Actual replacement and dash source labels required.");
        caseSpan=new GlobalSpanBinding(policy,span);
    }
    private sealed class GlobalSpanBinding(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyOwnedMainWordSourceServices.SpanLease span):ICaseSpanBinding
    {
        private LegacyOwnedMainWordSourceServices Source=>policy.Services;
        private readonly uint[] headers=policy.Services.CopySourceHeaders();
        public void Validate(int endingNode,LegacySymbolContextFactory contexts,bool knowledgeF5,int subwordSourceIndex22ec,
            bool extendedMarkers22f8,bool suppressPrevious2609,CancellationToken token)
        {
            Source.ValidateSpan(span,token);
            if(endingNode!=span.EndingNode||!ReferenceEquals(contexts,Source.Contexts)||knowledgeF5!=policy.KnowledgeF5||
                subwordSourceIndex22ec!=Source.SourceSummary.LastKind800Source||extendedMarkers22f8||
                suppressPrevious2609!=(policy.Configuration.SuppressPreviousContext2609!=0))
                throw new ArgumentException("Actual global node/context/source/history flags required.");
        }
        public void ValidateSecondaryOrdinal(ushort ordinal)
        {if(ordinal<policy.Configuration.PrimaryArcLimit230a)throw new ArgumentException("Actual secondary row required, not a primary anchor reference.");}
        public void ValidateExtent(int count)
        {if(count!=Source.Graph.NodeCount)throw new ArgumentException("Actual global source graph extent required.");}
        public void ValidateHeader(byte id,uint header)
        {if(id>=headers.Length||header!=headers[id])throw new ArgumentException("Actual global source header required.");}
        public ReadOnlyMemory<byte> ReadTerminal(ulong tag,ushort symbol,CancellationToken token)
            =>Source.ReadWordTerminal(span,tag,symbol,0,token);
        public ulong ResolveRestart(ulong tag,CancellationToken token)=>Source.ResolveWordRestart(span,tag,token);
    }
}
