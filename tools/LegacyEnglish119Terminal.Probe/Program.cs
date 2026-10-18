using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory controller-costs.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));
int guards=0,terminals=0,cells=0,blank=0,closed=0,boundaries=0,empty=0;var canceled=new CancellationToken(true);
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] bytes=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
var actual=new List<Actual>();LegacyEnglish119TerminalResult? previous=null;
foreach(var reference in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(reference.GetProperty("Raw"));var prepared=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);
 var candidates=LegacyOwnedEnglish119Candidates.Prepare(prepared,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);
 var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
 var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
 var groups=candidates.CopyLiveGroupIds();var cacheObjects=groups.Select(g=>cache.ScoreGroup(g)).ToArray();int scored=cache.ScoredGroupCount;
 var rows=Enumerable.Range(0,contexts.NodeCount).Select(n=>Enumerable.Range(0,contexts.BeamWidth).Select(r=>
 {var row=contexts.GetB8RowCopy(n,r);return new Records(row.Count,row.CopyStorage());}).ToArray()).ToArray();
 var result=LegacyOwnedEnglish119Terminal.Select(contexts);var repeated=LegacyOwnedEnglish119Terminal.Select(contexts);
 if(!ReferenceEquals(result.Context,contexts)||!ReferenceEquals(result.Policy,primary.Owner.Policy)||
    result.Invocation!=new LegacyFirstTerminalSelectionPolicy(ushort.MaxValue,0xa000,false,false,0x135,0)||
    cache.ScoredGroupCount!=scored||groups.Where((g,i)=>!ReferenceEquals(cacheObjects[i],cache.ScoreGroup(g))).Any())
    throw new Exception("Same actual context/primary/cache or invocation provenance changed.");
 Alternative[] Capture(LegacyEnglish119TerminalResult value)=>value.Alternatives.Select(a=>
 {
  var c=a.Choice;var s=c.Score;return new Alternative(c.Row,c.Column,c.TotalCost,c.AcousticCost,c.ContextIdentity,
   new[]{c.TraitNode,c.TraitRow,c.TraitColumn},new[]{s.RelativeCost,BitConverter.SingleToInt32Bits(s.NormalizedCost10),BitConverter.SingleToInt32Bits(s.LogMass),BitConverter.SingleToInt32Bits(s.Mass08),BitConverter.SingleToInt32Bits(s.NormalizedAcousticCost14)},
   a.Cells.Select(v=>new Cell(new[]{v.Node,v.Row,v.Column,(int)v.Arc,v.Symbol,v.Span,v.WordBoundaryBit?1:0,v.PreviousNode,v.PreviousArc},v.CopyRecord().ToArray())).ToArray());
 }).ToArray();
 var output=Capture(result);var again=Capture(repeated);
 if(!JsonSerializer.Serialize(output,CaptureJson.Default.AlternativeArray).Equals(JsonSerializer.Serialize(again,CaptureJson.Default.AlternativeArray),StringComparison.Ordinal)||result.ReferenceCost78!=repeated.ReferenceCost78)
    throw new Exception("Repeated selection changed owned cells/scalars.");
 if(previous is not null&&(ReferenceEquals(result.Context,previous.Context)||ReferenceEquals(result.Policy,previous.Policy)))throw new Exception("Foreign equal-model context was substituted.");previous=result;
 foreach(var a in result.Alternatives)
 {
  terminals++;foreach(var c in a.Cells){cells++;if(c.Symbol==217)blank++;if(c.Symbol==211)closed++;if(c.WordBoundaryBit)boundaries++;}
  if(a.Cells.Count>0){var copy=a.Cells[0].CopyRecord().ToArray();byte original=copy[0];copy[0]^=255;if(a.Cells[0].CopyRecord().Span[0]!=original)throw new Exception("Mutable cell escaped.");}
  Guard(()=>((IList<LegacyFirstSelectedGlyphCell>)a.Cells)[0]=null!);
 }
 Guard(()=>LegacyOwnedEnglish119Terminal.Select(contexts,canceled));
 Guard(()=>((IList<LegacyEnglish119TerminalAlternative>)result.Alternatives).Add(null!));
 if(result.Alternatives.Count==0)empty++;
 var arcs=Enumerable.Range(0,candidates.NodeCount).Select(n=>candidates.CopyNode(n).Select(a=>new long[]{a.GroupId,a.StartContextId,a.EndContextId,a.Span}).ToArray()).ToArray();
 actual.Add(new(raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),rows,contexts.CopySymbolContexts().ToArray(),arcs,result.ReferenceCost78,output,scored,cache.ScoredGroupCount));
}
Guard(()=>LegacyOwnedEnglish119Terminal.Select(null!));if(actual.Count!=124||cells==0||terminals==0)throw new Exception("Missing actual source/terminal coverage.");
File.WriteAllText(args[2],JsonSerializer.Serialize(new Output(path,model.Source.Fingerprint,actual,terminals,cells,blank,closed,boundaries,empty,guards),CaptureJson.Default.Output));
Console.WriteLine($"Owned119 terminal{actual.Count}/{terminals} choices/{cells}cells/blank{blank}/closed{closed}/boundaries{boundaries}/empty{empty}/{guards}guards; no extraNN/no WORD/text.");
record Records(int Count,byte[] Bytes);
record Cell(int[] Coordinates,byte[] Bytes);
record Alternative(int Row,int Column,int Total,int Acoustic,ulong Identity,int[] Trait,int[] ScoreBits,Cell[] Cells);
record Actual(float[][][] Raw,Records[][] B8,LegacySymbolContextNode[] Contexts,long[][][] Arcs,long Reference78,Alternative[] Alternatives,int ScoredBefore,int ScoredAfter);
record Output(string Path,string Fingerprint,List<Actual> Actual,int Terminals,int Cells,int Blank,int Closed,int Boundaries,int Empty,int Guards);
[JsonSerializable(typeof(Output))][JsonSerializable(typeof(Alternative[]))]partial class CaptureJson:JsonSerializerContext;
