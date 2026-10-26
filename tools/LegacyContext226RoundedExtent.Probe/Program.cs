using System.Text.Json;
using InkRecognition;

if (args.Length != 2 || args[1] is not ("before" or "after"))
    throw new ArgumentException("Expected fake-inputs.json before|after.");
if (!BitConverter.IsLittleEndian) throw new PlatformNotSupportedException("Pinned little-endian float fixture.");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases=0,changed=0;
foreach(var c in input.RootElement.GetProperty("Cases").EnumerateArray())
{
    string name=c.GetProperty("Name").GetString()!;
    var strokes=c.GetProperty("Points").EnumerateArray().Select(row=>new InkStroke(row.EnumerateArray()
        .Select(p=>new InkPoint(Read(p[0]),Read(p[1]))).ToArray())).ToArray();
    var actual=LegacyFloatContextGeometry.Extract(strokes,Read(c.GetProperty("Height")),
        Read(c.GetProperty("XReference")),Read(c.GetProperty("YReference")),
        c.GetProperty("MaximumX").GetBoolean(),c.GetProperty("PointMeanY").GetBoolean());
    var shorts=LegacyFloatInputQuantization.Quantize(actual,10);
    var expected=c.GetProperty(args[1]=="after"?"ExpectedAfter":"ExpectedBefore");
    var expectedShort=c.GetProperty(args[1]=="after"?"ShortAfter":"ShortBefore");
    if(actual.Length!=30||shorts.Length!=30||expected.GetArrayLength()!=30||expectedShort.GetArrayLength()!=30)
        throw new InvalidDataException($"{name}: exact30 required.");
    for(int i=0;i<30;i++)
    {
        string hex=Convert.ToHexString(BitConverter.GetBytes(actual[i])).ToLowerInvariant();
        if(hex!=expected[i].GetString()||shorts[i]!=expectedShort[i].GetInt16())
            throw new InvalidDataException($"{name}: first mismatch lane{i}: {hex}/{shorts[i]}.");
    }
    if(Enumerable.Range(0,30).Any(i=>c.GetProperty("ExpectedBefore")[i].GetString()!=c.GetProperty("ExpectedAfter")[i].GetString()))changed++;
    cases++;
}
if(cases!=25||changed==0)throw new InvalidDataException("Exact genuine25 artificial cases with changed lanes required.");
Console.WriteLine(JsonSerializer.Serialize(new {Mode=args[1],Cases=cases,FloatLanes=cases*30,ShortLanes=cases*30,ChangedCases=changed,NeuralCalls=0}));
static float Read(JsonElement v)
{
    byte[] b=Convert.FromHexString(v.GetString()!);
    if(b.Length!=4)throw new InvalidDataException("Exact binary32 bytes required.");
    float f=BitConverter.ToSingle(b);
    if(!float.IsFinite(f))throw new InvalidDataException("Finite artificial input required.");
    return f;
}
