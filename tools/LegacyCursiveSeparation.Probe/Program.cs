using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

float[] table=Enumerable.Repeat(-1f,256).ToArray();
IReadOnlyList<InkStroke> three=[Stroke(0,4),Stroke(1,3),Stroke(0,4)];
var prepared=LegacyCursivePreparedInk.Create(three);LegacyInkRange[] ranges=[new(0,0,2),new(1,0,2),new(2,0,2)];
Require(LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,10,0,1,table,0x4000000)==16384,"Two pen-ups must sum two independent separation costs.");
Require(LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,10,0,1,table,9000)==9000,"Native final maximum cost must cap the sum.");
Require(LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,10,0,1,table,0x4000000,false)==0,"Nonordinary candidate has no separation cost.");
Require(LegacyCursiveGroupSeparationCost.Evaluate(prepared,[ranges[0]],10,0,1,table,0x4000000)==0,"A sole physical range has no pen-up cost.");
var disjoint=LegacyCursivePreparedInk.Create([Stroke(0,0),Stroke(100,100)]);
Require(LegacyCursiveGroupSeparationCost.Evaluate(disjoint,[new(0,0,2),new(1,0,2)],10,0,1,table,0x4000000)==81920,"Below minus4 must use native linear tail, not lookup clamping.");
Require(LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,1,100,1,table,0x4000000)==0,"Above plus4 must have zero contribution.");
var largeIntegral=LegacyCursivePreparedInk.Create([Stroke(0,0),Stroke(8388609,8388609)]);
Require(LegacyCursiveGroupSeparationCost.Evaluate(largeIntegral,[new(0,0,2),new(1,0,2)],1,0,1f/8192,table,0x4000000)==8388609,"Optimized native float-bit rounding must preserve odd integral costs above2^23.");
using var cts=new CancellationTokenSource();cts.Cancel();
Reject<OperationCanceledException>(()=>LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,10,0,1,table,100,cancellationToken:cts.Token));
Reject<ArgumentException>(()=>LegacyCursiveGroupSeparationCost.Evaluate(prepared,[new(0,0,3)],10,0,1,table,100));
Reject<ArgumentException>(()=>LegacyCursiveGroupSeparationCost.Evaluate(prepared,ranges,0,0,1,table,100));
int controls=0,profiles=0;
if(args.Length>0)
{
 using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));var root=json.RootElement;
 var referenceTable=root.GetProperty("table").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
 foreach(var test in root.GetProperty("cases").EnumerateArray())
 {
  var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  var ink=LegacyCursivePreparedInk.Create(strokes);
  var groups=strokes.Select((s,i)=>new LegacyInkRange(i,0,s.Points.Count)).ToArray();
  int cost=LegacyCursiveGroupSeparationCost.Evaluate(ink,groups,test.GetProperty("height").GetSingle(),test.GetProperty("offset").GetSingle(),test.GetProperty("slope").GetSingle(),referenceTable,test.GetProperty("cap").GetInt32());
  Require(cost==test.GetProperty("expected").GetInt32(),$"Independent float32 separation differs at{controls}: {cost}.");controls++;
 }
 foreach(var test in root.GetProperty("profiles").EnumerateArray())
 {
  byte[] resource=Convert.FromBase64String(test.GetProperty("bytes").GetString()!);int offset=test.GetProperty("offset").GetInt32(),fields=test.GetProperty("fields").GetInt32();
  var expected=new LegacyCursiveSeparationProfile(test.GetProperty("gap").GetSingle(),test.GetProperty("slope").GetSingle(),test.GetProperty("cap").GetInt32());
  Require(LegacyCursiveSeparationProfile.ReadV106Node(resource,offset)==expected,"Independent serialized sampler separation fields differ.");
  foreach(int cut in new[]{offset+15,offset+65,fields+198,resource.Length-1})
   Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(resource.AsSpan(0,cut),offset));
  var bad=resource.ToArray();bad[offset+64]=0;Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  bad=resource.ToArray();bad[offset+12]=1;Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  bad=resource.ToArray();bad[offset+14]=5;Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  bad=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(bad.AsSpan(fields+25),float.NaN);Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  bad=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(bad.AsSpan(fields+185),-1);Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  bad=resource.ToArray();BinaryPrimitives.WriteInt32LittleEndian(bad.AsSpan(fields+198),-1);Reject<InvalidDataException>(()=>LegacyCursiveSeparationProfile.ReadV106Node(bad,offset));
  profiles++;
 }
}
if(args.Length>1)
{
 byte[] resource=File.ReadAllBytes(args[1]);
 foreach(int node in new[]{11506,188157})
 {
  var profile=LegacyCursiveSeparationProfile.ReadV106Node(resource,node);
  Require(profile==new LegacyCursiveSeparationProfile(.2f,3,0x4000000),"Actual Russian grouping profile differs.");
 }
 Console.WriteLine("Actual Russian sampler297: gap offset.2, slope3, maximum cost0x4000000 in both profiles; native256float lookup remains explicit.");
}
Console.WriteLine($"PASS ordinary sampler grouping: {controls} independent numeric controls, {profiles} serialized profile controls; prefix/suffix unions, optimized/large range branches, linear/table/zero tails, final cap, explicit candidate kind, source ranges, malformed parameters and cancellation. No automatic line/cursive accuracy claim.");
static InkStroke Stroke(float x,float end)=>new(new InkPoint[]{new(x,0),new(end,0)});
static void Require(bool ok,string reason){if(!ok)throw new InvalidOperationException(reason);}
static void Reject<T>(Action action)where T:Exception{try{action();throw new Exception("Unsupported data accepted.");}catch(T){}}
