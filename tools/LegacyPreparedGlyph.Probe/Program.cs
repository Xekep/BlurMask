using InkRecognition;
using System.Buffers.Binary;
var min=Enumerable.Repeat((short)-1024,274).ToArray();var max=Enumerable.Repeat((short)1024,274).ToArray();
int[] widths=[256,238,313,313];ushort[] kinds=[0x1f,0x2f,0x3f,0x4f];
var classifier=new LegacyDirectionalGlyphClassifier(widths.Select((width,i)=>new LegacyDirectionalAcousticFamily(LegacyInkNetwork.Load(BuildNetwork(width,3,kinds[i])),new float[]{.5f,.5f,.5f},new float[]{1,1,1})).ToArray(),new(new int[48],Enumerable.Repeat(512,128).ToArray()),new(min,max),new(min,max));
var configuration=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(OwnPrefix(),out _);
var endpoint=new LegacyPreparedGlyphClassifier(classifier,configuration);
var prepared=LegacyCursivePreparedInk.Create(new InkStroke[]{new(new InkPoint[]{new(10,10),new(30,50)})}).Insert(0,1,new(10.25f,49.75f),2);
LegacyInkRange[] ranges=[new(0,0,3)];byte[] enabled=[1,1,1,1,0,0,0];
var result=endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,true,enabled,1);
Require(ReferenceEquals(result.Geometry.PreparedInk,prepared)&&result.Geometry.Strokes[0].Points.SequenceEqual(new LegacyInkPoint[]{new(0,-100),new(0,0),new(50,0)})&&result.Geometry.Baseline==new LegacyInkBaseline(0,100),"Composed endpoint skipped exact float source normalization.");
float denominator=0;int contributions=0;int expectedAcoustic=-(int)(8192*Math.Log(683/2048d));
foreach(float weight in result.ResolvedWeights.Span){denominator+=weight;contributions+=(int)((float)expectedAcoustic*weight);}
int expected=(int)((float)contributions*(1f/denominator));
Require(result.ResolvedWeights.Span[0]==.3f&&result.ResolvedWeights.Span[1]==result.DynamicWeights.Mode1Weight&&result.ResolvedWeights.Span[2]==result.DynamicWeights.Mode2Weight&&result.IntegerCosts.Span.ToArray().All(cost=>cost==expected),"Stored dynamic mode0/1/2 or native contribution rounding transport differs.");
Require(result.Scores.FamilyProbabilities.Take(4).All(p=>p is not null)&&result.Scores.FamilyProbabilities.Skip(4).All(p=>p is null),"Four model probability channels differ.");
var masked=endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,true,enabled,1,new byte[]{1,0,1});
Require(masked.IntegerCosts.Span[1]==LegacyAcousticFixedLog.ImpossibleCost&&masked.IntegerCosts.Span[0]==expected,"Explicit enabled classes bypassed integer acoustic mask.");
var noBaseline=endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,false,new byte[]{0,0,1,1,0,0,0},1);
Require(noBaseline.Scores.FamilyProbabilities[0] is null&&noBaseline.ResolvedWeights.Span[0]==0,"Explicit disabled baseline families were evaluated/scored.");
Reject(()=>endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,false,enabled,1));
Reject(()=>endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,true,new byte[]{1},1));
try{endpoint.Classify(prepared,ranges,10,50,40,100,10,[],1024,1024,true,enabled,1,cancellationToken:new CancellationToken(true));throw new Exception("Cancel ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS own composed prepared float ranges -> exact normalized shorts -> four NN probability families -> stored mode0/1/2 dynamic geometry weights -> integer acoustic {expected}; explicit masks/baseline flags, inactive slot rejection, provenance and cancellation.");
if(args.Length==0)return;
if(args.Length!=3)throw new ArgumentException("Usage: LegacyPreparedGlyph.Probe [acoustic.res lexical.res engine-profile.json]");
byte[] acoustic=File.ReadAllBytes(args[0]);var bundle=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]));
var actualConfiguration=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(acoustic.AsSpan(189734),out _);
var actual=new LegacyPreparedGlyphClassifier(bundle.CreateGlyphClassifier(),actualConfiguration);
foreach(var(name,source) in new(string,InkPoint[][])[]{("M",[new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)}]),("I",[new InkPoint[]{new(0,10),new(0,90),new(50,10),new(50,90)}]),("R",[new InkPoint[]{new(0,90),new(0,10),new(40,10),new(50,20),new(50,40),new(40,50),new(0,50)}]),("A",[new InkPoint[]{new(0,90),new(25,10),new(50,90)},new InkPoint[]{new(10,60),new(40,60)}])})
{
 var ink=LegacyCursivePreparedInk.Create(source.Select(p=>new InkStroke(p)).ToArray());
 var selected=source.Select((p,i)=>new LegacyInkRange(i,0,p.Length)).ToArray();
 var scored=actual.Classify(ink,selected,0,90,80,100,10,[],1024,1024,true,enabled,1);
 Require(scored.IntegerCosts.Length==180&&scored.Geometry.Baseline==new LegacyInkBaseline(0,100),"Actual composed profile width/frame differ.");
 foreach(var(scale,dx,dy) in new(int,float,float)[]{(1,-100,300),(2,0,0),(3,200,-100)})
 {
  var transformed=LegacyCursivePreparedInk.Create(source.Select(points=>new InkStroke(points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray())).ToArray());
  var consistent=actual.Classify(transformed,selected,dx,90*scale+dy,80*scale,100,10,[],1024,1024,true,enabled,1);
  Require(consistent.IntegerCosts.Span.SequenceEqual(scored.IntegerCosts.Span),"Consistent source scale/translation changed actual normalized probability costs.");
 }
 var best=scored.IntegerCosts.ToArray().Select((cost,i)=>(cost,i)).OrderBy(p=>p.cost).Take(5).Select(p=>$"{p.i}:{(bundle.AcousticAlphabet.TryGetText((ushort)p.i,out string text)?text:"?")}={p.cost}");
 Console.WriteLine($"ACTUAL own printed {name}, sourceBaseline90/height80, neutral context explicit: overlap={scored.DynamicWeights.Overlap:R}; mode2={scored.DynamicWeights.Mode2Weight:R}; top integer classes {string.Join(", ",best)}.");
}
Console.WriteLine("PASS actual four glyph transport paths; top classes are diagnostic, not representative printed/cursive accuracy.");
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Bad composed input accepted.");}catch(ArgumentException){}}
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
  w.Write((byte)0);w.Write((byte)(i<4?1:0));if(i<4)Network();w.Write((byte)0);
 }
 var bytes=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
 void Network(){w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(3u);w.Write(new byte[3]);}
}
static byte[] BuildNetwork(int inputs,int outputs,ushort kind)
{
    using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);
    writer.Write((ushort)0xfea0);writer.Write((ushort)0x106);writer.Write(0u);writer.Write((byte)0x12);writer.Write(kind);
    writer.Write((uint)inputs);writer.Write(2u);writer.Write(0u);writer.Write(0u);writer.Write((uint)(outputs+1));writer.Write((ushort)10);
    writer.Write(new byte[inputs]);writer.Write(new byte[]{1,1});writer.Write(new byte[2*outputs]);writer.Write(Enumerable.Repeat((byte)1,outputs+1).ToArray());
    var data=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4),(uint)data.Length-8);return data;
}
