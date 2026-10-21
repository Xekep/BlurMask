using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("MODELS BLIND-INPUTS CAPTURE");
byte[] R(string n)=>File.ReadAllBytes(Path.Combine(args[0],n));
if(args[1]=="--controls") { Controls(); return; }
void Controls()
{
 var ak=R("ru-RU/ru_RU-ak-cur.lite.res");var lk=R("ru-RU/ru_RU-lk-text.lite.res");var profileBytes=R("ru-RU/legacy-engine-profile.json");
 var names=new List<string>();
 void Refuse(string name,Action action)
 {
  try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){names.Add(name);return;}
  throw new Exception("Guard accepted: "+name);
 }
 foreach(var part in new[]{"temporal","spatial"})foreach(var field in new[]{"minima","maxima"})
 {
  var node=System.Text.Json.Nodes.JsonNode.Parse(profileBytes)!;int value=node[part]![field]![1]!.GetValue<int>();node[part]![field]![1]=value+1;
  var changed=System.Text.Encoding.UTF8.GetBytes(node.ToJsonString());Refuse("same-hash-json-"+part+"-"+field,()=>LegacyRussianModelBundle.Load(ak,lk,changed));
 }
 var changedAk=(byte[])ak.Clone();changedAk[195872]=1;Refuse("foreign-acoustic-selector",()=>LegacyRussianModelBundle.Load(changedAk,lk,profileBytes));
 using var canceled=new CancellationTokenSource();canceled.Cancel();Refuse("canceled-load",()=>LegacyRussianModelBundle.Load(ak,lk,profileBytes,canceled.Token));
 var tmin=LegacyDirectional2B1DefaultCalibration.CopyMinimum(false);var tmax=LegacyDirectional2B1DefaultCalibration.CopyMaximum(false);var smin=LegacyDirectional2B1DefaultCalibration.CopyMinimum(true);var smax=LegacyDirectional2B1DefaultCalibration.CopyMaximum(true);var context=Enumerable.Range(0,28).Select(i=>(short)(i*67-1024)).ToArray();
 var simple=new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(25,100),new LegacyInkPoint(75,12)})};
 (short[] Temporal,short[] Spatial) Pair(IReadOnlyList<LegacyInkStroke> ink,LegacyInkBaseline baseline,short[] ctx,short first,short last,CancellationToken ct=default)=>LegacySaturatedDirectionalFeatures.Extract(ink,baseline,tmin,tmax,smin,smax,ctx,first,last,ct);
 Refuse("canceled-extraction",()=>Pair(simple,new(0,100),context,1024,-1024,canceled.Token));
 Refuse("empty-source",()=>Pair(Array.Empty<LegacyInkStroke>(),new(0,100),context,1024,-1024));
 Refuse("empty-stroke",()=>Pair(new[]{new LegacyInkStroke(Array.Empty<LegacyInkPoint>())},new(0,100),context,1024,-1024));
 var manyIdentical=new[]{new LegacyInkStroke(Enumerable.Repeat(new LegacyInkPoint(1,1),256).ToArray())};
 if(LegacyDirectionalResampling.Prepare(manyIdentical,100,false).Strokes.Sum(st=>st.Points.Length)!=1)throw new Exception("Native zero-step did not retain one point perphysical stroke");
 _=Pair(manyIdentical,new(0,100),context,1024,-1024);names.Add("raw-256-collapse-to-one-prepared");
 _=LegacyDirectional2B1Landmarks.Extract(new[]{new LegacyInkStroke(Enumerable.Repeat(new LegacyInkPoint(1,1),255).ToArray())});names.Add("standalone-255-landmarks-admitted");
 Refuse("context-width",()=>Pair(simple,new(0,100),new short[27],1024,-1024));Refuse("zero-baseline-height",()=>Pair(simple,new(0,0),context,1024,-1024));Refuse("bad-boundary",()=>Pair(simple,new(0,100),context,0,-1024));
 Refuse("sixteen-strokes",()=>Pair(Enumerable.Repeat(simple[0],16).ToArray(),new(0,100),context,1024,-1024));Refuse("point-cap",()=>Pair(new[]{new LegacyInkStroke(Enumerable.Repeat(new LegacyInkPoint(1,1),4097).ToArray())},new(0,100),context,1024,-1024));
 var bundle=LegacyRussianModelBundle.Load(ak,lk,profileBytes);var classifier=bundle.CreateGlyphClassifier();var before=classifier.Classify(simple,new(0,100),context,1024,-1024);Array.Fill(profileBytes,(byte)0);Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);var after=classifier.Classify(simple,new(0,100),context,1024,-1024);
 for(int i=0;i<4;i++)if(!before.FamilyProbabilities[i]!.Value.Span.SequenceEqual(after.FamilyProbabilities[i]!.Value.Span))throw new Exception("Caller buffer mutation changed owned actual classifier");names.Add("owned-model-profile-buffers");
 Parallel.For(0,16,_=>{var concurrent=classifier.Classify(simple,new(0,100),context,1024,-1024);for(int family=0;family<4;family++)if(!before.FamilyProbabilities[family]!.Value.Span.SequenceEqual(concurrent.FamilyProbabilities[family]!.Value.Span))throw new Exception("Concurrent owned classification changed");});names.Add("concurrent-owned-classification");
 GC.Collect();GC.WaitForPendingFinalizers();var retained=classifier.Classify(simple,new(0,100),context,1024,-1024);for(int family=0;family<4;family++)if(!before.FamilyProbabilities[family]!.Value.Span.SequenceEqual(retained.FamilyProbabilities[family]!.Value.Span))throw new Exception("GC changed owned model");names.Add("retained-model-after-GC");
 var cases=new List<LegacyInkStroke[]> {simple,new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(4,5)})},new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(100,0)})},new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(0,100)})},new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(-100,-50),new LegacyInkPoint(-25,50),new LegacyInkPoint(70,-10)})},new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(100,100)}),new LegacyInkStroke(new[]{new LegacyInkPoint(100,0),new LegacyInkPoint(0,100)})}};
 cases.Add(new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(1,4),new LegacyInkPoint(3,1)})});
 cases.Add(new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(4,1),new LegacyInkPoint(1,3)})});
 cases.Add(new[]{new LegacyInkStroke(new[]{new LegacyInkPoint(0,0),new LegacyInkPoint(0,16),new LegacyInkPoint(20,0),new LegacyInkPoint(1400,0)})});
 using var output=File.Create(args[2]);using var writer=new Utf8JsonWriter(output);writer.WriteStartObject();writer.WriteString("scope","Actual2B1 explicit numerical controls; synthetic geometry, no quality labels");writer.WriteStartArray("guards");foreach(var name in names)writer.WriteStringValue(name);writer.WriteEndArray();writer.WriteStartArray("cases");int index=0;
 foreach(var ink in cases)
 {
  var pair=Pair(ink,new(0,100),context,1024,-1024);foreach(var vector in new[]{pair.Temporal,pair.Spatial})if(!vector.AsSpan(283,28).SequenceEqual(context)||vector[311]!=1024||vector[312]!=-1024)throw new Exception("Context/boundary changed");
  var actualSamples=LegacyDirectionalResampling.Prepare(ink,100,false);var unitSamples=LegacyDirectionalResampling.Prepare(ink,100,true);
  if(!actualSamples.CumulativePhysicalLengths.Span.SequenceEqual(unitSamples.CumulativePhysicalLengths.Span)||actualSamples.SourceStepFixed!=unitSamples.SourceStepFixed||actualSamples.Strokes.Count!=unitSamples.Strokes.Count)throw new Exception("Whole-unit flag changed resampling allocation/lengths");
  for(int st=0;st<actualSamples.Strokes.Count;st++)if(!actualSamples.Strokes[st].Points.SequenceEqual(unitSamples.Strokes[st].Points))throw new Exception("Whole-unit flag changed prepared points");
  var actualScore=classifier.Classify(ink,new(0,100),context,1024,-1024,includeBaselineFamilies:false);
  writer.WriteStartObject();writer.WriteNumber("id",index++);writer.WriteStartArray("strokes");foreach(var stroke in ink){writer.WriteStartArray();foreach(var point in stroke.Points){writer.WriteStartArray();writer.WriteNumberValue(point.X);writer.WriteNumberValue(point.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();writer.WriteStartArray("context");foreach(var value in context)writer.WriteNumberValue(value);writer.WriteEndArray();writer.WriteStartArray("inputs");foreach(var vector in new[]{pair.Temporal,pair.Spatial}){writer.WriteStartArray();foreach(var value in vector)writer.WriteNumberValue(value);writer.WriteEndArray();}writer.WriteEndArray();
  writer.WriteNumber("shift",actualSamples.ScaleShift);writer.WriteNumber("step",actualSamples.SourceStepFixed);writer.WriteBoolean("recomputedSamplePointSequenceSame",true);
  writer.WriteStartArray("dimensions");writer.WriteNumberValue(actualSamples.Width);writer.WriteNumberValue(actualSamples.Height);writer.WriteEndArray();writer.WriteStartArray("oldUnitDimensions");writer.WriteNumberValue(unitSamples.Width);writer.WriteNumberValue(unitSamples.Height);writer.WriteEndArray();
  writer.WriteStartArray("prepared");foreach(var stroke in actualSamples.Strokes){writer.WriteStartArray();foreach(var point in stroke.Points){writer.WriteStartArray();writer.WriteNumberValue(point.X);writer.WriteNumberValue(point.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
  writer.WriteStartArray("probabilities");for(int family=2;family<4;family++){writer.WriteStartArray();foreach(var value in actualScore.FamilyProbabilities[family]!.Value.Span)writer.WriteNumberValue(value);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteEndObject();
 }
 writer.WriteEndArray();writer.WriteEndObject();writer.Flush();Console.WriteLine($"PASS actual2B1 {names.Count} guards, {cases.Count} controls");
}

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
w.WriteStartObject();w.WriteString("inputSha256",Convert.ToHexStringLower(SHA256.HashData(inputBytes)));w.WriteString("directionalProducer","Actual1111Raw88=1Raw89=2/SaturateCalibrateSaturate/DefaultCatalog/WholeMaxNoUnit/ActualBC280/PreparedCap255");w.WriteBoolean("expectedLabelsRead",false);w.WriteStartArray("samples");int index=0;
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
   var pair=LegacySaturatedDirectionalFeatures.Extract(normalized.Strokes,normalized.Baseline,temporalMin,temporalMax,spatialMin,spatialMax,context,signs.Start,signs.End,cancel.Token);
   short[][] inputs=[LegacyStrokeFeatures.ExtractInputWithBaseline(normalized.Strokes,normalized.Baseline,context,signs.Start,signs.End,cancel.Token),LegacyRasterFeatures.ExtractInputWithBaseline(normalized.Strokes,normalized.Baseline,context,signs.Start,signs.End,cancel.Token),pair.Temporal,pair.Spatial];
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
