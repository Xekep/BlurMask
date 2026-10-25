using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("fake-controls.json capture.json");
byte[] bytes=File.ReadAllBytes(args[0]);if(bytes.Length>2_000_000)throw new ArgumentException("Bounded fake controls required.");
using var fixture=JsonDocument.Parse(bytes);var root=fixture.RootElement;
if(root.GetProperty("Version").GetInt32()!=1)throw new ArgumentException("Version1 fake controls required.");
var mixed=new List<object>();var generic=new List<object>();var sigmoid=new List<object>();
var canceled=new CancellationToken(true);
foreach(string group in new[]{"MixedRows","GenericRows"})
{
 var list=group=="MixedRows"?mixed:generic;int count=0;
 foreach(var row in root.GetProperty(group).EnumerateArray())
 {
  if(++count>256)throw new ArgumentException("Finite probability fixture required.");
  int[] raw=row.GetProperty("Raw").EnumerateArray().Select(v=>v.GetInt32()).ToArray();int[] original=raw.ToArray();
  if(raw.Length>512)throw new ArgumentException("Fake vector bounds exceeded.");
  var token=row.GetProperty("Canceled").GetBoolean()?canceled:default;string exception="none";bool tokenMatches=false;int[] probability=[];int? offset=null;
  try
  {
   if(group=="MixedRows")probability=LegacyClassScores.ConvertMixedProbabilities(raw,row.GetProperty("Special").GetInt32(),token);
   else{var actual=LegacyClassScores.Convert(raw,token);probability=actual.Probability32768.ToArray();offset=actual.StabilizationOffset;}
  }
  catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException)
  {exception=e.GetType().Name;tokenMatches=e is OperationCanceledException oce&&oce.CancellationToken==canceled;}
  var expected=row.GetProperty("Expected");int[] want=expected.GetProperty("Q15").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
  int? wantOffset=expected.GetProperty("Offset").ValueKind==JsonValueKind.Null?null:expected.GetProperty("Offset").GetInt32();
  if(exception!=expected.GetProperty("Exception").GetString()||tokenMatches!=expected.GetProperty("TokenMatches").GetBoolean()||
   !probability.SequenceEqual(want)||offset!=wantOffset||!raw.SequenceEqual(original))throw new InvalidDataException("Independent fake probability controls differ: "+row.GetProperty("Name").GetString());
  list.Add(new{Name=row.GetProperty("Name").GetString(),Exception=exception,TokenMatches=tokenMatches,Q15=probability,Offset=offset,InputUnchanged=true});
 }
}
foreach(var row in root.GetProperty("SigmoidRows").EnumerateArray())
{
 int raw=row.GetProperty("Raw").GetInt32(),actual=LegacyClassScores.MixedSigmoidQ15(raw);
 if(actual!=row.GetProperty("Expected").GetInt32())throw new InvalidDataException("Independent fake sigmoid interval differs.");
 sigmoid.Add(new{Raw=raw,Q15=actual});
}
var output=new{Version=1,Scope="FAKE_PROBABILITY_ONLY; actual compiled converter; no models/weights/NN",FixtureSha256=Convert.ToHexStringLower(SHA256.HashData(bytes)),
 LibrarySha256=Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(LegacyClassScores).Assembly.Location))),MixedRows=mixed,GenericRows=generic,SigmoidRows=sigmoid};
File.WriteAllText(args[1],JsonSerializer.Serialize(output,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"NUMERIC {mixed.Count} mixed/{generic.Count} generic/{sigmoid.Count} sigmoid fake controls; no model execution.");
