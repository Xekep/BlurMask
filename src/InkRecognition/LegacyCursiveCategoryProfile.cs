using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Opaque geometry prototype serialized in the old sampler category profile.</summary>
public readonly record struct LegacyCursiveCategoryPrototype(float First, float Second, float Third, bool Flag);

/// <summary>Native composite category: original first-list ID and geometry-list index.</summary>
public readonly record struct LegacyCursiveCompositeCategory(int FirstId, int GeometryIndex);

/// <summary>
/// Bounded version201 reader010d80 and composite-category builder0103e0.
/// This is a discrete mapping/prototype structure, not a neural network.
/// </summary>
public sealed class LegacyCursiveCategoryProfile
{
    public int BaseEntryCount { get; }
    public ReadOnlyMemory<int> FirstIds { get; }
    public ReadOnlyMemory<int> GeometryIds { get; }
    public ReadOnlyMemory<sbyte> FirstMap { get; }
    public ReadOnlyMemory<sbyte> StartGeometryMap { get; }
    public ReadOnlyMemory<sbyte> EndGeometryMap { get; }
    public ReadOnlyMemory<LegacyCursiveCategoryPrototype> Prototypes { get; }
    public ReadOnlyMemory<LegacyCursiveCompositeCategory> CompositeCategories { get; }
    public ReadOnlyMemory<int> StartCompositeMap { get; }
    public ReadOnlyMemory<int> EndCompositeMap { get; }
    public ReadOnlyMemory<int> ZeroStartCompositeMap { get; }
    public ReadOnlyMemory<int> ZeroEndCompositeMap { get; }

    private LegacyCursiveCategoryProfile(int baseCount, int[] firstIds, int[] geometryIds,
        sbyte[] first, sbyte[] start, sbyte[] end, LegacyCursiveCategoryPrototype[] prototypes,
        LegacyCursiveCompositeCategory[] categories, int[][] maps)
    {
        BaseEntryCount=baseCount; FirstIds=firstIds; GeometryIds=geometryIds;
        FirstMap=first; StartGeometryMap=start; EndGeometryMap=end; Prototypes=prototypes;
        CompositeCategories=categories; StartCompositeMap=maps[0]; EndCompositeMap=maps[1];
        ZeroStartCompositeMap=maps[2]; ZeroEndCompositeMap=maps[3];
    }

    /// <summary>
    /// Read one old profile beginning with ushort version201 and ushort base
    /// entry count. Alignment is relative to the original resource stream,
    /// not this sliced span; absoluteOffset must identify its original start.
    /// Following sampler configuration remains unconsumed.
    /// </summary>
    public static LegacyCursiveCategoryProfile Load(ReadOnlySpan<byte> serialized, int absoluteOffset,
        out int consumedBytes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (absoluteOffset < 0) throw new ArgumentOutOfRangeException(nameof(absoluteOffset));
        var reader = new Reader(serialized, absoluteOffset);
        if (reader.UInt16()!=0x201) throw new InvalidDataException("Only confirmed profile version201 is supported.");
        int baseCount=reader.UInt16();
        if(baseCount is < 1 or > 4093) throw new InvalidDataException("Category entry count exceeds the bounded profile.");
        int entries=baseCount+3;
        int[] firstIds=reader.Ids(7);
        sbyte[] first=reader.Map(entries,firstIds.Length);
        int[] geometryIds=reader.Ids(63);
        sbyte[] start=reader.Map(entries,geometryIds.Length),end=reader.Map(entries,geometryIds.Length);
        var prototypes=new LegacyCursiveCategoryPrototype[geometryIds.Length];
        for(int i=0;i<prototypes.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float a=reader.Float(),b=reader.Float(),c=reader.Float();
            byte flag=reader.Byte();
            prototypes[i]=new(a,b,c,flag!=0);
        }
        int zeroIndex=Array.IndexOf(firstIds,0);
        if(zeroIndex < 0) throw new InvalidDataException("Profile lacks the native zero-ID category.");
        var categories=new List<LegacyCursiveCompositeCategory>();
        var keyToCategory=new Dictionary<(int First,int Geometry),int>();
        int[][] maps=Enumerable.Range(0,4).Select(_=>new int[entries]).ToArray();
        for(int i=0;i<entries;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Preserve native entry-major, four-map encounter order.
            maps[0][i]=Intern(first[i],start[i]); maps[1][i]=Intern(first[i],end[i]);
            maps[2][i]=Intern(zeroIndex,start[i]); maps[3][i]=Intern(zeroIndex,end[i]);
        }
        consumedBytes=reader.Position;
        return new(baseCount,firstIds,geometryIds,first,start,end,prototypes,categories.ToArray(),maps);

        int Intern(int a,int b)
        {
            if(a==-1 || b==-1)return -1;
            if(keyToCategory.TryGetValue((a,b),out int category))return category;
            if(categories.Count>=127)throw new InvalidDataException("Composite category index exceeds native signed-byte range.");
            int index=categories.Count; keyToCategory.Add((a,b),index);
            categories.Add(new(firstIds[a],b));return index;
        }
    }

    private ref struct Reader(ReadOnlySpan<byte> bytes, int absoluteOffset)
    {
        public int Position { get; private set; }
        private readonly ReadOnlySpan<byte> bytes=bytes;
        private readonly int absoluteOffset=absoluteOffset;
        private ReadOnlySpan<byte> Take(int n)
        {
            if(n < 0 || n>bytes.Length-Position)throw new InvalidDataException("Truncated category profile.");
            var value=bytes.Slice(Position,n);Position+=n;return value;
        }
        public byte Byte()=>Take(1)[0];
        public ushort UInt16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public float Float()
        {
            float value=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(Take(4)));
            if(!float.IsFinite(value))throw new InvalidDataException("Nonfinite category prototype.");return value;
        }
        public int[] Ids(int maximum)
        {
            int count=Byte();
            if(count<1 || count>maximum)throw new InvalidDataException("Category count exceeds native table bounds.");
            int padding=(int)(-(absoluteOffset+(long)Position)&3);Take(padding);
            int[] ids=new int[count];
            for(int i=0;i<count;i++)ids[i]=BinaryPrimitives.ReadInt32LittleEndian(Take(4));
            return ids;
        }
        public sbyte[] Map(int count,int categories)
        {
            var data=Take(count);var map=new sbyte[count];
            for(int i=0;i<count;i++)
            {
                sbyte value=unchecked((sbyte)data[i]);
                if(value < -1 || value >= categories)throw new InvalidDataException("Category map index is out of bounds.");
                map[i]=value;
            }
            return map;
        }
    }
}
