using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishRefreshBudget.Probe")]
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    //Detached scalars only; preserved even if the following cumulative charge
    //refuses. Default zero field is72 logical bytes, not a CLR/RSS guarantee.
    internal readonly record struct GraphGlyphAllocationProgress(int References,int ColdOwners,int MatchedRows,
        long PayloadBefore,long RequestedPayload,long CopiesBefore,long RequestedCopies,long VisitsBefore,long RequestedVisits,bool Reserved);
    internal readonly record struct GraphGlyphAllocationSnapshot(long CurrentPayload,long CurrentCopies,long CurrentVisits,GraphGlyphAllocationProgress LastRefresh);
    private GraphGlyphAllocationProgress graphGlyphAllocationProgress;
    internal GraphGlyphAllocationSnapshot GraphGlyphAllocationForProbe=>new(ledger.Payload,ledger.Copies,ledger.Visits,graphGlyphAllocationProgress);
    internal readonly record struct GraphRefreshAllocation(long Payload,long Copies,long Visits);
    //Same217670 compact predicate, pure numeric and never a source issuer.
    internal static bool GraphRefreshMatches(ReadOnlySpan<byte> row,ulong key)
    {
        if(row.Length!=40)throw new ArgumentException("Complete compact40 row required.");
        byte traits=row[32];return(traits&0x20)!=0&&(traits&7)==0&&BinaryPrimitives.ReadUInt64LittleEndian(row[8..])==key;
    }
    internal static GraphRefreshAllocation GraphRefreshCharge(long sourcePayload,long sourceCopies,int matched,int ownerRows)
    {
        if(sourcePayload<0||sourceCopies<0||matched is<0 or>4096||ownerRows is<0 or>2040)throw new ArgumentOutOfRangeException();
        //Cache creation upper:182 possible source labels (cache may later refuse>150),128 prototype costs.
        //Per candidate256 includes values/candidate/rows/ROM/float[][] clones,
        //short per-vector allocations+metadata;16/prototype covers both copies.
        //4096 fixed covers closures/delegates,40 scratch and cache dictionary
        //growth for one additional group (the cache is shared by all refs).
        //Each matched attempt covers row40,CopyRecord twice,request16, wrappers
        //and list growth; existing descriptor hits still charged, no refund.
        long rows=checked((long)ownerRows+matched);
        return new(checked(sourcePayload+4096+182L*256+128L*16+matched*512L+rows*64),
            checked(sourceCopies+8192+182L*512+128L*32+matched*1024L+rows*128),
            checked(4096+matched*8192L+32*rows*rows));
    }
    // Only the private periodic compact+cache-index route uses this bound.
    // Original P/C and refusal domains stay unchanged; traversal, compact index,
    // batch initialization and cache construction are separately prepaid.
    internal static GraphRefreshAllocation GraphIndexedRefreshCharge(long sourcePayload,long sourceCopies,int matched,int ownerRows,int depth)
    {
        if(depth is<1 or>120)throw new ArgumentOutOfRangeException(nameof(depth));
        var original=GraphRefreshCharge(sourcePayload,sourceCopies,matched,ownerRows);
        long rows=checked((long)ownerRows+matched);
        // No repeated descriptor scan or global owner lookup in issued-Node row access.
        // Remaining private-row work7200 fits7680; List backing and source26 are separate.
        return original with{Visits=checked(4096L+matched*7680L+64L*rows+256L*depth)};
    }
    internal static bool GraphCharacterEntryAllowed(bool requested,bool previousContext,bool wordOwner,bool sameCounts,bool geometry)
        =>requested&&(!previousContext||wordOwner)&&sameCounts&&geometry;
}
