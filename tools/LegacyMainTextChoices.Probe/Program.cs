using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 11) throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]), engine = File.ReadAllBytes(args[2]);
var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf = File.OpenRead(args[4]); using var pf = File.OpenRead(args[5]); using var nf = File.OpenRead(args[6]);
var cases = LegacyNativeCaseData.Load(cf); var publication = LegacyNativeTextPublicationData.Load(pf);
var normalization = LegacyNativeUnicodeNormalizationData.Load(nf);
using var cir = File.OpenRead(args[7]); var circles = LegacyNativeCircledPayloadData.Load(cir);
byte[] l30 = File.ReadAllBytes(args[8]), l50 = File.ReadAllBytes(args[9]);
var samples = new[] { Word(0), Word(0).Concat(Word(230)).ToArray(), Word(0).Concat(Word(300)).ToArray(), Word(0).Concat(Word(450)).ToArray(), Word(0).Concat(Word(700)).ToArray(), Word(450).Concat(Word(0)).ToArray(), Word(0).Select(s => new InkStroke(s.Points.Concat(Enumerable.Repeat(s.Points[^1], 3)).ToArray())).ToArray() };
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,l30,l50);
Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);Array.Fill(engine,(byte)0);Array.Fill(l30,(byte)0);Array.Fill(l50,(byte)0);
object? previousModel=null,previousDictionaryContexts=null;
int failedPublicationGuards = 0, foreignSourceGuards = 0;
LegacyOwnedMainGlyphSourceMetadataResult? previousSource = null;
LegacyOwnedMainSpanGlyphSourceResult? previousSpan = null;
int guards = 0, glyphCount = 0, alternatives = 0, selectedBytes = 0;
using var file = File.Create(args[10]); using var w = new Utf8JsonWriter(file);
w.WriteStartObject(); w.WriteStartArray("inputs");
for (int input = 0; input < samples.Length; input++)
{
    var mutable = samples[input].Select(s => new InkStroke(s.Points.ToList())).ToArray();
    var recognition = context.Prepare(mutable, 0, table, cases, publication);
    int originalCount = mutable[0].Points.Count;
    ((List<InkPoint>)mutable[0].Points).Clear();
    if (recognition.SourceDocument.Strokes[0].Points.Count != originalCount) throw new Exception("Original source document aliases caller storage.");
    guards++;
    var raw = recognition.Run(new byte[120], new byte[40], new ushort[256], new byte[64]);
    var currentModel=raw.Session.Plan.SourcePolicy.Model;var currentDictionaryContexts=raw.Session.Plan.SourcePolicy.DictionaryOwner.Contexts;
    if(previousModel is not null && (!ReferenceEquals(previousModel,currentModel)||ReferenceEquals(previousDictionaryContexts,currentDictionaryContexts)))
        throw new Exception("Reusable model or isolated dictionary contexts lost.");
    previousModel=currentModel;previousDictionaryContexts=currentDictionaryContexts;
    var completed = LegacyFreshMainFirstCandidateCompletion.Complete(raw, new byte[400], new byte[4], new byte[4]);
    byte[] residue = Enumerable.Range(0, 40).Select(n => (byte)(n * 13 + input)).ToArray();
    using var owner = context.PrepareTextChoices(completed, table, publication, normalization, circles);
    Reject(() => owner.Publish(new byte[39], new byte[2040 * 4]));
    try { owner.Publish(residue, new byte[2040 * 4], new CancellationToken(true)); throw new Exception("Cancellation ignored."); }
    catch (OperationCanceledException) { guards++; }
    var words = owner.Publish(residue, new byte[2040 * 4]);
    try { owner.Publish(residue, new byte[2040 * 4]); throw new Exception("Publication repeated."); }
    catch (InvalidOperationException e) when (e.Message.StartsWith("This text-choice publication has already started")) { guards++; }
    w.WriteStartObject(); w.WriteNumber("input", input);
    w.WriteStartArray("rawCounts"); foreach(var stroke in recognition.SourceDocument.Strokes) w.WriteNumberValue(stroke.Points.Count); w.WriteEndArray();
    w.WriteStartArray("words");
    int wordIndex = 0;
    foreach (var word in words)
    {
        if (!ReferenceEquals(word.Word, completed.RawWordsInNativeHeadOrder[wordIndex].Publication.PublishedWord))
            throw new Exception("WORD head/next order changed.");
        using var text = new MemoryStream();
        w.WriteStartObject(); w.WriteString("raw", Convert.ToHexString(completed.RawWordsInNativeHeadOrder[wordIndex].CopyUtf8()));
        w.WriteStartArray("glyphs");
        foreach (var glyph in word.Glyphs)
        {
            glyphCount++; alternatives += glyph.Published.Records.Count;
            byte[] selected = glyph.CopySelectedUtf8() ?? throw new Exception("Actual selected glyph became unavailable.");
            selectedBytes += selected.Length; text.Write(selected);
            var selection = glyph.Selection;
            w.WriteStartObject(); w.WriteNumber("reference", glyph.SelectedReference);
            w.WriteString("owner", Convert.ToHexString(selection.CopyOwnerStorage40()));
            w.WriteString("beforeSelections", Convert.ToHexString(selection.CopySelectionRows4()));
            w.WriteString("retainedSelections", Convert.ToHexString(glyph.Published.RetainedSelectionRows.Span));
            w.WriteNumber("beforeMarker", selection.SelectedMarker!.Value); w.WriteNumber("afterMarker", glyph.Published.AdjustedMarkerIndex);
            w.WriteString("selected", Convert.ToHexString(selected)); w.WriteStartArray("published");
            foreach (var record in glyph.Published.Records)
            {
                w.WriteStartObject(); w.WriteNumber("flags", record.Flags); w.WriteNumber("count", record.ByteCount);
                w.WriteNumber("intern", record.Text.Identity); w.WriteString("utf8", Convert.ToHexString(record.Text.ReadBytes())); w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteString("source24", Convert.ToHexString(glyph.OrdinaryRefreshed!.RefreshedOwner.Source.CopyReferenceStorage24()));
            w.WriteString("residual2", Convert.ToHexString(glyph.OrdinaryRefreshed!.RefreshedOwner.Source.CopyResidualStorage2()));
            w.WriteStartArray("positions"); foreach(var range in glyph.SourcePositions.Ranges)
            {w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();} w.WriteEndArray();
            w.WriteEndObject();
        }
        w.WriteEndArray(); w.WriteString("selected", Convert.ToHexString(text.ToArray())); w.WriteEndObject();
        if (!text.ToArray().AsSpan().SequenceEqual(completed.RawWordsInNativeHeadOrder[wordIndex].CopyUtf8()))
            throw new Exception("These ordinary fixture selected bytes changed unexpectedly.");
        wordIndex++;
    }
    w.WriteEndArray();
    w.WriteStartArray("spanOwners");
    foreach(var node in raw.Session.Ownership.GlyphPool.CopyOwnerNodes())
    {
        if(unchecked((short)(node.Key>>24))>=0) continue;
        var span = owner.SpanRefresh.RefreshOwner(node.Key);
        var projected = recognition.SourceDocument.Project(raw, span.Source);
        if(projected.OrdinarySource is not null || !ReferenceEquals(projected.SpanSource,span.Source))
            throw new Exception("Span projection substituted an ordinary source owner.");
        w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteString("source24",Convert.ToHexString(span.Source.CopySourceStorage24()));
        w.WriteStartArray("positions");foreach(var range in projected.Ranges)
        {w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray(); w.WriteEndObject();
    if(previousSource is not null)
    {
        try { recognition.SourceDocument.Project(raw,previousSource); throw new Exception("Foreign ordinary source accepted."); }
        catch(ArgumentException) { foreignSourceGuards++; }
        try { recognition.SourceDocument.Project(raw,previousSpan!); throw new Exception("Foreign span source accepted."); }
        catch(ArgumentException) { foreignSourceGuards++; }
    }
    previousSource = words[0].Glyphs[0].OrdinaryRefreshed!.RefreshedOwner.Source;
    previousSpan = owner.SpanRefresh.RefreshOwner(raw.Session.Ownership.GlyphPool.CopyOwnerNodes().First(n=>unchecked((short)(n.Key>>24))<0).Key).Source;
    using(var failed = context.PrepareTextChoices(completed,table,publication,normalization,circles))
    {
        try { failed.Publish(residue,[]); throw new Exception("Selection allocation failure ignored."); }
        catch(NotSupportedException e) when(e.Message.StartsWith("Actual refreshed glyph selection allocation refused")) { failedPublicationGuards++; }
        try { failed.Publish(residue,new byte[2040*4]); throw new Exception("Failed publication prefix retried."); }
        catch(InvalidOperationException e) when(e.Message.StartsWith("This text-choice publication has already started")) { failedPublicationGuards++; }
    }
    var retainedString = words[0].Glyphs[0].Published.Records[0].Text;
    owner.Dispose();
    try { retainedString.ReadBytes(); throw new Exception("Disposed string owner was still readable."); }
    catch (ObjectDisposedException) { guards++; }
}
w.WriteEndArray(); WritePositionControls(w); w.WriteNumber("glyphs", glyphCount); w.WriteNumber("alternatives", alternatives); w.WriteNumber("selectedBytes", selectedBytes); w.WriteNumber("guards", guards);w.WriteNumber("failedPublicationGuards",failedPublicationGuards);w.WriteNumber("foreignSourceGuards",foreignSourceGuards); w.WriteEndObject(); w.Flush();
if (glyphCount != 26 || alternatives != 64 || selectedBytes != 52 || guards != 35 || failedPublicationGuards != 14 || foreignSourceGuards != 12) throw new Exception("Actual text-choice coverage changed.");
Console.WriteLine($"PASS7 actual completed candidates/{glyphCount} glyphs/{alternatives} classified alternatives/{selectedBytes} selected UTF8 bytes/{guards} guards; document source acceptance remains separate.");
void Reject(Action action) { try { action(); } catch (ArgumentException) { guards++; return; } throw new Exception("Malformed allocation accepted."); }
InkStroke[] Word(float x) => [Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)), Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)), Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params (float X,float Y)[] knots)
{
    var points = new List<InkPoint>(); for(int i=1;i<knots.Length;i++) for(int t=0;t<8;t++)
    {float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}
    points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());
}

void WritePositionControls(Utf8JsonWriter w)
{
    w.WriteStartArray("positionControls");
    int controls = 0;
    foreach(int count in new[]{2,3,5,13,32})
    foreach(float firstFraction in new[]{0f,.125f,.995f,.999f})
    foreach(float lastFraction in new[]{0f,.25f,.995f,.999f})
    {
        int end = count-2;
        if(end == 0 && firstFraction > lastFraction) continue;
        var rows = new[]{new LegacyNativeSelectedInkRange(7,count,0,end,firstFraction,lastFraction,true,true),
            new LegacyNativeSelectedInkRange(11,count,0,-1,0,0,true,true)};
        var map = new LegacyNativeTextSourceInputMap(rows,[],0);
        var original = new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(7,firstFraction,3,count),
            LegacyNativeInkPosition.Pack(11,count-1,5,count));
        foreach(bool ordinary in new[]{true,false})
        foreach(int shape in Enumerable.Range(0,4))
        {
            LegacyNativeTextSourceReference[] references = shape switch
            {
                0 => [new(new(0,0,0),new(0,0,0))],
                1 => [new(new(0,0,0),new(0,1,count-1))],
                2 => [new(new(0,0,end),new(0,0,end)),new(new(0,1,0),new(0,1,0))],
                _ => [new(new(0,1,1),new(0,1,count-1),1,0)]
            };
            var positions = LegacyNativeTextSourcePositionPublication.Project(map,references,ordinary,[original]);
            w.WriteStartObject();w.WriteBoolean("ordinary",ordinary);
            w.WriteStartArray("rows");foreach(var row in rows)
            {w.WriteStartArray();w.WriteNumberValue(row.SourceIndex);w.WriteNumberValue(row.PointCount);w.WriteNumberValue(row.FirstPoint);w.WriteNumberValue(row.LastPoint);
             w.WriteNumberValue(row.FirstFraction);w.WriteNumberValue(row.LastFraction);w.WriteEndArray();}w.WriteEndArray();
            w.WriteStartArray("original");w.WriteNumberValue(original.First.Value);w.WriteNumberValue(original.Last.Value);w.WriteEndArray();
            w.WriteStartArray("references");foreach(var r in references)
            {w.WriteStartArray();w.WriteNumberValue(r.First.InputOrdinal);w.WriteNumberValue(r.First.PointOrdinal);w.WriteNumberValue(r.Last.InputOrdinal);w.WriteNumberValue(r.Last.PointOrdinal);
             w.WriteNumberValue(r.FirstResidual);w.WriteNumberValue(r.LastResidual);w.WriteEndArray();}w.WriteEndArray();
            w.WriteStartArray("positions");foreach(var range in positions)
            {w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();controls++;
        }
    }
    w.WriteEndArray();if(controls < 300)throw new Exception("Source publication branch coverage absent.");
}
