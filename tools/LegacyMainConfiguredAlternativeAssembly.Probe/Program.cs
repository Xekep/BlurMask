using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 7) throw new ArgumentException("AK LK engine separation case publication capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]);
using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf);
var publication = LegacyNativeTextPublicationData.Load(pf);
var sampled = MainFixture.Create(ak, engine, table);
int guards = 0;
var foreign = Create();

using var output = File.Create(args[6]);
using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject();
writer.WriteStartArray("runs");
for (int n = 0; n < 4; n++)
{
    var (compact, geometry, callbacks, interior) = Create();
    byte[] word = Bytes(120, 37, n);
    foreach (var (at, size) in new[] { (32, 8), (88, 12), (104, 12) }) word.AsSpan(at, size).Clear();
    byte[] glyph = Bytes(40, 17, n), backtrack = Bytes(72, 29, n), request = Bytes(64, 19, n);
    ushort[] labels = Enumerable.Range(0, 256).Select(i => (ushort)(i * 13 + n)).ToArray();
    byte[] leading = [1, 2, 3, (byte)n], trailing = [5, 6, 7, (byte)n];
    var residue = new LegacyMainConfiguredAlternativeResidue(word, backtrack, glyph, labels, request,
        leading, trailing, new byte[] { 9, 10, 11, (byte)n }, new byte[] { 13, 14, 15, (byte)n });
    int allocations = 0;
    byte[] Allocate(LegacyMainAlternativeAllocationRequest r)
    {
        allocations++;
        byte[] value = Bytes(r.RequiredBytes, 23, n);
        r.PreviousStorage.Span.CopyTo(value);
        return value;
    }
    Reject(() => LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(new byte[ak.Length], compact,
        geometry, callbacks, interior, table, Allocate, residue));
    Reject(() => LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, compact,
        geometry, callbacks, interior, table, Allocate, residue, new(true)));
    var assembly = LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, compact,
        geometry, callbacks, interior, table, Allocate, residue);
    if (n == 0)
    {
        Reject(() => LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, compact,
            foreign.Geometry, callbacks, interior, table, Allocate, residue));
        Reject(() => new LegacyMainConfiguredAlternativeResidue(new byte[119], backtrack, glyph, labels, request,
            leading, trailing, new byte[4], new byte[4]));
        Reject(() => new LegacyMainConfiguredAlternativeResidue(Bytes(120, 1, 1), backtrack, glyph, labels, request,
            leading, trailing, new byte[4], new byte[4]));
    }
    Reject(() => assembly.Run(new(true)));
    assembly.Run();
    Reject(() => assembly.Run());
    var competing = LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, compact,
        geometry, callbacks, interior, table, Allocate, residue);
    Reject(() => competing.Run());
    var active = assembly.CopyActiveAlternatives();
    if (!assembly.IsComplete || assembly.IsFaulted || active.Length != 2 || allocations != 1 || assembly.OutputIndex != 2)
        throw new Exception("Actual configured pass did not complete two distinct native alternatives.");
    if (!ReferenceEquals(active[0].Ownership.TopOwner, active[1].Ownership.TopOwner) ||
        !ReferenceEquals(active[0].Ownership.GlyphPool, active[1].Ownership.GlyphPool))
        throw new Exception("Alternatives lost actual shared TOP/275/pool ownership.");
    byte[] escaped = assembly.CopyStorage();
    escaped[0] ^= 255;
    if (escaped.SequenceEqual(assembly.CopyStorage())) throw new Exception("Storage getter exposed writable ownership.");

    var final = compact.GetNodeSnapshot(compact.PassResult.NodeCount - 1);
    var controller = compact.PassResult.SourcePolicy.Controller;
    writer.WriteStartObject();
    writer.WriteNumber("seed", n);
    writer.WriteNumber("sourceCount", compact.PassResult.NodeCount);
    writer.WriteNumber("sourceEnd", final.SourceEnd23f0);
    writer.WriteNumber("primary", controller.Configuration.PrimaryArcLimit230a);
    writer.WriteNumber("cumulative", final.CumulativePathCost2520);
    writer.WriteBase64String("records", final.Records.Span);
    writer.WriteBase64String("secondary", final.SecondaryAlternatives.Span);
    writer.WriteBase64String("storage", assembly.CopyStorage());
    writer.WriteNumber("reference", assembly.Reference78);
    writer.WriteNumber("top80bits", BitConverter.SingleToUInt32Bits(assembly.FinalScores!.Top80));
    writer.WriteNumber("outputIndex", assembly.OutputIndex);
    writer.WriteStartArray("excluded"); foreach (ulong id in assembly.CopyExcludedIdentities()) writer.WriteNumberValue(id); writer.WriteEndArray();
    writer.WriteStartArray("contexts");
    foreach (var context in controller.Owner.Contexts.CopyNodes())
    { writer.WriteStartArray(); writer.WriteNumberValue(context.Identity); writer.WriteNumberValue(context.ParentLink); writer.WriteNumberValue(context.Symbol); writer.WriteEndArray(); }
    writer.WriteEndArray();
    writer.WriteStartArray("attempts");
    foreach (var attempt in assembly.CopyAttempts())
    {
        writer.WriteStartObject();
        writer.WriteNumber("outputIndex", attempt.OutputIndex);
        writer.WriteNumber("exclusionCount", attempt.ExclusionCount);
        writer.WriteBoolean("duplicate", attempt.Duplicate);
        writer.WriteNumber("duplicateOf", attempt.DuplicateOf);
        if (attempt.Candidate is { } candidate)
        {
            var selected = candidate.Terminal.Selection;
            writer.WriteNumber("ordinal", selected.Winner.Ordinal);
            writer.WriteNumber("secondaryOrdinal", selected.SecondaryOrdinal);
            writer.WriteNumber("identity", selected.ExclusionIdentity);
            writer.WriteNumber("total", selected.Winner.TotalCost);
            writer.WriteNumber("row14", selected.Winner.RowCost14);
            writer.WriteBoolean("released", candidate.IsReleased);
            writer.WriteNumber("selected", candidate.Terminal.SelectedRecordIndex);
            writer.WriteBase64String("initial", candidate.Session.CopyInitialWordStorage());
            writer.WriteStartArray("packets");
            foreach (var published in candidate.CopyPublications()) writer.WriteBase64StringValue(published.Association.Packet.CopyStorage());
            writer.WriteEndArray();
            writer.WriteStartArray("words");
            for (var node = candidate.WordHead18; node is not null; node = node.Next)
            {
                writer.WriteStartObject(); writer.WriteNumber("index", node.WordIndex);
                writer.WriteNumber("first", node.Span.First); writer.WriteNumber("last", node.Span.Last);
                writer.WriteBase64String("storage", node.Word.CopyStorage());
                writer.WriteStartArray("references"); foreach (ulong reference in node.Word.FlatReferences) writer.WriteNumberValue(reference); writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteStartArray("rawUtf8"); foreach (byte[] bytes in candidate.CopyRawUtf8Words()) writer.WriteBase64StringValue(bytes); writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
    writer.WriteEndArray();
    writer.WriteStartArray("pool");
    foreach (var node in assembly.TopOwner!.GlyphPool.CopyOwnerNodes())
    {
        writer.WriteStartObject(); writer.WriteNumber("key", node.Key);
        writer.WriteBase64String("records", node.Records.Span); writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray();

//A cancellation after allocator entry consumes this exact result pass, while
//preserving the already selected exclusion prefix; no replacement raw run is
//silently used to turn the failed operation into success.
var fault = Create();
using (var cancel = new CancellationTokenSource())
{
    var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40],
        new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
    var assembly = LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, fault.Compact,
        fault.Geometry, fault.Callbacks, fault.Interior, table, r =>
        { cancel.Cancel(); return new byte[r.RequiredBytes]; }, residue);
    Reject(() => assembly.Run(cancel.Token));
    if (!assembly.IsFaulted || assembly.IsComplete || assembly.CopyExcludedIdentities().Length != 1 || assembly.TopOwner is not null)
        throw new Exception("Allocation cancellation lost the exact pre-callback selected prefix.");
    Reject(() => assembly.Run());
}
var stale = Create();
{
    var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40],
        new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
    var assembly = LegacyOwnedMainConfiguredAlternativeAssembly.Prepare(ak, stale.Compact,
        stale.Geometry, stale.Callbacks, stale.Interior, table, r =>
        {
            stale.Compact.PassResult.SourcePolicy.Controller.Owner.AdvanceNodeEpoch(-1);
            return new byte[r.RequiredBytes];
        }, residue);
    Reject(() => assembly.Run());
    if (!assembly.IsFaulted || assembly.CopyExcludedIdentities().Length != 1 || assembly.TopOwner is not null)
        throw new Exception("Foreign allocator epoch mutation was not rejected before WORD writes.");
    Reject(() => assembly.Run());
}
writer.WriteNumber("guards", guards);
writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"PASS four actual configured inference runs -> eight sameTOP alternatives, native selection/callback/quote/equality/final scores, {guards} guards; raw UTF8 only.");

(LegacyOwnedMainSecondaryCompactWordMaterialization Compact, LegacyOwnedRussianMainSourceGeometry Geometry,
    LegacyOwnedRussianMainSourceSymbolCallbacks Callbacks, LegacyOwnedRussianMainInteriorDelimiter Interior) Create()
{
    var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
    var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
    var controller = LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(ak, owner, 2);
    var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
    ulong identity = 0;
    var pass = new LegacyOwnedMainCaseWordPass(ak, policy, table, r =>
    { byte[] data = new byte[r.RequiredBytes]; r.PreviousStorage.Span.CopyTo(data); return new(data, ++identity); }).RunOwned();
    var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary);
    var compact = LegacyOwnedMainSecondaryCompactWordMaterialization.Materialize(ak, pass, confidence,
        Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray());
    var geometry = LegacyOwnedRussianMainSourceGeometry.Prepare(ak, confidence, sampled);
    var callbacks = LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak, confidence, sampled);
    return (compact, geometry, callbacks, LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak, callbacks, geometry));
}
static byte[] Bytes(int count, int multiplier, int seed) => Enumerable.Range(0, count).Select(i => (byte)(i * multiplier + seed)).ToArray();
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid configured assembly ownership/state accepted.");
}
