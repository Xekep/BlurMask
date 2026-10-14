using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Resource-owned fresh main0 source before the later15B260 descriptor
/// split and15F050 physical reordering. Actual raw points, primary source scale,
/// and2E3 unknown-line memberships share the same ingress owner. No24A frame,
/// decoded text, prior-source metadata or selected main winner is manufactured.</summary>
public sealed class LegacyFreshMainPhysicalGrouping
{
    private const string AcousticHash="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978";
    private const string SpatialHash="10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e";
    private const string PhysicalHash="01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e";
    public LegacyFreshOrdinaryRawPacket Packet {get;}
    public LegacyOrdinaryLineScaleSnapshot Scale {get;}
    public LegacyOrdinaryNeuralLineGrouping Grouping {get;}
    public string SourceFingerprint=>AcousticHash;
    public int SourceOffset=>0;
    public int ProviderType=>0x2e3;
    public byte CoordinateMode=>2;
    public bool QuarterVerticalWhenHalfHorizontal=>true;
    public float InternalScaleQuantile=>.8f;
    public float WindowWidthMultiplier=>5;
    public float WindowOverlap=>.75f;
    public float HorizontalOrderTolerance=>.25f;
    public float PhysicalResolution {get;}
    private LegacyFreshMainPhysicalGrouping(LegacyFreshOrdinaryRawPacket packet,
        LegacyOrdinaryLineScaleSnapshot scale,LegacyOrdinaryNeuralLineGrouping grouping,float resolution)
    {Packet=packet;Scale=scale;Grouping=grouping;PhysicalResolution=resolution;}

    /// <summary>Exactly pinned RU247/v10D main source0, direct unscoped input,
    /// no property events/prior source/page guides. Model streams must be the
    /// two portable embedded2E3 resources. Actual source resolution is mandatory;
    /// zero is legitimate, a guessed DPI is not. This is the memberships stage
    /// of15B260, not completed descriptor splitting,15F050 or main recognition.</summary>
    public static LegacyFreshMainPhysicalGrouping Prepare(ReadOnlySpan<byte> actualResource,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,float actualPhysicalResolution,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(rawPhysicalStrokes);
        if(!float.IsFinite(actualPhysicalResolution)||actualPhysicalResolution<0||actualPhysicalResolution>1_000_000)
            throw new ArgumentException("Mandatory bounded actual source resolution required.");
        CheckHash(actualResource,572763,AcousticHash,"RU main source");
        CheckHash(spatial30Resource,85812,SpatialHash,"Embedded spatial30 classifier");
        CheckHash(physical50Resource,161332,PhysicalHash,"Embedded physical50 classifier");
        var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(actualResource,0);
        var alphabet=LegacySourceAlphabetByteLookup.ReadInlineV10dSource(actualResource,0);
        int kindAt=checked(alphabet.AlphabetPayloadOffset+alphabet.BytesConsumed);
        if(profile.SourceKind!=0x135||BinaryPrimitives.ReadInt32LittleEndian(actualResource[kindAt..])!=0x135||
            actualResource[kindAt+4]!=2)
            throw new InvalidDataException("Main ordinary135/mode2 source binding required.");
        //1D41E0 ->163970 copies config flags to source28..2B. Fresh15A1C0
        //selects2E3 by processor6/2AB's child2A9 byte9, not a caller default.
        int flags=profile.ParametersOffset+16;
        if(!actualResource.Slice(flags,4).SequenceEqual(new byte[]{0,1,1,1}))
            throw new InvalidDataException("Actual fresh main processor flags required.");
        int processor6=flags+8;
        int processor6End=NodeEnd(actualResource,processor6,0x2ab,0x101,actualResource.Length);
        int providerChild=processor6+16;
        _=NodeEnd(actualResource,providerChild,0x2a9,0x100,processor6End);
        if(actualResource[providerChild+16+9]!=0)
            throw new NotSupportedException("Page/prior2E2 provider branch is not this fresh2E3 stage.");
        float orderTolerance=BinaryPrimitives.ReadSingleLittleEndian(actualResource[(flags+4)..]);
        if(orderTolerance!=.25f)throw new InvalidDataException("Actual fresh main ordering tolerance required.");
        var packet=LegacyFreshOrdinaryRawPacket.PrepareDirect(rawPhysicalStrokes,profile,cancellationToken);
        var scale=LegacyOrdinaryLineScaleSource.Extract(packet.Source,
            LegacyOrdinaryLineScaleSource.ReadV10dResourceProfile(actualResource,0),actualPhysicalResolution,cancellationToken);
        using var spatialStream=new MemoryStream(spatial30Resource.ToArray(),false);
        using var physicalStream=new MemoryStream(physical50Resource.ToArray(),false);
        var spatial=PreparedLineBoundaryClassifier.Load(spatialStream);
        var physical=PreparedLineBoundaryClassifier.Load(physicalStream);
        //Fresh077910 writes receiver physical byte4=0. Source physical+4=-1
        //is a distinct association field and must not be used as a half-X flag.
        bool[] halfHorizontalWeights=new bool[packet.PhysicalStrokeCount];
        var grouping=LegacyOrdinaryNeuralLineInferenceFactory.InferGrouping(scale,actualPhysicalResolution,
            halfHorizontalWeights,true,.8f,spatial,physical,5,.75f,cancellationToken);
        if(!ReferenceEquals(grouping.Source,scale))throw new InvalidDataException("Source ownership lost.");
        return new(packet,scale,grouping,actualPhysicalResolution);
    }
    private static void CheckHash(ReadOnlySpan<byte> bytes,int size,string expected,string name)
    {
        if(bytes.Length!=size||!Convert.ToHexString(SHA256.HashData(bytes)).Equals(expected,StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Exact checked {name} resource required.");
    }
    private static int NodeEnd(ReadOnlySpan<byte> resource,int at,int kind,ushort version,int parentEnd)
    {
        if(at<0||at>parentEnd-16||BinaryPrimitives.ReadUInt32LittleEndian(resource[at..])!=0x0101fe90||
            BinaryPrimitives.ReadInt32LittleEndian(resource[(at+8)..])!=kind||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(at+14)..])!=version)
            throw new InvalidDataException("Actual source provider resource node required.");
        uint size=BinaryPrimitives.ReadUInt32LittleEndian(resource[(at+4)..]);
        if(size<8||size>(uint)(parentEnd-at-8))throw new InvalidDataException("Invalid source provider node bounds.");
        return checked(at+8+(int)size);
    }
}
