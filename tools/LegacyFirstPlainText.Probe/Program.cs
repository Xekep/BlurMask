using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("AK engine separation capture");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] raw=[Word(0),[Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],[Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))],
 Word(0).Concat(Word(450)).ToArray(),Word(0).Concat(Word(700)).ToArray(),Hi(0).Concat(Hi(220)).ToArray()];
int guards=0,realWords=0,delimiterWords=0,multiple=0;
using var output=File.Create(args[3]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("samples");
for(int sample=0;sample<raw.Length;sample++)
{
    var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw[sample],activations,table);var run=owner.RunText();var text=run.Text;
    if(!ReferenceEquals(text.Owner,owner)||!ReferenceEquals(text.Result.Factory,owner.Factory)||!ReferenceEquals(text.Graph,owner.Prepared.SourceRanges.Graph))throw new Exception("Actual TEXT ownership lost.");
    int ordinary=text.Words.Count(x=>!x.IsDelimiter);realWords+=ordinary;delimiterWords+=text.Words.Count(x=>x.IsDelimiter);if(ordinary>1)multiple++;
    var grid=new LegacyFirstMergedRecordGrid(run.Contexts.ColumnCountB8,run.Contexts.BeamWidth,run.Contexts.CopyB8Storage());var trace=LegacyFirstSelectedGlyphTrace.Read(grid,text.Selection);
    if(sample<4)
    {
        var strict=owner.Run().Materialized;
        if(ordinary!=1||text.Words.Count!=1||!text.Words[0].Published.Word.CopyStorage().AsSpan().SequenceEqual(strict.PublishedWord!.Word.CopyStorage())||
            text.Words[0].Published.Span!=strict.PublishedWord.Span||text.MinimumOwnerFirstScore80!=strict.MinimumOwnerFirstScore80)
            throw new Exception("Existing single-word materialization/scalars/span changed through TEXT.");
        if(!text.AllocatedGlyphReferences.SequenceEqual(strict.GlyphReferences))throw new Exception("Existing reading glyph references changed through TEXT.");
        foreach(ulong reference in text.AllocatedGlyphReferences)
            if(!text.Result.GlyphPool.CopyRecord(reference).AsSpan().SequenceEqual(strict.Result.GlyphPool.CopyRecord(reference)))throw new Exception("Existing glyph40 values changed through TEXT.");
    }
    else
    {
        if(ordinary<2)throw new Exception("Actual selected wide-gap first path did not publish multiple words.");
        try{owner.Run();throw new Exception("Strict old endpoint silently widened to multiword.");}catch(NotSupportedException e)when(e.Message.Contains("single-word path")){guards++;}
    }
    try{owner.RunText(new CancellationToken(true));throw new Exception("Canceled TEXT accepted");}catch(OperationCanceledException){guards++;}
    w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteNumber("cost",text.Selection.TotalCost);w.WriteNumber("acoustic",text.Selection.AcousticCost);w.WriteNumber("nodes",grid.NodeCount);
    w.WriteStartArray("b8");foreach(var row in run.Contexts.CopyB8Storage())w.WriteBase64StringValue(row.Span);w.WriteEndArray();
    w.WriteStartArray("arcs");for(int n=0;n<text.Graph.NodeCount;n++){w.WriteStartArray();foreach(var arc in text.Graph.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteNumber("terminalRow",text.Selection.Row);w.WriteNumber("terminalColumn",text.Selection.Column);w.WriteNumber("columns",run.Contexts.ColumnCountB8);w.WriteNumber("beam",run.Contexts.BeamWidth);
    w.WriteStartArray("words");foreach(var word in text.Words)
    {
        w.WriteStartObject();w.WriteBoolean("delimiter",word.IsDelimiter);w.WriteNumber("delta",word.TotalCostDelta);w.WriteNumber("acousticDelta",word.AcousticCostDelta);w.WriteNumber("span",word.SourceNodeSpan);
        w.WriteNumber("first",word.Published.Span.First);w.WriteNumber("last",word.Published.Span.Last);w.WriteBase64String("storage",word.Published.Word.CopyStorage());
        w.WriteStartArray("glyphs");foreach(ulong r in word.Published.Word.FlatReferences)
        {w.WriteStartObject();w.WriteString("reference",r.ToString("X16"));w.WriteBase64String("record",text.Result.GlyphPool.CopyRecord(r));w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteStartArray("detached");foreach(ulong r in text.DetachedDelimiterReferences){w.WriteStartObject();w.WriteString("reference",r.ToString("X16"));w.WriteBase64String("record",text.Result.GlyphPool.CopyRecord(r));w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
    Console.WriteLine($"Actual first sample{sample}: ordinaryWords{ordinary}/delimiterWords{text.Words.Count-ordinary}, cost{text.Selection.TotalCost}; no expected text supplied.");
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("realWords",realWords);w.WriteNumber("delimiterWords",delimiterWords);w.WriteNumber("multipleInputs",multiple);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS selected actualfirst TEXT:7 inputs/{realWords} words/{delimiterWords} delimiter words/{multiple} multiword inputs/{guards} guards; unchanged4 strict single-word materializations. No Russian accuracy claim.");
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static InkStroke[] Word(float x)=>[
 Draw((x,90),(x,10),(x+25,60),(x+50,10),(x+50,90)),Draw((x+65,90),(x+65,10)),Draw((x+65,90),(x+115,10)),Draw((x+115,90),(x+115,10)),
 Draw((x+130,90),(x+130,10),(x+175,10),(x+185,20),(x+185,45),(x+175,55),(x+130,55))];
static InkStroke[] Hi(float x)=>[Draw((x,10),(x,90)),Draw((x+40,10),(x+40,90)),Draw((x,50),(x+40,50)),Draw((x+70,10),(x+70,90))];
