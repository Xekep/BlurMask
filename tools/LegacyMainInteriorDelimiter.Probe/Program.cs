using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=9)throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(separation);
var source=MainFixture.Create(ak,engine,table);
var line=MainLineWordFixture.Create(ak,engine,separation,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),cd,pd,2);
var co=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,source,cd,pd);
var dl=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,source);
var cl=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,cd,pd);
var ll=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source);
var confidence=new[]{LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,co),LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dl),
    LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,cl,line.Context),LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,ll,line.Context)};
int calls=0,guards=0;
using(var stream=File.Create(args[8]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteStartArray("preparedNumericalControls");
    for(int n=0;n<1024;n++)
    {
        int policy=n%6;float h=new[]{1f,40f,100f}[n/6%3],sg=new[]{-1f,0f,17.5f}[n/18%3];
        float threshold=policy==1?(sg<=0?h*.29f:sg*.12f):policy==2?(sg<=0?h*2.95f:Math.Max(h*2.35f,sg*1.87f)):
            ((sg<=0?h*.29f:sg*.12f)+(sg<=0?h*2.95f:Math.Max(h*2.35f,sg*1.87f)))*.5f;
        float gap=(n/54%4) switch{0=>MathF.BitDecrement(threshold),1=>threshold,2=>MathF.BitIncrement(threshold),_=>threshold*2};
        float before=n%3==0?0:float.MaxValue,after=n%5==0?0:float.MaxValue;
        bool prior=(n&1)!=0,current=(n&2)!=0;
        bool result=LegacyOwnedRussianMainInteriorDelimiter.EvaluatePolicy(policy,h,sg,gap,before,after,prior,current);
        w.WriteStartArray();w.WriteNumberValue(policy);foreach(float v in new[]{h,sg,gap,before,after})w.WriteNumberValue(BitConverter.SingleToInt32Bits(v));w.WriteBooleanValue(prior);w.WriteBooleanValue(current);w.WriteBooleanValue(result);w.WriteEndArray();
    }
    w.WriteEndArray();w.WriteStartArray("sources");
    for(int owner=0;owner<confidence.Length;owner++)
    {
        var actual=confidence[owner];var callbacks=owner<2?LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,actual,source):LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,actual,line.Source);
        var geometry=owner<2?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,actual,source):LegacyOwnedRussianMainSourceGeometry.Prepare(ak,actual,line.Source);
        var delimiter=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,callbacks,geometry);
        var arcs=Enumerable.Range(0,geometry.Graph.NodeCount).SelectMany(n=>Enumerable.Range(0,geometry.Graph.GetNode(n).Count).Select(a=>(N:n,A:(ushort)a))).ToArray();
        ushort[] symbols=new[]{"м","M",":",";","!","?","#","@","-","/"}.Select(t=>{if(!model.AcousticAlphabet.TryGetLabel(t,out ushort v))throw new Exception("Expected fixture symbol absent");return v;}).ToArray();
        w.WriteStartObject();w.WriteNumber("owner",owner);w.WriteNumber("heightBits",BitConverter.SingleToInt32Bits(geometry.ReadGuide(geometry.ReadLine(0,0)).Height));
        w.WriteStartArray("requests");
        for(int n=0;n<640;n++)
        {
            var prev=arcs[n%arcs.Length];var cur=arcs[(n*17+3)%arcs.Length];var prior=arcs[(n*7+1)%arcs.Length];var next=arcs[(n*11+2)%arcs.Length];
            byte[] request=new byte[64];I(0,prior.N);U(4,prior.A);I(8,prev.N);U(12,prev.A);I(16,prior.N);U(20,prior.A);
            U(22,n%13==0?(ushort)65535:symbols[n%symbols.Length]);I(24,cur.N);U(28,cur.A);U(30,symbols[n/10%symbols.Length]);I(32,next.N);U(36,next.A);I(40,1);
            I(44,n/100%3);I(48,BitConverter.SingleToInt32Bits(n%3==0?0:n%3==1?-1:17.5f));I(52,n%11==0?10:0);I(56,n%17==0?10:0);
            int cursor=n%4==0?0:256;var result=delimiter.ReadDelimiter(request,cursor);calls++;
            if(!ReferenceEquals(result.Owner,callbacks)||result.NativeFlag0C!=0||result.NativeFlag0D!=0)throw new Exception("Delimiter owner/flags differ.");
            w.WriteStartObject();w.WriteString("request",Convert.ToHexString(request));w.WriteNumber("cursor",cursor);w.WriteNumber("remaining",result.RemainingCursor);w.WriteNumber("count",result.EmittedCount);w.WriteNumber("symbol",result.EmittedCount==0?0:result.CopySymbols()[0]);
            w.WriteNumber("priorStart",geometry.Graph.GetNode(prev.N)[prev.A].StartContextId);w.WriteNumber("flags",geometry.Graph.GetNode(cur.N)[cur.A].Flags);
            w.WriteStartArray("boxes");foreach(var arc in new[]{cur,prev,prior,next}){var b=geometry.ReadBounds(arc.N,arc.A);w.WriteStartArray();foreach(float v in new[]{b.MinimumX,b.MaximumX})w.WriteNumberValue(BitConverter.SingleToInt32Bits(v));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
            void I(int p,int v)=>BinaryPrimitives.WriteInt32LittleEndian(request.AsSpan(p),v);void U(int p,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(request.AsSpan(p),v);
        }
        w.WriteEndArray();w.WriteEndObject();
        byte[] invalid=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(invalid.AsSpan(40),1);
        Reject(()=>delimiter.ReadDelimiter(new byte[63],1));Reject(()=>delimiter.ReadDelimiter(invalid,257));Reject(()=>delimiter.ReadDelimiter(invalid,1,new CancellationToken(true)));
        var foreign=owner<2?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence[owner^1],source):LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence[owner^1],line.Source);
        Reject(()=>LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,callbacks,foreign));
        BinaryPrimitives.WriteInt32LittleEndian(invalid.AsSpan(40),2);Reject(()=>delimiter.ReadDelimiter(invalid,1));
        var noFallback=arcs.First(a=>(geometry.Graph.GetNode(a.N)[a.A].Flags&8)==0);
        byte[] unused=new byte[64];BinaryPrimitives.WriteInt32LittleEndian(unused.AsSpan(40),1);
        BinaryPrimitives.WriteInt32LittleEndian(unused.AsSpan(8),noFallback.N);BinaryPrimitives.WriteUInt16LittleEndian(unused.AsSpan(12),noFallback.A);
        BinaryPrimitives.WriteInt32LittleEndian(unused.AsSpan(24),noFallback.N);BinaryPrimitives.WriteUInt16LittleEndian(unused.AsSpan(28),noFallback.A);
        BinaryPrimitives.WriteUInt16LittleEndian(unused.AsSpan(22),symbols[9]);BinaryPrimitives.WriteUInt16LittleEndian(unused.AsSpan(30),symbols[9]);
        BinaryPrimitives.WriteInt32LittleEndian(unused,int.MaxValue);BinaryPrimitives.WriteInt32LittleEndian(unused.AsSpan(32),int.MaxValue);
        if(delimiter.ReadDelimiter(unused,1).EmittedCount!=0)throw new Exception("Nonpositive gap unexpectedly used dash neighbors or emitted.");
    }
    w.WriteEndArray();w.WriteNumber("calls",calls);w.WriteNumber("guards",guards);w.WriteNumber("unusedNeighborChecks",4);w.WriteEndObject();
}
Console.WriteLine($"PASS1024 independent-numeric inputs and {calls} preparedkind1 requests over actual owning source bounds/guide;{guards} guards; no winning-packet/accuracy claim.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid scope accepted");}
