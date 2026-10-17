using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("capture.json");
var random=new Random(219580);int guards=0;
using var stream=File.Create(args[0]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
for(int trial=0;trial<16384;trial++)
{
    var input=new byte[160];random.NextBytes(input);
    int[] limits=[0,1,-1,int.MinValue,int.MaxValue,16777217,-16777217,8192];
    for(int row=0;row<2;row++)for(int channel=0;channel<2;channel++)
        BinaryPrimitives.WriteInt32LittleEndian(input.AsSpan(row*40+20+channel*4),
            trial<64?limits[(trial+row*3+channel)%limits.Length]:unchecked((int)random.NextInt64(int.MinValue,(long)int.MaxValue+1)));
    int first=trial%5==0?-1:random.Next(4096),last=random.Next(Math.Max(first,0),4096);
    int parent=trial%3==0?-1:0,kind=trial%2==0?0x135:random.Next(-1,0x140);
    byte[] before=input.ToArray();var r=LegacyPreparedMainAugmentedWordCosts.Calculate(input,2,1,parent,first,last,kind);
    if(!input.AsSpan().SequenceEqual(before))throw new InvalidDataException("Compact source was mutated.");
    var output=new byte[12];BinaryPrimitives.WriteSingleLittleEndian(output,r.Score10);
    BinaryPrimitives.WriteSingleLittleEndian(output.AsSpan(4),r.Cost18);BinaryPrimitives.WriteSingleLittleEndian(output.AsSpan(8),r.Cost1C);
    w.WriteStartObject();w.WriteBase64String("input",input);w.WriteNumber("parent",parent);w.WriteNumber("first",first);
    w.WriteNumber("last",last);w.WriteNumber("kind",kind);w.WriteBase64String("output",output);w.WriteEndObject();
}
w.WriteEndArray();
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[79],1,0,-1,0,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],3,0,-1,0,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,2,-1,0,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,1,2,0,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,1,-2,0,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,1,-1,2,1,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,1,-1,0,4096,0x135));
Reject(()=>LegacyPreparedMainAugmentedWordCosts.Calculate(new byte[80],2,1,-1,0,1,0x135,new(true)));
Console.WriteLine($"PASS16384 prepared219580 cost transfers/{guards} guards; no candidate selection or WORD interning claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Cost guard failed.");}
