using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=3)throw new ArgumentException("models blind-inputs capture");
var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));int guards=0,cancels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected refusal");}
var ink=new[]{new InkStroke(Enumerable.Range(0,32).Select(i=>new InkPoint(10*i,(float)(40*Math.Sin(i*1.3)))).ToArray())};
LegacyFullEnglishFirstPrimary B3(IReadOnlyList<InkStroke>? raw=null)
{var parent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,raw??ink,0,m30,m50);var b1=LegacyFullEnglishFirstController.Prepare(parent);b1.Run(table);var b2=LegacyFullEnglishFirstControllerCosts.Prepare(b1,table);b2.Run();var b3=LegacyFullEnglishFirstPrimary.Prepare(b2);b3.Run();return b3;}
var primary=B3();var b4=LegacyFullEnglishFirstContextPasses.Prepare(primary);b4.Run();
var owner=LegacyFullEnglishFirstTerminal.Prepare(b4);Guard(()=>owner.Run(new CancellationToken(true)));if(owner.IsFaulted||owner.IsComplete)throw new Exception("Preentry consumed");cancels++;owner.Run();Guard(()=>owner.Run());
Guard(()=>LegacyFullEnglishFirstTerminal.Prepare(null!));Guard(()=>LegacyFullEnglishFirstTerminal.Prepare(b4,new CancellationToken(true)));cancels++;
var incomplete=LegacyFullEnglishFirstContextPasses.Prepare(primary);Guard(()=>LegacyFullEnglishFirstTerminal.Prepare(incomplete));
var bad=LegacyFullEnglishFirstContextPasses.Prepare(primary);Guard(()=>bad.RunPrepared(s=>{if(s=="child:published")throw new InvalidDataException("Injected");},null));if(!bad.IsFaulted||bad.CopyCompletedChildren().Count==0)throw new Exception("No upstream history");Guard(()=>LegacyFullEnglishFirstTerminal.Prepare(bad));
var exact=LegacyFullEnglishFirstTerminal.PrepareBounded(b4,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork);exact.Run();
Guard(()=>LegacyFullEnglishFirstTerminal.PrepareBounded(b4,owner.AdditionalLogicalPeakBytes-1,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork));Guard(()=>LegacyFullEnglishFirstTerminal.PrepareBounded(b4,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes-1,owner.LogicalVisitWork));Guard(()=>LegacyFullEnglishFirstTerminal.PrepareBounded(b4,owner.AdditionalLogicalPeakBytes,owner.LogicalCopyWorkBytes,owner.LogicalVisitWork-1));
Guard(()=>LegacyFullEnglishFirstTerminalBudget.Calculate(4097,1));Guard(()=>LegacyFullEnglishFirstTerminalBudget.Calculate(1,2));Guard(()=>LegacyFullEnglishFirstTerminalBudget.Calculate(1,257));
var max=LegacyFullEnglishFirstTerminalBudget.Calculate(4096,256);if(max!=(12845056L,21495808L,5242880L))throw new Exception("Max budget arithmetic");
foreach(string stage in new[]{"entry","child:entry","grid","selected","trace","child:published"})
{var p=LegacyFullEnglishFirstTerminal.Prepare(b4);using var cts=new CancellationTokenSource();Guard(()=>p.RunPrepared(s=>{if(s==stage)cts.Cancel();},cts.Token));if(!p.IsFaulted||p.IsComplete)throw new Exception("Entered cancel not poisoned");Guard(()=>p.Run());_=p.CopyCompletedChildren();cancels++;}
var fault=LegacyFullEnglishFirstTerminal.Prepare(b4);Guard(()=>fault.RunPrepared(s=>{if(s=="selected")throw new InvalidDataException("Injected");}));if(!fault.IsFaulted||fault.IsComplete)throw new Exception("Fault not poisoned");Guard(()=>fault.Run());
foreach(var child in owner.CopyCompletedChildren())
{if(!ReferenceEquals(child.Owner,owner)||!ReferenceEquals(child.Context.Owner,b4))throw new Exception("Foreign child");var trace=child.Cells.ToArray();if(trace.Length>0){var saved=trace[0].CopyRecord().ToArray();var copy=trace[0].CopyRecord().ToArray();copy[0]^=255;if(!trace[0].CopyRecord().Span.SequenceEqual(saved))throw new Exception("Mutable trace bytes");trace[0]=null!;if(child.Cells[0]is null)throw new Exception("Mutable trace collection");}}
var concurrent=LegacyFullEnglishFirstTerminal.Prepare(b4);using(var entry=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{var task=Task.Run(()=>concurrent.RunPrepared(s=>{if(s=="entry"){entry.Set();release.Wait();}}));if(!entry.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Barrier timeout");try{Guard(()=>concurrent.Run());_=concurrent.CopyCompletedChildren();}finally{release.Set();}task.GetAwaiter().GetResult();}
var multiRaw=ink.Concat(ink.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();var multiB4=LegacyFullEnglishFirstContextPasses.Prepare(B3(multiRaw));multiB4.Run();if(multiB4.CopyCompletedChildren().Count<2)throw new Exception("Expected multiline guard input");var multi=LegacyFullEnglishFirstTerminal.Prepare(multiB4);using(var cts=new CancellationTokenSource()){int entries=0;Guard(()=>multi.RunPrepared(s=>{if(s=="child:entry"&&++entries==2)cts.Cancel();},cts.Token));if(!multi.IsFaulted||multi.CopyCompletedChildren().Count!=1)throw new Exception("Missing whole historical prefix");cancels++;}
//PREPARED-only nofinite grid: no raw source/word promotion.
var impossible=new byte[200];for(int r=0;r<5;r++)BinaryPrimitives.WriteInt32LittleEndian(impossible.AsSpan(r*40),0x4000000);
var empty=LegacyFirstTerminalSelection.Select(new(1,5,new ReadOnlyMemory<byte>[] {impossible}),owner.Invocation);if(empty.Choices.Count!=0||empty.ReferenceCost78!=0)throw new Exception("Synthetic empty grid");
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
using var stream=File.Create(args[2]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteString("Scope","Actual full FIRST numeric terminal only, no WORD720/guide");w.WriteBoolean("TargetsRead",false);w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);w.WriteNumber("PreparedEmptyGrid",1);w.WriteStartArray("Actual");int accepted=0,refused=0,children=0,cells=0;
void Ints(IEnumerable<int> xs){w.WriteStartArray();foreach(int x in xs)w.WriteNumberValue(x);w.WriteEndArray();}
string Bits(float x)=>Convert.ToHexString(BitConverter.GetBytes(x)).ToLowerInvariant();
foreach(var item in items)
{
 var raw=item.TryGetProperty("Raw",out var r)?r:item.GetProperty("strokes");string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;float resolution=item.TryGetProperty("Resolution",out var rr)?rr.GetSingle():0;var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();w.WriteStartObject();w.WriteString("Name",name);
 try
 {
  var parent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);var b1=LegacyFullEnglishFirstController.Prepare(parent);b1.Run(table);int nn=parent.NetworkEvaluationCount;var b2=LegacyFullEnglishFirstControllerCosts.Prepare(b1,table);b2.Run();var b3=LegacyFullEnglishFirstPrimary.Prepare(b2);b3.Run();var context=LegacyFullEnglishFirstContextPasses.Prepare(b3);context.Run();var terminal=LegacyFullEnglishFirstTerminal.Prepare(context);terminal.Run();if(!terminal.IsComplete||terminal.IsFaulted||parent.NetworkEvaluationCount!=nn)throw new Exception("Completion or NN replay");
  w.WriteString("Status","AcceptedTerminal");w.WriteNumber("NetworkCount",nn);w.WriteNumber("Peak",terminal.AdditionalLogicalPeakBytes);w.WriteNumber("Work",terminal.LogicalCopyWorkBytes);w.WriteNumber("Visits",terminal.LogicalVisitWork);w.WriteStartArray("Children");
  foreach(var child in terminal.CopyCompletedChildren())
  {
   w.WriteStartObject();w.WriteNumber("Descriptor",child.Context.Primary.Initializer.ActualCache.Owner.Child.DescriptorOrdinal);w.WriteNumber("Reference78",child.ReferenceCost78);w.WriteBoolean("HasFinite",child.HasFiniteTerminal);
   w.WriteStartArray("Contexts");foreach(var node in child.Context.CopySymbolContexts()){w.WriteStartArray();foreach(ulong id in new[]{node.Identity,node.ParentLink,node.ChildTreeRoot,node.Left,node.Right})w.WriteNumberValue(id);w.WriteNumberValue(node.Symbol);w.WriteNumberValue(node.Balance);w.WriteNumberValue(node.PageIndex);w.WriteNumberValue(node.PageSlot);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("Alternatives");
   if(child.Choice is{}q)
   {w.WriteStartObject();w.WriteNumber("Row",q.Row);w.WriteNumber("Column",q.Column);w.WriteNumber("Total",q.TotalCost);w.WriteNumber("Acoustic",q.AcousticCost);w.WriteNumber("Identity",q.ContextIdentity);w.WritePropertyName("Trait");Ints(new[]{q.TraitNode,q.TraitRow,q.TraitColumn});w.WriteStartArray("ScoreBits");w.WriteNumberValue(q.Score.RelativeCost);foreach(var v in new[]{q.Score.NormalizedCost10,q.Score.LogMass,q.Score.Mass08,q.Score.NormalizedAcousticCost14})w.WriteStringValue(Bits(v));w.WriteEndArray();w.WriteStartArray("Cells");
    foreach(var c in child.Cells){w.WriteStartObject();w.WritePropertyName("Coordinates");Ints(new[]{c.Node,c.Row,c.Column,(int)c.Arc,c.Symbol,c.Span,c.WordBoundaryBit?1:0,c.PreviousNode,c.PreviousArc});w.WriteBase64String("Bytes",c.CopyRecord().Span);w.WriteEndObject();cells++;}w.WriteEndArray();w.WriteEndObject();}
   w.WriteEndArray();w.WriteEndObject();children++;
  }w.WriteEndArray();accepted++;
 }
 catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){w.WriteString("Status","Refused");w.WriteString("Reason",e.GetType().Name+":"+e.Message);refused++;}
 w.WriteEndObject();w.Flush();
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteNumber("Children",children);w.WriteNumber("Cells",cells);w.WriteEndObject();Console.WriteLine($"PASS B5 actual{items.Length} accepted{accepted} refused{refused} children{children} cells{cells} guards{guards} cancel{cancels}");
