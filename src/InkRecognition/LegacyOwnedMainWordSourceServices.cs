using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalWordPass.Probe")]
namespace InkRecognition;

//Private old/global sum type for the shared controller consumers. No concrete
//same-source graph or old span is synthesized for a global source.
internal sealed class LegacyOwnedMainWordSourceServices
{
    internal sealed class SpanLease
    {
        internal LegacyOwnedMainWordSourceServices Services {get;}
        internal LegacyOwnedMainWordSpan? SameSource {get;}
        internal LegacyOwnedGlobalMainWordSpan? GlobalSource {get;}
        internal int EndingNode=>SameSource?.EndingNode??GlobalSource!.EndingNode;
        internal byte Span=>SameSource?.Span??GlobalSource!.Span;
        internal int PreviousEndingNode=>EndingNode-Span-1;
        internal bool IsInitial=>PreviousEndingNode==-1;
        internal LegacyNativeWordSymbolQualifier Qualifier=>SameSource?.Qualifier??GlobalSource!.Qualifier;
        internal SpanLease(LegacyOwnedMainWordSourceServices services,LegacyOwnedMainWordSpan span)
        {Services=services;SameSource=span;}
        internal SpanLease(LegacyOwnedMainWordSourceServices services,LegacyOwnedGlobalMainWordSpan span)
        {Services=services;GlobalSource=span;}
    }
    private readonly LegacyOwnedMainWordSpanPreparation? old;
    private readonly LegacyFreshMainGlobalCaseSession? global;
    private readonly LegacyFreshMainGlobalGroupCosts? globalCosts;
    internal LegacyFreshMainGlobalWordTransitions? GlobalTransitions {get;}
    internal LegacyOrdinaryRecognitionGraph Graph {get;}
    internal bool IsGlobal=>global is not null;
    internal bool IsCaseAware=>IsGlobal||old!.IsCaseAware;
    internal LegacyRussianModelBundle Model=>global?.Model??old!.CommonModel;
    internal LegacySymbolContextFactory Contexts=>global?.Contexts??old!.Contexts;
    internal LegacySourceSamplerCategoryOwner Categories=>global?.Symbols.Geometry.CategoryOwner??old!.SelectedSourceOwner;
    internal LegacyPreparedDictionarySourceDescriptor Descriptor=>global?.Symbols.Descriptor??old!.CommonDescriptor;
    internal LegacyPreparedLinguisticSourceSummary SourceSummary=>global?.Symbols.SourceSummary??old!.CommonSourceSummary;
    internal LegacyPreparedFreshMain240AcousticPolicy AcousticPolicy=>global?.Acoustic.Policy??old!.AcousticPolicy;
    internal int OrdinaryAcousticOffset=>global?.Graph.Source.Source.AcousticOffset??old!.OrdinaryAcousticOffset;
    internal LegacyNativeKnownWordLanguageRecord LanguageRecords=>global?.Symbols.LanguageRecords??
        (old!.CaseDictionaryOwner?.LanguageRecords??old.DictionaryOwner.LanguageRecords);
    internal LegacyOwnedMainWordSpanPreparation RequireSameSource()=>old??throw new InvalidOperationException("Global controller owns a global source, not an old same-source span preparation.");
    internal LegacyFreshMainGlobalCaseSession RequireGlobalSource()=>global??throw new InvalidOperationException("Old controller does not own a global source.");
    private LegacyOwnedMainWordSourceServices(LegacyOwnedMainWordSpanPreparation owner)
    {old=owner;Graph=LegacyOrdinaryRecognitionGraph.FromSameSource(owner.SourceGraph);}
    private LegacyOwnedMainWordSourceServices(LegacyFreshMainGlobalCaseSession owner,
        LegacyFreshMainGlobalGroupCosts costs,LegacyFreshMainGlobalWordTransitions transitions)
    {global=owner;globalCosts=costs;GlobalTransitions=transitions;Graph=LegacyOrdinaryRecognitionGraph.FromGlobal(owner.Graph);}
    internal static LegacyOwnedMainWordSourceServices FromSameSource(LegacyOwnedMainWordSpanPreparation owner)
    {ArgumentNullException.ThrowIfNull(owner);return new(owner);}
    internal static LegacyOwnedMainWordSourceServices FromGlobal(LegacyFreshMainGlobalCaseSession owner,
        LegacyFreshMainGlobalGroupCosts costs,LegacyFreshMainGlobalWordTransitions transitions)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(costs);ArgumentNullException.ThrowIfNull(transitions);
        if(!ReferenceEquals(costs.Owner,owner)||!ReferenceEquals(transitions.Cache.Owner,owner)||
            !ReferenceEquals(costs.Graph,owner.Graph)||!ReferenceEquals(transitions.Graph,owner.Graph))
            throw new ArgumentException("One actual global cache/transition/case-source owner required.");
        return new(owner,costs,transitions);
    }
    internal uint[] CopySourceHeaders()=>global?.Symbols.CopySourceHeaders()??old!.CopySourceHeaders();
    internal int GetSpanExtent(int node)=>global?.GetSpanExtent(node)??old!.GetSpanExtent(node);
    internal void AdvanceNodeEpoch(int completedNode,CancellationToken token)
    {if(global is {} g)g.AdvanceNodeEpoch(completedNode,token);else old!.AdvanceNodeEpoch(completedNode,token);}
    internal SpanLease Prepare(int node,byte span,CancellationToken token)
        =>global is {} g?new(this,g.PrepareSpan(node,span,token)):new(this,old!.Prepare(node,span,token));
    internal SpanLease Wrap(LegacyOwnedMainWordSpan span,CancellationToken token=default)
    {
        _=RequireSameSource();return new(this,span);
    }
    internal void ValidateSpan(SpanLease span,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(span);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(span.Services,this))throw new ArgumentException("Actual source service/span lease identity required.");
        if(global is {} g)g.ValidateOwnedSpan(span.GlobalSource??throw new ArgumentException("Global lease required."),token);
        else old!.ValidateSpan(span.SameSource??throw new ArgumentNullException(nameof(span)),token);
    }
    internal int ReadGlobalGeometryCost(int group,CancellationToken token)
        =>(globalCosts??throw new InvalidOperationException("Actual global geometry owner required.")).ReadGeometryCost(group,token);
    internal LegacyNativeWordAcousticGroupCosts GetCosts(SpanLease span,int node,ushort arc,CancellationToken token)
    {
        ValidateSpan(span,token);
        return global is not null?globalCosts!.ReadCosts(span.GlobalSource!,node,arc,token):old!.GetCosts(span.SameSource!,node,arc,token);
    }
    internal LegacyAggregateDictionaryQueryPage Read(SpanLease span,ulong tag,LegacyPreparedDictionaryQuery query,bool auxiliary,CancellationToken token)
    {
        ValidateSpan(span,token);
        return global is {} g?g.Read(span.GlobalSource!,tag,query,auxiliary,token):old!.Read(span.SameSource!,tag,query,auxiliary,false,token);
    }
    internal ReadOnlyMemory<byte> ReadWordTerminal(SpanLease span,ulong tag,ushort symbol,byte boundary,CancellationToken token)
    {
        ValidateSpan(span,token);
        return global is {} g?g.ReadWordTerminal(span.GlobalSource!,tag,symbol,boundary,token):old!.ReadWordTerminal(span.SameSource!,tag,symbol,boundary,token);
    }
    internal ulong ResolveWordRestart(SpanLease span,ulong tag,CancellationToken token)
    {
        ValidateSpan(span,token);
        return global is {} g?g.ResolveWordRestart(span.GlobalSource!,tag,token):old!.ResolveWordRestart(span.SameSource!,tag,token);
    }
    internal bool Qualifies(SpanLease span,ushort symbol,byte length,ulong context,CancellationToken token)
    {
        ValidateSpan(span,token);
        return global is {} g?g.Qualifies(span.GlobalSource!,symbol,length,context,token):span.Qualifier.Qualifies(symbol,length,context,Contexts,token);
    }
}
