using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
return ActualEntry.Run(args);
internal static class ActualEntry
{
 const string InputSha="d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99";
 const int Index=4;
 static readonly Dictionary<string,(int Length,string Hash)> Pins=new()
 {
        ["en-US/en_US-ak-cur.lite.res"] = (545871, "ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657"),
        ["en-US/en_US-lk-text.lite.res"] = (3300812, "9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9"),
        ["line-geometry/line-2e3-30.res"] = (85812, "10f4b43cd02873f1995f543dece70e70c1f22cbdb7dcbf63e1c5c205966ac70e"),
        ["line-geometry/line-2e3-50.res"] = (161332, "01f9fd0bf4b5c0c3c1acc170ee82938e24a132696acbfbcfb34573a9d0550c7e"),
        ["ru-RU/legacy-separation-profile.json"] = (6812, "5efd16a324730bc450613c9208eeffee4c303aff6b7ad5ddae0b5458cbe4ba36"),
        ["text-unicode/legacy-case-data.json"] = (35482, "0a7a5dbf1f6e5c97c860e4b13a5f7533b6e766be92487fa43142a8817e14c3e9"),
        ["text-unicode/legacy-text-publication-data.json"] = (19024, "6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673"),
 };
 internal static int Run(string[] args)
 {
  if(args.Length!=4||args[0]!="--actual-entry")throw new ArgumentException("--actual-entry <models> <pinned-141-raw-json> <fresh-output-json>");
  string models=args[1],input=args[2],output=args[3];if(File.Exists(output))throw new IOException("Fresh output path required.");
  using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(60));var token=stop.Token;var clock=Stopwatch.StartNew();string stage="pins";
  var stages=new List<string>(16);void Stage(string value){token.ThrowIfCancellationRequested();stage=value;if(stages.Count==32)throw new InvalidDataException("Stage log bound exceeded.");stages.Add(value);}
  int graphNodes=-1,children=-1,rawPoints=0;LegacyEnglishCaseNode0Completion? owner=null;
  try
  {
   byte[] Load(string name){var pin=Pins[name];return ReadPinned(Path.Combine(models,name),pin.Length,pin.Hash,token);}
   byte[] raw=ReadPinned(input,checked((int)new FileInfo(input).Length),InputSha,token,8*1024*1024);
   using var json=JsonDocument.Parse(raw);if(json.RootElement.ValueKind!=JsonValueKind.Array||json.RootElement.GetArrayLength()!=141)throw new InvalidDataException("Original pinned141 raw array required.");
   var item=json.RootElement[Index];if(!item.EnumerateObject().Select(p=>p.Name).SequenceEqual(new[]{"Name","Raw","Resolution"}))throw new InvalidDataException("Raw-only exact fixture schema required.");
   var strokes=item.GetProperty("Raw").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(point=>
   {if(point.GetArrayLength()!=2)throw new InvalidDataException("XY only.");float x=point[0].GetSingle(),y=point[1].GetSingle();if(!float.IsFinite(x)||!float.IsFinite(y)||MathF.Abs(x)>100000||MathF.Abs(y)>100000)throw new InvalidDataException("Bounded finite original points required.");return new InkPoint(x,y);}).ToArray())).ToArray();
   rawPoints=strokes.Sum(s=>s.Points.Count);float resolution=item.GetProperty("Resolution").GetSingle();
   if(strokes.Length!=1||rawPoints!=3||resolution!=96)throw new InvalidDataException("Exact unchanged original-index4 metadata required.");
   Stage("source-load");var acoustic=Load("en-US/en_US-ak-cur.lite.res");var source=LegacySource247WireLayout.Read(acoustic);
   var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(acoustic,source,source.Children.First(c=>c.Kind==0x249));
   var lexical=Load("en-US/en_US-lk-text.lite.res");var packed=LegacyEnglishLkPackedData.Read(source,lexical);var language=LegacyEnglishLkSparseLanguageData.Read(packed,lexical);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
   var m30=Load("line-geometry/line-2e3-30.res");var m50=Load("line-geometry/line-2e3-50.res");var table=LegacyCursiveSeparationTable.Load(Load("ru-RU/legacy-separation-profile.json"));
   using var cstream=new MemoryStream(Load("text-unicode/legacy-case-data.json"),false);using var pstream=new MemoryStream(Load("text-unicode/legacy-text-publication-data.json"),false);
   var cases=LegacyNativeCaseData.Load(cstream);var publication=LegacyNativeTextPublicationData.Load(pstream);
   Stage("parent-prepare");var parent=LegacyEnglish119ParentSubmission.Prepare(model,strokes,resolution,m30,m50,token);children=parent.Children.Count;
   Stage("parent-FIRST");var results=new List<LegacyEnglish119PlainTextResult>(children);
   foreach(var child in parent.Children)
   {
    token.ThrowIfCancellationRequested();var candidates=LegacyOwnedEnglish119Candidates.Prepare(child.Submission.Preparation,table,token);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates,token);
    var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache,token);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table,token);
    var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table,token).Run(token);var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary,token).Run(token);
    results.Add(LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts,token),new(new byte[120],new byte[40]),token));
   }
   parent.Publish(results,token);Stage("parent-post");var post=LegacyEnglish119ParentPostprocessing.Prepare(parent,cases,publication,token).Run(token);
   if(post.CopySnapshots()[0].CopyDescriptors().Length!=1)throw new NotSupportedException("Actual fixture did not produce single descriptor; no fallback/skip.");
   Stage("main-local");var finalized=LegacyEnglish119FinalizedSingleSource.Prepare(post,token).Run(token);var local=LegacyEnglish119MainLocalCandidates.Prepare(finalized,table,token).Run(token);
   graphNodes=local.NodeCount;if(graphNodes<2)throw new NotSupportedException("Actual fixture graph has no node1; no prepared substitution.");var knowledge=LegacyEnglish119MainKnowledge.Create(local,token);
   Stage("node1-prepare-model-index");owner=LegacyEnglishCaseNode0Completion.PrepareNode1Spans(knowledge,numeric,cases,publication,token:token);
   Stage("owning-generic-graph-run");var graphResult=owner.RunGraphFirstLoop(token);Stage("capture");var metrics=owner.GraphLoopDiagnostics;
   Save(output,new{Scope="ACTUAL_OWNING_ENGLISH_GENERIC_LOOP_FIRST_LANGUAGE_PHASE1_NOT_FINAL_WORD",Completed=true,InputSha,OriginalIndex=Index,RawPoints=rawPoints,RawStrokes=strokes.Length,Resolution=resolution,GraphNodes=graphNodes,ParentChildren=children,Stages=stages,
    PrefixScope="ONE_OWNING_GENERIC_NODE_SPAN_LOOP_FIRST_ONLY_NOT_FINAL_WORD",Metrics=metrics,
    GraphNode=new{graphResult.Node,graphResult.SourceEnd,graphResult.RetainedEnd,graphResult.OriginalT,graphResult.InitializedN,
     Rows156=Hex(graphResult.CopyRows()),RowMasks156=Hex(graphResult.CopyRowMasks()),PrimaryAlternatives80=Hex(graphResult.CopyAlternatives()),PrimaryAlternativeMasks=Hex(graphResult.CopyAlternativeMasks()),Backing250=Hex(graphResult.CopyHeap()),BackingMasks250=Hex(graphResult.CopyHeapMasks()),Ordinals156=graphResult.CopyOrdinals()},
    NoExpectedLabels=true,Admission="NONE",GlobalReadinessGateUnchanged=true,TimeoutMs=60000});return 0;
  }
  catch(Exception error)
  {
   Save(File.Exists(output)?output+".failure.json":output,new{Scope="ACTUAL_ENTRY_FAILED_NO_SUBSTITUTION",Completed=false,InputSha,OriginalIndex=Index,RawPoints=rawPoints,GraphNodes=graphNodes,ParentChildren=children,Stage=stage,Stages=stages,
    Error=error.GetType().FullName,Message=error.Message,Canceled=token.IsCancellationRequested,TokenMatches=error is OperationCanceledException cancel&&cancel.CancellationToken==token,
    PendingCapture=owner?.HasPendingCaptureForProbe,PendingReceipt=owner?.HasPendingReceiptForProbe,ElapsedMs=clock.ElapsedMilliseconds,NoFallback=true});
   Console.Error.WriteLine(error);return 1;
  }
 }
 static string Hex(byte[] bytes)=>Convert.ToHexStringLower(bytes);
 static void Save(string path,object result)
 {using var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);JsonSerializer.Serialize(file,result);if(file.Length>16*1024*1024)throw new InvalidDataException("Diagnostic capture budget exceeded.");}
 static byte[] ReadPinned(string path,int expected,string hash,CancellationToken token,int limit=4*1024*1024)
 {
  token.ThrowIfCancellationRequested();if(expected<1||expected>limit)throw new InvalidDataException("Bounded pinned input size required.");
  using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);if(file.Length!=expected)throw new InvalidDataException("Pinned input length changed.");var bytes=new byte[expected];int offset=0;
  while(offset<bytes.Length){token.ThrowIfCancellationRequested();int n=file.Read(bytes,offset,Math.Min(65536,bytes.Length-offset));if(n==0)throw new EndOfStreamException();offset+=n;}
  if(file.ReadByte()!=-1||Convert.ToHexStringLower(SHA256.HashData(bytes))!=hash)throw new InvalidDataException("Pinned input digest changed.");token.ThrowIfCancellationRequested();return bytes;
 }
}
