namespace InkRecognition;

/// <summary>Private per-invocation logical allocation/work envelope, not RSS.
///Old public graph paths pass null and retain their exact behavior.</summary>
internal sealed class LegacyOrdinaryGraphWorkBudget
{
    private readonly long maximumBytes,maximumEdges,maximumRanges,maximumCompose;
    private long bytes,edges,ranges,seeds,compose;
    private readonly CancellationToken token;
    internal LegacyOrdinaryGraphWorkBudget(CancellationToken token=default,long maximumBytes=64L*1024*1024,
        long maximumEdges=131072,long maximumRanges=65536,long maximumCompose=1048576)
    {this.token=token;this.maximumBytes=maximumBytes;this.maximumEdges=maximumEdges;this.maximumRanges=maximumRanges;this.maximumCompose=maximumCompose;
     if(maximumBytes<0||maximumEdges<0||maximumRanges<0||maximumCompose<0)throw new ArgumentOutOfRangeException(nameof(maximumBytes));}
    private void Add(ref long value,long amount,long limit)
    {token.ThrowIfCancellationRequested();if(amount<0||amount>limit-value)throw new ArgumentException("Whole ordinary graph work budget exceeded.");value+=amount;}
    private void Bytes(long count)=>Add(ref bytes,count,maximumBytes);
    internal void PreflightSeeds(LegacyOrdinaryCursiveSeedResult actual,int physicalCount)
    {
        token.ThrowIfCancellationRequested();int n=actual.Seeds.Count;
        if(n is<1 or>4096||physicalCount is<1 or>256)throw new ArgumentException("Bounded whole source required.");
        var ids=new HashSet<int>();foreach(var seed in actual.Seeds){token.ThrowIfCancellationRequested();int id=seed.Atom.Range.StrokeIndex;if((uint)id>=(uint)physicalCount)throw new ArgumentException("Actual physical seed identity required.");ids.Add(id);}
        long memberships=checked(10L*n*ids.Count),unionWork=checked(20L*n*ids.Count),links=checked(20L*n);
        if(memberships>1048576||unionWork>1048576||links>131072)throw new ArgumentException("Conservative seed context work budget exceeded.");
        //Hash-set members, all mutable/final seed arcs/edge vectors/arrays,
        //before+after union temporaries and subsequent pruning snapshot copies.
        Bytes(checked(memberships*32+unionWork*8+links*16+5L*n*256+4L*1024*1024));
    }
    internal void InitialGroups(int count){Groups(count,count);Bytes(checked(256L*count));}
    internal void InitialEdges(long count){Add(ref edges,count,maximumEdges);Bytes(checked(count*16));}
    internal void Composition(int count)
    {if(count is<1 or>8)throw new ArgumentException("Actual cap8 path required.");Add(ref compose,count,maximumCompose);Bytes(checked(1024L+256L*count));}
    internal void Groups(int seedCount,int rangeCount)
    {Add(ref seeds,seedCount,maximumRanges);Add(ref ranges,rangeCount,maximumRanges);Bytes(checked(seedCount*16L+rangeCount*32L+128));}
    internal void MergedEdges(int predecessors,int successors)
    {long count=checked(2L*(predecessors+successors));Add(ref edges,count,maximumEdges);Bytes(checked(count*16+256));}
    internal void PruningCopies(int nodeCount,int groupCount,int candidates)
    {
        //Both connection-index constructions use15 selector lists per arc;
        //pruning also keeps ordinal/remap/cost arrays and a final graph image.
        Bytes(checked(edges*128+ranges*24+seeds*8+nodeCount*256L+groupCount*128L+candidates*4096L+1024));
    }
    internal void FinalCopies(int nodeCount,int groupCount,int candidates)
    {Bytes(checked(edges*16+ranges*24+seeds*8+nodeCount*32L+groupCount*64L+candidates*128L));}
}
