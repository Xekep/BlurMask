using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=3)throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));




var output=new List<object>();int guards=0;
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected range-owner refusal.");}
Reject(()=>LegacyFreshMainGlobalSourceRangeUtf8.Prepare(null!));
foreach(var counts in new[]{new[]{-1,0,0},new[]{65537,0,0},new[]{4097,0,0},new[]{1,1},new int[4099],new[]{4096,4096,0}})Reject(()=>LegacyFreshMainGlobalSourceRangeUtf8.Preflight(counts));
if(LegacyFreshMainGlobalSourceRangeUtf8.Preflight([4096,0,0])<=0)throw new Exception("Exact work boundary refused.");
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("refusal","238B0")));continue;}
 LegacyFreshMainGlobalDocumentSelectedSources state;
 try{state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);}catch(NotSupportedException e)when(e.Message=="Native01FD40 actually reaches C25 raw-object lookup; concrete kind is not inferred."){output.Add(D(("name",name),("refusal","C25")));continue;}
 var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run()).Run();
 var handoff=LegacyFreshMainGlobalCounterHandoff.Apply(LegacyFreshMainGlobalInputTransforms.Prepare(cleanup).Run());
 var before=scope.SharedState.Selections.CopyPendingCommands().Select(c=>c.CommandIdentity).ToArray();long next=scope.SharedState.Selections.NextSelectionId;
 Reject(()=>LegacyFreshMainGlobalSourceRangeUtf8.Prepare(handoff,new(true)));
 for(int stop=1;stop<=state.Characters.Count+2;stop++)
 {using var cancel=new CancellationTokenSource();int reached=0;Reject(()=>LegacyFreshMainGlobalSourceRangeUtf8.PrepareDiagnostic(handoff,n=>{reached=n;if(n==stop)cancel.Cancel();},cancel.Token));if(reached!=stop)throw new Exception("Missing exact cancellation prefix.");handoff.ValidateCurrent();}
 var result=LegacyFreshMainGlobalSourceRangeUtf8.Prepare(handoff);result.ValidateCurrent();
 var sources=new List<object>();foreach(var entry in result.Sources)
 {
  if(entry.Kind=="CHAR"&&(entry.CharacterSource is null||!ReferenceEquals(entry.CharacterSource.Owner,parent.Wrapper)))throw new Exception("Detached CHAR identity.");
  if(entry.Kind=="WORD"&&!ReferenceEquals(entry.WordSource,parent.WordSource)||entry.Kind=="TEXT"&&!ReferenceEquals(entry.TextSource,parent.Text))throw new Exception("Detached WORD/TEXT identity.");
  var original=entry.CopyOriginalRanges();var normal=entry.CopyNormalizedRanges();var bytes=entry.CopyUtf8();var caller=entry.CopyCallerUtf8();
  sources.Add(D(("kind",entry.Kind),("id",entry.SourceNodeIdentity),("flags",entry.SourceFlags),("original",original.Select(r=>new[]{r.First.Value,r.Last.Value}).ToArray()),("normalized",normal.Select(r=>new[]{r.First.Value,r.Last.Value}).ToArray()),("utf8",Convert.ToBase64String(bytes)),("callerUtf8",Convert.ToBase64String(caller))));
  if(original.Length!=0){original[0]=default;if(entry.CopyOriginalRanges()[0]==default)throw new Exception("Original key alias.");}
  if(normal.Length!=0){normal[0]=default;if(entry.CopyNormalizedRanges()[0]==default)throw new Exception("Normalized key alias.");}
  if(bytes.Length!=0){bytes[0]^=255;if(entry.CopyUtf8()[0]==bytes[0])throw new Exception("UTF8 alias.");}
  if(caller.Length!=0){caller[0]^=255;if(entry.CopyCallerUtf8()[0]==caller[0])throw new Exception("Caller UTF8 alias.");}
 }
 if(!before.SequenceEqual(scope.SharedState.Selections.CopyPendingCommands().Select(c=>c.CommandIdentity))||next!=scope.SharedState.Selections.NextSelectionId)throw new Exception("Formatting executed pending commands.");
 output.Add(D(("name",name),("sources",sources),("points",scope.Document.Strokes.Select(s=>s.Points.Count).ToArray()),("pending",before),("next",next)));
 var retained=result.Sources[0];scope.Dispose();Reject(()=>result.ValidateCurrent());Reject(()=>retained.CopyUtf8());Reject(()=>{_=result.Sources;});
}
using(var f=File.Create(args[2]))using(var j=new Utf8JsonWriter(f))WriteJson(j,D(("samples",output),("guards",guards)));
Console.WriteLine($"PASS genuine global source range strings/{guards} guards; no DOM/replay acceptance.");
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}

Dictionary<string,object?> D(params (string Name,object? Value)[] fields)=>fields.ToDictionary(p=>p.Name,p=>p.Value);
void WriteJson(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
  case null:w.WriteNullValue();break;
  case string text:w.WriteStringValue(text);break;
  case bool flag:w.WriteBooleanValue(flag);break;
  case int number:w.WriteNumberValue(number);break;
  case uint number:w.WriteNumberValue(number);break;
  case long number:w.WriteNumberValue(number);break;
  case ulong number:w.WriteNumberValue(number);break;
  case Dictionary<string,object?> values:w.WriteStartObject();foreach(var pair in values){w.WritePropertyName(pair.Key);WriteJson(w,pair.Value);}w.WriteEndObject();break;
  case System.Collections.IEnumerable values:w.WriteStartArray();foreach(var item in values)WriteJson(w,item);w.WriteEndArray();break;
  default:throw new Exception("Untracked diagnostic JSON type "+value.GetType());
 }
}
