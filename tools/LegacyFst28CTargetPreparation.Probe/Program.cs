using InkRecognition;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("MODELS CAPTURE");
void Require(bool ok, string name) { if (!ok) throw new Exception(name); }
var guards = new List<string>(); var canceled = new CancellationToken(true);
void Refuse<T>(string name, Action call) where T : Exception
{ try { call(); } catch (T) { guards.Add(name); return; } throw new Exception("Missing refusal: " + name); }
byte[] lk = File.ReadAllBytes(Path.Combine(args[0], "ru-RU/ru_RU-lk-text.lite.res"));
byte[] ak = File.ReadAllBytes(Path.Combine(args[0], "ru-RU/ru_RU-ak-cur.lite.res"));
var root = LegacyLinguisticRoot13FWireLayout.Read(lk);
var raw = LegacyFst23CWireLayout.ReadActualRussianNumber(root);
var outgoing = LegacyFst13AOutgoingSource.CreateActualRussianNumber(root, raw, ak);
var primary = LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing, ak);
var target = LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak);
Require(ReferenceEquals(target.PrimaryMap, primary) && ReferenceEquals(target.PrimaryMap.Source, outgoing), "Actual owner replaced"); guards.Add("exact-primary-source-root-owner");
var root2 = LegacyLinguisticRoot13FWireLayout.Read(lk);
var raw2 = LegacyFst23CWireLayout.ReadActualRussianNumber(root2);
var source2 = LegacyFst13AOutgoingSource.CreateActualRussianNumber(root2, raw2, ak);
var primary2 = LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(source2, ak);
var target2 = LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary2, ak);
Require(!ReferenceEquals(target2.PrimaryMap, primary) && ReferenceEquals(target2.PrimaryMap, primary2) &&
    target.CopyTargetFields().SequenceEqual(target2.CopyTargetFields()) && target.CopySecondary().SequenceEqual(target2.CopySecondary()), "Equal owners coalesced"); guards.Add("equal-content-independent-owner");
var duplicate = LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak);
Require(!ReferenceEquals(duplicate, target) && ReferenceEquals(duplicate.PrimaryMap, primary), "Fresh preparations coalesced"); guards.Add("fresh-policy-per-preparation");
var diagnostic = LegacyFst13AOutgoingSource.CreateDiagnostic(raw, outgoing.CopyCostTable()!);
Refuse<ArgumentException>("diagnostic-cannot-acquire-primary", () => LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(diagnostic, ak));
Refuse<ArgumentException>("foreign-root-actual-source", () => LegacyFst13AOutgoingSource.CreateActualRussianNumber(root2, raw, ak));
Refuse<ArgumentNullException>("null-primary", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(null!, ak));
Refuse<ArgumentException>("wrong-ak-length", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak.AsSpan(1)));
foreach (int at in new[] { 93, 97, 99, 5233, ak.Length - 1 })
{
    var wrong = ak.ToArray(); wrong[at] ^= 1;
    Refuse<ArgumentException>("wrong-ak-byte-" + at, () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, wrong));
}
Refuse<ArgumentOutOfRangeException>("negative-budget", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak, -1));
Refuse<ArgumentOutOfRangeException>("excess-budget", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak, int.MaxValue));
Refuse<InvalidDataException>("short-budget", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak, LegacyFst28CTargetPreparation.PreparationWork - 1));
Refuse<OperationCanceledException>("cancel-create", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, ak, token: canceled));
Refuse<OperationCanceledException>("cancel-null-first", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(null!, [], token: canceled));
Refuse<OperationCanceledException>("cancel-invalid-first", () => LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary, [], -1, canceled));
Refuse<OperationCanceledException>("cancel-fields", () => target.CopyTargetFields(canceled));
Refuse<OperationCanceledException>("cancel-ranges", () => target.CopyRangeChecks(canceled));
Refuse<OperationCanceledException>("cancel-secondary", () => target.CopySecondary(canceled));
foreach (byte slot in new byte[] { 0, 0x9e, 0xa1, 0xbb, 0xbc, 0xff }) Refuse<ArgumentOutOfRangeException>("slot-" + slot, () => target.GetTargetField(slot));
foreach (ushort id in new ushort[] { 16, 192, 65535 }) Refuse<ArgumentOutOfRangeException>("label-" + id, () => target.MapSecondary(id));
var fields = target.CopyTargetFields(); var ranges = target.CopyRangeChecks(); var secondary = target.CopySecondary();
var changedFields = target.CopyTargetFields(); Array.Fill(changedFields, default);
var changedRanges = target.CopyRangeChecks(); Array.Fill(changedRanges, default);
var changedSecondary = target.CopySecondary(); Array.Fill(changedSecondary, default);
Require(target.CopyTargetFields().SequenceEqual(fields) && target.CopyRangeChecks().SequenceEqual(ranges) && target.CopySecondary().SequenceEqual(secondary), "Output mutation escaped"); guards.Add("defensive-all-output-copies");
Require(fields.Length == 14 && ranges.Length == 59 && secondary.Length == 16, "Actual dimensions differ"); guards.Add("14-fields-59-adjacencies-16-secondary");
Require(target.MapSecondary(0).Code == 0xfffd && target.MapSecondary(1).Code == 0xffff && target.MapSecondary(11).Code == 0xfffb, "Fresh special precedence differs"); guards.Add("fresh-zero-c4-nul-space-not-digit0");
Array.Fill(ak, (byte)0); Array.Fill(lk, (byte)0); GC.Collect(); GC.WaitForPendingFinalizers();
Require(target.CopyTargetFields().SequenceEqual(fields) && target.CopySecondary().SequenceEqual(secondary), "Caller snapshot aliases"); guards.Add("caller-mutation-gc");
Parallel.For(0, 1024, i =>
{
    Require(target.GetTargetField(fields[i % 14].Slot) == fields[i % 14].Code && target.MapSecondary((ushort)(i % 16)) == secondary[i % 16] &&
        target.CopyRangeChecks().SequenceEqual(ranges), "Immutable parallel read changed");
}); guards.Add("parallel-immutable-1024");
using var stream = File.Create(args[1]); using var w = new Utf8JsonWriter(stream);
w.WriteStartObject(); w.WriteNumber("schema", 1); w.WriteNumber("preparationWork", LegacyFst28CTargetPreparation.PreparationWork);
w.WriteString("rootSha", root.Fingerprint.ToLowerInvariant()); w.WriteString("acousticSha", primary.AcousticFingerprint.ToLowerInvariant());
w.WriteStartArray("fields"); foreach (var row in fields) { w.WriteStartArray(); w.WriteNumberValue(row.Slot); w.WriteNumberValue(row.Code); w.WriteEndArray(); } w.WriteEndArray();
w.WriteStartArray("ranges"); foreach (var row in ranges) { w.WriteStartArray(); w.WriteNumberValue(row.Slot); w.WriteNumberValue((int)row.Character); w.WriteNumberValue(row.ExpectedCode); w.WriteNumberValue(row.ActualCode); w.WriteEndArray(); } w.WriteEndArray();
w.WriteStartArray("secondary"); foreach (var row in secondary) { w.WriteStartArray(); w.WriteNumberValue(row.SourceLabel); w.WriteNumberValue(row.Code); w.WriteEndArray(); } w.WriteEndArray();
w.WriteStartArray("states"); int total = 0;
for (uint state = 0; state < outgoing.StateCount; state++)
{
    var rows = outgoing.Query(state); w.WriteStartObject(); w.WriteNumber("state", state); w.WriteNumber("outputFlag", rows.OutputFlag); w.WriteStartArray("rows");
    for (int i = 0; i < rows.Count; i++)
    {
        var row = rows.GetRow(i); w.WriteStartArray(); w.WriteNumberValue(i); w.WriteNumberValue(row.TargetState); w.WriteNumberValue(row.SourceLabel);
        w.WriteNumberValue(primary.MapLabel(row.SourceLabel).Code); w.WriteNumberValue(target.MapSecondary(row.SourceLabel).Code);
        w.WriteNumberValue(row.Cost); w.WriteNumberValue(row.TargetSignedByte6); w.WriteNumberValue(row.TargetByte7); w.WriteEndArray(); total++;
    }
    w.WriteEndArray(); w.WriteEndObject();
}
w.WriteEndArray(); w.WriteNumber("rowCount", total); w.WriteStartArray("guards"); foreach (var name in guards) w.WriteStringValue(name); w.WriteEndArray();
w.WriteString("policy", "NewLibraryOwnedConstructorOnlyFreshZeroC4"); w.WriteBoolean("configuredKnowledgeClaim", false); w.WriteBoolean("query28CImplemented", false); w.WriteBoolean("historyAssumedEmpty", false); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS target28C14fields/59adjacencies/16secondary/98states/{total}projections/{guards.Count}guards; NEW fresh-policy owner, no28Cquery/history.");
