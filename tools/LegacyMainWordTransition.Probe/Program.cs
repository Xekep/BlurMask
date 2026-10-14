using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length is not (5 or 6))
    throw new ArgumentException("AK LK engine.json separation.json audit.json [expected.json] required");
static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static InkStroke Stroke(params (float X, float Y)[] points)
    => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
static ushort Label(LegacyInkAlphabet alphabet, string text)
    => (ushort)Enumerable.Range(0, alphabet.LabelCount).First(i => alphabet.TryGetText((ushort)i, out string candidate) && candidate == text);
static void Write32(Span<byte> bytes, int at, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value);
var ak = File.ReadAllBytes(args[0]);
var model = LegacyRussianModelBundle.Load(ak, File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]));
var profile = LegacyMainWordTransitionProfile.ReadV106Node(ak, 188157);
var separation = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
var categories = model.CreateCursiveCategoryMapper(1);
Require(profile.NodeOffset == 188157 && profile.Categories.CompositeCategories.Length == 35 &&
    !float.IsFinite(BinaryPrimitives.ReadSingleLittleEndian(profile.DisabledPairFields.Span[12..])),
    "Main resource disabled pair field must preserve the observed inactive NaN as data.");
int guards = 0;
void Unsupported(Action action)
{
    try { action(); throw new Exception("Unsupported main transition configuration was accepted."); }
    catch (NotSupportedException) { guards++; }
}
Unsupported(() => LegacyMainWordTransitionProfile.ReadV106Node(ak, 11506));
byte[] invalidProfile = ak.ToArray(); invalidProfile[189165 + 189] = 1;
Unsupported(() => LegacyMainWordTransitionProfile.ReadV106Node(invalidProfile, 188157));
using var auditStream = File.Create(args[4]);
using var audit = new Utf8JsonWriter(auditStream);
JsonDocument? expectedDocument = args.Length == 6 ? JsonDocument.Parse(File.ReadAllBytes(args[5])) : null;
audit.WriteStartArray();
int fixtures = 0, states = 0, transitions = 0, canceled = 0;
ushort currentSymbol = Label(model.AcousticAlphabet, "м"), previousSymbol = Label(model.AcousticAlphabet, "и");
var raw = new[]
{
    Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),
    Stroke((70,10),(70,90),(120,10),(120,90)),
    Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
};
foreach (float scale in new[] { 1f, 2f })
    foreach (float sourceGap in new[] { -1f, 0.2f, 1.5f })
        foreach (int line in new[] { 0, -1 })
        {
            var translated = raw.Select(s => new InkStroke(s.Points.Select(p => new InkPoint(p.X * scale + 17, p.Y * scale - 9)).ToArray())).ToArray();
            // Main frames are explicit prepared inputs. These tests do not
            // substitute the first6120 frame or invent main postprocessing.
            float baseline = 90 * scale - 9, height = 80 * scale;
            var source = LegacyOrdinaryCursiveCutSource.Generate(translated, LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak), baseline, height);
            var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak, 189379);
            var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, height, seedProfile,
                new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
            var seedGraph = LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
            var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph, model.CursiveSpanProfile.MaximumAtoms);
            var connections = new LegacyOrdinaryCandidateConnections(graph);
            var sourceFacts = Enumerable.Range(0, graph.NodeCount).Select(node => (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)
                Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0, line, 0x10, -1, 0, 0, 0), graph.GetNode(node).Count).ToArray()).ToArray();
            var geometry = new LegacyMainWordTransitionGeometry(profile, model.AcousticAlphabet,
                source.Cuts.PreparedInk, graph, sourceFacts, height, sourceGap, separation);
            if (fixtures == 0)
            {
                var foreignFacts = sourceFacts.ToArray();
                foreignFacts[0] = new LegacyMainWordArcSourceFacts[65536];
                try
                {
                    _ = new LegacyMainWordTransitionGeometry(profile, model.AcousticAlphabet,
                        source.Cuts.PreparedInk, graph, foreignFacts, height, sourceGap, separation);
                    throw new Exception("Foreign oversized source row was accepted.");
                }
                catch (ArgumentException) { guards++; }
                foreach (int field in Enumerable.Range(0, 5))
                {
                    var invalidFacts = sourceFacts.Select(row => (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)row.ToArray()).ToArray();
                    var first = invalidFacts[0].ToArray();
                    first[0] = field switch
                    {
                        0 => first[0] with { BoundaryIndex = 0 },
                        1 => first[0] with { EndpointNibbles = 1 },
                        2 => first[0] with { GroupKind = 1 },
                        3 => first[0] with { GroupFlags6 = 1 },
                        _ => first[0] with { DescriptorFlags = 0x12 }
                    };
                    invalidFacts[0] = first;
                    Unsupported(() => new LegacyMainWordTransitionGeometry(profile, model.AcousticAlphabet,
                        source.Cuts.PreparedInk, graph, invalidFacts, height, sourceGap, separation));
                }
            }
            JsonElement expected = expectedDocument is null ? default : expectedDocument.RootElement[fixtures];
            audit.WriteStartObject(); audit.WriteNumber("height", height); audit.WriteNumber("sourceGap", sourceGap); audit.WriteNumber("line", line);
            audit.WriteStartArray("profile");
            foreach (float v in new[] { profile.DefaultGap, profile.TranslationThreshold, profile.GapSlope, profile.SourceGapScale, profile.SourceMatrixScale }) audit.WriteNumberValue(v);
            audit.WriteEndArray(); audit.WriteNumber("joinMaximum", profile.JoinMaximumCost); audit.WriteNumber("matrixMaximum", profile.MatrixMaximumCost);
            audit.WriteStartArray("matrix"); foreach (float v in profile.Matrix.Span) audit.WriteNumberValue(v); audit.WriteEndArray();
            audit.WriteStartArray("table"); foreach (float v in separation.Values.Span) audit.WriteNumberValue(v); audit.WriteEndArray();
            audit.WriteStartArray("points");
            foreach (var point in source.Cuts.PreparedInk.Points.Span)
            {
                audit.WriteStartArray(); audit.WriteNumberValue(point.X); audit.WriteNumberValue(point.Y); audit.WriteEndArray();
            }
            audit.WriteEndArray(); audit.WriteStartArray("pointCounts");
            foreach (int count in source.Cuts.PreparedInk.StrokePointCounts.Span) audit.WriteNumberValue(count);
            audit.WriteEndArray(); audit.WriteStartArray("groups");
            foreach (var group in graph.Groups)
            {
                audit.WriteStartArray();
                foreach (var range in group.Ranges.Span)
                {
                    audit.WriteStartArray(); audit.WriteNumberValue(range.StrokeIndex); audit.WriteNumberValue(range.StartPoint);
                    audit.WriteNumberValue(range.EndPointExclusive); audit.WriteEndArray();
                }
                audit.WriteEndArray();
            }
            audit.WriteEndArray();
            audit.WriteStartArray("graph");
            for (int node = 0; node < graph.NodeCount; node++)
            {
                audit.WriteStartArray(); var arcs = graph.GetNode(node);
                for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
                {
                    var state = geometry.GetPreparedState(node, ordinal);
                    audit.WriteStartObject(); audit.WriteNumber("span", arcs[ordinal].Span);
                    audit.WriteNumber("group", arcs[ordinal].GroupId);
                    audit.WriteStartArray("box");
                    foreach (float v in new[] { state.Bounds.MinimumX, state.Bounds.MinimumY, state.Bounds.MaximumX, state.Bounds.MaximumY }) audit.WriteNumberValue(v);
                    audit.WriteEndArray();
                    audit.WriteStartArray("pred"); foreach (int value in arcs[ordinal].PredecessorArcIndices.Span) audit.WriteNumberValue(value); audit.WriteEndArray();
                    audit.WriteStartArray("succ"); foreach (int value in connections.GetSuccessorOrdinals(node, (ushort)ordinal, 0).Span) audit.WriteNumberValue(value); audit.WriteEndArray();
                    audit.WriteBoolean("wholeStarts", graph.Groups[arcs[ordinal].GroupId].Ranges.Span.ToArray().All(r => r.StartPoint == 0));
                    audit.WriteStartArray("state"); audit.WriteNumberValue(state.FutureMinimumX); audit.WriteNumberValue(state.PrefixMaximumX);
                    audit.WriteNumberValue(state.JoinCost); audit.WriteNumberValue(state.TripleJoinCost); audit.WriteEndArray(); audit.WriteEndObject();
                    if (expectedDocument is not null)
                    {
                        var e = expected.GetProperty("states")[node][ordinal];
                        Require(state.FutureMinimumX == e[0].GetSingle() && state.PrefixMaximumX == e[1].GetSingle() &&
                            state.JoinCost == e[2].GetInt32() && state.TripleJoinCost == e[3].GetInt32(), "Independent prepared cache differs.");
                    }
                    states++;
                }
                audit.WriteEndArray();
            }
            audit.WriteEndArray(); audit.WriteStartArray("calls");
            int callIndex = 0;
            var contexts = new LegacySymbolContextFactory();
            ulong suffix = contexts.ResolveInitial(currentSymbol).Identity;
            for (int node = 1; node < graph.NodeCount; node++)
                for (ushort arc = 0; arc < graph.GetNode(node).Count; arc++)
                {
                    var candidate = graph.GetNode(node)[arc];
                    int previous = node - candidate.Span - 1;
                    if (previous < 0) continue;
                    foreach (int preceding in candidate.PredecessorArcIndices.Span)
                        foreach (byte kind in new byte[] { 0, 2 })
                            foreach (uint sourceKind in new uint[] { 6, 7, 8 })
                                foreach (int sourcePrevious in new[] { previous, 0 }.Distinct())
                            {
                                ushort sourcePreviousArc = sourcePrevious == previous ? (ushort)preceding : (ushort)0;
                                int before = callIndex % 7 == 0 ? int.MaxValue : 100 + callIndex;
                                int accumulated = unchecked(10101010 + callIndex * 517);
                                var parentFields = new LegacyInitialWordRecordFields(1UL << 32, 0, before, 0, previousSymbol,
                                    (ushort)sourcePrevious, sourcePreviousArc, (ushort)preceding,
                                    (byte)categories.GetSymbolCompositeCategory(previousSymbol),
                                    0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, previous, ushort.MaxValue);
                                byte[] parentSlot = new byte[128]; LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(parentFields, 1).CopyTo(parentSlot, 0);
                                var parent = LegacyNativeWordRowMaterializer.Materialize(parentSlot, 0, 1, 1, 1);
                                var currentFields = parentFields with
                                {
                                    SourceTag = 0, Auxiliary08 = suffix, Label24 = currentSymbol, SourceNode18 = (ushort)node,
                                    PackedSourceArcIndex = arc, SourceArcIndex1c = arc,
                                    CompositeCategory = (byte)categories.GetSymbolCompositeCategory(currentSymbol),
                                    SourceSpan = (sbyte)candidate.Span, PackedKind27 = kind
                                };
                                byte[] currentSlot = new byte[128]; LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(currentFields, 1).CopyTo(currentSlot, 0);
                                byte[] residue = Enumerable.Repeat((byte)0x9d, 80).ToArray(); Write32(residue, 48, accumulated);
                                var request = LegacyNativeWordRecombinationRequestFactory.Create(currentSlot, (uint)node, parent.Record.Span,
                                    profile.Categories, (_, _) => parent, tag => (tag == 0 ? sourceKind : 6u) << 9,
                                    contexts, new(65527, 65531, 65532, ushort.MaxValue), 0, new(0x1122334455667788, 0), residue);
                                byte[] beforeBytes = request.GetSnapshot().ToArray();
                                bool accepted = geometry.Calculate118(request);
                                byte[] after118 = request.GetSnapshot().ToArray(); int after = request.Cost.Value;
                                Require(geometry.Validate140(request) && request.Cost.Value == after && request.GetSnapshot().Span.SequenceEqual(after118),
                                    "Main disabled pair updater must preserve the full packet and cost.");
                                for (int at = 0; at < 80; at++) if (at is < 48 or > 51)
                                    Require(beforeBytes[at] == after118[at], "Main callback changed unrelated request residue/flags.");
                                audit.WriteStartArray();
                                foreach (int value in new[] { node, (int)arc, sourcePrevious, (int)sourcePreviousArc, (int)kind, (int)sourceKind,
                                    request.ReadInt32(20), request.ReadInt32(24), before, accumulated, after, request.ReadInt32(48), accepted ? 1 : 0 }) audit.WriteNumberValue(value);
                                audit.WriteEndArray();
                                if (expectedDocument is not null)
                                {
                                    var e = expected.GetProperty("calls")[callIndex];
                                    Require(after == e[0].GetInt32() && request.ReadInt32(48) == e[1].GetInt32() && accepted == e[2].GetBoolean(),
                                        $"Independent full80 native callback differs at fixture{fixtures}/call{callIndex}.");
                                }
                                using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
                                try { geometry.Calculate118(request, cancellation.Token); throw new Exception("Main callback ignored cancellation."); }
                                catch (OperationCanceledException) { canceled++; }
                                callIndex++; transitions++;
                            }
                }
            audit.WriteEndArray(); audit.WriteEndObject(); fixtures++;
        }
audit.WriteEndArray(); audit.Flush(); expectedDocument?.Dispose();
ActualMainWordTransitionConsumer.Run(args);
Console.WriteLine($"PASS {fixtures} main188157 prepared source fixtures / {states} actual graph caches / {transitions} full80 callbacks / {canceled} cancellation guards / {guards} unsupported-scope guards. " +
    "Frame/gap/source facts explicit; first-frame substitution and production acceptance remain excluded.");
