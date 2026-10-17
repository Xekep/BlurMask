using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathCompletedProvenance.Probe")]

namespace InkRecognition.Math;

internal sealed partial class Math2Mode0PrefixGeneration
{
    // These arrays are private and immutable after the private constructor.
    // Append allocates a new generation; every external read remains detached.
    internal int CompletedParentLength(int parent)
    {
        if (parent < 0 || parent >= ActiveCount)
            throw new ArgumentException("Completed provenance requires an active parent of this exact generation.");
        if (primary[parent].LongLength != Offset || secondary[parent].LongLength != Offset)
            throw new InvalidDataException("Cold completed provenance must retain complete aligned old prefixes.");
        return primary[parent].Length;
    }
    internal ushort[] CopyCompletedPrimary(int parent)
    { CompletedParentLength(parent); return primary[parent].ToArray(); }
    internal ushort[] CopyCompletedSecondary(int parent)
    { CompletedParentLength(parent); return secondary[parent].ToArray(); }
    internal long CompletedAncestryElementCount => ancestry.Sum(row => row.LongLength);
    internal long CompletedPrefixElementCount => primary.Sum(row => row.LongLength) + secondary.Sum(row => row.LongLength);
}

internal sealed partial class Math2Mode0PrefixStep
{
    // Canonical active-Step identity is required, not matching shapes/offsets,
    // a detached CopyGeneration result, or an immutable foreign generation.
    internal Math2Mode0PrefixGeneration BindCompletedGeneration(Math2Mode0PrefixGeneration source, int parent)
        => WithActive(() =>
        {
            if (!ReferenceEquals(Generation, source))
                throw new ArgumentException("The exact immutable Step generation is required.");
            source.CompletedParentLength(parent);
            return source;
        });
}

internal sealed partial class Math2Mode0CompletedEventLease
{
    // Opaque observation for provenance tests/measurement, never mutable arrays.
    internal object CompletedGenerationIdentity => generation;
    internal int CompletedSourceParent => sourceParent;
    internal long SharedGenerationAncestryElements => generation.CompletedAncestryElementCount;
    internal long SharedGenerationPrefixElements => generation.CompletedPrefixElementCount;
    internal long AvoidedRetainedProvenanceDataBytes => checked(4 * generation.CompletedAncestryElementCount +
        4L * generation.CompletedParentLength(sourceParent));
}
