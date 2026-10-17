using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainWordAugmentationScan.Probe")]
namespace InkRecognition;

/// <summary>Prepared219580 compact candidate scan. Boundary callbacks are the
/// actual frontend168/170/178 services; dry-run must be218A30's direct-parent
/// inspection, not the21FFD0 alternate cursor. No WORDs are emitted here.</summary>
internal static class LegacyPreparedMainWordAugmentationCandidates
{
    internal readonly record struct DryRun(int Status,ulong FirstReference);
    internal readonly record struct Attempt(int CompactIndex,int MinimumNode,int Status,ulong FirstReference,bool Included);
    internal sealed record Result(int[] CompactIndices,Attempt[] Attempts);
    internal static int DecodeNode24(ulong key)=>unchecked((int)((uint)key<<8))>>8;
    internal static int DecodeContextNode(int context)=>unchecked((int)((uint)(context>>7)<<8))>>8;
    internal static Result Scan(ReadOnlySpan<byte> compact40,int actualRecordCount,ReadOnlySpan<int> nodeStarts2478,
        int firstContext,int lastContext,bool knowledge98,
        Func<int,ushort,int> firstBoundary168,Func<int,ushort,int> lastBoundary170,
        Func<int,int,DryRun> actualDirectParentInspect,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(firstBoundary168);ArgumentNullException.ThrowIfNull(lastBoundary170);
        ArgumentNullException.ThrowIfNull(actualDirectParentInspect);token.ThrowIfCancellationRequested();
        if(compact40.Length%40!=0||compact40.Length/40>131072||nodeStarts2478.Length is <2 or >4097)
            throw new ArgumentException("Bounded complete compact40 rows and actual2478 extents required.");
        if(actualRecordCount<0||actualRecordCount>compact40.Length/40)throw new ArgumentOutOfRangeException(nameof(actualRecordCount));
        int count=actualRecordCount;
        if(nodeStarts2478[0]!=0||nodeStarts2478[^1]!=count)throw new ArgumentException("Complete2478 compact extent required.");
        for(int i=1;i<nodeStarts2478.Length;i++)if(nodeStarts2478[i]<nodeStarts2478[i-1])
            throw new ArgumentException("Ordered native per-node compact starts required.");
        if(lastContext<firstContext)return new([],[]);
        int last=DecodeContextNode(lastContext),minimum=firstContext<0?-1:DecodeContextNode(firstContext);
        if((uint)last>=(uint)(nodeStarts2478.Length-1))throw new NotSupportedException("Actual forward source end must name an owned node.");
        var selected=new List<int>();var attempts=new List<Attempt>();
        for(int index=nodeStarts2478[last];index<nodeStarts2478[last+1];index++)
        {
            token.ThrowIfCancellationRequested();var row=compact40.Slice(index*40,40);byte flags=row[32];
            if((flags&8)==0||knowledge98&&(flags&16)==0)continue;
            ulong reference=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);int node=DecodeNode24(reference);
            ushort arc=unchecked((ushort)(reference>>24));
            if(node!=last||lastBoundary170(node,arc)!=lastContext)continue;
            var dry=actualDirectParentInspect(index,minimum);token.ThrowIfCancellationRequested();
            if(dry.Status==-1)throw new InvalidOperationException("Native218A30 failed; the caller must retain and poison its entered prefix.");
            if(dry.Status is <0 or >4)throw new NotSupportedException("Unexpected native218A30 result status.");
            bool include=false;
            if(dry.Status!=1)
            {
                int firstNode=DecodeNode24(dry.FirstReference);short firstArc=unchecked((short)(dry.FirstReference>>24));
                int first=firstArc>=0?firstBoundary168(firstNode,unchecked((ushort)firstArc)):unchecked(firstNode+firstArc);
                include=first==firstContext;if(include)selected.Add(index);
            }
            attempts.Add(new(index,minimum,dry.Status,dry.FirstReference,include));
        }
        token.ThrowIfCancellationRequested();return new(selected.ToArray(),attempts.ToArray());
    }
}
