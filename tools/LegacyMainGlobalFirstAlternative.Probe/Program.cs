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
int owners=0,guards=0,snapshots=0;ulong identity=0x1000;
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
    var ownership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner);
    var execution=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,new byte[120]);
    var tokens=new List<LegacyFreshMainGlobalGlyphExecutionResult>();
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[400]));
    while(!execution.IsComplete)
    {
        var result=execution.ExecuteNext(new byte[40],new ushort[256],new byte[64]);
        if(!result.Succeeded)throw new Exception("Actual callback failure cannot be replaced by an alternative.");
        tokens.Add(result);
    }
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(null!,execution,tokens,new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,null!,tokens,new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,null!,new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[399]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[401]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(terminal.Select(pass),execution,tokens,new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,[],new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens.Append(tokens[0]).ToArray(),new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens.AsEnumerable().Reverse().ToArray(),new byte[400]));
    var missing=tokens.ToArray();missing[0]=null!;
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,missing,new byte[400]));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[400],new(true)));
    var foreign=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,new byte[120]);
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,foreign,tokens,new byte[400]));
    var head=ownership.WordPublisher.Head;
    string poolBefore=PoolState(ownership.GlyphPool);
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("nodes",pass.NodeCount);
    w.WriteNumber("winnerOrdinal",selected.Winner.Ordinal);w.WriteNumber("total",selected.Winner.TotalCost);w.WriteNumber("rowCost14",selected.Winner.RowCost14);
    w.WriteBase64String("selectedBeam",selected.CopySelectedRecord());
    int wordCount=0;for(var node=head;node is not null;node=node.Next)wordCount++;
    w.WriteNumber("words",wordCount);w.WriteNumber("callbacks",tokens.Count);w.WriteStartArray("cases");
    for(int seed=0;seed<64;seed++)
    {
        byte[] allocation=Enumerable.Range(0,400).Select(i=>(byte)(i*17+seed*29+7)).ToArray();
        var snapshot=LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,allocation);
        if(!ReferenceEquals(snapshot.WordHead18,head)||!ReferenceEquals(snapshot.BeamOwner20,materialized)||snapshot.PublishedWordCount!=wordCount)
            throw new Exception("Snapshot lost actual publisher/beam identity.");
        w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteBase64String("residue",allocation);
        w.WriteBase64String("constructed",snapshot.CopyConstructorStorage400());w.WriteBase64String("scored",snapshot.CopyScoredStorage400());
        w.WriteBase64String("normalized",snapshot.CopyNormalizedScoreStorage400());w.WriteBase64String("publishedBeam",snapshot.CopySelectedBeam88());
        w.WriteNumber("reference",snapshot.Reference78);w.WriteNumber("rawFirst08",BitConverter.SingleToInt32Bits(snapshot.RawFirstMass08));
        w.WriteNumber("top80",BitConverter.SingleToInt32Bits(snapshot.NormalizedTop80));w.WriteEndObject();snapshots++;
    }
    w.WriteEndArray();w.WriteEndObject();
    if(!ReferenceEquals(head,ownership.WordPublisher.Head)||poolBefore!=PoolState(ownership.GlyphPool))
        throw new Exception("Read-only snapshot changed live publisher/glyph data.");
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Snapshot reran NN.");
    Reject(()=>owner.AdvanceNodeEpoch(graph.State.NodeCount));
    Reject(()=>LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[400]));
    Console.WriteLine($"PASS global alternative owner{owners}:64 explicit400B snapshots/actual{tokens.Count} successful callbacks/NN{evaluations} unchanged.");owners++;
}
w.WriteEndArray();w.WriteNumber("ownerCount",owners);w.WriteNumber("snapshots",snapshots);w.WriteNumber("guards",guards);w.WriteEndObject();
Console.WriteLine($"PASS{owners} actualglobalowners/{snapshots} postcallback snapshots/{guards} guards; no quote rewrite, acceptance or accuracy claim.");
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}
    throw new Exception("Expected bounded ownership/chronology rejection.");
}

string PoolState(LegacyNativeGlyphRecordPool pool)=>string.Join(";",pool.CopyOwnerNodes().Select(n=>$"{n.Key:X16}:{n.PackedCount:X4}:{Convert.ToHexString(n.Records.Span)}"));
