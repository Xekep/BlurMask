using System.Buffers.Binary;
namespace InkRecognition;

internal readonly record struct LegacyFullEnglishFirstGlyphEmissionPlan(int Encounter,ulong Reference,ulong Key,
    int Node,ushort Arc,int Group,int Descriptor,ushort Symbol,byte Likelihood,sbyte Prototype,int Category);
internal readonly record struct LegacyFullEnglishFirstGlyphEmissionQuery(int Node,ushort Arc,ushort Symbol,
    int Category,sbyte Prototype,ushort Ordinal,bool Top,bool Primitive,int Cost,float Score);

//Fresh full selected249/23F only. FC=0/channel88=0/capture101=0 are derived
//from the audited no-copy/no-events source path, not caller flag parameters.
internal sealed class LegacyFullEnglishFirstGlyphEmissionPolicy
{
    internal readonly LegacyFullEnglishFirstPlainText Owner;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyFirstAcoustic119WireModel model;
    internal LegacyFullEnglishFirstGlyphEmissionPolicy(LegacyFullEnglishFirstPlainText owner,
        LegacyFullEnglishFirstPlainChild first,CancellationToken token)
    {
        Owner=owner;token.ThrowIfCancellationRequested();var init=first.Factory.Initializer;model=init.ActualCache.Model;
        categories=first.Factory.Categories;var source=model.Source;model.ParentSource.ValidateChild(model.SelectedProcessor,token);
        if(model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||model.SelectedProcessor.Offset!=8827||model.SelectedProcessor.End!=373961||
           source.Offset!=8843||source.End!=373957||source.Version!=0x110||source.SourceKind!=0x135||source.Alphabet.LabelCount!=217||
           owner.Owner.Invocation.Configuration99||owner.Owner.Invocation.ConfigurationA8)
            throw new NotSupportedException("Actual fresh full249/23F no-geometry owner required.");
        //Revalidate the genuine successful selected-source1D8 binding/A0,A2,A4.
        _=new LegacyFullEnglishFirstContextPolicy(first.Terminal.Context.Primary.Owner,first.Terminal.Context.Primary,token);
        byte[] header=source.Alphabet.CopySerializedHeader();
        if(BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(8))!=184||BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(10))!=182||
           BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(12))!=183||categories.CompositeCategories.Length!=42||categories.Prototypes.Length!=24)
            throw new NotSupportedException("Pinned full alphabet/header/prototype owner required.");
    }
    internal void Validate(LegacyFullEnglishFirstPlainChild child,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var f=child.Factory;var init=f.Initializer;
        if(!ReferenceEquals(child.Owner,Owner)||!ReferenceEquals(child.Terminal.Owner,Owner.Owner)||
           !ReferenceEquals(init.ActualCache.Model,model)||!ReferenceEquals(f.Preparation.Source,model.Source)||
           !ReferenceEquals(f.Candidates.SourceRanges.OriginalSource,f.Preparation.OriginalSource)||
           !ReferenceEquals(f.Categories,categories)||!ReferenceEquals(child.Storage.GlyphPool,child.Storage.WordPublisher.Head?.Word.Pool??child.Storage.GlyphPool))
            throw new ArgumentException("Same canonical full B6/source/WORD pool required.");
        var head=child.Storage.WordPublisher.Head;
        foreach(var word in child.NativeWords)
        {
            token.ThrowIfCancellationRequested();if(!ReferenceEquals(word.Published,head)||!ReferenceEquals(word.Published.Word.Pool,child.Storage.GlyphPool))
                throw new InvalidDataException("Actual279 Head/Next reading order required.");head=head!.Next;
        }
        if(head is not null)throw new InvalidDataException("Complete279 chain required.");
    }
    internal LegacyFullEnglishFirstGlyphEmissionPlan? Plan(LegacyFullEnglishFirstPlainChild child,int encounter,ulong reference,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var key=LegacyFirstResultGlyphKey.Decode(reference);if(key.Arc<0)return null;
        var graph=child.Factory.Graph;var arcs=graph.GetNode(key.Node);if(key.Arc>=arcs.Count)throw new InvalidDataException("Same source arc required.");
        var a8=child.Terminal.Context.GetA8(key.Node);if(a8.BeamWidth!=5||a8.MaximumAlternatives!=1)throw new NotSupportedException("Full F8=5/A0=1 required.");
        for(int group=0;group<5;group++)
        {
            var cell=a8.GetCell(group,0);if(cell.TotalCost>=0x4000000||cell.ArcOrdinal!=key.Arc)continue;
            ushort symbol=cell.Symbol;
            var (category,prototype)=ResolvePrototype(symbol);
            return new(encounter,reference,LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference),key.Node,(ushort)key.Arc,
                arcs[key.Arc].GroupId,10+group,symbol,cell.Likelihood,(sbyte)prototype,category);
        }
        return null;
    }
    //The SAME actual reached-symbol admission used by Plan; friend-only scalar
    //controls can exercise header refusals without fabricating an owned A8.
    internal (int Category,int Prototype) ResolvePrototype(ushort symbol)
    {
        //Even an existing descriptor hit does not admit an unclosed reached header.
        if(symbol is 182 or 183 or 184)throw new NotSupportedException("B7 reached header prototype remains unproved; preentry refusal.");
        if(symbol>=categories.StartCompositeMap.Length)throw new NotSupportedException("Own published symbol category required.");
        int category=categories.StartCompositeMap.Span[symbol];if(category<0||category>=categories.CompositeCategories.Length)
            throw new NotSupportedException("No category0 coercion for absent full prototype.");
        int prototype=categories.CompositeCategories.Span[category].GeometryIndex;
        if((uint)prototype>=(uint)categories.Prototypes.Length||prototype>sbyte.MaxValue||symbol is >=0xfff9 and <=0xfffc)
            throw new NotSupportedException("Actual ordinary signed full prototype required.");
        return(category,prototype);
    }
    internal void PreflightSymbols(LegacyFullEnglishFirstPlainChild child,CancellationToken token)
    {
        Validate(child,token);int encounter=0;
        foreach(var word in child.NativeWords)foreach(ulong reference in word.Published.Word.FlatReferences)
        {token.ThrowIfCancellationRequested();_=Plan(child,encounter++,reference,token);}
    }
    internal Cache Bind(LegacyFullEnglishFirstPlainChild child,CancellationToken token)=>new(this,child,token);
    internal sealed class Cache
    {
        private readonly LegacyFullEnglishFirstGlyphEmissionPolicy policy;
        private readonly LegacyFullEnglishFirstPlainChild child;
        private readonly Dictionary<int,Group> groups=[];
        private sealed record Group(LegacyFullEnglishFirstRankedGroup Ranked,Dictionary<ushort,float> Scores);
        internal Cache(LegacyFullEnglishFirstGlyphEmissionPolicy policy,LegacyFullEnglishFirstPlainChild child,CancellationToken token)
        {this.policy=policy;this.child=child;policy.Validate(child,token);}
        private Group Read(LegacyFullEnglishFirstGlyphEmissionPlan plan,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(policy.Plan(child,plan.Encounter,plan.Reference,token)!=plan)throw new ArgumentException("Same canonical A8 plan required.");
            if(!groups.TryGetValue(plan.Group,out var value))
            {
                var init=child.Factory.Initializer;var actual=init.ActualCache.ScoreGroup(plan.Group);
                var ranked=init.ReadGroup(actual);var confidence=init.Owner.Owner.ReadGroup(actual);
                if(!ReferenceEquals(confidence.ActualCache,actual)||!ReferenceEquals(ranked.ActualCache,actual))throw new ArgumentException("Same full cache/confidence required.");
                var candidates=actual.CopyCandidates();var scores=confidence.CopyValues();
                if(candidates.Length!=scores.Length||scores.Length>10)throw new InvalidDataException("Bounded full four-family confidence required.");
                var map=new Dictionary<ushort,float>();for(int i=0;i<scores.Length;i++){token.ThrowIfCancellationRequested();map.TryAdd(candidates[i].Symbol,scores[i]);}
                value=new(ranked,map);groups.Add(plan.Group,value);
            }
            return value;
        }
        internal float Score(LegacyFullEnglishFirstGlyphEmissionPlan plan,CancellationToken token)=>Read(plan,token).Scores.GetValueOrDefault(plan.Symbol);
        internal LegacyFullEnglishFirstGlyphEmissionQuery Query(LegacyFullEnglishFirstGlyphEmissionPlan plan,CancellationToken token)
        {var value=Read(plan,token);int cost=value.Ranked.PreparedClassQuery(plan.Symbol,ushort.MaxValue,plan.Prototype,true,false,token);
         return new(plan.Node,plan.Arc,plan.Symbol,plan.Category,plan.Prototype,ushort.MaxValue,true,false,cost,value.Scores.GetValueOrDefault(plan.Symbol));}
    }
}
