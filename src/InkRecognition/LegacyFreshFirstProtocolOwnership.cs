namespace InkRecognition;

/// <summary>Fresh249 without external/reused/serialized driver state creates
/// a new20E (configured source count0), a23F controller and its2B3 container.
///00CAB0 binds container60 to that controller and controller18 to the container;
///1EF070's protocol194 identity comparison is therefore false. This does not
/// describe externally configured20E, main240 or reused controller ownership.</summary>
public sealed class LegacyFreshFirstProtocolOwnership
{
    public LegacyFreshFirstResultOwnershipFactory Factory {get;}
    public int ConfiguredLinguisticSourceCount=>0;
    public int ControllerKind=>0x23f;
    public bool NativeProtocolExclusion194=>false;

    private LegacyFreshFirstProtocolOwnership(LegacyFreshFirstResultOwnershipFactory factory)=>Factory=factory;

    public static LegacyFreshFirstProtocolOwnership Create(LegacyFreshFirstResultOwnershipFactory factory,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        factory.ValidateInvocation(factory.AcousticModel,factory.Frame,factory.InvocationPolicy,cancellationToken);
        return new(factory);
    }

    public void ValidateFactory(LegacyFreshFirstResultOwnershipFactory factory,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(factory,Factory))throw new ArgumentException("Protocol ownership belongs to one fresh controller/container invocation.",nameof(factory));
    }

    /// <summary>Execute the real owned/pruned regular+blank producer with its
    /// proven fresh controller identity, rather than a caller protocol switch.
    /// All graph, frame, cuts, NN/cache and cancellation checks remain in the
    /// existing owning producer. Its context passes are separate consumers.</summary>
    public LegacyFreshFirstBlankPrimary PreparePrimary(ReadOnlySpan<byte> resource,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,
        IReadOnlyList<LegacyFirstOrdinaryCacheScores> scores,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return LegacyFreshFirstBlankPrimary.Prepare(resource,Factory,cuts,prepared,scores,table,
            NativeProtocolExclusion194,cancellationToken);
    }
}
