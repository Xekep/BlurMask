using InkRecognition;
using System.Text.Json;
if(args.Length is not(9 or 11))throw new ArgumentException("AK LK engine separation case publication line30 line50 capture [--recorded manifest]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
byte[] line30=File.ReadAllBytes(args[6]),line50=File.ReadAllBytes(args[7]);
InkStroke[][] samples=[Word(0),Word(0).Concat(Word(230)).ToArray(),Word(0).Concat(Word(300)).ToArray(),Word(0).Concat(Word(450)).ToArray(),Word(0).Concat(Word(700)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Word(0).Select(s=>new InkStroke(s.Points.Concat(Enumerable.Repeat(s.Points[^1],3)).ToArray())).ToArray()];
if(args.Length==11){if(args[9]!="--recorded")throw new ArgumentException("Unknown input mode.");samples=RecordedInputs.Load(args[10]).Samples;}
if(samples.Length is not(7 or 9))throw new InvalidDataException("Nonempty original seven/nine input corpus required.");
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);int guards=0,activeRows=0,completedInputs=0,refusedInputs=0;
const string EnabledMessage="Enabled219580 candidate augmentation needs its actual218A30/218560/WORD owner; no candidate is discarded.";
using var stream=File.Create(args[8]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteString("scope","ACTUAL_NO_CANDIDATE_TWO_DOMAIN_TEST_NO_DISCARDED_BIT8");w.WriteString("inputMode",args.Length==11?"recorded":"generated-seven");w.WriteStartArray("inputs");
for(int input=0;input<samples.Length;input++)
{
    int guardsBefore=guards;
    var raw=context.Prepare(samples[input],0,table,cases,publication).Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
    var candidate=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
    var source=LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation(candidate);
    byte[] before=source.Word.Word.CopyStorage();ulong[] references=source.Word.Word.FlatReferences.ToArray();
    byte[] sourceBefore=source.CopySourceStorage26();int sourceCountBefore=source.ReferenceCount;
    int wordIndexBefore=source.Word.WordIndex;var headBefore=raw.Session.Ownership.WordPublisher.Head;
    byte[] compact=raw.Session.Ownership.Materialization.CopyCompactStorage();int count=raw.Session.Ownership.Materialization.RecordCount;
    if(count<1||compact.Length<count*40)throw new InvalidDataException("Complete nonempty actual compact required.");
    int bit8=0;for(int i=0;i<count;i++)if((compact[i*40+32]&8)!=0)bit8++;
    Reject(()=>LegacyFreshMainNoCandidateSourceCompletion.Prepare(new byte[ak.Length],source));
    LegacyFreshMainNoCandidateSourceCompletion? owner=null;
    var refusals=new List<Exception>(2);
    int sourceRunCalls=0;
    if(bit8>0)
    {
        for(int repeat=0;repeat<2;repeat++)
        {
            var error=RefuseEnabled(()=>owner=LegacyFreshMainNoCandidateSourceCompletion.Prepare(ak,source));
            refusals.Add(error);if(owner is not null)throw new InvalidDataException("Unsupported Prepare returned a completion owner.");
        }
        refusedInputs++;
    }
    else
    {
        var completedOwner=LegacyFreshMainNoCandidateSourceCompletion.Prepare(ak,source);owner=completedOwner;
        var sibling=LegacyFreshMainNoCandidateSourceCompletion.Prepare(ak,source);
        sourceRunCalls++;Reject(()=>completedOwner.Run(new(true)));
        sourceRunCalls++;completedOwner.Run();sourceRunCalls++;Reject(()=>completedOwner.Run());sourceRunCalls++;Reject(()=>sibling.Run());
        if(!owner.IsTopV68Completed||owner.IsFaulted||!owner.SourcesMarkedComplete||!owner.RankingApplied||
            !owner.SpanScoresApplied||!owner.CheckboxGuardVerified||source.IsTopV68Completed||
            !source.Word.Word.FlatReferences.SequenceEqual(references)||source.Word.WordIndex!=0)
            throw new InvalidDataException("Bounded source completion lost actual phase/provenance state.");
        guards++;completedInputs++;
    }
    if(!ReferenceEquals(headBefore,raw.Session.Ownership.WordPublisher.Head)||source.Word.Next is not null||
       source.IsTopV68Completed||source.Word.WordIndex!=wordIndexBefore||
       !source.Word.Word.FlatReferences.SequenceEqual(references)||source.ReferenceCount!=sourceCountBefore||
       !sourceBefore.AsSpan().SequenceEqual(source.CopySourceStorage26()))
        throw new InvalidDataException("Completion/refusal changed source-prefix ownership or references.");
    if(bit8>0&&!before.AsSpan().SequenceEqual(source.Word.Word.CopyStorage()))
        throw new InvalidDataException("Unsupported positive-bit8 Prepare changed the WORD.");
    activeRows+=count;
    w.WriteStartObject();w.WriteNumber("input",input);w.WriteString("branch",bit8>0?"positive-bit8-refused":"zero-bit8-completed");w.WriteNumber("bit8Rows",bit8);
    w.WriteNumber("count",count);w.WriteBase64String("compact",compact.AsSpan(0,count*40));
    w.WriteBase64String("before",before);w.WriteBase64String("after",source.Word.Word.CopyStorage());
    w.WriteBase64String("sources26",sourceBefore);w.WriteBase64String("sourcesAfter26",source.CopySourceStorage26());
    w.WriteNumber("sourceCount",sourceCountBefore);w.WriteNumber("sourceCountAfter",source.ReferenceCount);
    w.WriteBoolean("completionObjectCreated",owner is not null);w.WriteBoolean("completed",owner?.IsTopV68Completed??false);
    w.WriteNumber("sourceRunCalls",sourceRunCalls);w.WriteNumber("validResourcePrepareCalls",2);w.WriteNumber("guards",guards-guardsBefore);
    w.WriteNumber("wordIndexBefore",wordIndexBefore);w.WriteNumber("wordIndexAfter",source.Word.WordIndex);
    w.WriteBoolean("headUnchanged",ReferenceEquals(headBefore,raw.Session.Ownership.WordPublisher.Head));w.WriteBoolean("sourcePrefixTopCompleted",source.IsTopV68Completed);
    w.WriteStartArray("references");foreach(ulong value in references)w.WriteNumberValue(value);w.WriteEndArray();
    w.WriteStartArray("afterReferences");foreach(ulong value in source.Word.Word.FlatReferences)w.WriteNumberValue(value);w.WriteEndArray();
    w.WriteStartArray("refusals");foreach(var e in refusals){w.WriteStartObject();w.WriteString("type",e.GetType().FullName);w.WriteString("message",e.Message);w.WriteEndObject();}w.WriteEndArray();
    w.WritePropertyName("completion");if(owner is null)w.WriteNullValue();else{w.WriteStartObject();w.WriteBoolean("sourcesMarked",owner.SourcesMarkedComplete);w.WriteBoolean("ranking",owner.RankingApplied);w.WriteBoolean("spanScores",owner.SpanScoresApplied);w.WriteBoolean("checkbox",owner.CheckboxGuardVerified);w.WriteBoolean("faulted",owner.IsFaulted);w.WriteEndObject();}
    w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("activeRows",activeRows);w.WriteNumber("completedInputs",completedInputs);w.WriteNumber("refusedInputs",refusedInputs);w.WriteNumber("actualInputs",samples.Length);w.WriteEndObject();
Console.WriteLine($"PASS{samples.Length} actual inputs: {completedInputs} zero-bit8 completed/{refusedInputs} positive-bit8 exact pre-entry refusals; {activeRows} compact rows/{guards} guards. No candidate discarded; completed count may be zero. No transcription assertion.");
Exception RefuseEnabled(Action action)
{
    try{action();}catch(NotSupportedException error)when(error.GetType()==typeof(NotSupportedException)&&error.Message==EnabledMessage){guards++;return error;}
    throw new InvalidDataException("Expected EXACT enabled219580 refusal before a completion owner exists.");
}
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Source precondition/retry guard failed.");
}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
