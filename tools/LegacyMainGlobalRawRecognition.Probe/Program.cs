using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Security.Cryptography;


if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
var shared=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);
LegacyRussianModelBundle? firstModel=null;int entryGuards=0;
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("owners");
var rawInputs=new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()};
foreach(var raw in rawInputs)
{
    LegacyFreshMainGlobalPlainRecognitionResult RunVariant(int n)
    {
        var prepared=shared.PrepareGlobal(raw,0,table,cd,pd);
        byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*37+n)).ToArray();
        foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
        byte[] record=Enumerable.Range(0,40).Select(i=>(byte)(i*17+n)).ToArray();
        ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(i*13+n)).ToArray();
        byte[] request=Enumerable.Range(0,64).Select(i=>(byte)(i*19+n)).ToArray();
        byte[] compact=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
        RejectEntry(()=>prepared.Run(compact,new byte[119],record,labels,request,new byte[72]));
        RejectEntry(()=>prepared.Run(compact,residue,record,labels,request,new byte[72],new(true)));
        var result=prepared.Run(compact,residue,record,labels,request,new byte[72]);
        RejectEntry(()=>prepared.Run(compact,residue,record,labels,request,new byte[72]));
        if(!result.HasWinner||!result.PublicationCompleted||result.Session is null)throw new Exception("Actual bounded context fixture did not complete WORD callbacks.");
        var model=result.Selection.Publication.Policy.Owner.Model;
        firstModel??=model;if(!ReferenceEquals(model,firstModel))throw new Exception("Context reparsed/replaced its immutable model.");
        if(result.CachedMainGroupEvaluations!=result.Selection.Publication.Policy.Owner.AcousticEvaluations)throw new Exception("Wrapped publication reevaluated groups.");
        return result;
    }
    var first=RunVariant(0);var pass=first.Selection.Publication.PassResult;
    var graph=pass.Policy.Owner.Graph;var owner=pass.Policy.Owner;int evaluations=first.CachedMainGroupEvaluations;
    int guards=0,words=0,emptyCallbacks=0;var readingOrderTexts=new List<byte[]>();
    w.WriteStartObject();w.WriteNumber("owner",owners);
    w.WriteStartArray("starts");foreach(int start in first.Selection.Publication.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<graph.State.NodeCount;node++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var group in graph.State.Groups){w.WriteStartArray();foreach(var range in group.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<4;n++)
    {
        var result=n==0?first:RunVariant(n);var session=result.Session!;var ownership=session.Ownership;
        var textsByNode=new Dictionary<LegacyNativePublishedWordNode,byte[]>();
        w.WriteStartArray();
        foreach(var publicationResult in result.Publications)
        {
            var association=publicationResult.Association;
            if(!publicationResult.Succeeded)throw new Exception("Actual CASE word callback failed.");
            var projection=LegacyFreshMainGlobalWordUtf8Projection.Project(publicationResult);
            if(!ReferenceEquals(projection.Publication,publicationResult))throw new Exception("Projection lost owning callback.");
            byte[] snapshot=projection.CopyUtf8();var copy=projection.CopyUtf8();
            if(copy.Length>0){copy[0]^=255;if(copy.AsSpan().SequenceEqual(projection.CopyUtf8()))throw new Exception("UTF8 escaped snapshot.");}
            if(!System.Text.Encoding.UTF8.GetBytes(projection.Text).AsSpan().SequenceEqual(snapshot))throw new Exception("Text changed raw UTF8.");
            w.WriteStartObject();w.WriteBase64String("associatedRows",association.CopyRows72());w.WriteBase64String("utf8",snapshot);w.WriteString("text",projection.Text);
            if(publicationResult.PublishedWord is {} node)
            {
                words++;textsByNode.Add(node,snapshot);w.WriteBase64String("word120",node.Word.CopyStorage());
                w.WriteStartArray("references");foreach(ulong reference in node.Word.FlatReferences)w.WriteNumberValue(reference);w.WriteEndArray();
                w.WriteStartArray("pool");foreach(ulong reference in node.Word.FlatReferences){w.WriteStartObject();w.WriteNumber("reference",reference);w.WriteBase64String("record",node.Word.Pool.CopyRecord(reference));w.WriteEndObject();}w.WriteEndArray();
            }
            else{emptyCallbacks++;w.WriteNull("word120");}
            w.WriteStartArray("glyphs");foreach(var glyph in projection.Glyphs)
            {
                w.WriteStartObject();w.WriteNumber("reference",glyph.Reference);w.WriteNumber("key",glyph.CanonicalSourceKey);w.WriteBoolean("protocol",glyph.IsProtocolSource);
                if(glyph.OriginalGraphSourceKey is ulong original)w.WriteNumber("original",original);else w.WriteNull("original");
                w.WriteNumber("kindFlags",glyph.Descriptor.KindFlags);w.WriteNumber("symbol",glyph.Descriptor.SymbolOrRunLength);
                w.WriteBase64String("record",glyph.CopyRecord40());w.WriteBase64String("utf8",glyph.CopyUtf8());
                w.WriteStartArray("rows");foreach(int row in glyph.CopyAssociatedRowOrdinals())w.WriteNumberValue(row);w.WriteEndArray();
                w.WriteStartArray("ranges");foreach(var range in glyph.CopySampledSourceRanges()){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
                byte[] mutation=glyph.CopyRecord40();mutation[8]^=255;if(mutation.AsSpan().SequenceEqual(glyph.CopyRecord40()))throw new Exception("Glyph record escaped snapshot.");
                var rowCopy=glyph.CopyAssociatedRowOrdinals();rowCopy[0]=-1;if(glyph.CopyAssociatedRowOrdinals()[0]==-1)throw new Exception("Glyph row map escaped.");
                var ranges=glyph.CopySampledSourceRanges();if(ranges.Length>0){ranges[0]=new(999,0,1);if(glyph.CopySampledSourceRanges()[0].StrokeIndex==999)throw new Exception("Glyph range escaped.");}
            }
            w.WriteEndArray();w.WriteEndObject();
            Reject(()=>LegacyFreshMainGlobalWordUtf8Projection.Project(null!));Reject(()=>LegacyFreshMainGlobalWordUtf8Projection.Project(publicationResult,new(true)));

            //The typed published-word snapshot can be read after acknowledgement;
            //no new winning label or mutable working WORD is substituted.
            if(!LegacyFreshMainGlobalWordUtf8Projection.Project(publicationResult).CopyUtf8().AsSpan().SequenceEqual(snapshot))throw new Exception("Completed callback projection changed.");
        }
        w.WriteEndArray();
        using var finalBytes=new MemoryStream();
        for(var node=ownership.WordPublisher.Head;node is not null;node=node.Next)finalBytes.Write(textsByNode[node]);
        readingOrderTexts.Add(finalBytes.ToArray());
        using var wrapped=new MemoryStream();foreach(var word in result.RawWordsInNativeHeadOrder)wrapped.Write(word.CopyUtf8());
        if(!wrapped.ToArray().AsSpan().SequenceEqual(finalBytes.ToArray()))throw new Exception("Wrapped WORD head order differs from actual tokens.");
    }
    w.WriteEndArray();w.WriteStartArray("readingOrderUtf8");foreach(byte[] text in readingOrderTexts)w.WriteBase64StringValue(text);w.WriteEndArray();
    w.WriteNumber("guards",guards);w.WriteNumber("words",words);w.WriteNumber("emptyCallbacks",emptyCallbacks);w.WriteEndObject();w.Flush();
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Projection reran acoustic networks.");
    void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid UTF8 projection accepted.");}
    Console.WriteLine($"PASS globalUTF8 owner{owners}:actual{words} WORDs/{emptyCallbacks} zero callbacks/{guards} guards; no acceptedtext.");owners++;
}
w.WriteEndArray();
var longRaw=Enumerable.Range(0,31).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray();
var noWinner=shared.PrepareGlobal(longRaw,0,table,cd,pd).Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
if(noWinner.HasWinner||noWinner.PublicationCompleted||noWinner.Session is not null||noWinner.Publications.Count!=0||noWinner.RawWordsInNativeHeadOrder.Count!=0)throw new Exception("NoWinner manufactured a WORD session.");
w.WriteStartObject("noWinner");w.WriteNumber("nodes",noWinner.Selection.Publication.NodeCount);w.WriteNumber("compactRecords",noWinner.Selection.Publication.RecordCount2470);w.WriteBoolean("hasWinner",noWinner.HasWinner);w.WriteNumber("groups",noWinner.CachedMainGroupEvaluations);w.WriteEndObject();
w.WriteNumber("entryGuards",entryGuards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS reusable context/32 actual invocations/{entryGuards} single-use/preflight guards and explicit noWinner; no accepted text.");
void RejectEntry(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){entryGuards++;return;}throw new Exception("Expected raw invocation preflight/single-use refusal.");}
