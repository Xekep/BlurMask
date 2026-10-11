using System.Diagnostics;
using System.Text.Json;
using InkRecognition;
if(args.Length is not (1 or 4))throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,passes=0;
foreach(var c in controls.RootElement.EnumerateArray())
{
 var graph=OwnGraph(c.GetProperty("n").GetInt32(),c.GetProperty("cap").GetInt32());int calls=0;
 var source=new LegacyPreparedDictionaryWorkingMap(3,0xc00,new ushort[]{0,1,2},[]);ushort[] slots=[65535,65535,2,65535,65535,65535];
 var session=new LegacyOrdinaryGraphAcousticPermissionSession(graph,source,slots,(g,t)=>
 {calls++;int[] values=[0x4000000,39999,40000,40001,-1,100,0x4000001];return new(Enumerable.Range(0,6).Select(s=>values[(g.GroupId*13+calls*5+s*11)%7]).ToArray(),1);});
 foreach(var op in c.GetProperty("ops").EnumerateArray())
 {
  var result=session.Evaluate(op.GetProperty("node").GetInt32(),op.GetProperty("scope").GetByte(),op.GetProperty("allow").GetBoolean());
  var expected=op.GetProperty("emitted");
  Require(result.Succeeded&&result.ComputedRows==op.GetProperty("new").GetInt32()&&result.ReusedRows==op.GetProperty("reused").GetInt32()&&calls==op.GetProperty("calls").GetInt32()&&
   result.PermissionResult.Permissions.Span.SequenceEqual(op.GetProperty("permission").EnumerateArray().Select(x=>x.GetByte()).ToArray())&&result.SelectedRows.Count==expected.GetArrayLength(),$"Lifecycle trace {cases},pass {passes} differs.");
  for(int i=0;i<result.SelectedRows.Count;i++)
  {
   var row=result.SelectedRows[i];var e=expected[i];Require(row.Position==new LegacyGraphGlyphPosition(e.GetProperty("node").GetInt32(),e.GetProperty("ordinal").GetInt32())&&row.GroupId==e.GetProperty("group").GetInt32()&&row.Recomputed==e.GetProperty("recomputed").GetBoolean()&&row.Costs.Span.SequenceEqual(e.GetProperty("costs").EnumerateArray().Select(x=>x.GetInt32()).ToArray()),"Cache identity/snapshot differs.");
  }
  passes++;
 }
 cases++;
}
var failureGraph=OwnGraph(2,2);var failureMap=new LegacyPreparedDictionaryWorkingMap(3,0xc00,new ushort[]{0,1,2},[]);ushort[] none=[65535,65535,65535,65535,65535,65535];
var failure=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>null);
var failed=failure.Evaluate(0,0,false);Require(!failed.Succeeded&&failed.PermissionResult.WorkingMap is null&&failed.PermissionResult.Permissions.IsEmpty,"Failed computation forwarded permissions.");Reject(()=>failure.Evaluate(0,0,true));
var canceled=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>new(new int[6],1));Reject(()=>canceled.Evaluate(0,0,false,new CancellationToken(true)));Reject(()=>canceled.Evaluate(0,0,true));
var badWidth=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>new(new int[5],1));Reject(()=>badWidth.Evaluate(0,0,false));
var badScale=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>new(new int[6],float.NaN));Reject(()=>badScale.Evaluate(0,0,false));
var badAnchor=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>new(new int[6],1));Reject(()=>badAnchor.Evaluate(2,0,false));Reject(()=>badAnchor.Evaluate(0,0,true));
Reject(()=>new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,new LegacyPreparedDictionaryWorkingMap(4094,0xc00,new ushort[]{0},[]),none,(g,t)=>new(new int[4097],1)));
var ownership=new LegacyOrdinaryGraphAcousticPermissionSession(failureGraph,failureMap,none,(g,t)=>new(new int[]{g.GroupId,40000,0x4000000,0,0,0},1));
var old=ownership.Evaluate(0,0,false);int[] snapshot=old.SelectedRows[0].Costs.ToArray();ownership.Evaluate(1,0,false);Require(old.SelectedRows[0].Costs.Span.SequenceEqual(snapshot),"Emitted row snapshot mutated when pool buffer was recycled.");
Console.WriteLine($"PASS {cases} independent lifecycle traces / {passes} native selector/pool passes; scope vs span, empty selection, computed/pool reuse, scoped invalidation, pooled pointer aliases, immutable row snapshots, failure/no-forward and cancellation guards.");
if(args.Length==1)return;
byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));
var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var dictionary=LegacyLinguisticSymbolMaskController.CreateDictionarySource(model.AcousticAlphabet,model.LexicalAlphabet);
var binding=new LegacyPreparedOrdinaryLinguisticMaskBinding(model.AcousticAlphabet,ak.AsSpan(99,16),[dictionary]);var masks=binding.Build(false,true,[],[],[],[]).Masks;
var workingSource=new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount,descriptor.Flags,dictionary.DictionaryMap.Span,[]);
var retained=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(model.AcousticAlphabet,ak.AsSpan(99,16));
var sourceProfile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak);var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,189379);
InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var printed=new[]{Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((70,10),(70,90),(120,10),(120,90)),Draw((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var late=printed.Append(Draw((10,0),(20,0))).ToArray();
foreach(var raw in new[]{printed,late})
{
 var timer=Stopwatch.StartNew();var options=new LegacyCursiveWordSearchOptions(90,80,float.NaN,prefix.RuntimeLanguageScale,MaximumScoredGroups:512,BeamWidth:128,MaximumExpandedChoices:10_000_000);
 var source=LegacyOrdinaryCursiveCutSource.Generate(raw,sourceProfile,90,80);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts,80,seedProfile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),model.CursiveSpanProfile.MaximumAtoms);
 // Only external cache policy remains explicit. Primitive boundary/probability fields are replaced by the owned source metadata producer.
 var policy=new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
 var producer=new LegacyPreparedOrdinaryGraphAcousticProducer(model,ak,source,raw.Select(s=>s.Points.Count).ToArray(),options,masks.Primary.Span,policy);
 var session=new LegacyOrdinaryGraphAcousticPermissionSession(graph,workingSource,retained,producer.Compute);
 var rows=new Dictionary<LegacyGraphGlyphPosition,LegacyOrdinaryGraphSelectedAcousticRow>();var maps=new Dictionary<LegacyGraphGlyphPosition,LegacyPreparedDictionaryWorkingMap>();
 int reused=0,selection=0;var rowSnapshots=new List<(LegacyOrdinaryGraphSelectedAcousticRow Row,int[] Snapshot)>();
 for(int node=0;node<graph.NodeCount;node++)
  foreach(byte span in graph.GetNode(node).Select(a=>a.Span).Distinct().Order())
  {
   var pass=session.Evaluate(node,span,false);Require(pass.Succeeded,"Actual native-selected row pass failed.");
   int before=producer.ClassifiedGroups;var repeat=session.Evaluate(node,span,false);
   Require(repeat.ComputedRows==0&&before==producer.ClassifiedGroups&&repeat.PermissionResult.Permissions.Span.SequenceEqual(pass.PermissionResult.Permissions.Span),"Immediate native cache pass recomputed NN or changed permissions.");
   selection+=pass.SelectedRows.Count;reused+=repeat.ReusedRows;
   foreach(var row in pass.SelectedRows){rows.Add(row.Position,row);maps.Add(row.Position,pass.PermissionResult.WorkingMap!);rowSnapshots.Add((row,row.Costs.ToArray()));}
  }
 Require(selection==graph.CandidateCount&&rows.Count==graph.CandidateCount&&producer.ClassifiedGroups>0,"Graph selector omitted or invented candidate rows.");
 foreach(var(row,rowCopy)in rowSnapshots)Require(row.Costs.Span.SequenceEqual(rowCopy),"Actual row snapshot changed after native pool reuse.");
 var lexical=new LegacyWordCandidateScorer(model.CreateResourceLanguageScorer(prefix.RuntimeLanguageScale));
 int? Terminal(int state,int cost){Require(model.WordIndex.TryGetWordId(state,out int id),"Nonterminal dictionary state.");var score=lexical.Score(id,[],0,false,true,false,cost,LegacyWordCandidateScorer.ImpossibleCost,false);return score.IsAccepted?(int)score.TotalCost:null;}
 var search=LegacyGraphWordSearch.Search(model.Lexicon,graph,(p,a,g)=>maps[p].MapAcousticCosts(rows[p].Costs.Span[..model.AcousticAlphabet.LabelCount],false,0),128,10_000_000,Terminal);
 foreach(var word in search.Matches)Require(model.WordIndex.TryGetWordId(word.TerminalState,out _),"Native-selected cost bridge yielded non-dictionary terminal.");
 timer.Stop();
 Console.WriteLine($"ACTUAL graph→selector→cache→permissions→map→lexical: deferred={seeds.DeferredCount},nodes={graph.NodeCount},arcs={graph.CandidateCount},computed={producer.ClassifiedGroups},immediateReuses={reused},context={producer.ContextWindowCount},words={search.Matches.Count},ms={timer.Elapsed.TotalMilliseconds:F2};top={string.Join(',',search.Matches.Take(5).Select(w=>string.Concat(w.Labels.Span.ToArray().Select(l=>model.LexicalAlphabet.TryGetText(l,out var text)?text:"?"))+":"+w.Cost))}. External cache policy and guide frame remain explicit; original beam, no native winner/accuracy claim.");
}
Console.WriteLine("PASS actual RU ordinary+deferred candidate graph derives span/kind/recompute/success and real source primitive scope without caller-provided row facts; final195 permissions/maps reach real native lexical totals.");
static LegacyOrdinaryCursiveCandidateGraph OwnGraph(int n,int cap)
{
 var raw=Enumerable.Range(0,n).Select(i=>new InkStroke(new[]{new InkPoint(i*100,10),new InkPoint(i*100+20,90)})).ToArray();var ink=LegacyCursivePreparedInk.Create(raw);
 var markers=Enumerable.Range(0,n).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,1,0,.5f),new byte[n],Enumerable.Repeat(-1,n).ToArray());
 return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),cap);
}
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or OperationCanceledException){return;}throw new Exception("Malformed/invalidated cache session accepted.");}
