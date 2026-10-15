using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=5)throw new ArgumentException("controls AK LK engine separation required");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));var expected=document.RootElement;
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]),engine=File.ReadAllBytes(args[3]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);
Require(model.LanguageModel.GetStoredFinalClassLevel(false)==expected.GetProperty("finalUni").GetInt32(),"Final unigram codebook selection differs.");
Require(model.LanguageModel.GetStoredFinalClassLevel(true)==expected.GetProperty("finalBi").GetInt32(),"Final bigram codebook selection differs.");
Require(model.LanguageModel.GetStoredInitialClassLevel(false)==expected.GetProperty("initialUni").GetInt32(),"Existing initial codebook getter changed.");
int controls=0;
foreach(var test in expected.GetProperty("controls").EnumerateArray())
{
    float slope=test[0].GetSingle(),offset=test[1].GetSingle(),scale=test[2].GetSingle();
    //Word and bigram coefficients deliberately disagree withunigram fields.
    var profile=new LegacyWordLanguageCostProfile(-11,23,slope,offset,19,-31);
    int value=LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,profile,scale);
    Require(value==test[3].GetInt32(),"Independent final-unigram scalar conversion differs.");controls++;
}
foreach(var test in expected.GetProperty("actual").EnumerateArray())
{
    int value=LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
        model.LanguageProfiles[test[0].GetInt32()],test[1].GetSingle());
    Require(value==test[2].GetInt32(),"Actual stored profile conversion differs.");controls++;
}
var fixture=MainFixture.Create(ak,lk,engine,File.ReadAllBytes(args[4]));
var terminal=LegacyOwnedRussianTerminalSources.Prepare(fixture.Dictionary);
int actualPenalty=expected.GetProperty("penalty").GetInt32();
Require(ReferenceEquals(terminal.DictionaryOwner,fixture.Dictionary)&&terminal.LanguageProfileIndex==2&&
    terminal.RuntimeLanguageScale==expected.GetProperty("resourceScale").GetSingle()&&
    terminal.LanguagePenalty2534==actualPenalty,"Owned source-kind/profile/loadedscale binding differs.");
var sources=terminal.CopySources();
Require(sources.Length==2&&sources[0].Header==0&&sources[1].Header==fixture.Dictionary.Descriptor.Flags&&
    sources[1].LanguagePenalty2534==actualPenalty,"Actual selected source table differs.");
sources[1]=default;Require(terminal.CopySources()[1].LanguagePenalty2534==actualPenalty,"Mutable source array escaped.");
byte[] row=new byte[88];row[4]=1;
BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(16),123);
BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(20),-456);
BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(48),-1);
BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(34),ushort.MaxValue);
var winner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(row,1,terminal.CopySources(),
    new(1,false,true,false,false,77),987,fixture.Dictionary.Contexts);
Require(winner.HasWinner&&winner.Ordinal==0&&winner.RelativeCost==unchecked(123+expected.GetProperty("terminalDelta").GetInt32())&&
    winner.TotalCost==unchecked(winner.RelativeCost+987)&&winner.RowCost14==-456&&winner.RowField22==65535&&
    winner.FallbackFlag==1&&winner.ResultFlag8==77,"Actual derived source penalty did not reach2257C0.");
int guards=0;
foreach(float invalid in new[]{0f,-1,float.NaN,float.PositiveInfinity})
{Reject(()=>LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,model.LanguageProfiles[2],invalid));guards++;}
Reject(()=>LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
    new(1,1,float.MaxValue,0,1,1),float.MaxValue));guards++;
Reject(()=>LegacyOwnedRussianTerminalSources.Prepare(fixture.Dictionary,new CancellationToken(true)));guards++;
Reject(()=>LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,model.LanguageProfiles[2],1,new CancellationToken(true)));guards++;
Console.WriteLine($"PASS{controls} independent scalar/actual profile controls; actual ownedRU finalunigram{terminal.StoredFinalUnigramLevel}, scale{terminal.RuntimeLanguageScale}, penalty{actualPenalty} ->2257C0; {guards} guards.");
static void Require(bool condition,string text){if(!condition)throw new InvalidDataException(text);}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){return;}throw new InvalidDataException("Expected refusal.");}
