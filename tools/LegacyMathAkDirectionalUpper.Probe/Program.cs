using InkRecognition;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
if(args.Length!=2 || File.Exists(args[1]))throw new ArgumentException("REPOSITORY FRESH_CAPTURE");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"models/math/math-ak.res"));
var profile=LegacyMathAkFeatureProfile.PrepareActual(resource);
var input=new (string Name,Vector2[] Points)[]{
 ("triangle",new[]{new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}),
 ("horizontal",new[]{new Vector2(0,0),new Vector2(40,0)}),
 ("vertical",new[]{new Vector2(0,0),new Vector2(0,30)}),
 ("nonsquare",new[]{new Vector2(0,0),new Vector2(3,8),new Vector2(4,2)}),
 ("turning",new[]{new Vector2(0,0),new Vector2(5,2),new Vector2(1,6),new Vector2(8,3),new Vector2(2,0)}),
 ("duplicate",new[]{new Vector2(0,0),new Vector2(0,0),new Vector2(2,1),new Vector2(4,0)}),
 ("negative",new[]{new Vector2(8,8),new Vector2(6,4),new Vector2(2,3)})};
var cases=new List<object>();
foreach(var (name,points)in input)
{
 var source=profile.CreateGeometry(new[]{points});var features=source.PrepareUnguidedSingleStrokeDirectionalUpper();
 if(!source.OwnsDirectionalUpperFeatures(features)||!ReferenceEquals(source.FeatureProfile,profile))throw new Exception("Private source/profile issuer mismatch");
 cases.Add(new{name,original=points.Select(p=>new[]{p.X,p.Y}).ToArray(),normalized=source.CopyNormalizedStrokes()[0].Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray(),features.PreparedPointCount,features.ModelStart,features.FeatureCount,features.HasCandidateAuthority,features.HasClassifierOutput,values=features.CopyValues()});
}
var guards=new List<string>();void Need(bool x,string name){if(!x)throw new Exception(name);guards.Add(name);}
var geometry=profile.CreateGeometry(new[]{input[0].Points});var feature=geometry.PrepareUnguidedSingleStrokeDirectionalUpper();
var equal=profile.CreateGeometry(new[]{input[0].Points});Need(!equal.OwnsDirectionalUpperFeatures(feature),"same-profile-foreign-geometry-refused");
var foreign=LegacyMathAkFeatureProfile.PrepareActual(resource).CreateGeometry(new[]{input[0].Points});Need(!foreign.OwnsDirectionalUpperFeatures(feature),"equal-data-foreign-profile-refused");
var clone=feature.CopyValues();int first=clone[0];clone[0]^=1;Need(feature.CopyValues()[0]==first,"values-defensive-copy");
using(var cancellation=new CancellationTokenSource())
{
 cancellation.Cancel();bool refused=false;try{geometry.PrepareUnguidedSingleStrokeDirectionalUpper(cancellation.Token);}catch(OperationCanceledException e){refused=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancellation.Token;}Need(refused,"pre-cancel-exact-token");
}
void Refuse(string name,Vector2[][] rows){bool refused=false;try{profile.CreateGeometry(rows).PrepareUnguidedSingleStrokeDirectionalUpper();}catch(NotSupportedException){refused=true;}Need(refused,name);}
Refuse("single-point-refused",new[]{new[]{Vector2.Zero}});
Refuse("degenerate-refused",new[]{new[]{Vector2.Zero,Vector2.Zero}});
Refuse("multistroke-refused",new[]{input[0].Points,input[1].Points});
Refuse("source-over4096-refused",new[]{Enumerable.Range(0,4097).Select(i=>new Vector2(i%100,i%3)).ToArray()});
Array.Clear(resource);Need(geometry.PrepareUnguidedSingleStrokeDirectionalUpper().CopyValues().SequenceEqual(feature.CopyValues()),"shared-profile-after-caller-resource-clear");
Need(feature.FeatureCount==223&&feature.ModelStart==1629111,"exact5F-feature-scope");
Need(feature.CopyValues()[218..].SequenceEqual(new short[]{-1024,-1024,-1024,1024,1024}),"absent-guide-whole-boundaries");
const BindingFlags binding=BindingFlags.NonPublic|BindingFlags.Static;
var histogram=typeof(LegacyMathAkSourceGeometry).GetMethod("AccumulateUpperDirections",binding)!.CreateDelegate<Histogram>();
var transform=typeof(LegacyMathAkFeatureProfile).GetMethod("TransformUpperMass",binding)!.CreateDelegate<Mass>();
var compose=typeof(LegacyMathAkFeatureProfile).GetMethod("ComposeUnguidedDirectionalUpper",BindingFlags.NonPublic|BindingFlags.Instance)!.CreateDelegate<Compose>(profile);
var octants=new List<object>();
for(int dx=-17;dx<=17;dx++)for(int dy=-17;dy<=17;dy++)
{
 var values=new int[8];histogram(new[]{new LegacyInkPoint(32,32),new LegacyInkPoint((short)(32+dx),(short)(32-dy))},new[]{0},new byte[]{64,128},values,default);
 octants.Add(new{dx,dy,values});
}
var massControls=new List<object>();
foreach(int mass in new[]{int.MinValue,-1048577,-1048576,-1,0,1,1048575,1048576,int.MaxValue})
 foreach(int denominator in new[]{0,1,3,20,1000})massControls.Add(new{mass,denominator,value=transform(mass,denominator)});
var pointPair=new[]{new LegacyInkPoint(0,10),new LegacyInkPoint(10,0)};
var kernelControls=new List<object>();
void Kernel(string name,int[] members,byte firstFlag)
{
 var values=new int[8];histogram(pointPair,members,new[]{firstFlag,(byte)64},values,default);kernelControls.Add(new{name,values});
}
Kernel("empty-members",Array.Empty<int>(),64);Kernel("end2-skip",new[]{0},128);Kernel("singleton3-skip",new[]{0},192);Kernel("last-point-skip",new[]{1},64);Kernel("start1-next-outside-member",new[]{0},64);
int[] global=Enumerable.Range(0,17).Select(i=>i*40000-50000).ToArray();
int[][] cells=Enumerable.Range(0,25).Select(r=>Enumerable.Range(0,8).Select(c=>(r*8+c-100)*100000).ToArray()).ToArray();
var composition=new List<object>();foreach(int arc in new[]{0,20,2000})composition.Add(new{arc,values=compose(global,cells,arc,default)});
File.WriteAllText(args[1],JsonSerializer.Serialize(new{cases,guards,octants,massControls,kernelControls,global,cells,composition,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS {cases.Count} upper223 vectors / {octants.Count} directed controls / {guards.Count} guards /0NN");
delegate void Histogram(ReadOnlySpan<LegacyInkPoint> points,ReadOnlySpan<int> members,ReadOnlySpan<byte> flags,Span<int> values,CancellationToken token);
delegate short Mass(int mass,int denominator);
delegate short[] Compose(ReadOnlySpan<int> global,int[][] cells,int arc,CancellationToken token);
