using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("character-line-controls.json");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 var rows=c.GetProperty("rows").EnumerateArray().Select(r=>new LegacyCharacterLineObservation(r[0].GetDouble(),r[1].GetDouble(),r[2].GetDouble(),r[3].GetDouble(),r[4].GetDouble(),r[5].GetDouble())).ToArray();
 var m=LegacyPreparedCharacterLineEquations.Accumulate(rows);var fit=LegacyPreparedCharacterLineEquations.Solve(m,-100,100);var e=c.GetProperty("solution");Near(fit.HeightScale,e[0].GetDouble());Near(fit.Slope,e[1].GetDouble());Near(fit.Intercept,e[2].GetDouble());
 foreach(var bounds in new[]{(-.05,.05),(-.01,0.0),(0.0,.01),(.4,.5)})
 {
  var v=LegacyPreparedCharacterLineEquations.Solve(m,bounds.Item1,bounds.Item2);Require(v.Slope>=bounds.Item1&&v.Slope<=bounds.Item2,"Constrained slope exceeded bounds.");
  Near(m.W*v.Intercept+m.X*v.Slope+m.Q*v.HeightScale,m.Y);
  if(v.Slope!=0)Near((m.QX*m.Q-m.QQ*m.X)*v.HeightScale+((m.XX+1e-12)*m.Q-m.QX*m.X)*v.Slope,m.Q*m.YX-m.YQ*m.X);
 }
 var zero=LegacyPreparedCharacterLineEquations.Solve(m,-100,100,true);Require(zero.Slope==0,"Explicit force-zero slope ignored.");Near(zero.HeightScale,(m.YQ-m.Y*m.Q/m.W)/(m.QQ-m.Q*m.Q/m.W));
 cases++;
}
var known=LegacyPreparedCharacterLineEquations.Accumulate([new(0,10,90,1,-1,1),new(10,10,90,1,-1,1)]);var solution=LegacyPreparedCharacterLineEquations.Solve(known,-1,1);Require(solution.ToPreparedFitOutput()==(90,80,0),"Known baseline90/xheight80 output semantics differ.");
// Prepared profiles distinguish an ascender and a small detached mark from
// the ordinary body. Physical heights 160 and2 must not become x-height.
LegacyCharacterLineObservation[] variedProfiles=[new(0,10,90,1,-1,1),new(10,-70,90,3,-1,1),new(20,88,90,-.95,-1,1),new(30,10,90,1,-1,1)];
var varied=LegacyPreparedCharacterLineEquations.Solve(LegacyPreparedCharacterLineEquations.Accumulate(variedProfiles),-1,1).ToPreparedFitOutput();
Near(varied.BaselineY,90);Near(varied.XHeight,80);Near(varied.Slope,0);
var resizedProfiles=variedProfiles.Select(o=>o with{CenterX=o.CenterX*3,TopY=(o.TopY+15)*3,BottomY=(o.BottomY+15)*3}).ToArray();
var resized=LegacyPreparedCharacterLineEquations.Solve(LegacyPreparedCharacterLineEquations.Accumulate(resizedProfiles),-1,1).ToPreparedFitOutput();
Near(resized.BaselineY,315);Near(resized.XHeight,240);Near(resized.Slope,0);
foreach(double det in new[]{Math.BitDecrement(1e-10),1e-10,Math.BitIncrement(1e-10)})
{
 // A constructed prepared moment set with determinant exactly det.
 var synthetic=new LegacyCharacterLineMoments(1,0,0,1,det-1e-12,2e-10,0,0,-1,0);
 var s=LegacyPreparedCharacterLineEquations.Solve(synthetic,-100,100);
 if(det<=1e-10)Require(s.Slope==0,"Prepared determinant singular threshold differs.");else Near(s.Slope,2e-10/det);
}
Reject(()=>LegacyPreparedCharacterLineEquations.Solve(default,-1,1));Reject(()=>LegacyPreparedCharacterLineEquations.Solve(known,1,-1));Reject(()=>LegacyPreparedCharacterLineEquations.Accumulate([new(0,0,1,0,1,-1)]));Reject(()=>LegacyPreparedCharacterLineEquations.Accumulate([new(double.NaN,0,1,0,1,1)]));Reject(()=>LegacyPreparedCharacterLineEquations.Accumulate([new(double.MaxValue,0,1,0,1,1)]));Reject(()=>LegacyPreparedCharacterLineEquations.Accumulate([new(0,0,1,0,1,1)],new CancellationToken(true)));
Reject(()=>LegacyPreparedCharacterLineEquations.Solve(known with{QX=double.MaxValue,Q=2},-1,1));Reject(()=>new LegacyCharacterLineSolution(double.MaxValue,0,0).ToPreparedFitOutput());
Console.WriteLine($"PASS {cases} independent weighted design-matrix solves; native moment producer, constrained residuals/force-zero, baseline/xheight output, varied-height prepared profiles/world-scale invariance, malformed/overflow/cancel guards. Prepared C1A candidate producer and iterative orchestration remain separate.");
static void Near(double a,double b){Require(Math.Abs(a-b)<=Math.Max(1,Math.Max(Math.Abs(a),Math.Abs(b)))*1e-10,$"Independent line solve differs:{a:R}/{b:R}");}
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Malformed prepared character-line data accepted.");}
