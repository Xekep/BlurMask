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
foreach(var sample in doc.RootElement.GetProperty("samples").EnumerateArray())
{
 var raw=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw,activations,separation,cancel.Token);
 var row=owner.Factory.Frame.Row;w.WriteStartObject();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteNumber("baseline",row.Frame.Line.Baseline);w.WriteNumber("height",row.Frame.Line.Height);
 w.WriteStartArray("source");foreach(var st in row.Prepared.Strokes){w.WriteStartArray();foreach(var p in st.Points){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("confidence");foreach(var x in row.OriginalSource.SourceStrokeProbabilities.Span)w.WriteNumberValue(x);w.WriteEndArray();
 Ints("counts",owner.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray());w.WriteStartArray("records");foreach(var p in owner.Cuts.Cuts.PreparedInk.Points.Span){w.WriteStartArray();foreach(float f in new[]{p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength})w.WriteNumberValue(BitConverter.SingleToInt32Bits(f));w.WriteNumberValue(p.Tag);w.WriteNumberValue(p.StrokeIndex);w.WriteNumberValue(p.OriginalPointIndex);w.WriteEndArray();total++;}w.WriteEndArray();
 w.WriteStartArray("markers");foreach(var marker in owner.Cuts.Cuts.Markers){w.WriteStartArray();w.WriteNumberValue(marker.Kind);w.WriteNumberValue(marker.StrokeIndex);w.WriteNumberValue(marker.PointIndex);w.WriteEndArray();}w.WriteEndArray();

 Ints("rawCounts",raw.Select(st=>st.Points.Count));
 w.WriteStartArray("retainedRawIndices");foreach(var ids in row.OriginalSource.OriginalPointIndices){w.WriteStartArray();foreach(int id in ids.Span)w.WriteNumberValue(id);w.WriteEndArray();}w.WriteEndArray();
 Ints("smoothingTicks200",row.Prepared.PointKeys200.ToArray());
 w.WriteStartArray("chronology");for(int stroke=0;stroke<owner.Cuts.Chronology.StrokePointCounts.Length;stroke++)for(int point=0;point<owner.Cuts.Chronology.StrokePointCounts.Span[stroke];point++)
 {var key=owner.Cuts.Chronology.Resolve(stroke,point);var end=row.OriginalSource.ResolveEndingMetadata(key);w.WriteStartArray();foreach(int v in new[]{key.Key.First,key.Key.Second,key.Key.Third,key.MetadataPointIndex,key.KeyRecordIndex,(int)key.RemainderByte,end.Key.Third})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
 var graph=owner.Prepared.SourceRanges.Graph;w.WriteStartArray("arcs");for(int node=0;node<graph.NodeCount;node++)for(int ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)
 {var arc=graph.GetNode(node)[ordinal];w.WriteStartObject();w.WriteNumber("node",node);w.WriteNumber("ordinal",ordinal);w.WriteNumber("group",arc.GroupId);w.WriteNumber("firstContext",arc.StartContextId);w.WriteNumber("lastContext",arc.EndContextId);
 w.WriteStartArray("ranges");foreach(var rg in graph.Groups[arc.GroupId].Ranges.Span){w.WriteStartArray();foreach(int v in new[]{rg.StrokeIndex,rg.StartPoint,rg.EndPointExclusive})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("references");foreach(var reference in owner.Prepared.SourceRanges.ResolveCandidateWithResiduals(node,ordinal,cancel.Token)){w.WriteStartArray();foreach(int v in new[]{reference.First.First,reference.First.Second,reference.First.Third,reference.Last.First,reference.Last.Second,reference.Last.Third,(int)reference.FirstResidual,(int)reference.LastResidual})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
 w.WriteEndObject();if(++count%20==0)Console.WriteLine($"CUT{count}samples/{total}actualrecords");
}
w.WriteEndArray();w.WriteNumber("samplesCount",count);w.WriteNumber("recordsCount",total);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS complete CUT{count}/{total} actual records; no target labels read.");