namespace InkRecognition;

/// <summary>Wrapper292 symbol callback232C80. Expansion order and duplicates are native observable behavior.</summary>
public sealed class LegacyNativeCaseSymbolEnumeration
{
    public LegacyNativeCaseAlphabetRanges Ranges {get;}
    public LegacyInkAlphabet Alphabet=>Ranges.Alphabet;
    public LegacyNativeCaseSymbolEnumeration(LegacyNativeCaseAlphabetRanges ranges)
    {ArgumentNullException.ThrowIfNull(ranges);Ranges=ranges;}

    /// <summary>Expand one incoming source label. A false sink result immediately terminates the native enumeration.</summary>
    public bool Enumerate(ushort sourceLabel,Func<ushort,bool> sink,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(sink);cancellationToken.ThrowIfCancellationRequested();
        if(sourceLabel>=Alphabet.LabelCount || !Alphabet.TryGetRelatedForms(sourceLabel,0,out var first))
            return Send(sourceLabel);
        if(!Forms(first))return false;
        if(Alphabet.TryGetRelatedForms(sourceLabel,1,out var second) && !Forms(second))return false;
        if(Alphabet.TryGetRelatedForms(sourceLabel,2,out var third) && !Forms(third))return false;
        return true;
        bool Send(ushort label){cancellationToken.ThrowIfCancellationRequested();return sink(label);}
        bool Forms(LegacyInkRelatedForms forms)
        {
            return (forms.First==ushort.MaxValue || Send(forms.First)) &&
                (forms.Second==ushort.MaxValue || Send(forms.Second)) &&
                (forms.Third==ushort.MaxValue || Send(forms.Third));
        }
    }
}
