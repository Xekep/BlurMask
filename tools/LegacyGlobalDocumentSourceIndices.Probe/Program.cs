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




var output=new List<object>();int guards=0,complete=0,refused=0,totalSources=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);parent.Publish(Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray(),new byte[8160]);
 LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}
 catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("completed",false),("refusal","238B0")));refused++;continue;}
 var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);
var pending=parent.CopyPendingSelections().ToArray();
 Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state,new(true)));
 var indices=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state);Reject(()=>indices.ValidateCurrent());
 Reject(()=>indices.Run(new(true)));if(indices.IsReady||indices.IsFaulted||indices.CopySources().Count!=0)throw new Exception("Pre-entry cancellation changed fresh owner.");
 Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state));
 var rebuilt=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(rebuilt));
 indices.Run();indices.ValidateCurrent();Reject(()=>indices.Run());complete++;
 var sources=indices.CopySources().ToArray();totalSources+=sources.Length;
 foreach(var source in sources)if(!ReferenceEquals(source.State,state))throw new Exception("Typed indexed source changed actual selected owner.");
 foreach(var snapshot in new[]{indices.CopyCharacterIndex(),indices.CopyWordIndex()})
  if(!ReferenceEquals(snapshot.Owner,indices)||snapshot.Intervals.Concat(snapshot.Vectors).Any(n=>!ReferenceEquals(n.Owner,snapshot))||snapshot.Intervals.Any(n=>n.CachedVectorCount is not null)||snapshot.Vectors.Any(n=>n.CachedVectorCount!=0))throw new Exception("Index snapshot ownership/cache scalar missing.");
 output.Add(D(("name",name),("completed",true),("ready",indices.IsReady),("faulted",indices.IsFaulted),
  ("charInsertion",parent.Wrapper.CharacterInterner.CopyInsertionOrder().Select(n=>n.Identity).ToArray()),("charInorder",state.Characters.Select(c=>c.OriginalSource.Node.Identity).ToArray()),
  ("sourceBound",indices.PreflightSourceCount),("membershipBound",indices.PreflightMembershipCount),
  ("sources",sources.Select(s=>D(("word",s.IsWord),("node",s.SourceNodeIdentity),("flags",s.Flags),("header",s.NativeRangeHeaderOffset),("ranges",Ranges(s.CopyRanges())))).ToArray()),
  ("characters",Actual(indices.CopyCharacterIndex(),sources)),("words",Actual(indices.CopyWordIndex(),sources)),("prefixes",Prefixes(indices))));
 if(!pending.SequenceEqual(parent.CopyPendingSelections()))throw new Exception("Source indexing changed pending commands.");
 var key=indices.CopyCharacterIndex().Intervals[0].CopyKey();key[0]=default;if(indices.CopyCharacterIndex().Intervals[0].CopyKey()[0]==default)throw new Exception("Mutable key alias.");
 Reject(()=>_=sources.First(s=>!s.IsWord).Word);Reject(()=>_=sources.First(s=>s.IsWord).Character);
 if(complete%3==0)parent.Scope.SharedState.Words.InternPrepared([]);
 else{long id=-1;_ = parent.Scope.SharedState.Selections.CreatePrepared([],0,ref id,complete%3==1?1:0);}
 Reject(()=>indices.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state));
 scope.Dispose();Reject(()=>indices.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state));
}
var failures=new List<object>();
for(int at=0;at<=2;at++)
{
 using var scope=Scope([M()]);var parent=Parent(scope);parent.Publish(new byte[40],new byte[8160]);
 var coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);

 var index=LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state);Reject(()=>index.RunPreparedFailure(at));
 if(!index.IsFaulted||index.IsReady)throw new Exception("Entered index prefix did not poison owner.");
 var sources=index.CopySources().ToArray();failures.Add(D(("after",at),("faulted",index.IsFaulted),("ready",index.IsReady),("visited",sources.Length),
  ("characters",Actual(index.CopyCharacterIndex(),sources)),("words",Actual(index.CopyWordIndex(),sources)),("prefixes",Prefixes(index))));
 Reject(()=>index.Run());Reject(()=>index.ValidateCurrent());Reject(()=>LegacyFreshMainGlobalDocumentSourceIndices.Prepare(state));
 state.Validate();
}
var prepared=new List<object>();
for(int c=0;c<40;c++)
{
 var k=new LegacyMainDocumentSourceIndexKernel<int>();var inputs=new List<LegacyNativeInkPositionRange[]>();
 for(int i=0;i<320;i++)
 {
  int v=(c%4)switch{0=>i,1=>319-i,2=>(i*73)%181,_=>(i*127+31)%193};ulong a=unchecked((ulong)(v*2654435761L+c*17L));if(c%3==0)a|=1UL<<63;
  LegacyNativeInkPositionRange r=new(new(a),new(a+3));LegacyNativeInkPositionRange[] key=i%11==0?[]:i%9==0?[r,r]:i%7==0?[new(new(a+10),new(a+12)),r]:[r];
  if(i%13==0&&inputs.Count>1)key=inputs[1].ToArray();inputs.Add(key);k.Add(i+1,key);
 }
 prepared.Add(D(("inputs",inputs.Select(Ranges).ToArray()),("index",Numeric(k))));
 int n=k.SourcesAdded,memberCount=k.Memberships.Count;Reject(()=>k.Add(999,[],new(true)));if(k.SourcesAdded!=n||k.Memberships.Count!=memberCount)throw new Exception("Cancelled generic kernel changed prefix.");
}
var vectorFailure=new LegacyMainDocumentSourceIndexKernel<int>();
for(int i=0;i<65536;i++)vectorFailure.Vectors.Intern([new(new((ulong)i),new((ulong)i+1))]);
LegacyNativeInkPositionRange last=new(new(ulong.MaxValue-1),new(ulong.MaxValue));Reject(()=>vectorFailure.Add(99,[last]));
if(vectorFailure.SourcesAdded!=0||vectorFailure.Intervals.Nodes.Count!=1||vectorFailure.Memberships.Count!=1||!vectorFailure.Dirty||vectorFailure.Vectors.Nodes.Count!=65536)throw new Exception("Membership-before-vector failure prefix changed.");
var partial=D(("vectorNodes",vectorFailure.Vectors.Nodes.Count),("sources",vectorFailure.SourcesAdded),("dirty",vectorFailure.Dirty),("intervalKey",Ranges(vectorFailure.Intervals.Nodes[0].Key)),
 ("member",new[]{vectorFailure.Memberships[0].Identity,vectorFailure.Memberships[0].Source,vectorFailure.Memberships[0].Next}));
var range=new LegacyNativeInkPositionRange(new(1),new(3));
Reject(()=>new LegacyGlobalDocumentIndexBudget().Add(new LegacyNativeInkPositionRange[65537]));
var membershipBudget=new LegacyGlobalDocumentIndexBudget();for(int i=0;i<16;i++)membershipBudget.Add(Enumerable.Repeat(range,65536).ToArray());Reject(()=>membershipBudget.Add([range]));
var intervalBudget=new LegacyGlobalDocumentIndexBudget();intervalBudget.Add(Enumerable.Range(0,65536).Select(i=>new LegacyNativeInkPositionRange(new((ulong)i),new((ulong)i+1))).ToArray());Reject(()=>intervalBudget.Add([new(new(65537),new(65538))]));
var vectorBudget=new LegacyGlobalDocumentIndexBudget();for(int i=0;i<65536;i++)vectorBudget.Add([new(new((ulong)(i>>8)),new(65535)),new(new((ulong)(i&255)),new(65535))]);Reject(()=>vectorBudget.Add([range,range,range]));
using(var jf=File.Create(args[2]))using(var jw=new Utf8JsonWriter(jf))WriteJson(jw,D(("samples",output),("completed",complete),("refused",refused),("sources",totalSources),("failures",failures),("prepared",prepared),("partialVectorFailure",partial),("budgetGuards",4),("guards",guards)));
if(complete!=13||refused!=0||totalSources!=50)throw new Exception("Actual index coverage changed.");
Console.WriteLine($"PASS13 actualglobal indices/{complete} ready/{refused} explicit selected-source refusals/{totalSources} typed sources/40 prepared AVL cases/3 entered-failure owners/{guards} guards; no023260/DOM.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected source-index refusal.");}
ulong[][] Ranges(IEnumerable<LegacyNativeInkPositionRange> key)=>key.Select(r=>new[]{r.First.Value,r.Last.Value}).ToArray();
Dictionary<string,object?> Actual(LegacyGlobalDocumentSourceIndexSnapshot s,LegacyGlobalDocumentIndexedSource[] sources)=>D(("intervalRoot",s.IntervalRoot),("vectorRoot",s.VectorRoot),("dirty",s.IntervalDirty),("added",s.SourcesAdded),
 ("intervals",s.Intervals.Select(n=>D(("fields",new[]{n.Identity,n.CreationPageIdentity,n.Slot,n.Left,n.Right,(int)n.Balance,n.MembershipHead,0}),("key",Ranges(n.CopyKey())))).ToArray()),
 ("vectors",s.Vectors.Select(n=>D(("fields",new[]{n.Identity,n.CreationPageIdentity,n.Slot,n.Left,n.Right,(int)n.Balance,n.MembershipHead,n.CurrentSource is null?0:Array.IndexOf(sources,n.CurrentSource)+1}),("key",Ranges(n.CopyKey())))).ToArray()),
 ("members",s.Memberships.Select(m=>new[]{m.Identity,m.CreationPageIdentity,m.Slot,Array.IndexOf(sources,m.Source)+1,m.Next}).ToArray()));
Dictionary<string,object?> Numeric(LegacyMainDocumentSourceIndexKernel<int> k)=>D(("intervalRoot",k.Intervals.Root?.Identity??0),("vectorRoot",k.Vectors.Root?.Identity??0),("dirty",k.Dirty),("added",k.SourcesAdded),
 ("intervals",k.Intervals.Nodes.Select(n=>D(("fields",new[]{n.Identity,n.PageIdentity,n.Slot,n.Left?.Identity??0,n.Right?.Identity??0,(int)n.Balance,n.MembershipHead,0}),("key",Ranges(n.Key)))).ToArray()),
 ("vectors",k.Vectors.Nodes.Select(n=>D(("fields",new[]{n.Identity,n.PageIdentity,n.Slot,n.Left?.Identity??0,n.Right?.Identity??0,(int)n.Balance,n.MembershipHead,n.CurrentSource}),("key",Ranges(n.Key)))).ToArray()),
 ("members",k.Memberships.Select(m=>new[]{m.Identity,m.PageIdentity,m.Slot,m.Source,m.Next}).ToArray()));
object[] Prefixes(LegacyFreshMainGlobalDocumentSourceIndices index)=>index.CopyPrefixes().Select(p=>(object)D(("stage",p.Stage),("char",new[]{p.CharacterSources,p.CharacterIntervals,p.CharacterVectors,p.CharacterMemberships,p.CharacterDirty?1:0}),("word",new[]{p.WordSources,p.WordIntervals,p.WordVectors,p.WordMemberships,p.WordDirty?1:0}))).ToArray();
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
