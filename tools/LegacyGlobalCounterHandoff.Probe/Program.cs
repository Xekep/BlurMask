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




var output=new List<object>();int guards=0,complete=0,refused=0,total=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("completed",false),("refusal","238B0")));refused++;continue;}
 var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);
var cleanup=LegacyFreshMainGlobalControlCleanup.Prepare(LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state).Run()).Run();var transforms=LegacyFreshMainGlobalInputTransforms.Prepare(cleanup);
 Reject(()=>LegacyFreshMainGlobalCounterHandoff.Apply(transforms));transforms.Run();Reject(()=>LegacyFreshMainGlobalCounterHandoff.Apply(transforms,new(true)));
 var owner=scope.SharedState.Selections;var foreign=LegacyOwnedFreshTextSourceContext.PrepareSelectedBinding(scope.Context.InputBinding);Reject(()=>foreign.AppendActualGlobalCounter(transforms,false,default));if(foreign.CopyPendingCommands().Count!=0)throw new Exception("Foreign transaction changed.");
 var before=owner.CopyPendingSelections().ToArray();long initial=scope.Seed.InputBinding.Next,next=owner.NextSelectionId;var registry=scope.Context.InputBinding.CopyInitialRegistryIds();
 var result=LegacyFreshMainGlobalCounterHandoff.Apply(transforms);result.ValidateCurrent();Reject(()=>LegacyFreshMainGlobalCounterHandoff.Apply(transforms));
 if(!before.SequenceEqual(owner.CopyPendingSelections())||!registry.SequenceEqual(scope.Context.InputBinding.CopyInitialRegistryIds())||owner.NextSelectionId!=next||!ReferenceEquals(result.TransactionOwner,owner)||!ReferenceEquals(result.Transforms,transforms)||result.InitialPendingCounter!=initial||result.FinalPendingCounter!=next||result.CounterCommand is null||result.Commands.Count!=before.Length+1)throw new Exception("Actual global command prefix/counters/ownership changed.");
 long id=-1;Reject(()=>owner.CreatePrepared([],0,ref id,0));result.ValidateCurrent();
 var body=result.CounterCommand.CopyAssignedBody56();body[0]^=255;if(result.CounterCommand.CopyAssignedBody56()[0]==body[0])throw new Exception("Counter byte alias.");
 complete++;total+=result.Commands.Count;
 output.Add(D(("name",name),("completed",true),("initialIds",registry),("initial",initial),("final",next),("selectionCount",before.Length),("commands",Commands(result.Commands)),("body56",Convert.ToHexString(result.CounterCommand.CopyAssignedBody56()))));
 scope.Dispose();Reject(()=>result.ValidateCurrent());
}
using(var scope=Scope([M()]))
{
 var transforms=Transform(scope);var owner=scope.SharedState.Selections;var before=owner.CopyPendingSelections().ToArray();long next=owner.NextSelectionId;Reject(()=>LegacyFreshMainGlobalCounterHandoff.ApplyPreparedAllocationFailure(transforms));
 if(!owner.Failed||owner.NextSelectionId!=next||!before.SequenceEqual(owner.CopyPendingSelections())||owner.CopyPendingCommands().Any(c=>c.OperationCode!=4))throw new Exception("Allocation failure discarded source prefix.");Reject(()=>LegacyFreshMainGlobalCounterHandoff.Apply(transforms));Reject(()=>transforms.ValidateCurrent());
 output.Add(D(("name","prepared-allocation"),("failed",owner.Failed),("next",next),("commands",Commands(owner.CopyPendingCommands()))));
}
using(var f=File.Create(args[2]))using(var j=new Utf8JsonWriter(f))WriteJson(j,D(("samples",output),("completed",complete),("refused",refused),("commands",total),("guards",guards)));
if(complete!=13||refused!=0||total!=76)throw new Exception("Actual global command coverage changed.");
Console.WriteLine($"PASS13 genuine global counter owners/{complete} complete/{refused} explicit selected-source refusals/{total} ordered operation4+7 commands/{guards} guards; no replay/DOM acceptance.");
LegacyFreshMainGlobalInputTransforms Transform(LegacyLibraryGlobalTextPublicationScope scope){var p=Parent(scope);p.Publish(new byte[40],new byte[8160]);return LegacyFreshMainGlobalInputTransforms.Prepare(LegacyFreshMainGlobalControlCleanup.Prepare(LegacyFreshMainGlobalDocumentSourceIndices.Prepare(LegacyFreshMainGlobalDocumentSelectedSources.Prepare(LegacyFreshMainGlobalDocumentCoverage.Prepare(p))).Run()).Run()).Run();}
object[] Commands(IReadOnlyList<LegacyOwnedFreshTextPendingCommand> all)=>all.Select(c=>(object)D(("op",(int)c.OperationCode),("id",c.CommandIdentity),("previous",c.PreviousCommandIdentity),("next",c.NextCommandIdentity),("selection",c.Selection is{}s?D(("id",s.SelectionId),("name",s.Name),("category",s.Category),("ranges",s.CopyRanges().Select(r=>new[]{r.First.Value,r.Last.Value}).ToArray())):null),("counter",c.Counter is{}counter?new[]{counter.OldCounter20,counter.NewCounter18}:null))).ToArray();
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected global counter refusal.");}
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
