using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Security.Cryptography;


if(args.Length is not(9 or 11))throw new ArgumentException("AK engine separation case publication line30 line50 capture LK [recorded-manifest word-index]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0;ulong identity=0x1000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
var rawInputs=new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()};
string? trackHash=null;int? recordedIndex=null;
if(args.Length==11)
{
    using var manifest=JsonDocument.Parse(File.ReadAllBytes(args[9]));
    if(manifest.RootElement.GetProperty("commit").GetString()!="c6629026b1fdef51fd08e3d2fddffd82f97e60a5"||
        manifest.RootElement.GetProperty("penUpMetadata").GetString()!="absent")
        throw new InvalidDataException("Pinned explicitly unsegmented recorded input required.");
    int index=int.Parse(args[10]);
    var sample=manifest.RootElement.GetProperty("samples").EnumerateArray().Where(x=>x.GetProperty("kind").GetString()=="word").ElementAt(index);
    string filename=sample.GetProperty("file").GetString()!;
    if(filename!=Path.GetFileName(filename))throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[9]))!,filename));
    trackHash=Convert.ToHexStringLower(SHA256.HashData(bytes));
    if(bytes.Length>200000||trackHash!=sample.GetProperty("trackSha256").GetString())throw new InvalidDataException("Bounded unchanged coordinate track required.");
    using var track=JsonDocument.Parse(bytes);
    var points=track.RootElement.GetProperty("points").EnumerateArray().Select(x=>new InkPoint(x[0].GetSingle(),-x[1].GetSingle())).ToArray();
    if(points.Length is <2 or >4096)throw new InvalidDataException("Bounded actual coordinate vector required.");
    //The recording has no pen-up events. Do not infer them from jumps or read
    //the expected transcription to create a more favorable stroke sequence.
    rawInputs=[new[]{new InkStroke(points)}];recordedIndex=index;
}
foreach(var raw in rawInputs)
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
    byte[] compactResidue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,compactResidue);

    byte[] compact=materialized.CopyCompactStorage();var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);
    var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
    var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);

    var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);
    int guards=0,words=0,emptyCallbacks=0;var readingOrderTexts=new List<byte[]>();
    w.WriteStartObject();w.WriteNumber("owner",owners);
    if(recordedIndex is int ri){w.WriteNumber("recordedIndex",ri);w.WriteString("trackSha256",trackHash);w.WriteString("penUpMetadata","absent");}
    w.WriteStartArray("starts");foreach(int start in materialized.CopyNodeStarts2478())w.WriteNumberValue(start);w.WriteEndArray();
    w.WriteStartArray("graph");for(int node=0;node<graph.State.NodeCount;node++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var group in graph.State.Groups){w.WriteStartArray();foreach(var range in group.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("controls");
    for(int n=0;n<32;n++)
    {
        var ownership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner);
        byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*37+n)).ToArray();
        foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
        var session=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,residue);
        byte[] record=Enumerable.Range(0,40).Select(i=>(byte)(i*17+n)).ToArray();
        ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(i*13+n)).ToArray();
        byte[] request=Enumerable.Range(0,64).Select(i=>(byte)(i*19+n)).ToArray();
        var textsByNode=new Dictionary<LegacyNativePublishedWordNode,byte[]>();
        w.WriteStartArray();
        while(!session.IsComplete)
        {
            var publicationResult=session.ExecuteNext(record,labels,request);var association=publicationResult.Association;
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
    }
    w.WriteEndArray();w.WriteStartArray("readingOrderUtf8");foreach(byte[] text in readingOrderTexts)w.WriteBase64StringValue(text);w.WriteEndArray();
    w.WriteNumber("guards",guards);w.WriteNumber("words",words);w.WriteNumber("emptyCallbacks",emptyCallbacks);w.WriteEndObject();w.Flush();
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Projection reran acoustic networks.");
    void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid UTF8 projection accepted.");}
    Console.WriteLine($"PASS globalUTF8 owner{owners}:actual{words} WORDs/{emptyCallbacks} zero callbacks/{guards} guards; no acceptedtext.");owners++;
}
w.WriteEndArray();w.Flush();
