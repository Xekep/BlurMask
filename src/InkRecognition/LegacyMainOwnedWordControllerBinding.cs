namespace InkRecognition;

//Internal policy sum type; old public APIs retain their exact concrete owner.
internal sealed class LegacyMainOwnedWordControllerBinding
{
    private readonly LegacyOwnedFreshMainWordControllerPolicy? old;
    internal LegacyFreshMainGlobalWordControllerPolicy? Global {get;}
    private LegacyOwnedMainWordSourceServices? source;
    internal LegacyOwnedMainWordSourceServices Source=>source??=old is {} p?
        LegacyOwnedMainWordSourceServices.FromSameSource(p.Owner):Global!.Services;
    internal int NodeCount=>old?.Owner.SourceGraph.NodeCount??Global!.Owner.Graph.State.NodeCount;
    internal LegacyNativeWordControllerConfiguration Configuration=>old?.Configuration??Global!.Configuration;
    internal LegacyNativeWordScalarRefreshResult Refresh=>old?.Refresh??Global!.Refresh;
    internal int MaximumParentDistance54=>old?.MaximumParentDistance54??Global!.MaximumParentDistance54;
    internal int FlagPrepassDistance58=>old?.FlagPrepassDistance58??Global!.FlagPrepassDistance58;
    internal bool KnowledgeF5=>old?.KnowledgeF5??Global!.KnowledgeF5;
    internal float SecondaryScaleEc=>old?.SecondaryScaleEc??Global!.SecondaryScaleEc;
    internal ushort? SecondaryCountProperty12d0003=>old?.SecondaryCountProperty12d0003;
    private LegacyMainOwnedWordControllerBinding(LegacyOwnedFreshMainWordControllerPolicy old){this.old=old;}
    private LegacyMainOwnedWordControllerBinding(LegacyFreshMainGlobalWordControllerPolicy global){Global=global;source=global.Services;}
    internal static LegacyMainOwnedWordControllerBinding FromSameSource(LegacyOwnedFreshMainWordControllerPolicy policy)
    {ArgumentNullException.ThrowIfNull(policy);return new(policy);}
    internal static LegacyMainOwnedWordControllerBinding FromGlobal(LegacyFreshMainGlobalWordControllerPolicy policy)
    {ArgumentNullException.ThrowIfNull(policy);return new(policy);}
    internal LegacyOwnedFreshMainWordControllerPolicy RequireSameSource()=>old??throw new InvalidOperationException("Global controller does not own a same-source public policy.");
    internal bool AllowsParent(int node,int anchor,byte state)=>old is {} p?p.AllowsParent(node,anchor,state):Global!.AllowsParent(node,anchor,state);
}
