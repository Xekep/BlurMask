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
int owners=0,calls=0,pathCalls=0,pending=0,guards=0,selectorChecks=0,kind7Calls=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);var operations=transitions.CreateOperations();
    ushort Label(string s)=>model.AcousticAlphabet.TryGetLabel(s,out var label)?label:throw new Exception("Actual label missing.");
    var pairs=new[]{new[]{Label("\u043c"),Label("\u0438")},new[]{Label("'"),Label("\u043c")},new[]{Label("\ufffd"),Label("\u043c")}};
    w.WriteStartObject();w.WriteStartArray("descriptors");foreach(var d in cuts.Steps.Where(d=>!d.IsPendingSeparator)){var bytes=d.CopyDescriptor();Ints(null,[d.DescriptorOrdinal,d.LineId,BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(20)),BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(24))]);}w.WriteEndArray();
    w.WriteStartArray("contexts");foreach(var c in global.State.Contexts)w.WriteStringValue(Convert.ToHexString(c.CopyPrefix()));w.WriteEndArray();w.WriteNumber("rootGroup",graph.State.Root.GroupId);
    w.WriteStartArray("groups");foreach(var g in graph.State.Groups){w.WriteStartObject();w.WriteNumber("descriptor",g.DescriptorOrdinal);w.WriteBoolean("wholeStarts",g.CopyRanges().All(r=>r.StartPoint==0));w.WriteNumber("flags6",g.Flags06);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("arcs");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.State.GetNode(n))Ints(null,[(int)a.GroupId,a.Span,a.Flags,a.AuxiliaryContextIndex,unchecked((int)a.StartContextId),unchecked((int)a.EndContextId)]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("calls");for(int n=0;n<graph.State.NodeCount;n++)for(ushort a=0;a<graph.State.GetNode(n).Count;a++)
    {
        var arc=graph.State.GetNode(n)[a];int previousEnd=n-arc.Span-1;if(previousEnd<0||graph.State.GetNode(previousEnd).Count==0)continue;
        foreach(byte kind in new byte[]{0,2,5})foreach(var pair in pairs)foreach(int initial in new[]{0,int.MaxValue-4096})
        {
            int previousSource=previousEnd;
            if((calls&1)!=0&&previousEnd>0&&graph.State.GetNode(previousEnd-1).Count!=0)previousSource=previousEnd-1;
            byte[] bytes=Enumerable.Range(0,80).Select(i=>(byte)(i*7+calls)).ToArray();I(bytes,0,n);I(bytes,4,previousSource);I(bytes,8,previousEnd);U(bytes,12,a);U(bytes,14,0);U(bytes,16,0);I(bytes,20,calls%6);I(bytes,24,calls/6%6);bytes[28]=kind;I(bytes,32,arc.Span);U(bytes,36,pair[0]);U(bytes,38,pair[1]);I(bytes,48,12345);I(bytes,52,0x4000000);I(bytes,64,calls%5==0?9:calls%4==0?7:6);I(bytes,68,calls%3==0?10:6);
            var cell=new LegacyNativeWordTransitionCostCell((ulong)calls+1,initial);BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40),cell.Identity);var request=new LegacyNativeWordRecombinationRequest(bytes,cell);var before=request.GetSnapshot();bool accepted=transitions.Calculate118(request);
            var after=request.GetSnapshot();int actualCost=cell.Value;if(!transitions.Validate140(request)||!request.GetSnapshot().Span.SequenceEqual(after.Span)||cell.Value!=actualCost)throw new Exception("Main140 disabled pair branch changed prepared request.");
            var next=operations.SuccessorsF0F8(request,default);if(!next.Span.SequenceEqual(graph.State.GetNode(previousEnd)[0].CopySelector(arc.Span+1)))throw new Exception("Actual compact successor order changed.");selectorChecks++;
            if(unchecked((short)arc.AuxiliaryContextIndex)>=0)pending++;
            w.WriteStartObject();w.WriteString("before",Convert.ToHexString(before.Span));w.WriteNumber("costBefore",initial);w.WriteBoolean("accepted",accepted);w.WriteNumber("costAfter",actualCost);w.WriteString("after",Convert.ToHexString(after.Span));w.WriteEndObject();calls++;
            var canceled=new LegacyNativeWordRecombinationRequest(before.ToArray(),new LegacyNativeWordTransitionCostCell(cell.Identity,initial));Reject(()=>transitions.Calculate118(canceled,new(true)));if(canceled.Cost.Value!=initial||!canceled.GetSnapshot().Span.SequenceEqual(before.Span))throw new Exception("Cancellation changed prepared request.");
        }
    }w.WriteEndArray();
    //Prepared native kind7 packets over the real source graph. These are
    //distinct from the actual213C00-owned parent continuation observations.
    w.WriteStartArray("kind7Calls");
    for (int n = 0; n < graph.State.NodeCount; n++)
    for (ushort a = 0; a < graph.State.GetNode(n).Count; a++)
    {
        var arc = graph.State.GetNode(n)[a]; int previous = n - arc.Span - 1;
        if (previous < 0 || graph.State.GetNode(previous).Count == 0) continue;
        foreach (int sourceKind in new[] { 6, 9 })
        foreach (int initial in new[] { 0, int.MaxValue - 4096 })
        {
            byte[] bytes = Enumerable.Range(0,80).Select(i => (byte)(i * 11 + kind7Calls)).ToArray();
            I(bytes,0,n); I(bytes,4,previous); I(bytes,8,previous); U(bytes,12,a); U(bytes,14,0); U(bytes,16,0);
            I(bytes,20,0); I(bytes,24,0); bytes[28]=7; I(bytes,32,arc.Span);
            U(bytes,36,Label("\u043c")); U(bytes,38,Label("-")); I(bytes,48,12345); I(bytes,52,0x4000000);
            I(bytes,64,sourceKind); I(bytes,68,6);
            var cell = new LegacyNativeWordTransitionCostCell((ulong)kind7Calls + 200000, initial);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40),cell.Identity);
            var request = new LegacyNativeWordRecombinationRequest(bytes,cell); var before=request.GetSnapshot();
            bool accepted=transitions.Calculate118(request); var after=request.GetSnapshot(); int cost=cell.Value;
            if (!transitions.Validate140(request) || cell.Value!=cost || !request.GetSnapshot().Span.SequenceEqual(after.Span))
                throw new Exception("Main kind7 graph192=false140 changed its packet/cost.");
            w.WriteStartObject();w.WriteString("before",Convert.ToHexString(before.Span));w.WriteNumber("costBefore",initial);
            w.WriteBoolean("accepted",accepted);w.WriteNumber("costAfter",cost);w.WriteString("after",Convert.ToHexString(after.Span));w.WriteEndObject();kind7Calls++;
        }
    }
    w.WriteEndArray();
    w.WriteStartArray("pathCalls");
    for(int n=0;n<graph.State.NodeCount;n++)for(ushort a=0;a<graph.State.GetNode(n).Count;a++)
    {
        var arc=graph.State.GetNode(n)[a];
        foreach(int delta in new[]{0,1})
        {
            int previous=n-delta;if(previous<0||graph.State.GetNode(previous).Count==0)continue;
            var end=graph.State.GetNode(previous)[0];byte[] bytes=new byte[80];I(bytes,0,n);U(bytes,12,a);I(bytes,4,unchecked((int)(delta==0?end.StartContextId:graph.State.GetNode(Math.Max(0,previous-2))[0].StartContextId)));I(bytes,8,previous);U(bytes,16,0);bytes[28]=6;I(bytes,52,pathCalls%3==0?0:0x4000000);
            var cell=new LegacyNativeWordTransitionCostCell((ulong)pathCalls+100000,0);BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40),cell.Identity);var request=new LegacyNativeWordRecombinationRequest(bytes,cell);
            var snapshot=request.GetSnapshot();if(!transitions.Calculate118(request)||cell.Value!=0||!request.GetSnapshot().Span.SequenceEqual(snapshot.Span))throw new Exception("Ordinary kind6 Calculate118 changed its default-success packet.");bool accepted=transitions.Validate140(request);
            if(!request.GetSnapshot().Span.SequenceEqual(snapshot.Span))throw new Exception("Path140 changed packet bytes.");
            w.WriteStartObject();w.WriteString("request",Convert.ToHexString(snapshot.Span));w.WriteBoolean("accepted",accepted);w.WriteNumber("cost",cell.Value);w.WriteEndObject();pathCalls++;
            var canceled=new LegacyNativeWordRecombinationRequest(snapshot.ToArray(),new(cell.Identity,0));var oldBoxes=transitions.CopyGeometryState();Reject(()=>transitions.Validate140(canceled,new(true)));
            if(canceled.Cost.Value!=0||!canceled.GetSnapshot().Span.SequenceEqual(snapshot.Span)||!transitions.CopyGeometryState().SequenceEqual(oldBoxes))throw new Exception("Canceled path140 changed state.");
        }
    }
    w.WriteEndArray();
    w.WriteStartArray("finalBounds");foreach(var b in transitions.CopyGeometryState()){w.WriteStartArray();foreach(float v in new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("selectors");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.State.GetNode(n))Ints(null,a.CopySelector(1).Select(i=>(int)i));w.WriteEndArray();}w.WriteEndArray();
    for(int s=0;s<graph.State.MaximumAtoms;s++)
    {
        byte[] bytes=new byte[80];I(bytes,8,-1);I(bytes,32,s);var request=new LegacyNativeWordRecombinationRequest(bytes,new(1,0));if(!operations.SuccessorsF0F8(request,default).Span.SequenceEqual(graph.State.Root.CopySelector(s+1)))throw new Exception("Actual root selector changed.");selectorChecks++;
    }
    var preparedPolicy = LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,
        LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table),transitions);
    var pathGuard = new LegacyGlobalOrdinaryWordPathGuard(preparedPolicy.Services);
    byte[] kind7Packet = new byte[80]; kind7Packet[28]=7; I(kind7Packet,64,6); I(kind7Packet,68,6);
    var kind7GuardRequest = new LegacyNativeWordRecombinationRequest(kind7Packet,new(1,83));
    if (!pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,0,false,default))
        throw new Exception("Native null context kind7 branch rejected.");
    foreach (ushort rootSymbol in new ushort[] {0xfff8,0xfff9,0xfffa,0xfffb,0xfffc,0xfffd,0xfffe,0xffff,0})
    {
        ulong root = owner.Contexts.ResolveInitial(rootSymbol).Identity;
        var resolvedChild = owner.Contexts.ResolveChild(root,Label("\u043c"));
        if (resolvedChild.Created) owner.Contexts.LinkCreatedContext(resolvedChild.Identity,root);
        ulong child = resolvedChild.Identity;
        bool expected = unchecked((ushort)(rootSymbol+7)) >= 4;
        if (pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,root,false,default)!=expected ||
            pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,child,false,default)!=expected)
            throw new Exception("Native kind7 follows root parent links or ushort control wrap incorrectly.");
    }
    if (kind7GuardRequest.Cost.Value!=83 || !kind7GuardRequest.GetSnapshot().Span.SequenceEqual(kind7Packet))
        throw new Exception("Kind7 symbolic guard changed its prepared packet/cost.");
    Reject(()=>pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,0,true,default));
    Reject(()=>pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,new LegacySymbolContextFactory().ResolveInitial(1).Identity,false,default));
    Reject(()=>pathGuard.AllowsSoftHyphenContinuation(kind7GuardRequest,0,false,new(true)));
    var unsupportedBytes=new byte[80];unsupportedBytes[28]=1;var unsupported=new LegacyNativeWordRecombinationRequest(unsupportedBytes,new(1,0));Reject(()=>transitions.Calculate118(unsupported));Reject(()=>transitions.Validate140(unsupported));
    Reject(()=>LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache,new(true)));var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalWordTransitions.Prepare(bad,cache));w.WriteEndObject();owners++;
}
w.WriteEndArray();w.Flush();if(pending==0)throw new Exception("Actual pending descriptor contexts untested.");Console.WriteLine($"PASS {owners} owned global118/140 sources/{calls} full80 prepared callbacks/{kind7Calls} additional prepared kind7 callbacks/{pathCalls} actual-key path140 callbacks/{pending} pending-context callbacks/{selectorChecks} actual ordered selectors/{guards} cancellation/resource guards; no controller or winner claim.");
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void I(byte[] b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),value);
void U(byte[] b,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),value);
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Unsupported global transition callback accepted.");}
