using System.Buffers.Binary;
using System.Text;
namespace InkRecognition;

/// <summary>Actual fresh290/292 initialization only. No query, scorer, terminal
///or completed language-history entry point is exposed.</summary>
public sealed class LegacyEnglishCaseQueryInitialization
{
    private LegacyEnglishCaseInitializationPlan? plan;
    private LegacyEnglishCaseQueryInitialization(){}
    public const long RequiredAdditionalLogicalBytes=2907401;
    public bool IsFaulted=>plan is null||plan.IsFailed||(plan.BoundRoot?.IsFaulted??false);
    public long AdditionalLogicalEnvelopeBytes=>RequiredAdditionalLogicalBytes;
    public static LegacyEnglishCaseQueryInitialization Prepare(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,
        long maximumAdditionalBytes=3L*1024*1024,CancellationToken token=default)
        =>PrepareCore(knowledge,numeric,cases,publication,maximumAdditionalBytes,null,token);
    internal static LegacyEnglishCaseQueryInitialization PrepareCore(LegacyEnglish119MainKnowledge knowledge,
        LegacyEnglishLkFreshNumericBinding numeric,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,
        long maximumAdditionalBytes,Action<LegacyEnglishCaseQueryInitialization,string>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);ArgumentNullException.ThrowIfNull(cases);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();Preflight(maximumAdditionalBytes);
        var result=new LegacyEnglishCaseQueryInitialization();var p=LegacyEnglishCaseInitializationPlan.Issue(result,knowledge,numeric,cases,publication,observer);
        try
        {
            p.Check("init:preflight",token);var root=LegacyEnglishFreshCaseMainSource.PrepareCore(knowledge,numeric,cases,publication,stage=>p.Check("root:"+stage,token),token,p);
            p.Publish(root,token);return result;
        }
        catch{p.FailBeforePublication();throw;}
    }
    internal static long Preflight(long maximum)
    {
        long bytes=checked(1048576L+1048576L+644893+1536*4+2*(217+29)+2*217*16+104*192+2*128+2*(86+89)*96+3*104*12+8*256*4+9*8*256+8*256+65536);
        if(bytes!=RequiredAdditionalLogicalBytes||maximum<bytes||maximum>16L*1024*1024)throw new ArgumentException("Complete initialization-only logical budget required before entry.");return bytes;
    }
    internal void AttachPlan(LegacyEnglishCaseInitializationPlan value)
    {if(Interlocked.CompareExchange(ref plan,value,null) is not null)throw new InvalidOperationException("One private initialization plan required.");}
    public LegacyEnglishCaseInitializationSnapshot CopySnapshot(CancellationToken token=default)=>CopySnapshotCore(null,token);
    internal LegacyEnglishCaseInitializationSnapshot CopySnapshotCore(Action<string>? observer,CancellationToken token)
    {
        var p=plan??throw new InvalidOperationException("Unpublished initializer.");token.ThrowIfCancellationRequested();p.RequirePublished(token);
        var root=p.BoundRoot!;LegacyEnglishFreshCaseMainSource.InitializationLease? lease=null;
        try
        {
            //Preentry busy/cancel/foreign checks never poison another operation.
            lease=root.BeginInitializationAccess(this,token);observer?.Invoke("snapshot:claimed");token.ThrowIfCancellationRequested();
            var snapshot=p.Snapshot.Copy();observer?.Invoke("snapshot:copied");token.ThrowIfCancellationRequested();p.RequirePublished(token);
            root.CompleteInitializationAccess(lease);return snapshot;
        }
        catch{if(lease is not null){root.PoisonInitializationAccess(lease);p.FailOwnedInspection();}throw;}
    }
    internal LegacyEnglishFreshCaseMainSource RootForAudit=>plan?.BoundRoot??throw new InvalidOperationException("No bound private root.");
    internal LegacyEnglishCaseInitializationNumericTables NumericForAudit=>plan?.NumericTables??throw new InvalidOperationException("No numerical tables.");
    internal LegacyEnglishCaseInitializationPlan PlanForAudit=>plan??throw new InvalidOperationException("No plan.");
}

//Only this private issuer can produce the typed source-prefix hook accepted
//by the old factory. A published/failed plan can never re-enter construction.
internal sealed class LegacyEnglishCaseInitializationPlan
{
    private readonly LegacyEnglishCaseQueryInitialization owner;
    private readonly LegacyEnglish119MainKnowledge knowledge;
    private readonly LegacyEnglishLkFreshNumericBinding numeric;
    private readonly LegacyNativeCaseData cases;
    private readonly LegacyNativeTextPublicationData publication;
    private readonly Action<LegacyEnglishCaseQueryInitialization,string>? observer;
    private int phase; //0Created,1Inputs,2Building,3Prefix,4Bound,5Published,6Failed
    private LegacyEnglishCaseInitializationBindings? binding;
    private LegacyTaggedAggregateDictionaryQuerySource? aggregate;
    private LegacyNativeDelimiterQuerySource? normal;
    private LegacyNativeAuxiliaryDelimiterQuerySource? auxiliary;
    private LegacyEnglishCaseInitializationSnapshot? snapshot;
    private LegacyEnglishCaseInitializationNumericTables? tables;
    internal int ContextCapacity=>104;
    internal bool IsFailed=>Volatile.Read(ref phase)==6;
    internal int PhaseForAudit=>Volatile.Read(ref phase);
    internal LegacyEnglishCaseInitializationBindings BindingForAudit=>binding??throw new InvalidOperationException("No binding.");
    internal LegacyEnglishFreshCaseMainSource? BoundRoot{get;private set;}
    internal LegacyEnglishCaseInitializationSnapshot Snapshot=>snapshot??throw new InvalidOperationException("No completed prefix snapshot.");
    internal LegacyEnglishCaseInitializationNumericTables NumericTables=>tables??throw new InvalidOperationException("No compiled numerical tables.");
    private LegacyEnglishCaseInitializationPlan(LegacyEnglishCaseQueryInitialization o,LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,
        LegacyNativeCaseData c,LegacyNativeTextPublicationData p,Action<LegacyEnglishCaseQueryInitialization,string>? callback)
    {owner=o;knowledge=k;numeric=n;cases=c;publication=p;observer=callback;}
    internal static LegacyEnglishCaseInitializationPlan Issue(LegacyEnglishCaseQueryInitialization o,LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,
        LegacyNativeCaseData c,LegacyNativeTextPublicationData p,Action<LegacyEnglishCaseQueryInitialization,string>? callback)
    {var result=new LegacyEnglishCaseInitializationPlan(o,k,n,c,p,callback);o.AttachPlan(result);return result;}
    internal void Check(string stage,CancellationToken token){observer?.Invoke(owner,stage);token.ThrowIfCancellationRequested();}
    private void Move(int expected,int next)
    {if(Interlocked.CompareExchange(ref phase,next,expected)!=expected)throw new InvalidOperationException("One-use ordered initialization phase required.");}
    internal void RequireInputs(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,LegacyNativeCaseData c,LegacyNativeTextPublicationData p,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(k,knowledge)||!ReferenceEquals(n,numeric)||!ReferenceEquals(c,cases)||!ReferenceEquals(p,publication))throw new ArgumentException("Same private initialization inputs required.");
        ValidateParent(token);LegacyEnglishCaseInitializationNumericTables.Preflight(numeric);Move(0,1);Check("init:inputs",token);
    }
    private void ValidateParent(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var source=knowledge.Source;
        if(!ReferenceEquals(source,numeric.DataOwner.Source)||!ReferenceEquals(source,knowledge.MainWire.Source)||!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted||
           !knowledge.Owner.Owner.IsComplete||knowledge.Owner.Owner.IsFaulted)throw new InvalidOperationException("Same completed actual MAIN/source numeric owners required.");
        if(!source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase)||source.SourceKind!=0x135||source.CoordinateMode!=2||source.Alphabet.LabelCount!=217||numeric.DataOwner.Prefix.Field68!=1)
            throw new NotSupportedException("Exact direct fresh English source required.");
    }
    internal void PrepareSourcePrefix(LegacyDictionaryInitialSymbolMaps maps,LegacySymbolContextFactory contexts,CancellationToken token)
    {
        ValidateParent(token);if(!ReferenceEquals(maps.Contexts,contexts)||!ReferenceEquals(maps.AcousticAlphabet,knowledge.Source.Alphabet)||!ReferenceEquals(maps.LexicalAlphabet,numeric.DataOwner.LexicalAlphabet)||contexts.NodeCount!=86)
            throw new ArgumentException("Exact initial source maps/context factory required.");
        Move(1,2);var initial=contexts.CopyNodes().ToArray();var initialPages=contexts.CopyPageUsedCounts();ulong initialRoot=contexts.RootTreeIdentity;int initialCapacity=contexts.PageDescriptorCapacity,initialCursor=contexts.CurrentPageIndex;Check("init:initial-snapshot",token);
        var graph=knowledge.Owner.RetainCompletedD0Graph(token);if(graph.NodeCount>4096||graph.Groups.Count>4096||graph.MaximumAtoms!=8)throw new NotSupportedException("Bounded actual ordinary MAIN graph required.");
        long count=0,rangesCount=0;for(int g=0;g<graph.Groups.Count;g++){token.ThrowIfCancellationRequested();rangesCount=checked(rangesCount+graph.Groups[g].Ranges.Length);if(rangesCount>131072)throw new InvalidDataException("Whole initial source-range bound exceeded.");}
        for(int node=0;node<graph.NodeCount;node++)foreach(var arc in graph.GetNode(node)){if((uint)arc.GroupId>=(uint)graph.Groups.Count||arc.Span>=8||++count>4096)throw new InvalidDataException("Whole source arc bound exceeded.");}
        var ordinaryGraph=LegacyOrdinaryRecognitionGraph.FromSameSource(graph);
        bool dutch=ReadDutch();if(dutch)throw new NotSupportedException("Actual English source must not select Dutch ligature mode.");
        var prepared=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(knowledge.Source.Alphabet,cases,publication,token),publication,token);
        var version=numeric.LanguageOwner.CopyFieldPayload("150.casePathVersion");if(version.Length!=2||BinaryPrimitives.ReadUInt16LittleEndian(version)!=0x200)throw new NotSupportedException("Actual0200 case policy required.");
        var policy=LegacyNativeCasePathPolicy.Create(0x200);var expansion=new LegacyNativeCaseRecordExpansion(prepared,policy,contexts);Check("init:case-preparation",token);
        var dictionary=maps.CreateQuerySource(numeric.DataOwner.Dictionary,0xc0e);Check("init:dictionary",token);
        var n=LegacyNativeCaseNormalQuerySource.BindOwnedOrdinaryDictionary(expansion,dictionary,ordinaryGraph,dutch,false);
        var a=LegacyNativeCaseAuxiliaryQuerySource.BindOwnedOrdinaryDictionary(dictionary,contexts,ordinaryGraph);
        var eligibility=LegacyOrdinaryDictionarySourceOwner.BindOwnedDictionary(dictionary,contexts,ordinaryGraph);
        binding=LegacyEnglishCaseInitializationBindings.Issue(this,dictionary,n,a,eligibility,contexts,ordinaryGraph);
        var child=LegacyTaggedDictionaryChild.BindEnglishInitialization(binding);aggregate=new([child]);Check("init:case-binding",token);
        //Genuine same-source delegates. They are not invoked during initialization.
        normal=new(knowledge.Source.Alphabet,contexts,(tag,q,ct)=>aggregate.Read(tag,q,false,false,ct),knowledge.Source.CoordinateMode);
        auxiliary=new(knowledge.Source.Alphabet,contexts,(tag,q,ct)=>aggregate.Read(tag,q,true,false,ct),graph.NodeCount);
        var post=contexts.CopyNodes().ToArray();if(post.Length!=89)throw new InvalidDataException("Exact native86->89 control prefix required.");
        var bySymbol=post.Where(x=>x.ParentLink==0).ToDictionary(x=>x.Symbol);var seen=initial.Where(x=>x.ParentLink==0).Select(x=>x.Symbol).ToHashSet();
        ushort[] keys=[0xfffb,0xfff9,0xfffb,0xfffc,0xfffa];var controls=new LegacyEnglishCaseInitializationControl[5];
        for(int i=0;i<keys.Length;i++){bool created=seen.Add(keys[i]);controls[i]=new(keys[i],bySymbol[keys[i]].Identity,created);}
        if(controls[0].Identity!=controls[2].Identity||!controls.Select(c=>c.Created).SequenceEqual(new[]{false,true,false,true,true}))throw new InvalidDataException("Actual default/FFFB alias and creation order required.");
        var prior=initial.Single(x=>x.Symbol==0xfffb);if(bySymbol[0xfffb].Identity!=prior.Identity||bySymbol[0xfffb].ParentLink!=prior.ParentLink)throw new InvalidDataException("ExistingFFFB parent must survive.");
        var first=new byte[246];var second=new byte[246];for(int i=0;i<246;i++){token.ThrowIfCancellationRequested();ushort key=unchecked((ushort)(i-29));first[i]=prepared.ReadClassification(key);second[i]=prepared.ReadClassification(key,true);}
        snapshot=new(contexts.ContainerIdentity,initialRoot,contexts.RootTreeIdentity,initial,post,initialPages,contexts.CopyPageUsedCounts(),maps.CopyPrimary(),maps.CopyAuxiliary(),maps.CopyInitialContexts(),first,second,controls,initialCapacity,initialCursor,contexts.PageDescriptorCapacity,contexts.CurrentPageIndex);
        Check("init:controls",token);tables=LegacyEnglishCaseInitializationNumericTables.Prepare(numeric,token);Check("init:numeric",token);
        ValidateParent(token);Move(2,3);Check("init:prefix-complete",token);
    }
    private bool ReadDutch()
    {
        var source=knowledge.Source;var raw=source.Fields.Single(f=>f.Owner=="133"&&f.Destination=="14.metadata").CopyBytes();var marker=source.Fields.Single(f=>f.Owner=="133"&&f.Destination=="identityMarker").CopyBytes();
        if(raw.Length!=56||marker.Length!=4||BinaryPrimitives.ReadUInt32LittleEndian(marker)!=uint.MaxValue)throw new NotSupportedException("Actual source133 encrypted name branch required.");
        ReadOnlySpan<byte> key=[0x14,0x12,0x19,0x77];for(int i=0;i<56;i++)raw[i]^=key[i&3];int end=Array.IndexOf(raw,(byte)0);
        if(end<0||raw.AsSpan(end).ContainsAnyExcept((byte)0)||!raw.AsSpan(0,end).SequenceEqual("en_US-ak-cur.lite"u8))throw new NotSupportedException("Exact decoded actual English source name required.");
        return raw.AsSpan(0,end).StartsWith("nl_"u8);
    }
    internal void RequireBinding(LegacyEnglishCaseInitializationBindings value)
    {if(!ReferenceEquals(binding,value)||Volatile.Read(ref phase)!=2)throw new InvalidOperationException("Exact private prefix-building292 binding required.");ValidateParent(default);}
    internal void RequireRootBinding(LegacyEnglishCaseInitializationBindings value,LegacyEnglishCaseQueryInitialization actualOwner,LegacyEnglishFreshCaseMainSource root)
    {if(!ReferenceEquals(binding,value)||!ReferenceEquals(owner,actualOwner)||Volatile.Read(ref phase)!=3||!ReferenceEquals(root.Knowledge,knowledge)||!ReferenceEquals(root.NumericOwner,numeric))throw new InvalidOperationException("Exact issued prefix/root lineage required.");ValidateParent(default);}
    internal void BindPrivateRoot(LegacyEnglishFreshCaseMainSource root,CancellationToken token)
    {
        ValidateParent(token);if(!ReferenceEquals(root.Knowledge,knowledge)||!ReferenceEquals(root.NumericOwner,numeric)||Volatile.Read(ref phase)!=3)throw new InvalidOperationException("Exact completed prefix/root required.");
        root.ClaimInitialization(owner,binding??throw new InvalidOperationException("No issued binding."),token);BoundRoot=root;Move(3,4);Check("init:root-bound",token);
    }
    internal void Publish(LegacyEnglishFreshCaseMainSource root,CancellationToken token)
    {
        if(!ReferenceEquals(BoundRoot,root))throw new InvalidOperationException("Exact bound root required.");ValidateParent(token);root.RequireCurrent();Check("init:publish",token);Move(4,5);Check("init:published",token);
    }
    internal void RequirePublished(CancellationToken token)
    {token.ThrowIfCancellationRequested();if(Volatile.Read(ref phase)!=5||BoundRoot is null)throw new InvalidOperationException("Completed initialization required.");ValidateParent(token);BoundRoot.RequireCurrent();}
    internal void FailBeforePublication(){Interlocked.Exchange(ref phase,6);BoundRoot?.PoisonInitializationOwner(owner);}
    internal void FailOwnedInspection()=>Interlocked.Exchange(ref phase,6);
}

internal sealed class LegacyEnglishCaseInitializationBindings
{
    private readonly LegacyEnglishCaseInitializationPlan plan;
    private int claimed;
    internal LegacyPreparedDictionaryQuerySource Dictionary{get;}
    internal LegacyNativeCaseNormalQuerySource Normal{get;}
    internal LegacyNativeCaseAuxiliaryQuerySource Auxiliary{get;}
    internal LegacyAggregateDictionaryChild Eligibility{get;}
    internal LegacySymbolContextFactory Contexts{get;}
    internal LegacyOrdinaryRecognitionGraph Graph{get;}
    private LegacyEnglishCaseInitializationBindings(LegacyEnglishCaseInitializationPlan p,LegacyPreparedDictionaryQuerySource d,LegacyNativeCaseNormalQuerySource n,LegacyNativeCaseAuxiliaryQuerySource a,
        LegacyAggregateDictionaryChild e,LegacySymbolContextFactory c,LegacyOrdinaryRecognitionGraph g){plan=p;Dictionary=d;Normal=n;Auxiliary=a;Eligibility=e;Contexts=c;Graph=g;}
    internal static LegacyEnglishCaseInitializationBindings Issue(LegacyEnglishCaseInitializationPlan p,LegacyPreparedDictionaryQuerySource d,LegacyNativeCaseNormalQuerySource n,LegacyNativeCaseAuxiliaryQuerySource a,
        LegacyAggregateDictionaryChild e,LegacySymbolContextFactory c,LegacyOrdinaryRecognitionGraph g)=>new(p,d,n,a,e,c,g);
    internal void RequireRootBinding(LegacyEnglishCaseQueryInitialization owner,LegacyEnglishFreshCaseMainSource root){if(Volatile.Read(ref claimed)!=1)throw new InvalidOperationException("Consumed initialization adapter required.");plan.RequireRootBinding(this,owner,root);}
    internal void RequireCurrent(){plan.RequireBinding(this);if(Interlocked.CompareExchange(ref claimed,1,0)!=0)throw new InvalidOperationException("One issued292 initialization adapter required.");}
}
