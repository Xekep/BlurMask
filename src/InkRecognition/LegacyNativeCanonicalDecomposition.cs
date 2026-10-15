namespace InkRecognition;

/// <summary>Default helper68.V60=048190 canonical-only decomposition and ordering,
/// before04AA10 composition. Input is the already-decoded scalar iterator.</summary>
public static class LegacyNativeCanonicalDecomposition
{
    public static int[] PrepareDecodedCodePoints(ReadOnlySpan<int> input,
        LegacyNativeUnicodeNormalizationData data,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(data);cancellationToken.ThrowIfCancellationRequested();
        if(input.Length>65536)throw new ArgumentOutOfRangeException(nameof(input));
        foreach(int cp in input)if((uint)cp>0x10ffff)throw new ArgumentException("Native bounded numeric code points required.");
        var output=new List<int>(input.Length);int boundary=0,visits=0;
        foreach(int cp in input)
        {
            Visit();
            if(data.ReadCombiningClass(cp)==0){boundary=output.Count;Append(cp,0);}
            else _=Insert(cp,boundary,0);
        }
        return output.ToArray();
        void Visit(){cancellationToken.ThrowIfCancellationRequested();if(++visits>1_048_576)throw new ArgumentOutOfRangeException(nameof(input),"Portable canonical decomposition work budget exceeded.");}
        void Guard(int depth){if(depth>32)throw new InvalidDataException("Canonical decomposition recursion bound exceeded.");}
        void AddGuard(){if(output.Count>=262144)throw new ArgumentOutOfRangeException(nameof(input),"Portable canonical output bound exceeded.");}
        void Append(int cp,int depth)
        {
            Visit();Guard(depth);
            if(data.TryReadDirectCanonicalDecomposition(cp,out var parts))foreach(int part in parts)Append(part,depth+1);
            else{AddGuard();output.Add(Encode(cp));}
        }
        int Insert(int cp,int hint,int depth)
        {
            Visit();Guard(depth);
            if(data.TryReadDirectCanonicalDecomposition(cp,out var parts))
            {foreach(int part in parts)hint=Insert(part,hint,depth+1);return hint;}
            int insertion=hint,previous=256,current=data.ReadCombiningClass(cp);
            for(int at=hint;at<output.Count;at++)
            {Visit();int existing=data.ReadCombiningClass(output[at]);if(existing<=current||existing==previous){insertion=at+1;previous=existing;}}
            AddGuard();output.Insert(insertion,Encode(cp));return insertion+1;
        }
    }
    private static int Encode(int cp)=>cp is >=0xd800 and <=0xdfff or 0xfffe or 0xffff?0x1a:cp;
}
