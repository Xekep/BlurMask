using System.Runtime.CompilerServices;
namespace InkRecognition;

internal delegate LegacyPreparedDictionaryQueryPage LegacyTaggedDictionaryReader(ulong sourceTag,
    LegacyPreparedDictionaryQuery query,bool auxiliary,bool extraCosts,CancellationToken cancellationToken);

/// <summary>Already selected dictionary reader with its genuine full-tag consumer.
/// Native source selection/permissions and container options remain separate.</summary>
public sealed class LegacyTaggedDictionaryChild
{
    private readonly LegacyTaggedDictionaryReader reader;
    private readonly Action prepareControllerNode;
    internal Func<uint,bool> IsEligible {get;}
    public LegacySymbolContextFactory Contexts {get;}
    private readonly LegacyOrdinaryRecognitionGraph graph;
    public LegacyOrdinaryCursiveCandidateGraph Graph=>graph.RequireSameSource();
    internal object GraphIdentity=>graph.Identity;
    public bool UsesCase292 {get;}
    public ulong LastSourceTag {get;private set;}
    public int LastMethodOffset {get;private set;}
    private LegacyTaggedDictionaryChild(LegacyTaggedDictionaryReader reader,Func<uint,bool> eligible,
        LegacySymbolContextFactory contexts,LegacyOrdinaryRecognitionGraph graph,bool case292,Action prepareControllerNode)
    {this.reader=reader;IsEligible=eligible;Contexts=contexts;this.graph=graph;UsesCase292=case292;this.prepareControllerNode=prepareControllerNode;}

    public static LegacyTaggedDictionaryChild BindOrdinaryDictionary(LegacyPreparedDictionaryQuerySource source,
        LegacySymbolContextFactory contexts,LegacyOrdinaryCursiveCandidateGraph graph)
    {
        var child=LegacyOrdinaryDictionarySourceOwner.BindDictionary(source,contexts,graph);
        return new((tag,query,aux,extra,token)=>child.Read(query,aux,extra,token),child.IsEligible,contexts,LegacyOrdinaryRecognitionGraph.FromSameSource(graph),false,source.ResetQueries);
    }

    /// <summary>Own both292 paths over the SAME backend/context/ordinary graph.
    /// Selected resource lexicalAL supplies the case predicate; acousticAL
    /// supplies prepared form tables. No category18 or Unicode-script guessing.</summary>
    public static LegacyTaggedDictionaryChild BindCase292(LegacyDictionaryResourcePrefix resource,
        uint containerCaseMode90,LegacyWordLanguageModel language,LegacyInkAlphabet sourceAlphabet,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,
        LegacyNativeCasePreparation preparation,LegacySymbolContextFactory contexts,
        LegacyPreparedDictionaryQuerySource dictionary,LegacyOrdinaryCursiveCandidateGraph graph,bool extraCostSideTable)
        =>BindOwnedCase292(resource,containerCaseMode90,language,sourceAlphabet,caseData,publication,preparation,
            contexts,dictionary,LegacyOrdinaryRecognitionGraph.FromSameSource(graph),extraCostSideTable);

    internal static LegacyTaggedDictionaryChild BindOwnedCase292(LegacyDictionaryResourcePrefix resource,
        uint containerCaseMode90,LegacyWordLanguageModel language,LegacyInkAlphabet sourceAlphabet,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,
        LegacyNativeCasePreparation preparation,LegacySymbolContextFactory contexts,
        LegacyPreparedDictionaryQuerySource dictionary,LegacyOrdinaryRecognitionGraph graph,bool extraCostSideTable)
    {
        var normal=LegacyNativeCaseDictionaryFactory.BindOwned292(resource,containerCaseMode90,language,sourceAlphabet,
            caseData,publication,preparation,contexts,dictionary,graph,extraCostSideTable);
        var auxiliary=LegacyNativeCaseAuxiliaryQuerySource.BindOwnedOrdinaryDictionary(dictionary,contexts,graph);
        var eligibility=LegacyOrdinaryDictionarySourceOwner.BindOwnedDictionary(dictionary,contexts,graph);
        return BindPreparedCase292(dictionary,normal,auxiliary,eligibility,contexts,graph,extraCostSideTable);
    }

    //Only an initialization-issued binding can enter this English adapter.
    //No public callback, source flags or arbitrary language owner is accepted.
    internal static LegacyTaggedDictionaryChild BindEnglishInitialization(LegacyEnglishCaseInitializationBindings binding)
    {
        ArgumentNullException.ThrowIfNull(binding);binding.RequireCurrent();
        return BindPreparedCase292(binding.Dictionary,binding.Normal,binding.Auxiliary,binding.Eligibility,
            binding.Contexts,binding.Graph,false);
    }

    private static LegacyTaggedDictionaryChild BindPreparedCase292(LegacyPreparedDictionaryQuerySource dictionary,LegacyNativeCaseNormalQuerySource normal,
        LegacyNativeCaseAuxiliaryQuerySource auxiliary,LegacyAggregateDictionaryChild eligibility,
        LegacySymbolContextFactory contexts,LegacyOrdinaryRecognitionGraph graph,bool extraCostSideTable)
    {
        //The original request owns the native wrapper-cache pointer. Ephemeron
        //keys preserve that identity without retaining every dead controller
        //query forever; value->Request cycles do not keep dead keys alive.
        var queries=new ConditionalWeakTable<LegacyPreparedDictionaryQuery,LegacyNativeCaseNormalQuery>();
        LegacyPreparedDictionaryQueryPage Read(ulong tag,LegacyPreparedDictionaryQuery query,bool aux,bool extra,CancellationToken token)
        {
            if(extra!=extraCostSideTable)
                throw new ArgumentException("Caller side-cost extent must match the prepared case source flag.");
            if(aux)return auxiliary.Read(tag,query,extra,token); //V48: copy only case bits.
            var owned=queries.GetValue(query,static request=>new(){Request=request});
            owned.SourceTag=tag;
            var page=normal.Read(owned,token); //V40: stateful forms/cost/context expansion.
            return new(page.ReturnedCount,page.Records,page.SlotIndex,page.BackendOffset,page.AcceptedCount,page.ExtraCosts);
        }
        void PrepareNode(){dictionary.ResetQueries();normal.ResetForControllerNode();}
        return new(Read,eligibility.IsEligible,contexts,graph,true,PrepareNode);
    }

    internal void PrepareControllerNode()=>prepareControllerNode();

    internal LegacyPreparedDictionaryQueryPage Read(ulong tag,LegacyPreparedDictionaryQuery query,
        bool auxiliary,bool extraCosts,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if((uint)query.Node!=(uint)tag)throw new ArgumentException("Full tag and child node differ.");
        LastSourceTag=tag;LastMethodOffset=auxiliary?0x48:0x40;
        return reader(tag,query,auxiliary,extraCosts,token);
    }
}

/// <summary>Owning full-source-tag frame around the existing236910 aggregate.
/// It retains native cursor/bases/stamping and forwards the SAME64-bit tag to
/// each selected child. Only Skip is temporarily made relative by236910.</summary>
public sealed class LegacyTaggedAggregateDictionaryQuerySource
{
    private readonly LegacyTaggedDictionaryChild[] children;
    private readonly LegacyPreparedAggregateDictionaryQuerySource aggregate;
    private ulong activeSourceTag;
    private int active;
    private bool valid=true;
    public int CurrentSelectedOrdinal=>aggregate.CurrentSelectedOrdinal;
    public int[] CopyAcceptedCountBases()=>aggregate.CopyAcceptedCountBases();

    //236DC0 forwards V80 in selected order. It does not clear aggregate bases.
    internal void PrepareControllerNode(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Tagged aggregate calls must be serialized.");
        try
        {
            if(!valid)throw new InvalidOperationException("Failed tagged aggregate requires a fresh session.");
            foreach(var child in children)child.PrepareControllerNode();
        }
        catch{valid=false;throw;}
        finally{Volatile.Write(ref active,0);}
    }
    public LegacyTaggedAggregateDictionaryQuerySource(IReadOnlyList<LegacyTaggedDictionaryChild> selectedChildren)
    {
        ArgumentNullException.ThrowIfNull(selectedChildren);
        if(selectedChildren.Count>255)throw new ArgumentException("Native selected source IDs have255 slots.");
        children=selectedChildren.ToArray();
        if(children.Any(c=>c is null))throw new ArgumentException("Actual selected child bindings required.");
        if(children.Length>0 && children.Any(c=>!ReferenceEquals(c.Contexts,children[0].Contexts)||!ReferenceEquals(c.GraphIdentity,children[0].GraphIdentity)))
            throw new ArgumentException("Selected children require the same prepared context and ordinary graph owner.");
        aggregate=new(children.Select(child=>new LegacyAggregateDictionaryChild(
            (query,aux,extra,token)=>
            {
                if(Volatile.Read(ref active)!=1)throw new InvalidOperationException("Child requires its active owning aggregate tag frame.");
                return child.Read(activeSourceTag,query,aux,extra,token);
            },child.IsEligible)).ToArray());
    }
    public LegacyAggregateDictionaryQueryPage Read(ulong sourceTag,LegacyPreparedDictionaryQuery query,
        bool auxiliary=false,bool clearExtraCosts=false,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(query);cancellationToken.ThrowIfCancellationRequested();
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Tagged aggregate calls must be serialized.");
        try
        {
            if(!valid)throw new InvalidOperationException("Failed tagged aggregate requires a fresh session.");
            activeSourceTag=sourceTag;
            return aggregate.Read(sourceTag,query,auxiliary,clearExtraCosts,cancellationToken);
        }
        catch{valid=false;throw;}
        finally{activeSourceTag=0;Volatile.Write(ref active,0);}
    }
}
