namespace InkRecognition;

/// <summary>Engine058200 post-load cased ranges, including native sentinel/one-past-end behavior.</summary>
public sealed class LegacyNativeCaseAlphabetRanges
{
    public LegacyInkAlphabet Alphabet {get;}
    public ushort FirstStart {get;}
    public ushort FirstEnd {get;}
    public ushort SecondStart {get;}
    public ushort SecondEnd {get;}
    public ushort LastCasedBoundary=>SecondEnd==ushort.MaxValue?FirstEnd:SecondEnd;
    private LegacyNativeCaseAlphabetRanges(LegacyInkAlphabet a,ushort s,ushort e,ushort s2,ushort e2)
    {Alphabet=a;FirstStart=s;FirstEnd=e;SecondStart=s2;SecondEnd=e2;}

    public static LegacyNativeCaseAlphabetRanges Prepare(LegacyInkAlphabet alphabet,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(alphabet);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        cancellationToken.ThrowIfCancellationRequested();
        ushort first=65535,end=65535,second=65535,end2=65535;
        if(!alphabet.IsCaseRangeMetadataEnabled)return new(alphabet,first,end,second,end2);
        int count=alphabet.LabelCount,i=0;
        while(i<count && !Cased(i))i++;
        if(i<count)
        {
            first=(ushort)i;
            while(i<count && Cased(i))i++;
            //Native only writes FirstEnd when a noncased symbol closes the run.
            if(i<count)
            {
                end=(ushort)(i-1);
                while(i<count && !Cased(i))i++;
                if(i<count)
                {
                    second=(ushort)i;
                    while(i<count && Cased(i))i++;
                    end2=i<count?(ushort)(i-1):(ushort)i;
                }
            }
        }
        while(i<count)
        {
            if(Cased(i))throw new InvalidDataException("Native058200 refuses a third cased-symbol range.");
            i++;
        }
        return new(alphabet,first,end,second,end2);
        bool Cased(int label){cancellationToken.ThrowIfCancellationRequested();return caseData.IsCasedSymbol(alphabet,(ushort)label,publication);}
    }
}
