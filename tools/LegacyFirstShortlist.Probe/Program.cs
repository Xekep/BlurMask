using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 4))throw new ArgumentException("Usage: LegacyFirstShortlist.Probe controls.json [ru-ak.res engine-profile.json separation-profile.json]");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,cells=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 var primary=new LegacyFirstAcousticRecordTable(Convert.FromHexString(c.GetProperty("primary").GetString()!),c.GetProperty("count").GetInt32());
 var table=LegacyFirstAcousticShortlistTable.Build(primary,c.GetProperty("cap").GetInt32(),100,101,arc=>c.GetProperty("providers").GetProperty(arc.ToString()).EnumerateArray().Select(e=>new LegacyFirstAcousticShortlistCandidate(e[0].GetUInt16(),e[1].GetSingle())).ToArray(),Convert.FromHexString(c.GetProperty("initial").GetString()!));
 if(!table.CopyStorage().AsSpan().SequenceEqual(Convert.FromHexString(c.GetProperty("expected").GetString()!))||table.GroupCount!=c.GetProperty("owners").GetArrayLength())throw new Exception("Native A8 defined/opaque bytes or distinctarc grouping differed from independent oracle.");
 for(int g=0;g<table.GroupCount;g++)if(table.GetPrimaryOrdinal(g)!=c.GetProperty("owners")[g].GetInt32()||table.GetFiniteAlternativeCount(g)!=c.GetProperty("counts")[g].GetInt32())throw new Exception("Native first-primary arc ownership/count differs.");
 var copy=table.CopyStorage();copy[0]^=255;if(copy.AsSpan().SequenceEqual(table.CopyStorage()))throw new Exception("Shortlist storage exposes mutable ownership.");
 cells+=table.BeamWidth*table.MaximumAlternatives;cases++;
}
Console.WriteLine($"PASS {cases} independent freshA8 first-arc/top20/finite-stop controls/{cells} fixed-width cells with opaque bytes.");
{
 var source=new LegacyFirstAcousticRecordTable(1);source.Insert(new(100,80,255,0,0,0,10,true,0,0,0));
 using var cancelled=new CancellationTokenSource();
 try{LegacyFirstAcousticShortlistTable.Build(source,1,100,101,_=>{cancelled.Cancel();return new[]{new LegacyFirstAcousticShortlistCandidate(10,1)};},cancellationToken:cancelled.Token);throw new Exception("Provider cancellation ignored.");}catch(OperationCanceledException){}
 try{LegacyFirstAcousticShortlistTable.Build(source,1,100,101,_=>new[]{new LegacyFirstAcousticShortlistCandidate(10,float.NaN)});throw new Exception("Nonfinite likelihood accepted.");}catch(ArgumentException){}
 var old=source.CopyStorage();var captured=LegacyFirstAcousticShortlistTable.Build(source,1,100,101,_=>{source.Insert(new(-100,-100,255,0,5,0,20,true,0,0,0));return new[]{new LegacyFirstAcousticShortlistCandidate(10,1)};});
 if(captured.GetCell(0,0).TotalCost!=100||captured.GetCell(0,0).ArcOrdinal!=0)throw new Exception("Callback mutation changed retained source snapshot.");
}
if(args.Length==4)
{
 var bytes=File.ReadAllBytes(args[1]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");
 var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));var geometryProfile=LegacyFirstTransitionGeometryProfile.ReadSource(bytes,6120);
 InkStroke[] mir=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var frame=LegacyFreshOrdinaryPacketFrame.PrepareFirstSource(bytes,6120,mir);var row=frame.Row;var cuts=row.GenerateRussianCuts();
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(bytes,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,mir.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(bytes,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(bytes,12727),new byte[mir.Length],Enumerable.Repeat(-1,mir.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);var links=new LegacyOrdinaryCandidateConnections(graph);
 var geometry=new LegacyFirstOrdinaryTransitionGeometry(geometryProfile,frame,cuts,graph,logTable);var categories=new LegacyCursiveCategoryMapper(geometryProfile.Categories);
 var separation=LegacyCursiveSeparationProfile.ReadV106Node(bytes,11506);var rankedGroups=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
 for(int g=0;g<graph.Groups.Count;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var indices=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var normalized=indices.Select(i=>ranges[i]).ToArray();
  var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(normalized,cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  var scores=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
  int geometryCost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,row.Frame.Line.Height,separation.GapOffset,separation.Slope,logTable.Values.Span,separation.MaximumCost);
  rankedGroups[g]=new(scores.Cache,scorer.Alphabet,bytes.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(geometryCost,categories.CompositeCategoryCount).ToArray(),1);
 }
 var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
 for(int node=0;node<graph.NodeCount;node++)for(byte span=(byte)Math.Min(4,node);;span--)
 {
  var arcs=graph.GetNode(node);for(int ordinal=0;ordinal<arcs.Count;ordinal++)if(arcs[ordinal].Span==span)
  {
   var ranked=rankedGroups[arcs[ordinal].GroupId];foreach(var entry in ranked.Entries.Span[..Math.Min(20,ranked.Entries.Length)])
   {
    if(!scorer.Alphabet.TryGetText(entry.Symbol,out string text)||text.Length!=1||!(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))continue;
    foreach(bool end in new[]{false,true}){int category=categories.GetSymbolCompositeCategory(entry.Symbol,end);if(category>=0&&categories.GetFirstId(category)<=5)requests[node].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,ranked.Query(entry.Symbol,ushort.MaxValue,category,false,false),ranked.GetFirstControllerLikelihoodByte(entry.Symbol)));}
   }
  }if(span==0)break;
 }
 var recurrence=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,5,geometry.Correct118,geometry.Correct140);
 ushort fallback=(ushort)scorer.Alphabet.LabelCount;ushort closed=scorer.Alphabet.TryGetLabel("\U000f0001",out ushort id)?id:ushort.MaxValue;
 int actualGroups=0;for(int node=0;node<graph.NodeCount;node++)
 {
  var primary=recurrence.GetRowCopy(node);var arcs=graph.GetNode(node);
  var shortlist=LegacyFirstAcousticShortlistTable.BuildFromRanked(primary,1,fallback,closed,arc=>rankedGroups[arcs[arc].GroupId]);
  var seen=new HashSet<ushort>();int expectedGroup=0;
  for(int i=0;i<primary.Count;i++)
  {
   var p=primary.Get(i);if(!seen.Add(p.ArcOrdinal))continue;
   var actual=shortlist.GetCell(expectedGroup,0);var expected=rankedGroups[arcs[p.ArcOrdinal].GroupId].Entries.Span.ToArray().Take(20).First(e=>e.Symbol!=fallback&&e.Symbol!=closed);
   if(actual.TotalCost!=p.TotalCost||actual.AcousticCost!=p.AcousticCost||actual.Symbol!=expected.Symbol||actual.ArcOrdinal!=p.ArcOrdinal||actual.Likelihood!=(byte)Math.Clamp((int)(expected.NormalizedLikelihood*255),1,255)||shortlist.GetPrimaryOrdinal(expectedGroup)!=i)throw new Exception("Actual native A8 lost first primary-arc cost/ranked output ownership.");
   expectedGroup++;
  }
  if(expectedGroup!=shortlist.GroupCount||shortlist.BeamWidth!=5)throw new Exception("Actual fixedF8 A8 extent differs from active group count.");
  actualGroups+=shortlist.GroupCount;
 }
 Console.WriteLine($"PASS actual first NN35groups->complete primary nonroot recurrence->fresh A8 fixedF8=5,A0=1: {actualGroups} active groups. No B0/B8/final native winner claim.");
}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
