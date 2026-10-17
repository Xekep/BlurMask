using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=1)throw new ArgumentException("capture.json");
var rng=new Random(219580);int calls=0,included=0,guards=0;
using var stream=File.Create(args[0]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
for(int trial=0;trial<2048;trial++)
{
    int nodes=1+rng.Next(16);var starts=new int[nodes+1];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+rng.Next(3,20);
    int count=starts[^1],capacity=count+rng.Next(1,8);var compact=new byte[capacity*40];rng.NextBytes(compact);
    int lastNode=rng.Next(nodes),last=(lastNode<<7)+rng.Next(2),first=trial%4==0?-1:trial%4==1?0:rng.Next(lastNode+1)<<7;
    bool knowledge=trial%2==0;var dry=new LegacyPreparedMainWordAugmentationCandidates.DryRun[count];
    for(int node=0;node<nodes;node++)for(int i=starts[node];i<starts[node+1];i++)
    {
        int sourceNode=trial%3==0?node:rng.Next(nodes);ushort arc=(ushort)rng.Next(6);
        ulong key=((uint)sourceNode&0xffffffUL)|((ulong)arc<<24);
        BinaryPrimitives.WriteUInt64LittleEndian(compact.AsSpan(i*40+8),key);
        compact[i*40+32]=(byte)rng.Next(32);
        short a=(short)(rng.Next(4)==0?-rng.Next(1,4):rng.Next(4));int source=rng.Next(nodes);
        if(trial%5==0){source=0;a=0;}
        ulong firstKey=((uint)source&0xffffffUL)|((ulong)(ushort)a<<24);
        dry[i]=new(rng.Next(5),firstKey);
    }
    int First(int node,ushort arc)=>unchecked((node-arc%3)<<7);
    int Last(int node,ushort arc)=>unchecked((node<<7)+(arc%2));
    int minimum=first<0?-1:LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(first);
    LegacyPreparedMainWordAugmentationCandidates.DryRun Inspect(int index,int min)
    {Check(min==minimum);calls++;return dry[index];}
    var result=LegacyPreparedMainWordAugmentationCandidates.Scan(compact,count,starts,first,last,knowledge,First,Last,Inspect);
    included+=result.CompactIndices.Length;
    w.WriteStartObject();w.WriteNumber("first",first);w.WriteNumber("last",last);w.WriteNumber("active",count);w.WriteBoolean("knowledge98",knowledge);
    w.WriteBase64String("storage",compact);w.WriteStartArray("starts");foreach(int n in starts)w.WriteNumberValue(n);w.WriteEndArray();
    w.WriteStartArray("dry");foreach(var d in dry){w.WriteStartArray();w.WriteNumberValue(d.Status);w.WriteNumberValue(d.FirstReference);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("selected");foreach(int n in result.CompactIndices)w.WriteNumberValue(n);w.WriteEndArray();
    w.WriteStartArray("attempts");foreach(var a in result.Attempts)
    {w.WriteStartArray();w.WriteNumberValue(a.CompactIndex);w.WriteNumberValue(a.MinimumNode);w.WriteNumberValue(a.Status);w.WriteNumberValue(a.FirstReference);w.WriteBooleanValue(a.Included);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();Check(calls>1000&&included>100);
for(int node=-2048;node<2048;node++)for(int suffix=0;suffix<128;suffix++)
    Check(LegacyPreparedMainWordAugmentationCandidates.DecodeContextNode(unchecked((node<<7)|suffix))==node);
Console.WriteLine($"PASS2048 prepared219580 scans/{calls} prepared inspector calls/{included} selected controls/{guards} guards; not owned218A30 or WORD augmentation.");
void Check(bool value){if(!value)throw new InvalidDataException("Prepared scan guard failed.");guards++;}
