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
int guards=0,comparisons=0,postGlyphs=0,completedInputs=0,refusedInputs=0;
const string Enabled="Enabled219580 candidate augmentation cannot be omitted.";
using var stream=File.Create(args[10]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
foreach(var input in inputs)
{
    var raw=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,input,0,table,cases,publication)
        .Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
    var alternatives=raw.Alternatives.CopyActiveAlternatives();var heads=alternatives.Select(a=>a.WordHead18!).ToArray();
    var prefixPublisher=alternatives[0].Ownership.WordPublisher;
    var prefix=prefixPublisher.CaptureSpan(heads[0],heads);
    var prefixTop=raw.Alternatives.TopOwner??throw new InvalidDataException("Completed actual TOP missing.");
    var prefixPool=prefixTop.GlyphPool.CopyOwnerNodes();
    var sourcePrefix=LegacyOwnedMainWordSourceCollection.PrepareConfigured(LegacyOwnedMainCandidateSource.FromConfigured(raw,alternatives[0]));
    byte[] source26=sourcePrefix.CopySourceStorage26();
    byte[] compact=raw.Compact.CopyCompactStorage();int compactCount=raw.Compact.RecordCount;
    if(compactCount<1||compact.Length<compactCount*40)throw new InvalidDataException("Complete nonempty actual compact required.");
    int bit8=0;for(int i=0;i<compactCount;i++)if((compact[i*40+32]&8)!=0)bit8++;
    if(bit8>0)
    {
        int startGuards=guards;LegacyFreshMainConfiguredNoCandidateSourceCompletion? refusedOwner=null;
        Reject(()=>LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(new byte[ak.Length],raw,normalization,circles,new byte[4],new byte[4]));
        Reject(()=>LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,raw,normalization,circles,new byte[3],new byte[4]));
        var errors=new List<Exception>(2);
        for(int repeat=0;repeat<2;repeat++)
        {
            try{refusedOwner=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,raw,normalization,circles,new byte[4],new byte[4]);}
            catch(NotSupportedException e)when(e.GetType()==typeof(NotSupportedException)&&e.Message==Enabled){errors.Add(e);guards++;}
            if(refusedOwner is not null||errors.Count!=repeat+1)throw new InvalidDataException("EXACT positive-bit8 pre-entry refusal required.");
        }
        var unchanged=prefixPublisher.CaptureSpan(heads[0],heads);
        if(!prefix.SelectedOrdinals.SequenceEqual(unchanged.SelectedOrdinals)||prefix.Words.Count!=unchanged.Words.Count||
           !prefix.SelectedNodes.SequenceEqual(unchanged.SelectedNodes)||sourcePrefix.IsTopV68Completed||
           !source26.AsSpan().SequenceEqual(sourcePrefix.CopySourceStorage26()))throw new InvalidDataException("Refusal changed selected/source ownership.");
        for(int i=0;i<prefix.Words.Count;i++)if(!prefix.Words[i].CopyStorage().AsSpan().SequenceEqual(unchanged.Words[i].CopyStorage())||
            !prefix.Words[i].FlatReferences.SequenceEqual(unchanged.Words[i].FlatReferences))throw new InvalidDataException("Refusal changed WORD120 or refs.");
        var afterPool=prefixTop.GlyphPool.CopyOwnerNodes();if(prefixPool.Count!=afterPool.Count)throw new InvalidDataException("Refusal changed pool.");
        for(int i=0;i<prefixPool.Count;i++)if(prefixPool[i].Key!=afterPool[i].Key||!prefixPool[i].Records.Span.SequenceEqual(afterPool[i].Records.Span))throw new InvalidDataException("Refusal changed pool bytes.");
        w.WriteStartObject();w.WriteString("branch","positive-bit8-refused");w.WriteNumber("bit8Rows",bit8);w.WriteNumber("count",compactCount);w.WriteBase64String("compact",compact.AsSpan(0,compactCount*40));
        WriteWords("before",prefix);WriteWords("after",unchanged);w.WriteNumber("guards",guards-startGuards);w.WriteBoolean("completionObjectCreated",false);w.WriteNumber("sourceRunCalls",0);w.WriteBoolean("sourcePrefixCompleted",sourcePrefix.IsTopV68Completed);
        w.WriteBase64String("sources26",source26);w.WriteBase64String("sourcesAfter26",sourcePrefix.CopySourceStorage26());
        w.WriteStartArray("pool");foreach(var node in prefixPool){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();
        w.WriteStartArray("poolAfter");foreach(var node in afterPool){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();
        w.WriteStartArray("refusals");foreach(var e in errors){w.WriteStartObject();w.WriteString("type",e.GetType().FullName);w.WriteString("message",e.Message);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();refusedInputs++;continue;
    }
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
    w.WriteStartObject();w.WriteString("branch","zero-bit8-completed");w.WriteNumber("bit8Rows",bit8);WriteWords("before",before);WriteWords("after",after);
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
    w.WriteEndArray();w.WriteEndObject();completedInputs++;
}
w.WriteEndArray();Console.WriteLine($"PASS3 actual configured inputs: {completedInputs} zero-bit8 completions/{refusedInputs} positive-bit8 exact refusals; {comparisons} linked-WORD comparisons/{postGlyphs} postcompletion glyph choices/{guards} guards; no0283B0 acceptance.");
void WriteWords(string name,LegacyNativePlainWordSpanSnapshot span)
{
    w.WriteStartObject(name);w.WriteStartArray("words");foreach(var word in span.Words)w.WriteBase64StringValue(word.CopyStorage());w.WriteEndArray();
    w.WriteStartArray("selected");foreach(int i in span.SelectedOrdinals)w.WriteNumberValue(i);w.WriteEndArray();
    w.WriteStartArray("refs");foreach(var word in span.Words){w.WriteStartArray();foreach(ulong r in word.FlatReferences)w.WriteNumberValue(r);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Configured completion guard failed.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
