using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 2))throw new ArgumentException("Usage: LegacyRawPacket.Probe independent-controls.json [ru-ak.res]");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,physical=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 var events=item.GetProperty("events");
 //Checkeveryappendprefix, not just a preselected caller-provided mapping.
 for(int n=1;n<=raw.Length;n++)
 {
  var packet=LegacyFreshOrdinaryRawPacket.PrepareDirect(raw[..n],new(0x135,false,0,0));var e=events[n-1];
  if(packet.DescriptorCount!=1||packet.Descriptor.FirstPhysicalStroke!=e.GetProperty("first").GetInt32()||packet.Descriptor.LastPhysicalStroke!=e.GetProperty("last").GetInt32()||packet.Descriptor.Flags!=0x10||packet.Descriptor.SourceItemId!=int.MinValue||packet.Descriptor.CurrentLineId!=-1||packet.Descriptor.PriorLineId!=-1||packet.Descriptor.ScopeId!=-1||packet.Descriptor.AuxiliaryId!=-1||packet.Descriptor.PhysicalBoundaryId!=-1||!packet.Descriptor.Horizontal||packet.Descriptor.FirstEndpointCategory!=ushort.MaxValue||packet.Descriptor.FirstX!=raw[0].Points[0].X||packet.Descriptor.FirstY!=raw[0].Points[0].Y)throw new Exception("Fresh native descriptor state differed from own append-event oracle.");
  for(int s=0;s<n;s++)
  {
   if(packet.GetDescriptorOrdinalForPhysicalStroke(s)!=events[s].GetProperty("descriptor").GetInt32()||packet.Source.Chronology.Resolve(s,0).Key.Second!=events[s].GetProperty("inputStroke").GetInt32())throw new Exception("Input descriptor identity was confused with physical pen-up chronology.");
  }
  physical+=n;
 }
 using var cancel=new CancellationTokenSource();cancel.Cancel();try{LegacyFreshOrdinaryRawPacket.PrepareDirect(raw,new(0x135,false,0,0),cancel.Token);throw new Exception("Rawpacket cancellation ignored.");}catch(OperationCanceledException){}
 cases++;
}
Console.WriteLine($"PASS {cases} independent raw append-packet fixtures/{physical} physical ownership checks; descriptor identity is separate from pen-up chronology.");
if(args.Length==2)
{
 var bytes=File.ReadAllBytes(args[1]);InkStroke[] mir=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var frame=LegacyFreshOrdinaryPacketFrame.PrepareFirstSource(bytes,6120,mir);var packet=frame.Packet;var profile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(bytes,6120);
 var row=frame.Row;var oldRow=LegacyOrdinaryRowAcousticInput.Prepare(mir,profile);var cuts=row.GenerateRussianCuts();
 if(!ReferenceEquals(packet.Source,row.OriginalSource)||frame.Descriptor.CurrentLineId!=0||packet.Descriptor.CurrentLineId!=-1||!frame.GetFrameForDescriptor(0).Equals(row.Frame))throw new Exception("Native fresh rawdescriptor->frame assignment/ingress ownership failed.");
 if(row.Frame.Line!=oldRow.Frame.Line||!row.Prepared.PointKeys200.Span.SequenceEqual(oldRow.Prepared.PointKeys200.Span)||row.Prepared.Strokes.Count!=oldRow.Prepared.Strokes.Count)throw new Exception("Owned-source row overload changed old preparation.");
 for(int s=0;s<row.Prepared.Strokes.Count;s++)if(!row.Prepared.Strokes[s].Points.SequenceEqual(oldRow.Prepared.Strokes[s].Points))throw new Exception("Owned-source row geometry changed.");
 if(!packet.Source.SourceStrokeProbabilities.Span.SequenceEqual(row.OriginalSource.SourceStrokeProbabilities.Span))throw new Exception("Raw packet descriptor producer changed original physical service probability ownership.");
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(bytes,12727),new byte[mir.Length],Enumerable.Repeat(-1,mir.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);
 foreach(var g in graph.Groups)
  if(packet.GetDescriptorOrdinalForGroup(g.Ranges.Span)!=0)throw new Exception("Actual fresh descriptor ownership was not preserved over265/297 candidate cuts.");
 if(graph.Groups.Count<5||packet.Source.Strokes.Count!=5)throw new Exception("Actual own fixture did not retain physical pen lifts.");
 try{LegacyOrdinaryRowAcousticInput.PrepareOwnedSource(packet.Source,profile with {Source=profile.Source with {SourceKind=0x136}});throw new Exception("Foreign source row profile accepted.");}catch(ArgumentException){}
 using var cancelled=new CancellationTokenSource();cancelled.Cancel();try{LegacyOrdinaryRowAcousticInput.PrepareOwnedSource(packet.Source,profile,cancelled.Token);throw new Exception("Owned source cancellation ignored.");}catch(OperationCanceledException){}
 Console.WriteLine($"PASS actual first247@6120 rawpacket->same owned ingress/probabilities->descriptor line0/24a/265/297->graph physical ranges {graph.Groups.Count} groups share proven descriptor0; chronology retains five physical ordinals. Existing row geometry/keys unchanged; continuation geometry/winner stillseparate.");
}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
