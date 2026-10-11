using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

ushort[] bypass=[1,2,3,4,5];var scorer=new LegacyPreparedAcousticCandidateScorer(new(10,20,30),bypass);
int[] classes=[10,10,10,10,10,10,10,10];int[] geometry=[20];
Require(scorer.Evaluate(classes,0,geometry,0,1,0,31).Accepted,"Equality at kind threshold must pass.");
Require(!scorer.Evaluate(classes,0,geometry,0,1,0,30).Accepted,"Equality at total limit must fail.");
Require(scorer.Evaluate(classes,0,geometry,0,0,-50,1).TotalCost==-20,"Explicit negative previous path must not be clamped.");
foreach(ushort special in bypass)
 Require(scorer.Evaluate(classes,special,[],999,0,0,11).Accepted,"Special symbol must bypass category memory access.");
classes[0]=0x4000000;Require(scorer.Evaluate(classes,0,[],999,0,0,1).Rejection==LegacyAcousticCandidateRejection.ImpossibleClass,"Impossible class must reject before geometry lookup.");
classes[0]=11;Require(scorer.Evaluate(classes,0,[],999,1,0,1).Rejection==LegacyAcousticCandidateRejection.KindThreshold,"Kind threshold must reject before geometry lookup.");
classes[0]=10;Reject<ArgumentException>(()=>scorer.Evaluate(classes,0,[],0,0,0,100));
Reject<OverflowException>(()=>scorer.Evaluate(classes,0,geometry,0,0,int.MaxValue,100));
using var cts=new CancellationTokenSource();cts.Cancel();Reject<OperationCanceledException>(()=>scorer.Evaluate(classes,0,geometry,0,0,0,100,cts.Token));
int count=0,profileCount=0;
if(args.Length>0)
{
 using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
 foreach(var test in json.RootElement.GetProperty("cases").EnumerateArray())
 {
  var ts=test.GetProperty("thresholds").EnumerateArray().Select(v=>v.GetInt32()).ToArray();var instance=new LegacyPreparedAcousticCandidateScorer(new(ts[0],ts[1],ts[2]),bypass);
  int[] costs=Enumerable.Repeat(test.GetProperty("cls").GetInt32(),8).ToArray(),geom=Enumerable.Repeat(test.GetProperty("geometry").GetInt32(),35).ToArray();
  var result=instance.Evaluate(costs,(ushort)test.GetProperty("symbol").GetInt32(),geom,test.GetProperty("category").GetInt32(),test.GetProperty("kind").GetInt32(),test.GetProperty("previous").GetInt32(),test.GetProperty("limit").GetInt32());
  Require(result.TotalCost==test.GetProperty("total").GetInt32()&&result.GeometryCost==test.GetProperty("applied").GetInt32()&&result.Accepted==test.GetProperty("accepted").GetBoolean()&&(int)result.Rejection==test.GetProperty("reason").GetInt32(),$"Independent candidate control differs at{count}.");count++;
 }
 foreach(var test in json.RootElement.GetProperty("profiles").EnumerateArray())
 {
  byte[] bytes=Convert.FromBase64String(test.GetProperty("bytes").GetString()!);int consumed=test.GetProperty("consumed").GetInt32(),tail=test.GetProperty("tail").GetInt32(),thresholdStart=test.GetProperty("threshold_start").GetInt32();
  int[] ts=test.GetProperty("thresholds").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
  Require(LegacyAcousticCandidateThresholds.ReadV110Node(bytes,out int used)==new LegacyAcousticCandidateThresholds(ts[0],ts[1],ts[2])&&used==consumed,"Own serialized candidate thresholds/consumption differ.");
  foreach(int cut in new[]{15,tail,thresholdStart,consumed-1})
   Reject<InvalidDataException>(()=>LegacyAcousticCandidateThresholds.ReadV110Node(bytes.AsSpan(0,cut),out _));
  var bad=bytes.ToArray();BinaryPrimitives.WriteInt32LittleEndian(bad.AsSpan(thresholdStart),-1);Reject<InvalidDataException>(()=>LegacyAcousticCandidateThresholds.ReadV110Node(bad,out _));
  bad=bytes.ToArray();bad[12]=1;Reject<InvalidDataException>(()=>LegacyAcousticCandidateThresholds.ReadV110Node(bad,out _));
  profileCount++;
 }
}
if(args.Length>1)
{
 byte[] resource=File.ReadAllBytes(args[1]);var thresholds=LegacyAcousticCandidateThresholds.ReadV110Node(resource.AsSpan(189734),out int used);
 Require(thresholds==new LegacyAcousticCandidateThresholds(20480,24576,32768)&&used==566402-189734,"Actual Russian candidate thresholds/boundary differ.");
 var alphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),0x103);var actualScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(thresholds,alphabet);
 int present=0;int[] classCache=Enumerable.Repeat(1000,alphabet.LabelCount).ToArray();
 foreach(string symbol in new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
 {
  if(!alphabet.TryGetLabel(symbol,out ushort label))continue;
  Require(actualScorer.Evaluate(classCache,label,[],999,0,0,1001).Accepted,"Actual acoustic special symbol did not bypass geometry access.");present++;
 }
 Require(present>0,"No actual special geometry bypass labels found.");
 Console.WriteLine($"Actual RU v110: kind gates20480/24576/32768; {present} of5 native private-use symbol strings present and correctly bypass geometry. No raw neural-to-special cache or word accuracy claim.");
}
Console.WriteLine($"PASS acoustic candidate: {count} independent integer controls, {profileCount} serialized tail profiles; strict total budget, inclusive kind gates, sentinel and symbol bypass ordering, signed prior path, bounds/overflow/cancellation.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject<T>(Action action)where T:Exception{try{action();throw new Exception("Malformed input accepted.");}catch(T){}}
