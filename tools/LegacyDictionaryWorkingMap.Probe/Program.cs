using System.Diagnostics;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 4)) throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0;
foreach (var item in controls.RootElement.EnumerateArray())
{
    ushort[] original = U16(item, "original"), aux = U16(item, "aux");
    var source = new LegacyPreparedDictionaryWorkingMap(item.GetProperty("count").GetInt32(), item.GetProperty("flags").GetUInt32(), original, aux);
    var refreshed = source.Refresh(Bytes(item, "permission"));
    Require(refreshed.CopySelectedMap(item.GetProperty("auxiliary").GetBoolean(), item.GetProperty("query").GetByte()).AsSpan().SequenceEqual(U16(item, "expected")) &&
        refreshed.CopySelectedMap(false, 0).AsSpan().SequenceEqual(U16(item, "working")), $"Working map control {cases} differs.");
    original.AsSpan().Fill(0); aux.AsSpan().Fill(0);
    Require(refreshed.CopySelectedMap(false, 0x40).AsSpan().SequenceEqual(U16(item, "original")), "Prepared maps must own snapshots.");
    // A later refresh is a rebuild from preserved58, not cumulative filtering.
    Require(refreshed.Refresh(Enumerable.Repeat((byte)5, source.AcousticSymbolCount).ToArray()).CopySelectedMap(false, 0).AsSpan().SequenceEqual(U16(item, "original")),
        "Refresh must restore previously excluded symbols from its preserved map.");
    cases++;
}

var lexicon = LegacyPackedLexicon.Load(OwnLexicon());
var mapping = new LegacyPreparedDictionaryWorkingMap(3, 0xc00, new ushort[] { 0, 1, 2, 0xfffb, 0xffff }, new ushort[] { 0, 1, 2, 0xfffb, 0xffff }).Refresh(new byte[] { 1, 0, 1 });
Require(mapping.GetTransitions(lexicon, 0, false, 0).AsSpan().SequenceEqual(new LegacyPreparedDictionaryTransition[]
    { new(1, 0, 0, 1), new(3, 2, 2, 1) }), "Working root edge suppression/order/flags differ.");
Require(mapping.GetTransitions(lexicon, 0, false, 0x40).AsSpan().SequenceEqual(new LegacyPreparedDictionaryTransition[]
    { new(1, 0, 0, 1), new(2, 1, 1, 3), new(3, 2, 2, 1) }), "Preserved-map override must restore excluded root transition.");
Require(mapping.GetTransitions(lexicon, 0, true, 0).AsSpan().SequenceEqual(new LegacyPreparedDictionaryTransition[]
    { new(2, 1, 1, 3) }), "Auxiliary edge enumeration must retain terminals only.");
Require(mapping.GetTransitions(lexicon, 1, false, 0).AsSpan().SequenceEqual(new LegacyPreparedDictionaryTransition[]
    { new(4, 0, 0, 2) }), "Nonroot transition/terminal flags differ.");
Require(mapping.MapAcousticCosts(new int[] { 100, 0, 200 }, false, 0).AsSpan().SequenceEqual(new LegacyIntegerLexicalCandidate[]
    { new(0, 100), new(2, 200) }), "Selected working map must exclude the best disallowed acoustic score.");
Reject(() => mapping.Refresh(new byte[2]));
Reject(() => mapping.GetTransitions(lexicon, -1, false, 0));
Reject(() => mapping.Refresh(new byte[3], new CancellationToken(true)));
Reject(() => mapping.MapAcousticCosts(new int[] { -1 }, false, 0));
var absentAux = new LegacyPreparedDictionaryWorkingMap(3, 0xc00, new ushort[] { 0 }, []);
try { absentAux.CopySelectedMap(true, 0); throw new InvalidDataException("Missing auxiliary map accepted."); }
catch (NotSupportedException) { }
Console.WriteLine($"PASS {cases} independent working-map controls; snapshot/rebuild, own actual packed backend transitions, preserved override, auxiliary terminal gate, cost adapter and guards.");

if (args.Length == 4)
{
    var bundle = LegacyRussianModelBundle.Load(File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
    var initial = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(bundle.AcousticAlphabet, bundle.LexicalAlphabet);
    // The actual source descriptor producer remains open.0xc00 is an explicit
    // ordinary test scope, not a descriptor inferred from this RU resource.
    var actualMap = new LegacyPreparedDictionaryWorkingMap(bundle.AcousticAlphabet.LabelCount, 0xc00, initial, []);
    byte[] allPermissions = Enumerable.Repeat((byte)1, bundle.AcousticAlphabet.LabelCount).ToArray();
    var all = actualMap.Refresh(allPermissions);
    var actualRoot = all.GetTransitions(bundle.Lexicon, 0, false, 0);
    Require(actualRoot.Length > 0 && actualRoot.All(t => t.AcousticSymbol != ushort.MaxValue), "Actual RU dictionary transition map is empty.");
    foreach (var transition in actualRoot)
        Require(bundle.Lexicon.TryAdvance(0, transition.LexicalLabel, out int next) && next == transition.NextState,
            "Actual transition identity differs from dictionary backend.");

    // This opt-in diagnostic excludes ALL Latin lowercase labels uniformly;
    // it does not rename/alias them or inspect expected fixture words.
    byte[] cyrillicScope = allPermissions.ToArray();
    for (char letter = 'a'; letter <= 'z'; letter++)
        if (bundle.AcousticAlphabet.TryGetLabel(letter.ToString(), out ushort id)) cyrillicScope[id] = 0;
    var restricted = actualMap.Refresh(cyrillicScope);
    Require(bundle.LexicalAlphabet.TryGetLabel("p", out ushort latin) && bundle.LexicalAlphabet.TryGetLabel("\u0440", out ushort cyrillic) &&
        restricted.CopySelectedMap(false, 0)[latin] == ushort.MaxValue && restricted.CopySelectedMap(false, 0)[cyrillic] != ushort.MaxValue,
        "Explicit script scope must suppress distinct Latin labels without replacing Cyrillic IDs.");
    var recognizer = new LegacyRussianCursiveWordRecognizer(bundle, relatedFormMode: null);
    InkStroke Stroke(params (float X, float Y)[] p) => new(p.Select(v => new InkPoint(v.X, v.Y)).ToArray());
    var printed = new InkStroke[]
    {
        Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),
        Stroke((70,10),(70,90),(120,10),(120,90)),
        Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
    };
    var joined = new InkStroke[] { Stroke((0,90),(10,20),(20,90),(30,20),(40,90),(50,90),(60,20),(70,90),(80,20),(90,90),(100,90),(110,20),(120,90),(130,20),(140,90)) };
    foreach (var (name, raw) in new[] { ("own printed fixture", printed), ("own joined zigzag", joined) })
    foreach (var (scope, working) in new[] { ("all exact labels", all), ("explicit Latin-lowercase exclusion", restricted) })
    {
        int evaluated = 0; var timer = Stopwatch.StartNew();
        var result = recognizer.RecognizeSourceOrderDiagnostic(raw,
            new LegacyCursiveWordSearchOptions(90, 80, 0, 1, BeamWidth:128, MaximumExpandedChoices:10_000_000),
            costAdapter:(group,glyph) => { evaluated++; return working.MapAcousticCosts(glyph.IntegerCosts.Span, false, 0); });
        timer.Stop();
        Require(evaluated > 0 && evaluated == result.ClassifiedGroups, "Actual classifier/cut pipeline did not evaluate groups.");
        foreach (var word in result.Words)
        {
            Require(bundle.WordIndex.TryGetWordId(word.Match.Labels.Span, out int id) && id == word.WordId,
                "Working-map inference produced a non-dictionary terminal.");
            foreach (ushort label in word.Match.Labels.Span)
                Require(working.CopySelectedMap(false, 0)[label] != ushort.MaxValue, "Excluded lexical edge escaped into actual inference.");
        }
        Console.WriteLine($"ACTUAL {name};{scope}: groups={evaluated},context={result.ContextWindowCount},words={result.Words.Count},ms={timer.Elapsed.TotalMilliseconds:F2};top={string.Join(",",result.Words.Take(5).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}. Explicit frame/probability/permission scope, original beam; no expected answer or accuracy claim.");
    }
    Console.WriteLine($"PASS actual RU exact map -> packed dictionary transitions ({actualRoot.Length} root edges) and real NN/context/cuts -> working lexical map -> terminal language scoring diagnostic.");
}

static ushort[] U16(JsonElement item, string key) => item.GetProperty(key).EnumerateArray().Select(x => x.GetUInt16()).ToArray();
static byte[] Bytes(JsonElement item, string key) => item.GetProperty(key).EnumerateArray().Select(x => x.GetByte()).ToArray();
static byte[] OwnLexicon()
{
    using var stream = new MemoryStream(); using var w = new BinaryWriter(stream);
    w.Write(0x0101fe90u); w.Write(0u); w.Write(0x2b4u); w.Write(0x01000000u); w.Write(0u);
    w.Write(7u); w.Write(2u);
    WritePacked(w, new uint[] { 0, 0, 5, 2, 4, 1, 5 }, 3); w.Write((byte)3);
    WritePacked(w, new uint[] { 3, 2, 0, 1, 0, 0, 0 }, 2); w.Write(3u);
    w.Flush(); var bytes = stream.ToArray(); BitConverter.GetBytes((uint)(bytes.Length - 8)).CopyTo(bytes, 4); return bytes;
}
static void WritePacked(BinaryWriter w, uint[] values, int width)
{
    w.Write(0x01000000u); w.Write((uint)values.Length); w.Write((byte)width);
    byte[] bytes = new byte[(values.Length * width + 31) / 32 * 4];
    for (int i = 0; i < values.Length; i++) for (int bit = 0; bit < width; bit++)
        if ((values[i] & 1u << bit) != 0) bytes[(i * width + bit) / 8] |= (byte)(1 << ((i * width + bit) % 8));
    w.Write(bytes);
}
static void Require(bool ok, string message) { if (!ok) throw new InvalidDataException(message); }
static void Reject(Action action)
{ try { action(); } catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled working map accepted."); }
