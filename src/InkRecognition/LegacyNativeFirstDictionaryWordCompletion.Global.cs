namespace InkRecognition;

public sealed partial class LegacyNativeFirstDictionaryWordCompletion
{
    internal static LegacyNativeFirstDictionaryWordCompletion ForGlobalSpan(
        LegacyFreshMainGlobalWordControllerPolicy policy,LegacyOwnedMainWordSourceServices.SpanLease span,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(span);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(policy.Services,span.Services))throw new ArgumentException("Actual global controller/source/span lease required.");
        policy.Services.ValidateSpan(span,token);
        if(policy.Configuration.Mode22f6!=0||policy.Configuration.Trace2368!=0||policy.SecondaryScaleEc>0||policy.HasAssociatedSource)
            throw new NotSupportedException("Fresh ordinary nontracing global case source required.");
        return new(policy,span);
    }
    private LegacyNativeFirstDictionaryWordCompletion(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyOwnedMainWordSourceServices.SpanLease span)
    {
        var source=policy.Services;alphabet=source.Model.AcousticAlphabet;categories=source.Model.CreateCursiveCategoryMapper(1);
        categoryPolicy=new(categories);language=source.LanguageRecords;sourceId=1;sourceHeader=source.Descriptor.Flags;
        if(!language.IsForModel(source.Model)||(sourceHeader&0x1e00)!=0xc00||(sourceHeader&0x1c)<=7)
            throw new ArgumentException("Actual order2/C00 global case language owner required.");
        replacement=alphabet.TryGetLabel("\ufffd",out ushort label)?label:ushort.MaxValue;
        caseSpan=new GlobalSpanBinding(policy,span);
    }
    private sealed class GlobalSpanBinding(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyOwnedMainWordSourceServices.SpanLease span):ICaseSpanBinding
    {
        private LegacyOwnedMainWordSourceServices Source=>policy.Services;
        public void Validate(int endingNode,bool knowledgeF5,bool suppressPrevious2609,CancellationToken token)
        {
            Source.ValidateSpan(span,token);
            if(endingNode!=span.EndingNode||knowledgeF5!=policy.KnowledgeF5||
                suppressPrevious2609!=(policy.Configuration.SuppressPreviousContext2609!=0))
                throw new ArgumentException("Actual global node/knowledge/history flags required.");
        }
        public ReadOnlyMemory<byte> ReadTerminal(ulong tag,ushort symbol,CancellationToken token)
            =>Source.ReadWordTerminal(span,tag,symbol,0,token);
        public ulong ResolveRestart(ulong tag,CancellationToken token)=>Source.ResolveWordRestart(span,tag,token);
    }
}
