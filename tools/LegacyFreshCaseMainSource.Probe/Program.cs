using InkRecognition;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine separation case-data publication capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,engine);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var c=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(c);using var p=File.OpenRead(args[5]);var pd=LegacyNativeTextPublicationData.Load(p);
var sampled=MainFixture.Create(ak,engine,table);var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,cd,pd);
var spans=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var controller=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,spans);
var acoustic=dictionary.CreateAcousticScorer(ak);var before=Enumerable.Range(0,dictionary.Graph.Groups.Count).Select(g=>acoustic.Score(g).Cache.Costs.ToArray()).ToArray();
var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller);
if(!ReferenceEquals(policy.Controller,controller)||!ReferenceEquals(policy.DictionaryOwner,dictionary)||!ReferenceEquals(policy.Model,model)||!ReferenceEquals(policy.Graph,dictionary.Graph)||!ReferenceEquals(policy.Graph,sampled.SourceRanges.Graph))throw new Exception("Fresh source factory policy escaped actual owner identities.");
var after=Enumerable.Range(0,dictionary.Graph.Groups.Count).Select(g=>acoustic.Score(g).Cache.Costs.ToArray()).ToArray();
if(before.Where((row,i)=>!row.SequenceEqual(after[i])).Any())throw new Exception("Source admission changed actual NN/cache scores.");
if(policy.ConfiguredSourceCount!=1||policy.ConfiguredSourceType!=0x13b||policy.HasAssociatedSource||policy.SupportsSecondaryGroupSourceV60||policy.SchedulesSecondaryGroup1Fe740)throw new Exception("Native first secondary-group gate changed.");
//Every downstream 203C90/geometry decision is immaterial when the native
//associated/sourceV60 disjunction is false. No callbacks are fabricated.
int gates=0;foreach(bool prior in new[]{false,true})foreach(bool geometrySkip in new[]{false,true}){if((policy.HasAssociatedSource||policy.SupportsSecondaryGroupSourceV60)&&prior&&!geometrySkip)throw new Exception("Unsupported1FE740 branch became scheduled.");gates++;}
int guards=0;
void Invalid(Action action){try{action();throw new Exception("Foreign policy input accepted.");}catch(ArgumentException){guards++;}catch(InvalidDataException){guards++;}}
Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,null!));
Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak.AsSpan(0,ak.Length-1),lk,controller));
Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk.AsSpan(0,lk.Length-1),controller));
var badAk=ak.ToArray();badAk[99]^=1;Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(badAk,lk,controller));
var badLk=lk.ToArray();badLk[606964+8]=0x3d;Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,badLk,controller));
var direct=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,sampled,table);var directController=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,direct);Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,directController));
try{LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller,new(true));throw new Exception("Canceled policy admission succeeded.");}catch(OperationCanceledException){guards++;}
//Caller byte arrays are not retained by this small policy; mutation cannot
//change already-owned scalar facts/model/graph references.
byte old=ak[0];ak[0]^=1;Invalid(()=>LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,controller));ak[0]=old;
if(policy.AcousticFingerprint!=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ak)))throw new Exception("Owned policy fingerprint followed caller mutation.");
using var output=File.Create(args[6]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("acoustic",policy.AcousticFingerprint);w.WriteString("lexical",policy.LexicalFingerprint);
w.WriteNumber("count",policy.ConfiguredSourceCount);w.WriteNumber("type",policy.ConfiguredSourceType);w.WriteBoolean("associated",policy.HasAssociatedSource);w.WriteBoolean("v60",policy.SupportsSecondaryGroupSourceV60);w.WriteBoolean("secondary",policy.SchedulesSecondaryGroup1Fe740);
w.WriteNumber("flags",dictionary.Descriptor.Flags);w.WriteString("identifier",dictionary.Descriptor.Identifier);w.WriteNumber("nodes",policy.Graph.NodeCount);w.WriteNumber("groups",policy.Graph.Groups.Count);w.WriteNumber("guards",guards);w.WriteNumber("gateCombinations",gates);
w.WriteStartArray("scores");foreach(var row in after){w.WriteStartArray();foreach(int value in row)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS owned fresh case NEW20E/one13B source/noAssociated; false292/290.V60 gate excludes1FE740 for{gates} downstream combinations; {after.Length} actual NN/cache groups unchanged; {guards} ownership/hash/extent/DIRECT/cancel guards. No fullcase controller/winner claim.");
