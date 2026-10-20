using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length is not(3 or 4))throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE [PREPARED-CONTROLS]");
if(args.Length==4)PreparedControls.Run(args[3]);
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));
var captures=new List<byte[]>();int guards=0,success=0,refused=0,totalReuse=0;
foreach(var (name,raw) in samples)
{
    using var scope=Scope(raw);var result=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
    var completion=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);
    var wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,completion,ak,pd,norm,circles);
    if(name=="M")
    {
        //Independent invocation over the SAME immutable original Document.
        var other=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(scope.Document),["x"]);other.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(other,other.State.Inputs[0]);selected.Run();
        using var foreign=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);foreign.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var fc=foreign.CompleteSources(new byte[400],new byte[4],new byte[4]);
        Reject(()=>LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,fc,ak,pd,norm,circles));Reject(()=>LegacyFreshMainGlobalTextSourceWrapper.Prepare(foreign,completion,ak,pd,norm,circles));
    }
    long next=scope.SharedState.Selections.NextSelectionId;var pool=result.Session!.Ownership.GlyphPool;
    ulong[] refs=completion.Word.Word.FlatReferences.ToArray();var before=refs.Select(r=>pool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(r)).Records.ToArray()).ToArray();
    Reject(()=>wrapper.Publish(new byte[39],new byte[1024]));Reject(()=>wrapper.Publish(new byte[40],new byte[1023]));Reject(()=>wrapper.Publish(new byte[40],new byte[1024],new(true)));
    byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray();
    try
    {
        var output=wrapper.Publish(residue,new byte[2040*4]);Reject(()=>wrapper.Publish(residue,new byte[2040*4]));
        var glyphs=new List<byte[]>();foreach(var g in output.Glyphs)
        {
            var source=g.ProcessedSource;var meta=source.OriginalPositions.Source;var p=g.Payload;totalReuse+=g.Reused?1:0;
            glyphs.Add(Json(w=>{
                w.WriteStartObject();w.WriteNumber("index",source.WordIndex);w.WriteNumber("reference",source.Reference);w.WriteNumber("node",p.Node.Identity);w.WriteBoolean("reused",g.Reused);w.WriteBoolean("ordinary",source.IsOrdinarySource);w.WriteNumber("group",meta.AcousticGroupId);
                w.WriteString("before",Hex(before[source.WordIndex]));w.WriteString("owner",Hex(pool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(source.Reference)).Records.Span));w.WriteString("descriptorBeforeRefresh",Hex(wrapper.CopyDescriptorSnapshots()[source.WordIndex]));w.WriteString("source24",Hex(meta.CopyReferenceStorage24()));w.WriteString("residual2",Hex(meta.CopyResidualStorage2()));
                w.WriteNumber("beforeMarker",g.Selection.SelectedMarker!.Value);w.WriteString("selection4",Hex(g.Selection.CopySelectionRows4()));w.WriteNumber("marker",g.Marker);w.WriteString("selected",Hex(g.CopySelectedUtf8()));
                Ranges(w,"positions",source.OriginalPositions.CopyRanges());Ranges(w,"processed",source.CopyRanges());Payload(w,p);w.WriteEndObject();
            }));
        }
        captures.Add(Json(w=>{
            w.WriteStartObject();w.WriteString("name",name);w.WriteString("stage","complete");w.WriteString("residue",Hex(residue));w.WriteNumber("nextBefore",next);w.WriteNumber("nextAfter",scope.SharedState.Selections.NextSelectionId);w.WriteNumber("nn",completion.Source.CachedMainGroupEvaluations);w.WriteNumber("sourceFlags",output.SourceFlags);w.WriteNumber("selectedFlags",output.SelectedFlags);
            w.WriteStartArray("rawCounts");foreach(var stroke in raw)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();Ranges(w,"original",wrapper.CopyOriginalDomain());Ranges(w,"wordClip",wrapper.CopyWordClip());Ranges(w,"ordinaryRanges",output.CopyOrdinaryRanges());
            w.WriteString("word26",Hex(completion.CopySourceStorage26()));w.WriteStartArray("sourceDescriptors");foreach(var d in completion.Snapshot.SourcePolicy.Owner.Graph.Source.Source.CopyDescriptors())w.WriteStringValue(Hex(d));w.WriteEndArray();
            w.WriteStartArray("groups");foreach(var group in completion.Snapshot.SourcePolicy.Owner.Graph.State.Groups)w.WriteNumberValue(group.DescriptorOrdinal);w.WriteEndArray();
            w.WriteStartArray("glyphs");foreach(var g in glyphs)w.WriteRawValue(g);w.WriteEndArray();Tree(w,wrapper);Commands(w,scope);Trace(w,wrapper);w.WriteEndObject();
        }));success++;
        var first=output.Glyphs.FirstOrDefault();var direct=first?.Payload.CopyRecords()[0].Text;scope.Dispose();if(first is not null){Reject(()=>first.CopySelectedUtf8());Reject(()=>first.ProcessedSource.CopyRanges());Reject(()=>first.Payload.CopyRecords());Reject(()=>direct!.ReadBytes());}
    }
    catch(Exception ex)when(ex is NotSupportedException or InvalidOperationException or ArgumentException)
    {
        if(!name.EndsWith(".json",StringComparison.Ordinal))throw;
        captures.Add(Json(w=>{w.WriteStartObject();w.WriteString("name",name);w.WriteString("stage","refused");w.WriteString("type",ex.GetType().Name);w.WriteString("message",ex.Message);Trace(w,wrapper);w.WriteEndObject();}));refused++;
    }
}
var prefixes=new List<byte[]>();for(int failure=1;failure<=4;failure++)
{
    using var scope=Scope([M()]);scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);var wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles);long next=scope.SharedState.Selections.NextSelectionId;
    Reject(()=>wrapper.PublishPreparedCharFailure(new byte[40],new byte[8160],failure));Reject(()=>wrapper.Publish(new byte[40],new byte[8160]));
    prefixes.Add(Json(w=>{w.WriteStartObject();w.WriteNumber("preparedCharFailure",failure);w.WriteBoolean("faulted",wrapper.IsFaulted);w.WriteNumber("nextBefore",next);w.WriteNumber("nextAfter",scope.SharedState.Selections.NextSelectionId);w.WriteStartArray("payloads");foreach(var p in wrapper.CopyPayloadPrefix()){w.WriteStartObject();Payload(w,p);w.WriteEndObject();}w.WriteEndArray();Tree(w,wrapper);Commands(w,scope);Trace(w,wrapper);w.WriteEndObject();}));
}
//Prepared traversal controls do not create production processed receipts.
var priorInputs=LegacyOwnedRawInkDocument.Create([M(),M()],default).CopySelectedInputs();
if(LegacyFreshMainGlobalTextSourceWrapper.VisitFreshPrior([],priorInputs,default)!=(1,0)||
    LegacyFreshMainGlobalTextSourceWrapper.VisitFreshPrior([new(new(0,1,0),new(0,0,0),0,0)],priorInputs,default)!=(1,0)||
    LegacyFreshMainGlobalTextSourceWrapper.VisitFreshPrior([new(new(0,0,0),new(0,1,0),0,0)],priorInputs,default)!=(0,2))
    throw new Exception("Prepared empty/reversed/positive C4A traversal differs.");
byte[] skipControl;
{
    using var scope=Scope([M()]);scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);var wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles);
    //Explicit post-inference descriptor mutation, not a genuine OCR winner.
    var pool=c.Source.Session!.Ownership.GlyphPool;ulong reference=c.Word.Word.FlatReferences[0];byte[] row=pool.CopyRecord(reference);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),1);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),7);pool.ReplaceRecord(reference,row);
    var output=wrapper.Publish(new byte[40],new byte[8160]);if(output.Glyphs.Count!=0||wrapper.CharacterInterner.Count!=0||scope.SharedState.Selections.CopyPendingSelections().Count!=0||output.SourceFlags!=0x200000)throw new Exception("Prepared actual-route skip entered second pass.");
    skipControl=Json(w=>{w.WriteStartObject();w.WriteString("label","prepared post-inference kind1/run7");w.WriteNumber("sourceFlags",output.SourceFlags);Trace(w,wrapper);w.WriteEndObject();});
}
//Explicit prepared interner injection: must refuse BEFORE claim/refresh/selection.
{
    using var scope=Scope([M()]);scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);var wrapper=LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles);wrapper.CharacterInterner.InternPrepared(wrapper.CopyOriginalDomain());Reject(()=>wrapper.Publish(new byte[40],new byte[8160]));if(wrapper.CopyTrace().Length!=0||wrapper.IsFaulted)throw new Exception("Prepared node crossed preclaim guard.");
}
File.WriteAllBytes(args[2],Json(w=>{w.WriteStartObject();w.WriteNumber("success",success);w.WriteNumber("refused",refused);w.WriteNumber("guards",guards);w.WriteNumber("actualReuse",totalReuse);w.WriteNumber("preparedPriorCases",3);w.WritePropertyName("preparedSkip");w.WriteRawValue(skipControl);w.WriteStartArray("samples");foreach(var c in captures)w.WriteRawValue(c);w.WriteEndArray();w.WriteStartArray("preparedCharPrefixes");foreach(var p in prefixes)w.WriteRawValue(p);w.WriteEndArray();w.WriteEndObject();}));
Console.WriteLine($"PASS actual{success}/refused{refused}/actualReuse{totalReuse}/guards{guards}; preparedCHARprefix4; no WORD/TEXT/acceptance.");
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var o=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);o.Run();var input=LegacyLibraryOriginalSelectedFieldContext.Prepare(o,o.State.Inputs[1]);input.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,input,0,table,cd,pd);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported publication accepted.");}
static void Payload(Utf8JsonWriter w,LegacyFreshMainGlobalTextSourcePayload p){var s=p.CopyAssignedScalars();w.WriteNumber("written",(int)p.Written);w.WriteBoolean("complete",p.IsComplete);w.WriteNumber("prior",p.PriorByte);w.WriteNumber("priorVisits",p.PriorVisits);w.WriteNumber("priorRangeCount",p.PriorRangeCount);w.WriteStartArray("scalars");foreach(long? v in new long?[]{s.SelectionId,s.Field08,s.Field40,s.Field00,s.Field1C,s.Field64,s.Field48,s.SourceLineId,s.Flags}){if(v is{} n)w.WriteNumberValue(n);else w.WriteNullValue();}w.WriteEndArray();w.WriteStartArray("records");foreach(var row in p.CopyRecords()){w.WriteStartObject();w.WriteString("scalar40",Hex(row.CopyStorage40()));w.WriteNumber("intern",row.Text!.Identity);w.WriteString("utf8",Hex(row.Text.ReadBytes()));w.WriteEndObject();}w.WriteEndArray();}
static void Commands(Utf8JsonWriter w,LegacyLibraryGlobalTextPublicationScope scope){w.WriteStartArray("commands");foreach(var c in scope.SharedState.Selections.CopyPendingSelections()){w.WriteStartObject();w.WriteNumber("id",c.SelectionId);w.WriteString("name",c.Name);w.WriteNumber("command",c.CommandIdentity);w.WriteNumber("previous",c.PreviousCommandIdentity);w.WriteNumber("operation",c.OperationCode);w.WriteNumber("enabled",c.Enabled);w.WriteNumber("category",c.Category);Ranges(w,"ranges",c.CopyRanges());w.WriteEndObject();}w.WriteEndArray();}
static void Tree(Utf8JsonWriter w,LegacyFreshMainGlobalTextSourceWrapper wrapper){w.WriteStartArray("tree");foreach(var n in wrapper.CharacterInterner.CopyTree()){w.WriteStartArray();foreach(int x in new[]{n.Identity,n.Left,n.Right,n.Balance,n.PreviousInserted,n.NextInserted})w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("insertion");foreach(var n in wrapper.CharacterInterner.CopyInsertionOrder())w.WriteStringValue(Hex(n.CopyRanges16()));w.WriteEndArray();}
static void Trace(Utf8JsonWriter w,LegacyFreshMainGlobalTextSourceWrapper wrapper){w.WriteStartArray("trace");foreach(string e in wrapper.CopyTrace())w.WriteStringValue(e);w.WriteEndArray();}
static void Ranges(Utf8JsonWriter w,string name,IEnumerable<LegacyNativeInkPositionRange> ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
static string Hex(ReadOnlySpan<byte> bytes)=>Convert.ToHexStringLower(bytes);
static byte[] Json(Action<Utf8JsonWriter> write){using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);write(writer);writer.Flush();return stream.ToArray();}
