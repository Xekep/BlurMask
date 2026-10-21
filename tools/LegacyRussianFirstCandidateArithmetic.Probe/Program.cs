using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("MODELS BLIND-COORDINATES OUTPUT");
byte[] Read(string name)=>File.ReadAllBytes(Path.Combine(args[0],name));
byte[] resource=Read("ru-RU/ru_RU-ak-cur.lite.res"),input=File.ReadAllBytes(args[1]);
using var profile=JsonDocument.Parse(Read("ru-RU/legacy-engine-profile.json"));var a=profile.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var separation=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var doc=JsonDocument.Parse(input);using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);
w.WriteStartObject();w.WriteBoolean("expectedLabelsRead",false);w.WriteString("inputSha256",Convert.ToHexStringLower(SHA256.HashData(input)));w.WriteStartArray("samples");int count=0,total=0;
void Ints(string name,IEnumerable<int> values){w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Shorts(string name,IEnumerable<LegacyInkStroke> strokes){w.WriteStartArray(name);foreach(var st in strokes){w.WriteStartArray();foreach(var p in st.Points){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
foreach(var sample in doc.RootElement.GetProperty("samples").EnumerateArray())
{
 var raw=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw,activations,separation,cancel.Token);
 var row=owner.Factory.Frame.Row;var context=LegacyFirstRowContext.Prepare(resource,6120,row,activations,cancel.Token);
 w.WriteStartObject();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteNumber("baseline",row.Frame.Line.Baseline);w.WriteNumber("height",row.Frame.Line.Height);w.WriteNumber("target",owner.Factory.AcousticModel.NormalizationTarget);
 Ints("counts",owner.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray());w.WriteStartArray("prepared");foreach(var p in owner.Cuts.Cuts.PreparedInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();
 w.WriteString("mask",Convert.ToHexString(owner.MaskPolicy.CopyMasks().Primary.Span));
 w.WriteStartArray("groups");int index=0;
 foreach(var score in owner.GroupScores)
 {
  var originalRanges=owner.Prepared.SourceRanges.Graph.Groups[index].Ranges.ToArray();var glyph=score.Glyph;var ranges=glyph.Geometry.Ranges.ToArray();
  var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,owner.Cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  w.WriteStartObject();w.WriteNumber("ordinal",index++);w.WriteStartArray("ranges");foreach(var rg in ranges){w.WriteStartArray();w.WriteNumberValue(rg.StrokeIndex);w.WriteNumberValue(rg.StartPoint);w.WriteNumberValue(rg.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
  Ints("context",context.GetContext(owner.Cuts,originalRanges,cancel.Token).Select(v=>(int)v));Ints("boundaries",new[]{(int)signs.Start,signs.End});
  Shorts("normalized",glyph.Geometry.Strokes);Shorts("merged",glyph.MergedGeometry.Strokes);Ints("retained",glyph.MergedGeometry.RetainedBoundaryIndices.ToArray());
  Ints("trajectoryProbabilities",glyph.Scores.TrajectoryProbability32768.ToArray());Ints("rasterProbabilities",glyph.Scores.RasterProbability32768.ToArray());Ints("costs",glyph.IntegerCosts.ToArray());
  w.WriteEndObject();total++;
 }
 w.WriteEndArray();w.WriteEndObject();if(++count%20==0)Console.WriteLine($"FIRST{count}samples/{total}actualNNgroups");
}
w.WriteEndArray();w.WriteNumber("samplesCount",count);w.WriteNumber("groupsCount",total);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS complete FIRST{count}/{total} actual groups; no target labels read.");
