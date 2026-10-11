using System.Buffers.Binary;
using InkRecognition;
using System.Text.Json;
var profile=new LegacyDynamicAcousticWeightProfile(21,.5f,.01f,.75f,.2f,.6f,5,.1f,.9f);
InkStroke[] dots=[new(new InkPoint[]{new(0,0)}),new(new InkPoint[]{new(0,0)})];
var dot=LegacyDynamicAcousticWeights.Evaluate(dots,profile);
float dotMode2=(float)((1/(Math.Exp(((1-0d)-profile.LogisticCenter)*-(double)profile.LogisticSlope)+1))*Math.Abs((double)(profile.UpperWeight-profile.LowerWeight))+profile.LowerWeight);
Require(dot.Overlap==0&&dot.ResampledPointCount==2&&dot.Mode2Weight==dotMode2&&dot.Mode1Weight==1-dotMode2,"Degenerate physical stroke or configured logistic endpoints differ.");
InkStroke[] parallel=[new(new InkPoint[]{new(0,0),new(100,0)}),new(new InkPoint[]{new(0,1),new(100,1)})];
var near=LegacyDynamicAcousticWeights.Evaluate(parallel,profile);
Require(near.Overlap>0&&near.Mode2Weight<dot.Mode2Weight&&near.Mode1Weight==1-near.Mode2Weight,"Close parallel geometry must alter complementary weights.");
var neutral=LegacyDynamicAcousticWeights.Evaluate(parallel,profile with{LogisticSlope=0,LowerWeight=.2f,UpperWeight=.8f});
Require(neutral.Mode2Weight==.5f&&neutral.Mode1Weight==.5f,"Zero logistic slope configured midpoint differs.");
var reset=LegacyDynamicAcousticWeights.Evaluate(parallel,profile with{LogisticSlope=0,LowerWeight=.6f,UpperWeight=.6f});
Require(reset.Mode2Weight==.5f,"Native equal-endpoint constructor reset differs.");
var shifted=LegacyDynamicAcousticWeights.Evaluate(parallel.Select(stroke=>new InkStroke(stroke.Points.Select(p=>new InkPoint(p.X+250,p.Y-100)).ToArray())).ToArray(),profile);
Require(Math.Abs(near.Overlap-shifted.Overlap)<2e-6&&Math.Abs(near.Mode2Weight-shifted.Mode2Weight)<2e-6,"Translation changed local overlap weights.");
foreach(var invalid in new[]{profile with{SampleCount=1},profile with{SineLimit=0},profile with{RadiusFraction=float.NaN},profile with{UpperWeight=2}})
{try{LegacyDynamicAcousticWeights.Evaluate(dots,invalid);throw new Exception("Bad config accepted.");}catch(ArgumentException){}}
try{LegacyDynamicAcousticWeights.Evaluate(parallel,profile,new CancellationToken(true));throw new Exception("Cancel ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS type257 own geometry: close parallel overlap={near.Overlap:R}, complementary weights={near.Mode1Weight:R}/{near.Mode2Weight:R}; logistic/endpoint reset, translation, zero arc, bounds and cancellation. Explicit config, no baseline/default/resource selection claim.");
// Own v110 prefix gives nontrivial mode0/1/2 resolution without resources.
var ownPrefix=OwnPrefix();var stored=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(ownPrefix,out int used);
Require(stored.StoredContextSelector==0 && stored.ContextSelectionMode==LegacyNativePriorContextMode.AllContained,"Own context selector forwarding differs.");
// Own network payload starts after byte20, its11-byte header/payload ends32;
// serialized context selector is21 bytes into the24-byte initial parameters.
foreach(byte mode in new byte[]{0,1,2,255})
{
 var variant=ownPrefix.ToArray();variant[32+21]=mode;
 var config=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(variant,out _);
 Require(config.StoredContextSelector==mode && config.ContextSelectionMode==(mode<2 ? (LegacyNativePriorContextMode)mode:LegacyNativePriorContextMode.SameLineAll),"Stored context selector normalization differs.");
}
Require(used==ownPrefix.Length&&stored.DynamicProfile.SampleCount==30&&stored.ResourceCostMultiplier==1&&stored.Slots.Span[0].StoredWeight==.3f,"Own serialized prefix consumed/mapped incorrectly.");
var weights=stored.ResolveWeights(new(.25f,.7f,.3f,10));
Require(weights.AsSpan().SequenceEqual(new float[]{.3f,.7f,.3f,0,0,0,0}),"Stored mode0/1/2 resolution differs.");
for(int cut=0;cut<ownPrefix.Length;cut++)
{try{LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(ownPrefix.AsSpan(0,cut),out _);throw new Exception("Truncated prefix accepted.");}catch(InvalidDataException){}}
var wrong=ownPrefix.ToArray();wrong[14]=0x11;
try{LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(wrong,out _);throw new Exception("Unknown version accepted.");}catch(InvalidDataException){}
Console.WriteLine("PASS own serialized v110 prefix, seven mode0/1/2 weights, exact consumption, all truncations and unsupported version.");
if(args.Length==2&&args[0]=="--resource")
{
 byte[] resource=File.ReadAllBytes(args[1]);
 var actual=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(resource.AsSpan(189734),out int consumed);
 Require(actual.StoredContextSelector==2 && actual.ContextSelectionMode==LegacyNativePriorContextMode.SameLineAll && resource[195767]==2,"Actual RU native context selector must be2, same-line-all.");
 var expectedProfile=new LegacyDynamicAcousticWeightProfile(30,.15f,.30f,.40f,.50f,.95f,25,.20f,1);
 Require(consumed==566330-189734&&actual.DynamicProfile==expectedProfile&&actual.ResourceCostMultiplier==1,"Actual type21d native prefix profile/consumption differs.");
 for(int i=0;i<7;i++){var slot=actual.Slots.Span[i];Require(i<4?slot.Mode==2&&slot.NetworkMask==2&&slot.FirstFlag&&slot.SecondFlag:slot.Mode==0&&slot.StoredWeight==0&&slot.NetworkMask==0&&!slot.FirstFlag&&!slot.SecondFlag,"Actual seven-family modes/flags/network mask differs.");}
 var geometric=LegacyDynamicAcousticWeights.Evaluate(parallel,actual.DynamicProfile);
 var resolved=actual.ResolveWeights(geometric);
 Require(resolved.AsSpan(0,4).ToArray().All(value=>value==geometric.Mode2Weight)&&resolved.AsSpan(4).ToArray().All(value=>value==0),"Actual dynamic mode2 resolution differs.");
 Console.WriteLine($"PASS actual resource type21d v110 prefix through566330: sample30 radius.15 exclusion.30 sameStroke.40 sine.50 center.95 slope25 lower.20 upper1; four dynamic mode2 NN slots then three zero slots. Own parallel ink overlap={geometric.Overlap:R}, resolved={geometric.Mode2Weight:R}. No recognition accuracy claim.");return;
}
if(args.Length==0)return;
if(args.Length!=2||args[0]!="--controls")throw new ArgumentException("Usage: LegacyDynamicWeights.Probe [--controls controls.json]");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[1]));int count=0;
foreach(var c in document.RootElement.GetProperty("cases").EnumerateArray())
{
 var p=c.GetProperty("profile").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
 var config=new LegacyDynamicAcousticWeightProfile((int)p[0],p[1],p[2],p[3],p[4],p[5],p[6],p[7],p[8]);
 var input=c.GetProperty("strokes").EnumerateArray().Select(stroke=>new InkStroke(stroke.EnumerateArray().Select(point=>new InkPoint(point[0].GetSingle(),point[1].GetSingle())).ToArray())).ToArray();
 var actual=LegacyDynamicAcousticWeights.Evaluate(input,config);var expected=c.GetProperty("expected");
 Require(actual.ResampledPointCount==expected[3].GetInt32()&&Math.Abs(actual.Overlap-expected[0].GetSingle())<2e-6&&Math.Abs(actual.Mode1Weight-expected[1].GetSingle())<2e-6&&Math.Abs(actual.Mode2Weight-expected[2].GetSingle())<2e-6,$"Independent float geometry/weights case{count} differs: {actual} vs{expected}.");count++;
}
Require(count>0,"Empty references.");Console.WriteLine($"PASS {count} independent float32 resampling, geometric overlap and configured dynamic weight controls.");
static byte[] OwnPrefix()
{
 using var stream=new MemoryStream();using var w=new BinaryWriter(stream);
 w.Write(0x0101fe90u);w.Write(0u);w.Write(0x21du);w.Write(0x01100000u);
 w.Write(new byte[]{255,255});w.Write((ushort)100);w.Write((byte)1);Network();
 w.Write(new byte[24]);w.Write((byte)1);w.Write((ushort)30);
 foreach(float value in new float[]{.15f,.30f,.40f,.50f,.95f,25,.20f,1,.10f})w.Write(value);
 w.Write((byte)1);w.Write((byte)0);w.Write(new byte[44]);
 for(int i=0;i<3;i++)w.Write((ushort)0);w.Write(new byte[16]);w.Write((byte)0);w.Write((byte)0);
 for(int i=0;i<7;i++)
 {
  w.Write((byte)0);w.Write((byte)0);w.Write((byte)0);w.Write((byte)(i==1?1:i==2?2:0));
  if(i!=1&&i!=2)w.Write(i==0?.3f:0f);
  w.Write((byte)0);w.Write((byte)0);w.Write((byte)0);
 }
 var bytes=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
 void Network(){w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(3u);w.Write(new byte[3]);}
}
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
