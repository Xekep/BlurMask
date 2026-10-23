namespace InkRecognition;

public sealed partial class LegacyPackedLexicon
{
    // Source-local scalar reader, never an owner/lease or transferable data view.
    // Only Index.Prepare creates and consumes it; no method accepts a supplied reader.
    internal readonly ref struct Node0IndexReader
    {
        private readonly LegacyPackedLexicon owner;
        private readonly byte[] labels,offsets;
        private readonly int count;
        private Node0IndexReader(LegacyPackedLexicon owner,byte[] labels,byte[] offsets,int count)
        {this.owner=owner;this.labels=labels;this.offsets=offsets;this.count=count;}
        internal bool IsFor(LegacyPackedLexicon expected)=>ReferenceEquals(owner,expected);
        internal void Read(int node,out uint packed,out int start,out int end)
        {
            if((uint)node>=(uint)count)throw new InvalidDataException("Bound node required for private packed reader.");
            // Cache exact private array references once per fused read. Widths8/17
            // are factory premises. Offset node+1 includes the genuine N sentinel.
            var l=labels;var o=offsets;
            packed=l[node];start=checked(node+(int)Read17(o,node));
            end=checked(node+1+(int)Read17(o,node+1));
        }
        private static uint Read17(byte[] bytes,int index)
        {
            long bit=(long)index*17;int offset=(int)(bit>>3),shift=(int)(bit&7);
            // 17+shift is17..24: exactly THREE source bytes, including shift0.
            uint word=(uint)(bytes[offset]|bytes[offset+1]<<8|bytes[offset+2]<<16);
            return(word>>shift)&0x1FFFFU;
        }
        internal static Node0IndexReader Open(LegacyPackedLexicon owner,LegacyEnglishLkFreshNumericBinding numeric)
        {
            if(!ReferenceEquals(numeric.DataOwner.Dictionary,owner)||owner.NodeCount!=666547||
                owner._labels.Count!=666547||owner._labels.Width!=8||
                owner._offsets.Count!=666548||owner._offsets.Width!=17||
                owner._labels.Data.Length!=666548||owner._offsets.Data.Length!=1416416||
                Read17(owner._offsets.Data,666547)!=0)
                throw new NotSupportedException("Exact owned8/17 packed index source and offsetN sentinel required.");
            return new(owner,owner._labels.Data,owner._offsets.Data,666547);
        }
    }
}
