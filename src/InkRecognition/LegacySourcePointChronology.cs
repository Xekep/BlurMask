namespace InkRecognition;

/// <summary>Three signed native source-key fields; no timestamp meaning is inferred.</summary>
public readonly record struct LegacySourcePointKey(int First,int Second,int Third);
public readonly record struct LegacySourcePointIdentity(LegacySourcePointKey Key,
    int MetadataPointIndex,int KeyRecordIndex,byte RemainderByte);

/// <summary>
/// Source218 point-key getter168d00 and map composition16aa00. Preserves
/// original metadata through inserted or explicitly resampled prepared points.
/// Range chronology ordering restores1b49a0 for glyph normalization, not the
/// upstream native line-DAG seed ordering.
/// </summary>
public sealed class LegacySourcePointChronology
{
    private readonly int[] counts,starts,pointKeyIndices;
    private readonly LegacySourcePointKey[] keys;
    private readonly uint[] pointMap;
    public ReadOnlyMemory<int> StrokePointCounts=>counts;
    public ReadOnlyMemory<uint> PreparedPointMap=>pointMap;
    public uint SourceFactor {get;}
    private LegacySourcePointChronology(int[] counts,LegacySourcePointKey[] keys,
        int[] pointKeyIndices,uint[] pointMap,uint sourceFactor)
    {
        this.counts=counts;this.keys=keys;this.pointKeyIndices=pointKeyIndices;this.pointMap=pointMap;SourceFactor=sourceFactor;
        starts=new int[counts.Length];for(int i=1;i<counts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        for(int i=0;i<counts.Sum();i++)ResolveGlobal(i);
    }

    /// <summary>
    /// Normal unsampled219 ingestion starts key field1 at zero, field2 at
    /// input-stroke ordinal, field3 at original point ordinal. No custom source
    /// hierarchy, opaque source or extended metadata ingestion is inferred.
    /// </summary>
    public static LegacySourcePointChronology CreateOrdinary(ReadOnlySpan<int> sourcePointCounts)
    {
        int[] counts=Counts(sourcePointCounts);int total=counts.Sum();
        var keys=new LegacySourcePointKey[total];int global=0;
        for(int s=0;s<counts.Length;s++)for(int p=0;p<counts[s];p++)keys[global++]=new(0,s,p);
        // Fresh native219 secondary16A130 initializes source+128 to200.
        // Retain fractional265 map ticks through the later metadata lookup.
        return new(counts,keys,Enumerable.Range(0,total).ToArray(),[],200);
    }

    /// <summary>
    /// Copy explicit native point keys, original-point-to-key record indices,
    /// current stroke counts and optional prepared fixed-point map. The source
    /// factor is native secondary+128. All retained inputs are owned snapshots.
    /// </summary>
    public static LegacySourcePointChronology CreatePrepared(ReadOnlySpan<int> currentPointCounts,
        ReadOnlySpan<LegacySourcePointKey> keyRecords,ReadOnlySpan<int> originalPointKeyIndices,
        ReadOnlySpan<uint> preparedPointMap=default,uint sourceFactor=1)
    {
        int[] counts=Counts(currentPointCounts);int total=counts.Sum();
        if(sourceFactor==0)throw new ArgumentOutOfRangeException(nameof(sourceFactor));
        if(keyRecords.Length is <1 or >4096 || originalPointKeyIndices.Length is <1 or >4096)
            throw new ArgumentException("Expected1..4096 key records and original-point indices.");
        foreach(int index in originalPointKeyIndices)
            if((uint)index>=(uint)keyRecords.Length)throw new ArgumentException("Invalid original-point key-record index.",nameof(originalPointKeyIndices));
        if(!preparedPointMap.IsEmpty && preparedPointMap.Length!=total)
            throw new ArgumentException("Prepared map must address every current point.",nameof(preparedPointMap));
        return new(counts,keyRecords.ToArray(),originalPointKeyIndices.ToArray(),preparedPointMap.ToArray(),sourceFactor);
    }

    public LegacySourcePointIdentity Resolve(int strokeIndex,int pointIndex)
    {
        if((uint)strokeIndex>=(uint)counts.Length || (uint)pointIndex>=(uint)counts[strokeIndex])
            throw new ArgumentOutOfRangeException(nameof(pointIndex));
        return ResolveGlobal(starts[strokeIndex]+pointIndex);
    }
    private LegacySourcePointIdentity ResolveGlobal(int global)
    {
        uint metadataIndex=(uint)global;byte remainderByte=0;
        if(pointMap.Length!=0)
        {
            uint ticks=pointMap[global],whole=ticks/SourceFactor,remainder=ticks%SourceFactor;
            metadataIndex=checked(whole+(remainder==0 ? 0u:1u));
            if(remainder!=0)remainderByte=unchecked((byte)(SourceFactor-remainder));
        }
        if(metadataIndex>=(uint)pointKeyIndices.Length)
            throw new ArgumentException("Mapped point is outside original point metadata.");
        int keyIndex=pointKeyIndices[(int)metadataIndex];
        return new(keys[keyIndex],(int)metadataIndex,keyIndex,remainderByte);
    }

    /// <summary>
    /// Native16aa00 first map branch. Composes incoming fixed-point indices
    /// through an explicit previous map, or scales them when no previous map
    /// exists. All differences/products/additions use native uint32 wrap;
    /// quotient division remains unsigned. SourceFactor is retained afterward.
    /// </summary>
    public static uint[] ComposePointMap(ReadOnlySpan<uint> incomingTicks,uint incomingFactor,
        ReadOnlySpan<uint> previousMap,uint sourceFactor,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(incomingTicks.Length is <1 or >4096 || previousMap.Length>4096)
            throw new ArgumentException("Point maps require1..4096 incoming entries and at most4096 previous entries.");
        if(incomingFactor==0 || sourceFactor==0)throw new ArgumentOutOfRangeException(nameof(incomingFactor));
        var result=new uint[incomingTicks.Length];
        for(int i=0;i<result.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();uint tick=incomingTicks[i];
            if(previousMap.IsEmpty)
                result[i]=incomingFactor==sourceFactor ? tick:unchecked(sourceFactor*tick)/incomingFactor;
            else
            {
                uint whole=tick/incomingFactor,remainder=tick%incomingFactor;
                if(whole>=(uint)previousMap.Length || remainder!=0 && whole+1>=(uint)previousMap.Length)
                    throw new ArgumentException("Incoming fixed-point position is outside previous point map.",nameof(incomingTicks));
                uint delta=remainder==0 ? 0u:unchecked((previousMap[(int)whole+1]-previousMap[(int)whole])*remainder)/incomingFactor;
                result[i]=unchecked(previousMap[(int)whole]+delta);
            }
        }
        return result;
    }

    public LegacySourcePointChronology RemapPreparedPoints(ReadOnlySpan<int> newPointCounts,
        ReadOnlySpan<uint> incomingTicks,uint incomingFactor=1,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(incomingFactor==0)throw new ArgumentOutOfRangeException(nameof(incomingFactor));
        int[] nextCounts=Counts(newPointCounts);int oldTotal=counts.Sum();
        if(nextCounts.Sum()!=incomingTicks.Length)throw new ArgumentException("New point counts differ from incoming map.",nameof(incomingTicks));
        // Incoming coordinates address the previous prepared source, even if
        // its metadata map was previously implicit.
        foreach(uint tick in incomingTicks)
        {
            uint whole=tick/incomingFactor,remainder=tick%incomingFactor;
            if(whole>=(uint)oldTotal || remainder!=0 && whole+1>=(uint)oldTotal)
                throw new ArgumentException("Incoming point lies outside previous prepared ink.",nameof(incomingTicks));
        }
        var nextMap=ComposePointMap(incomingTicks,incomingFactor,pointMap,SourceFactor,cancellationToken);
        return new(nextCounts,keys.ToArray(),pointKeyIndices.ToArray(),nextMap,SourceFactor);
    }

    /// <summary>
    /// Bridge native191950/18dde0's global original-index map into source218's
    /// virtual30 update (incoming factor1). The provided prepared ink must have
    /// been produced from this object's current coordinate source.
    /// </summary>
    public LegacySourcePointChronology WithInsertedPreparedInk(LegacyCursivePreparedInk prepared,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(prepared);cancellationToken.ThrowIfCancellationRequested();
        var map=prepared.Points.Span.ToArray().Select(p=>checked((uint)p.OriginalPointIndex)).ToArray();
        return RemapPreparedPoints(prepared.StrokePointCounts.Span,map,1,cancellationToken);
    }

    /// <summary>
    /// Restore1b49a0's signed lexicographic middle-pivot quicksort of start
    /// metadata. Equal keys intentionally keep native swaps, not stable input
    /// order. Returns input ordinals; does not change ranges or atom ownership.
    /// </summary>
    public int[] OrderRangeIndicesForNormalization(ReadOnlySpan<LegacyInkRange> ranges,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(ranges.Length is <1 or >4096)throw new ArgumentException("Expected1..4096 candidate ranges.",nameof(ranges));
        var sortKeys=new LegacySourcePointKey[ranges.Length];
        for(int i=0;i<ranges.Length;i++)
        {
            var r=ranges[i];
            if((uint)r.StrokeIndex>=(uint)counts.Length || r.StartPoint<0 || r.EndPointExclusive<=r.StartPoint || r.EndPointExclusive>counts[r.StrokeIndex])
                throw new ArgumentException("Range lies outside current prepared ink.",nameof(ranges));
            sortKeys[i]=Resolve(r.StrokeIndex,r.StartPoint).Key;
        }
        var indices=Enumerable.Range(0,ranges.Length).ToArray();var stack=new Stack<(int First,int Last)>();stack.Push((0,indices.Length-1));
        while(stack.Count!=0)
        {
            var (first,last)=stack.Pop();cancellationToken.ThrowIfCancellationRequested();
            int left=first,right=last;var pivot=sortKeys[indices[(first+last)/2]];
            while(left<=right)
            {
                while(Compare(sortKeys[indices[left]],pivot)<0)left++;
                while(Compare(sortKeys[indices[right]],pivot)>0)right--;
                if(left>right)break;
                (indices[left],indices[right])=(indices[right],indices[left]);left++;right--;
            }
            if(left<last)stack.Push((left,last));
            if(first<right)stack.Push((first,right));
        }
        return indices;
    }
    public int[] OrderAtomIndicesForNormalization(ReadOnlySpan<LegacyCursiveInkAtom> atoms,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return OrderRangeIndicesForNormalization(atoms.ToArray().Select(a=>a.Range).ToArray(),cancellationToken);
    }
    private static int Compare(LegacySourcePointKey a,LegacySourcePointKey b)
    {int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    private static int[] Counts(ReadOnlySpan<int> counts)
    {
        if(counts.Length is <1 or >256)throw new ArgumentException("Expected1..256 physical strokes.",nameof(counts));
        int total=0;foreach(int n in counts){if(n<1 || n>4096-total)throw new ArgumentException("Expected1..4096 total prepared points.",nameof(counts));total+=n;}
        return counts.ToArray();
    }
}
