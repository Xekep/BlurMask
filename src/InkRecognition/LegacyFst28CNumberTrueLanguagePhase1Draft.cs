using System.Buffers.Binary;

namespace InkRecognition;

// SOURCE ONLY; whole entry remains closed before the prefix claim. Physical157
// is an explicit safe scratch adaptation for the native transient row156 store,
// not a claim of original cross-node memory-layout equivalence.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    private sealed partial class NumericContinuation
    {
        private int finalization,sourceEnd,retainedEnd,cleanup,cumulative2520,backward2528;
        internal void FinalizeActualContinuation(CancellationToken token)
        {
            Validate(token);parentReadScope.Close();
            if(Interlocked.CompareExchange(ref finalization,1,0)!=0)
                throw new InvalidOperationException("Private numeric finalization is consumed or poisoned.");
            // Exact actual node0/header1 source only; no caller-prepared headers.
            var continuation=new LegacyNativeWordRecordComparer(LegacyNativeWordRecordComparison.ContinuationFull1fcb50,[0u,1u]);
            var storedComparer=new LegacyNativeStoredWordComparer([0u,1u]);
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();Known(heapMask,i*128,88);
                if(heap[i*128+4]!=1||I32(heap,i*128+88)!=i)
                    throw new InvalidDataException("Actual ordered source1 continuation ordinals required.");
            }
            SortOwned(false,0,count-1,continuation,storedComparer,token);
            int unique=count==0?0:1;
            for(int i=1;i<count;i++)
            {
                token.ThrowIfCancellationRequested();int leader=(unique-1)*128,other=i*128;
                if(continuation.Compare(heap.AsSpan(leader,128),heap.AsSpan(other,128))==0)
                    MergeSecondary(leader,other,token); // Never primary merge here.
                else{if(unique!=i){Copy(heap,other,unique*128,128);Copy(heapMask,other,unique*128,128);}unique++;}
            }
            int n=0;Span<byte> row=stackalloc byte[128];Span<byte> mask=stackalloc byte[128];
            for(int i=0;i<unique;i++)
            {
                token.ThrowIfCancellationRequested();heap.AsSpan(i*128,128).CopyTo(row);heapMask.AsSpan(i*128,128).CopyTo(mask);
                n=phase0.Insert(n,DescriptorCapacity,row,mask);
            }
            count=n;int destination=125;
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();int from=i*128,at=destination*88;
                Known(heapMask,from+88,4);int ordinal=I32(heap,from+88);
                if((uint)ordinal>=H||destination is <125 or >156)
                    throw new InvalidDataException("Bounded actual append ordinal/destination required.");
                heap.AsSpan(from,88).CopyTo(parents.AsSpan(at));heapMask.AsSpan(from,88).CopyTo(parentMasks.AsSpan(at));
                if(U16(parents,at+60)==65535)W16(parents,parentMasks,at+60,(ushort)destination);
                //213410 primary branch is false at every destination>=125.
                secondary.AsSpan(ordinal*80,80).CopyTo(parentSecondary.AsSpan(destination*80));
                secondaryMask.AsSpan(ordinal*80,80).CopyTo(parentSecondaryMasks.AsSpan(destination*80));
                // Preserve literal write THEN increment THEN unsignedJA.
                destination++;
                if(destination>156){destination=156;break;}
            }
            sourceEnd=destination;
            // Only published125..sourceEnd-1, not transient row156, is compacted.
            retainedEnd=CompactStored(storedComparer,continuation,token);
            CleanupPrimary(token);
            cumulative2520=0;backward2528=0;
            for(int i=0;i<sourceEnd;i++)
            {
                token.ThrowIfCancellationRequested();int at=i*88;Known(parentMasks,at+16,4);
                if(I32(parents,at+16)>=IMP)continue;
                Known(parentMasks,at+4,1);Known(parentMasks,at+52,8);
                if(parents[at+4]!=1||I32(parents,at+52)!=0||I32(parents,at+56)!=0)
                    throw new NotSupportedException("Bounded actual source1 node0 inverse tail differs.");
                // Native222.V178 -> ordinary21B.VD0(0) ==0. Sourcekind1&1E00
                // is0, no English dictionary category or parent history borrowed.
            }
            if(sourceEnd is <125 or >156||retainedEnd<125||retainedEnd>sourceEnd||cleanup>250||
                cumulative2520!=0||backward2528!=0||phase0.primary.Length!=125*80||
                parents.Length!=157*88||parentSecondary.Length!=157*80)
                throw new InvalidDataException("Private numeric node0 finalization bounds differ.");
            Volatile.Write(ref finalization,2);
            // No logical reset receipt, live source reset, output or Ready.
        }
        private void SortOwned(bool stored,int lower,int upper,LegacyNativeWordRecordComparer continuation,
            LegacyNativeStoredWordComparer storedComparer,CancellationToken token)
        {
            if(upper<=lower)return;
            var bytes=stored?parents:heap;var masks=stored?parentMasks:heapMask;int width=stored?88:128;
            var pending=new int[500];int top=0;pending[top++]=lower;pending[top++]=upper;
            while(top!=0)
            {
                token.ThrowIfCancellationRequested();upper=pending[--top];lower=pending[--top];int left=lower,right=upper;
                byte[] pivot=bytes.AsSpan(((upper-lower)/2+lower)*width,width).ToArray();
                int Compare(int index)=>stored?storedComparer.Compare(bytes.AsSpan(index*width,width),pivot):
                    continuation.Compare(bytes.AsSpan(index*width,width),pivot);
                do
                {
                    token.ThrowIfCancellationRequested();while(Compare(left)<0){left++;if(left>upper)throw new InvalidDataException("Owned comparison upper bound differs.");}
                    while(Compare(right)>0){right--;if(right<lower)throw new InvalidDataException("Owned comparison lower bound differs.");}
                    if(right<left)break;SwapOwned(stored,left,right);left++;right--;
                }while(left<=right);
                if(left<upper){if(top>pending.Length-2)throw new InvalidDataException("Owned sort partition bound differs.");pending[top++]=left;pending[top++]=upper;}
                if(lower<right){if(top>pending.Length-2)throw new InvalidDataException("Owned sort partition bound differs.");pending[top++]=lower;pending[top++]=right;}
            }
        }
        private void SwapOwned(bool stored,int a,int b)
        {
            if(a==b)return;
            var bytes=stored?parents:heap;var masks=stored?parentMasks:heapMask;int width=stored?88:128;
            SwapBytes(bytes,a*width,b*width,width);SwapBytes(masks,a*width,b*width,width);
            if(stored){SwapBytes(parentSecondary,a*80,b*80,80);SwapBytes(parentSecondaryMasks,a*80,b*80,80);}
        }
        private static void SwapBytes(byte[] bytes,int a,int b,int width)
        {Span<byte> temporary=stackalloc byte[128];bytes.AsSpan(a,width).CopyTo(temporary);bytes.AsSpan(b,width).CopyTo(bytes.AsSpan(a));temporary[..width].CopyTo(bytes.AsSpan(b));}
        private static void Copy(byte[] bytes,int from,int to,int n)=>bytes.AsSpan(from,n).CopyTo(bytes.AsSpan(to,n));
        private int CompactStored(LegacyNativeStoredWordComparer storedComparer,
            LegacyNativeWordRecordComparer continuation,CancellationToken token)
        {
            if(sourceEnd<=125)return sourceEnd;
            for(int i=125;i<sourceEnd;i++)
            {token.ThrowIfCancellationRequested();Known(parentMasks,i*88,88);if(parents[i*88+4]!=1)throw new InvalidDataException("Only actual header1 stored rows admitted.");}
            SortOwned(true,125,sourceEnd-1,continuation,storedComparer,token);
            int leader=125;
            for(int next=126;next<sourceEnd;next++)
            {
                token.ThrowIfCancellationRequested();
                if(storedComparer.Compare(parents.AsSpan(leader*88,88),parents.AsSpan(next*88,88))==0)
                {if(I32(parents,next*88+16)<I32(parents,leader*88+16))SwapOwned(true,leader,next);}
                else{leader++;SwapOwned(true,leader,next);}
            }
            // All authentic source headers are1, hence kind0. Native212B10
            // dictionary0200..0A00 rescue scan cannot select any such record.
            return leader+1;
        }
        private void CleanupPrimary(CancellationToken token)
        {
            var primary=phase0.primary;var masks=phase0.primaryMask;
            for(int i=0;i<250;i++)
            {
                token.ThrowIfCancellationRequested();int at=i*40;
                bool ck=All(masks,at,4),fk=(masks[at+31]&32)!=0,pk=All(masks,at+24,2);
                bool skip=ck&&I32(primary,at)>=IMP||fk&&(primary[at+31]&32)!=0||pk&&U16(primary,at+24)<125;
                if(I32(primary,at)<IMP&&(primary[at+31]&32)==0&&U16(primary,at+24)>=125)
                {W(primary,masks,at,IMP);cleanup++;if(!(ck&&fk&&pk))masks.AsSpan(at,4).Clear();}
                if(!(ck&&fk&&pk||skip))masks.AsSpan(at,4).Clear();
            }
        }
        private static bool All(byte[] bytes,int at,int n)
        {for(int i=0;i<n;i++)if(bytes[at+i]!=255)return false;return true;}
    private readonly record struct Pick(int Row,int Index,ulong Identity);
    private void MergeSecondary(int leader,int duplicate,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();bool primary=false;var p=secondary;var m=secondaryMask;
        int a=I32(heap,leader+88),b=I32(heap,duplicate+88);
        if(!primary&&I32(p,b*80)<I32(p,a*80))
        {SwapBytes(heap,leader,duplicate,128);SwapBytes(heapMask,leader,duplicate,128);(a,b)=(b,a);}
        bool heapCosts=primary&&(p[a*80+31]&p[b*80+31]&32)!=0;
        bool firstA=!primary||(heapCosts?I32(heap,leader+16):I32(p,a*80))<(heapCosts?I32(heap,duplicate+16):I32(p,b*80));
        Span<Pick> chosen=stackalloc Pick[2];chosen[0]=new(firstA?a:b,0,U64(p,(firstA?a:b)*80+8));
        int n=1,ai=firstA?1:0,bi=firstA?0:1;
        while(n<2)
        {
            bool av=ai<2&&I32(p,a*80+ai*40)<IMP,bv=bi<2&&I32(p,b*80+bi*40)<IMP;if(!av&&!bv)break;
            bool left=av&&(!bv||I32(p,a*80+ai*40)<I32(p,b*80+bi*40));int row=left?a:b,index=left?ai++:bi++;
            ulong id=U64(p,row*80+index*40+8);if(id!=chosen[0].Identity)chosen[n++]=new(row,index,id);
        }
        for(int i=n-1;i>=0;i--)
        {
            var pick=chosen[i];if(pick.Row==a&&pick.Index==i)continue;
            int src=pick.Row*80+pick.Index*40,dst=a*80+i*40;
            // Mirror every LIVE scalar/block write, including source mask
            // replacement. Unknown source bits must erase prior knowledge.
            for(int mode=0;mode<2;mode++)
            {
                var x=mode==0?p:m;Copy(x,src,dst,8);x[dst+30]=x[src+30];Copy(x,src+24,dst+24,4);Copy(x,src+8,dst+8,16);
                int mask=primary?63:31;x[dst+31]=(byte)((x[dst+31]&~mask)|(x[src+31]&mask));
                if(!primary)Copy(x,src+32,dst+32,4);
            }
        }
        if(!primary)
        {
            int best=a*80;
            for(int mode=0;mode<2;mode++)
            {
                var x=mode==0?p:m;var h=mode==0?heap:heapMask;
                x.AsSpan(best,8).CopyTo(h.AsSpan(leader+16));h[leader+38]=x[best+30];
                x.AsSpan(best+24,2).CopyTo(h.AsSpan(leader+30));h[leader+32]=x[best+26];
                h[leader+33]=(byte)((h[leader+33]&192)|(x[best+31]&31)|(mode==1?32:0));
                x.AsSpan(best+32,2).CopyTo(h.AsSpan(leader+62));
            }
        }
    }
    }
}
