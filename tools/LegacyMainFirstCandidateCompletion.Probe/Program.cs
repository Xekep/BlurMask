using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 9) throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] spatial = File.ReadAllBytes(args[6]), physical = File.ReadAllBytes(args[7]);
var samples = new[] { Word(0), Word(0).Concat(Word(230)).ToArray(), Word(0).Concat(Word(300)).ToArray(),
    Word(0).Concat(Word(450)).ToArray(), Word(0).Concat(Word(700)).ToArray() };
int guards = 0, wordCount = 0, glyphCount = 0;
Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(null!, new byte[400], new byte[4], new byte[4]));
using var output = File.Create(args[8]); using var w = new Utf8JsonWriter(output);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int i = 0; i < samples.Length; i++)
{
    var raw = Run(samples[i]);
    byte[] allocation = Enumerable.Range(0, 400).Select(n => (byte)(n * 31 + i)).ToArray();
    byte[] leading = [0xc1, 0x4e, 0xa5, 0x73], trailing = [0xd2, 0x5f, 0xb6, 0x84];
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, new byte[399], leading, trailing));
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, new byte[401], leading, trailing));
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, new byte[3], trailing));
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, leading, new byte[3]));
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, leading, leading));
    Reject(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, leading, trailing, new CancellationToken(true)));
    var done = LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, leading, trailing);
    if (!ReferenceEquals(done.RawResult, raw) || !ReferenceEquals(done.Snapshot.Terminal, raw.Terminal) ||
        !ReferenceEquals(done.GlyphNormalization.Snapshot, done.Snapshot) || done.GlyphNormalization.ExecutedGlyphRewriteBody ||
        done.Reference78 != raw.Terminal.Winner.TotalCost || done.RawFirstMass08 != 1 || done.FinalTop80 != -1)
        throw new Exception("Completion lost the actual result/terminal/fresh-normalization owner or scalar channels.");
    byte[] original = done.CopyFinalScoreStorage400(); var escaped = done.CopyFinalScoreStorage400(); escaped[0] ^= 0xff;
    if (!done.CopyFinalScoreStorage400().AsSpan().SequenceEqual(original)) throw new Exception("Completion leaked mutable score storage.");
    var node = raw.Session.Ownership.WordPublisher.Head;
    w.WriteStartObject(); w.WriteNumber("sourceNodes", done.Snapshot.SourceNodeCount2310);
    w.WriteNumber("total", raw.Terminal.Winner.TotalCost); w.WriteNumber("reference", done.Reference78);
    w.WriteNumber("row14", raw.Terminal.Winner.RowCost14);
    w.WriteString("allocation", Convert.ToHexString(allocation));
    w.WriteString("scored", Convert.ToHexString(done.CopyScoredAlternativeStorage400()));
    w.WriteString("final", Convert.ToHexString(original));
    w.WriteString("leading", Convert.ToHexString(leading)); w.WriteString("trailing", Convert.ToHexString(trailing));
    w.WriteStartArray("nativeHeadOrderWords");
    foreach (var projection in done.RawWordsInNativeHeadOrder)
    {
        if (node is null || !ReferenceEquals(node, projection.Publication.PublishedWord))
            throw new Exception("Raw word projection lost native279.Head/Next order.");
        w.WriteStartObject(); w.WriteBase64String("rawUtf8", projection.CopyUtf8());
        w.WriteString("word120", Convert.ToHexString(node.Word.CopyStorage()));
        w.WriteStartArray("glyphs");
        foreach (var glyph in projection.Glyphs)
        {
            w.WriteStartObject(); w.WriteNumber("reference", glyph.Reference); w.WriteNumber("source", glyph.CanonicalSourceKey);
            w.WriteBoolean("protocol", glyph.IsProtocolSource);
            if (glyph.OriginalGraphSourceKey is { } key) w.WriteNumber("original", key); else w.WriteNull("original");
            w.WriteString("record", Convert.ToHexString(glyph.CopyRecord40())); w.WriteBase64String("rawUtf8", glyph.CopyUtf8());
            w.WriteStartArray("associatedRows"); foreach (int index in glyph.CopyAssociatedRowOrdinals()) w.WriteNumberValue(index); w.WriteEndArray();
            w.WriteStartArray("ranges"); foreach (var range in glyph.CopySampledSourceRanges())
            { w.WriteStartArray(); w.WriteNumberValue(range.StrokeIndex); w.WriteNumberValue(range.StartPoint); w.WriteNumberValue(range.EndPointExclusive); w.WriteEndArray(); }
            w.WriteEndArray(); w.WriteEndObject(); glyphCount++;
        }
        w.WriteEndArray(); w.WriteEndObject(); node = node.Next; wordCount++;
    }
    if (node is not null) throw new Exception("Raw completion omitted an actual published WORD.");
    w.WriteEndArray(); w.WriteEndObject();
    RejectConsumed(() => LegacyFreshMainFirstCandidateCompletion.Complete(raw, allocation, leading, trailing));
}
w.WriteEndArray();

//Explicit PREPARED fault injection AFTER real inference: prove that a quote
//prefix is preserved when the later raw projection reaches an unsupported
//alternate alphabet. It does not establish positive NN quote recognition.
var fault = Run(samples[0]); var pool = fault.Session.Ownership.GlyphPool;
var word = fault.Session.Ownership.WordPublisher.Head!.Word;
var chain = word.FlatReferences.Reverse().ToArray();
if (chain.Length < 2) throw new Exception("Prepared fault needs two distinct actual glyph references.");
byte[] first = pool.CopyRecord(chain[0]), last = pool.CopyRecord(chain[^1]);
BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(4), 0);
BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(8), 193);
BinaryPrimitives.WriteUInt16LittleEndian(last.AsSpan(4), 4);
BinaryPrimitives.WriteUInt16LittleEndian(last.AsSpan(8), 0);
pool.ReplaceRecord(chain[0], first); pool.ReplaceRecord(chain[^1], last);
try
{
    LegacyFreshMainFirstCandidateCompletion.Complete(fault, new byte[400], new byte[4], new byte[4]);
    throw new Exception("Unowned alternate-alphabet projection was accepted.");
}
catch (NotSupportedException error) when (error.Message.StartsWith("Actual alternate/control-alphabet ownership")) { guards++; }
byte[] after = pool.CopyRecord(chain[0]);
if (BinaryPrimitives.ReadUInt16LittleEndian(after.AsSpan(8)) != 134)
    throw new Exception("Failed completion rolled back or skipped the genuine quote-kernel prefix.");
for (int at = 0; at < 40; at++)
    if (at is not (8 or 9) && first[at] != after[at]) throw new Exception("Quote changed an unrelated glyph byte.");
RejectConsumed(() => LegacyFreshMainFirstCandidateCompletion.Complete(fault, new byte[400], new byte[4], new byte[4]));
w.WriteStartObject("preparedPostInferenceFault"); w.WriteString("before", Convert.ToHexString(first));
w.WriteString("after", Convert.ToHexString(after)); w.WriteBoolean("retryRefused", true); w.WriteEndObject();
w.WriteNumber("words", wordCount); w.WriteNumber("glyphs", glyphCount); w.WriteNumber("guards", guards); w.WriteEndObject(); w.Flush();
if (guards != 38 || wordCount < 5) throw new Exception("Completion guard or actual output scope changed.");
Console.WriteLine($"PASS5 actual first-candidate completions/{wordCount} separate rawWORD UTF8/{glyphCount} glyphs/38 guards; prepared quote-prefix failure retained, no accepted-text claim.");

LegacyFreshMainSingleLinePlainRecognitionResult Run(InkStroke[] raw)
    => LegacyFreshMainSingleLinePlainRecognition.Prepare(ak, lk, engine, spatial, physical, raw, 0, table, cases, publication)
        .Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Malformed, canceled or previously consumed completion was accepted.");
}
void RejectConsumed(Action action)
{
    try { action(); }
    catch (InvalidOperationException error) when (error.Message.StartsWith("This actual first candidate has already entered completion"))
    { guards++; return; }
    throw new Exception("The exact per-result completion claim did not reject the retry.");
}
InkStroke[] Word(float x) => [Draw((x, 80), (x, 0), (x+30, 45), (x+60, 0), (x+60, 80)),
    Draw((x+85, 80), (x+85, 0), (x+135, 0), (x+135, 80)),
    Draw((x+165, 80), (x+165, 0), (x+215, 0), (x+215, 40), (x+165, 40))];
InkStroke Draw(params (float X, float Y)[] knots)
{
    var points = new List<InkPoint>();
    for (int i = 1; i < knots.Length; i++) for (int t = 0; t < 8; t++)
    {
        float f = t/8f;
        points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f, knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));
    }
    points.Add(new(knots[^1].X, knots[^1].Y)); return new(points.ToArray());
}
