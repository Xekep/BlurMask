using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The owned first-source graph cap and ordinary sampler68 prune
/// pass. Seeds must already come from the real source/cut ordering producer;
/// source direction flags, token groups and full word scheduling are separate.</summary>
public sealed record LegacyFreshFirstCandidateGraphPreparation(
    LegacyOrdinaryGraphSourceRanges SourceRanges,
    LegacyOrdinarySamplerGroupPruningResult Pruning,
    int ResourceGraphOffset,
    LegacyCursiveLatticeSpanProfile SpanProfile);

public static class LegacyFreshFirstCandidateGraphPreparer
{
    public static LegacyFreshFirstCandidateGraphPreparation Prepare(ReadOnlySpan<byte> actualResource,
        LegacyFreshFirstResultOwnershipFactory factory, LegacySmoothedRowCutResult cuts,
        LegacyOrdinaryCursiveSeedResult actualSeeds, LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(cuts);
        ArgumentNullException.ThrowIfNull(actualSeeds);ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        int sourceOffset=factory.InvocationPolicy.Binding.SourceOffset;
        var model=LegacyFirstAcousticModel.ReadSource(actualResource,sourceOffset,cancellationToken);
        if(model.SourceFingerprint!=factory.SourceFingerprint||!ReferenceEquals(cuts.Owner,factory.Frame.Row))
            throw new ArgumentException("Actual selected resource, factory and cut owner required.");
        var geometry=LegacyFirstTransitionGeometryProfile.ReadSource(actualResource,sourceOffset,cancellationToken);
        int graphOffset=checked(geometry.SamplerOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(actualResource[(geometry.SamplerOffset+4)..]));
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(actualResource,graphOffset);
        if(span.MaximumAtoms>15||graphOffset+355!=model.OrdinaryNodeOffset)
            throw new NotSupportedException("Owned ordinary21b cap and first21d adjacency required.");
        var ranges=LegacyOrdinaryGraphSourceRanges.Prepare(actualSeeds,cuts.Chronology,
            factory.Frame.Row.OriginalSource,span.MaximumAtoms,cancellationToken:cancellationToken);
        var profile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(actualResource,geometry.SamplerOffset);
        var prune=LegacyOrdinarySamplerGroupPruning.Prepare(ranges.Graph,cuts.Cuts.PreparedInk,
            factory.Frame.Row.Frame.Line.Height,profile,table,cancellationToken);
        return new(ranges.WithSamplerPruning(prune,cancellationToken),prune,graphOffset,span);
    }
}
