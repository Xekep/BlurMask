using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication-data output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var source=MainFixture.Create(ak,engine,table);
var caseOwner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,source,caseData,publication);
var directOwner=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,source);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,caseOwner);
var directConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directOwner);
var foreignSource=MainFixture.Create(ak,engine,table);
int guards=0,calls=0,fallbacks=0;
using(var stream=File.Create(args[6]))using(var writer=new Utf8JsonWriter(stream))
{
    writer.WriteStartObject();writer.WriteStartArray("owners");
    Check("case",confidence);Check("direct",directConfidence);
    writer.WriteEndArray();
    Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(null!));
    Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(confidence,new CancellationToken(true)));
    Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,(LegacyFreshSingleGlyphMainSampled)null!));
    Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,foreignSource));
    Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,source,new CancellationToken(true)));
    byte[] corrupt=ak.ToArray();corrupt[189729]^=1;Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(corrupt,confidence,source));
    writer.WriteNumber("calls",calls);writer.WriteNumber("fallbacks",fallbacks);writer.WriteNumber("guards",guards);
    writer.WriteEndObject();writer.Flush();

    void Check(string name,LegacyOwnedRussianMainGlyphConfidence actual)
    {
        var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,actual,source);var graph=callbacks.Graph;
        if(!ReferenceEquals(callbacks.Owner,actual)||!ReferenceEquals(graph,source.SourceRanges.Graph))throw new Exception("Source ownership lost.");
        var arcs=Enumerable.Range(0,graph.NodeCount).SelectMany(node=>Enumerable.Range(0,graph.GetNode(node).Count).Select(arc=>(Node:node,Arc:(ushort)arc))).ToArray();
        writer.WriteStartObject();writer.WriteString("name",name);writer.WriteNumber("nodes",graph.NodeCount);writer.WriteStartArray("fallbacks");
        foreach(var arc in arcs)
        {ushort label=callbacks.ReadFallback(arc.Node,arc.Arc);var replacement=callbacks.ResolveReplacement(arc.Node,arc.Arc);fallbacks++;
            if(replacement!=new LegacyRussianMainReplacementResolution(189,189,0))throw new Exception("Actual replacement callback branch differs.");
            writer.WriteStartArray();writer.WriteNumberValue(arc.Node);writer.WriteNumberValue(arc.Arc);writer.WriteNumberValue(label);writer.WriteNumberValue(graph.GetNode(arc.Node)[arc.Arc].Flags);
            writer.WriteNumberValue(replacement.EmittedSymbol);writer.WriteNumberValue(replacement.NormalizedOriginal);writer.WriteNumberValue(replacement.ExternalVariant);writer.WriteEndArray();}
        writer.WriteEndArray();writer.WriteStartArray("delimiterRequests");
        foreach(int cursor in new[]{0,1,255,256})foreach(int kind in new[]{0,1,2})
        {
            Record(-1,65535,graph.NodeCount,65535,cursor,kind);
            foreach(var arc in arcs){Record(-1,65535,arc.Node,arc.Arc,cursor,kind);Record(arc.Node,arc.Arc,graph.NodeCount,65535,cursor,kind);}
        }
        foreach(var left in arcs)foreach(var right in arcs)foreach(int cursor in new[]{0,1,255,256})
        {
            Record(left.Node,left.Arc,right.Node,right.Arc,cursor,0);
            Record(left.Node,left.Arc,right.Node,right.Arc,cursor,2);
            if(cursor==0)Record(left.Node,left.Arc,right.Node,right.Arc,cursor,1);
        }
        writer.WriteEndArray();writer.WriteEndObject();
        byte[] request=Request(arcs[0].Node,arcs[0].Arc,arcs[^1].Node,arcs[^1].Arc,1);
        Reject(()=>callbacks.ReadDelimiter(request,256));request[0x28]=2;
        Reject(()=>LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(actual).ReadDelimiter(request,256));
        Reject(()=>callbacks.ReadDelimiter(request,257));Reject(()=>callbacks.ReadDelimiter(request,-1));
        Reject(()=>callbacks.ReadDelimiter(new byte[63],1));Reject(()=>callbacks.ReadDelimiter(new byte[65],1));
        Reject(()=>callbacks.ReadFallback(-1,0));Reject(()=>callbacks.ReadFallback(graph.NodeCount,0));Reject(()=>callbacks.ReadFallback(0,65535));
        Reject(()=>callbacks.ReadFallback(0,0,new CancellationToken(true)));Reject(()=>callbacks.ReadDelimiter(request,0,new CancellationToken(true)));
        void Record(int previous,ushort previousArc,int current,ushort currentArc,int cursor,int kind)
        {
            byte[] packet=Request(previous,previousArc,current,currentArc,kind);var output=callbacks.ReadDelimiter(packet,cursor);calls++;
            if(!ReferenceEquals(output.Owner,callbacks))throw new Exception("Callback result lost its source owner.");
            writer.WriteStartArray();writer.WriteNumberValue(previous);writer.WriteNumberValue(previousArc);writer.WriteNumberValue(current);writer.WriteNumberValue(currentArc);
            writer.WriteNumberValue(cursor);writer.WriteNumberValue(kind);writer.WriteNumberValue(output.RemainingCursor);writer.WriteNumberValue(output.NativeFlag0C);writer.WriteNumberValue(output.NativeFlag0D);
            writer.WriteStartArray();foreach(ushort symbol in output.CopySymbols())writer.WriteNumberValue(symbol);writer.WriteEndArray();writer.WriteEndArray();
            var copy=output.CopySymbols();if(copy.Length!=0){ushort saved=copy[0];copy[0]=1;if(output.CopySymbols()[0]!=saved)throw new Exception("Owned delimiter snapshot escaped.");}
        }
    }
}
Console.WriteLine($"PASS owning main case/DIRECT callbacks: {fallbacks} actual ordinary-group FFFD lookups→FFFF;{calls} prepared delimiter requests overactualgraph;{guards} guards. Interior FFFB/unassigned-source geometry andwinning-row production unsupported.");
static byte[] Request(int previous,ushort previousArc,int current,ushort currentArc,int kind)
{
    byte[] request=Enumerable.Repeat((byte)0xa5,64).ToArray();
    BinaryPrimitives.WriteInt32LittleEndian(request.AsSpan(8),previous);BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(12),previousArc);
    BinaryPrimitives.WriteInt32LittleEndian(request.AsSpan(24),current);BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(28),currentArc);
    BinaryPrimitives.WriteInt32LittleEndian(request.AsSpan(40),kind);return request;
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid callback request accepted.");}
