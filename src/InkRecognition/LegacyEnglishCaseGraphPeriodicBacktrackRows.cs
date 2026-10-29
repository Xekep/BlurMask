using System.Buffers.Binary;
namespace InkRecognition;

// Same numeric21FFD0 predecessor/append body, privately bound to the live periodic publication.
// Only input ownership changes: compact/starts are borrowed synchronously; result DATA remains owned.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyPreparedMainGlyphBacktrackRows.Result RunGraphPeriodicBacktrackRows(GraphLanguageScope stage,LegacyPreparedMainCompactPublication.Result published,
        LegacyOrdinaryRecognitionGraph graph,LegacyInkAlphabet alphabet,ushort unit,int recordCount,int selectedRecordIndex,
        ushort selectedAlternative,ReadOnlySpan<byte> stackRowResidue72,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(!graphPeriodicActive||!stage.PeriodicEnabled||recordCount!=published.Count)
            throw new InvalidOperationException("Actual same-owner periodic compact publication required.");
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,16384); // Header/label/cursor fixed source work, separate from memory.
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        ushort marker=Label("\u0003"),joiner=Label("\u200d"),softHyphen=Label("\u00ad"),apostrophe=Label("'");
        // Private synchronous read: no capacity-wide compact clone or detached alias escapes this call.
        byte[] compact=published.Storage;int[] starts=published.Starts;
        if(recordCount<=0||compact.Length<recordCount*40||
            starts.Length!=graph.NodeCount+1||selectedRecordIndex<starts[^2]||
            selectedRecordIndex>=starts[^1])
            throw new InvalidDataException("Actual compact extent/selected final-node index required.");
        var cursor=LegacyPreparedCompactWordPredecessor.Begin(compact.AsSpan(0,checked(recordCount*40)),starts,graph.NodeCount-1,
            selectedRecordIndex-starts[^2],selectedAlternative,token);
        bool preciseMemory=graphOriginalSelectedRequested&&graphTextPublicationRequested;
        var result=new List<byte[]>();var visited=new List<int>();var residuals=new List<ulong>();
        var seen=LegacyEnglishPeriodicRawWork.CreateSeen(recordCount);
        HashSet<LegacyCompactWordBacktrackCursor>? filteredSeen=null;bool firstKey=true;
        int lastSeenNode=-1,earlierSeenNode=-1;ushort lastSeenArc=ushort.MaxValue,earlierSeenArc=ushort.MaxValue;
        ushort lastRegular=ushort.MaxValue;byte[] row=stackRowResidue72.ToArray();
        while(!cursor.IsEnd)
        {
            token.ThrowIfCancellationRequested();
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8); // BEFORE seen count metadata.
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicRawWork.OuterStepVisits(seen.Count));
            if(preciseMemory)
            {
                stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.VisitedPayload);
                stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.VisitedCopies);
            }
            if(visited.Count>=recordCount||!seen.Add(cursor))
                throw new InvalidDataException("Native predecessor walk is cyclic or exceeds the owned compact extent.");
            visited.Add(cursor.RecordIndex);var current=compact.AsSpan(cursor.RecordIndex*40,40);
            ValidateArc(cursor.SourceNode,cursor.Arc);
            ushort symbol=PeriodicBacktrackU16(current,28);byte subtype=(byte)(current[32]&7);
            int direct=PeriodicBacktrackI32(current,16);var directReference=direct==-1?(-1,ushort.MaxValue):PeriodicBacktrackReference(Row(direct));
            //Fresh9A=0 suppresses ZWJ;FFFF/FFFD have explicit native no-row
            //paths. Missing markerFFFF does not suppress arbitrary labels.
            if((marker==ushort.MaxValue||symbol!=marker)&&symbol!=joiner&&symbol!=ushort.MaxValue&&symbol!=0xfffd)
            {
                if(symbol==unit)
                {
                    if(preciseMemory)
                    {stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.ResidualPayload);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.ResidualCopies);}
                    residuals.Add(PeriodicBacktrackPack(cursor.SourceNode,cursor.Arc));
                }
                else
                {
                    if(subtype>3)throw new NotSupportedException("Subtype4+ row/key and synthetic protocol writers are separate.");
                    if(subtype<3)lastRegular=symbol;
                    //Lookahead uses its own copy of the alternative cursor;
                    //it must not advance the outer walk's selected alternative.
                    stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,256);
                    var filtered=LegacyPreparedCompactWordPredecessor.Advance(compact.AsSpan(0,checked(recordCount*40)),starts,cursor,marker,token);
                    // Lazy first allocation stays AFTER the original initial Advance; no-emission path allocates none.
                    if(filteredSeen is null)filteredSeen=LegacyEnglishPeriodicRawWork.CreateSeen(recordCount);
                    else
                    {
                        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,128); // BEFORE capacity metadata access.
                        int capacity=filteredSeen.EnsureCapacity(0);
                        LegacyEnglishPeriodicRawWork.RequireSeenCapacity(recordCount,capacity);
                        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicRawWork.SeenClearCopies(capacity));
                        filteredSeen.Clear();
                    }
                    while(!filtered.IsEnd)
                    {
                        token.ThrowIfCancellationRequested();
                        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8); // BEFORE seen count metadata.
                        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicRawWork.LookaheadStepVisits(filteredSeen.Count));
                        if(!filteredSeen.Add(filtered)||filteredSeen.Count>recordCount)
                            throw new InvalidDataException("Filtered predecessor lookahead is cyclic.");
                        var candidate=Row(filtered.RecordIndex);
                        if((candidate[32]&7)<3&&PeriodicBacktrackU16(candidate,28)!=ushort.MaxValue&&PeriodicBacktrackU16(candidate,28)!=unit)break;
                        filtered=LegacyPreparedCompactWordPredecessor.Advance(compact.AsSpan(0,checked(recordCount*40)),starts,filtered,marker,token);
                    }
                    ushort normalized=symbol switch {0xfff4=>softHyphen,0xfff5 or 0xfff7 or 0xfff8=>0xfff9,_=>symbol};
                    //Fresh Knowledge8 has bit10 set: no frontend240 fallback
                    //can rewrite FFF9 to FFFB. Synthetic apostrophe alone is
                    //rewritten here;216BC0 separately remaps both score labels.
                    if(normalized==alphabet.LabelCount+1)normalized=apostrophe;
                    ulong key=Key(subtype,normalized,filtered.SourceNode,cursor.SourceNode,
                        earlierSeenNode,filtered.Arc,cursor.Arc,earlierSeenArc,ref firstKey);
                    PeriodicBacktrackW32(row,0,cursor.SourceNode);PeriodicBacktrackW16(row,4,cursor.Arc);PeriodicBacktrackW32(row,8,cursor.RecordIndex);
                    PeriodicBacktrackW32(row,12,directReference.Item1);PeriodicBacktrackW16(row,16,directReference.Item2);PeriodicBacktrackW32(row,20,direct);
                    PeriodicBacktrackW32(row,24,filtered.SourceNode);PeriodicBacktrackW16(row,28,filtered.Arc);PeriodicBacktrackW32(row,32,filtered.RecordIndex);
                    PeriodicBacktrackW32(row,36,earlierSeenNode);PeriodicBacktrackW16(row,40,earlierSeenArc);PeriodicBacktrackW64(row,48,key);
                    PeriodicBacktrackW16(row,56,lastRegular);PeriodicBacktrackW16(row,58,normalized);row[60]=subtype;
                    row[61]=(byte)(current[32]>>6&1);row[62]=current[33];PeriodicBacktrackW32(row,64,0);row[68]=0;row[69]=(byte)(current[7]>>1&1);
                    if(preciseMemory)
                    {stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.EmittedPayload);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,LegacyEnglishPeriodicPublicationBudget.EmittedCopies);}
                    result.Add(row.ToArray());
                }
            }
            if(lastSeenNode!=cursor.SourceNode){earlierSeenNode=lastSeenNode;earlierSeenArc=lastSeenArc;}
            lastSeenNode=cursor.SourceNode;lastSeenArc=cursor.Arc;
            stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,256);
            cursor=LegacyPreparedCompactWordPredecessor.Advance(compact.AsSpan(0,checked(recordCount*40)),starts,cursor,marker,token);
        }
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,24); // BEFORE list count metadata.
        stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(1024L+result.Count*144L+visited.Count*8L+residuals.Count*16L));
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
            if(type<3){first=false;return PeriodicBacktrackPack(current,currentArc);}
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
    private static (int,ushort) PeriodicBacktrackReference(ReadOnlySpan<byte> row)
    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);return ((unchecked((int)key)<<8)>>8,(ushort)(key>>24));}
    private static ulong PeriodicBacktrackPack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
    private static ushort PeriodicBacktrackU16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static int PeriodicBacktrackI32(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static void PeriodicBacktrackW16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
    private static void PeriodicBacktrackW32(Span<byte> bytes,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(bytes[at..],value);
    private static void PeriodicBacktrackW64(Span<byte> bytes,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(bytes[at..],value);
}

// Pure numeric work/count functions. They issue no graph, source, callback or lease capability.
internal static class LegacyEnglishPeriodicRawWork
{
    private static void Count(int value,int maximum=4096){if((uint)value>(uint)maximum)throw new ArgumentOutOfRangeException(nameof(value));}
    internal static int MaximumSeenCapacity(int records){if(records is<1 or>4096)throw new ArgumentOutOfRangeException(nameof(records));return checked(4*(records+1));}
    internal static void RequireSeenCapacity(int records,int capacity){if(capacity<records+1||capacity>MaximumSeenCapacity(records))throw new NotSupportedException("Reviewed bounded HashSet capacity premise changed.");}
    internal static HashSet<LegacyCompactWordBacktrackCursor> CreateSeen(int records)
    {
        if(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyCompactWordBacktrackCursor>()||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyCompactWordBacktrackCursor>()>16)
            throw new NotSupportedException("Reference-free bounded cursor key required.");
        _=MaximumSeenCapacity(records);var value=new HashSet<LegacyCompactWordBacktrackCursor>(records+1);RequireSeenCapacity(records,value.EnsureCapacity(0));return value;
    }
    internal static long SeenClearCopies(int capacity){if(capacity is<1 or>16388)throw new ArgumentOutOfRangeException(nameof(capacity));return checked(16L+28L*capacity);}
    internal static long OuterStepVisits(int seen){Count(seen);return checked(1024L+32L*(seen+1));}
    internal static long LookaheadStepVisits(int seen){Count(seen);return checked(512L+32L*(seen+1));}
    internal static long FlushVisits(int visited,int rows){Count(visited);Count(rows);if(rows>visited)throw new ArgumentOutOfRangeException(nameof(rows));return checked(16384L+visited*1024L+rows*(long)visited*256L+rows*2048L);}
    internal static (long Payload,long Copy,long Visit) Chain(int ordinary,int protocol){Count(ordinary);Count(protocol);int rows=checked(ordinary+protocol);if(rows>4096)throw new ArgumentOutOfRangeException(nameof(protocol));return(checked(32768L+rows*4096L),checked(65536L+rows*16384L),checked(16384L+ordinary*24576L+protocol*4096L+1024L*rows*rows));}
    internal static (long Payload,long Copy,long Visit) PoolExport(int owners,int records)
    {Count(owners);Count(records,65536);if(System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyNativeGlyphRecordNode>()>32)throw new NotSupportedException("Reviewed bounded owner snapshot slot required.");return(checked(1024L+owners*256L+records*40L),checked(1024L+owners*512L+records*80L),checked(1024L+owners*512L+records*80L));}
}

// Numeric semantic envelopes only; no source, snapshot, model or request authority.
internal static class LegacyEnglishPeriodicPublicationBudget
{
    internal const long PreNeuralAuthenticationPayload=1024,PreNeuralAuthenticationCopies=2048,PreNeuralAuthenticationVisits=4096;
    internal const long VisitedPayload=256,VisitedCopies=512,EmittedPayload=2048,EmittedCopies=4096,ResidualPayload=64,ResidualCopies=128;
    internal static long RawInitialPayload(int records)
    {if(records is<1 or>4096)throw new ArgumentOutOfRangeException(nameof(records));return checked(65536L+224L*records);}
    internal static (long Payload,long Copies,long Visits) SpanCapture(int words)
    {if(words is<1 or>4095)throw new ArgumentOutOfRangeException(nameof(words));return(checked(2048L+16L*words),checked(4096L+32L*words),checked(4096L+128L*words));}
    internal static (long Payload,long Copies,long Visits) PreNeuralReuse(int objects,int points)
    {if(objects is<1 or>256||points<objects||points>4096)throw new ArgumentOutOfRangeException();return(4096L,8192L,4096L);}
}
