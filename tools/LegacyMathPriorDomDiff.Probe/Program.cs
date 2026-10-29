using InkRecognition;using InkRecognition.Math;using System.Text.Json;using System.Text.Json.Serialization;
if(args.Length!=5)throw new ArgumentException("model rawres officialORT frozenfixtures evidence");int controls=0;
void Require(bool yes){if(!yes)throw new InvalidDataException($"Diff assertion {controls}");controls++;}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){controls++;return;}throw new Exception($"Diff refusal {controls}");}
var features=Math2Mode0RawSourcePositions.PrepareFeatures(new InkStroke[]{new(new InkPoint[]{new(0,0),new(1,1)})},default);using var preparedField=Math2Mode0OriginalMathFieldSource.Prepare(LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document),"prepared-only");var nameArena=new Math2Mode0OriginalArena(preparedField.SelectedNameOwner);var canonical=new Dictionary<string,LegacyLibraryOriginalDomName>();
LegacyLibraryOriginalDomName Name(string text){if(!canonical.TryGetValue(text,out var n))canonical.Add(text,n=nameArena.NewDiffPayload(3,text,0,0,null,null,0).Name);return n;}
object oldWriter=new(),newWriter=new();int id=0;
Math2DomDiffNode Node(object writer,byte kind,string name,byte tag=0,string? text=null,long? number=null)=>new(writer,new object(),++id,kind,Name(name),Math2DomDiffScalar.From(tag,text,number));
Math2DomDiffNode Element(object writer,string name)=>Node(writer,3,name);
var fixtures=JsonSerializer.Deserialize(File.ReadAllText(args[3]),Json.Default.FixtureArray)!;
foreach(var f in fixtures){var a=f.Old.Select(c=>Element(oldWriter,c.ToString())).ToArray();var b=f.New.Select(c=>Element(newWriter,c.ToString())).ToArray();var work=new Math2DomDiffWork(default);var pairs=Math2DomDefaultDiff.Align(a,b,work);Require(pairs.Select(x=>new[]{x.Old,x.New}).Zip(f.Pairs,(x,y)=>x.SequenceEqual(y)).All(x=>x)&&pairs.Length==f.Pairs.Length);}
foreach(byte tag in new byte[]{1,2,3,4,7,10}){string? text=tag==1?new('x',16):tag==2?"abc":null;long? n=tag is 4 or 7?42:tag==10?0:null;var a=Math2DomDiffScalar.From(tag,text,n);Require(a.Equal(a.Clone(new(default)),new(default)));}
Require(!Math2DomDiffScalar.From(4,null,long.MaxValue).Equal(Math2DomDiffScalar.From(4,null,-1),new(default)));
Require(!Math2DomDiffScalar.From(2,"123456789012345",null).Equal(Math2DomDiffScalar.From(2,"123456789012344",null),new(default)));
foreach(byte tag in new byte[]{0,5,6,8,9,11})Reject(()=>Math2DomDiffScalar.From(tag,null,1).Equal(Math2DomDiffScalar.From(tag,null,1),new(default)));
Reject(()=>Math2DomDiffScalar.From(1,"x",null));Reject(()=>Math2DomDiffScalar.From(2,"",null));Reject(()=>Math2DomDiffScalar.From(10,null,1));Reject(()=>Math2DomDiffScalar.From(7,null,long.MaxValue));
Reject(()=>Math2DomDiffScalar.From(4,null,1).Equal(Math2DomDiffScalar.From(7,null,1),new(default)));
var workLimit=new Math2DomDiffWork(default);workLimit.Visit(Math2DomDiffWork.MaximumVisits);Reject(()=>workLimit.Visit());Require(workLimit.Visits==Math2DomDiffWork.MaximumVisits);
var cellLimit=new Math2DomDiffWork(default);cellLimit.Matrix(Math2DomDiffWork.MaximumCells);Reject(()=>cellLimit.Matrix(1));Require(cellLimit.Cells==Math2DomDiffWork.MaximumCells);
Reject(()=>Math2DomDefaultDiff.Align(Enumerable.Range(0,513).Select(_=>Element(oldWriter,"A")).ToArray(),[],new(default)));
using var canceled=new CancellationTokenSource();canceled.Cancel();Reject(()=>Math2DomDefaultDiff.Align([],[],new(canceled.Token)));
// Prepared operands verify action field/cursor branches only; never enter CB8 or confer ownership.
var prepared=new List<Math2DomDiffActionSnapshot>();
{
 var old=Element(oldWriter,"root");var next=Element(newWriter,"root");old.Attributes.Add(Node(oldWriter,0,"delete",4,number:1));old.Attributes.Add(Node(oldWriter,0,"change",4,number:2));next.Attributes.Add(Node(newWriter,0,"change",4,number:3));next.Attributes.Add(Node(newWriter,0,"add",4,number:4));old.Children.Add(Element(oldWriter,"A"));old.Children.Add(Element(oldWriter,"B"));next.Children.Add(Element(newWriter,"C"));
 Math2DomDiffNode Clone(Math2DomDiffNode n)=>new(oldWriter,new object(),++id,n.Kind,n.Name,n.Scalar.Clone(new(default)));
 var actions=Math2DomDefaultDiff.Produce(old,next,Clone,new(default));Require(actions.Select(x=>x.Operation).SequenceEqual(new byte[]{6,7,5,4,4,2}));Require(actions.Where(x=>x.Operation is 4 or 2).Select(x=>x.Ordinal).SequenceEqual(new[]{0,0,0}));Require(actions.Where(x=>x.Operation is 2 or 5).All(x=>ReferenceEquals(x.NodeAt20!.Writer,oldWriter)));Require(actions.Single(x=>x.Operation==7).NodeAt18.Name.Text=="change");prepared.AddRange(actions.Select(x=>x.Snapshot()));
}
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));var actual=new List<Actual>();long exactBudget=0;
foreach(int width in new[]{1,3,16})foreach(bool reverse in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle,width,reverse);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);var pre=current.CopyResult();using var next=width==1?Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current):Math2Mode0PriorRawOnlySource.PrepareDiagnostic(runtime,bundle,run,current,width);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);var before=candidate.CopyResult();
 Reject(()=>Math2Mode0PriorDomDiff.Create(candidate,canceled.Token));Reject(()=>Math2Mode0PriorDomDiff.CreateDiagnostic(candidate,0,null));
 Math2Mode0PriorCandidatePreparation.DiffBorrow? escaped=null;candidate.WithDiffInput(b=>{escaped=b;Reject(()=>Math2Mode0PriorDomDiff.Create(candidate));return 0;},default);Reject(()=>escaped!.Check());
 using var diff=Math2Mode0PriorDomDiff.Create(candidate);var data=diff.CopyResult();if(width==1&&!reverse)exactBudget=data.Reservation;
 Require(data.Complete&&data.Ready&&!data.Failed&&!data.Released&&data.HeaderLive);Require(data.Produced==3&&data.Kept==0&&data.Filtered==3);Require(data.Actions.All(x=>x.Operation==7&&x.Filtered&&x.EnvelopeKind==0&&x.Name=="tag"&&x.OldTag==4&&x.NewTag==4));Require(data.Actions.Select(x=>BitConverter.ToInt64(x.OldStorage)).SequenceEqual(new long[]{1,2,3}));Require(data.Actions.Select(x=>BitConverter.ToInt64(x.NewStorage)).SequenceEqual(new long[]{-2,-3,-4}));
 Require(JsonSerializer.Serialize(before,Json.Default.Math2Mode0PriorCandidateSnapshot)==JsonSerializer.Serialize(candidate.CopyResult(),Json.Default.Math2Mode0PriorCandidateSnapshot));Reject(()=>Math2Mode0PriorDomDiff.Create(candidate));var detached=diff.CopySnapshot();detached.Actions[0].OldStorage[0]=99;Require(diff.CopyResult().Actions[0].OldStorage[0]==1);
 var nnBeforeRelease=next.CopyResult();next.ReleaseDecoder();Require(diff.CopyResult().Ready);actual.Add(new(width,reverse,p.Field.Document.Strokes.ToArray(),scope.CopySnapshot(),pre,current.CopyResult(),nnBeforeRelease,before,data));diff.Dispose();Reject(()=>diff.CopyResult());Require(!diff.CopySnapshot().HeaderLive);
}
foreach(var stage in Enum.GetValues<Math2Mode0DomDiffStage>())foreach(bool cancel in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);using var stop=new CancellationTokenSource();Math2Mode0PriorDomDiff? retained=null;
 try{Math2Mode0PriorDomDiff.CreateDiagnostic(candidate,Math2Mode0PriorCandidatePreparation.DefaultBudget,(s,d)=>{retained=d;Reject(()=>d.CopyResult());if(s==stage){if(cancel)stop.Cancel();else throw new IOException("Produced actual diff prefix");}},stop.Token);throw new Exception();}catch(Exception e)when(e is IOException or OperationCanceledException){controls++;}
 var prefix=retained!.CopySnapshot();Require(prefix.Failed&&!prefix.Ready);Require(prefix.Produced==(stage==Math2Mode0DomDiffStage.ActionLinked?1:stage>=Math2Mode0DomDiffStage.Produced?3:0));Require(prefix.Kept+prefix.Filtered==prefix.Produced);Reject(()=>retained.CopyResult());Reject(()=>Math2Mode0PriorDomDiff.Create(candidate));Require(candidate.CopyResult().Ready);retained.Dispose();Require(!retained.CopySnapshot().HeaderLive);
}
foreach(string close in new[]{"scope","freeze","current","outgoing","next","candidate","self"})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);Math2Mode0PriorDomDiff? retained=null;
 Reject(()=>Math2Mode0PriorDomDiff.CreateDiagnostic(candidate,Math2Mode0PriorCandidatePreparation.DefaultBudget,(s,d)=>{retained=d;if(s!=Math2Mode0DomDiffStage.Completed)return;if(close=="scope")scope.Dispose();else if(close=="freeze")freeze.Dispose();else if(close=="current")current.Dispose();else if(close=="outgoing")p.Outgoing.Dispose();else if(close=="next")next.Dispose();else if(close=="candidate")candidate.Dispose();else d.Dispose();}));Require(retained!.CopySnapshot().Failed&&!retained.CopySnapshot().Ready);Reject(()=>retained.CopyResult());retained.Dispose();
}
using(var p=Pipeline.Create(runtime,run,bundle))
{
 p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);Reject(()=>Math2Mode0PriorDomDiff.CreateDiagnostic(candidate,exactBudget-1,null));using var exact=Math2Mode0PriorDomDiff.CreateDiagnostic(candidate,exactBudget,null);Require(exact.CopyResult().Reservation==exactBudget);
}
File.WriteAllText(args[4],JsonSerializer.Serialize(new Capture(controls,runtime.Version,fixtures.Length,prepared.ToArray(),actual.ToArray()),Json.Default.Capture));Console.WriteLine($"PASS {actual.Count} genuine completed NEW NN -> owned default DOM diff/CB8 filter; {fixtures.Length} frozen alignments; {controls} controls. No apply/receiver/update.");
record Fixture(string Old,string New,int[][] Pairs);
record Actual(int Width,bool Reversed,InkStroke[] Ink,Math2Mode0PriorSnapshot Prior,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0PriorNnSnapshot NewNn,Math2Mode0PriorCandidateSnapshot Candidate,Math2Mode0DomDiffSnapshot Diff);
record Capture(int Controls,string Runtime,int AlignmentCases,Math2DomDiffActionSnapshot[] PreparedActions,Actual[] Actual);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Fixture[]))][JsonSerializable(typeof(Math2Mode0PriorCandidateSnapshot))]internal partial class Json:JsonSerializerContext;
