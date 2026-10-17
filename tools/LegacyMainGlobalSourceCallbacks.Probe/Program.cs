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
int owners=0,arcs=0,requests=0,pendingWrites=0,guards=0,preparedBBoxUpdates=0;ulong identity=0x1000;
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
    int evaluations=owner.AcousticEvaluations;
    //Explicit actual-key kind6 controls exercise the SAME native mutable bbox
    //owner after inference. They are not claimed scheduler-produced packets.
    var before=transitions.CopyGeometryState();
    for(int n=0;n<graph.State.NodeCount;n++)for(ushort a=0;a<graph.State.GetNode(n).Count;a++)
    {
        byte[] packet=new byte[80];BinaryPrimitives.WriteInt32LittleEndian(packet,n);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4),unchecked((int)graph.State.GetNode(Math.Max(0,n-2))[0].StartContextId));
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(8),n);BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(12),a);
        packet[28]=6;BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(52),0x4000000);
        _=transitions.Validate140(new(packet,new((ulong)n*2048+a+1,0)));
    }
    var after=transitions.CopyGeometryState();preparedBBoxUpdates+=before.Zip(after).Count(p=>p.First!=p.Second);
    w.WriteStartObject();w.WriteNumber("owner",owners);w.WriteNumber("nodes",graph.State.NodeCount);
    if(owners==0)
    {
        w.WriteStartArray("preparedCursorControls");
        foreach(int cursor in new[]{0,1,7,256})foreach(int count in new[]{1,2,255,256,257,4096})
        {var value=LegacyFreshMainGlobalSourceSymbolCallbacks.CalculatePendingWriteSpan(cursor,count);w.WriteStartArray();w.WriteNumberValue(cursor);w.WriteNumberValue(count);w.WriteNumberValue(value.RemainingCursor);w.WriteNumberValue(value.WrittenCount);w.WriteEndArray();}
        w.WriteEndArray();
    }
    w.WriteStartArray("descriptors");foreach(var d in cuts.Steps.Where(d=>!d.IsPendingSeparator)){w.WriteStartObject();w.WriteNumber("ordinal",d.DescriptorOrdinal);w.WriteBase64String("raw",d.CopyDescriptor());w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("guides");foreach(var guide in cuts.CopyGuides())w.WriteBase64StringValue(guide);w.WriteEndArray();
    w.WriteStartArray("originalPointCounts");foreach(var stroke in raw)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
    w.WriteStartArray("contexts");foreach(var c in graph.Source.State.Contexts){w.WriteStartObject();w.WriteBase64String("prefix",c.CopyPrefix());w.WriteStartArray("controls");foreach(int v in c.CopyControls())w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("arcs");
    for(int node=0;node<graph.State.NodeCount;node++)for(ushort ordinal=0;ordinal<graph.State.GetNode(node).Count;ordinal++)
    {
        var arc=graph.State.GetNode(node)[ordinal];var group=graph.State.Groups[(int)arc.GroupId];var b=geometry.ReadBounds(node,ordinal);int line=geometry.ReadLine(node,ordinal);var guide=geometry.ReadGuide(line);
        var expected=after[arc.GroupId];if(b!=new LegacyNativeResidualBounds(expected.MinimumX,expected.MinimumY,expected.MaximumX,expected.MaximumY))throw new Exception("Callback read frozen or foreign bbox state.");
        if(geometry.ReadKind(node,ordinal)!=0||geometry.CollectionC1||callbacks.ReadFallback(node,ordinal)!=ushort.MaxValue||callbacks.ResolveReplacement(node,ordinal)!=new LegacyRussianMainReplacementResolution(189,189,0))throw new Exception("Ordinary global callback binding differs.");
        w.WriteStartObject();w.WriteNumber("node",node);w.WriteNumber("arc",ordinal);w.WriteNumber("group",arc.GroupId);w.WriteNumber("descriptor",group.DescriptorOrdinal);w.WriteNumber("auxiliary",arc.AuxiliaryContextIndex);w.WriteNumber("flags",arc.Flags);w.WriteNumber("line",line);w.WriteNumber("baselineBits",BitConverter.SingleToInt32Bits(guide.Baseline));w.WriteNumber("heightBits",BitConverter.SingleToInt32Bits(guide.Height));w.WriteBoolean("topMark",geometry.ReadTopMark(node,ordinal));
        w.WriteStartArray("boundsBits");foreach(float v in new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY})w.WriteNumberValue(BitConverter.SingleToInt32Bits(v));w.WriteEndArray();
        w.WriteStartArray("ranges");foreach(var r in geometry.CopyStoredGroupRanges(node,ordinal)){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("sourceReferences");foreach(var r in geometry.ResolveSourceRanges(node,ordinal)){w.WriteStartArray();foreach(int v in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("requests");
        foreach(int cursor in new[]{0,1,7,256})foreach(int kind in new[]{0,1,2,7})foreach(int previous in new[]{-1,node})
        {
            byte[] request=new byte[64];W(8,previous);H(12,ordinal);W(16,node);H(20,ordinal);W(24,node);H(28,ordinal);W(40,kind);W(44,2);
            var value=callbacks.ReadDelimiter(request,cursor);if(!ReferenceEquals(value.Owner,callbacks)||value.NativeFlag0C!=0||value.NativeFlag0D!=0)throw new Exception("Global delimiter owner/flags differ.");
            var symbols=value.CopySymbols();if(symbols.Length>0){ushort old=symbols[0];symbols[0]^=1;if(value.CopySymbols()[0]!=old)throw new Exception("Delimiter memory escaped.");}
            w.WriteStartArray();w.WriteNumberValue(cursor);w.WriteNumberValue(kind);w.WriteNumberValue(previous);w.WriteNumberValue(value.RemainingCursor);w.WriteStartArray();foreach(ushort symbol in value.CopySymbols())w.WriteNumberValue(symbol);w.WriteEndArray();w.WriteEndArray();requests++;pendingWrites+=value.CopySymbols().Count(s=>s==65529);
            void W(int at,int v)=>BinaryPrimitives.WriteInt32LittleEndian(request.AsSpan(at),v);
            void H(int at,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(at),v);
        }
        w.WriteEndArray();w.WriteEndObject();arcs++;
    }
    w.WriteEndArray();
    byte[] endpoint=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(8),-1);BinaryPrimitives.WriteInt32LittleEndian(endpoint.AsSpan(24),graph.State.NodeCount);
    var end=callbacks.ReadDelimiter(endpoint,256);if(end.RemainingCursor!=256||end.CopySymbols().Length!=0)throw new Exception("Actual consumed-fallback endpoint changed.");
    Reject(()=>callbacks.ReadDelimiter(endpoint,257));Reject(()=>callbacks.ReadDelimiter(new byte[63],1));Reject(()=>callbacks.ReadFallback(graph.State.NodeCount,0));Reject(()=>geometry.ReadGuide(-1));Reject(()=>geometry.ReadBounds(0,0,new(true)));
    byte[] general=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(general.AsSpan(40),1);
    if(graph.State.GetNode(0)[0].AuxiliaryContextIndex==ushort.MaxValue)Reject(()=>callbacks.ReadDelimiter(general,1));
    byte[] damaged=ak.ToArray();damaged[0]^=1;Reject(()=>LegacyFreshMainGlobalSourceGeometry.Prepare(damaged,policy,confidence));Reject(()=>LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(damaged,geometry));
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Pure geometry/source callbacks recalculated NN.");w.WriteEndObject();Console.WriteLine($"PASS global source owner{owners}:nodes{graph.State.NodeCount},NN{evaluations} unchanged.");owners++;
}
w.WriteEndArray();w.Flush();if(preparedBBoxUpdates==0)throw new Exception("Prepared actual-key controls never distinguished live bbox ownership.");Console.WriteLine($"PASS {owners} owners/{arcs} actual arcs/{requests} prepared requests/{pendingWrites} native pendingFFF9 writes/{preparedBBoxUpdates} separately prepared livebbox updates/{guards} guards; no glyph/text publication claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid global source callback accepted.");}
