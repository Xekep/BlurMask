using InkRecognition;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=3||args[0] is not("--validate-plan" or "--actual"))throw new ArgumentException("--validate-plan PLAN REPORT | --actual PLAN FRESH_DIRECTORY; ROOT ONLY");
using var plan=JsonDocument.Parse(File.ReadAllBytes(args[1]));var files=new Dictionary<string,byte[]>();
foreach(var x in plan.RootElement.GetProperty("files").EnumerateArray())
{
 string path=x.GetProperty("path").GetString()!,name=x.GetProperty("name").GetString()!;long size=x.GetProperty("bytes").GetInt64();
 if(size is<1 or>8000000||new FileInfo(path).Length!=size)throw new InvalidDataException("Bounded pinned complete file required.");
 var bytes=File.ReadAllBytes(path);if(Convert.ToHexStringLower(SHA256.HashData(bytes))!=x.GetProperty("sha256").GetString())throw new InvalidDataException("Resource/input SHA mismatch.");files.Add(name,bytes);
}
string[] resources=["ak","lk","engine","line30","line50","separation","case","publication","normalization","circles"];
if(files.Count!=19||!files.Keys.Order().SequenceEqual(resources.Concat(Enumerable.Range(0,9).Select(i=>"track"+i)).Order()))throw new InvalidDataException("Exact ten resources + unchanged nine recordings required.");
if(args[0]=="--validate-plan"){File.WriteAllText(args[2],JsonSerializer.Serialize(new{Scope="Pinned plan validation only; NO models/NN",Files=files.Count}));return;}
if(Directory.Exists(args[2]))throw new ArgumentException("Fresh output directory required.");Directory.CreateDirectory(args[2]);
byte[] R(string k)=>files[k];var model=ExperimentalGlobalTextRecognizer.Load(R("ak"),R("lk"),R("engine"),R("line30"),R("line50"),R("separation"),R("case"),R("publication"),R("normalization"),R("circles"));
if(!model.UsesCompletedPublication)throw new InvalidDataException("Completed publication adapter required.");
var output=new List<object>();for(int i=0;i<9;i++)
{
 using var d=JsonDocument.Parse(R("track"+i));var points=d.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();if(points.Length is<2 or>1024)throw new InvalidDataException("Unchanged bounded recording required.");
 var exceptions=new List<object>();EventHandler<FirstChanceExceptionEventArgs> handler=(_,e)=>{if(e.Exception is OperationCanceledException or NotSupportedException or InvalidDataException or ArgumentException or InvalidOperationException){lock(exceptions){if(exceptions.Count<256)exceptions.Add(new{Type=e.Exception.GetType().FullName,e.Exception.Message});}}};
 AppDomain.CurrentDomain.FirstChanceException+=handler;var sw=Stopwatch.StartNew();TextRecognition r;ExperimentalCompletedTextDiagnostic diagnostic;try{r=model.RecognizeCompletedDiagnostic([new InkStroke(points)],out diagnostic);}finally{sw.Stop();AppDomain.CurrentDomain.FirstChanceException-=handler;}
 object[] exceptionSnapshot;lock(exceptions)exceptionSnapshot=exceptions.ToArray();
 output.Add(new{Index=i,Points=points.Length,r.Text,ScoreBits=BitConverter.SingleToInt32Bits(r.Confidence),r.Frames,r.ExperimentalCandidateScore,Milliseconds=sw.Elapsed.TotalMilliseconds,FirstChance=exceptionSnapshot,Completed=new{diagnostic.Stage,diagnostic.Route,diagnostic.SourceInvocations,diagnostic.CachedAcousticEvaluations,diagnostic.CurrentAcousticEvaluations,diagnostic.Reason,diagnostic.ExceptionType,diagnostic.ExceptionMessage,diagnostic.CapturedUtf8Bytes,diagnostic.Utf8CaptureTruncated,diagnostic.FullCoverage,diagnostic.SelectedTopCount,diagnostic.Actual275PayloadCount,diagnostic.AllWordChoicesPublished,diagnostic.PublishedWordChoiceCount,diagnostic.SelectedWordOrdinal,diagnostic.AllWordEnumeratedArrayEnvelopeBytes,WordOriginalRanges=diagnostic.WordOriginalRanges.Select(r=>new{First=r.First.Value,Last=r.Last.Value}).ToArray(),FullInputRanges=diagnostic.FullInputRanges.Select(r=>new{First=r.First.Value,Last=r.Last.Value}).ToArray(),Glyphs=diagnostic.Glyphs.Select(g=>new{g.Reference,PoolRecord40=Convert.ToBase64String(g.ActualPoolRecord40),NativeScore16Bits=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(g.ActualPoolRecord40.AsSpan(16)),SelectedTextUtf8=Convert.ToBase64String(g.SelectedTextUtf8)}).ToArray()},Scope="ONE public2s all275 call; unchanged selectedTOP/noargmax; stage/reason and native16/selectedTEXT/coverage are DATA; no inferred accuracy"});
 File.WriteAllText(Path.Combine(args[2],"capture.json"),JsonSerializer.Serialize(new{Scope="Root-only nine unchanged recordings; ONE public invocation each; no targets/model tuning; NO diagnostic30 repeat",CompletedPublication=model.UsesCompletedPublication,Samples=output},new JsonSerializerOptions{WriteIndented=true}));
}
