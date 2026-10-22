using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-inputs output");
var models=args[0];var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","Actual fullFIRST acoustic only; no accepted word/finalized guide");w.WriteBoolean("TargetsRead",false);w.WriteStartArray("Actual");int accepted=0,refused=0,totalGroups=0;long diagnosticValues=0;
var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
int guards=0,cancels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or InvalidDataException or OperationCanceledException){guards++;return;}throw new Exception("Expected bounded refusal.");}
var controlInk=new[]{new InkStroke(Enumerable.Range(0,32).Select(i=>new InkPoint(i*10,(float)(40*Math.Sin(i*1.3)))).ToArray())};
LegacyFullEnglishFirstParentInput Parent(LegacyFullEnglishFirstBounds? bounds=null)=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,controlInk,0,m30,m50,bounds);
Guard(()=>LegacyFullEnglishFirstKnowledge.Load(new byte[0]));var invalidResource=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res"));invalidResource[200]^=1;Guard(()=>LegacyFullEnglishFirstKnowledge.Load(invalidResource));
Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,controlInk,float.NaN,m30,m50));Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,controlInk,-1,m30,m50));
Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,controlInk,0,new byte[85812],m50));Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,controlInk,0,m30,[]));
Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,[],0,m30,m50));Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,new[]{new InkStroke(new InkPoint[4097])},0,m30,m50));
Guard(()=>LegacyFullEnglishFirstParentInput.Prepare(knowledge,new[]{new InkStroke(new[]{new InkPoint(float.PositiveInfinity,0)})},0,m30,m50));
foreach(var bad in new[]{new LegacyFullEnglishFirstBounds(0,4),new LegacyFullEnglishFirstBounds(4097,4),new LegacyFullEnglishFirstBounds(1,3),new LegacyFullEnglishFirstBounds(1,16385)})Guard(()=>Parent(bad));
var groupLimited=Parent(new(1,16384));Guard(()=>groupLimited.Run(table));if(!groupLimited.IsFaulted||groupLimited.IsComplete||groupLimited.NetworkEvaluationCount!=0)throw new Exception("Group admission must precede neural work.");Guard(()=>groupLimited.Run(table));
var nnLimited=Parent(new(4096,4));Guard(()=>nnLimited.Run(table));if(!nnLimited.IsFaulted||nnLimited.NetworkEvaluationCount!=4)throw new Exception("Cumulative NN limit/entered poison drift.");Guard(()=>nnLimited.Run(table));
var preCanceled=Parent();Guard(()=>preCanceled.Run(table,new CancellationToken(true)));if(preCanceled.IsFaulted||preCanceled.IsComplete)throw new Exception("Pre-entry cancellation consumed parent.");preCanceled.Run(table);Guard(()=>preCanceled.Run(table));
foreach(string stage in new[]{"run:entry","run:candidates","score:features","score:network0","score:network1","score:network2","score:network3","score:publish","run:child"})
{
    var parentControl=Parent();using var cts=new CancellationTokenSource();Guard(()=>parentControl.RunPrepared(table,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;
    if(!parentControl.IsFaulted||parentControl.IsComplete)throw new Exception("Entered cancellation did not poison one-use parent.");Guard(()=>parentControl.Run(table));
}
var multiInk=controlInk.Concat(controlInk.Select(stroke=>new InkStroke(stroke.Points.Select(p=>new InkPoint(p.X,p.Y+1000)).ToArray()))).ToArray();
var multi=LegacyFullEnglishFirstParentInput.Prepare(knowledge,multiInk,0,m30,m50);if(multi.Children.Count<2)throw new Exception("Prepared multiline control did not produce actual separate descriptors.");
using(var cts=new CancellationTokenSource())
{
    int seen=0;Guard(()=>multi.RunPrepared(table,stage=>{if(stage=="run:child"&&++seen==2)cts.Cancel();},cts.Token));cancels++;
    if(!multi.IsFaulted||multi.IsComplete||multi.CopyCompletedChildren().Count!=1)throw new Exception("Entered multiline cancellation lost historical complete prefix.");
    var retained=multi.CopyCompletedChildren()[0];_=retained.ScoreGroup(retained.Owner.CopyLiveGroupIds()[0]);Guard(()=>multi.Run(table));
}
var competing=Parent();using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
{
    var winner=Task.Run(()=>competing.RunPrepared(table,stage=>{if(stage=="run:entry"){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(10)))throw new Exception("One-use reservation barrier timed out.");}},default));
    if(!entered.Wait(TimeSpan.FromSeconds(10)))throw new Exception("Reservation was not observed.");Guard(()=>competing.Run(table));release.Set();winner.GetAwaiter().GetResult();if(!competing.IsComplete||competing.IsFaulted)throw new Exception("Exactly one concurrent parent must complete.");
}
var diagnosticMutation=Parent();diagnosticMutation.RunDiagnostic(table,(_,_,costs)=>costs[0]^=12345);
var productionRows=preCanceled.CopyCompletedChildren()[0];var diagnosticRows=diagnosticMutation.CopyCompletedChildren()[0];
foreach(int group in productionRows.Owner.CopyLiveGroupIds())if(!productionRows.ScoreGroup(group).CopyFinalCosts().SequenceEqual(diagnosticRows.ScoreGroup(group).CopyFinalCosts()))throw new Exception("Diagnostic callback altered production costs.");
var completedControl=preCanceled.CopyCompletedChildren()[0];int testGroup=completedControl.Owner.CopyLiveGroupIds()[0];var immutable=completedControl.ScoreGroup(testGroup);int firstCost=immutable.CopyFinalCosts()[0];var changedCosts=immutable.CopyFinalCosts();changedCosts[0]^=1;if(immutable.CopyFinalCosts()[0]!=firstCost)throw new Exception("Cache copy alias.");
var changedRaw=preCanceled.CopyRaw();var originalRaw=preCanceled.CopyRaw()[0].Points[0];changedRaw[0]=new(new[]{new InkPoint(999,999)});if(preCanceled.CopyRaw()[0].Points[0]!=originalRaw)throw new Exception("Raw snapshot alias.");
var foreignKnowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.res")));if(ReferenceEquals(foreignKnowledge,knowledge)||ReferenceEquals(foreignKnowledge.Source,knowledge.Source))throw new Exception("Equal resource bytes conflated owner identity.");
Guard(()=>completedControl.ScoreGroup(-1));
if(LegacyFullEnglishFirstBudget.CacheEnvelope(4096,256)!=16580608||LegacyFullEnglishFirstBudget.CacheEnvelope(4096,256)>16L*1024*1024)throw new Exception("Max retained-cache bound arithmetic drift.");Guard(()=>LegacyFullEnglishFirstBudget.CacheEnvelope(4097,256));Guard(()=>LegacyFullEnglishFirstBudget.CacheEnvelope(4096,257));
if(LegacyFullEnglishFirstBudget.AddDerivedPoints(4096,4096)!=8192)throw new Exception("Derived point boundary drift.");Guard(()=>LegacyFullEnglishFirstBudget.AddDerivedPoints(8192,1));Guard(()=>LegacyFullEnglishFirstBudget.AddDerivedPoints(0,4097));
if(LegacyFullEnglishFirstBudget.SourceEnvelope(4096,8192,256)!=14999662)throw new Exception("Source logical peak arithmetic drift.");Guard(()=>LegacyFullEnglishFirstBudget.SourceEnvelope(4097,8192,256));Guard(()=>LegacyFullEnglishFirstBudget.SourceEnvelope(4096,8193,256));
var mutableTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(mutableTable.Values,out var mutableSegment))throw new Exception("Expected table snapshot control.");var savedTableValue=mutableSegment.Array![mutableSegment.Offset];mutableSegment.Array[mutableSegment.Offset]=savedTableValue+1;var preflightTable=Parent();Guard(()=>preflightTable.Run(mutableTable));if(preflightTable.IsFaulted||preflightTable.IsComplete)throw new Exception("Rejected table consumed entry.");mutableSegment.Array[mutableSegment.Offset]=savedTableValue;preflightTable.Run(mutableTable);
var zeroWindows=Enumerable.Range(0,16).Select(_=>new int[16]).ToArray();Guard(()=>LegacyBaselineFreeDirectional2B1Assembly.Compose(new int[16],zeroWindows,false,new short[274],new short[274]));Guard(()=>LegacyBaselineFreeDirectional2B1Assembly.Compose(new int[17],zeroWindows.Take(15).ToArray(),false,new short[274],new short[274]));Guard(()=>LegacyBaselineFreeDirectional2B1Assembly.Compose(new int[17],zeroWindows,false,new short[273],new short[274]));Guard(()=>LegacyBaselineFreeDirectional2B1Assembly.Compose(new int[17],zeroWindows,false,new short[274],new short[274],new CancellationToken(true)));
foreach(var item in items)
{
    var raw=item.TryGetProperty("Raw",out var rawValue)?rawValue:item.GetProperty("strokes");
    string name=item.TryGetProperty("Name",out var label)?label.GetString()!:item.GetProperty("id").GetString()!;
    float resolution=item.TryGetProperty("Resolution",out var res)?res.GetSingle():0;
    var strokes=raw.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    w.WriteStartObject();w.WriteString("Name",name);LegacyFullEnglishFirstParentInput? parent=null;
    try
    {
        parent=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);
        var diagnosticNeural=new Dictionary<(int Descriptor,int Group),int[]>();long retainedDiagnosticBytes=0;
        parent.RunDiagnostic(table,(candidate,group,values)=>{long bytes=values.Length*4L+128;if(bytes>4L*1024*1024-retainedDiagnosticBytes)throw new Exception("Probe-only perparent transient neural reservation exceeded.");retainedDiagnosticBytes+=bytes;diagnosticNeural.Add((candidate.Child.DescriptorOrdinal,group),values);});
        if(!parent.IsComplete||parent.IsFaulted)throw new Exception("Unexpected incomplete fullFIRST pass.");
        var production=LegacyFullEnglishFirstParentInput.Prepare(knowledge,strokes,resolution,m30,m50);production.Run(table);
        if(!production.IsComplete||production.IsFaulted||production.NetworkEvaluationCount!=parent.NetworkEvaluationCount)throw new Exception("Production/diagnostic wholepass mismatch.");
        var productionChildren=production.CopyCompletedChildren();var diagnosticChildren=parent.CopyCompletedChildren();if(productionChildren.Count!=diagnosticChildren.Count)throw new Exception("Production/diagnostic child count mismatch.");
        for(int ci=0;ci<productionChildren.Count;ci++)
        {
            var pc=productionChildren[ci];var dc=diagnosticChildren[ci];if(!pc.Owner.CopyLiveGroupIds().SequenceEqual(dc.Owner.CopyLiveGroupIds()))throw new Exception("Production/diagnostic live groups mismatch.");
            foreach(int group in pc.Owner.CopyLiveGroupIds())if(!pc.ScoreGroup(group).CopyFinalCosts().SequenceEqual(dc.ScoreGroup(group).CopyFinalCosts())||!pc.ScoreGroup(group).CopyCandidates().SequenceEqual(dc.ScoreGroup(group).CopyCandidates()))throw new Exception("Production/diagnostic cache mismatch.");
        }
        w.WriteBoolean("ProductionDiagnosticParity",true);

        w.WriteString("Status","AcceptedAcoustic");w.WriteNumber("NetworkCount",parent.NetworkEvaluationCount);w.WriteStartArray("Children");
        foreach(var cache in parent.CopyCompletedChildren())
        {
            var c=cache.Owner;w.WriteStartObject();w.WriteNumber("Descriptor",c.Child.DescriptorOrdinal);w.WriteNumber("KnowledgeFlags",cache.KnowledgeFlags);Numbers("Map",c.Child.CopyCurrentParentMap());w.WriteNumber("Nodes",c.NodeCount);w.WriteNumber("Baseline",c.Preparation.Line.Baseline);w.WriteNumber("Height",c.Preparation.Line.Height);FloatInk("Raw",c.Child.Submission.CopyRawChildInput());FloatInk("CutInk",c.CopyCutStrokes());Numbers("CutCounts",c.CopyCutCounts());
            w.WriteStartArray("Primary");foreach(byte b in cache.CopyMasks().Primary.Span)w.WriteNumberValue(b);w.WriteEndArray();w.WriteStartArray("Groups");
            foreach(int group in c.CopyLiveGroupIds())
            {
                var result=cache.ScoreGroup(group);w.WriteStartObject();w.WriteNumber("Group",group);Numbers("Neural",diagnosticNeural[(c.Child.DescriptorOrdinal,group)]);Numbers("Final",result.CopyFinalCosts());w.WriteNumber("Fallback",result.FallbackCost);w.WriteBoolean("Whole",result.WholePhysicalScope);w.WriteBoolean("Zero",result.EndpointKeysZero);if(result.SourceProbability is{} probability)w.WriteNumber("Probability",probability);else w.WriteNull("Probability");
                w.WriteStartArray("Candidates");foreach(var candidate in result.CopyCandidates()){w.WriteStartArray();w.WriteNumberValue(candidate.Symbol);w.WriteNumberValue(candidate.Cost);w.WriteEndArray();}w.WriteEndArray();
                //Probe-only transient replay. Stream each result immediately;
                //never retain NN histories inside the production cache. A hard
                //aggregate scalar ceiling is charged BEFORE any replay arrays.
                var plannedRanges=c.CopyGroupRanges(group);long replayCost=12000L+8L*plannedRanges.Sum(r=>r.EndPointExclusive-r.StartPoint);if(replayCost>400000000-diagnosticValues)throw new Exception("Probe-only aggregate streaming-work budget exceeded.");diagnosticValues+=replayCost;
                var feature=LegacyFullEnglishFirstFeatures.Prepare(c,group,default);var ranges=feature.CopyStoredRanges();RangeRows("Stored",ranges);RangeRows("Ordered",feature.CopyOrderedRanges());Numbers("Order",feature.CopyNormalizationOrder());Numbers("RetainedPenUps",feature.CopyRetainedPenUpBoundaries());
                w.WriteStartArray("StoredStartKeys");foreach(var range in ranges){var key=c.ResolveCutPoint(range.StrokeIndex,range.StartPoint).Key;w.WriteStartArray();foreach(int v in new[]{key.First,key.Second,key.Third})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
                w.WriteNumber("SourceCount",ranges.Select(r=>r.StrokeIndex).Distinct().Count());Numbers("Boundary",new[]{(int)feature.Boundary.Start,feature.Boundary.End});w.WriteBoolean("Degenerate",feature.DegenerateIntegerGeometry);ShortInk("Normalized",feature.CopyNormalizedStrokes());ShortInk("Merged",feature.CopyMergedStrokes());
                w.WriteStartArray("Inputs");foreach(var vector in feature.Inputs){w.WriteStartArray();foreach(short value in vector)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();
                w.WriteStartArray("Networks");for(int family=0;family<4;family++){var network=knowledge.Evaluate(family,feature.Inputs[family],default);w.WriteStartObject();Numbers("HiddenDots",network.CopyHiddenDots());Numbers("Hidden",network.CopyHiddenValues().Select(x=>(int)x));Numbers("Raw",network.CopyOutputDots());Numbers("Probabilities",network.CopyProbabilities());w.WriteNumber("StabilizationOffset",network.StabilizationOffset);w.WriteEndObject();}w.WriteEndArray();
                w.WriteEndObject();totalGroups++;
            }
            w.WriteEndArray();w.WriteEndObject();
        }
        w.WriteEndArray();accepted++;
    }
    catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or InvalidOperationException)
    {w.WriteString("Status","Refused");w.WriteString("Reason",e.GetType().Name+": "+e.Message);w.WriteBoolean("EnteredFault",parent?.IsFaulted??false);w.WriteNumber("CompletedChildren",parent?.CopyCompletedChildren().Count??0);refused++;}
    w.WriteEndObject();w.Flush();
}
w.WriteEndArray();w.WriteNumber("Guards",guards);w.WriteNumber("Cancellations",cancels);w.WriteStartArray("PreparedAssembly");
var rng=new Random(0x1011);int[] extrema=[int.MinValue,int.MaxValue,-65536,-32769,-32768,-1024,0,1024,32767,32768,65536];
for(int iteration=0;iteration<128;iteration++)
{
    var global=Enumerable.Range(0,17).Select(i=>extrema[(iteration+i)%extrema.Length]).ToArray();var windows=Enumerable.Range(0,16).Select(i=>Enumerable.Range(0,16).Select(j=>iteration<32?extrema[(iteration+i+j)%extrema.Length]:rng.Next(-100000,100000)).ToArray()).ToArray();
    var minimum=LegacyDirectional2B1DefaultCalibration.CopyMinimum(false);var maximum=LegacyDirectional2B1DefaultCalibration.CopyMaximum(false);if(iteration%3==0)for(int i=0;i<274;i+=7)maximum[i]=minimum[i];
    w.WriteStartObject();Numbers("Global",global);w.WriteStartArray("Windows");foreach(var window in windows){w.WriteStartArray();foreach(int value in window)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();Numbers("Minimum",minimum.Select(v=>(int)v));Numbers("Maximum",maximum.Select(v=>(int)v));
    Numbers("Temporal",LegacyBaselineFreeDirectional2B1Assembly.Compose(global,windows,false,minimum,maximum).Select(v=>(int)v));Numbers("Spatial",LegacyBaselineFreeDirectional2B1Assembly.Compose(global,windows,true,minimum,maximum).Select(v=>(int)v));w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteNumber("Groups",totalGroups);w.WriteEndObject();w.Flush();Console.WriteLine($"actual={items.Length} accepted={accepted} refused={refused} groups={totalGroups}");
void Numbers(string name,IEnumerable<int> values){w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}

void RangeRows(string name,IEnumerable<LegacyInkRange> ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();foreach(int v in new[]{r.StrokeIndex,r.StartPoint,r.EndPointExclusive})w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();}
void FloatInk(string name,IEnumerable<InkStroke> strokes){w.WriteStartArray(name);foreach(var stroke in strokes){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
void ShortInk(string name,IEnumerable<LegacyInkStroke> strokes){w.WriteStartArray(name);foreach(var stroke in strokes){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
