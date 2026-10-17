using System.Buffers.Binary;
namespace InkRecognition;

// Shared numeric21FFD0 predecessor/append algorithm. Typed callers retain
// source, terminal and frontend1A0 validation; no source identity is coerced.
internal static class LegacyPreparedMainGlyphBacktrackRows
{
    internal sealed record Result(byte[] Rows,int[] Visited,ulong[] Residuals);
    internal static Result Run(LegacyOrdinaryRecognitionGraph graph,LegacyInkAlphabet alphabet,ushort unit,
        ReadOnlySpan<byte> compactStorage,ReadOnlySpan<int> nodeStarts,int recordCount,int selectedRecordIndex,
        ushort selectedAlternative,ReadOnlySpan<byte> stackRowResidue72,CancellationToken token)
    {
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        ushort marker=Label("\u0003"),joiner=Label("\u200d"),softHyphen=Label("\u00ad"),apostrophe=Label("'");
        byte[] compact=compactStorage.ToArray();int[] starts=nodeStarts.ToArray();
        if(recordCount<=0||compact.Length<recordCount*40||
            starts.Length!=graph.NodeCount+1||selectedRecordIndex<starts[^2]||
            selectedRecordIndex>=starts[^1])
            throw new InvalidDataException("Actual compact extent/selected final-node index required.");
        compact=compact.AsSpan(0,recordCount*40).ToArray();
        var cursor=LegacyPreparedCompactWordPredecessor.Begin(compact,starts,graph.NodeCount-1,
            selectedRecordIndex-starts[^2],selectedAlternative,token);
        var result=new List<byte[]>();var visited=new List<int>();var residuals=new List<ulong>();
        var seen=new HashSet<LegacyCompactWordBacktrackCursor>();bool firstKey=true;
        int lastSeenNode=-1,earlierSeenNode=-1;ushort lastSeenArc=ushort.MaxValue,earlierSeenArc=ushort.MaxValue;
        ushort lastRegular=ushort.MaxValue;byte[] row=stackRowResidue72.ToArray();
        while(!cursor.IsEnd)
        {
            token.ThrowIfCancellationRequested();
            if(visited.Count>=recordCount||!seen.Add(cursor))
                throw new InvalidDataException("Native predecessor walk is cyclic or exceeds the owned compact extent.");
            visited.Add(cursor.RecordIndex);var current=compact.AsSpan(cursor.RecordIndex*40,40);
            ValidateArc(cursor.SourceNode,cursor.Arc);
            ushort symbol=U16(current,28);byte subtype=(byte)(current[32]&7);
            int direct=I32(current,16);var directReference=direct==-1?(-1,ushort.MaxValue):Reference(Row(direct));
            //Fresh9A=0 suppresses ZWJ;FFFF/FFFD have explicit native no-row
            //paths. Missing markerFFFF does not suppress arbitrary labels.
            if((marker==ushort.MaxValue||symbol!=marker)&&symbol!=joiner&&symbol!=ushort.MaxValue&&symbol!=0xfffd)
            {
                if(symbol==unit)residuals.Add(Pack(cursor.SourceNode,cursor.Arc));
                else
                {
                    if(subtype>3)throw new NotSupportedException("Subtype4+ row/key and synthetic protocol writers are separate.");
                    if(subtype<3)lastRegular=symbol;
                    //Lookahead uses its own copy of the alternative cursor;
                    //it must not advance the outer walk's selected alternative.
                    var filtered=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,cursor,marker,token);
                    var filteredSeen=new HashSet<LegacyCompactWordBacktrackCursor>();
                    while(!filtered.IsEnd)
                    {
                        token.ThrowIfCancellationRequested();
                        if(!filteredSeen.Add(filtered)||filteredSeen.Count>recordCount)
                            throw new InvalidDataException("Filtered predecessor lookahead is cyclic.");
                        var candidate=Row(filtered.RecordIndex);
                        if((candidate[32]&7)<3&&U16(candidate,28)!=ushort.MaxValue&&U16(candidate,28)!=unit)break;
                        filtered=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,filtered,marker,token);
                    }
                    ushort normalized=symbol switch {0xfff4=>softHyphen,0xfff5 or 0xfff7 or 0xfff8=>0xfff9,_=>symbol};
                    //Fresh Knowledge8 has bit10 set: no frontend240 fallback
                    //can rewrite FFF9 to FFFB. Synthetic apostrophe alone is
                    //rewritten here;216BC0 separately remaps both score labels.
                    if(normalized==alphabet.LabelCount+1)normalized=apostrophe;
                    ulong key=Key(subtype,normalized,filtered.SourceNode,cursor.SourceNode,
                        earlierSeenNode,filtered.Arc,cursor.Arc,earlierSeenArc,ref firstKey);
                    W32(row,0,cursor.SourceNode);W16(row,4,cursor.Arc);W32(row,8,cursor.RecordIndex);
                    W32(row,12,directReference.Item1);W16(row,16,directReference.Item2);W32(row,20,direct);
                    W32(row,24,filtered.SourceNode);W16(row,28,filtered.Arc);W32(row,32,filtered.RecordIndex);
                    W32(row,36,earlierSeenNode);W16(row,40,earlierSeenArc);W64(row,48,key);
                    W16(row,56,lastRegular);W16(row,58,normalized);row[60]=subtype;
                    row[61]=(byte)(current[32]>>6&1);row[62]=current[33];W32(row,64,0);row[68]=0;row[69]=(byte)(current[7]>>1&1);
                    result.Add(row.ToArray());
                }
            }
            if(lastSeenNode!=cursor.SourceNode){earlierSeenNode=lastSeenNode;earlierSeenArc=lastSeenArc;}
            lastSeenNode=cursor.SourceNode;lastSeenArc=cursor.Arc;
            cursor=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,cursor,marker,token);
        }
        byte[] all=new byte[result.Count*72];for(int i=0;i<result.Count;i++)result[i].CopyTo(all,i*72);
        token.ThrowIfCancellationRequested();return new(all,visited.ToArray(),residuals.ToArray());

        ReadOnlySpan<byte> Row(int index)
        {if((uint)index>=(uint)recordCount)throw new InvalidDataException("Compact predecessor escapes its owner.");return compact.AsSpan(index*40,40);}
        void ValidateArc(int node,ushort arc)
        {if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)throw new NotSupportedException("Higher/external source arc requires its actual boundary dispatcher.");}
        int First(int node,ushort arc){ValidateArc(node,arc);return unchecked((int)graph.GetNode(node)[arc].StartContextId);}
        int Last(int node,ushort arc){ValidateArc(node,arc);return unchecked((int)graph.GetNode(node)[arc].EndContextId);}
        ulong SpanKey(int last,int first,bool alternate)=>
            ((uint)last&0xffffffUL)|((ulong)unchecked((ushort)(first-last))<<24)|(alternate?1UL<<56:0);
        ulong Key(byte type,ushort label,int previous,int current,int earlier,ushort previousArc,ushort currentArc,ushort earlierArc,ref bool first)
        {
            if(type<3){first=false;return Pack(current,currentArc);}
            if(type==3&&(unchecked((ushort)(label+7))<4||label==softHyphen)&&previous>=0&&earlier>=0)
            {
                int a=First(earlier,earlierArc),b=Last(previous,previousArc);
                if(a==-1){a=b;b=First(previous,previousArc);}return SpanKey(a,b,false);
            }
            //216610 subtype3/4 general branch. Its low24 is LAST; signed
            //arc is FIRST-LAST, never a positive bbox width or screen point.
            bool alternate=!first||previous==-1;int selected=alternate?current:previous;
            ushort selectedArc=alternate?currentArc:previousArc;
            return SpanKey(Last(selected,selectedArc),First(selected,selectedArc),alternate);
        }
    }
    private static (int,ushort) Reference(ReadOnlySpan<byte> row)
    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);return ((unchecked((int)key)<<8)>>8,(ushort)(key>>24));}
    private static ulong Pack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static int I32(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
    private static void W32(Span<byte> bytes,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(bytes[at..],value);
    private static void W64(Span<byte> bytes,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(bytes[at..],value);
}
