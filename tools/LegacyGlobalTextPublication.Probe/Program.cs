using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=2)throw new ArgumentException("models capture");
string modelsPath=args[0];byte[] Read(string name)=>File.ReadAllBytes(Path.Combine(modelsPath,name));
byte[] ak=Read("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,Read("ru-RU/ru_RU-lk-text.lite.res"),Read("ru-RU/legacy-engine-profile.json"),Read("line-geometry/line-2e3-30.res"),Read("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(modelsPath,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(modelsPath,"text-unicode/legacy-text-publication-data.json"));var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke[] Word()=>[M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})];
var samples=new[]{new[]{M(0)},Word(),new[]{new InkStroke(new InkPoint[]{new(0,10),new(0,90)})}};
int guards=0,nn=0,queries=0;using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");
for(int sample=0;sample<samples.Length;sample++)
{
 var doc=LegacyOwnedRawInkDocument.Create(samples[sample],default);
 var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"left","right"});original.Run();
 //One public raw-route baseline per sample. No expected literal enters inference.
 var old=models.PrepareGlobal(samples[sample],0,table,cd,pd).Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
 for(int chosen=0;chosen<2;chosen++)
 {
  var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[chosen]);selected.Run();
  Reject(()=>LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd,new(true)));
  using var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);
  var context=scope.Context;var seed=scope.Seed;var shared=scope.SharedState;
  if(!ReferenceEquals(scope.Document,doc)||!ReferenceEquals(context.Result.PublicationSeed,seed)||!ReferenceEquals(seed.StringPool,context.Strings)||
   !ReferenceEquals(seed.ProcessingResult,context.Result)||!ReferenceEquals(seed.InputBinding,context.InputBinding)||!ReferenceEquals(shared.Selections.InputBinding,context.InputBinding)||
   !ReferenceEquals(seed.OriginalSelectedInputContext,selected)||context.Result.CopyIdentifier38().Any(b=>b!=0)||shared.Strings.Count!=0||shared.GlobalTopOwner is not null)
   throw new InvalidDataException("Pre-NN2DA/shared3E/Seed/selected binding changed.");
  Reject(()=>{_=seed.LibraryIdentifier38;});Reject(()=>{_=shared.TopOwner;});Reject(()=>{_=seed.SelectedInputContext;});
  Reject(()=>scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72],new(true)));
  var result=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
  if(!ReferenceEquals(result.Owner.SourceDocument,doc)||!result.Selection.Publication.CopyCompactStorage().AsSpan().SequenceEqual(old.Selection.Publication.CopyCompactStorage()))throw new InvalidDataException("Same Document prepare changed raw-route compact bytes.");
  Reject(()=>scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]));
  var graph=result.Selection.Publication.Policy.Owner.Graph;var cuts=graph.Source.Source;var word=result.Session!.Ownership.WordPublisher.Head!;byte[] before=word.Word.CopyStorage();
  Reject(()=>scope.CompleteSources(new byte[399],new byte[4],new byte[4]));Reject(()=>scope.CompleteSources(new byte[400],new byte[4],new byte[4],new(true)));
  context.Dispose();if(context.ResourcesReleased)throw new InvalidDataException("Pre-NN scope lease released early.");
  var completion=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);
  if(!completion.IsTopV68Completed||completion.IsFaulted||!completion.QuoteCleanupCompleted||!completion.SourcesMarkedComplete||!completion.RankingApplied||!completion.SpanScoresApplied||!completion.CheckboxGuardVerified||
   !ReferenceEquals(shared.GlobalTopOwner,result.Session.Ownership)||!ReferenceEquals(completion.Word,word)||!ReferenceEquals(result.Session.Ownership.WordPublisher.Head,word)||
   !ReferenceEquals(seed.ProcessingResult,context.Result)||!context.Result.CopyIdentifier38().AsSpan().SequenceEqual(seed.LibraryIdentifier38.CopyRawStorage16())||context.Result.CopyIdentifier38().All(b=>b==0))throw new InvalidDataException("Live completion lost source/seed/ranking identity.");
  Reject(()=>scope.CompleteSources(new byte[400],new byte[4],new byte[4]));
  Reject(()=>{_=shared.TopOwner;});
  var chronology=cuts.Chronology;var source=cuts.Preparation.Input.Input.Input.Owner.Controller.Source;
  var keys=new LegacySourcePointKey[source.Strokes.Sum(s=>s.Points.Count)];for(int s=0;s<source.Strokes.Count;s++)for(int point=0;point<source.Strokes[s].Points.Count;point++){var id=source.Chronology.Resolve(s,point);keys[id.KeyRecordIndex]=id.Key;}
  w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteNumber("chosen",chosen);w.WriteNumber("sourceTag",seed.SourceTag30);w.WriteNumber("registryNext",selected.State.NextRegistryId);w.WriteStartArray("registry");foreach(var row in selected.State.Registry)w.WriteNumberValue(row.RegistryId);w.WriteEndArray();
  w.WriteBase64String("inputRanges",context.Result.CopyInputRangeStorage());w.WriteBoolean("beforeNnSameBinding",true);w.WriteBoolean("sameDocument",true);w.WriteBoolean("rawRouteCompactEqual",true);w.WriteBoolean("retainedAfterContextDispose",true);
  w.WriteBase64String("compact",result.Selection.Publication.CopyCompactStorage().AsSpan(0,result.Selection.Publication.RecordCount2470*40));
  w.WriteNumber("first",word.Span.First);w.WriteNumber("last",word.Span.Last);w.WriteBase64String("before120",before);w.WriteBase64String("after120",word.Word.CopyStorage());w.WriteNumber("selected",word.WordIndex);w.WriteNumber("quota",completion.QuotaAfter);w.WriteBase64String("source26",completion.CopySourceStorage26());
  w.WriteStartArray("counts");foreach(var stroke in doc.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();w.WriteStartArray("positions");foreach(var r in completion.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("keys");foreach(var key in keys)Key(key);w.WriteEndArray();w.WriteStartArray("metadata");for(int s=0;s<chronology.StrokePointCounts.Length;s++){w.WriteStartArray();for(int point=0;point<chronology.StrokePointCounts.Span[s];point++){var id=chronology.Resolve(s,point);var end=source.ResolveEndingMetadata(id);w.WriteStartArray();Key(id.Key);Key(end.Key);w.WriteNumberValue(id.KeyRecordIndex);w.WriteNumberValue(id.RemainderByte);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("groups");foreach(var group in graph.State.Groups){w.WriteStartArray();foreach(var r in group.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("nodes");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var arc in graph.State.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteNumberValue(arc.Span);w.WriteStartArray();foreach(int a in arc.CopySelector(1))w.WriteNumberValue(a);w.WriteEndArray();w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
  if(result.CachedMainGroupEvaluations!=result.Selection.Publication.Policy.Owner.AcousticEvaluations)throw new Exception("Source completion repeated NN.");w.WriteNumber("nn",result.CachedMainGroupEvaluations);nn+=result.CachedMainGroupEvaluations;queries+=completion.ReferenceCount;w.WriteEndObject();
  var detached=completion.CopySourceStorage26();if(detached.Length>0){detached[0]^=255;if(completion.CopySourceStorage26()[0]==detached[0])throw new Exception("Source26 aliases mutable caller buffer.");}
  scope.Dispose();if(!context.ResourcesReleased)throw new InvalidDataException("Final scope lease did not release context resources.");Reject(()=>completion.CopyReferences());Reject(()=>{_=seed.LibraryIdentifier38;});Reject(()=>scope.CompleteSources(new byte[400],new byte[4],new byte[4]));
 }
}
w.WriteEndArray();
//A real two-descriptor input is not silently reduced to one WORD/source scope.
var multiDoc=LegacyOwnedRawInkDocument.Create(new[]{M(0),M(250)},default);var multiOriginal=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(multiDoc),new[]{"a","b"});multiOriginal.Run();var multiSelected=LegacyLibraryOriginalSelectedFieldContext.Prepare(multiOriginal,multiOriginal.State.Inputs[1]);multiSelected.Run();
using(var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,multiSelected,0,table,cd,pd))
{
 var result=scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);
 if(result.Selection.Publication.Policy.Owner.Graph.Source.Source.Steps.Count<2)throw new Exception("Real multi-descriptor refusal fixture did not reach its branch.");
 Reject(()=>scope.CompleteSources(new byte[400],new byte[4],new byte[4]));if(!scope.IsFaulted)throw new Exception("Unsupported completion prefix was retryable.");Reject(()=>scope.CompleteSources(new byte[400],new byte[4],new byte[4]));
}
using(var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,multiSelected,0,table,cd,pd))
{
 Reject(()=>scope.Run(new byte[39],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]));if(!scope.IsFaulted||scope.Context.ResourcesReleased)throw new Exception("Failed actual prepare/run did not preserve poisoned scope prefix.");Reject(()=>scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]));
}
int preparedFlags=0;for(int flags=0;flags<256;flags++){byte[] compact=new byte[80];compact[72]=(byte)flags;if((flags&8)!=0)Reject(()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.RequireNoActiveAugmentation(compact,2,default));else LegacyFreshMainGlobalNoCandidateSourceCompletion.RequireNoActiveAugmentation(compact,2,default);preparedFlags++;}
Reject(()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.RequireNoActiveAugmentation(new byte[79],2,default));Reject(()=>LegacyFreshMainGlobalNoCandidateSourceCompletion.RequireNoActiveAugmentation(new byte[80],2,new(true)));
w.WriteNumber("preparedFlagControls",preparedFlags);w.WriteNumber("guards",guards);w.WriteNumber("nn",nn);w.WriteNumber("references",queries);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS6 actual same-Document pre-NN global scopes/{nn} NN/{queries} source refs/{guards} guards; no live CHAR/TEXT/accepted transcription.");
void Key(LegacySourcePointKey k){w.WriteStartArray();w.WriteNumberValue(k.First);w.WriteNumberValue(k.Second);w.WriteNumberValue(k.Third);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException or OperationCanceledException or InvalidOperationException or ObjectDisposedException){guards++;return;}throw new Exception("Invalid/stale global publication request accepted.");}
