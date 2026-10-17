using InkRecognition;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
byte[] lk=File.ReadAllBytes(args[8]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[1]));
int cases=0,groups=0,guards=0,cross=0,parity=0;using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var refs=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,refs,activations);
    var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);var symbols=LegacyFreshMainGlobalCaseSymbols.Prepare(ak,lk,model,geometry,cd,pd);var scorer=LegacyFreshMainGlobalAcousticScorer.Prepare(ak,symbols);
    if(!ReferenceEquals(scorer.Owner,symbols)||!ReferenceEquals(symbols.Geometry,geometry)||!ReferenceEquals(symbols.Model,model)||!ReferenceEquals(symbols.Symbols.Preparation.Alphabet,model.AcousticAlphabet))throw new Exception("Global NN/model/AL ownership changed.");
    var mask=symbols.CopyMasks();if(mask.Primary.Span.Count((byte)1)!=145)throw new Exception("Case-aware actual145 mask changed.");
    w.WriteStartObject();w.WriteNumber("target",geometry.TargetHeight);w.WriteStartArray("points");foreach(var s in cuts.CopyStrokes()){w.WriteStartArray();foreach(var p in s.Points)Floats(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();Ints("mask",mask.Primary.ToArray().Select(v=>(int)v));
    Floats("cacheProfile",new[]{scorer.CacheProfile.FallbackPenalty,scorer.CacheProfile.LongDashWeight,scorer.CacheProfile.CjkDashWeight,scorer.CacheProfile.NegativeDirectionDivisor,scorer.CacheProfile.PositiveDirectionDivisor,scorer.CacheProfile.ClosedPrimitiveWeight});Ints("cacheCaps",[scorer.CacheProfile.CandidateThreshold,scorer.CacheProfile.MinimumCandidates,scorer.CacheProfile.CandidateLimit]);
    var score=new LegacyPreparedFreshMain240AcousticScores[graph.State.Groups.Count];w.WriteStartArray("groups");
    for(int g=0;g<score.Length;g++)
    {
        score[g]=scorer.Score(g);groups++;var result=score[g];var frame=geometry.ReadFrame(g);var norm=geometry.NormalizeGroup(g);
        if(!norm.Strokes.SelectMany(s=>s.Points.ToArray()).SequenceEqual(result.Glyph.Geometry.Strokes.SelectMany(s=>s.Points.ToArray())))throw new Exception("Global normalization/classifier geometry mismatch.");
        var expectedSign=geometry.ReadBoundarySigns(g);if(expectedSign!=result.BoundarySigns)throw new Exception("Actual clip policy changed.");
        if(graph.ResolveGroupSource(g).Secondary is not null)cross++;
        var geo=result.Glyph.Geometry;var cx=result.Context.Span;var bounds=result.BoundarySigns;
        var tm=Calibration("temporal","minima");var tx=Calibration("temporal","maxima");var sm=Calibration("spatial","minima");var sx=Calibration("spatial","maxima");
        short[][] features=[LegacyStrokeFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyRasterFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyTemporalDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,tm,tx,cx,bounds.Start,bounds.End),LegacySpatialDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,sm,sx,cx,bounds.Start,bounds.End)];
        w.WriteStartObject();w.WriteNumber("descriptor",frame.DescriptorOrdinal);w.WriteNumber("line",frame.LineId);w.WriteNumber("baseline",frame.Baseline);w.WriteNumber("height",frame.Height);w.WriteNumber("minX",geometry.ReadMinimumX(g));var sourcePair=graph.ResolveGroupSource(g);w.WriteNumber("primaryBaseline",sourcePair.Primary.Baseline);w.WriteNumber("primaryHeight",sourcePair.Primary.Height);if(sourcePair.Secondary is{} secondary){w.WriteNumber("secondaryBaseline",secondary.Baseline);w.WriteNumber("secondaryHeight",secondary.Height);}
        w.WriteStartArray("ranges");foreach(var r in geo.Ranges.Span)Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();w.WriteStartArray("startKeys");foreach(var r in geo.Ranges.Span){var key=cuts.Chronology.Resolve(r.StrokeIndex,r.StartPoint).Key;Ints(null,[key.First,key.Second,key.Third]);}w.WriteEndArray();w.WriteStartArray("normalized");foreach(var s in geo.Strokes){w.WriteStartArray();foreach(var p in s.Points)Ints(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
        Ints("context",cx.ToArray().Select(v=>(int)v));Ints("boundaries",[bounds.Start,bounds.End]);w.WriteStartArray("inputs");foreach(var f in features)Ints(null,f.Select(v=>(int)v));w.WriteEndArray();w.WriteStartArray("probabilities");foreach(var f in result.Glyph.Scores.FamilyProbabilities.Take(4))Ints(null,f!.Value.ToArray());w.WriteEndArray();Floats("weights",result.Glyph.ResolvedWeights.ToArray());Ints("integerCosts",result.Glyph.IntegerCosts.ToArray());Ints("cacheCosts",result.Cache.Costs.ToArray());w.WriteStartArray("candidates");foreach(var c in result.Cache.Candidates.Span)Ints(null,[c.Symbol,c.Cost]);w.WriteEndArray();w.WriteBoolean("whole",result.PrimitiveScope.BoundaryAlignedSingleNormalizedStroke);w.WriteBoolean("zero",result.PrimitiveScope.EndpointFieldsAreZero);w.WriteEndObject();
    }w.WriteEndArray();w.WriteEndObject();cases++;
    if(local.Steps.Count==1)
    {
        var single=LegacyFreshMainSingleLineSampler.Prepare(ak,input.Smoothed).Sample(table);var oldContext=LegacyFreshMainSingleLineContext.Prepare(ak,single,activations);var oldOwner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,single,cd,pd);var oldScorer=oldOwner.CreateAcousticScorer(ak,oldContext);var saved=new Dictionary<int,LegacyPreparedFreshMain240AcousticScores>();
        if(!oldOwner.CopyMasks().Primary.Span.SequenceEqual(mask.Primary.Span))throw new Exception("Actual old/new case-aware masks differ.");
        for(int n=0;n<graph.State.NodeCount;n++)for(int a=0;a<graph.State.GetNode(n).Count;a++)
        {
            var arc=graph.State.GetNode(n)[a];var same=single.SourceRanges.Graph.GetNode(n).Single(v=>v.Span==arc.Span&&v.StartContextId==arc.StartContextId&&v.EndContextId==arc.EndContextId);
            if(!saved.TryGetValue(same.GroupId,out var old)){old=oldScorer.Score(same.GroupId);saved.Add(same.GroupId,old);}
            var now=score[arc.GroupId];if(!old.Glyph.IntegerCosts.Span.SequenceEqual(now.Glyph.IntegerCosts.Span)||!old.Cache.Costs.Span.SequenceEqual(now.Cache.Costs.Span)||!old.Context.Span.SequenceEqual(now.Context.Span))throw new Exception("Old actual single-domain NN/final-cache parity failed.");parity++;
        }
    }
    Reject(()=>LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context,new(true)));Reject(()=>geometry.NormalizeGroup(-1));Reject(()=>scorer.Score(0,new(true)));Reject(()=>LegacyFreshMainGlobalCaseSymbols.Prepare(ak,lk,model,geometry,cd,pd,new(true)));
    var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalAcousticScorer.Prepare(bad,symbols));var badlk=lk.ToArray();badlk[0]^=1;Reject(()=>LegacyFreshMainGlobalCaseSymbols.Prepare(ak,badlk,model,geometry,cd,pd));
    var expose=geometry.NormalizeGroup(0);var copy=expose.PreparedInk.Points.ToArray();copy[0]=copy[0]with{X=100000};if(!scorer.Score(0).Cache.Costs.Span.SequenceEqual(score[0].Cache.Costs.Span))throw new Exception("Exposed diagnostic geometry changed retained source.");
}
w.WriteEndArray();w.Flush();if(cross==0||parity==0)throw new Exception("Actual cross/single-domain NN path not exercised.");Console.WriteLine($"PASS {cases} actual owners/{groups} four-family/cache groups/{cross} cross groups/{parity} old single-domain arc parity/{guards} guards; model resources reused.");
short[] Calibration(string part,string field)=>engine.RootElement.GetProperty(part).GetProperty(field).EnumerateArray().Select(v=>v.GetInt16()).ToArray();
void Floats(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported global NN source accepted.");}
