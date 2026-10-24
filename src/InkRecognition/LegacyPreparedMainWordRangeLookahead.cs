using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalBatch.Probe")]
namespace InkRecognition;

//Numerical0283B0 WORD-level lookahead only. Arrays are DATA, never source/claim authority.
internal static class LegacyPreparedMainWordRangeLookahead
{
    internal sealed record Result(LegacyNativeInkPositionRange[][] Keys,long Pairs,long CopiedRangePayload);
    internal static Result Run(LegacyOwnedRawInkRangeOperations operations,IReadOnlyList<LegacyNativeInkPositionRange[]> originals,
        long maximumPairs,long maximumRangePayload,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(operations);ArgumentNullException.ThrowIfNull(originals);token.ThrowIfCancellationRequested();
        if(originals.Count is <1 or >4096||maximumPairs is <0 or >8_386_560||maximumRangePayload is <0 or >128L*1024*1024)
            throw new NotSupportedException("Bounded WORD queue and explicit range-work budget required.");
        long initial=0;foreach(var key in originals){if(key is null||key.Length>65536)throw new ArgumentException("Complete bounded projected WORD keys required.");initial=checked(initial+key.Length);}
        if(initial>65536||checked(16*initial)>maximumRangePayload)throw new NotSupportedException("Initial whole WORD range payload exceeds preflight.");
        //Maximum pair count is known before copying/mutating any queued key.
        long pairBound=checked((long)originals.Count*(originals.Count-1)/2);
        if(pairBound>maximumPairs)throw new NotSupportedException("WORD lookahead pair envelope exceeded before entry.");
        var keys=new LegacyNativeInkPositionRange[originals.Count][];long copied=0,pairs=0;
        for(int i=0;i<keys.Length;i++){Charge(originals[i].Length);keys[i]=originals[i].ToArray();}
        for(int current=0;current<keys.Length;current++)
        {
            token.ThrowIfCancellationRequested();var key=keys[current];
            for(int later=current+1;later<keys.Length;later++)
            {
                token.ThrowIfCancellationRequested();if(++pairs>maximumPairs)throw new NotSupportedException("WORD lookahead pairs exhausted.");
                int a=keys[later].Length,b=key.Length;
                //Intersect's output loop is bounded by this exact product even for
                //noncanonical DATA. Refuse before invoking allocation-producing helper.
                long outputBound=checked((long)a*b);
                if(outputBound>65536)throw new NotSupportedException("WORD range intersection expansion exceeds subset.");
                Charge(checked(3L*a+5L*outputBound));
                //Native V88 is an exact copy, not Union/normalization of later key.
                var overlap=operations.Intersect(keys[later].ToArray(),key,token);
                if(overlap.Length==0)continue;
                bool equal=LegacyOwnedRawInkRangeOperations.Equal(key,overlap);
                int target=equal?a:b;long differenceBound=checked((long)target+overlap.Length);
                if(differenceBound>65536)throw new NotSupportedException("WORD range subtraction expansion exceeds subset.");
                Charge(checked(2L*target+6L*differenceBound));
                if(equal)keys[later]=operations.Subtract(keys[later],overlap,token);
                else key=operations.Subtract(key,overlap,token);
                //Native breaks on the first nonempty intersection only.
                break;
            }
            keys[current]=key;
        }
        token.ThrowIfCancellationRequested();return new(keys,pairs,copied);
        void Charge(long rows)
        {
            long bytes=checked(rows*16);
            if(bytes>maximumRangePayload-copied)throw new NotSupportedException("Cumulative WORD range payload exhausted before helper growth.");
            copied+=bytes;
        }
    }
}
