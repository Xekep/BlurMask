using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("capture.json");
var random=new Random(130360);int guards=0,cases=0,changed=0,quotaChanges=0;
using var stream=File.Create(args[0]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
for(int trial=0;trial<2048;trial++)
{
    int n=1+random.Next(24);bool special=trial%2==0;int quota=trial%8;
    var words=new byte[n][];var identity=new int[n];
    for(int i=0;i<n;i++)
    {
        words[i]=new byte[120];random.NextBytes(words[i]);identity[i]=random.Next(Math.Max(1,n/3));
        BinaryPrimitives.WriteInt32LittleEndian(words[i],identity[i]);
        BinaryPrimitives.WriteSingleLittleEndian(words[i].AsSpan(16),random.Next(-5,6)*0.25f);
        BinaryPrimitives.WriteInt32LittleEndian(words[i].AsSpan(60),random.Next(3));
        BinaryPrimitives.WriteUInt16LittleEndian(words[i].AsSpan(48),(ushort)(random.Next(3)==0?65534:7));
        BinaryPrimitives.WriteUInt16LittleEndian(words[i].AsSpan(50),(ushort)(random.Next(4)*0x100+(random.Next(3)==0?0x40:0)));
    }
    var selected=Enumerable.Range(0,1+random.Next(n)).Select(_=>random.Next(n)).ToArray();
    var before=words.Select(x=>x.ToArray()).ToArray();
    bool Equal(byte[] a,byte[] b)=>BinaryPrimitives.ReadInt32LittleEndian(a)==BinaryPrimitives.ReadInt32LittleEndian(b);
    var result=LegacyPreparedMainWordRanking.Rank(words,selected,quota,special,Equal);
    for(int i=0;i<n;i++)Check(words[i].AsSpan().SequenceEqual(before[i]));
    Check(result.OldToNew[selected[0]]==0);
    Check(selected.All(s=>result.OldToNew[s]>=0&&result.OldToNew[s]<result.Words.Length));
    if(result.Words.Length!=n)changed++;if(result.QuotaAfter!=quota)quotaChanges++;
    w.WriteStartObject();w.WriteNumber("quota",quota);w.WriteBoolean("special",special);
    w.WriteStartArray("words");foreach(var b in before)w.WriteBase64StringValue(b);w.WriteEndArray();
    w.WriteStartArray("selected");foreach(int s in selected)w.WriteNumberValue(s);w.WriteEndArray();
    w.WriteNumber("quotaAfter",result.QuotaAfter);
    w.WriteStartArray("output");foreach(var b in result.Words)w.WriteBase64StringValue(b);w.WriteEndArray();
    w.WriteStartArray("map");foreach(int i in result.OldToNew)w.WriteNumberValue(i);w.WriteEndArray();
    w.WriteStartArray("released");foreach(int i in result.Released)w.WriteNumberValue(i);w.WriteEndArray();
    w.WriteStartArray("rows");foreach(var b in result.RankingByOriginalIndex)w.WriteBase64StringValue(b);w.WriteEndArray();w.WriteEndObject();cases++;
}
w.WriteEndArray();
foreach(ushort a in new ushort[]{0,1,2,10,65535})foreach(ushort b in new ushort[]{0,1,2,10,65535})foreach(bool mode in new[]{false,true})
    Check(LegacyPreparedMainWordRanking.InitialQuota(a,b,mode)==Math.Max(a==65535?2:a,b==65535?2:b)+(mode?1:0));
Check(changed>1500&&quotaChanges>1500);
Console.WriteLine($"PASS {cases} prepared ranking cases/{changed} pruned/{quotaChanges} quota changes/{guards} guards; numeric kernel only.");
void Check(bool condition){if(!condition)throw new InvalidDataException("Ranking guard failed.");guards++;}
