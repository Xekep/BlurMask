using System.Text.Json;
using System.Security.Cryptography;
using System.Buffers.Binary;
using InkRecognition;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication-data output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var sampled=MainFixture.Create(ak,engine,table);
var owner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var resolver=LegacyOwnedRussianGlyphSymbolResolver.Prepare(owner,owner.Symbols.Preparation);
Require(ReferenceEquals(resolver.DictionaryOwner,owner)&&ReferenceEquals(resolver.Preparation,owner.Symbols.Preparation)&&
    ReferenceEquals(resolver.Alphabet,model.AcousticAlphabet),"Resolver lost its actual preparation owner.");
Require(model.AcousticAlphabet.TryGetLabel("\ufffd",out ushort replacement),"Replacement label missing.");
ulong[] conversions=[0,1,2,255,256,257,ulong.MaxValue];
var rows=new List<(ushort Input,ulong Conversion,bool Knowledge,ushort Output,ushort Original)>();int rejected=0,guards=0;
for(int i=0;i<model.AcousticAlphabet.LabelCount;i++)foreach(ulong conversion in conversions)foreach(bool knowledge in new[]{false,true})
{
    ushort symbol=(ushort)i;
    if(symbol==replacement&&!knowledge){Reject(()=>resolver.Resolve(symbol,conversion));rejected++;continue;}
    var result=resolver.Resolve(symbol,conversion,knowledge);rows.Add((symbol,conversion,knowledge,result.EmittedSymbol,result.NormalizedOriginal));
}
foreach(ushort symbol in new[]{resolver.SyntheticApostrophe,resolver.SyntheticQuote})foreach(ulong conversion in conversions)
foreach(bool knowledge in new[]{false,true})
{
    var result=resolver.Resolve(symbol,conversion,knowledge);rows.Add((symbol,conversion,knowledge,result.EmittedSymbol,result.NormalizedOriginal));
}
Reject(()=>LegacyOwnedRussianGlyphSymbolResolver.Prepare(null!,owner.Symbols.Preparation));
Reject(()=>LegacyOwnedRussianGlyphSymbolResolver.Prepare(owner,null!));
var otherPreparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication),publication);
Reject(()=>LegacyOwnedRussianGlyphSymbolResolver.Prepare(owner,otherPreparation));
//Equal resource bytes and equal tables do not establish preparation identity.
var otherModel=LegacyRussianModelBundle.Load(ak,lk,engine);
var foreignPreparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(otherModel.AcousticAlphabet,caseData,publication),publication);
Reject(()=>LegacyOwnedRussianGlyphSymbolResolver.Prepare(owner,foreignPreparation));
//Compact exhaustive ushort coverage. Protocol/sentinel values are passed
//through at216860; this is not an emitted UTF8/text or216BC0 classification.
using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);byte[] record=new byte[8];int exhaustive=0;
for(int symbol=0;symbol<=ushort.MaxValue;symbol++)for(ushort conversion=0;conversion<2;conversion++)
{
    if(symbol==replacement)continue;
    var result=resolver.Resolve((ushort)symbol,conversion);
    BinaryPrimitives.WriteUInt16LittleEndian(record,(ushort)symbol);BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(2),conversion);
    BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4),result.EmittedSymbol);BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(6),result.NormalizedOriginal);
    hash.AppendData(record);exhaustive++;
}
string exhaustiveHash=Convert.ToHexString(hash.GetHashAndReset());
Require(model.AcousticAlphabet.TryGetLabel("a",out ushort plain),"Actual ordinary label absent.");
Reject(()=>LegacyOwnedRussianGlyphSymbolResolver.Prepare(owner,owner.Symbols.Preparation,new(true)));
Reject(()=>resolver.Resolve(plain,0,false,new(true)));Reject(()=>resolver.Resolve(resolver.SyntheticQuote,1,false,new(true)));
using(var file=File.Create(args[6]))using(var writer=new Utf8JsonWriter(file))
{
    writer.WriteStartObject();writer.WriteNumber("labels",model.AcousticAlphabet.LabelCount);writer.WriteNumber("replacement",replacement);
    writer.WriteNumber("exhaustiveUshortRows",exhaustive);writer.WriteString("exhaustiveSha256",exhaustiveHash);writer.WriteNumber("guards",guards);writer.WriteNumber("rejectedDomainRows",rejected);writer.WriteStartArray("rows");
    foreach(var r in rows){writer.WriteStartArray();writer.WriteNumberValue(r.Input);writer.WriteNumberValue(r.Conversion);writer.WriteBooleanValue(r.Knowledge);writer.WriteNumberValue(r.Output);writer.WriteNumberValue(r.Original);writer.WriteEndArray();}
    writer.WriteEndArray();writer.WriteEndObject();writer.Flush();
}
Console.WriteLine($"PASS {exhaustive} exhaustive ushort rows +{rows.Count} real-AL/synthetic conversion scalar controls +{guards} policy/domain/foreign-owner/cancel refusals; no native glyph/context/winner claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid resolver scope accepted.");}
static void Require(bool value,string message){if(!value)throw new Exception(message);}
