using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=4)throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
using var output=File.Create(args[3]);using var writer=new Utf8JsonWriter(output);writer.WriteStartArray();int actualGroups=0,synthetic=0,selectedHidden=0;
foreach(var raw in fixtures)
{
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
 var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 actualGroups+=scores.Length;var owned=LegacyFreshFirstProtocolOwnership.Create(factory);var producer=owned.PreparePrimary(resource,cuts,prepared,scores,table);
 var primary=producer.Run();var context=LegacyFirstBlankContextPasses.Run(resource,factory,cuts,prepared,producer,table);
 var materializer=new LegacyFirstPlainWordMaterializer(resource,6120,factory,table);var guard=LegacyFreshFirstSelectedGlyphPublication.Create(factory);
 if(guard.BlankE2!=192||guard.ClosedE4!=191||guard.Configuration99)throw new Exception("Actual fresh allocation policy changed.");
 for(int s=0;s<=194;s++)if(guard.ShouldAllocate((ushort)s)!=(s!=192&&s!=191))throw new Exception("Outer allocation predicate differs.");
 try{guard.ShouldAllocate(65535);throw new Exception("Unbounded selected label accepted.");}catch(ArgumentOutOfRangeException){}
 using(var canceled=new CancellationTokenSource())
 {canceled.Cancel();try{guard.ShouldAllocate(192,canceled.Token);throw new Exception("Canceled publication decision accepted.");}catch(OperationCanceledException){}}
 Capture(primary,context,false);
 //Friend-only controlled selected paths use actual owned graph and NN/cache,
 //but prepared scalar costs. Every selected symbol is hidden by the proven
 //outer guard, so no fabricated per-label model confidence is needed/read.
 for(int test=0;test<6;test++)
 {
  var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
  ushort arc=checked((ushort)links.GetSuccessorOrdinals(-1,ushort.MaxValue,0).Span[0]);
  for(int n=0;n<graph.NodeCount;n++)
  {
   if(n>0)arc=checked((ushort)links.GetSuccessorOrdinals(n-1,arc,0).Span[0]);
   ushort symbol=test==0||n==0?guard.ClosedE4:test==1?guard.BlankE2:(n%2==0?guard.ClosedE4:guard.BlankE2);
   if(test==3&&n==graph.NodeCount-1||test==5&&n==0)
   {int group=graph.GetNode(n)[arc].GroupId;symbol=scores[group].Cache.Candidates.ToArray().First(c=>c.Symbol<180&&c.Symbol!=178&&profile.Categories.StartCompositeMap.Span[c.Symbol]>=0).Symbol;}
   int category=profile.Categories.StartCompositeMap.Span[symbol];if(category<0)throw new Exception("Hidden symbol category missing.");
   requests[n].Add(new(arc,0,symbol,(byte)category,100+n,123));
  }
  static LegacyFirstOrdinaryTransitionResult Correction(LegacyFirstOrdinaryTransitionRequest r,int value,CancellationToken token)
  {token.ThrowIfCancellationRequested();return new(true,value);}
  var own=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,factory.PrimaryBeamWidthF8,Correction,Correction);
  int Cost(int n,ushort arc,ushort symbol,byte category,CancellationToken token){token.ThrowIfCancellationRequested();return 100+n;}
  LegacyFirstOrdinaryContextPasses controlled;
  if(test==0)controlled=LegacyFirstBlankContextPasses.RunPreparedZeroTraitRows(resource,factory,cuts,prepared,producer,table,own,Cost,_=>{});
  else
  {
   //Publication-only prepared controls: actual gap corrections can reject an
   //artificial all-blank path. Do not override those callbacks or claim they
   //accepted. Seed valid context ownership with all-E4 and replace selected
   //metadata in deep-copied tables; blank/E4 both share B8 symbolic identity.
   var closedRequests=requests.Select(rs=>rs.Select(r=>r with {Symbol=guard.ClosedE4,CompositeCategory=(byte)profile.Categories.StartCompositeMap.Span[guard.ClosedE4]}).ToArray()).ToArray();
   var closedPrimary=LegacyFirstOrdinaryAcousticRecurrence.Run(links,closedRequests,factory.PrimaryBeamWidthF8,Correction,Correction);
   var closedContext=LegacyFirstBlankContextPasses.RunPreparedZeroTraitRows(resource,factory,cuts,prepared,producer,table,closedPrimary,Cost,_=>{});
   LegacyFirstAcousticRecordTable[][] Rewrite(bool final)
   {
    var tables=Enumerable.Range(0,own.NodeCount).Select(n=>Enumerable.Range(0,own.BeamWidth).Select(r=>final?closedContext.GetB8RowCopy(n,r):closedContext.GetB0RowCopy(n,r)).ToArray()).ToArray();
    for(int n=0;n<own.NodeCount;n++)for(int r=0;r<own.GetRowCopy(n).Count;r++)
    {
     var p=own.GetRowCopy(n).Get(r);byte[] data=tables[n][r].CopyStorage();
     for(int col=0;col<tables[n][r].Count;col++)
     {
      int at=col*40;BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at+12),p.Symbol);ushort old=BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(at+14));
      ushort packed=(ushort)((old&0xfc0f)|(p.CompositeCategory<<4));if(test==4)packed|=1;BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at+14),packed);
      ulong id;
      if(n==0)id=closedContext.Contexts.ResolveInitial(p.Symbol).Identity;
      else
      {
       ulong parent=tables[n-1][0].Get(0).Identity;
       if(p.Symbol==guard.BlankE2||final&&p.Symbol==guard.ClosedE4)id=parent;
       else {var child=closedContext.Contexts.ResolveChild(parent,p.Symbol);if(child.Created)closedContext.Contexts.LinkCreatedContext(child.Identity,parent);id=child.Identity;}
      }
      BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(at+24),id);
     }
     tables[n][r]=new(data,tables[n][r].Count);
    }
    return tables;
   }
   controlled=LegacyFirstOrdinaryContextPasses.FromOwnedTables(own,Rewrite(false),Rewrite(true),own.BeamWidth,closedContext.ColumnCountB0,closedContext.ColumnCountB8,closedContext.Contexts);
  }
  Capture(own,controlled,true);synthetic++;
 }
 void Capture(LegacyFirstOrdinaryAcousticRecurrence pri,LegacyFirstOrdinaryContextPasses ctx,bool syntheticControl)
 {
  var grid=new LegacyFirstMergedRecordGrid(ctx.ColumnCountB8,ctx.BeamWidth,ctx.CopyB8Storage());
  var terminal=LegacyFirstTerminalSelection.Select(grid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,factory.InvocationPolicy.Invocation.Configuration99,factory.InvocationPolicy.Invocation.ConfigurationA8,factory.InvocationPolicy.Binding.SourceKind,0));
  if(terminal.Choices.Count!=1)throw new Exception("Publication fixture requires one finite actual choice.");
  var trace=LegacyFirstSelectedGlyphTrace.Read(grid,terminal.Choices[0]);var emitted=trace.Where(c=>guard.ShouldAllocate(c.Symbol)).ToArray();selectedHidden+=trace.Count-emitted.Length;
  var result=materializer.MaterializeNativePlainResult(resource,6120,cuts,graph,pri,ctx,scores);
  if(result.Selection!=terminal.Choices[0]||!ReferenceEquals(result.Graph,graph)||!ReferenceEquals(result.Cuts,cuts)||result.GlyphReferences.Count!=emitted.Length)throw new Exception("Publication altered selected/source ownership.");
  bool published=guard.ShouldAllocate(trace[0].Symbol)&&emitted.Length>0;
  if((result.PublishedWord is not null)!=published||result.Result.WordPublisher.Head!=result.PublishedWord||result.DetachedLeadingReference.HasValue!=guard.ShouldAllocate(trace[0].Symbol))throw new Exception("Native root publication/leading-endpoint guard differs.");
  if(!published)
  {if((emitted.Length==0&&result.OwnerFirstScores1C.Count!=1)||result.MinimumOwnerFirstScore80!=1)throw new Exception("Hidden cells allocated/read glyph-owner scores or leading endpoint.");try{materializer.MaterializeLiteralSymbols(resource,6120,cuts,graph,pri,ctx,scores);throw new Exception("Word-only API fabricated hidden-root word.");}catch(NotSupportedException){}}
  else
  {var old=materializer.MaterializeLiteralSymbols(resource,6120,cuts,graph,pri,ctx,scores);if(!old.PublishedWord.Word.CopyStorage().AsSpan().SequenceEqual(result.PublishedWord!.Word.CopyStorage()))throw new Exception("Existing nonempty publication changed.");
   int first=unchecked((int)graph.GetNode(emitted[0].Node)[emitted[0].Arc].StartContextId),last=unchecked((int)graph.GetNode(trace[^1].Node)[trace[^1].Arc].EndContextId);
   if(result.PublishedWord!.Span!=new LegacyNativeWordSourceSpan(first,last))throw new Exception("Skipped cells changed native source callback/terminal override policy.");}
  ulong[] endpoints=result.DetachedLeadingReference.HasValue?[result.DetachedTrailingReference,result.DetachedLeadingReference.Value]:[result.DetachedTrailingReference];
  foreach(ulong endpoint in endpoints)
  {var record=result.Result.GlyphPool.CopyRecord(endpoint);if((BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4))&7)!=1||BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(8))!=1)throw new Exception("Actual detached zero-span endpoint lost.");}
  writer.WriteStartObject();writer.WriteBoolean("synthetic",syntheticControl);writer.WriteNumber("blank",guard.BlankE2);writer.WriteNumber("closed",guard.ClosedE4);writer.WriteBoolean("configuration99",guard.Configuration99);
  writer.WriteNumber("selectedCost",result.Selection.TotalCost);writer.WriteNumber("acousticCost",result.Selection.AcousticCost);writer.WriteNumber("nodeCount",graph.NodeCount);writer.WriteNumber("sourceKind",factory.InvocationPolicy.Binding.SourceKind);
  writer.WriteNumber("wordScalar10",result.WordScalars.LogMass10);writer.WriteNumber("wordScalar18",result.WordScalars.NormalizedUnadjustedCost18);writer.WriteNumber("wordScalar1c",result.WordScalars.NormalizedAcousticCost1C);
  writer.WriteBoolean("hasWord",result.PublishedWord is not null);
  if(result.PublishedWord is not null){writer.WriteNumber("spanFirst",result.PublishedWord.Span.First);writer.WriteNumber("spanLast",result.PublishedWord.Span.Last);writer.WriteString("word",Convert.ToHexString(result.PublishedWord.Word.CopyStorage()));}
  writer.WriteStartArray("trace");foreach(var cell in trace){writer.WriteStartArray();writer.WriteNumberValue(cell.Node);writer.WriteNumberValue(cell.Arc);writer.WriteNumberValue(cell.Symbol);writer.WriteNumberValue(cell.Row);writer.WriteNumberValue(cell.Column);writer.WriteNumberValue(pri.GetRowCopy(cell.Node).GetLikelihoodByte(cell.Row));writer.WriteNumberValue(unchecked((int)graph.GetNode(cell.Node)[cell.Arc].EndContextId));writer.WriteNumberValue(unchecked((int)graph.GetNode(cell.Node)[cell.Arc].StartContextId));writer.WriteEndArray();}writer.WriteEndArray();
  writer.WriteStartArray("glyphs");foreach(ulong reference in result.GlyphReferences){writer.WriteStartObject();writer.WriteNumber("key",LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference));writer.WriteString("record",Convert.ToHexString(result.Result.GlyphPool.CopyRecord(reference)));writer.WriteEndObject();}writer.WriteEndArray();
  writer.WriteStartArray("endpoints");foreach(ulong reference in endpoints)writer.WriteStringValue(Convert.ToHexString(result.Result.GlyphPool.CopyRecord(reference)));writer.WriteEndArray();writer.WriteEndObject();
  Console.WriteLine($"{(syntheticControl?"INTERNAL selected-path":"RealNN")} cost{result.Selection.TotalCost}: trace[{string.Join(',',trace.Select(c=>c.Symbol))}] ->{emitted.Length} glyphs/word={result.PublishedWord is not null}.");
 }
}
writer.WriteEndArray();writer.Flush();if(actualGroups!=57||synthetic!=24||selectedHidden==0)throw new Exception("Publication coverage incomplete.");
Console.WriteLine($"PASS4 actual57NNgroups +24 INTERNAL selected-path controls; {selectedHidden} selected E2/E4 allocation suppressions. Full selected/source/scalars retained; no Russian winner claim.");
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
