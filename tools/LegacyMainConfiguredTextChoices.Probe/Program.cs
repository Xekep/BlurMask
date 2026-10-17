using InkRecognition;
using System.Text.Json;

if (args.Length != 11) throw new ArgumentException("AK LK engine separation case publication line30 line50 normalization circles capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
byte[] l30 = File.ReadAllBytes(args[6]), l50 = File.ReadAllBytes(args[7]);
using var nf = File.OpenRead(args[8]); using var cir = File.OpenRead(args[9]);
var normalization = LegacyNativeUnicodeNormalizationData.Load(nf); var circles = LegacyNativeCircledPayloadData.Load(cir);
InkStroke[][] inputs = [[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],
    Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
var residue = new LegacyMainConfiguredAlternativeResidue(new byte[120], new byte[72], new byte[40], new ushort[256], new byte[64], new byte[4], new byte[4], new byte[4], new byte[4]);
int guards = 0, glyphs = 0, records = 0, selectedBytes = 0;
using var file = File.Create(args[10]); using var w = new Utf8JsonWriter(file);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int input = 0; input < inputs.Length; input++)
{
    var raw = LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak, lk, engine, l30, l50, inputs[input], 0, table, cases, publication)
        .Run(residue, r => new byte[r.RequiredBytes], new byte[20000 * 40]);
    var alternatives = raw.Alternatives.CopyActiveAlternatives();
    for (int a = 0; a < alternatives.Length; a++)
    {
        var view = LegacyOwnedMainCandidateSource.FromConfigured(raw, alternatives[a]);
        using var owner = LegacyFreshMainTextGlyphChoices.PrepareConfigured(ak, view, table, publication, normalization, circles);
        Reject(() => _ = owner.Candidate);
        Reject(() => owner.Publish(new byte[39], new byte[2040 * 4]));
        Reject(() => owner.Publish(new byte[40], new byte[2040 * 4], new(true)));
        var words = owner.Publish(new byte[40], new byte[2040 * 4]);
        Reject(() => owner.Publish(new byte[40], new byte[2040 * 4]));
        w.WriteStartObject(); w.WriteNumber("input", input); w.WriteNumber("alternative", a);
        w.WriteStartArray("rawCounts"); foreach (var stroke in view.SourceDocument.Strokes) w.WriteNumberValue(stroke.Points.Count); w.WriteEndArray();
        w.WriteStartArray("words"); int wi = 0;
        foreach (var word in words)
        {
            var actual = view.CopyRawWordsInNativeHeadOrder()[wi++];
            if (!ReferenceEquals(word.Word, actual.Publication.PublishedWord)) throw new Exception("Configured WORD list changed native head order.");
            w.WriteStartObject(); w.WriteString("raw", Convert.ToHexString(actual.CopyUtf8())); w.WriteStartArray("glyphs");
            using var selectedWord = new MemoryStream();
            foreach (var glyph in word.Glyphs)
            {
                if (!ReferenceEquals(glyph.SourcePositions.SelectedSession, view.Session)) throw new Exception("Projection lost configured selected session.");
                var selected = glyph.CopySelectedUtf8();
                if (selected is not null) { selectedWord.Write(selected); selectedBytes += selected.Length; }
                w.WriteStartObject(); w.WriteNumber("reference", glyph.SelectedReference);
                w.WriteString("owner", Convert.ToHexString(glyph.Selection.CopyOwnerStorage40()));
                w.WriteString("beforeSelections", Convert.ToHexString(glyph.Selection.CopySelectionRows4()));
                w.WriteString("retainedSelections", Convert.ToHexString(glyph.Published.RetainedSelectionRows.Span));
                w.WriteNumber("beforeMarker", glyph.Selection.SelectedMarker!.Value); w.WriteNumber("afterMarker", glyph.Published.AdjustedMarkerIndex);
                if (selected is null) w.WriteNull("selected"); else w.WriteString("selected", Convert.ToHexString(selected));
                w.WriteStartArray("published"); foreach (var record in glyph.Published.Records)
                { w.WriteStartObject(); w.WriteNumber("flags", record.Flags); w.WriteNumber("count", record.ByteCount); w.WriteNumber("intern", record.Text.Identity); w.WriteString("utf8", Convert.ToHexString(record.Text.ReadBytes())); w.WriteEndObject(); records++; }
                w.WriteEndArray();
                if (glyph.OrdinaryRefreshed is { } ordinary)
                { w.WriteBoolean("ordinary", true); w.WriteString("source24", Convert.ToHexString(ordinary.RefreshedOwner.Source.CopyReferenceStorage24())); w.WriteString("residual2", Convert.ToHexString(ordinary.RefreshedOwner.Source.CopyResidualStorage2())); }
                else { w.WriteBoolean("ordinary", false); w.WriteString("source24", Convert.ToHexString(glyph.SpanRefreshed!.Source.CopySourceStorage24())); }
                w.WriteStartArray("positions"); foreach (var range in glyph.SourcePositions.Ranges)
                { w.WriteStartArray(); w.WriteNumberValue(range.First.Value); w.WriteNumberValue(range.Last.Value); w.WriteEndArray(); }
                w.WriteEndArray(); w.WriteEndObject(); glyphs++;
            }
            w.WriteEndArray(); w.WriteString("selected", Convert.ToHexString(selectedWord.ToArray())); w.WriteEndObject();
        }
        w.WriteEndArray(); w.WriteEndObject();
        using (var failed = LegacyFreshMainTextGlyphChoices.PrepareConfigured(ak, view, table, publication, normalization, circles))
        { Reject(() => failed.Publish(new byte[40], [])); Reject(() => failed.Publish(new byte[40], new byte[2040 * 4])); }
        var text = words[0].Glyphs[0].Published.Records[0].Text; owner.Dispose(); Reject(() => text.ReadBytes());
    }
}
w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteNumber("glyphs", glyphs); w.WriteNumber("records", records); w.WriteNumber("selectedBytes", selectedBytes); w.WriteEndObject(); w.Flush();
Console.WriteLine($"PASS {inputs.Length} actual configured sources/six selected candidates/{glyphs} glyphs/{records} published choices/{selectedBytes} raw selected bytes/{guards} guards; no accepted text or024F30 claim.");
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or NotSupportedException or InvalidOperationException or OperationCanceledException) { guards++; return; } throw new Exception("Malformed, cancelled, disposed or reused publication accepted."); }
InkStroke[] Word(float x) => [Draw((x, 80), (x, 0), (x + 30, 45), (x + 60, 0), (x + 60, 80)), Draw((x + 85, 80), (x + 85, 0), (x + 135, 0), (x + 135, 80)), Draw((x + 165, 80), (x + 165, 0), (x + 215, 0), (x + 215, 40), (x + 165, 40))];
InkStroke Draw(params (float X, float Y)[] knots) { var points = new List<InkPoint>(); for (int i = 1; i < knots.Length; i++) for (int t = 0; t < 8; t++) { float f = t / 8f; points.Add(new(knots[i - 1].X + (knots[i].X - knots[i - 1].X) * f, knots[i - 1].Y + (knots[i].Y - knots[i - 1].Y) * f)); } points.Add(new(knots[^1].X, knots[^1].Y)); return new(points.ToArray()); }
