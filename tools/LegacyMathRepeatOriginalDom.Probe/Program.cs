using InkRecognition.Math;
int controls=0;
void Check(bool value){if(!value)throw new InvalidDataException($"Prepared DOM control {controls}");controls++;}
void Reject(Action body){try{body();}catch(Exception e)when(e is InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException("Expected prepared refusal");}
Math2Mode0OriginalArena Arena(params Math2Mode0MathFieldNode[] rows){var a=new Math2Mode0OriginalArena(Math2Mode0RepeatDomViews.PreparedNames());a.Import(rows);return a;}
Math2Mode0OriginalArena Original()=>Arena(new Math2Mode0MathFieldNode(1,3,"mathField",0,1),new(2,3,"A",1,2),new(3,3,"B",1,3),new(4,0,"tag",3,4,4,null,3));
var payload=Arena(new Math2Mode0MathFieldNode(1,3,"X",0,1),new(2,0,"tag",0,2,4,null,5),new(3,0,"absent",0,3,4,null,9));
var original=Original();var oldAttribute=original.Nodes[3];
var ops=new Math2RepeatDomOperand[]{new(2,[],0,payload,1),new(7,[1],0,payload,2),new(4,[],1,payload,0),new(6,[1],0,payload,3)};
var plan=Math2RepeatDomPlan.Resolve(original,original.Nodes[0],ops,new(default));
Check(ReferenceEquals(plan.Parents[1],original.Nodes[2]));Check(ReferenceEquals(plan.Parents[3],original.Nodes[2]));
Reject(()=>plan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}}));Check(plan.Applied==0&&original.Nodes.Count==4);
plan.ApplyNext(new(default));Check(original.Children(1).Select(n=>n.Name.Text).SequenceEqual(new[]{"X","A","B"}));
// Native -1 mapping sentinel preserves OLD payload value rather than writing -1.
var sentinelOriginal=Original();var sentinelAttribute=sentinelOriginal.Nodes[3];
var sentinelPlan=Math2RepeatDomPlan.Resolve(sentinelOriginal,sentinelOriginal.Nodes[0],[new(7,[1],0,payload,2)],new(default));
sentinelPlan.ApplyNext(new(default),new Dictionary<long,long>{{5,-1}});Check(sentinelAttribute.Number==5&&ReferenceEquals(sentinelAttribute,sentinelOriginal.Nodes[3]));
plan.ApplyNext(new(default),new Dictionary<long,long>{{5,6}});Check(ReferenceEquals(oldAttribute,original.Nodes[3]));Check(oldAttribute.Number==6&&oldAttribute.Parent==3);
plan.ApplyNext(new(default));Check(original.Children(1).Select(n=>n.Name.Text).SequenceEqual(new[]{"X","B"}));Check(original.WriterFreed(original.Nodes[1]));Check(!original.WriterFreed(original.Nodes[2]));
plan.ApplyNext(new(default));Check(plan.Completed&&plan.Applied==4);Check(!original.WriterFreed(oldAttribute));Reject(()=>plan.ApplyNext(new(default)));
// Both missing-attribute6 and existing-attribute6 detach without allocator free.
var removal=Math2RepeatDomPlan.Resolve(original,original.Nodes[0],[new(6,[1],0,payload,2)],new(default));removal.ApplyNext(new(default));Check(oldAttribute.Parent==0&&!original.WriterFreed(oldAttribute));
var before=Original();using var canceled=new CancellationTokenSource();canceled.Cancel();
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,new(canceled.Token)));Check(before.Nodes.Count==4&&before.Children(1).Count==2);
var pending=Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,new(default));Reject(()=>pending.ApplyNext(new(canceled.Token)));Check(pending.Applied==0&&before.Nodes.Count==4);
var text=Arena(new Math2Mode0MathFieldNode(1,1,"text",0,1,1,"unsupported"));
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],[ops[0],new(7,[],0,text,1)],new(default)));Check(before.Nodes.Count==4);
Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],[new(7,[9],0,payload,2)],new(default)));Check(before.Nodes.Count==4);
var exhausted=new Math2DomDiffWork(default){Visits=Math2DomDiffWork.MaximumVisits};Reject(()=>Math2RepeatDomPlan.Resolve(before,before.Nodes[0],ops,exhausted));Check(before.Nodes.Count==4);
// Native EB9C0 preserves tag4/tag7 widths; From enforces canonical widths.
foreach(var test in new (byte Tag,string? Text,long? Number)[]{(4,null,long.MinValue),(4,null,long.MaxValue),(7,null,int.MinValue),(7,null,int.MaxValue),(3,"",null),(2,"123456789012345",null),(1,"1234567890123456",null),(1,new string((char)0x00e9,8),null),(10,null,0)})
{
 var a=Original();var existing=a.Nodes[3];var v=Arena(new Math2Mode0MathFieldNode(1,0,"tag",0,1,test.Tag,test.Text,test.Number));
 var q=Math2RepeatDomPlan.Resolve(a,a.Nodes[0],[new(7,[1],0,v,1)],new(default));q.ApplyNext(new(default));
 Check(ReferenceEquals(existing,a.Nodes[3])&&existing.Tag==test.Tag&&existing.Text==(test.Tag==3?"":test.Text)&&existing.Number==test.Number);
}
foreach(var malformed in new (byte Tag,string? Text,long? Number)[]{(1,"",null),(1,"123456789012345",null),(2,"",null),(2,"1234567890123456",null),(7,null,(long)int.MaxValue+1),(7,null,(long)int.MinValue-1)})
{
 var a=Original();var v=Arena(new Math2Mode0MathFieldNode(1,0,"tag",0,1,malformed.Tag,malformed.Text,malformed.Number));
 Reject(()=>Math2RepeatDomPlan.Resolve(a,a.Nodes[0],[new(7,[1],0,v,1)],new(default)));Check(a.Nodes.Count==4&&a.Nodes[3].Tag==4&&a.Nodes[3].Number==3);
}
Console.WriteLine($"PASS {controls} prepared cached-parent/current-ordinal/scalar-identity/detach/cancellation controls; no original apply authority.");
