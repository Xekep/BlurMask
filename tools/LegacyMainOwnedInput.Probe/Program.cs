using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture.json");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var samples=new List<(string Id,InkStroke[] Raw)> {("oneMIR",Word(0,0)),("twoRowsReverse",Word(0,250).Concat(Word(0,0)).ToArray())};
foreach(int offset in new[]{230,300,450,700})samples.Add(("twoWordsOffset"+offset,Word(0,0).Concat(Word(offset,0)).ToArray()));
samples.Add(("ownEnglishHIHI",Hi(0).Concat(Hi(220)).ToArray()));
int passed=0,unsupported=0,guards=0,multiple=0;
LegacyFreshMainOwnedInputPreparation? firstOwner=null;LegacyFreshMainOwnedFirstStage? firstStage=null;
using(var output=File.Create(args[7]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteStartArray("inputs");
    foreach(var sample in samples)
    {
        var owner=Prepare(sample.Raw);w.WriteStartObject();w.WriteString("id",sample.Id);
        int count=owner.FirstSubmissions.Count;w.WriteNumber("submissions",count);if(count>1)multiple++;
        if(sample.Id=="twoRowsReverse"&&count!=2)throw new Exception("Actual embeddedNN did not split two source rows.");
        w.WriteStartArray("descriptorsBeforeFirst");foreach(var d in owner.Reordered.CopyDescriptors())w.WriteBase64StringValue(d);w.WriteEndArray();
        foreach(var submitted in owner.FirstSubmissions)
        {
            var native=owner.FirstSource.FirstSubmissions.Single(s=>s.DescriptorOrdinal==submitted.DescriptorOrdinal);
            if(!ReferenceEquals(native.Factory,submitted.Recognition.Factory))throw new Exception("Same first factory was replaced.");
            var masks=submitted.Recognition.MaskPolicy.CopyMasks();if(masks.Primary.Length!=192||masks.Primary.Span.IndexOf((byte)0)!=-1)throw new Exception("Actual native first-mask proof changed.");
        }
        try
        {
            var stage=owner.RunFirstStage();var result=owner.CompletePostprocessing(stage);passed++;
            if(firstOwner is null){firstOwner=owner;firstStage=stage;}
            w.WriteBoolean("supported",true);w.WriteStartArray("first");
            foreach(var r in stage.Results)
            {
                var m=r.Materialized;w.WriteStartObject();w.WriteNumber("groups",r.Owner.GroupScores.Count);w.WriteNumber("cost",m.Selection.TotalCost);
                var grid=new LegacyFirstMergedRecordGrid(r.Contexts.ColumnCountB8,r.Contexts.BeamWidth,r.Contexts.CopyB8Storage());var trace=LegacyFirstSelectedGlyphTrace.Read(grid,m.Selection);
                w.WriteStartArray("labels");foreach(var c in trace)w.WriteNumberValue(c.Symbol);w.WriteEndArray();
                w.WriteStartArray("wordBoundary");foreach(var c in trace)w.WriteBooleanValue(c.WordBoundaryBit);w.WriteEndArray();
                w.WriteString("mask",Convert.ToHexString(r.Owner.MaskPolicy.CopyWorkingPrimary(default)));
                w.WriteStartArray("primary98");for(int n=0;n<r.Primary.NodeCount;n++)w.WriteBase64StringValue(r.Primary.GetRowCopy(n).CopyStorage());w.WriteEndArray();
                w.WriteStartArray("b8");foreach(var row in r.Contexts.CopyB8Storage())w.WriteBase64StringValue(row.Span);w.WriteEndArray();w.WriteEndObject();
            }
            w.WriteEndArray();w.WriteStartArray("descriptors");foreach(var d in result.Lines.CopyDescriptors())w.WriteBase64StringValue(d);w.WriteEndArray();
            w.WriteStartArray("guides");foreach(var g in result.Lines.CopyGuides())w.WriteBase64StringValue(g);w.WriteEndArray();
            w.WriteStartArray("source720");foreach(var r in result.Lines.PublishedRows)w.WriteBase64StringValue(r.CopyStorage().Span);w.WriteEndArray();
            WritePoints("parentPoints",owner.FirstSource.Controller.Source.Strokes.Select(s=>s.Points));
            WritePoints("currentPoints",result.Lines.CopyPhysicalPoints());
            var controller=owner.FirstSource.Controller;
            w.WriteStartArray("markers");foreach(var v in new[]{controller.Markers.First,controller.Markers.Second,controller.Markers.Hyphen,controller.Markers.Underscore,controller.Markers.ProlongedSound,controller.Markers.IdeographOne,controller.Markers.Replacement})w.WriteNumberValue(v);w.WriteEndArray();
            var fp=controller.Profile.PrepareFitProfile(0,0,[]);w.WriteStartArray("fitProfile");foreach(float v in new[]{fp.MinimumItemCount,fp.MaximumItemCount,fp.MinimumSlope,fp.MaximumSlope,fp.MinimumHeight,fp.MaximumBoxWidth,fp.KnownGlyphWeightCap})w.WriteNumberValue(v);w.WriteEndArray();
            w.WriteStartArray("smoothedCounts");foreach(var s in result.Smoothed.Strokes)w.WriteNumberValue(s.Points.Count);w.WriteEndArray();
            w.WriteNumber("sourceGapBits",BitConverter.SingleToInt32Bits(result.Smoothed.SourceGap));
            if(result.Lines.CopyGuides().Length!=count)throw new Exception("Actual per-descriptor main guide owner was lost.");
            if(count>1)Reject(()=>LegacyFreshMainSingleLineSampler.Prepare(ak,result.Smoothed));
        }
        catch(NotSupportedException e)when(e.Message.Contains("single-word path")||e.Message.Contains("24A empty-result"))
        {unsupported++;w.WriteBoolean("supported",false);w.WriteString("refusal",e.Message);Console.WriteLine(sample.Id+": scoped refusal: "+e.Message);}
        w.WriteEndObject();
    }
    w.WriteEndArray();
    if(firstOwner is null||firstStage is null||multiple==0)throw new Exception("Owning positive/multiple-descriptor coverage absent.");
    var other=Prepare(Word(0,0));Reject(()=>other.CompletePostprocessing(firstStage));
    Reject(()=>firstOwner.RunFirstStage(new CancellationToken(true)));Reject(()=>firstOwner.CompletePostprocessing(firstStage,new CancellationToken(true)));
    Reject(()=>LegacyFreshMainOwnedInputPreparation.Prepare(ak,line30,line50,Word(0,0),0,activations,table,caseData,publication,new CancellationToken(true)));
    var factory=firstOwner.FirstSubmissions[0].Recognition.Factory;byte[] corrupt=ak.ToArray();corrupt[factory.AcousticModel.OrdinaryNodeOffset+100]^=1;
    Reject(()=>LegacyFreshFirstOwnedRecognition.PrepareForOwnedFactory(corrupt,factory,activations,table));
    var same=LegacyFreshFirstOwnedRecognition.PrepareForOwnedFactory(ak,factory,activations,table);
    if(!ReferenceEquals(same.Factory,factory))throw new Exception("Internal shared core silently replaced factory.");
    //Ending108 must retain trailing duplicate raw points, not infer raw count
    //from the last retained starting ordinal. No raw-source helper is changed.
    var duplicate=LegacyFreshFirstResultOwnershipFactory.Prepare(ak,6120,[new InkStroke([new(0,0),new(0,80),new(0,80),new(0,80)])]);
    var src=duplicate.Frame.Packet.Source;var last=src.Chronology.Resolve(0,src.Strokes[0].Points.Count-1);
    if(src.ResolveEndingMetadata(last).Key.Third!=3)throw new Exception("Trailing raw endpoint owner changed.");
    var duplicateOwned=LegacyFreshFirstOwnedRecognition.PrepareForOwnedFactory(ak,duplicate,activations,table);_=duplicateOwned.Run();
    w.WriteNumber("duplicateRawEndThird",src.ResolveEndingMetadata(last).Key.Third);w.WriteNumber("duplicateRetainedCount",src.Strokes[0].Points.Count);
    //All-identical input distinguishes raw endpoint3 from the incorrect
    //retained-count endpoint0 used by a reconstructed count of one.
    var tiny=LegacyFreshFirstResultOwnershipFactory.Prepare(ak,6120,[new InkStroke([new(0,0),new(0,0),new(0,0),new(0,0)])]);
    //The pinned Russian classifier now owns native00A9F0 degenerate features.
    //Their exact vectors/NN costs have a separate independent oracle in
    //LegacyRussianFirstDegenerate.Probe; here check the real owning entry point
    //without inventing a printable result for a point.
    var tinyOwned=LegacyFreshFirstOwnedRecognition.PrepareForOwnedFactory(ak,tiny,activations,table);
    if(!ReferenceEquals(tinyOwned.Factory,tiny)||tinyOwned.GroupScores.Count!=1)
        throw new Exception("Degenerate input lost its actual first factory/group.");
    w.WriteNumber("identicalClassifiedGroups",tinyOwned.GroupScores.Count);
    var tinyCuts=tiny.Frame.Row.GenerateRussianCuts();var tinyScope=new LegacyFirstRowPrimitiveScope(tiny.Frame.Row,tinyCuts,[4]);
    var whole=new[]{new LegacyInkRange(0,0,tinyCuts.Cuts.PreparedInk.StrokePointCounts.Span[0])};
    if(tiny.Frame.Row.OriginalSource.Strokes[0].Points.Count!=1||tinyScope.Evaluate(whole,1).EndpointFieldsAreZero)
        throw new Exception("Raw duplicate ending ownership collapsed to a retained-count zero endpoint.");
    w.WriteNumber("identicalRetainedCount",tiny.Frame.Row.OriginalSource.Strokes[0].Points.Count);
    w.WriteNumber("identicalEndingThird",tinyScope.EndKeys.Resolve(0,whole[0].EndPointExclusive-1).Key.Third);
    w.WriteNumber("passed",passed);w.WriteNumber("unsupported",unsupported);w.WriteNumber("multipleInputs",multiple);w.WriteNumber("guards",guards);w.WriteEndObject();
    void WritePoints(string name,IEnumerable<IReadOnlyList<InkPoint>> strokes){w.WriteStartArray(name);foreach(var s in strokes){w.WriteStartArray();foreach(var p in s){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
}
Console.WriteLine($"PASS owning main rawinput preparation supported={passed}, truthfulscope-refusals={unsupported}, multiple-descriptor inputs={multiple}, guards={guards}; samefactory/actualmasks/720/24B/24D/29B/265. No fulltext or accuracy claim.");
LegacyFreshMainOwnedInputPreparation Prepare(InkStroke[] raw)=>LegacyFreshMainOwnedInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,caseData,publication);
static InkStroke[] Word(float x,float y)=>[
 new(new InkPoint[]{new(x,y+90),new(x,y+90),new(x,y+10),new(x+25,y+60),new(x+50,y+10),new(x+50,y+90)}),
 new(new InkPoint[]{new(x+70,y+10),new(x+70,y+90),new(x+120,y+10),new(x+120,y+90)}),
 new(new InkPoint[]{new(x+140,y+90),new(x+140,y+10),new(x+180,y+10),new(x+190,y+20),new(x+190,y+40),new(x+180,y+50),new(x+140,y+50)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),
 new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported owning input accepted");}
