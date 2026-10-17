using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Copied274 pool conversion13B350/139DC0 into a fresh dynamic2A
/// namespace. It is not a renderer or an accepted TOP result. Failed attempts
/// retain their converted prefix and inserted control entries.</summary>
public sealed class LegacyNativeGlyphControlImport
{
    private readonly LegacyInkAlphabet primary;
    private readonly LegacyNativeGlyphRecordNode[] nodes;
    private readonly LegacyNativeDynamicControlAlphabet control;
    private readonly object gate=new();
    private LegacyNativeGlyphRecordOwner? resolvedSnapshot;
    private int started;
    public bool Succeeded {get;private set;}
    public bool IsStopped {get;private set;}
    public bool HasFinished {get;private set;}
    public int ProcessedRecordCount {get;private set;}
    public int ChangedRecordCount {get;private set;}
    public ulong? FailureReference {get;private set;}
    public string? FailureReason {get;private set;}
    public int ControlSymbolCount=>control.Count;
    public int ControlCapacity=>control.Capacity;
    public ushort LineFeedLabel=>control.LineFeedLabel;
    public ushort SpaceLabel=>control.SpaceLabel;
    public ushort NonbreakingSpaceLabel=>control.NonbreakingSpaceLabel;
    internal LegacyControlAlphabetState ControlState=>control.ReadState();
    internal LegacyNativeGlyphControlImport(IReadOnlyList<LegacyNativeGlyphRecordNode> source,
        LegacyInkAlphabet primary,LegacyNativeDynamicControlAlphabet? control=null)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(primary);
        _=new LegacyNativeGlyphRecordOwner(source); //Full key/count/record-budget validation before any conversion.
        this.primary=primary;this.control=control??LegacyNativeDynamicControlAlphabet.CreateFresh();
        if(this.control.Count!=0||this.control.ReadState().LeafCount!=0)
            throw new ArgumentException("A new, empty, separately owned control2A is required.");
        nodes=source.OrderBy(n=>unchecked((long)n.Key)).Select(n=>new LegacyNativeGlyphRecordNode(n.Key,n.PackedCount,n.Records.ToArray())).ToArray();
    }
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyNodes()
    {lock(gate)return Array.AsReadOnly(nodes.Select(n=>new LegacyNativeGlyphRecordNode(n.Key,n.PackedCount,n.Records.ToArray())).ToArray());}
    public byte[] ReadControlSymbolBytes(ushort label,CancellationToken token=default)=>control.ReadSymbolBytes(label,token);
    public ushort LookupControlBytes(ReadOnlySpan<byte> bytes,CancellationToken token=default)=>control.LookupBytes(bytes,token);
    public LegacyNativeOwnedGlyphRecord Resolve(ulong reference)
    {lock(gate)return (resolvedSnapshot??=new LegacyNativeGlyphRecordOwner(nodes)).Resolve(reference);}

    internal void Execute(CancellationToken token=default)
    {lock(gate)ExecuteCore(token);}
    private void ExecuteCore(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(Interlocked.Exchange(ref started,1)!=0)throw new InvalidOperationException("Control import is single-use, including a failed prefix.");
        resolvedSnapshot=null;
        try
        {
            ushort unit=BinaryPrimitives.ReadUInt16LittleEndian(primary.CopySerializedHeader().AsSpan(6));
            for(int nodeIndex=0;nodeIndex<nodes.Length;nodeIndex++)
            {
                var node=nodes[nodeIndex];byte[] copy=node.Records.ToArray();
                nodes[nodeIndex]=new(node.Key,node.PackedCount,copy);
                int count=unchecked((short)(node.PackedCount<<2))>>4;
                for(int ordinal=0;ordinal<count;ordinal++)
                {
                    token.ThrowIfCancellationRequested();ulong reference=node.Key|((ulong)ordinal<<40);
                    //The array is private owned storage; no caller can mutate this view.
                    var row=copy.AsSpan(ordinal*40,40);
                    ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(row[4..]),symbol=BinaryPrimitives.ReadUInt16LittleEndian(row[8..]);
                    int kind=flags&7;ushort updated=flags,label=symbol;
                    if(kind==0)
                    {
                        if(symbol==unit)label=control.InsertNativeUnitLiteral(token);
                        else if(symbol<primary.LabelCount)
                        {
                            if(!primary.TryGetText(symbol,out string text)){Fail(reference,"Primary alphabet has no text for the native source label.");return;}
                            label=control.InsertUtf8(Encoding.UTF8.GetBytes(text),token);
                        }
                        else label=control.InsertUtf8(ReservedText(symbol),token);
                        if(label==ushort.MaxValue){Fail(reference,"Native control interning returnedFFFF; its prefix is retained.");return;}
                        updated=(ushort)((flags&0xfffa)|2);
                    }
                    else if(kind==1)
                    {
                        if((symbol&0x7ffc)!=0)
                        {
                            int subkind=symbol&3;
                            if(subkind==1){Fail(reference,"Nonempty kind1/subkind1 has no defined native import token.");return;}
                            ushort inserted=control.InsertUtf8(subkind switch{0=>"\n"u8,2=>" "u8,_=>"\u00a0"u8},token);
                            if(inserted==ushort.MaxValue){Fail(reference,"Native run-token interning returnedFFFF; its prefix is retained.");return;}
                        }
                        updated=(ushort)((flags&0xfffb)|3);
                    }
                    if(updated!=flags||label!=symbol)
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(row[4..],updated);BinaryPrimitives.WriteUInt16LittleEndian(row[8..],label);
                        ChangedRecordCount++;
                    }
                    ProcessedRecordCount++;
                }
            }
            token.ThrowIfCancellationRequested();Succeeded=true;HasFinished=true;
        }
        catch{IsStopped=true;throw;}
    }
    private void Fail(ulong reference,string reason)
    {FailureReference=reference;FailureReason=reason;HasFinished=true;IsStopped=true;}
    private static ReadOnlySpan<byte> ReservedText(ushort symbol)=>symbol switch
    {
        0xffeb=>"\U000f0018"u8,0xffec=>"\U000f0017"u8,0xffed=>"\U000f0016"u8,
        0xffee=>"\U000f0015"u8,0xffef=>"\U000f0014"u8,0xfff0=>"\U000f0013"u8,
        0xfff1=>"\U000f0012"u8,0xfff2=>"\U000f0011"u8,0xfff3=>"\U000f0010"u8,
        0xfff6=>"\U000f0000"u8,0xfff9=>"\n"u8,0xfffb=>" "u8,0xfffc=>"\u00a0"u8,_=>"\ufffd"u8
    };
}
