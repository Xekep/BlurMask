using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0; LegacyLibraryTextDomTransportCache? foreignCache=null;
w.WriteStartObject();w.WriteStartArray("actual");
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
for(int index=0;index<samples.Length;index++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,samples[index],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var selected=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(configured.SourceDocument)).CreateSelectedCopy();
 var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,configured,norm,circles,new byte[4],new byte[4]);completion.Run();
 using var words=LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selected,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var text=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(choices));text.Register();
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(LegacyFreshMainDocumentLineConsistency.Apply(text))));
 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);indices.Run();var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);cleanup.Run();
 var handoff=LegacyFreshMainDocumentCounterHandoff.Apply(LegacyFreshMainDocumentInputTransforms.Apply(cleanup));var source=LegacyFreshMainSourceRangeUtf8.Prepare(handoff);
 var target=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);
 LegacyLibraryTextDomEditResult Edit(LegacyLibraryTextDomTarget t)
     =>LegacyLibraryTextDomInsertion.Prepare(LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),t).Build()).Export();
 var actualEdit=Edit(target);var commands=handoff.TransactionOwner.CopyPendingCommands();
 Reject(()=>LegacyLibraryTextDomTransfer.Prepare(actualEdit,new(true)));
 var failedDom=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),target).Build();
 var failedEdit=LegacyLibraryTextDomInsertion.Prepare(failedDom).ExportPrepared(fail:0);
 Reject(()=>LegacyLibraryTextDomTransfer.Prepare(failedEdit));
 var staleTarget=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);var stale=Edit(staleTarget);staleTarget.Apply(stale);
 Reject(()=>LegacyLibraryTextDomTransfer.Prepare(stale));
 var receipt=LegacyLibraryTextDomTransfer.Prepare(actualEdit);
 if(!ReferenceEquals(receipt.PendingEdit,actualEdit)||!ReferenceEquals(receipt.PendingTransaction,handoff.TransactionOwner)||
    !ReferenceEquals(receipt.PendingTransaction!.SelectedInputContext,selected))throw new InvalidDataException("Lost actual pending C4D/selected source identity.");
 Reject(()=>LegacyLibraryTextDomTransfer.Prepare(actualEdit));
 var cache=LegacyLibraryTextDomTransportCache.CreateFresh(target);
 w.WriteStartObject();w.WriteNumber("sample",index);w.WriteNumber("selectedTag",selected.Input.RegistryId);w.WriteNumber("initialPending",handoff.InitialPendingCounter);w.WriteNumber("finalPending",handoff.FinalPendingCounter);
 w.WriteStartArray("commands");foreach(var command in commands)
 {
  w.WriteStartObject();w.WriteNumber("operation",command.OperationCode);w.WriteNumber("id",command.CommandIdentity);w.WriteNumber("next",command.NextCommandIdentity);
  if(command.Selection is{} selection){w.WriteNumber("tag",selection.SelectionId);w.WriteString("name",selection.Name);w.WriteNumber("category",selection.Category);w.WriteBase64String("ranges16",Storage(selection.CopyRanges()));}
  else{w.WriteNumber("old",command.Counter!.OldCounter20);w.WriteNumber("new",command.Counter.NewCounter18);w.WriteBase64String("body56",command.Counter.CopyAssignedBody56());}w.WriteEndObject();
 }w.WriteEndArray();w.WriteStartArray("states");State("prepared",receipt,cache);
 var wrong=LegacyLibraryTextDomTransportCache.CreateFresh(LegacyLibraryTextDomTarget.CreateFresh(source.Seed));Reject(()=>receipt.TransferTo(wrong));wrong.Dispose();
 if(foreignCache is not null){Reject(()=>receipt.TransferTo(foreignCache));foreignCache.Dispose();}
 var disposedCache=LegacyLibraryTextDomTransportCache.CreateFresh(target);disposedCache.Dispose();Reject(()=>receipt.TransferTo(disposedCache));
 Reject(()=>receipt.TransferTo(cache,new(true)));State("cancelled-before-entry",receipt,cache);
 Reject(()=>receipt.TransferPrepared(cache,true));State("failed-reserve",receipt,cache);
 byte[]? rowStorage=null;
 if(index==3)
 {
  receipt.Dispose();receipt.Dispose();State("disposed-incoming",receipt,cache);Reject(()=>receipt.TransferTo(cache));cache.Dispose();State("disposed-cache",receipt,cache);
 }
 else
 {
  var row=receipt.TransferTo(cache);State("transferred",receipt,cache);
  if(!ReferenceEquals(row.DomEdit,actualEdit)||!ReferenceEquals(row.TransactionOwner,handoff.TransactionOwner)||cache.CopyRows().Count!=1||
      !ReferenceEquals(cache.CopyRows()[0],row)||!receipt.WasTransferred)throw new InvalidDataException("Transport row does not retain actual owners.");
  Reject(()=>receipt.TransferTo(cache));Reject(()=>LegacyLibraryTextDomTransfer.Prepare(Edit(target)));
  var raw=row.CopyScalarStorage40();if(raw.Length!=40)throw new InvalidDataException();raw[0]=0;if(row.CopyScalarStorage40()[0]!=255)throw new InvalidDataException("Mutable scalar snapshot.");
  receipt.Dispose();State("disposed-incoming",receipt,cache);
  cache.Dispose();cache.Dispose();State("disposed-cache",receipt,cache);
  Reject(()=>_=row.DomEdit);Reject(()=>_=row.TransactionOwner);Reject(()=>cache.CopyRows());
  if(!row.IsReleased||!actualEdit.Succeeded||actualEdit.Rows.Count!=1)throw new InvalidDataException("Logical release damaged borrowed actual edit.");
  rowStorage=row.CopyScalarStorage40();
 }
 w.WriteEndArray();
 if(rowStorage is null)w.WriteNull("row40");else w.WriteBase64String("row40",rowStorage);
 var after=handoff.TransactionOwner.CopyPendingCommands();
 bool same=after.Count==commands.Count&&after.Select((c,i)=>ReferenceEquals(c.Selection,commands[i].Selection)&&ReferenceEquals(c.Counter,commands[i].Counter)).All(v=>v);
 if(!same||handoff.TransactionOwner.Failed||target.Epoch!=0||target.Root.Children.Count!=0)throw new InvalidDataException("Transport mutated actual source/C20 or applied DOM edits.");
 w.WriteBoolean("sourceUnchanged",same);w.WriteNumber("targetEpoch",target.Epoch);w.WriteNumber("targetChildCount",target.Root.Children.Count);
 w.WriteEndObject();foreignCache=LegacyLibraryTextDomTransportCache.CreateFresh(target);
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual pending-C4D owners/3 transferred40B rows/4 reserve failures/{guards} guards; transport only.");
void State(string stage,LegacyLibraryTextDomTransfer receipt,LegacyLibraryTextDomTransportCache cache)
{w.WriteStartObject();w.WriteString("stage",stage);w.WriteBoolean("pendingEdit",receipt.PendingEdit is not null);w.WriteBoolean("pendingTransaction",receipt.PendingTransaction is not null);w.WriteBoolean("transferred",receipt.WasTransferred);w.WriteBoolean("disposedIncoming",receipt.IsDisposed);w.WriteNumber("count",cache.Count);w.WriteNumber("capacity",cache.Capacity);w.WriteBoolean("disposedCache",cache.IsDisposed);w.WriteEndObject();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing transport ownership guard.");}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
