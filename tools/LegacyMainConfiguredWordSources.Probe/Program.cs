using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");

byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]));
InkStroke[][] samples=[[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
using var stream=File.Create(args[10]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();int walks=0,rows=0,guards=0,shared=0,refused=0;
var historical=context.Prepare(samples[0],0,table,cases,publication).Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
var first=LegacyFreshMainFirstCandidateCompletion.Complete(historical,new byte[400],new byte[4],new byte[4]);
var historicalSources=LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation(first);
var historicalCallbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,historicalSources.CandidateSource.ConfidenceOwner,historical.Sampled);
if(!ReferenceEquals(historicalSources.Candidate,first))throw new InvalidDataException("Historical candidate getter changed identity.");
Reject(()=>LegacyOwnedMainWordSourceCollection.PrepareConfigured(historicalSources.CandidateSource));
Reject(()=>LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,historicalSources,historicalCallbacks,new byte[64]));
foreach(var sample in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),sample,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var alternatives=configured.Alternatives.CopyActiveAlternatives();if(alternatives.Length!=2)throw new InvalidDataException("Expected two genuine retained alternatives.");
 var heads=alternatives.Select(a=>a.WordHead18).ToArray();var spanSnapshot=alternatives[0].Ownership.WordPublisher.CaptureSpan(heads[0]!,heads);
 if(spanSnapshot.SelectedNodes.Count!=2||!spanSnapshot.SelectedNodes.Contains(heads[0]!)||!spanSnapshot.SelectedNodes.Contains(heads[1]!))throw new InvalidDataException("Actual private shared SPAN did not retain both actual279 selections.");
 LegacyOwnedMainWordSourceCollection? firstSource=null;
 foreach(var alternative in alternatives)
 {
 var view=LegacyOwnedMainCandidateSource.FromConfigured(configured,alternative);
 var source=LegacyOwnedMainWordSourceCollection.PrepareConfigured(view);var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,view.ConfidenceOwner,view.Sampled);
 if(!ReferenceEquals(source.CandidateSource,view)||!ReferenceEquals(source.Document,view.SourceDocument)||source.IsTopV68Completed)throw new InvalidDataException("Lost actual configured source ownership.");
 if(firstSource is not null){if(firstSource.Word.Span!=source.Word.Span||ReferenceEquals(firstSource.Word,source.Word)||!firstSource.CopySourceStorage26().AsSpan().SequenceEqual(source.CopySourceStorage26())||!firstSource.CopyRanges().SequenceEqual(source.CopyRanges()))throw new InvalidDataException("Actual shared SPAN source exports differ.");shared++;}else firstSource=source;
 Reject(()=>_=source.Candidate);Reject(()=>LegacyOwnedMainSpanAlternativeWalk.Prepare(ak,source,callbacks,new byte[64]));Reject(()=>LegacyOwnedMainWordSourceCollection.PrepareConfigured(view,new(true)));
 byte[] requestResidue=Enumerable.Repeat((byte)0xbd,64).ToArray(),rowResidue=Enumerable.Repeat((byte)0xa7,72).ToArray();
 var walker=LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,source,callbacks,requestResidue);
 Reject(()=>LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,source,historicalCallbacks,requestResidue));
 var materialization=view.Ownership.SelectedOwner;var compact=materialization.CopyCompactStorage();var graph=callbacks.Graph;int eligible=0;
 w.WriteStartObject();w.WriteStartArray("counts");foreach(var stroke in source.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();w.WriteBase64String("source26",source.CopySourceStorage26());w.WriteStartArray("ranges");foreach(var r in source.CopyRanges()){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();w.WriteNumber("nodes",graph.NodeCount);w.WriteNumber("count",materialization.RecordCount);w.WriteBase64String("compact",compact.AsSpan(0,materialization.RecordCount*40));w.WriteBase64String("residue",rowResidue);
 w.WriteStartArray("arcs");for(int node=0;node<graph.NodeCount;node++){w.WriteStartArray();foreach(var arc in graph.GetNode(node)){w.WriteStartArray();w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("walks");
 //Exercise real end-node compact rows as numeric diagnostic inputs. Actual
 //219580 bit8 eligibility is counted separately; it is zero in these fixtures.
 //No input flags are modified to manufacture an augmentation candidate.
 for(int index=0;index<materialization.RecordCount;index++)
 {
  var record=compact.AsSpan(index*40,40);ulong key=BinaryPrimitives.ReadUInt64LittleEndian(record[8..]);int node=(int)(key&0xffffff);if((node&0x800000)!=0)node-=0x1000000;
  if((record[32]&8)!=0)eligible++;if(node!=((source.LastContext>>7)&0xffffff))continue;
  foreach(int min in new[]{-1,(source.FirstContext>>7)&0xffffff,node+1}.Distinct())
  {
   var inspect=walker.Inspect(index,min);LegacyOwnedMainSpanAlternativeWalkResult emitted;try{emitted=walker.Emit(index,min,65534,rowResidue);}catch(NotSupportedException){w.WriteStartObject();w.WriteNumber("index",index);w.WriteNumber("minimum",min);WriteResult("inspect",inspect);w.WriteBoolean("refused",true);w.WriteEndObject();refused++;continue;}
   w.WriteStartObject();w.WriteNumber("index",index);w.WriteNumber("minimum",min);WriteResult("inspect",inspect);WriteResult("emit",emitted);w.WriteEndObject();walks++;rows+=emitted.RowCount;
   byte[] copied=emitted.CopyRows72();if(copied.Length>0){byte original=copied[0];copied[0]^=255;if(emitted.CopyRows72()[0]!=original)throw new InvalidDataException("Walk row aliases returned storage.");}
  }
 }
 w.WriteEndArray();w.WriteNumber("nativeEligible",eligible);w.WriteEndObject();
 Reject(()=>walker.Inspect(-1,-1));Reject(()=>walker.Inspect(materialization.RecordCount,-1));Reject(()=>walker.Emit(0,-1,65534,new byte[71]));Reject(()=>walker.Inspect(0,-1,new CancellationToken(true)));
 Reject(()=>LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,source,callbacks,new byte[63]));Reject(()=>LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,source,callbacks,new byte[64],new(true)));
 }
 var after=alternatives[0].Ownership.WordPublisher.CaptureSpan(heads[0]!,heads);
 if(!after.SelectedNodes.SequenceEqual(spanSnapshot.SelectedNodes)||after.Words.Count!=spanSnapshot.Words.Count||after.Words.Where((word,i)=>!ReferenceEquals(word,spanSnapshot.Words[i])).Any())throw new InvalidDataException("Prefix/walk unexpectedly changed shared SPAN payload ownership.");
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS3 configured sources/6 selected owners/{shared} shared SPAN pairs/{walks} numeric walks/{rows} rows/{refused} unsupported path refusals/{guards} guards; no V68/augmentation claim.");
void WriteResult(string name,LegacyOwnedMainSpanAlternativeWalkResult result){w.WriteStartObject(name);w.WriteNumber("status",result.Status);w.WriteNumber("minimum",result.UpdatedMinimumNode);w.WriteNumber("parent",result.LastDirectParentIndex);w.WriteNumber("source",result.LastSourceKey);w.WriteNumber("wordFlags",result.WordFlags);w.WriteBase64String("rows",result.CopyRows72());w.WriteStartArray("visited");foreach(int v in result.CopyVisitedIndices())w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("residuals");foreach(ulong v in result.CopyResidualReferences())w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing source-walk guard.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
