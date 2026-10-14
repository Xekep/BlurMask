using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] acoustic = File.ReadAllBytes(args[1]);
var model = LegacyRussianModelBundle.Load(acoustic, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
Require(model.CursiveSpanProfile.MaximumAtoms == 8, "Actual main graph resource cap differs.");
Require(model.AcousticAlphabet.TryGetLabel("\ufffd", out ushort replacement), "Actual replacement symbol missing.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, graphs = 0;
LegacyOrdinaryCursiveCandidateGraph? firstGraph = null;
foreach (var profile in document.RootElement.GetProperty("profiles").EnumerateArray())
{
    var counts = profile.GetProperty("counts").EnumerateArray().Select(c => c.GetInt32()).ToArray();
    var graph = CreateGraph(counts, model.CursiveSpanProfile.MaximumAtoms);
    firstGraph ??= graph;
    var gate = new LegacyNativeWordLanguageGate(model, graph);
    Require(graph.MaximumAtoms == 8, "Candidate graph lost its actual generation cap.");
    foreach (var test in profile.GetProperty("tests").EnumerateArray())
    {
        int node = test.GetProperty("node").GetInt32(), current = test.GetProperty("current").GetInt32();
        var rows = test.GetProperty("rows").EnumerateArray().Select(row =>
        {
            byte[] bytes = Convert.FromHexString(row.GetProperty("bytes").GetString()!);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(36), row.GetProperty("replacement").GetBoolean() ? replacement : (ushort)0);
            return bytes;
        }).SelectMany(row => row).ToArray();
        uint[] headers = test.GetProperty("headers").EnumerateArray().Select(h => h.GetUInt32()).ToArray();
        Require(gate.HasFollowingBoundary(node) == test.GetProperty("following").GetBoolean(),
            "Independent graph108 lookahead differs.");
        if (current != -1)
            Require(gate.HasBoundary(current, node) == test.GetProperty("boundary").GetBoolean(),
                "Independent graph100 span/physical overlap flag differs.");
        Require(gate.ShouldRun(node, current, test.GetProperty("packed").GetUInt32(), test.GetProperty("primary").GetUInt16(),
            rows, test.GetProperty("distance").GetInt32(), headers) == test.GetProperty("expected").GetBoolean(),
            "Independent primary/secondary row range, sentinel, category or signed wrap gate differs.");
        cases++;
    }
    graphs++;
}
Console.WriteLine($"PASS {graphs} independent owned physical-overlap graphs/{cases} exact language gate controls.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid language gate ownership accepted.");
}
Reject(() => new LegacyNativeWordLanguageGate(null!, firstGraph!));
Reject(() => new LegacyNativeWordLanguageGate(model, null!));
Reject(() => new LegacyNativeWordLanguageGate(model, CreateGraph([2], 7)));
var guardGate = new LegacyNativeWordLanguageGate(model, CreateGraph([2], 8));
Reject(() => guardGate.HasBoundary(-1, 0));
Reject(() => guardGate.HasBoundary(0, -2));
Reject(() => guardGate.HasFollowingBoundary(2));
Reject(() => guardGate.ShouldRun(-1, -1, 8, 0, [], 0, []));
Reject(() => guardGate.ShouldRun(0, 2, 8, 0, [], 0, []));
Reject(() => guardGate.ShouldRun(0, -1, 0, 1, new byte[87], 0, [0]));
Reject(() => guardGate.ShouldRun(0, -1, 0, 1, [], 0, [0]));
Reject(() => guardGate.ShouldRun(0, -1, 0, 0, [], 0, []));
byte[] invalidSource = new byte[88];
invalidSource[4] = 1;
Reject(() => guardGate.ShouldRun(0, -1, 0, 1, invalidSource, 0, [0]));
Reject(() => guardGate.ShouldRun(0, -1, 8, 0, [], 0, [], new CancellationToken(true)));
Require(guardGate.ShouldRun(0, -1, 8, ushort.MaxValue, new byte[1], 0, []),
    "Global controller bit should return before unvisited row access.");
Console.WriteLine($"PASS {guards} cap/model/node/extent/source/cancellation guards and native short-circuit ownership.");
ActualMainLanguageGate.Run(args);

static LegacyOrdinaryCursiveCandidateGraph CreateGraph(int[] counts, int cap)
{
    int horizontal = 0;
    var strokes = counts.Select(count =>
    {
        var points = Enumerable.Range(0, count * 2 + 1).Select(i => new InkPoint(horizontal + i * 10, i % 2 == 0 ? 10 : 20)).ToArray();
        horizontal += count * 20 + 30;
        return new InkStroke(points);
    }).ToArray();
    var prepared = LegacyCursivePreparedInk.Create(strokes);
    var markers = counts.SelectMany((count, stroke) => Enumerable.Range(0, count).Select(i => new LegacyCursiveCutMarker(0, stroke, i * 2))).ToArray();
    var cuts = new LegacyCursiveCutResult(prepared, markers, LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span, markers));
    var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts, 80, new(false, 1, 0, 0),
        new byte[strokes.Length], Enumerable.Repeat(-1, strokes.Length).ToArray());
    return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds), cap);
}
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
