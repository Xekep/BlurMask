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
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);int guards=0,activeRows=0;
using var stream=File.Create(args[8]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteStartArray("inputs");
for(int input=0;input<samples.Length;input++)
{
    var raw=context.Prepare(samples[input],0,table,cases,publication).Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
    var candidate=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
    var source=LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation(candidate);
    var owner=LegacyFreshMainNoCandidateSourceCompletion.Prepare(ak,source);
    var sibling=LegacyFreshMainNoCandidateSourceCompletion.Prepare(ak,source);
    Reject(()=>LegacyFreshMainNoCandidateSourceCompletion.Prepare(new byte[ak.Length],source));
    Reject(()=>owner.Run(new(true)));
    byte[] before=source.Word.Word.CopyStorage();ulong[] references=source.Word.Word.FlatReferences.ToArray();
    byte[] compact=raw.Session.Ownership.Materialization.CopyCompactStorage();int count=raw.Session.Ownership.Materialization.RecordCount;
    owner.Run();Reject(()=>owner.Run());Reject(()=>sibling.Run());
    if(!owner.IsTopV68Completed||owner.IsFaulted||!owner.SourcesMarkedComplete||!owner.RankingApplied||
        !owner.SpanScoresApplied||!owner.CheckboxGuardVerified||source.IsTopV68Completed||
        !source.Word.Word.FlatReferences.SequenceEqual(references)||source.Word.WordIndex!=0)
        throw new InvalidDataException("Bounded source completion lost actual phase/provenance state.");
    guards++;activeRows+=count;
    w.WriteStartObject();w.WriteNumber("input",input);w.WriteNumber("count",count);w.WriteBase64String("compact",compact.AsSpan(0,count*40));
    w.WriteBase64String("before",before);w.WriteBase64String("after",source.Word.Word.CopyStorage());w.WriteBase64String("sources26",source.CopySourceStorage26());
    w.WriteNumber("sourceCount",source.ReferenceCount);w.WriteBoolean("completed",owner.IsTopV68Completed);
    w.WriteStartArray("references");foreach(ulong r in references)w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();
Console.WriteLine($"PASS{samples.Length} actual no-bit8 singleton TOP source completions/{activeRows} active compact rows/{guards} guards; enabled augmentation/document acceptance remain separate.");
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Source precondition/retry guard failed.");
}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
