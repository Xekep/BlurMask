using InkRecognition;
using System.Reflection;
using System.Numerics;
using System.Text.Json;
if(args.Length==3 && args[0]=="--actual")
{
 if(File.Exists(args[2]))throw new IOException("Fresh output required");
 byte[] bytes=File.ReadAllBytes(Path.Combine(args[1],"models/math/math-ak.res"));
 var xy=new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)};
 var models=LegacyMathAkSelectedModels.PrepareActual(bytes);
 var source=models.CreateGeometry(new[]{xy});
 var features=source.PrepareUnguidedSingleStrokeDirectionalUpper();
 var network=models.BindDirectionalUpper(source);
 bytes.AsSpan(1629111,1772466-1629111).Clear();
 var result=network.EvaluateNetworkCosts(features);
 var detached=result.CopyRawOutputs();int first=detached[0];detached[0]^=1;
 if(result.CopyRawOutputs()[0]!=first)throw new Exception("Output alias");
 File.WriteAllText(args[2],JsonSerializer.Serialize(new{scope="actual same-source selected5F numerical channels only",xy=xy.Select(p=>new[]{p.X,p.Y}).ToArray(),features=features.CopyValues(),result.ModelStart,hiddenDots=result.CopyHiddenDots(),hidden=result.CopyHiddenValues(),raw=result.CopyRawOutputs(),probability=result.CopyProbability32768(),costs=result.CopyNetworkCosts8192(),result.HasCandidateAuthority,result.HasUnifiedAlphabetCosts,modelExecutions=1,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));return;
}
if(args.Length!=2 || File.Exists(args[1]))throw new ArgumentException("REPOSITORY FRESH_CAPTURE or --actual REPOSITORY FRESH_CAPTURE");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var owner=LegacyMathAkSelectedModels.PrepareActual(resource);
var geometry=owner.CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
var view=owner.BindDirectionalUpper(geometry);
var foreign=owner.CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
var guards=new List<string>();void Require(bool x,string n){if(!x)throw new Exception(n);guards.Add(n);}
bool rejected=false;try{view.EvaluateRaw(foreign.PrepareUnguidedSingleStrokeDirectionalUpper());}catch(ArgumentException){rejected=true;}Require(rejected,"equal-data-foreign-features-before-forward");
var foreignProfile=LegacyMathAkFeatureProfile.PrepareActual(resource).CreateGeometry(new[]{new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}});
rejected=false;try{owner.BindDirectionalUpper(foreignProfile);}catch(ArgumentException){rejected=true;}Require(rejected,"equal-data-foreign-profile-bind-refused");
using(var cancel=new CancellationTokenSource())
{
 cancel.Cancel();rejected=false;try{view.EvaluateRaw(geometry.PrepareUnguidedSingleStrokeDirectionalUpper(),cancel.Token);}catch(OperationCanceledException e){rejected=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token;}Require(rejected,"pre-cancel-exact-token-before-forward");
}
Require(ReferenceEquals(view.ModelOwner,owner),"same-shared-model-owner");
Require(owner.Models[4].OriginalStart==1629111 && owner.Models[4].InputWidth==223 && owner.Models[4].FirstStride==224 && owner.Models[4].SecondStride==208,"exact5F-matrix-scope");
const BindingFlags f=BindingFlags.Static|BindingFlags.NonPublic;
var run=typeof(LegacyMathAkTemporalNetwork).GetMethod("RunModel",f)!.CreateDelegate<Run>();
var convert=typeof(LegacyMathAkTemporalNetwork).GetMethod("ConvertNetworkRaw",f)!.CreateDelegate<Convert>();
short[] input=Enumerable.Range(0,223).Select(i=>(short)(i*127-12000)).ToArray();
short[] firstWeights=new short[200*224];sbyte[] secondWeights=new sbyte[257*208];byte[] scales=new byte[258];scales[0]=1;
for(int r=0;r<200;r++)for(int c=0;c<223;c++)firstWeights[r*224+c]=(short)(((r+c)%11-5)*1000);
for(int r=0;r<257;r++){scales[r+1]=(byte)(17+r%23);for(int c=0;c<201;c++)secondWeights[r*208+c]=(sbyte)((r*3+c)%7-3);}
var numerical=run(input,firstWeights,secondWeights,scales,224,208,default);var converted=convert(numerical.Raw,default);
//Prepared result construction is diagnostic-only; it grants no source feature/candidate authority.
var rawResult=(LegacyMathAkDirectionalUpperNetwork.NumericalResult)Activator.CreateInstance(typeof(LegacyMathAkDirectionalUpperNetwork.NumericalResult),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{view,numerical.Dots,numerical.Hidden,numerical.Raw},null)!;
var costs=(LegacyMathAkDirectionalUpperNetwork.NetworkCostResult)Activator.CreateInstance(typeof(LegacyMathAkDirectionalUpperNetwork.NetworkCostResult),BindingFlags.Instance|BindingFlags.NonPublic,null,new object[]{rawResult,converted.Probability,converted.Costs},null)!;
var clone=costs.CopyHiddenDots();clone[0]^=1;Require(costs.CopyHiddenDots()[0]==numerical.Dots[0],"prepared-hidden-dots-detached");
var hidden=costs.CopyHiddenValues();hidden[0]^=1;Require(costs.CopyHiddenValues()[0]==numerical.Hidden[0],"prepared-hidden-values-detached");
var rawCopy=costs.CopyRawOutputs();rawCopy[0]^=1;Require(costs.CopyRawOutputs()[0]==numerical.Raw[0],"prepared-raw-detached");
var probability=costs.CopyProbability32768();probability[0]^=1;Require(costs.CopyProbability32768()[0]==converted.Probability[0],"prepared-probability-detached");
var cost=costs.CopyNetworkCosts8192();cost[0]^=1;Require(costs.CopyNetworkCosts8192()[0]==converted.Costs[0],"prepared-cost-detached");
Require(!costs.HasCandidateAuthority && !costs.HasUnifiedAlphabetCosts && ReferenceEquals(costs.Source,geometry),"prepared-result-scope");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{scope="synthetic223 matrix kernel; actual ownership guards only",input,hiddenDots=numerical.Dots,hidden=numerical.Hidden,raw=numerical.Raw,probability=converted.Probability,costs=converted.Costs,guards,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS synthetic223 matrix / {guards.Count} guards / 0 actual NN");
delegate (int[] Dots,short[] Hidden,int[] Raw) Run(ReadOnlySpan<short> input,ReadOnlySpan<short> first,ReadOnlySpan<sbyte> second,ReadOnlySpan<byte> scales,int firstStride,int secondStride,CancellationToken token);
delegate (int[] Probability,int[] Costs) Convert(ReadOnlySpan<int> raw,CancellationToken token);
