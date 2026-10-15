using System.Security.Cryptography;
using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Selected main247.V48, frontend222.V50/151BF0 and240.V58/20C8C0 scalar
/// preparation over the existing DIRECT dictionary/source owner. This is a
/// fresh, unmodified2B3/240 branch; case/decorator construction and the complete
///2144F0 scheduling loop are not claimed by this policy.
/// </summary>
public sealed class LegacyOwnedFreshMainWordControllerPolicy
{
    public LegacyOwnedMainWordSpanPreparation Owner { get; }
    public LegacyNativeWordControllerConfiguration Configuration { get; }
    public LegacyNativeWordScalarRefreshResult Refresh { get; }
    public int MaximumParentDistance54 => 120;
    public int FlagPrepassDistance58 { get; }
    public bool KnowledgeF5 => false;
    public float SecondaryScaleEc => 0;

    private LegacyOwnedFreshMainWordControllerPolicy(LegacyOwnedMainWordSpanPreparation owner,
        LegacyNativeWordControllerConfiguration configuration, LegacyNativeWordScalarRefreshResult refresh,
        int flagDistance)
    {
        Owner = owner; Configuration = configuration; Refresh = refresh; FlagPrepassDistance58 = flagDistance;
    }

    public static LegacyOwnedFreshMainWordControllerPolicy Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainWordSpanPreparation owner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        cancellationToken.ThrowIfCancellationRequested();
        if (resource.Length != 572763)
            throw new ArgumentException("Actual main resource extent required before hashing.");
        var actualSource = owner.SelectedSourceOwner;
        string fingerprint = Convert.ToHexString(SHA256.HashData(resource));
        if (!StringComparer.OrdinalIgnoreCase.Equals(fingerprint, actualSource.SourceFingerprint) ||
            actualSource.SelectedSourceOffset != 0)
            throw new ArgumentException("Scalar preparation requires this owner's actual selected main0 resource.");
        var selected = ReadSelectedPolicy(resource,owner.OrdinaryAcousticOffset,actualSource.SelectedSourceOffset);
        if (selected != owner.AcousticPolicy)
            throw new ArgumentException("Selected source metadata must match the already prepared acoustic owner.");
        //1D60D0 preserves NEW00C860 mode13/variant2 and resolves28 from the
        //selected source's variant table.151BF0 writes54=120,58=that28.
        int distance = selected.SourceAlternativeLimit;
        var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource, 0);
        byte consecutive = resource[sourceProfile.ParametersOffset - 6];
        byte format = resource[sourceProfile.ParametersOffset - 5];
        if (consecutive > 1 || format > 1)
            throw new InvalidDataException("Validated247 B8/B9 flags required.");
        var model = owner.CommonModel;
        bool marker = model.AcousticAlphabet.TryGetLabel("\u202b", out _);
        var refresh = LegacyNativeWordScalarRefresh.Apply(0, owner.CommonSourceSummary.Flags,
            false, consecutive, 0, format, -1, 0, marker);
        var fresh = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
        { PackedSource22e8 = refresh.PackedSource22e8 };
        var prepared = LegacyNativeWordControllerLimitPreparation.Prepare(fresh,
            checked((uint)selected.MaximumSourceArcs), LegacyNativeWordKnowledgeLimits.CreateFresh00c860(), true);
        cancellationToken.ThrowIfCancellationRequested();
        return new(owner, prepared.Configuration, refresh, distance);
    }

    //The same selected metadata reader for both owned source variants. No
    //single-glyph owner is manufactured to satisfy the earlier reader's type.
    private static LegacyPreparedFreshMain240AcousticPolicy ReadSelectedPolicy(ReadOnlySpan<byte> resource,
        int ordinary,int selectedSource)
    {
        int tail=checked(ordinary+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(ordinary+4)..]));
        int end=checked(selectedSource+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(selectedSource+4)..]));
        if(end-tail!=190||BinaryPrimitives.ReadInt32LittleEndian(resource[tail..])!=0)
            throw new NotSupportedException("Selected main metadata without source extensions required.");
        var metadata=resource.Slice(tail+4,186);const int mode=13,variant=2;
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(metadata[(mode*4)..]);
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(64+mode*2)..]);
        int auxiliary=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(96+mode*2)..]);
        int alternatives=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(162+variant*2)..]);
        int network=ordinary+21;
        int contextEnd=checked(network+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(network+4)..]));
        int profile=contextEnd+65;
        short gate=BinaryPrimitives.ReadInt16LittleEndian(resource[profile..]);
        int cap=gate==0?short.MaxValue:BinaryPrimitives.ReadInt16LittleEndian(resource[(profile+6)..])/3;
        if(arcs<1||alternatives<1||cap<1)throw new InvalidDataException("Positive selected main source caps required.");
        return new(flags,arcs,auxiliary,alternatives,metadata[128]>mode,cap);
    }

    /// <summary>201270 signed32 distance guard after reading the real parent
    ///state21. Arithmetic wraps exactly like SUB ECX. No parent identity is
    ///invented here; the controller must obtain its anchor from its owned row.</summary>
    public bool AllowsParent(int currentEndingNode, int actualAnchorEndingNode, byte actualParentState21)
        => unchecked((sbyte)actualParentState21) >= 0 &&
            (unchecked(currentEndingNode - actualAnchorEndingNode) <= MaximumParentDistance54 ||
             Configuration.SuppressPreviousContext2609 != 0);
}
