using System.Text.Json;
using InkRecognition;

if(args.Length!=5)throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");
var acoustic=File.ReadAllBytes(args[1]);
var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379);
Require(profile.Reorder && profile.InsertionShift==1 && profile.BackwardHeightTolerance==.3f && profile.InsertionFraction==.6f,"Actual RU21b reorder profile differs.");
using(var controls=JsonDocument.Parse(File.ReadAllBytes(args[0])))
{
 int count=0,deferred=0;
 foreach(var test in controls.RootElement.EnumerateArray())
 {
  var source=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  var prepared=LegacyCursivePreparedInk.Create(source);
  var markers=test.GetProperty("markers").EnumerateArray().Select(m=>new LegacyCursiveCutMarker(0,m[0].GetInt32(),m[1].GetInt32())).ToArray();
  var atoms=LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers);
  var cuts=new LegacyCursiveCutResult(prepared,markers,atoms);
  var output=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,test.GetProperty("height").GetSingle(),
      profile with{Reorder=test.GetProperty("reorder").GetBoolean()},new byte[source.Length],Enumerable.Repeat(-1,source.Length).ToArray());
  var expected=test.GetProperty("expected");
  Require(output.Seeds.Count==expected.GetArrayLength(),"Seed ownership count differs.");
  for(int i=0;i<output.Seeds.Count;i++)
  {
   var seed=output.Seeds[i];var reference=expected[i];
   Require(seed.SourceAtomIndex==reference[0].GetInt32() && seed.Deferred==reference[1].GetBoolean() &&
    seed.OriginalInsertionSlot==reference[2].GetInt32() && seed.InsertionDisplacement==reference[3].GetInt32(),$"Seed insertion/lineage differs in control{count} at{i}.");
   Require(seed.Atom==atoms[seed.SourceAtomIndex],"Seed changed prepared ownership.");
  }
  if(output.DeferredCount==0)Require(output.CreateNondeferredOrdinaryChain(8).NodeCount==atoms.Length,"Native ordinary seed bridge count differs.");
  else{deferred++;Unsupported(()=>output.CreateNondeferredOrdinaryChain(8));}
  count++;
 }
 Console.WriteLine($"PASS {count} independent seed-order controls; {deferred} deferred cases explicitly refuse incorrect chain expansion.");
}
var bundle=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var recognizer=new LegacyRussianCursiveWordRecognizer(bundle,relatedFormMode:0);
var cache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),bundle.AcousticAlphabet,acoustic.AsSpan(99,16));
var nativeScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),bundle.AcousticAlphabet);
var bridge=new LegacyCursiveFinalCacheGroupScorer(cache,nativeScorer,bundle.CreateCursiveCategoryMapper(1),
    LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    bundle.AcousticAlphabet,bundle.LexicalAlphabet,Enumerable.Repeat((byte)1,bundle.AcousticAlphabet.LabelCount).ToArray(),
    LegacyWideSymbolPairs.CreateUnrestricted(bundle.AcousticAlphabet),relatedFormMode:0);
InkStroke Stroke(params (float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var printed=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var joined=new[]{Stroke((0,90),(0,10),(15,90),(30,10),(45,90),(60,10),(75,90),(90,10),(105,90),(120,10),(135,90),(150,10),(165,90),(180,10))};
var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
LegacyOrdinaryAcousticCacheContext Scope(LegacyCursiveCandidateGroup group,LegacyPreparedGlyphClassification glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
foreach(var source in new[]{printed,joined})
{
 var native=recognizer.RecognizeNondeferredNativeSeedsWithFinalCache(source,options,profile,new byte[source.Length],Enumerable.Repeat(-1,source.Length).ToArray(),bridge,Scope,false,false,false);
 var diagnostic=recognizer.RecognizeSourceOrderWithFinalCache(source,options,bridge,Scope,false,false,false);
 Require(native.Seeds.DeferredCount==0 && native.Recognition.ClassifiedGroups>0 && native.Recognition.Words.Count>0,"Actual resource seed bridge was empty.");
 Require(native.Recognition.Words.Select(w=>(w.Text,w.Score.TotalCost)).SequenceEqual(diagnostic.Words.Select(w=>(w.Text,w.Score.TotalCost))),"Native nondeferred producer/consumer differs from direct ordinary chain.");
 Require(native.Seeds.Seeds.Select(s=>s.SourceAtomIndex).SequenceEqual(Enumerable.Range(0,native.Seeds.Seeds.Count)),"Actual nondeferred ownership reordered.");
 Console.WriteLine($"ACTUAL native seeds -> cut lattice -> neural180 -> final195 ->35 geometry -> lexical word: {native.Recognition.ClassifiedGroups} groups; top={string.Join(",",native.Recognition.Words.Take(5).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}. Original beam and explicit source scope; no accuracy claim.");
}
var late=printed.Append(Stroke((10,0),(20,0))).ToArray();
var lateCuts=LegacyCursiveCutProducer.GenerateRussianVersion106(late,90,80,0);
var lateSeeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(lateCuts,80,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray());
Require(lateSeeds.DeferredCount>0,"Actual late stroke did not activate native deferred seed policy.");
int lateClassifications=0;
Unsupported(()=>recognizer.RecognizeNondeferredNativeSeedsWithFinalCache(late,options,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray(),bridge,(g,c)=>{lateClassifications++;return Scope(g,c);},false,false,false));
Require(lateClassifications==0,"Unsupported deferred native graph reached recognition.");
var wrongFlags=new byte[late.Length];wrongFlags[0]=1;
Unsupported(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(lateCuts,80,profile,wrongFlags,Enumerable.Repeat(-1,late.Length).ToArray()));
var wrongBoundaries=Enumerable.Repeat(-1,late.Length).ToArray();wrongBoundaries[0]=1;
Unsupported(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(lateCuts,80,profile,new byte[late.Length],wrongBoundaries));
var invalid=acoustic.ToArray();invalid[189379+308]=1;Unsupported(()=>LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(invalid,189379));
var missing=new LegacyCursiveCutResult(lateCuts.PreparedInk,lateCuts.Markers,lateCuts.Atoms.Skip(1).ToArray());
Invalid(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(missing,80,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray()));
var malformed=lateCuts.Atoms.ToArray();malformed[0]=malformed[0] with{Marker=malformed[0].Marker with{PointIndex=1}};
Invalid(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(new(lateCuts.PreparedInk,lateCuts.Markers,malformed),80,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray()));
Invalid(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(lateCuts,float.NaN,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray()));
try{LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(lateCuts,80,profile,new byte[late.Length],Enumerable.Repeat(-1,late.Length).ToArray(),new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine("PASS real-resource nondeferred native seed producer/consumer and late-stroke refusal before classification.");
static void Require(bool okay,string message){if(!okay)throw new InvalidDataException(message);}
static void Unsupported(Action action){try{action();throw new Exception("Unsupported native graph accepted.");}catch(NotSupportedException){}}
static void Invalid(Action action){try{action();throw new Exception("Invalid native ownership accepted.");}catch(ArgumentException){}}
