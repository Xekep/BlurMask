namespace InkRecognition;

/// <summary>Category and metadata supplied to ordinary word-cost evaluation204290.</summary>
public readonly record struct LegacyOrdinaryWordCategory(
    int CompositeCategory, int WordCategory, byte MetadataFlags,
    bool UsedCandidateStartMap, bool ForcedCategoryOne);

/// <summary>
/// Checked ordinary text-beam category selection. Caller-resolved native metadata
/// is explicit; this component does not infer categories from neural output slots.
/// </summary>
public sealed class LegacyOrdinaryWordCategoryPolicy
{
    private readonly LegacyCursiveCategoryMapper mapper;

    public LegacyOrdinaryWordCategoryPolicy(LegacyCursiveCategoryMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        this.mapper=mapper;
    }

    /// <summary>
    /// Native204ace..204b36: a nonzero candidate+5e and label below the
    /// frontend-supplied limit select data+50; otherwise use beam+1a low six bits.
    /// Runtime24f4/local307 can override the mapped first ID with one.
    /// </summary>
    public LegacyOrdinaryWordCategory Resolve(byte beamRecordCategory,
        ushort candidateSymbolEntry, byte candidateIncrement, ushort symbolEntryLimit,
        byte candidateMetadata, ushort specialCategoryOneSymbol,
        bool resolvedForceCategoryOne, CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool useStartMap=candidateIncrement!=0 && candidateSymbolEntry<symbolEntryLimit;
        int composite=useStartMap ? mapper.GetSymbolCompositeCategory(candidateSymbolEntry):
            beamRecordCategory&0x3f;
        // Native dereferences the composite before the category-one override.
        // Reject missing/invalid maps rather than masking an unsupported branch.
        if(composite<0)throw new InvalidDataException("Selected symbol has no native start-category mapping.");
        int firstId=mapper.GetFirstId(composite);
        bool forced=candidateSymbolEntry==specialCategoryOneSymbol || resolvedForceCategoryOne;
        return new(composite,forced ? 1:firstId,(byte)(candidateMetadata&7),useStartMap,forced);
    }

    /// <summary>
    /// Ordinary204290 predecessor correction: after resolving the previous group
    /// and physical-owner difference, semantic category two/four costs8192 at
    /// the beginning or at an ownership transition. Native22f6 remains explicit.
    /// </summary>
    public static int GetPreviousCategoryPathPenalty(bool nativeMode22f6,
        uint knowledgeFlags, int previousWordCategory, int previousGroupStartNode,
        int physicalOwnerDifference)
    {
        if(previousGroupStartNode<0)throw new ArgumentOutOfRangeException(nameof(previousGroupStartNode));
        return !nativeMode22f6 && (knowledgeFlags&0x1c)>7 &&
            previousWordCategory is 2 or 4 &&
            (previousGroupStartNode==0 || physicalOwnerDifference!=0) ? 8192:0;
    }
}
