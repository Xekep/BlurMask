namespace InkRecognition;

/// <summary>Owned scalar outputs before04F170's mutable alphabet label lookup.
/// The third stream is normalized uppercase with CCC200..234 removed; it is
/// neither titlecase nor generic Unicode-mark stripping.</summary>
public sealed class LegacyNativeMutableCaseStreams
{
    private readonly int[] lower,upper,strippedUpper;
    public int[] CopyLower()=>lower.ToArray();
    public int[] CopyUpper()=>upper.ToArray();
    public int[] CopyStrippedUpper()=>strippedUpper.ToArray();
    private LegacyNativeMutableCaseStreams(int[] lower,int[] upper,int[] stripped)
    {this.lower=lower;this.upper=upper;strippedUpper=stripped;}

    /// <summary>04F170 scalar-choice→lower/upper writers→V68 canonical composition;
    /// RAW uppercase writer→V60 decomposition→CCC filter→V68 (not NFC output
    /// fed back intoV60). Flags/language are literal
    /// caller configuration. Insertion/lookup/recursive form consistency remain
    /// separate mutable-alphabet operations.</summary>
    public static LegacyNativeMutableCaseStreams Prepare(ReadOnlySpan<int> scalars,
        LegacyNativeCaseData caseData,LegacyNativeUnicodeNormalizationData normalization,
        uint requestFlags,int requestedLanguage,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(normalization);
        var context=LegacyNativeCaseContext.Prepare(scalars,normalization,cancellationToken);
        var lo=new List<int>(scalars.Length);var up=new List<int>(scalars.Length);
        for(int at=0;at<scalars.Length;at++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var selected=context.Select(caseData,at,requestFlags,requestedLanguage);
            if(selected is{} row){Append(lo,row.Lower.Span);Append(up,row.Upper.Span);}
            else{lo.Add(scalars[at]);up.Add(scalars[at]);}
        }
        int[] lower=LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(lo.ToArray(),normalization,cancellationToken);
        int[] upper=LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(up.ToArray(),normalization,cancellationToken);
        int[] decomposed=LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints(up.ToArray(),normalization,cancellationToken);
        var stripped=new List<int>(decomposed.Length);
        foreach(int cp in decomposed)
        {cancellationToken.ThrowIfCancellationRequested();int ccc=normalization.ReadCombiningClass(cp);if(ccc is <200 or >234)stripped.Add(cp);}
        return new(lower,upper,LegacyNativeCanonicalScratch.PrepareDecodedCodePoints(stripped.ToArray(),normalization,cancellationToken));
        void Append(List<int> destination,ReadOnlySpan<int> values)
        {
            if(destination.Count+values.Length>65536)throw new ArgumentOutOfRangeException(nameof(scalars),"Portable case writer scalar bound exceeded.");
            foreach(int value in values)destination.Add(value);
        }
    }
}
