using InkRecognition;using InkRecognition.Math;using System.Text.Json;using System.Text.Json.Serialization;
if(args.Length!=2)throw new ArgumentException("frozen-fixtures output");
var fixtures=JsonSerializer.Deserialize(File.ReadAllText(args[0]),Json.Default.FixtureArray)!;
var features=Math2Mode0RawSourcePositions.PrepareFeatures(new InkStroke[]{new(new InkPoint[]{new(0,0),new(1,1)})},default);
using var field=Math2Mode0OriginalMathFieldSource.Prepare(LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document),"prepared-only");
var results=new List<Result>();int guards=0;
Math2Mode0OriginalArena Make(InputNode[] input)
{
 var arena=new Math2Mode0OriginalArena(field.SelectedNameOwner);
 foreach(var n in input)arena.NewDiffPayload(n.Kind,n.Name,0,n.Tag,n.Text,n.Number,n.Parent);
 return arena;
}
Math2DomDiffNode Wrap(Math2Mode0OriginalArena arena,int id)
{
 var n=arena.Nodes[id-1];return new(arena,n,n.Id,n.Kind,n.Name,Math2DomDiffScalar.From(n.Tag,n.Text,n.Number));
}
Result Snapshot(string name,string status,int visited,Math2Mode0OriginalArena arena)=>new(name,status,visited,
 arena.Nodes.Select(n=>new NodeState(n.Id,n.Kind,n.Name.Text,n.Parent,n.Tag,n.Text,n.Number,arena.WriterFreed(n))).ToArray(),
 arena.Nodes.Select(n=>new Link(n.Id,arena.Children(n.Id).Select(c=>c.Id).ToArray())).ToArray());
foreach(var f in fixtures)
{
 var arena=Make(f.Nodes);var work=new Math2DomDiffWork(default);string status="success";int visited=0;
 foreach(var a in f.Actions)
 {
  try
  {
   var record=new Math2DomDiffAction(a.Op,Wrap(arena,a.Target),a.Payload==0?null:Wrap(arena,a.Payload),a.Ordinal,
      newValue:a.Op==7?Math2DomDiffScalar.From(a.Tag,a.Text,a.Number):null);
   if(!arena.ApplyTemporaryAction(record,work)){status="unknown-opcode";break;}visited++;
  }
  catch(NotSupportedException){status="scope-refusal";break;}
 }
 results.Add(Snapshot(f.Name,status,visited,arena));
}
void Reject(Action action){try{action();}catch(Exception e)when(e is OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new Exception("Invalid kernel input accepted.");}
{
 var arena=Make([new(4,"",0,0,null,null),new(3,"root",1,0,null,null),new(3,"x",0,0,null,null)]);
 var op=new Math2DomDiffAction(2,Wrap(arena,2),Wrap(arena,3),0);
 string Before()=>JsonSerializer.Serialize(Snapshot("guard","",0,arena),Json.Default.Result);
 string before=Before();using var stop=new CancellationTokenSource();stop.Cancel();Reject(()=>arena.ApplyTemporaryAction(op,new(stop.Token)));if(Before()!=before)throw new Exception("Precancel mutated arena.");
 var exhausted=new Math2DomDiffWork(default);exhausted.Visit(Math2DomDiffWork.MaximumVisits);Reject(()=>arena.ApplyTemporaryAction(op,exhausted));if(Before()!=before)throw new Exception("Prework refusal mutated arena.");
 var foreign=Make([new(4,"",0,0,null,null),new(3,"root",1,0,null,null)]);Reject(()=>arena.ApplyTemporaryAction(new(2,Wrap(arena,2),Wrap(foreign,2),0),new(default)));
 Reject(()=>arena.ApplyTemporaryAction(new(2,Wrap(arena,2),Wrap(arena,2),0),new(default)));
 Reject(()=>arena.ApplyTemporaryAction(new(5,Wrap(arena,2),Wrap(arena,3)),new(default)));
}
{
 var arena=Make([new(4,"",0,0,null,null),new(3,"root",1,0,null,null),new(0,"a",2,4,null,1),new(0,"a",0,4,null,2)]);
 arena.ApplyTemporaryAction(new(5,Wrap(arena,2),Wrap(arena,4)),new(default));
 Reject(()=>arena.ApplyTemporaryAction(new(7,Wrap(arena,3),newValue:Math2DomDiffScalar.From(4,null,3)),new(default)));
}
// Tight budgets distinguish real list scans/shifts from one-unit bookkeeping.
// These refuse BEFORE the tested shift and must leave this operation unchanged.
foreach(int kind in new[]{0,1,2})
{
 var input=new List<InputNode>{new(4,"",0,0,null,null),new(3,"root",1,0,null,null)};
 if(kind==1){for(int i=0;i<10;i++)input.Add(new(0,"a"+i,2,4,null,i));input.Add(new(0,"a0",0,4,null,99));}
 else {for(int i=0;i<10;i++)input.Add(new(3,"n"+i,2,0,null,null));if(kind==0)input.Add(new(3,"payload",0,0,null,null));}
 var arena=Make(input.ToArray());var action=kind==1?new Math2DomDiffAction(5,Wrap(arena,2),Wrap(arena,13)):
   new Math2DomDiffAction(2,Wrap(arena,2),Wrap(arena,kind==0?13:3),kind==0?9:999);
 string before=JsonSerializer.Serialize(Snapshot("budget","",0,arena),Json.Default.Result);
 var work=new Math2DomDiffWork(default);work.Visit(Math2DomDiffWork.MaximumVisits-(kind==0?27:kind==1?20:30));
 Reject(()=>arena.ApplyTemporaryAction(action,work));
 if(before!=JsonSerializer.Serialize(Snapshot("budget","",0,arena),Json.Default.Result))throw new Exception("A scan/shift was performed before its work reservation.");
}
File.WriteAllText(args[1],JsonSerializer.Serialize(new Capture("PREPARED_TYPED_ARENA_ONLY_NO_CURRENT_OWNER_OR_APPLY_AUTHORITY",guards,results.ToArray()),Json.Default.Capture));
Console.WriteLine($"PASS {results.Count} frozen structural/scalar/prefix cases and {guards} guards; prepared kernel only, no owning apply/update.");
record InputNode(byte Kind,string Name,int Parent,byte Tag,string? Text,long? Number);
record InputAction(byte Op,int Target,int Payload,int Ordinal,byte Tag,string? Text,long? Number);
record Fixture(string Name,InputNode[] Nodes,InputAction[] Actions);
record NodeState(int Id,byte Kind,string Name,int Parent,byte Tag,string? Text,long? Number,bool Freed);
record Link(int Id,int[] Children);
record Result(string Name,string Status,int Visited,NodeState[] Nodes,Link[] Links);
record Capture(string Scope,int Guards,Result[] Results);
[JsonSerializable(typeof(Fixture[]))][JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Result))]internal partial class Json:JsonSerializerContext;
