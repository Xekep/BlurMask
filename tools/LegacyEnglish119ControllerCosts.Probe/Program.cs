using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=4)throw new ArgumentException("models-directory actual-cache-capture controls.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));
var canceled=new CancellationToken(true);int guards=0,groups=0,positiveGeometry=0,distinctChannels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException or InvalidDataException){guards++;return;}throw new Exception("Expected controller-cost refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] resource=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(resource);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(resource,parent,parent.Children.First(c=>c.Kind==0x249));
byte[] tableBytes=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json"));var table=LegacyCursiveSeparationTable.Load(tableBytes);
var rows=new List<Row>();LegacyOwnedEnglish119ControllerCosts? first=null,positiveOwner=null;int positiveGroup=-1;
foreach(var item in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(item.GetProperty("Raw"));var candidates=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw),table);
 var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);
 var owner=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);first??=owner;
 if(!ReferenceEquals(owner.Owner,confidence)||!ReferenceEquals(owner.Model,model)||owner.GeometryPrototypeCount!=24||owner.CompositeCategoryCount!=42||
    owner.ReadCompositeCategory(40)!=new LegacyCursiveCompositeCategory(6,7)||owner.CopyFirstCategoryIds()[5]!=6||owner.CostScale!=1)
    throw new Exception("Actual AL/category/source ownership lost.");
 var output=new List<Group>();foreach(int id in candidates.CopyLiveGroupIds())
 {
  var actual=cache.ScoreGroup(id);var scalar=confidence.ScoreGroup(id);int scored=cache.ScoredGroupCount;
  var result=owner.ScoreGroup(id);var entries=result.CopyEntries();
  if(!ReferenceEquals(result.ActualCache,actual)||!ReferenceEquals(result.Confidence,scalar)||!ReferenceEquals(result,owner.ScoreGroup(id))||cache.ScoredGroupCount!=scored)
   throw new Exception("Controller requests lost cached NN identity.");
  var costs=new int[220];var top=new int[220];var primitive=new int[220];var weights=new float[220];var bytes=new byte[220];
  for(ushort s=0;s<220;s++)
  {
   costs[s]=result.ReadClassCost(s,65535,7,false,false);top[s]=result.ReadClassCost(s,65535,7,true,false);
   primitive[s]=result.ReadClassCost(s,65535,7,false,true);weights[s]=result.ReadNormalizedCandidateWeight(s);bytes[s]=result.ReadLikelihoodByte(s);
   if(weights[s]!=scalar.ReadSymbolScore(s))distinctChannels++;
  }
  var queries=new List<Query>();
  void Q(ushort symbol,ushort ordinal,int geometry,bool restrict,bool primitiveCost)
   =>queries.Add(new(symbol,ordinal,geometry,restrict,primitiveCost,result.ReadClassCost(symbol,ordinal,geometry,restrict,primitiveCost)));
  foreach(ushort ordinal in new ushort[]{0,19,20,149,65535}){Q(217,ordinal,0,false,false);Q(0,ordinal,23,true,false);}
  foreach(ushort bypass in new ushort[]{179,182,183,184,189,190,211})Q(bypass,65535,-1,false,true);
  Q(0,65535,23,false,true);Q(217,65535,23,true,false);Q(218,65535,7,false,false);Q(219,65535,7,false,false);
  Guard(()=>result.ReadClassCost(220,65535,7,false,false));Guard(()=>result.ReadClassCost(0,65535,128,false,true));
  Guard(()=>result.ReadClassCost(0,65535,-1,false,true));Guard(()=>result.ReadClassCost(0,65535,24,false,true));
  Guard(()=>result.ReadClassCost(0,65535,0,false,false,canceled));Guard(()=>result.ReadNormalizedCandidateWeight(0,canceled));Guard(()=>result.ReadLikelihoodByte(0,canceled));
  if(result.ReadNormalizedCandidateWeight(65535)!=0||result.ReadLikelihoodByte(65535)!=1)throw new Exception("Missing weight sentinel.");
  if(entries.Length>0){var old=entries[0];entries[0]=default;if(result.CopyEntries()[0]!=old)throw new Exception("Mutable ranked storage escaped.");}
  if(result.GeometryCost>0){positiveGeometry++;positiveOwner??=owner;if(ReferenceEquals(positiveOwner,owner))positiveGroup=id;}
  output.Add(new(id,candidates.CopyGroupRanges(id),actual.CopyFinalCosts(),actual.FallbackCost,actual.CopyCandidates(),result.CopyEntries(),
   scalar.CopyValues(),result.GeometryCost,costs,top,primitive,weights,bytes,queries));groups++;
 }
 if(owner.ScoredGroupCount!=cache.ScoredGroupCount||confidence.ScoredGroupCount!=cache.ScoredGroupCount)throw new Exception("Mismatched completed cache count.");
 int completed=owner.ScoredGroupCount;
 Guard(()=>owner.ScoreGroup(-1));Guard(()=>owner.ScoreGroup(candidates.GroupCount));Guard(()=>owner.ScoreGroup(0,canceled));
 Guard(()=>LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table,canceled));Guard(()=>owner.ReadCompositeCategory(-1));Guard(()=>owner.ReadCompositeCategory(42));
 if(owner.ScoredGroupCount!=completed)throw new Exception("A rejected request published a cache entry.");
 var ids=owner.CopyFirstCategoryIds();ids[0]=999;if(owner.CopyFirstCategoryIds()[0]!=0)throw new Exception("Mutable category storage escaped.");
 rows.Add(new(raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),candidates.CopyCutStrokes().Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),
  candidates.Preparation.Line.Height,owner.GapOffset,owner.GapSlope,owner.GeometryMaximumCost,output));
}
var prepared=new List<Prepared>();
foreach(var control in controls.RootElement.GetProperty("ranked").EnumerateArray())
{
 int[] values=control.GetProperty("costs").EnumerateArray().Select(e=>e.GetInt32()).ToArray();float scale=control.GetProperty("scale").GetSingle();
 var candidates=values.Select((v,i)=>new LegacyAcousticCacheCandidate((ushort)i,v)).ToArray();var ranked=new LegacyOrdinaryRankedAcousticGroup(
  new(new int[220],candidates,10000),model.Source.Alphabet,model.Source.Alphabet.CopySerializedHeader(),new int[24],scale);
 prepared.Add(new(values,scale,ranked.Entries.ToArray(),candidates.Select(c=>ranked.GetFirstControllerLikelihoodByte(c.Symbol)).ToArray()));
}
var boundaryInk=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0)}),new InkStroke(new[]{new InkPoint(0,0)})});
LegacyInkRange[] boundaryRanges=[new(0,0,1),new(1,0,1)];var geometryBoundaries=new List<GeometryBoundary>();
foreach(var value in controls.RootElement.GetProperty("geometry").EnumerateArray())
{
 float offset=value.GetSingle();int cost=LegacyCursiveGroupSeparationCost.Evaluate(boundaryInk,boundaryRanges,1,offset,1,table.Values.Span,0x4000000);
 geometryBoundaries.Add(new(offset,cost));
}
Guard(()=>LegacyOwnedEnglish119ControllerCosts.Prepare(null!,table));Guard(()=>LegacyOwnedEnglish119ControllerCosts.Prepare(first!.Owner,null!));
//ReadOnlyMemory can expose a table's array. An owned service must keep its
//own checked snapshot, and a later Prepare must reject the changed table.
var isolated=LegacyOwnedEnglish119ControllerCosts.Prepare(positiveOwner!.Owner,table);int originalGeometry=positiveOwner.ScoreGroup(positiveGroup).GeometryCost;
if(!MemoryMarshal.TryGetArray(table.Values,out ArraySegment<float> storage))throw new Exception("Table snapshot unavailable.");
float saved=storage.Array![storage.Offset];storage.Array[storage.Offset]=1;
Guard(()=>LegacyOwnedEnglish119ControllerCosts.Prepare(first!.Owner,table));
if(isolated.ScoreGroup(positiveGroup).GeometryCost!=originalGeometry)throw new Exception("Existing private table snapshot changed.");
storage.Array[storage.Offset]=saved;
if(groups!=3241||rows.Count!=124||positiveGeometry==0||distinctChannels==0)throw new Exception("Missing real positive geometry/channel controls.");
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(path,model.Source.Fingerprint,rows,prepared,geometryBoundaries,groups,positiveGeometry,distinctChannels,guards),CaptureJson.Default.Capture));
Console.WriteLine($"Owned119 controller costs124/{groups}; {positiveGeometry} positive geometry groups/{distinctChannels} different weight-confidence values;512 prepared numeric groups/233 geometry bucket discriminators/{guards}guards. No extra NN/winner.");
record Query(ushort Symbol,ushort Ordinal,int Geometry,bool Restrict,bool Primitive,int Cost);
record Group(int Id,LegacyInkRange[] Ranges,int[] FinalCosts,int Fallback,LegacyAcousticCacheCandidate[] Candidates,LegacyRankedAcousticEntry[] Entries,
 LegacyEnglish119ConfidenceValue[] Confidence,int GeometryCost,int[] Lookup,int[] Top,int[] Primitive,float[] Weights,byte[] Bytes,List<Query> Queries);
record Row(float[][][] Raw,float[][][] Cut,float Height,float Gap,float Slope,int Cap,List<Group> Groups);
record Prepared(int[] Costs,float Scale,LegacyRankedAcousticEntry[] Entries,byte[] Bytes);
record GeometryBoundary(float Offset,int Cost);
record Capture(string Path,string Fingerprint,List<Row> Rows,List<Prepared> Prepared,List<GeometryBoundary> GeometryBoundaries,int Groups,int PositiveGeometry,int DistinctChannels,int Guards);
[JsonSerializable(typeof(Capture))]partial class CaptureJson:JsonSerializerContext;
