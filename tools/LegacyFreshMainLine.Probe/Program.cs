using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=5)throw new ArgumentException("AK, engine profile, separation profile, initial controls and audit output required.");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[3]));int initialCases=0;
foreach(var test in controls.RootElement.EnumerateArray())
{
 var points=test.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray();
 var invocation=LegacyFreshSingleStrokeMainLine.Prepare(resource,0,new InkStroke(points),0);
 byte[] expected=Convert.FromBase64String(test.GetProperty("descriptor").GetString()!);
 if(!invocation.CopyInitialDescriptor().Span.SequenceEqual(expected))throw new Exception("Independent full88 fresh descriptor differs.");
 if(invocation.Controller.Source.Strokes.Count!=1||invocation.Controller.SourceScoreExponent!=.75f||invocation.Controller.FirstSourceOffset!=6120)
  throw new Exception("Native singleton main/child/resource ownership differs.");
 initialCases++;
}
InkStroke[] raw=[Draw((0,90),(0,90),(0,10),(25,60),(50,10),(50,90))];
var main=LegacyFreshSingleStrokeMainLine.Prepare(resource,0,raw[0],0);
var factory=main.FirstFactory;var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();
if(row.OriginalSource.Strokes[0].Points.Count!=5||main.Controller.Source.Strokes[0].Points.Count!=5)
 throw new Exception("Actual parent retained submission lost duplicate chronology.");
if(!main.Controller.Source.OriginalPointIndices[0].Span.SequenceEqual(new[]{0,2,3,4,5})||
 !row.OriginalSource.OriginalPointIndices[0].Span.SequenceEqual(new[]{0,1,2,3,4}))
 throw new Exception("Child raw source ordinals were replaced by the user's duplicate-containing ordinal owner.");
 var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var categories=new LegacyCursiveCategoryMapper(profile.Categories);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,row.OriginalSource.Strokes.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[row.OriginalSource.Strokes.Count],Enumerable.Repeat(-1,row.OriginalSource.Strokes.Count).ToArray());
 var sourceRanges=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,cuts.Chronology,row.OriginalSource,5);var graph=sourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);
 var geometry=new LegacyFirstOrdinaryTransitionGeometry(profile,frame,cuts,graph,logTable);var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
 var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];var ranked=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var normalized=order.Select(i=>ranges[i]).ToArray();var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(normalized,cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
  int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,row.Frame.Line.Height,separation.GapOffset,separation.Slope,logTable.Values.Span,separation.MaximumCost);
  ranked[g]=new(scores[g].Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(cost,categories.CompositeCategoryCount).ToArray(),1);
 }
 var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
 for(int node=0;node<graph.NodeCount;node++)for(byte span=(byte)Math.Min(4,node);;span--)
 {
  var arcs=graph.GetNode(node);for(int ordinal=0;ordinal<arcs.Count;ordinal++)if(arcs[ordinal].Span==span)
  {
   var group=ranked[arcs[ordinal].GroupId];foreach(var entry in group.Entries.Span[..Math.Min(20,group.Entries.Length)])
   {
    if(!scorer.Alphabet.TryGetText(entry.Symbol,out string text)||text.Length!=1||!(char.IsAsciiLetterOrDigit(text[0])||text[0] is >= '\u0400' and <= '\u04ff'))continue;
    foreach(bool end in new[]{false,true}){int category=categories.GetSymbolCompositeCategory(entry.Symbol,end);if(category>=0&&categories.GetFirstId(category)<=5)requests[node].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,group.Query(entry.Symbol,ushort.MaxValue,category,false,false),group.GetFirstControllerLikelihoodByte(entry.Symbol)));}
   }
  }if(span==0)break;
 }
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,5,geometry.Correct118,geometry.Correct140);
 int GlyphCost(int n,ushort arc,ushort symbol,byte category,CancellationToken token)=>ranked[graph.GetNode(n)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,token);
 var merged=LegacyFirstOrdinaryContextPasses.RunForFreshSelectedChild(primary,links,factory.InvocationPolicy.Binding,GlyphCost,geometry.Correct118,geometry.Correct140,new());

 var materializer=new LegacyFirstPlainWordMaterializer(resource,6120,factory,logTable);var result=materializer.Materialize(cuts,graph,primary,merged,scores);
var output=main.ApplyFirstResult(result,sourceRanges);
if(output.SourceRows.Count==0||output.Next24ARequiresLineInference||!output.Output.AddedLine||output.Output.NewLineFit is null||output.Output.Deskew is null)
 throw new Exception("Nonempty actual first result failed native24B/main line assignment.");
byte[] descriptor=output.Output.CopyDescriptor().ToArray();
byte[] initial=main.CopyInitialDescriptor().ToArray();
BinaryPrimitives.WriteInt32LittleEndian(initial.AsSpan(16),0);
BinaryPrimitives.WriteInt32LittleEndian(initial.AsSpan(56),output.SourceRows.Count);
if(!descriptor.AsSpan().SequenceEqual(initial))throw new Exception("Defined descriptor changes exceed actual10/38 writer fields.");
var independentlyMaterialized=LegacyFirstResultSourceMaterializer.Materialize(result,sourceRanges,main.Controller.FirstCategoryOwner,[0],main.Controller.Source.Strokes);
if(independentlyMaterialized.Length!=output.SourceRows.Count)throw new Exception("Actual source720 interval differs.");
for(int i=0;i<independentlyMaterialized.Length;i++)
 if(!independentlyMaterialized[i].CopyStorage().Span.SequenceEqual(output.SourceRows[i].CopyStorage().Span))throw new Exception("Actualsource720 ownership differs.");
Reject(()=>main.ApplyFirstResult(result,sourceRanges,new CancellationToken(true)));
var foreign=LegacyFreshSingleStrokeMainLine.Prepare(resource,0,raw[0],0);
Reject(()=>foreign.ApplyFirstResult(result,sourceRanges));
Reject(()=>LegacyFreshSingleStrokeMainLine.Prepare(resource,0,raw[0],float.NaN));
var copy=main.CopyInitialDescriptor().ToArray();copy[0]=0;
if(main.CopyInitialDescriptor().Span[0]!=16)throw new Exception("Caller mutates main descriptor snapshot.");
var guide=output.Output.CopyGuides()[0];var fit=output.Output.NewLineFit.Value;
if(guide.Length!=40||BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(12))!=fit.Baseline||
 BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(16))!=fit.Height||BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(24))!=0)
 throw new Exception("Actual main guide lost pre-deskew publication geometry.");
using var stream=File.Create(args[4]);using var writer=new Utf8JsonWriter(stream);
writer.WriteStartObject();writer.WriteString("scope","actual singleton physical source, first NN/recurrence result, before24D/29B");
writer.WriteNumber("initialCases",initialCases);writer.WriteNumber("groups",scores.Length);writer.WriteNumber("sourceRows",output.SourceRows.Count);
writer.WriteString("initialDescriptor",Convert.ToBase64String(main.CopyInitialDescriptor().Span));writer.WriteString("descriptor",Convert.ToBase64String(descriptor));
writer.WriteString("guide",Convert.ToBase64String(guide));writer.WriteNumber("baseline",fit.Baseline);writer.WriteNumber("height",fit.Height);writer.WriteNumber("slope",fit.Slope);
writer.WriteString("basis",fit.Basis.ToString());writer.WriteStartArray("glyphReferences");foreach(var value in result.GlyphReferences)writer.WriteStringValue(value.ToString("x16"));writer.WriteEndArray();
writer.WriteStartArray("raw");foreach(var p in raw[0].Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("source720");foreach(var record in output.SourceRows)writer.WriteStringValue(Convert.ToBase64String(record.CopyStorage().Span));writer.WriteEndArray();writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS {initialCases} Python full88 descriptors + actual first NN/{scores.Length} groups/{output.SourceRows.Count} source720 ->24B guide0; retained5/6raw, loaded249 .75,24A skip, foreign/cancel guards. This is not main OCR or completed24D/29B.");
static InkStroke Draw(params(float X,float Y)[]points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Invalid ownership admitted.");}
