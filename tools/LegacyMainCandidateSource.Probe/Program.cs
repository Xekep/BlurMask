using InkRecognition;
using System.Text.Json;

if (args.Length != 9) throw new ArgumentException("AK LK engine separation case publication line30 line50 output");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
byte[] line30 = File.ReadAllBytes(args[6]), line50 = File.ReadAllBytes(args[7]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
InkStroke[] raw = [new(new InkPoint[] { new(0,90), new(0,90), new(0,10), new(25,60), new(50,10), new(50,90) })];
var old = LegacyFreshMainSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50, raw, 0, table, cases, publication)
    .Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
var first = LegacyFreshMainFirstCandidateCompletion.Complete(old, new byte[400], new byte[4], new byte[4]);
var oldView = LegacyOwnedMainCandidateSource.FromFirst(first);
var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256],
    new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
var configured = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50,
    raw, 0, table, cases, publication).Run(residue, request => new byte[request.RequiredBytes], new byte[20000*40]);
var actual = configured.Alternatives.CopyActiveAlternatives();
var firstConfigured = LegacyOwnedMainCandidateSource.FromConfigured(configured, actual[0]);
var secondConfigured = LegacyOwnedMainCandidateSource.FromConfigured(configured, actual[1]);
int guards = 0, projected = 0;
Reject(() => LegacyOwnedMainCandidateSource.FromConfigured(configured, actual[0], new(true)));
Reject(() => LegacyOwnedMainCandidateSource.FromFirst(first, new(true)));
Reject(() => _ = oldView.ConfiguredResult);
Reject(() => _ = oldView.ConfiguredAlternative);
if (!ReferenceEquals(firstConfigured.SourceIdentity, secondConfigured.SourceIdentity) ||
    !ReferenceEquals(firstConfigured.TopOwner, secondConfigured.TopOwner) ||
    ReferenceEquals(firstConfigured.CandidateIdentity, secondConfigured.CandidateIdentity) ||
    ReferenceEquals(firstConfigured.Session, secondConfigured.Session) ||
    !ReferenceEquals(oldView.RawResult, old) || !ReferenceEquals(oldView.FirstCandidate, first))
    throw new Exception("Source/TOP/candidate identities were conflated.");

using var output = File.Create(args[8]); using var writer = new Utf8JsonWriter(output);
writer.WriteStartObject(); writer.WriteStartArray("sources");
foreach (var view in new[] { oldView, firstConfigured, secondConfigured })
{
    view.Validate();
    var metadata = view.IsConfigured
        ? LegacyOwnedMainGlyphSourceMetadata.PrepareConfigured(view, view.SourceOverlay)
        : LegacyOwnedMainGlyphSourceMetadata.Prepare(view.Ownership, view.SourceOverlay, view.Sampled.SourceRanges);
    if (view.IsConfigured)
    {
        Reject(() => _ = view.RawResult);
        Reject(() => _ = view.FirstCandidate);
        Reject(() => LegacyOwnedMainGlyphSourceMetadata.Prepare(view.Ownership, view.SourceOverlay, view.Sampled.SourceRanges));
    }
    else Reject(() => LegacyOwnedMainGlyphSourceMetadata.PrepareConfigured(view, view.SourceOverlay));
    writer.WriteStartObject(); writer.WriteBoolean("configured", view.IsConfigured);
    writer.WriteStartArray("glyphs");
    foreach (var word in view.CopyRawWordsInNativeHeadOrder())
    foreach (var glyph in word.Glyphs.Where(g => !g.IsProtocolSource))
    {
        var source = metadata.Read(glyph.CanonicalSourceKey);
        var positions = view.SourceDocument.ProjectCandidate(view, source);
        if (!ReferenceEquals(positions.SelectedSession, view.Session) || !ReferenceEquals(positions.CandidateSource, view) ||
            !ReferenceEquals(positions.Document, view.SourceDocument)) throw new Exception("Source projection lost its selected session/document.");
        if (view.IsConfigured)
        {
            Reject(() => _ = positions.Result);
            Reject(() => oldView.SourceDocument.ProjectCandidate(view, source));
        }
        else
        {
            var legacy = view.SourceDocument.Project(old, source);
            if (!legacy.Ranges.SequenceEqual(positions.Ranges) || !ReferenceEquals(positions.Result, old))
                throw new Exception("Historical source projection changed.");
        }
        Reject(() => view.SourceDocument.ProjectCandidate(view, source, new(true)));
        if (view.IsConfigured)
        {
            var other = ReferenceEquals(view, firstConfigured) ? secondConfigured : firstConfigured;
            Reject(() => view.SourceDocument.ProjectCandidate(other, source));
        }
        writer.WriteStartObject(); writer.WriteNumber("pointCount", view.SourceDocument.Strokes[0].Points.Count);
        writer.WriteBase64String("references", source.CopyReferenceStorage24());
        writer.WriteBase64String("residuals", source.CopyResidualStorage2());
        writer.WriteStartArray("positions");
        foreach (var range in positions.Ranges)
        { writer.WriteStartArray(); writer.WriteNumberValue(range.First.Value); writer.WriteNumberValue(range.Last.Value); writer.WriteEndArray(); }
        writer.WriteEndArray(); writer.WriteEndObject(); projected++;
    }
    writer.WriteEndArray(); writer.WriteEndObject();
}
writer.WriteEndArray();
//Reject a genuine foreign result/candidate pairing instead of adopting a
//matching graph, matching label string or the other result's original document.
var foreign = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, line30, line50,
    raw, 0, table, cases, publication).Run(residue, r => new byte[r.RequiredBytes], new byte[20000*40]);
Reject(() => LegacyOwnedMainCandidateSource.FromConfigured(foreign, actual[0]));
writer.WriteNumber("guards", guards); writer.WriteNumber("projected", projected); writer.WriteEndObject(); writer.Flush();
Console.WriteLine($"PASS historical + two actual configured source candidates, {projected} raw-document projections/{guards} guards; no count1 cast.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Foreign/stale/incorrect-variant source ownership accepted.");
}
