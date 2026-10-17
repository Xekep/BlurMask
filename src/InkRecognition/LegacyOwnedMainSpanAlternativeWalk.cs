using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainSpanAlternativeWalk.Probe")]
[assembly:InternalsVisibleTo("LegacyMainConfiguredWordSources.Probe")]

namespace InkRecognition;

/// <summary>One218A30 direct-parent walk. Status and all output references
/// retain native numeric meanings; this is not a selected word or219580
/// augmentation. Rows are emitted only by the separate Emit operation.</summary>
public sealed class LegacyOwnedMainSpanAlternativeWalkResult
{
    public LegacyOwnedMainSpanAlternativeWalk Owner {get;}
    public int StartingRecordIndex {get;}
    public int Status {get;}
    public int UpdatedMinimumNode {get;}
    public int LastDirectParentIndex {get;}
    public ulong LastSourceKey {get;}
    public ushort WordFlags {get;}
    private readonly byte[] rows;
    private readonly int[] visited;
    private readonly ulong[] residuals;
    public int RowCount=>rows.Length/72;
    internal LegacyOwnedMainSpanAlternativeWalkResult(LegacyOwnedMainSpanAlternativeWalk owner,
        int index,int status,int minimum,int parent,ulong source,ushort flags,
        byte[] rows,int[] visited,ulong[] residuals)
    {Owner=owner;StartingRecordIndex=index;Status=status;UpdatedMinimumNode=minimum;LastDirectParentIndex=parent;
        LastSourceKey=source;WordFlags=flags;this.rows=rows;this.visited=visited;this.residuals=residuals;}
    public byte[] CopyRows72()=>rows.ToArray();
    public int[] CopyVisitedIndices()=>visited.ToArray();
    public ulong[] CopyResidualReferences()=>residuals.ToArray();
}

/// <summary>218A30 ordinary0..3 direct-parent source walk bound to a completed
/// fresh one-WORD source collection and its exact compact/graph/callback owner.
/// It does not use21FFD0's alternate predecessor cursor or create expected labels.</summary>
public sealed class LegacyOwnedMainSpanAlternativeWalk
{
    public LegacyOwnedMainWordSourceCollection Sources {get;}
    public LegacyOwnedRussianMainSourceSymbolCallbacks Callbacks {get;}
    private readonly LegacyMainSelectedCompactWordOwner compactOwner;
    private readonly byte[] compact,requestResidue;
    private readonly int[] starts;
    private readonly ushort marker,joiner,closed,unit,softHyphen;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private LegacyOwnedMainSpanAlternativeWalk(ReadOnlySpan<byte> resource,LegacyOwnedMainWordSourceCollection source,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,ReadOnlySpan<byte> requestResidue64)
    {
        Sources=source;Callbacks=callbacks;requestResidue=requestResidue64.ToArray();
        compactOwner=source.CandidateSource.Ownership.SelectedOwner;
        compact=compactOwner.CopyCompactStorage().AsSpan(0,checked(compactOwner.RecordCount*40)).ToArray();
        starts=compactOwner.CopyNodeStarts2478();graph=callbacks.Graph;
        var alphabet=callbacks.Owner.Model.AcousticAlphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort id)?id:ushort.MaxValue;
        marker=Label("\u0003");joiner=Label("\u200d");closed=Label("\U000f0001");softHyphen=Label("\u00ad");
        unit=U16(resource,105);
    }

    public static LegacyOwnedMainSpanAlternativeWalk Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainWordSourceCollection actualSources,LegacyOwnedRussianMainSourceSymbolCallbacks actualCallbacks,
        ReadOnlySpan<byte> delimiterCallerResidue64,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSources);ArgumentNullException.ThrowIfNull(actualCallbacks);token.ThrowIfCancellationRequested();
        var raw=actualSources.Candidate.RawResult;
        if(!raw.PublicationCompleted)throw new ArgumentException("The actual first-candidate publication must be complete.");
        return PrepareCore(resource,actualSources,actualCallbacks,delimiterCallerResidue64,token);
    }

    /// <summary>The same numeric218A30 algorithm over an actual configured
    /// selected2410 compact owner. No count1 candidate or source is fabricated.
    /// Unsupported compact subtypes remain explicit path-level refusals.</summary>
    public static LegacyOwnedMainSpanAlternativeWalk PrepareConfigured(ReadOnlySpan<byte> resource,
        LegacyOwnedMainWordSourceCollection actualSources,LegacyOwnedRussianMainSourceSymbolCallbacks actualCallbacks,
        ReadOnlySpan<byte> delimiterCallerResidue64,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSources);ArgumentNullException.ThrowIfNull(actualCallbacks);
        actualSources.CandidateSource.Validate(token);
        if(!actualSources.CandidateSource.IsConfigured)throw new ArgumentException("Actual configured source collection required.");
        return PrepareCore(resource,actualSources,actualCallbacks,delimiterCallerResidue64,token);
    }

    private static LegacyOwnedMainSpanAlternativeWalk PrepareCore(ReadOnlySpan<byte> resource,
        LegacyOwnedMainWordSourceCollection actualSources,LegacyOwnedRussianMainSourceSymbolCallbacks actualCallbacks,
        ReadOnlySpan<byte> delimiterCallerResidue64,CancellationToken token)
    {
        var source=actualSources.CandidateSource;source.Validate(token);
        var owner=source.Ownership.SelectedOwner;var policy=owner.Controller;
        if(delimiterCallerResidue64.Length!=64||!ReferenceEquals(actualCallbacks.Owner,owner.ConfidenceOwner)||
            !ReferenceEquals(actualCallbacks.Graph,source.Sampled.SourceRanges.Graph)||
            !source.Session.IsComplete||source.Session.IsStopped||!ReferenceEquals(owner.CaseSourcePolicy,source.SourcePolicy)||
            owner.CaseSourcePolicy is null||owner.CaseSourcePolicy.HasAssociatedSource)
            throw new ArgumentException("Completed identical fresh one-WORD CASE/compact/source callback owner and full delimiter scratch required.");
        if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(Convert.ToHexString(SHA256.HashData(resource)),
            policy.Owner.SelectedSourceOwner.SourceFingerprint)||policy.Configuration.Trace2368!=0||
            policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||policy.KnowledgeF5)
            throw new NotSupportedException("Pinned fresh ordinary no-property main scope required.");
        token.ThrowIfCancellationRequested();return new(resource,actualSources,actualCallbacks,delimiterCallerResidue64);
    }

    /// <summary>Native param8=true: no72 rows or residuals are appended.
    /// Every normal return writes LastSourceKey, includingFFFF when no eligible
    /// glyph was visited. No native allocator-failure success is synthesized.</summary>
    public LegacyOwnedMainSpanAlternativeWalkResult Inspect(int compactRecordIndex,int minimumSourceNode,
        CancellationToken token=default)=>Run(compactRecordIndex,minimumSourceNode,false,ushort.MaxValue,[],token);

    /// <summary>Native param8=false, including the independent direct-parent
    /// lookahead, nativeWORD30 byte transfer and explicit72-byte stack residue.
    /// This produces numeric append events, not a published WORD.</summary>
    public LegacyOwnedMainSpanAlternativeWalkResult Emit(int compactRecordIndex,int minimumSourceNode,
        ushort actualInitialWordFlags,ReadOnlySpan<byte> stackRowResidue72,CancellationToken token=default)
    {
        if(stackRowResidue72.Length!=72)throw new ArgumentException("Complete actual72-byte stack residue required.");
        return Run(compactRecordIndex,minimumSourceNode,true,actualInitialWordFlags,stackRowResidue72,token);
    }

    private LegacyOwnedMainSpanAlternativeWalkResult Run(int initialIndex,int minimum,bool emit,
        ushort wordFlags,ReadOnlySpan<byte> residue72,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();_ = Row(initialIndex);
        int index=initialIndex,previousNode=-1,lastNode=-1,lastParent=I32(Row(index),16),carry=0;
        ushort previousArc=ushort.MaxValue,lastRegular=ushort.MaxValue;bool first=true;
        ulong lastSource=ulong.MaxValue;byte initialFlags=Row(index)[32];
        var rows=new List<byte[]>();var residuals=new List<ulong>();var visited=new List<int>();var seen=new HashSet<int>();
        byte[] row=emit?residue72.ToArray():[];
        var initial=Reference(Row(index));
        //Fresh mode2601=0. The initial endpoint callback's capacity is0;
        //the same owned1AAE70 service must prove no synthetic type5 row.
        if(emit&&initial.Node==graph.NodeCount-1)
        {var endpoint=Endpoint(graph.NodeCount,ushort.MaxValue);RequireEmpty(Callbacks.ReadDelimiter(endpoint,0,token));}
        while(true)
        {
            token.ThrowIfCancellationRequested();var current=Row(index);var reference=Reference(current);
            int node=reference.Node;ushort arc=reference.Arc;
            if(node==-1||node<minimum)return Finish(node==-1?4:1);
            if(visited.Count>=compactOwner.RecordCount||!seen.Add(index))throw new InvalidDataException("218A30 direct-parent walk is cyclic.");
            visited.Add(index);lastNode=node;lastParent=I32(current,16);
            var parent=lastParent==-1?(-1,ushort.MaxValue):Reference(Row(lastParent));
            ushort symbol=U16(current,28);bool continueWalk=true,eligible=false;
            if(marker==ushort.MaxValue||symbol!=marker)
            {
                bool skipFinalFffa=symbol==0xfffa&&rows.Count==0&&node==graph.NodeCount-1;
                if(skipFinalFffa)RequireEmpty(Callbacks.ReadDelimiter(Endpoint(graph.NodeCount,ushort.MaxValue),0,token));
                else if(symbol is 0xfffb or 0xfff9 or 0xfff5||symbol==0xfffa)
                {
                    var delimiter=Callbacks.ReadDelimiter(Endpoint(node,arc),0,token);RequireEmpty(delimiter);
                    return Finish(2); //Actual capacity0 C=0; configured flag-C path remains excluded.
                }
                else if(symbol!=joiner&&symbol!=0xfffd)
                {
                    continueWalk=AdvanceClosedLabel(symbol,closed,initialFlags,ref carry);
                    if(symbol==ushort.MaxValue)continueWalk=false;
                    eligible=continueWalk;
                }
            }
            if(eligible)
            {
                if(symbol==unit)
                {
                    if(emit)residuals.Add(Pack(node,arc));
                }
                else if(!emit)lastSource=Pack(node,arc);
                else
                {
                    byte type=(byte)(current[32]&7);
                    if(type>3)throw new NotSupportedException("218A30 subtype4+ requires its actual separate key/row producer.");
                    if(type<3)lastRegular=symbol;
                    int look=lastParent;var lookRef=parent;var lookSeen=new HashSet<int>();
                    while(look>=0)
                    {
                        token.ThrowIfCancellationRequested();if(!lookSeen.Add(look))throw new InvalidDataException("218A30 lookahead is cyclic.");
                        var candidate=Row(look);
                        if((candidate[32]&7)<3&&U16(candidate,28)!=ushort.MaxValue&&U16(candidate,28)!=unit)break;
                        look=I32(candidate,16);lookRef=look==-1?(-1,ushort.MaxValue):Reference(Row(look));
                    }
                    ushort normalized=symbol switch{0xfff4=>softHyphen,0xfff5 or 0xfff7 or 0xfff8=>0xfff9,_=>symbol};
                    //Fresh Knowledge8 bit10 is set: no frontend240 FFF9 rewrite.
                    ulong key=Key(type,normalized,lookRef.Item1,node,previousNode,lookRef.Item2,arc,previousArc,ref first);
                    W32(row,0,node);W16(row,4,arc);W32(row,8,index);W32(row,12,parent.Item1);W16(row,16,parent.Item2);W32(row,20,lastParent);
                    W32(row,24,lookRef.Item1);W16(row,28,lookRef.Item2);W32(row,32,look);W32(row,36,previousNode);W16(row,40,previousArc);
                    W64(row,48,key);W16(row,56,lastRegular);W16(row,58,normalized);row[60]=type;row[61]=(byte)(current[32]>>6&1);row[62]=current[33];
                    W32(row,64,0);row[68]=0;row[69]=(byte)(current[7]>>1&1);rows.Add(row.ToArray());
                }
            }
            if(emit&&current[4]!=255)wordFlags=current[4];
            previousNode=node;previousArc=arc;index=lastParent;
            if(index==-1)return Finish(4);
        }

        LegacyOwnedMainSpanAlternativeWalkResult Finish(int status)
        {
            if(emit&&wordFlags==65534)wordFlags=ushort.MaxValue;
            byte[] all=new byte[rows.Count*72];for(int i=0;i<rows.Count;i++)rows[i].CopyTo(all,i*72);
            token.ThrowIfCancellationRequested();return new(this,initialIndex,status,lastNode,lastParent,lastSource,wordFlags,all,visited.ToArray(),residuals.ToArray());
        }
        void RequireEmpty(LegacyOwnedMainDelimiterResult result)
        {if(!ReferenceEquals(result.Owner,Callbacks)||result.RemainingCursor!=0||result.EmittedCount!=0||result.NativeFlag0C!=0||result.NativeFlag0D!=0)
            throw new NotSupportedException("Actual fresh capacity0 callback must not produce protocol output or flags.");}
        byte[] Endpoint(int node,ushort arc)
        {
            byte[] request=requestResidue.ToArray();W32(request,0,-1);W16(request,4,ushort.MaxValue);W32(request,8,-1);W16(request,12,ushort.MaxValue);W64(request,16,ulong.MaxValue);
            W32(request,24,node);W64(request,28,ulong.MaxValue);W16(request,28,arc);W16(request,36,ushort.MaxValue);W64(request,40,1);W64(request,48,0);W32(request,56,0);request[60]=0;return request;
        }
        ulong Key(byte type,ushort label,int earlier,int current,int later,ushort earlierArc,ushort currentArc,ushort laterArc,ref bool firstKey)
        {
            if(type<3){firstKey=false;return Pack(current,currentArc);}
            if(type==3&&(unchecked((ushort)(label+7))<4||label==softHyphen)&&earlier>=0&&later>=0)
            {
                int a=First(later,laterArc),b=Last(earlier,earlierArc);
                if(a==-1){a=b;b=First(earlier,earlierArc);}return SpanKey(a,b,false);
            }
            bool alternate=!firstKey||earlier==-1;int selected=alternate?current:earlier;ushort selectedArc=alternate?currentArc:earlierArc;
            return SpanKey(Last(selected,selectedArc),First(selected,selectedArc),alternate);
        }
        int First(int node,ushort arc){ValidateArc(node,arc);return unchecked((int)graph.GetNode(node)[arc].StartContextId);}
        int Last(int node,ushort arc){ValidateArc(node,arc);return unchecked((int)graph.GetNode(node)[arc].EndContextId);}
        void ValidateArc(int node,ushort arc){if((uint)node>=graph.NodeCount||arc>=graph.GetNode(node).Count)throw new NotSupportedException("Actual base ordinary graph arc required.");}
    }
    internal static bool AdvanceClosedLabel(ushort symbol,ushort closedLabel,byte initialFlags,ref int state)
    {
        if(closedLabel!=ushort.MaxValue){state&=255;if(symbol==closedLabel)state=1;}
        if(symbol!=0xfff6)return true;
        if((state&255)!=0){state=0xff00;return true;}
        return(initialFlags&0x10)==0;
    }
    private ReadOnlySpan<byte> Row(int index)
    {if((uint)index>=(uint)compactOwner.RecordCount)throw new ArgumentOutOfRangeException(nameof(index));return compact.AsSpan(index*40,40);}
    private static (int Node,ushort Arc) Reference(ReadOnlySpan<byte> row)
    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;return(node,(ushort)(key>>24));}
    private static ulong Pack(int node,ushort arc)=>(uint)node&0xffffffUL|((ulong)arc<<24);
    private static ulong SpanKey(int last,int first,bool alternate)=>(uint)last&0xffffffUL|((ulong)unchecked((ushort)(first-last))<<24)|(alternate?1UL<<56:0);
    private static ushort U16(ReadOnlySpan<byte> b,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(b[o..]);
    private static int I32(ReadOnlySpan<byte> b,int o)=>BinaryPrimitives.ReadInt32LittleEndian(b[o..]);
    private static void W16(byte[] b,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(o),v);
    private static void W32(byte[] b,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(o),v);
    private static void W64(byte[] b,int o,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(o),v);
}
