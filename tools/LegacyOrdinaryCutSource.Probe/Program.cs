using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 5))throw new ArgumentException("Usage: controls.json [acoustic.res lexical.res engine-profile.json separation-profile.json]");
var controls=JsonSerializer.Deserialize(File.ReadAllText(args[0]),ControlsContext.Default.Controls)!;
int count=0,insertions=0,nonzero=0;
foreach(var test in controls.Cases)
{
    var raw=test.Strokes.Select(s=>new InkStroke(s.Select(p=>new InkPoint(p[0],p[1])).ToArray())).ToArray();
    var bridge=LegacyOrdinaryCursiveCutSource.Generate(raw,new(0x135,true,0,0),test.Baseline,test.Height);
    int[] starts=new int[raw.Length];for(int s=1;s<starts.Length;s++)starts[s]=starts[s-1]+test.Indices[s-1].Length;
    for(int s=0;s<raw.Length;s++)
    {
        Require(bridge.Source.OriginalPointIndices[s].Span.SequenceEqual(test.Indices[s]),"Raw ingress map differs from independent oracle.");
        float actual=bridge.GetSourceStrokeProbability(s);if(actual>0)nonzero++;
        Require(MathF.Abs(actual-test.Probabilities[s])<.0002f,"Per-stroke source geometry differs from independent oracle.");
        int begin=s==0?0:bridge.Cuts.PreparedInk.StrokePointCounts.Span[..s].ToArray().Sum();
        for(int p=0;p<bridge.Cuts.PreparedInk.StrokePointCounts.Span[s];p++)
        {
            var point=bridge.Cuts.PreparedInk.Points.Span[begin+p];
            int retained=point.OriginalPointIndex-starts[s];
            Require(bridge.Chronology.Resolve(s,p).Key==new LegacySourcePointKey(0,s,test.Indices[s][retained]),"Cut insertion/dedup lost original raw point identity.");
        }
        var whole=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(bridge.Cuts.PreparedInk,
            [new(s,0,bridge.Cuts.PreparedInk.StrokePointCounts.Span[s])],0,test.Baseline,test.Height,100,10);
        var scope=new LegacyOrdinaryAcousticCacheContext(true,true,true,true,false,.123f,false,ReadOnlyMemory<ushort>.Empty);
        Require(bridge.BindCacheProbability(scope,whole)==scope with{SourceStrokeProbability=actual},"Final-cache source service selected wrong physical record or changed scope.");
        Require(bridge.BindCacheProbability(scope with{IsWholePhysicalStroke=false},whole).SourceStrokeProbability is null,"Inactive whole scope retained an invented probability.");
    }
    insertions+=bridge.Cuts.PreparedInk.Points.Length-test.Indices.Sum(s=>s.Length);
    var disabled=LegacyOrdinaryCursiveCutSource.Generate(raw,new(0x135,false,0,0),test.Baseline,test.Height);
    Require(disabled.Source.SourceStrokeProbabilities.Span.ContainsAnyExcept(0f)==false,"Explicit disabled source service not respected.");
    count++;
}
Require(nonzero>0&&insertions>0,"Fixtures must exercise nonzero physical records and inserted cut points.");
InkStroke Stroke(params (float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var square=Stroke((0,0),(10,0),(10,10),(0,10),(0,0));var enclosed=Stroke((3,3),(4,4),(5,5));
var squareBridge=LegacyOrdinaryCursiveCutSource.Generate([square,enclosed],new(0x135,true,0,0),10,10);
Require(MathF.Abs(squareBridge.GetSourceStrokeProbability(0)-12.566f/16)<1e-6&&squareBridge.GetSourceStrokeProbability(1)==0,"Distinct source record values were averaged/replicated.");
var partial=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(squareBridge.Cuts.PreparedInk,[new(0,1,3)],0,10,10,100,10);
var wholeScope=new LegacyOrdinaryAcousticCacheContext(true,true,true,true,false,null,false,ReadOnlyMemory<ushort>.Empty);
Reject(()=>squareBridge.BindCacheProbability(wholeScope,partial));
var foreign=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(LegacyCursivePreparedInk.Create([square]),[new(0,0,5)],0,10,10,100,10);
Reject(()=>squareBridge.BindCacheProbability(wholeScope,foreign));
var multiple=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(squareBridge.Cuts.PreparedInk,
    [new(0,0,squareBridge.Cuts.PreparedInk.StrokePointCounts.Span[0]),new(1,0,3)],0,10,10,100,10);
try{squareBridge.BindCacheProbability(wholeScope,multiple);throw new Exception("Merged whole scope accepted.");}catch(NotSupportedException){}
Reject(()=>squareBridge.GetSourceStrokeProbability(-1));Reject(()=>squareBridge.GetSourceStrokeProbability(2));
try{LegacyOrdinaryCursiveCutSource.Generate([square],new(0x135,true,0,0),10,10,cancellationToken:new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {count} independent ingress/enclosure controls; {insertions} inserted cut points preserve raw keys; {nonzero} nonzero physical records; scoped final-cache binding, disabled service, malformed ownership and cancellation.");
if(args.Length==1)return;
byte[] acoustic=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
var cache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var candidateScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var finalScorer=new LegacyCursiveFinalCacheGroupScorer(cache,candidateScorer,model.CreateCursiveCategoryMapper(1),
    LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),[],0);
var printed=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var duplicates=printed.Select(s=>new InkStroke(s.Points.SelectMany(p=>new[]{p,p,p}).ToArray())).ToArray();
var recognizer=new LegacyRussianCursiveWordRecognizer(model,0);
var options=new LegacyCursiveWordSearchOptions(90,80,float.NaN,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
int callbackCount=0,completeGroups=0;
LegacyOrdinaryAcousticCacheContext Scope(LegacyCursiveCandidateGroup group,LegacyPreparedGlyphClassification glyph)
{
    callbackCount++;var ranges=glyph.Geometry.Ranges.Span;
    bool whole=ranges.Length==1&&ranges[0].StartPoint==0&&ranges[0].EndPointExclusive==glyph.Geometry.PreparedInk.StrokePointCounts.Span[ranges[0].StrokeIndex];
    if(whole)completeGroups++;
    return new(true,true,true,whole,false,null,false,ReadOnlyMemory<ushort>.Empty);
}
var plain=recognizer.RecognizeOrdinarySourceWithFinalCache(printed,profile,options,finalScorer,Scope,false,false,false);
var cleaned=recognizer.RecognizeOrdinarySourceWithFinalCache(duplicates,profile,options with{SourceStrokeProbability=float.PositiveInfinity},finalScorer,Scope,false,false,false);
Require(plain.ClassifiedGroups>0&&plain.ClassifiedGroups==cleaned.ClassifiedGroups&&callbackCount==plain.ClassifiedGroups*2&&completeGroups>0,"Actual source/final-cache pipeline skipped classification.");
Require(plain.Words.Select(w=>(w.Text,w.Score.TotalCost)).SequenceEqual(cleaned.Words.Select(w=>(w.Text,w.Score.TotalCost))),"Raw duplicate-point ingress changed actual word costs/ranks.");
Console.WriteLine($"PASS actual RU resource ingress→cuts→raw chronology→4 neural families→final cache→dictionary: {plain.ClassifiedGroups} groups; top={string.Join(", ",plain.Words.Take(5).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}. Own printed fixture is diagnostic, not accuracy evidence.");
static void Require(bool value,string message){if(!value)throw new Exception(message);}
static void Reject(Action action){try{action();throw new Exception("Malformed scoped source accepted.");}catch(ArgumentException){}}
sealed record Controls(Case[] Cases);
sealed record Case(float[][][] Strokes,int[][] Indices,float[] Probabilities,float Baseline,float Height);
[System.Text.Json.Serialization.JsonSerializable(typeof(Controls))]
partial class ControlsContext:System.Text.Json.Serialization.JsonSerializerContext;
