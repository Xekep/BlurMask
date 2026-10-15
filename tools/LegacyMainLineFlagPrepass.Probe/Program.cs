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
var spanOwner=new LegacyOwnedMainWordSpanPreparation(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]),sampled,mainContext,table);
var source=sampled;var graph=source.SourceRanges.Graph;var categories=source.Owner.TransitionProfile.Categories;
var geometry=new LegacyMainBlankTransitionGeometry(ak,source,table);
Throws<InvalidOperationException>(()=>{_=geometry.Source;});
Require(ReferenceEquals(geometry.MainLineSource,source)&&geometry.Threshold70==-.5f&&!geometry.RequireWholePhysicalStarts10c,"Actual main blank model/source ownership differs.");
var ordinary=source.CreatePrepared240TransitionGeometry(table);int gaps=0;
for(int node=0;node<graph.NodeCount;node++)for(ushort ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)
{
    var arc=graph.GetNode(node)[ordinal];int expected=0x4000000;
    if(node!=arc.Span)
    {
        int previous=node-arc.Span-1;float prefix=-float.MaxValue;
        foreach(int prior in arc.PredecessorArcIndices.Span)prefix=Math.Max(prefix,ordinary.GetPreparedState(previous,prior).PrefixMaximumX);
        var box=ordinary.GetPreparedState(node,ordinal).Bounds;
        if(prefix==-float.MaxValue)prefix=box.MaximumX;
        expected=OriginalGap(box.MinimumX,prefix,source.Owner.SourceHeight,-.5f,3,table.Values.Span);gaps++;
    }
    Require(geometry.ReadPreparedBlankCost(node,ordinal)==expected,"Independent multi-physical minX/prefix/guide gap differs.");
}
var span=spanOwner.Prepare(0,0);
var page=spanOwner.Read(span,8UL<<40,new(){Node=0,Capacity=512,ContextRoot=spanOwner.Contexts.ContainerIdentity});
var thresholds=LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734),out _);
var startup=new LegacyNativeOrdinaryWordStartup(graph,model.AcousticAlphabet,categories,spanOwner.Contexts,
    LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds,model.AcousticAlphabet),new(0x1122334455667788,0),new byte[80]);
var parents=new LegacyNativeWordRecordHeap(125);var parentRouter=new LegacyNativeWordHeapRouter([parents,new(0)],new byte[]{0,0},0,uint.MaxValue);
uint[] headers=spanOwner.DictionaryOwner.CopySourceHeaders();
var initial=startup.ProcessPage(page.Records.Span,0,0,0,span.Qualifier,new Dictionary<byte,uint>{[0]=headers[0],[1]=headers[1]},
    (node,arc)=>spanOwner.GetCosts(span,node,arc),parentRouter,0,false,0,false,false,null);
Require(initial.Succeeded&&parents.Count>0,"Full unfiltered owned initial source page produced no native parent.");
var parent=LegacyNativeWordRowMaterializer.Materialize(parents.GetSlotSnapshot(0).Span,0,1,1,1);
byte[] before=parent.Record.ToArray(),residue=new byte[80];BinaryPrimitives.WriteInt32LittleEndian(residue.AsSpan(52),0x4000000);
bool flag=(before[5]&128)!=0;
var prepass=new LegacyNativeMainWordFlagPrepass(ak,scorer,table,new(0x1122334455667788,0),residue);
byte[] storage=Enumerable.Range(0,128).Select(i=>(byte)(i*43+19)).ToArray();
var heap=new LegacyNativeWordRecordHeap(1,storage);var router=new LegacyNativeWordHeapRouter([heap,new(0)],new byte[]{0,0},0,uint.MaxValue);
ReadOnlyMemory<byte> Lookup(int node,ushort ordinal){Require(node==0&&ordinal==0,"Foreign parent lookup.");return parent.Record;}
Require(!prepass.Process(1,0,0,0,Lookup,headers,spanOwner.Contexts,router,flag,32,false).Applied,"Native flag polarity failed to skip.");
var prepassResult=prepass.Process(1,0,0,0,Lookup,headers,spanOwner.Contexts,router,!flag,32,false);
Require(prepassResult.Applied&&prepassResult.Qualified&&prepassResult.GeometryCalls>0&&prepassResult.AcousticCalls>0&&prepassResult.AcceptedAllocations>0,"Multi-physical count-symbol branch emitted no real native candidate.");
Require(parent.Record.Span.SequenceEqual(before),"Prepass modified its owned parent.");
var separation=LegacyCursiveSeparationProfile.ReadV106Node(ak,source.Owner.CategoryOwner.SamplerNodeOffset);
for(int i=0;i<heap.Count;i++)
{
    var slot=heap.GetSlotSnapshot(i).Span;ushort arc=BinaryPrimitives.ReadUInt16LittleEndian(slot[28..]);int group=graph.GetNode(1)[arc].GroupId;
    int sep=LegacyCursiveGroupSeparationCost.Evaluate(source.Cuts.PreparedInk,graph.Groups[group].Ranges.Span,source.Owner.SourceHeight,
        separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost);
    int delta=unchecked(geometry.ReadPreparedBlankCost(1,arc)+scores[group].Cache.Costs.Span[192]+sep);
    var expected=OriginalSlot(before,storage,arc,unchecked(BinaryPrimitives.ReadInt32LittleEndian(before.AsSpan(16))+delta),
        unchecked(BinaryPrimitives.ReadInt32LittleEndian(before.AsSpan(20))+delta));
    Require(slot.SequenceEqual(expected),"Independent full128 multi-physical blank/prepass record differs.");
}
Require(!prepass.Process(1,0,0,0,Lookup,headers,spanOwner.Contexts,router,!flag,1,false).Applied,"Knowledge distance equality changed.");
Throws<ArgumentException>(()=>new LegacyNativeMainWordFlagPrepass(ak,scorer,table,new(1,0),new byte[79]));
var canceled=new LegacyNativeMainWordFlagPrepass(ak,scorer,table,new(1,0),residue);
Throws<OperationCanceledException>(()=>canceled.Process(1,0,0,0,Lookup,headers,spanOwner.Contexts,router,!flag,32,false,new(true)));
Throws<InvalidOperationException>(()=>canceled.Process(1,0,0,0,Lookup,headers,spanOwner.Contexts,router,!flag,32,false));
using(var stream=File.Create(args[5]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteNumber("physicals",smoothed.Strokes.Count);w.WriteNumber("groups",graph.Groups.Count);w.WriteNumber("blankControls",gaps);
    w.WriteNumber("queries",page.ReturnedCount);w.WriteNumber("initialParents",parents.Count);w.WriteNumber("geometryCalls",prepassResult.GeometryCalls);
    w.WriteNumber("acousticCalls",prepassResult.AcousticCalls);w.WriteNumber("allocations",prepassResult.AcceptedAllocations);w.WriteString("slot128",Convert.ToHexString(heap.GetSlotSnapshot(0).Span));w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS {gaps} actual multi-physical blank gap controls, {page.ReturnedCount} fullquery records/{parents.Count} native parents, {prepassResult.GeometryCalls}/{prepassResult.AcousticCalls}/{prepassResult.AcceptedAllocations} kind5/full128 rows; flag/distance/cancellation/owner guards.");

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

static byte[] OriginalSlot(ReadOnlySpan<byte> parent, ReadOnlySpan<byte> storage,
    ushort currentArc, int total, int increment)
{
    byte[] expected = new byte[128];
    storage[120..128].CopyTo(expected.AsSpan(120));
    parent[..16].CopyTo(expected);
    expected[5] = (byte)((parent[5] & 0x7d) | 128 | ((parent[5] & 2) != 0 ? 2 : 0));
    expected[6] = (byte)(parent[6] & 0xfc);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(16), total);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(20), increment);
    parent[24..28].CopyTo(expected.AsSpan(24));
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(28), currentArc);
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(30), 0);
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(34), ushort.MaxValue);
    parent[36..39].CopyTo(expected.AsSpan(36));
    expected[39] = 10;
    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(40), ulong.MaxValue);
    parent[48..62].CopyTo(expected.AsSpan(48));
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(62), ushort.MaxValue);
    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(88), ulong.MaxValue);
    int anchor = BinaryPrimitives.ReadInt32LittleEndian(parent[56..]);
    Require(BinaryPrimitives.ReadUInt16LittleEndian(parent[60..]) is 0 or ushort.MaxValue && anchor == 0,
        "Original bounded root fixture unexpectedly needs foreign anchor rows.");
    int previous = anchor + ~unchecked((sbyte)parent[32]);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(104), previous);
    Require(previous == -1, "Original root tail fixture unexpectedly needs prior linguistic context.");
    return expected;
}

static int OriginalGap(float current, float prefix, float height, float threshold, float slope, ReadOnlySpan<float> table)
{
    float transformed = ((current - prefix) / height - threshold) * -slope;
    if (transformed > 4) return 0;
    float log = transformed;
    if (transformed >= -4) log = table[(int)((double)(transformed + 4f) * 255 + .5) >> 3];
    float value = -log * 8192f;
    if (value >= 0x4000000) return 0x4000000;
    uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(value));
    uint mantissa = (bits & 0x7fffff) | 0x800000;
    int shift = (int)(bits >> 23 & 255) - 127 - 23;
    if (value < .5f) return 0;
    return shift >= 0 ? checked((int)(mantissa << shift))
        : checked((int)((mantissa + (1u << (-shift - 1))) >> -shift));
}
