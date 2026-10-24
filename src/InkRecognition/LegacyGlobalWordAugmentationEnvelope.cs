using System.Buffers.Binary;
namespace InkRecognition;
// Operational numeric envelope, not source-instruction/CLR/RSS certification.
// Preflight is before entered/Claim. Dynamic counters run before loop growth.
internal sealed class LegacyGlobalWordAugmentationEnvelope
{
 internal const long WorkLimit=64L*1024*1024, CopyLimit=128L*1024*1024;
 internal readonly record struct Estimate(int Count,int Candidates,long Parents,long Lookahead,long Rows,long Requests,long Equalities,long EqualityRecords,long CopyBytes,int MaximumParentDepth,int CandidateStart,int CandidateEnd);
 internal Estimate Bounds{get;}
 private long parents,lookahead,rows,requests,equalities,copied;
 private LegacyGlobalWordAugmentationEnvelope(Estimate bounds){Bounds=bounds;}
 internal static LegacyGlobalWordAugmentationEnvelope Prepare(ReadOnlySpan<byte> compact,ReadOnlySpan<int> starts,int nodes,int sourcePoints,CancellationToken token=default,int originalReferences=0,int? actualLastContext=null)
 {
  token.ThrowIfCancellationRequested();int n=compact.Length/40,q=0;
  if(compact.Length%40!=0||n is <1 or >131072||nodes is <1 or >4096||starts.Length!=nodes+1||sourcePoints is <1 or >4096||starts[0]!=0||starts[^1]!=n||originalReferences is <0 or >65536)
   throw new NotSupportedException("Preclaim complete compact/start/source geometry required.");
  for(int i=0;i<starts.Length;i++){token.ThrowIfCancellationRequested();if(starts[i]<0||starts[i]>n||i>0&&starts[i]<starts[i-1])throw new InvalidDataException("Preclaim monotone starts required.");}
  for(int i=0;i<n;i++)
  {
   token.ThrowIfCancellationRequested();var r=compact.Slice(i*40,40);int parent=BinaryPrimitives.ReadInt32LittleEndian(r[16..]);ulong key=BinaryPrimitives.ReadUInt64LittleEndian(r[8..]);int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;
   if(parent < -1||parent>=n||node < -1||node>=nodes)throw new InvalidDataException("Preclaim compact parent/node extent required.");

  }
  // Scan219580 visits only the actual span-last node interval. Default pure
  // callers retain the conservative whole-storage interval; live owner passes
  // its privately bound span.Last, never a diagnostic/imported selector.
  int candidateStart=0,candidateEnd=n;
  if(actualLastContext is int context)
  {
   int last=LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(context);
   if((uint)last>=(uint)nodes)throw new NotSupportedException("Preclaim source span-last must name an owned node.");
   candidateStart=starts[last];candidateEnd=starts[last+1];
  }
  for(int i=candidateStart;i<candidateEnd;i++)if((compact[i*40+32]&8)!=0)q++;
  // Functional parent graph: each row becomes active once and finishes once.
  // The exact immutable compact is this owner's private copy; no producer or
  // observer runs between this preflight and Claim. Reject cycles before entry.
  int[] depth=new int[n], path=new int[n];int maximumDepth=0;
  for(int root=0;root<n;root++)
  {
   token.ThrowIfCancellationRequested();if(depth[root]!=0)continue;
   int cursor=root,top=0;
   while(cursor!=-1&&depth[cursor]==0)
   {
    token.ThrowIfCancellationRequested();depth[cursor]=-1;path[top++]=cursor;
    cursor=BinaryPrimitives.ReadInt32LittleEndian(compact.Slice(cursor*40+16,4));
   }
   if(cursor!=-1&&depth[cursor]<0)throw new InvalidDataException("Preclaim compact parent graph is cyclic.");
   int length=cursor==-1?0:depth[cursor];
   while(top>0){token.ThrowIfCancellationRequested();depth[path[--top]]=++length;}
   maximumDepth=Math.Max(maximumDepth,length);
  }
  // Every direct walk visits at most D rows. Each emitted row's lookahead
  // is a suffix of that same acyclic parent chain, hence <=D visits.
  long parents=checked(2L*q*maximumDepth), look=checked((long)q*maximumDepth*maximumDepth), rows=checked((long)q*maximumDepth), requests=checked(6L*q*(maximumDepth+1));
  // Search sum(1..q), final ranking <=q(q+1)/2; existing equality reader caps131072.
  long eq=checked((long)q*(q+1)),eqRecords=checked(eq*131072);
  // Full output/temporary row copies, source count/fill+26N export, equality40,
  // and conservative repeated WORD/reference-vector snapshot payload. BCL backing
  // growth/object fields and producer/calibration internals are separate frontiers.
  long copy=checked(20L*n+eqRecords*40+parents*16+look*32+rows*288+requests*128+4096L*104+
    (6L*q+12)*(240L*(q+1)+16L*(originalReferences+(long)q*n))+n*40L+starts.Length*4L+4096);
  // Source helper has <=1048576 DFS iterations/read, <=4096 range rows.
  // Reserve both reads plus conservative quadratic stable-sort/compaction units.
  if(checked(2L*1048576+2L*4096*4096+5L*n+parents+look+rows+requests+eqRecords)>WorkLimit||copy>CopyLimit)
   throw new NotSupportedException("Preclaim cumulative augmentation structural envelope exceeded.");
  return new(new(n,q,parents,look,rows,requests,eq,eqRecords,copy,maximumDepth,candidateStart,candidateEnd));
 }
 internal void Parent(){if(++parents>Bounds.Parents)throw new NotSupportedException("Augmentation cumulative parent bound exceeded.");Copy(16);}
 internal void Lookahead(){if(++lookahead>Bounds.Lookahead)throw new NotSupportedException("Augmentation cumulative lookahead bound exceeded.");Copy(32);}
 internal void Row(){if(++rows>Bounds.Rows)throw new NotSupportedException("Augmentation cumulative emitted-row bound exceeded.");}
 internal void Request(){if(++requests>Bounds.Requests)throw new NotSupportedException("Augmentation cumulative callback request bound exceeded.");}
 internal void Equality(){if(++equalities>Bounds.Equalities)throw new NotSupportedException("Augmentation cumulative equality bound exceeded.");Copy(131072L*40);}
 internal void Copy(long count){if(count<0||count>Bounds.CopyBytes-copied)throw new NotSupportedException("Augmentation cumulative selected-copy bound exceeded.");copied+=count;}
}
