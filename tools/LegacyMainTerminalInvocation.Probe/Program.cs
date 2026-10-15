using System.Text.Json;
using InkRecognition;
if(args.Length!=7)throw new ArgumentException("controls AK LK engine separation case-data publication-data required");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]),engine=File.ReadAllBytes(args[3]);
var fixture=MainFixture.Create(ak,lk,engine,File.ReadAllBytes(args[4]));
var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var invocation=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy);
Require(ReferenceEquals(invocation.ControllerOwner,policy)&&ReferenceEquals(invocation.SourceOwner.DictionaryOwner,owner.DictionaryOwner),"Terminal invocation lost actual controller/dictionary identity.");
Require(invocation.Options==new LegacyPreparedMainTerminalOptions(controls.RootElement.GetProperty("primary").GetUInt16(),false,false,false,false,0)&&
    invocation.NativeRescueRequested7&&!invocation.NativeRescueGateActive&&invocation.MaximumInvocationCount==1&&invocation.AlternativeIndex==0,
    "Fresh9C/result route/EC0 iteration/rescue gates differ.");
int count=0,winners=0;
foreach(var test in controls.RootElement.GetProperty("tests").EnumerateArray())
{
    byte[] rows=test.GetProperty("rows").EnumerateArray().SelectMany(x=>Convert.FromHexString(x.GetString()!)).ToArray();
    var result=invocation.SelectPreparedRows(rows,test.GetProperty("end").GetUInt16(),test.GetProperty("cumulative").GetInt32());
    var expected=test.GetProperty("winner");
    if(expected.ValueKind==JsonValueKind.Null)
        Require(!result.HasWinner&&result.Ordinal==-1&&result.RelativeCost==0x4000000&&result.TotalCost==0x4000000&&result.RowCost14==0x4000000&&result.RowField22==uint.MaxValue&&result.FallbackFlag==0&&result.ResultFlag8==0,"Fresh no-winner publication differs.");
    else
    {
        Require(result.HasWinner&&result.Ordinal==expected[0].GetInt32()&&result.RelativeCost==expected[1].GetInt32()&&
            result.TotalCost==expected[2].GetInt32()&&result.RowCost14==expected[3].GetInt32()&&result.RowField22==expected[4].GetUInt32()&&
            result.FallbackFlag==expected[5].GetByte()&&result.ResultFlag8==expected[6].GetByte(),"Independent fresh invocation selection differs.");winners++;
    }
    count++;
}
Require(winners>0&&winners<count,"Independent cases must exercise both actual invocation outcomes.");
int guards=0;
Reject(()=>LegacyOwnedFreshMainTerminalInvocation.Prepare(policy,new CancellationToken(true)));guards++;
Reject(()=>invocation.SelectPreparedRows(new byte[1],0,0));guards++;
Reject(()=>invocation.SelectPreparedRows([],1,0));guards++;
Reject(()=>invocation.SelectPreparedRows([],0,0,new CancellationToken(true)));guards++;
using(var caseFile=File.OpenRead(args[5]))using(var publicationFile=File.OpenRead(args[6]))
{
    var caseDictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,fixture.Model,fixture.Sampled,
        LegacyNativeCaseData.Load(caseFile),LegacyNativeTextPublicationData.Load(publicationFile));
    var caseOwner=new LegacyOwnedMainWordSpanPreparation(ak,caseDictionary,fixture.Sampled,fixture.Table);
    var casePolicy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,caseOwner);
    Require(caseOwner.IsCaseAware,"Actual case guard fixture lost its scope.");
    Reject(()=>LegacyOwnedFreshMainTerminalInvocation.Prepare(casePolicy));guards++;
}
Console.WriteLine($"PASS{count} independent fresh terminal invocations/{winners} prepared winners; actual7true rescueinactive andEC0single iteration; {guards} guards. Source rows/cumulative2520 remain explicit.");
static void Require(bool condition,string text){if(!condition)throw new InvalidDataException(text);}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){return;}throw new InvalidDataException("Expected refusal.");}
