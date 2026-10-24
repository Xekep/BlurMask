using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=3||args[0] is not("--validate-plan" or "--actual"))
    throw new ArgumentException("--validate-plan|--actual EXACT_MATH_RES FRESH_CAPTURE");
if(File.Exists(args[2]))throw new IOException("Fresh capture required.");
const int resourceBytes=3685881,maximumCaptureBytes=8192;
const string resourceSHA="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
if(new FileInfo(args[1]).Length!=resourceBytes)throw new InvalidDataException("Exact resource size required before read.");
byte[] caller=File.ReadAllBytes(args[1]);
if(caller.Length!=resourceBytes||Convert.ToHexString(SHA256.HashData(caller)).ToLowerInvariant()!=resourceSHA)
    throw new InvalidDataException("Exact immutable Math resource required.");
//No target mode has been executed by the author, including validate-plan.
if(args[0]=="--validate-plan")
{
    Console.WriteLine(JsonSerializer.Serialize(new{mode="validate-plan",resourceSHA,resourceBytes,
        profileSuccesses=0,geometryCalls=0,sourceCalls=0,firstCalls=0,NN=0,
        maximumCaptureBytes,plannedSuccessfulProfiles=1,scalarContentReads=222628}));return;
}
//Fixed operation reservation BEFORE diagnostic arrays, profile or scalar loops.
const long maximumScalarCalls=222637,maximumWireBytes=337828;
const long knownLoopArrayPayloadBytes=1360,callerOverwriteBytes=3685881;
var reservation=new DiagnosticReservation(maximumScalarCalls,maximumWireBytes);
var scratch=new byte[1024];var guards=new GuardData[12];int guardCount=0;
using var cts=new CancellationTokenSource();cts.Cancel();
void Guard(string name,Action action,Type type,string? parameter=null,CancellationToken? token=null)
{
    if(guardCount>=guards.Length)throw new InvalidDataException("Guard extent exceeded.");
    try{action();}catch(Exception e)
    {
        if(e.GetType()!=type||parameter is not null&&(e as ArgumentException)?.ParamName!=parameter||
            token is not null&&(e as OperationCanceledException)?.CancellationToken!=token.Value)throw;
        guards[guardCount++]=new(name,type.Name,parameter,token is not null);return;
    }
    throw new InvalidDataException("Expected exact refusal: "+name);
}
Guard("prepare-pre-cancel",()=>LegacyMathAkFeatureProfile.PrepareActual(caller,cts.Token),typeof(OperationCanceledException),token:cts.Token);
Guard("empty-resource",()=>LegacyMathAkFeatureProfile.PrepareActual(ReadOnlySpan<byte>.Empty),typeof(ArgumentException),"resource");
var profile=LegacyMathAkFeatureProfile.PrepareActual(caller);
string[] DigestPass()
{
    var result=new string[3];
    for(int bank=0;bank<3;bank++)
    {
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int count=bank==0?57600:bank==1?53456:258,at=0;
        for(int i=0;i<count;i++)
        {
            reservation.BeforeScalar();int value=profile.ReadFirst276RetainedScalar(bank,i);
            if(bank==0)
            {
                if(value<short.MinValue||value>short.MaxValue)throw new InvalidDataException("SHORT scalar differs.");
                ushort bits=unchecked((ushort)value);reservation.BeforeWire(2);
                scratch[at++]=(byte)bits;scratch[at++]=(byte)(bits>>8);
            }
            else
            {
                if(bank==1&&(value<sbyte.MinValue||value>sbyte.MaxValue)||bank==2&&(value<0||value>255))
                    throw new InvalidDataException("Byte scalar differs.");
                reservation.BeforeWire(1);scratch[at++]=unchecked((byte)value);
            }
            if(at==scratch.Length){hash.AppendData(scratch,0,at);at=0;}
        }
        if(at>0)hash.AppendData(scratch,0,at);
        result[bank]=Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
    return result;
}
var before=DigestPass();Array.Fill(caller,(byte)0xa5);var after=DigestPass();
//Compare raw-derived expected digests only in independent DATA auditor.
for(int bank=0;bank<3;bank++)
{
    int b=bank,count=b==0?57600:b==1?53456:258;
    Guard("negative-index-"+b,()=>{reservation.BeforeScalar();profile.ReadFirst276RetainedScalar(b,-1);},typeof(ArgumentOutOfRangeException),"index");
    Guard("past-end-index-"+b,()=>{reservation.BeforeScalar();profile.ReadFirst276RetainedScalar(b,count);},typeof(ArgumentOutOfRangeException),"index");
}
Guard("negative-bank",()=>{reservation.BeforeScalar();profile.ReadFirst276RetainedScalar(-1,0);},typeof(ArgumentOutOfRangeException),"bank");
Guard("past-end-bank",()=>{reservation.BeforeScalar();profile.ReadFirst276RetainedScalar(3,0);},typeof(ArgumentOutOfRangeException),"bank");
Guard("scalar-pre-cancel",()=>{reservation.BeforeScalar();profile.ReadFirst276RetainedScalar(0,0,cts.Token);},typeof(OperationCanceledException),token:cts.Token);
Guard("wiped-resource-hash",()=>LegacyMathAkFeatureProfile.PrepareActual(caller),typeof(InvalidDataException));
if(guardCount!=12||reservation.ScalarCalls!=maximumScalarCalls||reservation.WireBytes!=maximumWireBytes)
    throw new InvalidDataException("Exact diagnostic extents differ.");
var data=new{version=1,resourceSHA,resourceBytes,profileSuccesses=1,profileHashRefusals=1,
    callerOverwriteByte=165,sameProfileAcrossPasses=true,before,after,
    scalarCalls=reservation.ScalarCalls,wireBytes=reservation.WireBytes,
    knownLoopArrayPayloadBytes,callerOverwriteBytes,maximumCaptureBytes,
    calibrationPayloadBytes=profile.CalibrationPayloadBytes,hasCandidateAuthority=profile.HasCandidateAuthority,
    guards,geometryCalls=0,sourceCalls=0,firstCalls=0,NN=0};
byte[] json=JsonSerializer.SerializeToUtf8Bytes(data,new JsonSerializerOptions{WriteIndented=true});
if(json.Length>maximumCaptureBytes)throw new InvalidDataException("Capture UTF8 extent exceeded.");
File.WriteAllBytes(args[2],json);Console.WriteLine("immutable model DATA capture written");
sealed record GuardData(string Name,string Type,string? Parameter,bool TokenMatched);
sealed class DiagnosticReservation(long scalarLimit,long wireLimit)
{
    public long ScalarCalls{get;private set;}
    public long WireBytes{get;private set;}
    public void BeforeScalar(){if(ScalarCalls>=scalarLimit)throw new InvalidDataException("Scalar reservation exhausted.");ScalarCalls++;}
    public void BeforeWire(int n){if(n<0||WireBytes>wireLimit-n)throw new InvalidDataException("Wire reservation exhausted.");WireBytes+=n;}
}
