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
  Reject(()=>LegacyLibraryTextDomPatchCompatibility.Prepare(doc));doc.BuildAndTransfer();Reject(()=>LegacyLibraryTextDomPatchCompatibility.Prepare(doc,new(true)));
  var service=LegacyLibraryTextDomPatchCompatibility.Prepare(doc);var before=service.State;
  var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);var input=LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay);
  if(!ReferenceEquals(doc.DomPatchCompatibility,service)||!ReferenceEquals(service.TransactionOwner,replay.TransactionOwner)||
    !ReferenceEquals(service.Edit,replay.TransportRow.DomEdit)||!ReferenceEquals(service.Seed,scope.Seed))throw new InvalidDataException("DOM/Edit/C4D identity lost.");
  Reject(()=>LegacyLibraryTextDomPatchCompatibility.Prepare(doc));Reject(()=>service.Run(new(true)));Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());
  input.Run();Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());
  if(mode==0){using var foreign=LegacyLibraryTextPublicationScope.PrepareConfigured(models,m,0,table,cases,publication);Reject(()=>new LegacyLibraryTextDomPatchCompatibility(doc,foreign,service.TransportRow,default));}
  var targetBefore=service.Target.Root;var epoch=service.Target.Epoch;var registryBefore=replay.InitialContext.CopyRegistryCounterStorage();
  w.WriteStartObject();w.WriteNumber("mode",mode);w.WritePropertyName("targetBefore");Tree(targetBefore);
  w.WriteStartArray("rows");foreach(var row in service.Edit.Rows)Row(row.Operation,(int)row.ChildOrdinal,row.Path.ToArray(),row.Payload);w.WriteEndArray();
  if(mode<4)
  {
   scope.Context.Dispose();service.Run();if(!service.State.IsReady)throw new InvalidDataException("Actual DOM preflight failed.");
   if(service.Target.Epoch!=epoch||service.Target.Root.Children.Count!=0||service.Target.IsFaulted||!registryBefore.AsSpan().SequenceEqual(replay.InitialContext.CopyRegistryCounterStorage()))throw new InvalidDataException("Scratch preflight mutated target/registry.");
   if(mode==0)doc.WithRegistryReplay(replay,()=>{if(Task.Run(()=>service.Target.CanEnterOperationPrepared()).GetAwaiter().GetResult())throw new InvalidDataException("Replay consumption dropped target gate.");guards++;return 0;},default);
   replay.Run();doc.ApplyToLibraryTarget();if(doc.Snapshot.TargetRoot!.Children.Count!=1)throw new InvalidDataException("Ready result was not consumed.");
  }
  else if(mode==4)service.RunPrepared(new(1,-1,-1,-1));
  else if(mode==5)service.RunPrepared(new(2,-1,-1,-1));
  else if(mode==6)Reject(()=>service.RunPrepared(new(0,-1,0,-1)));
  else if(mode==7)Reject(()=>service.RunPrepared(new(0,-1,-1,0)));
  else if(mode==8){scope.Dispose();Reject(()=>service.Run());}
  else{doc.Dispose();Reject(()=>service.Run());}
  if(mode>=4){Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());}
  Reject(()=>service.Run());
  if(before.WasRun||before.CompatibilityWasWritten)throw new InvalidDataException("Historical state mutated.");
  w.WritePropertyName("state");State(service.State);w.WritePropertyName("targetAfter");Tree(service.Target.Root);
  if(mode==0){Replacement(doc.ActualSources.CounterHandoff);Prepared(scope.Seed);}w.WriteEndObject();
 }
}
w.WriteEndArray();
//Unclaimed behavior remains available; no late compatibility claim.
{
 var(scope,words)=Recognize(m);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {doc.BuildAndTransfer();var r=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);r.Run();Reject(()=>LegacyLibraryTextDomPatchCompatibility.Prepare(doc));doc.ApplyToLibraryTarget();}
}
//Both separately claimed compatibility outputs must be ready.
{
 var(scope,words)=Recognize(m);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {doc.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);var original=LegacyLibraryOriginalInputPatchCompatibility.Prepare(replay);var d=LegacyLibraryTextDomPatchCompatibility.Prepare(doc);original.RunPrepared(failureStage:0);d.Run();if(!d.State.IsReady)throw new InvalidDataException();Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());}
}
//A direct target operation is a different explicit API; it makes the captured
//document edit stale instead of allowing a checksum to authorize a new epoch.
{
 var(scope,words)=Recognize(m);using(scope)using(var doc=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {doc.BuildAndTransfer();var d=LegacyLibraryTextDomPatchCompatibility.Prepare(doc);d.Run();var replay=LegacyLibrarySelectedInputRegistryReplay.Prepare(doc);d.Target.Apply(d.Edit);Reject(()=>d.Run());Reject(()=>replay.Run());Reject(()=>doc.ApplyToLibraryTarget());if(replay.HasEntered)throw new InvalidDataException("Stale compatibility consumed replay.");}
}
w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual DOM preflights +6 allocation/undefined/cancellation/lifetime prefixes,2048 prepared tree controls/{guards} guards; no page acceptance.");

void Replacement(LegacyFreshMainDocumentCounterHandoff handoff)
{
 var target=LegacyLibraryTextDomTarget.CreateFresh(handoff.Transforms.Seed);
 LegacyLibraryTextDomEditResult Export()=>LegacyLibraryTextDomInsertion.Prepare(LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),target).Build()).Export();
 target.Apply(Export());var edit=Export();var before=target.Root;
 var rows=edit.OwnedRows.Select(r=>new LegacyPreparedDomPatchRow(r.Operation,(int)r.ChildOrdinal,r.Path.ToArray(),edit.Source.Arena,r.LivePayload)).ToArray();
 var result=target.WithPatchPreflight(edit,(a,r)=>LegacyPreparedDomPatchPreflight.Run(a,r,rows,LegacyPreparedDomPatchFault.None,default),default);
 if(!result.Worked||result.Compatible!=1||result.InitialChildrenCopied!=1||target.Root.Children.Count!=1||target.Epoch!=1)throw new InvalidDataException("Actual replacement scratch copy mutated its source.");
 w.WriteStartObject("actualLibraryReplacement");w.WritePropertyName("target");Tree(before);w.WriteStartArray("rows");foreach(var row in edit.Rows)Row(row.Operation,(int)row.ChildOrdinal,row.Path.ToArray(),row.Payload);w.WriteEndArray();w.WritePropertyName("result");Result(result);
 var foreign=LegacyLibraryTextDomTarget.CreateFresh(handoff.Transforms.Seed);Reject(()=>foreign.WithPatchPreflight(edit,(a,r)=>0,default));
 target.Apply(edit);w.WritePropertyName("after");Tree(target.Root);w.WriteEndObject();Reject(()=>target.WithPatchPreflight(edit,(a,r)=>0,default));
}
void Prepared(LegacyFreshMainTextDocumentSeed seed)
{
 w.WriteStartArray("prepared");
 for(int i=0;i<2048;i++)
 {
  var arena=new LegacyLibraryTextDomArena(seed);var root=arena.ConstructorRoot();int count=i%4;
  for(int n=0;n<count;n++){var e=arena.Element("child"+n)!;arena.StringAttribute(e,"a",Encoding.UTF8.GetBytes("value"+n));arena.Append(e,arena.Text(Encoding.UTF8.GetBytes("ink"+n))!);arena.Append(root,e);}
  var payloadArena=new LegacyLibraryTextDomArena(seed,arena.NamePool);var payload=payloadArena.Element("payload")!;
  payloadArena.I32(payload,"raw",unchecked((int)0xABCD0080u+i));payloadArena.Append(payload,payloadArena.Text(Encoding.UTF8.GetBytes(i%2==0?"literal<raw>&":"long literal text at threshold17"))!);
  LegacyPreparedDomPatchRow R(uint op,int ordinal,params uint[] path)=>new(op,ordinal,path,payloadArena,payload);
  LegacyPreparedDomPatchRow[] rows=(i%12) switch
  {
   0=>[R(2,count)],1=>[R(2,0)],2=>[R(4,0)],3=>[R(2,count+1)],4=>[R(2,0,0)],
   5=>[R(4,0),R(2,0,0)],6=>[R(4,0),R(2,0)],7=>[R(2,0),R(2,0,0)],
   8=>[R(2,-1)],9=>[],10=>[R(4,-1)],_=>[R(2,0),R(2,0)]
  };
  var fault=(i%64) switch{0=>new LegacyPreparedDomPatchFault(1,-1,-1,-1),1=>new(2,-1,-1,-1),2=>new(0,0,-1,-1),3=>new(0,-1,0,-1),4=>new(0,-1,-1,1),_=>LegacyPreparedDomPatchFault.None};
  w.WriteStartObject();w.WriteNumber("index",i);w.WritePropertyName("target");Tree(arena.Snapshot(root));w.WriteStartArray("rows");foreach(var row in rows)Row(row.Operation,row.ChildOrdinal,row.Path,row.PayloadArena.Snapshot(row.Payload));w.WriteEndArray();
  w.WriteStartArray("fault");w.WriteNumberValue(fault.Stage);w.WriteNumberValue(fault.InitialChild);w.WriteNumberValue(fault.PayloadRow);w.WriteNumberValue(fault.CancelBeforeRow);w.WriteEndArray();
  var result=LegacyPreparedDomPatchPreflight.Run(arena,root,rows,fault,default);w.WritePropertyName("result");Result(result);w.WriteEndObject();
  if(arena.Snapshot(root).Children.Count!=count)throw new InvalidDataException("Prepared input was mutated.");
 }
 w.WriteEndArray();
 //Foreign exact Edit/Target check, no prepared caller identities.
 var target=LegacyLibraryTextDomTarget.CreateFresh(seed);var other=LegacyLibraryTextDomTarget.CreateFresh(seed);
 var a=new LegacyLibraryTextDomArena(seed);var rootNode=a.ConstructorRoot();var p=a.Element("x")!;
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(a,rootNode,[new(2,0,[],new LegacyLibraryTextDomArena(seed),p)],default));
 var deep=a.Element("deep")!;var current=deep;for(int i=0;i<129;i++){var child=a.Element("deep")!;a.Append(current,child);current=child;}
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(a,rootNode,[new(2,0,[],a,deep)],default));
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(a,rootNode,[new(7,0,[],a,p)],default));
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(a,rootNode,Enumerable.Repeat(new LegacyPreparedDomPatchRow(2,0,[],a,p),LegacyPreparedDomPatchPreflight.MaximumRows+1).ToArray(),default));
 var oversized=new LegacyLibraryTextDomArena(seed);var hugeRoot=oversized.ConstructorRoot();for(int i=0;i<LegacyPreparedDomPatchPreflight.MaximumNodes;i++)oversized.Append(hugeRoot,oversized.Text([])!);
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(oversized,hugeRoot,[],default));
 var largeText=a.Text(new byte[LegacyPreparedDomPatchPreflight.MaximumValueBytes+1].Select(_=>(byte)'x').ToArray())!;
 Reject(()=>LegacyPreparedDomPatchPreflight.Validate(a,rootNode,[new(2,0,[],a,largeText)],default));
}
void Row(uint op,int ordinal,uint[] path,LegacyLibraryTextDomNode payload)
{w.WriteStartObject();w.WriteNumber("operation",op);w.WriteNumber("ordinal",ordinal);w.WriteStartArray("path");foreach(var s in path)w.WriteNumberValue(s);w.WriteEndArray();w.WritePropertyName("payload");Tree(payload);w.WriteEndObject();}
void State(LegacyLibraryTextDomPatchCompatibilityState s)
{w.WriteStartObject();w.WriteBoolean("wasRun",s.WasRun);w.WriteBoolean("worked",s.Worked);w.WriteNumber("compatible",s.CompatibilityByte);w.WriteBoolean("written",s.CompatibilityWasWritten);w.WriteBoolean("ready",s.IsReady);w.WriteBoolean("faulted",s.IsFaulted);w.WriteNumber("initial",s.InitialChildrenCopied);w.WriteNumber("mapped",s.MappedRowCount);w.WriteNumber("simulated",s.SimulatedRowCount);w.WriteBoolean("uncheckedClone",s.RefusedUncheckedClone);w.WritePropertyName("tree");Value(s.DiagnosticScratchTree);w.WriteStartArray("trace");foreach(var t in s.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteEndObject();}
void Result(LegacyPreparedDomPatchResult s)
{w.WriteStartObject();w.WriteBoolean("worked",s.Worked);w.WriteNumber("compatible",s.Compatible);w.WriteBoolean("written",s.Written);w.WriteNumber("initial",s.InitialChildrenCopied);w.WriteNumber("mapped",s.MappedRows);w.WriteNumber("simulated",s.SimulatedRows);w.WriteBoolean("uncheckedClone",s.RefusedUncheckedClone);w.WriteBoolean("canceled",s.Canceled);w.WritePropertyName("tree");Value(s.Tree);w.WriteEndObject();}
void Tree(LegacyLibraryTextDomNode? n)
{if(n is null){w.WriteNullValue();return;}w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Value.NativeTag);w.WriteBase64String("value",n.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)Tree(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)Tree(c);w.WriteEndArray();w.WriteEndObject();}
void Value(LegacyLibraryTextDomTreeValue? n)
{if(n is null){w.WriteNullValue();return;}w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.NativeTag);w.WriteBase64String("value",n.CopyValueBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)Value(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)Value(c);w.WriteEndArray();w.WriteEndObject();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing DOM preflight guard.");}

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

