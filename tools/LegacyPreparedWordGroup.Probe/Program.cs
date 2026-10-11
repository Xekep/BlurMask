using InkRecognition;

if(args.Length!=3)throw new ArgumentException("Usage: acoustic.res lexical.res engine-profile.json");
var model=LegacyRussianModelBundle.Load(File.ReadAllBytes(args[0]),File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]));
var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(File.ReadAllBytes(args[0]));
InkStroke Stroke(params (float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var letter=Stroke((0,90),(0,10),(25,60),(50,10),(50,90));
var expected=new LegacyInkStroke(new LegacyInkPoint[]{new(0,0),new(0,-100),new(31,-37),new(62,-100),new(62,0)});
int neuralControls=0;
foreach(bool baselineFamilies in new[]{false,true})foreach(bool suppress in new[]{false,true})
foreach(var(scale,dx,dy)in new[]{(1f,0f,0f),(2f,-100f,300f),(.5f,100f,-30f),(4f,-200f,-100f)})
{
    var source=new InkStroke(letter.Points.SelectMany(p=>new[]{new InkPoint(p.X*scale+dx,p.Y*scale+dy),new InkPoint(p.X*scale+dx,p.Y*scale+dy)}).ToArray());
    var options=new LegacyCursiveWordSearchOptions(90*scale+dy,80*scale,float.NaN,1,
        IncludeBaselineFamilies:baselineFamilies,SuppressStartBoundary:suppress,SuppressEndBoundary:suppress);
    var bridge=LegacyOrdinaryCursiveCutSource.Generate([source],profile,options.SourceBaseline,options.SourceHeight,wholeStrokeOnly:true);
    var evaluator=new LegacyPreparedWordGroupEvaluator(model,bridge,options);
    Require(evaluator.ContextWindowCount==0,"One physical stroke must not manufacture prior windows.");
    var actual=evaluator.Classify([new(0,0,bridge.Cuts.PreparedInk.StrokePointCounts.Span[0])]);
    Require(actual.Geometry.Strokes.Single().Points.SequenceEqual(expected.Points),"Analytic normalized M fixture differs.");
    short boundary=suppress?(short)-1024:(short)1024;
    var independently=model.CreateGlyphClassifier().Classify([expected],new(0,100),[],boundary,boundary,baselineFamilies);
    Require(actual.Scores.Confidence.Span.SequenceEqual(independently.Confidence.Span),"Direct fixed-glyph neural confidence differs.");
    for(int i=0;i<7;i++)
    {
        Require(actual.Scores.FamilyProbabilities[i]?.ToArray().SequenceEqual(independently.FamilyProbabilities[i]?.ToArray()??[])??independently.FamilyProbabilities[i] is null,"Direct fixed-glyph probability channel differs.");
        if(i<2&&!baselineFamilies)Require(actual.ResolvedWeights.Span[i]==0,"Inactive baseline family has nonzero weight.");
    }
    Require(actual.IntegerCosts.Span.SequenceEqual(independently.GetIntegerCosts(actual.ResolvedWeights.Span,1)),"Independent fixed-glyph integer costs differ.");
    Require(bridge.Chronology.Resolve(0,2).Key.Third==4,"Doubled raw points lost ingress ordinal.");
    neuralControls++;
}
var multi=new[]{letter,Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var cutSource=LegacyOrdinaryCursiveCutSource.Generate(multi,profile,90,80);
var groupEvaluator=new LegacyPreparedWordGroupEvaluator(model,cutSource,new(90,80,float.NaN,1));
Require(groupEvaluator.ContextWindowCount>0,"Multi-stroke real prior network did not produce accepted windows.");
var whole=Enumerable.Range(0,3).Select(s=>new LegacyInkRange(s,0,cutSource.Cuts.PreparedInk.StrokePointCounts.Span[s])).ToArray();
var ordered=groupEvaluator.Classify(whole);var permuted=groupEvaluator.Classify(whole.Reverse().ToArray());
Require(permuted.Geometry.Ranges.Span.SequenceEqual(ordered.Geometry.Ranges.Span)&&permuted.IntegerCosts.Span.SequenceEqual(ordered.IntegerCosts.Span),"Caller range order replaced native raw chronology.");
var internalRange=new LegacyInkRange(1,1,cutSource.Cuts.PreparedInk.StrokePointCounts.Span[1]-1);
var internalGlyph=groupEvaluator.Classify([internalRange]);
Require(internalGlyph.Geometry.Ranges.Span[0]==internalRange&&internalGlyph.IntegerCosts.Length==180,"Arbitrary internal candidate ownership lost.");
Reject(()=>groupEvaluator.Classify([]));Reject(()=>groupEvaluator.Classify(Enumerable.Repeat(new LegacyInkRange(0,0,1),11).ToArray()));
Reject(()=>groupEvaluator.Classify([new(0,0,9999)]));
Reject(()=>new LegacyPreparedWordGroupEvaluator(model,cutSource,new(90,0,float.NaN,1)));
Reject(()=>new LegacyPreparedWordGroupEvaluator(model,multi,cutSource.Cuts.PreparedInk,LegacySourcePointChronology.CreateOrdinary([1,1,1]),new(90,80,0,1)));
try{groupEvaluator.Classify(whole,new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
try{new LegacyPreparedWordGroupEvaluator(model,cutSource,new(90,80,0,1),new CancellationToken(true));throw new Exception("Constructor cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {neuralControls} actual-resource analytic normalized geometry→direct fixed-glyph probability/confidence/cost controls; source raw identities, active/inactive families, boundary signs, reusable context, reordered/internal candidates, bounds and cancellation. Own glyph fixtures do not measure Russian accuracy.");
static void Require(bool value,string message){if(!value)throw new Exception(message);}
static void Reject(Action action){try{action();throw new Exception("Malformed prepared group accepted.");}catch(ArgumentException){}}
