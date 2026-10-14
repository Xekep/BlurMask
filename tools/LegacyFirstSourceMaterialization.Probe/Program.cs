using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=4)throw new ArgumentException("AK, engine profile, separation profile and audit output required.");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
 var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
 InkStroke[] raw=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();
 var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var categories=new LegacyCursiveCategoryMapper(profile.Categories);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
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
 var firstCategories=LegacySourceSamplerCategoryOwner.ReadFirstChildOf249(resource,6104);
 var directCategories=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,6120);
 if(firstCategories.Serialized249SourceScoreExponent!=.75f||!StringComparer.OrdinalIgnoreCase.Equals(firstCategories.SourceFingerprint,factory.SourceFingerprint))throw new Exception("Actual first249 ownership differs.");
 using var stream=File.Create(args[3]);using var writer=new Utf8JsonWriter(stream);writer.WriteStartArray();int snapshots=0;
 foreach(bool discontinuous in new[]{false,true})foreach(bool loaded in new[]{false,true})
 {
  int[] map=discontinuous?new[]{1,3,4,6,8}:Enumerable.Range(0,raw.Length).ToArray();
  var parent=Enumerable.Range(0,map[^1]+1).Select(_=>new InkStroke(new[]{new InkPoint(-999,-999)})).ToArray();
  for(int i=0;i<map.Length;i++)parent[map[i]]=raw[i];
  var categoriesOwner=loaded?firstCategories:directCategories;var output=LegacyFirstResultSourceMaterializer.Materialize(result,sourceRanges,categoriesOwner,map,parent);
  if(output.Length!=result.GlyphReferences.Count)throw new Exception("Source720 count differs from actual selected rich references.");
  for(int i=0;i<output.Length;i++)
  {
   var source=output[i];byte[] glyph=result.Result.GlyphPool.CopyRecord(result.GlyphReferences[i]);
   writer.WriteStartObject();writer.WriteNumber("exponent",loaded?.75f:1f);writer.WriteString("glyph",Convert.ToBase64String(glyph));writer.WriteString("source",Convert.ToBase64String(source.CopyStorage().Span));
   writer.WriteStartArray("parent");foreach(var stroke in parent){writer.WriteStartArray();foreach(var p in stroke.Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
   writer.WriteStartArray("map");foreach(int mapped in map)writer.WriteNumberValue(mapped);writer.WriteEndArray();
   var key=LegacyFirstResultGlyphKey.Decode(result.GlyphReferences[i]);
   writer.WriteStartArray("childRanges");foreach(var r in sourceRanges.ResolveCandidate(key.Node,key.Arc)){writer.WriteStartArray();foreach(int v in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();
   writer.WriteStartArray("ranges");foreach(var r in source.Ranges){writer.WriteStartArray();foreach(int v in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteEndObject();snapshots++;
  }
  parent[map[0]]=new InkStroke(new[]{new InkPoint(-77,-77)});
  if(BinaryPrimitives.ReadSingleLittleEndian(output[0].CopyStorage().Span[24..])==-77)throw new Exception("Source rows retain caller buffers.");
 }
 writer.WriteEndArray();writer.Flush();
 Reject(()=>LegacyFirstResultSourceMaterializer.Materialize(result,sourceRanges,LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0),Enumerable.Range(0,raw.Length).ToArray(),raw));
 Reject(()=>LegacyFirstResultSourceMaterializer.Materialize(result,sourceRanges,firstCategories,new[]{0,0,2,3,4},raw));
 Reject(()=>LegacyFirstResultSourceMaterializer.Materialize(result,sourceRanges,firstCategories,Enumerable.Range(0,raw.Length).ToArray(),raw,new CancellationToken(true)));
 var foreign=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,cuts.Chronology,row.OriginalSource,5);
 Reject(()=>LegacyFirstResultSourceMaterializer.Materialize(result,foreign,firstCategories,Enumerable.Range(0,raw.Length).ToArray(),raw));
 Console.WriteLine($"Prepared actual raw first NN/{scores.Length}groups -> rich275/{result.GlyphReferences.Count} glyph40 ->{snapshots} source720 snapshots; actual249 .75 vs raw1, continuous/discontinuous parent submission. Run independent Python source auditor; main frame/LM/controller still excluded.");
static InkStroke Draw(params(float X,float Y)[]points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Invalid ownership admitted.");}
