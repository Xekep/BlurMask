using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length is not (7 or 11))
    throw new ArgumentException("AK LK engine separation case publication capture [line30 line50 manifest word-index]");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, engine);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var data = LegacyNativeTextPublicationData.Load(pf);
LegacyFreshCaseRussianDictionaryPreparation dictionary;
LegacyOwnedMainWordSpanPreparation owner;
LegacyFreshMainSingleLineContext? lineContext = null;
if (args.Length == 7)
{
    var sampled = MainFixture.Create(ak, engine, table);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, data);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
}
else
{
    using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[9]));
    if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
        manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent")
        throw new InvalidDataException("Explicitly unsegmented pinned recorded coordinates required.");
    var sample = manifest.RootElement.GetProperty("samples").EnumerateArray()
        .Where(s => s.GetProperty("kind").GetString() == "word").ElementAt(int.Parse(args[10]));
    string name = sample.GetProperty("file").GetString()!;
    if (name != Path.GetFileName(name)) throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[9]))!, name));
    if (bytes.Length > 200000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != sample.GetProperty("trackSha256").GetString())
        throw new InvalidDataException("Recorded track hash changed.");
    using var parsed = JsonDocument.Parse(bytes);
    var points = parsed.RootElement.GetProperty("points").EnumerateArray()
        .Select(p => new InkPoint(p[0].GetSingle(), -p[1].GetSingle())).ToArray();
    if (points.Length is < 2 or > 4096) throw new InvalidDataException("Bounded recorded point vector required.");
    //No expected transcription or recovered pen-up labels enter this route.
    var input = TrackedCaseInput.Create(ak, engine, table, File.ReadAllBytes(args[7]), File.ReadAllBytes(args[8]),
        [new InkStroke(points)], cases, data);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, data);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
    lineContext = input.Context;
}

var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner));
ulong allocationId = 1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
{
    byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
    return new(bytes, allocationId++);
}
var pass = new LegacyOwnedMainCaseWordPass(ak, policy, lineContext is null ? null : owner.MainLineAcoustic, table, Allocate).RunOwned();
var invocation = LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy);
var terminal = invocation.SelectOwnedPass(pass);
var confidence = lineContext is null ? LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary) :
    LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary, lineContext);
var compact = LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak, policy, terminal, confidence, new byte[4096 * 40]);
var callbacks = lineContext is null ? LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak, confidence, owner.Source) :
    LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak, confidence, owner.MainLineSource);
var geometry = lineContext is null ? LegacyOwnedRussianMainSourceGeometry.Prepare(ak, confidence, owner.Source) :
    LegacyOwnedRussianMainSourceGeometry.Prepare(ak, confidence, owner.MainLineSource);
var interior = LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak, callbacks, geometry);
var backtrack = LegacyOwnedMainGlyphBacktrackRows.Prepare(ak, policy, compact, callbacks, new byte[72]);
var plan = LegacyOwnedMainGlyphFlushPlan.Prepare(policy, backtrack);
var ownership = LegacyFreshMainPlainResultOwnership.Create(compact, geometry);
var session = LegacyOwnedMainGlyphFlushSession.Prepare(ownership, plan, new byte[120]);
var publisher = LegacyOwnedMainPlainWordPublication.Prepare(ak, session, table, callbacks, interior);
var tokens = new List<LegacyOwnedMainWordPublicationResult>();
int guards = 0;
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, new byte[400]));
while (!session.IsComplete)
{
    var current = session.PrepareNext();
    var publication = publisher.Publish(current, new byte[40], new ushort[256], new byte[64]);
    if (!publication.Succeeded) throw new Exception("Actual callback failed; no alternative snapshot may be fabricated.");
    tokens.Add(publication); session.CompleteCurrent(publication);
}

Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(null!, session, tokens, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, null!, tokens, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, null!, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, new byte[399]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, new byte[401]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(invocation.SelectOwnedPass(pass), session, tokens, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, [], new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens.Append(tokens[0]).ToArray(), new byte[400]));
var missing = tokens.ToArray(); missing[0] = null!;
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, missing, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, new byte[400], new CancellationToken(true)));
var foreignSession = LegacyOwnedMainGlyphFlushSession.Prepare(ownership, plan, new byte[120]);
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, foreignSession, tokens, new byte[400]));
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(new byte[39]));

using var output = File.Create(args[6]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteNumber("nodes", pass.Storage.NodeCount);
w.WriteNumber("winnerOrdinal", terminal.Winner.Ordinal); w.WriteNumber("total", terminal.Winner.TotalCost);
w.WriteNumber("rowCost14", terminal.Winner.RowCost14); w.WriteNumber("cumulative", pass.Storage.CopyCumulativePathCosts()[^1]);
w.WriteString("selectedBeam", Convert.ToHexString(terminal.CopySelectedRecord()));
w.WriteString("publishedBeam", Convert.ToHexString(compact.GetNodeSnapshot(pass.Storage.NodeCount - 1)
    .Records.Span.Slice(terminal.Winner.Ordinal * 88, 88)));
w.WriteStartArray("cases");
for (int n = 0; n < 64; n++)
{
    byte[] residue = Enumerable.Range(0, 400).Select(i => (byte)(i * 37 + n * 13)).ToArray();
    byte[] before = residue.ToArray();
    var snapshot = LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, residue);
    if (!ReferenceEquals(snapshot.Terminal, terminal) || !ReferenceEquals(snapshot.Session, session) ||
        !ReferenceEquals(snapshot.WordHead18, ownership.WordPublisher.Head) ||
        !ReferenceEquals(snapshot.BeamOwner20, compact) ||
        snapshot.SelectedBeamOrdinal20 != terminal.Winner.Ordinal || snapshot.SourceNodeCount2310 != owner.SourceGraph.NodeCount ||
        snapshot.Reference78 != terminal.Winner.TotalCost || !residue.AsSpan().SequenceEqual(before) ||
        !snapshot.CopySelectedBeam88().AsSpan().SequenceEqual(compact.GetNodeSnapshot(pass.Storage.NodeCount - 1)
            .Records.Span.Slice(terminal.Winner.Ordinal * 88, 88)))
        throw new Exception("Snapshot lost actual source, terminal, WORD or raw storage ownership.");
    byte[] ctor40 = residue.AsSpan(0, 40).ToArray();
    LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(ctor40);
    if (!snapshot.CopyConstructorStorage400().AsSpan(0, 40).SequenceEqual(ctor40))
        throw new Exception("First row constructor did not operate in-place on actual allocation.");
    byte[] normalized = snapshot.CopyNormalizedScoreStorage400();
    byte[] escaped = snapshot.CopyNormalizedScoreStorage400(); escaped[0] ^= 0xff;
    if (!snapshot.CopyNormalizedScoreStorage400().AsSpan().SequenceEqual(normalized))
        throw new Exception("Snapshot leaked mutable output storage.");
    w.WriteStartObject(); w.WriteNumber("reference", snapshot.Reference78);
    w.WriteNumber("top80", BitConverter.SingleToUInt32Bits(snapshot.NormalizedTop80));
    w.WriteNumber("rawFirst08", BitConverter.SingleToUInt32Bits(snapshot.RawFirstMass08));
    w.WriteNumber("words", snapshot.PublishedWordCount);
    w.WriteString("residue", Convert.ToHexString(before));
    w.WriteString("constructed", Convert.ToHexString(snapshot.CopyConstructorStorage400()));
    w.WriteString("scored", Convert.ToHexString(snapshot.CopyScoredStorage400()));
    w.WriteString("normalized", Convert.ToHexString(normalized));
    w.WriteEndObject();
}
w.WriteEndArray();
//A genuine same-pool external append must be rejected, not adopted as if it
//were one of the session's completed callback nodes. This does not run207CB0.
var extra = ownership.CreateWorkingWord(new byte[120]);
extra.ReplaceHeadReference(ownership.WordPublisher.Head!.Word.HeadReference);
ownership.WordPublisher.Publish(extra, (_, _) => 0, (_, _) => 0);
Reject(() => LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, tokens, new byte[400]));
w.WriteNumber("guards", guards); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS actual first alternative snapshot:64 explicit400 allocations/{guards} guards; typed beam/WORD identities, no native failure-order or quote claim.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Malformed, incomplete, foreign or canceled snapshot accepted.");
}
