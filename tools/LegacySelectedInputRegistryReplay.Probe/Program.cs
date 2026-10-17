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
var models=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);

using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0;
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
w.WriteStartObject();w.WriteStartArray("actual");
for(int mode=0;mode<10;mode++)
{
 var (scope,words)=Recognize(samples[mode<4?mode:0]);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {
  Reject(()=>LegacyLibrarySelectedInputRegistryReplay.Prepare(doc));
  doc.BuildAndTransfer();Reject(()=>LegacyLibrarySelectedInputRegistryReplay.Prepare(doc,new(true)));
  var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);var initial=replay.State;Reject(()=>doc.ApplyToLibraryTarget());
  Reject(()=>LegacyLibrarySelectedInputRegistryReplay.Prepare(doc));Reject(()=>replay.Run(new(true)));
  if(!ReferenceEquals(doc.SelectedRegistryReplay,replay)||!ReferenceEquals(replay.Input,scope.Context.SelectedInput.Input)||
   !ReferenceEquals(replay.TransportRow.TransactionOwner,scope.Seed.PendingSelections))throw new InvalidDataException("Orphan registry/C4D/INPUT identity.");
  using(var other=LegacyLibraryTextPublicationScope.PrepareConfigured(models,m,0,table,cases,publication))
   Reject(()=>new LegacyLibrarySelectedInputRegistryReplay(doc,other,replay.TransportRow,doc.ActualSources.CounterHandoff,default));
  byte[] historical=replay.InitialContext.CopyRegistryCounterStorage();var commands=replay.TransactionOwner.CopyPendingCommands();
  int selectionCount=commands.Count(c=>c.OperationCode==4),fail=-1,cancel=-1;
  w.WriteStartObject();w.WriteNumber("mode",mode);w.WriteNumber("initialNext",replay.InitialContext.NextRegistryId);w.WriteNumber("initialMax",replay.InitialContext.MaximumRegistryId);
  w.WriteNumber("inputId",replay.Input.RegistryId);w.WriteBase64String("inputRanges",replay.Input.CopyRangeStorage());
  w.WriteStartArray("commands");foreach(var c in commands)
  {
   w.WriteStartObject();w.WriteNumber("operation",c.OperationCode);w.WriteNumber("identity",c.CommandIdentity);w.WriteNumber("next",c.NextCommandIdentity);
   if(c.Selection is{} s){w.WriteNumber("id",s.SelectionId);w.WriteString("name",s.Name);w.WriteNumber("category",s.Category);w.WriteBase64String("ranges",Storage(s.CopyRanges()));}
   else{w.WriteNumber("new",c.Counter!.NewCounter18);w.WriteNumber("old",c.Counter.OldCounter20);w.WriteBase64String("body56",c.Counter.CopyAssignedBody56());}w.WriteEndObject();
  }w.WriteEndArray();
  if(mode is >=4 and <=6){fail=mode==4?0:mode==5?selectionCount/2:selectionCount-1;replay.RunPrepared(failCloneAtCommand:fail);}
  else if(mode==7){cancel=selectionCount/2;Reject(()=>replay.RunPrepared(cancelBeforeCommand:cancel));}
  else if(mode==8){scope.Dispose();Reject(()=>replay.Run());}
  else if(mode==9){doc.Dispose();Reject(()=>replay.Run());}
  else
  {
   var state=replay.Run();if(!state.Succeeded||state.IsFaulted)throw new InvalidDataException("Actual op4/7 replay failed.");
   foreach(var s in state.Selections)
   {
    if(!ReferenceEquals(s.TargetContext,replay.InitialContext)||!ReferenceEquals(s.Owner,replay)||
     !ReferenceEquals(s.SourceCommand,commands[s.SourceCommand.CommandIdentity-1].Selection))throw new InvalidDataException("Lost actual C22 provenance.");
    var copy=s.CopyRanges();if(copy.Length>0)copy[0]=default;
    if(!s.CopyRangeStorage16().AsSpan().SequenceEqual(Storage(s.SourceCommand.CopyRanges())))throw new InvalidDataException("Range alias.");
   }
   var counter=state.CopyCounterStorage16();counter[0]^=255;
   if(state.CopyCounterStorage16()[0]==counter[0])throw new InvalidDataException("Counter alias.");
   if(doc.Snapshot.TargetRoot!.Children.Count!=0)throw new InvalidDataException("Replay applied DOM implicitly.");
   doc.ApplyToLibraryTarget();
  }
  if(mode is >=4 and <=7)Reject(()=>doc.ApplyToLibraryTarget());
  w.WriteNumber("fail",fail);w.WriteNumber("cancel",cancel);w.WriteBoolean("preentryClosed",mode is 8 or 9);
  State(replay.State);Reject(()=>replay.Run());
  if(!historical.AsSpan().SequenceEqual(replay.InitialContext.CopyRegistryCounterStorage())||initial.Selections.Count!=0||initial.CompletedCommandCount!=0)
   throw new InvalidDataException("Historical INPUT/prior state mutated.");
  var retained=replay.State;doc.Dispose();Reject(()=>replay.Run());if(!ReferenceEquals(retained,replay.State))throw new InvalidDataException("Prefix lost on disposal.");
  w.WriteEndObject();
 }
}
w.WriteEndArray();w.WriteStartArray("numeric");
for(int i=0;i<4096;i++)
{
 long maximum=i%10003,next=maximum+1+i%5,requested=(i*7919L)%100003;
 var c=LegacyLibrarySelectedInputRegistryReplay.AddExplicit(next,maximum,requested);
 w.WriteStartArray();w.WriteNumberValue(next);w.WriteNumberValue(maximum);w.WriteNumberValue(requested);w.WriteNumberValue(c.Next);w.WriteNumberValue(c.Maximum);w.WriteEndArray();
}
w.WriteEndArray();w.WriteStartArray("counterControls");
for(int i=0;i<2048;i++)
{
 long maximum=i%131,next=maximum+1+(i%7),oldValue=i%31,newValue=(i*17)%257;
 long result=LegacyLibrarySelectedInputRegistryReplay.ApplyCounter(next,maximum,oldValue,newValue);
 w.WriteStartArray();foreach(long v in new[]{next,maximum,oldValue,newValue,result})w.WriteNumberValue(v);w.WriteEndArray();
}
w.WriteEndArray();Reject(()=>LegacyLibrarySelectedInputRegistryReplay.ApplyCounter(1,0,-1,2));
Reject(()=>LegacyLibrarySelectedInputRegistryReplay.ApplyCounter(1,0,1,-1));
Reject(()=>LegacyLibrarySelectedInputRegistryReplay.AddExplicit(0,-1,-1));Reject(()=>LegacyLibrarySelectedInputRegistryReplay.AddExplicit(0,-1,long.MaxValue));
w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual active registry overlays +6 clone/cancel/close prefixes,4096 explicit ID controls/{guards} guards; no SDK acceptance.");
void State(LegacyLibrarySelectedInputRegistryState s)
{
 w.WriteStartObject("state");w.WriteBoolean("success",s.Succeeded);w.WriteBoolean("faulted",s.IsFaulted);w.WriteNumber("completed",s.CompletedCommandCount);
 w.WriteBase64String("counter16",s.CopyCounterStorage16());w.WriteStartArray("selections");foreach(var c in s.Selections){w.WriteStartObject();w.WriteNumber("command",c.SourceCommand.CommandIdentity);w.WriteNumber("id",c.RegistryId);w.WriteString("name",c.Name);w.WriteNumber("category",c.Category);w.WriteBase64String("ranges",c.CopyRangeStorage16());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult) Recognize(InkStroke[] raw)
{
 var scope=LegacyLibraryTextPublicationScope.PrepareConfigured(models,raw,0,table,cases,publication);
 try
 {
  var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
  var result=scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
  var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completion.Run();
  var words=scope.PrepareWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]);return(scope,words);
 }
 catch{scope.Dispose();throw;}
}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing replay guard.");}
