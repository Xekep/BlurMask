using InkRecognition;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using P=InkRecognition.LegacyMathAkFeatureProfile;

if(args.Length!=3 || (args[0]!="--validate-plan" && args[0]!="--actual-profile"))throw new ArgumentException("mode resource output required");
const string sha="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
byte[] input=File.ReadAllBytes(args[1]);
if(input.Length!=3685881 || Convert.ToHexString(SHA256.HashData(input)).ToLowerInvariant()!=sha)throw new InvalidDataException("Pinned resource required.");
// This branch exits BEFORE any profile method, geometry, source, evaluator or model call.
if(args[0]=="--validate-plan"){
 File.WriteAllText(args[2],JsonSerializer.Serialize(new{Schema="first276-profile-plan-v1",ResourceSha256=sha,ResourceBytes=input.Length,ProfileCalls=0,GeometryCalls=0,SourceCalls=0,FirstCalls=0,NnCalls=0,MaximumCaptureBytes=262144}));return;
}
var guards=new List<object>();
void Guard(string name,Action action,Type type,string? param=null,CancellationToken? token=null){
 try{action();throw new Exception("Guard accepted: "+name);}catch(Exception e){
  if(e.GetType()!=type || (param!=null && ((ArgumentException)e).ParamName!=param) || (token!=null && ((OperationCanceledException)e).CancellationToken!=token.Value))throw;
  guards.Add(new{Name=name,Type=e.GetType().FullName,Param=(e as ArgumentException)?.ParamName,TokenMatches=token==null?(bool?)null:true});
 }
}
using var cancelled=new CancellationTokenSource();cancelled.Cancel();
Guard("pre-cancel",()=>P.PrepareActual(input,cancelled.Token),typeof(OperationCanceledException),token:cancelled.Token);
Guard("null-span",()=>P.PrepareActual((byte[]?)null),typeof(ArgumentException),"resource");
Guard("truncated",()=>P.PrepareActual(input.AsSpan(0,input.Length-1)),typeof(ArgumentException),"resource");
byte[] damaged=(byte[])input.Clone();damaged[7481]^=1;
Guard("hash-failure",()=>P.PrepareActual(damaged),typeof(InvalidDataException));
var profile=P.PrepareActual(input);
object Read(){
 var flags=BindingFlags.Instance|BindingFlags.NonPublic;
 short[][] Copy(string name)=>((short[][])typeof(P).GetField(name,flags)!.GetValue(profile)!).Select(x=>(short[])x.Clone()).ToArray();
 return new{Description=profile.First276ProgramInfo,Program=Enumerable.Range(0,109).Select(i=>(int)profile.ReadFirst276ProgramByte(i)).ToArray(),Minimum=Enumerable.Range(0,276).Select(i=>(int)profile.ReadFirst276Minimum(i)).ToArray(),Maximum=Enumerable.Range(0,276).Select(i=>(int)profile.ReadFirst276Maximum(i)).ToArray(),Tail=new{Program=profile.Program,Minimum=Copy("minimum"),Maximum=Copy("maximum"),profile.CalibrationPayloadBytes,profile.HasCandidateAuthority,profile.SourceFingerprint}};
}
var before=Read();Array.Fill(input,(byte)0xA5);var after=Read();
if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(after))throw new InvalidDataException("Caller mutation changed retained data.");
foreach(int i in new[]{-1,109})Guard("program-index-"+i,()=>profile.ReadFirst276ProgramByte(i),typeof(ArgumentOutOfRangeException),"index");
foreach(int i in new[]{-1,276}){
 Guard("minimum-index-"+i,()=>profile.ReadFirst276Minimum(i),typeof(ArgumentOutOfRangeException),"index");
 Guard("maximum-index-"+i,()=>profile.ReadFirst276Maximum(i),typeof(ArgumentOutOfRangeException),"index");
}
var result=new{Schema="first276-profile-actual-v1",ResourceSha256=sha,ResourceBytes=3685881,Before=before,After=after,CallerOverwritten=true,Guards=guards,SuccessfulProfiles=1,HashRejectedProfiles=1,GeometryCalls=0,SourceCalls=0,FirstCalls=0,NnCalls=0,MaximumCaptureBytes=262144};
byte[] output=JsonSerializer.SerializeToUtf8Bytes(result,new JsonSerializerOptions{WriteIndented=true});
if(output.Length>262144)throw new InvalidDataException("Diagnostic capture cap exceeded.");File.WriteAllBytes(args[2],output);
