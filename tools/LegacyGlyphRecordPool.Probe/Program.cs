using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length!=1) throw new ArgumentException("controls.json required");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
void Reject(Action action)
{ try { action(); } catch(Exception e) when(e is ArgumentException or NotSupportedException or OperationCanceledException) {return;} throw new InvalidOperationException("Invalid glyph pool input accepted."); }
int cases=0,operations=0,nodes=0;
foreach(var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    foreach(var op in test.GetProperty("operations").EnumerateArray())
    {
        byte[] initial=Convert.FromBase64String(op.GetProperty("template").GetString()!);
        var inserted=pool.AppendDescriptor(op.GetProperty("key").GetUInt64(),op.GetProperty("descriptor").GetInt32(),op.GetProperty("force").GetBoolean(),initial);
        Require(inserted.Reference==op.GetProperty("reference").GetUInt64()&&inserted.ExistingDescriptor==op.GetProperty("existing").GetBoolean(),"Native descriptor interning/reference ordinal differs.");
        Require(pool.CopyRecord(inserted.Reference).AsSpan().SequenceEqual(Convert.FromBase64String(op.GetProperty("before").GetString()!)),"Native allocated row writes or descriptor reuse differs.");
        initial.AsSpan().Fill(0xff);
        pool.SetOrdinarySymbol(inserted.Reference,op.GetProperty("symbol").GetUInt16());
        Require(pool.CopyRecord(inserted.Reference).AsSpan().SequenceEqual(Convert.FromBase64String(op.GetProperty("after").GetString()!)),"Native normal label/flags producer changed opaque data.");
        operations++;
    }
    var snapshots=pool.CopyOwnerNodes().OrderBy(n=>n.Key).ToArray();
    var expected=test.GetProperty("snapshots").EnumerateArray().ToArray();
    Require(snapshots.Length==expected.Length,"Owner count differs.");
    var resolver=new LegacyNativeGlyphRecordOwner(snapshots);
    for(int node=0;node<snapshots.Length;node++)
    {
        var actual=snapshots[node];var item=expected[node];
        Require(actual.Key==item.GetProperty("key").GetUInt64()&&actual.PackedCount==item.GetProperty("count").GetUInt16()&&
            actual.Records.Span.SequenceEqual(Convert.FromBase64String(item.GetProperty("rows").GetString()!)),"Native lookup-relevant header/whole owned rows differ.");
        var copied=pool.CopyOwnerNode(actual.Key);
        Require(copied.Records.Span.SequenceEqual(actual.Records.Span),"13ac80 owner access differs from frozen lookup snapshot.");
        int count=unchecked((short)(actual.PackedCount<<2))>>4;
        for(int ordinal=0;ordinal<count;ordinal++)
        {
            ulong reference=actual.Key|((ulong)ordinal<<40);
            Require(resolver.Resolve(reference).ReadRecord().AsSpan().SequenceEqual(actual.Records.Span.Slice(ordinal*40,40)),"Produced owner fails existing13ae90 lookup.");
        }
        nodes++;
    }
    cases++;
}
Reject(()=>LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(true));
Reject(()=>LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,new(true)));
var guard=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);byte[] blank=new byte[40];
Reject(()=>guard.AppendDescriptor(1UL<<40,0,false,blank));
Reject(()=>guard.AppendDescriptor(0,0,false,new byte[39]));
var first=guard.AppendDescriptor(0,17,false,blank);guard.SetOrdinarySymbol(first.Reference,3);
var second=guard.AppendDescriptor(0,18,false,blank);guard.SetOrdinarySymbol(second.Reference,3);
Require(first.Reference!=second.Reference&&guard.AppendDescriptor(0,17,false,blank).ExistingDescriptor,"Equal symbols incorrectly deduplicated distinct descriptor IDs.");
var clone=guard.CopyRecord(first.Reference);BinaryPrimitives.WriteSingleLittleEndian(clone.AsSpan(12),.125f);
guard.ReplaceRecord(first.Reference,clone);clone.AsSpan().Clear();
Require(BinaryPrimitives.ReadSingleLittleEndian(guard.CopyRecord(first.Reference).AsSpan(12))==.125f,"Caller-provided actual score snapshot isn't owned.");
Reject(()=>guard.ReplaceRecord(first.Reference,blank));
Reject(()=>guard.SetOrdinarySymbol(first.Reference,0xfffa));
Reject(()=>guard.CopyRecord(2UL<<40));
Reject(()=>guard.CopyOwnerNode(1));
Reject(()=>guard.CopyOwnerNode(1UL<<40));
Reject(()=>guard.SetOrdinarySymbol(first.Reference,3,new(true)));
var capacity=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
for(int i=0;i<2040;i++)capacity.AppendDescriptor(0,i,true,blank);
Require(unchecked((short)(capacity.CopyOwnerNode(0).PackedCount<<2))>>4==2040,"2040 native row capacity unavailable.");
Reject(()=>capacity.AppendDescriptor(0,2040,true,blank));
Require(capacity.CopyOwnerNode(0).Records.Length==2040*40,"Capacity rejection changed owned records.");
Console.WriteLine($"PASS {cases} independent glyph pools / {operations} native insertion+symbol operations / {nodes} owner snapshots→existing13ae90; signed descriptor dedup, persistent forced-duplicate flag, opaque fields, capacity and ownership/cancellation guards. Geometry capture false is explicit; no rawglyph score or Unicode accuracy claim.");
