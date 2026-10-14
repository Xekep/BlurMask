using InkRecognition;
using System.Text.Json;

if(args.Length!=3)throw new ArgumentException("AK.res engine-profile.json separation-profile.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));
var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
int groups=0,primaryRecords=0,contextRecords=0;
foreach(var raw in fixtures)
{
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);
 var ownership=LegacyFreshFirstProtocolOwnership.Create(factory);
 if(ownership.NativeProtocolExclusion194||ownership.ControllerKind!=0x23f||ownership.ConfiguredLinguisticSourceCount!=0)throw new Exception("Fresh selected controller identity changed.");
 ownership.ValidateFactory(factory);
 var foreign=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);
 try{ownership.ValidateFactory(foreign);throw new Exception("Equal-looking foreign controller accepted.");}catch(ArgumentException){}
 using(var cancel=new CancellationTokenSource())
 {cancel.Cancel();try{LegacyFreshFirstProtocolOwnership.Create(factory,cancel.Token);throw new Exception("Canceled ownership accepted.");}catch(OperationCanceledException){}}
 var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);
 int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
  var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 groups+=scores.Length;
 var owned=ownership.PreparePrimary(resource,cuts,prepared,scores,table);
 var reference=LegacyFreshFirstBlankPrimary.Prepare(resource,factory,cuts,prepared,scores,table,false);
 for(int n=0;n<graph.NodeCount;n++)if(!owned.Requests[n].SequenceEqual(reference.Requests[n]))throw new Exception("Owned protocol changes actual proposals.");
 var primary=owned.Run();var expected=reference.Run();
 for(int n=0;n<graph.NodeCount;n++)
 {var p=primary.GetRowCopy(n);primaryRecords+=p.Count;if(!p.CopyStorage().AsSpan().SequenceEqual(expected.GetRowCopy(n).CopyStorage()))throw new Exception("Owned protocol changes primary98.");}
 var context=LegacyFirstBlankContextPasses.Run(resource,factory,cuts,prepared,owned,table);
 var referenceContext=LegacyFirstBlankContextPasses.Run(resource,factory,cuts,prepared,reference,table);
 Equal(context.CopyB0Storage(),referenceContext.CopyB0Storage(),context,referenceContext);Equal(context.CopyB8Storage(),referenceContext.CopyB8Storage(),context,referenceContext);
 for(int n=0;n<graph.NodeCount;n++)for(int r=0;r<context.BeamWidth;r++)contextRecords+=context.GetB0RowCopy(n,r).Count+context.GetB8RowCopy(n,r).Count;
 var grid=new LegacyFirstMergedRecordGrid(context.ColumnCountB8,context.BeamWidth,context.CopyB8Storage());
 var terminal=LegacyFirstTerminalSelection.Select(grid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,factory.InvocationPolicy.Invocation.Configuration99,factory.InvocationPolicy.Invocation.ConfigurationA8,factory.InvocationPolicy.Binding.SourceKind,0));
 Console.WriteLine($"Owned23F protocol194=false: {scores.Length} actualNN groups, selectedcost{terminal.Choices[0].TotalCost}, labels[{string.Join(',',LegacyFirstSelectedGlyphTrace.Read(grid,terminal.Choices[0]).Select(c=>c.Symbol))}].");
 using(var canceled=new CancellationTokenSource())
 {canceled.Cancel();try{ownership.PreparePrimary(resource,cuts,prepared,scores,table,canceled.Token);throw new Exception("Canceled real producer accepted.");}catch(OperationCanceledException){}}
 try{ownership.PreparePrimary(resource,foreign.Frame.Row.GenerateRussianCuts(),prepared,scores,table);throw new Exception("Foreign cuts accepted.");}catch(ArgumentException){}
}
if(groups!=57)throw new Exception("Owned/pruned actual fixture coverage changed.");
Console.WriteLine($"PASS4 ownedprotocol controls/{groups} realNN groups/{primaryRecords} primary/{contextRecords} context records; exact explicit-false regression, foreign-owner/cancel guards. Not a linguistic winner or Russian accuracy proof.");
static void Equal(IReadOnlyList<ReadOnlyMemory<byte>> actual,IReadOnlyList<ReadOnlyMemory<byte>> expected,
 LegacyFirstOrdinaryContextPasses actualContext,LegacyFirstOrdinaryContextPasses expectedContext)
{
 if(actual.Count!=expected.Count)throw new Exception("Owned context extents differ.");
 var am=actualContext.Contexts.CopyNodes().ToDictionary(c=>c.Identity);var em=expectedContext.Contexts.CopyNodes().ToDictionary(c=>c.Identity);
 int[] Path(ulong id,bool first)
 {var map=first?am:em;var path=new List<int>();while(id!=0){var node=map[id];path.Add(node.Symbol);id=node.ParentLink;}path.Reverse();return path.ToArray();}
 for(int i=0;i<actual.Count;i++)for(int p=0;p<actual[i].Length;p+=40)
 {
  var aa=actual[i].Span.Slice(p,40);var ee=expected[i].Span.Slice(p,40);
  //Context identities are globally unique allocation IDs. Compare every
  //remaining byte AND the complete owned symbolic chain independently.
  if(!aa[..24].SequenceEqual(ee[..24])||!aa[32..].SequenceEqual(ee[32..])||
   !Path(BitConverter.ToUInt64(aa[24..]),true).SequenceEqual(Path(BitConverter.ToUInt64(ee[24..]),false)))
    throw new Exception("Owned protocol changes context scalar/predecessor/path.");
 }
}
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
