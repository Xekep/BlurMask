using System.Buffers.Binary;

namespace InkRecognition;

//Shared numeric207CB0 expansion. Typed adapters provide the exact source graph,
//post-V80 reachability, model policies and cached candidate-weight owner.
internal static class LegacyPreparedMainCompactPublication
{
    internal sealed record Result(byte[] Storage,int Count,int[] Starts,int[] PrefixOwners,LegacyMainDirectWordNodeResult[] Nodes);
    internal static Result Run(LegacyNativeWordControllerConfiguration configuration,LegacyOrdinaryRecognitionGraph graph,
        LegacySymbolContextFactory contexts,LegacyInkAlphabet alphabet,LegacyCursiveCategoryProfile categoryProfile,
        bool contextSentinels,int languagePenalty,bool configuredSecondary,int startingNode,int endingNode,
        LegacyMainDirectWordNodeResult[] markedNodes,Func<int,ushort,CancellationToken,bool> hasLanguageContinuation,
        Func<int,LegacyMainCompactGroupScores> scoreGroup,ReadOnlySpan<byte> initialStorage,int initialCount,
        ReadOnlySpan<int> initialStarts,ReadOnlySpan<int> initialPrefixOwners,Action<CancellationToken> validate,
        CancellationToken cancellationToken,ReadOnlySpan<uint> actualSourceHeaders)
    {
        int primary = configuration.PrimaryArcLimit230a;
                var categories = new LegacyCursiveCategoryMapper(categoryProfile);
        var policy = LegacyPreparedCompactWordContextPolicy.FromAlphabet(alphabet, false, false,
            contextSentinels);
        ushort specialMarker = Label("\u0003");
        ushort shadda = Label("\u0651"), closed = Label("\U000f0001");
        byte[] header = alphabet.CopySerializedHeader();
        ushort[] reserved = [checked((ushort)alphabet.LabelCount), U16(header, 8), U16(header, 10), U16(header, 12)];
        var snapshots = markedNodes;
        var rows = snapshots.Select(s => s.Records.ToArray()).ToArray();
        var pools = snapshots.Select(s => s.PrimaryAlternatives.ToArray()).ToArray();
        var secondary = snapshots.Select(s => s.SecondaryAlternatives.ToArray()).ToArray();
        byte[] storage = initialStorage.ToArray();
        var nodeStarts = initialStarts.ToArray();
        var prefixOwners = initialPrefixOwners.ToArray();
        var scores = new Dictionary<int, LegacyMainCompactGroupScores>();
        int count = initialCount;
        float penalty = languagePenalty * .8f;
        int forcedPenalty = (int)(penalty <= 0 ? penalty - .5f : penalty + .5f);

        for (int node = startingNode; node <= endingNode; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            nodeStarts[node] = count;
            prefixOwners[node] = startingNode - 1;
            var usedArcs = new bool[graph.GetNode(node).Count];
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
                    //DIRECT2B7.V58=05A3F0 returns0. Fresh292's preceding V80
                    //also resets its separate V58 cache; normal233090 query
                    //slots never fill8FC0/8FC4. Thus LIVE and final233FC0 fetch
                    //the same empty innerV58 and take209095's real fallback.
                    candidates.Add(new(ordinal, alternative, I32(pools[node], at), I32(pools[node], at + 4),
                        U64(row, 0), U64(pools[node], at + 0x10), I32(row, 0x34),
                        graph.GetNode(node)[arc].EndContextId));
                }
            }
            Sort(candidates, contexts, cancellationToken);
            int groupStart = 0, selectedDistinct = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var candidate = candidates[i];
                if (i == 0 || candidate.GroupKey != candidates[i - 1].GroupKey || candidate.EndContext != candidates[i - 1].EndContext)
                {groupStart = i;selectedDistinct = 0;}
                var selectedFlags=LegacyPreparedMainPrimaryCompactFlags.Select(candidates,i,groupStart,selectedDistinct,
                    configuration.PrimaryAlternativeCount2402,actualSourceHeaders,
                    ()=>hasLanguageContinuation(node,candidate.Ordinal,cancellationToken),cancellationToken);
                selectedDistinct=selectedFlags.SelectedDistinctAfter;
                candidate.TerminalBit3=selectedFlags.TerminalBit3;
                bool forced=selectedFlags.TerminalBit4;
                candidate.Forced = forced;
                //207CB0 applies the forced penalty only for a nonzero source header low classification.
                bool applyForcedPenalty = forced && (actualSourceHeaders[(byte)(candidate.Tag >> 32)] & 0x1c) != 0;
                int cost = unchecked(candidate.Cost + (applyForcedPenalty ? forcedPenalty : 0));
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
                var frame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(contexts, candidate.Context,
                    previousContext, row, (uint)node, difference, policy, Category, cancellationToken).CopyFrame();
                int parent = previousNode < 0 || reset ? -1 : CompactParent(previousNode,
                    U16(Alternative(previousNode, previousOrdinal, previousAlternative), 0x1c));
                int terminalIndex = AppendFrame(frame, row, candidate.Tag, parent, cost, candidate.Increment,
                    ushort.MaxValue, candidate.Auxiliary, candidate.TerminalBit3, forced, ScoreByte(node, row), node);
                BinaryPrimitives.WriteUInt16LittleEndian(entry[0x1c..], Relative(node, terminalIndex));
                usedArcs[U16(row, 0x1c)] = true;
            }

            //Native207CB0 publishes marked2410 slots before EACH matching88 row;
            //the88 path remains active for configured2, too.
            for (int ordinal = 0; ordinal < snapshots[node].SourceEnd23f0; ordinal++)
            {
                if (configuredSecondary) PublishSecondary(node, ordinal, usedArcs);
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
                    var frame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(contexts, U64(row, 8), 0,
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
                foreach (ushort symbol in current.CandidateSymbols.Span)
                {
                    cancellationToken.ThrowIfCancellationRequested();
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
        nodeStarts[endingNode + 1] = count;
        prefixOwners[endingNode + 1] = endingNode;
        validate(cancellationToken);
        for (int node = 0; node < snapshots.Length; node++)
            snapshots[node] = snapshots[node] with { Records = rows[node], PrimaryAlternatives = pools[node], SecondaryAlternatives = secondary[node] };
        return new(storage,count,nodeStarts,prefixOwners,snapshots);

        void PublishSecondary(int node, int ordinal, bool[] usedArcs)
        {
            for (int k = 0; k < 2; k++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = secondary[node].AsSpan((ordinal * 2 + k) * 40, 40);
                if ((sbyte)entry[0x1b] >= 0) continue;
                var row = rows[node].AsSpan(ordinal * 88, 88);
                if ((row[7] & 1) != 0)
                    throw new NotSupportedException("Associated configured secondary publication remains separate.");
                int arc = U16(row, 0x1c); ValidateArc(node, arc); usedArcs[arc] = true;
                int previousNode = unchecked(node - (sbyte)entry[0x1a] - 1);
                int previousOrdinal = U16(entry, 0x18), previousAlternative = entry[0x1b] & 0x7f;
                if (previousNode > node)
                    throw new InvalidDataException("Native secondary predecessor cannot point beyond its current node.");
                ushort alternative = ushort.MaxValue;
                if (ordinal >= primary)
                {
                    if (U16(entry, 0x20) != ushort.MaxValue || I32(row, 0x50) != -1)
                        throw new NotSupportedException("Anchored/auxiliary configured secondary publication remains separate.");
                    alternative = 0xfffe;
                }
                ReadOnlySpan<byte> parentEntry = previousNode < 0 ? default : Secondary(previousNode, previousOrdinal, previousAlternative);
                int parent = previousNode < 0 ? -1 : CompactParent(previousNode, U16(parentEntry, 0x1c));
                ulong currentContext = U64(entry, 0x10), previousContext = previousNode < 0 ? 0 : U64(parentEntry, 0x10);
                int terminalIndex;
                if (previousNode == node)
                {
                    //20A2xx counts the literal raw context chain up to the
                    //actual predecessor context; control tokens are not mapped away.
                    var symbols = new List<ushort>();
                    ulong context = currentContext;
                    while (context != previousContext)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (context == 0 || symbols.Count == 65536)
                            throw new InvalidDataException("Secondary current context must descend from its exact predecessor context.");
                        var item = contexts.ReadNode(context);
                        symbols.Add(item.Symbol); context = item.ParentLink;
                    }
                    if (symbols.Count == 0)
                    {
                        terminalIndex = parent;
                        if (alternative != ushort.MaxValue)
                        {
                            EnsureCapacity(1); terminalIndex = count++;
                            Write(storage.AsSpan(terminalIndex * 40, 40), U64(row, 0), Pack(node, arc), parent,
                                I32(entry, 0), I32(entry, 4), ushort.MaxValue, alternative, 5, 255, -1);
                        }
                    }
                    else
                    {
                        symbols.Reverse(); terminalIndex = parent;
                        for (int index = 0; index < symbols.Count; index++)
                        {
                            ushort symbol = symbols[index];
                            byte type = symbol > 0xffea && symbol < 0xfffe || symbol == policy.Special24de ? (byte)5 : (byte)6;
                            EnsureCapacity(1); int current = count++;
                            Write(storage.AsSpan(current * 40, 40), U64(row, 0), Pack(node, arc), terminalIndex,
                                I32(entry, 0), I32(entry, 4), symbol, index == 0 ? alternative : ushort.MaxValue,
                                type, ScoreByte(node, row), -1);
                            terminalIndex = current;
                        }
                    }
                }
                else
                {
                    byte difference = (byte)(entry[0x1f] & 0x1f);
                    if (previousOrdinal >= primary)
                        difference = unchecked((byte)(difference - (previousNode < 0 ? 0 : parentEntry[0x1f] & 0x1f)));
                    var frame = LegacyPreparedCompactWordFrameProducer.CreateOrdinary(contexts, currentContext,
                        previousContext, row, (uint)node, difference, policy, Category, cancellationToken).CopyFrame();
                    terminalIndex = AppendFrame(frame, row, U64(row, 0), parent, I32(entry, 0), I32(entry, 4),
                        alternative, -1, false, false, ScoreByte(node, row), node);
                }
                BinaryPrimitives.WriteUInt16LittleEndian(entry[0x1c..], terminalIndex < 0 ? ushort.MaxValue : Relative(node, terminalIndex));
            }
        }
        ReadOnlySpan<byte> Secondary(int node, int ordinal, int alternative)
        {
            if ((uint)node >= rows.Length || (uint)ordinal >= snapshots[node].SourceEnd23f0 || (uint)alternative >= 2)
                throw new InvalidDataException("Configured predecessor must name an actual owned2410 slot.");
            return secondary[node].AsSpan((ordinal * 2 + alternative) * 40, 40);
        }
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
            if ((uint)arc >= graph.GetNode(node).Count)
                throw new InvalidDataException("Native record arc must belong to its actual source graph node.");
        }
        void EnsureCapacity(int additional)
        {
            if (count > storage.Length / 40 - additional)
                throw new InvalidOperationException("Explicit compact allocator storage budget exhausted.");
        }
        LegacyMainCompactGroupScores Score(int node, int arc)
        {
            int group = graph.GetNode(node)[arc].GroupId;
            if (!scores.TryGetValue(group, out var current)) scores.Add(group, current = scoreGroup(group));
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

    private sealed class Candidate(ushort ordinal, int alternative, int cost, int increment,
        ulong tag, ulong context, int groupKey, uint endContext) : ILegacyMainPrimaryCompactFlagRow
    {
        public ushort Ordinal { get; } = ordinal;
        public int Alternative { get; } = alternative;
        public int Cost { get; } = cost;
        public int Increment { get; } = increment;
        public ulong Tag { get; } = tag;
        public ulong Context { get; } = context;
        public int GroupKey { get; } = groupKey;
        public uint EndContext { get; } = endContext;
        //209095 empty-V58 fallback was admitted above only for row30==-1.
        //20907F also tests original tag.bit41: only terminal tags set byte3=1.
        //Byte4=0; other expansion producers remain refused.
        public bool FallbackByte3 => (Tag & (1UL << 41)) != 0;
        public int Auxiliary => -1;
        public bool TerminalBit3 { get; set; }
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
