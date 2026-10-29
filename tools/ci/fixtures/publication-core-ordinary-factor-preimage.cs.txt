using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Checked source219 configuration extracted from one ordinary resource247/v10d.</summary>
public readonly record struct LegacyOrdinarySourceProfile(int SourceKind, bool EnclosureEnabled,
    int ParametersOffset, int EnclosureFlagOffset);

/// <summary>
/// Ordinary167210 physical float ingress, point identities and2ad enclosure
/// service composition. No opaque records, custom input scopes, page coordinates
/// or automatic line/group metadata are reconstructed here.
/// </summary>
public sealed class LegacyOrdinarySourceInk
{
    private readonly InkStroke[] strokes;
    private readonly int[][] rawIndices;
    private readonly float[] probabilities;
    private readonly int[] originalCounts, metadataStarts;
    private readonly LegacyOrdinarySourceInk? originalMetadataOwner;
    private readonly IReadOnlyList<InkStroke> strokeView;
    private readonly IReadOnlyList<ReadOnlyMemory<int>> indexView;
    public IReadOnlyList<InkStroke> Strokes => strokeView;
    public IReadOnlyList<ReadOnlyMemory<int>> OriginalPointIndices => indexView;
    public ReadOnlyMemory<float> SourceStrokeProbabilities => probabilities;
    public LegacySourcePointChronology Chronology { get; }
    public LegacyOrdinarySourceProfile Profile { get; }

    private LegacyOrdinarySourceInk(InkStroke[] strokes, int[][] rawIndices, float[] probabilities,
        LegacySourcePointChronology chronology, LegacyOrdinarySourceProfile profile,int[] originalCounts,
        LegacyOrdinarySourceInk? originalMetadataOwner=null)
    {
        this.strokes=strokes;this.rawIndices=rawIndices;this.probabilities=probabilities;
        strokeView=Array.AsReadOnly(strokes);indexView=Array.AsReadOnly(rawIndices.Select(a=>new ReadOnlyMemory<int>(a)).ToArray());
        Chronology=chronology;Profile=profile;
        this.originalCounts=originalCounts;this.originalMetadataOwner=originalMetadataOwner;metadataStarts=new int[strokes.Length];
        for(int i=1;i<metadataStarts.Length;i++)metadataStarts[i]=metadataStarts[i-1]+rawIndices[i-1].Length;
    }

    public static LegacyOrdinarySourceInk Prepare(IReadOnlyList<InkStroke> rawSource,
        LegacyOrdinarySourceProfile profile, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(rawSource);
        cancellationToken.ThrowIfCancellationRequested();
        if(rawSource.Count is <1 or >256)throw new ArgumentException("Expected1..256 physical strokes.",nameof(rawSource));
        if(profile.SourceKind is <0x135 or >0x137)throw new ArgumentException("Unsupported ordinary source kind.",nameof(profile));
        var strokes=new InkStroke[rawSource.Count];var rawIndices=new int[rawSource.Count][];var originalCounts=new int[rawSource.Count];
        var keys=new List<LegacySourcePointKey>();int rawTotal=0;
        for(int s=0;s<strokes.Length;s++)
        {
            var raw=rawSource[s]?.Points??throw new ArgumentException("Null stroke.",nameof(rawSource));
            if(raw.Count is <1 or >4096 || rawTotal>4096-raw.Count)
                throw new ArgumentException("Expected1..4096 total raw points.",nameof(rawSource));
            rawTotal+=raw.Count;
            originalCounts[s]=raw.Count;
            var retained=new List<InkPoint>();var indices=new List<int>();
            for(int p=0;p<raw.Count;p++)
            {
                cancellationToken.ThrowIfCancellationRequested();InkPoint point=raw[p];
                if(!float.IsFinite(point.X)||!float.IsFinite(point.Y)||MathF.Abs(point.X)>1_000_000||MathF.Abs(point.Y)>1_000_000)
                    throw new ArgumentException("Expected finite raw coordinates within ±1000000.",nameof(rawSource));
                // Compare with the preceding RAW point, even when it was dropped.
                if(p!=0 && !Different(point.X,raw[p-1].X) && !Different(point.Y,raw[p-1].Y))continue;
                retained.Add(point);indices.Add(p);keys.Add(new(0,s,p));
            }
            strokes[s]=new(retained.ToArray());rawIndices[s]=indices.ToArray();
        }
        int[] counts=strokes.Select(s=>s.Points.Count).ToArray();
        var chronology=LegacySourcePointChronology.CreatePrepared(counts,keys.ToArray(),Enumerable.Range(0,keys.Count).ToArray());
        float[] probabilities=profile.EnclosureEnabled?LegacySourceStrokeEnclosure.Extract(strokes,cancellationToken):new float[strokes.Length];
        return new(strokes,rawIndices,probabilities,chronology,profile,originalCounts);
    }

    /// <summary>Ordinary167210 writes a second twelve-byte metadata owner108:
    /// retained point's end ordinal is next retained original ordinal minus1,
    /// or original input count minus1 at the physical end.168e10 addresses that
    /// owner using the SAME ceil-map/d8 index as168d00. Prepared identity must
    /// resolve this source's actual retained metadata; duplicate runs are not
    /// collapsed to their starting ordinal.</summary>
    public LegacySourcePointIdentity ResolveEndingMetadata(LegacySourcePointIdentity actualPreparedIdentity,
        CancellationToken cancellationToken=default)=>ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,null);
    internal LegacySourcePointIdentity ResolveEndingMetadataMetered(LegacySourcePointIdentity actualPreparedIdentity,
        CancellationToken cancellationToken,LegacyOrdinarySourceTraversalMeter meter)
        =>ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,meter);
    private LegacySourcePointIdentity ResolveEndingMetadataCore(LegacySourcePointIdentity actualPreparedIdentity,
        CancellationToken cancellationToken,LegacyOrdinarySourceTraversalMeter? meter)
    {
        cancellationToken.ThrowIfCancellationRequested();meter?.Before(256);
        if(originalMetadataOwner is{} owner)return owner.ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,meter);
        int physical=actualPreparedIdentity.Key.Second;
        if(actualPreparedIdentity.Key.First!=0||(uint)physical>=(uint)rawIndices.Length)
            throw new ArgumentException("Actual ordinary source metadata required.");
        int local=actualPreparedIdentity.KeyRecordIndex-metadataStarts[physical];
        if((uint)local>=(uint)rawIndices[physical].Length||actualPreparedIdentity.Key.Third!=rawIndices[physical][local])
            throw new ArgumentException("Prepared metadata does not belong to this source's retained key owner.");
        int end=local+1<rawIndices[physical].Length?rawIndices[physical][local+1]-1:originalCounts[physical]-1;
        return actualPreparedIdentity with{Key=new(0,physical,end)};
    }

    /// <summary>Typed current source218 view after the owned fresh main physical
    /// reorder. No167210 filter/enclosure pass is repeated. Physical probabilities
    /// and original point indices are permuted; original metadata keys continue
    /// addressing the original raw owner even when current physical IDs differ.</summary>
    internal static LegacyOrdinarySourceInk CreateOwnedMainPhysicalReorderedView(
        LegacyFreshMainPhysicalReordering actualReordering,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualReordering);cancellationToken.ThrowIfCancellationRequested();
        var original=actualReordering.Input.Input.Packet.Source;
        int count=original.Strokes.Count;int[] order=actualReordering.OriginalPhysicalStrokeIndices.ToArray();
        if(order.Length!=count||actualReordering.Strokes.Count!=count||order.Distinct().Count()!=count||
            order.Any(i=>(uint)i>=(uint)count))throw new ArgumentException("Actual complete physical reorder required.");
        var strokes=new InkStroke[count];var indices=new int[count][];var counts=new int[count];var probabilities=new float[count];
        for(int current=0;current<count;current++)
        {
            cancellationToken.ThrowIfCancellationRequested();int old=order[current];
            var points=actualReordering.Strokes[current].Points;
            if(!points.SequenceEqual(original.Strokes[old].Points)||actualReordering.SourceStrokeProbabilities.Span[current]!=original.probabilities[old])
                throw new ArgumentException("Retained current points/probabilities do not belong to the original owner.");
            for(int point=0;point<points.Count;point++)
            {
                var actual=actualReordering.Chronology.Resolve(current,point);var expected=original.Chronology.Resolve(old,point);
                if(actual.Key!=expected.Key||actual.KeyRecordIndex!=expected.KeyRecordIndex||actual.MetadataPointIndex!=expected.MetadataPointIndex||actual.RemainderByte!=0)
                    throw new ArgumentException("Actual original retained metadata identity required.");
            }
            strokes[current]=new(points.ToArray());indices[current]=original.rawIndices[old].ToArray();
            counts[current]=original.originalCounts[old];probabilities[current]=original.probabilities[old];
        }
        return new(strokes,indices,probabilities,actualReordering.Chronology,original.Profile,counts,original);
    }

    /// <summary>
    /// Parse resource247/v10d's source-configuration prefix using readers1d4950
    /// and1d41e0. Metadata bytes are skipped as data, not interpreted as a license.
    /// No authentication, activation, vendor resource execution or mode defaults.
    /// </summary>
    public static LegacyOrdinarySourceProfile ReadV10dResourceProfile(ReadOnlySpan<byte> resource,
        int offset=0)
    {
        if(resource.Length is <16 or >64*1024*1024 || offset<0 || offset>resource.Length-16)
            throw new InvalidDataException("Invalid source resource bounds.");
        var r=new Reader(resource,offset);
        if(r.U32()!=0x0101fe90)throw new InvalidDataException("Invalid resource magic.");
        uint length=r.U32();if(length>resource.Length-offset-8 || length<8)throw new InvalidDataException("Invalid resource length.");
        int end=checked(offset+8+(int)length);r.End=end;
        if(r.U32()!=0x247 || r.U16()!=0 || r.U16()!=0x10d || r.U16()!=0x201 || r.U16()!=0x100)
            throw new InvalidDataException("Expected unencoded247/v10d,134/v201 and133/v100.");
        r.Skip(4+56);int marker=r.I32();if(marker==-1)r.Skip(4);r.Skip(4);
        if(r.Flag())
        {
            if(r.U32()!=0x2b)throw new InvalidDataException("Expected inline acoustic alphabet.");
            ushort version=r.U16();var alphabet=LegacyInkAlphabet.Load(resource.Slice(r.Position,end-r.Position),version);
            r.Skip(alphabet.BytesConsumed);
        }
        int sourceKind=r.I32();if(sourceKind is <0x135 or >0x137)throw new InvalidDataException("Unsupported source kind.");
        byte mode=r.U8();if(mode>2)throw new InvalidDataException("Unsupported source coordinate mode.");
        if(mode==2){int count=r.U8();r.Skip(checked(count*4+8+7));}
        r.Skip(1); // v10d's legacy skip byte before b8/b9/ba/bc.
        r.Flag();r.Flag();r.Skip(4);
        int parametersOffset=r.Position;
        r.Skip(16);bool processor5=r.Flag(),processor6a=r.Flag(),processor7=r.Flag(),processor6b=r.Flag();r.Skip(4);
        if(processor5)r.Node();if(processor6a||processor6b)r.Node();if(processor7)r.Node();
        int flagOffset=r.Position;bool enabled=r.Flag();r.Flag();r.Skip(4);r.Flag();r.Skip(4);r.Flag();
        int processors=r.I32();if(processors is <0 or >256)throw new InvalidDataException("Invalid source processor count.");
        for(int i=0;i<processors;i++)r.Node();
        return new(sourceKind,enabled,parametersOffset,flagOffset);
    }

    private static bool Different(float a,float b)=>Math.Abs((double)a)+Math.Abs((double)b)<Math.Abs((double)(a-b))*8388608d;

    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> data;
        public int Position;public int End;
        public Reader(ReadOnlySpan<byte> data,int position){this.data=data;Position=position;End=data.Length;}
        public void Skip(int count){if(count<0||count>End-Position)throw new InvalidDataException("Truncated source configuration.");Position+=count;}
        public byte U8(){int p=Position;Skip(1);return data[p];}
        public bool Flag(){byte value=U8();if(value>1)throw new InvalidDataException("Invalid source flag.");return value!=0;}
        public ushort U16(){int p=Position;Skip(2);return BinaryPrimitives.ReadUInt16LittleEndian(data[p..]);}
        public uint U32(){int p=Position;Skip(4);return BinaryPrimitives.ReadUInt32LittleEndian(data[p..]);}
        public int I32()=>unchecked((int)U32());
        public void Node(){int start=Position;if(U32()!=0x0101fe90)throw new InvalidDataException("Invalid nested source resource.");uint size=U32();if(size<8||size>End-start-8)throw new InvalidDataException("Invalid nested source resource length.");Skip((int)size);}
    }
}
