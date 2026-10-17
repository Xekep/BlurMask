using InkRecognition;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication line30 line50 normalization circles capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[8]);using var cr=File.OpenRead(args[9]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] l30=File.ReadAllBytes(args[6]),l50=File.ReadAllBytes(args[7]);
InkStroke[][] inputs=[[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
int guards=0,comparisons=0,postGlyphs=0;
using var stream=File.Create(args[10]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
foreach(var input in inputs)
{
    var raw=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,input,0,table,cases,publication)
        .Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
    var alternatives=raw.Alternatives.CopyActiveAlternatives();var heads=alternatives.Select(a=>a.WordHead18!).ToArray();
    var owner=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,raw,normalization,circles,new byte[4],new byte[4]);
    var sibling=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,raw,normalization,circles,new byte[4],new byte[4]);
    Reject(()=>LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(new byte[ak.Length],raw,normalization,circles,new byte[4],new byte[4]));
    Reject(()=>LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,raw,normalization,circles,new byte[3],new byte[4]));
    Reject(()=>owner.Run(new(true)));
    var publisher=alternatives[0].Ownership.WordPublisher;
    var before=publisher.CaptureSpan(heads[0],heads);var actualTop=raw.Alternatives.TopOwner??throw new InvalidDataException("Completed actual TOP missing.");
    var pool=actualTop.GlyphPool.CopyOwnerNodes();
    owner.Run();Reject(()=>owner.Run());Reject(()=>sibling.Run());
    if(!owner.IsTopV68Completed||owner.IsFaulted||!owner.SourcesMarkedComplete||!owner.RankingApplied||
        !owner.SpanScoresApplied||!owner.CheckboxGuardVerified||owner.Sources.IsTopV68Completed)
        throw new InvalidDataException("Configured source completion lost phase state.");
    var after=publisher.CaptureSpan(heads[0],heads);comparisons+=owner.ComparisonCalls;
    w.WriteStartObject();WriteWords("before",before);WriteWords("after",after);
    w.WriteStartArray("pool");foreach(var node in pool){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();
    w.WriteBase64String("compact",raw.Compact.CopyCompactStorage().AsSpan(0,raw.Compact.RecordCount*40));
    w.WriteNumber("comparisons",owner.ComparisonCalls);w.WriteNumber("quota",owner.QuotaAfter);
    w.WriteBase64String("sources26",owner.Sources.CopySourceStorage26());
    w.WriteStartArray("textChoices");
    foreach(var alternative in alternatives)
    {
        var source=LegacyOwnedMainCandidateSource.FromConfigured(raw,alternative);
        using var choices=LegacyFreshMainTextGlyphChoices.PrepareConfigured(ak,source,table,publication,normalization,circles);
        var words=choices.Publish(new byte[40],new byte[2040*4]);w.WriteStartArray();
        foreach(var word in words)foreach(var glyph in word.Glyphs)
        {w.WriteBase64StringValue(glyph.CopySelectedUtf8()??[]);postGlyphs++;}
        w.WriteEndArray();
    }
    w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();Console.WriteLine($"PASS3 actual shared-span configured source completions/{comparisons} linked-WORD comparisons/{postGlyphs} postcompletion glyph choices/{guards} guards; no0283B0 acceptance.");
void WriteWords(string name,LegacyNativePlainWordSpanSnapshot span)
{
    w.WriteStartObject(name);w.WriteStartArray("words");foreach(var word in span.Words)w.WriteBase64StringValue(word.CopyStorage());w.WriteEndArray();
    w.WriteStartArray("selected");foreach(int i in span.SelectedOrdinals)w.WriteNumberValue(i);w.WriteEndArray();
    w.WriteStartArray("refs");foreach(var word in span.Words){w.WriteStartArray();foreach(ulong r in word.FlatReferences)w.WriteNumberValue(r);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Configured completion guard failed.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
