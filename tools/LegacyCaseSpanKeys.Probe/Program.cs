using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var c=File.OpenRead(args[4]);var caseData=LegacyNativeCaseData.Load(c);using var p=File.OpenRead(args[5]);var publication=LegacyNativeTextPublicationData.Load(p);
var sampled=MainFixture.Create(ak,engine,table);var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var graph=owner.SourceGraph;
LegacyPreparedDictionaryQuery Q()=>new(){Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity};
var raw=dictionary.Symbols.Symbols.CreateQuerySource(model.Lexicon,dictionary.Descriptor.Flags).Read(Q(),false);
using var output=File.Create(args[6]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("raw",Convert.ToHexString(raw.Records.Span));w.WriteStartArray("spans");
int count=0,records=0;var events=new List<int>{-1};var query=Q();owner.AdvanceNodeEpoch(-1);
var auxiliaryQuery=Q();var auxiliaryKeys=new List<(int Node,uint Span,int Owned,int Public)>();
for(int node=0;node<graph.NodeCount;node++)
{
 for(byte span=0;span<owner.GetSpanExtent(node);span++)
 {
  var actual=owner.Prepare(node,span);var pass=actual.PermissionPass;
  //Source-level cache exercise. A noninitial root page is not a claim that
  //2144F0 would invoke the startup consumer at this noninitial span.
  LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query,new(0,0,false,false,120),(uint)node,span,0,false);
  var page=owner.Read(actual,8UL<<40,query);
  for(int i=0;i<page.ReturnedCount;i++)
  {
   var row=page.Records.Span.Slice(i*32,32);ulong context=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
   if(dictionary.Contexts.ReadNode(context).Symbol!=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]))throw new Exception("Persistent query context is not live/same-owner.");
  }
  w.WriteStartObject();w.WriteNumber("node",node);w.WriteNumber("span",span);w.WriteNumber("preparedEpoch",node-1);
  w.WriteNumber("parameter1c",query.Parameter1c);w.WriteNumber("parameter20",query.Parameter20);
  w.WriteBase64String("permissions",pass.PermissionResult.Permissions.Span);w.WriteBase64String("eligibility",actual.Qualifier.GetEligibilitySnapshot().Span);
  w.WriteString("records",Convert.ToHexString(page.Records.Span));w.WriteStartArray("map");foreach(ushort value in pass.PermissionResult.WorkingMap!.CopySelectedMap(false,0))w.WriteNumberValue(value);w.WriteEndArray();
  w.WriteStartArray("costs");foreach(var row in pass.SelectedRows){w.WriteStartArray();foreach(int value in row.Costs.Span)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
  count++;records+=page.ReturnedCount;
  //204290 language queries use the parent-row signed span, not the latest
  //D0 key. Keep one auxiliary request across all source68 map refreshes.
  auxiliaryQuery.Parameter1c=(uint)node;auxiliaryQuery.Parameter20=uint.MaxValue;
  var aux=owner.Read(actual,8UL<<40,auxiliaryQuery,true);
  var publicAux=dictionary.Session.Read(8UL<<40,auxiliaryQuery,true);
  if(aux.ReturnedCount!=0||!aux.Records.IsEmpty||publicAux.ReturnedCount!=0||!publicAux.Records.IsEmpty)
      throw new Exception("Actual immutable60 must not admit normal letters.");
  //Exercise the same shared backend slot in normal/auxiliary/normal modes.
  query.Parameter20=uint.MaxValue;
  if(owner.Read(actual,8UL<<40,query,true).ReturnedCount!=0)throw new Exception("Auxiliary mode leaked normal map.");
  LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query,new(0,0,false,false,120),(uint)node,span,0,false);
  var resumed=owner.Read(actual,8UL<<40,query);
  if(resumed.ReturnedCount!=page.ReturnedCount||!resumed.Records.Span.SequenceEqual(page.Records.Span))
      throw new Exception("Auxiliary interleave changed normal case wrapper results.");
  auxiliaryKeys.Add((node,uint.MaxValue,aux.ReturnedCount,publicAux.ReturnedCount));
 }
 owner.AdvanceNodeEpoch(node);events.Add(node);
}
w.WriteEndArray();w.WriteStartArray("epochs");foreach(int value in events)w.WriteNumberValue(value);w.WriteEndArray();
w.WriteStartArray("contexts");foreach(var node in owner.Contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(node.Identity);w.WriteNumberValue(node.ParentLink);w.WriteNumberValue(node.Symbol);w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("auxiliary");foreach(var key in auxiliaryKeys){w.WriteStartArray();w.WriteNumberValue(key.Node);w.WriteNumberValue(key.Span);w.WriteNumberValue(key.Owned);w.WriteNumberValue(key.Public);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
if(count!=10||records!=397||!events.SequenceEqual(Enumerable.Range(-1,graph.NodeCount+1)))throw new Exception("Actual source-level key regression changed.");
int guards=0;
void Reject(Action action){try{action();throw new Exception("Unsupported query progression accepted.");}catch(InvalidOperationException){guards++;}}
LegacyOwnedMainWordSpanPreparation NewOwner()=>new(ak,dictionary,sampled,table);
void Set(LegacyPreparedDictionaryQuery q,uint node,uint span)=>LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(q,new(0,0,false,false,120),node,span,0,false);
//Repeated span, descending span, stale fields, public-query and public-refresh
//contamination must not silently enable a cached cross-map request.
{
 var o=NewOwner();o.AdvanceNodeEpoch(-1);var a=o.Prepare(0,0);var q=Q();Set(q,0,0);o.Read(a,8UL<<40,q);var b=o.Prepare(0,0);Reject(()=>o.Read(b,8UL<<40,q));
}
{
 var o=NewOwner();o.AdvanceNodeEpoch(1);var a=o.Prepare(2,1);var q=Q();Set(q,2,1);o.Read(a,8UL<<40,q);var b=o.Prepare(2,0);Set(q,2,0);Reject(()=>o.Read(b,8UL<<40,q));
}
{
 var o=NewOwner();o.AdvanceNodeEpoch(1);var a=o.Prepare(2,0);var q=Q();Set(q,2,0);o.Read(a,8UL<<40,q);var b=o.Prepare(2,1);Reject(()=>o.Read(b,8UL<<40,q));
}
{
 var o=NewOwner();o.AdvanceNodeEpoch(1);var a=o.Prepare(2,0);var q=Q();Set(q,2,0);dictionary.Session.Read(8UL<<40,q);var b=o.Prepare(2,1);Set(q,2,1);Reject(()=>o.Read(b,8UL<<40,q));
}
{
 var o=NewOwner();o.AdvanceNodeEpoch(1);var a=o.Prepare(2,0);var q=Q();Set(q,2,0);o.Read(a,8UL<<40,q);dictionary.Session.RefreshPermissions(a.PermissionPass.PermissionResult.Permissions.Span);var b=o.Prepare(2,1);Set(q,2,1);Reject(()=>o.Read(b,8UL<<40,q));
}
{
 var o=NewOwner();o.AdvanceNodeEpoch(-1);var a=o.Prepare(2,0);var q=Q();Set(q,2,0);o.Read(a,8UL<<40,q);var b=o.Prepare(2,1);Set(q,2,1);Reject(()=>o.Read(b,8UL<<40,q));
}
var survivor=NewOwner();survivor.AdvanceNodeEpoch(-1);var survived=survivor.Prepare(0,0);var sq=Q();Set(sq,0,0);if(survivor.Read(survived,8UL<<40,sq).ReturnedCount<=0)throw new Exception("Rejected owning query poisoned shared session.");
{
 var o=NewOwner();o.AdvanceNodeEpoch(-1);var stale=o.Prepare(0,0);dictionary.Session.RefreshPermissions(stale.PermissionPass.PermissionResult.Permissions.Span);
 Reject(()=>o.Read(stale,8UL<<40,Q(),true));
}
Console.WriteLine($"PASS actual{count}span maps/{records}records, ONE292 backend/ONE persistent request, native epochs[{string.Join(',',events)}], no per-span resets; {guards}repeat/descending/wrongkey/external/native-epoch guards. Source-level queries, not whole240 startup/winner.");
Console.WriteLine("PASS10 immutable60 auxiliary cross-map requests with signed span-1, public auxiliary interleave and normal/auxiliary/normal shared-slot recovery; stale auxiliary lease refused.");
