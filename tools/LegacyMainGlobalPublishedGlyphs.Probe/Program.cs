using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;


if (args.Length != 9) throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[8]), engine = File.ReadAllBytes(args[1]);
using var ed = JsonDocument.Parse(engine); var activation = ed.RootElement.GetProperty("activations");
var activations = new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v => v.GetInt32()).ToArray(), activation.GetProperty("output").EnumerateArray().Select(v => v.GetInt32()).ToArray());
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2])); using var cf = File.OpenRead(args[3]); using var pf = File.OpenRead(args[4]); var cd = LegacyNativeCaseData.Load(cf); var pd = LegacyNativeTextPublicationData.Load(pf);
var model = LegacyRussianModelBundle.Load(ak, lk, engine); byte[] line30 = File.ReadAllBytes(args[5]), line50 = File.ReadAllBytes(args[6]);
InkStroke M(float y) => new(new InkPoint[] { new(0, y + 90), new(0, y + 10), new(25, y + 60), new(50, y + 10), new(50, y + 90) });
InkStroke V(float y) => new(new InkPoint[] { new(0, y + 10), new(0, y + 90) });
var mir = new InkStroke[] { M(0), new(new InkPoint[] { new(70, 10), new(70, 90), new(120, 10), new(120, 90) }), new(new InkPoint[] { new(140, 90), new(140, 10), new(180, 10), new(190, 20), new(190, 40), new(180, 50), new(140, 50) }) };
var rows = new[] { V(0), new InkStroke(V(0).Points.Select(p => new InkPoint(p.X + 40, p.Y)).ToArray()), V(140), new InkStroke(V(140).Points.Select(p => new InkPoint(p.X + 40, p.Y)).ToArray()) };
int owners = 0, guards = 0, executed = 0, published = 0, zero = 0; ulong identity = 0x1000;
using var file = File.Create(args[7]); using var w = new Utf8JsonWriter(file); w.WriteStartObject(); w.WriteStartArray("owners");
int noWinnerGuards = 0;
foreach (int fixture in new[] { 0, 1, 2 })

{
    var raw = fixture < 2 ? new[] { M(0) } : Enumerable.Range(0, 31).Select(n => new InkStroke(M(0).Points.Select(p => new InkPoint(p.X + n * 70, p.Y)).ToArray())).ToArray();
    var input = LegacyFreshMainOwnedTextInputPreparation.Prepare(ak, line30, line50, raw, 0, activations, table, cd, pd).Run();
    var cuts = LegacyFreshMainDescriptorCutSource.Prepare(ak, input.Smoothed); var local = LegacyFreshMainDescriptorGraphAppend.Prepare(ak, cuts, table); var global = LegacyFreshMainGlobalOrdinaryState.Prepare(ak, local, table);
    var graph = LegacyFreshMainCompactOrdinaryGraph.Prepare(ak, global); var source = LegacyFreshMainGlobalSourceRanges.Prepare(graph); var context = LegacyFreshMainGlobalContext.Prepare(ak, source, activations); var geometry = LegacyFreshMainGlobalGlyphGeometry.Prepare(ak, context);
    var owner = LegacyFreshMainGlobalCaseSession.Prepare(ak, lk, model, geometry, cd, pd); var cache = LegacyFreshMainGlobalTransitionCache.Prepare(ak, owner, table); var transitions = LegacyFreshMainGlobalWordTransitions.Prepare(ak, cache);
    var costs = LegacyFreshMainGlobalGroupCosts.Prepare(ak, owner, table); var policy = LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak, owner, costs, transitions);
    var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, owner); var terminal = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    byte[] residue = Enumerable.Repeat((byte)0xa7, graph.State.NodeCount * 1024 * 40).ToArray();
    LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r)
    { byte[] bytes = new byte[r.RequiredBytes]; r.PreviousStorage.Span.CopyTo(bytes); return new(bytes, ++identity); }
    var producer = fixture == 1
        ? LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, policy, Allocate, confidence, residue, 2)
        : LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, policy, Allocate, confidence, residue);
    var pass = producer.RunOwned(); int evaluations = owner.AcousticEvaluations;
    var final = LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, ak);
    var selected = terminal.SelectPublished(final);
    var geometryOwner = LegacyFreshMainGlobalSourceGeometry.Prepare(ak, policy, confidence);
    var callbacks = LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak, geometryOwner);
    if (!selected.Winner.HasWinner)
    {
        Reject(() => LegacyFreshMainGlobalGlyphBacktrackRows.PreparePublished(ak, selected, callbacks, new byte[72]));
        Reject(() => LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, geometryOwner));
        if (owner.AcousticEvaluations != evaluations) throw new Exception("noWinner guard reevaluated NN.");
        noWinnerGuards += 2; Console.WriteLine($"PASS actual{pass.NodeCount} noWinner rejected before backtrack/glyph-pool creation."); continue;
    }
    byte[] compact = final.CopyCompactStorage();
    var walk = LegacyFreshMainGlobalGlyphBacktrackRows.PreparePublished(ak, selected, callbacks, new byte[72]);
    Reject(() => _ = walk.Materialization);
    if (!ReferenceEquals(walk.PublishedTerminal, selected)) throw new Exception("New backtrack lost actual published owner.");
    var plan = LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);
    var ordinary = LegacyFreshMainGlobalOrdinaryGlyphProducer.Prepare(ak, callbacks); var protocol = LegacyFreshMainGlobalProtocolGlyphProducer.Prepare(ak, callbacks);
    w.WriteStartObject(); w.WriteNumber("owner", owners); w.WriteBoolean("preparedEarlyGate", fixture == 1); w.WriteBase64String("compact", compact); w.WriteNumber("recordCount", final.RecordCount2470); w.WriteStartArray("starts"); foreach (int n in final.CopyNodeStarts2478()) w.WriteNumberValue(n); w.WriteEndArray();
    w.WriteStartArray("graph"); for (int n = 0; n < graph.State.NodeCount; n++) { w.WriteStartArray(); foreach (var arc in graph.State.GetNode(n)) { w.WriteStartArray(); w.WriteNumberValue(arc.StartContextId); w.WriteNumberValue(arc.EndContextId); w.WriteEndArray(); } w.WriteEndArray(); }
    w.WriteEndArray();
    w.WriteStartArray("graphGroups"); for (int n = 0; n < graph.State.NodeCount; n++) { w.WriteStartArray(); foreach (var arc in graph.State.GetNode(n)) w.WriteNumberValue(arc.GroupId); w.WriteEndArray(); }
    w.WriteEndArray();
    w.WriteStartArray("groupRanges"); foreach (var group in graph.State.Groups) { w.WriteStartArray(); foreach (var range in group.CopyRanges()) { w.WriteStartArray(); w.WriteNumberValue(range.StrokeIndex); w.WriteNumberValue(range.StartPoint); w.WriteNumberValue(range.EndPointExclusive); w.WriteEndArray(); } w.WriteEndArray(); }
    w.WriteEndArray();
    w.WriteNumber("nnEvaluations", evaluations);
    w.WriteStartArray("cases");
    for (int seed = 0; seed < 4; seed++)
    {
        var ownership = LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, geometryOwner);
        Reject(() => _ = ownership.Materialization);
        if (!ReferenceEquals(ownership.PublishedTerminal, selected)) throw new Exception("New TOP lost published selection.");
        byte[] allocator = Allocator(seed), record = Enumerable.Range(0, 40).Select(i => (byte)(i * 17 + seed * 23 + 3)).ToArray(), request = Enumerable.Range(0, 64).Select(i => (byte)(i * 11 + seed * 29)).ToArray();
        ushort[] labels = Enumerable.Range(0, 256).Select(i => (ushort)(0x9000 + i * 13 + seed)).ToArray();
        var execution = LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, ownership, plan, allocator);
        w.WriteStartObject(); w.WriteNumber("seed", seed); w.WriteNumber("ac", execution.Width.TopAcBits); w.WriteNumber("b0", execution.Width.TopB0Bits);
        w.WriteBase64String("allocator120", allocator); w.WriteBase64String("initial120", execution.CopyInitialWordStorage()); w.WriteBase64String("recordResidue40", record); w.WriteStartArray("flushes");
        while (!execution.IsComplete)
        {
            int ordinal = execution.CompletedFlushCount; var flush = plan.Flushes[ordinal]; byte[] before = execution.CopyWorkingWordStorage();
            Reject(() => execution.ExecuteNext(new byte[39], labels, request)); Reject(() => execution.ExecuteNext(record, new ushort[255], request)); Reject(() => execution.ExecuteNext(record, labels, new byte[63])); Reject(() => execution.ExecuteNext(record, labels, request, new(true)));
            if (!before.AsSpan().SequenceEqual(execution.CopyWorkingWordStorage())) throw new Exception("Prewrite guard changed WORD.");
            var result = execution.ExecuteNext(record, labels, request); if (!result.Succeeded || execution.IsStopped) throw new Exception("Actual global callback did not complete.");
            Reject(() => execution.Associations.CompleteExecuted(result, default));
            w.WriteStartObject(); w.WriteNumber("ordinal", ordinal); w.WriteBase64String("before120", before); w.WriteBase64String("template64", flush.CopyPacketWriteTemplate()); w.WriteStartArray("origins"); foreach (int origin in flush.CopyInitialWordSeedOrigins()) w.WriteNumberValue(origin); w.WriteEndArray();
            w.WriteBase64String("packet64", result.Packet.CopyStorage()); w.WriteBase64String("rows72", result.Association.CopyRows72()); w.WriteNumber("span", result.SymbolSpan); w.WriteBoolean("succeeded", result.Succeeded);
            w.WriteBase64String("completed120", result.CopyCompletedWordStorage120()); w.WriteBase64String("reset120", execution.CopyWorkingWordStorage());
            if (result.Chain is not { } chain) throw new Exception("Actual successful association skipped chain.");
            w.WriteNumber("firstNew", chain.FirstNewReference); w.WriteNumber("last", chain.LastProcessedReference); w.WriteNumber("created", chain.CreatedCount); w.WriteNumber("reused", chain.ReusedCount);
            w.WriteStartArray("producerScalars"); byte[] rows72 = result.Association.CopyRows72();
            for (int i = rows72.Length / 72 - 1; i >= 0; i--)
            {
                byte[] row = rows72.AsSpan(i * 72, 72).ToArray(); var scratch = new LegacyOrdinaryGlyphProductionScratch(labels, 256); LegacyPreparedGlyphFinalizationInput finalized;
                if (row[60] == 0) finalized = ordinary.Produce(row, scratch, 0, compact);
                else if (row[60] == 3) finalized = protocol.Produce(row, compact, scratch, execution.Width.TopAcBits, execution.Width.TopB0Bits, request).Finalization;
                else throw new NotSupportedException("Additional selected subtype remains explicit.");
                w.WriteStartObject(); w.WriteNumber("ordinal", i); w.WriteNumber("remaining", scratch.RemainingCursor); w.WriteNumber("component", scratch.ComponentFlag); w.WriteNumber("external", scratch.ExternalVariant); w.WriteNumber("score0C", scratch.Score0CBits); w.WriteNumber("score10", scratch.Score10Bits); w.WriteNumber("cost14", scratch.Cost14); w.WriteNumber("publication", finalized.PublicationFlags06); w.WriteStartArray("labels"); foreach (ushort label in scratch.CopyLabels()) w.WriteNumberValue(label); w.WriteEndArray(); w.WriteEndObject();
            }
            w.WriteEndArray(); Nodes("pool", ownership.GlyphPool.CopyOwnerNodes());
            if (result.PublishedWord is { } node)
            {
                w.WriteStartObject("published"); w.WriteNumber("first", node.Span.First); w.WriteNumber("last", node.Span.Last); w.WriteNumber("index", node.WordIndex); w.WriteBase64String("word120", node.Word.CopyStorage()); w.WriteStartArray("flat"); foreach (ulong r in node.Word.FlatReferences) w.WriteNumberValue(r); w.WriteEndArray(); w.WriteEndObject(); published++;
            }
            else { w.WriteNull("published"); if (result.SymbolSpan != 0) throw new Exception("Positive span not published."); zero++; }
            //Execute the actual callback before reading its raw glyph projection.
            var projection = LegacyFreshMainGlobalWordUtf8Projection.Project(result);
            w.WriteBase64String("rawUtf8", projection.CopyUtf8()); w.WriteStartArray("rawGlyphs");
            foreach (var glyph in projection.Glyphs)
            {
                w.WriteStartObject(); w.WriteNumber("reference", glyph.Reference); w.WriteNumber("key", glyph.CanonicalSourceKey);
                w.WriteBoolean("protocol", glyph.IsProtocolSource);
                if (glyph.OriginalGraphSourceKey is ulong original) w.WriteNumber("original", original); else w.WriteNull("original");
                w.WriteStartArray("rows"); foreach (int row in glyph.CopyAssociatedRowOrdinals()) w.WriteNumberValue(row); w.WriteEndArray();
                w.WriteStartArray("ranges"); foreach (var range in glyph.CopySampledSourceRanges()) { w.WriteStartArray(); w.WriteNumberValue(range.StrokeIndex); w.WriteNumberValue(range.StartPoint); w.WriteNumberValue(range.EndPointExclusive); w.WriteEndArray(); }
                w.WriteEndArray();
                w.WriteBase64String("record", glyph.CopyRecord40()); w.WriteBase64String("utf8", glyph.CopyUtf8());
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject(); executed++;
        }
        Reject(() => execution.ExecuteNext(record, labels, request));
        w.WriteEndArray(); w.WriteEndObject();
    }
    w.WriteEndArray();
    //Real public misuse after packet write must stop with its written prefix.
    var failed = LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, geometryOwner), plan, Allocator(9));
    _ = failed.Associations.PrepareNext(); Reject(() => failed.ExecuteNext(new byte[40], new ushort[256], new byte[64]));
    if (!failed.IsStopped) throw new Exception("Exception after packet did not stop execution.");
    w.WriteBase64String("failedInitial120", failed.CopyInitialWordStorage()); w.WriteBase64String("failedAfterPacket120", failed.CopyWorkingWordStorage());
    w.WriteBase64String("failedTemplate64", plan.Flushes[0].CopyPacketWriteTemplate()); w.WriteStartArray("failedOrigins"); foreach (int origin in plan.Flushes[0].CopyInitialWordSeedOrigins()) w.WriteNumberValue(origin); w.WriteEndArray();
    Reject(() => failed.ExecuteNext(new byte[40], new ushort[256], new byte[64]));
    var foreignOwnership = LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, LegacyFreshMainGlobalSourceGeometry.Prepare(ak, policy, confidence));
    Reject(() => LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, foreignOwnership, plan, Allocator(2)));
    var differentSelection = terminal.SelectPublished(final);
    Reject(() => LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,
        LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(differentSelection, geometryOwner), plan, Allocator(3)));
    var leased = LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, geometryOwner), plan, Allocator(12));
    byte[] leasedBefore = leased.CopyWorkingWordStorage();
    Reject(() => owner.AdvanceNodeEpoch(graph.State.NodeCount)); Reject(() => leased.ExecuteNext(new byte[40], new ushort[256], new byte[64]));
    if (!leasedBefore.AsSpan().SequenceEqual(leased.CopyWorkingWordStorage())) throw new Exception("Invalid result lease changed WORD before its write boundary.");
    if (owner.AcousticEvaluations != evaluations) throw new Exception("Global WORD execution reran NN."); w.WriteEndObject(); Console.WriteLine($"PASS globalglyph owner{owners}:real callbacks4variants/NN{evaluations} unchanged."); owners++;
}
w.WriteEndArray(); w.WriteStartArray("preparedOutcomes"); w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteNumber("noWinnerGuards", noWinnerGuards); w.WriteEndObject(); w.Flush();
if (owners != 2 || noWinnerGuards != 2) throw new Exception("Missing actual published-positive or noWinner route.");
Console.WriteLine($"PASS {owners} actualpublished positive owners/{executed} callbacks/{published} words/{zero} zero-span/{guards} guards; noWinner2; no new NN/old terminal coercion.");
static byte[] Allocator(int seed) { var r = Enumerable.Range(0, 120).Select(i => (byte)(i * 19 + seed * 31 + 3)).ToArray(); foreach (var (offset, length) in new[] { (32, 8), (88, 12), (104, 12) }) r.AsSpan(offset, length).Clear(); return r; }
void Nodes(string name, IReadOnlyList<LegacyNativeGlyphRecordNode> nodes) { w.WriteStartArray(name); foreach (var node in nodes) { w.WriteStartObject(); w.WriteNumber("key", node.Key); w.WriteNumber("packed", node.PackedCount); w.WriteBase64String("records", node.Records.Span); w.WriteEndObject(); } w.WriteEndArray(); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { guards++; return; } throw new Exception("Invalid global execution input accepted."); }
