using InkRecognition;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.Reflection;
if(args.Length is not (4 or 5))throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json [external-corpus.json]");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
using var captureFile=File.Create(args[3]);using var w=new Utf8JsonWriter(captureFile);w.WriteStartArray();using var replayStream=File.Create(args[3]+".owning-replay.json");using var replayWriter=new Utf8JsonWriter(replayStream);replayWriter.WriteStartArray();int groups=0,actual=0,external=0,externalUnsupported=0;
foreach(var raw in fixtures)
{
 var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,raw,activations,table);var result=owner.Run();groups+=owner.GroupScores.Count;actual++;
 var copied=owner.MaskPolicy.CopyMasks();if(copied.Primary.Length!=192||copied.Primary.Span.IndexOf((byte)0)!=-1||copied.Secondary.Span.IndexOf((byte)0)!=-1||owner.MaskPolicy.WidePairCount!=0||owner.MaskPolicy.ClassPreselectionActive)throw new Exception("Actual native first-mask profile changed.");
 if(!MemoryMarshal.TryGetArray(copied.Primary,out ArraySegment<byte> storage))throw new Exception("Copy fixture must be backed by owned storage.");storage.Array![storage.Offset]=0;
 if(owner.MaskPolicy.CopyMasks().Primary.Span[0]!=1)throw new Exception("Public mask copy altered the live owning session.");
 var rerun=owner.Run();if((rerun.Materialized.Selection with {ContextIdentity=0})!=(result.Materialized.Selection with {ContextIdentity=0})||!rerun.Primary.GetRowCopy(0).CopyStorage().AsSpan().SequenceEqual(result.Primary.GetRowCopy(0).CopyStorage()))throw new Exception("Owned model/session rerun changed primary/selection.");
 if(!ReferenceEquals(result.Materialized.Result.Factory,owner.Factory)||!ReferenceEquals(result.Materialized.Cuts,owner.Cuts)||!ReferenceEquals(result.Materialized.Graph,owner.Prepared.SourceRanges.Graph))throw new Exception("Actual whole first result escaped its owning frame/cuts/graph.");
 using(var canceled=new CancellationTokenSource())
 {canceled.Cancel();try{owner.Run(canceled.Token);throw new Exception("Canceled owned recognition accepted.");}catch(OperationCanceledException){}}
 Capture(owner,result,false,null,null);
}
if(groups!=57||actual!=4)throw new Exception("Native owned fixture coverage changed.");
//Mutating caller raw point storage after Prepare cannot alter the owned frame.
var mutablePoints=new[]{new InkPoint(0,0),new InkPoint(60,0),new InkPoint(60,80),new InkPoint(0,80),new InkPoint(0,0)};
var isolated=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,new[]{new InkStroke(mutablePoints)},activations,table);var baseline=isolated.Run();mutablePoints[0]=new(float.NaN,float.NaN);var after=isolated.Run();
if((baseline.Materialized.Selection with {ContextIdentity=0})!=(after.Materialized.Selection with {ContextIdentity=0}))throw new Exception("Caller point mutation altered owned source.");
if(args.Length==5)
{
 using var corpus=JsonDocument.Parse(File.ReadAllBytes(args[4]));
 foreach(var sample in corpus.RootElement.GetProperty("samples").EnumerateArray())
 {
  //Expected text is metrics-only and is NEVER supplied to the owning engine.
  string id=sample.GetProperty("id").GetString()!,expected=sample.GetProperty("expected").GetString()!;
  var ink=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  try{var owner=LegacyFreshFirstOwnedRecognition.Prepare(resource,6120,ink,activations,table);var result=owner.Run();Capture(owner,result,true,id,expected);external++;}
  catch(Exception e)when(e is NotSupportedException or ArgumentException or InvalidDataException or InvalidOperationException)
  {w.WriteStartObject();w.WriteBoolean("external",true);w.WriteString("id",id);w.WriteString("expected",expected);w.WriteString("error",e.GetType().Name+": "+e.Message);w.WriteEndObject();externalUnsupported++;Console.WriteLine($"External diagnostic unsupported {id}: {e.Message}");}
 }
}
w.WriteEndArray();w.Flush();replayWriter.WriteEndArray();replayWriter.Flush();Console.WriteLine($"PASS4 actual raw owning first/57NNgroups; modelmask copies, rerun, cancellation and raw-point ownership. External coordinates diagnostics supported={external}/unsupported={externalUnsupported}; no complete Russian/winner claim.");
void Capture(LegacyFreshFirstOwnedRecognition owner,LegacyFreshFirstOwnedRecognitionResult result,bool externalSample,string? id,string? expected)
{
 if(!externalSample)CaptureOwningReplay(owner,result);
 var native=result.Materialized;var grid=new LegacyFirstMergedRecordGrid(result.Contexts.ColumnCountB8,result.Contexts.BeamWidth,result.Contexts.CopyB8Storage());var trace=LegacyFirstSelectedGlyphTrace.Read(grid,native.Selection);
 string text=string.Concat(native.GlyphReferences.Select(reference=>{byte[] row=native.Result.GlyphPool.CopyRecord(reference);ushort label=BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8));return owner.Factory.InvocationPolicy.Binding.Alphabet.TryGetText(label,out string value)?value:"";}));
 w.WriteStartObject();w.WriteBoolean("external",externalSample);if(id is not null)w.WriteString("id",id);if(expected is not null)w.WriteString("expected",expected);
 w.WriteNumber("groups",owner.GroupScores.Count);w.WriteNumber("cost",native.Selection.TotalCost);w.WriteBoolean("hasWord",native.PublishedWord is not null);w.WriteString("allocatedLiteralText",text);w.WriteString("publishedLiteralText",native.PublishedWord is not null?text:null);w.WriteNumber("glyphs",native.GlyphReferences.Count);
 var masks=owner.MaskPolicy.CopyMasks();w.WriteString("primaryMask",Convert.ToHexString(masks.Primary.Span));w.WriteString("secondaryMask",Convert.ToHexString(masks.Secondary.Span));w.WriteNumber("widePairs",owner.MaskPolicy.WidePairCount);
 w.WriteStartArray("labels");foreach(var c in trace)w.WriteNumberValue(c.Symbol);w.WriteEndArray();
 w.WriteStartArray("costs");foreach(var score in owner.GroupScores){w.WriteStartArray();foreach(int v in score.Cache.Costs.Span)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("primary98");for(int n=0;n<result.Primary.NodeCount;n++)w.WriteStringValue(Convert.ToHexString(result.Primary.GetRowCopy(n).CopyStorage()));w.WriteEndArray();
 w.WriteStartArray("b8");foreach(var b in result.Contexts.CopyB8Storage())w.WriteStringValue(Convert.ToHexString(b.Span));w.WriteEndArray();w.WriteEndObject();
 Console.WriteLine($"{(externalSample?"External coordinates":"Own printed")} {(id??actual.ToString())}: cost{native.Selection.TotalCost}, selected[{string.Join(',',trace.Select(c=>c.Symbol))}], literal {JsonEncodedText.Encode(text)}, word={native.PublishedWord is not null}.");
}
void CaptureOwningReplay(LegacyFreshFirstOwnedRecognition owner,LegacyFreshFirstOwnedRecognitionResult actualResult)
{
 const BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic;
 var producer=(LegacyFreshFirstBlankPrimary)(typeof(LegacyFreshFirstOwnedRecognition).GetField("primaryProducer",fields)!.GetValue(owner)??throw new InvalidDataException("Actual retained producing kernel required."));
 int PrivateInt(string n)=>Convert.ToInt32(typeof(LegacyFreshFirstBlankPrimary).GetField(n,fields)!.GetValue(producer));
 var primaryCalls=new List<int[]>();
 var observedPrimary=producer.RunWithObserver((stage,r,value,result)=>
 {var p=r.Previous;primaryCalls.Add([stage,r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalAlternative,p.OriginalRecord.ArcOrdinal,p.OriginalRecord.Symbol,p.GeometryNode,p.GeometryAlternative,p.GeometryRecord.ArcOrdinal,p.GeometryRecord.Symbol,p.GeometryRecord.CompositeCategory,p.SkippedCount,value,result.Cost,result.Accepted?1:0]);});
 for(int n=0;n<observedPrimary.NodeCount;n++)if(!observedPrimary.GetRowCopy(n).CopyStorage().AsSpan().SequenceEqual(actualResult.Primary.GetRowCopy(n).CopyStorage()))throw new InvalidDataException("Read-only primary observation changed actual owning output.");
 var contextCalls=new List<int[]>();
 var observed=LegacyFirstBlankContextPasses.RunWithObserver(resource,owner.Factory,owner.Cuts,owner.Prepared,producer,table,c=>
 {var r=c.Request;var p=c.Previous;contextCalls.Add([c.Stage,c.FinalPass?1:0,c.CurrentNode,c.CurrentPrimaryRow,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalPrimaryRow,p.OriginalContextAlternative,p.OriginalRecord.ArcOrdinal,p.OriginalRecord.Symbol,p.GeometryNode,p.GeometryPrimaryRow,p.GeometryContextAlternative,p.GeometryRecord.ArcOrdinal,p.GeometryRecord.Symbol,p.GeometryRecord.CompositeCategory,p.SkippedCount,c.IncomingCost,c.Result.Cost,c.Result.Accepted?1:0]);});
 var graph=owner.Prepared.SourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);var successors=new List<int[]>();
 for(byte span=0;span<8;span++)successors.Add(new[]{-1,65535,(int)span}.Concat(links.GetSuccessorOrdinals(-1,ushort.MaxValue,span).ToArray()).ToArray());
 for(int n=0;n<graph.NodeCount;n++)for(ushort a=0;a<graph.GetNode(n).Count;a++)for(byte span=0;span<8;span++)if(n+span+1<graph.NodeCount)successors.Add(new[]{n,(int)a,(int)span}.Concat(links.GetSuccessorOrdinals(n,a,span).ToArray()).ToArray());
 var costs=new List<int[]>();for(int n=0;n<actualResult.Primary.NodeCount;n++)for(int i=0;i<actualResult.Primary.GetRowCopy(n).Count;i++)
 {var r=actualResult.Primary.GetRowCopy(n).Get(i);costs.Add([n,r.ArcOrdinal,r.Symbol,r.CompositeCategory,producer.ReadGroupCost(n,r.ArcOrdinal,r.Symbol,r.CompositeCategory)]);}
 //Additional diagnostic context-network evaluation only. It does NOT replace
 //the57 actual owner GroupScores or provide a cost/proposal/selection input.
 var inputContext=LegacyFirstRowContext.Prepare(resource,6120,owner.Factory.Frame.Row,activations);
 replayWriter.WriteStartObject();var outWriter=replayWriter;
 void Obj(string n,object v){outWriter.WritePropertyName(n);JsonSerializer.Serialize(outWriter,v);}
 void Ints(string n,IEnumerable<int> a)=>Obj(n,a.ToArray());
 void Storage(string n,IEnumerable<ReadOnlyMemory<byte>> a)=>Obj(n,a.Select(x=>Convert.ToHexString(x.Span)).ToArray());
 void Strokes(string n,IEnumerable<LegacyInkStroke> a)=>Obj(n,a.Select(x=>x.Points.ToArray().Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray()).ToArray());
 object ContextNodes(LegacyFirstOrdinaryContextPasses c)=>c.Contexts.CopyNodes().Select(n=>new{identity=n.Identity,parent=n.ParentLink,symbol=n.Symbol}).ToArray();
 outWriter.WriteNumber("fixture",actual);outWriter.WriteNumber("groups",owner.GroupScores.Count);outWriter.WriteNumber("beam",actualResult.Contexts.BeamWidth);outWriter.WriteNumber("cap0",actualResult.Contexts.ColumnCountB0);outWriter.WriteNumber("cap8",actualResult.Contexts.ColumnCountB8);outWriter.WriteBoolean("modeE0",owner.Factory.InvocationPolicy.Binding.NativeModeE0);outWriter.WriteBoolean("excludeConsecutive",producer.NativeConsecutiveBlankExclusion);
 foreach(string n in new[]{"blank","replacement","closed","shadda","newline","space","nbsp"})outWriter.WriteNumber(n,PrivateInt(n));
 outWriter.WriteNumber("trailing",owner.Factory.InvocationPolicy.Binding.TrailingSymbolE6);outWriter.WriteNumber("total",actualResult.Materialized.Selection.TotalCost);outWriter.WriteNumber("acoustic",actualResult.Materialized.Selection.AcousticCost);
 Obj("requests",producer.Requests.Select((r,n)=>r.Select(x=>new[]{n,(int)x.ArcOrdinal,(int)x.Span,(int)x.Symbol,(int)x.CompositeCategory,x.GroupCost,(int)x.LikelihoodByte}).ToArray()).ToArray());Obj("successors",successors);Obj("primaryCalls",primaryCalls);Obj("contextCalls",contextCalls);Obj("costs",costs);
 Obj("arcs",Enumerable.Range(0,graph.NodeCount).Select(n=>graph.GetNode(n).Select(a=>new[]{a.GroupId,(int)a.Span,unchecked((int)a.StartContextId),unchecked((int)a.EndContextId)}).ToArray()).ToArray());
 Storage("primary",Enumerable.Range(0,actualResult.Primary.NodeCount).Select(n=>(ReadOnlyMemory<byte>)actualResult.Primary.GetRowCopy(n).CopyStorage()));Storage("actualB0",actualResult.Contexts.CopyB0Storage());Storage("actualB8",actualResult.Contexts.CopyB8Storage());Obj("actualContexts",ContextNodes(actualResult.Contexts));Storage("b0",observed.CopyB0Storage());Storage("b8",observed.CopyB8Storage());Obj("contexts",ContextNodes(observed));
 var grid=new LegacyFirstMergedRecordGrid(actualResult.Contexts.ColumnCountB8,actualResult.Contexts.BeamWidth,actualResult.Contexts.CopyB8Storage());Obj("selected",LegacyFirstSelectedGlyphTrace.Read(grid,actualResult.Materialized.Selection).Select(x=>new[]{x.Node,(int)x.Arc,(int)x.Span,(int)x.Symbol,x.Row,(int)x.Column,x.WordBoundaryBit?1:0}).ToArray());
 Obj("wordStorage",Convert.ToHexString(actualResult.Materialized.PublishedWord!.Word.CopyStorage()));
 outWriter.WriteStartArray("modelGroups");for(int g=0;g<owner.GroupScores.Count;g++)
 {
  var x=owner.GroupScores[g];var ranges=graph.Groups[g].Ranges.ToArray();outWriter.WriteStartObject();outWriter.WriteNumber("ordinal",g);Strokes("normalized",x.Glyph.Geometry.Strokes);Strokes("merged",x.Glyph.MergedGeometry.Strokes);Ints("context",inputContext.GetContext(owner.Cuts,ranges).Select(v=>(int)v));
  var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(x.Glyph.Geometry.Ranges.ToArray(),owner.Cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);Ints("boundaries",new[]{(int)signs.Start,(int)signs.End});Ints("trajectoryProbabilities",x.Glyph.Scores.TrajectoryProbability32768.ToArray());Ints("rasterProbabilities",x.Glyph.Scores.RasterProbability32768.ToArray());Ints("integerCosts",x.Glyph.IntegerCosts.ToArray());Ints("cacheCosts",x.Cache.Costs.ToArray());outWriter.WriteEndObject();
 }outWriter.WriteEndArray();outWriter.WriteBoolean("additionalDiagnosticContextEvaluation",true);outWriter.WriteEndObject();
}
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
