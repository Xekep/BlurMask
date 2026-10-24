using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=4||args[0] is not("--validate-plan" or "--actual"))
    throw new ArgumentException("--validate-plan|--actual EXACT_MATH_RES ORIGINAL14_FIXTURES FRESH_CAPTURE");
if(File.Exists(args[3]))throw new IOException("Fresh capture required.");
string Hash(byte[] b)=>Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();
const string resourceSHA="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
const string fixtureSHA="ce3505ffeb0014a62eacde7c2d4841a23dda5f704fb127820fac4c937134eacb";
var raw=File.ReadAllBytes(args[1]);var fixtureBytes=File.ReadAllBytes(args[2]);
if(raw.Length!=3685881||Hash(raw)!=resourceSHA||Hash(fixtureBytes)!=fixtureSHA)
    throw new InvalidDataException("Exact immutable resource/fixture pins required.");
using var fixture=JsonDocument.Parse(fixtureBytes);var input=fixture.RootElement.GetProperty("cases");
if(input.GetArrayLength()!=14||input.EnumerateArray().Select(c=>c.GetProperty("name").GetString()).Distinct().Count()!=14)
    throw new InvalidDataException("Original14 ordered unique cases required.");
//ROOT review required even for this no-profile branch. Author does NOT run it.
if(args[0]=="--validate-plan")
{
    Console.WriteLine(JsonSerializer.Serialize(new{mode="validate-plan",resourceSHA,fixtureSHA,cases=14,
        profileCalls=0,geometryCalls=0,owningSourceCalls=0,firstCalls=0,modelEvaluations=0,maximumCaptureBytes=131072}));return;
}
var guards=new List<object>(128);
void Guard(string name,Action action,Type type,string? param=null,CancellationToken? token=null)
{
    try{action();throw new InvalidOperationException("Guard unexpectedly accepted: "+name);}
    catch(Exception e)
    {
        if(e.GetType()!=type||param!=null&&(e as ArgumentException)?.ParamName!=param||
            token!=null&&(e as OperationCanceledException)?.CancellationToken!=token.Value)throw;
        guards.Add(new{Name=name,Type=e.GetType().FullName,Param=(e as ArgumentException)?.ParamName,
            TokenMatches=token==null?(bool?)null:true});
    }
}
void Check(string name,bool ok)
{
    if(!ok)throw new InvalidDataException(name);
    guards.Add(new{Name=name,Type="assertion",Param=(string?)null,TokenMatches=(bool?)null});
}
using var cancelled=new CancellationTokenSource();cancelled.Cancel();
Guard("profile-pre-cancel",()=>LegacyMathAkFeatureProfile.PrepareActual(raw,cancelled.Token),typeof(OperationCanceledException),token:cancelled.Token);
Guard("profile-null-span",()=>LegacyMathAkFeatureProfile.PrepareActual((byte[]?)null),typeof(ArgumentException),"resource");
Guard("profile-truncated",()=>LegacyMathAkFeatureProfile.PrepareActual(raw.AsSpan(0,raw.Length-1)),typeof(ArgumentException),"resource");
byte[] damaged=(byte[])raw.Clone();damaged[7481]^=1;
Guard("profile-hash-refusal",()=>LegacyMathAkFeatureProfile.PrepareActual(damaged),typeof(InvalidDataException));
Array.Clear(damaged);
var profile=LegacyMathAkFeatureProfile.PrepareActual(raw);
object ReadAlphabet()=>new{Count=(int)profile.FirstAlphabetCount,
    Labels=Enumerable.Range(0,14).Select(i=>(int)profile.ResolveFirstAlphabetSpecial(i)).ToArray(),
    RetainedPayload=LegacyMathAkFeatureProfile.FirstAlphabetAdditionalPayloadBytes,
    ChargedCopies=LegacyMathAkFeatureProfile.FirstAlphabetAdditionalCopyBytes,
    ChargedSourceWork=LegacyMathAkFeatureProfile.FirstAlphabetAdditionalSourceUnits};
var before=ReadAlphabet();Array.Fill(raw,(byte)0xA5);var after=ReadAlphabet();
Check("caller-A5-overwrite-retained",JsonSerializer.Serialize(before)==JsonSerializer.Serialize(after));
Guard("special-index-negative",()=>profile.ResolveFirstAlphabetSpecial(-1),typeof(ArgumentOutOfRangeException),"ordinal");
Guard("special-index-past",()=>profile.ResolveFirstAlphabetSpecial(14),typeof(ArgumentOutOfRangeException),"ordinal");
Vector2[][] Original(JsonElement c)
{
    if(c.TryGetProperty("input",out var source))
        return source.EnumerateArray().Select(row=>row.EnumerateArray().Select(p=>new Vector2(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var r=c.GetProperty("recipe");int n=r.GetProperty("count").GetInt32();float step=r.GetProperty("step").GetSingle();
    bool horizontal=r.GetProperty("kind").GetString()=="horizontal";float[] pattern=[0f,.001f,0f,-.001f];
    return [Enumerable.Range(0,n).Select(i=>new Vector2(i*step,horizontal?0f:pattern[i%4])).ToArray()];
}
var rows=new List<object>(14);int owningCalls=0;
foreach(var c in input.EnumerateArray())
{
    string name=c.GetProperty("name").GetString()!;var source=profile.CreateGeometry(Original(c));
    if(rows.Count==0)
    {
        Guard("source-pre-cancel",()=>LegacyMathAkOriginalInputContext.PrepareFreshFirstAlphabetDiagnostic(source,cancelled.Token),typeof(OperationCanceledException),token:cancelled.Token);
        Guard("source-null",()=>LegacyMathAkOriginalInputContext.PrepareFreshFirstAlphabetDiagnostic(null!),typeof(ArgumentNullException),"original");
    }
    owningCalls++;
    var data=LegacyMathAkOriginalInputContext.PrepareFreshFirstAlphabetDiagnostic(source);
    Check("version-"+name,data.Version==1);
    Check("count-labels-"+name,data.AlphabetCount==profile.FirstAlphabetCount&&data.SpecialLabels.Length==14&&
        data.SpecialLabels.Select((x,i)=>(int)x==profile.ResolveFirstAlphabetSpecial(i)).All(x=>x));
    Check("request-opacity-"+name,data.KnownRequestNumericBytes==17&&data.OpaqueRequestBytes==7);
    Check("ledger-"+name,data.ProfileRetainedPayload==5875&&data.AdditionalPayload==182&&data.AdditionalCopies==226&&data.AdditionalSourceWork==105755);
    Check("no-FIRST-"+name,!data.HasPreparedFirstReceiver&&!data.HasCandidateAuthority&&!data.Header.HasFirstRequest&&!data.Header.HasCandidateAuthority);
    Check("header-W-"+name,data.Header.WorkWords==((data.Header.SegmentCount>>5)+1));
    rows.Add(new{name,data});
}
//Detached DATA only: no Context/header/operand escapes. Fixed row count
//is not an allocation bound; accepted UTF8 output is capped after serialization.
if(rows.Count!=14)throw new InvalidDataException("Exact14 diagnostic cases required.");
var result=new{schema="first276-al-source-actual-v1",resourceSHA,fixtureSHA,AlphabetBefore=before,AlphabetAfter=after,
    CallerOverwritten=true,cases=rows,guards,successfulProfiles=1,hashRejectedProfiles=1,owningSourceCalls=owningCalls,
    preentrySourceRequests=2,firstCalls=0,modelEvaluations=0,processedGraphAuthority=false,matcherRequestAuthority=false,maximumCaptureBytes=131072};
byte[] output=JsonSerializer.SerializeToUtf8Bytes(result,new JsonSerializerOptions{WriteIndented=true});
if(output.Length>131072)throw new NotSupportedException("Diagnostic UTF8 JSON cap exceeded.");
File.WriteAllBytes(args[3],output);
Console.WriteLine($"PASS {rows.Count} fresh AL source headers/{guards.Count} guards/0 FIRST or model evaluations");
