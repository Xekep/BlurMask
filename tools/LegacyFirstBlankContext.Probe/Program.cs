using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=4)throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
using var stream=File.Create(args[3]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();int actual=0,synthetic=0,skipped=0;
foreach(var raw in fixtures)
{
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 foreach(bool protocol in new[]{false,true})
 {
  var producer=LegacyFreshFirstBlankPrimary.Prepare(resource,factory,cuts,prepared,scores,table,protocol);var primary=producer.Run();var calls=new List<int[]>();
  void Observe(LegacyFirstBlankContextCorrection c)
  {
   var r=c.Request;var p=c.Previous;calls.Add([c.Stage,c.FinalPass?1:0,c.CurrentNode,c.CurrentPrimaryRow,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalPrimaryRow,p.OriginalContextAlternative,p.OriginalRecord.ArcOrdinal,p.OriginalRecord.Symbol,p.GeometryNode,p.GeometryPrimaryRow,p.GeometryContextAlternative,p.GeometryRecord.ArcOrdinal,p.GeometryRecord.Symbol,p.GeometryRecord.CompositeCategory,p.SkippedCount,c.IncomingCost,c.Result.Cost,c.Result.Accepted?1:0]);skipped+=p.SkippedCount;
  }
  var context=LegacyFirstBlankContextPasses.RunWithObserver(resource,factory,cuts,prepared,producer,table,Observe);actual++;
  var grid=new LegacyFirstMergedRecordGrid(context.ColumnCountB8,context.BeamWidth,context.CopyB8Storage());var terminal=LegacyFirstTerminalSelection.Select(grid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,factory.InvocationPolicy.Invocation.Configuration99,factory.InvocationPolicy.Invocation.ConfigurationA8,factory.InvocationPolicy.Binding.SourceKind,0));
  Console.WriteLine($"Actual owned fullcontext {scores.Length} NN groups/protocol194={protocol}: {terminal.Choices[0].TotalCost} labels[{string.Join(',',LegacyFirstSelectedGlyphTrace.Read(grid,terminal.Choices[0]).Select(r=>r.Symbol))}].");
  Capture(primary,context,producer.ReadGroupCost,calls,false);
  if(actual==1)
  {
   LegacyFirstAcousticRecordTable[][] CopyTables(bool final)=>Enumerable.Range(0,context.NodeCount).Select(n=>Enumerable.Range(0,context.BeamWidth).Select(r=>final?context.GetB8RowCopy(n,r):context.GetB0RowCopy(n,r)).ToArray()).ToArray();
   var good0=CopyTables(false);var good8=CopyTables(true);
   var published=LegacyFirstOrdinaryContextPasses.FromOwnedTables(primary,good0,good8,context.BeamWidth,context.ColumnCountB0,context.ColumnCountB8,context.Contexts);
   if(!published.CopyB0Storage().Zip(context.CopyB0Storage()).All(r=>r.First.Span.SequenceEqual(r.Second.Span)))throw new Exception("Deep table publication changed fields.");
   int changedNode=-1,changedRow=-1;
   for(int n=0;n<primary.NodeCount&&changedNode<0;n++)for(int r=0;r<primary.GetRowCopy(n).Count;r++)if(good0[n][r].Count>0&&n>good0[n][r].Get(0).Span){changedNode=n;changedRow=r;break;}
   if(changedNode<0)throw new Exception("Predecessor publication guard fixture missing.");
   foreach(int field in new[]{9,10,12,24})
   {
    var broken=CopyTables(false);var bytes=broken[changedNode][changedRow].CopyStorage();
    if(field==9||field==10)bytes[field]=255;else if(field==12)BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12),65535);else bytes.AsSpan(24,8).Clear();
    broken[changedNode][changedRow]=new(bytes,broken[changedNode][changedRow].Count);
    try{LegacyFirstOrdinaryContextPasses.FromOwnedTables(primary,broken,good8,context.BeamWidth,context.ColumnCountB0,context.ColumnCountB8,context.Contexts);throw new Exception("Malformed context tuple/current symbol/identity accepted.");}catch(InvalidDataException){}
   }
   var narrow=CopyTables(false);narrow[0]=narrow[0][..^1];try{LegacyFirstOrdinaryContextPasses.FromOwnedTables(primary,narrow,good8,context.BeamWidth,context.ColumnCountB0,context.ColumnCountB8,context.Contexts);throw new Exception("Wrong context extent accepted.");}catch(ArgumentException){}
   //Original supplied tables can change after publication; output owns copies.
   byte[] mutated=good0[changedNode][changedRow].CopyStorage();mutated[12]^=1;good0[changedNode][changedRow]=new(mutated,good0[changedNode][changedRow].Count);
   if(!published.CopyB0Storage().Zip(context.CopyB0Storage()).All(r=>r.First.Span.SequenceEqual(r.Second.Span)))throw new Exception("Context publication retained caller table aliases.");
  }
  if(calls.Count>0)using(var canceled=new CancellationTokenSource())
  {try{LegacyFirstBlankContextPasses.RunWithObserver(resource,factory,cuts,prepared,producer,table,_=>canceled.Cancel(),canceled.Token);throw new Exception("Context observer cancellation ignored.");}catch(OperationCanceledException){}}
  if(actual==1)
  {
   ushort[] symbols=[22,192,180,181,182,189,191];
   for(int test=0;test<128;test++)
   {
    var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();ushort ordinal=checked((ushort)links.GetSuccessorOrdinals(-1,ushort.MaxValue,0).Span[0]);
    int OwnCost(int n,ushort arc,ushort s,byte cat,CancellationToken token){token.ThrowIfCancellationRequested();return 100+(n*13+s*3+cat*7+test)%61;}
    for(int n=0;n<graph.NodeCount;n++)
    {
     if(n>0)ordinal=checked((ushort)links.GetSuccessorOrdinals(n-1,ordinal,0).Span[0]);
     foreach(ushort s in new[]{symbols[(test+n)%symbols.Length],symbols[(test*3+n*5+1)%symbols.Length]}.Distinct())
     {if(n==0&&s is 192 or 191)continue;byte cat=(byte)((test+n)%4);requests[n].Add(new(ordinal,0,s,cat,OwnCost(n,ordinal,s,cat,default),123));}
     if(n==0&&requests[n].Count==0)requests[n].Add(new(ordinal,0,22,0,OwnCost(n,ordinal,22,0,default),123));
    }
    LegacyFirstOrdinaryTransitionResult OwnCorrection(LegacyFirstOrdinaryTransitionRequest r,int value,CancellationToken token)
    {token.ThrowIfCancellationRequested();if(r.CurrentSymbol==192&&r.Kind==2||r.CurrentSymbol is 180 or 181 or 182&&r.Kind==0)return new(false,0x4000000);return new(true,value+(r.CurrentSymbol+r.PreviousSymbol+r.Kind+test)%9-4);}
    var own=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,factory.PrimaryBeamWidthF8,OwnCorrection,OwnCorrection);calls.Clear();
    var controlled=LegacyFirstBlankContextPasses.RunPreparedZeroTraitRows(resource,factory,cuts,prepared,producer,table,own,OwnCost,Observe);Capture(own,controlled,OwnCost,calls,true);synthetic++;
   }
  }
  void Capture(LegacyFirstOrdinaryAcousticRecurrence pri,LegacyFirstOrdinaryContextPasses ctx,LegacyFirstOrdinaryGlyphCost cost,List<int[]> observed,bool preparedControl)
  {
   w.WriteStartObject();w.WriteBoolean("synthetic",preparedControl);w.WriteBoolean("modeE0",factory.InvocationPolicy.Binding.NativeModeE0);w.WriteBoolean("excludeConsecutive",producer.NativeConsecutiveBlankExclusion);w.WriteNumber("blank",192);w.WriteNumber("closed",191);w.WriteNumber("beam",ctx.BeamWidth);w.WriteNumber("cap0",ctx.ColumnCountB0);w.WriteNumber("cap8",ctx.ColumnCountB8);
   void Arrays(string name,IEnumerable<int[]> data){w.WriteStartArray(name);foreach(var r in data){w.WriteStartArray();foreach(int v in r)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();}
   void Storage(string name,IEnumerable<ReadOnlyMemory<byte>> data){w.WriteStartArray(name);foreach(var r in data)w.WriteStringValue(Convert.ToHexString(r.Span));w.WriteEndArray();}
   Storage("primary",Enumerable.Range(0,pri.NodeCount).Select(n=>(ReadOnlyMemory<byte>)pri.GetRowCopy(n).CopyStorage()));Storage("b0",ctx.CopyB0Storage());Storage("b8",ctx.CopyB8Storage());
   Arrays("costs",Enumerable.Range(0,pri.NodeCount).SelectMany(n=>Enumerable.Range(0,pri.GetRowCopy(n).Count).Select(i=>{var r=pri.GetRowCopy(n).Get(i);return new[]{n,(int)r.ArcOrdinal,(int)r.Symbol,(int)r.CompositeCategory,cost(n,r.ArcOrdinal,r.Symbol,r.CompositeCategory,default)};})));
   var successors=new List<int[]>();for(int n=0;n<graph.NodeCount;n++)for(ushort arc=0;arc<graph.GetNode(n).Count;arc++)for(byte span=0;span<8;span++)if(n+span+1<graph.NodeCount)successors.Add(new[]{n,(int)arc,(int)span}.Concat(links.GetSuccessorOrdinals(n,arc,span).ToArray()).ToArray());Arrays("successors",successors);Arrays("calls",observed);
   w.WriteStartArray("contexts");foreach(var c in ctx.Contexts.CopyNodes()){w.WriteStartObject();w.WriteNumber("identity",c.Identity);w.WriteNumber("parent",c.ParentLink);w.WriteNumber("symbol",c.Symbol);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
  }
 }
}
w.WriteEndArray();w.Flush();if(actual==0||synthetic==0||skipped==0)throw new Exception("Real producer/synthetic context ancestors were not exercised.");
Console.WriteLine($"PASS {actual} realNN fullcontext captures +{synthetic} INTERNAL prepared controls; {skipped} skipped context-row steps. Independent tuple oracle required; actual fixtures still not Russian/cursive accuracy.");
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
