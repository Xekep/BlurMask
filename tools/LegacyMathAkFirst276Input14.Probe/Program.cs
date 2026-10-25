using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=4||args[0] is not("--validate-plan" or "--actual"))
    throw new ArgumentException("--validate-plan|--actual EXACT_MATH_RES ORIGINAL14_FIXTURES FRESH_CAPTURE");
if(File.Exists(args[3]))throw new IOException("Fresh capture required.");
const string resourceSHA="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
const string fixtureSHA="ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb";
const string missing="Actual FIRST evaluator/shortlist continuation remains unported.";
const int maximumCaptureBytes=262144;
string Hash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
if(new FileInfo(args[1]).Length!=3685881||new FileInfo(args[2]).Length!=4242)
    throw new InvalidDataException("Exact input extents required before read.");
byte[] raw=File.ReadAllBytes(args[1]),fixtureBytes=File.ReadAllBytes(args[2]);
if(Hash(raw)!=resourceSHA||Hash(fixtureBytes)!=fixtureSHA)throw new InvalidDataException("Immutable input pin differs.");
using var fixture=JsonDocument.Parse(fixtureBytes);var cases=fixture.RootElement.GetProperty("cases");
if(cases.GetArrayLength()!=14||cases.EnumerateArray().Select(c=>c.GetProperty("name").GetString()).Distinct().Count()!=14)
    throw new InvalidDataException("Original ordered14 fixtures required.");
if(args[0]=="--validate-plan")
{
    Console.WriteLine(JsonSerializer.Serialize(new{mode="validate-plan",resourceSHA,fixtureSHA,cases=14,
        profileCalls=0,owningSourceCalls=0,FIRST=0,NN=0,maximumCaptureBytes}));return;
}
Vector2[][] Original(JsonElement c)
{
    if(c.TryGetProperty("input",out var source))
        return source.EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var r=c.GetProperty("recipe");int n=r.GetProperty("count").GetInt32();float step=r.GetProperty("step").GetSingle();
    bool horizontal=r.GetProperty("kind").GetString()=="horizontal";float[] pattern=[0f,.001f,0f,-.001f];
    return [Enumerable.Range(0,n).Select(i=>new Vector2(i*step,horizontal?0f:pattern[i%4])).ToArray()];
}
var profile=LegacyMathAkFeatureProfile.PrepareActual(raw);
// Authenticated retained profile must survive caller-byte overwrite.
Array.Fill(raw,(byte)0xA5);
var rows=new List<object>(14);int owningCalls=0,complete=0,earlier=0;long exportedShortPayloadBytes=0;
foreach(var c in cases.EnumerateArray())
{
    string name=c.GetProperty("name").GetString()!;
    var original=Original(c);var source=profile.CreateGeometry(original);
    owningCalls++;
    try
    {
        LegacyMathAkOriginalInputContext.PrepareFreshFirstInputBoundary(source);
        throw new InvalidOperationException("Missing evaluator boundary unexpectedly returned.");
    }
    catch(LegacyMathAkOriginalInputContext.First276InputBoundaryException e)
    {
        var d=e.Prefix;
        if(e.GetType()!=typeof(LegacyMathAkOriginalInputContext.First276InputBoundaryException)||e.Message!=missing||
            d.EndNode!=0||d.SuffixCount!=1||d.SelectedCount!=1||d.RangeCount!=1||
            d.CacheEntries!=1||d.Descriptor108Count!=1||d.InitializedInputCount!=276||
            !d.HasInitializedFeatures||d.HasEvaluatorOutput||d.InitializedInput is not {Length:276}||
            d.InitializedInput[0]!=1024||d.ChargedPayloadBytes<0||d.ChargedCopyBytes<0||d.ChargedSourceWork<0)
            throw new InvalidDataException("Exact first-request complete-input boundary differs.");
        // The production export is its precharged fresh276 copy, never input288.
        // Retain that DATA directly; no unmetered second short-array clone.
        foreach(var stroke in original)Array.Fill(stroke,new Vector2(float.NaN,float.NaN));
        exportedShortPayloadBytes=checked(exportedShortPayloadBytes+552);complete++;
        rows.Add(new{name,outcome="complete-input-missing-evaluator",exceptionType=e.GetType().FullName,message=e.Message,data=d});
    }
    catch(NotSupportedException e)when(e.GetType()==typeof(NotSupportedException))
    {
        // Honest pre-boundary refusal: no substitute vector and no feature claim.
        // Python audit preserves this reason and reports coverage as incomplete.
        if(e.Message.Length>1024)throw new InvalidDataException("Refusal text cap exceeded.");
        earlier++;rows.Add(new{name,outcome="earlier-refusal-no-input",exceptionType=e.GetType().FullName,message=e.Message,data=(object?)null});
    }
}
if(owningCalls!=14||complete+earlier!=14)throw new InvalidDataException("Original14 cardinality differs.");
var result=new{schema="first276-input14-boundary-v1",resourceSHA,fixtureSHA,cases=rows,
    successfulProfiles=1,owningSourceCalls=owningCalls,completeInputBoundaries=complete,earlierRefusals=earlier,
    modelEvaluations=0,evaluatorSuccesses=0,fullOldALHeaderProjectionVerified=false,
    liveMultiRangeCoverage=false,physicalPaddingExported=false,exportedShortPayloadBytes,
    maximumCaptureBytes,capMeaning="accepted serialized UTF8 extent; not allocator/RSS bound"};
byte[] output=JsonSerializer.SerializeToUtf8Bytes(result,new JsonSerializerOptions{WriteIndented=true});
if(output.Length>maximumCaptureBytes)throw new NotSupportedException("Diagnostic UTF8 cap exceeded.");
using(var file=new FileStream(args[3],FileMode.CreateNew,FileAccess.Write,FileShare.None))file.Write(output);
Console.WriteLine($"CAPTURE14 complete-input={complete} earlier-refusal={earlier} evaluator-success=0");
