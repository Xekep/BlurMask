using InkRecognition;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case-data publication-data line30 line50 manifest output word-index");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cs=File.OpenRead(args[4]);using var ps=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cs);var publication=LegacyNativeTextPublicationData.Load(ps);
using var manifest=JsonDocument.Parse(File.ReadAllBytes(args[8]));
if(manifest.RootElement.GetProperty("commit").GetString()!="c6629026b1fdef51fd08e3d2fddffd82f97e60a5"||
    manifest.RootElement.GetProperty("penUpMetadata").GetString()!="absent")throw new InvalidDataException("Expected explicitly unsegmented tracked coordinates.");
var sample=manifest.RootElement.GetProperty("samples").EnumerateArray().Where(s=>s.GetProperty("kind").GetString()=="word").ElementAt(int.Parse(args[10]));
string file=sample.GetProperty("file").GetString()!;if(file!=Path.GetFileName(file))throw new InvalidDataException("Direct sibling track required.");
byte[] track=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[8]))!,file));
if(track.Length>200000||Convert.ToHexStringLower(SHA256.HashData(track))!=sample.GetProperty("trackSha256").GetString())throw new InvalidDataException("Track hash differs.");
using var parsed=JsonDocument.Parse(track);
var points=parsed.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
if(points.Length is <2 or >4096)throw new InvalidDataException("Bounded recorded points required.");
var timer=Stopwatch.StartNew();string stage="actual first/main input";Exception? failure=null;int nodes=0,groups=0,language=0;
try
{
    //The corpus omits pen-up. This single unsegmented path is an explicit
    //diagnostic assumption, not reconstructed physical-stroke metadata.
    var input=TrackedCaseInput.Create(ak,engine,table,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),[new InkStroke(points)],cases,publication);
    var source=input.Source;
    stage="case dictionary preparation";nodes=source.SourceRanges.Graph.NodeCount;groups=source.SourceRanges.Graph.Groups.Count;
    var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,source,cases,publication);
    var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,source,input.Context,table);
    var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner));
    stage="actual case main pass";ulong identity=0x1000;
    var pass=new LegacyOwnedMainCaseWordPass(ak,policy,owner.MainLineAcoustic,table,r=>{byte[] storage=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(storage);return new(storage,++identity);});
    var result=pass.Run();language=result.Sum(n=>n.LanguageAllocations);stage="completed logical case rows";
}
catch(Exception e)when(e is NotSupportedException or InvalidDataException or ArgumentException or InvalidOperationException)
{failure=e;}
timer.Stop();
//Labels are recorded only after the pipeline. No expected text is passed to it.
var report=new{sample=sample.GetProperty("id").GetString(),points=points.Length,penUpMetadata="absent",stage,nodes,groups,language,milliseconds=timer.ElapsedMilliseconds,
    failure=failure?.ToString(),scope="input/word-row diagnostic only; no emitted text or handwriting accuracy claim"};
File.WriteAllText(args[9],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine(JsonSerializer.Serialize(report));
