using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory candidate-raw-capture.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));int guards=0;var canceled=new CancellationToken(true);
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected cache refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
int[][][] Shorts(IEnumerable<LegacyInkStroke>s)=>s.Select(s=>s.Points.ToArray().Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray()).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[]data=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(data);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
//A valid foreign AL keeps its own immutable source/model owner, but must not
//borrow the pinned EN no-derived-Arabic proof merely by keeping217 labels.
byte[] changed=data.ToArray();var alField=model.Source.Fields.Single(f=>f.Owner=="2B"&&f.Destination=="payload");
int at=alField.Offset+16,n=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(changed.AsSpan(at));at+=2;
foreach(int step in new[]{32,64,1}){int last=-1;for(int i=0;i<n;i++){int value=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(changed.AsSpan(at+i*2));if(value!=65535)last=value;}at+=n*2;n=last+step;}
int entries=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(changed.AsSpan(at+n*2));at+=(n+1)*2+(entries+1)*8;
if(changed[at]!=37)throw new Exception("Actual first AL token changed.");changed[at]=1;
var foreignParent=LegacySource247WireLayout.Read(changed);var foreignModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(changed,foreignParent,foreignParent.Children.First(c=>c.Kind==0x249));
var foreignRaw=Ink(input.RootElement.GetProperty("Cases")[0].GetProperty("Raw"));
var foreignSource=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(foreignModel,foreignRaw),table);
Guard(()=>LegacyOwnedEnglish119AcousticCache.Prepare(foreignSource));
byte[] unsupported=data.ToArray();int coefficient=model.Fields.Single(f=>f.Owner=="21D"&&f.Destination=="E0[0]").Offset;
System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(unsupported.AsSpan(coefficient),1.01f);
var unsupportedParent=LegacySource247WireLayout.Read(unsupported);var unsupportedModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(unsupported,unsupportedParent,unsupportedParent.Children.First(c=>c.Kind==0x249));
var unsupportedSource=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(unsupportedModel,foreignRaw),table);
Guard(()=>LegacyOwnedEnglish119AcousticCache.Prepare(unsupportedSource));
var rows=new List<RowCapture>();int total=0;
foreach(var item in input.RootElement.GetProperty("Cases").EnumerateArray())
{
 var raw=Ink(item.GetProperty("Raw"));var prep=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);
 var source=LegacyOwnedEnglish119Candidates.Prepare(prep,table);var owner=LegacyOwnedEnglish119AcousticCache.Prepare(source);var masks=owner.CopyMasks();
 var exposed=owner.CopyMasks();if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(exposed.Primary,out var segment))throw new Exception("Expected detached mask storage.");segment.Array![segment.Offset]=0;
 if(owner.CopyMasks().Primary.Span[0]!=1)throw new Exception("Public mask mutated private ownership.");
 if(!ReferenceEquals(owner.Model,model)||owner.KnowledgeFlags!=0x21700||!owner.PruneCandidates||owner.PositiveGroupCount!=8)throw new Exception("Wrong actual119 policy/source binding.");
 var groups=new List<GroupCapture>();foreach(int id in source.CopyLiveGroupIds())
 {
  var g=owner.ScoreGroup(id);int before=owner.ScoredGroupCount;
  if(!ReferenceEquals(g,owner.ScoreGroup(id))||owner.ScoredGroupCount!=before||!ReferenceEquals(g.Raw.Owner,source))throw new Exception("Cache repeated NN or lost raw ownership.");
  groups.Add(new(id,source.CopyGroupRanges(id),Shorts(g.Raw.Features.CopyMergedStrokes()),g.Raw.Trajectory.CopyProbabilities(),g.Raw.Raster.CopyProbabilities(),
   g.CopyNeuralCosts(),g.CopyFinalCosts(),g.CopyCandidates(),g.FallbackCost,g.WholePhysicalScope,g.EndpointKeysZero,g.SourceProbability));
  var costs=g.CopyFinalCosts();costs[0]=-123456;var candidates=g.CopyCandidates();var originalCandidate=candidates.Length==0?default:candidates[0];if(candidates.Length!=0)candidates[0]=default;
  if(g.CopyFinalCosts()[0]==-123456||candidates.Length!=0&&g.CopyCandidates()[0]!=originalCandidate)throw new Exception("Mutable cached result.");
 }
 if(owner.ScoredGroupCount!=groups.Count)throw new Exception("Wrong per-group cache lifecycle.");total+=groups.Count;
 rows.Add(new(raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),source.CopyCutCounts(),masks.Primary.ToArray(),masks.Secondary.ToArray(),owner.KnowledgeFlags,groups));
 Guard(()=>owner.ScoreGroup(-1));Guard(()=>owner.ScoreGroup(source.GroupCount));Guard(()=>owner.ScoreGroup(0,canceled));
 Guard(()=>LegacyOwnedEnglish119AcousticCache.Prepare(source,canceled));
}
var prepared=new List<PreparedCapture>();var random=new Random(119020);
for(int i=0;i<1024;i++)
{
 int[] costs=Enumerable.Range(0,8).Select(j=>i<512?random.Next(0,110000):((i-512)*127+j*997)%65536).ToArray();
 if(i%11==0)costs[3]=0x4000000;if(i%13==0)costs[2]=-1;
 byte[] mask=Enumerable.Range(0,8).Select(j=>(byte)(j==0||random.Next(4)!=0?1:0)).ToArray();
 float alpha=new[]{.99f,.5f,1f,0f,-.25f}[i%5];
 LegacySymbolMaskKnowledgeGroup[] groups=[new(new ushort[]{0,1,0,2},alpha),new(new ushort[]{2,0,3},.75f),new(new ushort[]{1,2,1},.99f)];
 var result=costs.ToArray();var trace=new List<LegacyAcousticGroupWrite>();LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(result,mask,groups,default,trace);
 prepared.Add(new(costs,mask,groups.Select(g=>g.Symbols.ToArray()).ToArray(),groups.Select(g=>g.Weight).ToArray(),result,trace.ToArray()));
}
Guard(()=>LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(new int[2],new byte[3],[],default));
Guard(()=>LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(new int[2],new byte[]{1,1},[new(new ushort[]{2},.5f)],default));
Guard(()=>LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(new int[2],new byte[]{1,1},[new(new ushort[]{0},1.01f)],default));
Guard(()=>LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(new int[2],new byte[]{1,1},[],canceled));
Guard(()=>LegacyAcousticPositiveGroupSmoothing.ApplyInPlace(new int[]{-2,0},new byte[]{1,1},[new(new ushort[]{0},.5f)],default));
var capture=new CaptureData(path,model.Source.Fingerprint,rows,prepared,total,guards);
File.WriteAllText(args[2],JsonSerializer.Serialize(capture,CaptureContext.Default.CaptureData));
Console.WriteLine($"OwnedEN119 cache: {rows.Count} raw owners/{total} real groups;8 ordered sourcegroups;{prepared.Count} separate prepared duplicate/overlap controls/{guards} guards. No confidence/winner.");
record GroupCapture(int Id,LegacyInkRange[] Ranges,int[][][] Normalized,int[] Trajectory,int[] Raster,int[] Neural,int[] Costs,LegacyAcousticCacheCandidate[] Candidates,int Fallback,bool Whole,bool Zero,float? Probability);
record RowCapture(float[][][] Raw,int[] CutCounts,byte[] Primary,byte[] Secondary,uint Flags,List<GroupCapture> Groups);
record PreparedCapture(int[] Costs,byte[] Mask,ushort[][] Groups,float[] Weights,int[] Output,LegacyAcousticGroupWrite[] Writes);
record CaptureData(string Path,string Fingerprint,List<RowCapture> Rows,List<PreparedCapture> Prepared,int Groups,int Guards);
[JsonSerializable(typeof(CaptureData))]
partial class CaptureContext:JsonSerializerContext;
