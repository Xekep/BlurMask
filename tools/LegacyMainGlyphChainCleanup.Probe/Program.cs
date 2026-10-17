using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("capture.json");
var rng=new Random(131890);int guards=0,removed=0;
using var stream=File.Create(args[0]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
for(int trial=0;trial<1024;trial++)
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);int owners=1+rng.Next(8);
    var keys=Enumerable.Range(1,owners).Select(i=>(ulong)(uint)i|((ulong)(uint)(trial%2)<<56)).ToArray();
    foreach(ulong key in keys)for(int j=0;j<rng.Next(3);j++)pool.AppendDescriptor(key,j,true,new byte[40]);
    var references=new List<ulong>();ulong previous=ulong.MaxValue;
    for(int i=0,n=1+rng.Next(24);i<n;i++)
    {
        var residue=new byte[40];rng.NextBytes(residue);
        ulong reference=pool.AppendDescriptor(keys[rng.Next(owners)],100+i,true,residue).Reference;
        byte[] row=pool.CopyRecord(reference);BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(32),previous);
        pool.ReplaceRecord(reference,row);references.Add(reference);previous=reference;
    }
    ulong first=references[rng.Next(references.Count)],last=references[^1];
    var before=pool.CopyOwnerNodes();var result=pool.RemoveCreatedChainSegment(first,last);var after=pool.CopyOwnerNodes();
    removed+=result.RemovedReferences.Length;Check(result.ReversedTraversal[0]==first&&result.ReversedTraversal[^1]==last);
    w.WriteStartObject();w.WriteNumber("first",first);w.WriteNumber("last",last);
    Snapshot("before",before);Snapshot("after",after);
    w.WriteStartArray("visited");foreach(ulong value in result.ReversedTraversal)w.WriteNumberValue(value);w.WriteEndArray();
    w.WriteStartArray("removed");foreach(ulong value in result.RemovedReferences)w.WriteNumberValue(value);w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();Check(removed>2000);
var control=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
ulong sole=control.AppendDescriptor(1,0,false,new byte[40]).Reference;
var cyclic=control.CopyRecord(sole);BinaryPrimitives.WriteUInt64LittleEndian(cyclic.AsSpan(32),sole);control.ReplaceRecord(sole,cyclic);
Reject(()=>control.RemoveCreatedChainSegment(2,sole));
Check(control.CopyRecord(sole).AsSpan().SequenceEqual(cyclic));
Reject(()=>control.RemoveCreatedChainSegment(sole,sole,new(true)));
BinaryPrimitives.WriteUInt64LittleEndian(cyclic.AsSpan(24),1);control.ReplaceRecord(sole,cyclic);
Reject(()=>control.RemoveCreatedChainSegment(sole,sole));Check(control.CopyRecord(sole).AsSpan().SequenceEqual(cyclic));
BinaryPrimitives.WriteUInt64LittleEndian(cyclic.AsSpan(24),0);control.ReplaceRecord(sole,cyclic);
var freed=control.RemoveCreatedChainSegment(sole,sole);
Check(freed.RemovedReferences.SequenceEqual(new[]{sole})&&control.CopyOwnerNode(1).Records.Length==0);
Check(control.AppendDescriptor(1,2,false,new byte[40]).Reference==sole);
Console.WriteLine($"PASS1024 prepared chain cleanup pools/{removed} tail rows released/{guards} guards; native tail-only policy, no owner compaction.");
void Check(bool b){if(!b)throw new InvalidDataException("Cleanup guard failed.");guards++;}
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Unsafe cleanup was accepted.");
}
void Snapshot(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes)
{
    w.WriteStartArray(name);foreach(var node in nodes)
    {w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("rows",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();
}
