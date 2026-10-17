using InkRecognition.Math;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("capture.json");
uint[] costs=[0xff800000,0xff7fffff,0xbf800000,0x80000000,0,0x3c23d70a,0x3f000000,0x3f800000,0x7f7fffff,0x7f800000,0x7fc00000,0x7fa00001];
using var file=File.Create(args[0]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("cases");int guards=0,cases=0;
byte[] Make(int count,int seed,bool equal)
{
    var bytes=new byte[count*168];uint random=unchecked((uint)(seed*7919+17));
    for(int i=0;i<count;i++)
    {
        for(int j=0;j<168;j++)bytes[i*168+j]=unchecked((byte)(seed*23+i*37+j*19));
        random^=random<<13;random^=random>>17;random^=random<<5;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i*168),equal?0x3f800000:costs[random%costs.Length]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i*168+4),i);
    }
    return bytes;
}
void Run(int count,int seed,bool equal,bool full)
{
    byte[] input=Make(count,seed,equal),before=input.ToArray();var sorted=LegacyPreparedMathCandidateOrdering.Sort(input);
    if(!input.AsSpan().SequenceEqual(before))throw new Exception("Input storage changed.");
    if(equal&&count==20&&!sorted.AsSpan().SequenceEqual(input))throw new Exception("Native20-row strict-tie insertion order changed.");
    if(equal&&count==21)for(int i=0;i<count;i++)if(BinaryPrimitives.ReadInt32LittleEndian(sorted.AsSpan(i*168+4))!=20-i)throw new Exception("Native21-row all-tie partition order changed.");
    if(equal&&count==42)for(int i=0;i<count;i++)if(BinaryPrimitives.ReadInt32LittleEndian(sorted.AsSpan(i*168+4))!=(i+21)%42)throw new Exception("Native42-row recursive tie order changed.");
    w.WriteStartObject();w.WriteNumber("count",count);w.WriteNumber("seed",seed);w.WriteBoolean("equal",equal);
    if(full){w.WriteBase64String("input",input);w.WriteBase64String("output",sorted);}
    else{w.WriteString("inputSha256",Convert.ToHexStringLower(SHA256.HashData(input)));w.WriteString("outputSha256",Convert.ToHexStringLower(SHA256.HashData(sorted)));}
    w.WriteEndObject();cases++;
}
foreach(int count in new[]{0,1,2,19,20,21,22,41,42,64,127})for(int seed=0;seed<16;seed++)Run(count,seed,false,true);
foreach(int count in new[]{20,21,42})Run(count,7,true,true);
Run(4752,19,false,false);Run(65536,29,true,false);
w.WriteEndArray();
Reject(()=>LegacyPreparedMathCandidateOrdering.Sort(new byte[167]));
Reject(()=>LegacyPreparedMathCandidateOrdering.Sort(new byte[169]));
Reject(()=>LegacyPreparedMathCandidateOrdering.Sort(new byte[65537*168]));
Reject(()=>LegacyPreparedMathCandidateOrdering.Sort(new byte[168],new(true)));
w.WriteStartArray("completionScores");
uint completionRandom=0x4ed92a31;
using var scoredRecords=new MemoryStream();
for(int n=0;n<4096;n++)
{
 completionRandom^=completionRandom<<13;completionRandom^=completionRandom>>17;completionRandom^=completionRandom<<5;
 uint bits=(completionRandom&0x7fffffff);if((bits&0x7f800000)==0x7f800000)bits&=0x7f7fffff;
 if(n==0)bits=0x80000000;if(n==1)bits=0;if(n==2)bits=1;if(n==3)bits=0x7f7fffff;
 int length=n==0?0:n==1?1_000_000:(int)(completionRandom%1_000_001);
 float cost=BitConverter.Int32BitsToSingle(unchecked((int)bits));byte[] input=Make(1,n,false),before=input.ToArray();
 var output=LegacyPreparedMathCompletionScore.WritePreparedRecord(input,cost,length);
 if(!input.AsSpan().SequenceEqual(before)||!output.AsSpan(4).SequenceEqual(input.AsSpan(4)))throw new Exception("Completion score changed opaque record fields.");
 if(BinaryPrimitives.ReadSingleLittleEndian(output)!=LegacyPreparedMathCompletionScore.WithoutConstraintModel(cost,length))throw new Exception("Scalar/record completion score diverged.");
 w.WriteStartObject();w.WriteNumber("sample",n);w.WriteNumber("costBits",bits);w.WriteNumber("prefix",length);w.WriteBase64String("input",input);w.WriteBase64String("output",output);w.WriteEndObject();scoredRecords.Write(output);
}
w.WriteEndArray();w.WriteBase64String("completedSorted",LegacyPreparedMathCandidateOrdering.Sort(scoredRecords.ToArray()));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(1,-1));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(1,1_000_001));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(-1,0));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(float.NaN,0));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(float.PositiveInfinity,0));
Reject(()=>LegacyPreparedMathCompletionScore.WritePreparedRecord(new byte[167],1,0));
Reject(()=>LegacyPreparedMathCompletionScore.WithoutConstraintModel(1,0,new(true)));
w.WriteNumber("caseCount",cases);w.WriteNumber("guards",guards);w.WriteEndObject();
Console.WriteLine($"PASS{cases} complete candidate-ordering controls/{guards} guards; prepared kernel only, no beam integration or accuracy claim.");
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Expected candidate ordering boundary refusal.");}
