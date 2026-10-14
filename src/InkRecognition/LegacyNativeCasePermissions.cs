namespace InkRecognition;

/// <summary>Wrapper292 stored-triplet permission phase234840, before the inner source virtual68.</summary>
public sealed class LegacyNativeCasePermissions
{
    private readonly LegacyInkRelatedForms[] _triplets;
    public LegacyInkAlphabet Alphabet { get; }
    public LegacyWordLanguageModel LanguageModel { get; }
    public LegacyNativeCasePermissions(LegacyInkAlphabet alphabet, LegacyWordLanguageModel languageModel)
    {
        ArgumentNullException.ThrowIfNull(alphabet);ArgumentNullException.ThrowIfNull(languageModel);
        if(languageModel.CasePathVersion is not (0 or 0x200))
            throw new NotSupportedException("Native232090 supports only TCM0/200 case-path tables.");
        Alphabet=alphabet;LanguageModel=languageModel;_triplets=alphabet.CopyStoredCaseTriplets();
    }
    /// <summary>Copy exact byte permissions and apply ordered, in-place native triplet OR writes.
    /// Values are byte fields, not booleans. The inner dictionary phase remains separate.</summary>
    public byte[] ExpandStoredPermissions(ReadOnlySpan<byte> permissions, CancellationToken cancellationToken=default)
    {
        if(permissions.Length!=Alphabet.LabelCount)throw new ArgumentException("Permission extent must match owned alphabet.");
        cancellationToken.ThrowIfCancellationRequested();byte[] result=permissions.ToArray();
        foreach(var row in _triplets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ushort first=row.First,second=row.Second,third=row.Third;
            if(first!=65535)
            {
                if(second!=65535)result[first]|=result[second];
                if(third!=65535)result[first]|=result[third];
            }
            if(second!=65535 && third!=65535)result[second]|=result[third];
            if(LanguageModel.CasePathVersion==0x200 && first!=65535 && second!=65535)
                result[second]|=result[first];
        }
        return result;
    }
}
