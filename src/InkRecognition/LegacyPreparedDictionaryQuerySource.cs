using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Native query fields. Context values are caller-owned identities, never native pointers.</summary>
public sealed class LegacyPreparedDictionaryQuery
{
    public int Node { get; set; }
    public int Skip { get; set; }
    public int Capacity { get; set; } = 1;
    public uint Parameter1c { get; set; }
    public uint Parameter20 { get; set; }
    public ushort Flags24 { get; set; }
    public ushort Flags26 { get; set; }
    public byte Category18 { get; set; }
    public byte Parameter19 { get; set; }
    public ulong ContextRoot { get; set; }
    public ulong ParentContext { get; set; }
    internal LegacyPreparedDictionaryQuerySource? Owner;
    internal int SlotIndex;
}

public readonly record struct LegacyDictionaryQueryContext(ulong Identity, bool Created);
public sealed record LegacyPreparedDictionaryQueryPage(int ReturnedCount,
    ReadOnlyMemory<byte> Records, int SlotIndex, int NextChild, int AcceptedCount,
    ReadOnlyMemory<byte> ClearedExtraCosts);

/// <summary>
/// Text093b50's eight-slot query cache and 256-label block path with091e10/0920a0.
/// Supported backend is the checked type2b4 single-packed labels (wire flags0),
/// including the native root-child skip. Native context allocation remains an
/// explicit callback, and no dictionary state or final word winner is invented.
/// </summary>
public sealed class LegacyPreparedDictionaryQuerySource
{
    private readonly LegacyPackedLexicon lexicon;
    private readonly Slot[] slots = Enumerable.Range(0, 8).Select(_ => new Slot()).ToArray();
    private LegacyPreparedDictionaryWorkingMap map;
    private readonly ulong[] initialContexts;
    private int nextSlot;
    private bool reading;
    private byte[] recordScratch = [];
    public uint DescriptorFlags => map.DescriptorFlags;

    private sealed class Slot
    {
        public int Node = -1, Start, End, BlockCount, BlockCursor, Accepted;
        public uint Parameter1c, Parameter20;
        public ushort Flags24, Flags26;
        public bool Auxiliary;
        public uint[] Labels = new uint[256];
    }

    public LegacyPreparedDictionaryQuerySource(LegacyPackedLexicon lexicon,
        LegacyPreparedDictionaryWorkingMap map, ReadOnlySpan<ulong> initialContexts)
    {
        ArgumentNullException.ThrowIfNull(lexicon); ArgumentNullException.ThrowIfNull(map);
        if (initialContexts.Length != map.LexicalSymbolCount)
            throw new ArgumentException("One explicit initial context identity per lexical symbol is required.");
        this.lexicon = lexicon; this.map = map; this.initialContexts = initialContexts.ToArray();
    }

    /// <summary>Permission refresh changes maps in place in native; query invalidation is a separate operation.</summary>
    public void SetWorkingMap(LegacyPreparedDictionaryWorkingMap value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (reading) throw new InvalidOperationException("Dictionary queries are not reentrant.");
        if (value.LexicalSymbolCount != map.LexicalSymbolCount || value.AcousticSymbolCount != map.AcousticSymbolCount)
            throw new ArgumentException("Prepared alphabet identity must retain its map dimensions.");
        map = value;
    }

    /// <summary>092840 invalidates all eight nodes/counts, including the overlapping zero of2200 ring index.</summary>
    public void ResetQueries()
    {
        if (reading) throw new InvalidOperationException("Dictionary queries are not reentrant.");
        foreach (var slot in slots)
        {
            slot.Node = -1; slot.Parameter1c = slot.Parameter20 = uint.MaxValue;
            slot.BlockCount = slot.BlockCursor = slot.Accepted = 0;
        }
        nextSlot = 0;
    }

    public LegacyPreparedDictionaryQueryPage Read(LegacyPreparedDictionaryQuery query, bool auxiliary,
        Func<ulong, ushort, LegacyDictionaryQueryContext>? resolveContext = null,
        Action<ulong, ulong>? linkCreatedContext = null, bool clearExtraCosts = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query); cancellationToken.ThrowIfCancellationRequested();
        if (reading) throw new InvalidOperationException("Dictionary queries are not reentrant.");
        if ((uint)query.Node >= lexicon.NodeCount || query.Skip is < 0 or > 2_000_000 || query.Capacity is < 1 or > 512)
            throw new ArgumentException("Expected a checked node, nonnegative offset and positive bounded page capacity.");
        //Select also refuses absent auxiliary data before mutating the ring.
        //The map is immutable; no per-page copy or public alias is needed.
        ReadOnlySpan<ushort> selected = map.ReadSelectedMap(auxiliary, (byte)query.Flags24);
        reading = true;
        try
        {
            if (!ReferenceEquals(query.Owner, this))
            {
                query.Owner = this; query.SlotIndex = nextSlot; nextSlot = (nextSlot + 1) & 7;
                slots[query.SlotIndex].Node = -1;
            }
            var slot = slots[query.SlotIndex];
            if (slot.Node != query.Node)
            {
                (slot.Start, slot.End) = lexicon.GetChildRange(query.Node);
                if (query.Node == 0) slot.Start++;
                slot.Node = query.Node; slot.Accepted = -1;
            }
            if (slot.Accepted != query.Skip || slot.Parameter1c != query.Parameter1c || slot.Parameter20 != query.Parameter20 ||
                slot.Flags24 != query.Flags24 || slot.Flags26 != query.Flags26 || slot.Auxiliary != auxiliary)
            {
                (slot.Start, slot.End) = lexicon.GetChildRange(query.Node);
                if (query.Node == 0) slot.Start++;
                slot.BlockCount = slot.BlockCursor = slot.Accepted = 0;
                slot.Parameter1c = query.Parameter1c; slot.Parameter20 = query.Parameter20;
                slot.Flags24 = query.Flags24; slot.Flags26 = query.Flags26; slot.Auxiliary = auxiliary;
            }
            while (NextChild(slot) < slot.End && slot.Accepted < query.Skip)
            {
                cancellationToken.ThrowIfCancellationRequested(); Refill(slot);
                //091e10: count only mapped edges, and only terminals for auxiliary queries.
                while (slot.BlockCursor < slot.BlockCount && slot.Accepted < query.Skip)
                {
                    uint packed = slot.Labels[slot.BlockCursor++];
                    if (Accept(packed, selected, auxiliary)) slot.Accepted++;
                }
            }
            int storageLength = query.Capacity * 32;
            if (recordScratch.Length < storageLength) recordScratch = new byte[storageLength];
            Span<byte> recordStorage = recordScratch.AsSpan(0, storageLength);
            while (NextChild(slot) < slot.End && slot.Accepted < query.Skip + query.Capacity)
            {
                cancellationToken.ThrowIfCancellationRequested(); Refill(slot);
                int cursor = slot.BlockCursor, accepted = slot.Accepted;
                int written = accepted - query.Skip;
                while (cursor < slot.BlockCount && written < query.Capacity)
                {
                    uint packed = slot.Labels[cursor];
                    int node = slot.Start + cursor++;
                    if (!Accept(packed, selected, auxiliary)) continue;
                    int lexical = (int)(packed & lexicon.PackedLabelMask);
                    ushort symbol = selected[lexical];
                    ulong context;
                    if (query.ParentContext == 0) context = initialContexts[lexical];
                    else if (query.ContextRoot == 0) context = 0;
                    else
                    {
                        if (resolveContext is null || linkCreatedContext is null)
                            throw new NotSupportedException("Exact parent-context lookup and new-node parent linkage must be supplied.");
                        var child = resolveContext(query.ParentContext, symbol);
                        context = child.Identity;
                        if (context != 0 && child.Created) linkCreatedContext(context, query.ParentContext);
                    }
                    byte flags = (byte)((query.Node == 0 ? 1 : 0) | ((packed & lexicon.PackedTerminalBit) != 0 ? 2 : 0));
                    Span<byte> record = recordStorage.Slice(written * 32, 32);
                    record.Clear(); //Preserve fresh-zero padding on every emitted row.
                    BinaryPrimitives.WriteUInt32LittleEndian(record, (uint)node);
                    record[5] = flags;
                    BinaryPrimitives.WriteUInt64LittleEndian(record[8..], context);
                    BinaryPrimitives.WriteUInt16LittleEndian(record[20..], symbol);
                    record[22] = query.Category18;
                    accepted++; written++;
                }
                slot.BlockCursor = cursor; slot.Accepted = accepted;
            }
            int returned = slot.Accepted - query.Skip;
            // Native return may be negative for an offset beyond the available edges.
            // A supplied extra-cost buffer would then have a negative memset size.
            if (clearExtraCosts && returned < 0)
                throw new ArgumentException("Native extra-cost clearing requires an in-range page offset.");
            return new(returned, recordStorage.Slice(0, Math.Max(0, returned) * 32).ToArray(), query.SlotIndex, NextChild(slot), slot.Accepted,
                clearExtraCosts ? new byte[returned * 12] : []);
        }
        finally { reading = false; }
    }

    private bool Accept(uint packed, ReadOnlySpan<ushort> selected, bool auxiliary)
    {
        uint lexical = packed & lexicon.PackedLabelMask;
        if (lexical >= selected.Length) throw new InvalidDataException("Packed edge exceeds the prepared lexical map.");
        return selected[(int)lexical] != ushort.MaxValue && (!auxiliary || (packed & lexicon.PackedTerminalBit) != 0);
    }

    private static int NextChild(Slot slot) => slot.Start + slot.BlockCursor;

    private void Refill(Slot slot)
    {
        if (slot.BlockCursor < slot.BlockCount) return;
        slot.Start += slot.BlockCount; slot.BlockCursor = 0;
        slot.BlockCount = Math.Min(256, Math.Max(0, slot.End - slot.Start));
        for (int i = 0; i < slot.BlockCount; i++) slot.Labels[i] = lexicon.GetPackedLabel(slot.Start + i);
    }
}
