using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=7)throw new ArgumentException("AK LK engine-profile separation-table case-data publication-data capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engineBytes=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,engineBytes);
using var engine=JsonDocument.Parse(engineBytes);var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var c=File.OpenRead(args[4]);var caseData=LegacyNativeCaseData.Load(c);using var p=File.OpenRead(args[5]);var publication=LegacyNativeTextPublicationData.Load(p);
var raw=new InkStroke(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var sampler=LegacyFreshSingleGlyphMainSampler.Prepare(ak,0,raw,0);var factory=sampler.Postprocessing.Input.FirstFactory;var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
var profile=LegacyFirstTransitionGeometryProfile.ReadSource(ak,6120);var mask=LegacyFreshFirstAcousticMaskPolicy.Create(ak,factory);
var firstPolicy=LegacyFreshFirstControllerPolicy.ReadFirstSource(ak,6120);var firstMask=mask.CopyMasks().Primary.ToArray();
var firstPairs=LegacyWideSymbolPairs.CreateUnrestricted(factory.InvocationPolicy.Binding.Alphabet);
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,12727),[0],[-1]);
var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(ak,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;
var firstScorer=LegacyFirstOrdinaryCacheScorer.Prepare(ak,6120,row,activations);var primitive=new LegacyFirstRowPrimitiveScope(row,cuts,[row.OriginalSource.Strokes[0].Points.Count]);
var firstScores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
for(int g=0;g<firstScores.Length;g++)
{
 var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
 firstScores[g]=firstScorer.Score(cuts,ranges,clip.Start,clip.End,15,firstPolicy.MaximumNormalizedStrokes,firstMask,glyph=>firstPolicy.CreateContext(primitive.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),firstPairs);
}
var plan=LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak,cuts,prepared,firstScores,table);var primary=plan.Run();var firstContext=LegacyFirstBlankContextPasses.Run(ak,factory,cuts,prepared,plan,table);
var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).MaterializeLiteralSymbols(ak,6120,cuts,graph,primary,firstContext,firstScores);
var sampled=sampler.ApplyFirstResult(word,prepared.SourceRanges,table);
var owner=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
if(!ReferenceEquals(owner.Contexts,owner.Session.Contexts)||!ReferenceEquals(owner.Graph,sampled.SourceRanges.Graph)||!ReferenceEquals(owner.Model,model)||!ReferenceEquals(owner.Symbols.Symbols.AcousticAlphabet,model.AcousticAlphabet)||!ReferenceEquals(owner.Symbols.Symbols.LexicalAlphabet,model.LexicalAlphabet)||owner.ContainerCaseMode90!=uint.MaxValue||owner.IgnoreCaseC4||owner.CaptureSideCosts2368)throw new Exception("Fresh case source ownership/configuration mismatch.");
if(!owner.LanguageRecords.IsForModel(model)||owner.Descriptor.Flags!=0xc0a||owner.SourceSummary.Flags!=2||!owner.CopySourceHeaders().SequenceEqual(new uint[]{0,0xc0a}))throw new Exception("Case source descriptor/order/language profile changed.");
var primaryMask=owner.CopyMasks().Primary.ToArray();var scorer=owner.CreateAcousticScorer(ak);var scores=Enumerable.Range(0,sampled.SourceRanges.Graph.Groups.Count).Select(g=>scorer.Score(g)).ToArray();
foreach(var scored in scores)for(int label=0;label<180;label++)if(primaryMask[label]==0&&(scored.Glyph.IntegerCosts.Span[label]!=0x4000000||scored.Cache.Costs.Span[label]!=0x4000000))throw new Exception("Owned case mask did not reach NN integer/cache channels.");
LegacyPreparedDictionaryQuery Q()=>new(){Node=0,Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity,Category18=77};
//Independent raw reference only. Production owner constructed ONE backend;
//this diagnostic unwrapped source is not installed in its controller/session.
var rawPage=owner.Symbols.Symbols.CreateQuerySource(model.Lexicon,owner.Descriptor.Flags).Read(Q(),false);
using var output=File.Create(args[6]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("source",Convert.ToHexString(rawPage.Records.Span));w.WriteStartArray("states");int emissions=0;
for(int state=0;state<28;state++)
{
 ulong tag=(0x83UL<<56)|((ulong)state<<58)|(0xa5UL<<48)|(0x89UL<<40);var actual=owner.Session.Read(tag,Q());
 if(actual.SelectedOrdinal!=0||actual.ExtraCosts.Length!=0)throw new Exception("Fresh native capture0 source query supplied side costs.");
 byte[] records=actual.Records.ToArray();for(int i=0;i<actual.ReturnedCount;i++){if(records[i*32+4]!=1)throw new Exception("Actual selected source1 stamp missing.");records[i*32+4]=0;}
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteString("records",Convert.ToHexString(records));w.WriteEndObject();emissions+=actual.ReturnedCount;
}
w.WriteEndArray();w.WriteNumber("firstGroups",firstScores.Length);w.WriteNumber("mainGroups",scores.Length);w.WriteNumber("firstSelectedCost",word.Selection.TotalCost);w.WriteBase64String("mask",primaryMask);w.WriteStartArray("metadata");
uint terminal=0;for(int i=0;i<rawPage.ReturnedCount;i++){uint node=BinaryPrimitives.ReadUInt32LittleEndian(rawPage.Records.Span[(i*32)..]);if(model.WordIndex.TryGetWordId((int)node,out _)){terminal=node;break;}}
foreach(bool start in new[]{false,true})for(int state=0;state<32;state++)foreach(string text in new[]{",",".","a","A"})
{
 model.AcousticAlphabet.TryGetLabel(text,out ushort label);byte[] bytes=owner.Session.ReadTerminal(terminal|(1UL<<32)|((ulong)state<<58)|(start?1UL<<40:0),label,7).ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes,0);
 w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("label",label);w.WriteBoolean("ignored",false);w.WriteBoolean("start",start);w.WriteString("bytes",Convert.ToHexString(bytes));w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("restarts");
for(int state=0;state<32;state++)
{
 ulong tag=terminal|(1UL<<32)|(4UL<<40)|(0x83UL<<56)|((ulong)state<<58);
 ulong restarted=owner.Session.ResolveRestart(tag);ulong expected=1UL|(1UL<<32)|(12UL<<40)|((ulong)state<<58);
 if(restarted!=expected)throw new Exception("Owned fresh native restart/case/source identity mismatch.");
 w.WriteStartArray();w.WriteNumberValue(tag);w.WriteNumberValue(restarted);w.WriteEndArray();
}
w.WriteEndArray();
//Finite-cost permissions come from actual NN final cache, not supplied success,
//mask wildcard or expected word. The caller here explicitly processes group0;
//native whole-controller group scheduling remains outside this composition.
float permissionScale=LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(ak.AsSpan(189734));
var result=owner.Session.BuildAndRefreshPermissions([new(scores[0].Cache.Costs,permissionScale,true,0,true,0,true)]);
if(!result.Succeeded||result.WorkingMap is null)throw new Exception("Actual cache permission production failed.");
var refreshed=owner.Session.Read(8UL<<40,Q());w.WriteNumber("refreshedCount",refreshed.ReturnedCount);w.WriteBase64String("permissions",result.Permissions.Span);
w.WriteStartArray("permissionCosts");foreach(int value in scores[0].Cache.Costs.Span)w.WriteNumberValue(value);w.WriteEndArray();w.WriteNumber("permissionScale",permissionScale);
w.WriteStartArray("workingMap");foreach(ushort value in result.WorkingMap.CopySelectedMap(false,0))w.WriteNumberValue(value);w.WriteEndArray();
w.WriteStartArray("contexts");foreach(var node in owner.Contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(node.Identity);w.WriteNumberValue(node.ParentLink);w.WriteNumberValue(node.Symbol);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
try{LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication,new(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
byte[] changed=lk.ToArray();changed[0]^=1;try{LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,changed,model,sampled,caseData,publication);throw new Exception("Foreign resource accepted.");}catch(InvalidDataException){}
Console.WriteLine($"PASS raw→ownedfirst {firstScores.Length}NNgroups→720/guide/265/main {scores.Length}NNgroups→fresh2B7/292 sharedbackend {emissions} case records/28states, mask/permission/cache/256metadata/32restarts; no explicit case defaults/costs/masks/expectedtext. Whole240scheduler/winner excluded.");
