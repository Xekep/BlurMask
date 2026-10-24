using InkRecognition;
using System.Text.Json;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("fresh prepared capture");
int[][] cases=[[1],[3],[1,2],[2,1,3],Enumerable.Repeat(3,16).ToArray(),Enumerable.Repeat(1,256).ToArray(),[4096],Enumerable.Repeat(16,256).ToArray()];
var output=new List<object>();int aliases=0;
foreach(var counts in cases)
{
 var doc=Document(counts);var view=LegacyFreshRawInkCurrentView.Prepare(doc);
 var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,["english-field"]);var a=original.Run();
 var chosen=a.Inputs.Single();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);var b=selected.Run();
 var binding=LegacyLibrarySelectedInputBinding.FromOriginal(selected);var pending=LegacyOwnedFreshTextSourceContext.PrepareSelectedBinding(binding);
 if(a.Nodes.Count!=10||b.Nodes.Count!=10||a.Attempts!=19||b.Attempts!=17+counts.Length||a.History.Count!=5||a.ObserverStates.Count!=10||a.Trace.Count!=33||b.Trace.Count!=27+counts.Length)
  throw new InvalidDataException("Source helper cardinality inventory differs.");
 if(a.NextRegistryId!=1||b.NextRegistryId!=1||chosen.RegistryId!=0||pending.NextSelectionId!=1||pending.CopyPendingSelections().Count!=0||
  !ReferenceEquals(binding.Document,doc)||!ReferenceEquals(binding.CurrentView,view)||!ReferenceEquals(binding.OriginalSelected,selected)||
  !ReferenceEquals(b.Registry[0].OriginalEntry,chosen)||!ReferenceEquals(pending.InputBinding,binding))throw new InvalidDataException("Actual original/selected/pending lineage differs.");
 var raw=b.CopyRawStorage();byte saved=raw[0][0];raw[0][0]^=255;
 if(b.CopyRawStorage()[0][0]!=saved)throw new InvalidDataException("Selected raw snapshot aliases caller mutation.");aliases++;
 var id=a.Nodes.Single(n=>n.Name=="id");var value=id.CopyValueBytes();value[0]^=255;
 if(id.CopyValueBytes()[0]!=(byte)'e')throw new InvalidDataException("Original DOM value aliases caller mutation.");aliases++;
 output.Add(new{Counts=counts,OriginalAttempts=a.Attempts,SelectedAttempts=b.Attempts,OriginalNext=a.NextRegistryId,SelectedNext=b.NextRegistryId,InputId=chosen.RegistryId,
  PendingNext=pending.NextSelectionId,PendingCount=pending.CopyPendingSelections().Count,OriginalHistory=a.History.Count,OriginalObservers=a.ObserverStates.Count,
  OriginalTrace=a.Trace.Count,SelectedTrace=b.Trace.Count,Raw=b.CopyRawStorage(),
  Original=a.Nodes.Select(n=>new{n.Identity,n.Kind,n.Name,n.NativeValueTag,Value=n.CopyValueBytes(),n.ParentIdentity,Children=n.Children.ToArray(),Attributes=n.Attributes.ToArray(),Namespace=n.NamespaceUriName?.Text,Prefix=n.NamespacePrefixName?.Text}).ToArray(),
  Selected=b.Nodes.Select(n=>new{n.Identity,n.OriginalNodeIdentity,n.Name,n.NativeValueTag,Value=n.CopyValueBytes(),n.ParentIdentity,Children=n.Children.ToArray(),Attributes=n.Attributes.ToArray(),Namespace=n.NamespaceUriName?.Text,Prefix=n.NamespacePrefixName?.Text}).ToArray(),
  Registry=b.Registry.Select(r=>new{r.RegistryId,r.Name,r.Category,Pairs=r.CopyRanges().SelectMany(k=>new[]{k.First.Value,k.Last.Value}).ToArray()}).ToArray(),
  SameDocument=true,SameCurrentView=true,NamesHaveDistinctOwners=!ReferenceEquals(original.NameOwner,selected.NameOwner)});
}
var guards=new List<string>();var guardProofs=new List<object>();var d=Document([3]);var v=LegacyFreshRawInkCurrentView.Prepare(d);
var originalGuard=LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["english-field"]);var og=originalGuard.Run();
var foreign=LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["other"]);var fg=foreign.Run();
Refuse<ArgumentException>("foreign-chosen-input",()=>LegacyLibraryOriginalSelectedFieldContext.Prepare(originalGuard,fg.Inputs[0]));
Refuse<InvalidOperationException>("original-once",()=>originalGuard.Run());
var freshSelected=LegacyLibraryOriginalSelectedFieldContext.Prepare(originalGuard,og.Inputs[0]);
Refuse<NotSupportedException>("incomplete-binding",()=>LegacyLibrarySelectedInputBinding.FromOriginal(freshSelected));
freshSelected.Run();Refuse<InvalidOperationException>("selected-once",()=>freshSelected.Run());
Refuse<ArgumentException>("no-fields",()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,[]));
Refuse<NotSupportedException>("duplicate-fields",()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["same","same"]));
Refuse<NotSupportedException>("invalid-field-path",()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["a/b"]));
Refuse<ArgumentException>("raw-object-bound",()=>Document(Enumerable.Repeat(1,257).ToArray()));
Refuse<ArgumentException>("raw-point-bound",()=>Document([4097]));
var canceled=new CancellationToken(true);
Cancel("view-prepare-cancel",()=>LegacyFreshRawInkCurrentView.Prepare(d,canceled));
Cancel("original-prepare-cancel",()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["english-field"],canceled));
Cancel("selected-prepare-cancel",()=>LegacyLibraryOriginalSelectedFieldContext.Prepare(originalGuard,og.Inputs[0],canceled));
var beforeEntry=LegacyLibraryOriginalTextFieldBootstrap.Prepare(v,["english-field"]);
Cancel("original-preentry-cancel",()=>beforeEntry.Run(canceled));if(beforeEntry.State.Succeeded||beforeEntry.State.IsFaulted)throw new InvalidDataException("Preentry cancellation mutated bootstrap.");
var entered=beforeEntry.Run();var selectedBefore=LegacyLibraryOriginalSelectedFieldContext.Prepare(beforeEntry,entered.Inputs[0]);
Cancel("selected-preentry-cancel",()=>selectedBefore.Run(canceled));if(selectedBefore.State.Succeeded||selectedBefore.State.IsFaulted)throw new InvalidDataException("Preentry cancellation mutated selected owner.");
selectedBefore.Run();
File.WriteAllBytes(args[0],JsonSerializer.SerializeToUtf8Bytes(new{Scope="MODEL_FREE_REAL_BOOTSTRAP_HELPERS_NOT_EN_GRAPH_ISSUER",Cases=output,Guards=guards,GuardProofs=guardProofs,DefensiveCopyChecks=aliases,BootstrapHelpersExecuted=true,ENOwningExecuted=false,NN=false,DocumentApplied=false}));
void Refuse<T>(string name,Action f)where T:Exception{bool reached=false;try{reached=true;f();}catch(Exception e)when(e.GetType()==typeof(T)){guards.Add(name);guardProofs.Add(new{Name=name,ExceptionType=e.GetType().FullName,Reached=reached,ExpectedTokenSupplied=false,TokenMatches=false,TokenRequested=false});return;}throw new InvalidDataException("Exact helper refusal required: "+name);}
void Cancel(string name,Action f){bool reached=false;try{reached=true;f();}catch(OperationCanceledException e)when(e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==canceled&&canceled.IsCancellationRequested){guards.Add(name);guardProofs.Add(new{Name=name,ExceptionType=e.GetType().FullName,Reached=reached,ExpectedTokenSupplied=true,TokenMatches=e.CancellationToken==canceled,TokenRequested=canceled.IsCancellationRequested});return;}throw new InvalidDataException("Exact requested token required: "+name);}
static LegacyOwnedRawInkDocument Document(int[] counts)=>LegacyOwnedRawInkDocument.Create(counts.Select((n,s)=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(s*100+i,i*2)).ToArray())).ToArray(),default);
