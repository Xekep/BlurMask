using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace InkRecognition;

public readonly record struct LegacyNativeCaseVariant(ReadOnlyMemory<int> Lower, ReadOnlyMemory<int> Upper,
    ReadOnlyMemory<int> Title, int LanguageFlags, int ContextFlags);
public sealed record LegacyNativeCaseAlternatives(int TotalCount, IReadOnlyList<LegacyNativeCaseVariant> Written);

/// <summary>Engine05F4C0 scalar alternatives and null-output count. Context matching is a separate consumer.</summary>
public sealed class LegacyNativeCaseData
{
    public const string ExpectedTableSha256 = "5e99b64e4e7c2893a50e60552eb5c6a11894e466e18b8c4945071b5699916c04";
    private readonly int[][] alternating, lowerRanges, lowerSingles, upperRanges, upperSingles, titles, specials;
    private readonly int[] sequences;
    private LegacyNativeCaseData(int[][] a, int[][] lr, int[][] ls, int[][] ur, int[][] us, int[][] ti, int[][] sp, int[] seq)
    { alternating=a; lowerRanges=lr; lowerSingles=ls; upperRanges=ur; upperSingles=us; titles=ti; specials=sp; sequences=seq; }

    public static LegacyNativeCaseData Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var storage=new MemoryStream(); byte[] buffer=new byte[8192]; int read;
        while((read=stream.Read(buffer))!=0)
        { if(storage.Length+read>131072) throw new InvalidDataException("Case table budget exceeded."); storage.Write(buffer,0,read); }
        using var json=JsonDocument.Parse(storage.ToArray()); var root=json.RootElement;
        if(root.GetProperty("format").GetString()!="legacy-native-case-data-v1" ||
           root.GetProperty("engineSha256").GetString()!="d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c" ||
           root.GetProperty("tableSha256").GetString()!=ExpectedTableSha256)
            throw new InvalidDataException("Case table provenance differs.");
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var a=Read("alternatingRanges",58,2); var lr=Read("lowerRanges",34,3); var ls=Read("lowerSingles",89,2);
        var ur=Read("upperRanges",35,3); var us=Read("upperSingles",110,2);
        var ti=Read("titleSingles",8,2); var sp=Read("specialRows",130,9); var seq=Read("sequenceScalars",348,1);
        if(!Convert.ToHexString(hash.GetHashAndReset()).Equals(ExpectedTableSha256,StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Case numeric checksum differs.");
        return new(a,lr,ls,ur,us,ti,sp,seq.Select(row=>row[0]).ToArray());
        void Hash(int value) { Span<byte> b=stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(b,value); hash.AppendData(b); }
        int[][] Read(string name,int count,int width)
        {
            var values=root.GetProperty(name); if(values.GetArrayLength()!=count)throw new InvalidDataException("Case table count differs.");
            Hash(count); var result=new int[count][]; int i=0;
            foreach(var row in values.EnumerateArray())
            { if(row.GetArrayLength()!=width)throw new InvalidDataException("Case table width differs.");
              result[i]=row.EnumerateArray().Select(v=>v.GetInt32()).ToArray(); foreach(int v in result[i])Hash(v); i++; }
            return result;
        }
    }

    /// <summary>Exact alternative count with NULL output and capacity0; contextual variants are counted, not filtered.</summary>
    public int GetAlternativeCount(int codePoint)
    {
        if(codePoint<0 || codePoint>0x10ffff || codePoint is >=0xd800 and <=0xdfff)
            throw new ArgumentOutOfRangeException(nameof(codePoint));
        if(codePoint==0xb5)return 0;
        int count=0, index=LowerBound(specials,codePoint);
        while(index<specials.Length && specials[index++][0]==codePoint) count++;
        int lo=FindRange(alternating,codePoint), up=FindRange(alternating,codePoint-1);
        bool hasLower=lo>=0 ? ((codePoint-alternating[lo][0])&1)==0 :
            FindRange(lowerRanges,codePoint)>=0 || Contains(lowerSingles,codePoint);
        bool hasUpper=up>=0 ? ((codePoint-alternating[up][0])&1)==1 :
            FindRange(upperRanges,codePoint)>=0 || Contains(upperSingles,codePoint);
        return count+(hasLower||hasUpper?1:0);
    }

    /// <summary>Preserve special-row order, flags, and a final generic variant. Capacity truncates writes, not total count.</summary>
    public LegacyNativeCaseAlternatives ReadAlternatives(int codePoint, int capacity)
    {
        int total=GetAlternativeCount(codePoint);
        if(capacity is <0 or >64)throw new ArgumentOutOfRangeException(nameof(capacity));
        var output=new List<LegacyNativeCaseVariant>(Math.Min(total,capacity));
        if(codePoint==0xb5)return new(0,output.AsReadOnly());
        int index=LowerBound(specials,codePoint);
        while(index<specials.Length && specials[index][0]==codePoint)
        {
            var row=specials[index++];
            if(output.Count<capacity)output.Add(new(sequences.AsMemory(row[1],row[2]).ToArray(),
                sequences.AsMemory(row[5],row[6]).ToArray(), sequences.AsMemory(row[3],row[4]).ToArray(),row[7],row[8]));
        }
        int lower=Mapping(codePoint,false),upper=Mapping(codePoint,true);
        if((lower!=-1 || upper!=-1) && output.Count<capacity)
        {
            int title=LowerBound(titles,codePoint);
            int titleValue=title<titles.Length && titles[title][0]==codePoint ? codePoint+titles[title][1] : upper==-1?codePoint:upper;
            output.Add(new(new[]{lower==-1?codePoint:lower},new[]{upper==-1?codePoint:upper},new[]{titleValue},0,0));
        }
        return new(total,output.AsReadOnly());
    }
    private int Mapping(int cp,bool upper)
    {
        int a=FindRange(alternating,upper?cp-1:cp);
        if(a>=0)return ((cp-alternating[a][0])&1)==(upper?1:0)?cp+(upper?-1:1):-1;
        var ranges=upper?upperRanges:lowerRanges; int r=FindRange(ranges,cp);
        if(r>=0)return cp+ranges[r][2];
        var singles=upper?upperSingles:lowerSingles; int s=LowerBound(singles,cp);
        return s<singles.Length && singles[s][0]==cp ? cp+singles[s][1] : -1;
    }

    /// <summary>Engine04FFE0 fallback scan, only after the caller establishes absent stored relation maps.</summary>
    public bool HasAlphabetCaseAlternatives(LegacyInkAlphabet alphabet, LegacyNativeTextPublicationData publication,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(alphabet); ArgumentNullException.ThrowIfNull(publication);
        for(int label=0;label<alphabet.LabelCount;label++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(alphabet.IsVirtual((ushort)label))continue;
            if(!alphabet.TryGetText((ushort)label,out string text))
                throw new NotSupportedException("Native missing-label error is not a false case predicate.");
            foreach(var rune in text.EnumerateRunes())
                if(publication.ReadScript(rune.Value)!=24 && GetAlternativeCount(rune.Value)!=0)return true;
        }
        return false;
    }
    /// <summary>Engine052BD0: any stored relation map returns true, otherwise use04FFE0.</summary>
    public bool HasCaseAlternatives(LegacyInkAlphabet alphabet, LegacyNativeTextPublicationData publication,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(alphabet); ArgumentNullException.ThrowIfNull(publication);
        cancellationToken.ThrowIfCancellationRequested();
        return alphabet.HasStoredRelationMaps || HasAlphabetCaseAlternatives(alphabet,publication,cancellationToken);
    }

    /// <summary>Engine059A70 per-symbol predicate. Stored maps suppress Unicode fallback even when this symbol has no entry.</summary>
    public bool IsCasedSymbol(LegacyInkAlphabet alphabet,ushort label,LegacyNativeTextPublicationData publication)
    {
        ArgumentNullException.ThrowIfNull(alphabet);ArgumentNullException.ThrowIfNull(publication);
        if(label>=alphabet.LabelCount)throw new ArgumentOutOfRangeException(nameof(label));
        if(alphabet.HasStoredRelationMaps)
        {
            for(int mode=0;mode<3;mode++)if(alphabet.TryGetRelatedForms(label,mode,out _))return true;
            return false;
        }
        if(alphabet.IsVirtual(label))return false;
        if(!alphabet.TryGetText(label,out string text))throw new NotSupportedException("Native missing-label error cannot become a false predicate.");
        foreach(var rune in text.EnumerateRunes())
            if(publication.ReadScript(rune.Value)!=24 && GetAlternativeCount(rune.Value)!=0)return true;
        return false;
    }
    private static bool Contains(int[][] rows,int cp)
    { int i=LowerBound(rows,cp); return i<rows.Length && rows[i][0]==cp; }
    private static int LowerBound(int[][] rows,int cp)
    { int l=0,r=rows.Length; while(l<r){int m=(l+r)/2;if(rows[m][0]<cp)l=m+1;else r=m;}return l; }
    private static int FindRange(int[][] rows,int cp)
    { int l=0,r=rows.Length; while(l<r){int m=(l+r)/2;if(rows[m][0]<=cp){if(cp<=rows[m][1])return m;l=m+1;}else r=m;}return -1; }
}
