using System.Buffers.Binary;
namespace InkRecognition;

// Successful fresh1FA300 subset. Pointer-vector bytes are opaque zero-owned
// representatives; actual alias authority is the SAME block reference+offset.
// No native pointer identity is invented or published.
internal sealed class LegacyEnglishCaseNode0Ring
{
    private const int RowBytes=156*88,PrimaryBytes=125*80;
    private readonly LegacyEnglishCaseNode0Ledger ledger;
    private readonly byte[][] vectors,rows,primary;
    private readonly int[] physical;
    internal int Length{get;}
    internal int DistinctRows{get;}
    private readonly byte[] currentRowMask,currentPrimaryMask;
    // Opt-in generic cursor. OLD node0 construction does not allocate these
    // tables or move current. Masks follow the SAME physical120-row aliases.
    private byte[][]? rowMasks,primaryMasks;
    private int currentNode;
    internal int CurrentNode=>currentNode;
    internal int ActualNodeCount{get;}

    internal byte[] CurrentRowMask{get{ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,1);return rowMasks is null?currentRowMask:rowMasks[Row(currentNode)];}}
    internal byte[] CurrentPrimaryMask{get{ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,1);return primaryMasks is null?currentPrimaryMask:primaryMasks[Row(currentNode)];}}
    internal LegacyEnglishCaseNode0Ring(int nodeCount,int retention,LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(nodeCount is <1 or >4096||retention!=120)throw new NotSupportedException("Bounded actual native node window/retention required.");
        this.ledger=ledger;ActualNodeCount=nodeCount;Length=checked((nodeCount+39)/40*40);DistinctRows=Math.Min(retention,Length);
        vectors=ledger.References(LegacyEnglishCaseNode0Ledger.Site.VectorOwners,9);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.VectorOwners,9*8);
        // Nine distinct calls preserve the selected successful native order.
        vectors[0]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2528,4*Length);
        vectors[1]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector23e0,8*Length);
        vectors[2]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2408,8*Length);
        vectors[3]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector23f0,2*Length);
        vectors[4]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector23f8,2*Length);
        vectors[5]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2478,4*(Length+1));
        vectors[6]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2480,Length);
        vectors[7]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2488,4*(Length+1));
        vectors[8]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Vector2520,4*Length);
        // Fixed3+3 ownership arrays and map, never collection growth.
        rows=ledger.References(LegacyEnglishCaseNode0Ledger.Site.RowOwners,3);
        primary=ledger.References(LegacyEnglishCaseNode0Ledger.Site.PrimaryOwners,3);
        physical=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,Length);
        for(int start=0;start<DistinctRows;start+=40)
        {
            token.ThrowIfCancellationRequested();int group=start/40,n=Math.Min(40,DistinctRows-start);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.RowOwners,8);
            rows[group]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,n*RowBytes);
            // Install stored aliases before attempting the primary allocation.
            for(int j=0;j<n;j++)
            {ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,1);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,4);physical[start+j]=start+j;}
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PrimaryOwners,8);primary[group]=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.PrimaryBlocks,n*PrimaryBytes);
        }
        for(int i=DistinctRows;i<Length;i++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,3);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,4);physical[i]=physical[i-120];
        }
        currentRowMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,RowBytes);
        currentPrimaryMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,PrimaryBytes);
    }
    internal Span<byte> CurrentRows=>RowsAt(currentNode);
    internal Span<byte> CurrentPrimary=>PrimaryAt(currentNode);
    internal void EnableGraphCursor(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(rowMasks is not null||currentNode!=0)throw new InvalidOperationException("One generic ring claim required.");
        // Fixed reference slots; allocate masks only as each physical node enters.
        var r=ledger.References(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,DistinctRows);
        var a=ledger.References(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,DistinctRows);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,checked(16L*DistinctRows));
        r[0]=currentRowMask;a[0]=currentPrimaryMask;rowMasks=r;primaryMasks=a;
    }
    internal void EnterGraphNode(int node,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(rowMasks is null||primaryMasks is null||node!=currentNode+1||(uint)node>=(uint)ActualNodeCount)
            throw new InvalidOperationException("Only next actual graph node may enter this private ring.");
        int physicalRow=Row(node);
        if(rowMasks[physicalRow] is null)
        {
            var r=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,RowBytes);
            var a=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,PrimaryBytes);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,16);rowMasks[physicalRow]=r;primaryMasks[physicalRow]=a;
        }
        else
        {
            // Native data residue is preserved across alias reuse; definedness
            // is reset before new writers, never promoted from old known bytes.
            ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,rowMasks[physicalRow],0);
            ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,primaryMasks[physicalRow],0);
        }
        currentNode=node;
    }
    private void RequireRetainedNode(int node)
    {
        if(rowMasks is null||(uint)node>(uint)currentNode||node<Math.Max(0,currentNode-119))
            throw new InvalidOperationException("Live retained graph node required; historical images are DATA only.");
    }
    internal Span<byte> OwnedRowsAt(int node){RequireRetainedNode(node);return RowsAt(node);}
    internal Span<byte> OwnedPrimaryAt(int node){RequireRetainedNode(node);return PrimaryAt(node);}
    internal Span<byte> OwnedRowMaskAt(int node){RequireRetainedNode(node);return rowMasks![Row(node)];}
    internal Span<byte> OwnedPrimaryMaskAt(int node){RequireRetainedNode(node);return primaryMasks![Row(node)];}
    internal int SourceEndAt(int node){RequireRetainedNode(node);return BinaryPrimitives.ReadUInt16LittleEndian(vectors[3].AsSpan(2*node));}
    internal int RetainedEndAt(int node){RequireRetainedNode(node);return BinaryPrimitives.ReadUInt16LittleEndian(vectors[4].AsSpan(2*node));}
    private int Row(int node)
    {ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PhysicalRows,2);if((uint)node>=(uint)Length)throw new ArgumentOutOfRangeException(nameof(node));return physical[node];}
    internal Span<byte> RowsAt(int node)
    {int p=Row(node);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,2);return rows[p/40].AsSpan(p%40*RowBytes,RowBytes);}
    internal Span<byte> PrimaryAt(int node)
    {int p=Row(node);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.PrimaryBlocks,2);return primary[p/40].AsSpan(p%40*PrimaryBytes,PrimaryBytes);}
    internal void ClearCurrent3e(CancellationToken token)
    {
        for(int i=0;i<125;i++)
        {token.ThrowIfCancellationRequested();ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,4);BinaryPrimitives.WriteUInt16LittleEndian(CurrentRows[(i*88+62)..],ushort.MaxValue);CurrentRowMask.AsSpan(i*88+62,2).Fill(255);}
    }
    internal int Cleanup(CancellationToken token)
    {
        int changed=0;var p=CurrentPrimary;
        for(int i=0;i<250;i++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Cleanup,24);int at=i*40;
            var mask=CurrentPrimaryMask.AsSpan(at,40);
            // Literal native scan; no assumption that each entry is terminal.
            if(BinaryPrimitives.ReadInt32LittleEndian(p[at..])<0x4000000&&(p[at+31]&32)==0&&BinaryPrimitives.ReadUInt16LittleEndian(p[(at+24)..])>=125)
            {
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Cleanup,4);BinaryPrimitives.WriteInt32LittleEndian(p[at..],0x4000000);changed++;
                // English frozen4-file errata contract: definedness tracks the
                // concrete selected branch write. Only these four bytes become
                // known; no universal unknown-predicate abstraction is applied.
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Cleanup,4);mask[..4].Fill(255);
            }
        }
        return changed;
    }
    internal void ClearNode0Counters(int sourceEnd,int retainedEnd)
    {
        if(sourceEnd is <125 or >156||retainedEnd<125||retainedEnd>sourceEnd)throw new InvalidDataException("Exact bounded secondary endpoints required.");
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Descriptor,12);
        BinaryPrimitives.WriteUInt16LittleEndian(vectors[3].AsSpan(currentNode*2),checked((ushort)sourceEnd));BinaryPrimitives.WriteUInt16LittleEndian(vectors[4].AsSpan(currentNode*2),checked((ushort)retainedEnd));
        vectors[8].AsSpan(currentNode*4,4).Clear();vectors[0].AsSpan(currentNode*4,4).Clear();
    }
}
