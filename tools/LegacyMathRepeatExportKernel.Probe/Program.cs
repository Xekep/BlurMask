using InkRecognition;using InkRecognition.Math;
int count=0;void Check(bool yes){if(!yes)throw new InvalidDataException($"Prepared native export control {count}");count++;}
void Refuse(Action a){try{a();}catch(Exception e)when(e is InvalidOperationException or NotSupportedException or OperationCanceledException){count++;return;}throw new InvalidDataException("Prepared refusal failed");}
for(int export=0;export<2;export++)
{
 var oldWriter=new object();var nextWriter=new object();var fourth=new object();var names=Math2Mode0RepeatDomViews.PreparedNames();var outputNames=Math2Mode0RepeatDomViews.PreparedNames();int cloned=0;
 Math2DomDiffNode Node(object writer,int id,byte kind,string name)=>new(writer,new object(),id,kind,Math2Mode0RepeatDomViews.PreparedName(names,name),Math2DomDiffScalar.From(kind==0?(byte)4:(byte)0,null,kind==0?3:null));
 var old=Node(oldWriter,1,3,"mathField");var next=Node(nextWriter,1,3,"mathField");var gone=Node(oldWriter,2,0,"old");gone.Parent=old;old.Attributes.Add(gone);var added=Node(nextWriter,2,0,"new");added.Parent=next;next.Attributes.Add(added);var child=Node(oldWriter,3,3,"mathResult");child.Parent=old;old.Children.Add(child);
 Math2DomDiffNode Clone(Math2DomDiffNode n){cloned++;return new(export==1?fourth:oldWriter,new object(),100+cloned,n.Kind,Math2Mode0RepeatDomViews.PreparedName(export==1?outputNames:names,n.Name.Text),n.Scalar.Clone(new(default)));}
 var actions=export==1?Math2DomDefaultDiff.ProduceExport(old,next,fourth,Clone,new(default)):Math2DomDefaultDiff.Produce(old,next,Clone,new(default));
 Check(actions.Select(a=>a.Operation).SequenceEqual(new byte[]{6,5,4}));Check(cloned==(export==1?3:1));
 foreach(var a in actions){Check(ReferenceEquals(a.NodeAt18.Writer,oldWriter));Check(ReferenceEquals(a.NodeAt20!.Writer,export==1?fourth:oldWriter));}
 if(export==0){Check(ReferenceEquals(actions[0].NodeAt20,gone));Check(ReferenceEquals(actions[2].NodeAt20,child));}
 else{Check(!ReferenceEquals(actions[0].NodeAt20,gone));Check(!ReferenceEquals(actions[2].NodeAt20,child));}
 Refuse(()=>Math2DomDefaultDiff.ProduceExport(old,next,oldWriter,Clone,new(default)));Refuse(()=>Math2DomDefaultDiff.ProduceExport(old,next,nextWriter,Clone,new(default)));
 var before=cloned;using var cancellation=new CancellationTokenSource();cancellation.Cancel();Refuse(()=>Math2DomDefaultDiff.ProduceExport(old,next,fourth,Clone,new(cancellation.Token)));Check(cloned==before);
 var cap=new Math2DomDiffWork(default){Visits=Math2DomDiffWork.MaximumVisits};Refuse(()=>Math2DomDefaultDiff.ProduceExport(old,next,fourth,Clone,cap));Check(cloned==before);
 old.Attributes.Clear();old.Children.Clear();next.Attributes.Clear();next.Children.Add(Node(nextWriter,4,3,"mathResult"));next.Children[0].Parent=next;
 var insert=export==1?Math2DomDefaultDiff.ProduceExport(old,next,fourth,Clone,new(default)):Math2DomDefaultDiff.Produce(old,next,Clone,new(default));Check(insert.Count==1&&insert[0].Operation==2&&insert[0].Ordinal==0);
 var input=Node(oldWriter,7,3,"input");input.Parent=old;old.Children.Add(input);var math=Node(oldWriter,8,3,"mathResult");math.Parent=old;old.Children.Add(math);Math2DomDiffNode? symbol=null;
 for(int k=0;k<3;k++){symbol=Node(oldWriter,9+k,3,"mathSymbol");symbol.Parent=math;math.Children.Add(symbol);}
 Check(Math2Mode0PriorRepeatOutgoing.PreparedPath(old,symbol,new(default)).SequenceEqual(new[]{1,2}));Check(Math2Mode0PriorRepeatOutgoing.PreparedPath(old,old,new(default)).Length==0);
 var attr=Node(oldWriter,20,0,"tag");attr.Parent=symbol;symbol!.Attributes.Add(attr);Refuse(()=>Math2Mode0PriorRepeatOutgoing.PreparedPath(old,attr,new(default)));
 Check(Math2Mode0PriorRepeatOutgoing.PreparedPath(old,attr.Parent,new(default)).SequenceEqual(new[]{1,2}));
}
Console.WriteLine($"PASS {count} prepared fourth-writer/old-vs-export operand/path controls; no owning transfer/NN/original apply.");
