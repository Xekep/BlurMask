using InkRecognition;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[11]);using var w=new Utf8JsonWriter(file);using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));
w.WriteStartObject();w.WriteStartArray("prepared");int guards=0,appends=0,failures=0,skips=0;
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 var doc=LegacyOwnedRawInkDocument.Create([new(new InkPoint[]{new(0,0),new(1,1)})],default);
 var context=LegacyOwnedFreshTextSourceContext.Prepare(doc);int prefix=c.GetProperty("prefix").GetInt32();
 for(int i=0;i<prefix;i++){long id=-1;context.CreatePrepared([],i%3,ref id);}
 var old=context.CopyPendingSelections().ToArray();long next=context.NextSelectionId;
 Reject(()=>context.AppendPreparedCounter(c.GetProperty("old").GetInt64(),c.GetProperty("new").GetInt64(),false,new(true)));
 var a=context.AppendPreparedCounter(c.GetProperty("old").GetInt64(),c.GetProperty("new").GetInt64(),c.GetProperty("allocationFailure").GetBoolean());
 if(a.Succeeded!=c.GetProperty("succeeded").GetBoolean()||context.CopyPendingCommands().Count!=c.GetProperty("commandCount").GetInt32()||
    !old.SequenceEqual(context.CopyPendingSelections())||context.NextSelectionId!=next)throw new InvalidDataException("Pending operation7 prefix/status differs.");
 if(a.Command is not null)
 {if(!a.Command.CopyAssignedBody56().AsSpan().SequenceEqual(c.GetProperty("body56").GetBytesFromBase64())||!ReferenceEquals(a.Command.Owner,context))throw new InvalidDataException("Native command18/20 values differ.");appends++;}
 else if(!a.Succeeded)failures++;else skips++;
 Reject(()=>context.AppendPreparedCounter(0,1));long untouched=77;Reject(()=>context.CreatePrepared([],0,ref untouched));if(untouched!=77)throw new InvalidDataException("Closed transaction guard wrote ID.");
 w.WriteStartObject();w.WriteBoolean("succeeded",a.Succeeded);w.WriteNumber("next",context.NextSelectionId);w.WriteBoolean("failed",context.Failed);WriteCommands(context.CopyPendingCommands());w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("actual");
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
 var transforms=LegacyFreshMainDocumentInputTransforms.Apply(cleanup);var before=words.CopyPendingSelections().ToArray();
 var registry=selected.CopyRegistryCounterStorage();long next=words.NextSelectionId;int strings=words.DocumentSeed!.StringPool.Count;
 Reject(()=>LegacyFreshMainDocumentCounterHandoff.Apply(transforms,new(true)));
 var foreign=LegacyOwnedFreshTextSourceContext.PrepareSelectedInput(selected);Reject(()=>foreign.AppendOwnedCounter(transforms,default));
 if(foreign.CopyPendingCommands().Count!=0)throw new InvalidDataException("Foreign transaction was modified.");
 var result=LegacyFreshMainDocumentCounterHandoff.Apply(transforms);var command=result.CounterCommand!;
 if(!ReferenceEquals(result.TransactionOwner,words.SelectionContext)||!ReferenceEquals(command.Owner,words.SelectionContext)||
    result.InitialPendingCounter!=selected.NextRegistryId||result.FinalPendingCounter!=next||command.OldCounter20!=selected.NextRegistryId||command.NewCounter18!=next||
    result.Commands.Count!=before.Length+1||!ReferenceEquals(result.Commands[^1].Counter,command)||
    !before.SequenceEqual(words.CopyPendingSelections())||!registry.AsSpan().SequenceEqual(selected.CopyRegistryCounterStorage())||
    next!=words.NextSelectionId||strings!=words.DocumentSeed.StringPool.Count)
  throw new InvalidDataException("Actual same-C4D old/final counter handoff or retained snapshot differs.");
 Reject(()=>LegacyFreshMainDocumentCounterHandoff.Apply(transforms));
 var selections=result.Commands.Where(c=>c.OperationCode==4).Select(c=>c.Selection).ToArray();if(!selections.SequenceEqual(before))throw new InvalidDataException("Complete transaction lost actual selection objects.");
 w.WriteStartObject();w.WriteNumber("sample",index);w.WriteNumber("inputNext",selected.NextRegistryId);w.WriteNumber("oldCounter",result.InitialPendingCounter);w.WriteNumber("newCounter",result.FinalPendingCounter);
 w.WriteBase64String("registryBefore",registry);w.WriteBase64String("registryAfter",selected.CopyRegistryCounterStorage());w.WriteNumber("selectionsBefore",before.Length);w.WriteNumber("selectionsAfter",words.CopyPendingSelections().Count);
 w.WriteStartArray("selectionIds");foreach(var s in before)w.WriteNumberValue(s.SelectionId);w.WriteEndArray();
 WriteCommands(result.Commands);w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS1024 prepared counter attempts/{appends} appends/{failures} retained failures/{skips} equality skips;4 actual same-C4D operation7 tails/{guards} guards; pending only, no2E7/registry execution.");
void WriteCommands(IReadOnlyList<LegacyOwnedFreshTextPendingCommand> commands)
{w.WriteStartArray("commands");foreach(var c in commands){w.WriteStartObject();w.WriteNumber("op",c.OperationCode);w.WriteNumber("id",c.CommandIdentity);w.WriteNumber("previous",c.PreviousCommandIdentity);w.WriteNumber("next",c.NextCommandIdentity);if(c.Counter is{} counter)w.WriteBase64String("body56",counter.CopyAssignedBody56());else {w.WriteNumber("selectionId",c.Selection!.SelectionId);w.WriteString("name",c.Selection.Name);w.WriteNumber("category",c.Selection.Category);}w.WriteEndObject();}w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing pending counter owner/lifetime guard.");}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
