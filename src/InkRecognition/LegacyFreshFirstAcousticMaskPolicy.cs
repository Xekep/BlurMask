using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned fresh23F masks: no linguisticA8 callback, no external/reused
/// restriction and the first profile's inactive class-preselection branch.
/// Virtual stripping/header reconstruction still run. This is not a default
/// mask for an external20E, main240, or a user-configured frontend.</summary>
public sealed class LegacyFreshFirstAcousticMaskPolicy
{
    private readonly byte[] primary,secondary;
    private readonly LegacyAcousticCachePair[] widePairs;
    private readonly LegacyFreshFirstControllerPolicy controller;
    public LegacyFreshFirstResultOwnershipFactory Factory {get;}
    public int SymbolCount=>primary.Length;
    public int PrimaryEnabledCount=>primary.Count(b=>b!=0);
    public int SecondaryEnabledCount=>secondary.Count(b=>b!=0);
    public int WidePairCount=>widePairs.Length;
    public bool ClassPreselectionActive=>false;

    private LegacyFreshFirstAcousticMaskPolicy(LegacyFreshFirstResultOwnershipFactory factory,
        LegacyAcousticSymbolMasks masks,LegacyAcousticCachePair[] pairs,LegacyFreshFirstControllerPolicy controller)
    {Factory=factory;primary=masks.Primary.ToArray();secondary=masks.Secondary.ToArray();widePairs=pairs;this.controller=controller;}

    public static LegacyFreshFirstAcousticMaskPolicy Create(ReadOnlySpan<byte> resource,
        LegacyFreshFirstResultOwnershipFactory factory,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        int sourceOffset=factory.InvocationPolicy.Binding.SourceOffset;
        _=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(resource,sourceOffset,out var model,cancellationToken);
        if(model.SourceFingerprint!=factory.SourceFingerprint)
            throw new ArgumentException("Mask source must match the owned fresh first factory.");
        //Checks the exact first AL and no derived Arabic-form table. Stored
        //case triplets must never be substituted for alphabet.virtual70.
        _=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,sourceOffset,factory,cancellationToken);
        _=LegacyFreshFirstProtocolOwnership.Create(factory,cancellationToken);
        int p=checked(sourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        var pairs=LegacyWideSymbolPairs.CreateUnrestricted(factory.InvocationPolicy.Binding.Alphabet,cancellationToken);
        //No pair means unrestricted/restricted eligibility has the same exact
        //result; do not silently assign the unrestricted producer to a future
        //alphabet with actual wide substitutions.
        if(pairs.Length!=0)throw new NotSupportedException("Fresh first wide-pair eligibility is not yet owned for a nonempty wide alphabet.");
        var producer=new LegacyOrdinarySymbolMaskProducer(factory.InvocationPolicy.Binding.Alphabet,resource.Slice(p,16));
        //The checked cache tail has zero knowledge groups.23F.A8=NULL bypasses
        //their seed expansion in any case. Fresh frontend38/ordinary2B8=NULL;
        //v110 source-count bounds148/150 are also absent.
        var masks=producer.Build(LegacySymbolMaskControllerOutcome.UnboundOrBypassed,[],[],[],[],pairs,cancellationToken);
        return new(factory,masks,pairs,LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,sourceOffset,cancellationToken));
    }

    /// <summary>Caller receives copies, never the session's live mask arrays.</summary>
    public LegacyAcousticSymbolMasks CopyMasks(CancellationToken cancellationToken=default)
    {cancellationToken.ThrowIfCancellationRequested();return new(primary.ToArray(),secondary.ToArray());}

    internal byte[] CopyWorkingPrimary(CancellationToken cancellationToken)
    {cancellationToken.ThrowIfCancellationRequested();return primary.ToArray();}
    internal LegacyAcousticCachePair[] CopyPairs(CancellationToken cancellationToken)
    {cancellationToken.ThrowIfCancellationRequested();return widePairs.ToArray();}
    internal int MaximumNormalizedStrokes=>controller.MaximumNormalizedStrokes;
    internal LegacyOrdinaryAcousticCacheContext CreateCacheContext(LegacySourcePrimitiveScope primitive)
        //Fresh secondary16A130.148=0 makes218.D8 false: no F0/F8 source penalty.
        //Fresh containerF5=0 also disables the separate partial-scope branch.
        =>controller.CreateContext(primitive,[]);
}
