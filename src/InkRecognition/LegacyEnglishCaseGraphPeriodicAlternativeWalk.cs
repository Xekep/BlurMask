using System.Buffers.Binary;

namespace InkRecognition;

// PURE218A30 numeric core. It issues no query/lease/actual-global owner.
// The live owner privately binds exact graph/delimiter callbacks; prepared tests
// may inspect numeric outputs without acquiring production authority.
internal sealed class LegacyEnglishPeriodicSpanAlternativeWalkKernel
{
    private readonly byte[] compact,requestResidue;
    private readonly LegacyEnglishCaseNode0Completion.GraphLanguageScope stage;
    private readonly LegacyGlobalWordAugmentationEnvelope? envelope;
    private readonly int count,nodeCount;
    private readonly ushort marker,joiner,closed,unit,softHyphen;
    private readonly Func<int,ushort,int> firstBoundary,lastBoundary;
    private readonly Func<byte[],int,CancellationToken,LegacyGlobalSpanAlternativeWalkKernel.Delimiter> delimiter;
    internal LegacyEnglishPeriodicSpanAlternativeWalkKernel(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,byte[] compact,int count,int nodeCount,ushort marker,ushort joiner,
        ushort closed,ushort unit,ushort softHyphen,byte[] requestResidue,
        Func<int,ushort,int> firstBoundary,Func<int,ushort,int> lastBoundary,
        Func<byte[],int,CancellationToken,LegacyGlobalSpanAlternativeWalkKernel.Delimiter> delimiter, LegacyGlobalWordAugmentationEnvelope? envelope=null)
    {
        stage.Owner.RequireGraphPublication(stage,default);if(!stage.PeriodicEnabled)throw new InvalidOperationException("Actual periodic source session required.");this.stage=stage;
        if(count<1||count>131072||compact.Length%40!=0||compact.Length/40<count||nodeCount is<1 or>4096||requestResidue.Length!=64)
            throw new ArgumentException("Complete bounded numeric compact rows/node count/request64 required.");
        this.envelope=envelope;this.compact=compact;this.count=count;this.nodeCount=nodeCount;this.marker=marker;this.joiner=joiner;
        this.closed=closed;this.unit=unit;this.softHyphen=softHyphen;this.requestResidue=requestResidue;
        this.firstBoundary=firstBoundary;this.lastBoundary=lastBoundary;this.delimiter=delimiter;
    }
    internal LegacyGlobalSpanAlternativeWalkKernel.Result Run(int initialIndex,int minimum,bool emit,
        ushort wordFlags,ReadOnlySpan<byte> residue72,CancellationToken token)
    {
        stage.Owner.RequireGraphPublication(stage,token);Allocate(512);Copy(512);token.ThrowIfCancellationRequested();_ = Row(initialIndex);
        int index=initialIndex,previousNode=-1,lastNode=-1,lastParent=I32(Row(index),16),carry=0;
        ushort previousArc=ushort.MaxValue,lastRegular=ushort.MaxValue;bool first=true;
        ulong lastSource=ulong.MaxValue;byte initialFlags=Row(index)[32];
        var rows=new List<byte[]>();var residuals=new List<ulong>();var visited=new List<int>();var seen=new HashSet<int>();
        byte[] row=emit?residue72.ToArray():[];
        var initial=Reference(Row(index));
        //Fresh mode2601=0. The initial endpoint callback's capacity is0;
        //the same owned1AAE70 service must prove no synthetic type5 row.
        if(emit&&initial.Node==nodeCount-1)
        {var endpoint=Endpoint(nodeCount,ushort.MaxValue);RequireEmpty(delimiter(endpoint,0,token));}
        while(true)
        {
            token.ThrowIfCancellationRequested();var current=Row(index);var reference=Reference(current);
            int node=reference.Node;ushort arc=reference.Arc;
            if(node==-1||node<minimum)return Finish(node==-1?4:1);
            Allocate(192);Copy(192);Visit(128);envelope?.Parent();if(visited.Count>=count||!seen.Add(index))throw new InvalidDataException("218A30 direct-parent walk is cyclic.");
            visited.Add(index);lastNode=node;lastParent=I32(current,16);
            var parent=lastParent==-1?(-1,ushort.MaxValue):Reference(Row(lastParent));
            ushort symbol=U16(current,28);bool continueWalk=true,eligible=false;
            if(marker==ushort.MaxValue||symbol!=marker)
            {
                bool skipFinalFffa=symbol==0xfffa&&rows.Count==0&&node==nodeCount-1;
                if(skipFinalFffa)RequireEmpty(delimiter(Endpoint(nodeCount,ushort.MaxValue),0,token));
                else if(symbol is 0xfffb or 0xfff9 or 0xfff5||symbol==0xfffa)
                {
                    var reply=delimiter(Endpoint(node,arc),0,token);RequireEmpty(reply);
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
                    int look=lastParent;var lookRef=parent;Allocate(64);var lookSeen=new HashSet<int>();
                    while(look>=0)
                    {
                        token.ThrowIfCancellationRequested();Allocate(128);Copy(128);Visit(64);envelope?.Lookahead();if(!lookSeen.Add(look))throw new InvalidDataException("218A30 lookahead is cyclic.");
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
                    W32(row,64,0);row[68]=0;row[69]=(byte)(current[7]>>1&1);Allocate(128);Copy(144);envelope?.Row();envelope?.Copy(144);rows.Add(row.ToArray());
                }
            }
            if(emit&&current[4]!=255)wordFlags=current[4];
            previousNode=node;previousArc=arc;index=lastParent;
            if(index==-1)return Finish(4);
        }

        LegacyGlobalSpanAlternativeWalkKernel.Result Finish(int status)
        {
            if(emit&&wordFlags==65534)wordFlags=ushort.MaxValue;
            Allocate(checked(96L+rows.Count*72L+visited.Count*4L+residuals.Count*8L));Copy(checked(rows.Count*72L+visited.Count*4L+residuals.Count*8L));
            byte[] all=new byte[rows.Count*72];for(int i=0;i<rows.Count;i++)rows[i].CopyTo(all,i*72);
            token.ThrowIfCancellationRequested();return new(initialIndex,status,lastNode,lastParent,lastSource,wordFlags,all,visited.ToArray(),residuals.ToArray());
        }
        void RequireEmpty(LegacyGlobalSpanAlternativeWalkKernel.Delimiter result)
        {if(result.RemainingCursor!=0||result.EmittedCount!=0||result.NativeFlag0C!=0||result.NativeFlag0D!=0)
            throw new NotSupportedException("Actual fresh capacity0 callback must not produce protocol output or flags.");}
        byte[] Endpoint(int node,ushort arc)
        {
            Allocate(96);Copy(128);Visit(64);envelope?.Request();envelope?.Copy(64);byte[] request=requestResidue.ToArray();W32(request,0,-1);W16(request,4,ushort.MaxValue);W32(request,8,-1);W16(request,12,ushort.MaxValue);W64(request,16,ulong.MaxValue);
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
        int First(int node,ushort arc)=>firstBoundary(node,arc);
        int Last(int node,ushort arc)=>lastBoundary(node,arc);

    }
    internal static bool AdvanceClosedLabel(ushort symbol,ushort closedLabel,byte initialFlags,ref int state)
    {
        if(closedLabel!=ushort.MaxValue){state&=255;if(symbol==closedLabel)state=1;}
        if(symbol!=0xfff6)return true;
        if((state&255)!=0){state=0xff00;return true;}
        return(initialFlags&0x10)==0;
    }
    private ReadOnlySpan<byte> Row(int index)
    {Visit(40);if((uint)index>=(uint)count)throw new ArgumentOutOfRangeException(nameof(index));return compact.AsSpan(index*40,40);}
    private static (int Node,ushort Arc) Reference(ReadOnlySpan<byte> row)
    {ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;return(node,(ushort)(key>>24));}
    private static ulong Pack(int node,ushort arc)=>(uint)node&0xffffffUL|((ulong)arc<<24);
    private static ulong SpanKey(int last,int first,bool alternate)=>(uint)last&0xffffffUL|((ulong)unchecked((ushort)(first-last))<<24)|(alternate?1UL<<56:0);
    private static ushort U16(ReadOnlySpan<byte> b,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(b[o..]);
    private static int I32(ReadOnlySpan<byte> b,int o)=>BinaryPrimitives.ReadInt32LittleEndian(b[o..]);
    private static void W16(byte[] b,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(o),v);
    private static void W32(byte[] b,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(o),v);
    private static void W64(byte[] b,int o,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(o),v);    private void Allocate(long bytes){stage.Owner.RequireGraphPublication(stage,default);stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes);}
    private void Copy(long bytes)=>stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes);
    private void Visit(long units)=>stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,units);

}
