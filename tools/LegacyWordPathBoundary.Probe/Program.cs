using System.Text.Json;
using InkRecognition;

if (args.Length != 2)
    throw new ArgumentException("controls.json acoustic.res");

// Actual resource configuration, shared by the independent controls.
var resource = File.ReadAllBytes(args[1]);
var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource, 189379);
var categoryProfile = LegacyCursiveCategoryProfile.Load(resource.AsSpan(188222), 188222, out _);
var categories = new LegacyCursiveCategoryMapper(categoryProfile);
var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource);
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, penalties = 0, lookups = 0;

foreach (var test in controls.RootElement.EnumerateArray())
{
    // Produce ordinary native primary cells and composed group ownership.
    int count = test.GetProperty("n").GetInt32();
    var raw = Enumerable.Range(0, count).Select(index => new InkStroke(new[]
    {
        new InkPoint(index * 100, 10),
        new InkPoint(index * 100 + 20, 90)
    })).ToArray();
    var prepared = LegacyCursivePreparedInk.Create(raw);
    var markers = Enumerable.Range(0, count)
        .Select(index => new LegacyCursiveCutMarker(0, index, 0)).ToArray();
    var atoms = LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span, markers);
    var cuts = new LegacyCursiveCutResult(prepared, markers, atoms);
    var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts, 80, seedProfile,
        new byte[count], Enumerable.Repeat(-1, count).ToArray());
    var seedGraph = LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
    var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph, 3);

    // Independent serialized input descriptors and group-to-input assignments.
    var descriptors = Convert.FromBase64String(test.GetProperty("descriptors").GetString()!);
    var inputs = test.GetProperty("inputs").EnumerateArray()
        .Select(value => (ushort)value.GetInt32()).ToArray();
    var boundaries = test.GetProperty("boundaries").EnumerateArray()
        .Select(value => value.GetInt32()).ToArray();
    var snapshot = new LegacyPreparedSourceBoundarySnapshot(graph, descriptors, inputs);
    for (int group = 0; group < graph.Groups.Count; group++)
    {
        Require(snapshot.GetGroupBoundary(group) == boundaries[inputs[group]],
            "Primary group/input descriptor bridge differs.");
    }
    for (int node = 0; node < graph.NodeCount; node++)
    {
        for (int arc = 0; arc < graph.GetNode(node).Count; arc++)
        {
            int group = graph.GetNode(node)[arc].GroupId;
            Require(snapshot.GetBoundary(node, arc) == boundaries[inputs[group]],
                "Node/arc source lookup used physical stroke or wrong group.");
        }
    }

    // Compare native field projection and exact conditional lookup order.
    var calls = new List<int>();
    int Lookup(int node, int arc)
    {
        Require(arc == 0, "Native transition boundary callback must use primary arc0.");
        calls.Add(node);
        return snapshot.GetBoundary(node, arc);
    }
    var record = Convert.FromBase64String(test.GetProperty("record").GetString()!);
    var word = LegacyPreparedWordPathReference.Read(record, test.GetProperty("end").GetInt32());
    var projection = LegacyWordPathBoundaryCost.Evaluate(word, categories,
        test.GetProperty("available").GetInt32(), test.GetProperty("mode").GetBoolean(),
        test.GetProperty("pathType").GetByte(), test.GetProperty("lookup").GetBoolean() ? Lookup : null);
    int[] actual =
    [
        projection.MappedCategory,
        projection.SourceStartNode,
        projection.PreviousNode,
        projection.BoundaryDifference,
        projection.AddedCost
    ];
    var expected = test.GetProperty("expected").EnumerateArray().Select(value => value.GetInt32());
    Require(actual.SequenceEqual(expected), "Packed path/native category/transition correction differs.");
    var expectedCalls = test.GetProperty("calls").EnumerateArray().Select(value => value.GetInt32());
    Require(calls.SequenceEqual(expectedCalls), "Conditional native source lookup order differs.");

    // Check the separately proven fresh ordinary input-record producer.
    var ordinary = LegacyOrdinaryCursiveCutSource.Generate(raw, sourceProfile, 90, 80);
    var fresh = LegacyPreparedSourceBoundarySnapshot.CreateFreshOrdinary(ordinary, graph);
    for (int node = 0; node < graph.NodeCount; node++)
    {
        for (int arc = 0; arc < graph.GetNode(node).Count; arc++)
        {
            Require(fresh.GetBoundary(node, arc) == -1,
                "Fresh ordinary default input boundary differs.");
        }
    }
    Require(snapshot.GetGroupBoundary(-1) == -1, "Missing-group callback branch changed.");

    // Bounds and cancellation are checked before invalid source access.
    try
    {
        var missingInputs = Enumerable.Repeat(ushort.MaxValue, inputs.Length).ToArray();
        new LegacyPreparedSourceBoundarySnapshot(graph, descriptors, missingInputs);
        throw new Exception("Missing input descriptor accepted.");
    }
    catch (ArgumentException) { }
    try
    {
        LegacyPreparedWordPathReference.Read(new byte[0x57], 0);
        throw new Exception("Malformed native record accepted.");
    }
    catch (ArgumentException) { }
    try
    {
        LegacyWordPathBoundaryCost.Evaluate(word, categories, count, false, 8, Lookup,
            new CancellationToken(true));
        throw new Exception("Cancellation ignored.");
    }
    catch (OperationCanceledException) { }

    penalties += projection.AddedCost != 0 ? 1 : 0;
    lookups += calls.Count;
    cases++;
}

Console.WriteLine($"PASS {cases} independent packed0x58 word/input-descriptor controls; " +
    $"{penalties} native category2/4 corrections/{lookups} ordered source lookups, " +
    "real RU35-composite mapper, primary merged-graph source mapping, fresh ordinary input producer " +
    "and malformed/missing/cancel guards. Native previous-record selection/winner/confidence " +
    "remain outside this stage.");

static void Require(bool value, string message)
{
    if (!value)
        throw new InvalidDataException(message);
}
