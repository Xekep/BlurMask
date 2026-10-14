using System.Text;
using System.Text.Json;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("controls.json AK LK required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, virtualSlots = 0;
foreach (var row in controls.RootElement.EnumerateArray())
{
    string[] a = row.GetProperty("acoustic").EnumerateArray().Select(x => x.GetString()!).ToArray();
    string[] l = row.GetProperty("lexical").EnumerateArray().Select(x => x.GetString()!).ToArray();
    int virtualLabel = row.GetProperty("virtual").GetInt32();
    var acoustic = LegacyInkAlphabet.Load(Alphabet(a, -1), 0x103);
    var lexical = LegacyInkAlphabet.Load(Alphabet(l, virtualLabel), 0x103);
    ushort[] residue = Values(row, "residue"), before = residue.ToArray();
    var contexts = new LegacySymbolContextFactory();
    var prepared = LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, contexts, residue);
    Require(prepared.CopyPrimary().AsSpan().SequenceEqual(Values(row, "primary")) &&
        prepared.CopyAuxiliary().AsSpan().SequenceEqual(Values(row, "auxiliary")), "Independent initial symbol maps differ.");
    Require(residue.AsSpan().SequenceEqual(before), "Allocator residue input was mutated.");
    ushort[] primary = prepared.CopyPrimary(); ulong[] ids = prepared.CopyInitialContexts();
    for (int i = 0; i < ids.Length; i++)
        if (primary[i] is ushort.MaxValue or 0xfffd) Require(ids[i] == 0, "Excluded symbol obtained a context.");
        else Require(contexts.ReadNode(ids[i]).Symbol == primary[i] && contexts.ReadNode(ids[i]).ParentLink == 0, "Initial source68 context has foreign symbol/parent.");
    Require(contexts.NodeCount == primary.Where(x => x is not (ushort.MaxValue or 0xfffd)).Distinct().Count(), "Duplicate context keys were not shared.");
    var working = prepared.CreateWorkingMap(0xc0a).Refresh(new byte[acoustic.LabelCount]);
    Require(working.CopySelectedMap(true, 0).AsSpan().SequenceEqual(Values(row, "auxiliary")), "Permissions changed native auxiliary60.");
    Require(working.CopySelectedMap(false, 0x40).AsSpan().SequenceEqual(primary), "Permissions changed preserved58.");
    Require(prepared.CreateSymbolSource().DictionaryMap.Span.SequenceEqual(primary), "Symbol enumeration used filtered50 instead of preserved58.");
    var copied = prepared.CopyAuxiliary(); copied[0] ^= 1;
    Require(prepared.CopyAuxiliary().AsSpan().SequenceEqual(Values(row, "auxiliary")), "Public copy mutated the owner.");
    if (virtualLabel >= 0)
    { Reject(() => LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, new(), [])); virtualSlots++; }
    else Require(LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, new(), []).CopyAuxiliary().AsSpan().SequenceEqual(Values(row, "auxiliary")), "Nonvirtual branch unnecessarily required allocator residue.");
    Reject(() => LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, new(), new ushort[1]));
    Reject(() => LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, new(), residue, new CancellationToken(true)));
    cases++;
}
byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]);
var actualAcoustic = LegacyInkAlphabet.Load(ak.AsSpan(99), 0x103);
var actualLexical = LegacyInkAlphabet.Load(lk.AsSpan(607074), 0x103);
var actualContexts = new LegacySymbolContextFactory();
var actual = LegacyDictionaryInitialSymbolMaps.Prepare(actualAcoustic, actualLexical, actualContexts, []);
Console.WriteLine($"ACTUAL mapped{actual.CopyPrimary().Count(x => x < actualAcoustic.LabelCount)} auxiliary{string.Join(',',actual.CopyAuxiliary().Where(x=>x!=ushort.MaxValue))} contexts{actualContexts.NodeCount}");
Require(actual.CopyPrimary().Count(x => x < actualAcoustic.LabelCount) == 138 &&
    actual.CopyAuxiliary().Where(x => x != ushort.MaxValue).SequenceEqual(new ushort[] {184,0xfffb}) && actualContexts.NodeCount == 139, "Actual Russian source50/60/68 facts differ.");
var dictionary = LegacyPackedLexicon.Load(lk.AsSpan(609980, 2513123 - 609980));
var source = actual.CreateQuerySource(dictionary, 0xc0a);
var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 256, ContextRoot = actualContexts.ContainerIdentity };
var normal = source.Read(query, false);
Require(normal.ReturnedCount == 132, "Actual owned initial maps -> normal dictionary root page differs.");
var auxiliary = source.Read(new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 256, ContextRoot = actualContexts.ContainerIdentity }, true);
Console.WriteLine($"ACTUAL root normal{normal.ReturnedCount} auxiliary{auxiliary.ReturnedCount}");
Require(auxiliary.ReturnedCount == 0, "Native auxiliary root gate unexpectedly admitted ordinary letters.");
Console.WriteLine($"PASS {cases} independent initial maps/{virtualSlots} exact virtual-residue cases; actual139 lexical symbols ->138 acoustic+space, auxiliary60 joiner184+FFFB,139 source68 contexts ->132 ordinary root records/0 auxiliary records. Normalization-disabled symbols only; source refresh/factory separate.");
static ushort[] Values(JsonElement row, string key) => row.GetProperty(key).EnumerateArray().Select(x => x.GetUInt16()).ToArray();
static byte[] Alphabet(string[] text, int virtualLabel)
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    for (int i = 0; i < 7; i++) w.Write(ushort.MaxValue);
    w.Write((ushort)text.Length); w.Write((ushort)1); w.Write((ushort)0);
    foreach (int n in new[] { 32, 64 }) for (int i = 0; i < n; i++) w.Write(i == n - 1 ? (ushort)0 : ushort.MaxValue);
    w.Write((ushort)0); w.Write((ushort)text.Length); uint offset = 0;
    for (int i = 0; i < text.Length; i++) { w.Write(offset); w.Write((uint)i); offset += (uint)Encoding.UTF8.GetByteCount(text[i]); }
    w.Write(offset); w.Write(65535u); foreach (string value in text) w.Write(Encoding.UTF8.GetBytes(value));
    for (int i = 0; i < text.Length; i++) w.Write(i == virtualLabel ? ushort.MaxValue : (ushort)i);
    w.Write((byte)0); w.Write((ushort)0); return stream.ToArray();
}
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Expected map preparation rejection."); }
