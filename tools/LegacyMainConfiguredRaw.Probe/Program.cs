using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 10) throw new ArgumentException("AK LK engine separation case publication line30 line50 manifest output");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
byte[] line30 = File.ReadAllBytes(args[6]), line50 = File.ReadAllBytes(args[7]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
var inputs = new List<(string Id, InkStroke[] Strokes, bool Recorded)>
{
    ("original-M-trajectory", [new(new InkPoint[] { new(0,90), new(0,90), new(0,10), new(25,60), new(50,10), new(50,90) })], false)
};
if (args[8] != "--synthetic-only")
{
    using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[8]));
    if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
        manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent")
        throw new InvalidDataException("Pinned recorded corpus with explicitly absent pen-up metadata required.");
    foreach (var entry in manifest.RootElement.GetProperty("samples").EnumerateArray().Where(s => s.GetProperty("kind").GetString() == "word"))
    {
        string file = entry.GetProperty("file").GetString()!;
        if (file != Path.GetFileName(file)) throw new InvalidDataException("Direct sibling coordinate file required.");
        byte[] bytes = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[8]))!, file));
        if (bytes.Length > 200000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != entry.GetProperty("trackSha256").GetString())
            throw new InvalidDataException("Pinned actual coordinate bytes required.");
        using var track = JsonDocument.Parse(bytes);
        var points = track.RootElement.GetProperty("points").EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), -p[1].GetSingle())).ToArray();
        if (points.Length is < 2 or > 4096) throw new InvalidDataException("Bounded recorded track required.");
        inputs.Add((entry.GetProperty("id").GetString()!, [new(points)], true));
    }
}
int completed = 0, refused = 0, guards = 0;
using var output = File.Create(args[9]); using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject(); writer.WriteStartArray("inputs");
foreach (var input in inputs)
{
    var prepared = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50,
        input.Strokes, 0, table, cases, publication);
    if (!ReferenceEquals(prepared.SourceDocument, prepared.InputOwner.SourceDocument))
        throw new Exception("Configured input lost its actual raw document identity.");
    var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40],
        new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
    Reject(() => prepared.Run(residue, Allocate, new byte[39]));
    Reject(() => prepared.Run(residue, Allocate, new byte[40], new(true)));
    writer.WriteStartObject(); writer.WriteString("id", input.Id); writer.WriteBoolean("recorded", input.Recorded);
    try
    {
        var result = prepared.Run(residue, Allocate, new byte[20000 * 40]);
        if (!ReferenceEquals(result.InputOwner, prepared.InputOwner) || !ReferenceEquals(result.SourceDocument, prepared.SourceDocument) ||
            !ReferenceEquals(result.Sampled.SourceRanges.Graph, result.Pass.SourcePolicy.Graph) ||
            !ReferenceEquals(result.Pass, result.Compact.PassResult) || !ReferenceEquals(result.Compact, result.Alternatives.Materialization))
            throw new Exception("Configured raw source/pass/pool lineage changed.");
        completed++;
        writer.WriteBoolean("completed", true); writer.WriteNumber("nodes", result.Pass.NodeCount);
        writer.WriteNumber("compactCount", result.Compact.RecordCount);
        writer.WriteNumber("reference", result.Alternatives.Reference78);
        writer.WriteBase64String("scores", result.Alternatives.CopyStorage());
        writer.WriteNumber("outputIndex", result.Alternatives.OutputIndex);
        writer.WriteNumber("top80bits", BitConverter.SingleToUInt32Bits(result.Alternatives.FinalScores!.Top80));
        var final = result.Compact.GetNodeSnapshot(result.Pass.NodeCount - 1);
        writer.WriteNumber("sourceEnd", final.SourceEnd23f0);
        writer.WriteNumber("primary", result.Pass.SourcePolicy.Controller.Configuration.PrimaryArcLimit230a);
        writer.WriteNumber("cumulative", final.CumulativePathCost2520);
        writer.WriteBase64String("records", final.Records.Span);
        writer.WriteBase64String("secondary", final.SecondaryAlternatives.Span);
        writer.WriteStartArray("contexts");
        foreach (var context in result.Pass.SourcePolicy.Controller.Owner.Contexts.CopyNodes())
        { writer.WriteStartArray(); writer.WriteNumberValue(context.Identity); writer.WriteNumberValue(context.ParentLink); writer.WriteNumberValue(context.Symbol); writer.WriteEndArray(); }
        writer.WriteEndArray();
        writer.WriteStartArray("excluded"); foreach (ulong id in result.Alternatives.CopyExcludedIdentities()) writer.WriteNumberValue(id); writer.WriteEndArray();
        writer.WriteStartArray("attempts");
        foreach (var attempt in result.Alternatives.CopyAttempts())
        {
            writer.WriteStartObject(); writer.WriteNumber("outputIndex", attempt.OutputIndex);
            writer.WriteBoolean("duplicate", attempt.Duplicate); writer.WriteNumber("duplicateOf", attempt.DuplicateOf);
            if (attempt.Candidate is { } candidate)
            {
                writer.WriteNumber("identity", candidate.Terminal.Selection.ExclusionIdentity);
                writer.WriteNumber("total", candidate.Terminal.Selection.Winner.TotalCost);
                writer.WriteNumber("row14", candidate.Terminal.Selection.Winner.RowCost14);
                writer.WriteNumber("ordinal", candidate.Terminal.Selection.Winner.Ordinal);
                writer.WriteNumber("secondaryOrdinal", candidate.Terminal.Selection.SecondaryOrdinal);
                writer.WriteNumber("exclusionCount", attempt.ExclusionCount);
                writer.WriteBoolean("released", candidate.IsReleased);
                writer.WriteBase64String("initial", candidate.Session.CopyInitialWordStorage());
                writer.WriteStartArray("packets");
                foreach (var token in candidate.CopyPublications()) writer.WriteBase64StringValue(token.Association.Packet.CopyStorage());
                writer.WriteEndArray();
                writer.WriteStartArray("words");
                for (var node = candidate.WordHead18; node is not null; node = node.Next)
                {
                    writer.WriteStartObject(); writer.WriteBase64String("storage", node.Word.CopyStorage());
                    writer.WriteStartArray("references"); foreach (ulong reference in node.Word.FlatReferences) writer.WriteNumberValue(reference); writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                if (!candidate.IsReleased)
                { writer.WriteStartArray("rawUtf8"); foreach (var text in candidate.CopyRawUtf8Words()) writer.WriteBase64StringValue(text); writer.WriteEndArray(); }
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteStartArray("pool");
        foreach (var node in result.Alternatives.TopOwner!.GlyphPool.CopyOwnerNodes())
        { writer.WriteStartObject(); writer.WriteNumber("key", node.Key); writer.WriteBase64String("records", node.Records.Span); writer.WriteEndObject(); }
        writer.WriteEndArray();
    }
    catch (NotSupportedException error)
    {
        refused++; writer.WriteBoolean("completed", false); writer.WriteString("refusal", error.ToString());
        Console.WriteLine($"REFUSED {input.Id}: {error.Message}");
    }
    Reject(() => prepared.Run(residue, Allocate, new byte[40]));
    writer.WriteEndObject();
}
writer.WriteEndArray(); writer.WriteNumber("completed", completed); writer.WriteNumber("refused", refused);
writer.WriteNumber("guards", guards); writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"Actual configured raw inputs: {completed} completed/{refused} explicit unsupported/{guards} lifecycle guards. No expected transcription was read.");
static byte[] Allocate(LegacyMainAlternativeAllocationRequest request)
{ byte[] result = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(result); return result; }
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Configured raw input reused a consumed session or accepted a failed precondition.");
}
