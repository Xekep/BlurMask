using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("models output-directory");
string m=args[0],output=args[1];Directory.CreateDirectory(output);
byte[] ak=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res")),lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var source=LegacySource247WireLayout.Read(ak);var firstModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(ak,source,source.Children.First(c=>c.Kind==0x249));
var data=LegacyEnglishLkPackedData.Read(source,lk);var language=LegacyEnglishLkSparseLanguageData.Read(data,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var line30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var line50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var separation=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cfile=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pfile=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cfile);var publication=LegacyNativeTextPublicationData.Load(pfile);
//An original diagnostic drawing creates the real completed MAIN owner. No
//expected handwriting text, terminal or class is inferred from this drawing.
InkStroke[] input=[new(new InkPoint[]{new(0,0),new(7,40),new(14,0)})];
var parent=LegacyEnglish119ParentSubmission.Prepare(firstModel,input,96,line30,line50);
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var candidates=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,separation);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,separation);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,separation).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
parent.Publish(parent.Children.Select(c=>Recognize(c.Submission)).ToArray());
var post=LegacyEnglish119ParentPostprocessing.Prepare(parent,caseData,publication).Run();var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,separation).Run();
var masks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);
var scorer=LegacyEnglishMainOnlyAcousticScorer.Prepare(masks);var languageQuery=LegacyEnglishSparseTerminalQuery.Create(masks);
var bounds=new LegacyEnglishQuerySessionBounds(2048,512,8192,4);int guards=0,canceledPrepares=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected strict preparation refusal.");}
var sessions=new Dictionary<string,LegacyEnglishFreshDictionaryQuerySession>();var ids=new Dictionary<string,Dictionary<ulong,int>>();var cursors=new Dictionary<string,LegacyEnglishPreparedQueryCursor>();var saved=new Dictionary<string,LegacyEnglishQuerySessionCheckpoint>();var events=new List<object>();
int Id(string session,ulong id){if(id==0)return 0;var map=ids[session];if(!map.TryGetValue(id,out int value))map[id]=value=map.Count+1;return value;}
Dictionary<string,object?> State(string name)
{
 var s=sessions[name];var p=s.CopyPreparationSnapshot();var nodes=p.CopyNodes();foreach(var node in nodes)Id(name,node.Identity);var owners=p.CopyTreeOwners();
 return D(("Faulted",p.IsFaulted),("Attempts",p.AttemptedInsertions),("Operations",p.Operations),("Outputs",p.OutputRows),("Checkpoints",p.Checkpoints),("Epoch",p.Epoch),("Root",Id(name,p.Root)),("PageCapacity",p.PageDescriptorCapacity),("CurrentPage",p.CurrentPage),("Pages",p.CopyPageUsed()),("Primary",p.CopyPrimaryMap()),("Auxiliary",p.CopyAuxiliaryMap()),("Working",p.CopyWorkingMap()),("Initial",p.CopyInitialSource68().Select(x=>Id(name,x)).ToArray()),("Nodes",nodes.Select((n,i)=>new long[]{Id(name,n.Identity),Id(name,n.ParentLink),Id(name,n.ChildTreeRoot),Id(name,n.Left),Id(name,n.Right),n.Symbol,n.Balance,n.PageIndex,n.PageSlot,Id(name,owners[i])}).ToArray()));
}
void Fresh(string name,LegacyEnglishQuerySessionBounds? selected=null)
{var b=selected??bounds;sessions.Add(name,LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,languageQuery,b));ids.Add(name,[]);events.Add(D(("Session",name),("Kind","fresh"),("Args",new long[]{b.MaximumAttemptedInsertions,b.MaximumOperations,b.MaximumOutputRows,b.MaximumCheckpoints,b.MaximumAdditionalBytes}),("State",State(name))));}
void Run(string name,string kind,object? arguments,Func<Action<string>?,CancellationToken,object?> body,string? failStage=null,bool expectedFailure=false)
{
 object? result=null;string? failure=null;int seen=0;using var cts=new CancellationTokenSource();Action<string>? observer=failStage is null?null:s=>{if(s==failStage){seen++;cts.Cancel();}};
 try{result=body(observer,cts.Token);}catch(Exception e)when(expectedFailure&&e is ArgumentException or InvalidOperationException or OperationCanceledException){failure=e.GetType().Name;}
 if(expectedFailure!=(failure is not null)||failStage is not null&&seen!=1)throw new Exception("Prepared failure boundary not reached exactly.");
 events.Add(D(("Session",name),("Kind",kind),("Args",arguments),("FailureStage",failStage),("Failure",failure),("Result",result),("State",State(name))));
}
void Cursor(string name,string cursor,int node,int cap=128,ulong parentContext=0,bool root=true,bool auxiliary=false,ushort flags=0)
{Run(name,"cursor",D(("Name",cursor),("Node",node),("Capacity",cap),("Parent",Id(name,parentContext)),("RootPresent",root),("Auxiliary",auxiliary),("Flags",flags)),(_,token)=>{cursors[name+"/"+cursor]=sessions[name].CreatePreparedCursor(node,cap,parentContext,root,auxiliary,flags,token:token);return null;});}
object Page(string name,LegacyPreparedDictionaryQueryPage page)
{byte[] b=page.Records.ToArray();for(int at=0;at<b.Length;at+=32){ulong id=BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(at+8));BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at+8),(ulong)Id(name,id));}return D(("Returned",page.ReturnedCount),("Slot",page.SlotIndex),("NextChild",page.NextChild),("Accepted",page.AcceptedCount),("Rows",Convert.ToHexString(b)),("Extra",Convert.ToHexString(page.ClearedExtraCosts.Span)));}
LegacyPreparedDictionaryQueryPage? Read(string name,string cursor,int skip=0,string? fail=null,bool expectedFailure=false)
{LegacyPreparedDictionaryQueryPage? result=null;Run(name,"read",D(("Name",cursor),("Skip",skip)),(observer,token)=>{result=sessions[name].ReadPreparedPage(cursors[name+"/"+cursor],skip,observer,token);return Page(name,result);},fail,expectedFailure);return result;}
ulong Root(string name,ushort symbol,string? fail=null,bool expectedFailure=false)
{ulong result=0;Run(name,"root",symbol,(observer,token)=>{result=sessions[name].ResolvePreparedRoot(symbol,observer,token);return Id(name,result);},fail,expectedFailure);return result;}
ulong Child(string name,ulong owner,ushort symbol,string? fail=null,bool expectedFailure=false)
{ulong result=0;Run(name,"child",D(("Parent",Id(name,owner)),("Symbol",symbol)),(observer,token)=>{result=sessions[name].ResolvePreparedChild(owner,symbol,observer,token);return Id(name,result);},fail,expectedFailure);return result;}
void Checkpoint(string name,string key,string? fail=null,bool expectedFailure=false)
{Run(name,"capture",key,(observer,token)=>{saved[name+"/"+key]=sessions[name].CapturePreparedCheckpoint(observer,token);return null;},fail,expectedFailure);}
void Restore(string name,string? key=null,string? fail=null,bool expectedFailure=false)
{Run(name,"restore",key,(observer,token)=>{if(key is null)sessions[name].RestorePreparedBaseline(observer,token);else sessions[name].RestorePreparedCheckpoint(saved[name+"/"+key],observer,token);return null;},fail,expectedFailure);}
void Refresh(string name,byte[] permission,string? fail=null,bool expectedFailure=false)
{Run(name,"refresh",Convert.ToHexString(permission),(observer,token)=>{sessions[name].RefreshPreparedPermissions(permission,observer,token);return null;},fail,expectedFailure);}
long budget=LegacyEnglishFreshDictionaryQuerySession.Preflight(bounds);
Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(null!,languageQuery,bounds));Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,null!,bounds));Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,languageQuery,bounds,new(true)));
Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,languageQuery,bounds with{MaximumAdditionalBytes=budget-1}));if(LegacyEnglishFreshDictionaryQuerySession.Preflight(bounds with{MaximumAdditionalBytes=budget})!=budget)throw new Exception("Whole budget equality failed.");
foreach(var b in new[]{bounds with{MaximumAttemptedInsertions=-1},bounds with{MaximumAttemptedInsertions=65537},bounds with{MaximumOperations=0},bounds with{MaximumOperations=4097},bounds with{MaximumOutputRows=-1},bounds with{MaximumOutputRows=65537},bounds with{MaximumCheckpoints=0},bounds with{MaximumCheckpoints=9},bounds with{MaximumAttemptedInsertions=65536,MaximumAdditionalBytes=long.MaxValue}})Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Preflight(b));
var foreignMasks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);var foreignQuery=LegacyEnglishSparseTerminalQuery.Create(foreignMasks);Guard(()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,foreignQuery,bounds));
foreach(string stage in new[]{"session:preflight","session:maps","session:source","session:checkpoint","session:publish"})
{using var cts=new CancellationTokenSource();int seen=0;Guard(()=>LegacyEnglishFreshDictionaryQuerySession.PreparePrepared(scorer,languageQuery,bounds,s=>{if(s==stage){seen++;cts.Cancel();}},cts.Token));if(seen!=1)throw new Exception("Canceled preparation missing stage.");canceledPrepares++;}
Fresh("main");var main=sessions["main"];
Cursor("main","pages",0,7);int skip=0;while(true){var page=Read("main","pages",skip)!;skip+=page.ReturnedCount;if(page.ReturnedCount<7)break;}if(skip!=78)throw new Exception("Actual mapped root denominator changed.");
Cursor("main","root",0);var all=Read("main","root")!;var rootRecords=all.Records.ToArray();var mapped=new Dictionary<ushort,ulong>();ulong sentinel=Root("main",0xfffa);
for(int at=0;at<rootRecords.Length;at+=32){ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(rootRecords.AsSpan(at+20));mapped[symbol]=Child("main",sentinel,symbol);}
if(main.CopyPreparationSnapshot().CopyNodes().Length!=165||masks.ContextCount!=86)throw new Exception("Prepared >104 growth or untouched M failed.");Checkpoint("main","grafted");
int branchNode=-1;ushort branchSymbol=0;
for(int at=0;at<rootRecords.Length;at+=32){int node=BinaryPrimitives.ReadInt32LittleEndian(rootRecords.AsSpan(at));if(data.Dictionary.GetChildCount(node)>0){branchNode=node;branchSymbol=BinaryPrimitives.ReadUInt16LittleEndian(rootRecords.AsSpan(at+20));break;}}
if(branchNode<0)throw new Exception("Actual child query not available.");Cursor("main","child",branchNode,parentContext:mapped[branchSymbol]);var childRows=Read("main","child")!;var repeated=Read("main","child")!;if(!childRows.Records.Span.SequenceEqual(repeated.Records.Span))throw new Exception("Duplicate child contexts changed.");
Cursor("main","null-root",branchNode,parentContext:mapped[branchSymbol],root:false);Read("main","null-root");
ulong firstParent=mapped.Values.First(),secondParent=mapped.Values.Skip(1).First();ulong a=Child("main",firstParent,60000),bchild=Child("main",secondParent,60000);if(a==bchild)throw new Exception("Equal key under distinct parents aliased.");Checkpoint("main","children");
ulong stale=Child("main",firstParent,60001);Restore("main","children");Child("main",stale,3,expectedFailure:true);ulong replacement=Child("main",firstParent,60001);if(replacement==stale)throw new Exception("Restored physical slot reused stale opaque token.");Restore("main");
byte[] permission=Enumerable.Range(0,217).Select(i=>(byte)(i%3==0?0:1)).ToArray();Refresh("main",permission);Cursor("main","filtered",0);Read("main","filtered");Cursor("main","preserved",0,flags:0x40);Read("main","preserved");Cursor("main","aux",0,auxiliary:true);Read("main","aux");
Run("main","reset",null,(observer,token)=>{main.ResetPreparedQueries(observer,token);return null;});Cursor("main","after-reset",0);Read("main","after-reset");Restore("main");Read("main","after-reset");
for(int i=0;i<10;i++){Cursor("main","ring"+i,i%2==0?0:branchNode,3);Read("main","ring"+i);}Read("main","root");
var stablePage=Read("main","preserved")!.Records.ToArray();
Parallel.For(0,32,_=>{var page=main.ReadPreparedPage(cursors["main/preserved"]);if(!stablePage.AsSpan().SequenceEqual(page.Records.Span))throw new Exception("Serialized duplicate query changed rows.");});
events.Add(D(("Session","main"),("Kind","parallel-read"),("Args",D(("Name","preserved"),("Count",32))),("Failure",null),("Result",null),("State",State("main"))));
//Defensive snapshots are not pool authority.
var before=Encode(State("main"));var exposed=main.CopyPreparationSnapshot();exposed.CopyPrimaryMap()[0]^=1;exposed.CopyInitialSource68()[0]=0;exposed.CopyPageUsed()[0]=0;if(before!=Encode(State("main")))throw new Exception("Snapshot aliases private source state.");
Fresh("no-insert",bounds with{MaximumAttemptedInsertions=0});Root("no-insert",0xfffa,expectedFailure:true);Fresh("no-output",bounds with{MaximumOutputRows=0});Cursor("no-output","q",0);Read("no-output","q",expectedFailure:true);Fresh("no-operations",bounds with{MaximumOperations=1});Cursor("no-operations","q",0);Read("no-operations","q",expectedFailure:true);Fresh("no-checkpoint",bounds with{MaximumCheckpoints=1});Checkpoint("no-checkpoint","bad",expectedFailure:true);
Fresh("foreign");Guard(()=>main.RestorePreparedCheckpoint(sessions["foreign"].CapturePreparedCheckpoint()));Guard(()=>main.ReadPreparedPage(sessions["foreign"].CreatePreparedCursor(0)));Guard(()=>main.ResolvePreparedChild(sessions["foreign"].CopyPreparationSnapshot().CopyInitialSource68().First(x=>x!=0),1));
Guard(()=>main.CreatePreparedCursor(-1));Guard(()=>main.CreatePreparedCursor(666547));Guard(()=>main.CreatePreparedCursor(0,0));Guard(()=>main.CreatePreparedCursor(0,129));Guard(()=>main.RefreshPreparedPermissions(new byte[216]));Guard(()=>main.ReadPreparedPage(cursors["main/root"],-1));Guard(()=>main.CopyPreparationSnapshot(new(true)));Guard(()=>main.ResolvePreparedRoot(7,token:new(true)));
foreach(string stage in new[]{"prepared:enter","prepared:resolved"}){string name="root-fail-"+stage;Fresh(name);Root(name,0xfffa,stage,true);Guard(()=>sessions[name].ResolvePreparedRoot(1));}
foreach(string stage in new[]{"prepared:resolved","prepared:linked"}){string name="child-fail-"+stage;Fresh(name);ulong p=sessions[name].CopyPreparationSnapshot().CopyInitialSource68().First(x=>x!=0);Child(name,p,60000,stage,true);}
foreach(string stage in new[]{"prepared:resolved","prepared:linked","prepared:page","prepared:complete"}){string name="page-fail-"+stage;Fresh(name);ulong p=sessions[name].CopyPreparationSnapshot().CopyInitialSource68().First(x=>x!=0);Cursor(name,"q",branchNode,parentContext:p);Read(name,"q",fail:stage,expectedFailure:true);}
Fresh("capture-fail");Checkpoint("capture-fail","bad","prepared:captured",true);Fresh("refresh-fail");Refresh("refresh-fail",permission,"prepared:refreshed",true);Fresh("restore-fail");Root("restore-fail",0xfffa);Restore("restore-fail",fail:"prepared:restored",expectedFailure:true);
Fresh("skip-fail");Cursor("skip-fail","q",0);Read("skip-fail","q",2_000_000,expectedFailure:true);
Fresh("reentrant");Run("reentrant","root",(ushort)0xfffa,(_,token)=>Id("reentrant",sessions["reentrant"].ResolvePreparedRoot(0xfffa,s=>{if(s=="prepared:enter")Guard(()=>sessions["reentrant"].ResolvePreparedRoot(123));},token)));
if(main.IsFaulted||masks.ContextCount!=86||!masks.CopyPrimaryMap().SequenceEqual(main.CopyPreparationSnapshot().CopyPrimaryMap())||!ReferenceEquals(main.Scorer,scorer)||!ReferenceEquals(main.Source,data.Source)||main.Scope.Contains("ready")&&!main.Scope.Contains("not-controller-ready"))throw new Exception("Fresh prep mutated M or invented readiness.");
events.Add(D(("Session","main"),("Kind","guard-state"),("Args",null),("Failure",null),("Result",null),("State",State("main"))));
File.WriteAllText(Path.Combine(output,"capture.json"),Encode(D(("Scope",main.Scope),("AdditionalLogicalBytes",budget),("Events",events),("Guards",guards),("CanceledPreparations",canceledPrepares),("MContextCountAfter",masks.ContextCount),("ControllerRequestsAvailable",false),("TargetsRead",false),("VendorExecuted",false))));Console.WriteLine($"PASS fresh query preparation {events.Count}state events/{guards}guards/{canceledPrepares}prep cancels; no NN/controller query API");
static Dictionary<string,object?> D(params(string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
static string Encode(object value){using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,value);return System.Text.Encoding.UTF8.GetString(stream.ToArray());}
static void Write(Utf8JsonWriter w,object? v)
{
 switch(v){case null:w.WriteNullValue();break;case string s:w.WriteStringValue(s);break;case bool b:w.WriteBooleanValue(b);break;case int n:w.WriteNumberValue(n);break;case uint n:w.WriteNumberValue(n);break;case long n:w.WriteNumberValue(n);break;case ulong n:w.WriteNumberValue(n);break;case ushort n:w.WriteNumberValue(n);break;case byte n:w.WriteNumberValue(n);break;case Dictionary<string,object?> d:w.WriteStartObject();foreach(var pair in d){w.WritePropertyName(pair.Key);Write(w,pair.Value);}w.WriteEndObject();break;case System.Collections.IEnumerable xs:w.WriteStartArray();foreach(var x in xs)Write(w,x);w.WriteEndArray();break;default:throw new Exception("Explicit portable JSON value required.");}
}
