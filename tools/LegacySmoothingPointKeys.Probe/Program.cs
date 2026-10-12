using InkRecognition;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Usage: LegacySmoothingPointKeys.Probe controls.json");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0, keys = 0, fractional = 0, duplicates = 0;
foreach (var item in doc.RootElement.EnumerateArray())
{
    var strokes = Read(item.GetProperty("strokes")); float[] p = item.GetProperty("profile").EnumerateArray().Select(v => v.GetSingle()).ToArray();
    var profile = new LegacyOrientationSmoothingProfile(p[0],p[1],p[2],p[3],p[4],p[5],p[6],p[7]);
    var supplied = new LegacyOrientationStrokeMetrics(item.GetProperty("metrics")[0].GetSingle(),item.GetProperty("metrics")[1].GetSingle());
    var actual = LegacyOrientationStrokePreparation.PrepareWithPointKeys(strokes, profile, item.GetProperty("scale").GetSingle(), supplied);
    int[] expectedKeys = item.GetProperty("keys").EnumerateArray().Select(v => v.GetInt32()).ToArray();
    if (!actual.PointKeys200.Span.SequenceEqual(expectedKeys)) throw new Exception($"Native left/right key mismatch case{count}.");
    if (!actual.PhysicalPointCounts.Span.SequenceEqual(actual.Strokes.Select(s => s.Points.Count).ToArray())) throw new Exception("Output count ownership mismatch.");
    var expected = Read(item.GetProperty("prepared"));
    for (int s = 0; s < expected.Length; s++)
    {
        if (actual.Strokes[s].Points.Count != expected[s].Points.Count) throw new Exception("Coordinate/key count mismatch.");
        for (int i = 0; i < expected[s].Points.Count; i++)
        { Near(actual.Strokes[s].Points[i].X, expected[s].Points[i].X); Near(actual.Strokes[s].Points[i].Y, expected[s].Points[i].Y); }
    }
    //Independent original identities make ceil-map metadata and remainder observable.
    var original = LegacySourcePointChronology.CreateOrdinary(strokes.Select(s => s.Points.Count).ToArray());
    var records = new List<LegacySourcePointKey>();
    for (int s = 0; s < strokes.Length; s++) for (int i = 0; i < strokes[s].Points.Count; i++) records.Add(original.Resolve(s,i).Key);
    var mapped = LegacySourcePointChronology.CreatePrepared(actual.PhysicalPointCounts.Span, records.ToArray(), Enumerable.Range(0,records.Count).ToArray(), actual.PointKeys200.ToArray().Select(v => (uint)v).ToArray(),200);
    int start=0;
    for (int s=0; s<actual.Strokes.Count; s++)
    {
        for (int i=0; i<actual.Strokes[s].Points.Count; i++)
        {
            int tick=expectedKeys[start+i],index=(tick+199)/200,remainder=tick%200;var resolved=mapped.Resolve(s,i);
            if (resolved.Key!=records[index] || resolved.MetadataPointIndex!=index || resolved.RemainderByte!=(remainder==0?0:200-remainder)) throw new Exception("Source ceil metadata/remainder mismatch.");
            if (remainder!=0)fractional++;
        }
        start+=actual.Strokes[s].Points.Count;
    }
    duplicates+=strokes.Sum(s=>s.Points.Zip(s.Points.Skip(1)).Count(pair=>pair.First==pair.Second));
    keys+=actual.PointKeys200.Length;count++;
}
if (fractional<100 || duplicates<100) throw new Exception("Fractional/duplicate controls missing.");
Console.WriteLine($"PASS smoothing map {count} independent full preparations,{keys} keys,{fractional} fractional identities,{duplicates} duplicates.");
static InkStroke[] Read(JsonElement strokes)=>strokes.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
static void Near(float a,float b) { if(!float.IsFinite(a)||MathF.Abs(a-b)>5e-5f*MathF.Max(1,MathF.Abs(b)))throw new Exception($"Coordinate mismatch{a}!={b}."); }
