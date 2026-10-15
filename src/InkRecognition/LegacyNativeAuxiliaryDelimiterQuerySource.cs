using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Distinct290.V48/2392E0 ->239100 ->2388E0 no-extra-cost auxiliary
///traversal. The one owned cache is separate from290.V40's eight-slot ring.
///The backend must supply the aggregate's V48 pages and the actual shared
///context owner. Custom side costs remain unsupported.</summary>
public sealed class LegacyNativeAuxiliaryDelimiterQuerySource
{
    private readonly LegacyNativeDelimiterBackend backend;
    private readonly LegacySymbolContextFactory contexts;
    private readonly int nodeCount;
    private readonly ushort alphabetCount, zwnj, zwj;
    private readonly Level[] levels = Enumerable.Range(0, 8).Select(_ => new Level()).ToArray();
    private ulong tag = ulong.MaxValue;
    private int accepted, active;
    private uint parameter1c = uint.MaxValue, parameter20 = uint.MaxValue;
    private ushort flags24 = ushort.MaxValue, flags26 = ushort.MaxValue;
    private bool valid = true;

    private sealed class Level
    {
        public ulong Tag = ulong.MaxValue;
        public LegacyPreparedDictionaryQuery Query = new() { Capacity = 128 };
        public byte[] Records = [];
        public int Cursor, Count;
    }

    public LegacyNativeAuxiliaryDelimiterQuerySource(LegacyInkAlphabet alphabet, LegacySymbolContextFactory contexts,
        LegacyNativeDelimiterBackend backend, int actualFrontendNodeCount, bool extraCostSideTable = false)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(backend);
        if (actualFrontendNodeCount is < 0 or > 2_000_000 || alphabet.LabelCount > ushort.MaxValue)
            throw new ArgumentException("Actual bounded frontend222.V88 graph node count and alphabet required.");
        if (extraCostSideTable) throw new NotSupportedException("Auxiliary290 side-cost producer is separate.");
        this.backend = backend; this.contexts = contexts; nodeCount = actualFrontendNodeCount;
        alphabetCount = checked((ushort)alphabet.LabelCount);
        zwnj = alphabet.TryGetLabel("\u200c", out ushort first) ? first : ushort.MaxValue;
        zwj = alphabet.TryGetLabel("\u200d", out ushort second) ? second : ushort.MaxValue;
    }

    /// <summary>237080/236F30's single auxiliary-cache portion of sourceV80.
    ///Backend epoch forwarding belongs to the same owning controller call.</summary>
    public void ResetForControllerNode(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Auxiliary delimiter reset must be serialized with reads.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed auxiliary preparation cannot be revived by a node reset.");
            tag = ulong.MaxValue; accepted = -1;
            parameter1c = parameter20 = uint.MaxValue;
            flags24 = flags26 = ushort.MaxValue;
            foreach (var level in levels)
            {
                level.Tag = ulong.MaxValue; level.Cursor = level.Count = 0;
                level.Query = new() { Capacity = 128 };
            }
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    public LegacyNativeDelimiterQueryPage Read(ulong sourceTag, LegacyPreparedDictionaryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native auxiliary delimiter queries must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed auxiliary delimiter requires fresh preparation.");
            if (query.Node != unchecked((int)(uint)sourceTag) || query.Skip is < 0 or > 2_000_000 ||
                query.Capacity is < 1 or > 512 || query.ContextRoot != contexts.ContainerIdentity)
                throw new ArgumentException("Owned source tag, context and bounded auxiliary request required.");
            bool changed = parameter1c != query.Parameter1c || parameter20 != query.Parameter20 ||
                flags24 != query.Flags24 || flags26 != query.Flags26;
            if (tag != sourceTag || accepted != query.Skip || changed)
            {
                tag = sourceTag; accepted = 0; levels[0].Tag = ulong.MaxValue;
                levels[0].Query.Category18 = query.Category18;
                levels[0].Query.Parameter19 = query.Parameter19;
            }
            if (changed)
            {
                //2391DB writes the OUTER cache tag to-1 after reset. The
                //next same-query page therefore replays once from skip0.
                tag = ulong.MaxValue;
                parameter1c = query.Parameter1c; parameter20 = query.Parameter20;
                flags24 = query.Flags24; flags26 = query.Flags26;
                foreach (var level in levels)
                {
                    level.Query.Parameter1c = parameter1c; level.Query.Parameter20 = parameter20;
                    level.Query.Flags24 = flags24; level.Query.Flags26 = flags26;
                }
            }
            byte[] output = new byte[query.Capacity * 32];
            int written = 0, calls = 0;
            if (!Traverse(0, sourceTag, query.ParentContext, 0, 0, 0, 0))
            { valid = false; return new(-1, ReadOnlyMemory<byte>.Empty, 0, accepted, calls); }
            cancellationToken.ThrowIfCancellationRequested();
            return new(written, output.AsMemory(0, written * 32).ToArray(), 0, accepted, calls);

            bool Traverse(int depth, ulong currentTag, ulong parent, byte length, int cost, byte kind, byte flag)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int ancestor = 0; ancestor < depth; ancestor++) if (levels[ancestor].Tag == currentTag) return true;
                var level = levels[depth];
                if (level.Tag != currentTag)
                {
                    level.Tag = currentTag; level.Query.Node = unchecked((int)(uint)currentTag);
                    level.Query.ContextRoot = query.ContextRoot; level.Query.ParentContext = parent;
                    level.Query.Skip = 0; level.Cursor = 0;
                    level.Query.Flags24 = depth == 0 ? flags24 : (ushort)(flags24 & ~8);
                    if (!Fetch(level)) return false;
                    if (depth < 7)
                    {
                        levels[depth + 1].Tag = ulong.MaxValue;
                        levels[depth + 1].Query.Category18 = level.Query.Category18;
                        levels[depth + 1].Query.Parameter19 = level.Query.Parameter19;
                    }
                }
                while (true)
                {
                    while (level.Cursor < level.Count)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var row = level.Records.AsSpan(level.Cursor * 32, 32);
                        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                        bool protocol = symbol >= alphabetCount && unchecked((ushort)(symbol + 29)) >= 8;
                        //238AF2 overwrites the low three bits with incoming
                        //kind; it does not use V40's initial Carry operation.
                        row[23] = (byte)(row[23] & 0xf0 | kind | flag << 3);
                        bool handled = false, follow = false;
                        if (protocol)
                        {
                            bool skip = false;
                            switch (symbol)
                            {
                                case 0xfff6: row[23] |= 8; break;
                                case 0xfff8 or 0xfff9:
                                    if (unchecked((int)level.Query.Parameter1c) == nodeCount - 1) { skip = true; break; }
                                    goto case 0xfff7;
                                case 0xfff7: row[23] = (byte)(row[23] & 0xf9 | 1); DropContext(row, required: true); break;
                                case 0xfffb:
                                    if ((row[23] & 7) != 1) row[23] = (byte)(row[23] & 0xfa | 2);
                                    DropContext(row, required: true); break;
                                case 0xfffc:
                                    int current = row[23] & 7;
                                    if (current != 1 && ((current - 2) & 0xfd) != 0) row[23] = (byte)(row[23] & 0xfb | 3);
                                    DropContext(row, required: true); break;
                            }
                            if (skip) { level.Cursor++; continue; }
                            follow = true; handled = true;
                        }
                        else if (symbol == zwnj) { follow = true; handled = true; }
                        else if (symbol == zwj) { DropContext(row, required: false); follow = true; handled = true; }
                        else if ((row[23] & 0x10) != 0)
                        {
                            if ((flags24 & 0x20) == 0) DropContext(row, required: false);
                            handled = true;
                        }
                        if (follow && depth < 7)
                        {
                            if (!Traverse(depth + 1, BinaryPrimitives.ReadUInt64LittleEndian(row),
                                BinaryPrimitives.ReadUInt64LittleEndian(row[8..]), unchecked((byte)(length + row[22])),
                                unchecked(cost + BinaryPrimitives.ReadInt32LittleEndian(row[16..])),
                                (byte)(row[23] & 7), (byte)(row[23] >> 3 & 1))) return false;
                        }
                        if (written == query.Capacity) return true;
                        if (handled && ((row[5] & 2) != 0 || (flags24 & 0x20) != 0))
                        {
                            if (accepted >= query.Skip)
                            {
                                var target = output.AsSpan(written * 32, 32); row.CopyTo(target);
                                byte terminal = symbol != zwj && (row[5] & 2) != 0 ? (byte)2 : (byte)0;
                                target[5] = (byte)((target[5] & 0x79) | terminal | (symbol == zwj ? 4 : 0) | (byte)(currentTag >> 40) & 0x80);
                                BinaryPrimitives.WriteInt32LittleEndian(target[16..], unchecked(BinaryPrimitives.ReadInt32LittleEndian(target[16..]) + cost));
                                target[22] = unchecked((byte)(target[22] + length));
                                target[23] = Carry(target[23], kind, (byte)(target[23] >> 3 & 1 | flag));
                                written++;
                            }
                            accepted = checked(accepted + 1);
                        }
                        level.Cursor++;
                    }
                    if (written == query.Capacity) return true;
                    level.Query.Skip = checked(level.Query.Skip + level.Count); level.Cursor = 0;
                    if (!Fetch(level)) return false;
                    if (level.Count == 0) return true;
                }
            }

            bool Fetch(Level level)
            {
                level.Query.Capacity = 128; calls++;
                var page = backend(level.Tag, level.Query, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (page.ReturnedCount is < -1 or > 128 || page.Records.Length != Math.Max(0, page.ReturnedCount) * 32)
                    throw new InvalidDataException("Auxiliary290 backend must supply complete128-row blocks.");
                if (!page.ExtraCosts.IsEmpty) throw new NotSupportedException("Auxiliary290 side-cost branch requires its producer.");
                level.Count = page.ReturnedCount; level.Records = page.Records.ToArray();
                return level.Count != -1;
            }

            void DropContext(Span<byte> row, bool required)
            {
                ulong identity = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if (identity == 0)
                {
                    if (required) throw new ArgumentException("Native auxiliary protocol edge requires its actual context leaf.");
                    return;
                }
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], contexts.ReadNode(identity).ParentLink);
            }
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private static byte Carry(byte value, byte kind, byte flag)
    {
        int current = value & 7;
        current = kind switch
        {
            1 => 1, 2 => current == 1 ? 1 : 2,
            3 => ((current - 1) & 0xfc) != 0 || current == 3 ? 3 : current,
            4 => unchecked((byte)(current - 1)) > 1 ? 4 : current, _ => current
        };
        return (byte)(value & 0xf0 | current | flag << 3);
    }
}
