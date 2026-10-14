using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=4)throw new ArgumentException("AK, engine profile, separation profile and audit output required.");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[] raw=[Draw((0,90),(0,90),(0,10),(25,60),(50,10),(50,90))];
var post=LegacyFreshSingleGlyphMainPostprocessing.Prepare(resource,0,raw[0],0);
var main=post.Input;
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
var prepared=post.ApplyFirstResult(result,sourceRanges);
if(prepared.SourceGap!=1.5f||prepared.Shear!=0||prepared.Smoothed.Strokes.Count!=1||prepared.Smoothed.Strokes[0].Points.Count<=5)
 throw new Exception("Actual owned main24D/29B/265 output differs.");
var unchangedGuide=prepared.LinePreparation.Output.CopyGuides()[0];
if(!prepared.CopyGuide().Span.SequenceEqual(unchangedGuide))throw new Exception("24D/29B/265 must retain actual main guide bytes.");
for(int p=0;p<prepared.Smoothed.Strokes[0].Points.Count;p++)
 if(prepared.Chronology.Resolve(0,p).Key.First!=0)throw new Exception("Generated265 key lost its actual retained metadata owner.");
Reject(()=>post.ApplyFirstResult(result,sourceRanges,new CancellationToken(true)));
var changed=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(changed.AsSpan(post.Processor29BOffset+42),2.75f);
var changedPost=LegacyFreshSingleGlyphMainPostprocessing.Prepare(changed,0,raw[0],0);
if(changedPost.SerializedZeroGap!=2.75f)throw new Exception("29B serialized gap was replaced with a default.");
Reject(()=>changedPost.ApplyFirstResult(result,sourceRanges));
var invalid=resource.ToArray();invalid[post.Processor24DOffset+17]=invalid[post.Processor24DOffset+16];
Reject(()=>LegacyFreshSingleGlyphMainPostprocessing.Prepare(invalid,0,raw[0],0));
using var stream=File.Create(args[3]);using var writer=new Utf8JsonWriter(stream);
writer.WriteStartObject();writer.WriteNumber("groups",scores.Length);writer.WriteNumber("sourceRows",prepared.LinePreparation.SourceRows.Count);
writer.WriteNumber("shear",prepared.Shear);writer.WriteNumber("gap",prepared.SourceGap);
writer.WriteString("guide",Convert.ToBase64String(prepared.CopyGuide().Span));
writer.WriteString("before720",Convert.ToBase64String(prepared.LinePreparation.Output.Deskew!.Rows[0].CopyStorage().Span));
writer.WriteString("after720",Convert.ToBase64String(prepared.SourceRow.CopyStorage().Span));
writer.WriteStartArray("ranges");foreach(var r in prepared.SourceRow.Ranges){writer.WriteStartArray();foreach(var v in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("original");foreach(var p in main.Controller.Source.Strokes[0].Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("before265");foreach(var p in prepared.CopyBefore265Points()[0]){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("smoothed");foreach(var p in prepared.Smoothed.Strokes[0].Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("keys200");foreach(int key in prepared.Smoothed.PointKeys200.Span)writer.WriteNumberValue(key);writer.WriteEndArray();
writer.WriteStartArray("sourceKeys");for(int point=0;point<prepared.Smoothed.Strokes[0].Points.Count;point++)
{var key=prepared.Chronology.Resolve(0,point).Key;writer.WriteStartArray();writer.WriteNumberValue(key.First);writer.WriteNumberValue(key.Second);writer.WriteNumberValue(key.Third);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("metrics");writer.WriteNumberValue(prepared.OriginalSourceMetrics.OrthogonalFraction);writer.WriteNumberValue(prepared.OriginalSourceMetrics.CornerFraction);writer.WriteEndArray();
writer.WriteStartArray("profile");foreach(float v in new[]{post.Smoothing.Minimum,post.Smoothing.Maximum,post.Smoothing.Extra,post.Smoothing.OrthogonalLow,post.Smoothing.OrthogonalHigh,post.Smoothing.CornerLow,post.Smoothing.CornerHigh,post.Smoothing.FlatnessSquaredFactor})writer.WriteNumberValue(v);writer.WriteEndArray();
writer.WriteNumber("weight",prepared.Smoothed.SmoothingWeight);writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS actual raw/first NN{scores.Length}groups ->source720/24B/24A-assigned/24D-zero-gap/29B-loaded/265:{prepared.Smoothed.Strokes[0].Points.Count}points +factor200chronology. Generalmulti-glyphshear/mainwinner excluded.");
static InkStroke Draw(params(float X,float Y)[]points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException or InvalidDataException){return;}throw new Exception("Invalid ownership/profile admitted.");}
