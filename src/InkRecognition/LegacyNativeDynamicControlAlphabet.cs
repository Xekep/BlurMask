using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyControlAlphabetImport.Probe")]

namespace InkRecognition;

internal enum LegacyControlAlphabetAllocation { HashRoot, HashMiddle, HashBucket, Leaf, SymbolVector }

/// <summary>Engine2A/050F10's fresh IO5 byte interner. Symbol IDs are insertion
/// IDs, independent of primary acoustic labels. Native pointers and hash-chain
/// storage are represented by owned semantic entries, never serialized pointers.</summary>
public sealed class LegacyNativeDynamicControlAlphabet
{
    private sealed class Entry(byte[] bytes) { internal readonly byte[] Bytes=bytes; internal ushort Label; }
    private readonly Dictionary<string,Entry> entries=[];
    private readonly List<Entry> labels=[];
    private readonly HashSet<int> middle=[],buckets=[];
    private readonly Func<LegacyControlAlphabetAllocation,int,bool>? allocator;
    private readonly byte? unwrittenNewFlagResidue;
    private readonly object gate=new();
    private bool root;
    private int capacity,totalBytes,invalidations;
    private ushort lineFeed=ushort.MaxValue,space=ushort.MaxValue,nbsp=ushort.MaxValue;
    private LegacyNativeDynamicControlAlphabet(Func<LegacyControlAlphabetAllocation,int,bool>? allocator=null,byte? unwrittenNewFlagResidue=null)
    { this.allocator=allocator;this.unwrittenNewFlagResidue=unwrittenNewFlagResidue; }
    public static LegacyNativeDynamicControlAlphabet CreateFresh()=>new();
    internal static LegacyNativeDynamicControlAlphabet CreateWithAllocator(Func<LegacyControlAlphabetAllocation,int,bool> allocator,
        byte explicitUnwrittenNewFlagResidue)=>new(allocator,explicitUnwrittenNewFlagResidue);
    public int Count { get { lock(gate)return labels.Count; } }
    public int Capacity { get { lock(gate)return capacity; } }
    public ushort LineFeedLabel { get { lock(gate)return lineFeed; } }
    public ushort SpaceLabel { get { lock(gate)return space; } }
    public ushort NonbreakingSpaceLabel { get { lock(gate)return nbsp; } }

    /// <summary>Valid UTF8 is copied without normalization/case folding. Native
    /// malformed-codec conversion is not inferred from framework replacement.</summary>
    public ushort InsertUtf8(ReadOnlySpan<byte> utf8,CancellationToken token=default)
    { ValidateUtf8(utf8,token);return InsertCore(utf8,token); }
    public ushort LookupUtf8(ReadOnlySpan<byte> utf8,CancellationToken token=default)
    { ValidateUtf8(utf8,token);return LookupBytes(utf8,token); }
    public ushort LookupBytes(ReadOnlySpan<byte> bytes,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(bytes.Length>1_048_576)throw new ArgumentOutOfRangeException(nameof(bytes));
        lock(gate)return entries.TryGetValue(Convert.ToBase64String(bytes),out var entry)?entry.Label:ushort.MaxValue;
    }
    public byte[] ReadSymbolBytes(ushort label,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();lock(gate){if(label>=labels.Count)throw new ArgumentOutOfRangeException(nameof(label));return labels[label].Bytes.ToArray();}}

    //139DC0 passes precisely one C2 byte from literal2EC8BC=C2A7. This named
    //native boundary preserves that byte; the public valid-UTF8 helper refuses it.
    internal ushort InsertNativeUnitLiteral(CancellationToken token)=>InsertCore([0xc2],token);
    internal LegacyControlAlphabetState ReadState()
    {lock(gate)return new(labels.Count,capacity,lineFeed,space,nbsp,root,middle.Count,buckets.Count,entries.Count,totalBytes,invalidations);}

    private ushort InsertCore(ReadOnlySpan<byte> bytes,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(bytes.Length>1_048_576)throw new ArgumentOutOfRangeException(nameof(bytes));
        byte[] copy=bytes.ToArray();string key=Convert.ToBase64String(copy);
        lock(gate)
        {
            if(entries.TryGetValue(key,out var existing))return existing.Label;
            //Portable bounds avoid native U16 round16 overflow and excessive
            //source storage. They are checked before mutating the trie prefix.
            if(labels.Count==65520||entries.Count==65536||copy.Length>8_388_608-totalBytes)
                throw new NotSupportedException("Portable dynamic alphabet count/byte budget exceeded.");
            uint hash=ComputeHash(copy);int high=(int)(hash>>16&15),mid=(int)(hash>>6&31),prefix=high*32+mid;
            if(!root){if(!Allocate(LegacyControlAlphabetAllocation.HashRoot,128))return BeforeLeafFailure();root=true;}
            if(!middle.Contains(high)){if(!Allocate(LegacyControlAlphabetAllocation.HashMiddle,256))return BeforeLeafFailure();middle.Add(high);}
            if(!buckets.Contains(prefix)){if(!Allocate(LegacyControlAlphabetAllocation.HashBucket,512))return BeforeLeafFailure();buckets.Add(prefix);}
            if(!Allocate(LegacyControlAlphabetAllocation.Leaf,checked(copy.Length+20)))return BeforeLeafFailure();
            var entry=new Entry(copy);entries.Add(key,entry);totalBytes+=copy.Length;
            int required=labels.Count+1;
            //050F10 keeps the newly inserted leaf with valueFFFF if0521F0
            //fails. A duplicate subsequently returnsFFFF without retrying.
            bool reserved=true;
            if(required>capacity)
            {
                int next=(required+15)&~15;
                reserved=Allocate(LegacyControlAlphabetAllocation.SymbolVector,checked(next*8));
                if(reserved)capacity=next;
            }
            if(reserved){entry.Label=checked((ushort)labels.Count);labels.Add(entry);}
            else entry.Label=ushort.MaxValue;
            if(copy.AsSpan().SequenceEqual("\n"u8))lineFeed=entry.Label;
            else if(copy.AsSpan().SequenceEqual(" "u8))space=entry.Label;
            else if(copy.AsSpan().SequenceEqual("\u00a0"u8))nbsp=entry.Label;
            invalidations++; //Diagnostic count of the four native cache clears.
            token.ThrowIfCancellationRequested();return entry.Label;
        }
    }
    private bool Allocate(LegacyControlAlphabetAllocation stage,int bytes)=>allocator?.Invoke(stage,bytes)??true;
    private ushort BeforeLeafFailure()
    {
        //04D950 has not written its new-entry output.050F10's epilogue tests
        //that native stack byte; only the explicit fault-control boundary has it.
        if(unwrittenNewFlagResidue is not byte residue)
            throw new NotSupportedException("Native pre-leaf allocator failure needs its untouched stack-byte residue.");
        if(residue!=0)invalidations++;
        return ushort.MaxValue;
    }
    internal static uint ComputeHash(ReadOnlySpan<byte> bytes)
    {uint hash=0x1505;foreach(byte value in bytes)hash=unchecked(hash*33^value);return hash;}
    private static void ValidateUtf8(ReadOnlySpan<byte> bytes,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(bytes.Length>1_048_576)throw new ArgumentOutOfRangeException(nameof(bytes));
        while(!bytes.IsEmpty)
        {token.ThrowIfCancellationRequested();if(Rune.DecodeFromUtf8(bytes,out _,out int n)!=OperationStatus.Done)throw new ArgumentException("Valid UTF8 IO5 bytes required.");bytes=bytes[n..];}
    }
}

internal readonly record struct LegacyControlAlphabetState(int Count,int Capacity,ushort LineFeed,ushort Space,
    ushort NonbreakingSpace,bool HashRoot,int HashMiddleCount,int HashBucketCount,int LeafCount,int ByteCount,int CacheInvalidations);
