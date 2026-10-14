using System.Buffers.Binary;

namespace InkRecognition;

public delegate int LegacyFirstOrdinaryGlyphCost(int node,ushort arc,ushort symbol,byte compositeCategory,CancellationToken cancellationToken);

/// <summary>Scoped fresh23f B0/B8 plain-label, zero-Arabic-trait passes from
///1ef070. Aliasfold=0; no primitive, fallback, punctuation-skipping or cross-
///descriptor branches. E0 and caps are explicit native caller policy; no
///source137/shadda/default owner flag is inferred. Not a dictionary decoder.</summary>
public sealed class LegacyFirstOrdinaryContextPasses
{
    private readonly LegacyFirstAcousticRecordTable[][] b0,b8;
    public int NodeCount=>b0.Length;
    public int BeamWidth {get;}
    public int ColumnCountB0 {get;}
    public int ColumnCountB8 {get;}
    public LegacySymbolContextFactory Contexts {get;}
    private LegacyFirstOrdinaryContextPasses(LegacyFirstAcousticRecordTable[][] b0,LegacyFirstAcousticRecordTable[][] b8,
        int width,int cap0,int cap8,LegacySymbolContextFactory contexts)
    {this.b0=b0;this.b8=b8;BeamWidth=width;ColumnCountB0=cap0;ColumnCountB8=cap8;Contexts=contexts;}

    public LegacyFirstAcousticRecordTable GetB0RowCopy(int node,int row)=>Copy(b0,node,row);
    public LegacyFirstAcousticRecordTable GetB8RowCopy(int node,int row)=>Copy(b8,node,row);
    public ReadOnlyMemory<byte>[] CopyB0Storage()=>Storage(b0,ColumnCountB0);
    public ReadOnlyMemory<byte>[] CopyB8Storage()=>Storage(b8,ColumnCountB8);

    public static LegacyFirstOrdinaryContextPasses Run(LegacyFirstOrdinaryAcousticRecurrence primary,
        LegacyOrdinaryCandidateConnections connections,LegacyInkAlphabet ownedAlphabet,
        LegacyFirstOrdinaryGlyphCost glyphCost,LegacyFirstOrdinaryTransitionCorrection correction118,
        LegacyFirstOrdinaryTransitionCorrection correction140,int columnCountB0,int columnCountB8,
        bool nativeModeE0,LegacySymbolContextFactory contexts,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(primary);ArgumentNullException.ThrowIfNull(connections);ArgumentNullException.ThrowIfNull(ownedAlphabet);
        ArgumentNullException.ThrowIfNull(glyphCost);ArgumentNullException.ThrowIfNull(correction118);ArgumentNullException.ThrowIfNull(correction140);
        ArgumentNullException.ThrowIfNull(contexts);cancellationToken.ThrowIfCancellationRequested();
        if(primary.NodeCount!=connections.NodeCount||columnCountB0 is <1 or >20||columnCountB8 is <1 or >20||
            (long)primary.NodeCount*primary.BeamWidth*(columnCountB0+columnCountB8)>1_048_576||contexts.NodeCount!=0)
            throw new ArgumentException("Matching bounded primary graph and fresh empty native symbolic container required.");
        var rows=new LegacyFirstAcousticRecordTable[primary.NodeCount];
        for(int node=0;node<rows.Length;node++){cancellationToken.ThrowIfCancellationRequested();rows[node]=primary.GetRowCopy(node);}
        foreach(var row in rows)for(int i=0;i<row.Count;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var r=row.Get(i);
            if(r.Traits!=0||!ownedAlphabet.TryGetText(r.Symbol,out string text)||text.Length!=1||
                !(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))
                throw new NotSupportedException("Only the proved plain Latin/Cyrillic/digit zero-trait context branch is supported.");
        }
        var first=new LegacyFirstAcousticRecordTable[rows.Length][];var second=new LegacyFirstAcousticRecordTable[rows.Length][];
        for(int node=0;node<rows.Length;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            first[node]=Enumerable.Range(0,primary.BeamWidth).Select(_=>new LegacyFirstAcousticRecordTable(columnCountB0)).ToArray();
            second[node]=Enumerable.Range(0,primary.BeamWidth).Select(_=>new LegacyFirstAcousticRecordTable(columnCountB8)).ToArray();
            Produce(first,false);Produce(second,true);
            void Produce(LegacyFirstAcousticRecordTable[][] tables,bool final)
            {
                var current=rows[node];
                for(int ordinal=0;ordinal<current.Count;ordinal++)
                {
                    cancellationToken.ThrowIfCancellationRequested();var r=current.Get(ordinal);if(r.TotalCost>=0x4000000)break;
                    int previousNode=node-r.Span-1;
                    if(previousNode==-1)
                    {
                        //Native root branch copies all40 bytes, then overwrites18.
                        var source=current.CopyStorage().AsSpan(ordinal*40,40).ToArray();
                        ulong identity=contexts.ResolveInitial(r.Symbol,cancellationToken).Identity;
                        if(identity==0)throw new InvalidOperationException("Bounded symbolic allocation failed.");
                        BinaryPrimitives.WriteUInt64LittleEndian(source.AsSpan(24),identity);
                        var storage=tables[node][ordinal].CopyStorage();source.CopyTo(storage,0);
                        tables[node][ordinal]=new(storage,1);continue;
                    }
                    if(previousNode<0)throw new InvalidDataException("Invalid native packed span.");
                    cancellationToken.ThrowIfCancellationRequested();int groupCost=glyphCost(node,r.ArcOrdinal,r.Symbol,r.CompositeCategory,cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();if(groupCost>=0x4000000)continue;
                    for(int priorRow=0;priorRow<rows[previousNode].Count;priorRow++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();if(rows[previousNode].Get(priorRow).TotalCost>=0x4000000)break;
                        var previous=tables[previousNode][priorRow];for(int alternative=0;alternative<previous.Count;alternative++)
                        {
                            var p=previous.Get(alternative);if(p.TotalCost>=0x4000000)break;
                            bool compatible=connections.GetSuccessorOrdinals(previousNode,p.ArcOrdinal,r.Span,cancellationToken).Span.Contains((int)r.ArcOrdinal);
                            if(!compatible)continue;
                            //B0 selects the primary path bit; B8 tries both when
                            //E0=0 and the primary-selected alternative whenE0=1.
                            if(final?(!nativeModeE0||!r.PathBit):!r.PathBit)Insert(0);
                            if(final?(!nativeModeE0||r.PathBit):r.PathBit)Insert(2);
                            void Insert(byte kind)
                            {
                                var request=new LegacyFirstOrdinaryTransitionRequest(node,r.ArcOrdinal,r.Symbol,r.CompositeCategory,
                                    previousNode,p.ArcOrdinal,p.Symbol,p.CompositeCategory,r.Span,kind);
                                int subtotal=checked(p.TotalCost+groupCost);
                                //Context passes reverse the primary callback order.
                                cancellationToken.ThrowIfCancellationRequested();var a=correction140(request,subtotal,cancellationToken);
                                cancellationToken.ThrowIfCancellationRequested();if(!a.Accepted)return;
                                var b=correction118(request,a.Cost,cancellationToken);
                                cancellationToken.ThrowIfCancellationRequested();if(!b.Accepted||b.Cost>=0x4000000)return;
                                ulong identity;
                                if(!final&&kind==2)identity=contexts.ResolveInitial(r.Symbol,cancellationToken).Identity;
                                else
                                {
                                    ulong parent=p.Identity;
                                    if(final&&kind==2&&!nativeModeE0)parent=Resolve(parent,0xfffb);
                                    identity=Resolve(parent,r.Symbol);
                                }
                                if(identity==0)throw new InvalidOperationException("Bounded symbolic allocation failed.");
                                tables[node][ordinal].Insert(new(b.Cost,checked(p.AcousticCost+groupCost),checked((byte)priorRow),
                                    checked((byte)alternative),r.ArcOrdinal,r.Span,r.Symbol,kind==2,identity,r.CompositeCategory,0),cancellationToken);
                            }
                        }
                    }
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();return new(first,second,primary.BeamWidth,columnCountB0,columnCountB8,contexts);
        ulong Resolve(ulong parent,ushort symbol)
        {
            var context=parent==0?contexts.ResolveInitial(symbol,cancellationToken):contexts.ResolveChild(parent,symbol,cancellationToken);
            if(context.Created&&parent!=0)contexts.LinkCreatedContext(context.Identity,parent);
            if(context.Identity==0)throw new InvalidOperationException("Bounded symbolic allocation failed.");return context.Identity;
        }
    }
    private static LegacyFirstAcousticRecordTable Copy(LegacyFirstAcousticRecordTable[][] data,int node,int row)
    {if((uint)node>=(uint)data.Length||(uint)row>=(uint)data[node].Length)throw new ArgumentOutOfRangeException(nameof(node));var t=data[node][row];return new(t.CopyStorage(),t.Count);}
    private static ReadOnlyMemory<byte>[] Storage(LegacyFirstAcousticRecordTable[][] data,int cap)
    {
        var result=new ReadOnlyMemory<byte>[data.Length];for(int node=0;node<data.Length;node++)
        {
            var bytes=new byte[checked(data[node].Length*cap*40)];for(int row=0;row<data[node].Length;row++)data[node][row].CopyStorage().CopyTo(bytes,row*cap*40);result[node]=bytes;
        }return result;
    }
}
