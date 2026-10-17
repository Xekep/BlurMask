namespace InkRecognition;

public sealed class LegacyOwnedMainWordSpan
{
    internal LegacyOwnedMainWordSpanPreparation Owner { get; }
    internal long Epoch { get; }
    public int EndingNode { get; }
    public byte Span { get; }
    public int PreviousEndingNode => EndingNode - Span - 1;
    public bool IsInitial => PreviousEndingNode == -1;
    public LegacyNativeWordSymbolQualifier Qualifier { get; }
    public LegacyOrdinaryGraphPermissionPass PermissionPass { get; }

    internal LegacyOwnedMainWordSpan(LegacyOwnedMainWordSpanPreparation owner, long epoch,
        int ending, byte span, LegacyNativeWordSymbolPreparationResult result)
    {
        Owner = owner;
        Epoch = epoch;
        EndingNode = ending;
        Span = span;
        Qualifier = result.Qualifier ?? throw new InvalidDataException("Successful owned span lacks eligibility.");
        PermissionPass = result.PermissionPass;
    }
}

/// <summary>
/// Owned ordinary main frontend98/D0 preparation, including real NN/cache
/// lifecycle, source68 dictionary permission refresh and frontend110 union.
/// DIRECT constructors retain their original dictionary behavior. Additive
/// case constructors use an owning fresh292 session; complete2144F0 scheduling
/// and the outer290 search layer remain separate.
/// </summary>
public sealed class LegacyOwnedMainWordSpanPreparation
{
    private readonly LegacyNativeWordSymbolPreparationSession session;
    private readonly LegacyOwnedMainDictionaryBinding dictionaryBinding;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly LegacyCursiveSeparationTable table;
    private readonly Dictionary<int, LegacyNativeWordAcousticGroupCosts> costs = [];
    private long epoch;
    private bool valid = true;
    private int active;
    private LivePublicationEpoch? activeLivePublicationEpoch;

    /// <summary>Original DIRECT API; a case source is never represented by a
    /// fabricated parallel DIRECT owner.</summary>
    public LegacyOwnedRussianDictionaryPreparation DictionaryOwner=>dictionaryBinding.Direct??
        throw new InvalidOperationException("The case-aware span owns CaseDictionaryOwner; use common source services.");
    public LegacyFreshCaseRussianDictionaryPreparation? CaseDictionaryOwner=>dictionaryBinding.Case;
    public bool IsCaseAware=>CaseDictionaryOwner is not null;
    public LegacyRussianModelBundle CommonModel=>dictionaryBinding.Model;
    public LegacyPreparedDictionarySourceDescriptor CommonDescriptor=>dictionaryBinding.Descriptor;
    public LegacyPreparedLinguisticSourceSummary CommonSourceSummary=>dictionaryBinding.SourceSummary;
    public uint[] CopySourceHeaders()=>dictionaryBinding.CopySourceHeaders();
    private readonly LegacyPreparedFreshMain240AcousticScorer? singleAcoustic;
    private readonly LegacyPreparedFreshMainLine240AcousticScorer? lineAcoustic;
    private readonly SourceBinding source;
    /// <summary>The original single-glyph acoustic API, preserved for its exact
    /// owner. A multi-physical source is never cast into this earlier type.</summary>
    public LegacyPreparedFreshMain240AcousticScorer Acoustic => singleAcoustic ??
        throw new InvalidOperationException("The owned multi-physical line uses MainLineAcoustic.");
    public LegacyFreshSingleGlyphMainSampled Source => Acoustic.Source;
    public LegacyPreparedFreshMainLine240AcousticScorer MainLineAcoustic => lineAcoustic ??
        throw new InvalidOperationException("The owned single-glyph source uses Acoustic.");
    public LegacyFreshMainSingleLineSampled MainLineSource => MainLineAcoustic.Source;
    public LegacyOrdinaryCursiveCandidateGraph SourceGraph => source.Graph;
    public LegacyCursivePreparedInk SourceInk => source.Ink;
    public float SourceHeight => source.Height;
    internal int OrdinaryAcousticOffset => source.Single is {} s ? s.Owner.AcousticOffset : source.Line!.Owner.AcousticOffset;
    public LegacySourceSamplerCategoryOwner SelectedSourceOwner => source.Categories;
    public LegacyPreparedFreshMain240AcousticPolicy AcousticPolicy => singleAcoustic?.Policy ?? lineAcoustic!.Policy;
    public LegacySymbolContextFactory Contexts => dictionaryBinding.Contexts;
    public int SourceRowBudget => AcousticPolicy.MaximumSourceArcs;

    internal int ReadSourceDescriptorLine(int node, int ordinal, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var arcs = SourceGraph.GetNode(node);
        if ((uint)ordinal >= (uint)arcs.Count) throw new ArgumentOutOfRangeException(nameof(ordinal));
        return source.Single is { } single ? single.GetSourceLineIdForArc(node, ordinal)
            : source.Line!.Owner.SourceLineId;
    }

    public LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        LegacyFreshSingleGlyphMainSampled actualSource, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken = default)
        : this(acousticResource, lexicalResource, engineProfile,
            SourceBinding.FromSingle(actualSource), null, table, cancellationToken) { }

    /// <summary>Owned main single-descriptor/multi-physical acoustic and source68
    /// lifecycle. The supplied context must be the same owned sampled source;
    /// no first-source frame, synthetic singleton or neutral context is used.</summary>
    public LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        LegacyFreshMainSingleLineSampled actualSource, LegacyFreshMainSingleLineContext actualContext,
        LegacyCursiveSeparationTable table, CancellationToken cancellationToken = default)
        : this(acousticResource, lexicalResource, engineProfile,
            SourceBinding.FromLine(actualSource), actualContext, table, cancellationToken) { }

    /// <summary>Same frontend/cache/epoch lifecycle over ONE already owned
    /// fresh292 backend. No DIRECT backend is constructed in this branch.</summary>
    public LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        LegacyFreshCaseRussianDictionaryPreparation dictionaryOwner,LegacyFreshSingleGlyphMainSampled actualSource,
        LegacyCursiveSeparationTable table,CancellationToken cancellationToken=default)
        :this(acousticResource,[],[],SourceBinding.FromSingle(actualSource),null,table,cancellationToken,dictionaryOwner??throw new ArgumentNullException(nameof(dictionaryOwner))){}

    public LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        LegacyFreshCaseRussianDictionaryPreparation dictionaryOwner,LegacyFreshMainSingleLineSampled actualSource,
        LegacyFreshMainSingleLineContext actualContext,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
        :this(acousticResource,[],[],SourceBinding.FromLine(actualSource),actualContext,table,cancellationToken,dictionaryOwner??throw new ArgumentNullException(nameof(dictionaryOwner))){}

    private LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        SourceBinding actualSource, LegacyFreshMainSingleLineContext? actualContext,
        LegacyCursiveSeparationTable table, CancellationToken cancellationToken,
        LegacyFreshCaseRussianDictionaryPreparation? caseOwner=null)
    {
        ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        source = actualSource;
        var model = caseOwner?.Model??LegacyRussianModelBundle.Load(acousticResource, lexicalResource, engineProfile);
        if(actualSource.Single is {} single)
        {
            if(caseOwner is null)
            {
                var direct=LegacyOwnedRussianDictionaryPreparation.Prepare(acousticResource,lexicalResource,model,single,cancellationToken);
                dictionaryBinding=new(direct,actualSource.Graph);singleAcoustic=direct.CreateAcousticScorer(acousticResource,cancellationToken);
            }
            else
            {
                dictionaryBinding=new(caseOwner,actualSource.Graph);singleAcoustic=caseOwner.CreateAcousticScorer(acousticResource,cancellationToken);
                if(!ReferenceEquals(singleAcoustic.Source,single))throw new ArgumentException("Case scorer/source identity must match the supplied sampled source.");
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(actualContext);
            if(!ReferenceEquals(actualContext.Source,actualSource.Line))
                throw new ArgumentException("Context must belong to the actual multi-physical sampled source.");
            if(caseOwner is null)
            {
                var direct=LegacyOwnedRussianDictionaryPreparation.Prepare(acousticResource,lexicalResource,model,actualSource.Line!,cancellationToken);
                dictionaryBinding=new(direct,actualSource.Graph);lineAcoustic=direct.CreateAcousticScorer(acousticResource,actualContext,cancellationToken);
            }
            else
            {
                dictionaryBinding=new(caseOwner,actualSource.Graph);lineAcoustic=caseOwner.CreateAcousticScorer(acousticResource,actualContext,cancellationToken);
                if(!ReferenceEquals(lineAcoustic.Source,actualSource.Line))throw new ArgumentException("Case scorer/source identity must match the actual sampled line.");
            }
        }
        this.table = table;
        separation = LegacyCursiveSeparationProfile.ReadV106Node(acousticResource,
            actualSource.Categories.SamplerNodeOffset);
        var map = dictionaryBinding.CreateRawPermissionMap();
        ushort[] reserved = LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(
            model.AcousticAlphabet, acousticResource.Slice(99, 16));
        session = new(actualSource.Graph, map, reserved, Compute);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>1A7580 scans arc byte4 and returns its maximum plus one,
    /// or zero for an empty row. This includes empty intermediate spans.
    /// It is neither node+1, maximum atoms nor the125 source-row budget.</summary>
    public int GetSpanExtent(int node)
    {
        var arcs = SourceGraph.GetNode(node);
        int extent = 0;
        foreach (var arc in arcs) extent = Math.Max(extent, arc.Span + 1);
        return extent;
    }

    /// <summary>Explicit native sourceV80 epoch, not a source68 map refresh.
    /// 2144F0 calls with start−1 before the node loop and after each completed
    /// node. This bounded endpoint does not infer those scheduler events from
    /// D0/Prepare calls. Prior owned spans become invalid.</summary>
    public void AdvanceNodeEpoch(int completedNode,CancellationToken cancellationToken=default)
    {
        Enter();
        try
        {
            if(completedNode< -1||completedNode>=SourceGraph.NodeCount)throw new ArgumentOutOfRangeException(nameof(completedNode));
            dictionaryBinding.AdvanceNodeEpoch(completedNode,cancellationToken);epoch=checked(epoch+1);
        }
        catch{valid=false;throw;}
        finally{Volatile.Write(ref active,0);}
    }

    /// <summary>2144F0 calls D0 with param5=false in ascending span order.
    /// The cache session itself decides whether an existing pool row is reused.
    /// Selected owned195 costs refresh dictionary50 before any subsequent query.</summary>
    public LegacyOwnedMainWordSpan Prepare(int endingNode, byte span,
        CancellationToken cancellationToken = default)
    {
        Enter();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (span >= GetSpanExtent(endingNode) || span > endingNode)
                throw new ArgumentException("Actual frontend98 span extent and owned previous prefix required.");
            var result = session.Prepare(endingNode, span, false, cancellationToken);
            if (!result.PermissionPass.Succeeded || result.PermissionPass.PermissionResult.WorkingMap is null)
                throw new InvalidOperationException("Owned main cache/source preparation failed.");
            var actualMap=dictionaryBinding.Refresh(result.PermissionPass.PermissionResult,SourceGraph,endingNode,span,cancellationToken);
            //Permissions are the acoustic pre-wrapper bytes. WorkingMap is
            //the actual post234840 map installed in the case query backend.
            result=result with {PermissionPass=result.PermissionPass with
                {PermissionResult=result.PermissionPass.PermissionResult with {WorkingMap=actualMap}}};
            cancellationToken.ThrowIfCancellationRequested();
            return new(this, checked(++epoch), endingNode, span, result);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    /// <summary>The same selected group costs feed frontend128. An older span
    /// cannot query after another D0 preparation has changed its dictionary map.</summary>
    public LegacyNativeWordAcousticGroupCosts GetCosts(LegacyOwnedMainWordSpan span,
        int node, ushort ordinal, CancellationToken cancellationToken = default)
    {
        Enter();
        try
        {
            Check(span, cancellationToken);
            var arcs = SourceGraph.GetNode(node);
            if (node != span.EndingNode || ordinal >= arcs.Count || arcs[ordinal].Span != span.Span)
                throw new ArgumentException("Acoustic callback must remain inside its owned selected span.");
            if (!costs.TryGetValue(arcs[ordinal].GroupId, out var result))
                throw new InvalidDataException("Selected main group has no computed owned cache.");
            return result;
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    public LegacyAggregateDictionaryQueryPage Read(LegacyOwnedMainWordSpan span, ulong tag,
        LegacyPreparedDictionaryQuery query, bool auxiliary = false, bool clearExtraCosts = false,
        CancellationToken cancellationToken = default)
    {
        Enter();
        try
        {
            Check(span, cancellationToken);
            return dictionaryBinding.Read(tag, query, auxiliary, clearExtraCosts, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    public ReadOnlyMemory<byte> ReadWordTerminal(LegacyOwnedMainWordSpan span,ulong sourceTag,ushort endingSymbol,
        byte boundaryArgument,CancellationToken cancellationToken=default)
    {
        Enter();try{Check(span,cancellationToken);return dictionaryBinding.ReadTerminal(sourceTag,endingSymbol,boundaryArgument,cancellationToken);}
        catch{valid=false;throw;}finally{Volatile.Write(ref active,0);}
    }
    public ulong ResolveWordRestart(LegacyOwnedMainWordSpan span,ulong sourceTag,CancellationToken cancellationToken=default)
    {
        Enter();try{Check(span,cancellationToken);return dictionaryBinding.ResolveRestart(sourceTag,cancellationToken);}
        catch{valid=false;throw;}finally{Volatile.Write(ref active,0);}
    }

    private LegacyOrdinaryGraphAcousticRow Compute(LegacyOrdinaryCursiveCandidateInk group,
        CancellationToken token)
    {
        var scores = singleAcoustic is {} single ? single.Score(group.GroupId, token) : lineAcoustic!.Score(group.GroupId, token);
        int geometry = LegacyCursiveGroupSeparationCost.Evaluate(SourceInk,
            group.Ranges.Span, SourceHeight, separation.GapOffset, separation.Slope,
            table.Values.Span, separation.MaximumCost, cancellationToken: token);
        costs[group.GroupId] = new(scores.Cache.Costs.Span,
            Enumerable.Repeat(geometry, source.CompositeCategoryCount).ToArray());
        token.ThrowIfCancellationRequested();
        return new(scores.Cache.Costs, (singleAcoustic?.CostScale ?? lineAcoustic!.CostScale));
    }

    private void Enter()
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Owned main span preparation/query calls must be serialized.");
        if (!valid)
        {
            Volatile.Write(ref active, 0);
            throw new InvalidOperationException("Failed main span owner requires a fresh preparation.");
        }
    }

    private void Check(LegacyOwnedMainWordSpan span, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(span);
        token.ThrowIfCancellationRequested();
        if (!valid || !ReferenceEquals(span.Owner, this) || span.Epoch != epoch)
            throw new InvalidOperationException("Span cache/query belongs to a stale or foreign D0 preparation.");
        dictionaryBinding.ValidateCurrentMap();
    }

    internal void ValidateSpan(LegacyOwnedMainWordSpan span, CancellationToken token)
        => Check(span, token);

    internal sealed class LivePublicationEpoch
    {
        internal LegacyOwnedMainWordSpanPreparation Owner { get; }
        internal long Epoch { get; }
        internal int EndingNode { get; }
        internal bool Completed { get; set; }
        internal LivePublicationEpoch(LegacyOwnedMainWordSpanPreparation owner, long epoch, int endingNode)
        { Owner = owner; Epoch = epoch; EndingNode = endingNode; }
    }

    //2144F0's intermediate207CB0 precedes V80: retain the actual last D0
    //map and query caches instead of manufacturing a final-result epoch.
    internal LivePublicationEpoch CaptureLivePublicationEpoch(LegacyOwnedMainWordSpan span, CancellationToken token)
    {
        Enter();
        try
        {
            Check(span, token);
            dictionaryBinding.CaptureLivePublicationQueryLease(span.EndingNode, token);
            return activeLivePublicationEpoch = new(this, epoch, span.EndingNode);
        }
        finally { Volatile.Write(ref active, 0); }
    }

    internal void ValidateLivePublicationEpoch(LivePublicationEpoch lease, CancellationToken token)
    {
        Enter();
        try { CheckLivePublicationEpoch(lease, token); }
        finally { Volatile.Write(ref active, 0); }
    }

    private void CheckLivePublicationEpoch(LivePublicationEpoch lease, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(lease);
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(lease.Owner, this) || !ReferenceEquals(lease, activeLivePublicationEpoch) ||
            lease.Epoch != epoch || lease.Completed)
            throw new InvalidOperationException("Live publication must retain its producing D0 owner and epoch.");
        dictionaryBinding.ValidateLivePublicationQueryLease(token);
    }

    internal LegacyAggregateDictionaryQueryPage ReadLivePublicationQuery(LivePublicationEpoch lease, ulong tag,
        LegacyPreparedDictionaryQuery query, bool auxiliary, CancellationToken token)
    {
        Enter();
        try
        {
            CheckLivePublicationEpoch(lease, token);
            return dictionaryBinding.ReadLivePublicationQuery(tag, query, auxiliary, token);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    internal void EndLivePublicationEpoch(LivePublicationEpoch lease, CancellationToken token)
    {
        Enter();
        try
        {
            CheckLivePublicationEpoch(lease, token);
            dictionaryBinding.EndLivePublicationQueryLease(token);
            lease.Completed = true;
            activeLivePublicationEpoch = null;
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    // Result reconstruction runs after the controller's final source V80.
    // Its query source is the same backend, without another D0 or map refresh.
    internal long CaptureResultQueryEpoch(CancellationToken token)
    {
        Enter();
        try
        {
            token.ThrowIfCancellationRequested();
            dictionaryBinding.CaptureResultQueryLease(token);
            return epoch;
        }
        finally { Volatile.Write(ref active, 0); }
    }

    internal LegacyAggregateDictionaryQueryPage ReadResultQuery(long resultEpoch, ulong tag,
        LegacyPreparedDictionaryQuery query, bool auxiliary, CancellationToken token)
    {
        Enter();
        try
        {
            token.ThrowIfCancellationRequested();
            if (resultEpoch != epoch)
                throw new InvalidOperationException("A result query cannot use a source changed by a later D0/controller epoch.");
            return dictionaryBinding.ReadResultQuery(tag, query, auxiliary, token);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    internal void ValidateResultQueryEpoch(long resultEpoch, CancellationToken token)
    {
        Enter();
        try
        {
            token.ThrowIfCancellationRequested();
            if (resultEpoch != epoch)
                throw new InvalidOperationException("Result reconstruction requires the producing controller source epoch.");
            dictionaryBinding.ValidateResultQueryLease(token);
        }
        finally { Volatile.Write(ref active, 0); }
    }

    private sealed record SourceBinding(LegacyFreshSingleGlyphMainSampled? Single, LegacyFreshMainSingleLineSampled? Line)
    {
        public static SourceBinding FromSingle(LegacyFreshSingleGlyphMainSampled source)
        { ArgumentNullException.ThrowIfNull(source); return new(source,null); }
        public static SourceBinding FromLine(LegacyFreshMainSingleLineSampled source)
        { ArgumentNullException.ThrowIfNull(source); return new(null,source); }
        public LegacyOrdinaryCursiveCandidateGraph Graph => Single is {} s ? s.SourceRanges.Graph : Line!.SourceRanges.Graph;
        public LegacyCursivePreparedInk Ink => Single is {} s ? s.Cuts.PreparedInk : Line!.Cuts.PreparedInk;
        public float Height => Single is {} s ? s.SourceHeight : Line!.Owner.SourceHeight;
        public LegacySourceSamplerCategoryOwner Categories => Single is {} s ? s.Owner.CategoryOwner : Line!.Owner.CategoryOwner;
        public int CompositeCategoryCount => Single is {} s ? s.Owner.TransitionProfile.Categories.CompositeCategories.Length :
            Line!.Owner.TransitionProfile.Categories.CompositeCategories.Length;
    }

}
