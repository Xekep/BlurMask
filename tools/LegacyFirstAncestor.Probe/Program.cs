using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("AK.res separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[1]));
InkStroke[] raw=[new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)}),new(new InkPoint[]{new(65,90),new(65,10)}),new(new InkPoint[]{new(65,90),new(115,10)}),new(new InkPoint[]{new(115,90),new(115,10)}),new(new InkPoint[]{new(130,90),new(130,10),new(175,10),new(185,20),new(185,45),new(175,55),new(130,55)})];
var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var cuts=factory.Frame.Row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,factory.Frame.Row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;var connections=new LegacyOrdinaryCandidateConnections(graph);var blank=new LegacyFirstBlankTransitionGeometry(resource,factory,cuts,prepared,table);
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartArray();int walks=0,skipped=0,rootStopped=0,pairCalls=0;
ushort[] symbols=[22,192,180,181,182,189];
for(int test=0;test<256;test++)
{
 var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
 ushort arc=checked((ushort)connections.GetSuccessorOrdinals(-1,ushort.MaxValue,0).Span[0]);
 for(int n=0;n<requests.Length;n++)
 {
  if(n>0)arc=checked((ushort)connections.GetSuccessorOrdinals(n-1,arc,0).Span[0]);
  foreach(ushort symbol in new[]{symbols[(test+n)%symbols.Length],symbols[(test*3+n*5+1)%symbols.Length]}.Distinct())
   requests[n].Add(new(arc,0,symbol,(byte)((test+n)%4),100+n*3+symbol%13,123));
 }
 LegacyFirstOrdinaryTransitionResult OwnCorrection(LegacyFirstOrdinaryTransitionRequest r,int value,CancellationToken token)
 {token.ThrowIfCancellationRequested();return new(true,value+(r.PreviousSymbol+r.CurrentSymbol+test)%7-3);}
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,factory.PrimaryBeamWidthF8,OwnCorrection,OwnCorrection);
 var resolver=LegacyFirstPrimaryGeometricAncestors.Prepare(resource,factory,cuts,prepared,primary);
 if(test==0)
 {
  var wrong=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,factory.PrimaryBeamWidthF8+1,OwnCorrection,OwnCorrection);
  try{LegacyFirstPrimaryGeometricAncestors.Prepare(resource,factory,cuts,prepared,wrong);throw new Exception("Nonnative primary beam accepted.");}catch(ArgumentException){}
  try{resolver.Resolve(primary.NodeCount,0);throw new Exception("Invalid original node accepted.");}catch(ArgumentOutOfRangeException){}
  try{resolver.Resolve(0,primary.GetRowCopy(0).Count);throw new Exception("Invalid original alternative accepted.");}catch(ArgumentOutOfRangeException){}
  using var canceled=new CancellationTokenSource();canceled.Cancel();try{LegacyFirstPrimaryGeometricAncestors.Prepare(resource,factory,cuts,prepared,primary,canceled.Token);throw new Exception("Preparation cancellation ignored.");}catch(OperationCanceledException){}
 }
 w.WriteStartObject();w.WriteStartArray("rows");for(int n=0;n<primary.NodeCount;n++)w.WriteStringValue(Convert.ToHexString(primary.GetRowCopy(n).CopyStorage()));w.WriteEndArray();w.WriteStartArray("walks");
 for(int n=0;n<primary.NodeCount;n++)for(int a=0;a<primary.GetRowCopy(n).Count;a++)
 {
  var result=resolver.Resolve(n,a);walks++;skipped+=result.SkippedCount;
  if(result.GeometryNode==0&&result.GeometryRecord.Symbol is 180 or 181 or 182 or 192)rootStopped++;
  if(result.OriginalRecord!=primary.GetRowCopy(n).Get(a))throw new Exception("Ancestor walk altered original acoustic parent.");
  w.WriteStartArray();foreach(int value in new[]{n,a,result.GeometryNode,result.GeometryAlternative,result.SkippedCount,result.OriginalRecord.TotalCost,result.GeometryRecord.TotalCost})w.WriteNumberValue(value);w.WriteEndArray();
  if(n+1<primary.NodeCount)
  {
   var next=connections.GetSuccessorOrdinals(n,result.OriginalRecord.ArcOrdinal,0).Span;
   if(next.Length>0){_ = blank.Correct140(n+1,checked((ushort)next[0]),result.GeometryNode,result.GeometryRecord.ArcOrdinal,result.GeometryRecord.Symbol,result.GeometryRecord.CompositeCategory,0);pairCalls++;}
  }
  using var canceled=new CancellationTokenSource();canceled.Cancel();try{resolver.Resolve(n,a,canceled.Token);throw new Exception("Ancestor cancellation ignored.");}catch(OperationCanceledException){}
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.Flush();if(walks==0||skipped==0||rootStopped==0||pairCalls==0)throw new Exception("Skipped chains/root stops/actual pair consumer not exercised.");
Console.WriteLine($"PASS {walks} prepared first-row ancestor walks/{skipped} skipped records/{rootStopped} skipped-root stops->{pairCalls} actual owned/pruned blank140 consumers. Primary scores are explicit scalar controls; not native blank proposal/whole primary orchestration.");
