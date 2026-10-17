using InkRecognition;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));int streams=0,requests=0,failures=0,guards=0;
foreach(var row in controls.RootElement.GetProperty("streams").EnumerateArray())
{
 int n=row.GetProperty("objects").GetInt32();
 var document=LegacyOwnedRawInkDocument.Create(Enumerable.Range(0,n).Select(i=>new InkStroke(new InkPoint[]{new(i,0),new(i,1)})).ToArray(),default);
 var owner=LegacyOwnedFreshTextSourceContext.Prepare(document);
 if(owner.NextSelectionId!=n||owner.CopyPendingSelections().Count!=0)throw new InvalidDataException("Fresh registry/list initialization differs.");
 foreach(var req in row.GetProperty("requests").EnumerateArray())
 {
  var pairs=Key(req.GetProperty("ranges"));long outputId=req.GetProperty("residue").GetInt64();
  var result=owner.CreatePrepared(pairs,req.GetProperty("kind").GetInt32(),ref outputId,req.GetProperty("stage").GetInt32());
  if(outputId!=req.GetProperty("output").GetInt64()||owner.NextSelectionId!=req.GetProperty("next").GetInt64()||
   owner.CopyPendingSelections().Count!=req.GetProperty("commandCount").GetInt32())throw new InvalidDataException("Native counter/output/failure prefix differs.");
  bool failed=req.GetProperty("stage").GetInt32()!=0;
  if((result is null)!=failed||owner.Failed!=failed)throw new InvalidDataException("Registration outcome differs.");
  if(result is not null)
  {
   var saved=result.CopyRanges();Array.Clear(pairs);var exposed=result.CopyRanges();Array.Clear(exposed);
   if(!result.CopyRanges().AsSpan().SequenceEqual(saved))throw new InvalidDataException("Selection ranges are mutable aliases.");
  }
  else
  {failures++;long unchanged=77;Reject(()=>owner.CreatePrepared([],0,ref unchanged));if(unchanged!=77)throw new InvalidDataException("Failed-owner guard wrote output.");}
  requests++;
 }
 var expected=row.GetProperty("pending");var actual=owner.CopyPendingSelections();
 for(int i=0;i<actual.Count;i++)
 {var c=actual[i];var e=expected[i];if(c.SelectionId!=e.GetProperty("id").GetInt64()||c.Name!=e.GetProperty("name").GetString()||
  c.OperationCode!=4||c.Enabled!=1||c.Category!=100||c.CommandIdentity!=i+1||c.PreviousCommandIdentity!=i||!ReferenceEquals(c.Owner,owner)||
  !c.CopyRanges().AsSpan().SequenceEqual(Key(e.GetProperty("ranges"))))throw new InvalidDataException("C4D command/selection sidecars differ.");}
 streams++;
}
var rawOwner=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),
 Word(),0,table,cases,publication);var raw=rawOwner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
var completed=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
using var choices=LegacyFreshMainTextGlyphChoices.Prepare(ak,completed,table,publication,norm,circles);
var words=choices.Publish(new byte[40],new byte[2040*4]);var source=LegacyOwnedFreshTextSourceContext.Prepare(rawOwner.SourceDocument);
using var output=File.Create(args[11]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteNumber("originalObjects",rawOwner.SourceDocument.ObjectCount);w.WriteStartArray("actual");int real=0;
foreach(var glyph in words.SelectMany(word=>word.Glyphs))
{
 var positions=glyph.SourcePositions;
 //Fresh0210C0 does NOT deduplicate equal range keys;024F30 controls whether it is called.
 for(int repeat=0;repeat<2;repeat++)
 {
  var selection=source.CreateCharacterSelection(positions);w.WriteStartObject();w.WriteNumber("id",selection.SelectionId);
  w.WriteString("name",selection.Name);w.WriteNumber("command",selection.CommandIdentity);w.WriteNumber("previous",selection.PreviousCommandIdentity);
  w.WriteStartArray("ranges");foreach(var range in selection.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();real++;
 }
 long before=source.NextSelectionId;Reject(()=>source.CreateCharacterSelection(positions,new CancellationToken(true)));
 if(source.NextSelectionId!=before)throw new InvalidDataException("Canceled registration consumed identity.");
 var foreign=LegacyOwnedRawInkDocument.Create(Word(),default);Reject(()=>LegacyOwnedFreshTextSourceContext.Prepare(foreign).CreateCharacterSelection(positions));
}
w.WriteEndArray();w.WriteNumber("next",source.NextSelectionId);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {streams} independent registry/transaction streams/{requests} registrations/{failures} retained failure prefixes/{real} actual source-position registrations/{guards} guards. Pending commands only; no024D40 or fullwrapper claim.");
LegacyNativeInkPositionRange[] Key(JsonElement rows)=>rows.EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Required guard missing.");}
InkStroke[] Word()=>[Draw((0,80),(0,0),(30,45),(60,0),(60,80)),Draw((85,80),(85,0),(135,0),(135,80)),Draw((165,80),(165,0),(215,0),(215,40),(165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
