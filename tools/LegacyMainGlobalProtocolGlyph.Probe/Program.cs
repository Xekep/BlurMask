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
int owners=0,guards=0,actual=0,prepared=0,unsupported=0;ulong identity=0x1000;
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
    var producer=LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(ak,callbacks);
    var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);byte[] backtrack=walk.CopyRows();
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("nodeCount",graph.State.NodeCount);w.WriteBase64String("compact",compact);w.WriteBase64String("actualRows72",backtrack);
    w.WriteStartArray("headers");foreach(uint h in owner.Symbols.CopySourceHeaders())w.WriteNumberValue(h);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.Flags);w.WriteNumberValue(arc.AuxiliaryContextIndex);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("contexts");foreach(var c in graph.Source.State.Contexts){w.WriteStartArray();foreach(int x in c.CopyControls())w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("cases");
    for(int i=0;i<backtrack.Length/72;i++)if(backtrack[i*72+60]==3)
        foreach(int cursor in new[]{0,1,7,256})Capture(backtrack.AsSpan(i*72,72).ToArray(),cursor,0,0,true);
    for(int n=0;n<graph.State.NodeCount;n++)for(ushort a=0;a<graph.State.GetNode(n).Count;a++)
        foreach(ushort symbol in new ushort[]{0xfffa,0xfffb,0xfffc})foreach(int cursor in new[]{0,1,7,256})
        {
            byte[] row=new byte[72];W(row,0,n);H(row,4,a);W(row,12,n);H(row,16,a);W(row,24,n);H(row,28,a);W(row,32,-1);W(row,36,-1);H(row,40,65535);H(row,56,74);H(row,58,symbol);row[60]=3;
            Capture(row,cursor,symbol==0xfffb?2u:0u,0,false);
        }
    w.WriteEndArray();
    if(owners==0)
    {
        w.WriteStartArray("preparedPendingCursorControls");
        foreach(int cursor in new[]{0,1,7,256})foreach(int count in new[]{1,2,255,256,257,4096})
        {
            var scratch=Scratch(cursor);int remaining=cursor==0?0:cursor-count,written=cursor==0?0:Math.Min(cursor,count);
            LegacyMainProtocolGlyphProductionKernel.ApplyDelimiter(scratch,new(remaining,0,Enumerable.Repeat((ushort)0xfff9,written).ToArray()),default);
            if(scratch.RemainingCursor!=remaining)throw new Exception("Prepared pending cursor lost native underflow.");
            ushort[] pendingLabels=scratch.CopyLabels();int observedWrites=pendingLabels.Count(x=>x==0xfff9);
            if(observedWrites!=written)throw new Exception("Prepared native pending writes were lost.");
            bool refused=false;if(remaining<0)
            {
                var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);ulong r=pool.AppendDescriptor(1,0,false,new byte[40]).Reference;
                try{_=LegacyPreparedGlyphFinalization.Apply(pool,r,new byte[72],scratch.Snapshot(),false);}catch(NotSupportedException){refused=true;}
                if(!refused)throw new Exception("Unsafe negative-cursor finalization accepted.");
            }
            w.WriteStartArray();w.WriteNumberValue(cursor);w.WriteNumberValue(count);w.WriteNumberValue(scratch.RemainingCursor);w.WriteNumberValue(observedWrites);w.WriteBooleanValue(refused);w.WriteStartArray();foreach(ushort x in pendingLabels)w.WriteNumberValue(x);w.WriteEndArray();w.WriteEndArray();
        }w.WriteEndArray();
    }
    Reject(()=>LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(new byte[ak.Length],callbacks));
    Reject(()=>LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(ak,callbacks,new(true)));
    var guardScratch=Scratch(256);
    Reject(()=>producer.Produce(new byte[71],compact,guardScratch,0,0,new byte[64]));
    Reject(()=>producer.Produce(new byte[72],compact,guardScratch,0,0,new byte[64]));
    Reject(()=>producer.Produce(new byte[72],compact,guardScratch,0,0,new byte[64],new(true)));
    if(guardScratch.RemainingCursor!=256||guardScratch.ComponentFlag!=7||guardScratch.ExternalVariant!=9||guardScratch.Score0CBits!=17||guardScratch.Cost14!=23)
        throw new Exception("Pre-entry protocol guard mutated scratch.");
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Protocol production reran NN.");w.WriteEndObject();Console.WriteLine($"PASS global protocol owner{owners}, NN{evaluations} unchanged.");owners++;
    void Capture(byte[] row,int cursor,uint ac,uint b0,bool actualRow)
    {
        byte[] residue=Enumerable.Range(0,64).Select(i=>(byte)(i*17+23)).ToArray();var scratch=Scratch(cursor);bool succeeded=true;byte[] request=[];
        try{request=producer.Produce(row,compact,scratch,ac,b0,residue).CopyRequest();}catch(NotSupportedException){succeeded=false;unsupported++;}
        w.WriteStartObject();w.WriteBoolean("actualRow",actualRow);w.WriteBase64String("row72",row);w.WriteNumber("cursor",cursor);w.WriteNumber("ac",ac);w.WriteNumber("b0",b0);w.WriteBoolean("succeeded",succeeded);w.WriteBase64String("request64",request);
        w.WriteNumber("remaining",scratch.RemainingCursor);w.WriteNumber("score0C",scratch.Score0CBits);w.WriteNumber("score10",scratch.Score10Bits);w.WriteNumber("cost14",scratch.Cost14);w.WriteNumber("component",scratch.ComponentFlag);w.WriteNumber("external",scratch.ExternalVariant);w.WriteNumber("flags06",scratch.PublicationFlags06);
        w.WriteStartArray("labels");foreach(ushort x in scratch.CopyLabels())w.WriteNumberValue(x);w.WriteEndArray();w.WriteEndObject();if(actualRow)actual++;else prepared++;
    }
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {owners} owners/{actual} actual-backtrack-row controls/{prepared} explicitly prepared protocol rows/{unsupported} bounded unsupported callbacks/{guards} guards; packet scalars still prepared.");
static LegacyOrdinaryGlyphProductionScratch Scratch(int cursor)=>new(Enumerable.Range(0,256).Select(i=>(ushort)(0x8000+i)).ToArray(),cursor,7,9,17,19,23,29);
static void W(Span<byte> b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
static void H(Span<byte> b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b[at..],value);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid protocol scope accepted.");}
