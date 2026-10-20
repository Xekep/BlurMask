using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models controller-input.json capture.json");
var bytes=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var m30=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[1]));
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
var cases=doc.RootElement.GetProperty("Rows").EnumerateArray().Select((x,i)=>(Name:"actual-"+i,Raw:Ink(x.GetProperty("Raw")),Resolution:96f)).ToList();
InkStroke Zig(float x,float y,int n=12)=>new(Enumerable.Range(0,n).Select(i=>new InkPoint(x+3*i,y+(i%2)*20)).ToArray());
cases.Add(("multiline-reverse",new[]{Zig(120,1000),Zig(0,1000),Zig(120,0),Zig(0,0)},96));
cases.Add(("sole-flat",new[]{new InkStroke([new(0,0),new(100,0)])},0));
cases.Add(("all-flat-multiple",new[]{new InkStroke([new(0,0),new(100,0)]),new InkStroke([new(0,20),new(100,20)])},0));
for(int i=0;i<4;i++)
{
 var row=cases[i].Raw;cases.Add(("duplicate-"+i,row.Select(st=>new InkStroke(st.Points.Concat(new[]{st.Points[^1]}).ToArray())).ToArray(),96));
 cases.Add(("near-duplicate-"+i,row.Select(st=>new InkStroke(st.Points.Concat(new[]{new InkPoint(MathF.BitIncrement(st.Points[^1].X),st.Points[^1].Y)}).ToArray())).ToArray(),96));
}
cases.Add(("filtered-parent-gap",new[]{Zig(0,0),new InkStroke([new(0,2),new(300,2)]),Zig(70,0)},0));
cases.Add(("multiline-forward",new[]{Zig(0,0),Zig(120,0),Zig(0,1000),Zig(120,1000)},96));
using var cf=File.OpenRead(Path.Combine(args[0],"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(args[0],"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
cases.Add(("active-gap-14",Enumerable.Range(0,14).Select(i=>Zig(i*70,0)).ToArray(),96));
foreach(int dy in new[]{20,40,60})
{
 var left=cases[25].Raw[0];var right=new InkStroke(left.Points.Select(p=>new InkPoint(p.X+150,p.Y+dy)).ToArray());
 cases.Add(("filtered-slant-"+dy,new[]{left,new InkStroke([new(0,20),new(500,20)]),right},0));
}
int productionComparisons=0,failureComparisons=0;
var profileComparisons=new List<object>();var posts=new List<object>();var results=new List<object>();var refusals=new List<object>();int guards=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new Exception("Expected refusal.");}
float[][][] XY(IReadOnlyList<InkStroke> ink)=>ink.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
int[] K(LegacySourcePointKey k)=>new[]{k.First,k.Second,k.Third};
int[][][] R(IReadOnlyList<LegacyFirstResultSourceRange> ranges)=>ranges.Select(r=>new[]{K(r.First),K(r.Last)}).ToArray();
object ChildCapture(LegacyEnglish119ParentChild child,LegacyEnglish119PlainTextResult text)
{
 var prep=child.Submission.Preparation;var candidates=text.Factory.Candidates;
 return D(("DescriptorOrdinal",child.DescriptorOrdinal),("Map",child.CopyCurrentParentMap()),("SoleFlat",child.IsSoleFlat),("Raw",XY(child.Submission.CopyRawChildInput())),("Deduplicated",XY(prep.CopyOriginalStrokes())),("Cut",XY(candidates.CopyCutStrokes())),("Height",prep.Line.Height),("Map200",prep.CopyPointKeys200()),("CutSourceIndices",candidates.CopyCutRecords().Select(r=>r.OriginalPointIndex).ToArray()),("CutCounts",candidates.CopyCutCounts()),("Atoms",candidates.CopyAtoms().Select(a=>new[]{a.Range.StrokeIndex,a.Range.StartPoint,a.Range.EndPointExclusive}).ToArray()),("Nodes",Enumerable.Range(0,candidates.NodeCount).Select(n=>candidates.CopyNode(n).Select(a=>new[]{a.StartContextId,a.EndContextId,a.Span}).ToArray()).ToArray()),("Words",text.NativeWords.Select(w=>w.Published.Word.FlatReferences.ToArray()).ToArray()),("Owners",text.CopyGlyphOwners().Select(o=>D(("Key",o.Key),("PackedCount",o.PackedCount),("Records",Convert.ToBase64String(o.Records.Span)))).ToArray()));
}

LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{
 var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);
 var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
 var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
 return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));
}
LegacyEnglish119ParentSubmission Completed(InkStroke[] raw,float resolution)
{var p=LegacyEnglish119ParentSubmission.Prepare(model,raw,resolution,m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return p;}
object Image(LegacyEnglish119PostprocessingSnapshot x)=>D(("Stage",x.Stage),("Descriptors",x.CopyDescriptors().Select(Convert.ToHexString).ToArray()),("Guides",x.CopyGuides().Select(Convert.ToHexString).ToArray()),("Points",XY(x.CopyStrokes())),("Rows",x.Rows.Select(row=>D(("Storage",Convert.ToBase64String(row.CopyStorage().Span)),("Ranges",R(row.Ranges)))).ToArray()),("Keys",x.CopyPointKeys200()));
byte[] Enc(object value){using var buffer=new MemoryStream();using(var w=new Utf8JsonWriter(buffer)){Write(w,value);w.Flush();}return buffer.ToArray();}
void Equivalent(LegacyEnglish119ParentPostprocessing diagnostic,LegacyEnglish119ParentPostprocessing production)
{
 var a=diagnostic.CopySnapshots();var b=production.CopySnapshots();
 if(b.Count!=(a.Count==0?0:1)||a.Count>0&&!Enc(Image(a[^1])).SequenceEqual(Enc(Image(b[^1]))))throw new Exception("Production latest image differs from diagnostic prefix.");
 if(!diagnostic.CopyTrace().SequenceEqual(production.CopyTrace())||!diagnostic.CopyLineFits().SequenceEqual(production.CopyLineFits())||diagnostic.OriginalMetrics!=production.OriginalMetrics||diagnostic.HasEntered!=production.HasEntered||diagnostic.IsFaulted!=production.IsFaulted||diagnostic.IsComplete!=production.IsComplete)throw new Exception("Production scalar lifecycle differs.");
 if(diagnostic.Shear is not null){var x=diagnostic.Shear;var y=production.Shear!;if(x.Blend!=y.Blend||x.GapSearchShear!=y.GapSearchShear||x.DirectionShear!=y.DirectionShear||x.Shear!=y.Shear||x.GridIterations!=y.GridIterations)throw new Exception("Production shear differs.");}
 if(diagnostic.Gap is not null){var x=diagnostic.Gap;var y=production.Gap!;if(x.Blend!=y.Blend||x.Output.SourceGap!=y.Output.SourceGap||!x.Output.NormalizedGaps.SequenceEqual(y.Output.NormalizedGaps))throw new Exception("Production gap differs.");}
 if(diagnostic.IsComplete)foreach(var (stroke,si)in b[^1].CopyStrokes().Select((x,i)=>(x,i)))for(int pi=0;pi<stroke.Points.Count;pi++)
 {if(diagnostic.ResolveFinalPoint(si,pi)!=production.ResolveFinalPoint(si,pi)||diagnostic.ResolveFinalEndingMetadata(si,pi)!=production.ResolveFinalEndingMetadata(si,pi))throw new Exception("Production complete metadata differs.");}
}
foreach(var item in cases)
{
 
 LegacyEnglish119ParentSubmission p;
 try{p=LegacyEnglish119ParentSubmission.Prepare(model,item.Raw,item.Resolution,m30,m50);}
 catch(NotSupportedException e){refusals.Add(D(("Name",item.Name),("Reason",e.Message)));continue;}
 var texts=p.Children.Select(c=>Recognize(c.Submission)).ToArray();
 var before=p.CopyDescriptors();
 Guard(()=>p.Publish(texts,new(true)));if(p.IsFaulted||p.CopyRowPrefix().Count!=0)throw new Exception("Pre-entry cancel mutated parent.");
 var foreign=LegacyEnglish119ParentSubmission.Prepare(model,item.Raw,item.Resolution,m30,m50);
 Guard(()=>foreign.Publish(texts));
 p.Publish(texts);Guard(()=>p.Publish(texts));
 Guard(()=>LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication,new(true)));
 var post=LegacyEnglish119ParentPostprocessing.PrepareDiagnostic(p,caseData,publication);Guard(()=>LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication));
 Guard(()=>post.Run(new(true)));if(post.HasEntered||post.IsFaulted)throw new Exception("Pre-entry continuation cancel mutated owner.");
 post.Run();Guard(()=>post.Run());
 var snapshots=post.CopySnapshots();var last=snapshots[^1];
 var productionParent=Completed(item.Raw,item.Resolution);var production=LegacyEnglish119ParentPostprocessing.Prepare(productionParent,caseData,publication);
 if(production.CopySnapshots().Count!=0)throw new Exception("Production retained a pre-entry image.");production.Run();Equivalent(post,production);productionComparisons++;if(!post.IsComplete||post.IsFaulted||!ReferenceEquals(post.MainWire.Source,p.Model.ParentSource))throw new Exception("Actual source lost.");
 var mapped=last.CopyStrokes().Select((st,si)=>st.Points.Select((v,pi)=>D(("Key",K(post.ResolveFinalPoint(si,pi).Key)),("End",K(post.ResolveFinalEndingMetadata(si,pi).Key)))).ToArray()).ToArray();
 posts.Add(D(("Name",item.Name),("Metrics",new[]{post.OriginalMetrics.OrthogonalFraction,post.OriginalMetrics.CornerFraction}),("Snapshots",snapshots.Select(x=>D(("Stage",x.Stage),("Descriptors",x.CopyDescriptors().Select(Convert.ToHexString).ToArray()),("Guides",x.CopyGuides().Select(Convert.ToHexString).ToArray()),("Points",XY(x.CopyStrokes())),("Rows",x.Rows.Select(row=>D(("Storage",Convert.ToBase64String(row.CopyStorage().Span)),("Ranges",R(row.Ranges)))).ToArray()),("Keys",x.CopyPointKeys200()))).ToArray()),("Metadata",mapped),("Shear",new[]{post.Shear!.Blend,post.Shear.GapSearchShear,post.Shear.DirectionShear,post.Shear.Shear}),("Grid",post.Shear.GridIterations),("Gap",post.Gap!.Output.SourceGap),("Blend",post.Gap.Blend),("Gaps",post.Gap.Output.NormalizedGaps.ToArray()),("Wire",D(("CacheWidth",post.MainWire.CacheWidth),("PrefixEnd",post.MainWire.PrefixEnd),("SamplerOffset",post.MainWire.Sampler.Child.Offset),("Fields",post.MainWire.Fields.Select(f=>D(("Owner",f.Owner),("Name",f.Destination),("Offset",f.Offset),("Size",f.ElementSize),("Count",f.Count),("Bytes",f.ElementSize*f.Count<=4096?Convert.ToBase64String(f.CopyBytes()):null),("Sha256",Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(f.CopyBytes()))))).ToArray()),("Networks",post.MainWire.Networks.Select(n=>D(("Offset",n.Offset),("End",n.End),("Inputs",n.InputWidth),("Outputs",n.OutputWidth),("Sha256",n.Fingerprint))).ToArray()))),("Trace",post.CopyTrace())));
 if(item.Name=="actual-44")
 {
  var stage=post.CopySnapshots().Single(x=>x.Stage=="24D");var guide=stage.CopyGuides()[0];float height=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4));
  var enProfile=LegacyOrientationSmoothingProfile.ReadV102Node(bytes,182362);
  byte[] ruBytes=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));var ruProfile=LegacyOrientationSmoothingProfile.ReadV102Node(ruBytes,188108);
  var en=LegacyOrientationStrokePreparation.PrepareWithPointKeys(stage.CopyStrokes(),enProfile,height,post.OriginalMetrics);
  var ru=LegacyOrientationStrokePreparation.PrepareWithPointKeys(stage.CopyStrokes(),ruProfile,height,post.OriginalMetrics);
  if(en.SmoothingWeight==ru.SmoothingWeight)throw new Exception("Prepared distinct EN/RU265 extra branches not covered.");
  profileComparisons.Add(D(("Name",item.Name),("ENWeight",en.SmoothingWeight),("RUWeight",ru.SmoothingWeight),("ENPoints",XY(en.Strokes)),("RUPoints",XY(ru.Strokes)),("ENKeys",en.PointKeys200.ToArray()),("RUKeys",ru.PointKeys200.ToArray())));
 }
 var rawCopy=p.CopyRaw();if(rawCopy[0].Points is InkPoint[] writable)writable[0]=new(99999,99999);
 if(!p.CopyRaw()[0].Points.SequenceEqual(item.Raw[0].Points))throw new Exception("Raw owner escaped.");
 var current=p.CopyCurrentStrokes();var order=p.CopyCurrentOriginalPhysical();
 var meta=current.Select((st,si)=>st.Points.Select((v,pi)=>D(("Key",K(p.ResolveCurrentPoint(si,pi).Key)),("End",K(p.ResolveCurrentEndingMetadata(si,pi).Key)),("MetadataPointIndex",p.ResolveCurrentPoint(si,pi).MetadataPointIndex),("KeyRecordIndex",p.ResolveCurrentPoint(si,pi).KeyRecordIndex),("RemainderByte",p.ResolveCurrentPoint(si,pi).RemainderByte))).ToArray()).ToArray();
 var rows=p.CopyRowPrefix();var origin=p.CopyOriginPrefix();
 foreach(var o in origin)if(!ReferenceEquals(o.Text,texts[p.Children.ToList().IndexOf(o.Child)]))throw new Exception("Actual richresult provenance lost.");
 results.Add(D(("Name",item.Name),("Resolution",item.Resolution),("Raw",XY(p.CopyRaw())),("Retained",XY(p.CopyRetainedIngress())),("Split",XY(p.CopySplitStrokes())),("Current",XY(current)),("SourcePrimary",p.SourceScales.Primary),("SourceSecondary",p.SourceScales.Secondary),("Samples",p.CopyScaleSamples()),("InitialNeuralScale",p.InitialNeuralScale),("Groups",p.CopyProviderGroups()),("SplitOrder",p.CopySplitOriginalPhysical()),("Order",order),("Calls",p.CopyReceiverCalls().Select(c=>D(("DescriptorOrdinal",c.DescriptorOrdinal),("StartingNeuralScale",c.StartingNeuralScale),("EndingNeuralScale",c.EndingNeuralScale),("LocalPhysicalCount",c.LocalPhysicalCount),("ProviderGroupCount",c.ProviderGroupCount),("VerticalStripCount",c.VerticalStripCount))).ToArray()),("Metadata",meta),("DescriptorsBefore",before.Select(Convert.ToHexString).ToArray()),("Descriptors",p.CopyDescriptors().Select(Convert.ToHexString).ToArray()),("Child",p.Children.Select((c,i)=>ChildCapture(c,texts[i])).ToArray()),("Rows",rows.Select((r,i)=>D(("Descriptor",origin[i].Child.DescriptorOrdinal),("Word",origin[i].WordOrdinal),("Ordinal",origin[i].ReferenceOrdinal),("Reference",origin[i].Reference),("Flat",origin[i].SoleFlatOverride),("ChildRanges",R(origin[i].CopyChildRanges())),("Ranges",R(r.Ranges)),("Storage",Convert.ToBase64String(r.CopyStorage().Span)),("Labels",r.LabelCount))).ToArray()),("Trace",p.CopyTrace())));
}


var preparedFailures=new List<object>();
foreach(string point in new[]{"enter","24B:0","24A:assigned","24D","29B","265:0","post265"})
{
 var parent=LegacyEnglish119ParentSubmission.Prepare(model,cases[0].Raw,96,m30,m50);parent.Publish(parent.Children.Select(c=>Recognize(c.Submission)).ToArray());
 var post=LegacyEnglish119ParentPostprocessing.PrepareDiagnostic(parent,caseData,publication);using var cts=new CancellationTokenSource();
 Guard(()=>post.RunPrepared(stage=>{if(stage==point)cts.Cancel();},cts.Token));if(!post.IsFaulted||post.IsComplete)throw new Exception("Entered continuation not poisoned.");
 Guard(()=>post.Run());Guard(()=>LegacyEnglish119ParentPostprocessing.Prepare(parent,caseData,publication));
 var prodParent=Completed(cases[0].Raw,96);var prod=LegacyEnglish119ParentPostprocessing.Prepare(prodParent,caseData,publication);using var prodCts=new CancellationTokenSource();
 Guard(()=>prod.RunPrepared(stage=>{if(stage==point)prodCts.Cancel();},prodCts.Token));Equivalent(post,prod);failureComparisons++;
 Guard(()=>prod.Run());Guard(()=>LegacyEnglish119ParentPostprocessing.Prepare(prodParent,caseData,publication));
 preparedFailures.Add(D(("Point",point),("Trace",post.CopyTrace()),("Stages",post.CopySnapshots().Select(x=>x.Stage).ToArray())));
}
var notPublished=LegacyEnglish119ParentSubmission.Prepare(model,cases[0].Raw,96,m30,m50);Guard(()=>LegacyEnglish119ParentPostprocessing.Prepare(notPublished,caseData,publication));
var raceParent=LegacyEnglish119ParentSubmission.Prepare(model,cases[0].Raw,96,m30,m50);raceParent.Publish(raceParent.Children.Select(c=>Recognize(c.Submission)).ToArray());int winners=0,losers=0;LegacyEnglish119ParentPostprocessing? claimed=null;
Parallel.For(0,2,_=>{try{var candidate=LegacyEnglish119ParentPostprocessing.Prepare(raceParent,caseData,publication);Interlocked.Increment(ref winners);claimed=candidate;}catch(InvalidOperationException){Interlocked.Increment(ref losers);}});
if(winners!=1||losers!=1)throw new Exception("Reservation race not single-winner.");claimed!.Run();Guard(()=>claimed.Run());

var control=LegacyEnglish119ParentSubmission.Prepare(model,cases[0].Raw,96,m30,m50);control.Publish(control.Children.Select(c=>Recognize(c.Submission)).ToArray());
var row0=control.CopyRowPrefix()[0];var origin0=control.CopyOriginPrefix()[0];var desc=control.CopyDescriptors();
byte[][] Shape(int count){var d=desc.Select(x=>x.ToArray()).ToArray();BinaryPrimitives.WriteInt32LittleEndian(d[0].AsSpan(52),0);BinaryPrimitives.WriteInt32LittleEndian(d[0].AsSpan(56),count);return d;}
int itemCap=control.ValidatePreparedPostprocessingShape(Shape(256),Enumerable.Repeat(row0,256).ToArray(),Enumerable.Repeat(origin0,256).ToArray());
if(itemCap!=256)throw new Exception("Exact prepared256 point visits not admitted.");
Guard(()=>control.ValidatePreparedPostprocessingShape(Shape(257),Enumerable.Repeat(row0,257).ToArray(),Enumerable.Repeat(origin0,257).ToArray()));
Guard(()=>control.ValidatePreparedPostprocessingShape(desc,[row0],raceParent.CopyOriginPrefix()));
using(var cts=new CancellationTokenSource()){Guard(()=>control.ValidatePreparedPostprocessingShape(desc,new CancelRows([row0],cts),[origin0],cts.Token));}
var glyphOwner=origin0.Text.CopyGlyphOwners().Single(o=>o.Key==LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(origin0.Reference));var key0=LegacyFirstResultGlyphKey.Decode(origin0.Reference);

var filtered=LegacyEnglish119ParentSubmission.Prepare(model,cases.Single(c=>c.Name=="filtered-parent-gap").Raw,0,m30,m50);filtered.Publish(filtered.Children.Select(c=>Recognize(c.Submission)).ToArray());
var projected=filtered.CopyCurrentStrokes();var unchanged=filtered.CopyCurrentStrokes();var projectedRows=filtered.CopyRowPrefix().Select(LegacyFirstResultLineRow.FromFirstResult).ToArray();var fd=filtered.CopyDescriptors()[0];
byte[] preparedGuide=new byte[40];BinaryPrimitives.WriteSingleLittleEndian(preparedGuide,20);BinaryPrimitives.WriteSingleLittleEndian(preparedGuide.AsSpan(4),20);BinaryPrimitives.WriteSingleLittleEndian(preparedGuide.AsSpan(8),.2f);BinaryPrimitives.WriteSingleLittleEndian(preparedGuide.AsSpan(12),20);BinaryPrimitives.WriteSingleLittleEndian(preparedGuide.AsSpan(16),20);
var movedGuide=LegacyEnglish119ParentPostprocessing.DeskewDescriptor(projected,0,2,projectedRows,0,projectedRows.Length,preparedGuide,1e-6f);
if(projected[1].Points.SequenceEqual(unchanged[1].Points))throw new Exception("Prepared whole-descriptor projection skipped filtered physical.");
var preparedFiltered=D(("Before",XY(unchanged)),("After",XY(projected)),("InputGuide",Convert.ToHexString(preparedGuide)),("OutputGuide",Convert.ToHexString(movedGuide)),("RowsBefore",filtered.CopyRowPrefix().Select(x=>Convert.ToBase64String(x.CopyStorage().Span)).ToArray()),("RowsAfter",projectedRows.Select(x=>Convert.ToBase64String(x.CopyStorage().Span)).ToArray()));
var denseRow=LegacyFirstResultSourceRecord.Create(glyphOwner.Records.Span,key0.Alternative,Enumerable.Repeat(origin0.CopyChildRanges()[0],4096).ToArray(),origin0.Child.CopyCurrentParentMap(),control.CopyCurrentStrokes(),origin0.Text.Factory.Categories,model.SourceScoreExponent);
int rangeCap=control.ValidatePreparedPostprocessingShape(Shape(16),Enumerable.Repeat(denseRow,16).ToArray(),Enumerable.Repeat(origin0,16).ToArray());
if(rangeCap!=65536)throw new Exception("Exact prepared65536 ranges not admitted.");
Guard(()=>control.ValidatePreparedPostprocessingShape(Shape(17),Enumerable.Repeat(denseRow,17).ToArray(),Enumerable.Repeat(origin0,17).ToArray()));
//No large allocations: conservative shape arithmetic covers the maximum
//511 descriptor /256 ordinary /4096 row shape and rejects diagnostic retention.
var maximumBudget=LegacyEnglish119SnapshotCopyBudget.Preflight(511,256,256,4096,false);
if(maximumBudget.PeakRetainedBytes!=2*maximumBudget.MaximumImageBytes||maximumBudget.MaximumImages!=516||maximumBudget.MaximumCopyWorkBytes!=516*maximumBudget.MaximumImageBytes)throw new Exception("Maximum copy envelope arithmetic differs.");
Guard(()=>LegacyEnglish119SnapshotCopyBudget.Preflight(511,256,256,4096,true));
var diagnosticBoundary=LegacyEnglish119SnapshotCopyBudget.Preflight(511,72,256,4096,true);
if(diagnosticBoundary.MaximumCopyWorkBytes>LegacyEnglish119SnapshotCopyBudget.DiagnosticWorkLimit)throw new Exception("Diagnostic admitted over budget.");
Guard(()=>LegacyEnglish119SnapshotCopyBudget.Preflight(511,73,256,4096,true));
Guard(()=>LegacyEnglish119SnapshotCopyBudget.Preflight(int.MaxValue,int.MaxValue,int.MaxValue,int.MaxValue,true));
Guard(()=>LegacyEnglish119SnapshotCopyBudget.Preflight(512,256,256,4096,false));
Guard(()=>LegacyEnglish119SnapshotCopyBudget.Preflight(511,257,256,4096,false));
if(productionComparisons!=141||failureComparisons!=7)throw new Exception("Missing production/diagnostic comparisons.");
using var captureFile=File.Create(args[2]);using var writer=new Utf8JsonWriter(captureFile);Write(writer,D(("ProductionComparisons",productionComparisons),("FailureComparisons",failureComparisons),("MaximumCopyBudget",D(("ImageBytes",maximumBudget.MaximumImageBytes),("ProductionPeak",maximumBudget.PeakRetainedBytes),("CopyWork",maximumBudget.MaximumCopyWorkBytes),("Images",maximumBudget.MaximumImages),("DiagnosticBoundaryOrdinary",72),("DiagnosticBoundaryWork",diagnosticBoundary.MaximumCopyWorkBytes))),("Actual",results),("Post",posts),("Refused",refusals),("Guards",guards),("PreparedFailures",preparedFailures),("ReservationWinners",winners),("ReservationLosers",losers),("PreparedProfileComparisons",profileComparisons),("PreparedItemCap",256),("PreparedRangeCap",65536),("PreparedFilteredDeskew",preparedFiltered)));
static Dictionary<string,object?> D(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
static void Write(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
 case null:w.WriteNullValue();break;
 case string s:w.WriteStringValue(s);break;
 case bool b:w.WriteBooleanValue(b);break;
 case int n:w.WriteNumberValue(n);break;
 case uint n:w.WriteNumberValue(n);break;
 case long n:w.WriteNumberValue(n);break;
 case ulong n:w.WriteNumberValue(n);break;
 case byte n:w.WriteNumberValue(n);break;
 case short n:w.WriteNumberValue(n);break;
 case ushort n:w.WriteNumberValue(n);break;
 case float n:w.WriteNumberValue(n);break;
 case double n:w.WriteNumberValue(n);break;
 case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}



sealed class CancelRows(IReadOnlyList<LegacyFirstResultSourceRecord> values,CancellationTokenSource cts):IReadOnlyList<LegacyFirstResultSourceRecord>
{
 public int Count=>values.Count;
 public LegacyFirstResultSourceRecord this[int i]{get{cts.Cancel();return values[i];}}
 public IEnumerator<LegacyFirstResultSourceRecord> GetEnumerator()=>values.GetEnumerator();
 System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}

