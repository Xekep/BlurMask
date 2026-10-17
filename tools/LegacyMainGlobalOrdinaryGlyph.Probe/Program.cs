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
int owners=0,groups=0,controls=0,guards=0,partial=0;ulong identity=0x1000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var glyphGeometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,glyphGeometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);
    var geometry=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometry);
    _=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] b=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(b);return new(b,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var producer=LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(ak,callbacks);var scoring=producer.Scoring;
    byte[] requestBefore=scoring.CreateRequest(0,0,0,false);
    if(!MemoryMarshal.TryGetArray(owner.Symbols.Geometry.CategoryOwner.Categories.StartCompositeMap,out var exposedMap))throw new Exception("Recoverable map fixture unavailable.");
    int savedMap=exposedMap.Array![exposedMap.Offset];exposedMap.Array[exposedMap.Offset]=-1;
    try{if(!scoring.CreateRequest(0,0,0,false).AsSpan().SequenceEqual(requestBefore))throw new Exception("Exposed source map changed private glyph scoring.");}
    finally{exposedMap.Array[exposedMap.Offset]=savedMap;}
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteStartArray("points");foreach(var stroke in cuts.CopyStrokes()){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");
    for(int g=0;g<graph.State.Groups.Count;g++)
    {
        var endpoint=Enumerable.Range(0,graph.State.NodeCount).SelectMany(n=>graph.State.GetNode(n).Select((a,i)=>(node:n,arc:(ushort)i,group:a.GroupId))).First(x=>x.group==g);
        var actual=confidence.ReadGroup(g);var frame=glyphGeometry.ReadFrame(g);var pair=graph.ResolveGroupSource(g);var candidates=actual.Candidates.ToArray();
        w.WriteStartObject();w.WriteNumber("group",g);w.WriteNumber("node",endpoint.node);w.WriteNumber("arc",endpoint.arc);w.WriteNumber("height",frame.Height);
        w.WriteNumber("primaryBaseline",pair.Primary.Baseline);w.WriteNumber("primaryHeight",pair.Primary.Height);if(pair.Secondary is{} secondary){w.WriteNumber("secondaryBaseline",secondary.Baseline);w.WriteNumber("secondaryHeight",secondary.Height);}
        w.WriteNumber("geometry",costs.ReadGeometryCost(g));w.WriteStartArray("ranges");foreach(var r in graph.State.Groups[g].CopyRanges()){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
        w.WriteNumber("sourceProbabilityBits",BitConverter.SingleToUInt32Bits(actual.FirstStoredSourceProbability));w.WriteStartArray("probabilities");for(int family=0;family<4;family++){w.WriteStartArray();foreach(int probability in actual.CopyFamilyProbabilities(family))w.WriteNumberValue(probability);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("final");foreach(int value in actual.CopyFinalClassCosts())w.WriteNumberValue(value);w.WriteEndArray();
        w.WriteStartArray("candidates");foreach(var c in candidates){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToUInt32Bits(c.Confidence));w.WriteEndArray();}w.WriteEndArray();
        if(scoring.ReadFirstCandidateScore(endpoint.node,endpoint.arc)!=(candidates.Length==0?0:candidates[0].Confidence))throw new Exception("Native first-vector channel changed.");
        w.WriteStartArray("requests");
        foreach(ushort label in new ushort[]{0,178,179,180,181,182,187,188,189,191,193,194,65535})for(int mode=0;mode<3;mode++)
        {
            byte[] request=new byte[16];W(request,0,endpoint.node);H(request,4,endpoint.arc);H(request,6,65535);H(request,8,label);request[10]=0;request[11]=(byte)(mode==1?1:0);request[12]=(byte)(mode==2?1:0);
            int cost=scoring.ReadRequestCost(request);w.WriteStartArray();w.WriteNumberValue(label);w.WriteNumberValue(mode);w.WriteNumberValue(cost);w.WriteEndArray();
        }w.WriteEndArray();
        w.WriteStartArray("cases");
        foreach(ushort label in candidates.Select(c=>c.Symbol).Concat(new ushort[]{178,179,189,191,193,194,0xfff7,0xfff8,0xfff9}).Distinct())for(int mode=0;mode<4;mode++)
        {
            byte[] row=new byte[72];W(row,0,endpoint.node);H(row,4,endpoint.arc);W(row,32,mode<2?-1:0);H(row,58,label);row[61]=(byte)(mode&1);row[62]=new byte[]{0,1,127,255}[mode];
            byte[] compact=mode<2?[]:new byte[40];int cursor=mode==3?1:256;ulong conversion=new ulong[]{0,1,256,257}[mode];
            var scratch=new LegacyOrdinaryGlyphProductionScratch(Enumerable.Range(0,256).Select(i=>(ushort)(0x8000+i)).ToArray(),cursor,7,9,0x7fc12345,0x80000000,-123456789,0xa55a);
            bool success=true;try{_=producer.Produce(row,scratch,conversion,compact);}catch(Exception e)when(e is NotSupportedException or ArgumentException){success=false;partial++;}
            ushort[] labels=scratch.CopyLabels();ushort emitted=labels[cursor-1];for(int i=0;i<256;i++)if(i!=cursor-1&&labels[i]!=(ushort)(0x8000+i))throw new Exception("Ordinary producer wrote outside native scratch slot.");
            int requestGeometry=-1;if(success&&label!=178&&unchecked((ushort)(label+9))>5){ushort scoreLabel=label==193?(ushort)134:label==194?(ushort)129:label;requestGeometry=scoring.CreateRequest(endpoint.node,endpoint.arc,scoreLabel,(mode&1)!=0)[10];}
            w.WriteStartArray();foreach(long value in new long[]{label,mode,(long)conversion,success?1:0,emitted,scratch.RemainingCursor,scratch.Score0CBits,scratch.Score10Bits,scratch.Cost14,scratch.ComponentFlag,scratch.ExternalVariant,scratch.PublicationFlags06,requestGeometry})w.WriteNumberValue(value);w.WriteEndArray();controls++;
        }w.WriteEndArray();w.WriteEndObject();groups++;
    }w.WriteEndArray();
    byte[] valid=new byte[72];W(valid,32,-1);H(valid,58,178);var scratchGuard=new LegacyOrdinaryGlyphProductionScratch(new ushort[256],256);
    Reject(()=>producer.Produce(new byte[71],scratchGuard));valid[60]=3;Reject(()=>producer.Produce(valid,scratchGuard));valid[60]=0;
    Reject(()=>producer.Produce(valid,scratchGuard,token:new(true)));Reject(()=>scoring.ReadRequestCost(new byte[15]));Reject(()=>scoring.CreateRequest(-1,0,0,false));
    byte[] bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(bad,callbacks));Reject(()=>LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(ak,callbacks,new(true)));
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Glyph production recalculated NN.");
    Reject(()=>owner.AdvanceNodeEpoch(graph.State.NodeCount));Reject(()=>scoring.ReadSymbolScore(0,0,0));
    w.WriteEndObject();Console.WriteLine($"PASS global ordinary owner{owners}:NN{evaluations} unchanged.");owners++;
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS {owners} owners/{groups}actual NN groups/{controls} prepared72 controls/{partial} bounded partial refusals/{guards} guards; no selected-row/text claim.");
static void W(Span<byte> b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
static void H(Span<byte> b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b[at..],value);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid glyph production accepted.");}
