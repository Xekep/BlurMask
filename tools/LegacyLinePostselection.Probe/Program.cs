using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 7))throw new ArgumentException("Supply independent postselection controls.");
if(new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Oversized controls.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,groups=0;
foreach(var item in doc.RootElement.EnumerateArray())
{
    var records=item.GetProperty("statistics").EnumerateArray().Select(s=>new LegacyLineStrokeStatistics(s.EnumerateArray().Select(v=>v.GetSingle()).ToArray())).ToArray();
    var input=item.GetProperty("input").EnumerateArray().Select(g=>new ReadOnlyMemory<int>(g.EnumerateArray().Select(v=>v.GetInt32()).ToArray())).ToArray();
    var result=LegacyUnknownLinePostselection.Apply(records,new(input,ReadOnlyMemory<int>.Empty),item.GetProperty("scale").GetSingle());
    var expected=item.GetProperty("groups").EnumerateArray().ToArray();
    Require(result.Groups.Count==expected.Length,$"Group count {count}.");
    for(int i=0;i<expected.Length;i++)Require(result.Groups[i].Span.SequenceEqual(expected[i].EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Group order/membership {count}:{i}.");
    Require(result.StrokeGroupIndices.Span.SequenceEqual(item.GetProperty("membership").EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Physical mapping {count}.");
    count++;groups+=expected.Length;
}
var r=LegacyLineStrokeStatistics.Extract(new InkPoint[]{new(0,0),new(10,10)});
Throws<ArgumentException>(()=>LegacyUnknownLinePostselection.Apply([r],new(new ReadOnlyMemory<int>[] {new int[]{0},new int[]{0}},ReadOnlyMemory<int>.Empty),10));
Throws<ArgumentException>(()=>LegacyUnknownLinePostselection.Apply([r],new(new ReadOnlyMemory<int>[] {new int[]{1}},ReadOnlyMemory<int>.Empty),10));
Throws<ArgumentException>(()=>LegacyUnknownLinePostselection.Apply([r],new(Array.Empty<ReadOnlyMemory<int>>(),ReadOnlyMemory<int>.Empty),float.NaN));
using(var cts=new CancellationTokenSource()){cts.Cancel();Throws<OperationCanceledException>(()=>LegacyUnknownLinePostselection.Apply([r],new(Array.Empty<ReadOnlyMemory<int>>(),ReadOnlyMemory<int>.Empty),10,cts.Token));}
Console.WriteLine($"PASS: {count} independent ordinary postselection cases / {groups} groups; malformed input and cancellation.");
if(args.Length==7)
{
byte[] acoustic=File.ReadAllBytes(args[1]);
using var m30=File.OpenRead(args[5]);using var m50=File.OpenRead(args[6]);
var line30=PreparedLineBoundaryClassifier.Load(m30);var line50=PreparedLineBoundaryClassifier.Load(m50);
var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var nativeCache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var candidate=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var cache=new LegacyCursiveFinalCacheGroupScorer(nativeCache,candidate,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
InkStroke Stroke(params (float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var printed=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
foreach(var scenario in new[]{(Rows:1,Reverse:false),(Rows:2,Reverse:false),(Rows:2,Reverse:true)})
{
    int rows=scenario.Rows;
    var rowOrder=Enumerable.Range(0,rows);if(scenario.Reverse)rowOrder=rowOrder.Reverse();
    var raw=rowOrder.SelectMany(row=>printed.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+200*row)).ToArray()))).ToArray();
    var scaleSource=LegacyOrdinaryLineScaleSource.Prepare(raw,acoustic,96);Require(scaleSource.PrimaryScale==80,"Own source219 scale regression.");
    foreach(int offset in new[]{5789,11421,187976})
    {
        var frames=LegacyPostselected2e3LineFrames.Infer(scaleSource,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,offset),5,.75f);
        Require(frames.Lines.Count==rows && frames.StrokeLineIndices.Span.IndexOf(-1)<0,"Inferred ordinary fixture lost rows/strokes.");
        Require(frames.Lines.Zip(frames.Lines.Skip(1)).All(p=>p.First.Deskewed.Line.Baseline<p.Second.Deskewed.Line.Baseline),"Native mean-Y materialization must preserve top-down rows for translated fixtures.");
        foreach(var frame in frames.Lines)
        {
            var ink=frame.Deskewed.Strokes.Select(s=>new InkStroke(s)).ToArray();
            var options=new LegacyCursiveWordSearchOptions(frame.Deskewed.Line.Baseline,frame.Deskewed.Line.Height,float.NaN,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
            var words=new LegacyNativeGraphWordRecognizer(model).Recognize(ink,scaleSource.Source.Profile,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379),new byte[ink.Length],Enumerable.Repeat(-1,ink.Length).ToArray(),options,cache,
                (position,arc,glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty),false,false,false);
            Require(words.Words.Count>0 && words.Words[0].Text=="мир" && words.Words[0].Match.Cost==54456,"Own raw→scale→NN→groups→Gaussian→glyph/cache/word regression.");
        }
        Console.WriteLine($"PASS actual raw own MIR rows={rows}, reverse={scenario.Reverse}, source scale={scaleSource.PrimaryScale}, profile@{offset}, best=мир/54456; no supplied baseline/height/scale.");
    }
}
// Exact upstream singleton branch, essential before a joined physical word can
// have geometry. A single physical stroke does not have adjacent pair frames.
var single=LegacyPostselected2e3LineFrames.Infer(LegacyOrdinaryLineScaleSource.Prepare([printed[0]],acoustic,96),line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f);
Require(single.Lines.Count==1 && single.Lines[0].PhysicalStrokeIndices.Span.SequenceEqual(new[]{0}) && single.PairEvidence.Count==0,"Native07ca80 singleton was lost without NN pairs.");
Throws<OperationCanceledException>(()=>LegacyPostselected2e3LineFrames.Infer(LegacyOrdinaryLineScaleSource.Prepare([printed[0]],acoustic,96),line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f,new(true)));
Console.WriteLine("PASS actual ordinary source/profile→2e3 NN/group/frame→Russian model path and single physical stroke branch; own fixture only, not cursive accuracy.");

}
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}


