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
    public LegacyPreparedFreshMain240AcousticScorer Acoustic { get; }
    public LegacyFreshSingleGlyphMainSampled Source => Acoustic.Source;
    public LegacySymbolContextFactory Contexts => DictionaryOwner.Contexts;
    public int SourceRowBudget => Acoustic.Policy.MaximumSourceArcs;

    public LegacyOwnedMainWordSpanPreparation(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, ReadOnlySpan<byte> engineProfile,
        LegacyFreshSingleGlyphMainSampled actualSource, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualSource);
        ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        var model = LegacyRussianModelBundle.Load(acousticResource, lexicalResource, engineProfile);
        DictionaryOwner = LegacyOwnedRussianDictionaryPreparation.Prepare(acousticResource,
            lexicalResource, model, actualSource, cancellationToken);
        Acoustic = DictionaryOwner.CreateAcousticScorer(acousticResource, cancellationToken);
        this.table = table;
        separation = LegacyCursiveSeparationProfile.ReadV106Node(acousticResource,
            actualSource.Owner.CategoryOwner.SamplerNodeOffset);
        var map = DictionaryOwner.Symbols.CreateWorkingMap(DictionaryOwner.Descriptor.Flags);
        dictionary = new(model.Lexicon, map, DictionaryOwner.Symbols.CopyInitialContexts());
        aggregate = new([LegacyOrdinaryDictionarySourceOwner.BindDictionary(dictionary, Contexts,
            actualSource.SourceRanges.Graph)]);
        ushort[] reserved = LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(
            model.AcousticAlphabet, acousticResource.Slice(99, 16));
        session = new(actualSource.SourceRanges.Graph, map, reserved, Compute);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>1A7580 scans arc byte4 and returns its maximum plus one,
    /// or zero for an empty row. This includes empty intermediate spans.
    /// It is neither node+1, maximum atoms nor the125 source-row budget.</summary>
    public int GetSpanExtent(int node)
    {
        var arcs = Source.SourceRanges.Graph.GetNode(node);
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
            var arcs = Source.SourceRanges.Graph.GetNode(node);
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
        var scores = Acoustic.Score(group.GroupId, token);
        int geometry = LegacyCursiveGroupSeparationCost.Evaluate(Source.Cuts.PreparedInk,
            group.Ranges.Span, Source.SourceHeight, separation.GapOffset, separation.Slope,
            table.Values.Span, separation.MaximumCost, cancellationToken: token);
        costs[group.GroupId] = new(scores.Cache.Costs.Span,
            Enumerable.Repeat(geometry, Source.Owner.TransitionProfile.Categories.CompositeCategories.Length).ToArray());
        token.ThrowIfCancellationRequested();
        return new(scores.Cache.Costs, Acoustic.CostScale);
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
}
