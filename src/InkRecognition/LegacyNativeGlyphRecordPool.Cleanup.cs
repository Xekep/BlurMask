#if BLURMASK_PUBLICATION_DIAGNOSTICS
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
[assembly: InternalsVisibleTo("LegacyMainGlyphChainCleanup.Probe")]
namespace InkRecognition;

public sealed partial class LegacyNativeGlyphRecordPool
{
    internal sealed record ChainCleanupResult(ulong[] ReversedTraversal,ulong[] RemovedReferences);
    /// <summary>274.V40/13B1D0. Reverse the inclusive linked segment from
    /// lastReference to firstReference, then visit it in reverse order and pop
    /// only current trailing rows of each source owner. No compaction/reindexing
    /// or native geometry allocation ownership is inferred.</summary>
    internal ChainCleanupResult RemoveCreatedChainSegment(ulong firstReference,ulong lastReference,
        CancellationToken token=default,LegacyPublicationScoreTrace? candidateTrace=null)
    {
        token.ThrowIfCancellationRequested();
        var chain=new List<ulong>();var seen=new HashSet<ulong>();ulong reference=lastReference;
        if(candidateTrace is not null&&candidateTrace.CandidateAdmitCurrent(this))candidateTrace.BeginCleanup(firstReference,lastReference);
        while(true)
        {
            token.ThrowIfCancellationRequested();
            if(reference==ulong.MaxValue||!seen.Add(reference)||chain.Count==4096)
                throw new ArgumentException("Actual acyclic inclusive temporary chain segment required.");
            byte[] row=ResolveRow(reference);
            if(ReadU64(row,24)!=0)throw new NotSupportedException("Native geometry payload destruction requires its actual allocator owner.");
            if(candidateTrace is not null&&candidateTrace.CandidateAdmitCurrent(this))candidateTrace.CaptureCleanupRead(reference,row);
            chain.Add(reference);if(reference==firstReference)break;reference=ReadU64(row,32);
        }
        //After preflight there are no callbacks, cancellation or allocation
        //inside the linked mutation. Retain native reversal and tail-only pop.
        var removed=new List<ulong>(chain.Count);var traversal=new ulong[chain.Count];
        LegacyPublicationCandidateCleanup? observation=null;
        if(candidateTrace is not null&&candidateTrace.CandidateAdmitCurrent(this))observation=candidateTrace.PrepareCleanupMutation(chain.Count);
        token.ThrowIfCancellationRequested();
        for(int i=0;i<chain.Count;i++)
        {
            ulong next=i==0?ulong.MaxValue:chain[i-1];WriteU64(ResolveRow(chain[i]),32,next);
            if(observation is not null){observation.ReversalReferences[i]=chain[i];observation.NewLinks[i]=next;}
        }
        int output=0;
        for(int i=chain.Count-1;i>=0;i--)
        {
            ulong current=chain[i],key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(current);
            var node=nodes[key];int ordinal=unchecked((short)(current>>40));traversal[output++]=current;
            int countBefore=node.Rows.Count;
            if(observation is not null){observation.PopReferences[output-1]=current;observation.CountsBefore[output-1]=countBefore;}
            if(ordinal==countBefore-1)
            {
                node.Rows.RemoveAt(ordinal);totalRecords--;removed.Add(current);
                if(observation is not null)observation.Removed[output-1]=true;
            }
        }
        if(observation is not null)observation.Applied=true;
        return new(traversal,removed.ToArray());
    }
    private static ulong ReadU64(ReadOnlySpan<byte> row,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(row[at..]);
    private static void WriteU64(byte[] row,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(at),value);
}
#else
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
[assembly: InternalsVisibleTo("LegacyMainGlyphChainCleanup.Probe")]
namespace InkRecognition;

public sealed partial class LegacyNativeGlyphRecordPool
{
    internal sealed record ChainCleanupResult(ulong[] ReversedTraversal,ulong[] RemovedReferences);
    /// <summary>274.V40/13B1D0. Reverse the inclusive linked segment from
    /// lastReference to firstReference, then visit it in reverse order and pop
    /// only current trailing rows of each source owner. No compaction/reindexing
    /// or native geometry allocation ownership is inferred.</summary>
    internal ChainCleanupResult RemoveCreatedChainSegment(ulong firstReference,ulong lastReference,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        var chain=new List<ulong>();var seen=new HashSet<ulong>();ulong reference=lastReference;
        while(true)
        {
            token.ThrowIfCancellationRequested();
            if(reference==ulong.MaxValue||!seen.Add(reference)||chain.Count==4096)
                throw new ArgumentException("Actual acyclic inclusive temporary chain segment required.");
            byte[] row=ResolveRow(reference);
            if(ReadU64(row,24)!=0)throw new NotSupportedException("Native geometry payload destruction requires its actual allocator owner.");
            chain.Add(reference);if(reference==firstReference)break;reference=ReadU64(row,32);
        }
        //After preflight there are no callbacks, cancellation or allocation
        //inside the linked mutation. Retain native reversal and tail-only pop.
        var removed=new List<ulong>(chain.Count);var traversal=new ulong[chain.Count];
        token.ThrowIfCancellationRequested();
        for(int i=0;i<chain.Count;i++)WriteU64(ResolveRow(chain[i]),32,i==0?ulong.MaxValue:chain[i-1]);
        int output=0;
        for(int i=chain.Count-1;i>=0;i--)
        {
            ulong current=chain[i],key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(current);
            var node=nodes[key];int ordinal=unchecked((short)(current>>40));traversal[output++]=current;
            if(ordinal==node.Rows.Count-1){node.Rows.RemoveAt(ordinal);totalRecords--;removed.Add(current);}
        }
        return new(traversal,removed.ToArray());
    }
    private static ulong ReadU64(ReadOnlySpan<byte> row,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(row[at..]);
    private static void WriteU64(byte[] row,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(at),value);
}
#endif
