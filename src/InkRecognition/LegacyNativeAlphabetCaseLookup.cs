using System.Text;
namespace InkRecognition;

public readonly record struct LegacyNativeAlphabetCaseLookupResult(bool InputHasCase,
    ushort Lower,ushort Upper,ushort StrippedUpper);

/// <summary>Successful checked04F170 lookup/optional recursive consistency over
/// an owned readable alphabet. This does not mutate stored relation maps or
/// synthesize missing labels. Native allocation/failure ABI is not emulated.</summary>
public sealed class LegacyNativeAlphabetCaseLookup
{
    public LegacyInkAlphabet Alphabet {get;}
    private readonly LegacyNativeCaseData cases;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly LegacyNativeTextPublicationData publication;
    public LegacyNativeAlphabetCaseLookup(LegacyInkAlphabet alphabet,LegacyNativeCaseData caseData,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeTextPublicationData publication)
    {ArgumentNullException.ThrowIfNull(alphabet);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(publication);Alphabet=alphabet;cases=caseData;this.normalization=normalization;this.publication=publication;}

    public LegacyNativeAlphabetCaseLookupResult Read(ushort label,uint requestFlags,int requestedLanguage,
        bool checkRecursiveConsistency,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(label>=Alphabet.LabelCount)throw new ArgumentOutOfRangeException(nameof(label));
        if(Alphabet.IsVirtual(label))return Empty();
        if(!Alphabet.TryGetText(label,out string text))throw new NotSupportedException("Native missing-label error is not a no-case result.");
        if(text.StartsWith('\ufeff'))text=text[1..]; //default charset5 initial BOM consumption
        int[] scalars=text.EnumerateRunes().Select(r=>r.Value).ToArray();
        if(scalars.Length>4096)throw new ArgumentOutOfRangeException(nameof(label),"Portable scalar context bound exceeded.");
        bool hasCase=false;
        foreach(int cp in scalars){cancellationToken.ThrowIfCancellationRequested();if(publication.ReadScript(cp)!=24&&cases.GetAlternativeCount(cp)!=0)hasCase=true;}
        if(!hasCase)return Empty(); //050400, independent of stored relation maps
        var streams=LegacyNativeMutableCaseStreams.Prepare(scalars,cases,normalization,requestFlags,requestedLanguage,cancellationToken);
        ushort lower=Lookup(streams.CopyLower()),upper=Lookup(streams.CopyUpper()),stripped=Lookup(streams.CopyStrippedUpper());
        if(checkRecursiveConsistency)
        {
            if(label==lower&&upper!=65535)
            {
                var back=Read(upper,requestFlags,requestedLanguage,false,cancellationToken);
                if(back.Lower!=lower){upper=65535;stripped=65535;}
            }
            else if(label==upper&&lower!=65535)
            {
                var back=Read(lower,requestFlags,requestedLanguage,false,cancellationToken);
                if(back.Upper!=upper)lower=65535;
            }
        }
        return new(true,lower,upper,stripped);
        ushort Lookup(int[] points)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string value=string.Concat(points.Select(cp=>new Rune(cp).ToString()));
            byte[] utf8=Encoding.UTF8.GetBytes(value);
            //04F170 accepts exactlyone tailoredcluster, including multiscalar
            //clusters, then passes that iterator slice toAL.V28 exactlookup.
            int scalarCount=points.Length;if(value.StartsWith('\ufeff')){value=value[1..];scalarCount--;}
            if(scalarCount==0||LegacyNativeTextGlyphPublication.ReadFirstClusterScalarCount(utf8,publication,normalization,cancellationToken)!=scalarCount)return 65535;
            return Alphabet.TryGetLabel(value,out ushort found)?found:ushort.MaxValue;
        }
        static LegacyNativeAlphabetCaseLookupResult Empty()=>new(false,65535,65535,65535);
    }
}
