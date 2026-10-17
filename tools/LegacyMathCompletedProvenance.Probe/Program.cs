using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
byte[] resource = File.ReadAllBytes(args[1]);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
var features = MathPreprocessor.Process([
    new InkStroke([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new InkStroke([new InkPoint(35, 0), new(35, 30)])]);
using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or
        OperationCanceledException or InvalidDataException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Required shared-generation guard did not reject.");
}
string AncestryHash(int[][] values)
{
    byte[] bytes = new byte[values.Sum(row => row.Length) * 4]; int at = 0;
    foreach (var row in values) foreach (int value in row)
    { BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(at), value); at += 4; }
    return Convert.ToHexStringLower(SHA256.HashData(bytes));
}
bool Equal(ushort[][] a, ushort[][] b) => a.Length == b.Length && a.Zip(b).All(pair => pair.First.SequenceEqual(pair.Second));
void MutateReadCopies(Math2Mode0CompletedEventLease lease)
{
    var primary = lease.CopyPrimary(); var secondary = lease.CopySecondary();
    var ancestry = lease.CopyAncestry(); string expected = AncestryHash(ancestry);
    ushort[] expectedPrimary = primary.ToArray(), expectedSecondary = secondary.ToArray();
    primary.AsSpan().Fill(65535); secondary.AsSpan().Fill(65535);
    foreach (var row in ancestry) row.AsSpan().Fill(1234567);
    if (ancestry.Length != 0) ancestry[0] = [7654321];
    if (!lease.CopyPrimary().SequenceEqual(expectedPrimary) || !lease.CopySecondary().SequenceEqual(expectedSecondary) ||
        AncestryHash(lease.CopyAncestry()) != expected)
        throw new InvalidDataException("Inspection must never expose mutable shared arrays.");
}
var runs = new List<ActualRun>();
foreach (int width in new[] { 1, 3, 16 })
{
    var prefix = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, width);
    var vector = prefix.CreateCompletedRecords();
    var retained = new List<Math2Mode0CompletedEventLease>();
    var steps = new List<Step>();
    var generationIds = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
    var originals = new List<(Math2Mode0PrefixGeneration Generation, ushort[][] Primary, ushort[][] Secondary, string Hash)>();
    using var other = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, width);
    var foreignStep = other.RunStep();
    for (ulong position = 0; position < prefix.ColdMaximumPosition; position++)
    {
        var step = prefix.RunStep();
        var generation = step.Generation;
        var candidates = step.CreateCandidates(1f, width == 1 ? -1f : 1000f);
        var primary = generation.CopyPrimary(); var secondary = generation.CopySecondary();
        var ancestry = generation.CopyAncestry(); string hash = AncestryHash(ancestry);
        int generationId = generationIds.Count + 1; generationIds.Add(generation, generationId);
        originals.Add((generation, primary, secondary, hash));
        if (position == 0)
        {
            Reject(() => step.BindCompletedGeneration(foreignStep.Generation, 0));
            Reject(() => step.BindCompletedGeneration(prefix.CopyGeneration(), 0));
            Reject(() => step.BindCompletedGeneration(null!, 0));
        }
        Reject(() => step.BindCompletedGeneration(generation, -1));
        Reject(() => step.BindCompletedGeneration(generation, generation.ActiveCount));
        if (!ReferenceEquals(step.BindCompletedGeneration(generation, 0), generation))
            throw new InvalidDataException("A valid binding must preserve the actual generation identity.");
        // One output copy per Step in this diagnostic ONLY, to measure the
        // future attention matrix footprint. No per-event attention is copied.
        long[] attentionShape = step.CopyOutput(1).Shape;
        if (attentionShape.Length != 3 || attentionShape[1] != generation.ActiveCount || attentionShape[2] < 1)
            throw new InvalidDataException("Actual current attention shape required.");
        var events = new List<Event>();
        for (int ordinal = 0; ordinal < candidates.CopyCompletedScoreFacts().Length; ordinal++)
        {
            var lease = vector.Capture(candidates, ordinal);
            if (!ReferenceEquals(lease.CompletedGenerationIdentity, generation))
                throw new InvalidDataException("Every actual event must share that exact Step generation.");
            long ancestryElements = ancestry.Sum(row => (long)row.Length);
            int parent = lease.CompletedSourceParent;
            if (lease.SharedGenerationAncestryElements != ancestryElements ||
                lease.SharedGenerationPrefixElements != primary.Sum(row => (long)row.Length) + secondary.Sum(row => (long)row.Length) ||
                lease.AvoidedRetainedProvenanceDataBytes != ancestryElements * 4 + primary[parent].Length * 2L + secondary[parent].Length * 2L)
                throw new InvalidDataException("Retained data-element accounting disagrees with the actual generation.");
            MutateReadCopies(lease);
            events.Add(new(lease.Rank, parent, Convert.ToBase64String(lease.CopyRawRecord()),
                lease.CopyPrimary(), lease.CopySecondary(), lease.CopyAncestry(), hash, generationId,
                lease.AvoidedRetainedProvenanceDataBytes, checked(attentionShape[0] * generation.Offset * 4)));
            retained.Add(lease); vector.Append(lease);
        }
        if (events.Count > 1 && events.Any(item => item.GenerationId != events[0].GenerationId))
            throw new InvalidDataException("Multiple EOS events of one Step must share a single generation.");
        if (!Equal(primary, generation.CopyPrimary()) || !Equal(secondary, generation.CopySecondary()) ||
            AncestryHash(generation.CopyAncestry()) != hash)
            throw new InvalidDataException("Copy mutation reached the immutable generation.");
        prefix.ApplySelected(candidates.Select());
        Reject(() => step.BindCompletedGeneration(generation, 0));
        steps.Add(new(position, generationId, generation.ActiveCount, generation.Width, primary,
            secondary, ancestry, hash, attentionShape, events.ToArray()));
    }
    foreach (var prior in originals)
        if (!Equal(prior.Primary, prior.Generation.CopyPrimary()) ||
            !Equal(prior.Secondary, prior.Generation.CopySecondary()) ||
            AncestryHash(prior.Generation.CopyAncestry()) != prior.Hash)
            throw new InvalidDataException("Advancing selected parents mutated an old generation.");
    // Same immutable provenance still readable after tensor-context disposal;
    // this copies no tensor handles and confers no active-Step permission.
    prefix.Dispose();
    foreach (var lease in retained) MutateReadCopies(lease);
    runs.Add(new(width, prefix.ColdMaximumPosition, steps.ToArray(), retained.Count,
        retained.Select(lease => lease.CompletedGenerationIdentity).Distinct(ReferenceEqualityComparer.Instance).Count(),
        retained.Sum(lease => lease.AvoidedRetainedProvenanceDataBytes), true));
}
File.WriteAllText(args[3], JsonSerializer.Serialize(new Capture(runtime.Version,
    Convert.ToHexStringLower(SHA256.HashData(resource)), runs.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS{runs.Count} actual runs/{runs.Sum(value => value.Events)} events; " +
    $"{runs.Sum(value => value.AvoidedRetainedDataBytes)} duplicated provenance data bytes avoided; " +
    $"{guards} guards; no automatic attention storage or CLR-overhead estimate");

record Event(int Rank, int Parent, string Raw48, ushort[] Primary, ushort[] Secondary, int[][] Ancestry,
    string AncestryHash, int GenerationId, long AvoidedRetainedDataBytes, long PotentialAttentionMatrixBytes);
record Step(ulong Offset, int GenerationId, int Active, int Width, ushort[][] Primary,
    ushort[][] Secondary, int[][] Ancestry, string AncestryHash, long[] AttentionShape, Event[] Events);
record ActualRun(int PreparedWidth, ulong Maximum, Step[] Steps, int Events, int RetainedGenerationIdentities,
    long AvoidedRetainedDataBytes, bool DetachedReadsAfterDispose);
record Capture(string Runtime, string Resource, ActualRun[] Runs, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext { }
