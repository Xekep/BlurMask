using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyAugmentationResidualBudget.Probe")]
namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalResidualSource
{
    internal byte[] AssociateAugmentation(LegacyFreshMainGlobalWordAugmentation worker,
        LegacyFreshMainGlobalSourceBatchCompletion.SpanScope scope,ReadOnlySpan<byte> rows,ReadOnlySpan<ulong> residual,CancellationToken token)
    {
        worker.RequireResidualScope(scope,this,token);
        if(rows.Length%72!=0||rows.Length/72>4096||residual.Length is <1 or >65536)
            throw new NotSupportedException("Bounded actual augmentation rows/residual vector required.");
        int e=rows.Length/72,r=residual.Length;
        //Preflight itself is charged BEFORE its scalar loops. No range copies,
        //cache mutation or association allocations have occurred at this point.
        scope.ChargeAssociation(worker,checked((long)e+r),0,token);
        lock(gate)
        {
            worker.RequireResidualScope(scope,this,token);
            long rangeRows=0;
            foreach(ulong reference in residual)
            {
                token.ThrowIfCancellationRequested();var source=LegacyNativeResidualSource.Decode(reference);
                if(reference!=Pack(source.Node,source.Arc))throw new InvalidDataException("Canonical actual residual base reference required.");
                rangeRows=checked(rangeRows+Geometry.StoredBaseRangeCount(source.Node,source.Arc,token));
            }
            for(int i=0;i<e;i++)
            {
                token.ThrowIfCancellationRequested();var row=rows.Slice(i*72,72);int node=BinaryPrimitives.ReadInt32LittleEndian(row);ushort arc=BinaryPrimitives.ReadUInt16LittleEndian(row[4..]);
                if(row[60]!=0||row[68]!=0||BinaryPrimitives.ReadInt32LittleEndian(row[64..])!=0||
                    BinaryPrimitives.ReadUInt64LittleEndian(row[48..])!=Pack(node,arc))
                    throw new NotSupportedException("Canonical untouched actual ordinary218A30 row required before residual association.");
                rangeRows=checked(rangeRows+Geometry.StoredBaseRangeCount(node,arc,token));
            }
            var bound=LegacyAugmentationResidualBudget.Calculate(e,r,rangeRows,totalExtras,
                Graph.State.NodeCount,Graph.State.CandidateCount,Graph.State.Groups.Count);
            //The actual private cache remains locked from extent calculation
            //through association. An unrelated append cannot enlarge a loop
            //after its bound was charged. No private arrays leave this method.
            scope.ChargeAssociation(worker,bound.LoopReserve-e-r,bound.AdditionalCopyReserve,token);
            int beforeExtras=totalExtras;
            var result=Associate(rows,residual,token);
            byte[] associated=result.CopyRows();int merged=0;
            for(int i=0;i<e;i++){token.ThrowIfCancellationRequested();if(associated[i*72+68]!=0)merged++;}
            scope.RecordAssociation(worker,r,e,merged,totalExtras-beforeExtras,token);
            worker.RequireResidualScope(scope,this,token);
            if(!result.Succeeded||result.RemainingCount!=0)
                throw new InvalidOperationException("Actual residual217DB0 association did not complete the current augmentation walk.");
            return associated;
        }
    }
}

//Operational loop/copy bound, not unfolded BCL helper/CPU/RSS certification.
//Input rangeRows sums the actual base range counts of EVERY residual and row
//source. Successful217DB0 assigns each residual to at mostone row and adds
//that row's base reference once, so all native merge windows total<=R+E,
//and all concatenated range counts total<=K. Cache lookup occurs AFTERconcat.
internal static class LegacyAugmentationResidualBudget
{
    internal readonly record struct Estimate(long LoopReserve,long AdditionalCopyReserve);
    internal static Estimate Calculate(int e,int r,long k,int x,int n,int a,int g)
    {
        if(e is <0 or >4096||r is <1 or >65536||k<0||x is <0 or >65536||n is <1 or >4096||a<0||g<0)
            throw new ArgumentException("Actual admitted association scalar extents required.");
        long re=checked((long)r*e),ri=checked((long)r*(r-1)/2),cache=checked((long)x+e);
        //Kernel: residual scan, nearest rows, insertion, suffix counts, reverse
        //and compaction. DescriptorSingle scans<=4RE in one-descriptor scope;
        //topmark range scans<=E*K. Sort scan/partition/compaction<=3K*K+4K.
        long loops=checked(e+r+r+2*re+ri+2L*e+(long)e*(r+1)+4*re+(long)e*k+
            2L*(r+e)+3*k*k+4*k+(long)e*cache+k*cache+(long)e*g+k*g+(long)e*(n+a));
        //Only NEW association work: two72E row clones, residual clones/List
        //growth/result, merge-window GetRange/ToArray +MergeSources refs;
        //range clones/collected growth/sort workspace/compaction and extra refs.
        //List/Stack doubling bounds are conditional .NET10.0.12 source policy.
        //Range=12B, ulong=8B, tuple(int,int)=8B; explicit row/index stores and
        //quicksort swaps included. Existing emission arrays are NOT reservedagain.
        //Payload/copy decomposition (including conservative growth estimates):
        //rows144E; residual arrays/growth56R; window/refcopies24(R+E);
        //rowstores9E; insertion4R(R-1); suffixcount4RE+compact8RE;
        //range arrays/writes132K +Stack growth96K; sort swaps12K(K+1);
        //extra reference-list growth48X+96E. Semantic object-field allowance
        //128E+1024 is separate from CLR headers/alignment/RSS (excluded).
        //See SOURCE-REVIEW.md for the pinned growth assumptions and scope.
        long copy=checked(144L*e+56L*r+24L*(r+e)+9L*e+4L*r*(r-1)+12*re+
            228*k+12*k*(k+1)+48L*x+96L*e+128L*e+1024);
        return new(loops,copy);
    }
}
