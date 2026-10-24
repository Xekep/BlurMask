using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if (args.Length != 9) throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[8]), engine = File.ReadAllBytes(args[1]);
using var ed = JsonDocument.Parse(engine); var activation = ed.RootElement.GetProperty("activations");
var activations = new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v => v.GetInt32()).ToArray(), activation.GetProperty("output").EnumerateArray().Select(v => v.GetInt32()).ToArray());
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2])); using var cf = File.OpenRead(args[3]); using var pf = File.OpenRead(args[4]); var cd = LegacyNativeCaseData.Load(cf); var pd = LegacyNativeTextPublicationData.Load(pf);
var model = LegacyRussianModelBundle.Load(ak, lk, engine); byte[] line30 = File.ReadAllBytes(args[5]), line50 = File.ReadAllBytes(args[6]);

int guards = 0; long comparedBytes = 0; int comparedContexts = 0; ulong identity = 0x10000;
using var output = File.Create(args[7]); using var writer = new Utf8JsonWriter(output); writer.WriteStartArray();
foreach (int fixture in new[] { 31, 35, 60, 1, 0 })
{
    int physical = Math.Max(1, fixture); bool prepared = fixture == 1;
    var raw = Enumerable.Range(0, physical).Select(n => new InkStroke(new InkPoint[]
        {new(n*70,90),new(n*70,10),new(n*70+25,60),new(n*70+50,10),new(n*70+50,90)})).ToArray();
    var input = LegacyFreshMainOwnedTextInputPreparation.Prepare(ak, line30, line50, raw, 0, activations, table, cd, pd).Run();
    var cuts = LegacyFreshMainDescriptorCutSource.Prepare(ak, input.Smoothed);
    var graph = LegacyFreshMainCompactOrdinaryGraph.Prepare(ak, LegacyFreshMainGlobalOrdinaryState.Prepare(ak,
        LegacyFreshMainDescriptorGraphAppend.Prepare(ak, cuts, table), table));
    var geometry = LegacyFreshMainGlobalGlyphGeometry.Prepare(ak, LegacyFreshMainGlobalContext.Prepare(ak,
        LegacyFreshMainGlobalSourceRanges.Prepare(graph), activations));
    LegacyFreshMainGlobalWordControllerPolicy Fresh()
    {
        var owner = LegacyFreshMainGlobalCaseSession.Prepare(ak, lk, model, geometry, cd, pd);
        return LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak, owner, LegacyFreshMainGlobalGroupCosts.Prepare(ak, owner, table),
            LegacyFreshMainGlobalWordTransitions.Prepare(ak, LegacyFreshMainGlobalTransitionCache.Prepare(ak, owner, table)));
    }
    var policy = Fresh(); var owner = policy.Owner; var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, owner);
    byte[] residue = Enumerable.Repeat((byte)0xa7, graph.State.NodeCount * 1024 * 40).ToArray();
    var producer = prepared
        ? LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, policy, Allocate, confidence, residue, 2)
        : LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, policy, Allocate, confidence, residue);
    var pass = producer.RunOwned(); int evaluations = owner.AcousticEvaluations;
    var retainedReader = pass.RequirePublicationReader(default); byte[] retainedPrefix = pass.CopyIntermediateCompactStorage();
    Reject(() => LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, new byte[ak.Length]));
    Reject(() => LegacyFreshMainGlobalFinalPublication.Append(pass, LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, Fresh().Owner), ak));
    Reject(() => LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, ak, new(true)));
    var final = LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, ak);
    //This is deliberately the first terminal scan on this producing pass.
    var invocation = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);
    var selected = invocation.SelectPublished(final);
    if (!ReferenceEquals(final.PassResult, pass) || !ReferenceEquals(final.Policy, policy) ||
        !ReferenceEquals(retainedReader, pass.RequirePublicationReader(default)) || owner.AcousticEvaluations != evaluations ||
        !retainedPrefix.AsSpan().SequenceEqual(pass.CopyIntermediateCompactStorage()) ||
        !final.CopyCompactStorage().AsSpan(0, final.PreviousRecordCount2470 * 40).SequenceEqual(retainedPrefix.AsSpan(0, final.PreviousRecordCount2470 * 40)))
        throw new Exception("Final append replaced source/reader/NN or modified immutable LIVE prefix.");
    int expectedStart = fixture > 1 ? 120 : prepared ? 3 : 0;
    if (final.StartingNode2490 != expectedStart || final.NextNode2490 != pass.NodeCount ||
        final.PublicationCount != (expectedStart == 0 ? 1 : 2)) throw new Exception("Final2490/count lifecycle differs.");
    if (fixture > 1 && (selected.Winner.HasWinner || selected.SelectedRecordIndex is not null || selected.CopySelectedRecord().Length != 0))
        throw new Exception("Natural long noWinner acquired a manufactured selected record.");
    if (fixture <= 1 && (!selected.Winner.HasWinner || selected.SelectedRecordIndex is null)) throw new Exception("Actual short positive selection missing.");
    Reject(() => LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, ak));
    Reject(() => LegacyFreshMainGlobalTerminalInvocation.Prepare(Fresh()).SelectPublished(final));
    Reject(() => invocation.SelectPublished(final, new(true)));
    Reject(() => final.GetNodeSnapshot(-1)); Reject(() => final.GetNodeSnapshot(final.NodeCount));
    if (invocation.SelectPublished(final).Winner != selected.Winner) throw new Exception("Read-only selection changed final rows.");

    var referencePolicy = Fresh(); var publications = new List<OriginalGlobalFinalSchedulerOracle.Publication>();
    var reference = OriginalGlobalFinalSchedulerOracle.Run(ak, referencePolicy, residue, prepared ? 2 : null, publications);
    var referenceConfidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, referencePolicy.Owner);
    var referenceInvocation = LegacyFreshMainGlobalTerminalInvocation.Prepare(referencePolicy);
    void Validate(CancellationToken token) => referencePolicy.Owner.ValidateResultQueryLease(reference.Lease, token);
    var reach = LegacyPreparedMainCompactReachability.Run(referencePolicy.Configuration, graph.State.MaximumAtoms,
        reference.NextNode, pass.NodeCount - 1, n => reference.Rows[n], reference.Reader, referencePolicy.Owner.Contexts.ContainerIdentity,
        Validate, false, default);
    var expected = LegacyPreparedMainCompactPublication.Run(referencePolicy.Configuration, referencePolicy.Services.Graph,
        referencePolicy.Owner.Contexts, model.AcousticAlphabet, referencePolicy.Owner.Symbols.Geometry.CategoryOwner.Categories,
        referenceInvocation.Options.ContextSentinels24b8, referenceInvocation.LanguagePenalty2534, false, reference.NextNode,
        pass.NodeCount - 1, reach.Nodes, reach.LanguageProbe, g => LegacyMainCompactGroupScores.FromGlobal(referenceConfidence.ReadGroup(g)),
        reference.Compact, reference.Count, reference.Starts, reference.Prefix, Validate, default, referencePolicy.Services.CopySourceHeaders());
    if (final.RecordCount2470 != expected.Count || final.QueryChecks != reach.Counters.Checks || final.BackendCalls != reach.Counters.Calls ||
        !final.CopyCompactStorage().AsSpan().SequenceEqual(expected.Storage) || !final.CopyNodeStarts2478().AsSpan().SequenceEqual(expected.Starts) ||
        !final.CopyPrefixOwners2488().AsSpan().SequenceEqual(expected.PrefixOwners)) throw new Exception("Independent final complete pool/lifecycle differs.");
    for (int node = 0; node < pass.NodeCount; node++)
    {
        var actual = final.GetNodeSnapshot(node); var e = expected.Nodes[node];
        if ((actual.SourceEnd23f0, actual.RetainedEnd23f8, actual.CumulativePathCost2520) != (e.SourceEnd23f0, e.RetainedEnd23f8, e.CumulativePathCost2520))
            throw new Exception("Final source/retained/cumulative fields differ.");
        Compare(actual.Records, e.Records, 88, [8], owner.Contexts, referencePolicy.Owner.Contexts);
        Compare(actual.PrimaryAlternatives, e.PrimaryAlternatives, 40, [8, 16], owner.Contexts, referencePolicy.Owner.Contexts);
    }
    if (fixture == 1) guards += FinalControlTests.Run(ak, Fresh, residue, Allocate);
    //All public snapshots are detached, even after a noWinner return.
    byte[] saved = final.CopyCompactStorage(); Array.Clear(final.CopyCompactStorage());
    if (!saved.AsSpan().SequenceEqual(final.CopyCompactStorage())) throw new Exception("Mutable compact snapshot escaped.");
    byte[] selectedBytes = selected.CopySelectedRecord(); Array.Clear(selected.CopySelectedRecord());
    if (!selectedBytes.AsSpan().SequenceEqual(selected.CopySelectedRecord())) throw new Exception("Mutable selected88 escaped.");

    writer.WriteStartObject(); writer.WriteNumber("factory", 0x292); writer.WriteBoolean("preparedEarlyGate", prepared);
    writer.WriteNumber("nodeCount", pass.NodeCount); writer.WriteNumber("maximumAtoms", graph.State.MaximumAtoms);
    writer.WriteNumber("intermediatePublicationDistance", policy.MaximumParentDistance54); writer.WriteNumber("boundedLogicalNodeLimit", 240); writer.WriteNumber("startingNode2490", final.StartingNode2490);
    writer.WriteNumber("nextNode2490", final.NextNode2490); writer.WriteNumber("publicationCount", final.PublicationCount);
    writer.WriteNumber("previousCount", final.PreviousRecordCount2470); writer.WriteNumber("count", final.RecordCount2470);
    writer.WriteNumber("primary", policy.Configuration.PrimaryArcLimit230a); writer.WriteNumber("penalty", invocation.LanguagePenalty2534);
    writer.WriteNumber("sourceEnd", final.GetNodeSnapshot(pass.NodeCount - 1).SourceEnd23f0); writer.WriteNumber("cumulative", final.GetNodeSnapshot(pass.NodeCount - 1).CumulativePathCost2520);
    writer.WriteStartArray("winner"); var win = selected.Winner; writer.WriteBooleanValue(win.HasWinner); writer.WriteNumberValue(win.Ordinal); writer.WriteNumberValue(win.RelativeCost);
    writer.WriteNumberValue(win.TotalCost); writer.WriteNumberValue(win.RowCost14); writer.WriteNumberValue(win.RowField22); writer.WriteNumberValue(win.FallbackFlag); writer.WriteNumberValue(win.ResultFlag8); writer.WriteEndArray();
    if (selected.SelectedRecordIndex is int index) writer.WriteNumber("selected", index); else writer.WriteNull("selected");
    writer.WriteBase64String("selected88", selected.CopySelectedRecord()); writer.WriteBase64String("compact", final.CopyCompactStorage());
    writer.WriteStartArray("starts"); foreach (int v in final.CopyNodeStarts2478()) writer.WriteNumberValue(v); writer.WriteEndArray();
    writer.WriteStartArray("prefixOwners2488"); foreach (int v in final.CopyPrefixOwners2488()) writer.WriteNumberValue(v); writer.WriteEndArray();
    writer.WriteStartArray("inputNodes"); for (int n = 0; n < pass.NodeCount; n++) WriteNode(pass.GetNodeSnapshot(n)); writer.WriteEndArray();
    writer.WriteStartArray("nodes"); for (int n = 0; n < pass.NodeCount; n++) WriteNode(final.GetNodeSnapshot(n)); writer.WriteEndArray();
    writer.WriteStartArray("primaryMap"); foreach (ushort v in owner.Symbols.Symbols.Symbols.CopyPrimary()) writer.WriteNumberValue(v); writer.WriteEndArray();
    writer.WriteStartArray("graph"); for (int n = 0; n < pass.NodeCount; n++)
    {
        writer.WriteStartArray(); foreach (var a in graph.State.GetNode(n))
        { writer.WriteStartArray(); writer.WriteNumberValue(a.GroupId); writer.WriteNumberValue(a.Span); writer.WriteNumberValue(a.EndContextId); writer.WriteEndArray(); }
        writer.WriteEndArray();
    }
    writer.WriteEndArray();
    writer.WriteStartArray("contexts"); foreach (var n in owner.Contexts.CopyNodes()) { writer.WriteStartArray(); writer.WriteNumberValue(n.Identity); writer.WriteNumberValue(n.ParentLink); writer.WriteNumberValue(n.Symbol); writer.WriteEndArray(); }
    writer.WriteEndArray();
    writer.WriteStartArray("scores"); for (int g = 0; g < graph.State.Groups.Count; g++)
    {
        var score = confidence.ReadGroup(g); writer.WriteStartObject(); writer.WriteStartArray("candidates"); foreach (var c in score.Candidates.Span)
        { writer.WriteStartArray(); writer.WriteNumberValue(c.Symbol); writer.WriteNumberValue(c.Cost); writer.WriteEndArray(); }
        writer.WriteEndArray(); writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
        writer.WriteStartArray("costs"); foreach (int c in score.CopyFinalClassCosts()) writer.WriteNumberValue(c); writer.WriteEndArray(); writer.WriteEndObject();
    }
    writer.WriteEndArray();
    writer.WriteEndObject(); writer.Flush();
    Console.WriteLine($"PASS final207CB0 then2257C0: {pass.NodeCount}nodes/{final.StartingNode2490}start/{final.RecordCount2470}compact/winner{selected.Winner.HasWinner}/index{selected.SelectedRecordIndex}; prepared={prepared}");
    void WriteNode(LegacyMainDirectWordNodeResult n) { writer.WriteStartObject(); writer.WriteNumber("sourceEnd", n.SourceEnd23f0); writer.WriteBase64String("rows", n.Records.Span); writer.WriteBase64String("alternatives", n.PrimaryAlternatives.Span); writer.WriteEndObject(); }
}
writer.WriteEndArray(); writer.Flush(); Console.WriteLine($"PASS {guards} guards/{comparedBytes} full row/pool bytes/{comparedContexts} symbolic fields");
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r)
{ var b = new byte[r.RequiredBytes]; r.PreviousStorage.Span.CopyTo(b); return new(b, ++identity); }
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { guards++; return; } throw new Exception("Expected final publication guard."); }
void Compare(ReadOnlyMemory<byte> a, ReadOnlyMemory<byte> e, int stride, int[] fields, LegacySymbolContextFactory ac, LegacySymbolContextFactory ec)
{
    byte[] left = a.ToArray(), right = e.ToArray(); if (left.Length != right.Length || left.Length % stride != 0) throw new Exception("Reference allocation extent differs.");
    for (int at = 0; at < left.Length; at += stride) foreach (int field in fields)
    {
        ulong x = BinaryPrimitives.ReadUInt64LittleEndian(left.AsSpan(at + field)), y = BinaryPrimitives.ReadUInt64LittleEndian(right.AsSpan(at + field));
        if (!Path(ac, x).SequenceEqual(Path(ec, y))) throw new Exception("Reference final symbolic lineage differs."); left.AsSpan(at + field, 8).Clear(); right.AsSpan(at + field, 8).Clear(); comparedContexts++;
    }
    if (!left.AsSpan().SequenceEqual(right)) throw new Exception("Reference final scalar bytes differ."); comparedBytes += left.Length;
}
static ushort[] Path(LegacySymbolContextFactory contexts, ulong id)
{ var values = new List<ushort>(); while (id != 0) { if (values.Count > 4096) throw new Exception("Context cycle."); var n = contexts.ReadNode(id); values.Add(n.Symbol); id = n.ParentLink; } values.Reverse(); return values.ToArray(); }
