using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=5)throw new ArgumentException("AK LK engine separation capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
var fixture=MainFixture.Create(ak,lk,engine,separation);
var span=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,span);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,span.DictionaryOwner);
var geometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,fixture.Sampled);
ulong identity=1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{var bytes=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,fixture.Table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var compact=LegacyOwnedMainCompactWordMaterialization.Materialize(ak,terminal,confidence,new byte[4096*40]);
var source=LegacyOwnedMainSourceWidthStatistics.Prepare(compact,geometry);
int guards=0;
using(var file=File.Create(args[4]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteStartArray("prepared");
    for(int n=0;n<256;n++)
    {
        byte[] records=new byte[24*40],rows=new byte[32*72];
        for(int i=0;i<24;i++)records[i*40+32]=(byte)((i+n)%8|0xa0);
        var boxes=Enumerable.Range(0,32).Select(i=>new LegacyNativeResidualBounds(
            (i-16)*0.125f,0,(i-16)*0.125f+(i+n)%11*0.37f,1)).ToArray();
        if(n%7==0){boxes[0]=new(0,0,1e20f,1);boxes[1]=new(0,0,-1e20f,1);boxes[2]=new(0,0,1,1);}
        for(int i=0;i<32;i++)
        {
            var row=rows.AsSpan(i*72,72);BinaryPrimitives.WriteInt32LittleEndian(row,i);
            BinaryPrimitives.WriteUInt16LittleEndian(row[4..],(ushort)(i*13));
            BinaryPrimitives.WriteInt32LittleEndian(row[8..],(i*7+n)%29);
            BinaryPrimitives.WriteUInt16LittleEndian(row[56..],(ushort)(i%4==0?65535:71));
            BinaryPrimitives.WriteUInt16LittleEndian(row[58..],(ushort)((i%5) switch{0=>65530,1=>65531,2=>65532,3=>65535,_=>72}));
        }
        var seed=new LegacyMainSourceWidthStatistics(n%3==0?1.25:0,n%17==0?int.MaxValue:0);
        var calls=new List<int>();
        var result=LegacyOwnedMainSourceWidthStatistics.Accumulate(rows,records,24,seed,(node,arc)=>
        {if(arc!=node*13)throw new Exception("Source key changed");calls.Add(node);return boxes[node];});
        w.WriteStartObject();w.WriteString("compact",Convert.ToHexString(records));w.WriteString("rows",Convert.ToHexString(rows));
        w.WriteNumber("seedSumBits",BitConverter.DoubleToUInt64Bits(seed.WidthSum));w.WriteNumber("seedCount",seed.Count);
        w.WriteStartArray("boxes");foreach(var b in boxes){w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToUInt32Bits(b.MinimumX));w.WriteNumberValue(BitConverter.SingleToUInt32Bits(b.MaximumX));w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("calls");foreach(int call in calls)w.WriteNumberValue(call);w.WriteEndArray();
        w.WriteNumber("sumBits",BitConverter.DoubleToUInt64Bits(result.WidthSum));w.WriteNumber("count",result.Count);
        w.WriteNumber("averageBits",BitConverter.SingleToUInt32Bits(result.AverageWidth));w.WriteEndObject();
    }
    w.WriteEndArray();
    //An explicit one-row prepared segment over a genuine compact/source
    //owner checks the geometry bridge, without claiming native segmentation.
    byte[] actualCompact=compact.CopyCompactStorage();
    int selected=Enumerable.Range(0,compact.RecordCount).First(i=>
        (actualCompact[i*40+32]&7)!=2);
    byte[] one=new byte[72];BinaryPrimitives.WriteInt32LittleEndian(one.AsSpan(8),selected);
    BinaryPrimitives.WriteUInt16LittleEndian(one.AsSpan(56),ushort.MaxValue);
    var expected=geometry.ReadBounds(0,0);var actual=source.AccumulatePreparedSegment(one,default);
    if(actual.Count!=1||actual.WidthSum!=(double)(expected.MaximumX-expected.MinimumX))throw new Exception("Actual source bbox bridge mismatch");
    w.WriteNumber("actualWidthBits",BitConverter.SingleToUInt32Bits(actual.AverageWidth));
    Reject(()=>source.AccumulatePreparedSegment(new byte[71],default));
    byte[] negative=new byte[72];BinaryPrimitives.WriteInt32LittleEndian(negative.AsSpan(8),-1);
    Reject(()=>source.AccumulatePreparedSegment(negative,default));
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();
    Reject(()=>source.AccumulatePreparedSegment(one,default,cancelled.Token));
    w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS256 prepared2188F0 segments/reverse-width accumulation and actual compact/source bbox bridge;{guards} guards. No native segment producer claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid segment accepted");}
