namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalSourceBatchCompletion
{
    internal sealed partial class CompletedTextWords
    {
        private EqualityReadSet? equalityReadSet;
        internal void RequireEqualityReadSet(CancellationToken token)
        {
            Require(token);
            if(equalityReadSet is null)return; //Same native span performs no equality.
            int n=equalityReadSet.ReferenceCount;
            owner.DebitValidationLoops(checked(2L*n),token);
            //Actual CopyRecord40 requests are precharged before ANY read.
            long bytes=checked(40L*equalityReadSet.CapturedCount);
            if(bytes>copyLimit-EnumeratedCopyBytes)
                throw new NotSupportedException("Equality readset validation exceeds remaining TEXT copy envelope.");
            EnumeratedCopyBytes+=bytes;
            equalityReadSet.RequireUnchanged(this,token);
        }
        private sealed class EqualityReadSet
        {
            private readonly CompletedTextWords issuer;
            private readonly LegacyNativeGlyphRecordPool pool;
            private readonly ulong[] references;
            private readonly byte[]?[] records;
            private readonly int thread=Environment.CurrentManagedThreadId;
            private readonly int referenceCount;
            internal int CapturedCount{get;private set;}
            internal int ReferenceCount=>referenceCount;
            internal EqualityReadSet(CompletedTextWords issuer,LegacyNativeGlyphRecordPool pool,
                LegacyNativePlainWordSpanSnapshot[] owners,int maximumReferences)
            {
                this.issuer=issuer;this.pool=pool;
                if(maximumReferences is<1 or>65536)throw new NotSupportedException("Bounded genuine equality references required.");
                references=new ulong[maximumReferences];records=new byte[maximumReferences][];int n=0;
                foreach(var owner in owners)foreach(var word in owner.Words)foreach(ulong reference in word.FlatReferences)
                {
                    if(n==maximumReferences)throw new InvalidOperationException("Actual equality references exceed charged queue extent.");
                    references[n++]=reference;
                }
                Array.Sort(references,0,n);int unique=0;
                for(int i=0;i<n;i++)if(i==0||references[i]!=references[i-1])references[unique++]=references[i];
                referenceCount=unique;
            }
            internal void Observe(ulong reference,byte[] actualRead40)
            {
                if(thread!=Environment.CurrentManagedThreadId||actualRead40.Length!=40)
                    throw new InvalidOperationException("Same-thread native equality read required.");
                if((actualRead40[6]&2)==0&&(actualRead40[4]&7)==3)
                    throw new NotSupportedException("Shared WORD equality kind3 requires genuine caller scalar/control context.");
                //At most17 probes over the <=65536 actual flat references.
                issuer.owner.DebitValidationLoops(18,default);
                int lo=0,hi=referenceCount-1;
                while(lo<=hi)
                {
                    int mid=lo+(hi-lo)/2;
                    if(references[mid]<reference){lo=mid+1;continue;}
                    if(references[mid]>reference){hi=mid-1;continue;}
                    if(records[mid] is{} before)
                    {
                        if(!LegacyPreparedGlobalWordPayloadReuse.SameEqualityInputs(before,actualRead40))throw new InvalidOperationException("Glyph record changed within equality preflight.");
                    }
                    else{records[mid]=actualRead40;CapturedCount++;}
                    return;
                }
                throw new NotSupportedException("Equality traversed outside the genuine completed WORD flat-reference domain.");
            }
            internal void RequireUnchanged(CompletedTextWords actual,CancellationToken token)
            {
                if(!ReferenceEquals(actual,issuer)||thread!=Environment.CurrentManagedThreadId)
                    throw new InvalidOperationException("Exact private equality readset issuer/thread required.");
                for(int i=0;i<referenceCount;i++)
                {
                    token.ThrowIfCancellationRequested();
                    if(records[i] is{} before&&!LegacyPreparedGlobalWordPayloadReuse.SameEqualityInputs(pool.CopyRecord(references[i],token),before))
                        throw new InvalidOperationException("Native equality consumed glyph fields changed before WORD payload reuse.");
                }
            }
        }
    }
}
