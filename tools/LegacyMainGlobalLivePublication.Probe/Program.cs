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

int guards = 0; long comparedBytes = 0; int comparedContexts = 0; ulong allocationIdentity = 0x10000;
using var file = File.Create(args[7]); using var writer = new Utf8JsonWriter(file); writer.WriteStartArray();
foreach (int physical in new[] { 31, 35, 60, 61, 1 })
{
    var raw = Enumerable.Range(0, physical).Select(n => new InkStroke(new InkPoint[]
    {new(n*70,90),new(n*70,10),new(n*70+25,60),new(n*70+50,10),new(n*70+50,90)})).ToArray();
    var input = LegacyFreshMainOwnedTextInputPreparation.Prepare(ak, line30, line50, raw, 0, activations, table, cd, pd).Run();
    var cuts = LegacyFreshMainDescriptorCutSource.Prepare(ak, input.Smoothed);
    var local = LegacyFreshMainDescriptorGraphAppend.Prepare(ak, cuts, table);
    var global = LegacyFreshMainGlobalOrdinaryState.Prepare(ak, local, table);
    var graph = LegacyFreshMainCompactOrdinaryGraph.Prepare(ak, global);
    var ranges = LegacyFreshMainGlobalSourceRanges.Prepare(graph);
    var context = LegacyFreshMainGlobalContext.Prepare(ak, ranges, activations);
    var geometry = LegacyFreshMainGlobalGlyphGeometry.Prepare(ak, context);
    LegacyFreshMainGlobalWordControllerPolicy FreshPolicy()
    {
        var s = LegacyFreshMainGlobalCaseSession.Prepare(ak, lk, model, geometry, cd, pd);
        var c = LegacyFreshMainGlobalGroupCosts.Prepare(ak, s, table);
        var t = LegacyFreshMainGlobalWordTransitions.Prepare(ak, LegacyFreshMainGlobalTransitionCache.Prepare(ak, s, table));
        return LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak, s, c, t);
    }
    var policy = FreshPolicy(); var owner = policy.Owner;
    var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, owner);
    owner.LiveReadLog = [];
    var residue = Enumerable.Repeat((byte)0xa7, graph.State.NodeCount * 1024 * 40).ToArray();
    if (physical == 61)
    {
        int allocated = 0;
        Reject(() => LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, policy, r =>
        { allocated++; return Allocate(r); }, confidence, residue));
        if (graph.State.NodeCount != 244 || allocated != 0 || owner.AcousticEvaluations != 0)
            throw new Exception("Unverified second-cycle source must fail before controller allocation/main NN.");
        Console.WriteLine("PASS actual244-node second-cycle refusal before controller allocation/main NN.");
        continue;
    }
    bool nativeGate = physical != 1;
    LegacyFreshMainGlobalIntermediatePublication? observed = null;
    void Observe(LegacyFreshMainGlobalIntermediatePublication value)
    {
        if (observed is not null) throw new Exception("Expected exactly one publication.");
        observed = value;
        if (value.AcousticEvaluationsBefore != value.AcousticEvaluationsAfter) throw new Exception("Publication repeated NN.");
    }
    if (nativeGate) Reject(() => new LegacyFreshMainGlobalWordPass(ak, policy, Allocate));
    var pass = nativeGate
        ? LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, policy, Allocate, confidence, residue, Observe)
        : LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, policy, Allocate, confidence, residue, 2, Observe);
    var result = pass.RunOwned();
    var terminal = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy).Select(result);
    var referencePolicy = FreshPolicy(); referencePolicy.Owner.LiveReadLog = [];
    var referencePublications = new List<OriginalGlobalLiveSchedulerOracle.Publication>();
    var expected = OriginalGlobalLiveSchedulerOracle.Run(ak, referencePolicy, residue, nativeGate ? null : 2, referencePublications);
    if (expected.Count != result.NodeCount || referencePublications.Count != 1) throw new Exception("Reference lifecycle extent differs.");
    for (int n = 0; n < result.NodeCount; n++)
    {
        var a = result.GetNodeSnapshot(n); var e = expected[n];
        if ((a.RootQueries, a.ParentQueries, a.FlagAllocations, a.PrimaryCandidates, a.LanguageRan, a.LanguageQueries, a.LanguageAllocations,
             a.SourceEnd23f0, a.RetainedEnd23f8, a.CumulativePathCost2520, a.AppliedPeriodicOffset) !=
            (e.RootQueries, e.ParentQueries, e.FlagAllocations, e.PrimaryCandidates, e.LanguageRan, e.LanguageQueries, e.LanguageAllocations,
             e.SourceEnd23f0, e.RetainedEnd23f8, e.CumulativePathCost2520, e.AppliedPeriodicOffset))
            throw new Exception($"Reference node{n} counters/costs differ.");
        Compare(a.Records, e.Records, 88, [8], owner.Contexts, referencePolicy.Owner.Contexts);
        Compare(a.PrimaryAlternatives, e.PrimaryAlternatives, 40, [8, 16], owner.Contexts, referencePolicy.Owner.Contexts);
    }
    var ep = referencePublications[0];
    if (observed is null || !observed.CopyCompactStorage().AsSpan().SequenceEqual(ep.Published.Storage) ||
        observed.RecordCount2470 != ep.Published.Count || observed.QueryChecks != ep.Reachability.Counters.Checks ||
        observed.BackendCalls != ep.Reachability.Counters.Calls) throw new Exception("Reference complete compact pool/query walk differs.");
    if (owner.LiveReadLog.Count != referencePolicy.Owner.LiveReadLog.Count) throw new Exception("Same-backend LIVE call counts differ.");
    for (int i = 0; i < owner.LiveReadLog.Count; i++)
    {
        var a = owner.LiveReadLog[i]; var e = referencePolicy.Owner.LiveReadLog[i];
        if (!Path(owner.Contexts, a.ParentContext).SequenceEqual(Path(referencePolicy.Owner.Contexts, e.ParentContext)) ||
            a with { ParentContext = 0 } != e with { ParentContext = 0 }) throw new Exception("Persistent LIVE backend chronology differs.");
    }
    int boundary = nativeGate ? 120 : 3;
    if (observed is null || observed.StartingNode2490 != 0 || observed.EndingNode != boundary - 1 ||
        result.IntermediatePublicationCount != 1 || result.NextCompactPublicationNode2490 != boundary)
        throw new Exception("Native gate/cursor lifecycle differs.");
    Console.WriteLine($"ACTUAL {graph.State.NodeCount}nodes/{graph.State.Groups.Count}groups LIVE{observed.EndingNode} pool{observed.RecordCount2470} queries{observed.QueryChecks}/{observed.BackendCalls} winner{terminal.Winner.HasWinner} NN{owner.AcousticEvaluations}; nativeGate={nativeGate}");
    Reject(() => pass.RunOwned());
    Reject(() => observed.GetInputNodeSnapshot(-1));
    Reject(() => observed.GetPublishedNodeSnapshot(boundary));
    Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak, terminal, confidence, residue));
    Reject(() => LegacyFreshMainGlobalCompactReachability.Prepare(ak, terminal));
    if (!observed.CopyNodeStarts2478().AsSpan().SequenceEqual(ep.Published.Starts.AsSpan(0, boundary + 1)) ||
        !observed.CopyPrefixOwners2488().AsSpan().SequenceEqual(ep.Published.PrefixOwners.AsSpan(0, boundary + 1)))
        throw new Exception("Reference persistent2478/2488 differs.");
    for (int n = 0; n < boundary; n++)
    {
        Compare(observed.GetInputNodeSnapshot(n).Records, ep.Before[n].Records, 88, [8], owner.Contexts, referencePolicy.Owner.Contexts);
        Compare(observed.GetPublishedNodeSnapshot(n).Records, ep.Published.Nodes[n].Records, 88, [8], owner.Contexts, referencePolicy.Owner.Contexts);
        Compare(observed.GetPublishedNodeSnapshot(n).PrimaryAlternatives, ep.Published.Nodes[n].PrimaryAlternatives, 40, [8, 16], owner.Contexts, referencePolicy.Owner.Contexts);
    }
    //Copies must not alter the mutable live pool/node storage retained privately.
    byte[] observedPool = observed.CopyCompactStorage(); byte firstByte = observedPool[0]; observedPool[0] ^= 255;
    if (observed.CopyCompactStorage()[0] != firstByte) throw new Exception("Mutable observation pool escaped.");
    var exposed = observed.GetPublishedNodeSnapshot(0); byte[] saved = exposed.Records.ToArray();
    if (MemoryMarshal.TryGetArray(exposed.Records, out var segment)) Array.Clear(segment.Array!);
    if (!observed.GetPublishedNodeSnapshot(0).Records.Span.SequenceEqual(saved)) throw new Exception("Mutable observation beam escaped.");
    if (!nativeGate)
    {
        guards += LiveControlTests.Run(ak, FreshPolicy, residue, Allocate);
        guards += LiveControlTests.CheckNodeWriteback(policy, FreshPolicy(), observed.GetInputNodeSnapshot(0));
    }

    LegacyFreshMainGlobalCompactWordMaterialization? final = null;
    if (terminal.Winner.HasWinner)
    {
        Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(new byte[ak.Length], terminal, confidence));
        Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak, terminal,
            LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, FreshPolicy().Owner)));
        Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak, terminal, confidence, new(true)));
        byte[] originalPool = result.CopyIntermediateCompactStorage();
        final = LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak, terminal, confidence);
        Reject(() => _ = final.Reachability);
        if (!originalPool.AsSpan().SequenceEqual(result.CopyIntermediateCompactStorage()) ||
            result.NextCompactPublicationNode2490 != boundary || result.IntermediatePublicationCount != 1)
            throw new Exception("Final append mutated the immutable producing LIVE result snapshot.");
        if (!final.CopyCompactStorage().AsSpan(0, observed.RecordCount2470 * 40).SequenceEqual(observed.CopyCompactStorage().AsSpan(0, observed.RecordCount2470 * 40)))
            throw new Exception("Final append lost native compact prefix.");
        Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak, terminal, confidence));
    }
    else Reject(() => LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak, terminal, confidence));
    writer.WriteStartObject(); writer.WriteNumber("factory", 0x292); writer.WriteString("publicationKind", "live");
    writer.WriteBoolean("nativeGate", nativeGate); writer.WriteNumber("nodeCount", boundary);
    writer.WriteNumber("actualGraphNodeCount", result.NodeCount); writer.WriteNumber("maximumAtoms", graph.State.MaximumAtoms);
    writer.WriteNumber("intermediatePublicationDistance", policy.MaximumParentDistance54);
    writer.WriteNumber("primary", policy.Configuration.PrimaryArcLimit230a); writer.WriteNumber("count", observed.RecordCount2470);
    writer.WriteNumber("previousCount", observed.PreviousRecordCount2470); writer.WriteNumber("startingNode", 0);
    writer.WriteNumber("endingNode", observed.EndingNode); writer.WriteNumber("nextNode2490", result.NextCompactPublicationNode2490);
    writer.WriteNumber("penalty", terminal.Invocation.LanguagePenalty2534); writer.WriteBoolean("hasTerminalWinner", terminal.Winner.HasWinner);
    writer.WriteNumber("queries", observed.QueryChecks); writer.WriteNumber("backendCalls", observed.BackendCalls);
    writer.WriteNumber("evaluations", owner.AcousticEvaluations); writer.WriteNumber("liveBackendReadLogCount", owner.LiveReadLog.Count);
    if (final is not null)
    {
        writer.WriteNumber("finalCount", final.RecordCount); writer.WriteNumber("finalSelected", final.SelectedRecordIndex);
        writer.WriteBase64String("finalCompact", final.CopyCompactStorage()); WriteInts("finalStarts", final.CopyNodeStarts2478());
        WriteInts("finalPrefixOwners2488", final.CopyPrefixOwners2488()); writer.WriteNumber("finalWinner", terminal.Winner.Ordinal);
        WriteNodes("finalInputNodes", result.GetNodeSnapshot, result.NodeCount); WriteNodes("finalNodes", final.GetNodeSnapshot, result.NodeCount);
        WriteGraph("finalGraph", result.NodeCount);
    }
    writer.WriteBase64String("compact", observed.CopyCompactStorage()); WriteInts("starts", observed.CopyNodeStarts2478());
    WriteInts("prefixOwners2488", observed.CopyPrefixOwners2488());
    writer.WriteStartArray("primaryMap"); foreach (ushort value in owner.Symbols.Symbols.Symbols.CopyPrimary()) writer.WriteNumberValue(value); writer.WriteEndArray();
    WriteNodes("inputNodes", observed.GetInputNodeSnapshot, boundary); WriteNodes("nodes", observed.GetPublishedNodeSnapshot, boundary);
    WriteGraph("graph", boundary);
    writer.WriteStartArray("contexts"); foreach (var node in owner.Contexts.CopyNodes())
    { writer.WriteStartArray(); writer.WriteNumberValue(node.Identity); writer.WriteNumberValue(node.ParentLink); writer.WriteNumberValue(node.Symbol); writer.WriteEndArray(); }
    writer.WriteEndArray();
    writer.WriteStartArray("scores"); for (int group = 0; group < graph.State.Groups.Count; group++)
    {
        var score = confidence.ReadGroup(group); writer.WriteStartObject(); writer.WriteStartArray("candidates");
        foreach (var c in score.Candidates.Span) { writer.WriteStartArray(); writer.WriteNumberValue(c.Symbol); writer.WriteNumberValue(c.Cost); writer.WriteEndArray(); }
        writer.WriteEndArray(); writer.WriteNumber("scaleBits", BitConverter.SingleToUInt32Bits(score.GroupPowerScale));
        writer.WriteStartArray("costs"); foreach (int c in score.CopyFinalClassCosts()) writer.WriteNumberValue(c); writer.WriteEndArray(); writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteEndObject(); writer.Flush();
    void WriteInts(string name, int[] values) { writer.WriteStartArray(name); foreach (int v in values) writer.WriteNumberValue(v); writer.WriteEndArray(); }
    void WriteNodes(string name, Func<int, LegacyMainDirectWordNodeResult> read, int count)
    {
        writer.WriteStartArray(name); for (int n = 0; n < count; n++)
        {
            var row = read(n); writer.WriteStartObject(); writer.WriteNumber("sourceEnd", row.SourceEnd23f0);
            writer.WriteBase64String("rows", row.Records.Span); writer.WriteBase64String("alternatives", row.PrimaryAlternatives.Span); writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
    void WriteGraph(string name, int count)
    {
        writer.WriteStartArray(name); for (int n = 0; n < count; n++)
        {
            writer.WriteStartArray(); foreach (var arc in graph.State.GetNode(n))
            { writer.WriteStartArray(); writer.WriteNumberValue(arc.GroupId); writer.WriteNumberValue(arc.Span); writer.WriteNumberValue(arc.EndContextId); writer.WriteEndArray(); }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

}
writer.WriteEndArray(); writer.Flush(); Console.WriteLine($"PASS actual global LIVE/append; {guards} guards/{comparedBytes} full bytes/{comparedContexts} symbolic fields");
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r)
{ var b = new byte[r.RequiredBytes]; r.PreviousStorage.Span.CopyTo(b); return new(b, ++allocationIdentity); }
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { guards++; return; }
    throw new Exception("Expected explicit ownership/lifecycle refusal.");
}

void Compare(ReadOnlyMemory<byte> a, ReadOnlyMemory<byte> e, int stride, int[] fields,
    LegacySymbolContextFactory ac, LegacySymbolContextFactory ec)
{
    byte[] left = a.ToArray(), right = e.ToArray();
    if (left.Length != right.Length || left.Length % stride != 0) throw new Exception("Reference allocation extent differs.");
    for (int at = 0; at < left.Length; at += stride) foreach (int field in fields)
    {
        ulong x = BinaryPrimitives.ReadUInt64LittleEndian(left.AsSpan(at + field)), y = BinaryPrimitives.ReadUInt64LittleEndian(right.AsSpan(at + field));
        if (!Path(ac, x).SequenceEqual(Path(ec, y))) throw new Exception($"Reference context lineage differs row{at / stride} field{field}.");
        left.AsSpan(at + field, 8).Clear(); right.AsSpan(at + field, 8).Clear(); comparedContexts++;
    }
    if (!left.AsSpan().SequenceEqual(right)) throw new Exception("Reference complete scalar storage differs."); comparedBytes += left.Length;
}
static ushort[] Path(LegacySymbolContextFactory contexts, ulong id)
{
    var path = new List<ushort>(); while (id != 0) { if (path.Count > 4096) throw new Exception("Context cycle."); var n = contexts.ReadNode(id); path.Add(n.Symbol); id = n.ParentLink; }
    path.Reverse(); return path.ToArray();
}
