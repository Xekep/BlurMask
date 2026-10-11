using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [ak.res lk.res engine-profile.json]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0;
foreach(var control in controls.RootElement.EnumerateArray())
{
 var row=control.GetProperty("input");
 var input=new LegacyPreparedResultScoreInputs(row[0].GetInt32(),row[1].GetInt32(),row[2].GetInt64(),row[3].GetInt32(),row[4].GetInt32()!=0,row[5].GetSingle(),row[6].GetInt32());
 var score=LegacyPreparedResultScore.Evaluate(input);float[] channels=[score.NormalizedPathCost,score.NormalizedLanguageCost,score.LogMass,score.Mass];
 var expected=control.GetProperty("expected");
 for(int i=0;i<4;i++)
 {
  uint actual=BitConverter.SingleToUInt32Bits(channels[i]),wanted=expected[i].GetUInt32();
  Require(i==3?Math.Abs((long)actual-wanted)<=2:actual==wanted,$"Control{count} channel{i} differs: {actual:x8}/{wanted:x8}.");
 }
 count++;
}
var seed=new LegacyPreparedResultScoreInputs(8192,0,0,0,false,5,0);
Require(LegacyPreparedResultScore.Evaluate(seed).NormalizedPathCost==1,"Source span1 differs.");
Require(LegacyPreparedResultScore.Evaluate(seed with{IncludeExtraSourceNode=true}).NormalizedPathCost==.5f,"Extra source node was ignored.");
Require(LegacyPreparedResultScore.Evaluate(seed with{CumulativePathCost=-8192,SourceScoreMultiplier=1}).Mass>1,"Native unnormalised mass was incorrectly clamped to1.");
Reject(()=>LegacyPreparedResultScore.Evaluate(seed with{SourceNodeEndInclusive=-1}));
Reject(()=>LegacyPreparedResultScore.Evaluate(seed with{SourceNodeEndInclusive=262142,IncludeExtraSourceNode=true}));
Reject(()=>LegacyPreparedResultScore.Evaluate(seed with{CumulativePathCost=int.MaxValue,CandidateCost=1}));
foreach(float invalid in new[]{0f,-1f,float.NaN,float.PositiveInfinity})Reject(()=>LegacyPreparedResultScore.Evaluate(seed with{SourceScoreMultiplier=invalid}));
using(var canceled=new CancellationTokenSource()){canceled.Cancel();try{LegacyPreparedResultScore.Evaluate(seed,canceled.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}}
Console.WriteLine($"PASS {count} independent float32 result controls, clamp boundaries, native cast order, explicit span/multiplier, rejection and cancellation.");
if(args.Length==4)
{
 var model=LegacyRussianModelBundle.Load(File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
 var source=new[]{new InkStroke(new[]{new InkPoint(0,90),new InkPoint(0,10),new InkPoint(25,60),new InkPoint(50,10),new InkPoint(50,90)}),new InkStroke(new[]{new InkPoint(70,10),new InkPoint(70,90),new InkPoint(120,10),new InkPoint(120,90)}),new InkStroke(new[]{new InkPoint(140,90),new InkPoint(140,10),new InkPoint(180,10),new InkPoint(190,20),new InkPoint(190,40),new InkPoint(180,50),new InkPoint(140,50)})};
 var words=new LegacyRussianCursiveWordRecognizer(model,relatedFormMode:0).RecognizeSourceOrderDiagnostic(source,new(90,80,0,1,BeamWidth:32));
 Require(words.Words.Count>0,"Actual variable pipeline produced no word candidates.");
 foreach(var word in words.Words)
 {
  // Caller-owned diagnostic mapping, not a restored native source provider.
  int node=words.Lattice.NodeCount-1;int lexical=checked((int)word.Score.Language.LexicalSubtotal);
  var score=LegacyPreparedResultScore.Evaluate(new(word.Match.AcousticCost,lexical,0,node,false,5,lexical));
  float independent=(float)word.Score.TotalCost/(float)(words.Lattice.NodeCount*8192);
  Require(score.NormalizedPathCost==independent,"Actual word costs were lost in explicit result composition.");
  Require(score.NormalizedLanguageCost==(float)lexical/8192f,"Actual language channel differs.");
 }
 Console.WriteLine($"PASS actual RU neural/cut/dictionary pipeline -> explicit prepared result channels for {words.Words.Count} candidates. Diagnostic source-node mapping; mass is not acceptance confidence.");
}
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Invalid result request accepted.");}catch(ArgumentException){}}
