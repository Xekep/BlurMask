using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 4))throw new ArgumentException("Usage: LegacyFirstRecurrence.Probe independent-controls.json [ru-ak.res engine-profile.json separation-profile.json]");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));int tableCases=0,operations=0,recurrenceCases=0,paths=0,callback118=0,callback140=0;
foreach(var item in json.RootElement.GetProperty("tables").EnumerateArray())
{
 var table=new LegacyFirstAcousticRecordTable(Convert.FromHexString(item.GetProperty("initial").GetString()!),0);
 foreach(var operation in item.GetProperty("operations").EnumerateArray())
 {
  var f=operation.GetProperty("fields");var fields=new LegacyFirstAcousticRecordFields(f[0].GetInt32(),f[1].GetInt32(),f[2].GetByte(),f[3].GetByte(),f[4].GetUInt16(),f[5].GetByte(),f[6].GetUInt16(),f[7].GetBoolean(),f[8].GetUInt64(),f[9].GetByte(),f[10].GetByte());
  int count=table.Insert(fields);
  if(count!=operation.GetProperty("count").GetInt32()||!table.CopyStorage().AsSpan().SequenceEqual(Convert.FromHexString(operation.GetProperty("raw").GetString()!)))throw new Exception("Native signed insertion/identity update/opaque bytes differed from independent byte oracle.");
  operations++;
 }
 using var cancel=new CancellationTokenSource();cancel.Cancel();var before=table.CopyStorage();
 try{table.Insert(default,cancel.Token);throw new Exception("Record insertion cancellation ignored.");}catch(OperationCanceledException){}
 if(!before.AsSpan().SequenceEqual(table.CopyStorage()))throw new Exception("Pre-cancel insertion mutated records.");
 tableCases++;
}
foreach(var item in json.RootElement.GetProperty("cases").EnumerateArray())
{
 int n=item.GetProperty("count").GetInt32(),cap=item.GetProperty("cap").GetInt32(),width=item.GetProperty("width").GetInt32();
 var source=Source(n);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),cap);
 var links=new LegacyOrdinaryCandidateConnections(graph);var requests=item.GetProperty("requests").EnumerateArray().Select(row=>(IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>)row.EnumerateArray().Select(g=>new LegacyFirstOrdinaryGlyphRequest(g.GetProperty("arc").GetUInt16(),g.GetProperty("span").GetByte(),g.GetProperty("symbol").GetUInt16(),g.GetProperty("category").GetByte(),g.GetProperty("cost").GetInt32(),g.GetProperty("likelihood").GetByte())).ToArray()).ToArray();
 var result=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,width,Correction118,Correction140);
 for(int node=0;node<n;node++)
 {
  var expected=item.GetProperty("rows")[node];var row=result.GetRowCopy(node);
  if(row.Count!=expected.GetArrayLength())throw new Exception("Native recurrence beam count differed from exhaustive prefixes.");
  for(int i=0;i<row.Count;i++)
  {
   var e=expected[i];var request=e.GetProperty("request");var f=row.Get(i);
   if(f.TotalCost!=e.GetProperty("cost").GetInt32()||f.AcousticCost!=e.GetProperty("acoustic").GetInt32()||f.PreviousAlternative!=e.GetProperty("previous").GetByte()||f.PathBit!=e.GetProperty("pathbit").GetBoolean()||f.ArcOrdinal!=request.GetProperty("arc").GetUInt16()||f.Symbol!=request.GetProperty("symbol").GetUInt16()||f.CompositeCategory!=request.GetProperty("category").GetByte()||f.ContinuationType!=0||row.GetLikelihoodByte(i)!=request.GetProperty("likelihood").GetByte())throw new Exception("Native primary recurrence scalar fields differed from independent prefix oracle.");
   if(!result.TracePrimary(node,i).Select(v=>(int)v.Symbol).SequenceEqual(e.GetProperty("trace").EnumerateArray().Select(v=>v.GetInt32())))throw new Exception("Native packed-span/previous-alt primary traceback differed.");
   paths++;
  }
 }
 using var cancelled=new CancellationTokenSource();
 if(n>1)
 {
  try{LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,width,(r,c,t)=>{cancelled.Cancel();return new(true,c);},Correction140,cancelled.Token);throw new Exception("Callback cancellation ignored.");}catch(OperationCanceledException){}
 }
 recurrenceCases++;
}
Console.WriteLine($"PASS {tableCases} independent native40-byte insertion cases/{operations} operations, {recurrenceCases} exhaustive compatible-prefix recurrence cases/{paths} primary paths; explicit callbacks118={callback118},140={callback140}. No default geometry corrections/final SDK winner.");
{
 var source=Source(2);var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),1);
 var links=new LegacyOrdinaryCandidateConnections(graph);IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[] requests=[new[]{new LegacyFirstOrdinaryGlyphRequest(0,0,10,0,4,255)},new[]{new LegacyFirstOrdinaryGlyphRequest(0,0,11,0,5,255)}];
 var rejected=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,5,(_,_,_)=>new(false,0),(_,_,_)=>throw new Exception("Callback140 ran after118 rejected."));
 if(rejected.GetRowCopy(1).Count!=0)throw new Exception("Rejected native geometry continuation produced a record.");
 var mutation=rejected.GetRowCopy(0);mutation.Insert(new(-100,0,255,0,0,0,12,true,0,0,0));
 if(rejected.GetRowCopy(0).Get(0).Symbol!=10)throw new Exception("Row copy exposed mutable recurrence ownership.");
 var bad=(IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[])requests.Clone();bad[0]=new[]{requests[0][0],requests[0][0] with {LikelihoodByte=1}};
 try{LegacyFirstOrdinaryAcousticRecurrence.Run(links,bad,5,Correction118,Correction140);throw new Exception("Conflicting native symbol/arc likelihood accepted.");}catch(ArgumentException){}
}
if(args.Length==4)
{
 var resource=File.ReadAllBytes(args[1]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");
 var activation=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 InkStroke[] mir=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var session=LegacyFreshFirstGlyphSession.Prepare(resource,6120,mir,activation);
 var seed=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(session.Cuts.Cuts,session.Row.Frame.Line.Height,
     LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[mir.Length],Enumerable.Repeat(-1,mir.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seed,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seed),5);
 var connections=new LegacyOrdinaryCandidateConnections(graph);var mask=Enumerable.Repeat((byte)1,session.Scorer.Alphabet.LabelCount).ToArray();
 var sep=LegacyCursiveSeparationProfile.ReadV106Node(resource,11506);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
 var categories=new LegacyCursiveCategoryMapper(LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out _));
 var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();int groups=0;
 for(byte span=0;span<5&&span<graph.NodeCount;span++)
  foreach(int ordinal in connections.GetSuccessorOrdinals(-1,ushort.MaxValue,span).Span)
  {
   var arc=graph.GetNode(span)[ordinal];var ranges=graph.Groups[arc.GroupId].Ranges;var scored=session.Score(ranges.Span,0,15,mask,[]);
   int geometry=LegacyCursiveGroupSeparationCost.Evaluate(session.Cuts.Cuts.PreparedInk,ranges.Span,session.Row.Frame.Line.Height,sep.GapOffset,sep.Slope,table.Values.Span,sep.MaximumCost);
   var ranked=new LegacyOrdinaryRankedAcousticGroup(scored.Cache,session.Scorer.Alphabet,resource.AsSpan(session.Scorer.AlphabetOffset,16),Enumerable.Repeat(geometry,categories.CompositeCategoryCount).ToArray(),1);
   foreach(var entry in ranked.Entries.Span[..Math.Min(20,ranked.Entries.Length)])
   {
    //Only plain Latin/Cyrillic letters/digits: primitive and Arabic/contextual
    //branches are not silently sent through this scoped regular recurrence.
    if(!session.Scorer.Alphabet.TryGetText(entry.Symbol,out string text)||text.Length!=1||
        !(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))continue;
    int category=categories.GetSymbolCompositeCategory(entry.Symbol);if(category<0)continue;
    int cost=ranked.Query(entry.Symbol,ushort.MaxValue,category,false,false);
    requests[span].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,cost,ranked.GetFirstControllerLikelihoodByte(entry.Symbol)));
   }
   groups++;
  }
 var result=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,5,Unknown,Unknown);
 int finite=0;for(int node=0;node<result.NodeCount;node++)
 {
  var row=result.GetRowCopy(node);for(int i=0;i<row.Count;i++)
  {
   var path=result.TracePrimary(node,i);if(path.Length!=1)throw new Exception("Actual scoped source-root check unexpectedly entered a continuation callback.");
   if(!requests[node].Any(r=>r.Symbol==path[0].Symbol&&r.ArcOrdinal==path[0].ArcOrdinal&&r.GroupCost==path[0].TotalCost))throw new Exception("Actual native record lost resource-owned glyph cost.");finite++;
  }
 }
 if(groups<5||finite==0)throw new Exception("Actual first-model source-root score did not reach native records.");
 Console.WriteLine($"PASS actual owned247/24a/265/297->firstNN/raw180/final195->ranked->graph50/58->native40-byte initial records: {groups} variable source-root groups/{finite} records; category geometry=explicit separation diagnostic, no continuation correction/default winner claim.");
}
LegacyFirstOrdinaryTransitionResult Correction118(LegacyFirstOrdinaryTransitionRequest r,int incoming,CancellationToken token)
{
 token.ThrowIfCancellationRequested();callback118++;
 if(incoming!=0)throw new Exception("Native plain trait branch initial correction was not zero.");
 if((r.CurrentSymbol+r.PreviousSymbol+r.Kind+r.CurrentNode)%11==0)return new(false,0);
 return new(true,(r.CurrentSymbol*13+r.PreviousSymbol*7+r.CurrentCategory+r.Kind*9)%51-25);
}
LegacyFirstOrdinaryTransitionResult Correction140(LegacyFirstOrdinaryTransitionRequest r,int incoming,CancellationToken token)
{
 token.ThrowIfCancellationRequested();callback140++;
 if((r.CurrentCategory+r.PreviousCategory+r.Kind)%13==12)return new(false,0);
 return new(true,incoming+(r.CurrentNode+r.PreviousArc*3+r.CurrentArc*5+r.Kind*4)%23-11);
}
static LegacyOrdinaryCursiveSeedResult Source(int count)
{
 var raw=Enumerable.Range(0,count).Select(i=>new InkStroke(new[]{new InkPoint(i*100,10),new InkPoint(i*100+20,90)})).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,count).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 return LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(true,0,.3f,.3f),new byte[count],Enumerable.Repeat(-1,count).ToArray());
}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static LegacyFirstOrdinaryTransitionResult Unknown(LegacyFirstOrdinaryTransitionRequest request,int incoming,CancellationToken token)
    =>throw new NotSupportedException("Native continuation geometry producer is deliberately not guessed in this source-root diagnostic.");
