using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=9)throw new ArgumentException("AK, engineprofile,separationprofile,line30,line50,auditoutput,case-data,publication-data,LK.");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[3]),line50=File.ReadAllBytes(args[4]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
var raw=new List<InkStroke>();
for(int i=0;i<14;i++)raw.Add(new InkStroke(new InkPoint[]{new(i*70,90),new(i*70,10),new(i*70+25,60),new(i*70+50,10),new(i*70+50,90)}));
var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,line30,line50,raw,0);
var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),line30,line50);
var invocation=LegacyFreshMainFirstResultLines.Prepare(ak,reordered);
var words=invocation.FirstSubmissions.Select(Build).ToArray();var result=invocation.ApplyFirstResults(words);
using var caseStream=File.OpenRead(args[6]);var caseData=LegacyNativeCaseData.Load(caseStream);
using var publicationStream=File.OpenRead(args[7]);var publication=LegacyNativeTextPublicationData.Load(publicationStream);
var shear=LegacyFreshMainShearPreparation.Prepare(ak,result,caseData,publication);
var active=LegacyFreshMainActiveGapPreparation.Prepare(ak,shear);
var smoothed=active.Smooth(ak);
var sampler=LegacyFreshMainSingleLineSampler.Prepare(ak,smoothed);
var sampled=sampler.Sample(table);
var mainContext=LegacyFreshMainSingleLineContext.Prepare(ak,sampled,activations);
var model=LegacyRussianModelBundle.Load(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]));
var dictionary=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,File.ReadAllBytes(args[8]),model,sampled);
var masks=dictionary.CopyMasks();var scorer=dictionary.CreateAcousticScorer(ak,mainContext);
Throws<InvalidOperationException>(()=>dictionary.CreateAcousticScorer(ak));
Require(mainContext.Windows.Count==36&&mainContext.Groups.Count>0,"Main14physical must evaluate nonempty actual189755 context windows.");
Console.WriteLine($"ACTUAL main14physical: {sampled.Cuts.PreparedInk.Points.Length} cutpoints/{sampled.SourceRanges.Graph.Groups.Count} groups, {mainContext.Windows.Count} context windows/{mainContext.Groups.Count} accepted.");
var scores=Enumerable.Range(0,sampled.SourceRanges.Graph.Groups.Count).Select(g=>scorer.Score(g)).ToArray();
Throws<OperationCanceledException>(()=>sampler.Sample(table,new(true)));
Throws<OperationCanceledException>(()=>LegacyFreshMainSingleLineContext.Prepare(ak,sampled,activations,new(true)));
Throws<OperationCanceledException>(()=>scorer.Score(0,new(true)));
Require(scorer.Policy.FrontendFlags==0x21400&&scorer.Policy.MaximumNormalizedStrokes==10&&sampler.SpanProfile.MaximumAtoms==8,"Actualmain caps/mode metadata.");
var spanOwner = CreateOwner();
var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,spanOwner);
Require(ReferenceEquals(spanOwner.SourceGraph,sampled.SourceRanges.Graph)&&ReferenceEquals(spanOwner.SourceInk,sampled.Cuts.PreparedInk)&&
    ReferenceEquals(spanOwner.MainLineSource,sampled)&&spanOwner.SourceHeight==40&&spanOwner.AcousticPolicy==scorer.Policy,
    "Common source view must preserve actual sampler/ink/policy ownership.");
Throws<InvalidOperationException>(()=>{_=spanOwner.Source;});
Throws<InvalidOperationException>(()=>{_=spanOwner.Acoustic;});
Require(controller.Configuration.PrimaryArcLimit230a==125&&controller.FlagPrepassDistance58==20&&controller.MaximumParentDistance54==120,
    "Main source scalar/controller policy changed for the multi-physical owner.");
var foreignSource=sampler.Sample(table);
Throws<ArgumentException>(()=>new LegacyOwnedMainWordSpanPreparation(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]),foreignSource,mainContext,table));
Throws<OperationCanceledException>(()=>new LegacyOwnedMainWordSpanPreparation(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]),sampled,mainContext,table,new(true)));
var graph=spanOwner.SourceGraph;
var reserved=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(model.AcousticAlphabet,ak.AsSpan(99,16));
var originalMap=spanOwner.DictionaryOwner.Symbols.CopyPrimary();
int steps=0,selectedRows=0,classes=0,queries=0,records=0;
LegacyOwnedMainWordSpan? firstSpan=null;
for(int node=0;node<graph.NodeCount;node++)
{
    var arcs=graph.GetNode(node);int extent=arcs.Count==0?0:arcs.Max(a=>(int)a.Span)+1;
    Require(spanOwner.GetSpanExtent(node)==extent,"Native actual main span extent differs.");
    for(byte span=0;span<extent;span++)
    {
        var prepared=spanOwner.Prepare(node,span);firstSpan??=prepared;
        var positions=Enumerable.Range(0,arcs.Count).Where(p=>arcs[p].Span==span).ToArray();
        var permissions=new byte[192];var eligibility=new byte[195];
        foreach(ushort symbol in reserved)if(symbol!=ushort.MaxValue)permissions[symbol]=3;
        foreach(int ordinal in positions)
        {
            var expected=scores[arcs[ordinal].GroupId].Cache.Costs.Span;
            var actual=spanOwner.GetCosts(prepared,node,(ushort)ordinal);
            Require(actual.ClassCosts.Span.SequenceEqual(expected),"Selected multi-physical cache changed actual4NN/context costs.");
            for(int label=0;label<195;label++)
            {
                if(expected[label]<0x4000000)eligibility[label]=1;
                if(label<192)
                {
                    if(expected[label]<0x4000000)permissions[label]|=3;
                    if(expected[label]<40000)permissions[label]|=4;
                }
            }
            selectedRows++;classes+=195;
        }
        Require(prepared.PermissionPass.SelectedPositions.Select(p=>p.ArcIndex).SequenceEqual(positions)&&
            prepared.PermissionPass.PermissionResult.Permissions.Span.SequenceEqual(permissions)&&
            prepared.Qualifier.GetEligibilitySnapshot().Span.SequenceEqual(eligibility),"Native span/permission/finite-union refresh differs.");
        var expectedMap=originalMap.Select(label=>label<192&&(permissions[label]&1)==0?ushort.MaxValue:label).ToArray();
        Require(prepared.PermissionPass.PermissionResult.WorkingMap!.CopySelectedMap(false,0).AsSpan().SequenceEqual(expectedMap),
            "Main source58/50 working map changed.");
        if(prepared.IsInitial)
        {
            var page=spanOwner.Read(prepared,8UL<<40,new(){Node=0,Capacity=512,ContextRoot=spanOwner.Contexts.ContainerIdentity});
            var range=model.Lexicon.GetChildRange(0);
            var expected=Enumerable.Range(range.Start+1,range.End-range.Start-1).Where(child=>expectedMap[model.Lexicon.GetLabel(child)]!=ushort.MaxValue).ToArray();
            Require(page.ReturnedCount==expected.Length,"Actual multi-physical initial source page ignored its refreshed permission map.");
            for(int i=0;i<expected.Length;i++)
            {
                var row=page.Records.Span.Slice(i*32,32);ushort label=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                Require(BinaryPrimitives.ReadInt32LittleEndian(row)==expected[i]&&row[4]==1&&label==expectedMap[model.Lexicon.GetLabel(expected[i])]&&
                    spanOwner.Contexts.ReadNode(BinaryPrimitives.ReadUInt64LittleEndian(row[8..])).Symbol==label,
                    "Real query source/context identities differ.");
            }
            records+=page.ReturnedCount;queries++;
        }
        steps++;
    }
}
var repeat=spanOwner.Prepare(graph.NodeCount-1,0);
Require(repeat.PermissionPass.ComputedRows==0&&repeat.PermissionPass.ReusedRows>0,"D0 repeated false failed to reuse owned rows.");
Throws<InvalidOperationException>(()=>spanOwner.GetCosts(firstSpan!,0,0));
var canceled=CreateOwner();Throws<OperationCanceledException>(()=>canceled.Prepare(0,0,new(true)));
Throws<InvalidOperationException>(()=>canceled.Prepare(0,0));
var cfg=controller.Configuration;int bufferCount=cfg.PrimaryArcLimit230a+(int)(cfg.ExpansionFactor230c*cfg.PrimaryArcLimit230a);
var nodeBuffer=new LegacyOwnedFreshMainWordNodeBuffer(controller,graph.NodeCount-1,new byte[bufferCount*88],new byte[cfg.PrimaryArcLimit230a*80]);
Require(nodeBuffer.SourceEnd23f0==125&&nodeBuffer.RetainedEnd23f8==125,"Common owner node allocation guard lost actual graph extent.");
nodeBuffer.BeginLanguage();
using(var stream=File.Create(args[5]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteNumber("steps",steps);w.WriteNumber("selectedRows",selectedRows);w.WriteNumber("classes",classes);
    w.WriteNumber("queries",queries);w.WriteNumber("records",records);w.WriteNumber("physicals",smoothed.Strokes.Count);
    w.WriteNumber("groups",graph.Groups.Count);w.WriteNumber("contextWindows",mainContext.Windows.Count);w.WriteNumber("acceptedContexts",mainContext.Groups.Count);
    w.WriteNumber("sourceHeight",spanOwner.SourceHeight);w.WriteNumber("budget",spanOwner.SourceRowBudget);w.WriteNumber("flags",spanOwner.AcousticPolicy.FrontendFlags);
    w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS typed main14physical: {steps} spans/{selectedRows} selected cache rows/{classes} classes, {queries} source queries/{records} real records; controller/node/common owner guards.");
LegacyOwnedMainWordSpanPreparation CreateOwner()=>new(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]),sampled,mainContext,table);

LegacyMainSelectedFirstWord Build(LegacyMainFirstSubmission submission)
{
    var factory=submission.Factory;var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
    var profile=LegacyFirstTransitionGeometryProfile.ReadSource(ak,6120);var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(ak,6120);
    var scope=new LegacyFirstRowPrimitiveScope(row,cuts,row.OriginalSource.Strokes.Select(s=>s.Points.Count).ToArray());
    var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(ak,6120,row,activations);byte[] preparedMask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
    int graphOffset=profile.SamplerOffset+8+BinaryPrimitives.ReadInt32LittleEndian(ak.AsSpan(profile.SamplerOffset+4));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,
        LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,graphOffset),new byte[row.OriginalSource.Strokes.Count],Enumerable.Repeat(-1,row.OriginalSource.Strokes.Count).ToArray());
    var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(ak,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;
    var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
    for(int g=0;g<scores.Length;g++)
    {
        var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
        var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
        scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),preparedMask,
            glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
    }
    var plan=LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak,cuts,prepared,scores,table);
    var primary=plan.Run();var context=plan.RunSupportedNonSkippedContext();
    var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).Materialize(cuts,graph,primary,context,scores);
    return new(submission.DescriptorOrdinal,word,prepared.SourceRanges);
}
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
