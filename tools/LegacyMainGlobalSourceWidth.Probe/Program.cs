using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

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
int owners=0,guards=0,produced=0;ulong identity=0x1000;
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
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,null!,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,[]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,new byte[41]));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(new byte[ak.Length],selected,confidence,residue));
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue,new(true)));
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue);
    Reject(()=>LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue));

    byte[] compact=materialized.CopyCompactStorage();var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);
    var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
    var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);
    var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);var width=LegacyFreshMainGlobalSourceWidthStatistics.Prepare(plan,geometryOwner);
    var ordinary=LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(ak,callbacks);var protocol=LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(ak,callbacks);
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("count",width.Statistics.Count);w.WriteNumber("sum",width.Statistics.WidthSum);w.WriteNumber("topAC",width.TopAcBits);w.WriteNumber("topB0Bits",width.TopB0Bits);w.WriteNumber("metadataEvents",plan.SourceMetadataEvents.Count);
    w.WriteBase64String("compact",compact);w.WriteNumber("recordCount",materialized.RecordCount);w.WriteStartArray("flushes");
    foreach(var flush in plan.Flushes)
    {
        byte[] rows72=flush.CopyRows72();w.WriteStartObject();w.WriteBase64String("rows72",rows72);w.WriteStartArray("bounds");
        for(int i=0;i<rows72.Length/72;i++){var row=rows72.AsSpan(i*72,72);var b=geometryOwner.ReadBounds(I(row,0),U(row,4));w.WriteStartArray();w.WriteNumberValue(b.MinimumX);w.WriteNumberValue(b.MaximumX);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("producers");
        for(int i=rows72.Length/72-1;i>=0;i--)
        {
            byte[] row=rows72.AsSpan(i*72,72).ToArray();var scratch=new LegacyOrdinaryGlyphProductionScratch(Enumerable.Range(0,256).Select(n=>(ushort)(0x8000+n)).ToArray(),256);
            byte[] request=[];
            if(row[60]==0)_=ordinary.Produce(row,scratch,0,compact);
            else if(row[60]==3)request=protocol.Produce(row,compact,scratch,width.TopAcBits,width.TopB0Bits,new byte[64]).CopyRequest();
            else throw new NotSupportedException("Additional actual glyph subtype remains unported.");
            int remaining=scratch.RemainingCursor;ushort emitted=remaining==256?ushort.MaxValue:remaining>=0?scratch.CopyLabels()[remaining]:throw new NotSupportedException("Unsafe native scratch read.");
            w.WriteStartObject();w.WriteNumber("ordinal",i);w.WriteNumber("subtype",row[60]);w.WriteNumber("label",U(row,58));w.WriteNumber("remaining",remaining);w.WriteNumber("emitted",emitted);w.WriteNumber("score0C",scratch.Score0CBits);w.WriteNumber("score10",scratch.Score10Bits);w.WriteNumber("cost14",scratch.Cost14);w.WriteBase64String("request64",request);w.WriteEndObject();produced++;
        }w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();
    Reject(()=>LegacyFreshMainGlobalSourceWidthStatistics.Prepare(null!,geometryOwner));Reject(()=>LegacyFreshMainGlobalSourceWidthStatistics.Prepare(plan,null!));Reject(()=>LegacyFreshMainGlobalSourceWidthStatistics.Prepare(plan,geometryOwner,new(true)));
    var cloneGeometry=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);Reject(()=>LegacyFreshMainGlobalSourceWidthStatistics.Prepare(plan,cloneGeometry));
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Width/prepared glyph production reran NN.");w.WriteEndObject();Console.WriteLine($"PASS width owner{owners}:count{width.Statistics.Count},B0={width.Statistics.AverageWidth},actualglyphproducer rowsNN{evaluations} unchanged.");owners++;
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {owners} genuine global width prepasses/{produced} actualbacktrack glyphproducer inputs/{guards} guards; WORD/stack allocator residue andfinalpublication separate.");
static int I(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
static ushort U(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid width owner accepted.");}
