using System.Security.Cryptography;

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
        var actualSource = owner.Acoustic.Source;
        string fingerprint = Convert.ToHexString(SHA256.HashData(resource));
        if (!StringComparer.OrdinalIgnoreCase.Equals(fingerprint, actualSource.Owner.CategoryOwner.SourceFingerprint) ||
            actualSource.Owner.CategoryOwner.SelectedSourceOffset != 0)
            throw new ArgumentException("Scalar preparation requires this owner's actual selected main0 resource.");
        var selected = LegacyPreparedFreshMain240AcousticPolicy.Read(resource, actualSource.Owner);
        if (selected != owner.Acoustic.Policy)
            throw new ArgumentException("Selected source metadata must match the already prepared acoustic owner.");
        //1D60D0 preserves NEW00C860 mode13/variant2 and resolves28 from the
        //selected source's variant table.151BF0 writes54=120,58=that28.
        int distance = selected.SourceAlternativeLimit;
        var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource, 0);
        byte consecutive = resource[sourceProfile.ParametersOffset - 6];
        byte format = resource[sourceProfile.ParametersOffset - 5];
        if (consecutive > 1 || format > 1)
            throw new InvalidDataException("Validated247 B8/B9 flags required.");
        var model = owner.DictionaryOwner.Model;
        bool marker = model.AcousticAlphabet.TryGetLabel("\u202b", out _);
        var refresh = LegacyNativeWordScalarRefresh.Apply(0, owner.DictionaryOwner.SourceSummary.Flags,
            false, consecutive, 0, format, -1, 0, marker);
        var fresh = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
        { PackedSource22e8 = refresh.PackedSource22e8 };
        var prepared = LegacyNativeWordControllerLimitPreparation.Prepare(fresh,
            checked((uint)selected.MaximumSourceArcs), LegacyNativeWordKnowledgeLimits.CreateFresh00c860(), true);
        cancellationToken.ThrowIfCancellationRequested();
        return new(owner, prepared.Configuration, refresh, distance);
    }

    /// <summary>201270 signed32 distance guard after reading the real parent
    ///state21. Arithmetic wraps exactly like SUB ECX. No parent identity is
    ///invented here; the controller must obtain its anchor from its owned row.</summary>
    public bool AllowsParent(int currentEndingNode, int actualAnchorEndingNode, byte actualParentState21)
        => unchecked((sbyte)actualParentState21) >= 0 &&
            (unchecked(currentEndingNode - actualAnchorEndingNode) <= MaximumParentDistance54 ||
             Configuration.SuppressPreviousContext2609 != 0);
}
