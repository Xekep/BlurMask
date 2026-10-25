using InkRecognition;
using System.Text.Json;

if(args.Length is not (1 or 2 or 7))throw new ArgumentException("controls [acoustic] or controls acoustic lexical engine separation line30 line50");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,observations=0;
foreach(var item in doc.RootElement.EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var p=item.GetProperty("profile");var profile=new LegacyLineScaleProfile(p[0].GetSingle(),p[1].GetSingle(),p[2].GetSingle(),p[3].GetSingle());
    var source=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0));
    var scaled=LegacyOrdinaryLineScaleSource.Extract(source,profile,item.GetProperty("resolution").GetSingle());
    Require(ReferenceEquals(scaled.Source,source),"Scale extraction replaced owned source chronology.");
    Require(scaled.Samples.Span.SequenceEqual(item.GetProperty("samples").EnumerateArray().Select(x=>x.GetSingle()).ToArray()),$"Source extrema samples {count}.");
    var expected=item.GetProperty("scales");Equal(scaled.Scales.Primary,expected[0].GetDouble());Equal(scaled.Scales.Secondary,expected[1].GetDouble());
    count++;observations+=scaled.Samples.Length;
}
var pointSource=LegacyOrdinarySourceInk.Prepare([new InkStroke(new InkPoint[]{new(1,1)})],new(0x135,false,0,0));
var ownProfile=new LegacyLineScaleProfile(.96f,.85f,3,10);
Require(LegacyOrdinaryLineScaleSource.Extract(pointSource,ownProfile,0).PrimaryScale==1,"Native no-sample/resolution fallback.");
Throws<ArgumentException>(()=>LegacyOrdinaryLineScaleSource.Extract(pointSource,ownProfile,float.NaN));
Throws<InvalidDataException>(()=>LegacyOrdinaryLineScaleSource.Extract(pointSource,ownProfile with{Quantile=2},96));
Throws<OperationCanceledException>(()=>LegacyOrdinaryLineScaleSource.Extract(pointSource,ownProfile,96,new(true)));
Console.WriteLine($"PASS ordinary ingress→source samples→source219 profile/scales: {count} cases, {observations} samples.");
if(args.Length==1)return;
byte[] acoustic=File.ReadAllBytes(args[1]);var nativeProfile=LegacyOrdinaryLineScaleSource.ReadV10dResourceProfile(acoustic);
Require(nativeProfile==ownProfile,"Checked RU resource source profile differs.");
var corrupt=acoustic.ToArray();int at=LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic).ParametersOffset;
System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(corrupt.AsSpan(at),float.NaN);
Throws<InvalidDataException>(()=>LegacyOrdinaryLineScaleSource.ReadV10dResourceProfile(corrupt));
Console.WriteLine($"PASS actual247/v10d scalar profile@{at}: {nativeProfile}.");
if(args.Length!=7)return;
using var m30=File.OpenRead(args[5]);using var m50=File.OpenRead(args[6]);
var line30=PreparedLineBoundaryClassifier.Load(m30);var line50=PreparedLineBoundaryClassifier.Load(m50);
var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var nativeCache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var candidate=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var cache=new LegacyCursiveFinalCacheGroupScorer(nativeCache,candidate,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
InkStroke Stroke(params (float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var printed=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
foreach(int rows in new[]{1,2})
{
    var raw=Enumerable.Range(0,rows).SelectMany(row=>printed.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+200*row)).ToArray()))).ToArray();
    var scaleSource=LegacyOrdinaryLineScaleSource.Prepare(raw,acoustic,96);Require(scaleSource.PrimaryScale==80,"Own source219 scale regression.");
    foreach(int offset in new[]{5789,11421,187976})
    {
        var frames=LegacyPrepared2e3LineFrames.Infer(scaleSource,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,offset),5,.75f);
        Require(frames.Lines.Count==rows && frames.StrokeLineIndices.Span.IndexOf(-1)<0,"Inferred ordinary fixture lost rows/strokes.");
        // Actual2B1/1111 features replaced the historical2B0 cost54456.
        // Mixed MAIN normalization excludes the standalone sigmoid channel.
        // For the latter profiles the final р edge is19163 rather than19178;
        // raw NN/Q15 replay and the returned lexical-choice sums verify this delta.
        int expectedCost=offset switch{5789=>53251,11421 or 187976=>53266,_=>throw new InvalidOperationException("Unexpected line profile.")};
        foreach(var frame in frames.Lines)
        {
            var ink=frame.Deskewed.Strokes.Select(s=>new InkStroke(s)).ToArray();
            var options=new LegacyCursiveWordSearchOptions(frame.Deskewed.Line.Baseline,frame.Deskewed.Line.Height,float.NaN,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
            var words=new LegacyNativeGraphWordRecognizer(model).Recognize(ink,scaleSource.Source.Profile,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379),new byte[ink.Length],Enumerable.Repeat(-1,ink.Length).ToArray(),options,cache,
                (position,arc,glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty),false,false,false);
            Require(words.Words.Count>0 && words.Words[0].Text=="мир" && words.Words[0].Match.Cost==expectedCost,"Own raw→scale→NN→groups→Gaussian→glyph/cache/word regression.");
            Require(words.Words[0].Match.AcousticCost==expectedCost-4537 && words.Words[0].Score.Language.LexicalSubtotal==4537,"Own acoustic/lexical path decomposition regression.");
        }
        Console.WriteLine($"PASS actual raw own MIR rows={rows}, source scale={scaleSource.PrimaryScale}, profile@{offset}, best=мир/{expectedCost}; no supplied baseline/height/scale.");
    }
}
// Exact upstream singleton branch, essential before a joined physical word can
// have geometry. A single physical stroke does not have adjacent pair frames.
var single=LegacyPrepared2e3LineFrames.Infer([printed[0]],acoustic,96,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f);
Require(single.Lines.Count==1 && single.Lines[0].PhysicalStrokeIndices.Span.SequenceEqual(new[]{0}) && single.PairEvidence.Count==0,"Native07ca80 singleton was lost without NN pairs.");
Throws<OperationCanceledException>(()=>LegacyPrepared2e3LineFrames.Infer([printed[0]],acoustic,96,line30,line50,LegacyAutomaticLineProfile.ReadResourceNode(acoustic,5789),5,.75f,new(true)));
Console.WriteLine("PASS actual ordinary source/profile→2e3 NN/group/frame→Russian model path and single physical stroke branch; own fixture only, not cursive accuracy.");
static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
static void Equal(double a,double b){if(Math.Abs(a-b)>1e-10*Math.Max(1,Math.Abs(b)))throw new InvalidOperationException($"Scale {a} vs {b}.");}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}.");}
