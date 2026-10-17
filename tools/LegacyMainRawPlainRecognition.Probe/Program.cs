using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var c=File.OpenRead(args[4]);using var p=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(c);var publication=LegacyNativeTextPublicationData.Load(p);
byte[] l30=File.ReadAllBytes(args[6]),l50=File.ReadAllBytes(args[7]);
var samples=new[]{Word(0,0),Word(0,250).Concat(Word(0,0)).ToArray(),Word(0,0).Concat(Word(230,0)).ToArray(),
    Word(0,0).Concat(Word(300,0)).ToArray(),Word(0,0).Concat(Word(450,0)).ToArray(),Word(0,0).Concat(Word(700,0)).ToArray()};
int completed=0,rejectedLines=0,guards=0,words=0;
using var output=File.Create(args[8]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("inputs");
for(int i=0;i<samples.Length;i++)
{
    var owner=Prepare(samples[i]);
    Reject(()=>owner.Run(new byte[119],new byte[40],new ushort[256],new byte[64]));
    Reject(()=>owner.Run(new byte[120],new byte[39],new ushort[256],new byte[64]));
    Reject(()=>owner.Run(new byte[120],new byte[40],new ushort[255],new byte[64]));
    Reject(()=>owner.Run(new byte[120],new byte[40],new ushort[256],new byte[63]));
    try{owner.Run(new byte[120],new byte[40],new ushort[256],new byte[64],new CancellationToken(true));throw new Exception("Cancellation ignored");}
    catch(OperationCanceledException){guards++;}
    w.WriteStartObject();w.WriteNumber("input",i);
    if(i==1)
    {
        var lineResult=owner.Input.Run();
        if(lineResult.Lines.CopyGuides().Length!=2)throw new Exception("Genuine two-line scope fixture changed");
        try{owner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);throw new Exception("Multiple lines flattened into single-line source");}
        catch(NotSupportedException e)when(e.Message.StartsWith("One actual assigned ordinary descriptor required")){rejectedLines++;}
        w.WriteBoolean("multipleLinesRefused",true);w.WriteEndObject();continue;
    }
    var result=owner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
    if(!result.PublicationCompleted||result.IntermediatePublicationCount!=0||!ReferenceEquals(result.Owner,owner)||!result.Terminal.Winner.HasWinner||
        !ReferenceEquals(result.Session.Ownership.Materialization.TerminalResult,result.Terminal)||
        !ReferenceEquals(result.Sampled.Owner.Preparation,result.Preprocessed.Smoothed)||
        result.Publications.Any(r=>!r.Succeeded||!ReferenceEquals(r.Association.Owner,result.Session)))
        throw new Exception("Raw-to-WORD pipeline lost its actual source/session/result ownership");
    var poolBefore=new Dictionary<ulong,byte[]>();
    foreach(var ownerNode in result.Session.Ownership.GlyphPool.CopyOwnerNodes())
        for(int j=0;j<ownerNode.Records.Length/40;j++)poolBefore[ownerNode.Key|((ulong)j<<40)]=ownerNode.Records.Span.Slice(j*40,40).ToArray();
    byte[] leading=[0xc1,0x4e,0xa5,0x73],trailing=[0xd2,0x5f,0xb6,0x84];
    Reject(()=>LegacyOwnedMainPublishedQuoteNormalization.Apply(result,new byte[3],trailing));
    Reject(()=>LegacyOwnedMainPublishedQuoteNormalization.Apply(result,leading,new byte[3]));
    Reject(()=>LegacyOwnedMainPublishedQuoteNormalization.Apply(result,leading,leading));
    try{LegacyOwnedMainPublishedQuoteNormalization.Apply(result,leading,trailing,new CancellationToken(true));throw new Exception("Quote cancellation ignored");}
    catch(OperationCanceledException){guards++;}
    LegacyOwnedMainPublishedQuoteNormalization.Apply(result,leading,trailing);
    w.WriteString("quoteLeading",Convert.ToHexString(leading));w.WriteString("quoteTrailing",Convert.ToHexString(trailing));
    completed++;w.WriteNumber("firstWords",result.Preprocessed.FirstResults.Sum(r=>r.Text.Words.Count(x=>!x.IsDelimiter)));
    w.WriteNumber("source720",result.Preprocessed.Lines.PublishedRows.Count);
    w.WriteNumber("selectedCost",result.Terminal.Winner.TotalCost);
    w.WriteStartArray("graph");var graph=result.Sampled.SourceRanges.Graph;
    for(int n=0;n<graph.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("words");
    for(var node=result.Session.Ownership.WordPublisher.Head;node is not null;node=node.Next)
    {
        words++;var associated=result.Publications.Single(r=>ReferenceEquals(r.PublishedWord,node)).Association;
        if(!ReferenceEquals(node.Word.Pool,result.Session.Ownership.GlyphPool))throw new Exception("Foreign published glyph pool");
        w.WriteStartObject();w.WriteString("storage",Convert.ToHexString(node.Word.CopyStorage()));
        w.WriteString("packet",Convert.ToHexString(associated.Packet.CopyStorage()));
        w.WriteStartArray("span");w.WriteNumberValue(node.Span.First);w.WriteNumberValue(node.Span.Last);w.WriteEndArray();
        w.WriteStartArray("glyphs");foreach(ulong reference in node.Word.FlatReferences)
        {w.WriteStartObject();w.WriteNumber("reference",reference);w.WriteString("record",Convert.ToHexString(node.Word.Pool.CopyRecord(reference)));w.WriteString("recordBeforeQuotes",Convert.ToHexString(poolBefore[reference]));w.WriteEndObject();}w.WriteEndArray();
        w.WriteEndObject();
    }
    w.WriteEndArray();
    //Explicitly PREPARED unrelated publisher append tests the owner guard;
    //it is performed only after the actual inference capture above.
    var extra=LegacyNativeWordBuilder.CreateEmpty(result.Session.Ownership.GlyphPool,new byte[120]);
    extra.AppendGlyph(0x7f0000,0,true,new byte[40]);extra.SetLastOrdinarySymbol(0);
    if(result.Session.Ownership.WordPublisher.Publish(extra,(_,_)=>0,(_,_)=>0) is null)
        throw new Exception("Prepared unrelated publisher guard fixture failed");
    Reject(()=>LegacyOwnedMainPublishedQuoteNormalization.Apply(result,leading,trailing));
    w.WriteBoolean("unrelatedWordRefused",true);w.WriteEndObject();
}
w.WriteEndArray();guards+=PreparedPublishedQuoteControls.Write(w,ak);w.WriteNumber("completed",completed);w.WriteNumber("rejectedLines",rejectedLines);w.WriteNumber("guards",guards);w.WriteNumber("words",words);w.WriteEndObject();w.Flush();
if(completed!=5||rejectedLines!=1||guards!=57||words<5)throw new Exception("Raw integration scope or guard count changed");
Console.WriteLine($"PASS {completed} actual raw-to-first-CASE WORD pipelines/{words} words/{guards} guards; genuine two-line refusal. No full alternatives or text-accuracy assertion.");

LegacyFreshMainSingleLinePlainRecognition Prepare(InkStroke[] raw)=>LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication);
void Reject(Action action){try{action();}catch(ArgumentException){guards++;return;}throw new Exception("Incomplete scratch accepted");}
InkStroke[] Word(float x,float y)=>[Draw((x,y+80),(x,y),(x+30,y+45),(x+60,y),(x+60,y+80)),
    Draw((x+85,y+80),(x+85,y),(x+135,y),(x+135,y+80)),
    Draw((x+165,y+80),(x+165,y),(x+215,y),(x+215,y+40),(x+165,y+40))];
InkStroke Draw(params(float X,float Y)[] knots)
{
    var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++)
    {float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}
    points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());
}
