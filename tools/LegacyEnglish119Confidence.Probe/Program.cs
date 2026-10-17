using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory raw-cache-capture.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));var canceled=new CancellationToken(true);int guards=0,groups=0,partial=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected confidence refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] resource=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(resource);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(resource,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));var rows=new List<Row>();ushort[][] mappings=[];
LegacyOwnedEnglish119GlyphConfidence? first=null;LegacyEnglish119CachedGroup? firstGroup=null;
foreach(var item in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(item.GetProperty("Raw"));var candidates=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw),table);
 var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);var owner=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);mappings=owner.CopyMappings();
 if(!ReferenceEquals(owner.Owner,cache)||!ReferenceEquals(owner.Model,model))throw new Exception("Confidence detached its actual source/model.");
 var output=new List<Group>();foreach(int id in candidates.CopyLiveGroupIds())
 {
  var actual=cache.ScoreGroup(id);int scored=cache.ScoredGroupCount;var result=owner.ScoreGroup(id);var values=result.CopyValues();
  if(!ReferenceEquals(actual,result.ActualCache)||!ReferenceEquals(result,owner.ScoreGroup(id))||cache.ScoredGroupCount!=scored)throw new Exception("Confidence lost its cache or repeated NN.");
  foreach(var v in values)if(owner.ReadSymbolScore(id,v.Symbol)!=v.Value)throw new Exception("Wrong actual scalar lookup.");
  var absent=new List<ushort>();for(int s=0;s<model.Source.Alphabet.LabelCount+3;s++)if(!values.Any(v=>v.Symbol==s)){if(owner.ReadSymbolScore(id,(ushort)s)!=0)throw new Exception("Absent label was guessed.");absent.Add((ushort)s);}
  if(owner.ReadSymbolScore(id,65535)!=0)throw new Exception("Missing sentinel not zero.");
  float probability=owner.ReadStoredSourceProbability(actual,default);if(!actual.WholePhysicalScope)partial++;
  output.Add(new(id,candidates.CopyGroupRanges(id),actual.WholePhysicalScope,probability,actual.Raw.Trajectory.CopyProbabilities(),actual.Raw.Raster.CopyProbabilities(),actual.CopyCandidates(),values.ToArray(),absent.ToArray()));
  if(values.Length!=0){var expected=values[0];values[0]=default;if(result.CopyValues()[0]!=expected)throw new Exception("Mutable private confidence vector.");}
  first??=owner;firstGroup??=actual;groups++;
 }
 if(owner.ScoredGroupCount!=cache.ScoredGroupCount)throw new Exception("Wrong confidence cache lifecycle.");
 rows.Add(new(raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),output));
 Guard(()=>owner.ScoreGroup(-1));Guard(()=>owner.ScoreGroup(candidates.GroupCount));Guard(()=>owner.ScoreGroup(0,canceled));Guard(()=>LegacyOwnedEnglish119GlyphConfidence.Prepare(cache,canceled));
 if(!ReferenceEquals(first,owner))Guard(()=>owner.ReadStoredSourceProbability(firstGroup!,default));
}
var curve=Enumerable.Range(0,32769).Select(LegacyOwnedEnglish119GlyphConfidence.CalibrateProbability).ToArray();
Guard(()=>LegacyOwnedEnglish119GlyphConfidence.CalibrateProbability(-1));Guard(()=>LegacyOwnedEnglish119GlyphConfidence.CalibrateProbability(32769));Guard(()=>LegacyOwnedEnglish119GlyphConfidence.Prepare(null!));
File.WriteAllText(args[2],JsonSerializer.Serialize(new Capture(path,model.Source.Fingerprint,mappings,rows,curve,groups,partial,guards),CaptureJson.Default.Capture));
Console.WriteLine($"Owned119 confidence124/{groups} groups; {partial} actualpartial sourceprobability inspections (not forcedretained211);32769 scalarcontrols/{guards} guards. No extraNN/wordwinner.");
record Group(int Id,LegacyInkRange[] Ranges,bool Whole,float SourceProbability,int[] Trajectory,int[] Raster,LegacyAcousticCacheCandidate[] Candidates,LegacyEnglish119ConfidenceValue[] Values,ushort[] Missing);
record Row(float[][][] Raw,List<Group> Groups);
record Capture(string Path,string Fingerprint,ushort[][] Mappings,List<Row> Rows,float[] Curve,int Groups,int Partial,int Guards);
[JsonSerializable(typeof(Capture))]partial class CaptureJson:JsonSerializerContext;
