using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyGlobalWordAugmentationEnvelope
{
 internal sealed class BatchTopology
 {
  internal readonly byte[] Compact;internal readonly int[] Starts;internal readonly int Nodes,Count,MaximumDepth,CompactAllocationBytes;
  internal BatchTopology(byte[] compact,int[] starts,int nodes,int maximumDepth,int compactAllocationBytes)
  {Compact=compact;Starts=starts;Nodes=nodes;Count=compact.Length/40;MaximumDepth=maximumDepth;CompactAllocationBytes=compactAllocationBytes;}
  //These are structural traversal/copy reserves, not CPU/CLR/RSS certificates.
  internal int MaximumSourceRanges;
  internal long SourceGraphScanWork;
  internal long SourceSelectorCopyBytes;
  internal long SharedWork=>checked(5L*Count+SourceGraphScanWork);
  internal long SharedCopy=>checked(60L*Count+4L*Starts.Length+CompactAllocationBytes);
 }
 internal static BatchTopology PrepareBatchTopology(byte[] storage,int[] offsets,int nodes,int sourcePoints,int compactAllocationBytes,CancellationToken token)
 {
  ReadOnlySpan<byte> compact=storage;ReadOnlySpan<int> starts=offsets;
  token.ThrowIfCancellationRequested();int n=compact.Length/40;
  if(compact.Length%40!=0||n is <1 or >131072||nodes is <1 or >4096||starts.Length!=nodes+1||sourcePoints is <1 or >4096||starts[0]!=0||starts[^1]!=n)
   throw new NotSupportedException("Preclaim complete compact/start/source geometry required.");
  for(int i=0;i<starts.Length;i++){token.ThrowIfCancellationRequested();if(starts[i]<0||starts[i]>n||i>0&&starts[i]<starts[i-1])throw new InvalidDataException("Preclaim monotone starts required.");}
  for(int i=0;i<n;i++)
  {
   token.ThrowIfCancellationRequested();var r=compact.Slice(i*40,40);int parent=BinaryPrimitives.ReadInt32LittleEndian(r[16..]);ulong key=BinaryPrimitives.ReadUInt64LittleEndian(r[8..]);int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;
   if(parent < -1||parent>=n||node < -1||node>=nodes)throw new InvalidDataException("Preclaim compact parent/node extent required.");

  }
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
  return new(storage,offsets,nodes,maximumDepth,compactAllocationBytes);
 }
 internal static LegacyGlobalWordAugmentationEnvelope PrepareBatchSpan(BatchTopology topology,int lastContext,int originalReferences,CancellationToken token)
 {
  token.ThrowIfCancellationRequested();int n=topology.Count,maximumDepth=topology.MaximumDepth;
  int last=LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(lastContext);
  if((uint)last>=(uint)topology.Nodes||originalReferences is <0 or >65536)
   throw new NotSupportedException("Actual privately bound batch span/reference extent required.");
  int candidateStart=topology.Starts[last],candidateEnd=topology.Starts[last+1],q=0;
  for(int i=candidateStart;i<candidateEnd;i++){token.ThrowIfCancellationRequested();if((topology.Compact[i*40+32]&8)!=0)q++;}
  long parents=checked(2L*q*maximumDepth),look=checked((long)q*maximumDepth*maximumDepth),rows=checked((long)q*maximumDepth),requests=checked(6L*q*(maximumDepth+1));
  long eq=checked((long)q*(q+1)),eqRecords=checked(eq*131072);
  long copy=checked(eqRecords*40+parents*16+look*32+rows*288+requests*128+4096L*104+topology.SourceSelectorCopyBytes+
   (6L*q+12)*(240L*(q+1)+16L*(originalReferences+(long)q*n))+4096);
  var estimate=new Estimate(n,q,parents,look,rows,requests,eq,eqRecords,copy,maximumDepth,candidateStart,candidateEnd);
  if(SpanWork(estimate,topology.MaximumSourceRanges)>WorkLimit||copy>CopyLimit)throw new NotSupportedException("Individual batch span structural envelope exceeded.");
  return new(estimate);
 }
 internal static long SpanWork(Estimate b,int maximumSourceRanges=4096)=>checked(2L*1048576+2L*maximumSourceRanges*maximumSourceRanges+b.Parents+b.Lookahead+b.Rows+b.Requests+b.EqualityRecords+2L*(b.CandidateEnd-b.CandidateStart));
}
