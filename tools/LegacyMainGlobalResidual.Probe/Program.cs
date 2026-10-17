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
int owners=0,guards=0,merges=0,positive=0;ulong identity=0x1000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
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
    var session=LegacyFreshMainGlobalGlyphAssociationSession.Prepare(plan,geometryOwner);
    var overlay=LegacyFreshMainGlobalResidualSource.Prepare(geometryOwner);var metadata=LegacyFreshMainGlobalGlyphSourceMetadata.Prepare(overlay);
    var chronology=cuts.Chronology;var original=cuts.Preparation.Input.Input.Input.Owner.Controller.Source;
    int total=original.Strokes.Sum(s=>s.Points.Count);var keys=new LegacySourcePointKey[total];var ends=new LegacySourcePointKey[total];
    for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
    {var q=original.Chronology.Resolve(s,p);keys[q.KeyRecordIndex]=q.Key;ends[q.KeyRecordIndex]=original.ResolveEndingMetadata(q).Key;}
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("factor",chronology.SourceFactor);
    w.WriteStartArray("map");foreach(uint tick in chronology.PreparedPointMap.Span)w.WriteNumberValue(tick);w.WriteEndArray();
    w.WriteStartArray("counts");foreach(int count in chronology.StrokePointCounts.Span)w.WriteNumberValue(count);w.WriteEndArray();
    w.WriteStartArray("keys");foreach(var key in keys)Key(key);w.WriteEndArray();w.WriteStartArray("endingKeys");foreach(var key in ends)Key(key);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.State.GetNode(n)){w.WriteStartObject();w.WriteNumber("group",a.GroupId);w.WriteNumber("start",a.StartContextId);w.WriteNumber("end",a.EndContextId);w.WriteNumber("span",a.Span);w.WriteStartArray("next");foreach(var next in a.CopySelector(1))w.WriteNumberValue(next);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");foreach(var group in graph.State.Groups){w.WriteStartArray();foreach(var r in group.CopyRanges())Range(r);w.WriteEndArray();}w.WriteEndArray();
    var refs=Enumerable.Range(0,graph.State.NodeCount).SelectMany(n=>Enumerable.Range(0,graph.State.GetNode(n).Count).Select(a=>Pack(n,a))).ToArray();
    w.WriteStartArray("actualFlushes");
    foreach(var flush in plan.Flushes)
    {
        var result=session.PrepareNext();byte[] after=result.CopyRows72();
        if(!result.Succeeded||result.RemainingCount!=0||!after.AsSpan().SequenceEqual(flush.CopyRows72())||flush.CopyResidualAdditions().Length!=0)throw new Exception("Actual zero-residual flush path changed.");
        Reject(()=>session.PrepareNext());Reject(()=>session.CompleteCurrent(result,false,new(true)));
        w.WriteStartObject();w.WriteBase64String("before",flush.CopyRows72());w.WriteBase64String("after",after);w.WriteNumber("remaining",result.RemainingCount);w.WriteNumber("additions",flush.CopyResidualAdditions().Length);w.WriteEndObject();
        //The callback is PREPARED acknowledgement here, not executed glyph/WORD publication.
        session.CompleteCurrent(result,true);Reject(()=>session.CompleteCurrent(result,true));
    }
    w.WriteEndArray();if(!session.IsComplete||session.CompletedFlushCount!=plan.Flushes.Count)throw new Exception("Owned flush chronology changed.");
    Reject(()=>session.PrepareNext());
    ulong firstHigher=ulong.MaxValue;
    w.WriteStartArray("queries");
    foreach(ulong reference in refs)Inspect(reference,[]);
    for(int n=0;n<96;n++)
    {
        ulong[] inputRefs=Enumerable.Range(0,2+n%5).Select(i=>refs[(n*17+i*11)%refs.Length]).ToArray();
        var result=overlay.MergeSources(inputRefs);if(!result.Succeeded||overlay.MergeSources(inputRefs).Reference!=result.Reference)throw new Exception("Owned merge cache changed.");
        Inspect(result.Reference,inputRefs);merges++;
    }
    w.WriteEndArray();
    if(firstHigher==ulong.MaxValue)throw new Exception("Actual graph controls did not exercise higher source allocation.");
    Reject(()=>overlay.MergeSources([firstHigher,refs[0]]));Reject(()=>overlay.ReadRangeCapacity(refs[0]));
    ulong decorated=firstHigher|0xab00000000000000UL;
    if(overlay.MergeSources([decorated]).Reference!=decorated)throw new Exception("Count1 must preserve all input bits.");
    var changedRanges=overlay.CopySourceRanges(firstHigher);changedRanges[0]=new(65535,0,1);
    if(overlay.CopySourceRanges(firstHigher)[0]==changedRanges[0])throw new Exception("Range snapshot changed owned source.");
    bool checkedBadOriginal=false;
    foreach(ulong reference in refs)
    {
        var sourceRanges=overlay.CopySourceRanges(reference);
        if(sourceRanges.Length!=1||sourceRanges[0].EndPointExclusive-sourceRanges[0].StartPoint<2)continue;
        ulong invalid=reference|(1UL<<40);var merged=overlay.MergeSources([invalid,invalid,invalid,invalid]);
        ulong originalLast=overlay.ResolveBaseReference(merged.Reference);
        if(originalLast==LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(originalLast))continue;
        Reject(()=>metadata.Read(merged.Reference));checkedBadOriginal=true;break;
    }
    if(!checkedBadOriginal)throw new Exception("Noncanonical higher originalLast guard not exercised.");
    w.WriteStartArray("preparedAssociations");
    var actual=plan.Flushes.SelectMany(f=>f.CopyRows72().Chunk(72)).First(r=>r[60]==0);
    for(int n=0;n<8;n++)
    {
        var owned=LegacyFreshMainGlobalResidualSource.Prepare(geometryOwner);byte[] row=actual.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(64),0);row[68]=0;
        ulong sourceRef=Pack(I(row,0),U(row,4));ulong[] inputRefs=Enumerable.Repeat(sourceRef,1+n%3).ToArray();
        var result=owned.Associate(row,inputRefs);byte[] after=result.CopyRows();
        if(!result.Succeeded||result.RemainingCount!=0||after[68]!=1)throw new Exception("Prepared positive real-source association failed.");
        var outMetadata=LegacyFreshMainGlobalGlyphSourceMetadata.Prepare(owned).Read(BinaryPrimitives.ReadUInt64LittleEndian(after.AsSpan(48)));
        w.WriteStartObject();w.WriteBase64String("before",row);w.WriteBase64String("after",after);w.WriteStartArray("input");foreach(ulong r in inputRefs)w.WriteNumberValue(r);w.WriteEndArray();w.WriteStartArray("storage");foreach(ulong r in result.CopyResidualStorage())w.WriteNumberValue(r);w.WriteEndArray();w.WriteNumber("remaining",result.RemainingCount);w.WriteNumber("base",outMetadata.BaseSourceKey);w.WriteBase64String("references24",outMetadata.CopyReferenceStorage24());w.WriteBase64String("residuals2",outMetadata.CopyResidualStorage2());w.WriteEndObject();positive++;
    }
    w.WriteEndArray();Reject(()=>LegacyFreshMainGlobalGlyphAssociationSession.Prepare(plan,LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence)));
    Reject(()=>LegacyFreshMainGlobalResidualSource.Prepare(null!));Reject(()=>LegacyFreshMainGlobalResidualSource.Prepare(geometryOwner,new(true)));
    Reject(()=>overlay.MergeSources([]));Reject(()=>overlay.MergeSources([ulong.MaxValue]));Reject(()=>metadata.Read(1UL<<40));Reject(()=>metadata.Read(0xffffUL<<24));Reject(()=>metadata.Read(0,new(true)));
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Residual/source metadata reran NN.");
    w.WriteNumber("preparedCallbackAcks",session.CompletedFlushCount);w.WriteEndObject();Console.WriteLine($"PASS global residual owner{owners}:actual{session.CompletedFlushCount} zero-residual flushes,96 prepared merges/8 prepared positive associations; NN{evaluations} unchanged.");owners++;
    void Inspect(ulong reference,ulong[] inputRefs)
    {
        var result=metadata.Read(reference);var decoded=LegacyNativeResidualSource.Decode(reference);
        if(result.IsHigherSource&&firstHigher==ulong.MaxValue)firstHigher=reference;
        if(result.BaseSourceKey!=overlay.ResolveBaseReference(reference))throw new Exception("Higher base ownership changed.");
        byte[] allocation=result.CopySourceAllocation26(),sourceBytes=result.CopyReferenceStorage24(),endpointBytes=result.CopyResidualStorage2();
        if(!allocation.AsSpan(0,sourceBytes.Length).SequenceEqual(sourceBytes)||!allocation.AsSpan(sourceBytes.Length).SequenceEqual(endpointBytes))throw new Exception("Source26 allocation layout changed.");
        allocation.AsSpan().Fill(0x55);
        if(!result.CopySourceAllocation26().AsSpan(0,sourceBytes.Length).SequenceEqual(sourceBytes))throw new Exception("Source allocation snapshot changed owned metadata.");
        w.WriteStartObject();w.WriteNumber("target",reference);w.WriteNumber("base",result.BaseSourceKey);w.WriteBoolean("higher",result.IsHigherSource);w.WriteNumber("group",result.AcousticGroupId);
        w.WriteStartArray("input");foreach(ulong r in inputRefs)w.WriteNumberValue(r);w.WriteEndArray();w.WriteStartArray("ranges");foreach(var r in overlay.CopySourceRanges(reference))Range(r);w.WriteEndArray();
        if(result.IsHigherSource)w.WriteNumber("capacity",overlay.ReadRangeCapacity(reference));else w.WriteNull("capacity");
        w.WriteBase64String("references24",result.CopyReferenceStorage24());w.WriteBase64String("residuals2",result.CopyResidualStorage2());w.WriteEndObject();
    }
    void Key(LegacySourcePointKey key){w.WriteStartArray();w.WriteNumberValue(key.First);w.WriteNumberValue(key.Second);w.WriteNumberValue(key.Third);w.WriteEndArray();}
    void Range(LegacyInkRange r){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {owners} actualglobalowners/{merges} prepared merges/{positive} prepared positiveassociations/{guards} guards; actual residuals0, callback acknowledgements prepared only.");
static int I(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
static ushort U(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
static ulong Pack(int node,int arc)=>((uint)node&0xffffffUL)|((ulong)checked((ushort)arc)<<24);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid global residual input accepted.");}
