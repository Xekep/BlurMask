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
int noWinnerGuards = 0, snapshots = 0, cleanupGuards = 0, noOps = 0;
LegacyFreshMainGlobalResultGlyphNormalization? priorPolicy = null;
RejectCleanup(() => LegacyFreshMainGlobalResultGlyphNormalization.Prepare(null!));
foreach (int fixture in new[] { 0, 1, 2 })

{
    var raw = fixture < 2 ? new[] { M(0) } : Enumerable.Range(0, 31).Select(n => new InkStroke(M(0).Points.Select(p => new InkPoint(p.X + n * 70, p.Y)).ToArray())).ToArray();
    var input = LegacyFreshMainOwnedTextInputPreparation.Prepare(ak, line30, line50, raw, 0, activations, table, cd, pd).Run();
    var cuts = LegacyFreshMainDescriptorCutSource.Prepare(ak, input.Smoothed); var local = LegacyFreshMainDescriptorGraphAppend.Prepare(ak, cuts, table); var global = LegacyFreshMainGlobalOrdinaryState.Prepare(ak, local, table);
    var graph = LegacyFreshMainCompactOrdinaryGraph.Prepare(ak, global); var source = LegacyFreshMainGlobalSourceRanges.Prepare(graph); var context = LegacyFreshMainGlobalContext.Prepare(ak, source, activations); var geometry = LegacyFreshMainGlobalGlyphGeometry.Prepare(ak, context);
    var owner = LegacyFreshMainGlobalCaseSession.Prepare(ak, lk, model, geometry, cd, pd); var cache = LegacyFreshMainGlobalTransitionCache.Prepare(ak, owner, table); var transitions = LegacyFreshMainGlobalWordTransitions.Prepare(ak, cache);
    var costs = LegacyFreshMainGlobalGroupCosts.Prepare(ak, owner, table); var policy = LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak, owner, costs, transitions);
    var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, owner); var terminal = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    var normalization = LegacyFreshMainGlobalResultGlyphNormalization.Prepare(policy);
    RejectCleanup(() => LegacyFreshMainGlobalResultGlyphNormalization.Prepare(policy, new(true)));
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
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, null!, [], new byte[400]));
        noWinnerGuards += 3; Console.WriteLine($"PASS actual{pass.NodeCount} noWinner rejected before backtrack/glyph-pool creation."); continue;
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
    w.WriteNumber("nodes", pass.NodeCount); w.WriteNumber("winnerOrdinal", selected.Winner.Ordinal);
    w.WriteNumber("total", selected.Winner.TotalCost); w.WriteNumber("rowCost14", selected.Winner.RowCost14);
    w.WriteBase64String("selectedBeam", selected.CopySelectedRecord());
    w.WriteStartArray("cases");
    LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot? completedCleanupSnapshot = null;
    LegacyFreshMainGlobalGlyphExecutionSession? completedSession = null;
    IReadOnlyList<LegacyFreshMainGlobalGlyphExecutionResult>? completedTokens = null;
    for (int seed = 0; seed < 4; seed++)
    {
        var ownership = LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected, geometryOwner);
        Reject(() => _ = ownership.Materialization);
        if (!ReferenceEquals(ownership.PublishedTerminal, selected)) throw new Exception("New TOP lost published selection.");
        byte[] allocator = Allocator(seed), record = Enumerable.Range(0, 40).Select(i => (byte)(i * 17 + seed * 23 + 3)).ToArray(), request = Enumerable.Range(0, 64).Select(i => (byte)(i * 11 + seed * 29)).ToArray();
        ushort[] labels = Enumerable.Range(0, 256).Select(i => (ushort)(0x9000 + i * 13 + seed)).ToArray();
        var execution = LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, ownership, plan, allocator);
        var tokens = new List<LegacyFreshMainGlobalGlyphExecutionResult>();
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[400]));
        w.WriteStartObject(); w.WriteNumber("seed", seed); w.WriteNumber("ac", execution.Width.TopAcBits); w.WriteNumber("b0", execution.Width.TopB0Bits);
        w.WriteBase64String("allocator120", allocator); w.WriteBase64String("initial120", execution.CopyInitialWordStorage()); w.WriteBase64String("recordResidue40", record); w.WriteStartArray("flushes");
        while (!execution.IsComplete)
        {
            int ordinal = execution.CompletedFlushCount; var flush = plan.Flushes[ordinal]; byte[] before = execution.CopyWorkingWordStorage();
            Reject(() => execution.ExecuteNext(new byte[39], labels, request)); Reject(() => execution.ExecuteNext(record, new ushort[255], request)); Reject(() => execution.ExecuteNext(record, labels, new byte[63])); Reject(() => execution.ExecuteNext(record, labels, request, new(true)));
            if (!before.AsSpan().SequenceEqual(execution.CopyWorkingWordStorage())) throw new Exception("Prewrite guard changed WORD.");
            var result = execution.ExecuteNext(record, labels, request); if (!result.Succeeded || execution.IsStopped) throw new Exception("Actual global callback did not complete.");
            tokens.Add(result);
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
        w.WriteEndArray();
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(null!, execution, tokens, new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, null!, tokens, new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, null!, new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[399]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[401]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(terminal.SelectPublished(final), execution, tokens, new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, [], new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens.Append(tokens[0]).ToArray(), new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens.AsEnumerable().Reverse().ToArray(), new byte[400]));
        var missing = tokens.ToArray(); missing[0] = null!;
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, missing, new byte[400]));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[400], new(true)));
        var foreignSession = LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak, ownership, plan, Allocator(11));
        Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, foreignSession, tokens, new byte[400]));
        var head = ownership.WordPublisher.Head;
        string beforePool = PoolState(ownership.GlyphPool);
        w.WriteStartArray("snapshots");
        for (int allocationSeed = 0; allocationSeed < 64; allocationSeed++)
        {
            byte[] allocation = Enumerable.Range(0, 400).Select(i => (byte)(i * 17 + allocationSeed * 29 + 7)).ToArray();
            var snapshot = LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, allocation);
            if (!ReferenceEquals(snapshot.Terminal, selected) || !ReferenceEquals(snapshot.Session, execution) ||
                !ReferenceEquals(snapshot.WordHead18, head) || !ReferenceEquals(snapshot.BeamOwner20, final) ||
                !ReferenceEquals(snapshot.SourcePolicy, policy) || snapshot.SelectedBeamOrdinal20 != selected.Winner.Ordinal ||
                snapshot.SourceNodeCount2310 != pass.NodeCount || snapshot.PublishedWordCount != 1)
                throw new Exception("Published snapshot lost actual source/list/publication identity.");
            w.WriteStartObject(); w.WriteNumber("seed", allocationSeed); w.WriteBase64String("residue", allocation);
            w.WriteBase64String("constructed", snapshot.CopyConstructorStorage400());
            w.WriteBase64String("scored", snapshot.CopyScoredStorage400());
            w.WriteBase64String("normalized", snapshot.CopyNormalizedScoreStorage400());
            w.WriteBase64String("publishedBeam", snapshot.CopySelectedBeam88());
            w.WriteNumber("reference", snapshot.Reference78); w.WriteNumber("rawFirst08", BitConverter.SingleToInt32Bits(snapshot.RawFirstMass08));
            w.WriteNumber("top80", BitConverter.SingleToInt32Bits(snapshot.NormalizedTop80)); w.WriteEndObject(); snapshots++;
            byte[] alias = snapshot.CopyConstructorStorage400(); alias.AsSpan().Clear();
            if (snapshot.CopyConstructorStorage400().AsSpan().SequenceEqual(alias)) throw new Exception("Snapshot exposes mutable constructor storage.");
            alias = snapshot.CopySelectedBeam88(); alias.AsSpan().Clear();
            if (snapshot.CopySelectedBeam88().AsSpan().SequenceEqual(alias)) throw new Exception("Snapshot exposes mutable selected row.");
        }
        if (!ReferenceEquals(head, ownership.WordPublisher.Head) || beforePool != PoolState(ownership.GlyphPool))
            throw new Exception("Score snapshot modified actual WORD list/glyph pool.");
        w.WriteEndArray();
        var cleanupSnapshot = LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[400]);
        byte[] leading = [0xc1, 0x4e, 0xa5, 0x73], trailing = [0xd2, 0x5f, 0xb6, 0x84];
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(null!, leading, trailing));
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, new byte[3], trailing));
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, new byte[5]));
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, leading));
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, trailing, new(true)));
        RejectCleanup(() => normalization.ApplyPublished(null!));
        RejectCleanup(() => normalization.ApplyPublished(cleanupSnapshot, new(true)));
        if (priorPolicy is not null) RejectCleanup(() => priorPolicy.ApplyPublished(cleanupSnapshot));
        byte[] scoreBefore = cleanupSnapshot.CopyNormalizedScoreStorage400();
        w.WriteStartObject("cleanup"); Nodes("before", ownership.GlyphPool.CopyOwnerNodes());
        w.WriteStartArray("heads"); for (var node = head; node is not null; node = node.Next) w.WriteNumberValue(node.Word.HeadReference); w.WriteEndArray();
        LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, trailing);
        Nodes("afterQuote", ownership.GlyphPool.CopyOwnerNodes());
        string poolAfterQuote = PoolState(ownership.GlyphPool);
        var proof = normalization.ApplyPublished(cleanupSnapshot);
        if (!ReferenceEquals(proof.Policy, normalization) || !ReferenceEquals(proof.Snapshot, cleanupSnapshot) ||
            !ReferenceEquals(normalization.SourcePolicy, policy) || proof.ExecutedGlyphRewriteBody ||
            normalization.ProvenFreshConfigurationF4 != 0 || normalization.NativePropertyId != 0x012d0017 ||
            poolAfterQuote != PoolState(ownership.GlyphPool) ||
            !scoreBefore.AsSpan().SequenceEqual(cleanupSnapshot.CopyNormalizedScoreStorage400()))
            throw new Exception("Fresh global F4 no-op lost ownership or mutated glyph/score data.");
        Nodes("afterNoOp", ownership.GlyphPool.CopyOwnerNodes());
        w.WriteBase64String("scoresBefore", scoreBefore); w.WriteBase64String("scoresAfter", cleanupSnapshot.CopyNormalizedScoreStorage400());
        w.WriteBase64String("leading", leading); w.WriteBase64String("trailing", trailing);
        w.WriteNumber("property", normalization.NativePropertyId); w.WriteNumber("f4", normalization.ProvenFreshConfigurationF4);
        w.WriteBoolean("executed", proof.ExecutedGlyphRewriteBody); w.WriteEndObject(); noOps++;
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, trailing));
        var recreated = LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, execution, tokens, new byte[400]);
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(recreated, leading, trailing));
        //Explicit prepared foreign-list mutation after the actual capture. It is
        //a negative ownership control, not another inferred glyph/WORD.
        var extra = ownership.CreateWorkingWord(new byte[120]);
        extra.AppendGlyph(0x7f0000, 0, true, new byte[40]); extra.SetLastOrdinarySymbol(0);
        ownership.WordPublisher.Publish(extra, (_, _) => 0, (_, _) => 0);
        RejectCleanup(() => normalization.ApplyPublished(cleanupSnapshot));
        RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(cleanupSnapshot, leading, trailing));
        w.WriteEndObject();
        completedSession = execution; completedTokens = tokens; completedCleanupSnapshot = cleanupSnapshot;
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
    Reject(() => LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(selected, completedSession!, completedTokens!, new byte[400]));
    //The result lease is now invalid; neither normalization may consume it.
    RejectCleanup(() => normalization.ApplyPublished(completedCleanupSnapshot!));
    RejectCleanup(() => LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(completedCleanupSnapshot!, new byte[4], new byte[4]));
    if (!leasedBefore.AsSpan().SequenceEqual(leased.CopyWorkingWordStorage())) throw new Exception("Invalid result lease changed WORD before its write boundary.");
    if (owner.AcousticEvaluations != evaluations) throw new Exception("Global WORD execution reran NN."); w.WriteEndObject(); Console.WriteLine($"PASS globalglyph owner{owners}:real callbacks4variants/NN{evaluations} unchanged."); priorPolicy = normalization; owners++;
}
w.WriteEndArray(); w.WriteStartArray("preparedOutcomes"); w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteNumber("noWinnerGuards", noWinnerGuards); w.WriteNumber("snapshotCount", snapshots); w.WriteNumber("cleanupGuards", cleanupGuards); w.WriteNumber("noOpCount", noOps); w.WriteEndObject(); w.Flush();
if (owners != 2 || noWinnerGuards != 3 || snapshots != 512 || noOps != 8 || cleanupGuards != 100) throw new Exception("Missing actual published-positive or noWinner route.");
Console.WriteLine($"PASS {owners} actualpublished positive owners/{executed} callbacks/{published} words/{zero} zero-span/{guards} guards; noWinner3;512 score snapshots; no new NN/old terminal coercion.");
Console.WriteLine($"PASS {noOps} actual published quote then zero-write F4 proofs/{cleanupGuards} cleanup guards; no synthetic positive quote or enabled21F320 claim.");
static byte[] Allocator(int seed) { var r = Enumerable.Range(0, 120).Select(i => (byte)(i * 19 + seed * 31 + 3)).ToArray(); foreach (var (offset, length) in new[] { (32, 8), (88, 12), (104, 12) }) r.AsSpan(offset, length).Clear(); return r; }
void Nodes(string name, IReadOnlyList<LegacyNativeGlyphRecordNode> nodes) { w.WriteStartArray(name); foreach (var node in nodes) { w.WriteStartObject(); w.WriteNumber("key", node.Key); w.WriteNumber("packed", node.PackedCount); w.WriteBase64String("records", node.Records.Span); w.WriteEndObject(); } w.WriteEndArray(); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { guards++; return; } throw new Exception("Invalid global execution input accepted."); }

static string PoolState(LegacyNativeGlyphRecordPool pool) => string.Join(";", pool.CopyOwnerNodes().Select(n => $"{n.Key:X16}:{n.PackedCount:X4}:{Convert.ToHexString(n.Records.Span)}"));

void RejectCleanup(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { cleanupGuards++; return; } throw new Exception("Invalid global published cleanup input accepted."); }
