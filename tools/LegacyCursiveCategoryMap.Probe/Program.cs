using InkRecognition;
using System.Text.Json;

int controls=0,missing=0;
if(args.Length>0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var profile=LegacyCursiveCategoryProfile.Load(Convert.FromBase64String(test.GetProperty("bytes").GetString()!),test.GetProperty("offset").GetInt32(),out _);
        var mapper=new LegacyCursiveCategoryMapper(profile);
        var expected=test.GetProperty("categories").EnumerateArray().Select(c=>c[0].GetInt32()).ToArray();
        Require(mapper.CompositeFirstIds.Span.SequenceEqual(expected),"Independent derived first-ID table differs.");
        for(int i=0;i<expected.Length;i++)Require(mapper.GetFirstId(i)==expected[i],"Composite ID lookup differs.");
        var maps=test.GetProperty("maps").EnumerateArray().Select(m=>m.EnumerateArray().Select(v=>v.GetInt32()).ToArray()).ToArray();
        for(int m=0;m<4;m++)for(int i=0;i<maps[m].Length;i++)
        {
            int category=mapper.GetSymbolCompositeCategory(i,(m&1)!=0,m>=2);
            Require(category==maps[m][i],"Symbol composite lookup differs.");
            Require(mapper.GetSymbolFirstId(i,(m&1)!=0,m>=2)==(category<0 ? -1:expected[category]),"Symbol first-ID lookup differs.");
            if(category<0)missing++;
        }
        Bounds(mapper);controls++;
    }
}
if(args.Length>1)
{
    int[] expected=[0,0,0,0,0,0,0,0,0,2,2,4,0,3,1,2,4,0,4,0,0,4,1,3,0,0,0,4,3,0,3,0,0,6,5];
    var data=File.ReadAllBytes(args[1]);
    foreach(int offset in new[]{11571,188222})
    {
        var mapper=new LegacyCursiveCategoryMapper(LegacyCursiveCategoryProfile.Load(data.AsSpan(offset),offset,out _));
        Require(mapper.CompositeCategoryCount==35 && mapper.SymbolEntryCount==195 && mapper.CompositeFirstIds.Span.SequenceEqual(expected),"Actual Russian35-composite first-ID table differs.");
        for(int i=0;i<35;i++)foreach(int flags in new[]{0,64,128,192})Require(mapper.GetBeamFirstId((byte)(flags|i))==expected[i],"Beam low-six-bit category mapping differs.");
        Reject(()=>mapper.GetBeamFirstId(63));Bounds(mapper);
    }
}
Console.WriteLine($"PASS sampler297 derived category mapper: {controls} independent profiles, {missing} missing mappings preserved, both actual Russian35-category tables, explicit beam masking, symbol boundary maps and bounded indices. Raw neural slot IDs and lexical history categories remain separate.");
static void Bounds(LegacyCursiveCategoryMapper m)
{
    Reject(()=>m.GetFirstId(-1));Reject(()=>m.GetFirstId(m.CompositeCategoryCount));
    Reject(()=>m.GetSymbolCompositeCategory(-1));Reject(()=>m.GetSymbolFirstId(m.SymbolEntryCount));
}
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Out-of-bounds category accepted.");}catch(ArgumentOutOfRangeException){}}
