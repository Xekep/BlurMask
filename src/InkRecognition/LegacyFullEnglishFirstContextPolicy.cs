namespace InkRecognition;

//Actual full selected249 successful original247/pair1D8 binding only. No caller flags.
internal sealed class LegacyFullEnglishFirstContextPolicy
{
    internal readonly LegacyFullEnglishFirstPrimaryPolicy Primary;
    internal readonly LegacyFullEnglishFirstPrimary Owner;
    internal const int A0=1,A2=1,A4=1;
    internal LegacyFullEnglishFirstContextPolicy(LegacyFullEnglishFirstPrimary owner,LegacyFullEnglishFirstPrimaryChild first,CancellationToken token)
    {
        Owner=owner;if(!ReferenceEquals(first.Owner,owner)||!ReferenceEquals(first.Initializer.Owner,owner.Owner))throw new ArgumentException("Canonical full primary child required.");
        Primary=new(owner.Owner,first.Initializer,token);var model=Primary.Profile.Model;
        model.ParentSource.ValidateChild(model.SelectedProcessor,token);
        if(model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||model.SelectedProcessor.Offset!=8827||model.SelectedProcessor.End!=373961||model.Source.Offset!=8843||model.Source.End!=373957||Primary.Width!=5||Primary.ExcludeConsecutiveBlank)
            throw new NotSupportedException("Actual successful full selected249 binding required, not standalone unbound249.");
        //155EF0 successful nonnull1E0 ->158DE0 getter ->150D00 selected
        //clone/pair+8 ->158DF0 complete pair store ->nonNULL1D8.156630
        //then sets container14/16/18=1 and alias11=0;1EF070 copies toA0/A2/A4/88.
    }
    internal Child Bind(LegacyFullEnglishFirstPrimaryChild child,CancellationToken token,Action<LegacyFullEnglishFirstContextClassCost>? observer=null)=>new(this,child,token,observer);
    internal sealed class Child
    {
        private readonly LegacyFullEnglishFirstContextPolicy policy;
        private readonly LegacyOrdinaryCursiveCandidateGraph graph;
        private readonly Action<LegacyFullEnglishFirstContextClassCost>? observer;
        private readonly Dictionary<int,LegacyFullEnglishFirstRankedGroup> ranked=[];
        private readonly Dictionary<int,LegacyFirstAcousticShortlistCandidate[]> shortlists=[];
        internal Child(LegacyFullEnglishFirstContextPolicy policy,LegacyFullEnglishFirstPrimaryChild child,CancellationToken token,Action<LegacyFullEnglishFirstContextClassCost>? observer)
        {
            this.policy=policy;this.observer=observer;if(!ReferenceEquals(child.Owner,policy.Owner)||!ReferenceEquals(child.Initializer.Owner,policy.Owner.Owner))throw new ArgumentException("Same canonical full primary owner required.");
            var init=child.Initializer;var cache=init.ActualCache;var c=cache.Owner;graph=c.SourceRanges.Graph;
            if(!ReferenceEquals(cache.Model,policy.Primary.Profile.Model)||!ReferenceEquals(c.Sampler.Source,policy.Primary.Profile.Model.Source))throw new ArgumentException("Same actual full source/model required.");
            foreach(int id in c.CopyLiveGroupIds())
            {
                token.ThrowIfCancellationRequested();var actual=cache.ScoreGroup(id);var group=init.ReadGroup(actual);var entries=group.CopyEntries();
                if(entries.Length>150||!ReferenceEquals(group.Owner,child.Owner.Owner))throw new InvalidDataException("Canonical bounded full B2 ranked group required.");
                ranked.Add(id,group);var top=new LegacyFirstAcousticShortlistCandidate[Math.Min(20,entries.Length)];
                for(int i=0;i<top.Length;i++)top[i]=new(entries[i].Symbol,entries[i].NormalizedLikelihood);shortlists.Add(id,top);
            }
        }
        internal ReadOnlyMemory<LegacyFirstAcousticShortlistCandidate> Candidates(int node,ushort arc,CancellationToken token)
        {token.ThrowIfCancellationRequested();return shortlists[Group(node,arc)];}
        private int Group(int node,ushort arc)
        {if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)throw new ArgumentOutOfRangeException(nameof(node));return graph.GetNode(node)[arc].GroupId;}
        internal int Cost(int node,ushort arc,ushort symbol,byte composite,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();var categories=policy.Primary.Categories;if(composite>=categories.CompositeCategories.Length)throw new ArgumentOutOfRangeException(nameof(composite));
            int prototype=categories.CompositeCategories.Span[composite].GeometryIndex;if((uint)prototype>=(uint)categories.Prototypes.Length)throw new InvalidDataException("Actual full geometry prototype required.");
            const ushort ordinal=ushort.MaxValue;const bool top=true,primitive=false;
            int value=ranked[Group(node,arc)].PreparedClassQuery(symbol,ordinal,prototype,top,primitive,token);
            observer?.Invoke(new(node,arc,symbol,composite,prototype,ordinal,top,primitive,value));token.ThrowIfCancellationRequested();return value;
        }
    }
}

//Diagnostic value only: cannot replace a source, argument or returned cost.
internal readonly record struct LegacyFullEnglishFirstContextClassCost(int Node,ushort Arc,ushort Symbol,byte Composite,int Prototype,ushort Ordinal,bool TopTwenty,bool Primitive,int Cost);
