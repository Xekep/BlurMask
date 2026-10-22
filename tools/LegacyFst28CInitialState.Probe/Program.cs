using InkRecognition;
using System.Text.Json;
using System.Runtime.CompilerServices;
if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
void Require(bool ok,string name){if(!ok)throw new Exception(name);}
var guards=new List<string>();var canceled=new CancellationToken(true);
void Refuse<T>(string name,Action call)where T:Exception{try{call();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal: "+name);}
var lk=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-lk-text.lite.res"));var ak=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));
LegacyFst28CTargetPreparation Target(LegacyLinguisticRoot13FWireLayout root)
{
 var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(root);var source=LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak);
 return LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(source,ak),ak);
}
var root=LegacyLinguisticRoot13FWireLayout.Read(lk);var target=Target(root);
var value=LegacyFst28CInitialState.CreateActualRussianNumberFresh(target);
Require(ReferenceEquals(value.TargetPreparation,target)&&ReferenceEquals(value.TargetPreparation.PrimaryMap.Source.SourceRoot,root),"Exact target/source identity lost");guards.Add("same-typed-target-source-root");
var again=LegacyFst28CInitialState.CreateActualRussianNumberFresh(target);var foreignTarget=Target(LegacyLinguisticRoot13FWireLayout.Read(lk));var foreign=LegacyFst28CInitialState.CreateActualRussianNumberFresh(foreignTarget);
Require(!ReferenceEquals(Access.Cohort(value),Access.Cohort(again))&&!ReferenceEquals(Access.Cohort(value),Access.Cohort(foreign))&&ReferenceEquals(again.TargetPreparation,target)&&ReferenceEquals(foreign.TargetPreparation,foreignTarget),"Fresh cohorts coalesced");guards.Add("per-owner-private-cohort");
Require(foreign.CopyAuxiliarySlots().SequenceEqual(value.CopyAuxiliarySlots())&&!ReferenceEquals(foreign.TargetPreparation.PrimaryMap.Source.SourceRoot,root),"Foreign equal roots mixed");guards.Add("equal-bytes-independent-retained-owners");
var diagnostic=LegacyFst13AOutgoingSource.CreateDiagnostic(LegacyFst23CWireLayout.ReadActualRussianNumber(root),target.PrimaryMap.Source.CopyCostTable()!);
Refuse<ArgumentException>("diagnostic-cannot-acquire-primary",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(diagnostic,ak));
Refuse<ArgumentNullException>("null-target",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(null!));
Refuse<ArgumentOutOfRangeException>("negative-byte-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,-1));
Refuse<ArgumentOutOfRangeException>("excess-byte-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,int.MaxValue));
Refuse<InvalidDataException>("short-byte-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,LegacyFst28CInitialState.LogicalByteEnvelope-1));
Refuse<ArgumentOutOfRangeException>("negative-work-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,workBudget:-1));
Refuse<ArgumentOutOfRangeException>("excess-work-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,workBudget:int.MaxValue));
Refuse<InvalidDataException>("short-work-budget",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,workBudget:LegacyFst28CInitialState.WorkEnvelope-1));
Refuse<OperationCanceledException>("cancel-create",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(target,token:canceled));
Refuse<OperationCanceledException>("cancel-null-first",()=>LegacyFst28CInitialState.CreateActualRussianNumberFresh(null!,-1,-1,canceled));
Refuse<OperationCanceledException>("cancel-slots",()=>value.CopyAuxiliarySlots(canceled));Refuse<OperationCanceledException>("cancel-degrees",()=>value.CopyDegrees(canceled));
foreach(int i in new[]{-1,8,int.MaxValue})Refuse<ArgumentOutOfRangeException>("cursor-"+i,()=>value.ReadCursor(i));
foreach(ushort i in new ushort[]{16,192,65535})Refuse<ArgumentOutOfRangeException>("source-label-"+i,()=>value.ReadAuxiliarySlot(i));
var keys=new List<ushort>();var diagnosticSequence=new ushort[]{4,7,4,0,65531,7,0};var diagnosticIds=diagnosticSequence.Select(key=>Access.Find(value,keys,key)).ToArray();
Require(diagnosticIds.SequenceEqual(new[]{0,1,0,2,3,1,2})&&keys.SequenceEqual(new ushort[]{4,7,0,65531}),"Stable duplicate keys changed");guards.Add("private-key-helper-duplicate-existing-identities");
var full=Enumerable.Range(0,15).Select(i=>(ushort)i).ToList();Require(Access.Find(value,full,4)==4&&full.Count==15,"Full existing key incorrectly refused");guards.Add("full-existing-key-no-allocation");
Refuse<InvalidDataException>("key-slot-capacity",()=>Access.Find(value,full,100));Refuse<ArgumentException>("oversize-private-key-domain",()=>Access.Find(value,Enumerable.Range(0,16).Select(i=>(ushort)i).ToList(),0));
var slots=value.CopyAuxiliarySlots();var degrees=value.CopyDegrees();var copy=value.CopyAuxiliarySlots();Array.Fill(copy,default);var changed=value.CopyDegrees();Array.Fill(changed,-999);
Require(value.CopyAuxiliarySlots().SequenceEqual(slots)&&value.CopyDegrees().SequenceEqual(degrees),"Defensive snapshots escaped");guards.Add("defensive-slot-degree-copies");
Require(slots.Length==15&&!slots.Any(s=>s.HasHistoryValue||s.HasSecondaryValue)&&value.ReadAuxiliarySlot(0)is null&&value.ReadAuxiliarySlot(1)==0,"Empty auxiliary substituted");guards.Add("nonnull-fifteen-slots-two-null-payloads");
Require(value.Capacity==12&&value.CopyDegrees()[0]==1&&value.NextCursor==0&&value.ReadInitialRootRow().Target==97&&value.ReadCursor(0).Count==1,"Capacity/state0/pool confused");guards.Add("capacity12-degree1-next0");
Array.Fill(lk,(byte)0);Array.Fill(ak,(byte)0);GC.Collect();GC.WaitForPendingFinalizers();Require(value.CopyAuxiliarySlots().SequenceEqual(slots)&&value.CopyDegrees().SequenceEqual(degrees),"Caller mutation escaped");guards.Add("caller-mutation-gc");
Parallel.For(0,1024,i=>Require(value.ReadCursor(i%8)==again.ReadCursor(i%8)&&value.ReadInitialRootRow()==again.ReadInitialRootRow()&&value.CopyAuxiliarySlots().SequenceEqual(slots),"Parallel immutable changed"));guards.Add("parallel-immutable-1024");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteNumber("schema",1);w.WriteNumber("byteEnvelope",LegacyFst28CInitialState.LogicalByteEnvelope);w.WriteNumber("workEnvelope",LegacyFst28CInitialState.WorkEnvelope);
w.WriteString("rootSha",root.Fingerprint.ToLowerInvariant());w.WriteString("acousticSha",target.PrimaryMap.AcousticFingerprint.ToLowerInvariant());w.WriteString("sourceName",value.DecodedSourceName);w.WriteNumber("capacity",value.Capacity);w.WriteNumber("nextCursor",value.NextCursor);w.WriteString("initialCallback",value.InitialCallback);w.WriteNumber("initialAlternative",value.InitialAlternative);
w.WriteStartArray("slots");foreach(var s in slots){w.WriteStartArray();w.WriteNumberValue(s.Slot);w.WriteNumberValue(s.Key);w.WriteBooleanValue(s.HasHistoryValue);w.WriteBooleanValue(s.HasSecondaryValue);w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("sourceSlots");for(ushort i=0;i<16;i++){if(value.ReadAuxiliarySlot(i)is{}slot)w.WriteNumberValue(slot);else w.WriteNullValue();}w.WriteEndArray();
w.WriteStartArray("cursors");for(int i=0;i<8;i++){var c=value.ReadCursor(i);w.WriteStartArray();foreach(int x in new[]{c.Slot,c.LogicalOffset,c.State,c.Context0,c.Context1,c.Count,c.PhysicalOffset,c.AcceptedOffset})w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("degrees");foreach(int n in degrees)w.WriteNumberValue(n);w.WriteEndArray();var r=value.ReadInitialRootRow();w.WriteStartArray("rootRow");foreach(long x in new long[]{r.Target,r.SourceLabel,r.Primary,r.Secondary,r.Cost,r.TargetSignedByte6,r.TargetByte7})w.WriteNumberValue(x);if(r.AuxiliarySlot is{}id)w.WriteNumberValue(id);else w.WriteNullValue();w.WriteEndArray();
w.WriteStartArray("duplicateKeySequence");foreach(ushort key in diagnosticSequence)w.WriteNumberValue(key);w.WriteEndArray();w.WriteStartArray("duplicateKeyIds");foreach(int i in diagnosticIds)w.WriteNumberValue(i);w.WriteEndArray();
w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteBoolean("query28CImplemented",false);w.WriteBoolean("historyConsumerImplemented",false);w.WriteBoolean("emptyAuxiliaryAssumed",false);w.WriteBoolean("unusedNativeBytesExposed",false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS initial28C15auxslots/8cursors/98degrees/state0degree1/{guards.Count}guards; privatefreshcohort, noquery/history.");
static class Access
{
 [UnsafeAccessor(UnsafeAccessorKind.Field,Name="cohort")]internal static extern ref object Cohort(LegacyFst28CInitialState owner);
 [UnsafeAccessor(UnsafeAccessorKind.StaticMethod,Name="FindOrInsertKey")]internal static extern int Find(LegacyFst28CInitialState? owner,List<ushort> keys,ushort key);
}
