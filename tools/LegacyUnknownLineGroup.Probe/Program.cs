using InkRecognition;
using System.Text.Json;

if (args.Length is not (1 or 2 or 5 or 7)) throw new ArgumentException("Supply controls; optionally acoustic resource; or acoustic/lexical/engine/separation profiles and optional live30/50 line models.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Oversized controls.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[]? acoustic = args.Length > 1 ? File.ReadAllBytes(args[1]) : null;
LegacyRussianModelBundle? model = args.Length >= 5 ? LegacyRussianModelBundle.Load(acoustic!, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3])) : null;
PreparedLineBoundaryClassifier[]? lineModels = args.Length == 7 ? args.Skip(5).Select(path => { using var stream=File.OpenRead(path);return PreparedLineBoundaryClassifier.Load(stream); }).ToArray() : null;
LegacyCursiveFinalCacheGroupScorer? cache = null;
if (model is not null)
{
    var nativeCache = new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic!.AsSpan(189734)), model.AcousticAlphabet, acoustic.AsSpan(99,16));
    var candidate = LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _), model.AcousticAlphabet);
    cache = new(nativeCache,candidate,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
        model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
}
int count = 0, groupCount = 0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var records = item.GetProperty("statistics").EnumerateArray().Select(x => new LegacyLineStrokeStatistics(x.EnumerateArray().Select(v => v.GetSingle()).ToArray())).ToArray();
    var pairs = item.GetProperty("pairs").EnumerateArray().Select(x => new LegacyNeuralLinePair(x[0].GetInt32(), x[1].GetInt32(), x[2].GetSingle(), x[3].GetBoolean())).ToArray();
    var result = LegacyUnknownLineGroupSelector.Select(records, pairs, item.GetProperty("scale").GetSingle());
    var expected = item.GetProperty("groups").EnumerateArray().ToArray();
    Require(result.Groups.Count == expected.Length, $"Group count {count}.");
    for (int g = 0; g < expected.Length; g++) Require(result.Groups[g].Span.SequenceEqual(expected[g].EnumerateArray().Select(x => x.GetInt32()).ToArray()), $"Group IDs {count}:{g}.");
    Require(result.StrokeGroupIndices.Span.SequenceEqual(item.GetProperty("membership").EnumerateArray().Select(x => x.GetInt32()).ToArray()), "Group membership.");
    if (item.TryGetProperty("strokes", out var source) && acoustic is not null)
    {
        var strokes = source.EnumerateArray().Select(s => s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray()).ToArray();
        if (lineModels is not null)
        {
            var liveRecords=strokes.Select(s=>LegacyLineStrokeStatistics.Extract(s)).ToArray();
            float explicitScale=item.GetProperty("scale").GetSingle();
            var liveProposals=LegacyNeuralLineProposals.Prepare(liveRecords,Enumerable.Repeat(true,strokes.Length).ToArray(),explicitScale*5f,.75f);
            var livePairs=LegacyNeuralLineProposals.Score(liveRecords,liveProposals,explicitScale,lineModels[0],lineModels[1]);
            result=LegacyUnknownLineGroupSelector.Select(liveRecords,livePairs,explicitScale);
            Require(result.Groups.Count==item.GetProperty("expectedRows").GetInt32() && result.StrokeGroupIndices.Span.IndexOf(-1)<0,"Live native NN/group chain lost an own printed row/stroke.");
        }
        // All three actual RU scalar nodes stay explicit; selecting a native
        // source219 callback profile is not replaced by a single guessed one.
        foreach (int offset in new[] { 5789, 11421, 187976 })
        {
            var profile = LegacyAutomaticLineProfile.ReadResourceNode(acoustic, offset);
            // Source15b260 sorts the returned physical IDs before its geometry callback.
            var physicalRows = result.Groups.Select(ids => ids.ToArray().Order().Select(i => strokes[i]).ToArray()).ToArray();
            var frames = physicalRows.Select(row => LegacyAutomaticLineEstimator.Estimate(row, profile)).ToArray();
            Require(frames.Length == item.GetProperty("expectedRows").GetInt32(), "Actual diagnostic row count.");
            Console.WriteLine($"Own MIR native NN/group chain profile@{offset}: "+string.Join("; ", frames.Select(f=>$"baseline={f.Line.Baseline:F3},height={f.Line.Height:F3},slope={f.Slope:F5}")));
            if (model is not null)
                for (int row = 0; row < frames.Length; row++)
                {
                    // Existing recovered24a postcallback deskew, not bbox placement.
                    var transformed = LegacyLineDeskew.Apply(physicalRows[row], frames[row].Line, (float)frames[row].Slope);
                    var ink = transformed.Strokes.Select(s => new InkStroke(s)).ToArray();
                    var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
                    var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379);
                    var options = new LegacyCursiveWordSearchOptions(transformed.Line.Baseline,transformed.Line.Height,float.NaN,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
                    var recognition = new LegacyNativeGraphWordRecognizer(model).Recognize(ink,sourceProfile,seedProfile,new byte[ink.Length],Enumerable.Repeat(-1,ink.Length).ToArray(),options,cache!,
                        (position,arc,glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty),false,false,false);
                    Require(recognition.ClassifiedGroups>0 && recognition.Words.Count>0,"Inferred frame did not reach real glyph/cache/dictionary pipeline.");
                    int rank = recognition.Words.Select(w=>w.Text).ToList().IndexOf("мир")+1;
                    Require(rank==1,"Own printed MIR inferred-frame regression; this is one diagnostic fixture, not a corpus claim.");
                    Console.WriteLine($"Own MIR inferred frame→native group NN/cache/lexicon profile@{offset},row{row}: {recognition.ClassifiedGroups} groups; expected rank={rank}; top="+
                        string.Join(", ",recognition.Words.Take(5).Select(w=>$"{w.Text}:{w.Match.Cost}")));
                }
        }
    }
    count++; groupCount += result.Groups.Count;
}
float[] fields = new float[17]; fields[2] = 10; fields[3] = 10; fields[6] = 20;
var record = new LegacyLineStrokeStatistics(fields);
var isolated = LegacyUnknownLineGroupSelector.Select([record, record, record], [new(0, 1, .999f, false)], 10);
Require(isolated.StrokeGroupIndices.Span.SequenceEqual(new[] { 0, 0, -1 }), "Unassigned strokes preserved; no invented singleton pass.");
var flat = Enumerable.Range(0,7).Select(i=>LegacyLineStrokeStatistics.Extract(new InkPoint[]{new(0,i is 2 or 4?100:0),new(100,i is 2 or 4?100:0)})).ToArray();
var interleaved=LegacyUnknownLineGroupSelector.Select(flat,[new(0,6,.9999f,false),new(2,4,.9998f,false),new(0,2,.9997f,false)],200);
Require(interleaved.Groups.Count==1 && interleaved.Groups[0].Span.SequenceEqual(new[]{0,6,2,4}),"Native lower5% rounds, not ceil: interleaved flat groups must pass temporal guard.");
Throws<ArgumentException>(()=>LegacyUnknownLineGroupSelector.Select([record, record], [new(0, 1, .8f, false),new(1,0,.9f,false)],10));
Throws<ArgumentException>(()=>LegacyUnknownLineGroupSelector.Select([record], [new(0,1,.8f,false)],10));
Throws<ArgumentException>(()=>LegacyUnknownLineGroupSelector.Select([record], [],0));
Throws<OperationCanceledException>(()=>LegacyUnknownLineGroupSelector.Select([record], [],10,new(true)));
Console.WriteLine($"PASS ordinary unknown-row native pair/group selection: {count} cases, {groupCount} groups.");
static void Require(bool condition,string text){if(!condition)throw new InvalidOperationException(text);}
static void Throws<T>(Action action) where T:Exception { try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
