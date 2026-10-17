using InkRecognition;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;
if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var samples=new[]{Word(0,0),Word(0,250).Concat(Word(0,0)).ToArray(),Word(0,0).Concat(Word(230,0)).ToArray(),Word(0,0).Concat(Word(300,0)).ToArray(),
    Word(0,0).Concat(Word(450,0)).ToArray(),Word(0,0).Concat(Word(700,0)).ToArray(),Hi(0).Concat(Hi(220)).ToArray()};
int guards=0,rows=0,multiwords=0;
using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("inputs");
for(int sample=0;sample<samples.Length;sample++)
{
    var owner=Prepare(samples[sample]);var result=owner.Run();var input=owner.Input;
    if(!ReferenceEquals(result.Fallback.Input.Owner,input.FirstSource) || !ReferenceEquals(result.Fallback.Lines,result.Lines) ||
        result.Fallback.CreatedLines.Count!=0 || !ReferenceEquals(result.Fallback.Input,result.Lines))
        throw new Exception("Actual assigned main guide did not retain24A no-op ownership.");
    rows+=result.Lines.PublishedRows.Count;if(result.FirstResults.Any(r=>r.Text.Words.Count(x=>!x.IsDelimiter)>1))multiwords++;
    if(sample<4)
    {
        var legacy=input.Run();var oldRows=legacy.Lines.PublishedRows;var newRows=result.Lines.PublishedRows;
        if(oldRows.Count!=newRows.Count||oldRows.Where((r,i)=>!r.CopyStorage().Span.SequenceEqual(newRows[i].CopyStorage().Span)).Any()||
            !Same(legacy.Lines.CopyDescriptors(),result.Lines.CopyDescriptors())||!Same(legacy.Lines.CopyGuides(),result.Lines.CopyGuides()))
            throw new Exception("Existing owning source720/descriptors/main guides changed through TEXT.");
        if(BitConverter.SingleToInt32Bits(legacy.Smoothed.SourceGap)!=BitConverter.SingleToInt32Bits(result.Smoothed.SourceGap)||
            legacy.Smoothed.Strokes.Count!=result.Smoothed.Strokes.Count||legacy.Smoothed.Strokes.Where((s,i)=>!s.Points.SequenceEqual(result.Smoothed.Strokes[i].Points)).Any())
            throw new Exception("Existing owning24D/29B/265 output changed through TEXT.");
    }
    else Reject(()=>input.Run());
    if(!ReferenceEquals(result.Owner,owner)||!ReferenceEquals(result.Lines.Owner,input.FirstSource))throw new Exception("Main TEXT source owner lost.");
    Reject(()=>owner.Run(new CancellationToken(true)));
    var submission=input.FirstSubmissions[0];var text=result.FirstResults[0].Text;var source=input.FirstSource;
    var foreign=Prepare(Word(0,0));Reject(()=>LegacyFirstPlainTextSourceMaterializer.MaterializeForSubmission(foreign.Input.FirstSource,0,text));
    Reject(()=>LegacyFreshMainFirstTextResultLines.Apply(source,[]));Reject(()=>LegacyFreshMainFirstTextResultLines.Apply(source,result.Sources,new CancellationToken(true)));
    Reject(()=>LegacyFirstPlainTextSourceMaterializer.MaterializeForSubmission(source,submission.DescriptorOrdinal,text,new CancellationToken(true)));
    var map=source.FirstSubmissions[0].CurrentParentPhysicalIndices;
    if(!MemoryMarshal.TryGetArray(map,out ArraySegment<int> array))throw new Exception("Submission memory fixture unavailable");
    int original=array.Array![array.Offset];array.Array[array.Offset]=int.MaxValue;
    Reject(()=>LegacyFirstPlainTextSourceMaterializer.MaterializeForSubmission(source,submission.DescriptorOrdinal,text));array.Array[array.Offset]=original;
    w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteNumber("submissions",result.FirstResults.Count);
    w.WriteStartArray("descriptors");foreach(var d in result.Lines.CopyDescriptors())w.WriteBase64StringValue(d);w.WriteEndArray();
    w.WriteStartArray("guides");foreach(var g in result.Lines.CopyGuides())w.WriteBase64StringValue(g);w.WriteEndArray();
    w.WriteStartArray("source720");foreach(var r in result.Lines.PublishedRows)w.WriteBase64StringValue(r.CopyStorage().Span);w.WriteEndArray();
    WritePoints("parentPoints",source.Controller.Source.Strokes.Select(s=>s.Points));WritePoints("currentPoints",result.Lines.CopyPhysicalPoints());
    var controller=source.Controller;w.WriteStartArray("markers");foreach(var v in new[]{controller.Markers.First,controller.Markers.Second,controller.Markers.Hyphen,controller.Markers.Underscore,controller.Markers.ProlongedSound,controller.Markers.IdeographOne,controller.Markers.Replacement})w.WriteNumberValue(v);w.WriteEndArray();
    var fp=controller.Profile.PrepareFitProfile(0,0,[]);w.WriteStartArray("fitProfile");foreach(float v in new[]{fp.MinimumItemCount,fp.MaximumItemCount,fp.MinimumSlope,fp.MaximumSlope,fp.MinimumHeight,fp.MaximumBoxWidth,fp.KnownGlyphWeightCap})w.WriteNumberValue(v);w.WriteEndArray();
    w.WriteStartArray("first");foreach(var r in result.FirstResults)
    {
        w.WriteStartObject();w.WriteNumber("words",r.Text.Words.Count(x=>!x.IsDelimiter));w.WriteStartArray("glyphs");
        foreach(var word in r.Text.Words)foreach(ulong reference in word.Published.Word.FlatReferences)
        {
            var key=LegacyFirstResultGlyphKey.Decode(reference);if(key.Arc<0)continue;
            w.WriteBase64StringValue(r.Text.Result.GlyphPool.CopyRecord(reference));
        }
        w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteNumber("exponentBits",BitConverter.SingleToInt32Bits(controller.SourceScoreExponent));
    w.WriteStartArray("smoothedCounts");foreach(var s in result.Smoothed.Strokes)w.WriteNumberValue(s.Points.Count);w.WriteEndArray();w.WriteEndObject();
    Console.WriteLine($"Main TEXT sample{sample}: {result.FirstResults.Count} first submissions, {result.FirstResults.Sum(r=>r.Text.Words.Count(x=>!x.IsDelimiter))} words, {result.Lines.PublishedRows.Count} real source720, {result.Lines.CopyGuides().Length} guides.");
}
w.WriteEndArray();w.WriteNumber("rows",rows);w.WriteNumber("multiwordInputs",multiwords);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS7 owned raw main TEXT inputs/{rows} actual source720/{multiwords} multiword inputs/{guards} guards; old4 source720+guide byte parity. No full main winner claim.");
LegacyFreshMainOwnedTextInputPreparation Prepare(InkStroke[] raw)=>LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd);
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported owner/scope accepted");}
void WritePoints(string name,IEnumerable<IReadOnlyList<InkPoint>> strokes){w.WriteStartArray(name);foreach(var s in strokes){w.WriteStartArray();foreach(var p in s){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
static bool Same(byte[][] a,byte[][] b)=>a.Length==b.Length&&a.Where((r,i)=>!r.AsSpan().SequenceEqual(b[i])).Any()==false;
static InkStroke[] Word(float x,float y)=>[
 new(new InkPoint[]{new(x,y+90),new(x,y+90),new(x,y+10),new(x+25,y+60),new(x+50,y+10),new(x+50,y+90)}),
 new(new InkPoint[]{new(x+70,y+10),new(x+70,y+90),new(x+120,y+10),new(x+120,y+90)}),
 new(new InkPoint[]{new(x+140,y+90),new(x+140,y+10),new(x+180,y+10),new(x+190,y+20),new(x+190,y+40),new(x+180,y+50),new(x+140,y+50)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
