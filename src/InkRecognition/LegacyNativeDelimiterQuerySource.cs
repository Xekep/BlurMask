using System.Buffers.Binary;

namespace InkRecognition;

public delegate LegacyAggregateDictionaryQueryPage LegacyNativeDelimiterBackend(
    ulong sourceTag, LegacyPreparedDictionaryQuery query, CancellationToken cancellationToken);

public sealed class LegacyNativeDelimiterQuery
{
    public required LegacyPreparedDictionaryQuery Request { get; init; }
    public ulong SourceTag { get; set; }
    internal LegacyNativeDelimiterQuerySource? Owner;
    internal int SlotIndex;
}

public sealed record LegacyNativeDelimiterQueryPage(int ReturnedCount, ReadOnlyMemory<byte> Records,
    int SlotIndex, int AcceptedCount, int BackendCalls);

/// <summary>Ordinary no-extra-cost290.V40/2382B0 and237610 protocol traversal.
/// The reader owns the aggregate below290 (including per-child case wrappers).
/// Custom49F58 callbacks and extra-cost side tables remain separate policies.</summary>
public sealed class LegacyNativeDelimiterQuerySource
{
    private readonly LegacyNativeDelimiterBackend reader;
    private readonly LegacySymbolContextFactory contexts;
    private readonly ushort count, zwnj, zwj;
    private readonly ulong initial, root1, root2, root34, ordinary;
    private readonly Slot[] slots = Enumerable.Range(0, 8).Select(_ => new Slot()).ToArray();
    private int nextSlot, active;
    private bool valid = true;

    /// <summary>1D4950 stores the signed coordinate-mode byte at247+80;
    ///1D60C0 supplies that value to237380. This reads the same bounded v10d
    ///parent134/133 prefix, not the distinct135/136/137 source-kind field.</summary>
    public static int ReadV10dCoordinateMode(ReadOnlySpan<byte> resource, int selectedSourceOffset = 0)
    {
        _ = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource, selectedSourceOffset);
        int at = checked(selectedSourceOffset + 20 + 4 + 56);
        int marker = BinaryPrimitives.ReadInt32LittleEndian(resource[at..]); at += 4;
        if (marker == -1) at += 4;
        at += 4;
        if (resource[at++] != 0)
        {
            at += 4;
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(resource[at..]); at += 2;
            at += LegacyInkAlphabet.Load(resource[at..], version).BytesConsumed;
        }
        at += 4; //The preceding int32 is source78, NOT coordinate mode80.
        int mode = unchecked((sbyte)resource[at]);
        if (mode is < 0 or > 2) throw new NotSupportedException("Confirmed v10d coordinate mode0..2 required.");
        return mode;
    }

    private sealed class Slot
    {
        public ulong Tag = ulong.MaxValue;
        public int Accepted;
        public uint Parameter1c = uint.MaxValue, Parameter20 = uint.MaxValue;
        public ushort Flags24 = ushort.MaxValue, Flags26 = ushort.MaxValue;
        public readonly Level[] Levels = Enumerable.Range(0, 8).Select(_ => new Level()).ToArray();
    }

    private sealed class Level
    {
        public ulong Tag = ulong.MaxValue;
        public LegacyPreparedDictionaryQuery Query = new() { Capacity = 128 };
        public byte[] Records = [];
        public int Cursor, Count;
    }

    public LegacyNativeDelimiterQuerySource(LegacyInkAlphabet alphabet, LegacySymbolContextFactory contexts,
        LegacyNativeDelimiterBackend reader, int sourceCoordinateMode, bool extraCostSideTable = false)
    {
        ArgumentNullException.ThrowIfNull(alphabet); ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(reader);
        if (sourceCoordinateMode is < 0 or > 4 || alphabet.LabelCount > ushort.MaxValue)
            throw new ArgumentException("Actual source247.V40 coordinate mode and bounded alphabet required.");
        if (extraCostSideTable)
            throw new NotSupportedException("290 extra-cost and custom callback branches require their owning producers.");
        this.contexts = contexts; this.reader = reader; count = checked((ushort)alphabet.LabelCount);
        zwnj = alphabet.TryGetLabel("\u200c", out ushort first) ? first : ushort.MaxValue;
        zwj = alphabet.TryGetLabel("\u200d", out ushort second) ? second : ushort.MaxValue;
        //237380: constructor2E=FFFF chooses FFFA for modes3/4, otherwiseFFFB.
        initial = contexts.ResolveInitial(sourceCoordinateMode is 3 or 4 ? (ushort)0xfffa : (ushort)0xfffb).Identity;
        root1 = contexts.ResolveInitial(0xfff9).Identity;
        root2 = contexts.ResolveInitial(0xfffb).Identity;
        root34 = contexts.ResolveInitial(0xfffc).Identity;
        ordinary = contexts.ResolveInitial(0xfffa).Identity;
    }

    /// <summary>2372F0 ->237080 ->236F30 normal-cache portion of sourceV80.
    ///The owning controller separately forwards the same epoch to its backend.
    ///This is never an implicit response to a permission-map refresh.</summary>
    public void ResetForControllerNode(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native delimiter reset must be serialized with reads.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed delimiter preparation cannot be revived by a node reset.");
            foreach (var slot in slots)
            {
                slot.Tag = ulong.MaxValue; slot.Accepted = -1;
                slot.Parameter1c = slot.Parameter20 = uint.MaxValue;
                slot.Flags24 = slot.Flags26 = ushort.MaxValue;
                foreach (var level in slot.Levels)
                {
                    level.Tag = ulong.MaxValue; level.Cursor = level.Count = 0;
                    //236F30 zeros query40/48 backend-owner/cache pointers.
                    level.Query = new() { Capacity = 128 };
                }
            }
            nextSlot = 0;
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    public LegacyNativeDelimiterQueryPage Read(LegacyNativeDelimiterQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query); ArgumentNullException.ThrowIfNull(query.Request);
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native delimiter queries must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed delimiter preparation requires a fresh source.");
            var q = query.Request;
            if ((uint)q.Node != (uint)query.SourceTag || q.Skip is < 0 or > 2_000_000 || q.Capacity is < 1 or > 512 ||
                q.ContextRoot != 0 && q.ContextRoot != contexts.ContainerIdentity)
                throw new ArgumentException("Actual source tag/node, same context owner and bounded query required.");
            if (!ReferenceEquals(query.Owner, this))
            {
                query.Owner = this; query.SlotIndex = nextSlot; nextSlot = (nextSlot + 1) & 7;
                slots[query.SlotIndex].Tag = ulong.MaxValue;
            }
            var slot = slots[query.SlotIndex];
            bool parameterChange = slot.Parameter1c != q.Parameter1c || slot.Parameter20 != q.Parameter20 ||
                slot.Flags24 != q.Flags24 || slot.Flags26 != q.Flags26;
            if (slot.Tag != query.SourceTag || slot.Accepted != q.Skip || parameterChange)
            {
                slot.Tag = query.SourceTag; slot.Accepted = 0;
                slot.Levels[0].Tag = ulong.MaxValue;
                slot.Levels[0].Query.Category18 = q.Category18;
                slot.Levels[0].Query.Parameter19 = q.Parameter19;
            }
            if (parameterChange)
            {
                slot.Parameter1c = q.Parameter1c; slot.Parameter20 = q.Parameter20;
                slot.Flags24 = q.Flags24; slot.Flags26 = q.Flags26;
                foreach (var level in slot.Levels)
                {
                    level.Query.Parameter1c = q.Parameter1c; level.Query.Parameter20 = q.Parameter20;
                    level.Query.Flags24 = q.Flags24; level.Query.Flags26 = q.Flags26;
                }
            }
            byte[] output = new byte[q.Capacity * 32];
            int written = 0, backendCalls = 0;
            if (!Traverse(0, query.SourceTag, q.ParentContext, 0, 0, 0, 0))
            { valid = false; return new(-1, ReadOnlyMemory<byte>.Empty, query.SlotIndex, slot.Accepted, backendCalls); }

            //2382B0 prefixes literal source controls after traversal. This is
            //236E90 concatenation, not1F7FF0's control-normalizing composition.
            if ((query.SourceTag & (8UL << 40)) != 0 &&
                (unchecked((int)q.Parameter20) < unchecked((int)q.Parameter1c) || (q.Flags24 & 0x81) == 1))
            {
                if ((uint)query.SourceTag == 0)
                    for (int i = 0; i < written; i++)
                    {
                        var row = output.AsSpan(i * 32, 32);
                        //238510 applies the OUTER tag's kind only in this
                        //low-node-zero branch; it preserves the row's bit3.
                        row[23] = Carry(row[23], (byte)(query.SourceTag >> 44 & 7), (byte)(row[23] >> 3 & 1));
                        int kind = row[23] & 7;
                        ulong prefix = kind switch { 0 => initial, 1 => root1, 2 => root2, 3 or 4 => root34, _ => 0 };
                        if (kind == 0 && contexts.ReadNode(initial).Symbol == 0xfffb) row[23] = (byte)(row[23] & 0xf8 | 2);
                        if (prefix != 0) Prefix(row, prefix);
                    }
                else if ((query.SourceTag & (4UL << 40)) == 0)
                    for (int i = 0; i < written; i++) Prefix(output.AsSpan(i * 32, 32), ordinary, allowExtra: false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(written, output.AsMemory(0, written * 32).ToArray(), query.SlotIndex, slot.Accepted, backendCalls);

            bool Traverse(int depth, ulong tag, ulong parentContext, byte carriedLength, int carriedCost, byte carriedKind, byte carriedFlag)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int ancestor = 0; ancestor < depth; ancestor++)
                    if (slot.Levels[ancestor].Tag == tag) return true;
                var level = slot.Levels[depth];
                if (level.Tag != tag)
                {
                    level.Tag = tag; level.Query.Node = unchecked((int)(uint)tag);
                    level.Query.ContextRoot = q.ContextRoot; level.Query.ParentContext = parentContext;
                    level.Query.Skip = 0; level.Cursor = 0;
                    level.Query.Flags24 = depth == 0 ? q.Flags24 : (ushort)(q.Flags24 & ~8);
                    if (!Fetch(level)) return false;
                    if (depth < 7)
                    {
                        slot.Levels[depth + 1].Tag = ulong.MaxValue;
                        slot.Levels[depth + 1].Query.Category18 = level.Query.Category18;
                        slot.Levels[depth + 1].Query.Parameter19 = level.Query.Parameter19;
                    }
                }
                while (true)
                {
                    while (level.Cursor < level.Count)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var row = level.Records.AsSpan(level.Cursor * 32, 32);
                        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                        if (symbol != zwj)
                        {
                            bool protocol = symbol >= count && unchecked((ushort)(symbol + 29)) >= 8;
                            row[23] = Carry(row[23], carriedKind, carriedFlag);
                            if (protocol || symbol == zwnj)
                            {
                                bool follow = true;
                                if (protocol)
                                    switch (symbol)
                                    {
                                        case 0xfff6: row[23] |= 8; break;
                                        case 0xfff7 or 0xfff9:
                                            if (q.Parameter1c == q.Parameter20) follow = false;
                                            else row[23] = (byte)(row[23] & 0xf9 | 1);
                                            break;
                                        case 0xfff8: row[23] = (byte)(row[23] & 0xf9 | 1); break;
                                        case 0xfffb: if ((row[23] & 7) != 1) row[23] = (byte)(row[23] & 0xfa | 2); break;
                                        case 0xfffc:
                                            int kind = row[23] & 7;
                                            if (kind != 1 && ((kind - 2) & 0xfd) != 0) row[23] = (byte)(row[23] & 0xfb | 3);
                                            break;
                                    }
                                if (follow && depth < 7)
                                {
                                    ulong nextTag = BinaryPrimitives.ReadUInt64LittleEndian(row);
                                    ulong nextParent = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                                    byte length = unchecked((byte)(carriedLength + row[22]));
                                    int cost = unchecked(carriedCost + BinaryPrimitives.ReadInt32LittleEndian(row[16..]));
                                    byte kind = (byte)(row[23] & 7), flag = (byte)(row[23] >> 3 & 1);
                                    if (!Traverse(depth + 1, nextTag, nextParent, length, cost, kind, flag)) return false;
                                    if (written == q.Capacity) return true;
                                }
                            }
                            else
                            {
                                if (written == q.Capacity) return true;
                                if (slot.Accepted >= q.Skip)
                                {
                                    var target = output.AsSpan(written * 32, 32);
                                    row.CopyTo(target);
                                    BinaryPrimitives.WriteInt32LittleEndian(target[16..], unchecked(BinaryPrimitives.ReadInt32LittleEndian(target[16..]) + carriedCost));
                                    target[22] = unchecked((byte)(target[22] + carriedLength));
                                    target[23] = Carry(target[23], carriedKind, carriedFlag);
                                    written++;
                                }
                                slot.Accepted = checked(slot.Accepted + 1);
                            }
                        }
                        level.Cursor++;
                    }
                    if (written == q.Capacity) return true;
                    level.Query.Skip = checked(level.Query.Skip + level.Count);
                    level.Cursor = 0;
                    if (!Fetch(level)) return false;
                    if (level.Count == 0) return true;
                }
            }

            bool Fetch(Level level)
            {
                level.Query.Capacity = 128;
                backendCalls++;
                var page = reader(level.Tag, level.Query, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (page.ReturnedCount is < -1 or > 128 || page.Records.Length != Math.Max(0, page.ReturnedCount) * 32)
                    throw new InvalidDataException("Native delimiter backend must own an exact128-row block.");
                if (!page.ExtraCosts.IsEmpty) throw new NotSupportedException("Unexpected delimiter extra-cost side table.");
                level.Count = page.ReturnedCount;
                level.Records = page.Records.ToArray();
                return level.Count != -1;
            }

            void Prefix(Span<byte> row, ulong prefix, bool allowExtra = true)
            {
                if (allowExtra && (q.Flags24 & 0x100) != 0)
                { BinaryPrimitives.WriteUInt64LittleEndian(row[24..], prefix); return; }
                ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if (suffix != 0 && q.ContextRoot == 0)
                { BinaryPrimitives.WriteUInt64LittleEndian(row[8..], 0); return; }
                var symbols = new List<ushort>();
                for (ulong cursor = suffix; cursor != 0;)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (symbols.Count == 4096) throw new NotSupportedException("Bounded literal delimiter context required.");
                    var node = contexts.ReadNode(cursor); symbols.Add(node.Symbol); cursor = node.ParentLink;
                }
                ulong result = prefix;
                for (int i = symbols.Count - 1; i >= 0; i--)
                {
                    var resolved = contexts.ResolveChild(result, symbols[i], cancellationToken);
                    if (resolved.Created) contexts.LinkCreatedContext(resolved.Identity, result);
                    result = resolved.Identity;
                }
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], result);
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
            4 => unchecked((byte)(current - 1)) > 1 ? 4 : current,
            _ => current
        };
        return (byte)((value & 0xf0) | current | (flag << 3));
    }
}
