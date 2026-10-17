namespace InkRecognition;

/// <summary>Computed ordinary cache data, not caller-supplied lifecycle flags.</summary>
public sealed record LegacyOrdinaryGraphAcousticRow(ReadOnlyMemory<int> Costs, float CostScale);

public sealed record LegacyOrdinaryGraphSelectedAcousticRow(LegacyGraphGlyphPosition Position, int GroupId,
    ReadOnlyMemory<int> Costs, bool Recomputed);

public sealed record LegacyOrdinaryGraphPermissionPass(bool Succeeded,
    LegacyPreparedAcousticPermissionResult PermissionResult,
    IReadOnlyList<LegacyGraphGlyphPosition> SelectedPositions,
    IReadOnlyList<LegacyOrdinaryGraphSelectedAcousticRow> SelectedRows, int ComputedRows, int ReusedRows);

/// <summary>
/// Bounded fresh-frontend1520b0/1c01b0 over an immutable ordinary same-source graph.
/// Ordinary group kind0 comes from19f850/1a3b30, not a supplied row flag.
/// The producer returns computed cache data; null means native preparation failed.
/// No special-token/cross-source graph, graph mutation or final winner is inferred.
/// Calls must be serialized. Failed/canceled/throwing preparation invalidates this
/// session, preventing an undefined partially prepared native state from escaping.
/// </summary>
public sealed class LegacyOrdinaryGraphAcousticPermissionSession
{
    private readonly LegacyOrdinaryRecognitionGraph graph;
    private readonly Func<int, CancellationToken, LegacyOrdinaryGraphAcousticRow?> compute;
    private readonly ushort[] reserved;
    private readonly Record[] records;
    private readonly List<PoolEntry> pool = [];
    private LegacyPreparedDictionaryWorkingMap source;
    private bool valid = true;
    private int active;

    public LegacyOrdinaryGraphAcousticPermissionSession(LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyPreparedDictionaryWorkingMap source, ReadOnlySpan<ushort> reservedSlots,
        Func<LegacyOrdinaryCursiveCandidateInk, CancellationToken, LegacyOrdinaryGraphAcousticRow?> compute)
        :this(LegacyOrdinaryRecognitionGraph.FromSameSource(graph),source,reservedSlots,AdaptCompute(graph,compute)){}

    internal LegacyOrdinaryGraphAcousticPermissionSession(LegacyOrdinaryRecognitionGraph graph,
        LegacyPreparedDictionaryWorkingMap source, ReadOnlySpan<ushort> reservedSlots,
        Func<int, CancellationToken, LegacyOrdinaryGraphAcousticRow?> compute)
    {
        ArgumentNullException.ThrowIfNull(graph); ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(compute);
        if (reservedSlots.Length != 6 || graph.GroupCount is < 1 or > 131072 ||
            source.AcousticSymbolCount > 4093)
            throw new ArgumentException("Checked ordinary graph, six retained slots and bounded group count required.");
        foreach (ushort slot in reservedSlots)
            if (slot != ushort.MaxValue && slot >= source.AcousticSymbolCount)
                throw new ArgumentException("Retained slot outside ordinary alphabet.");
        this.graph = graph; this.source = source; this.compute = compute; reserved = reservedSlots.ToArray();
        records = Enumerable.Range(0, graph.GroupCount).Select(_ => new Record()).ToArray();
    }

    private static Func<int,CancellationToken,LegacyOrdinaryGraphAcousticRow?> AdaptCompute(
        LegacyOrdinaryCursiveCandidateGraph graph,
        Func<LegacyOrdinaryCursiveCandidateInk,CancellationToken,LegacyOrdinaryGraphAcousticRow?> compute)
    {
        ArgumentNullException.ThrowIfNull(compute);
        return (group,token)=>compute(graph.Groups[group],token);
    }

    /// <summary>
    /// Native param3 is the graph anchor; param4 is the requested arc-span byte;
    /// param5 permits a computed-record reuse. All arc IDs at this anchor enter
    /// the pool invalidation set, including arcs rejected by the span callback.
    /// This implements fresh1503f0 flags0:150d00 maps bit9 to initializer+12,
    /// which1b0760 stores in runtime1a2=0. Later configuration overrides are
    /// outside this session; the nonzero1a2 no-invalidation branch is not guessed.
    /// A cached pool row also reuses when param5 is false. Before recomputation
    /// native310 chooses the first vacant pool entry, or appends one, and writes
    /// its group ID/scope. Per-group cost pointers retain that buffer identity.
    /// Successful row snapshots are consumed in callback order by source68.
    /// </summary>
    public LegacyOrdinaryGraphPermissionPass Evaluate(int anchorNode, byte requestedSpan,
        bool reuseComputed, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Ordinary cache session calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed ordinary cache preparation requires a fresh session.");
            var arcs = graph.GetNode(anchorNode);
            if (arcs.Count > ushort.MaxValue)
                throw new NotSupportedException("Native ordinary anchor count exceeds its ushort domain.");
            cancellationToken.ThrowIfCancellationRequested();
            var present = arcs.Select(a => a.GroupId).ToHashSet();
            foreach (var entry in pool)
                if (entry.Scope == requestedSpan && !present.Contains(entry.GroupId)) entry.GroupId = -1;
            var selected = new List<LegacyGraphGlyphPosition>();
            var rows = new List<LegacyPreparedAcousticPermissionGroup>();
            var emitted = new List<LegacyOrdinaryGraphSelectedAcousticRow>();
            int computed = 0, reused = 0;
            for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested(); var arc = arcs[ordinal];
                //1520b0 ->21b.virtual70=1a7850 reads native arc+4, not flags+6 or placement+5.
                if (arc.Span != requestedSpan) continue;
                selected.Add(new(anchorNode, ordinal));
                if ((uint)arc.GroupId >= records.Length)
                    throw new InvalidDataException("Checked graph group identity differs.");
                var record = records[arc.GroupId];
                bool recompute = !(reuseComputed && record.Computed) &&
                    !pool.Any(p => p.GroupId == arc.GroupId);
                if (recompute)
                {
                    var entry = pool.FirstOrDefault(p => p.GroupId == -1);
                    if (entry is null)
                    {
                        int width = source.AcousticSymbolCount + 3;
                        if (pool.Count >= 16_000_000 / width)
                            throw new InvalidOperationException("Ordinary pooled costs exceed the explicit bounded memory domain.");
                        entry = new(width); pool.Add(entry);
                    }
                    entry.Scope = requestedSpan; entry.GroupId = arc.GroupId; record.Entry = entry;
                    var actual = compute(arc.GroupId, cancellationToken);
                    record.Computed = true;
                    if (actual is null)
                    {
                        valid = false;
                        return new(false, new(false, ReadOnlyMemory<byte>.Empty, null),
                            Array.AsReadOnly(selected.ToArray()), Array.AsReadOnly(emitted.ToArray()), computed, reused);
                    }
                    float threshold = actual.CostScale * 40000f;
                    if (actual.Costs.Length != entry.Costs.Length || !float.IsFinite(actual.CostScale) ||
                        actual.CostScale < 0 || !float.IsFinite(threshold) || threshold >= 2147483648f)
                        throw new InvalidDataException("Computed ordinary row has invalid native width/scale.");
                    actual.Costs.Span.CopyTo(entry.Costs); record.Scale = actual.CostScale; computed++;
                }
                else reused++;
                if (record.Entry is null) throw new InvalidDataException("Reused ordinary cache record has no computed buffer.");
                // Ordinary-only graph producers initialize group+7 to0. No B0 special record is consulted.
                var snapshot = record.Entry.Costs.ToArray();
                rows.Add(new(snapshot, record.Scale, recompute, 0, false, 0, true));
                emitted.Add(new(new(anchorNode, ordinal), arc.GroupId, snapshot, recompute));
            }
            var permission = LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(source, reserved, rows, cancellationToken);
            source = permission.WorkingMap!;
            return new(true, permission, Array.AsReadOnly(selected.ToArray()), Array.AsReadOnly(emitted.ToArray()), computed, reused);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private sealed class Record { public bool Computed; public float Scale; public PoolEntry? Entry; }
    private sealed class PoolEntry(int width)
    {
        public int GroupId = -1; public byte Scope; public int[] Costs { get; } = new int[width];
    }
}
