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




var output=new List<object>();int guards=0,complete=0,refused=0,visited=0,records=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);parent.Publish(Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray(),new byte[8160]);
 LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}
 catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("completed",false),("refusal","238B0")));refused++;continue;}
 var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);
var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state);
 Reject(()=>LegacyFreshMainGlobalControlCleanup.Prepare(index));index.Run();
 Reject(()=>LegacyFreshMainGlobalControlCleanup.Prepare(index,new(true)));
 var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(index);Reject(()=>cleanup.ValidateCurrent());Reject(()=>cleanup.Run(new(true)));
 if(cleanup.IsFaulted||cleanup.IsComplete||cleanup.CopyPrefixes().Length!=0)throw new Exception("Pre-entry cancellation changed owner.");
 Reject(()=>LegacyFreshMainGlobalControlCleanup.Prepare(index));
 var before48=parent.WordSource.CopyChoiceScalar48();var before=Snapshot(state);var commands=parent.CopyPendingSelections().ToArray();
 cleanup.Run();cleanup.ValidateCurrent();Reject(()=>cleanup.Run());
 if(!before48.AsSpan().SequenceEqual(parent.WordSource.CopyChoiceScalar48())||!Json(before).AsSpan().SequenceEqual(Json(Snapshot(state)))||!commands.SequenceEqual(parent.CopyPendingSelections()))throw new Exception("Cleanup mutated historical source rows or commands.");
 complete++;visited+=cleanup.CopyCharacterVisitOrder().Length;records+=state.Characters.Sum(c=>c.OriginalSource.CopyRecords().Count);
 output.Add(D(("name",name),("completed",true),("word48",Convert.ToHexString(before48)),("wordBefore",state.WordFlags),("wordAfter",cleanup.WordFlags),("rows",cleanup.WordRowsInspected),("cleared",cleanup.WordMaskCleared),("characters",before),("order",cleanup.CopyCharacterVisitOrder()),("prefixes",Prefixes(cleanup))));
 var copied=cleanup.CopyCharacterVisitOrder();copied[0]=-999;if(cleanup.CopyCharacterVisitOrder()[0]==-999)throw new Exception("Visit alias.");
 long id=-1;_ = scope.SharedState.Selections.CreatePrepared([],0,ref id,1);Reject(()=>cleanup.ValidateCurrent());
 scope.Dispose();Reject(()=>cleanup.ValidateCurrent());
}
var failures=new List<object>();
foreach(var at in Enum.GetValues<LegacyGlobalControlCleanupStage>())
{
 using var scope=Scope([M()]);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(LegacyFreshMainGlobalDocumentCoverage.Prepare(parent));var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run();var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(index);
 using var cancel=new CancellationTokenSource();Reject(()=>cleanup.RunDiagnostic(stage=>{if(stage==at)cancel.Cancel();},cancel.Token));
 if(!cleanup.IsFaulted||cleanup.IsComplete)throw new Exception("Entered cancellation did not poison cleanup.");
 Reject(()=>cleanup.Run());Reject(()=>cleanup.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalControlCleanup.Prepare(index));index.ValidateCurrent();
 failures.Add(D(("stage",at.ToString()),("wordAfter",cleanup.WordFlags),("cleared",cleanup.WordMaskCleared),("rows",cleanup.WordRowsInspected),("order",cleanup.CopyCharacterVisitOrder()),("prefixes",Prefixes(cleanup))));
}
var invalidations=new List<object>();
foreach(bool dispose in new[]{false,true})
{
 using var scope=Scope([M()]);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(LegacyFreshMainGlobalDocumentCoverage.Prepare(parent));var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run();var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(index);
 Reject(()=>cleanup.RunDiagnostic(stage=>{if(stage==LegacyGlobalControlCleanupStage.Complete){if(dispose)scope.Dispose();else{long id=-1;_=scope.SharedState.Selections.CreatePrepared([],0,ref id,1);}}}));
 if(!cleanup.IsFaulted||cleanup.IsComplete)throw new Exception("Late owner invalidation escaped completion validation.");Reject(()=>cleanup.Run());Reject(()=>cleanup.ValidateCurrent());
 invalidations.Add(D(("disposed",dispose),("faulted",cleanup.IsFaulted),("complete",cleanup.IsComplete),("prefixes",Prefixes(cleanup))));
}
int controls=0;
for(int bit=0;bit<32;bit++)foreach(uint flags in new[]{0u,1u<<bit,~(1u<<bit)})
{
 if((flags&0x100000)!=0){Reject(()=>LegacyFreshMainGlobalControlCleanup.RequireNoWordControl(flags));Reject(()=>LegacyFreshMainGlobalControlCleanup.RequireNoCharacterControl(flags));}
 else{LegacyFreshMainGlobalControlCleanup.RequireNoWordControl(flags);LegacyFreshMainGlobalControlCleanup.RequireNoCharacterControl(flags);}
 controls++;
}
using(var scope=Scope([M()]))
{
 var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(LegacyFreshMainGlobalDocumentCoverage.Prepare(parent));var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run();int wins=0,losses=0;
 void Attempt(){try{_=LegacyFreshMainGlobalControlCleanup.Prepare(index);Interlocked.Increment(ref wins);}catch(InvalidOperationException){Interlocked.Increment(ref losses);}}
 Task.WaitAll(Task.Run(Attempt),Task.Run(Attempt));if(wins!=1||losses!=1)throw new Exception("Exclusive claim race.");
}
using(var f=File.Create(args[2]))using(var j=new Utf8JsonWriter(f))WriteJson(j,D(("samples",output),("completed",complete),("refused",refused),("characters",visited),("records",records),("failures",failures),("invalidations",invalidations),("flagControls",controls),("claimRace",true),("guards",guards)));
if(complete!=13||refused!=0||visited!=37||records!=80)throw new Exception($"Actual cleanup coverage changed {complete}/{refused}/{visited}/{records}.");
Console.WriteLine($"PASS13 genuine global cleanup/{complete} completed/{refused} explicit selected-source refusals/{visited} CHAR/{records} records/5 cancellation prefixes/{controls} flag controls/{guards} guards; no active control services/DOM.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected cleanup refusal.");}
byte[] Json(object value){using var m=new MemoryStream();using(var j=new Utf8JsonWriter(m))WriteJson(j,value);return m.ToArray();}
object[] Snapshot(LegacyFreshMainGlobalDocumentSelectedSources s)=>s.Characters.Select(c=>(object)D(("node",c.OriginalSource.Node.Identity),("flags",c.Flags),("records",c.OriginalSource.CopyRecords().Select(r=>D(("scalar40",Convert.ToHexString(r.CopyStorage40())),("utf8",r.Text is{}text?Convert.ToHexString(text.ReadBytes()):null))).ToArray()))).ToArray();
object[] Prefixes(LegacyFreshMainGlobalControlCleanup c)=>c.CopyPrefixes().Select(p=>(object)D(("stage",p.Stage),("rows",p.WordRows),("cleared",p.WordMaskCleared),("word",p.WordFlags),("characters",p.Characters))).ToArray();
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
