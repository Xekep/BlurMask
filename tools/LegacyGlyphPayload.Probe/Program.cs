using InkRecognition;
using System.Text.Json;
if (args.Length != 1) throw new ArgumentException("Supply glyph payload controls.");
if (new FileInfo(args[0]).Length > 4 * 1024 * 1024) throw new ArgumentException("Controls exceed 4 MiB.");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var payload = new LegacyGlyphGeometryPayload(); int count = 0;
foreach (var c in doc.RootElement.EnumerateArray())
{
    var g = c.GetProperty("geometry");
    var geometry = new LegacyGlyphAffineGeometry(g[0].GetSingle(),g[1].GetSingle(),g[2].GetSingle(),g[3].GetSingle(),g[4].GetSingle(),g[5].GetSingle());
    var l = c.GetProperty("lines");
    LegacyGlyphNormalizedLines? lines = l.ValueKind == JsonValueKind.Null ? null : new(l[0].GetSingle(),l[1].GetSingle());
    payload.TryGetNormalizedLines(out var previous);
    payload.SetGeometry(geometry,lines);
    var expected = c.GetProperty("corners");
    for (int i=0;i<8;i++) Equal(payload.Corners.Span[i],expected[i].GetSingle());
    bool present = payload.TryGetNormalizedLines(out var actual);
    Require(present==lines.HasValue,"Presence flag");
    Require(actual==(lines??previous),"Stored normalized lines readback/removal"); count++;
}
var snapshot = payload.Corners.ToArray(); bool flag = payload.HasNormalizedLines;
Throws<ArgumentException>(()=>payload.SetGeometry(new(float.NaN,0,0,0,0,0),null));
Throws<ArgumentException>(()=>payload.SetGeometry(new(0,0,1,0,0,1),new(0,-1)));
Throws<InvalidDataException>(()=>payload.SetGeometry(new(float.MaxValue,0,float.MaxValue,0,0,1),null));
Require(snapshot.AsSpan().SequenceEqual(payload.Corners.Span)&&flag==payload.HasNormalizedLines,"Atomic rejection");
Console.WriteLine($"PASS {count} own affine quad payload controls; normalized pair presence, retained absent pair, finite/overflow atomic rejection.");
static void Equal(float a,float b)=>Require(BitConverter.SingleToInt32Bits(a)==BitConverter.SingleToInt32Bits(b),$"Corner {a:R} vs {b:R}");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}");}
