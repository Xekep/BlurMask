using InkRecognition;
using System.Text.Json;
if(args.Length is not (2 or 12))throw new ArgumentException("[AK LK engine separation case publication normalization circles line30 line50] controls capture");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[^2]));int ordinal=0,contains=0;
using var stream=File.Create(args[^1]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteStartArray("numeric");
foreach(var t in controls.RootElement.GetProperty("tests").EnumerateArray())
{
 var counts=t.GetProperty("counts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 var doc=LegacyOwnedRawInkDocument.Create(counts.Select((n,i)=>new InkStroke(Enumerable.Range(0,n).Select(j=>new InkPoint(i,j)).ToArray())).ToArray(),default);
 var owner=LegacyOwnedRawInkRangeOperations.Prepare(doc);
 var target=Key(t.GetProperty("target"));var extra=Key(t.GetProperty("extra"));var filters=Key(t.GetProperty("filters"));var cuts=Key(t.GetProperty("cuts"));
 var u=owner.Union(target,extra);var intersection=owner.Intersect(target,filters);var d=owner.Subtract(target,cuts);
 Check(u,t.GetProperty("union"),"union");Check(intersection,t.GetProperty("intersect"),"intersect");Check(d,t.GetProperty("subtract"),"subtract");
 for(int i=0;i<filters.Length;i++)
 {if(LegacyOwnedRawInkRangeOperations.Contains(target,filters[i])!=t.GetProperty("contains")[i].GetBoolean())throw new InvalidDataException($"Contains case{ordinal} differs.");contains++;}
 if(!LegacyOwnedRawInkRangeOperations.Equal(target,target))throw new InvalidDataException("Raw equality differs.");
 w.WriteStartObject();Write("union",u);Write("intersect",intersection);Write("subtract",d);w.WriteEndObject();ordinal++;
}
w.WriteEndArray();int guards=0;
var guardDoc=LegacyOwnedRawInkDocument.Create(new InkStroke[]{new(new InkPoint[]{new(0,0),new(1,1),new(2,2)})},default);
var guardOwner=LegacyOwnedRawInkRangeOperations.Prepare(guardDoc);
LegacyNativeInkPositionRange[] sharp=[new(new(1),new(182021))];
var sharpResult=guardOwner.Union(sharp,new LegacyNativeInkPositionRange[]{new(new(50949),new(75783))});
if(sharpResult.Length!=1||sharpResult[0].First.Value!=1||sharpResult[0].Last.Value!=75783)throw new InvalidDataException("Native unsigned first-minus2 wrap branch changed.");
foreach(int kind in new[]{0,1,2})
{
 try{if(kind==0)guardOwner.Union(sharp,sharp,new CancellationToken(true));else if(kind==1)guardOwner.Intersect(sharp,sharp,new CancellationToken(true));else guardOwner.Subtract(sharp,sharp,new CancellationToken(true));}
 catch(OperationCanceledException){guards++;continue;}throw new InvalidDataException("Cancellation guard missing.");
}
if(sharp[0].Last.Value!=182021)throw new InvalidDataException("Range operation mutated caller storage.");
w.WriteStartArray("actual");int actualCases=0;
if(args.Length==12)
{
 byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
 var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
 var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
 using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
 var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
 var input=Word();var rawOwner=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),input,0,table,cases,publication);
 var raw=rawOwner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
 var completed=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
 using var choices=LegacyFreshMainTextGlyphChoices.Prepare(ak,completed,table,publication,norm,circles);
 var words=choices.Publish(new byte[40],new byte[2040*4]);var doc=rawOwner.SourceDocument;
 var sourceOperations=LegacyOwnedRawInkRangeOperations.Prepare(doc);
 LegacyNativeInkPositionRange[] whole=[new(LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
  LegacyNativeInkPosition.Pack((uint)doc.ObjectCount-1,doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))];
 foreach(var word in words)
  for(int i=0;i<word.Glyphs.Count;i++)
  {
   var glyph=word.Glyphs[i];var target=glyph.SourcePositions.Ranges.ToArray();
   var other=word.Glyphs.Where((_,j)=>i!=j).SelectMany(g=>g.SourcePositions.Ranges).ToArray();
   w.WriteStartObject();w.WriteStartArray("counts");foreach(var stroke in doc.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
   Write("target",target);Write("extra",other);Write("filters",whole);Write("cuts",other);
   Write("union",sourceOperations.Union(target,other));Write("intersect",sourceOperations.Intersect(target,whole));Write("subtract",sourceOperations.Subtract(target,other));
   w.WriteEndObject();actualCases++;
  }
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {ordinal} independent linear nativeC21 cases/{contains} containment checks/{actualCases} actual decoded-source compositions/{guards} guards/1native wrapping-edge control.");
void Check(LegacyNativeInkPositionRange[] actual,JsonElement expected,string op)
{if(!actual.AsSpan().SequenceEqual(Key(expected)))throw new InvalidDataException($"C21 {op} case{ordinal}: actual={string.Join(";",actual.Select(r=>$"[{r.First.Value},{r.Last.Value}]"))} expected={expected}");}
void Write(string name,LegacyNativeInkPositionRange[] values)
{w.WriteStartArray(name);foreach(var r in values){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
LegacyNativeInkPositionRange[] Key(JsonElement rows)=>rows.EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();

InkStroke[] Word()=>[Draw((0,80),(0,0),(30,45),(60,0),(60,80)),Draw((85,80),(85,0),(135,0),(135,80)),Draw((165,80),(165,0),(215,0),(215,40),(165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
