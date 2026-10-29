using System.Text;

namespace InkRecognition;

/// <summary>Owned non-Arabic stored-alphabet tables from wrapper292 preparation2324E0.
/// This prepares symbol classifications, not the stateful wrapper query.</summary>
public sealed class LegacyNativeCasePreparation
{
    private readonly byte[] _ordinary, _alternate;
    private readonly LegacyInkRelatedForms?[] _ordinaryForms, _alternateForms;
    public LegacyNativeCaseAlphabetRanges Ranges { get; }
    public LegacyInkAlphabet Alphabet => Ranges.Alphabet;
    public ushort Hyphen { get; }
    public ushort Apostrophe { get; }
    public ushort SmallI { get; }
    public ushort SmallJ { get; }

    private LegacyNativeCasePreparation(LegacyNativeCaseAlphabetRanges ranges,
        byte[] ordinary, byte[] alternate, LegacyInkRelatedForms?[] forms, LegacyInkRelatedForms?[] alternateForms)
    {
        Ranges=ranges; _ordinary=ordinary; _alternate=alternate;
        _ordinaryForms=forms; _alternateForms=alternateForms;
        Hyphen=Label("-"); Apostrophe=Label("'"); SmallI=Label("i"); SmallJ=Label("j");
    }

    public byte ReadClassification(ushort symbol, bool alternate=false) =>
        (alternate?_alternate:_ordinary)[Index(symbol)];

    public bool TryReadForms(ushort symbol, bool alternate, out LegacyInkRelatedForms forms)
    {
        Index(symbol);
        var row=symbol<Alphabet.LabelCount?(alternate?_alternateForms:_ordinaryForms)[symbol]:null;
        forms=row ?? new(65535,65535,65535); return row.HasValue;
    }

    public static LegacyNativeCasePreparation Prepare(LegacyNativeCaseAlphabetRanges ranges,
        LegacyNativeTextPublicationData publication, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ranges); ArgumentNullException.ThrowIfNull(publication);
        cancellationToken.ThrowIfCancellationRequested(); var alphabet=ranges.Alphabet;
        //0573B0 can allocate its Arabic map only after04E280 finds a script5
        //string. Prove the map absent instead of supplying a caller's Arabic bit.
        for(ushort label=0;label<alphabet.LabelCount;label++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(alphabet.TryGetText(label,out var text))
                foreach(var rune in text.EnumerateRunes())
                    if(publication.ReadScript(rune.Value)==5)
                        throw new NotSupportedException("Arabic map production is outside this owned preparation scope.");
        }
        int count=alphabet.LabelCount; var first=new byte[count+29]; var second=new byte[count+29];
        var forms=new LegacyInkRelatedForms?[count]; var altForms=new LegacyInkRelatedForms?[count];
        ushort hyphen=Find("-");
        for(int index=0;index<first.Length;index++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort label=unchecked((ushort)(index-29));
            if(label<count && label<=ranges.LastCasedBoundary && alphabet.TryGetRelatedForms(label,0,out var zero))
            {
                first[index]=Classify(label,zero);
                if(!alphabet.TryGetRelatedForms(label,1,out var one) || !alphabet.TryGetRelatedForms(label,2,out var two))
                    throw new InvalidDataException("Native case preparation requires all fallback form modes.");
                if(one.First==65535) second[index]=0;
                else if(label==one.First)
                {
                    if(one.Second==65535 && one.Third!=65535)
                    { one=new(one.First,one.Third,65535); second[index]=1; }
                    else second[index]=Classify(label,one);
                }
                else second[index]=(byte)(label==one.Third || zero.Third==65535?3:4);
                bool useTwo=false;
                if(first[index]==0)
                {
                    if(two.First==65535) second[index]=0; //Native writes SECOND here.
                    else
                    {
                        first[index]=Classify(label,two);
                        useTwo=first[index]!=0;
                    }
                }
                forms[label]=useTwo?two:zero; altForms[label]=one;
            }
            else if(label==65532 || hyphen!=65535 && label==hyphen) first[index]=second[index]=6;
            else if(unchecked((ushort)(label+9))<5) first[index]=second[index]=5;
            else if(unchecked((ushort)(label+29))<8) first[index]=second[index]=7;
        }
        foreach(string text in new[]{",",":",";"}) Override(text,8);
        foreach(string text in new[]{".","!","?"}) Override(text,9);
        return new(ranges,first,second,forms,altForms);
        ushort Find(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        void Override(string text,byte value){ushort label=Find(text);if(label!=65535)first[label+29]=value;}
    }

    internal static LegacyNativeCasePreparation PrepareOuterWide(LegacyNativeCaseAlphabetRanges ranges,
        LegacyNativeTextPublicationData publication,LegacyEnglishCaseExecutionPoolReceipt pool,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ranges); ArgumentNullException.ThrowIfNull(publication);
        cancellationToken.ThrowIfCancellationRequested(); var alphabet=ranges.Alphabet;
        //0573B0 can allocate its Arabic map only after04E280 finds a script5
        //string. Prove the map absent instead of supplying a caller's Arabic bit.
        for(ushort label=0;label<alphabet.LabelCount;label++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(alphabet.TryGetText(label,out var text))
                foreach(var rune in text.EnumerateRunes())
                    if(publication.ReadScript(rune.Value)==5)
                        throw new NotSupportedException("Arabic map production is outside this owned preparation scope.");
            // Original script reads finish in original order BEFORE this added fixed-label predicate.
            // Missing labels are observed too; only the private active pool may publish a fact.
            pool.ObserveOuterWideText(label,text,cancellationToken);
        }
        int count=alphabet.LabelCount; var first=new byte[count+29]; var second=new byte[count+29];
        var forms=new LegacyInkRelatedForms?[count]; var altForms=new LegacyInkRelatedForms?[count];
        ushort hyphen=Find("-");
        for(int index=0;index<first.Length;index++)
        {
            cancellationToken.ThrowIfCancellationRequested(); ushort label=unchecked((ushort)(index-29));
            if(label<count && label<=ranges.LastCasedBoundary && alphabet.TryGetRelatedForms(label,0,out var zero))
            {
                first[index]=Classify(label,zero);
                if(!alphabet.TryGetRelatedForms(label,1,out var one) || !alphabet.TryGetRelatedForms(label,2,out var two))
                    throw new InvalidDataException("Native case preparation requires all fallback form modes.");
                if(one.First==65535) second[index]=0;
                else if(label==one.First)
                {
                    if(one.Second==65535 && one.Third!=65535)
                    { one=new(one.First,one.Third,65535); second[index]=1; }
                    else second[index]=Classify(label,one);
                }
                else second[index]=(byte)(label==one.Third || zero.Third==65535?3:4);
                bool useTwo=false;
                if(first[index]==0)
                {
                    if(two.First==65535) second[index]=0; //Native writes SECOND here.
                    else
                    {
                        first[index]=Classify(label,two);
                        useTwo=first[index]!=0;
                    }
                }
                forms[label]=useTwo?two:zero; altForms[label]=one;
            }
            else if(label==65532 || hyphen!=65535 && label==hyphen) first[index]=second[index]=6;
            else if(unchecked((ushort)(label+9))<5) first[index]=second[index]=5;
            else if(unchecked((ushort)(label+29))<8) first[index]=second[index]=7;
        }
        foreach(string text in new[]{",",":",";"}) Override(text,8);
        foreach(string text in new[]{".","!","?"}) Override(text,9);
        return new(ranges,first,second,forms,altForms);
        ushort Find(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        void Override(string text,byte value){ushort label=Find(text);if(label!=65535)first[label+29]=value;}
    }

    private static byte Classify(ushort label,LegacyInkRelatedForms forms)
    {
        if(forms.First==65535)return 0;
        if(label==forms.First)
            return forms.Second==65535 || forms.Third==65535?(byte)0:(byte)(forms.Second==forms.Third?1:2);
        return label==forms.Third || forms.Third==65535?(byte)3:(byte)4;
    }
    private ushort Label(string text)=>Alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
    private int Index(ushort label)
    {
        int index=label>=65507?label-65536+29:label+29;
        if(index<0 || index>=_ordinary.Length)throw new ArgumentOutOfRangeException(nameof(label));
        return index;
    }
}
