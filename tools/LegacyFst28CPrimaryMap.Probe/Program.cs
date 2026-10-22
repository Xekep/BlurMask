using InkRecognition;
using System.Text.Json;

if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
void Require(bool ok,string name){if(!ok)throw new Exception(name);}
var guards=new List<string>();var canceled=new CancellationToken(true);
void Refuse<T>(string name,Action call)where T:Exception
{try{call();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal: "+name);}
byte[] lk=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-lk-text.lite.res"));
byte[] ak=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));
var root=LegacyLinguisticRoot13FWireLayout.Read(lk);var raw=LegacyFst23CWireLayout.ReadActualRussianNumber(root);
var outgoing=LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,raw,ak);
var map=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak,LegacyFst28CPrimaryAlphabetMap.PreparationWork);
Require(ReferenceEquals(map.Source,outgoing)&&ReferenceEquals(map.Source.SourceRoot,root)&&ReferenceEquals(map.Source.SourceChild,raw.SourceChild),"Source ownership changed");guards.Add("exact-source-root-child-identity");
Require(map.SourceLabelCount==16&&map.AcousticLabelCount==192&&map.SerializedSourceFlags68==0x101&&map.SourceAlphabetPayloadOffset==299110&&map.SourceAlphabetPayloadEnd==299573,"Source dimensions differ");guards.Add("actual-source-alphabet-provenance");
var otherRoot=LegacyLinguisticRoot13FWireLayout.Read(lk);var otherRaw=LegacyFst23CWireLayout.ReadActualRussianNumber(otherRoot);
var other=LegacyFst13AOutgoingSource.CreateActualRussianNumber(otherRoot,otherRaw,ak);
var otherMap=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(other,ak);
Require(ReferenceEquals(otherMap.Source,other)&&!ReferenceEquals(otherMap.Source,map.Source)&&!ReferenceEquals(otherMap.Source.SourceRoot,map.Source.SourceRoot)&&otherMap.CopyEntries().SequenceEqual(map.CopyEntries()),"Equal-content owners coalesced");guards.Add("equal-content-independent-owner-identity");
Refuse<ArgumentException>("foreign-root-outgoing-refused",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(otherRoot,raw,ak));
var diagnostic=LegacyFst13AOutgoingSource.CreateDiagnostic(raw,outgoing.CopyCostTable()!);
Refuse<ArgumentException>("diagnostic-source-refused",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(diagnostic,ak));
var nullCosts=LegacyFst13AOutgoingSource.CreateDiagnostic(raw,[]);
Refuse<ArgumentException>("diagnostic-null-cost-source-refused",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(nullCosts,ak));
var genericRaw=LegacyFst23CWireLayout.ReadInline(raw.CopyInlineBytes(),299576);
var genericSource=LegacyFst13AOutgoingSource.CreateDiagnostic(genericRaw,outgoing.CopyCostTable()!);
Refuse<ArgumentException>("generic-wire-source-refused",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(genericSource,ak));
var wrongRoot=lk.ToArray();wrongRoot[20]^=1;
Refuse<ArgumentException>("wrong-root-actual-adapter-refused",()=>LegacyFst23CWireLayout.ReadActualRussianNumber(LegacyLinguisticRoot13FWireLayout.Read(wrongRoot)));
Refuse<ArgumentException>("wrong-ak-length",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak.AsSpan(1)));
var wrong=ak.ToArray();wrong[5233]^=1;
Refuse<ArgumentException>("wrong-ak-source-kind-content",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,wrong));
wrong=ak.ToArray();wrong[99]^=1;
Refuse<ArgumentException>("wrong-ak-alphabet-content",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,wrong));
Refuse<ArgumentNullException>("null-source",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(null!,ak));
Refuse<ArgumentOutOfRangeException>("negative-preparation-budget",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak,-1));
Refuse<ArgumentOutOfRangeException>("excess-preparation-budget",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak,int.MaxValue));
Refuse<InvalidDataException>("insufficient-preparation-budget",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak,LegacyFst28CPrimaryAlphabetMap.PreparationWork-1));
Refuse<OperationCanceledException>("cancel-create",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak,token:canceled));
Refuse<OperationCanceledException>("cancel-null-first",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(null!,[],token:canceled));
Refuse<OperationCanceledException>("cancel-invalid-snapshot-first",()=>LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,[],token:canceled));
Refuse<OperationCanceledException>("cancel-copy",()=>map.CopyEntries(canceled));
foreach(ushort label in new ushort[]{16,192,255,65535})Refuse<ArgumentOutOfRangeException>("label-"+label,()=>map.MapLabel(label));
var baseline=map.CopyEntries();var changed=map.CopyEntries();Array.Fill(changed,default);
Require(map.CopyEntries().SequenceEqual(baseline)&&map.MapLabel(0).Code==0xfffd,"Output entries alias");guards.Add("defensive-entry-copy");
Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);GC.Collect();GC.WaitForPendingFinalizers();
Require(map.CopyEntries().SequenceEqual(baseline)&&ReferenceEquals(map.Source,outgoing),"Caller mutation/GC changed map");guards.Add("caller-snapshot-mutation-gc");
Parallel.For(0,1024,i=>{ushort id=(ushort)(i%16);Require(map.MapLabel(id)==baseline[id]&&map.CopyEntries().SequenceEqual(baseline),"Parallel immutable lookup changed");});guards.Add("parallel-immutable-1024");
Require(map.MapLabel(0).Code!=map.MapLabel(1).Code&&map.MapLabel(0).Kind==LegacyFst28CPrimaryCodeKind.Special&&map.MapLabel(1).Kind==LegacyFst28CPrimaryCodeKind.AcousticLabel,"NUL became acoustic digit0");guards.Add("nul-versus-digit-zero");
Require(map.MapLabel(11).Code==0xfffb&&map.MapLabel(11).Code!=180&&map.MapLabel(11).Kind==LegacyFst28CPrimaryCodeKind.Special,"Space became literal acoustic180");guards.Add("space-special-precedence");
using var stream=File.Create(args[1]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteNumber("schema",1);w.WriteNumber("preparationWork",LegacyFst28CPrimaryAlphabetMap.PreparationWork);w.WriteString("acousticSha",map.AcousticFingerprint.ToLowerInvariant());w.WriteString("rootSha",map.Source.SourceRoot!.Fingerprint.ToLowerInvariant());w.WriteNumber("sourceFlags68",map.SerializedSourceFlags68);w.WriteNumber("sourceAlphabetOffset",map.SourceAlphabetPayloadOffset);w.WriteNumber("sourceAlphabetEnd",map.SourceAlphabetPayloadEnd);w.WriteNumber("sourceLabels",map.SourceLabelCount);w.WriteNumber("acousticLabels",map.AcousticLabelCount);
w.WriteStartArray("entries");foreach(var row in map.CopyEntries()){w.WriteStartArray();w.WriteNumberValue(row.SourceLabel);w.WriteNumberValue(row.Code);w.WriteStringValue(row.Kind.ToString());w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("states");int rows=0;
for(uint state=0;state<outgoing.StateCount;state++)
{
 var result=outgoing.Query(state);w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("outputFlag",result.OutputFlag);w.WriteStartArray("rows");
 for(int i=0;i<result.Count;i++)
 {
  var rawRow=result.GetRow(i);var code=map.MapLabel(rawRow.SourceLabel);
  // Probe-level same-source projection only. No API accepts a detached query
  // result and grants authority; no state0/paging/history policy is implemented.
  w.WriteStartArray();w.WriteNumberValue(i);w.WriteNumberValue(rawRow.TargetState);w.WriteNumberValue(rawRow.SourceLabel);w.WriteNumberValue(code.Code);w.WriteStringValue(code.Kind.ToString());w.WriteNumberValue(rawRow.Cost);w.WriteNumberValue(rawRow.TargetSignedByte6);w.WriteNumberValue(rawRow.TargetByte7);w.WriteEndArray();rows++;
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("stateCount",outgoing.StateCount);w.WriteNumber("rowCount",rows);w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteBoolean("secondaryImplemented",false);w.WriteBoolean("query28CImplemented",false);w.WriteBoolean("historyAssumedEmpty",false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS primary28C map:16 labels/98 states/{rows} projected rows/{guards.Count} guards; actual typed source, NUL/space specials; no secondary/cursor/history/query.");
