namespace InkRecognition;

/// <summary>Private same-English-source copies, not a language query endpoint.</summary>
internal sealed class LegacyEnglishCaseInitializationNumericTables
{
    private readonly Dictionary<string,byte[]> codes;
    private readonly int[] word;
    private readonly int[][] classes,backoff;
    internal LegacyEnglishLkFreshNumericBinding Numeric{get;}
    private LegacyEnglishCaseInitializationNumericTables(LegacyEnglishLkFreshNumericBinding n,Dictionary<string,byte[]> c,int[] w,int[][] cl,int[][] b)
    {Numeric=n;codes=c;word=w;classes=cl;backoff=b;}
    private static readonly (string Name,int Count)[] expected=[("98.wordCodes",260075),("C0.0",1024),("C0.1",113643),("C0.2",155484),("C8.0",1024),("C8.1",113643)];
    internal static void Preflight(LegacyEnglishLkFreshNumericBinding numeric)
    {
        var l=numeric.LanguageOwner;if(l.Order!=3||l.ClassSlots!=1024||l.WordSlots!=260075||l.ReservedWordIndex!=260074||numeric.Profile!=2||numeric.SourceScale!=1||numeric.LiteralPacketByte0!=0)
            throw new NotSupportedException("Actual fresh English sparse numeric owner required.");
        var fields=l.CopyFields();foreach(var e in expected){var field=fields.Single(f=>f.Name==e.Name);if(field.End-field.Offset!=e.Count)throw new InvalidDataException("Complete exact code payload bounds required before copying.");}
    }
    internal static LegacyEnglishCaseInitializationNumericTables Prepare(LegacyEnglishLkFreshNumericBinding n,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Preflight(n);var c=new Dictionary<string,byte[]>(6,StringComparer.Ordinal);
        foreach(var e in expected){token.ThrowIfCancellationRequested();var raw=n.LanguageOwner.CopyFieldPayload(e.Name);if(raw.Length!=e.Count)throw new InvalidDataException("Code payload changed.");c.Add(e.Name,raw);}
        var w=n.CopyWordCosts();var cl=new int[3][];var b=new int[2][];
        for(int i=0;i<3;i++){token.ThrowIfCancellationRequested();cl[i]=n.CopyClassCosts(i);if(cl[i].Length!=256)throw new InvalidDataException("Exact class table required.");if(i<2){b[i]=n.CopyBackoffCosts(i);if(b[i].Length!=256)throw new InvalidDataException("Exact backoff table required.");}}
        if(w.Length!=256)throw new InvalidDataException("Exact word cost table required.");token.ThrowIfCancellationRequested();return new(n,c,w,cl,b);
    }
    //Diagnostic copies are never passed to an evaluator and are not public API.
    internal byte[] CopyCodesForAudit(string name)=>codes[name].ToArray();
    internal int[] CopyCostsForAudit(int kind)=>kind switch{0=>word.ToArray(),1 or 2 or 3=>classes[kind-1].ToArray(),4 or 5=>backoff[kind-4].ToArray(),_=>throw new ArgumentOutOfRangeException(nameof(kind))};
}
