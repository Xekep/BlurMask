namespace InkRecognition;

public delegate LegacyPreparedDictionaryQueryPage LegacyAggregateDictionaryReader(
    LegacyPreparedDictionaryQuery query, bool auxiliary, bool clearExtraCosts, CancellationToken cancellationToken);

/// <summary>One already selected child. Selection/priority/weight production is a separate stage.</summary>
public sealed record LegacyAggregateDictionaryChild(LegacyAggregateDictionaryReader Read,
    Func<uint, bool> IsEligible)
{
    /// <summary>Dictionary2b7 virtual88=093ab0. Category800 consults the actual
    /// frontend280 getter; all other categories, including RU C0A, are eligible.
    /// The shared symbol factory owns source68 and every supplied parent context.</summary>
    public static LegacyAggregateDictionaryChild BindDictionary(LegacyPreparedDictionaryQuerySource source,
        LegacySymbolContextFactory contexts, Func<bool>? frontendMode280 = null)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(contexts);
        bool Eligible(uint unusedOwnerNode)
        {
            if ((source.DescriptorFlags & 0x1e00) != 0x800) return true;
            if (frontendMode280 is null) throw new NotSupportedException("Category800 requires its live frontend280 getter.");
            return !frontendMode280();
        }
        LegacyPreparedDictionaryQueryPage Read(LegacyPreparedDictionaryQuery query, bool auxiliary,
            bool clearExtraCosts, CancellationToken token)
        {
            if (query.ContextRoot != 0 && query.ContextRoot != contexts.ContainerIdentity)
                throw new ArgumentException("Query context container differs from the dictionary's shared factory.");
            return source.Read(query, auxiliary,
                (parent, symbol) => contexts.ResolveChild(parent, symbol, token),
                contexts.LinkCreatedContext, clearExtraCosts, token);
        }
        return new(Read, Eligible);
    }
}

public sealed record LegacyAggregateDictionaryQueryPage(int ReturnedCount, ReadOnlyMemory<byte> Records,
    ReadOnlyMemory<byte> ExtraCosts, int SelectedOrdinal, int RestoredSkip,
    LegacyPreparedDictionaryQueryPage? ChildPage);

/// <summary>
/// Native aggregate28d normal/auxiliary query236910. An immutable already
/// selected child table is supplied; its current-source cursor and per-child
/// accepted-count bases are produced here. A startup page never combines
/// records from two children. Calls must be serialized. Failed preparation
/// invalidates this session; native out-of-bounds requests are rejected.
/// </summary>
public sealed class LegacyPreparedAggregateDictionaryQuerySource
{
    private readonly LegacyAggregateDictionaryChild[] children;
    private readonly int[] bases;
    private int current;
    private int active;
    private bool valid = true;
    public int CurrentSelectedOrdinal => current;
    public int[] CopyAcceptedCountBases() => bases.ToArray();

    public LegacyPreparedAggregateDictionaryQuerySource(IReadOnlyList<LegacyAggregateDictionaryChild> selectedChildren)
    {
        ArgumentNullException.ThrowIfNull(selectedChildren);
        if (selectedChildren.Count > 255) throw new ArgumentException("Native source IDs require at most255 selected children.");
        children = selectedChildren.ToArray();
        foreach (var child in children)
            if (child is null || child.Read is null || child.IsEligible is null)
                throw new ArgumentException("Each selected child requires a reader and eligibility producer.");
        bases = Enumerable.Repeat(-1, children.Length).ToArray();
    }

    /// <param name="sourceTag">Full query tag: node00, selected source ID04, header05.
    /// It is transported as data, never a native pointer.</param>
    public LegacyAggregateDictionaryQueryPage Read(ulong sourceTag, LegacyPreparedDictionaryQuery query,
        bool auxiliary = false, bool clearExtraCosts = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query); cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Aggregate source calls must be serialized.");
        int globalSkip = query.Skip;
        try
        {
            if (!valid) throw new InvalidOperationException("Failed aggregate preparation requires a fresh session.");
            if ((uint)query.Node != (uint)sourceTag || query.Node < 0 || query.Skip is < 0 or > 2_000_000 ||
                query.Capacity is < 1 or > 512)
                throw new ArgumentException("Query tag/node, bounded skip and capacity must agree.");
            byte sourceId = (byte)(sourceTag >> 32), header = (byte)(sourceTag >> 40);
            bool startup = query.Node == 0 && sourceId == 0 && (header & 8) != 0;
            if (startup)
            {
                if (globalSkip == 0) { current = 0; Array.Fill(bases, -1); }
                while (current < children.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (bases[current] == -1) bases[current] = globalSkip;
                    int relative = globalSkip - bases[current];
                    if (relative < 0) throw new ArgumentException("Startup skip precedes the current child's accepted-count base.");
                    query.Skip = relative;
                    var child = children[current];
                    bool eligible = child.IsEligible(query.Parameter1c);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (eligible)
                    {
                        var page = ReadChild(child, query, auxiliary, clearExtraCosts, cancellationToken);
                        query.Skip = globalSkip;
                        if (page.ReturnedCount != 0) return Stamp(page, current, globalSkip);
                    }
                    query.Skip = globalSkip;
                    current++;
                }
                return new(0, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, -1, globalSkip, null);
            }
            if (sourceId == 0 || sourceId > children.Length)
                throw new ArgumentException("Continuation source ID is absent from the selected table.");
            int ordinal = sourceId - 1;
            var selected = children[ordinal];
            bool selectedEligible = selected.IsEligible(query.Parameter1c);
            cancellationToken.ThrowIfCancellationRequested();
            if (!selectedEligible)
                return new(-1, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, ordinal, globalSkip, null);
            return Stamp(ReadChild(selected, query, auxiliary, clearExtraCosts, cancellationToken), ordinal, globalSkip);
        }
        catch { valid = false; throw; }
        finally { query.Skip = globalSkip; Volatile.Write(ref active, 0); }
    }

    private static LegacyPreparedDictionaryQueryPage ReadChild(LegacyAggregateDictionaryChild child,
        LegacyPreparedDictionaryQuery query, bool auxiliary, bool clearExtraCosts, CancellationToken token)
    {
        var before = (query.Node, query.Skip, query.Capacity, query.Parameter1c, query.Parameter20,
            query.Flags24, query.Flags26, query.Category18, query.Parameter19, query.ContextRoot, query.ParentContext);
        var page = child.Read(query, auxiliary, clearExtraCosts, token);
        token.ThrowIfCancellationRequested();
        var after = (query.Node, query.Skip, query.Capacity, query.Parameter1c, query.Parameter20,
            query.Flags24, query.Flags26, query.Category18, query.Parameter19, query.ContextRoot, query.ParentContext);
        if (before != after) throw new InvalidDataException("Child mutated aggregate request data.");
        int records = Math.Max(0, page.ReturnedCount);
        if (page.ReturnedCount < -2_000_000 || records > query.Capacity || page.Records.Length != records * 32 ||
            (clearExtraCosts ? page.ClearedExtraCosts.Length != records * 12 : !page.ClearedExtraCosts.IsEmpty))
            throw new InvalidDataException("Child page differs from its native record/cost extent.");
        return page;
    }

    private static LegacyAggregateDictionaryQueryPage Stamp(LegacyPreparedDictionaryQueryPage page, int ordinal, int skip)
    {
        byte[] records = page.Records.ToArray();
        for (int i = 0; i < Math.Max(0, page.ReturnedCount); i++) records[i * 32 + 4] = (byte)(ordinal + 1);
        return new(page.ReturnedCount, records, page.ClearedExtraCosts.ToArray(), ordinal, skip, page);
    }
}
