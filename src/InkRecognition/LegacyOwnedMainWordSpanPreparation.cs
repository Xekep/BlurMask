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
/// The dictionary is explicitly prepared directly; this does not resolve
/// outer290/292 decorator factories or the complete2144F0 search scheduler.
/// </summary>
public sealed class LegacyOwnedMainWordSpanPreparation
{
    private readonly LegacyNativeWordSymbolPreparationSession session;
    private readonly LegacyPreparedDictionaryQuerySource dictionary;
    private readonly LegacyPreparedAggregateDictionaryQuerySource aggregate;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly LegacyCursiveSeparationTable table;
    private readonly Dictionary<int, LegacyNativeWordAcousticGroupCosts> costs = [];
    private long epoch;
    private bool valid = true;
    private int active;

    public LegacyOwnedRussianDictionaryPreparation DictionaryOwner { get; }
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
    public LegacySymbolContextFactory Contexts => DictionaryOwner.Contexts;
    public int SourceRowBudget => AcousticPolicy.MaximumSourceArcs;

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

    private LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        SourceBinding actualSource, LegacyFreshMainSingleLineContext? actualContext,
        LegacyCursiveSeparationTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        source = actualSource;
        var model = LegacyRussianModelBundle.Load(acousticResource, lexicalResource, engineProfile);
        if(actualSource.Single is {} single)
        {
            DictionaryOwner = LegacyOwnedRussianDictionaryPreparation.Prepare(acousticResource,
                lexicalResource, model, single, cancellationToken);
            singleAcoustic = DictionaryOwner.CreateAcousticScorer(acousticResource, cancellationToken);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(actualContext);
            if(!ReferenceEquals(actualContext.Source,actualSource.Line))
                throw new ArgumentException("Context must belong to the actual multi-physical sampled source.");
            DictionaryOwner = LegacyOwnedRussianDictionaryPreparation.Prepare(acousticResource,
                lexicalResource, model, actualSource.Line!, cancellationToken);
            lineAcoustic = DictionaryOwner.CreateAcousticScorer(acousticResource, actualContext, cancellationToken);
        }
        this.table = table;
        separation = LegacyCursiveSeparationProfile.ReadV106Node(acousticResource,
            actualSource.Categories.SamplerNodeOffset);
        var map = DictionaryOwner.Symbols.CreateWorkingMap(DictionaryOwner.Descriptor.Flags);
        dictionary = new(model.Lexicon, map, DictionaryOwner.Symbols.CopyInitialContexts());
        aggregate = new([LegacyOrdinaryDictionarySourceOwner.BindDictionary(dictionary, Contexts,
            actualSource.Graph)]);
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
            dictionary.SetWorkingMap(result.PermissionPass.PermissionResult.WorkingMap);
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
            return aggregate.Read(tag, query, auxiliary, clearExtraCosts, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
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
    }

    internal void ValidateSpan(LegacyOwnedMainWordSpan span, CancellationToken token)
        => Check(span, token);

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
