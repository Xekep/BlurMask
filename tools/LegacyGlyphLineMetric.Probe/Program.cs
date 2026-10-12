using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("glyph-line-controls.json");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,glyphCount=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 var glyphs=c.GetProperty("glyphs").EnumerateArray().Select(r=>new LegacyPreparedGlyphLineMetricInput(r[0].GetSingle(),r[1].GetSingle(),r[2].GetSingle(),r[3].ValueKind==JsonValueKind.Null?null:r[3].GetSingle(),r[4].ValueKind==JsonValueKind.Null?null:r[4].GetSingle())).ToArray();var result=LegacyPreparedGlyphLineMetricProducer.Prepare(glyphs);
 var expected=c.GetProperty("expected");
 if(expected.ValueKind==JsonValueKind.Null)Require(result is null,"Native empty no-op became an invented line.");
 else
 {
  Require(result.HasValue,"Prepared glyph line was omitted.");var v=result!.Value;
  Require(v.OriginX==0&&v.Angle==0,"Native setter zero arguments changed.");
  Exact(v.BaselineY,expected[0].GetDouble());Exact(v.XHeight,expected[1].GetDouble());
  Require(BitConverter.SingleToUInt32Bits(v.ToSdkSourceLineXHeight())==BitConverter.SingleToUInt32Bits(expected[2].GetSingle()),"SDK double-to-float metric differs.");
  var sample=v.ToFontSizeSample(65,1,10,20);Require(sample.SourceLineXHeight==v.ToSdkSourceLineXHeight()&&sample.WordLeft==10&&sample.WordRight==20,"Font policy bridge lost line metric/coordinate ownership.");
 }
 count++;glyphCount+=glyphs.Length;
}
var glyph=new LegacyPreparedGlyphLineMetricInput(10,3,4,.75f,.5f);var single=LegacyPreparedGlyphLineMetricProducer.Prepare([glyph])!.Value;Require(single==new LegacyPreparedGlyphLineMetricValues(0,13.75,0,2.5),"Known5-unit affine glyph metric differs.");
var noPayload=LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{BaselineFraction=null,XHeightFraction=null}])!.Value;Require(noPayload==new LegacyPreparedGlyphLineMetricValues(0,15,0,5),"Native absent-fraction glyph geometry branch differs.");
var absentOrder=LegacyPreparedGlyphLineMetricProducer.Prepare([new(1e20f,0,0,null,null),new(-1e20f,3,4,null,null)])!.Value;Require(absentOrder.BaselineY==2.5,"Absent-fraction native (sum+Y)+norm was reassociated.");
var presentOrder=LegacyPreparedGlyphLineMetricProducer.Prepare([new(-1e20f,0,0,1,1),new(5,1e20f,0,1,1)])!.Value;Require(presentOrder.BaselineY==2.5,"Present-fraction native (sum+fraction*norm)+Y was reassociated.");
var normalized=LegacyGlyphTypesetGeometryBuilder.NormalizePrepared(new(new(0,0,10,8),0,0),new(0,10,10,8),16,4,1);
var affine=new LegacyGlyphAffineGeometry(0,10,10,0,0,8);var payload=new LegacyGlyphGeometryPayload();payload.SetGeometry(affine,new(normalized.BaselineFraction,normalized.XHeightFraction));Require(payload.TryGetNormalizedLines(out var stored),"Actual normalized-lines presence was lost.");
var roundTrip=LegacyPreparedGlyphLineMetricProducer.Prepare([LegacyPreparedGlyphLineMetricInput.FromPreparedGeometry(affine,stored)])!.Value;Require(roundTrip.BaselineY==16&&roundTrip.XHeight==4,"Rendered glyph fractions -> payload readback -> prepared line metrics round-trip differs.");
Require(LegacyHandwritingFontSizePolicy.MatchPrepared(64,8,[roundTrip.ToFontSizeSample(65,1,0,10)])==32,"Prepared source-line metric did not reach existing font-size policy.");
Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{OriginY=float.NaN}]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{VerticalVectorX=float.PositiveInfinity}]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{XHeightFraction=null}]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{XHeightFraction=-1}]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph with{BaselineFraction=float.NaN}]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare(new LegacyPreparedGlyphLineMetricInput[4097]));Reject(()=>LegacyPreparedGlyphLineMetricProducer.Prepare([glyph],new CancellationToken(true)));
Reject(()=>new LegacyPreparedGlyphLineMetricValues(0,0,0,double.MaxValue).ToSdkSourceLineXHeight());Reject(()=>new LegacyPreparedGlyphLineMetricValues(0,0,0,-1).ToSdkSourceLineXHeight());
var large=LegacyPreparedGlyphLineMetricProducer.Prepare([new(0,float.MaxValue,float.MaxValue,null,null)])!.Value;Require(double.IsFinite(large.XHeight)&&large.XHeight>float.MaxValue,"Native promoted-float norm overflowed before SDK narrowing.");Reject(()=>large.ToSdkSourceLineXHeight());
Console.WriteLine($"PASS {count} independent prepared glyph-line cases/{glyphCount} glyphs; native hypot/double ordered averaging, x/a zero setters, empty no-op, SDKfloat xheight, renderedfraction round-trip and fontpolicy bridge; malformed/canceled/bounded/overflow rejection. Initial handwriting line estimator remains separate.");
static void Exact(double a,double b){Require(BitConverter.DoubleToUInt64Bits(a)==BitConverter.DoubleToUInt64Bits(b),$"Native/Decimal binary64 metric differs:{a:R}/{b:R}");}
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Malformed prepared glyph-line input accepted.");}
