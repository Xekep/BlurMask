using InkRecognition;
using System.Text.Json;
using System.Runtime.InteropServices;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation case-data publication-data line30 line50 output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(separation);
var singleton=MainFixture.Create(ak,engine,table);
var owner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,singleton,caseData,publication);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner);
var line=MainLineWordFixture.Create(ak,engine,separation,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),caseData,publication,2);
var lineOwner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,caseData,publication);
var lineConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,lineOwner,line.Context);
var directOwner=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,singleton);
var directConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directOwner);
var directLineOwner=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source);
var directLineConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directLineOwner,line.Context);
int guards=0,groups=0,candidates=0,extended=0,zeroed=0;
using(var file=File.Create(args[8]))using(var writer=new Utf8JsonWriter(file))
{
    writer.WriteStartObject();writer.WriteStartArray("sources");
    Capture("onePhysical",confidence,singleton.Owner.Postprocessing.Input.Controller.Source.SourceStrokeProbabilities.Span);
    Capture("twoPhysicalOneLine",lineConfidence,line.Source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source.SourceStrokeProbabilities.Span);
    Capture("directOnePhysical",directConfidence,singleton.Owner.Postprocessing.Input.Controller.Source.SourceStrokeProbabilities.Span);
    Capture("directTwoPhysicalOneLine",directLineConfidence,line.Source.Owner.Preparation.Input.Input.Input.Owner.Controller.Source.SourceStrokeProbabilities.Span);
    writer.WriteEndArray();
    writer.WriteStartArray("preparedNumericalControls");
    for(int n=0;n<128;n++)
    {
        //Explicit numerical tuples exercise extended-original calibration,
        //top20 zeros and uncalibrated primitive floats; not actual NN rows.
        ReadOnlyMemory<int>?[] families=new ReadOnlyMemory<int>?[7];
        for(int family=0;family<4;family++)families[family]=Enumerable.Range(0,180).Select(i=>(i*197+family*7919+n*173)%32768).ToArray();
        ushort[] symbols=Enumerable.Range(0,25).Select(i=>(ushort)i).ToArray();
        symbols[n%25]=185;symbols[(n+1)%25]=186;symbols[(n+2)%25]=191;
        symbols[(n+3)%25]=193;symbols[(n+4)%25]=194;
        float sourceProbability=(n%7) switch{5=>-.125f,6=>1.125f,_=>(n%7)*.25f};int primitiveCalls=0;
        var entries=symbols.Select((symbol,i)=>new LegacyAcousticCacheCandidate(symbol,-i*17)).ToArray();
        var values=confidence.EvaluateCandidates(entries,families,()=>{primitiveCalls++;return sourceProbability;});
        Require(primitiveCalls==(Array.IndexOf(symbols,(ushort)191)<20?1:0),"Primitive outside top20 was evaluated.");
        writer.WriteStartObject();writer.WriteNumber("seed",n);writer.WriteNumber("sourceProbabilityBits",BitConverter.SingleToInt32Bits(sourceProbability));writer.WriteStartArray("rows");
        foreach(var value in values){writer.WriteStartArray();writer.WriteNumberValue(value.Symbol);writer.WriteNumberValue(value.Cost);writer.WriteNumberValue(BitConverter.SingleToInt32Bits(value.Confidence));writer.WriteEndArray();}
        writer.WriteEndArray();writer.WriteEndObject();
    }
    writer.WriteEndArray();
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,(LegacyFreshCaseRussianDictionaryPreparation)null!));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner,new CancellationToken(true)));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,lineOwner,null!));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner,line.Context));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,lineOwner));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,lineOwner,line.Context,new CancellationToken(true)));
    var corrupted=ak.ToArray();corrupted[566416]^=1;Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(corrupted,owner));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,(LegacyOwnedRussianDictionaryPreparation)null!));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directOwner,line.Context));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directLineOwner));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directOwner,new CancellationToken(true)));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directLineOwner,line.Context,new CancellationToken(true)));
    writer.WriteNumber("groups",groups);writer.WriteNumber("candidates",candidates);writer.WriteNumber("extendedLabels",extended);writer.WriteNumber("zeroedAfter20",zeroed);writer.WriteNumber("guards",guards);
    writer.WriteEndObject();writer.Flush();

    void Capture(string name,LegacyOwnedRussianMainGlyphConfidence actual,ReadOnlySpan<float> originalProbabilities)
    {
        Require(actual.OwnerScoreChannel88==0&&ReferenceEquals(actual.Model,model)&&
            (actual.CaseDictionaryOwner is{} c?ReferenceEquals(actual.Graph,c.Graph)&&actual.DirectDictionaryOwner is null:actual.DirectDictionaryOwner is not null),"Owned graph/model/channel mismatch.");
        writer.WriteStartObject();writer.WriteString("name",name);writer.WriteStartArray("groups");
        for(int group=0;group<actual.Graph.Groups.Count;group++)
        {
            var result=actual.ScoreGroup(group);groups++;Require(ReferenceEquals(result.Owner,actual)&&result.GroupIndex==group,"Owned result identity lost.");
            var entries=result.Candidates.Span;Require(result.ReadSymbolScore(65535)==0,"Missing label did not return0.");
            var copy=result.Candidates;
            if(!copy.IsEmpty&&MemoryMarshal.TryGetArray(copy,out var underlying))
            {var saved=copy.Span[0];underlying.Array![underlying.Offset]=new(saved.Symbol,saved.Cost,-10);Require(result.ReadSymbolScore(saved.Symbol)==saved.Confidence,"Public candidate copy changed owned confidence.");}
            Require(BitConverter.SingleToInt32Bits(result.ReadFirstCandidateScore())==(entries.IsEmpty?0:BitConverter.SingleToInt32Bits(entries[0].Confidence)),"First vector channel differs.");
            writer.WriteStartObject();writer.WriteNumber("group",group);int physical=actual.Graph.Groups[group].Ranges.Span[0].StrokeIndex;
            writer.WriteNumber("firstPhysical",physical);writer.WriteNumber("sourceProbabilityBits",BitConverter.SingleToInt32Bits(originalProbabilities[physical]));
            writer.WriteStartArray("probabilities");
            for(int family=0;family<4;family++)
            {
                int[] p=result.CopyFamilyProbabilities(family);writer.WriteStartArray();foreach(int value in p)writer.WriteNumberValue(value);writer.WriteEndArray();
                p[0]^=1;Require(p[0]!=result.CopyFamilyProbabilities(family)[0],"Probability snapshot escaped.");
            }
            writer.WriteEndArray();writer.WriteStartArray("candidates");
            for(int ordinal=0;ordinal<entries.Length;ordinal++)
            {
                var entry=entries[ordinal];candidates++;if(entry.Symbol>=180)extended++;if(ordinal>=20){zeroed++;Require(entry.Confidence==0,"Candidate after20 retained confidence.");}
                Require(BitConverter.SingleToInt32Bits(result.ReadSymbolScore(entry.Symbol))==BitConverter.SingleToInt32Bits(entry.Confidence),"Candidate read differs.");
                writer.WriteStartArray();writer.WriteNumberValue(entry.Symbol);writer.WriteNumberValue(entry.Cost);writer.WriteNumberValue(BitConverter.SingleToInt32Bits(entry.Confidence));writer.WriteEndArray();
            }
            writer.WriteEndArray();writer.WriteEndObject();
            if(group==0){Reject(()=>result.ReadSymbolScore(0,new CancellationToken(true)));Reject(()=>result.ReadFirstCandidateScore(new CancellationToken(true)));Reject(()=>result.CopyFamilyProbabilities(4));}
        }
        writer.WriteEndArray();writer.WriteEndObject();
        Reject(()=>actual.ScoreGroup(-1));Reject(()=>actual.ScoreGroup(actual.Graph.Groups.Count));Reject(()=>actual.ScoreGroup(0,new CancellationToken(true)));
    }
}
Console.WriteLine($"PASS owned DIRECT/case main graph/scorer {groups} actual NN/cache groups,{candidates} confidence rows,{extended} extended labels,{zeroed} post20 zeros; separately128 prepared/3200 numerical rows;{guards} guards; no recognition accuracy/winner claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Invalid owned confidence scope accepted.");}
static void Require(bool value,string message){if(!value)throw new Exception(message);}
