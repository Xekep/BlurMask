using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("models blind-inputs capture");
var models=args[0];var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));int guards=0,cancels=0;
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal.");}
var ink=new[]{new InkStroke(Enumerable.Range(0,32).Select(i=>new InkPoint(10*i,(float)(40*Math.Sin(i*1.3)))).ToArray())};
LegacyFullEnglishFirstController B1(IReadOnlyList<InkStroke>? strokes=null){var p=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes??ink,0,m30,m50);var c=LegacyFullEnglishFirstController.Prepare(p);c.Run(table);return c;}
var good=B1();var b=LegacyFullEnglishFirstControllerCosts.Prepare(good,table);Guard(()=>b.Run(new CancellationToken(true)));if(b.IsFaulted||b.IsComplete)throw new Exception("Preentry cancellation consumed state.");b.Run();Guard(()=>b.Run());if(!b.IsComplete||b.IsFaulted)throw new Exception("B2 completion drift.");
Guard(()=>LegacyFullEnglishFirstControllerCosts.PrepareBounded(good,table,1));Guard(()=>LegacyFullEnglishFirstControllerBudget.Envelope(4097,1,1,1,1));Guard(()=>LegacyFullEnglishFirstControllerBudget.Envelope(1,4097,1,1,1));Guard(()=>LegacyFullEnglishFirstControllerBudget.Envelope(1,1,4097,1,1));Guard(()=>LegacyFullEnglishFirstControllerBudget.Envelope(1,1,1,1048577,1));Guard(()=>LegacyFullEnglishFirstControllerBudget.Envelope(1,1,1,1,8193));if(LegacyFullEnglishFirstControllerBudget.Envelope(4096,4096,4096,1048576,8192)!=31457280)throw new Exception("Envelope arithmetic drift.");
var first=b.CopyCompletedChildren()[0];int id=first.CopyGroupIds()[0];var group=first.ReadGroup(first.ActualCache.ScoreGroup(id));var entries=group.CopyEntries();if(entries.Length>0){var saved=entries[0];entries[0]=new(0,0,0);if(group.CopyEntries()[0]!=saved)throw new Exception("Mutable ranked snapshot.");}
var conf=group.CopyConfidence();if(conf.Length>0){float saved=conf[0];conf[0]=-1;if(group.CopyConfidence()[0]!=saved)throw new Exception("Mutable confidence.");}
var geometry=first.Geometry.CopyNode(0);if(geometry.Length>0){var margins=geometry[0].CopyMarginCosts();int saved=margins[0];margins[0]=-1;if(first.Geometry.CopyNode(0)[0].CopyMarginCosts()[0]!=saved)throw new Exception("Mutable geometry.");}
var foreign=B1();Guard(()=>first.ReadGroup(foreign.Parent.CopyCompletedChildren()[0].ScoreGroup(id)));
var mutable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));var detached=LegacyFullEnglishFirstControllerCosts.Prepare(good,mutable);System.Runtime.InteropServices.MemoryMarshal.TryGetArray(mutable.Values,out var array);Array.Fill(array.Array!,1f,array.Offset,array.Count);detached.Run();var dg=detached.CopyCompletedChildren()[0];for(int node=0;node<first.Geometry.NodeCount;node++){var a=first.Geometry.CopyNode(node);var z=dg.Geometry.CopyNode(node);for(int ai=0;ai<a.Length;ai++)if(!a[ai].CopyMarginCosts().SequenceEqual(z[ai].CopyMarginCosts()))throw new Exception("Caller table mutation reached owned initializer.");}
var partial=LegacyFullEnglishFirstParentInput.Prepare(knowledge,ink,0,m30,m50);var incomplete=LegacyFullEnglishFirstController.Prepare(partial);Guard(()=>LegacyFullEnglishFirstControllerCosts.Prepare(incomplete,table));Guard(()=>incomplete.Run(table,new CancellationToken(true)));Guard(()=>LegacyFullEnglishFirstControllerCosts.Prepare(incomplete,table));
var limitedParent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,ink,0,m30,m50,new LegacyFullEnglishFirstBounds(4096,4));var failedB1=LegacyFullEnglishFirstController.Prepare(limitedParent);Guard(()=>failedB1.Run(table));if(!failedB1.IsFaulted||!limitedParent.IsFaulted)throw new Exception("Expected actual B1 entry failure.");Guard(()=>LegacyFullEnglishFirstControllerCosts.Prepare(failedB1,table));
foreach(string stage in new[]{"entry","child:entry","group","geometry","child:published"})
{
 var c=LegacyFullEnglishFirstControllerCosts.Prepare(good,table);using var cts=new CancellationTokenSource();Guard(()=>c.RunPrepared(s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;if(!c.IsFaulted||c.IsComplete)throw new Exception("Entered B2 cancellation drift.");Guard(()=>c.Run());foreach(var child in c.CopyCompletedChildren())foreach(int g in child.CopyGroupIds())_=child.ReadGroup(child.ActualCache.ScoreGroup(g)).CopyEntries();
}
var two=ink.Concat(ink.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();var multiB1=B1(two);var multi=LegacyFullEnglishFirstControllerCosts.Prepare(multiB1,table);using(var cts=new CancellationTokenSource())
{
 int seen=0;Guard(()=>multi.RunPrepared(s=>{if(s=="child:entry"&&++seen==2)cts.Cancel();},cts.Token));cancels++;if(!multi.IsFaulted||multi.IsComplete||multi.CopyCompletedChildren().Count!=1||multi.CopyCompletedChildren()[0].CopyGroupIds().Length==0)throw new Exception("Expected nonempty historical prefix.");
}
var concurrent=LegacyFullEnglishFirstControllerCosts.Prepare(good,table);using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{
 var task=Task.Run(()=>concurrent.RunPrepared(s=>{if(s=="entry"){entered.Set();release.Wait();}}));if(!entered.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Barrier timeout.");Guard(()=>concurrent.Run());_=concurrent.CopyCompletedChildren();release.Set();task.GetAwaiter().GetResult();
}
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","Actual full-FIRST ranked initializer only; no primary/context/terminal/WORD");w.WriteBoolean("TargetsRead",false);w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);
void Ints(IEnumerable<int> values){w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Floats(IEnumerable<float> values){w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
w.WriteStartArray("PreparedQueries");for(ushort s=0;s<220;s++)foreach(ushort ordinal in new ushort[]{ushort.MaxValue,0,10})foreach(int geo in new[]{0,23})foreach(bool top in new[]{false,true})foreach(bool primitive in new[]{false,true})
{w.WriteStartArray();w.WriteNumberValue(s);w.WriteNumberValue(ordinal);w.WriteNumberValue(geo);w.WriteBooleanValue(top);w.WriteBooleanValue(primitive);w.WriteNumberValue(group.PreparedClassQuery(s,ordinal,geo,top,primitive));w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("PreparedEntries");foreach(var e in group.CopyEntries()){w.WriteStartArray();w.WriteNumberValue(e.Symbol);w.WriteNumberValue(e.Cost);w.WriteNumberValue(e.NormalizedLikelihood);w.WriteEndArray();}w.WriteEndArray();w.WriteNumber("PreparedGeometry",group.GeometryCost);w.WriteNumber("PreparedFallback",group.ActualCache.FallbackCost);
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();int accepted=0,refused=0,groups=0,arcs=0;w.WriteStartArray("Actual");
foreach(var item in items)
{
 var raw=item.TryGetProperty("Raw",out var r)?r:item.GetProperty("strokes");string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;float resolution=item.TryGetProperty("Resolution",out var rr)?rr.GetSingle():0;var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();w.WriteStartObject();w.WriteString("Name",name);
 try
 {
  var parent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);var confidence=LegacyFullEnglishFirstController.Prepare(parent);confidence.Run(table);int nn=parent.NetworkEvaluationCount;var c=LegacyFullEnglishFirstControllerCosts.Prepare(confidence,table);c.Run();if(!c.IsComplete||c.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("B2 completion or NN replay drift.");
  w.WriteString("Status","AcceptedInitializer");w.WriteNumber("NetworkCount",nn);w.WriteNumber("LogicalAdditional",c.AdditionalLogicalEnvelopeBytes);w.WriteNumber("LogicalCombined",c.CombinedLogicalReservationBytes);w.WriteStartArray("Children");
  foreach(var child in c.CopyCompletedChildren())
  {
   var ca=child.ActualCache.Owner;w.WriteStartObject();w.WriteNumber("Descriptor",ca.Child.DescriptorOrdinal);w.WriteNumber("Height",ca.Preparation.Line.Height);w.WriteStartArray("Raw");foreach(var stroke in ca.CopyCutStrokes()){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("Groups");foreach(int g in child.CopyGroupIds())
   {
    var actual=child.ActualCache.ScoreGroup(g);var row=child.ReadGroup(actual);w.WriteStartObject();w.WriteNumber("Group",g);w.WriteNumber("Geometry",row.GeometryCost);w.WriteNumber("Fallback",actual.FallbackCost);w.WritePropertyName("Costs");Ints(actual.CopyFinalCosts());w.WritePropertyName("Confidence");Floats(row.CopyConfidence());w.WriteStartArray("Stored");foreach(var range in ca.CopyGroupRanges(g)){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("Entries");foreach(var e in row.CopyEntries()){w.WriteStartArray();w.WriteNumberValue(e.Symbol);w.WriteNumberValue(e.Cost);w.WriteNumberValue(e.NormalizedLikelihood);w.WriteNumberValue(row.PreparedLikelihood(e.Symbol));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();groups++;
   }w.WriteEndArray();
   w.WriteStartArray("Nodes");for(int node=0;node<ca.NodeCount;node++)
   {
    w.WriteStartArray();var ns=ca.CopyNode(node);var gs=child.Geometry.CopyNode(node);for(int a=0;a<ns.Length;a++)
    {
     var aa=ns[a];var state=gs[a];w.WriteStartObject();w.WriteNumber("Group",aa.GroupId);w.WriteNumber("Span",aa.Span);w.WritePropertyName("Pred");Ints(aa.PredecessorArcIndices.ToArray());w.WriteStartArray("Successors");foreach(var edge in aa.Successors){w.WriteStartArray();w.WriteNumberValue(edge.NodeOffset);w.WriteNumberValue(edge.ArcIndex);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("Bounds");w.WriteNumberValue(state.Bounds.MinimumX);w.WriteNumberValue(state.Bounds.MinimumY);w.WriteNumberValue(state.Bounds.MaximumX);w.WriteNumberValue(state.Bounds.MaximumY);w.WriteEndArray();w.WriteNumber("Future",state.FutureMinimumX);w.WriteNumber("Prefix",state.PrefixMaximumX);w.WriteNumber("Join",state.JoinCost);w.WriteNumber("Blank",state.BlankCost);w.WritePropertyName("Matrix");Ints(state.CopyMarginCosts());w.WriteEndObject();arcs++;
    }w.WriteEndArray();
   }w.WriteEndArray();w.WriteEndObject();
  }w.WriteEndArray();accepted++;
 }
 catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){w.WriteString("Status","Refused");w.WriteString("Reason",e.GetType().Name+":"+e.Message);refused++;}
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteNumber("Groups",groups);w.WriteNumber("Arcs",arcs);w.WriteEndObject();w.Flush();Console.WriteLine($"actual{items.Length}/accepted{accepted}/refused{refused}/groups{groups}/arcs{arcs}/guards{guards}/cancel{cancels}");
