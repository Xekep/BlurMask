using InkRecognition;
using System.Text.Json;

if (args.Length != 9) throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] acoustic = File.ReadAllBytes(args[0]), lexical = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] spatial = File.ReadAllBytes(args[6]), physical = File.ReadAllBytes(args[7]);
var samples = new[] { Word(0), Word(0).Concat(Word(230)).ToArray(), Word(0).Concat(Word(300)).ToArray(),
    Word(0).Concat(Word(450)).ToArray(), Word(0).Concat(Word(700)).ToArray() };
var results = new List<LegacyFreshMainSingleLinePlainRecognitionResult>();
var snapshots = new List<LegacyOwnedMainFirstAlternativeSnapshot>();
var policies = new List<LegacyFreshMainResultGlyphNormalization>();
int guards = 0;
Reject(() => LegacyFreshMainResultGlyphNormalization.Prepare(null!));
foreach (var raw in samples)
{
    var recognition = LegacyFreshMainSingleLinePlainRecognition.Prepare(acoustic, lexical, engine, spatial, physical,
        raw, 0, table, cases, publication);
    var result = recognition.Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
    var snapshot = LegacyOwnedMainFirstAlternativeSnapshot.Capture(result,
        Enumerable.Range(0, 400).Select(i => (byte)(i * 31 + results.Count)).ToArray());
    var policy = LegacyFreshMainResultGlyphNormalization.Prepare(snapshot.SourcePolicy);
    Reject(() => LegacyFreshMainResultGlyphNormalization.Prepare(snapshot.SourcePolicy, new CancellationToken(true)));
    Reject(() => policy.Apply(null!));
    Reject(() => policy.Apply(snapshot, new CancellationToken(true)));
    //The actual existing quote bridge precedes21F320. These inputs do not
    //assert a synthetic quote event; its positive branch has separate controls.
    LegacyOwnedMainPublishedQuoteNormalization.Apply(result, new byte[4], new byte[4]);
    results.Add(result); snapshots.Add(snapshot); policies.Add(policy);
}
for (int p = 0; p < policies.Count; p++)
    for (int s = 0; s < snapshots.Count; s++)
        if (p != s) Reject(() => policies[p].Apply(snapshots[s]));

using var output = File.Create(args[8]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int i = 0; i < policies.Count; i++)
{
    var snapshot = snapshots[i]; var policy = policies[i]; var result = results[i];
    var pool = result.Session.Ownership.GlyphPool;
    var before = pool.CopyOwnerNodes();
    byte[] beforeScores = snapshot.CopyNormalizedScoreStorage400();
    var proof = policy.Apply(snapshot);
    var after = pool.CopyOwnerNodes();
    if (!ReferenceEquals(proof.Policy, policy) || !ReferenceEquals(proof.Snapshot, snapshot) ||
        !ReferenceEquals(policy.SourcePolicy, snapshot.SourcePolicy) || proof.ExecutedGlyphRewriteBody ||
        policy.ProvenFreshConfigurationF4 != 0 || policy.NativePropertyId != 0x012d0017 ||
        !snapshot.CopyNormalizedScoreStorage400().AsSpan().SequenceEqual(beforeScores) || before.Count != after.Count)
        throw new Exception("No-op proof lost actual source/result identity or changed numerical storage.");
    w.WriteStartObject(); w.WriteNumber("sourceNodes", snapshot.SourceNodeCount2310);
    w.WriteNumber("property", policy.NativePropertyId); w.WriteNumber("f4", policy.ProvenFreshConfigurationF4);
    w.WriteBoolean("executed", proof.ExecutedGlyphRewriteBody);
    w.WriteString("scoresBefore", Convert.ToHexString(beforeScores));
    w.WriteString("scoresAfter", Convert.ToHexString(snapshot.CopyNormalizedScoreStorage400()));
    WritePool("poolBefore", before); WritePool("poolAfter", after);
    var ownership = result.Session.Ownership;
    var extra = LegacyNativeWordBuilder.CreateEmpty(ownership.GlyphPool, new byte[120]);
    extra.AppendGlyph(0x7f0000, 0, true, new byte[40]); extra.SetLastOrdinarySymbol(0);
    ownership.WordPublisher.Publish(extra, (_, _) => 0, (_, _) => 0);
    Reject(() => policy.Apply(snapshot));
    w.WriteBoolean("foreignAppendRefused", true); w.WriteEndObject();
}
w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteEndObject(); w.Flush();
if (guards != 41) throw new Exception("Ownership/cancellation guard count changed.");
Console.WriteLine("PASS5 actual fresh raw→published→quote→F4 no-op proofs/41 ownership and cancellation guards; no enabled21F320 or acceptance claim.");

void WritePool(string name, IReadOnlyList<LegacyNativeGlyphRecordNode> nodes)
{
    w.WriteStartArray(name);
    foreach (var node in nodes)
    {
        w.WriteStartObject(); w.WriteNumber("key", node.Key); w.WriteNumber("count", node.PackedCount);
        w.WriteString("storage", Convert.ToHexString(node.Records.Span)); w.WriteEndObject();
    }
    w.WriteEndArray();
}
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Null, foreign, stale or canceled no-op source accepted.");
}
InkStroke[] Word(float x) => [Draw((x, 80), (x, 0), (x+30, 45), (x+60, 0), (x+60, 80)),
    Draw((x+85, 80), (x+85, 0), (x+135, 0), (x+135, 80)),
    Draw((x+165, 80), (x+165, 0), (x+215, 0), (x+215, 40), (x+165, 40))];
InkStroke Draw(params (float X, float Y)[] knots)
{
    var points = new List<InkPoint>();
    for (int i = 1; i < knots.Length; i++)
        for (int t = 0; t < 8; t++)
        {
            float f = t/8f;
            points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f, knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));
        }
    points.Add(new(knots[^1].X, knots[^1].Y)); return new(points.ToArray());
}
