using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=3||args[0]!="--actual")throw new ArgumentException("Explicit --actual REVIEWED_PLAN FRESH_CAPTURE required; executes one recognition input.");
if(File.Exists(args[2]))throw new IOException("Fresh capture required.");
using var plan=JsonDocument.Parse(File.ReadAllBytes(args[1]));
var files=new Dictionary<string,byte[]>();
foreach(var f in plan.RootElement.GetProperty("files").EnumerateArray())
{
 string key=f.GetProperty("name").GetString()!,path=f.GetProperty("path").GetString()!;long size=f.GetProperty("bytes").GetInt64();
 if(size<1||size>8_000_000||new FileInfo(path).Length!=size)throw new InvalidDataException("Bounded pinned file required.");
 byte[] data=File.ReadAllBytes(path);
 if(Convert.ToHexStringLower(SHA256.HashData(data))!=f.GetProperty("sha256").GetString())throw new InvalidDataException("Reviewed input fingerprint changed.");
 files.Add(key,data);
}
byte[] R(string key)=>files[key];
using var track=JsonDocument.Parse(R("track"));
int pointCount=track.RootElement.GetProperty("points").GetArrayLength();
if(pointCount is <2 or >4096)throw new InvalidDataException("Bounded unchanged one-stroke input required.");
var points=track.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
var models=LegacyFreshMainRecognitionContext.Load(R("ak"),R("lk"),R("engine"),R("line30"),R("line50"));
var table=LegacyCursiveSeparationTable.Load(R("separation"));
using var cs=new MemoryStream(R("case"));using var ps=new MemoryStream(R("publication"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);
var doc=LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(points)},default);
var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"diagnostic"});original.Run();
var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[0]);selected.Run();
using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);
var source=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
var session=source.Session??throw new InvalidDataException("Actual successful session required.");
int compactCount=source.Selection.Publication.RecordCount2470;
byte[] compact=source.Selection.Publication.CopyCompactStorage();
if(compactCount is <1 or >4096||compact.Length<compactCount*40)throw new InvalidDataException("Diagnostic compact bound exceeded.");
for(int i=0;i<compactCount;i++)if((compact[i*40+32]&8)!=0)throw new InvalidDataException("This diagnostic requires actual zero-eligible input.");
int beforeNN=source.CachedMainGroupEvaluations;
var owner=LegacyFreshMainGlobalNoCandidateSourceCompletion.Prepare(scope,source,new byte[400],default);
using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(20));using var barrier=new Barrier(3);
Outcome Attempt()
{
 barrier.SignalAndWait(deadline.Token);
 try{owner.Run(new byte[4],new byte[4],default);return new(true,null,null);}
 catch(Exception e){return new(false,e.GetType().FullName,e.Message);}
}
var first=Task.Factory.StartNew(Attempt,CancellationToken.None,TaskCreationOptions.LongRunning,TaskScheduler.Default);
var second=Task.Factory.StartNew(Attempt,CancellationToken.None,TaskCreationOptions.LongRunning,TaskScheduler.Default);
barrier.SignalAndWait(deadline.Token);Task.WaitAll(first,second);
var results=new[]{first.Result,second.Result}.OrderByDescending(r=>r.Success).ToArray();
if(results.Count(r=>r.Success)!=1||results[1].Type!="System.InvalidOperationException"||results[1].Message!="Entered live V68 cannot restart.")
 throw new Exception("Expected exactly one successful execution and one pre-entry refusal.");
if(!owner.IsTopV68Completed||owner.IsFaulted||session.IsStopped)throw new Exception("Concurrent loser damaged the successful owner.");
if(source.Selection.Publication.Policy.Owner.AcousticEvaluations!=beforeNN)throw new Exception("Completion unexpectedly reevaluated acoustic models.");
File.WriteAllText(args[2],JsonSerializer.Serialize(new{Scenario="same-old-owner-two-starts",NNSourceInvocations=1,VerifiedNoEligible=true,
 ObservedCompactRecords=compactCount,owner.IsTopV68Completed,owner.IsFaulted,SessionStopped=session.IsStopped,NoAdditionalNN=true,
 Source26=Convert.ToBase64String(owner.CopySourceStorage26()),After120=Convert.ToBase64String(owner.Word.Word.CopyStorage()),owner.QuotaAfter,
 Results=results},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine("PASS one actual owner / two concurrent start attempts / one winner / no additional model evaluation");
readonly record struct Outcome(bool Success,string? Type,string? Message);
