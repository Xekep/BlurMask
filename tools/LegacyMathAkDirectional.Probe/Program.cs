using InkRecognition;
using System.Numerics;
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
 var source=profile.CreateGeometry(new[]{points});var features=source.PrepareUnguidedSingleStrokeDirectionalTemporal();
 if(!source.OwnsDirectionalTemporalFeatures(features)||!ReferenceEquals(source.FeatureProfile,profile))throw new Exception("Private source/profile issuer mismatch");
 cases.Add(new{name,original=points.Select(p=>new[]{p.X,p.Y}).ToArray(),normalized=source.CopyNormalizedStrokes()[0].Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray(),features.PreparedPointCount,features.ModelStart,features.FeatureCount,features.HasCandidateAuthority,features.HasClassifierOutput,values=features.CopyValues()});
}
var guards=new List<string>();void Need(bool x,string name){if(!x)throw new Exception(name);guards.Add(name);}
var geometry=profile.CreateGeometry(new[]{input[0].Points});var feature=geometry.PrepareUnguidedSingleStrokeDirectionalTemporal();
var equal=profile.CreateGeometry(new[]{input[0].Points});Need(!equal.OwnsDirectionalTemporalFeatures(feature),"same-profile-foreign-geometry-refused");
var foreign=LegacyMathAkFeatureProfile.PrepareActual(resource).CreateGeometry(new[]{input[0].Points});Need(!foreign.OwnsDirectionalTemporalFeatures(feature),"equal-data-foreign-profile-refused");
var clone=feature.CopyValues();int first=clone[0];clone[0]^=1;Need(feature.CopyValues()[0]==first,"values-defensive-copy");
using(var cancellation=new CancellationTokenSource())
{
 cancellation.Cancel();bool refused=false;try{geometry.PrepareUnguidedSingleStrokeDirectionalTemporal(cancellation.Token);}catch(OperationCanceledException e){refused=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancellation.Token;}Need(refused,"pre-cancel-exact-token");
}
void Refuse(string name,Vector2[][] rows){bool refused=false;try{profile.CreateGeometry(rows).PrepareUnguidedSingleStrokeDirectionalTemporal();}catch(NotSupportedException){refused=true;}Need(refused,name);}
Refuse("single-point-refused",new[]{new[]{Vector2.Zero}});
Refuse("degenerate-refused",new[]{new[]{Vector2.Zero,Vector2.Zero}});
Refuse("multistroke-refused",new[]{input[0].Points,input[1].Points});
Refuse("source-over4096-refused",new[]{Enumerable.Range(0,4097).Select(i=>new Vector2(i%100,i%3)).ToArray()});
Array.Clear(resource);Need(geometry.PrepareUnguidedSingleStrokeDirectionalTemporal().CopyValues().SequenceEqual(feature.CopyValues()),"shared-profile-after-caller-resource-clear");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{cases,guards,modelExecutions=0,vendorExecution=false},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS {cases.Count} directional279 vectors / {guards.Count} guards /0NN");
