using System.Text.Json;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-raw capture");
var m=args[0];var bytes=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var packed=LegacyEnglishLkPackedData.Read(layout,lk);var language=LegacyEnglishLkSparseLanguageData.Read(packed,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var m30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
LegacyEnglish119ParentPostprocessing Post(JsonElement item,LegacyFirstAcoustic119WireModel? selected=null)
{var p=LegacyEnglish119ParentSubmission.Prepare(selected??model,Ink(item.GetProperty("Raw")),item.GetProperty("Resolution").GetSingle(),m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication).Run();}

using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0,cancellations=0,totalPasses=0,totalSelected=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Expected strict D0 refusal.");}
bool tested=false;
var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var originalItem in items)
{
 JsonElement item=originalItem;string name;
 if(inputs.RootElement.ValueKind==JsonValueKind.Array)name=item.GetProperty("Name").GetString()!;
 else{name=originalItem.GetProperty("id").GetString()!;using var transformed=JsonDocument.Parse(Encode(D(("Name",name),("Raw",originalItem.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=transformed.RootElement.Clone();}
 var post=Post(item);if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();
 var masks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);var scorer=LegacyEnglishMainOnlyAcousticScorer.Prepare(masks);var query=LegacyEnglishSparseTerminalQuery.Create(masks);
 LegacyEnglishFreshDictionaryQuerySession Binding()=>LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,query,new(0,4096,0,1));
 var bounds=new LegacyEnglishMainD0Bounds(4096,16384);var coordinates=new List<(int Node,byte Span)>();var graph=new List<object>();
 for(int node=0;node<local.NodeCount;node++)
 {var arcs=local.CopyNode(node);graph.Add(arcs.Select(a=>new long[]{a.GroupId,a.Span,a.Flags,a.StartContextId,a.EndContextId}).ToArray());foreach(byte span in arcs.Select(a=>a.Span).Distinct().Order())coordinates.Add((node,span));}
 if(coordinates.Count==0)throw new Exception("Actual nonempty local graph required.");
 var first=coordinates[0];
 if(!tested)
 {
  foreach(int bad in new[]{0,4097,int.MaxValue}){Guard(()=>LegacyEnglishMainSpanSession.Preflight(bad,1,1,bounds));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,bad,1,bounds));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,bad,bounds));}
  Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,1,new(0,0)));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,1,new(4097,0)));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,1,new(1,-1)));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,1,new(1,16385)));Guard(()=>LegacyEnglishMainSpanSession.Preflight(1,1,1,new(1,0,0)));
  if(LegacyEnglishMainSpanSession.Preflight(4096,4096,4096,bounds)>64L*1024*1024)throw new Exception("D0 arithmetic bound.");
  foreach(string stage in new[]{"owner:claimed","owner:ready"})
  {
   var b=Binding();using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishMainSpanSession.PrepareCore(b,bounds,s=>{if(s==stage)cts.Cancel();},cts.Token));cancellations++;
   var snapshot=b.CopyPreparationSnapshot();if(!b.IsFaulted||snapshot.Operations!=0)throw new Exception("Claim failure prefix/counters.");Guard(()=>b.RestorePreparedBaseline());
  }
  var nonpristine=Binding();_=nonpristine.CreatePreparedCursor(0);Guard(()=>LegacyEnglishMainSpanSession.Prepare(nonpristine,bounds));if(nonpristine.IsFaulted)throw new Exception("Preflight refusal poisoned pristine transport.");
  var low=Binding();var limited=LegacyEnglishMainSpanSession.Prepare(low,new(1,0));Guard(()=>limited.PrepareSpan(first.Node,first.Span));if(limited.EnteredPreparations!=0||low.CopyPreparationSnapshot().Operations!=0||limited.IsFaulted)throw new Exception("Admission refusal consumed operation.");
  foreach(string stage in new[]{"span:entered","span:prepared","span:snapshot","span:installed"})
  {
   var b=Binding();var s=LegacyEnglishMainSpanSession.Prepare(b,bounds);var previous=s.PrepareSpan(first.Node,first.Span);var bytesBefore=previous.CopyPermissions();
   using var cts=new CancellationTokenSource();Guard(()=>s.PrepareSpanCore(first.Node,first.Span,false,t=>{if(t==stage)cts.Cancel();},cts.Token));cancellations++;
   if(!s.IsFaulted||!b.IsFaulted||s.EnteredPreparations!=2||b.CopyPreparationSnapshot().Operations!=2||previous.IsCurrent||!bytesBefore.SequenceEqual(previous.CopyPermissions()))throw new Exception("Entered cancellation must poison both owners without erasing historical snapshot.");
   Guard(()=>s.PrepareSpan(first.Node,first.Span));Guard(()=>b.RestorePreparedBaseline());
  }
  var b1=Binding();var s1=LegacyEnglishMainSpanSession.Prepare(b1,bounds);var b2=Binding();var s2=LegacyEnglishMainSpanSession.Prepare(b2,bounds);
  var r=s1.PrepareSpan(first.Node,first.Span);Guard(()=>r.RequireCurrentBinding(b2,default));Guard(()=>LegacyEnglishMainSpanSession.Prepare(b1,bounds));Guard(()=>b1.CreatePreparedCursor(0));Guard(()=>b1.RestorePreparedBaseline());
  Guard(()=>s1.PrepareSpanCore(first.Node,first.Span,true,null,default));Guard(()=>s1.PrepareSpan(-1,0));Guard(()=>s1.PrepareSpan(local.NodeCount,0));Guard(()=>s1.PrepareSpan(0,8));
  if(b1.CopyPreparationSnapshot().Operations!=1||s1.EnteredPreparations!=1||!r.IsCurrent)throw new Exception("Preentry guards changed valid receipt.");
  var selected=r.CopySelectedRows();if(selected.Length!=0){var costs=selected[0].CopyCosts();int beforeCost=costs[0];costs[0]^=1;if(selected[0].CopyCosts()[0]!=beforeCost)throw new Exception("Cost copy aliases.");selected[0]=null!;if(r.CopySelectedRows()[0] is null)throw new Exception("Selection copy aliases.");}
  var permission=r.CopyPermissions();permission[0]^=1;if(permission.SequenceEqual(r.CopyPermissions()))throw new Exception("Permission copy aliases.");var working=r.CopyWorkingMap();working[0]^=1;if(working.SequenceEqual(r.CopyWorkingMap()))throw new Exception("Working copy aliases.");var eligibility=r.CopyEligibility();eligibility[0]^=1;if(eligibility.SequenceEqual(r.CopyEligibility()))throw new Exception("Eligibility copy aliases.");
  var r2=s1.PrepareSpan(first.Node,first.Span);if(r.IsCurrent||!r2.IsCurrent)throw new Exception("Old receipt retains query authority.");Guard(()=>r.RequireCurrentBinding(b1,default));
  _=s1.PrepareSpanCore(first.Node,first.Span,false,stage=>{if(stage=="span:entered"){Guard(()=>s1.PrepareSpan(first.Node,first.Span));_=b1.CopyPreparationSnapshot();}},default);
  tested=true;
 }
 var binding=Binding();var before=binding.CopyPreparationSnapshot();var owner=LegacyEnglishMainSpanSession.Prepare(binding,bounds);var passes=new List<object>();var provider=new List<object>();var seen=new HashSet<int>();
 for(int node=0;node<local.NodeCount;node++)
 {var arcs=local.CopyNode(node);for(int ordinal=0;ordinal<arcs.Length;ordinal++)if(seen.Add(arcs[ordinal].GroupId)){var score=scorer.ScoreArc(node,ordinal);provider.Add(D(("Group",score.GroupId),("Span",score.Span),("Costs",score.CopyCosts()),("Scale",score.PowerScale)));}}
 var sequence=coordinates.Concat(coordinates.AsEnumerable().Reverse()).Concat(new[]{(0,(byte)7),first}).ToArray();
 foreach(var (node,span) in sequence)
 {
  var receipt=owner.PrepareSpan(node,span);receipt.RequireCurrentBinding(binding,default);
  passes.Add(D(("Anchor",node),("Span",span),("Epoch",receipt.Epoch),("Computed",receipt.ComputedRows),("Reused",receipt.ReusedRows),("Selected",receipt.CopySelectedRows().Select(r=>D(("Ordinal",r.ArcIndex),("Group",r.GroupId),("Costs",r.CopyCosts()))).ToArray()),("Permissions",receipt.CopyPermissions()),("Eligibility",receipt.CopyEligibility()),("Working",receipt.CopyWorkingMap())));
  totalSelected+=receipt.SelectedRowCount;totalPasses++;
 }
 var after=binding.CopyPreparationSnapshot();if(after.Operations!=passes.Count||after.AttemptedInsertions!=0||after.OutputRows!=0||after.Epoch!=before.Epoch||!before.CopyInitialSource68().SequenceEqual(after.CopyInitialSource68())||!before.CopyPrimaryMap().SequenceEqual(after.CopyPrimaryMap())||!before.CopyAuxiliaryMap().SequenceEqual(after.CopyAuxiliaryMap()))throw new Exception("D0 install rewrote stable context/source68 state or counters.");
 rows.Add(D(("Name",name),("Graph",graph),("Provider",provider),("Passes",passes),("InitialMap",before.CopyPrimaryMap()),("InitialSource68",before.CopyInitialSource68()),("FinalSource68",after.CopyInitialSource68()),("Operations",after.Operations),("Budget",owner.LogicalAllocationEnvelopeBytes)));
 Console.WriteLine(name+" passes="+passes.Count);
}
if(inputs.RootElement.ValueKind==JsonValueKind.Array){if(rows.Count!=134||refused.Count!=7)throw new Exception("Original owner denominator.");}else if(rows.Count!=100||refused.Count!=0)throw new Exception("PluColl owner denominator.");
File.WriteAllText(args[2],Encode(D(("Scope","actual EN MAIN D0 rebuildfalse/source68 only; no request/wholeword/nativeSDK"),("InputSha256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1])))),("TargetsRead",false),("Rows",rows),("Refused",refused),("Guards",guards),("Cancellations",cancellations),("PassCount",totalPasses),("SelectedRowCount",totalSelected))));
Console.WriteLine("PASS D0 "+rows.Count+" owners "+totalPasses+" passes "+totalSelected+" selected rows guards="+guards);
static string Encode(object value){using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,value);return System.Text.Encoding.UTF8.GetString(stream.ToArray());}
static Dictionary<string,object?> D(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
static void Write(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
 case null:w.WriteNullValue();break;
 case string s:w.WriteStringValue(s);break;
 case bool b:w.WriteBooleanValue(b);break;
 case int n:w.WriteNumberValue(n);break;
 case uint n:w.WriteNumberValue(n);break;
 case long n:w.WriteNumberValue(n);break;
 case ulong n:w.WriteNumberValue(n);break;
 case byte n:w.WriteNumberValue(n);break;
 case short n:w.WriteNumberValue(n);break;
 case ushort n:w.WriteNumberValue(n);break;
 case float n:w.WriteNumberValue(n);break;
 case double n:w.WriteNumberValue(n);break;
 case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}
