using InkRecognition.Math;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("capture.json raw-math2.res");
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
w.WriteStartArray("heaps");
foreach(int capacity in new[]{0,1,2,3,4,8,16,32})for(int seed=0;seed<8;seed++)
{
 var initial=new byte[capacity*48];for(int j=0;j<initial.Length;j++)initial[j]=unchecked((byte)(seed*43+j*17));
 var heap=new LegacyPreparedMathCandidateHeap(capacity,initial);uint random=unchecked((uint)(seed*7919+17));
 w.WriteStartObject();w.WriteNumber("capacity",capacity);w.WriteNumber("seed",seed);w.WriteStartArray("offers");
 for(int n=0;n<64;n++)
 {
  random^=random<<13;random^=random>>17;random^=random<<5;
  uint bits=seed==7?0x3f800000:costs[random%costs.Length];
  int slot=heap.TryOffer(BitConverter.Int32BitsToSingle(unchecked((int)bits)));
  w.WriteStartObject();w.WriteNumber("costBits",bits);w.WriteNumber("slot",slot);w.WriteNumber("count",heap.Count);
  w.WriteBase64String("zeroed",heap.CopyStorage());
  if(slot>=0)
  {
   var payload=new byte[44];for(int j=0;j<44;j++)payload[j]=unchecked((byte)(seed*23+n*37+j*19));
   heap.WritePreparedPayload(slot,payload);
  }
  w.WriteBase64String("filled",heap.CopyStorage());w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();
Reject(()=>new LegacyPreparedMathCandidateHeap(-1,[]));
Reject(()=>new LegacyPreparedMathCandidateHeap(33,new byte[33*48]));
Reject(()=>new LegacyPreparedMathCandidateHeap(1,new byte[47]));
var guardHeap=new LegacyPreparedMathCandidateHeap(1,new byte[48]);
Reject(()=>guardHeap.TryOffer(1,new(true)));
if(guardHeap.Count!=0)throw new Exception("Cancelled heap mutated.");
Reject(()=>guardHeap.WritePreparedPayload(0,new byte[44]));
int guardSlot=guardHeap.TryOffer(1);
Reject(()=>guardHeap.WritePreparedPayload(guardSlot,new byte[43]));
guardHeap.WritePreparedPayload(guardSlot,new byte[44]);
Reject(()=>guardHeap.WritePreparedPayload(guardSlot,new byte[44]));
w.WriteStartArray("rawSorts");
for(int count=0;count<=32;count++)for(int seed=0;seed<8;seed++)
{
 var rows=new byte[count*48];uint random=unchecked((uint)(seed*7919+17));
 for(int i=0;i<count;i++)
 {
  for(int j=0;j<48;j++)rows[i*48+j]=unchecked((byte)(seed*23+i*37+j*19));
  random^=random<<13;random^=random>>17;random^=random<<5;
  BinaryPrimitives.WriteUInt32LittleEndian(rows.AsSpan(i*48),seed==7?0x3f800000:costs[random%costs.Length]);
 }
 w.WriteStartObject();w.WriteNumber("count",count);w.WriteNumber("seed",seed);
 w.WriteBase64String("input",rows);w.WriteBase64String("output",LegacyPreparedMathRawCandidateOrdering.Sort(rows));w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("unrestricted");
foreach(int width in new[]{1,2,4,8,16})foreach(int active in new[]{1,Math.Max(1,width/2),width})for(int seed=0;seed<8;seed++)
{
 const int vocabulary=297;var rows=new byte[active*vocabulary*4];var info=new byte[active*8];var conditional=new byte[active*4];
 uint random=unchecked((uint)(seed*7919+17));
 for(int i=0;i<active*vocabulary;i++)
 {
  random^=random<<13;random^=random>>17;random^=random<<5;
  BinaryPrimitives.WriteUInt32LittleEndian(rows.AsSpan(i*4),seed==7?0x3f800000:costs[random%costs.Length]);
 }
 for(int j=0;j<info.Length;j++)info[j]=unchecked((byte)(seed*43+j*17));
 for(int j=0;j<conditional.Length;j++)conditional[j]=unchecked((byte)(seed*23+j*19));
 var result=LegacyPreparedMathUnrestrictedCandidates.Expand(rows,active,vocabulary,width,info,conditional);
 w.WriteStartObject();w.WriteNumber("width",width);w.WriteNumber("active",active);w.WriteNumber("seed",seed);
 w.WriteBase64String("heap",result.CopyHeapStorage());w.WriteBase64String("sorted",result.CopySortedStorage());w.WriteEndObject();
}
w.WriteEndArray();
Reject(()=>LegacyPreparedMathRawCandidateOrdering.Sort(new byte[47]));
Reject(()=>LegacyPreparedMathRawCandidateOrdering.Sort(new byte[33*48]));
Reject(()=>LegacyPreparedMathRawCandidateOrdering.Sort(new byte[48],new(true)));
Reject(()=>LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[4],1,1,1,new byte[8],new byte[4]));
Reject(()=>LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[8],1,2,17,new byte[8],new byte[4]));
Reject(()=>LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[8],1,2,1,new byte[7],new byte[4]));
Reject(()=>LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[8],1,2,1,new byte[8],new byte[3]));
Reject(()=>LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[8],1,2,1,new byte[8],new byte[4],new(true)));
var rawMath=File.ReadAllBytes(args[1]);var gate=LegacyMath2ObservableTokens.BindBundledResource(rawMath);
w.WriteStartArray("observableSlots");foreach(ushort field in gate.CopySlots())w.WriteNumberValue(field);w.WriteEndArray();
byte[] observable=new byte[8192];for(int i=0;i<=ushort.MaxValue;i++)if(gate.IsObservable((ushort)i))observable[i/8]|=(byte)(1<<(i%8));
w.WriteBase64String("observableBits",observable);
ushort[][] prefixPatterns=[[],[262,263],[262,2,263],[268],[190],[296],[1,0]];
uint[] deltas=[0xbf800000,0,0x3f000000,0x447a0000,0x7fc00000,0x7f800000,0xff800000];
w.WriteStartArray("eosHandoffs");
foreach(int width in new[]{1,2,4,8,16})for(int seed=0;seed<4;seed++)for(int pattern=0;pattern<7;pattern++)foreach(uint deltaBits in deltas)
{
 var rows=new byte[width*297*4];
 for(int beam=0;beam<width;beam++)for(int symbol=0;symbol<297;symbol++)
 {
  float cost=1f+((symbol*17+beam*29+seed*13)%100)/16f;
  if(symbol==1)cost=beam*.125f+seed*.25f;if(symbol==5)cost=.5f+beam*.25f;
  BinaryPrimitives.WriteSingleLittleEndian(rows.AsSpan((beam*297+symbol)*4),cost);
 }
 var source=LegacyPreparedMathUnrestrictedCandidates.Expand(rows,width,297,width,new byte[width*8],new byte[width*4]);
 var prefixes=Enumerable.Range(0,width).Select(beam=>prefixPatterns[(pattern+beam)%7]).ToArray();
 var step=LegacyPreparedMathEosHandoff.Apply(source,BitConverter.Int32BitsToSingle(unchecked((int)deltaBits)),gate,prefixes);
 if(!ReferenceEquals(step.Source,source))throw new Exception("Prepared EOS source ownership changed.");
 w.WriteStartObject();w.WriteNumber("width",width);w.WriteNumber("seed",seed);w.WriteNumber("pattern",pattern);w.WriteNumber("deltaBits",deltaBits);
 w.WriteBase64String("pruned",step.CopyAfterPruning());w.WriteBase64String("eos",step.CopyAfterEos());w.WriteBase64String("resorted",step.CopyAfterResort());w.WriteNumber("active",step.ActiveCount);
 w.WriteStartArray("scores");foreach(var score in step.Scores)
 {
  w.WriteStartObject();w.WriteNumber("rank",score.Rank);w.WriteNumber("beam",score.SourceBeam);
  w.WriteNumber("costBits",BitConverter.SingleToUInt32Bits(score.CandidateCost));w.WriteNumber("scoreBits",BitConverter.SingleToUInt32Bits(score.CompletedScore));
  w.WriteStartArray("prefix");foreach(ushort symbol in score.Prefix)w.WriteNumberValue(symbol);w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();
Reject(()=>LegacyMath2ObservableTokens.BindBundledResource([]));
Reject(()=>LegacyMath2ObservableTokens.BindBundledResource(rawMath,new(true)));
var eosSource=LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[297*4],1,297,1,new byte[8],new byte[4]);
var otherVocabulary=LegacyPreparedMathUnrestrictedCandidates.Expand(new byte[2*4],1,2,1,new byte[8],new byte[4]);
Reject(()=>LegacyPreparedMathEosHandoff.Apply(otherVocabulary,0,gate,[[]]));
Reject(()=>LegacyPreparedMathEosHandoff.Apply(eosSource,0,gate,[]));
Reject(()=>LegacyPreparedMathEosHandoff.Apply(eosSource,0,gate,[null!]));
Reject(()=>LegacyPreparedMathEosHandoff.Apply(eosSource,0,gate,[new ushort[1_000_001]]));
Reject(()=>LegacyPreparedMathEosHandoff.Apply(eosSource,0,gate,[[297]]));
var negativeRows=new byte[297*4];BinaryPrimitives.WriteSingleLittleEndian(negativeRows,-1);
var negativeSource=LegacyPreparedMathUnrestrictedCandidates.Expand(negativeRows,1,297,1,new byte[8],new byte[4]);
Reject(()=>LegacyPreparedMathEosHandoff.Apply(negativeSource,0,gate,[[]]));
Reject(()=>LegacyPreparedMathEosHandoff.Apply(eosSource,0,gate,[[]],new(true)));
w.WriteNumber("caseCount",cases);w.WriteNumber("guards",guards);w.WriteEndObject();
Console.WriteLine($"PASS{cases} complete candidate-ordering controls/{guards} guards; prepared kernel only, no beam integration or accuracy claim.");
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected candidate ordering boundary refusal.");}
