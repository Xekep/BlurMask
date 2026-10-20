using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
using System.Security.Cryptography;
if(args.Length is not(2 or 3))throw new ArgumentException("MODELS CAPTURE [RECORDED-MANIFEST]");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
if(args.Length==3){var recorded=RecordedInputs.Load(args[2]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));}
int guards=0,success=0,refused=0,foreignGuards=0,directStringGuards=0;var captures=new List<byte[]>();byte[] failurePrefix=[];
foreach(var (name,raw) in samples)
{
    var document=LegacyOwnedRawInkDocument.Create(raw,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(document),["a","b"]);original.Run();
    var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();
    using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);
    string stage="inference";
    try
    {
        var result=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
        stage="V68";var completed=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);
        stage="choices";var choices=LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope,completed,ak,pd,norm,circles);
        if(!ReferenceEquals(choices,LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope,completed,ak,pd,norm,circles))||!ReferenceEquals(choices.StringPool,scope.Context.Strings))throw new Exception("PreNN3E/owner changed.");
        Reject(()=>choices.Publish(new byte[39],new byte[1024]));Reject(()=>choices.Publish(new byte[40],new byte[1023]));Reject(()=>choices.Publish(new byte[40],new byte[1024],new(true)));
        byte[] bad=ak.ToArray();bad[^1]^=1;Reject(()=>LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope,completed,bad,pd,norm,circles));
        if(name=="M")
        {
            //Actual foreign invocation sharing the SAME immutable Document.
            var foreignOriginal=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(document),["x"]);foreignOriginal.Run();
            var foreignInput=LegacyLibraryOriginalSelectedFieldContext.Prepare(foreignOriginal,foreignOriginal.State.Inputs[0]);foreignInput.Run();
            using var foreignScope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,foreignInput,0,table,cd,pd);
            foreignScope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
            var foreignCompletion=foreignScope.CompleteSources(new byte[400],new byte[4],new byte[4]);
            try{_=LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope,foreignCompletion,ak,pd,norm,circles);throw new Exception("Foreign same-Document completion accepted.");}catch(ArgumentException){foreignGuards++;}
            var projector=new LegacyFreshMainGlobalGlyphProjectionContext(scope,completed,default);
            try{_=projector.Project(foreignCompletion.Source.Session!.Associations.Metadata.Read(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(foreignCompletion.Word.Word.FlatReferences[0])),default);throw new Exception("Foreign same-Document V140 accepted.");}catch(ArgumentException){foreignGuards++;}
        }
        var refs=completed.Word.Word.FlatReferences.ToArray();var pool=result.Session!.Ownership.GlyphPool;
        var before=refs.Select(r=>pool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(r))).ToArray();
        byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray();
        var output=choices.Publish(residue,new byte[2040*4]);Reject(()=>choices.Publish(residue,new byte[2040*4]));
        if(output.Count!=refs.Length)throw new Exception("Original WORD order changed.");
        if(output.Any(g=>!ReferenceEquals(g.SourcePositions.Projection,output[0].SourcePositions.Projection)))throw new Exception("Global glyphs cloned the shared original input map.");
        var glyphs=new List<byte[]>();int index=0;
        foreach(var glyph in output)
        {
            var source=glyph.SourcePositions.Source;var node=pool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(glyph.OriginalReference));
            if(glyph.OriginalReference!=refs[index]||!ReferenceEquals(glyph.SourcePositions.Document,document))throw new Exception("Reference/document changed.");
            var rows=glyph.Published.Records.Select(r=>new{utf8=Convert.ToHexStringLower(r.Text.ReadBytes()),flags=r.Flags,count=r.ByteCount,intern=r.Text.Identity}).ToArray();
            glyphs.Add(Json(gw=>{
                gw.WriteStartObject();gw.WriteNumber("reference",glyph.OriginalReference);gw.WriteString("before",Convert.ToHexStringLower(before[index++].Records.Span));gw.WriteString("owner",Convert.ToHexStringLower(node.Records.Span));gw.WriteString("residue",Convert.ToHexStringLower(residue));
                gw.WriteString("source24",Convert.ToHexStringLower(source.CopyReferenceStorage24()));gw.WriteString("residual2",Convert.ToHexStringLower(source.CopyResidualStorage2()));gw.WriteNumber("beforeMarker",glyph.Selection.SelectedMarker!.Value);gw.WriteString("beforeSelections",Convert.ToHexStringLower(glyph.Selection.CopySelectionRows4()));gw.WriteNumber("afterMarker",glyph.Published.AdjustedMarkerIndex);gw.WriteString("retainedSelections",Convert.ToHexStringLower(glyph.Published.RetainedSelectionRows.Span));
                gw.WriteStartArray("published");foreach(var row in rows){gw.WriteStartObject();gw.WriteString("utf8",row.utf8);gw.WriteNumber("flags",row.flags);gw.WriteNumber("count",row.count);gw.WriteNumber("intern",row.intern);gw.WriteEndObject();}gw.WriteEndArray();
                if(glyph.CopySelectedUtf8() is{} text)gw.WriteString("selected",Convert.ToHexStringLower(text));else gw.WriteNull("selected");gw.WriteStartArray("positions");foreach(var range in glyph.SourcePositions.CopyRanges()){gw.WriteStartArray();gw.WriteNumberValue(range.First.Value);gw.WriteNumberValue(range.Last.Value);gw.WriteEndArray();}gw.WriteEndArray();gw.WriteEndObject();
            }));
            var defensive=glyph.SourcePositions.CopyRanges();defensive[0]=default;if(glyph.SourcePositions.CopyRanges()[0]==default)throw new Exception("Source ranges exposed mutable storage.");
        }
        int nn=result.Selection.Publication.Policy.Owner.AcousticEvaluations;if(nn!=result.CachedMainGroupEvaluations)throw new Exception("Extra NN.");
        captures.Add(Json(cw=>{cw.WriteStartObject();cw.WriteString("name",name);cw.WriteString("stage","complete");cw.WriteNumber("nn",nn);cw.WriteStartArray("rawCounts");foreach(var stroke in raw)cw.WriteNumberValue(stroke.Points.Count);cw.WriteEndArray();cw.WriteStartArray("glyphs");foreach(var g in glyphs)cw.WriteRawValue(g);cw.WriteEndArray();cw.WriteEndObject();}));success++;
        scope.Dispose();Reject(()=>output[0].CopySelectedUtf8());Reject(()=>output[0].SourcePositions.CopyRanges());
        try{_=output[0].Published.Records[0].Text.ReadBytes();throw new Exception("Direct published UTF8 access survived scope disposal.");}catch(ObjectDisposedException){directStringGuards++;}
    }
    catch(Exception ex)when(ex is NotSupportedException or InvalidOperationException or ArgumentException)
    {
        if(!name.EndsWith(".json",StringComparison.Ordinal))throw;
        captures.Add(Json(cw=>{cw.WriteStartObject();cw.WriteString("name",name);cw.WriteString("stage",stage);cw.WriteString("refused",ex.GetType().Name);cw.WriteString("message",ex.Message);cw.WriteStartArray("rawCounts");foreach(var stroke in raw)cw.WriteNumberValue(stroke.Points.Count);cw.WriteEndArray();cw.WriteEndObject();}));refused++;
    }
}
//Genuine entered failed-selection prefix; retry stays poisoned, shared lease remains live.
{
    var d=LegacyOwnedRawInkDocument.Create([M()],default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a"]);original.Run();var input=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[0]);input.Run();using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,input,0,table,cd,pd);
    scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var completion=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);var choices=LegacyFreshMainGlobalTextGlyphChoices.Prepare(scope,completion,ak,pd,norm,circles);
    var pool=completion.Source.Session!.Ownership.GlyphPool;ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(completion.Word.Word.FlatReferences[0]);byte[] before=pool.CopyOwnerNode(key).Records.ToArray();byte[] word=completion.Word.Word.CopyStorage();int strings=scope.Context.Strings.Count;int nn=completion.Source.CachedMainGroupEvaluations;
    Reject(()=>choices.Publish(new byte[40],[]));if(!choices.IsFaulted)throw new Exception("Entered failure not poisoned.");byte[] after=pool.CopyOwnerNode(key).Records.ToArray();Reject(()=>choices.Publish(new byte[40],new byte[2040*4]));scope.Validate();
    if(after.Length<=before.Length||!after.AsSpan(0,before.Length).SequenceEqual(before)||!pool.CopyOwnerNode(key).Records.Span.SequenceEqual(after)||!completion.Word.Word.CopyStorage().AsSpan().SequenceEqual(word)||scope.Context.Strings.Count!=strings||completion.Snapshot.SourcePolicy.Owner.AcousticEvaluations!=nn)throw new Exception("Failed selection prefix lost order or mutated WORD/string/NN.");
    failurePrefix=Json(w=>{w.WriteStartObject();w.WriteString("before",Convert.ToHexStringLower(before));w.WriteString("after",Convert.ToHexStringLower(after));w.WriteNumber("strings",strings);w.WriteNumber("nn",nn);w.WriteBoolean("faulted",choices.IsFaulted);w.WriteEndObject();});
}
File.WriteAllBytes(args[1],Json(w=>{w.WriteStartObject();w.WriteNumber("success",success);w.WriteNumber("refused",refused);w.WriteNumber("guards",guards);w.WriteNumber("foreignSameDocumentGuards",foreignGuards);w.WriteNumber("directStringLifetimeGuards",directStringGuards);w.WritePropertyName("enteredSelectionFailure");w.WriteRawValue(failurePrefix);w.WriteStartArray("samples");foreach(var sample in captures)w.WriteRawValue(sample);w.WriteEndArray();w.WriteEndObject();}));
Console.WriteLine($"PASS {success} actual publications/{refused} explicit recorded refusals/{guards} guards; same preNN3E, no CHAR/TEXT acceptance.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported publication accepted.");}
static byte[] Json(Action<Utf8JsonWriter> write){using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);write(writer);writer.Flush();return stream.ToArray();}
