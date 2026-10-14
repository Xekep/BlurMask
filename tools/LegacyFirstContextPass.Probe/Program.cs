using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 4))throw new ArgumentException("Usage: LegacyFirstContextPass.Probe controls.json [ru-ak.res engine-profile.json separation-profile.json]");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));var alphabet=OwnAlphabet();int cases=0,totalCells=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 int n=c.GetProperty("count").GetInt32();var source=Source(n);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),c.GetProperty("atoms").GetInt32());var connections=new LegacyOrdinaryCandidateConnections(graph);
 var requests=c.GetProperty("requests").EnumerateArray().Select(row=>(IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>)row.EnumerateArray().Select(g=>new LegacyFirstOrdinaryGlyphRequest(g.GetProperty("arc").GetUInt16(),g.GetProperty("span").GetByte(),g.GetProperty("symbol").GetUInt16(),g.GetProperty("category").GetByte(),Cost(g.GetProperty("node").GetInt32(),g.GetProperty("arc").GetUInt16(),g.GetProperty("symbol").GetUInt16(),g.GetProperty("category").GetByte(),default),(byte)(50+g.GetProperty("symbol").GetInt32()*70))).ToArray()).ToArray();
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,c.GetProperty("width").GetInt32(),C118,C140);
 var result=LegacyFirstOrdinaryContextPasses.Run(primary,connections,alphabet,Cost,C118,C140,c.GetProperty("cap0").GetInt32(),c.GetProperty("cap8").GetInt32(),c.GetProperty("e0").GetBoolean(),new());
 foreach(bool final in new[]{false,true})for(int node=0;node<n;node++)for(int row=0;row<result.BeamWidth;row++)
 {
  var records=final?result.GetB8RowCopy(node,row):result.GetB0RowCopy(node,row);var expected=c.GetProperty(final?"b8":"b0")[node][row];
  if(records.Count!=expected.GetArrayLength())throw new Exception("Native context pass finite row count differs from independent symbolic oracle.");
  for(int col=0;col<records.Count;col++)
  {
   var a=records.Get(col);var e=expected[col];
   if(a.TotalCost!=e.GetProperty("total").GetInt32()||a.AcousticCost!=e.GetProperty("acoustic").GetInt32()||a.Symbol!=e.GetProperty("symbol").GetUInt16()||a.ArcOrdinal!=e.GetProperty("arc").GetUInt16()||a.CompositeCategory!=e.GetProperty("category").GetByte()||a.Span!=e.GetProperty("span").GetByte()||a.PreviousAlternative!=e.GetProperty("previous").GetByte()||a.ContinuationType!=e.GetProperty("column").GetByte()||a.PathBit!=e.GetProperty("pathbit").GetBoolean()||records.GetLikelihoodByte(col)!=e.GetProperty("likelihood").GetByte()||!Sequence(result.Contexts,a.Identity).Select(v=>(int)v).SequenceEqual(e.GetProperty("sequence").EnumerateArray().Select(v=>v.GetInt32())))throw new Exception("Native B0/B8 scalar fields/context identity/path choice differ from independent oracle.");
   totalCells++;
  }
 }
 cases++;
}
Console.WriteLine($"PASS {cases} independent first primary->B0/B8 context fixtures/{totalCells} records, both explicitE0 modes, root copy, signed identity dedup and predecessor row/column fields.");
{
 var source=Source(2);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),1);var connections=new LegacyOrdinaryCandidateConnections(graph);
 IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[] requests=[new[]{new LegacyFirstOrdinaryGlyphRequest(0,0,0,0,3,50)},new[]{new LegacyFirstOrdinaryGlyphRequest(0,0,1,0,4,120)}];
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,2,C118,C140);
 for(int stage=0;stage<3;stage++)
 {
  using var cancellation=new CancellationTokenSource();int laterCallbacks=0;
  try
  {
   LegacyFirstOrdinaryContextPasses.Run(primary,connections,alphabet,(n,a,s,c,t)=>{if(stage==0)cancellation.Cancel();return 4;},
    (r,v,t)=>{if(stage==1)laterCallbacks++;if(stage==2)cancellation.Cancel();return new(true,v);},
    (r,v,t)=>{if(stage==1)cancellation.Cancel();return new(true,v);},1,1,false,new(),cancellation.Token);
   throw new Exception("Context callback cancellation ignored.");
  }catch(OperationCanceledException){}
  if(stage==1&&laterCallbacks!=0)throw new Exception("Callback118 ran after140 canceled.");
 }
 var existing=new LegacySymbolContextFactory();existing.ResolveInitial(0);
 try{LegacyFirstOrdinaryContextPasses.Run(primary,connections,alphabet,Cost,C118,C140,1,1,false,existing);throw new Exception("Nonfresh context container admitted.");}catch(ArgumentException){}
 try{LegacyFirstOrdinaryContextPasses.Run(primary,connections,alphabet,Cost,C118,C140,0,1,false,new());throw new Exception("Zero unsupported B0 cap admitted.");}catch(ArgumentException){}
 var result=LegacyFirstOrdinaryContextPasses.Run(primary,connections,alphabet,Cost,C118,C140,1,1,false,new());
 var row=result.GetB8RowCopy(0,0);row.Insert(new(-100,-100,255,0,0,0,0,true,0,0,0));
 if(result.GetB8RowCopy(0,0).Get(0).TotalCost!=primary.GetRowCopy(0).Get(0).TotalCost)throw new Exception("Context record copy exposed mutable ownership.");
 Console.WriteLine("PASS context glyph/callback140/callback118 cancellation, fresh-container/cap bounds and independent record copy ownership.");
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
 foreach(bool mode in new[]{false,true})
 {
  int nativeCost(int node,ushort arc,ushort symbol,byte category,CancellationToken token)=>rankedGroups[graph.GetNode(node)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,token);
  var calls=new List<(int Stage,LegacyFirstOrdinaryTransitionRequest Request,int Incoming,LegacyFirstOrdinaryTransitionResult Result)>();
  LegacyFirstOrdinaryTransitionResult Apply118(LegacyFirstOrdinaryTransitionRequest r,int incoming,CancellationToken token){var v=geometry.Correct118(r,incoming,token);calls.Add((118,r,incoming,v));return v;}
  LegacyFirstOrdinaryTransitionResult Apply140(LegacyFirstOrdinaryTransitionRequest r,int incoming,CancellationToken token){var v=geometry.Correct140(r,incoming,token);calls.Add((140,r,incoming,v));return v;}
  var contexts=LegacyFirstOrdinaryContextPasses.Run(recurrence,links,scorer.Alphabet,nativeCost,Apply118,Apply140,1,1,mode,new());
  var finalNode=graph.NodeCount-1;int finite=0;var outputs=new List<string>();
  for(int i=0;i<contexts.BeamWidth;i++)
  {
   var records=contexts.GetB8RowCopy(finalNode,i);if(records.Count==0)continue;
   var record=records.Get(0);var symbols=Sequence(contexts.Contexts,record.Identity);
   outputs.Add(string.Concat(symbols.Select(s=>s==0xfffb?" ":scorer.Alphabet.TryGetText(s,out string v)?v:"?"))+":"+record.TotalCost);finite++;
  }
  if(finite==0||contexts.CopyB8Storage().Any(storage=>storage.Length!=200))throw new Exception("Actual first context passes did not reach fixedF8/A4 final grid.");
  Console.WriteLine($"Actual native explicitE0={mode}: B8 prepared alternatives[{string.Join(", ",outputs)}]; no final native273 result/automaticpolicy/accuracyclaim.");
  using var output=File.Create(args[0]+$".actual-e0-{mode}.json");using var writer=new Utf8JsonWriter(output);
  writer.WriteStartObject();writer.WriteBoolean("modeE0",mode);writer.WriteNumber("beamWidth",contexts.BeamWidth);writer.WriteNumber("columnCount",contexts.ColumnCountB8);
  writer.WriteStartArray("nodes");foreach(var storage in contexts.CopyB8Storage())writer.WriteStringValue(Convert.ToHexString(storage.Span));writer.WriteEndArray();
  writer.WriteStartArray("b0");foreach(var storage in contexts.CopyB0Storage())writer.WriteStringValue(Convert.ToHexString(storage.Span));writer.WriteEndArray();
  writer.WriteStartArray("primary");for(int node=0;node<recurrence.NodeCount;node++)writer.WriteStringValue(Convert.ToHexString(recurrence.GetRowCopy(node).CopyStorage()));writer.WriteEndArray();
  writer.WriteStartArray("costs");for(int node=0;node<requests.Length;node++)foreach(var request in requests[node]){writer.WriteStartArray();foreach(int v in new[]{node,(int)request.ArcOrdinal,(int)request.Symbol,(int)request.CompositeCategory,request.GroupCost})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();
  writer.WriteStartArray("successors");for(int node=0;node<graph.NodeCount;node++)for(ushort arc=0;arc<graph.GetNode(node).Count;arc++)for(byte span=0;span<5;span++)
  {
   if(node+span+1>=graph.NodeCount)break;
   writer.WriteStartArray();writer.WriteNumberValue(node);writer.WriteNumberValue(arc);writer.WriteNumberValue(span);foreach(int ordinal in links.GetSuccessorOrdinals(node,arc,span).Span)writer.WriteNumberValue(ordinal);writer.WriteEndArray();
  }writer.WriteEndArray();
  writer.WriteStartArray("calls");foreach(var call in calls){var r=call.Request;writer.WriteStartArray();foreach(int v in new[]{call.Stage,r.CurrentNode,(int)r.CurrentArc,(int)r.CurrentSymbol,(int)r.CurrentCategory,r.PreviousNode,(int)r.PreviousArc,(int)r.PreviousSymbol,(int)r.PreviousCategory,(int)r.Kind,call.Incoming,call.Result.Cost,call.Result.Accepted?1:0})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();
  writer.WriteStartArray("contexts");foreach(var context in contexts.Contexts.CopyNodes()){writer.WriteStartObject();writer.WriteNumber("identity",context.Identity);writer.WriteNumber("parent",context.ParentLink);writer.WriteNumber("symbol",context.Symbol);writer.WriteEndObject();}writer.WriteEndArray();writer.WriteEndObject();
 }

}

static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static int Cost(int node,ushort arc,ushort symbol,byte category,CancellationToken token){token.ThrowIfCancellationRequested();return((node+1)*3+(arc+1)*5+symbol*7+category*11)%91-20;}
static LegacyFirstOrdinaryTransitionResult C118(LegacyFirstOrdinaryTransitionRequest r,int v,CancellationToken token)
{token.ThrowIfCancellationRequested();return(r.CurrentSymbol+r.PreviousSymbol+r.Kind+r.CurrentNode)%11==0?new(false,0):new(true,v+(r.CurrentSymbol*13+r.PreviousSymbol*7+r.Kind*5+r.CurrentNode)%17-8);}
static LegacyFirstOrdinaryTransitionResult C140(LegacyFirstOrdinaryTransitionRequest r,int v,CancellationToken token)
{token.ThrowIfCancellationRequested();return new(true,v+(r.CurrentNode+r.PreviousArc*3+r.CurrentArc*5+r.Kind*2)%11-5);}
static ushort[] Sequence(LegacySymbolContextFactory contexts,ulong identity)
{var values=new List<ushort>();while(identity!=0){var node=contexts.ReadNode(identity);values.Add(node.Symbol);identity=node.ParentLink;}values.Reverse();return values.ToArray();}
static LegacyInkAlphabet OwnAlphabet()
{
 using var stream=new MemoryStream();using var w=new BinaryWriter(stream);for(int i=0;i<7;i++)w.Write(ushort.MaxValue);w.Write((ushort)2);w.Write((ushort)1);w.Write((ushort)0);
 foreach(int n in new[]{32,64})for(int i=0;i<n;i++)w.Write(i==n-1?(ushort)0:ushort.MaxValue);w.Write((ushort)0);w.Write((ushort)2);
 foreach(uint v in new uint[]{0,0,2,1,4,65535})w.Write(v);w.Write(System.Text.Encoding.UTF8.GetBytes("ИП"));w.Write((ushort)0);w.Write((ushort)1);w.Write((byte)0);w.Write((ushort)0);
 return LegacyInkAlphabet.Load(stream.ToArray(),0x103);
}
static LegacyOrdinaryCursiveSeedResult Source(int count)
{
 var raw=Enumerable.Range(0,count).Select(i=>new InkStroke(new[]{new InkPoint(i*100,10),new InkPoint(i*100+20,90)})).ToArray();var ink=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,count).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));return LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(true,0,.3f,.3f),new byte[count],Enumerable.Repeat(-1,count).ToArray());
}
