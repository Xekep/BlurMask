using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
if(args.Length!=3||args[0] is not("--actual" or "--validate-plan"))throw new ArgumentException("--validate-plan PLAN REPORT | --actual PLAN FRESH_E_DIRECTORY (root review required)");
using var plan=JsonDocument.Parse(File.ReadAllBytes(args[1]));var files=new Dictionary<string,byte[]>();
foreach(var x in plan.RootElement.GetProperty("files").EnumerateArray())
{
 string name=x.GetProperty("name").GetString()!,path=x.GetProperty("path").GetString()!;long size=x.GetProperty("bytes").GetInt64();
 if(size is<1 or>8000000||new FileInfo(path).Length!=size)throw new InvalidDataException("Exact bounded input/resource file required.");
 byte[] bytes=File.ReadAllBytes(path);if(Convert.ToHexStringLower(SHA256.HashData(bytes))!=x.GetProperty("sha256").GetString())throw new InvalidDataException("Input/resource pin mismatch.");files.Add(name,bytes);
}
string[] required=["ak","lk","engine","line30","line50","separation","case","publication","normalization","circles","track"];
if(files.Count!=required.Length||!files.Keys.Order().SequenceEqual(required.Order()))throw new InvalidDataException("Exact eleven pins required before any model or source creation.");
byte[] R(string key)=>files[key];
using var cs=new MemoryStream(R("case"));using var ps=new MemoryStream(R("publication"));using var ns=new MemoryStream(R("normalization"));using var cis=new MemoryStream(R("circles"));
var cases=LegacyNativeCaseData.Load(cs);var publication=LegacyNativeTextPublicationData.Load(ps);var normalization=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
if(args[0]=="--validate-plan")
{
 File.WriteAllText(args[2],JsonSerializer.Serialize(new{Scope="DATA plan/Unicode table validation only; no models loaded/no source or NN",Files=files.Count,NNCalls=0,Normalization=LegacyNativeUnicodeNormalizationData.ExpectedTableSha256,Circles=LegacyNativeCircledPayloadData.ExpectedTableSha256},new JsonSerializerOptions{WriteIndented=true}));return;
}
if(Directory.Exists(args[2]))throw new ArgumentException("Fresh output directory required.");Directory.CreateDirectory(args[2]);
string stage="setup";var evidence=new List<object>();
void Check(bool value,string name){if(!value)throw new Exception(name);evidence.Add(new{Name=name,Pass=true});}
void Refuse(string name,Action action,Type type,string? message=null,CancellationToken? expectedToken=null)
{
 try{action();}catch(Exception e){if(e.GetType()!=type||message is not null&&e.Message!=message||expectedToken is{} token&&(e is not OperationCanceledException cancel||cancel.CancellationToken!=token))throw;evidence.Add(new{Name=name,Type=e.GetType().FullName,e.Message,ExactMessageChecked=message is not null,TokenMatches=expectedToken.HasValue});return;}throw new Exception("Expected refusal: "+name);
}
try
{
 using var track=JsonDocument.Parse(R("track"));var points=track.RootElement.GetProperty("points").EnumerateArray().Select(v=>new InkPoint(v[0].GetSingle(),-v[1].GetSingle())).ToArray();
 if(points.Length is<2 or>4096)throw new NotSupportedException("Bounded unchanged one-physical-stroke recording required.");
 var models=LegacyFreshMainRecognitionContext.Load(R("ak"),R("lk"),R("engine"),R("line30"),R("line50"));var table=LegacyCursiveSeparationTable.Load(R("separation"));
 var doc=LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(points)},default);
 var bootstrap=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"diagnostic"});bootstrap.Run();
 var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(bootstrap,bootstrap.State.Inputs[0]);selected.Run();
 using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cases,publication);
 using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(30));
 stage="source-run";var source=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],deadline.Token);
 var session=source.Session??throw new InvalidDataException("Actual live source session required.");var word=session.Ownership.WordPublisher.Head??throw new InvalidDataException("Actual current selected279 required.");
 int nn=source.CachedMainGroupEvaluations;byte[] rawCompact=source.Selection.Publication.CopyCompactStorage();int bit8=0;for(int i=0;i<source.Selection.Publication.RecordCount2470;i++)if((rawCompact[i*40+32]&8)!=0)bit8++;
 Check(source.HasWinner&&source.PublicationCompleted&&bit8>0,"actual-positive-source");
 Refuse("old-typed-positive-eligibility-refusal",()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.Prepare(scope,source,new byte[400],default),typeof(NotSupportedException),"Active219580 bit8 augmentation is not restored.");
 var actual=LegacyFreshMainGlobalWordAugmentation.Prepare(R("ak"),source,new byte[400],deadline.Token);
 LegacyFreshMainGlobalTextSourceWrapper Wrap(LegacyLibraryGlobalTextPublicationScope s,byte[] ak,CancellationToken token=default)=>LegacyFreshMainGlobalTextSourceWrapper.Prepare(s,actual,ak,publication,normalization,circles,token);
 using var cancel=new CancellationTokenSource();cancel.Cancel();
 Refuse("early-incomplete-aug",()=>Wrap(scope,R("ak")),typeof(InvalidOperationException),"Successful actual owning augmentation required.");
 Refuse("pre-cancel-before-bind",()=>Wrap(scope,R("ak"),cancel.Token),typeof(OperationCanceledException),expectedToken:cancel.Token);
 Check(scope.SharedState.GlobalTopOwner is null&&!scope.IsFaulted&&!session.IsStopped,"early-guards-no-bind-or-poison");
 stage="augmentation";var augResult=actual.Run(new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],new byte[4],new byte[4],new byte[4],new byte[4],deadline.Token);
 Check(actual.IsComplete&&!actual.IsFaulted&&augResult.Candidates>0,"actual-aug-complete-positive");
 object ReadCurrentState()
 {
  var snapshot=session.Ownership.WordPublisher.CaptureSpan(word,[word],deadline.Token);if(snapshot.Words.Count is<1 or>4096)throw new NotSupportedException("Diagnostic275 payload cap");
  var nodes=session.Ownership.GlyphPool.CopyOwnerNodes(deadline.Token);if(nodes.Count>4096||nodes.Sum(n=>n.Records.Length)>65536*40)throw new NotSupportedException("Diagnostic274 cap");
  return new{snapshot.Span.First,snapshot.Span.Last,Words=snapshot.Words.Select(w=>new{Storage120=Convert.ToBase64String(w.CopyStorage()),FlatReferences=w.FlatReferences.ToArray()}).ToArray(),SelectedOrdinals=snapshot.SelectedOrdinals.ToArray(),Pool=nodes.Select(n=>new{n.Key,n.PackedCount,Records=Convert.ToBase64String(n.Records.Span)}).ToArray()};
 }
 object beforeText=ReadCurrentState();byte[][] before275=augResult.CopyRankedWords120();
 byte[] wrong=R("ak").ToArray();wrong[^1]^=1;
 Refuse("wrong-resource-before-bind",()=>Wrap(scope,wrong),typeof(ArgumentException),"Same actual acoustic source resource required before TEXT binding.");
 Refuse("wrong-resource-extent-before-bind",()=>Wrap(scope,R("ak")[..^1]),typeof(ArgumentException),"Complete acoustic source resource required.");
 var foreignBootstrap=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"foreign"});foreignBootstrap.Run();
 var foreignSelected=LegacyLibraryOriginalSelectedFieldContext.Prepare(foreignBootstrap,foreignBootstrap.State.Inputs[0]);foreignSelected.Run();
 using(var foreign=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,foreignSelected,0,table,cases,publication))
 {
  Check(ReferenceEquals(foreign.Document,scope.Document),"foreign-scope-same-original-document");
  Refuse("foreign-scope-no-run-refusal",()=>Wrap(foreign,R("ak")),typeof(ArgumentException),"Original same-scope actual source required.");
  Check(foreign.SharedState.GlobalTopOwner is null&&!foreign.IsFaulted,"foreign-scope-no-bind");
 }
 Check(scope.SharedState.GlobalTopOwner is null&&!scope.IsFaulted&&!session.IsStopped,"argument-guards-before-adoption");
 stage="text-prepare";var wrapper=Wrap(scope,R("ak"),deadline.Token);Check(ReferenceEquals(wrapper,Wrap(scope,R("ak"),deadline.Token)),"same-live-wrapper-cache");
 Check(wrapper.ActualWordPayloadCount==before275.Length&&wrapper.CopyActualWordPayloads120(deadline.Token).Zip(before275).All(x=>x.First.AsSpan().SequenceEqual(x.Second)),"all-current275-metadata-retained");
 var parent=LegacyFreshMainGlobalWordTextPublication.Prepare(wrapper,deadline.Token);
 Check(parent.AlternativeCount==actual.TextSnapshot(deadline.Token).ActiveAlternativeCount60,"selected-top-count-from-snapshot");
 Refuse("text-pre-cancel-exact-token",()=>parent.Publish(new byte[40],new byte[8160],cancel.Token),typeof(OperationCanceledException),expectedToken:cancel.Token);
 Refuse("text-bad-allocator-before-entry",()=>parent.Publish(new byte[39],new byte[8160]),typeof(ArgumentException));
 Check(!parent.IsFaulted&&!wrapper.IsFaulted&&!scope.IsFaulted,"text-pre-entry-guards-no-poison");
 stage="text-publish";parent.Publish(new byte[40],new byte[8160],deadline.Token);
 Check(parent.IsComplete&&!parent.IsFaulted&&parent.HasTextReceipt&&parent.HasDocumentWordLink,"actual-selected-text-complete");
 Check(source.Selection.Publication.Policy.Owner.AcousticEvaluations==nn,"no-additional-acoustic-NN");
 var links=parent.WordSource.CopyCharacterLinks();var glyphs=links.Select(link=>new{Reference=link.Choice.ProcessedSource.Reference,link.Marker,SelectedUtf8=Convert.ToBase64String(link.Choice.CopySelectedUtf8(deadline.Token)),Records=link.Source.CopyRecords(deadline.Token).Select(r=>new{Scalar40=Convert.ToBase64String(r.CopyStorage40()),Utf8=Convert.ToBase64String(r.Text!.ReadBytes())}).ToArray()}).ToArray();
 byte[] selectedUtf8=links.SelectMany(link=>link.Choice.CopySelectedUtf8(deadline.Token)).ToArray();
 var textData=new{SelectedUtf8=Convert.ToBase64String(selectedUtf8),SelectedText=Encoding.UTF8.GetString(selectedUtf8),SelectedWordIndex=parent.SelectedWordIndex,SelectedTopCount=parent.AlternativeCount,Actual275PayloadCount=parent.ActualWordPayloadCount,Actual275=parent.CopyActualWordPayloads120(deadline.Token).Select(Convert.ToBase64String).ToArray(),parent.WordSource.SelectionId,parent.WordSource.Flags,parent.WordSource.ChoiceFlags,parent.WordSource.ChoiceCount,TextSelection=parent.Text.SelectionId,SourceLines56=Convert.ToBase64String(parent.CopySourceLines56()),WordOriginalRanges=parent.OriginalWordSource.CopyOriginalKey().Select(r=>new{r.First,r.Last}).ToArray(),TextRanges=parent.Text.CopyRanges().Select(r=>new{r.First,r.Last}).ToArray(),OriginalNullTextRanges=parent.CopyNullProjectedTextKey().Select(r=>new{r.First,r.Last}).ToArray(),Glyphs=glyphs,Trace=parent.CopyTrace(),Pending=parent.CopyPendingSelections().Select(s=>new{s.SelectionId,s.Name,s.CommandIdentity,s.OperationCode,s.Category,s.Enabled}).ToArray()};
 object afterText=ReadCurrentState();
 Refuse("text-repeat",()=>parent.Publish(new byte[40],new byte[8160]),typeof(InvalidOperationException));
 Check(!scope.IsFaulted&&!session.IsStopped,"repeat-loser-preserves-completed-source");
 scope.Dispose();
 Refuse("disposed-wrapper",()=>wrapper.CopyActualWordPayloads120(),typeof(ObjectDisposedException));
 Refuse("disposed-text",()=>parent.Text.CopyRanges(),typeof(ObjectDisposedException));
 Refuse("disposed-pending",()=>parent.CopyPendingSelections(),typeof(ObjectDisposedException));
 stage="capture";var payload=new{Scope="ONE genuine Scope.Run -> SAME completedAug -> CURRENTSELECTED-TOP TEXT; not all275 alternateTEXT/appacceptance",NNSourceInvocations=1,AcousticEvaluations=nn,ActualBit8=bit8,Augmentation=new{augResult.Candidates,augResult.Appended,augResult.Duplicates,augResult.UpdatedNewDuplicates,augResult.QuotaAfter,Source26=Convert.ToBase64String(augResult.CopySourceStorage26())},BeforeText=beforeText,AfterText=afterText,Text=textData,Evidence=evidence};
 byte[] output=JsonSerializer.SerializeToUtf8Bytes(payload,new JsonSerializerOptions{WriteIndented=true});if(output.Length>64*1024*1024)throw new NotSupportedException("Diagnostic JSON64MiB cap");File.WriteAllBytes(Path.Combine(args[2],"capture.json"),output);
}
catch(Exception e){File.WriteAllText(Path.Combine(args[2],"failure.json"),JsonSerializer.Serialize(new{Stage=stage,Type=e.GetType().FullName,e.Message,Evidence=evidence,Scope="Preserved one actual failure; no retries"},new JsonSerializerOptions{WriteIndented=true}));throw;}
