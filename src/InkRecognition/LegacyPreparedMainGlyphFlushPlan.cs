using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalGlyphFlush.Probe")]
namespace InkRecognition;

// Numerical21FFD0 flush/packet planning shared by typed result owners.
internal static class LegacyPreparedMainGlyphFlushPlan
{
    internal sealed record Flush(int Current,int Filtered,bool Protocol,byte[] Rows,byte[] Template,int[] Origins,ulong[] Additions);
    internal sealed record Result(Flush[] Flushes,LegacyMainGlyphSourceMetadataEvent[] Events);
    internal static Result Run(LegacyOrdinaryRecognitionGraph graph,LegacyInkAlphabet alphabet,
        byte[] compact,byte[] allRows,int[] starts,int[] visited,int[] cumulative,int recordCount,
        int selectedRecordIndex,ushort selectedAlternative,LegacyPreparedMainTerminalWinner winner,
        Func<ulong,ushort,CancellationToken,ReadOnlyMemory<byte>> readTerminal,CancellationToken token)
    {
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        ushort marker=Label("\u0003"),joiner=Label("\u200d");
        byte[] ah=alphabet.CopySerializedHeader();ushort unit=U16(ah,6);
        var cursor=LegacyPreparedCompactWordPredecessor.Begin(compact,starts,graph.NodeCount-1,
            selectedRecordIndex-starts[^2],selectedAlternative,token);
        var output=new List<Flush>();var events=new List<LegacyMainGlyphSourceMetadataEvent>();
        var pending=new List<byte[]>();var additions=new List<ulong>();
        byte[] packet=new byte[64];int[] origins=Enumerable.Repeat(-1,64).ToArray();
        W16(packet,12,0xfffe);W16(packet,46,ushort.MaxValue);W32(packet,48,0);W32(packet,52,-1);
        W32(packet,56,-1);W32(packet,60,-1);
        Seed(14,0x32,2);Seed(20,0x3c,4);Seed(28,0x44,16);
        int rowOrdinal=0,visitOrdinal=0,baseNode=graph.NodeCount-1;
        int total=winner.TotalCost,field18=winner.RowCost14;
        while(!cursor.IsEnd)
        {
            token.ThrowIfCancellationRequested();
            if(visitOrdinal>=visited.Length||visited[visitOrdinal++]!=cursor.RecordIndex)
                throw new InvalidDataException("Owned append events and predecessor chronology differ.");
            var current=Row(cursor.RecordIndex);ushort raw=U16(current,28);int direct=I32(current,16);
            int directNode=direct==-1?-1:ReferenceNode(Row(direct));
            if(I32(current,36)!=-1)
            {
                if(current[4]!=1)throw new NotSupportedException("Implicit source0 metadata owner is separate.");
                ulong tag=BinaryPrimitives.ReadUInt64LittleEndian(current);
                byte[] metadata=readTerminal(tag,raw,token).ToArray();
                if(metadata.Length!=16)throw new InvalidDataException("Source70 must publish its complete16-byte token.");
                Write32(20,I32(current,36));Write(28,metadata);WriteByte(44,(byte)(current[32]>>7));
                events.Add(new(cursor.RecordIndex,tag,raw,metadata));
            }
            if(marker!=ushort.MaxValue&&raw==marker)baseNode=directNode;
            else if(raw!=joiner)
            {
                if(raw is 0xfffb or 0xfff9 or 0xfff5 or 0xfffa)Write32(52,-1);
                else if(I32(packet,52)==-1)Write32(52,Boundary(cursor.SourceNode,cursor.Arc,false));
                if(raw==unit)additions.Add(Pack(cursor.SourceNode,cursor.Arc));
                else if(raw is not (0xffff or 0xfffd))
                {
                    if(rowOrdinal>=allRows.Length/72||I32(allRows.AsSpan(rowOrdinal*72,72),8)!=cursor.RecordIndex)
                        throw new InvalidDataException("Actual numeric append event is missing at this predecessor.");
                    byte[] row=allRows.AsSpan(rowOrdinal++*72,72).ToArray();pending.Add(row);
                    var filtered=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,cursor,marker,token);
                    int guard=0;
                    while(!filtered.IsEnd)
                    {
                        token.ThrowIfCancellationRequested();
                        if(++guard>recordCount)throw new InvalidDataException("Filtered cursor is cyclic.");
                        var neighbor=Row(filtered.RecordIndex);
                        if((neighbor[32]&7)<3&&U16(neighbor,28)!=ushort.MaxValue&&U16(neighbor,28)!=unit)break;
                        if(I32(neighbor,36)!=-1)Write32(56,I32(neighbor,36));
                        filtered=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,filtered,marker,token);
                    }
                    if(filtered.RecordIndex!=I32(row,32)||filtered.SourceNode!=I32(row,24)||filtered.Arc!=U16(row,28))
                        throw new InvalidDataException("Filtered numeric event and actual cursor differ.");
                    if(U16(packet,12)==0xfffe&&(current[32]&7)!=2)Write16(12,current[4]);
                    ushort normalized=U16(row,58);
                    bool predecessorBoundary=direct!=-1&&U16(Row(direct),28) is 0xfffb or 0xfff9 or 0xfff5 or 0xfffa;
                    bool protocol=(unchecked((ushort)(normalized+7))&0xfffd)==0||normalized==0xfffa;
                    if(predecessorBoundary||protocol)
                    {
                        Write32(48,I32(packet,52)==-1?-1:Boundary(cursor.SourceNode,cursor.Arc,true));
                        if(protocol)
                        {
                            WriteFloat(0,1);WriteFloat(4,0);WriteFloat(8,0);
                            if(direct==-1||U16(Row(direct),28)==ushort.MaxValue)Write16(12,ushort.MaxValue);
                        }
                        else
                        {
                            int baseTotal=filtered.IsEnd?0:unchecked(cumulative[filtered.SourceNode]+I32(Row(filtered.RecordIndex),20));
                            int base18=filtered.IsEnd?0:I32(Row(filtered.RecordIndex),24);
                            var costs=CalculateCosts(total,baseTotal,field18,base18,baseNode,filtered.SourceNode);
                            Write32(0,unchecked((int)costs.LogMassBits));Write32(4,unchecked((int)costs.RelativeCostBits));
                            Write32(8,unchecked((int)costs.RawField18DeltaBits));
                            WriteByte(40,(byte)(!filtered.IsEnd&&U16(Row(filtered.RecordIndex),28)==joiner?1:0));
                            total=baseTotal;field18=base18;
                        }
                        byte[] combined=new byte[pending.Count*72];for(int i=0;i<pending.Count;i++)pending[i].CopyTo(combined,i*72);
                        output.Add(new(cursor.RecordIndex,filtered.RecordIndex,protocol,combined,
                            packet.ToArray(),origins.ToArray(),additions.ToArray()));
                        //Callback success resets the packet but preserves a dependency
                        //on its preceding raw24 field. No seed bytes are invented.
                        byte[] previous=packet.AsSpan(20,4).ToArray();int[] previousOrigins=origins.AsSpan(20,4).ToArray();
                        packet.AsSpan().Clear();Array.Fill(origins,-1);W16(packet,12,0xfffe);
                        W32(packet,20,-1);W16(packet,46,U16(pending[0],56));W32(packet,52,-1);W32(packet,56,-1);
                        previous.CopyTo(packet,60);previousOrigins.CopyTo(origins,60);
                        pending.Clear();additions.Clear();baseNode=directNode;
                    }
                    else if(direct==-1)
                        throw new NotSupportedException("A nonprotocol root requires the native terminal-tail branch.");
                }
            }
            cursor=LegacyPreparedCompactWordPredecessor.Advance(compact,starts,cursor,marker,token);
        }
        if(visitOrdinal!=visited.Length||rowOrdinal!=allRows.Length/72)
            throw new InvalidDataException("Owned numeric chronology was not fully consumed.");
        if(pending.Count!=0||additions.Count!=0)
            throw new NotSupportedException("Unflushed terminal rows or trailing residuals require the additional native end branch.");
        token.ThrowIfCancellationRequested();return new(output.ToArray(),events.ToArray());

        ReadOnlySpan<byte> Row(int index)
        {if((uint)index>=(uint)recordCount)throw new InvalidDataException("Predecessor escapes compact owner.");return compact.AsSpan(index*40,40);}
        int Boundary(int node,ushort arc,bool first)
        {if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)throw new NotSupportedException("External boundary source requires its real owner.");
            var source=graph.GetNode(node)[arc];return unchecked((int)(first?source.StartContextId:source.EndContextId));}
        void Seed(int destination,int source,int length){for(int i=0;i<length;i++)origins[destination+i]=source+i;}
        void Write(int at,ReadOnlySpan<byte> bytes){bytes.CopyTo(packet.AsSpan(at));origins.AsSpan(at,bytes.Length).Fill(-1);}
        void WriteByte(int at,byte value){packet[at]=value;origins[at]=-1;}
        void Write16(int at,ushort value){W16(packet,at,value);origins.AsSpan(at,2).Fill(-1);}
        void Write32(int at,int value){W32(packet,at,value);origins.AsSpan(at,4).Fill(-1);}
        void WriteFloat(int at,float value)=>Write32(at,BitConverter.SingleToInt32Bits(value));
    }
    //2210A6..221144: signed32 SUB/SHL wrap precedes CVTDQ2PS; DIVSS,
    //MULSS and their stores are separate. No exp/clamp/finite guard belongs
    //to this packet-writing stage, including zero denominators.
    internal static LegacyMainGlyphFlushCosts CalculateCosts(int total,int baseTotal,int field18,int base18,
        int baseNode,int filteredNode)
    {
        float relative=(float)unchecked(total-baseTotal)/(float)unchecked((baseNode-filteredNode)*8192);
        float delta=(float)unchecked(field18-base18)*(1f/8192);
        float mass=-5f*relative;
        return new(unchecked((uint)BitConverter.SingleToInt32Bits(mass)),
            unchecked((uint)BitConverter.SingleToInt32Bits(relative)),unchecked((uint)BitConverter.SingleToInt32Bits(delta)));
    }
    private static int ReferenceNode(ReadOnlySpan<byte> row)=>(unchecked((int)BinaryPrimitives.ReadUInt64LittleEndian(row[8..]))<<8)>>8;
    private static ulong Pack(int node,ushort arc)=>((uint)node&0xffffffUL)|((ulong)arc<<24);
    private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static void W16(Span<byte> b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b[at..],value);
    private static void W32(Span<byte> b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
}
