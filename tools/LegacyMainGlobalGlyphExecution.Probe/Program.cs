using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;


if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0,guards=0,executed=0,published=0,zero=0;ulong identity=0x1000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("owners");
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
    byte[] residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue);

    byte[] compact=materialized.CopyCompactStorage();var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);
    var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
    var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);

    var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);
    var ordinary=LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(ak,callbacks);var protocol=LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(ak,callbacks);
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteBase64String("compact",compact);w.WriteNumber("recordCount",materialized.RecordCount);w.WriteStartArray("starts");foreach(int n in materialized.CopyNodeStarts2478())w.WriteNumberValue(n);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteNumber("nnEvaluations",evaluations);
    w.WriteStartArray("cases");
    for(int seed=0;seed<4;seed++)
    {
        var ownership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner);
        byte[] allocator=Allocator(seed),record=Enumerable.Range(0,40).Select(i=>(byte)(i*17+seed*23+3)).ToArray(),request=Enumerable.Range(0,64).Select(i=>(byte)(i*11+seed*29)).ToArray();
        ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(0x9000+i*13+seed)).ToArray();
        var execution=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,allocator);
        w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteNumber("ac",execution.Width.TopAcBits);w.WriteNumber("b0",execution.Width.TopB0Bits);
        w.WriteBase64String("allocator120",allocator);w.WriteBase64String("initial120",execution.CopyInitialWordStorage());w.WriteBase64String("recordResidue40",record);w.WriteStartArray("flushes");
        while(!execution.IsComplete)
        {
            int ordinal=execution.CompletedFlushCount;var flush=plan.Flushes[ordinal];byte[] before=execution.CopyWorkingWordStorage();
            Reject(()=>execution.ExecuteNext(new byte[39],labels,request));Reject(()=>execution.ExecuteNext(record,new ushort[255],request));Reject(()=>execution.ExecuteNext(record,labels,new byte[63]));Reject(()=>execution.ExecuteNext(record,labels,request,new(true)));
            if(!before.AsSpan().SequenceEqual(execution.CopyWorkingWordStorage()))throw new Exception("Prewrite guard changed WORD.");
            var result=execution.ExecuteNext(record,labels,request);if(!result.Succeeded||execution.IsStopped)throw new Exception("Actual global callback did not complete.");
            Reject(()=>execution.Associations.CompleteExecuted(result,default));
            w.WriteStartObject();w.WriteNumber("ordinal",ordinal);w.WriteBase64String("before120",before);w.WriteBase64String("template64",flush.CopyPacketWriteTemplate());w.WriteStartArray("origins");foreach(int origin in flush.CopyInitialWordSeedOrigins())w.WriteNumberValue(origin);w.WriteEndArray();
            w.WriteBase64String("packet64",result.Packet.CopyStorage());w.WriteBase64String("rows72",result.Association.CopyRows72());w.WriteNumber("span",result.SymbolSpan);w.WriteBoolean("succeeded",result.Succeeded);
            w.WriteBase64String("completed120",result.CopyCompletedWordStorage120());w.WriteBase64String("reset120",execution.CopyWorkingWordStorage());
            if(result.Chain is not{} chain)throw new Exception("Actual successful association skipped chain.");
            w.WriteNumber("firstNew",chain.FirstNewReference);w.WriteNumber("last",chain.LastProcessedReference);w.WriteNumber("created",chain.CreatedCount);w.WriteNumber("reused",chain.ReusedCount);
            w.WriteStartArray("producerScalars");byte[] rows72=result.Association.CopyRows72();
            for(int i=rows72.Length/72-1;i>=0;i--)
            {
                byte[] row=rows72.AsSpan(i*72,72).ToArray();var scratch=new LegacyOrdinaryGlyphProductionScratch(labels,256);LegacyPreparedGlyphFinalizationInput finalized;
                if(row[60]==0)finalized=ordinary.Produce(row,scratch,0,compact);
                else if(row[60]==3)finalized=protocol.Produce(row,compact,scratch,execution.Width.TopAcBits,execution.Width.TopB0Bits,request).Finalization;
                else throw new NotSupportedException("Additional selected subtype remains explicit.");
                w.WriteStartObject();w.WriteNumber("ordinal",i);w.WriteNumber("remaining",scratch.RemainingCursor);w.WriteNumber("component",scratch.ComponentFlag);w.WriteNumber("external",scratch.ExternalVariant);w.WriteNumber("score0C",scratch.Score0CBits);w.WriteNumber("score10",scratch.Score10Bits);w.WriteNumber("cost14",scratch.Cost14);w.WriteNumber("publication",finalized.PublicationFlags06);w.WriteStartArray("labels");foreach(ushort label in scratch.CopyLabels())w.WriteNumberValue(label);w.WriteEndArray();w.WriteEndObject();
            }w.WriteEndArray();Nodes("pool",ownership.GlyphPool.CopyOwnerNodes());
            if(result.PublishedWord is{} node)
            {
                w.WriteStartObject("published");w.WriteNumber("first",node.Span.First);w.WriteNumber("last",node.Span.Last);w.WriteNumber("index",node.WordIndex);w.WriteBase64String("word120",node.Word.CopyStorage());w.WriteStartArray("flat");foreach(ulong r in node.Word.FlatReferences)w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();published++;
            }
            else {w.WriteNull("published");if(result.SymbolSpan!=0)throw new Exception("Positive span not published.");zero++;}
            w.WriteEndObject();executed++;
        }
        Reject(()=>execution.ExecuteNext(record,labels,request));
        w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();
    //Real public misuse after packet write must stop with its written prefix.
    var failed=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner),plan,Allocator(9));
    _=failed.Associations.PrepareNext();Reject(()=>failed.ExecuteNext(new byte[40],new ushort[256],new byte[64]));
    if(!failed.IsStopped)throw new Exception("Exception after packet did not stop execution.");
    w.WriteBase64String("failedInitial120",failed.CopyInitialWordStorage());w.WriteBase64String("failedAfterPacket120",failed.CopyWorkingWordStorage());
    w.WriteBase64String("failedTemplate64",plan.Flushes[0].CopyPacketWriteTemplate());w.WriteStartArray("failedOrigins");foreach(int origin in plan.Flushes[0].CopyInitialWordSeedOrigins())w.WriteNumberValue(origin);w.WriteEndArray();
    Reject(()=>failed.ExecuteNext(new byte[40],new ushort[256],new byte[64]));
    var foreignOwnership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence));
    Reject(()=>LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,foreignOwnership,plan,Allocator(2)));
    var leased=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner),plan,Allocator(12));
    byte[] leasedBefore=leased.CopyWorkingWordStorage();
    Reject(()=>owner.AdvanceNodeEpoch(graph.State.NodeCount));Reject(()=>leased.ExecuteNext(new byte[40],new ushort[256],new byte[64]));
    if(!leasedBefore.AsSpan().SequenceEqual(leased.CopyWorkingWordStorage()))throw new Exception("Invalid result lease changed WORD before its write boundary.");
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Global WORD execution reran NN.");w.WriteEndObject();Console.WriteLine($"PASS globalglyph owner{owners}:real callbacks4variants/NN{evaluations} unchanged.");owners++;
}
w.WriteEndArray();w.WriteStartArray("preparedOutcomes");
for(int seed=0;seed<16;seed++)for(int mode=0;mode<10;mode++)
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);byte[] allocated=Allocator(seed);var word=LegacyNativeWordBuilder.CreateEmpty(pool,allocated);
    byte[] packetBytes=Enumerable.Range(0,64).Select(i=>(byte)(i*13+seed)).ToArray();var packet=new LegacyPreparedMainWordPublicationPacket(packetBytes);word.ApplyMainPublicationPacket(packet);
    bool positive=mode is 3 or 4 or 5 or 6 or 7 or 9;
    if(positive){var insertion=word.AppendGlyph(0,7,false,new byte[40]);pool.SetOrdinarySymbol(insertion.Reference,22);}
    byte[] before=word.CopyStorage();var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);var log=new List<string>();
    LegacyMainPlainWordCallbackOutcome? outcome=null;bool exception=false;
    try
    {
        outcome=LegacyMainPlainWordCallbackKernel.Execute(word,()=>{log.Add("entry");if(mode is 0 or 9)return false;log.Add("chain");if(mode==6)throw new NotSupportedException("Prepared chain exception");return mode is 2 or 4 or 5 or 7 or 8;},()=>{log.Add("publish");if(mode==7)throw new NotSupportedException("Prepared publisher exception");return mode==4?null:publisher.Publish(word,(n,a)=>101,(n,a)=>202);},mode==8?new(true):default);
    }
    catch(Exception e)when(e is NotSupportedException or OperationCanceledException){exception=true;}
    w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteNumber("mode",mode);w.WriteBase64String("before120",before);w.WriteBase64String("after120",word.CopyStorage());w.WriteBoolean("exception",exception);w.WriteStartArray("log");foreach(string s in log)w.WriteStringValue(s);w.WriteEndArray();
    if(outcome is not null){w.WriteBoolean("success",outcome.Succeeded);w.WriteNumber("span",outcome.Span);w.WriteBoolean("published",outcome.Published is not null);w.WriteBase64String("completed120",outcome.CompletedWord);}w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {owners} actualglobalowners/{executed} callbacks/{published} published/{zero} zero-span;160 separatepreparedcallbackoutcomes/{guards} guards. No acceptedtext/quote/finalnormalization claim.");
static byte[] Allocator(int seed){var r=Enumerable.Range(0,120).Select(i=>(byte)(i*19+seed*31+3)).ToArray();foreach(var(offset,length)in new[]{(32,8),(88,12),(104,12)})r.AsSpan(offset,length).Clear();return r;}
void Nodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var node in nodes){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid global execution input accepted.");}
