using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [actual-ru-ak.res engine-profile.json separation-profile.json]");
void Require(bool condition,string message){if(!condition)throw new Exception(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){return;}throw new Exception("Invalid input admitted.");}
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int controls=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);
 ulong head=word.AppendGlyph(1,0,false,new byte[40]).Reference;word.SetLastOrdinarySymbol(17);ulong tail=word.AppendGlyph(0,0,false,new byte[40]).Reference;word.LinkGlyph(head,tail);word.SetLastOrdinarySymbol(19);
 float[] values=c.GetProperty("scalars").EnumerateArray().Select(x=>BitConverter.UInt32BitsToSingle(x.GetUInt32())).ToArray();word.SetRawScalars(values[0],values[1],values[2],values[3]);
 int firstCalls=0,lastCalls=0;var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
 var result=publisher.Publish(word,(n,a)=>{firstCalls++;return c.GetProperty("first").GetInt32();},(n,a)=>{lastCalls++;return c.GetProperty("last").GetInt32();},c.GetProperty("a").GetInt32(),c.GetProperty("b").GetInt32());
 Require(result.Span==new LegacyNativeWordSourceSpan(c.GetProperty("expectedFirst").GetInt32(),c.GetProperty("expectedLast").GetInt32())&&firstCalls==1&&lastCalls==1,"Native callbacks/overrides field ordering differs.");
 byte[] published=result.Word.CopyStorage();for(int i=0;i<4;i++)Require(BinaryPrimitives.ReadUInt32LittleEndian(published.AsSpan(16+i*4))==c.GetProperty("scalars")[i].GetUInt32(),"275 clone rewrote WORD rawscalar bytes.");
 var numeric=LegacyFirstPlainWordScalars.ReadUnadjusted(c.GetProperty("total").GetInt32(),c.GetProperty("acoustic").GetInt32(),c.GetProperty("nodes").GetInt32(),c.GetProperty("kind").GetInt32());
 float[] fields=[numeric.LogMass10,numeric.Calibration14,numeric.NormalizedUnadjustedCost18,numeric.NormalizedAcousticCost1C];for(int i=0;i<4;i++)Require(BitConverter.SingleToUInt32Bits(fields[i])==c.GetProperty("nativeWordScalars")[i].GetUInt32(),"WORD unadjusted integer-to-float producer differs from independent scalar oracle.");controls++;
}
using(var cancellation=new CancellationTokenSource())
{
 var p=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var w=LegacyNativeWordBuilder.CreateEmpty(p,new byte[120]);w.AppendGlyph(0,0,false,new byte[40]);w.SetLastOrdinarySymbol(1);
 var publisher=new LegacyNativePlainWordPublisher(p,false,false,false);int later=0;
 Reject(()=>publisher.Publish(w,(n,a)=>{cancellation.Cancel();return 1;},(n,a)=>{later++;return 2;},3,4,cancellation.Token));Require(publisher.Head is null&&later==0,"Override bypassed callback cancellation.");
}
Console.WriteLine($"PASS {controls} independent WORD276->275 rawscalars and nativeparam7/8 field overrides; original APIs/defaults unchanged, callbacks before overrides and cancellation.");
if(args.Length==4)
{
 var resource=File.ReadAllBytes(args[1]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");
 var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
 InkStroke[] raw=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();
 var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var categories=new LegacyCursiveCategoryMapper(profile.Categories);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);var links=new LegacyOrdinaryCandidateConnections(graph);
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
 Require(factory.PrimaryBeamWidthF8==5,"Actual first source BA/F8 capacity changed.");
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,factory.PrimaryBeamWidthF8,geometry.Correct118,geometry.Correct140);
 int GlyphCost(int n,ushort arc,ushort symbol,byte category,CancellationToken token)=>ranked[graph.GetNode(n)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,token);
 var merged=LegacyFirstOrdinaryContextPasses.RunForFreshSelectedChild(primary,links,factory.InvocationPolicy.Binding,GlyphCost,geometry.Correct118,geometry.Correct140,new());
 var diagnosticGrid=new LegacyFirstMergedRecordGrid(1,merged.BeamWidth,merged.CopyB8Storage());
 var diagnosticChoice=LegacyFirstTerminalSelection.Select(diagnosticGrid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,false,false,factory.InvocationPolicy.Binding.SourceKind,0)).Choices[0];
 foreach(var cell in LegacyFirstSelectedGlyphTrace.Read(diagnosticGrid,diagnosticChoice))Console.WriteLine($"cell node{cell.Node} arc{cell.Arc} span{cell.Span}/graph{graph.GetNode(cell.Node)[cell.Arc].Span} symbol{cell.Symbol} path{cell.WordBoundaryBit} row{cell.Row}");
 var materializer=new LegacyFirstPlainWordMaterializer(resource,6120,factory,logTable);var result=materializer.Materialize(cuts,graph,primary,merged,scores);
 var literalResult=materializer.MaterializeLiteralSymbols(resource,6120,cuts,graph,primary,merged,scores);
 Require(result.PublishedWord.Word.CopyStorage().SequenceEqual(literalResult.PublishedWord.Word.CopyStorage())&&result.PublishedWord.Span==literalResult.PublishedWord.Span&&result.Selection==literalResult.Selection,"Additive literal API changed the existing strict WORD path.");
 for(int i=0;i<result.GlyphReferences.Count;i++)Require(result.Result.GlyphPool.CopyRecord(result.GlyphReferences[i]).SequenceEqual(literalResult.Result.GlyphPool.CopyRecord(literalResult.GlyphReferences[i])),"Additive literal API changed ordinary glyph byte/scalar ownership.");
 var grid=new LegacyFirstMergedRecordGrid(1,merged.BeamWidth,merged.CopyB8Storage());var cells=LegacyFirstSelectedGlyphTrace.Read(grid,result.Selection);
 Require(result.PublishedWord.Word.GetReferenceCount()==cells.Count&&result.PublishedWord.Word.GetSymbolSpan()==cells.Count,"Zero-count endpoint leaked into selected word refs.");
 Require(!result.PublishedWord.Word.FlatReferences.Contains(result.DetachedLeadingReference)&&!result.PublishedWord.Word.FlatReferences.Contains(result.DetachedTrailingReference),"Reset did not detach endpoint references.");
 Require(result.GlyphReferences.SequenceEqual(result.PublishedWord.Word.FlatReferences),"Published word differs from selected reading order.");
 Require(ReferenceEquals(result.Graph,graph),"Materialization lost actual source graph ownership.");
 var symbols=result.GlyphReferences.Select(reference=>BinaryPrimitives.ReadUInt16LittleEndian(result.Result.GlyphPool.CopyRecord(reference).AsSpan(8))).ToArray();Require(symbols.SequenceEqual(cells.Select(cell=>cell.Symbol)),"Literal first labels changed through materialization.");
 byte[] header=result.PublishedWord.Word.CopyStorage();Require(BinaryPrimitives.ReadSingleLittleEndian(header.AsSpan(16))==-5f*result.Selection.TotalCost/8192f&&BinaryPrimitives.ReadSingleLittleEndian(header.AsSpan(20))==1&&BinaryPrimitives.ReadSingleLittleEndian(header.AsSpan(24))==(float)result.Selection.TotalCost/(grid.NodeCount*8192)&&BinaryPrimitives.ReadSingleLittleEndian(header.AsSpan(28))==(float)result.Selection.AcousticCost/8192,"Actual unadjusted WORD scalar mapping differs.");
 Require(BinaryPrimitives.ReadSingleLittleEndian(header.AsSpan(24))!=result.Selection.Score.NormalizedCost10&&result.Selection.Score.NormalizedCost10==0,"TOP relative cost was conflated with WORD's unadjusted cost.");
 string actual=string.Concat(symbols.Select(s=>scorer.Alphabet.TryGetText(s,out string text)?text:"?"));Require(actual=="MИP"&&result.Selection.TotalCost==41268,"Existing real plain first fixture changed; this is a mixed-script diagnostic, not Russian success.");
 for(int i=0;i<cells.Count;i++)
 {
  byte[] glyph=result.Result.GlyphPool.CopyRecord(result.GlyphReferences[i]);var cell=cells[i];var source=primary.GetRowCopy(cell.Node);var cache=scores[graph.GetNode(cell.Node)[cell.Arc].GroupId];
  Require(BinaryPrimitives.ReadInt32LittleEndian(glyph)==cell.Row&&BinaryPrimitives.ReadSingleLittleEndian(glyph.AsSpan(12))==source.GetLikelihoodByte(cell.Row)/255f,"Descriptor/primary98 likelihood ownership differs.");
  Require(BinaryPrimitives.ReadSingleLittleEndian(glyph.AsSpan(16))==LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,6120).ReadSymbolScore(cache.Glyph.Scores,cache.Cache,cell.Symbol),"Glyph score substituted TOP mass or cost exponential.");
  Require(BinaryPrimitives.ReadInt32LittleEndian(glyph.AsSpan(20))!=0x4000000&&BinaryPrimitives.ReadSingleLittleEndian(glyph.AsSpan(28))==0,"Ordinary enabled cost14 or separate AVL owner1C mapping differs.");
 }
 Reject(()=>materializer.Materialize(cuts,graph,primary,merged,Array.Empty<LegacyFirstOrdinaryCacheScores>()));Reject(()=>materializer.Materialize(cuts,graph,primary,merged,scores,new CancellationToken(true)));
 Reject(()=>materializer.Materialize(row.GenerateRussianCuts(),graph,primary,merged,scores));
 var changed=scores.ToArray();int selectedGroup=graph.GetNode(cells[0].Node)[cells[0].Arc].GroupId;changed[selectedGroup]=scores[(selectedGroup+1)%scores.Length];
 Reject(()=>materializer.Materialize(cuts,graph,primary,merged,changed));
 Console.WriteLine($"PASS actual rawfirst6120 NN/{scores.Length}groups->primary98/B8->richWORD/275 {actual}:{result.Selection.TotalCost}, {cells.Count} literal glyph40 rows; endpoints poolonly, scalars separate, source720 not synthesized.");
}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
