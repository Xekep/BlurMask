using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("Supply independent glyph typeset controls.");
if(new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Controls exceed 8 MiB.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
    var s=c.GetProperty("source");var source=new LegacyFontGlyphMetrics(new(s[0].GetSingle(),s[1].GetSingle(),s[2].GetSingle(),s[3].GetSingle()),s[4].GetSingle(),s[5].GetSingle());
    var m=c.GetProperty("measured");float? measured=m.ValueKind==JsonValueKind.Null?null:m.GetSingle();
    float reference=LegacyGlyphTypesetGeometryBuilder.ReferenceCharacterHeight(measured,c.GetProperty("styleLineHeight").GetSingle());Equal(reference,c.GetProperty("reference").GetSingle());
    var b=c.GetProperty("prepared");var box=new LegacyLineBounds(b[0].GetSingle(),b[1].GetSingle(),b[2].GetSingle(),b[3].GetSingle());
    var a=LegacyGlyphTypesetGeometryBuilder.NormalizePrepared(source,box,c.GetProperty("baseline").GetSingle(),reference,c.GetProperty("scale").GetSingle());var e=c.GetProperty("ratios");
    Equal(a.BaselineFraction,e[0].GetSingle());Equal(a.XHeightFraction,e[1].GetSingle());Equal(a.LeftBearingFraction,e[2].GetSingle());Equal(a.RightBearingEndpointFraction,e[3].GetSingle());Require(a.Bounds==box,"Explicit prepared-box ownership.");
    var sanitized=LegacyGlyphTypesetGeometryBuilder.SanitizeProviderMetrics(source);
    Require(sanitized.Bounds.Width==MathF.Max(source.Bounds.Width,0)&&sanitized.Bounds.Height==MathF.Max(source.Bounds.Height,0)&&sanitized.LeftSideBearing==source.LeftSideBearing&&sanitized.RightSideBearing==source.RightSideBearing,"Provider dimension sanitization.");count++;
}
Throws<ArgumentException>(()=>LegacyGlyphTypesetGeometryBuilder.ReferenceCharacterHeight(float.NaN,12));
Throws<ArgumentException>(()=>LegacyGlyphTypesetGeometryBuilder.ReferenceCharacterHeight(null,0));
Throws<ArgumentException>(()=>LegacyGlyphTypesetGeometryBuilder.NormalizePrepared(new(new(0,0,0,1),1,0),new(0,0,1,1),0,1,1));
Throws<ArgumentException>(()=>LegacyGlyphTypesetGeometryBuilder.NormalizePrepared(new(new(0,0,1,1),0,0),new(0,0,1,0),0,1,1));
Throws<InvalidDataException>(()=>LegacyGlyphTypesetGeometryBuilder.NormalizePrepared(new(new(0,0,1,1),0,0),new(0,0,1,float.Epsilon),0,1,1));
Console.WriteLine($"PASS {count} own rendered-glyph affine ratios, style font-line-height fallback and provider sanitization; missing/degenerate/nonfinite rejection.");
static void Equal(float a,float b)=>Require(float.IsFinite(a)&&MathF.Abs(a-b)<=2e-7f*MathF.Max(1,MathF.Abs(b)),$"Glyph ratio {a} vs {b}");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}");}
