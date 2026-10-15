using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Owned ordinary207CB0 compact storage and its real per-node2478
/// offsets. Typed DIRECT/CASE entrypoints retain their exact source owner;
/// this is not21FFD0 glyph/text output.</summary>
public sealed class LegacyOwnedMainCompactWordMaterialization
{
    private readonly byte[] storage;
    private readonly int[] starts;
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    public LegacyOwnedMainTerminalResult TerminalResult { get; }
    public LegacyOwnedMainCompactWordReachability Reachability { get; }
    public LegacyOwnedRussianMainGlyphConfidence ConfidenceOwner { get; }
    public LegacyFreshCaseMainSourcePolicy? CaseSourcePolicy { get; }
    public int RecordCount { get; }
    public int SelectedRecordIndex { get; }
    public ushort SelectedAlternative => ushort.MaxValue;

    private LegacyOwnedMainCompactWordMaterialization(LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedMainCompactWordReachability reachable, LegacyOwnedRussianMainGlyphConfidence confidence,
        byte[] storage, int count, int[] starts, LegacyMainDirectWordNodeResult[] nodes, int selected,
        LegacyFreshCaseMainSourcePolicy? caseSourcePolicy)
    {
        TerminalResult = terminal; Reachability = reachable; ConfidenceOwner = confidence;
        this.storage = storage; RecordCount = count; this.starts = starts; this.nodes = nodes;
        SelectedRecordIndex = selected;
        CaseSourcePolicy = caseSourcePolicy;
    }

    /// <summary>Fresh mode0/2/2/1 DIRECT route with no associated source or
    /// tracing. The residue is explicit complete compact40 allocator storage;
    /// no zeroed or native physical-allocation policy is inferred from it.</summary>
    public static LegacyOwnedMainCompactWordMaterialization Materialize(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyOwnedRussianMainGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, CancellationToken cancellationToken = default)
        => MaterializeCore(resource, terminal, confidence, allocatorResidue40, null, cancellationToken);

    internal static LegacyOwnedMainCompactWordMaterialization MaterializeCase(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy sourcePolicy, LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedRussianMainGlyphConfidence confidence, ReadOnlySpan<byte> allocatorResidue40,
        CancellationToken cancellationToken)
        => MaterializeCore(resource, terminal, confidence, allocatorResidue40, sourcePolicy, cancellationToken);

    private static LegacyOwnedMainCompactWordMaterialization MaterializeCore(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyOwnedRussianMainGlyphConfidence confidence,
        ReadOnlySpan<byte> allocatorResidue40, LegacyFreshCaseMainSourcePolicy? casePolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(terminal); ArgumentNullException.ThrowIfNull(confidence);
        cancellationToken.ThrowIfCancellationRequested();
        var pass = terminal.PassResult;
        if (pass.IntermediatePublicationState is null && pass.NodeCount > pass.ControllerOwner.MaximumParentDistance54)
            throw new NotSupportedException("Fresh one-shot publication requires a source before the native intermediate-publication boundary.");
        if (!terminal.Winner.HasWinner)
            throw new NotSupportedException("A real preselected numeric terminal is required.");
        ValidatePublicationOwner(resource, pass.ControllerOwner, casePolicy, confidence);
        var state = pass.IntermediatePublicationState?.Copy() ?? new LegacyMainCompactPublicationState(pass.ControllerOwner, allocatorResidue40);
        if (pass.IntermediatePublicationState is not null && !allocatorResidue40.SequenceEqual(state.InitialResidue))
            throw new ArgumentException("Final publication must retain the intermediate compact allocator owner.");
        pass.BeginCompactWordPublication(cancellationToken);
        var source = LegacyMainCompactPublicationSource.Final(resource, pass, casePolicy, state.NextNode, cancellationToken);
        var stage = Publish(resource, source, confidence, state, cancellationToken);
        int selected = state.Starts[pass.NodeCount - 1] + U16(stage.Nodes[^1].Records.Span, terminal.Winner.Ordinal * 88 + 0x22);
        if ((uint)selected >= state.Count) throw new InvalidDataException("Selected native row22 does not name the published compact pool.");
        return new(terminal, stage.Reachability, confidence, state.Storage.ToArray(), state.Count,
            state.Starts.ToArray(), stage.Nodes, selected, casePolicy);
    }

    internal sealed record PublicationStage(LegacyOwnedMainCompactWordReachability Reachability,
        LegacyMainDirectWordNodeResult[] Nodes);

    internal static void ValidatePublicationOwner(ReadOnlySpan<byte> resource,
        LegacyOwnedFreshMainWordControllerPolicy controller, LegacyFreshCaseMainSourcePolicy? casePolicy,
        LegacyOwnedRussianMainGlyphConfidence confidence)
    {
        var owner = controller.Owner;
        if (!ReferenceEquals(confidence.Model, owner.CommonModel) || !ReferenceEquals(confidence.Graph, owner.SourceGraph) ||
            (casePolicy is null ? owner.IsCaseAware || !ReferenceEquals(confidence.DirectDictionaryOwner, owner.DictionaryOwner) :
                !owner.IsCaseAware || !ReferenceEquals(casePolicy.Controller, controller) ||
                !ReferenceEquals(owner.CaseDictionaryOwner, casePolicy.DictionaryOwner) ||
                !ReferenceEquals(confidence.CaseDictionaryOwner, casePolicy.DictionaryOwner)))
            throw new ArgumentException("Publication requires its actual typed controller, graph and candidate owner.");
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("Compact publication must use its exact selected main resource.");
    }

    internal static PublicationStage Publish(ReadOnlySpan<byte> resource, LegacyMainCompactPublicationSource source,
        LegacyOwnedRussianMainGlyphConfidence confidence, LegacyMainCompactPublicationState state,
        CancellationToken cancellationToken)
    {
        source.Validate(cancellationToken);
        var owner = source.Owner;
        var casePolicy = source.CasePolicy;
        var configuration = source.Controller.Configuration;
        if (!ReferenceEquals(state.Controller, source.Controller) || state.NextNode != source.StartingNode)
            throw new ArgumentException("Publication must advance its actual persistent compact state.");
        if (!ReferenceEquals(confidence.Model, owner.CommonModel) || !ReferenceEquals(confidence.Graph, owner.SourceGraph))
            throw new ArgumentException("Candidate weights, source graph and terminal must share the actual model owner.");
        LegacyOwnedFreshCaseMainTerminalInvocation? caseInvocation = null;
        if (casePolicy is null)
        {
            if (owner.IsCaseAware || !ReferenceEquals(confidence.DirectDictionaryOwner, owner.DictionaryOwner))
                throw new ArgumentException("The DIRECT entrypoint requires its actual DIRECT dictionary owner.");
        }
        else
        {
            if (!owner.IsCaseAware || !ReferenceEquals(casePolicy.Controller, source.Controller) ||
                !ReferenceEquals(owner.CaseDictionaryOwner, casePolicy.DictionaryOwner) ||
                !ReferenceEquals(confidence.CaseDictionaryOwner, casePolicy.DictionaryOwner))
                throw new ArgumentException("One actual fresh CASE source/controller/candidate owner required.");
            caseInvocation = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(casePolicy, cancellationToken);
        }
        if (configuration.HeaderCount2400 != 2)
            throw new NotSupportedException("The actual fresh two-header publication policy is required.");
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("Compact publication must use its exact selected main resource.");
        var reachable = LegacyOwnedMainCompactWordReachability.PrepareSource(resource, source, cancellationToken);
        int primary = configuration.PrimaryArcLimit230a;
        var alphabet = owner.CommonModel.AcousticAlphabet;
        var categories = new LegacyCursiveCategoryMapper(owner.SelectedSourceOwner.Categories);
        var policy = LegacyPreparedCompactWordContextPolicy.FromAlphabet(alphabet, false, false,
            caseInvocation?.Options.ContextSentinels24b8 ??
            LegacyOwnedFreshMainTerminalInvocation.Prepare(source.Controller).Options.ContextSentinels24b8);
        ushort specialMarker = Label("\u0003");
        ushort shadda = Label("\u0651"), closed = Label("\U000f0001");
        byte[] header = alphabet.CopySerializedHeader();
        ushort[] reserved = [checked((ushort)alphabet.LabelCount), U16(header, 8), U16(header, 10), U16(header, 12)];
        var snapshots = Enumerable.Range(0, source.EndingNode + 1).Select(reachable.GetNodeSnapshot).ToArray();
        var rows = snapshots.Select(s => s.Records.ToArray()).ToArray();
        var pools = snapshots.Select(s => s.PrimaryAlternatives.ToArray()).ToArray();
        byte[] storage = state.Storage.ToArray();
        var nodeStarts = state.Starts.ToArray();
        var prefixOwners = state.PrefixOwners.ToArray();
        var scores = new Dictionary<int, LegacyOwnedRussianMainGlyphConfidenceGroup>();
        int count = state.Count;
        float penalty = (caseInvocation?.LanguagePenalty2534 ??
            LegacyOwnedRussianTerminalSources.Prepare(owner.DictionaryOwner, cancellationToken).LanguagePenalty2534) * .8f;
        int forcedPenalty = (int)(penalty <= 0 ? penalty - .5f : penalty + .5f);

        for (int node = source.StartingNode; node <= source.EndingNode; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            nodeStarts[node] = count;
            prefixOwners[node] = source.StartingNode - 1;
            var usedArcs = new bool[owner.SourceGraph.GetNode(node).Count];
            var candidates = new List<Candidate>();
            for (ushort ordinal = 0; ordinal < primary; ordinal++)
            {
                for (int alternative = 0; alternative < 2; alternative++)
                {
                    int at = (ordinal * 2 + alternative) * 40;
                    if ((sbyte)pools[node][at + 0x1b] >= 0) continue;
                    var row = rows[node].AsSpan(ordinal * 88, 88);
                    if (row[4] != 1 || (row[7] & 1) != 0 || I32(row, 0x30) != -1 || U16(row, 0x24) == specialMarker)
                        throw new NotSupportedException("Unexpanded ordinary source1 fallback candidate required.");
                    int arc = U16(row, 0x1c);
                    ValidateArc(node, arc);
                    //DIRECT2B7.V58=05A3F0 returns0. Fresh292 resets8FC0/count0
                    //at finalV80, then233FC0 fetches this same empty innerV58.
                    //Both owned routes publish209095's original fallback.
                    candidates.Add(new(ordinal, alternative, I32(pools[node], at), I32(pools[node], at + 4),
                        U64(row, 0), U64(pools[node], at + 0x10), I32(row, 0x34),
                        owner.SourceGraph.GetNode(node)[arc].EndContextId));
                }
            }
            Sort(candidates, owner.Contexts, cancellationToken);
            int groupStart = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var candidate = candidates[i];
                if (i == 0 || candidate.GroupKey != candidates[i - 1].GroupKey || candidate.EndContext != candidates[i - 1].EndContext)
                    groupStart = i;
                bool forced = !reachable.HasLanguageContinuation(node, candidate.Ordinal, cancellationToken);
                for (int prior = i - 1; prior >= groupStart; prior--)
                    if (candidates[prior].Forced && candidates[prior].Context == candidate.Context) forced = false;
                candidate.Forced = forced;
                int cost = unchecked(candidate.Cost + (forced ? forcedPenalty : 0));
                int at = (candidate.Ordinal * 2 + candidate.Alternative) * 40;
                var entry = pools[node].AsSpan(at, 40);
                int previousNode = unchecked(node - (sbyte)entry[0x1a] - 1);
                if (previousNode >= node)
                    throw new NotSupportedException("Same-node primary alternative context publication remains separate.");
                int previousOrdinal = U16(entry, 0x18), previousAlternative = entry[0x1b] & 0x7f;
                bool reset = (entry[0x1f] & 0x20) != 0;
                if ((entry[0x1f] & 0x80) != 0)
                    throw new NotSupportedException("Primary forced-boundary marker publication remains separate.");
                ulong previousContext = previousNode < 0 || reset ? 0 :
                    U64(Alternative(previousNode, previousOrdinal, previousAlternative), 0x10);
                byte difference = (byte)(entry[0x1f] & 0x1f);
                if (!reset && previousNode >= 0 && previousOrdinal >= primary)
                    difference = unchecked((byte)(difference - (Row(previousNode, previousOrdinal)[0x21] & 0x3f)));
                var row = rows[node].AsSpan(candidate.Ordinal * 88, 88);
                var frame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(owner.Contexts, candidate.Context,
                    previousContext, row, (uint)node, difference, policy, Category, cancellationToken).CopyFrame();
                int parent = previousNode < 0 || reset ? -1 : CompactParent(previousNode,
                    U16(Alternative(previousNode, previousOrdinal, previousAlternative), 0x1c));
                int terminalIndex = AppendFrame(frame, row, candidate.Tag, parent, cost, candidate.Increment,
                    ushort.MaxValue, -1, false, forced, ScoreByte(node, row), node);
                BinaryPrimitives.WriteUInt16LittleEndian(entry[0x1c..], Relative(node, terminalIndex));
                usedArcs[U16(row, 0x1c)] = true;
            }

            //2404=1 selects the ordinary beam publication after primary pools.
            for (int ordinal = 0; ordinal < snapshots[node].SourceEnd23f0; ordinal++)
            {
                var row = rows[node].AsSpan(ordinal * 88, 88);
                if ((row[6] & 2) == 0) continue;
                cancellationToken.ThrowIfCancellationRequested();
                if ((row[7] & 1) != 0) throw new NotSupportedException("Associated-source reconstruction remains separate.");
                int arc = U16(row, 0x1c); ValidateArc(node, arc); usedArcs[arc] = true;
                int previousNode = unchecked(node - (sbyte)row[0x20] - 1);
                int previousOrdinal = U16(row, 0x1e);
                ushort alternative = ushort.MaxValue;
                if (ordinal >= primary)
                {
                    if (U16(row, 0x3e) != ushort.MaxValue || I32(row, 0x50) != -1)
                        throw new NotSupportedException("Prior cached-word alternative/auxiliary lineage remains separate.");
                    alternative = 0xfffe;
                }
                int parent = previousNode < 0 ? -1 : CompactParent(previousNode, U16(Row(previousNode, previousOrdinal), 0x22));
                int terminalIndex;
                if (previousNode == node)
                {
                    if (U64(row, 8) != 0 || alternative == ushort.MaxValue)
                        throw new NotSupportedException("Only the owned empty-context cached-word marker is supported at the same node.");
                    EnsureCapacity(1);
                    terminalIndex = count++;
                    var compact = storage.AsSpan(terminalIndex * 40, 40);
                    Write(compact, U64(row, 0), Pack(node, arc), parent, I32(row, 0x10), I32(row, 0x14),
                        ushort.MaxValue, alternative, 3, 255, -1);
                }
                else
                {
                    if (previousNode > node) throw new InvalidDataException("Native predecessor cannot advance beyond its current source node.");
                    byte difference = (byte)(row[0x21] & 0x3f);
                    if (previousNode >= 0 && previousOrdinal >= primary)
                        difference = unchecked((byte)(difference - (Row(previousNode, previousOrdinal)[0x21] & 0x3f)));
                    var frame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(owner.Contexts, U64(row, 8), 0,
                        row, (uint)node, difference, policy, Category, cancellationToken).CopyFrame();
                    terminalIndex = AppendFrame(frame, row, U64(row, 0), parent, I32(row, 0x10), I32(row, 0x14),
                        alternative, -1, false, false, ScoreByte(node, row), node);
                }
                BinaryPrimitives.WriteUInt16LittleEndian(row[0x22..], Relative(node, terminalIndex));
            }

            //The native test is bitmap!=0: these are USED source arcs, not an
            //invented repair path for missing/unused arcs. Header2400=2 adds
            //up to two distinct mapped candidate labels after their beam rows.
            for (int arc = 0; arc < usedArcs.Length; arc++)
            {
                if (!usedArcs[arc]) continue;
                var current = Score(node, arc);
                var emitted = new Dictionary<ushort, int>();
                foreach (var candidate in current.Candidates.Span)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ushort symbol = candidate.Symbol;
                    if (symbol == ushort.MaxValue) break;
                    if (reserved.Contains(symbol)) continue;
                    int delta = QuantizedScore(current.ReadNormalizedCandidateWeight(symbol, cancellationToken));
                    if (emitted.TryGetValue(symbol, out int existing))
                    {
                        int combined = storage[existing * 40 + 0x21] + delta;
                        storage[existing * 40 + 0x21] = (byte)Math.Clamp(combined, 1, 255);
                    }
                    else
                    {
                        EnsureCapacity(1);
                        int index = count++;
                        Write(storage.AsSpan(index * 40, 40), ulong.MaxValue, Pack(node, arc), -1, 0, 0,
                            symbol, ushort.MaxValue, 0x20, (byte)Math.Clamp(delta, 1, 255), -1);
                        emitted.Add(symbol, index);
                        if (emitted.Count == configuration.HeaderCount2400) break;
                    }
                }
            }
        }
        nodeStarts[source.EndingNode + 1] = count;
        prefixOwners[source.EndingNode + 1] = source.EndingNode;
        source.Validate(cancellationToken);
        for (int node = 0; node < snapshots.Length; node++)
            snapshots[node] = snapshots[node] with { Records = rows[node], PrimaryAlternatives = pools[node] };
        state.Commit(source, storage, count, nodeStarts, prefixOwners, cancellationToken);
        return new(reachable, snapshots);

        ushort Label(string text) => alphabet.TryGetLabel(text, out ushort value) ? value : ushort.MaxValue;
        byte Category(ushort symbol) => unchecked((byte)categories.GetSymbolCompositeCategory(symbol));
        ReadOnlySpan<byte> Row(int node, int ordinal)
        {
            if ((uint)node >= rows.Length || (uint)ordinal >= snapshots[node].SourceEnd23f0)
                throw new InvalidDataException("Materialization predecessor must name an actual source row.");
            return rows[node].AsSpan(ordinal * 88, 88);
        }
        ReadOnlySpan<byte> Alternative(int node, int ordinal, int alternative)
        {
            if ((uint)node >= rows.Length || (uint)ordinal >= primary || (uint)alternative >= 2)
                throw new NotSupportedException("Actual primary-pool predecessor required; secondary alternatives remain separate.");
            return pools[node].AsSpan((ordinal * 2 + alternative) * 40, 40);
        }
        int CompactParent(int node, ushort relative)
        {
            if (relative == ushort.MaxValue) return -1;
            int index = checked(nodeStarts[node] + relative);
            if ((uint)index >= count) throw new InvalidDataException("A compact predecessor must have been published already.");
            return index;
        }
        ushort Relative(int node, int index) => checked((ushort)(index - nodeStarts[node]));
        void ValidateArc(int node, int arc)
        {
            if ((uint)arc >= owner.SourceGraph.GetNode(node).Count)
                throw new InvalidDataException("Native record arc must belong to its actual source graph node.");
        }
        void EnsureCapacity(int additional)
        {
            if (count > storage.Length / 40 - additional)
                throw new InvalidOperationException("Explicit compact allocator storage budget exhausted.");
        }
        LegacyOwnedRussianMainGlyphConfidenceGroup Score(int node, int arc)
        {
            int group = owner.SourceGraph.GetNode(node)[arc].GroupId;
            if (!scores.TryGetValue(group, out var current)) scores.Add(group, current = confidence.ScoreGroup(group, cancellationToken));
            return current;
        }
        int QuantizedScore(float value)
        {
            if (!float.IsFinite(value) || value < 0 || value > 1)
                throw new InvalidDataException("Owned normalized candidate weight must be finite and bounded.");
            return (int)(value * 255f); //207CB0 MULSS then CVTTSS2SI.
        }
        byte ScoreByte(int node, ReadOnlySpan<byte> row)
        {
            ushort symbol = U16(row, 0x24);
            if (symbol == ushort.MaxValue) return 255;
            if (U16(row, 0x18) != node && (sbyte)row[5] < 0) symbol = checked((ushort)alphabet.LabelCount);
            return (byte)Math.Clamp(QuantizedScore(Score(node, U16(row, 0x1c)).ReadNormalizedCandidateWeight(symbol, cancellationToken)), 1, 255);
        }
        int AppendFrame(ReadOnlySpan<byte> frame, ReadOnlySpan<byte> row, ulong tag, int parent, int cost,
            int increment, ushort alternative, int auxiliary, bool bit3, bool bit4, byte scoreByte, int node)
        {
            int length = I32(frame, 0);
            if (length is < 1 or > 50) throw new NotSupportedException("Native206D30 nonempty bounded frame prerequisite required.");
            EnsureCapacity(length);
            var symbols = new LegacyPreparedCompactWordSymbol[length];
            for (int i = 0; i < length; i++)
            {
                int coordinate = I32(frame, 8 + i * 12);
                if ((uint)coordinate >= I32(frame, 0x25c)) throw new InvalidDataException("Normalized symbol must own a source coordinate.");
                symbols[i] = new(U16(frame, 4 + i * 12), frame[6 + i * 12], U64(frame, 0x260 + coordinate * 16));
            }
            var options = new LegacyPreparedCompactWordOptions(tag, parent, cost, increment, alternative,
                auxiliary, false, bit3, bit4, scoreByte, policy.Special251c, policy.Special251e,
                shadda, closed, policy.KnowledgeSymbol0e, (uint)node);
            int[] indices = new int[length];
            var appended = LegacyPreparedCompactWordRecords.Append(storage, count, symbols, row, options, Category, indices, cancellationToken);
            count = appended.RecordCount;
            return appended.TerminalRecord;
        }
    }

    public byte[] CopyCompactStorage() => (byte[])storage.Clone();
    public int[] CopyNodeStarts2478() => (int[])starts.Clone();
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {
        if ((uint)node >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        var row = nodes[node];
        return row with { Records = row.Records.ToArray(), PrimaryAlternatives = row.PrimaryAlternatives.ToArray() };
    }

    private sealed class Candidate(ushort ordinal, int alternative, int cost, int increment,
        ulong tag, ulong context, int groupKey, uint endContext)
    {
        public ushort Ordinal { get; } = ordinal;
        public int Alternative { get; } = alternative;
        public int Cost { get; } = cost;
        public int Increment { get; } = increment;
        public ulong Tag { get; } = tag;
        public ulong Context { get; } = context;
        public int GroupKey { get; } = groupKey;
        public uint EndContext { get; } = endContext;
        public bool Forced { get; set; }
    }

    private static void Sort(List<Candidate> rows, LegacySymbolContextFactory contexts, CancellationToken token)
    {
        if (rows.Count == 0) return;
        int Compare(Candidate left, Candidate right)
        {
            int value = unchecked(left.GroupKey - right.GroupKey);
            if (value == 0) value = unchecked((int)left.EndContext - (int)right.EndContext);
            if (value == 0) value = unchecked(left.Cost - right.Cost);
            if (value != 0) return value;
            ushort a = left.Context == 0 ? ushort.MaxValue : contexts.ReadNode(left.Context).Symbol;
            ushort b = right.Context == 0 ? ushort.MaxValue : contexts.ReadNode(right.Context).Symbol;
            return a - b;
        }
        void QuickSort(int first, int last)
        {
            do
            {
                token.ThrowIfCancellationRequested();
                int left = first, right = last;
                Candidate pivot = rows[(first + last) / 2];
                do
                {
                    while (Compare(rows[left], pivot) < 0) left++;
                    while (Compare(rows[right], pivot) > 0) right--;
                    if (right < left) break;
                    (rows[left], rows[right]) = (rows[right], rows[left]); left++; right--;
                } while (left <= right);
                if (first < right) QuickSort(first, right);
                if (last <= left) return;
                first = left;
            } while (true);
        }
        QuickSort(0, rows.Count - 1);
    }

    private static void Write(Span<byte> row, ulong tag, ulong coordinates, int parent, int cost, int increment,
        ushort symbol, ushort alternative, byte flags, byte score, int auxiliary)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(row, tag); BinaryPrimitives.WriteUInt64LittleEndian(row[8..], coordinates);
        BinaryPrimitives.WriteInt32LittleEndian(row[0x10..], parent); BinaryPrimitives.WriteInt32LittleEndian(row[0x14..], cost);
        BinaryPrimitives.WriteInt32LittleEndian(row[0x18..], increment); BinaryPrimitives.WriteUInt16LittleEndian(row[0x1c..], symbol);
        BinaryPrimitives.WriteUInt16LittleEndian(row[0x1e..], alternative); row[0x20] = flags; row[0x21] = score;
        BinaryPrimitives.WriteInt32LittleEndian(row[0x24..], auxiliary);
    }
    private static ulong Pack(int node, int arc) => ((uint)node & 0xffffffUL) | (ulong)(ushort)arc << 24;
    private static ushort U16(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static ulong U64(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[at..]);
}
