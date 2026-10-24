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
for(int mode=0;mode<11;mode++)
{
 var (scope,words)=Recognize(samples[mode<4?mode:0]);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {
  doc.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);
  Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay,new(true)));
  var preflight=LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay);var before=preflight.State;
  if(!ReferenceEquals(preflight.OriginalContext,replay.InitialContext.Parent)||ReferenceEquals(preflight.OriginalContext,replay.InitialContext)||
   !ReferenceEquals(doc.OriginalPatchCompatibility,preflight)||!ReferenceEquals(preflight.TransactionOwner,replay.TransportRow.TransactionOwner))throw new InvalidDataException("Original/selected/C4D provenance lost.");
  Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay));Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());
  Reject(()=>preflight.Run(new(true)));
  using(var other=LegacyLibraryTextPublicationScope.PrepareConfigured(models,m,0,table,cases,publication))
   Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.ValidateOriginal(replay,other.Context.SelectedInput.Parent!));
  Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.ValidateOriginal(replay,replay.InitialContext));
  var commands=replay.TransactionOwner.CopyPendingCommands();int count=commands.Count(c=>c.Selection is not null),failure=-1,incompatible=-1,cancel=-1;
  byte[] history=preflight.OriginalContext.CopyRegistryCounterStorage();
  w.WriteStartObject();w.WriteNumber("mode",mode);w.WriteBase64String("domain",Storage(preflight.CopyOriginalInkDomain()));
  w.WriteStartArray("pointCounts");foreach(var s in scope.Document.Strokes)w.WriteNumberValue(s.Points.Count);w.WriteEndArray();
  w.WriteStartArray("commands");foreach(var c in commands){w.WriteStartObject();w.WriteNumber("operation",c.OperationCode);w.WriteNumber("identity",c.CommandIdentity);if(c.Selection is{} s)w.WriteBase64String("ranges",Storage(s.CopyRanges()));w.WriteEndObject();}w.WriteEndArray();
  if(mode<4)
  {
   scope.Context.Dispose();preflight.Run();if(!preflight.State.IsReady)throw new InvalidDataException("Genuine preflight not ready.");
   replay.Run();doc.ApplyToLibraryTarget();Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay));
  }
  else if(mode is 4 or 5){failure=mode-4;preflight.RunPrepared(failureStage:failure);}
  else if(mode is 6 or 7){incompatible=mode==6?0:count/2;preflight.RunPrepared(incompatibleAtCommand:incompatible);}
  else if(mode==8){cancel=count/2;Reject(()=>preflight.RunPrepared(cancelBeforeCommand:cancel));}
  else if(mode==9){scope.Dispose();Reject(()=>preflight.Run());}
  else{doc.Dispose();Reject(()=>preflight.Run());}
  if(mode>=4){Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());}
  Reject(()=>preflight.Run());
  if(!history.AsSpan().SequenceEqual(preflight.OriginalContext.CopyRegistryCounterStorage())||before.WasRun||before.CompatibilityWasWritten)
   throw new InvalidDataException("Original historical INPUT/preflight snapshot mutated.");
  var state=preflight.State;var copy=state.CopyRawPresence();if(copy.Length>0){copy[0]=0;if(state.CopyRawPresence()[0]!=1)throw new InvalidDataException("Raw presence aliases output.");}
  w.WriteNumber("failure",failure);w.WriteNumber("incompatible",incompatible);w.WriteNumber("cancel",cancel);
  w.WriteBoolean("preentryClosed",mode>=9);w.WriteStartObject("state");w.WriteBoolean("wasRun",state.WasRun);w.WriteBoolean("worked",state.Worked);w.WriteBoolean("ready",state.IsReady);w.WriteBoolean("written",state.CompatibilityWasWritten);w.WriteBoolean("faulted",state.IsFaulted);w.WriteNumber("compatible",state.CompatibilityByte);w.WriteNumber("inspected",state.InspectedCommandCount);w.WriteBase64String("presence",state.CopyRawPresence());w.WriteBase64String("outputs",state.CopyOutputBytes());w.WriteEndObject();w.WriteEndObject();
  doc.Dispose();Reject(()=>preflight.Run());if(!ReferenceEquals(state,preflight.State))throw new InvalidDataException("Prefix lost after close.");
 }
}
w.WriteEndArray();
//The unclaimed path still works, and readiness cannot be attached post hoc.
{
 var (scope,words)=Recognize(m);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {doc.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);replay.Run();Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay));doc.ApplyToLibraryTarget();}
}
w.WriteStartArray("prepared");
for(int i=0;i<2048;i++)
{
 uint source=(uint)(i%7);ulong first=((ulong)source<<32)|(uint)((i%3)<<16)|3UL;
 ulong last=((ulong)source<<32)|((ulong)(ushort)(3+i%8)<<16)|0xff05UL;
 LegacyNativeInkPositionRange[] domain=[new(new(first),new(last))];
 LegacyNativeInkPositionRange[] request=i%13==0?[]:[new(new(first+(ulong)(i%11)*0x100UL+(ulong)((i%5)==0?0x100000000L:0)),new(last-(ulong)(i%17)*0x100UL))];
 if(i%17==0)request=[new(new(last),new(first))];
 if(i%19==0)request=[new(new(first),new(last+1))];
 bool result=LegacyLibraryOriginalInputPatchCompatibility.CheckSelection(domain,request);
 w.WriteStartObject();w.WriteBase64String("domain",Storage(domain));w.WriteBase64String("request",Storage(request));w.WriteBoolean("compatible",result);w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual original-input preflights +7 prepared/lifetime prefixes,2048 range controls/{guards} guards; no CBB/page acceptance.");
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult) Recognize(InkStroke[] raw)
{
 var scope=LegacyLibraryTextPublicationScope.PrepareConfigured(models,raw,0,table,cases,publication);
 try
 {
  var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
  var result=scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
  bool enabledAugmentationRefused=false;
 try{_ = LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);}
 catch(NotSupportedException e)when(e.GetType()==typeof(NotSupportedException)&&e.Message=="Enabled219580 candidate augmentation cannot be omitted."){enabledAugmentationRefused=true;}
 if(!enabledAugmentationRefused)throw new InvalidDataException("Exact enabled-bit8 NoCandidate refusal required before augmentation.");
 var completion=LegacyFreshMainConfiguredWordAugmentation.Prepare(ak,result,table,norm,circles,residue);completion.Run();
 if(!completion.IsTopV68Completed||completion.IsFaulted||!completion.SourcesMarkedComplete||
    completion.EligibleCandidates!=2||completion.AppendedWords!=0||completion.DuplicateWords!=2||completion.UpdatedNewDuplicates!=0||completion.QuotaAfter!=2)
  throw new InvalidDataException("Genuine fixed-fixture augmentation must complete the duplicate-only native mark/rank prefix.");
  var words=scope.PrepareAugmentedWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]);return(scope,words);
 }
 catch{scope.Dispose();throw;}
}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing preflight guard.");}
