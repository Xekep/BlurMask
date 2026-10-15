using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual21FFD0 numeric append events from an owned compact winner.
/// Row72 padding remains explicit stack residue. Word-boundary callbacks,
/// residual217DB0 association and218560 chain publication are separate.</summary>
public sealed class LegacyOwnedMainGlyphBacktrackRows
{
    private readonly byte[] rows;
    private readonly int[] visited;
    private readonly ulong[] residuals;
    public LegacyOwnedMainCompactWordMaterialization Materialization {get;}
    public int Count=>rows.Length/72;
    public byte[] CopyRows()=>rows.ToArray();
    public int[] CopyVisitedCompactIndices()=>visited.ToArray();
    public ulong[] CopyResidualSourceReferences()=>residuals.ToArray();
    private LegacyOwnedMainGlyphBacktrackRows(LegacyOwnedMainCompactWordMaterialization materialization,
        byte[] rows,int[] visited,ulong[] residuals)
    {Materialization=materialization;this.rows=rows;this.visited=visited;this.residuals=residuals;}

    /// <summary>Walk the real numeric terminal and materialized2460/2478 owners.
    /// Supported ordinary graph subtypes0..3 retain native marker skipping,
    /// independent filtered lookahead, state carry and216610 key production.
    /// This returns append events, not flattened word segments or accepted text.</summary>
    public static LegacyOwnedMainGlyphBacktrackRows Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceSymbolCallbacks sourceCallbacks,
        ReadOnlySpan<byte> stackRowResidue72,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(sourceCallbacks);
        token.ThrowIfCancellationRequested();
        var terminal=materialization.TerminalResult;var policy=terminal.PassResult.ControllerOwner;
        var owner=policy.Owner;var confidence=materialization.ConfidenceOwner;
        if(stackRowResidue72.Length!=72||!ReferenceEquals(sourceCallbacks.Owner,confidence)||
            !ReferenceEquals(confidence.DirectDictionaryOwner,owner.DictionaryOwner)||
            !ReferenceEquals(confidence.Graph,owner.SourceGraph)||owner.IsCaseAware)
            throw new ArgumentException("Same owned DIRECT compact/source/confidence and complete72-byte stack residue required.");
        if(acousticResource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(acousticResource)),owner.SelectedSourceOwner.SourceFingerprint)||
            policy.Configuration.Trace2368!=0||policy.Configuration.Mode22f6!=0||
            policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Actual fresh unmodified Russian nontracing ordinary source required.");
        var graph=owner.SourceGraph;var alphabet=owner.CommonModel.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        ushort marker=Label("\u0003"),joiner=Label("\u200d"),softHyphen=Label("\u00ad"),apostrophe=Label("'");
        ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(acousticResource[105..]);
        //21FFD0 always calls frontend1A0 before its loop with capacity0.
        //The actual owned source callback supplies both zero result flags;
        //no boolean or manufactured delimiter result is accepted here.
        byte[] endpoint=new byte[64];W32(endpoint,0,-1);W16(endpoint,4,ushort.MaxValue);
        W32(endpoint,8,-1);W16(endpoint,12,ushort.MaxValue);W64(endpoint,16,ulong.MaxValue);
        W32(endpoint,24,graph.NodeCount);W64(endpoint,28,ulong.MaxValue);W16(endpoint,36,ushort.MaxValue);W64(endpoint,40,1);
        var initial=sourceCallbacks.ReadDelimiter(endpoint,0,token);
        if(initial.RemainingCursor!=0||initial.EmittedCount!=0||initial.NativeFlag0C!=0||initial.NativeFlag0D!=0)
            throw new NotSupportedException("Initial synthetic delimiter row requires its additional native path.");
        byte[] compact=materialization.CopyCompactStorage();int[] starts=materialization.CopyNodeStarts2478();
        if(materialization.RecordCount<=0||compact.Length<materialization.RecordCount*40||
            starts.Length!=graph.NodeCount+1||materialization.SelectedRecordIndex<starts[^2]||
            materialization.SelectedRecordIndex>=starts[^1])
            throw new InvalidDataException("Actual compact extent/selected final-node index required.");
        compact=compact.AsSpan(0,materialization.RecordCount*40).ToArray();
        var cursor=LegacyPreparedCompactWordPredecessor.Begin(compact,starts,graph.NodeCount-1,
            materialization.SelectedRecordIndex-starts[^2],materialization.SelectedAlternative,token);
        var result=new List<byte[]>();var visited=new List<int>();var residuals=new List<ulong>();
        var seen=new HashSet<LegacyCompactWordBacktrackCursor>();bool firstKey=true;
        int lastSeenNode=-1,earlierSeenNode=-1;ushort lastSeenArc=ushort.MaxValue,earlierSeenArc=ushort.MaxValue;
        ushort lastRegular=ushort.MaxValue;byte[] row=stackRowResidue72.ToArray();
        while(!cursor.IsEnd)
        {
            token.ThrowIfCancellationRequested();
            if(visited.Count>=materialization.RecordCount||!seen.Add(cursor))
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
                        if(!filteredSeen.Add(filtered)||filteredSeen.Count>materialization.RecordCount)
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
        token.ThrowIfCancellationRequested();return new(materialization,all,visited.ToArray(),residuals.ToArray());

        ReadOnlySpan<byte> Row(int index)
        {if((uint)index>=(uint)materialization.RecordCount)throw new InvalidDataException("Compact predecessor escapes its owner.");return compact.AsSpan(index*40,40);}
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
