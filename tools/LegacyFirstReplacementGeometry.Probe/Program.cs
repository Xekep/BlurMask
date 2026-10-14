using InkRecognition;
using System.Text.Json;
using System.Text.Json.Serialization;
if(args.Length!=3)throw new ArgumentException("AK.res separation-profile.json capture.json");
byte[] resource=File.ReadAllBytes(args[0]);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[1]));
InkStroke[] raw=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var cuts=factory.Frame.Row.GenerateRussianCuts();
var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,factory.Frame.Row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
var owned=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=owned.SourceRanges.Graph;
var geometry=new LegacyFirstOrdinaryTransitionGeometry(profile,factory.Frame,cuts,graph,table);
if(!profile.Alphabet.TryGetLabel("\ufffd",out ushort replacement)||replacement!=189||!profile.Alphabet.TryGetLabel("M",out ushort letter))throw new Exception("Actual owned graph1BE alphabet labels changed.");
var rows=new List<CallbackRow>();int controls=0,strict=0,nonzero=0;
for(int n=0;n<graph.NodeCount;n++)for(ushort a=0;a<graph.GetNode(n).Count;a++)
{
 var arc=graph.GetNode(n)[a];int parent=n-arc.Span-1;if(parent<0)continue;
 foreach(int p in arc.PredecessorArcIndices.Span)foreach(byte kind in new byte[]{0,2,6})foreach(int endpoints in new[]{0,1,2,3})
 {
  ushort current=(endpoints&1)!=0?replacement:letter,prior=(endpoints&2)!=0?replacement:letter;
  byte category=(byte)((n+a)%profile.Categories.CompositeCategories.Length),previousCategory=(byte)((parent+p)%profile.Categories.CompositeCategories.Length);
  var request=new LegacyFirstOrdinaryTransitionRequest(n,a,current,category,parent,(ushort)p,prior,previousCategory,(byte)arc.Span,kind);
  var box=geometry.GetBounds(n,a);var pb=geometry.GetBounds(parent,p);
  var cp=profile.Categories.Prototypes.Span[profile.Categories.CompositeCategories.Span[category].GeometryIndex];var pp=profile.Categories.Prototypes.Span[profile.Categories.CompositeCategories.Span[previousCategory].GeometryIndex];
  int delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(box,pb,cp,pp,factory.Frame.Row.Frame.Line.Height,profile.PairScale,profile.PairOffset,profile.PairMaximum);
  if(delta!=0)nonzero++;
  foreach(int incoming in new[]{int.MinValue,-100,0,100,0x4000000-1,0x4000000,int.MaxValue})
  {
   //Extreme costs are valid for the early no-op routes. Ordinary pair addition
   //is tested only where the native signed-domain sum remains representable.
   if(kind!=6&&endpoints==0&&((long)incoming+delta<int.MinValue||(long)incoming+delta>int.MaxValue))continue;
   var actual=geometry.CorrectLiteral140(request,incoming);
   int expected=kind==6||endpoints!=0?incoming:checked(incoming+delta);bool accepted=kind==6||expected<0x4000000;
   if(actual!=new LegacyFirstOrdinaryTransitionResult(accepted,expected))throw new Exception("Replacement endpoint/kind6 dispatch or limit changed.");
   if(kind!=6&&endpoints==0){if(geometry.Correct140(request,incoming)!=actual)throw new Exception("Strict ordinary API changed.");strict++;}
   else {try{geometry.Correct140(request,incoming);throw new Exception("Strict API admitted literal replacement/kind6.");}catch(NotSupportedException){}catch(ArgumentException){}}
   rows.Add(new(kind,current,prior,incoming,actual.Cost,actual.Accepted,[box.MinimumX,box.MinimumY,box.MaximumX,box.MaximumY],[pb.MinimumX,pb.MinimumY,pb.MaximumX,pb.MaximumY],[cp.First,cp.Second],[pp.First,pp.Second]));controls++;
  }
  using var canceled=new CancellationTokenSource();canceled.Cancel();try{geometry.CorrectLiteral140(request,0,canceled.Token);throw new Exception("Early return skipped cancellation.");}catch(OperationCanceledException){}
 }
}
if(controls==0||strict==0||nonzero==0)throw new Exception("Control graph did not exercise real pair corrections and no-op dispatch.");
File.WriteAllText(args[2],JsonSerializer.Serialize(new CallbackCapture(replacement,factory.Frame.Row.Frame.Line.Height,profile.PairScale,profile.PairOffset,profile.PairMaximum,rows),CaptureJson.Default.CallbackCapture));
Console.WriteLine($"PASS {controls} actual-owned/pruned first140 controls, {strict} strict ordinary regressions; current/prior/both FFFD, kind0/2/6, signed extremes, limit and cancellation. Independent Python audit required; not whole-word accuracy.");
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
record CallbackRow(byte Kind,ushort Current,ushort Prior,int Incoming,int Cost,bool Accepted,float[] A,float[] B,float[] Cp,float[] Pp);
record CallbackCapture(ushort Replacement,float Height,float Scale,float Offset,float Maximum,List<CallbackRow> Rows);
[JsonSourceGenerationOptions(PropertyNamingPolicy=JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CallbackCapture))]
partial class CaptureJson:JsonSerializerContext;
