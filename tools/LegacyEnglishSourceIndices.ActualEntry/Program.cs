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
        ["text-unicode/legacy-normalization-data.json"] = (57864, "3cc1d91269959a13b5700b7463d3db7442b83b593ba70435a94665d6531226f2"),
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
  if(args.Length!=8||args[0] is not("--validate-plan" or "--actual-source-indices"))throw new ArgumentException("--validate-plan|--actual-source-indices <models> <pinned-141-raw-json> <pinned-prior-char-json> <pinned-prior-text-json> <pinned-prior-early-selected-json> <pinned-prior-selected-json> <fresh-output-json>");
  bool validateOnly=args[0]=="--validate-plan";
  string models=args[1],input=args[2],priorPath=args[3],priorTextPath=args[4],priorPostPath=args[5],priorSelectedPath=args[6],output=args[7];if(File.Exists(output))throw new IOException("Fresh output path required.");
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
   const string PriorSha="ea0165ba621fa102441d3ae53f366c99dc9c678662d99aa5600e578d91b6c2bb";
   var priorBytes=ReadPinned(priorPath,checked((int)new FileInfo(priorPath).Length),PriorSha,token,32*1024*1024);
   using var priorDoc=JsonDocument.Parse(priorBytes);var prior=priorDoc.RootElement;
   if(prior.GetProperty("Completed").ValueKind!=JsonValueKind.True||prior.GetProperty("CHARCreated").ValueKind!=JsonValueKind.True||prior.GetProperty("TEXTCreated").ValueKind!=JsonValueKind.False)throw new InvalidDataException("Prior one-choice CHAR actual required.");
   const string PriorTextSha="1bb645b6c029541fc9ce345d74718b836f404694ee9bf7ab646312a4bfc577d9";
   var priorTextBytes=ReadPinned(priorTextPath,28374,PriorTextSha,token,32*1024*1024);
   using var priorTextDoc=JsonDocument.Parse(priorTextBytes);var priorText=priorTextDoc.RootElement;
   if(!priorText.GetProperty("Completed").GetBoolean()||!priorText.GetProperty("TEXTCreated").GetBoolean()||priorText.GetProperty("DocumentApplied").GetBoolean())throw new InvalidDataException("Pinned pending-TEXT prior required.");
   const string PriorPostSha="beaef516f78e14ff9f01a0fdb7d5a9bb8746766b24858d7f6836218e6696cb91";
   var priorPostBytes=ReadPinned(priorPostPath,29282,PriorPostSha,token,32*1024*1024);
   using var priorPostDoc=JsonDocument.Parse(priorPostBytes);var priorPost=priorPostDoc.RootElement;
   if(!priorPost.GetProperty("Completed").GetBoolean()||priorPost.GetProperty("DocumentApplied").GetBoolean())throw new InvalidDataException("Completed pinned prior post-TEXT required.");
   const string PriorSelectedSha="530dfb0a0d59b18d23e51b7a89677ba1ab6cb237b3ea07550bb36d08d1828818";
   var priorSelectedBytes=ReadPinned(priorSelectedPath,30088,PriorSelectedSha,token,32*1024*1024);
   using var priorSelectedDoc=JsonDocument.Parse(priorSelectedBytes);var priorSelected=priorSelectedDoc.RootElement;
   if(Environment.Version.ToString()!="10.0.12")throw new InvalidDataException("Pinned Managed runtime10.0.12 required.");
   var core=typeof(object).Assembly.Location;
   if(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(core)))!="1125acc8106c43fc8bad2d203c4c4485df6182d292846c2fff415c1040c54678")throw new InvalidDataException("Corelib pin changed.");
   if(validateOnly)
   {
    foreach(var name in Pins.Keys){token.ThrowIfCancellationRequested();_ = Load(name);}
    Save(output,new{Scope="VALIDATE_SOURCE_INDICES_FILES_ONLY_NO_MODEL_LOADING",Validated=true,InputSha,OriginalIndex=Index,RawPoints=rawPoints,RawStrokes=strokes.Length,Resolution=resolution,PriorCharSha=PriorSha,ResourcePins=Pins.Count,ModelParsingEntered=false,ParentPreparationEntered=false,OwningRunEntered=false,NN=0,RuntimeCalls=0,OwnerCalls=0,ModelLoads=0});return 0;
   }
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
   var vector10=new List<LegacyEnglishCaseNode0Completion.GraphMainConfidenceObservation>();
   void Observe(LegacyEnglishCaseNode0Completion.GraphMainConfidenceObservation value)
   {token.ThrowIfCancellationRequested();if(vector10.Count>=64)throw new InvalidDataException("Bounded diagnostic group observations required.");vector10.Add(value);}
   using var normalizationStream=new MemoryStream(Load("text-unicode/legacy-normalization-data.json"),false);var normalization=LegacyNativeUnicodeNormalizationData.Load(normalizationStream);
   Stage("owning-generic-graph-run");
   var sourceIndices=owner.RunGraphFirstSourceIndices(normalization,token,Observe,captureSourceInputs:true);
   var selectedSources=sourceIndices.Publication;
   var early=selectedSources.Publication;
   var posttext=early.Publication;
   var joined=posttext.Publication;
   var chars=joined.Words.LastChoiceCharacters;var graphResult=chars.Geometry.Glyphs;
   Stage("capture");var metrics=owner.GraphLoopDiagnostics;
   //Old selected choice0 is compared at its historical first-choice snapshot.
   //Its final payload may legitimately change on later275 reuse; not equated.
   if(joined.Words.Word.SelectedOrdinal!=0||joined.Words.Word.Choices.Length<1)throw new NotSupportedException("Pinned selected0 before reuse required; no substitution.");
   var result=new{Scope="ACTUAL_OWNING_ENGLISH_ALL275_WORD_GUIDE_NULL_SELECTED_PENDING_TEXT_NO_DOCUMENT_APPLY",Completed=true,InputSha,OriginalIndex=Index,RawPoints=rawPoints,RawStrokes=strokes.Length,Resolution=resolution,GraphNodes=graphNodes,ParentChildren=children,Stages=stages,
    Metrics=metrics,SourceCompletion=graphResult.Choices.Top.Source,Top=graphResult.Choices.Top.Top,Vector10=vector10,
    Word=joined.Words.Word,Text=joined.Text,LastChoiceCharacters=chars,
    FirstChoiceCharacterPrefixes=joined.Words.Word.Choices[0].Characters,
    CHARCreated=true,WORDCreated=true,TEXTCreated=true,DocumentApplied=false,
    PriorCharSha=PriorSha,PriorCanonicalFieldsMatched=true,OldCharFirstChoicePrefixMatched=true,LaterFinalPoolEqualityClaimed=false,
    ParentPreparationMayRunLineModels=true,NoExpectedLabels=true,Admission="NONE",GlobalReadinessGateUnchanged=true,TimeoutMs=60000,
    LifecycleScope="ONE_NORMAL_OWNING_PASS_NO_EXTRA_CANCEL_OR_POISON_INJECTION",NoRetry=true,CharacterContextAliveAtRegistrationSourceWitness=joined.Text.CharacterContextAliveAtRegistration,PostText=posttext.Coverage,EarlySelected=early.Bootstrap,SelectedSources=selectedSources.Sources,SourceIndices=sourceIndices.Indices};
   var element=JsonSerializer.SerializeToElement(result);
   try{foreach(string name in BaselineFields)Equal(element.GetProperty(name),prior.GetProperty(name));
    Equal(element.GetProperty("FirstChoiceCharacterPrefixes"),prior.GetProperty("Characters").GetProperty("PerReferencePrefixes"));
    var priorNames=priorText.EnumerateObject().Select(p=>p.Name).ToArray();
    if(element.EnumerateObject().Count()!=priorNames.Length+4||priorNames.Contains("PostText"))throw new InvalidDataException("Only PostText may extend the old whole capture.");
    foreach(var property in priorText.EnumerateObject())Equal(element.GetProperty(property.Name),property.Value);
    if(element.EnumerateObject().Count()!=priorPost.EnumerateObject().Count()+2)throw new InvalidDataException("Only SelectedSources and SourceIndices may extend exact prior early-selected capture.");
    foreach(var property in priorPost.EnumerateObject())Equal(element.GetProperty(property.Name),property.Value);
    if(element.EnumerateObject().Count()!=priorSelected.EnumerateObject().Count()+1)throw new InvalidDataException("Only SourceIndices may extend exact immediate prior selected-source capture.");
    foreach(var property in priorSelected.EnumerateObject())Equal(element.GetProperty(property.Name),property.Value);}
   catch(InvalidDataException error)
   {
    // Preserve the one genuine result even when baseline comparison refuses;
    // never require a second NN run just to see its changed fields.
    using var file=new FileStream(output,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
    using(var writer=new Utf8JsonWriter(file))
    {writer.WriteStartObject();foreach(var property in element.EnumerateObject())
     {if(property.Name is "PriorCanonicalFieldsMatched" or "OldCharFirstChoicePrefixMatched")writer.WriteBoolean(property.Name,false);else property.WriteTo(writer);}
     writer.WriteString("BaselineComparisonError",error.Message);writer.WriteEndObject();}
    throw;
   }
   Save(output,result);return 0;
  }
  catch(Exception error)
  {
   Save(File.Exists(output)?output+".failure.json":output,new{Scope="ACTUAL_ENTRY_FAILED_NO_SUBSTITUTION",Completed=false,InputSha,OriginalIndex=Index,RawPoints=rawPoints,GraphNodes=graphNodes,ParentChildren=children,Stage=stage,Stages=stages,
    Error=error.GetType().FullName,Message=error.Message,Canceled=token.IsCancellationRequested,TokenMatches=error is OperationCanceledException cancel&&cancel.CancellationToken==token,
    GlyphAllocationProgress=owner?.GraphGlyphAllocationForProbe,PendingCapture=owner?.HasPendingCaptureForProbe,PendingReceipt=owner?.HasPendingReceiptForProbe,ElapsedMs=clock.ElapsedMilliseconds,NoFallback=true});
   Console.Error.WriteLine(error);return 1;
  }
 }
 static readonly string[] BaselineFields={"Completed","InputSha","OriginalIndex","RawPoints","RawStrokes","Resolution","GraphNodes","ParentChildren","Stages","Metrics","SourceCompletion","Top","Vector10","NoExpectedLabels","Admission","GlobalReadinessGateUnchanged","TimeoutMs"};
 static void Equal(JsonElement actual,JsonElement expected)
 {
  if(actual.ValueKind!=expected.ValueKind)throw new InvalidDataException("Prior canonical field type changed.");
  if(expected.ValueKind==JsonValueKind.Object)
  {
   var a=actual.EnumerateObject().ToArray();var b=expected.EnumerateObject().ToArray();if(a.Length!=b.Length)throw new InvalidDataException("Prior canonical object shape changed.");
   for(int i=0;i<a.Length;i++){if(a[i].Name!=b[i].Name)throw new InvalidDataException("Prior canonical field order/name changed.");Equal(a[i].Value,b[i].Value);}
  }
  else if(expected.ValueKind==JsonValueKind.Array)
  {if(actual.GetArrayLength()!=expected.GetArrayLength())throw new InvalidDataException("Prior canonical array count changed.");for(int i=0;i<actual.GetArrayLength();i++)Equal(actual[i],expected[i]);}
  else if(actual.GetRawText()!=expected.GetRawText())throw new InvalidDataException("Prior canonical scalar/bytes changed.");
 }

 static void Save(string path,object result)
 {using var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);JsonSerializer.Serialize(file,result);if(file.Length>32*1024*1024)throw new InvalidDataException("Diagnostic capture budget exceeded.");}
 static byte[] ReadPinned(string path,int expected,string hash,CancellationToken token,int limit=4*1024*1024)
 {
  token.ThrowIfCancellationRequested();if(expected<1||expected>limit)throw new InvalidDataException("Bounded pinned input size required.");
  using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);if(file.Length!=expected)throw new InvalidDataException("Pinned input length changed.");var bytes=new byte[expected];int offset=0;
  while(offset<bytes.Length){token.ThrowIfCancellationRequested();int n=file.Read(bytes,offset,Math.Min(65536,bytes.Length-offset));if(n==0)throw new EndOfStreamException();offset+=n;}
  if(file.ReadByte()!=-1||Convert.ToHexStringLower(SHA256.HashData(bytes))!=hash)throw new InvalidDataException("Pinned input digest changed.");token.ThrowIfCancellationRequested();return bytes;
 }
}
