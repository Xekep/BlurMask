using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-raw.json capture.json");
var m=args[0];var bytes=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var m30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
float[][][] XY(IReadOnlyList<InkStroke> ink)=>ink.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
int[] K(LegacySourcePointKey k)=>[k.First,k.Second,k.Third];
int[] R(LegacyInkRange r)=>[r.StrokeIndex,r.StartPoint,r.EndPointExclusive];
int[][][] SR(IReadOnlyList<LegacyFirstResultSourceRange> rr)=>rr.Select(r=>new[]{K(r.First),K(r.Last)}).ToArray();
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{
 var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);
 var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
 var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
 return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));
}
LegacyEnglish119ParentPostprocessing Post(JsonElement item)
{var p=LegacyEnglish119ParentSubmission.Prepare(model,Ink(item.GetProperty("Raw")),item.GetProperty("Resolution").GetSingle(),m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication).Run();}
foreach(var item in inputs.RootElement.EnumerateArray())
{
 var post=Post(item);string name=item.GetProperty("Name").GetString()!;
 if(post.CopySnapshots()[0].CopyDescriptors().Length!=1)
 {Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post));Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post));refused.Add(name);continue;}
 Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post,new(true)));
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post);Guard(()=>LegacyEnglish119MainLocalCandidates.Prepare(final,table));Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post));
 Guard(()=>final.Run(new(true)));if(final.HasEntered||final.IsFaulted)throw new Exception("Pre-entry cancel mutated final owner.");final.Run();Guard(()=>final.Run());
 var before=post.CopySnapshots()[0];if(!before.CopyDescriptors()[0].SequenceEqual(final.CopyDescriptor())||!before.CopyGuides()[0].SequenceEqual(final.CopyGuide()))throw new Exception("Fresh finalizer changed actual bytes.");
 if(final.ExistingTextRecordCount!=0||final.ExistingUtf8SegmentCount!=0||final.ExistingControlCount!=0)throw new Exception("Fresh source identity changed.");
 Guard(()=>LegacyEnglish119MainLocalCandidates.Prepare(final,table,new(true)));var candidates=LegacyEnglish119MainLocalCandidates.Prepare(final,table);Guard(()=>LegacyEnglish119MainLocalCandidates.Prepare(final,table));
 Guard(()=>candidates.Run(new(true)));if(candidates.HasEntered)throw new Exception("Pre-entry candidate cancel mutated state.");
 if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(table.Values,out ArraySegment<float> borrowed))throw new Exception("Table memory control unavailable.");
 float savedTable=borrowed.Array![borrowed.Offset];borrowed.Array[borrowed.Offset]=savedTable+1;
 try{candidates.Run();}finally{borrowed.Array[borrowed.Offset]=savedTable;}Guard(()=>candidates.Run());
 var leakedDescriptor=final.CopyDescriptor();leakedDescriptor[0]^=0xff;if(final.CopyDescriptor()[0]==leakedDescriptor[0])throw new Exception("Descriptor alias.");
 var categories=candidates.ReadActualCategories();if(!System.Runtime.InteropServices.MemoryMarshal.TryGetArray(categories.FirstIds,out ArraySegment<int> ids))throw new Exception("Category control unavailable.");
 int originalId=ids.Array![ids.Offset];ids.Array[ids.Offset]=originalId+1;if(candidates.ReadActualCategories().FirstIds.Span[0]!=originalId)throw new Exception("Category alias.");
 var cut=candidates.CopyCutStrokes();var metas=cut.Select((st,s)=>st.Points.Select((p,i)=>D(("Key",K(candidates.ResolveCutPoint(s,i).Key)),("End",K(candidates.ResolveCutEndingMetadata(s,i).Key)))).ToArray()).ToArray();
 var parentRows=final.CopyRows();if(parentRows.Count!=before.Rows.Count||parentRows.Where((r,i)=>!r.CopyStorage().Span.SequenceEqual(before.Rows[i].CopyStorage().Span)||!r.Ranges.SequenceEqual(before.Rows[i].Ranges)).Any())throw new Exception("Source720 changed.");
 rows.Add(D(("Name",name),("Raw",item.GetProperty("Raw").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Physical",XY(final.CopyStrokes())),("Probabilities",final.CopyPhysicalProbabilities()),("InputMetadata",final.CopyStrokes().Select((st,si)=>st.Points.Select((p,pi)=>D(("Key",K(final.ResolvePoint(si,pi).Key)),("End",K(final.ResolveEndingMetadata(si,pi).Key)))).ToArray()).ToArray()),("Guide",Convert.ToHexString(final.CopyGuide())),("Descriptor",Convert.ToHexString(final.CopyDescriptor())),("SourceRows",parentRows.Select(r=>D(("Storage",Convert.ToBase64String(r.CopyStorage().Span)),("Ranges",SR(r.Ranges)))).ToArray()),("FinalTrace",final.CopyTrace()),("Trace",candidates.CopyTrace()),("Cut",XY(cut)),("CutRecords",candidates.CopyCutRecords().Select(p=>D(("X",p.X),("Y",p.Y),("DX",p.DeltaX),("DY",p.DeltaY),("Length",p.SegmentLength),("Tag",p.Tag),("Physical",p.StrokeIndex),("Original",p.OriginalPointIndex))).ToArray()),("Metadata",metas),("Markers",candidates.CopyMarkers().Select(x=>new[]{x.Kind,x.StrokeIndex,x.PointIndex}).ToArray()),("Atoms",candidates.CopyAtoms().Select(x=>R(x.Range)).ToArray()),("Seeds",candidates.CopySeeds().Select(x=>D(("Atom",x.SourceAtomIndex),("Range",R(x.Atom.Range)),("Deferred",x.Deferred),("Slot",x.OriginalInsertionSlot),("Displacement",(int)x.InsertionDisplacement),("Bounds",new[]{x.MinimumX,x.MinimumY,x.MaximumX,x.MaximumY}))).ToArray()),("Deferred",candidates.DeferredCount),("Groups",Enumerable.Range(0,candidates.GroupCount).Select(g=>candidates.CopyGroupRanges(g).Select(R).ToArray()).ToArray()),("Nodes",Enumerable.Range(0,candidates.NodeCount).Select(n=>candidates.CopyNode(n).Select((a,i)=>D(("Group",a.GroupId),("Span",a.Span),("Flags",a.Flags),("Start",a.StartContextId),("End",a.EndContextId),("Previous",a.PredecessorArcIndices.ToArray()),("Next",a.Successors.Select(s=>new[]{s.NodeOffset,s.ArcIndex}).ToArray()),("Source",SR(candidates.ResolveCandidate(n,i))))).ToArray()).ToArray()),("Removed",candidates.RemovedArcCount)));
}
if(rows.Count!=134||refused.Count!=7)throw new Exception("Missing genuine scope denominators.");
var failures=new List<object>();var firstInput=inputs.RootElement[0];
foreach(string stage in new[]{"enter","169C30:single","161D20:no-pair","15D360:fresh","15D920:fresh-identity","prior130:cleared"})
{
 var post=Post(firstInput);var final=LegacyEnglish119FinalizedSingleSource.Prepare(post);using var cts=new CancellationTokenSource();
 Guard(()=>final.RunPrepared(s=>{_=final.CopyTrace();if(s==stage)cts.Cancel();},cts.Token));
 if(!final.IsFaulted||final.IsComplete||!final.HasEntered)throw new Exception("Entered finalizer did not poison.");
 Guard(()=>final.Run());Guard(()=>LegacyEnglish119FinalizedSingleSource.Prepare(post));
 failures.Add(D(("Kind","final"),("Stage",stage),("Trace",final.CopyTrace()),("Latest",final.LastCompletedStage),("Physical",XY(final.CopyStrokes()))));
}
foreach(string stage in new[]{"enter","108:cuts","21B:deferred-seeds","21B:local-ranges","108:local-pruning"})
{
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(Post(firstInput)).Run();var c=LegacyEnglish119MainLocalCandidates.Prepare(final,table);using var cts=new CancellationTokenSource();
 Guard(()=>c.RunPrepared(s=>{_=c.CopyTrace();if(s==stage)cts.Cancel();},cts.Token));
 if(!c.IsFaulted||c.IsComplete||!c.HasEntered)throw new Exception("Entered local producer did not poison.");
 Guard(()=>c.Run());Guard(()=>LegacyEnglish119MainLocalCandidates.Prepare(final,table));
 failures.Add(D(("Kind","local"),("Stage",stage),("Trace",c.CopyTrace()),("Cut",XY(c.CopyCutStrokes())),("Nodes",c.NodeCount)));
}

//PREPARED work controls: no expected label or native model claim.
LegacyOrdinaryCursiveSeedResult BudgetSeeds(int n,int p)=>new(Enumerable.Range(0,n).Select(i=>new LegacyOrdinaryCursiveSeed(i,new(new(0,i%p,0),new(i%p,0,1)),false,i,0,i,0,i+1,1)).ToArray());
new LegacyOrdinaryGraphWorkBudget().PreflightSeeds(BudgetSeeds(4096,12),12);
Guard(()=>new LegacyOrdinaryGraphWorkBudget().PreflightSeeds(BudgetSeeds(4096,13),13));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(new(true)).PreflightSeeds(BudgetSeeds(1,1),1));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(maximumBytes:0).PreflightSeeds(BudgetSeeds(1,1),1));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(maximumEdges:0).InitialEdges(1));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(maximumEdges:3).MergedEdges(1,1));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(maximumRanges:0).InitialGroups(1));
Guard(()=>new LegacyOrdinaryGraphWorkBudget(maximumCompose:0).Composition(1));
var preparedSeeds=BudgetSeeds(3,1);var preparedGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(preparedSeeds);
Guard(()=>LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(preparedSeeds,preparedGraph,8,4096,null,default,new LegacyOrdinaryGraphWorkBudget(maximumEdges:0)));
Guard(()=>LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(preparedSeeds,preparedGraph,8,4096,null,default,new LegacyOrdinaryGraphWorkBudget(maximumCompose:0)));
var heavySeeds=new LegacyOrdinaryCursiveSeedResult(Enumerable.Range(0,12).Select(i=>new LegacyOrdinaryCursiveSeed(i,new(new(0,0,i),new(0,i,i+2)),i is 3 or 6 or 9,i,(sbyte)(i is 3 or 6 or 9?-1:0),i,0,i+1,1)).ToArray());
var heavyGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(heavySeeds);
var heavyBudget=new LegacyOrdinaryGraphWorkBudget();heavyBudget.PreflightSeeds(heavySeeds,1);
var boundedHeavy=LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(heavySeeds,heavyGraph,8,4096,null,default,heavyBudget);
var ordinaryHeavy=LegacyOrdinaryCursiveCandidateGraph.Generate(heavySeeds,heavyGraph,8,4096);
string GraphIdentity(LegacyOrdinaryCursiveCandidateGraph g)=>string.Join("|",Enumerable.Range(0,g.NodeCount).SelectMany(n=>g.GetNode(n).Select(a=>$"{n},{a.GroupId},{a.Span},{a.Flags},{a.StartContextId},{a.EndContextId}:"+string.Join(",",a.PredecessorArcIndices.ToArray())+":"+string.Join(";",a.Successors.Select(s=>$"{s.NodeOffset},{s.ArcIndex}")))))+"#"+string.Join(";",g.Groups.Select(g=>string.Join("/",g.Ranges.ToArray().Select(r=>$"{r.StrokeIndex},{r.StartPoint},{r.EndPointExclusive}"))));
if(GraphIdentity(boundedHeavy)!=GraphIdentity(ordinaryHeavy))throw new Exception("Bounded deferred graph changed old allocation identity.");
long initialHeavyEdges=Enumerable.Range(0,heavyGraph.NodeCount).Sum(n=>heavyGraph.GetNode(n).Sum(a=>(long)a.PredecessorArcIndices.Length+a.SuccessorArcIndices.Length));
Guard(()=>LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(heavySeeds,heavyGraph,8,4096,null,default,new LegacyOrdinaryGraphWorkBudget(maximumEdges:initialHeavyEdges)));
Guard(()=>LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(heavySeeds,heavyGraph,8,4096,null,default,new LegacyOrdinaryGraphWorkBudget(maximumRanges:12)));
long maximumCopy=LegacyEnglish119FinalInputCopyBudget.Preflight(4096,256,4096);
Guard(()=>LegacyEnglish119FinalInputCopyBudget.Preflight(4097,256,4096));Guard(()=>LegacyEnglish119FinalInputCopyBudget.Preflight(4096,257,4096));Guard(()=>LegacyEnglish119FinalInputCopyBudget.Preflight(4096,256,4097));
using var file=File.Create(args[2]);using var writer=new Utf8JsonWriter(file);Write(writer,D(("Rows",rows),("MultilineRefused",refused),("Guards",guards),("Failures",failures),("MaximumFinalInputCopyBytes",maximumCopy),("PreparedBudgetControls",D(("Scope","PREPARED; not genuine inference"),("AdmittedSeedContextUnionWork",20L*4096*12),("RefusedSeedContextUnionWork",20L*4096*13),("DeferredSeeds",heavySeeds.DeferredCount),("DeferredCandidates",boundedHeavy.CandidateCount),("InitialDeferredEdges",initialHeavyEdges),("DefaultPathExact",true)))));
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



