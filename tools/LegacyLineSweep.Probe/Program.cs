using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 7))throw new ArgumentException("Supply scalar stage controls, optionally six actual model paths.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0;
foreach(var item in doc.RootElement.GetProperty("stages").EnumerateArray())
{
    var records=item.GetProperty("statistics").EnumerateArray().Select(s=>new LegacyLineStrokeStatistics(s.EnumerateArray().Select(v=>v.GetSingle()).ToArray())).ToArray();
    var pairs=item.GetProperty("pairs").EnumerateArray().Select(p=>new LegacyNeuralLinePair(p[0].GetInt32(),p[1].GetInt32(),p[2].GetSingle(),p[3].GetBoolean())).ToArray();
    var previous=new LegacyUnknownLineGroups(item.GetProperty("previous").EnumerateArray().Select(g=>new ReadOnlyMemory<int>(g.EnumerateArray().Select(i=>i.GetInt32()).ToArray())).ToArray(),ReadOnlyMemory<int>.Empty);
    var stage=LegacyUnknownLineSweepSelection.Select(records,pairs,item.GetProperty("scale").GetSingle(),previous,item.GetProperty("mask").EnumerateArray().Select(v=>v.GetBoolean()).ToArray(),item.GetProperty("final").GetBoolean());
    var result=stage.Selected;
    var expectedDeferred=item.GetProperty("deferred").EnumerateArray().Select(p=>new LegacyNeuralLinePair(p[0].GetInt32(),p[1].GetInt32(),p[2].GetSingle(),p[3].GetBoolean())).ToArray();Require(stage.DeferredPairs.SequenceEqual(expectedDeferred),$"Deferred evidence{count}.");
    var expected=item.GetProperty("groups").EnumerateArray().ToArray();Require(result.Groups.Count==expected.Length,"Final strip group count.");
    for(int i=0;i<expected.Length;i++)Require(result.Groups[i].Span.SequenceEqual(expected[i].EnumerateArray().Select(v=>v.GetInt32()).ToArray()),"Final strip physical order.");
    Require(result.StrokeGroupIndices.Span.SequenceEqual(item.GetProperty("membership").EnumerateArray().Select(v=>v.GetInt32()).ToArray()),"Final strip membership.");count++;
}
Console.WriteLine($"PASS independent continuation/deferred-cost-block stages {count}.");
var ownPaths=doc.RootElement.GetProperty("models").EnumerateArray().Select(v=>v.GetString()!).ToArray();
using var own30Stream=File.OpenRead(ownPaths[0]);using var own50Stream=File.OpenRead(ownPaths[1]);
var own30=PreparedLineBoundaryClassifier.Load(own30Stream);var own50=PreparedLineBoundaryClassifier.Load(own50Stream);
int wholeCount=0;
foreach(var item in doc.RootElement.GetProperty("full").EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(st=>new InkStroke(st.EnumerateArray().Select(v=>new InkPoint(v[0].GetSingle(),v[1].GetSingle())).ToArray())).ToArray();
    var owned=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0));
    var source=LegacyOrdinaryLineScaleSource.Extract(owned,new(.96f,.85f,3,10),96);
    var result=LegacyOrdinaryNeuralLineInferenceFactory.Infer(source,96,item.GetProperty("half").EnumerateArray().Select(v=>v.GetBoolean()).ToArray(),
        item.GetProperty("quarter").GetBoolean(),item.GetProperty("quantile").GetSingle(),own30,own50,new(0,0,true,-.2f,.2f,3,6,0),5,.75f);
    var expected=item.GetProperty("expected");
    Require(result.NeuralScale==expected.GetProperty("neuralScale").GetSingle(),$"Whole scale override{wholeCount}.");
    Require(result.VerticalStripCount==expected.GetProperty("strips").GetInt32(),$"Whole strip count{wholeCount}.");
    Require(result.TinyPhysicalStrokeIndices.Span.SequenceEqual(expected.GetProperty("tiny").EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Whole tiny metadata{wholeCount}.");
    Require(result.Frames.StrokeLineIndices.Span.SequenceEqual(expected.GetProperty("membership").EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Whole membership{wholeCount}.");
    var expectedGroups=expected.GetProperty("groups").EnumerateArray().ToArray();Require(result.Frames.Lines.Count==expectedGroups.Length,"Whole group count.");
    for(int g=0;g<expectedGroups.Length;g++)Require(result.Frames.Lines[g].PhysicalStrokeIndices.Span.SequenceEqual(expectedGroups[g].EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Whole group IDs{wholeCount}:{g}.");
    var pairs=expected.GetProperty("pairs").EnumerateArray().ToArray();Require(result.Frames.PairEvidence.Count==pairs.Length,"Whole actual synthetic NN pair count.");
    for(int i=0;i<pairs.Length;i++)
    {
        var actual=result.Frames.PairEvidence[i];var row=pairs[i];
        Require(actual.FirstStroke==row[0].GetInt32()&&actual.SecondStroke==row[1].GetInt32()&&actual.LabelOne==row[3].GetBoolean()&&MathF.Abs(actual.Confidence-row[2].GetSingle())<2e-5f,$"Independent NN sweep pair{wholeCount}:{i}.");
    }
    wholeCount++;
}
var invalidRecord=LegacyLineStrokeStatistics.Extract(new InkPoint[]{new(0,0),new(10,10)});
var emptyGroups=new LegacyUnknownLineGroups(Array.Empty<ReadOnlyMemory<int>>(),ReadOnlyMemory<int>.Empty);
Throws<ArgumentException>(()=>LegacyUnknownLineSweepSelection.Select([invalidRecord],[],20,emptyGroups,Array.Empty<bool>(),true));
Throws<ArgumentException>(()=>LegacyUnknownLineSweepSelection.Select([invalidRecord],[],20,new(new ReadOnlyMemory<int>[] {new int[]{0},new int[]{0}},ReadOnlyMemory<int>.Empty),new bool[1],true));
Throws<OperationCanceledException>(()=>LegacyUnknownLineSweepSelection.Select([invalidRecord],[],20,emptyGroups,new bool[1],true,new(true)));
var validationSource=LegacyOrdinaryLineScaleSource.Extract(LegacyOrdinarySourceInk.Prepare([new InkStroke(new InkPoint[]{new(0,0),new(10,10)})],new(0x135,false,0,0)),new(.96f,.85f,3,10),96);
Throws<ArgumentException>(()=>LegacyOrdinaryNeuralLineInferenceFactory.Infer(validationSource,float.NaN,new bool[1],false,.8f,own30,own50,new(0,0,true,-.2f,.2f,3,6,0),5,.75f));
Throws<ArgumentException>(()=>LegacyOrdinaryNeuralLineInferenceFactory.Infer(validationSource,96,new bool[1],false,2,own30,own50,new(0,0,true,-.2f,.2f,3,6,0),5,.75f));
Throws<OperationCanceledException>(()=>LegacyOrdinaryNeuralLineInferenceFactory.Infer(validationSource,96,new bool[1],false,.8f,own30,own50,new(0,0,true,-.2f,.2f,3,6,0),5,.75f,new(true)));
Console.WriteLine($"PASS {wholeCount} independent full raw/synthetic-NN scale/sweep/singleton/postselection controls.");
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
foreach(var scenario in new[]{(Rows:1,Reverse:false),(Rows:2,Reverse:false),(Rows:2,Reverse:true),(Rows:3,Reverse:true)})
{
    int rows=scenario.Rows;
    var rowOrder=Enumerable.Range(0,rows);if(scenario.Reverse)rowOrder=rowOrder.Reverse();
    var raw=rowOrder.SelectMany(row=>printed.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+(rows==3?8000:200)*row)).ToArray()))).ToArray();
    var scaleSource=LegacyOrdinaryLineScaleSource.Prepare(raw,acoustic,96);Require(scaleSource.PrimaryScale==80,"Own source219 scale regression.");
    foreach(int offset in new[]{5789,11421,187976})
    {
        var inference=LegacyOrdinaryNeuralLineInferenceFactory.Infer(scaleSource,96,new bool[raw.Length],false,.8f,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,offset),5,.75f);
        var frames=inference.Frames;Require(rows!=3 || inference.VerticalStripCount>1,"Far-separated rows did not enter multiple native strips.");
        Console.WriteLine($"Actual native NNscale={inference.NeuralScale}, vertical strips={inference.VerticalStripCount}.");
        Require(frames.Lines.Count==rows && frames.StrokeLineIndices.Span.IndexOf(-1)<0,"Inferred ordinary fixture lost rows/strokes.");
        Require(frames.Lines.Zip(frames.Lines.Skip(1)).All(p=>p.First.Deskewed.Line.Baseline<p.Second.Deskewed.Line.Baseline),"Native mean-Y materialization must preserve top-down rows for translated fixtures.");
        // Actual2B1/1111 features replaced the historical2B0 cost54456.
        // Mixed MAIN normalization matches LegacyOrdinaryLineScale.Probe;
        // identical scorer inputs give a final р edge of 19163 for the latter profiles.
        int expectedCost=offset switch{5789=>53251,11421 or 187976=>53266,_=>throw new InvalidOperationException("Unexpected line profile.")};
        foreach(var frame in frames.Lines)
        {
            var ink=frame.Deskewed.Strokes.Select(s=>new InkStroke(s)).ToArray();
            var options=new LegacyCursiveWordSearchOptions(frame.Deskewed.Line.Baseline,frame.Deskewed.Line.Height,float.NaN,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
            var words=new LegacyNativeGraphWordRecognizer(model).Recognize(ink,scaleSource.Source.Profile,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379),new byte[ink.Length],Enumerable.Repeat(-1,ink.Length).ToArray(),options,cache,
                (position,arc,glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty),false,false,false);
            Console.WriteLine($"Own sweep rows={rows}, baseline={frame.Deskewed.Line.Baseline}, top="+string.Join(", ",words.Words.Take(5).Select(w=>$"{w.Text}:{w.Match.Cost}")));
            Require(words.Words.Count>0 && words.Words[0].Text=="мир" && (rows==3 || words.Words[0].Match.Cost==expectedCost),"Own raw→scale→NN→groups→Gaussian→glyph/cache/word regression.");
            if(rows!=3)Require(words.Words[0].Match.AcousticCost==expectedCost-4537 && words.Words[0].Score.Language.LexicalSubtotal==4537,"Own acoustic/lexical path decomposition regression.");
        }
        Console.WriteLine($"PASS actual raw own MIR rows={rows}, reverse={scenario.Reverse}, source scale={scaleSource.PrimaryScale}, profile@{offset}, best=мир; no supplied baseline/height/scale.");
    }
}
// Exact upstream singleton branch, essential before a joined physical word can
// have geometry. A single physical stroke does not have adjacent pair frames.
var single=LegacyOrdinaryNeuralLineInferenceFactory.Infer(LegacyOrdinaryLineScaleSource.Prepare([printed[0]],acoustic,96),96,new bool[1],false,.8f,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f).Frames;
Require(single.Lines.Count==1 && single.Lines[0].PhysicalStrokeIndices.Span.SequenceEqual(new[]{0}) && single.PairEvidence.Count==0,"Native07ca80 singleton was lost without NN pairs.");
Throws<OperationCanceledException>(()=>LegacyOrdinaryNeuralLineInferenceFactory.Infer(LegacyOrdinaryLineScaleSource.Prepare([printed[0]],acoustic,96),96,new bool[1],false,.8f,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f,new(true)));
Console.WriteLine("PASS actual ordinary source/profile→2e3 NN/group/frame→Russian model path and single physical stroke branch; own fixture only, not cursive accuracy.");

}
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
