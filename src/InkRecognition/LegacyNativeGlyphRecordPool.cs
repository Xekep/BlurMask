using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeGlyphRecordInsertion(ulong Reference, bool ExistingDescriptor);

/// <summary>274 per-source-key record production1399f0/13a670/13acc0 and
/// normal symbol update13af30. Portable key storage replaces native AVL
/// pointers/rotations; native balance bits are not reconstructed. Owned
/// snapshots retain lookup-relevant count/duplicate flags and complete rows.</summary>
public sealed partial class LegacyNativeGlyphRecordPool
{
    private const int MaximumOwners = 4096;
    private const int MaximumTotalRecords = 65536;
    private readonly Dictionary<ulong, Node> nodes = [];
    private int totalRecords;

    private LegacyNativeGlyphRecordPool() { }

    /// <summary>The source274 capture flag is actual caller configuration,
    /// not inferred from the alphabet, TOP273 scores or first model. The
    /// geometry280 payload allocator branch requires a separate producer.</summary>
    public static LegacyNativeGlyphRecordPool CreateWithoutGeometryCapture(bool actualCaptureFlag,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (actualCaptureFlag) throw new NotSupportedException("Native geometry280 capture requires its actual payload factory.");
        return new();
    }

    /// <summary>Native owner key is already canonical low24 node/arc16/bit56.
    /// DescriptorId is signedI32 field00, not the row's symbolU16 field08.
    /// Opaque bytes in an allocated row remain explicit caller storage.</summary>
    public LegacyNativeGlyphRecordInsertion AppendDescriptor(ulong ownerKey, int descriptorId,
        bool forceDuplicate, ReadOnlySpan<byte> initialRowStorage40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerKey != LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(ownerKey) || initialRowStorage40.Length != 40)
            throw new ArgumentException("Canonical native key and complete allocated-row storage required.");
        nodes.TryGetValue(ownerKey, out Node? node);
        if (node is not null && !forceDuplicate && !node.NoDescriptorDedup)
            for (int ordinal = 0; ordinal < node.Rows.Count; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (ReadI32(node.Rows[ordinal], 0) == descriptorId) return new(ownerKey | ((ulong)ordinal << 40), true);
            }
        return AppendNewDescriptor(ownerKey,descriptorId,forceDuplicate,initialRowStorage40,node,cancellationToken);
    }

    // Shared original allocation/mutation tail; callers perform their own original search or private first-ordinal lookup.
    private LegacyNativeGlyphRecordInsertion AppendNewDescriptor(ulong ownerKey,int descriptorId,bool forceDuplicate,
        ReadOnlySpan<byte> initialRowStorage40,Node? node,CancellationToken cancellationToken)
    {
        //13a670 grows capacity by ten and refuses the next growth above7FE.
        //Thus a fresh native node can hold2040 rows, not2046 or2047.
        int count = node?.Rows.Count ?? 0, capacity = node?.Capacity ?? 0;
        int nextCapacity = count < capacity ? capacity : capacity + 10;
        if (nextCapacity > 0x7fe || totalRecords == MaximumTotalRecords ||
            node is null && nodes.Count == MaximumOwners)
            throw new ArgumentException("Glyph pool exceeds native capacity or portable ownership budget.");
        byte[] row = initialRowStorage40.ToArray();
        WriteI32(row, 0, descriptorId);
        WriteU16(row, 4, (ushort)(ReadU16(row, 4) & 0xfff0));
        WriteI32(row, 6, unchecked((int)0xffff0000));
        WriteI32(row, 12, 0x3f800000); WriteI32(row, 16, unchecked((int)0xbf800000));
        WriteI32(row, 20, 0);
        row.AsSpan(24, 8).Clear(); row.AsSpan(32, 8).Fill(255);
        cancellationToken.ThrowIfCancellationRequested();
        if (node is null) { node = new(); nodes.Add(ownerKey, node); }
        node.Capacity = nextCapacity; node.NoDescriptorDedup |= forceDuplicate;
        node.Rows.Add(row); totalRecords++;
        return new(ownerKey | ((ulong)count << 40), false);
    }

    /// <summary>Exact13af30 ordinary one-component symbol branch: kind0 and
    /// raw alphabet label08. Encoded macro labelsFFF9..FFFC/count-zero/kind4
    /// remain separate. Existing upper flags and publication word06 survive.</summary>
    public void SetOrdinarySymbol(ulong reference, ushort symbol,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbol is >= 0xfff9 and <= 0xfffc)
            throw new NotSupportedException("Native encoded macro/count branch requires its actual component fields.");
        byte[] row = ResolveRow(reference);
        WriteU16(row, 4, (ushort)(ReadU16(row, 4) & 0xfff8)); WriteU16(row, 8, symbol);
    }

    public byte[] CopyRecord(ulong reference, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); return ResolveRow(reference).ToArray(); }

    /// <summary>Publish actual classifier/geometry/trait channels after their
    /// independent native producers. Descriptor identity00 must remain owned;
    /// this does not derive glyph scores from TOP273 mass.</summary>
    public void ReplaceRecord(ulong reference, ReadOnlySpan<byte> actualRecord40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] row = ResolveRow(reference);
        if (actualRecord40.Length != 40 || ReadI32(actualRecord40, 0) != ReadI32(row, 0))
            throw new ArgumentException("Complete actual record with unchanged descriptor identity required.");
        actualRecord40.CopyTo(row);
    }

    /// <summary>13ac80 owner lookup (not13ae90 single-record lookup). Key is
    /// already canonical. Missing owners are unavailable, not empty glyphs.</summary>
    public LegacyNativeGlyphRecordNode CopyOwnerNode(ulong canonicalKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (canonicalKey != LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalKey) ||
            !nodes.TryGetValue(canonicalKey, out Node? node))
            throw new ArgumentException("Actual canonical glyph owner is unavailable.");
        return Snapshot(canonicalKey, node, cancellationToken);
    }

    // Numeric private-pool observation only. The caller authenticates its SAME live pool before reading/exporting.
    // No rows, query/source handle, mutation, callback or owner capability is returned.
    internal (int Owners,int Records) ReadRawExportCounts() => (nodes.Count,totalRecords);

    /// <summary>Lookup snapshots for the existing13ae90 resolver. Low2 AVL
    /// balance bits are portablezero; count extraction discards those bits.
    /// This is not a byte-for-byte dump of native64-byte AVL nodes.</summary>
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyOwnerNodes(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<LegacyNativeGlyphRecordNode>(nodes.Count);
        foreach (var item in nodes) result.Add(Snapshot(item.Key, item.Value, cancellationToken));
        return result.AsReadOnly();
    }

    private static LegacyNativeGlyphRecordNode Snapshot(ulong key, Node node, CancellationToken token)
    {
        byte[] records = new byte[node.Rows.Count * 40];
        for (int ordinal = 0; ordinal < node.Rows.Count; ordinal++)
        { token.ThrowIfCancellationRequested(); node.Rows[ordinal].CopyTo(records, ordinal * 40); }
        ushort packed = (ushort)((node.Rows.Count << 2) | (node.NoDescriptorDedup ? 0x8000 : 0));
        return new(key, packed, records);
    }

    private byte[] ResolveRow(ulong reference)
    {
        int ordinal = unchecked((short)(reference >> 40));
        ulong key = LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
        if (!nodes.TryGetValue(key, out Node? node) || ordinal < 0 || ordinal >= node.Rows.Count)
            throw new ArgumentException("Glyph record reference is outside this actual owned pool.");
        return node.Rows[ordinal];
    }

    // Only the private rich refresh caller supplies this optional optimization to the kernel.
    // No field is added to the pool or its default Node; no batch is globally installed.
    internal DescriptorBatch CreateRichDescriptorBatch(ulong key,int length,CancellationToken token)
        =>new(this,key,length,token);
    internal sealed class DescriptorBatch:IDisposable
    {
        private readonly LegacyNativeGlyphRecordPool pool;
        private readonly ulong key;
        private readonly int[] first;
        private readonly int length;
        private Node? expectedNode;
        private int expectedCount;
        private bool expectedNoDedup,disposed;
        private byte[]? expectedLast;
        internal DescriptorBatch(LegacyNativeGlyphRecordPool pool,ulong key,int length,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(key!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(key)||length is<0 or>4096)
                throw new ArgumentException("Bounded actual rich owner interval required.");
            this.pool=pool;this.key=key;this.length=length;pool.nodes.TryGetValue(key,out expectedNode);Remember();
            // No map is needed for the authentic sticky bypass branch.
            first=expectedNoDedup?Array.Empty<int>():new int[length];
            if(!expectedNoDedup)
            {
                Array.Fill(first,-1);
                for(int i=0;i<expectedCount;i++)
                {
                    token.ThrowIfCancellationRequested();int descriptor=ReadI32(expectedNode!.Rows[i],0);
                    if((uint)descriptor<(uint)length&&first[descriptor]<0)first[descriptor]=i;
                }
            }
        }
        private void Remember()
        {expectedCount=expectedNode?.Rows.Count??0;expectedNoDedup=expectedNode?.NoDescriptorDedup??false;expectedLast=expectedCount==0?null:expectedNode!.Rows[expectedCount-1];}
        private void Require(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            // The dictionary is private readonly and append-only: an issued nonnull
            // Node cannot be detached/rebound. Preserve the lookup for the null case,
            // where external creation of this key must still invalidate the batch.
            var actual=expectedNode;
            if(actual is null)pool.nodes.TryGetValue(key,out actual);
            if(disposed||!ReferenceEquals(actual,expectedNode)||(actual?.Rows.Count??0)!=expectedCount||
                (actual?.NoDescriptorDedup??false)!=expectedNoDedup||
                !ReferenceEquals(expectedCount==0?null:actual!.Rows[expectedCount-1],expectedLast))
                throw new InvalidOperationException("Private rich descriptor batch invalidated by external pool mutation.");
        }
        internal LegacyNativeGlyphRecordInsertion Append(int descriptor,ReadOnlySpan<byte> residue,CancellationToken token)
        {
            Require(token);
            if(residue.Length!=40||descriptor<0||descriptor>=length)
                throw new ArgumentException("Actual same-interval descriptor and complete row required.");
            if(!expectedNoDedup&&first[descriptor]>=0)return new(key|((ulong)first[descriptor]<<40),true);
            var inserted=pool.AppendNewDescriptor(key,descriptor,false,residue,expectedNode,token);
            if(expectedNode is null)pool.nodes.TryGetValue(key,out expectedNode);Remember();
            if(!expectedNoDedup)first[descriptor]=expectedCount-1;
            return inserted;
        }
        // Actual pool/key fallback preserves the old generic kernel behavior for
        // foreign bindings. The closed English producer always takes the Node path.
        // Epoch checks stay at Append/Complete, not newly between callbacks: this
        // preserves partial-mutation chronology after reentrant append/cleanup.
        private byte[] ResolveBoundRow(LegacyNativeGlyphRecordPool actualPool,ulong reference)
        {
            var node=expectedNode;
            if(!ReferenceEquals(pool,actualPool)||node is null||
                LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference)!=key)
                return actualPool.ResolveRow(reference);
            int ordinal=unchecked((short)(reference>>40));
            if(ordinal<0||ordinal>=node.Rows.Count)
                throw new ArgumentException("Glyph record reference is outside this actual owned pool.");
            return node.Rows[ordinal];
        }
        internal byte[] CopyRecord(LegacyNativeGlyphRecordPool actualPool,ulong reference,CancellationToken token)
        {token.ThrowIfCancellationRequested();return ResolveBoundRow(actualPool,reference).ToArray();}
        internal void ReplaceRecord(LegacyNativeGlyphRecordPool actualPool,ulong reference,ReadOnlySpan<byte> record,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();byte[] row=ResolveBoundRow(actualPool,reference);
            if(record.Length!=40||ReadI32(record,0)!=ReadI32(row,0))
                throw new ArgumentException("Complete actual record with unchanged descriptor identity required.");
            record.CopyTo(row);
        }
        internal void Complete(CancellationToken token)=>Require(token);
        public void Dispose(){disposed=true;}
    }

    private sealed class Node
    { public readonly List<byte[]> Rows = []; public int Capacity; public bool NoDescriptorDedup; }
    private static int ReadI32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
    private static ushort ReadU16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static void WriteI32(byte[] data, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset), value);
    private static void WriteU16(byte[] data, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), value);
}
