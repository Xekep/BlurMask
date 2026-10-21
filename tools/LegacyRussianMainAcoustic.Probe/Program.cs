using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("MODELS BLIND-INPUTS CAPTURE");
byte[] R(string n)=>File.ReadAllBytes(Path.Combine(args[0],n));
var shared=LegacyFreshMainRecognitionContext.Load(R("ru-RU/ru_RU-ak-cur.lite.res"),R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=new MemoryStream(R("text-unicode/legacy-case-data.json"));using var ps=new MemoryStream(R("text-unicode/legacy-text-publication-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);
using var profile=JsonDocument.Parse(R("ru-RU/legacy-engine-profile.json"));
short[] Calibration(string part,string field)=>profile.RootElement.GetProperty(part).GetProperty(field).EnumerateArray().Select(v=>v.GetInt16()).ToArray();
var temporalMin=Calibration("temporal","minima");var temporalMax=Calibration("temporal","maxima");var spatialMin=Calibration("spatial","minima");var spatialMax=Calibration("spatial","maxima");
int totalGroups=0,missingGroups=0;
byte[] inputBytes=File.ReadAllBytes(args[1]);using var input=JsonDocument.Parse(inputBytes);using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);
void Ints(string name,IEnumerable<int> values){w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
w.WriteStartObject();w.WriteString("inputSha256",Convert.ToHexStringLower(SHA256.HashData(inputBytes)));w.WriteBoolean("expectedLabelsRead",false);w.WriteStartArray("samples");int index=0;
foreach(var item in input.RootElement.GetProperty("samples").EnumerateArray())
{
 string id=item.GetProperty("id").GetString()!;var raw=item.GetProperty("strokes").EnumerateArray().Select(st=>new InkStroke(st.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 w.WriteStartObject();w.WriteString("id",id);w.WriteNumber("points",raw.Sum(s=>s.Points.Count));
 try
 {
  using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));
  var invocation=shared.PrepareGlobal(raw,0,table,cd,pd,cancel.Token);
  var result=invocation.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],cancel.Token);
  int nn=result.Selection.Publication.PassResult.Policy.Owner.AcousticEvaluations;
  w.WriteBoolean("supported",true);w.WriteBoolean("hasWinner",result.HasWinner);w.WriteBoolean("publicationCompleted",result.PublicationCompleted);w.WriteNumber("nn",nn);w.WriteStartArray("words");
  foreach(var word in result.RawWordsInNativeHeadOrder)
  {
   w.WriteStartObject();w.WriteString("text",word.Text);w.WriteBase64String("utf8",word.CopyUtf8());w.WriteStartArray("glyphs");
   foreach(var g in word.Glyphs){w.WriteStartObject();w.WriteNumber("flags",g.Descriptor.KindFlags);w.WriteNumber("symbol",g.Descriptor.SymbolOrRunLength);w.WriteBase64String("utf8",g.CopyUtf8());w.WriteEndObject();}
   w.WriteEndArray();w.WriteEndObject();
  }
  w.WriteEndArray();
  var confidence=result.Selection.Publication.ConfidenceOwner;var owner=confidence.Owner;var geometry=owner.Acoustic.Geometry;
  var source=geometry.Graph.Source.Source.CopyStrokes();var masks=owner.Symbols.CopyMasks();Ints("mask",masks.Primary.ToArray().Select(v=>(int)v));
  w.WriteStartArray("mainGroups");int captured=0;
  for(int g=0;g<owner.Graph.State.Groups.Count;g++)
  {
   LegacyFreshMainGlobalGlyphConfidenceGroup actual;
   try{actual=confidence.ReadGroup(g,cancel.Token);}
   catch(InvalidOperationException e)when(e.Message=="The actual D0 controller has not evaluated this global group; confidence never runs a replacement NN."){missingGroups++;continue;}
   var normalized=geometry.NormalizeGroup(g,cancel.Token);var ranges=geometry.CopyChronologicalRanges(g,cancel.Token);var context=geometry.Context.GetContext(g,cancel.Token);var signs=geometry.ReadBoundarySigns(g,cancel.Token);var frame=geometry.ReadFrame(g,cancel.Token);var primitive=geometry.ReadPrimitiveScope(g,cancel.Token);
   short[][] inputs=[LegacyStrokeFeatures.ExtractInputWithBaseline(normalized.Strokes,normalized.Baseline,context,signs.Start,signs.End,cancel.Token),LegacyRasterFeatures.ExtractInputWithBaseline(normalized.Strokes,normalized.Baseline,context,signs.Start,signs.End,cancel.Token),LegacyTemporalDirectionalFeatures.Extract(normalized.Strokes,normalized.Baseline,temporalMin,temporalMax,context,signs.Start,signs.End,cancel.Token),LegacySpatialDirectionalFeatures.Extract(normalized.Strokes,normalized.Baseline,spatialMin,spatialMax,context,signs.Start,signs.End,cancel.Token)];
   var rawGroups=ranges.Select(range=>new InkStroke(source[range.StrokeIndex].Points.Skip(range.StartPoint).Take(range.EndPointExclusive-range.StartPoint).ToArray())).ToArray();
   var dynamic=LegacyDynamicAcousticWeights.Evaluate(rawGroups,owner.Model.AcousticWeights.DynamicProfile,cancel.Token);var weights=owner.Model.AcousticWeights.ResolveWeights(dynamic,cancel.Token);
   w.WriteStartObject();w.WriteNumber("id",g);w.WriteNumber("sourceHeight",frame.Height);w.WriteNumber("sourceBaseline",frame.Baseline);w.WriteNumber("minX",geometry.ReadMinimumX(g,cancel.Token));w.WriteNumber("target",geometry.TargetHeight);w.WriteNumber("normalizedBaseline",normalized.Baseline.Y);w.WriteNumber("normalizedHeight",normalized.Baseline.Height);w.WriteBoolean("whole",primitive.BoundaryAlignedSingleNormalizedStroke);w.WriteBoolean("zeroEndpoints",primitive.EndpointFieldsAreZero);
   w.WriteNumber("sourceProbability",actual.FirstStoredSourceProbability);Ints("context",context.Select(v=>(int)v));Ints("boundaries",new[]{(int)signs.Start,(int)signs.End});
   w.WriteStartArray("rawRanges");foreach(var st in rawGroups){w.WriteStartArray();foreach(var pt in st.Points){w.WriteStartArray();w.WriteNumberValue(pt.X);w.WriteNumberValue(pt.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("normalized");foreach(var st in normalized.Strokes){w.WriteStartArray();foreach(var pt in st.Points){w.WriteStartArray();w.WriteNumberValue(pt.X);w.WriteNumberValue(pt.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("inputs");foreach(var rowInput in inputs){w.WriteStartArray();foreach(short value in rowInput)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("probabilities");for(int f=0;f<4;f++){w.WriteStartArray();foreach(int value in actual.CopyFamilyProbabilities(f))w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("weights");foreach(float value in weights)w.WriteNumberValue(value);w.WriteEndArray();Ints("rawCosts",actual.CopyRawIntegerCosts());Ints("finalCosts",actual.CopyFinalClassCosts());
   w.WriteEndObject();captured++;totalGroups++;
  }
  w.WriteEndArray();if(captured!=nn)throw new Exception("Original D0 snapshot count differs from actualNN evaluations");
  if(result.Selection.Publication.PassResult.Policy.Owner.AcousticEvaluations!=nn)throw new Exception("Inspection reran inference.");
 }
 catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException)
 {throw new InvalidOperationException("MAIN diagnostic failed before target access: "+id,e);}
 w.WriteEndObject();w.Flush();if(++index%20==0)Console.WriteLine("Completed blind "+index);
}
w.WriteEndArray();w.WriteNumber("originalMainGroups",totalGroups);w.WriteNumber("unevaluatedGroups",missingGroups);w.WriteBoolean("originalProbabilitiesAndCosts",true);w.WriteBoolean("pureInputsRecomputed",true);w.WriteEndObject();w.Flush();Console.WriteLine($"Completed blind {index}, originalMAIN {totalGroups}, unevaluated {missingGroups}");
