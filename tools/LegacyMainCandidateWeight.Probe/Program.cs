using InkRecognition;
using System.Text.Json;
using System.Runtime.InteropServices;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation case publication line30 line50 capture.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separation=File.ReadAllBytes(args[3]);
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(separation);
var singleton=MainFixture.Create(ak,engine,table);
var line=MainLineWordFixture.Create(ak,engine,separation,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),caseData,publication,2);
var cases=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,singleton,caseData,publication);
var caseLine=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,caseData,publication);
var direct=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,singleton);
var directLine=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source);
int groups=0,rows=0,guards=0,numericalRows=0;
using(var file=File.Create(args[8]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteStartArray("preparedNumericalControls");
    for(int seed=0;seed<320;seed++)
    {
        int count=new[]{0,1,2,19,20,21,31,150}[seed%8];
        float scale=new[]{.125f,1f,1.125f,8f}[seed/8%4];
        int[] costs=Enumerable.Range(0,count).Select(i=>unchecked((seed*7919+i*65537)%200000-50000)).ToArray();
        if(seed/32==0)Array.Fill(costs,int.MaxValue);
        if(seed/32==1)Array.Fill(costs,int.MinValue);
        if(seed/32==2)Array.Fill(costs,0);
        if(seed/32==3)for(int i=0;i<count;i++)costs[i]=i==0?0:655360; // exp(-80), no clamp
        if(seed/32==4)for(int i=0;i<count;i++)costs[i]=i%2==0?16777217:-16777219; // int->float rounding
        var values=LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(costs,scale);
        Require(values.Length==costs.Length,"Kernel length differs.");
        if(count==1)Require(values[0]==1,"Native singleton override lost.");
        for(int i=20;i<count;i++)Require(values[i]==0,"Post20 field0 must be zero.");
        w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteNumber("scaleBits",Bits(scale));WriteCosts(costs);WriteValues(values);w.WriteEndObject();numericalRows+=count;
    }
    w.WriteEndArray();w.WriteStartArray("sources");
    Capture("caseSingleton",LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,cases));
    Capture("directSingleton",LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,direct));
    Capture("caseTwoPhysicalLine",LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,caseLine,line.Context));
    Capture("directTwoPhysicalLine",LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,directLine,line.Context));
    w.WriteEndArray();
    Reject(()=>LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(new int[151],1));
    foreach(float invalid in new[]{0f,-1,float.NaN,float.PositiveInfinity})Reject(()=>LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(new[]{1,2},invalid));
    Reject(()=>LegacyOwnedRussianMainGlyphConfidenceGroup.CalculateCandidateWeights(new[]{1},1,new CancellationToken(true)));
    w.WriteNumber("groups",groups);w.WriteNumber("actualRows",rows);w.WriteNumber("preparedRows",numericalRows);w.WriteNumber("guards",guards);w.WriteEndObject();

    void Capture(string name,LegacyOwnedRussianMainGlyphConfidence confidence)
    {
        w.WriteStartObject();w.WriteString("name",name);w.WriteStartArray("groups");
        for(int g=0;g<confidence.Graph.Groups.Count;g++)
        {
            var group=confidence.ScoreGroup(g);var entries=group.Candidates.ToArray();
            Require(ReferenceEquals(group.Owner,confidence)&&group.GroupIndex==g&&group.GroupPowerScale==1,"Actual owner/scale lost.");
            float[] values=entries.Select(e=>group.ReadNormalizedCandidateWeight(e.Symbol)).ToArray();
            Require(group.ReadNormalizedCandidateWeight(65535)==0,"Missing symbol weight differs.");
            if(entries.Length!=0)
            {
                var copy=group.Candidates;Require(MemoryMarshal.TryGetArray(copy,out var backing),"Candidate copy expected.");
                backing.Array![backing.Offset]=new(entries[0].Symbol,int.MinValue,-7);
                Require(Bits(group.ReadNormalizedCandidateWeight(entries[0].Symbol))==Bits(values[0]),"Candidate snapshot mutated owned field0.");
                Require(group.ReadSymbolScore(entries[0].Symbol)==entries[0].Confidence,"Field0 corrupted vector10 confidence.");
            }
            w.WriteStartObject();w.WriteNumber("group",g);w.WriteNumber("scaleBits",Bits(group.GroupPowerScale));WriteCosts(entries.Select(e=>e.Cost).ToArray());WriteValues(values);
            w.WriteStartArray("symbols");foreach(var e in entries)w.WriteNumberValue(e.Symbol);w.WriteEndArray();w.WriteEndObject();groups++;rows+=entries.Length;
            if(g==0)Reject(()=>group.ReadNormalizedCandidateWeight(0,new CancellationToken(true)));
        }
        w.WriteEndArray();w.WriteEndObject();
    }
    void WriteCosts(int[] costs){w.WriteStartArray("costs");foreach(int c in costs)w.WriteNumberValue(c);w.WriteEndArray();}
    void WriteValues(float[] values){w.WriteStartArray("weightBits");foreach(float value in values)w.WriteNumberValue(Bits(value));w.WriteEndArray();}
}
Console.WriteLine($"PASS 320 prepared1BE530 controls/{numericalRows} rows; {groups} actual owned groups/{rows} field0 rows; {guards} guards. No winner or text accuracy claim.");
static int Bits(float value)=>BitConverter.SingleToInt32Bits(value);
static void Require(bool value,string message){if(!value)throw new Exception(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Invalid numeric scope accepted.");}
