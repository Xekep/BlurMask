using System.Numerics;
using System.Text.Json;
using InkRecognition;
using C = InkRecognition.LegacyMathAkUnifiedClassifier;

if (args.Length is < 2 or > 3) throw new ArgumentException("REPOSITORY FRESH_CAPTURE [--actual]");
if (File.Exists(args[1])) throw new IOException("Fresh capture required.");
bool actual = args.Length == 3 && args[2] == "--actual";
if (args.Length == 3 && !actual) throw new ArgumentException("Unknown mode.");
byte[] raw = File.ReadAllBytes(Path.Combine(args[0], "models", "math", "math-ak.res"));
var models = LegacyMathAkSelectedModels.PrepareActual(raw);
Vector2[][] xy = [[new(0,0),new(2,1),new(4,0)]];
var source = models.CreateGeometry(xy);
if (actual)
{
    var classifier = C.Prepare(models, source);
    var result = classifier.Evaluate();
    File.WriteAllText(args[1], JsonSerializer.Serialize(new { scope="fixed-five fresh unmasked guide0 numerical primitive", xy=new int[][]{[0,0],[2,1],[4,0]},
        costs=result.CopyCosts(), rank=result.CopyNativeRank(), symbols=result.CopyTop20Symbols(),
        families=Enumerable.Range(0,5).Select(result.CopyIndividualNetworkCosts).ToArray(),
        result.NetworkExecutions, result.Confidence, result.HasGrammarCandidateAuthority }));
    return;
}
const int Inf=0x04000000;
var guards=new List<string>();
void Check(string name,bool pass) { if(!pass)throw new Exception(name);guards.Add(name); }
void Refuse<T>(string name,Action action) where T:Exception
{ try { action(); } catch(Exception e) when(e.GetType()==typeof(T)){ guards.Add(name);return;}throw new Exception(name); }
int[] Input(int label) { var x=Enumerable.Repeat(5000,262).ToArray();x[label]=1000;return x; }
var p=models.FeatureProfile.ShapePrototypes;
var adjustments=new List<object>();var blends=new List<object>();
for(int label=0;label<259;label++)
{
    int n=p.GetAlternativeCount(label);if(n==0)continue;
    int[] input=Input(label);var a=C.ApplyPrototypeAdjustment(source,input);
    Check("input-unchanged-"+label,input.SequenceEqual(Input(label)));
    adjustments.Add(new{label,a.BestLabel,a.Alternatives,a.SelectedAlternative,a.GeometryCalls,a.Distances,a.Costs});
    for(int ordinal=0;ordinal<n;ordinal++)
    {
        var costs=Input(label);float? factor=p.GetFactor(label,ordinal);var targets=p.CopyTargets(label,ordinal);
        if(factor is not null)C.BlendTargetCosts(costs,label,targets,factor.Value);
        blends.Add(new{label,ordinal,targets,factor,costs});
    }
}
var none=C.ApplyPrototypeAdjustment(source,Input(109));
Check("no-alternatives-no-geometry",none.GeometryCalls==0&&none.Alternatives==0&&none.Costs.SequenceEqual(Input(109)));
var absent=C.ApplyPrototypeAdjustment(source,Enumerable.Repeat(Inf,262).ToArray());
Check("no-best-no-geometry",absent.BestLabel==-1&&absent.GeometryCalls==0);
Check("closest-first-tie",C.SelectClosest([2,1,1])==1&&C.SelectClosest([0,0])==0);
var alias=Input(0);C.BlendTargetCosts(alias,0,[0,1],2);
Check("live-best-self-alias",alias[0]==500&&alias[1]==250);
var signed=Input(0);signed[0]=-3;C.BlendTargetCosts(signed,0,[1],2);
Check("negative-midpoint-away",signed[1]==-2);
Refuse<ArgumentException>("bad-width",()=>C.ApplyPrototypeAdjustment(source,new int[261]));
Refuse<ArgumentException>("bad-cost",()=>{var v=Input(0);v[1]=-1;C.ApplyPrototypeAdjustment(source,v);});
Refuse<ArgumentException>("nan-distance",()=>C.SelectClosest([float.NaN]));
Refuse<ArgumentException>("nan-factor",()=>C.BlendTargetCosts(Input(0),0,[1],float.NaN));
Refuse<ArgumentException>("bad-target",()=>C.BlendTargetCosts(Input(0),0,[259],.75f));
var foreign=LegacyMathAkFeatureProfile.PrepareActual(raw).CreateGeometry(xy);
Refuse<ArgumentException>("foreign-profile-before-features",()=>C.Prepare(models,foreign));
using(var cts=new CancellationTokenSource())
{
    cts.Cancel();bool reached=false;
    try{reached=true;C.ApplyPrototypeAdjustment(source,Input(0),cts.Token);throw new Exception("cancel missing");}
    catch(OperationCanceledException e){Check("precancel-exact-token",reached&&e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cts.Token);}
}
var owned=C.ApplyPrototypeAdjustment(source,Input(0));int first=owned.Costs[0];owned.Costs[0]^=1;
Check("numeric-data-copy-only",C.ApplyPrototypeAdjustment(source,Input(0)).Costs[0]==first);
Array.Clear(raw);Check("cleared-resource-owner",C.ApplyPrototypeAdjustment(source,Input(0)).Costs[0]==first);
File.WriteAllText(args[1],JsonSerializer.Serialize(new {scope="prepared numeric DATA only; genuine source/prototypes; no evaluated Result",modelExecutions=0,
    xy=new int[][]{[0,0],[2,1],[4,0]},adjustments,blends,alias,signed,guards,HasGrammarCandidateAuthority=false}));
Console.WriteLine($"PASS prepared {adjustments.Count} labels/{blends.Count} prototype entries/{guards.Count} guards/0 NN");
