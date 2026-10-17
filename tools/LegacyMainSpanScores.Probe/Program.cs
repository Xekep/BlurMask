using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=1)throw new ArgumentException("capture.json");
var rng=new Random(140590);int guards=0;
using var stream=File.Create(args[0]);using var writer=new Utf8JsonWriter(stream);writer.WriteStartArray();
for(int trial=0;trial<4096;trial++)
{
    int count=trial<72?trial:rng.Next(1,97);var words=new byte[count][];
    for(int i=0;i<count;i++)
    {
        words[i]=new byte[120];rng.NextBytes(words[i]);
        float score=trial%4==0?rng.Next(3):trial%4==1?rng.Next(-400,401):trial%4==2?-float.MaxValue:rng.Next(-760,761)*.1f;
        BinaryPrimitives.WriteSingleLittleEndian(words[i].AsSpan(16),score);
        BinaryPrimitives.WriteSingleLittleEndian(words[i].AsSpan(28),rng.Next(-400,401)*.25f);
    }
    var before=words.Select(b=>b.ToArray()).ToArray();var result=LegacyPreparedMainSpanScorePublication.Normalize(words);
    Check(result.Words.Length==count&&result.ScoreOrder.Distinct().Count()==count);
    for(int i=0;i<count;i++)Check(words[i].AsSpan().SequenceEqual(before[i]));
    writer.WriteStartObject();writer.WriteStartArray("input");foreach(var b in before)writer.WriteBase64StringValue(b);writer.WriteEndArray();
    writer.WriteStartArray("output");foreach(var b in result.Words)writer.WriteBase64StringValue(b);writer.WriteEndArray();
    writer.WriteStartArray("order");foreach(int i in result.ScoreOrder)writer.WriteNumberValue(i);writer.WriteEndArray();writer.WriteEndObject();
}
writer.WriteEndArray();
Console.WriteLine($"PASS4096 prepared275 score finalizations/{guards} guards; no accepted confidence or actual span mutation.");
void Check(bool b){if(!b)throw new InvalidDataException("Span score guard failed.");guards++;}
