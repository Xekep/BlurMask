using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=4||args[0] is not("--validate-plan" or "--actual"))
    throw new ArgumentException("--validate-plan|--actual EXACT_MATH_RES ORIGINAL14_FIXTURES FRESH_CAPTURE");
if(File.Exists(args[3]))throw new IOException("Fresh capture required.");
string Hash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
var raw=File.ReadAllBytes(args[1]);var fixtureBytes=File.ReadAllBytes(args[2]);
const string resourceSHA="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
const string fixtureSHA="ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb";
if(Hash(raw)!=resourceSHA||Hash(fixtureBytes)!=fixtureSHA)
    throw new InvalidDataException("Exact immutable original resource/fixture pins required.");
using var fixture=JsonDocument.Parse(fixtureBytes);var input=fixture.RootElement.GetProperty("cases");
if(input.GetArrayLength()!=14||input.EnumerateArray().Select(c=>c.GetProperty("name").GetString()).Distinct().Count()!=14)
    throw new InvalidDataException("Original14 ordered unique cases required.");
//Return BEFORE profile/geometry/source creation. This mode performs no owning
//stage or model preparation and cannot be interpreted as a source success.
if(args[0]=="--validate-plan")
{
    Console.WriteLine(JsonSerializer.Serialize(new{mode="validate-plan",resourceSHA,fixtureSHA,cases=14,
        owningSourceCalls=0,modelEvaluations=0,freshCapture=args[3]}));return;
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
var rows=new List<object>(14);var guards=new List<string>(128);int owningCalls=0;
void Guard(string name,bool ok){if(!ok)throw new InvalidOperationException(name);guards.Add(name);}
foreach(var c in input.EnumerateArray())
{
    string name=c.GetProperty("name").GetString()!;var source=profile.CreateGeometry(Original(c));
    if(rows.Count==0)
    {
        using var cancel=new CancellationTokenSource();cancel.Cancel();bool exact=false;
        try{LegacyMathAkOriginalInputContext.PrepareFreshWorkspaceDiagnostic(source,cancel.Token);}
        catch(OperationCanceledException e){exact=e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cancel.Token;}
        Guard("pre-cancel-exact-token",exact);
        bool nil=false;try{LegacyMathAkOriginalInputContext.PrepareFreshWorkspaceDiagnostic(null!);}
        catch(ArgumentNullException e){nil=e.ParamName=="original";}
        Guard("null-original-refused",nil);
    }
    owningCalls++;
    var data=LegacyMathAkOriginalInputContext.PrepareFreshWorkspaceDiagnostic(source);
    Guard("version-"+name,data.Version==1);
    Guard("actual-W-"+name,data.SegmentCount>=0&&data.WorkWords==((data.SegmentCount>>5)+1));
    Guard("parsed-tail-capacity-"+name,data.HeapCapacity==profile.FreshCommonHeapCapacity);
    Guard("fresh-pool-only-"+name,data.Descriptor108Count==0&&data.TemporaryHeapCount==0&&data.ResultSlot16Count==0&&data.Hypothesis24Count==0&&data.FreeSlotCount==0);
    Guard("ledger-"+name,data.ChargedPayloadBytes==182&&data.ChargedCopyBytes==296&&data.ChargedStructuralWork==898);
    Guard("no-FIRST-"+name,!data.HasFirstRequest&&!data.HasCandidateAuthority);
    rows.Add(new{name,data});
}
//Only detached fixed scalar DTOs retained; cap before formatting the JSON.
if(rows.Count!=14||14L*70>1024)throw new InvalidOperationException("Diagnostic scalar payload cap exceeded.");
var text=JsonSerializer.Serialize(new{resourceSHA,fixtureSHA,cases=rows,guards,owningSourceCalls=owningCalls,
    modelEvaluations=0,processedGraphAuthority=false,matcherRequestAuthority=false},new JsonSerializerOptions{WriteIndented=true})+"\n";
if(System.Text.Encoding.UTF8.GetByteCount(text)>128*1024)throw new NotSupportedException("Diagnostic JSON cap exceeded.");
File.WriteAllText(args[3],text,new System.Text.UTF8Encoding(false));
Console.WriteLine($"PASS {rows.Count} fresh SOURCE headers/{guards.Count} guards/0 model evaluations");
