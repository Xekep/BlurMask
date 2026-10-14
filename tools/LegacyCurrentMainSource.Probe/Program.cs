using InkRecognition;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("Supply independent actualreorder controls, AK,line30,line50.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));byte[] ak=File.ReadAllBytes(args[1]),m30=File.ReadAllBytes(args[2]),m50=File.ReadAllBytes(args[3]);
int cases=0,keys=0,changedPhysical=0,mixedCounts=0;
foreach(var item in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,m30,m50,raw,96);
    var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),m30,m50);
    var controller=LegacyMainFirstResultLineController.PrepareOwnedCurrentSource(ak,reordered);
    var current=controller.Source;var original=grouping.Packet.Source;
    Require(ReferenceEquals(current.Chronology,reordered.Chronology),"Actual retained chronology owner.");
    Require(controller.PhysicalResolution==96&&controller.SourceOffset==0&&controller.FirstSourceOffset==6120&&controller.SourceScoreExponent==.75f,"Actual main/firstcategory/resource roles.");
    if(raw.Select(s=>s.Points.Count).Distinct().Count()>1)mixedCounts++;
    for(int s=0;s<current.Strokes.Count;s++)
    {
        int old=reordered.OriginalPhysicalStrokeIndices.Span[s];
        Require(current.Strokes[s].Points.SequenceEqual(original.Strokes[old].Points),"No second source filter/currentpoint replacement.");
        Require(current.OriginalPointIndices[s].Span.SequenceEqual(original.OriginalPointIndices[old].Span),"Original point indices permuted unchanged.");
        Require(current.SourceStrokeProbabilities.Span[s]==original.SourceStrokeProbabilities.Span[old],"Original physical probabilities permuted unchanged.");
        if(old!=s)changedPhysical++;
        for(int p=0;p<current.Strokes[s].Points.Count;p++)
        {
            var identity=current.Chronology.Resolve(s,p);var expected=original.Chronology.Resolve(old,p);
            Require(identity.Key==expected.Key&&identity.KeyRecordIndex==expected.KeyRecordIndex,"Actual original metadata index.");
            var actualEnd=current.ResolveEndingMetadata(identity);var expectedEnd=original.ResolveEndingMetadata(expected);
            Require(actualEnd==expectedEnd,"Ending metadata must index original rawphysical owner, not current reorderedposition.");
            int expectedOrdinal=p+1<current.Strokes[s].Points.Count?original.OriginalPointIndices[old].Span[p+1]-1:raw[old].Points.Count-1;
            Require(actualEnd.Key.Third==expectedOrdinal&&actualEnd.Key.Second==old,"Independent originalraw endpointordinal.");keys++;
        }
    }
    Throws<OperationCanceledException>(()=>LegacyMainFirstResultLineController.PrepareOwnedCurrentSource(ak,reordered,new(true)));
    byte[] corrupt=ak.ToArray();corrupt[5301]^=1;Throws<ArgumentException>(()=>LegacyMainFirstResultLineController.PrepareOwnedCurrentSource(corrupt,reordered));cases++;
}
Require(changedPhysical>0&&mixedCounts>0,"Must cover reordered physical IDs and differing originalrawcounts.");
Console.WriteLine($"PASS {cases} retained currentmain owners/{keys} start+endmetadata identities/{changedPhysical} changedphysicalpositions/{mixedCounts} mixedrawcountcases; no filters/recomputedprobabilities or guide defaults.");
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
