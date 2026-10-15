namespace InkRecognition;

/// <summary>Actual fresh292 result publication over its retained post-V80
/// dictionary session. It shares the exact compact writer with DIRECT without
/// replacing the case backend, owner, permission map or full source tags.</summary>
public static class LegacyOwnedMainCaseCompactWordMaterialization
{
    public static LegacyOwnedMainCompactWordMaterialization Materialize(ReadOnlySpan<byte> resource,
        LegacyFreshCaseMainSourcePolicy sourcePolicy, LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedRussianMainGlyphConfidence candidateOwner, ReadOnlySpan<byte> allocatorResidue40,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourcePolicy);
        return LegacyOwnedMainCompactWordMaterialization.MaterializeCase(resource, sourcePolicy,
            terminal, candidateOwner, allocatorResidue40, cancellationToken);
    }
}
