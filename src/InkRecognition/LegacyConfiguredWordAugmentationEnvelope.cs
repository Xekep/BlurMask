using System.Buffers.Binary;
namespace InkRecognition;

// Selected numeric call/copy envelope, not CLR/RSS/whole CPU certification.
// All bounds depend on privately copied compact/starts and actual275 lengths.
internal sealed class LegacyConfiguredWordAugmentationEnvelope
{
    internal const long WorkLimit=64L*1024*1024,CopyLimit=128L*1024*1024;
    internal readonly record struct Bounds(int OriginalWords,int CandidateRows,int Depth,int MaximumChain,
        long Parents,long Lookahead,long Rows,long Requests,long Equalities,long EqualityGlyphVisits,long CopyBytes,long Work);
    internal Bounds Estimate {get;}
    private long requests,equalities;
    private LegacyConfiguredWordAugmentationEnvelope(Bounds bounds){Estimate=bounds;}
    internal static LegacyConfiguredWordAugmentationEnvelope Prepare(byte[] compact,int[] starts,int nodes,
        LegacyNativePlainWordSpanSnapshot span,int compactBackingBytes,int historyCount,CancellationToken token)
    {
        int n=compact.Length/40,o=span.Words.Count;
        if(n is<1 or>131072||compact.Length!=40*n||nodes is<1 or>120||starts.Length!=nodes+1||
            starts[0]!=0||starts[^1]!=n||o is<1 or>4095||span.SelectedNodes.Count!=2||historyCount is<2 or>4096||compactBackingBytes<compact.Length||compactBackingBytes>64*1024*1024)
            throw new NotSupportedException("Complete genuine configured compact and two selected279 heads required.");
        long originalReferences=0;int maximumChain=0;
        foreach(var word in span.Words)
        {
            token.ThrowIfCancellationRequested();int k=word.FlatReferences.Count;
            if(k is<1 or>4096||k>65536-originalReferences)throw new NotSupportedException("Complete bounded original275 references required.");
            var chain=LegacyNativeWordBuilder.CopyChain(word.Pool,word.HeadReference,token);
            if(chain.Length!=k)throw new InvalidDataException("Actual linked WORD extent and flat references disagree.");
            for(int i=0;i<k;i++){token.ThrowIfCancellationRequested();if(chain[k-1-i]!=word.FlatReferences[i])throw new InvalidDataException("Actual WORD reading-order ownership changed.");}
            originalReferences+=k;maximumChain=Math.Max(maximumChain,k);
        }
        for(int i=0;i<starts.Length;i++)
        {token.ThrowIfCancellationRequested();if(starts[i]<0||starts[i]>n||i>0&&starts[i]<starts[i-1])throw new InvalidDataException("Monotone actual compact starts required.");}
        var depth=new int[n];var path=new int[n];int d=0;
        for(int i=0;i<n;i++)
        {
            token.ThrowIfCancellationRequested();int p=I32(compact,i*40+16);ulong key=BinaryPrimitives.ReadUInt64LittleEndian(compact.AsSpan(i*40+8));
            int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;
            if(p < -1||p>=n||node < -1||node>=nodes)throw new InvalidDataException("Actual compact parent/node extent required.");
        }
        for(int root=0;root<n;root++)
        {
            token.ThrowIfCancellationRequested();if(depth[root]!=0)continue;int cursor=root,top=0;
            while(cursor!=-1&&depth[cursor]==0)
            {token.ThrowIfCancellationRequested();depth[cursor]=-1;path[top++]=cursor;cursor=I32(compact,cursor*40+16);}
            if(cursor!=-1&&depth[cursor]<0)throw new InvalidDataException("Actual compact parent cycle.");
            int length=cursor==-1?0:depth[cursor];while(top>0){token.ThrowIfCancellationRequested();depth[path[--top]]=++length;}d=Math.Max(d,length);
        }
        int last=LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(span.Span.Last),q=0;
        if((uint)last>=(uint)nodes)throw new NotSupportedException("Actual target span-last node required.");
        for(int i=starts[last];i<starts[last+1];i++){token.ThrowIfCancellationRequested();if((compact[i*40+32]&8)!=0)q++;}
        if(q>4095-o)throw new NotSupportedException("Complete potential275 growth exceeds existing4095 row domain.");
        maximumChain=Math.Max(maximumChain,Math.Min(d,4096));
        long parents=checked(2L*q*d),look=checked((long)q*d*d),rows=checked((long)q*d),requests=checked(8L*q*(d+1));
        long eq=checked((long)q*q+(2L*o-1)*q+(long)o*(o-1)/2),visits=checked(eq*(4L*maximumChain+2));
        // Existing source helper bounds4096 range rows, independent of raw point
        // count. No new4096-point admission is imposed on original16K ingress.
        long copy=checked(compactBackingBytes+56L*originalReferences+48L*n+4L*starts.Length+4096L*104+rows*288+parents*16+look*32+requests*128+
            visits*40+(6L*q+12)*(240L*(o+q)+16L*(originalReferences+rows))+4096);
        long work=checked(2L*1048576+2L*4096*4096+4L*originalReferences+(2L*q+80)*historyCount+12L*(q+1)*(o+q)+7L*n+parents+look+rows+requests+visits);
        if(copy>CopyLimit||work>WorkLimit)throw new NotSupportedException("Configured augmentation selected numeric envelope exceeded.");
        return new(new(o,q,d,maximumChain,parents,look,rows,requests,eq,visits,copy,work));
    }
    internal void Request(){if(++requests>Estimate.Requests)throw new NotSupportedException("Configured callback request reservation exceeded.");}
    internal void Equality(){if(++equalities>Estimate.Equalities)throw new NotSupportedException("Configured equality reservation exceeded.");}
    private static int I32(byte[] b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(at));
}
