using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json]");
using var control=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,checkedNodes=0;
foreach(var c in control.RootElement.EnumerateArray())
{
 var f=new LegacySymbolContextFactory();var ids=new Dictionary<ushort,ulong>();
 foreach(var item in c.GetProperty("sequence").EnumerateArray())
 {
  ushort symbol=item.GetUInt16();var result=f.ResolveInitial(symbol);
  if(ids.TryGetValue(symbol,out ulong prior))Require(!result.Created&&result.Identity==prior,"Duplicate key lost stable identity.");
  else {Require(result.Created&&result.Identity!=0,"New key failed.");ids.Add(symbol,result.Identity);}
 }
 var expected=c.GetProperty("nodes");Require(f.NodeCount==expected.GetArrayLength(),"AVL node count differs.");
 int root=f.RootTreeIdentity==0?-1:f.ReadNode(f.RootTreeIdentity).Symbol;Require(root==c.GetProperty("root").GetInt32(),$"AVL root differs in case{cases}.");
 foreach(var node in expected.EnumerateArray())
 {
  var actual=f.ReadNode(ids[node[0].GetUInt16()]);
  int l=actual.Left==0?-1:f.ReadNode(actual.Left).Symbol,r=actual.Right==0?-1:f.ReadNode(actual.Right).Symbol;
  Require(l==node[1].GetInt32()&&r==node[2].GetInt32()&&actual.Balance==node[3].GetInt32()&&actual.ParentLink==0&&actual.ChildTreeRoot==0,$"Native AVL shape/balance differs in case{cases}.");checkedNodes++;
 }
 CheckTree(f,f.RootTreeIdentity);cases++;
}
var paths=new LegacySymbolContextFactory();var a=paths.ResolveInitial(65535);var b=paths.ResolveInitial(0);
var ax=paths.ResolveChild(a.Identity,42);var bx=paths.ResolveChild(b.Identity,42);Require(ax.Created&&bx.Created&&ax.Identity!=bx.Identity,"Equal key under distinct path owners shared one context.");
paths.LinkCreatedContext(ax.Identity,a.Identity);paths.LinkCreatedContext(bx.Identity,b.Identity);
Require(paths.ReadNode(ax.Identity).ParentLink==a.Identity&&paths.ReadNode(bx.Identity).ParentLink==b.Identity&&!paths.ResolveChild(a.Identity,42).Created,"Child context/link reuse differs.");
for(ushort key=0;key<100;key++)if(key!=42){var child=paths.ResolveChild(a.Identity,key);paths.LinkCreatedContext(child.Identity,a.Identity);}
foreach(var child in paths.CopyNodes().Where(n=>n.ParentLink==a.Identity))Require(child.ParentLink==a.Identity,"AVL rotations changed symbol-path parent link.");
CheckTree(paths,paths.ReadNode(a.Identity).ChildTreeRoot);Reject(()=>paths.LinkCreatedContext(ax.Identity,b.Identity));Reject(()=>paths.LinkCreatedContext(a.Identity,b.Identity));
var foreign=new LegacySymbolContextFactory();Reject(()=>foreign.ReadNode(a.Identity));Reject(()=>foreign.ResolveChild(a.Identity,1));
var initial=new LegacySymbolContextFactory();ushort[] keys=[5,65535,5,0xfffd,0xfffa,0xfffb,0,0xffff,0xfffc];var mapped=initial.CreateInitialSourceMap(keys);
Require(mapped[0]==mapped[2]&&mapped[1]==0&&mapped[3]==0&&mapped[7]==0&&initial.NodeCount==5,"Initial source68 skip/shared acoustic-control keys differ.");
for(int i=0;i<keys.Length;i++)if(mapped[i]!=0)Require(initial.ReadNode(mapped[i]).Symbol==keys[i]&&initial.ReadNode(mapped[i]).ParentLink==0,"Initial map used lexical position instead of mapped key.");
var pool=new LegacySymbolContextFactory();for(ushort key=0;key<101;key++)pool.ResolveInitial(key);
var checkpoint=pool.Capture();var baseline=pool.CopyNodes().ToArray();int baseCapacity=pool.PageDescriptorCapacity;
var slot101=pool.ResolveInitial(101);var page1=pool.ResolveInitial(102);
Require(pool.PageCount==2&&pool.CopyPageUsedCounts().SequenceEqual(new[]{102,1})&&pool.ReadNode(slot101.Identity).PageIndex==0&&pool.ReadNode(slot101.Identity).PageSlot==101&&pool.ReadNode(page1.Identity).PageIndex==1&&pool.ReadNode(page1.Identity).PageSlot==0,"Native102-node page boundary differs.");
for(ushort key=103;key<420;key++)pool.ResolveInitial(key);int grownCapacity=pool.PageDescriptorCapacity;
pool.Restore(checkpoint);Require(pool.NodeCount==101&&pool.PageCount==1&&pool.PageDescriptorCapacity==grownCapacity&&pool.CopyNodes().SequenceEqual(baseline),"Checkpoint root/page restore or retained descriptor capacity differs.");
Reject(()=>pool.ReadNode(slot101.Identity));Reject(()=>pool.ReadNode(page1.Identity));
var reused=pool.ResolveInitial(5000);Require(reused.Identity!=slot101.Identity&&pool.ReadNode(reused.Identity).PageSlot==101&&pool.PageCount==1,"Restored partial page slot did not recycle safely.");
var newPage=pool.ResolveInitial(5001);Require(pool.ReadNode(newPage.Identity).PageIndex==1&&pool.ReadNode(newPage.Identity).PageSlot==0&&pool.PageDescriptorCapacity==grownCapacity,"Restored page descriptor capacity was not retained.");
Reject(()=>foreign.Restore(checkpoint));
var limited=new LegacySymbolContextFactory(1);var first=limited.ResolveInitial(1);Require(limited.ResolveInitial(2)==new LegacyDictionaryQueryContext(0,false)&&limited.ResolveInitial(1).Identity==first.Identity,"Bound failure changed native existing-key reuse.");
var before=limited.CopyNodes().ToArray();Reject(()=>limited.ResolveInitial(3,new CancellationToken(true)));Require(limited.CopyNodes().SequenceEqual(before),"Canceled lookup mutated tree.");
Reject(()=>pool.Restore(checkpoint,new CancellationToken(true)));Reject(()=>new LegacySymbolContextFactory(0));Reject(()=>new LegacySymbolContextFactory(1_000_001));
Console.WriteLine($"PASS {cases} independent exhaustive/random AVL shapes; {checkedNodes} exact node/link/balance checks; shared/different-parent keys, source68 mappings,102-node pages, snapshot/recycle/stale/foreign IDs, allocation bound and cancellation.");
if(args.Length==1)return;
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var dict=LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet,model.LexicalAlphabet);
var real=new LegacySymbolContextFactory();var contexts=real.CreateInitialSourceMap(dict);
Require(contexts.Length==model.LexicalAlphabet.LabelCount&&real.NodeCount==dict.Where(x=>x is not (0xffff or 0xfffd)).Distinct().Count(),"Actual initial lexical map/context cardinality differs.");
for(int i=0;i<dict.Length;i++)if(contexts[i]!=0)Require(real.ReadNode(contexts[i]).Symbol==dict[i],"Actual mapped acoustic key changed.");
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));
var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var map=new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,dict,[]).Refresh(Enumerable.Repeat((byte)7,model.AcousticAlphabet.LabelCount).ToArray());
var source=new LegacyPreparedDictionaryQuerySource(model.Lexicon,map,contexts);var q=new LegacyPreparedDictionaryQuery{Node=0,Capacity=128,ContextRoot=real.ContainerIdentity};
var records=new List<byte[]>();int pages=0;
while(true)
{
 var page=source.Read(q,false);if(page.ReturnedCount==0)break;pages++;
 for(int i=0;i<page.ReturnedCount;i++)records.Add(page.Records.Slice(i*32,32).ToArray());q.Skip+=page.ReturnedCount;
}
Require(records.Count==132&&pages==2&&records.All(r=>BinaryPrimitives.ReadUInt64LittleEndian(r.AsSpan(8))!=0),"Actual native root query lost contexts/root skip/pagination.");
foreach(var r in records)Require(real.ReadNode(BinaryPrimitives.ReadUInt64LittleEndian(r.AsSpan(8))).Symbol==BinaryPrimitives.ReadUInt16LittleEndian(r.AsSpan(20)),"Initial query context key is not its mapped acoustic symbol.");
var branch=records.First(r=>model.Lexicon.GetChildCount((int)BinaryPrimitives.ReadUInt32LittleEndian(r))>0);int branchNode=(int)BinaryPrimitives.ReadUInt32LittleEndian(branch);ulong parent=BinaryPrimitives.ReadUInt64LittleEndian(branch.AsSpan(8));
var childQuery=new LegacyPreparedDictionaryQuery{Node=branchNode,Capacity=128,ContextRoot=real.ContainerIdentity,ParentContext=parent};
var childPage=source.Read(childQuery,(false),(p,s)=>real.ResolveChild(p,s),real.LinkCreatedContext);
Require(childPage.ReturnedCount>0,"Actual continuation context query returned nothing.");
for(int i=0;i<childPage.ReturnedCount;i++)
{
 var r=childPage.Records.Span.Slice(i*32,32);ulong id=BinaryPrimitives.ReadUInt64LittleEndian(r[8..]);ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(r[20..]);
 Require(real.ReadNode(id).ParentLink==parent&&real.ReadNode(id).Symbol==symbol&&!real.ResolveChild(parent,symbol).Created,"Actual child context identity/link reuse differs.");
}
CheckTree(real,real.RootTreeIdentity);CheckTree(real,real.ReadNode(parent).ChildTreeRoot);
Console.WriteLine($"ACTUAL RU exact source68 map contexts: lexical={contexts.Length},rootKeys={contexts.Where(x=>x!=0).Distinct().Count()},mappedSlots={contexts.Count(x=>x!=0)},pages={real.PageCount};queryRootRecords={records.Count}/{pages}pages,childNode={branchNode},childRecords={childPage.ReturnedCount},totalContexts={real.NodeCount}. Direct prepared dictionary child path; source eligibility/path rewrites/native word startup are not claimed.");
Console.WriteLine("PASS actual092990 acoustic/control-key initial context map -> restored packed query contexts ->0918d0 per-parent child trees/parent links with stable portable IDs. No native pointer values or recognition confidence invented.");
static int CheckTree(LegacySymbolContextFactory f,ulong root)
{
 var visited=new HashSet<ulong>();
 int Walk(ulong id,int lower,int upper)
 {if(id==0)return 0;Require(visited.Add(id),"AVL cycle.");var n=f.ReadNode(id);Require(n.Symbol>lower&&n.Symbol<upper,"AVL unsigned key order differs.");int l=Walk(n.Left,lower,n.Symbol),r=Walk(n.Right,n.Symbol,upper);Require(Math.Abs(r-l)<=1&&n.Balance==r-l,"AVL property differs.");return 1+Math.Max(l,r);}
 return Walk(root,-1,65536);
}
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Malformed/canceled context request accepted.");}
