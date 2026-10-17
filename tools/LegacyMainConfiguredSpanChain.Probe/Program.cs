using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=11&&!(args.Length==13&&args[11]=="--costs"&&args[12]=="true"))throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture [--costs true]");
bool applyCosts=args.Length==13;
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]));
InkStroke[][] samples=[[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
using var stream=File.Create(args[10]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();int count=0,created=0,reused=0,guards=0;
LegacyFreshMainPlainResultOwnership? previousTop=null;LegacyOwnedMainSpanAlternativeWalkResult? previousEmission=null;
foreach(var sample in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),sample,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var alternatives=configured.Alternatives.CopyActiveAlternatives();if(alternatives.Length!=2)throw new InvalidDataException("Expected actual two alternatives.");
 foreach(var alternative in alternatives)
 {
 var view=LegacyOwnedMainCandidateSource.FromConfigured(configured,alternative);var sources=LegacyOwnedMainWordSourceCollection.PrepareConfigured(view);
 var compactOwner=view.Ownership.SelectedOwner;var confidence=view.ConfidenceOwner;var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,view.Sampled);
 var walker=LegacyOwnedMainSpanAlternativeWalk.PrepareConfigured(ak,sources,callbacks,new byte[64]);var executor=LegacyOwnedMainSpanAlternativeChain.PrepareConfigured(ak,walker,table);
 if(!ReferenceEquals(executor.CandidateSource,view)||!ReferenceEquals(executor.TopOwner,view.TopOwner))throw new InvalidDataException("Configured chain lost its actual selected owner.");
 Reject(()=>LegacyOwnedMainSpanAlternativeChain.Prepare(ak,walker,table));Reject(()=>LegacyOwnedMainSpanAlternativeChain.PrepareConfigured(ak,walker,table,new(true)));
 var graph=callbacks.Graph;byte[] compact=compactOwner.CopyCompactStorage();int[] starts=compactOwner.CopyNodeStarts2478();
 var selected=new List<LegacyOwnedMainSpanAlternativeWalkResult>();int eligible=0;
 for(int i=0;i<compactOwner.RecordCount;i++)
 {
  var c=compact.AsSpan(i*40,40);if((c[32]&8)!=0)eligible++;
  ulong key=BinaryPrimitives.ReadUInt64LittleEndian(c[8..]);if((key&0xffffff)!=(uint)((sources.LastContext>>7)&0xffffff))continue;
  var emission=walker.Emit(i,-1,65534,Enumerable.Repeat((byte)0x6d,72).ToArray());
  if(emission.RowCount==0||emission.Status==1||emission.CopyResidualReferences().Length!=0)continue;
  byte[] rows=emission.CopyRows72();if(Enumerable.Range(0,emission.RowCount).Any(n=>rows[n*72+60]!=0))continue;
  selected.Add(emission);if(selected.Count==8)break;
 }
 eligible=Enumerable.Range(0,compactOwner.RecordCount).Count(i=>(compact[i*40+32]&8)!=0);
 if(selected.Count==0||eligible!=0)throw new InvalidDataException("Expected scoped numeric diagnostic availability differs.");
 w.WriteStartObject();w.WriteNumber("nativeEligible",eligible);w.WriteStartArray("starts");foreach(int s in starts)w.WriteNumberValue(s);w.WriteEndArray();
 if(applyCosts){w.WriteBoolean("costTransfer",true);w.WriteBase64String("compact",compact.AsSpan(0,compactOwner.RecordCount*40));w.WriteNumber("firstContext",sources.FirstContext);w.WriteNumber("lastContext",sources.LastContext);w.WriteNumber("sourceKind",view.SourcePolicy.Model.SourceKind);}
 w.WriteStartArray("graph");for(int node=0;node<graph.NodeCount;node++){w.WriteStartArray();foreach(var arc in graph.GetNode(node))w.WriteNumberValue(arc.GroupId);w.WriteEndArray();}w.WriteEndArray();
 var groupIds=new SortedSet<int>();foreach(var emission in selected){var rows=emission.CopyRows72();for(int i=0;i<rows.Length;i+=72)groupIds.Add(graph.GetNode(BinaryPrimitives.ReadInt32LittleEndian(rows.AsSpan(i)))[BinaryPrimitives.ReadUInt16LittleEndian(rows.AsSpan(i+4))].GroupId);}
 var separation=LegacyCursiveSeparationProfile.ReadV106Node(ak,188157);var owner=view.SourcePolicy.Controller.Owner;
 w.WriteStartArray("groups");foreach(int g in groupIds)
 {
  var scores=confidence.ScoreGroup(g);int geometry=LegacyCursiveGroupSeparationCost.Evaluate(owner.SourceInk,graph.Groups[g].Ranges.ToArray(),owner.SourceHeight,separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost);
  w.WriteStartObject();w.WriteNumber("id",g);w.WriteNumber("geometry",geometry);w.WriteNumber("fallback",scores.ActualAcousticScores.Cache.FallbackCost);w.WriteStartArray("candidates");foreach(var c in scores.Candidates.Span){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToUInt32Bits(c.Confidence));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
 }w.WriteEndArray();
 w.WriteStartArray("cases");foreach(var emission in selected)
 {
  byte[] allocator=Enumerable.Range(0,120).Select(i=>(byte)(i*13+17)).ToArray();foreach(int o in new[]{32,88,104})allocator.AsSpan(o,8).Clear();foreach(int o in new[]{96,112})allocator.AsSpan(o,4).Clear();
  var word=executor.TopOwner.CreateWorkingWord(allocator);word.ApplySpanAlternativeFlags30(emission.WordFlags);
  var wrongFlags=executor.TopOwner.CreateWorkingWord(allocator);wrongFlags.ApplySpanAlternativeFlags30((ushort)(emission.WordFlags^1));Reject(()=>executor.Apply(emission,wrongFlags,new byte[40],new ushort[256],new byte[64]));
  if(previousTop is not null&&!ReferenceEquals(previousTop,executor.TopOwner)){var foreign=previousTop.CreateWorkingWord(allocator);foreign.ApplySpanAlternativeFlags30(emission.WordFlags);Reject(()=>executor.Apply(emission,foreign,new byte[40],new ushort[256],new byte[64]));}
  if(previousEmission is not null)Reject(()=>executor.Apply(previousEmission,word,new byte[40],new ushort[256],new byte[64]));
  var before=executor.TopOwner.GlyphPool.CopyOwnerNodes();byte[] beforeWord=word.CopyStorage(),recordResidue=Enumerable.Range(0,40).Select(i=>(byte)(i*7+23)).ToArray();
  Reject(()=>executor.Apply(emission,word,new byte[39],new ushort[256],new byte[64]));Reject(()=>executor.Apply(emission,word,recordResidue,new ushort[256],new byte[64],new CancellationToken(true)));
  var result=executor.Apply(emission,word,recordResidue,new ushort[256],new byte[64]);if(!result.Succeeded||!ReferenceEquals(result.Word,word)||!ReferenceEquals(result.Walk,emission))throw new InvalidDataException("Owned numeric chain failed.");
  if(applyCosts)
  {
   _=walker.ReadWordCosts(emission);long allocated=GC.GetAllocatedBytesForCurrentThread();
   for(int check=0;check<1024;check++)_=walker.ReadWordCosts(emission);
   if(GC.GetAllocatedBytesForCurrentThread()-allocated>16384)throw new InvalidDataException("Cost transfer copied compact allocation instead of retaining the exact walk snapshot.");
   Reject(()=>LegacyOwnedMainSpanAlternativeCosts.Apply(result,new(true)));LegacyOwnedMainSpanAlternativeCosts.Apply(result);Reject(()=>LegacyOwnedMainSpanAlternativeCosts.Apply(result));
  }
  var repeat=executor.TopOwner.CreateWorkingWord(allocator);repeat.ApplySpanAlternativeFlags30(emission.WordFlags);Reject(()=>executor.Apply(emission,repeat,recordResidue,new ushort[256],new byte[64]));
  w.WriteStartObject();w.WriteBase64String("rows",emission.CopyRows72());w.WriteBase64String("residue",recordResidue);w.WriteBase64String("beforeWord",beforeWord);WritePool("beforePool",before);w.WriteBase64String("afterWord",word.CopyStorage());WritePool("afterPool",executor.TopOwner.GlyphPool.CopyOwnerNodes());
  w.WriteNumber("firstNew",result.Chain.FirstNewReference);w.WriteNumber("last",result.Chain.LastProcessedReference);w.WriteNumber("created",result.Chain.CreatedCount);w.WriteNumber("reused",result.Chain.ReusedCount);
  if(applyCosts){w.WriteNumber("costCandidate",emission.StartingRecordIndex);w.WriteNumber("costParent",emission.LastDirectParentIndex);}
  w.WriteEndObject();count++;created+=result.Chain.CreatedCount;reused+=result.Chain.ReusedCount;
 }w.WriteEndArray();w.WriteEndObject();previousTop=executor.TopOwner;previousEmission=selected[^1];
 }
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS3 configured inputs/6 selected owners/{count} numeric chain connections/{created} created/{reused} reused/{guards} guards; zero native219580 eligibility, no append/winner claim.");
void WritePool(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var node in nodes){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing chain guard.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
