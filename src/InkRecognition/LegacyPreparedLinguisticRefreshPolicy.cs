namespace InkRecognition;

/// <summary>Verified built-in decorator selection before explicit resource286 extensions.</summary>
public sealed record LegacyLinguisticRefreshDecoratorPlan(ushort Delimiter,
    ReadOnlyMemory<ushort> OrderedWrapperClasses);

/// <summary>Prepared source descriptor summary from Text20d8f0/20c8c0.</summary>
public readonly record struct LegacyPreparedLinguisticSourceSummary(byte Flags,
    int LastKind800Source, int LastKindA00Source, bool ForceAlignmentAllowed);

/// <summary>
/// Bounded deterministic policy within source refresh; does not allocate native
/// decorators, call source.virtual10 or infer dirty/epoch/source descriptors.
/// </summary>
public static class LegacyPreparedLinguisticRefreshPolicy
{
    /// <summary>
    /// Text20c1c0 produces24b8 from container+C4, source kind137 or U+F0009;
    /// Text20d8f0 applies296 then28f around the existing class290/base chain.
    /// Delimiter is the supplied active-profile.virtual78 value.
    /// </summary>
    public static LegacyLinguisticRefreshDecoratorPlan PlanDecorators(int sourceKind,
        LegacyInkAlphabet acousticAlphabet, bool containerAlternateMode, ushort delimiter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acousticAlphabet);
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceKind is < 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(sourceKind));
        bool add296 = containerAlternateMode || sourceKind == 0x137 ||
            acousticAlphabet.TryGetLabel("\U000f0009", out _);
        ushort[] classes = containerAlternateMode ? [0x296, 0x28f] : add296 ? [0x296] : [];
        return new(delimiter, classes);
    }

    /// <summary>
    /// Summarize explicitly prepared source.virtual28 descriptor headers in
    /// enumeration order, including its implicit source at index0.20c8c0 clears
    /// aggregate bit3 when controller2300 is absent. No case/script is guessed.
    /// </summary>
    public static LegacyPreparedLinguisticSourceSummary Summarize(ReadOnlySpan<uint> sourceHeaders,
        bool forceAlignment, bool hasController2300, int previousKind800Source,
        int previousKindA00Source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceHeaders.Length is < 1 or > 257)
            throw new ArgumentException("Bounded prepared source descriptors including index0 required.");
        //20d8f0 clears aggregate flags but preserves indices when no new matching
        //source appears.20baa0 initializes fresh indices to-1; lifecycle is explicit.
        if (previousKind800Source is < -1 or > 256 || previousKindA00Source is < -1 or > 256)
            throw new ArgumentException("Explicit bounded previous source positions required.");
        byte flags = 0; int kind800 = previousKind800Source, kindA00 = previousKindA00Source;
        for (int i = 0; i < sourceHeaders.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            uint header = sourceHeaders[i], kind = header & 0x1e00;
            byte lower = (byte)((header >> 2) & 7);
            flags = (byte)((flags & ~7) | Math.Max(flags & 7, lower));
            flags |= (byte)((header >> 2) & 8);
            if ((header & 0x20) != 0 && kind != 0x800) flags |= 0x10;
            if (kind == 0xa00) { flags |= 0x20; kindA00 = i; }
            if (kind == 0x800) kind800 = i;
            flags |= (byte)((header >> 1) & 0xc0);
        }
        bool allowed = !forceAlignment || sourceHeaders.Length == 2 && !hasController2300;
        if (allowed && !hasController2300) flags &= 0xf7;
        return new(flags, kind800, kindA00, allowed);
    }

    /// <summary>
    /// Text20d8f0 input-method budget2602 after resource extensions. The supplied
    /// decorator-active flag is the post-extension2601 value, not inferred from
    /// the built-in wrapper plan. Flags/budgets are explicit live-container data.
    /// </summary>
    public static byte InputMethodBudget(bool containerAlternateMode, bool postExtensionDecoratorActive,
        byte containerBudget8D, byte normalLimit2603, byte alternateLimit2604)
    {
        if (!containerAlternateMode && !postExtensionDecoratorActive) return 0;
        return (byte)Math.Min(containerBudget8D / 50,
            containerAlternateMode ? alternateLimit2604 : normalLimit2603);
    }

    /// <summary>
    /// Forward the verified prepared descriptor policy into the restored bound
    /// mask consumer. Invalid FORCE_ALIGNMENT or explicit source preparation
    /// failure rejects the seed; a bypass does not invoke this controller.
    /// Native allocation/decorators/preparation still remain caller responsibilities.
    /// </summary>
    public static LegacyPreparedOrdinaryLinguisticMaskResult BuildPreparedMasks(
        LegacyPreparedOrdinaryLinguisticMaskBinding binding, ReadOnlySpan<uint> preparedSourceHeaders,
        bool forceAlignment, bool hasController2300, bool sourcePreparationSucceeded,
        bool languageControllerBypassed, IReadOnlyList<LegacySymbolMaskKnowledgeGroup> knowledgeGroups,
        ReadOnlySpan<byte> packedUserRestriction, ReadOnlySpan<LegacyContextualSymbolForms> contextualForms,
        ReadOnlySpan<LegacyAcousticCachePair> widePairs, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binding);
        // Only the count-based validation is consumed here; persisted descriptor
        //indices belong to the caller's separate refresh state.
        if (preparedSourceHeaders.Length is < 1 or > 257)
            throw new ArgumentException("Bounded prepared source descriptors including index0 required.");
        bool allowed = !forceAlignment || preparedSourceHeaders.Length == 2 && !hasController2300;
        return binding.Build(languageControllerBypassed, sourcePreparationSucceeded && allowed,
            knowledgeGroups, packedUserRestriction, contextualForms, widePairs, cancellationToken);
    }
}
