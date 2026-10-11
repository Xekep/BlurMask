using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 4 or 5))throw new ArgumentException("Usage: LegacyVariableWord.Probe controls.json [acoustic.res lexical.res engine-profile.json [separation-profile.json]]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));
var graph=LegacyPackedLexicon.Load(Convert.FromBase64String(controls.RootElement.GetProperty("graph").GetString()!));
LegacyCursiveCandidateLattice Lattice(int n,int cap)=>LegacyCursiveCandidateLattice.GenerateOrdinaryChain(
    Enumerable.Range(0,n).Select(i=>new LegacyCursiveInkAtom(new(1,0,i),new(0,i,i+2))).ToArray(),Enumerable.Repeat(-1,n).ToArray(),cap);

var wordKeys=new Dictionary<int,string>();
void Visit(int state,string prefix)
{
 if(graph.IsTerminal(state))wordKeys[state]=prefix;
 int[] children=new int[graph.GetChildCount(state)];graph.CopyChildren(state,children);
 foreach(int child in children)Visit(child,prefix+graph.GetLabel(child));
}
Visit(0,"");
int caseCount=0,matchCount=0;
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 var lattice=Lattice(c.GetProperty("n").GetInt32(),c.GetProperty("cap").GetInt32());
 var groups=c.GetProperty("groups").EnumerateArray().ToDictionary(g=>(g.GetProperty("first").GetInt32(),g.GetProperty("end").GetInt32()),
    g=>(ReadOnlyMemory<LegacyIntegerLexicalCandidate>)g.GetProperty("choices").EnumerateArray().Select(v=>new LegacyIntegerLexicalCandidate(v[0].GetUInt16(),v[1].GetInt32())).ToArray());
 int limit=c.GetProperty("limit").GetInt32();
 int? Terminal(int state,int cost){int total=cost+c.GetProperty("terminal").GetProperty(wordKeys[state]).GetInt32();return total<=limit?total:null;}
 var actual=LegacyVariableWordSearch.Search(graph,lattice,g=>groups[(g.FirstAtomIndex,g.NodeIndex)],256,2_000_000,Terminal);
 var expected=c.GetProperty("expected").EnumerateArray().ToArray();
 Require(actual.Matches.Count==expected.Length,"Exhaustive reference count differs.");
 for(int i=0;i<expected.Length;i++)
 {
  var result=actual.Matches[i];var reference=expected[i];
  Require(result.Labels.Span.SequenceEqual(reference.GetProperty("labels").EnumerateArray().Select(v=>v.GetUInt16()).ToArray())&&
    result.AcousticCost==reference.GetProperty("acoustic").GetInt32()&&result.Cost==reference.GetProperty("total").GetInt32(),"Exhaustive variable partition minimum differs.");
  int covered=0;foreach(var glyph in result.Glyphs){Require(glyph.FirstAtomIndex==covered,"Ownership has a gap/overlap.");covered=glyph.NodeIndex+1;}
  Require(covered==lattice.NodeCount,"Ownership does not cover all atoms.");
 }
 caseCount++;matchCount+=actual.Matches.Count;
}
// A dead intermediate cut must not stop a longer valid glyph.
var jumped=LegacyVariableWordSearch.Search(graph,Lattice(2,2),g=>g.FirstAtomIndex==0&&g.NodeIndex==1?new LegacyIntegerLexicalCandidate[]{new(1,7)}:[]);
Require(jumped.Matches.Count==1&&jumped.Matches[0].Glyphs.Count==1&&jumped.Matches[0].AcousticCost==7,"Long glyph did not cross dead intermediate cut.");
var reranked=LegacyVariableWordSearch.Search(graph,Lattice(1,1),g=>new LegacyIntegerLexicalCandidate[]{new(1,0),new(2,100)},1,100,
 (state,cost)=>cost+(graph.GetLabel(state)==1?1000:-200));
Require(reranked.Matches.Single().Labels.Span[0]==2&&reranked.Matches[0].Cost==-100,"Terminal scoring happened after final beam pruning.");
Reject(()=>LegacyVariableWordSearch.Search(graph,Lattice(1,1),g=>ReadOnlyMemory<LegacyIntegerLexicalCandidate>.Empty,0));
Reject(()=>LegacyVariableWordSearch.Search(graph,Lattice(1,1),g=>new LegacyIntegerLexicalCandidate[513]));
try{LegacyVariableWordSearch.Search(graph,Lattice(1,1),g=>new LegacyIntegerLexicalCandidate[]{new(1,0),new(2,0)},maximumExpandedChoices:1);throw new Exception("Expansion budget ignored.");}catch(InvalidOperationException e)when(e.Message.Contains("budget")){}
try{LegacyVariableWordSearch.Search(graph,Lattice(1,1),g=>ReadOnlyMemory<LegacyIntegerLexicalCandidate>.Empty,cancellationToken:new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {caseCount} exhaustive partition references / {matchCount} ranked words, complete ownership, signed costs, dead-cut crossing, terminal reranking, budgets, malformed inputs and cancellation.");
int boundaryCases=0;
foreach(var c in controls.RootElement.GetProperty("boundaries").EnumerateArray())
{
 var counts=c.GetProperty("counts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 var ranges=c.GetProperty("ranges").EnumerateArray().Select(v=>new LegacyInkRange(v[0].GetInt32(),v[1].GetInt32(),v[2].GetInt32())).ToArray();
 var actual=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,counts,c.GetProperty("kind").GetInt32(),c.GetProperty("suppressStart").GetBoolean(),c.GetProperty("suppressEnd").GetBoolean());
 var expected=c.GetProperty("expected");Require(actual.Start==expected[0].GetInt16()&&actual.End==expected[1].GetInt16(),"Native boundary sign policy differs.");boundaryCases++;
}
Require(LegacyCursiveGlyphBoundaryPolicy.Resolve([new(1,0,4),new(0,2,5)],[5,4],0,false,false)==new LegacyCursiveGlyphBoundarySigns(1024,1024),"Chronological endpoints were replaced with physical sort order.");
Reject(()=>LegacyCursiveGlyphBoundaryPolicy.Resolve([new(0,0,3)],[2],0,false,false));
Reject(()=>LegacyCursiveGlyphBoundaryPolicy.Resolve([],[],1,false,false));
Console.WriteLine($"PASS {boundaryCases} independent boundary sign controls, kind1 branch, explicit suppression, chronology, counts and malformed ownership.");
if(args.Length==1)return;
var bundle=LegacyRussianModelBundle.Load(File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var recognizer=new LegacyRussianCursiveWordRecognizer(bundle,relatedFormMode:0);
InkStroke Stroke(params (float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var printed=new InkStroke[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var joined=new InkStroke[]{Stroke((0,90),(10,20),(20,90),(30,20),(40,90),(50,90),(60,20),(70,90),(80,20),(90,90),(100,90),(110,20),(120,90),(130,20),(140,90))};
var subsetMapping = new LegacyAcousticLexicalMapping(bundle.AcousticAlphabet, bundle.LexicalAlphabet,
    bundle.CreateGlyphClassifier().OutputWidth, relatedFormMode: 0);
int subsetGroups = 0;
var subsetResult = recognizer.RecognizeSourceOrderDiagnostic(printed,
    new LegacyCursiveWordSearchOptions(90,80,0,1,IncludeBaselineFamilies:false),
    costAdapter: (group, glyph) =>
    {
        Require(glyph.Scores.FamilyProbabilities[0] is null && glyph.Scores.FamilyProbabilities[1] is null &&
            glyph.Scores.FamilyProbabilities[2] is not null && glyph.Scores.FamilyProbabilities[3] is not null &&
            glyph.ResolvedWeights.Span[0] == 0 && glyph.ResolvedWeights.Span[1] == 0 &&
            glyph.ResolvedWeights.Span[2] > 0 && glyph.ResolvedWeights.Span[3] > 0,
            "Two-family recognition activated unavailable baseline neural slots.");
        subsetGroups++;
        return subsetMapping.MapIntegerCosts(glyph.IntegerCosts.Span);
    });
Require(subsetGroups > 0 && subsetResult.ClassifiedGroups == subsetGroups, "Two-family word path did not evaluate real resources.");
Console.WriteLine("PASS actual temporal/spatial-only word recognition and inactive baseline-family weights.");
foreach(var(name,source)in new[]{("own printed MIR",printed),("own joined zigzag",joined)})
{
 var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumScoredGroups:512,MaximumExpandedChoices:10_000_000);
 var result=recognizer.RecognizeSourceOrderDiagnostic(source,options);
 Require(result.Lattice.CandidateCount>result.Lattice.NodeCount&&result.ClassifiedGroups>0,"Actual model did not evaluate variable groups.");
 if(source.Length>1)Require(result.ContextWindowCount>0,"Actual multi-stroke context was empty.");
 foreach(var word in result.Words)
 {
  Require(bundle.WordIndex.TryGetWordId(word.Match.Labels.Span,out int id)&&id==word.WordId,"Actual word ID differs from dictionary.");
  var independently=new LegacyWordCandidateScorer(bundle.CreateResourceLanguageScorer(1)).Score(id,[],0,false,true,false,word.Match.AcousticCost,LegacyWordCandidateScorer.ImpossibleCost,false);
  Require(independently==word.Score,"Actual terminal language components differ.");
 }
 foreach(var(scale,dx,dy)in new[]{(1f,-100f,300f),(2f,-100f,300f),(3f,200f,-100f)})
 {
  var transformed=source.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray())).ToArray();
  var moved=recognizer.RecognizeSourceOrderDiagnostic(transformed,options with{SourceBaseline=90*scale+dy,SourceHeight=80*scale});
  bool same=moved.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Score.TotalCost)).SequenceEqual(result.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Score.TotalCost)));
  
  Require(moved.Lattice.NodeCount==result.Lattice.NodeCount&&moved.ClassifiedGroups==result.ClassifiedGroups,"Actual frame transform changed the fixture group topology.");
  Require(moved.Words.Take(3).Select(w=>(w.Text,w.Match.AcousticCost,w.Score.TotalCost)).SequenceEqual(result.Words.Take(3).Select(w=>(w.Text,w.Match.AcousticCost,w.Score.TotalCost))),"Actual fixture top3 changed under tested frame transforms.");
  Console.WriteLine($"FRAME scale={scale} dx={dx} dy={dy}: atoms={moved.Lattice.NodeCount} candidates={moved.Lattice.CandidateCount}; allCostsIdentical={same}, top={string.Join(",",moved.Words.Take(3).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}.");
 }
 Console.WriteLine($"ACTUAL {name}: atoms={result.Lattice.NodeCount}, candidates={result.Lattice.CandidateCount}, evaluated={result.ClassifiedGroups}, context={result.ContextWindowCount}, words={result.Words.Count}; top={string.Join(", ",result.Words.Take(5).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}. Raw180 cache diagnostic; no cursive accuracy claim.");
}
// Real packed dictionary + variable cuts: exact known terminal identities and
// lexical totals, with a two-atom middle glyph competing against two letters.
ushort Label(string text)=>bundle.LexicalAlphabet.TryGetLabel(text,out ushort label)?label:throw new Exception("Missing actual symbol.");
ushort m=Label("\u043c"),iLabel=Label("\u0438"),r=Label("\u0440"),f=Label("\u0444");
var actualLattice=Lattice(4,2);
ReadOnlyMemory<LegacyIntegerLexicalCandidate> Known(LegacyCursiveCandidateGroup g)=>(g.FirstAtomIndex,g.NodeIndex) switch
{
 (0,0)=>new LegacyIntegerLexicalCandidate[]{new(m,10)},(1,2)=>new LegacyIntegerLexicalCandidate[]{new(iLabel,20)},
 (3,3)=>new LegacyIntegerLexicalCandidate[]{new(r,120),new(f,20)},_=>ReadOnlyMemory<LegacyIntegerLexicalCandidate>.Empty
};
var scorer=new LegacyWordCandidateScorer(bundle.CreateResourceLanguageScorer(1));
int? ActualTerminal(int state,int cost){Require(bundle.WordIndex.TryGetWordId(state,out int id),"Nonterminal actual path.");var score=scorer.Score(id,[],0,false,false,false,cost+7,LegacyWordCandidateScorer.ImpossibleCost,false);return score.IsAccepted?(int)score.TotalCost:null;}
var known=LegacyVariableWordSearch.Search(bundle.Lexicon,actualLattice,Known,1,100,ActualTerminal).Matches.Single();
Require(known.Labels.Span.SequenceEqual(new ushort[]{m,iLabel,r})&&known.AcousticCost==150&&known.Glyphs.Count==3&&known.Glyphs[1].Span==1,"Real dictionary variable partition/lexical beam1 winner differs.");
Require(bundle.WordIndex.TryGetWordId(known.TerminalState,out int knownId)&&knownId==198893,"Real variable word identity differs.");
Console.WriteLine("PASS actual resource end-to-end neural/context/variable-cut transport, frame topology/top3 checks and known MIR/MIF variable-partition whole-word reranking. Lower ranks can change; neural fixture outputs are diagnostic.");
if(args.Length==5)
{
 var tableBytes=File.ReadAllBytes(args[4]);var table=LegacyCursiveSeparationTable.Load(tableBytes);
 Require(table.Values.Length==256&&BitConverter.SingleToUInt32Bits(table.Values.Span[0])==0xc08094af&&BitConverter.SingleToUInt32Bits(table.Values.Span[^1])==0xbc94af19,"Exact static separation-table anchors differ.");
 try{LegacyCursiveSeparationTable.Load("{}"u8);throw new Exception("Bad table accepted.");}catch(InvalidDataException){}
 var acoustic=File.ReadAllBytes(args[1]);
 var cacheProfile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734));
 var finalCache=new LegacyOrdinaryAcousticClassCache(cacheProfile,bundle.AcousticAlphabet,acoustic.AsSpan(99,16));
 var finalScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),bundle.AcousticAlphabet);
 var nativeBridge=new LegacyCursiveFinalCacheGroupScorer(finalCache,finalScorer,bundle.CreateCursiveCategoryMapper(1),
    LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),table,bundle.AcousticAlphabet,bundle.LexicalAlphabet,
    Enumerable.Repeat((byte)1,bundle.AcousticAlphabet.LabelCount).ToArray(),[],relatedFormMode:0);
 foreach(var(name,source)in new[]{("own printed MIR",printed),("own joined zigzag",joined)})
 {
  var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
  int cached=0,nonzeroGeometry=0;
  ReadOnlyMemory<LegacyIntegerLexicalCandidate> Adapter(LegacyCursiveCandidateGroup group,LegacyPreparedGlyphClassification glyph)
  {
   // This control deliberately supplies a partial-group source scope, no
   // related-form native pairs, and no additional recognizer/prune hook.
   var scope=new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
   var result=nativeBridge.Score(glyph,scope,80,0,false,false,false);
   Require(result.Cache.Costs.Length==195&&result.CompositeCategoryCosts.Length==35,"Actual final cache/category widths differ.");
   if(result.CompositeCategoryCosts.Span[0]!=0)nonzeroGeometry++;
   Require(result.CompositeCategoryCosts.Span.ToArray().Distinct().Count()==1,"Ordinary category costs were not shared.");
   cached++;return result.Choices;
  }
  var result=recognizer.RecognizeSourceOrderDiagnostic(source,options,costAdapter:Adapter);
  Require(cached==result.ClassifiedGroups&&cached>0,"Actual final-cache bridge did not score neural groups.");
  if(source.Length>1)Require(nonzeroGeometry>0,"Actual multi-stroke grouping table was not used.");
  var complete=recognizer.RecognizeSourceOrderWithFinalCache(source,options,nativeBridge,
    (group,glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty),false,false,false);
  Require(complete.Words.Select(w=>(w.Text,w.Score.TotalCost)).SequenceEqual(result.Words.Select(w=>(w.Text,w.Score.TotalCost))),"Public complete final-cache endpoint differs from explicit composition.");
  Console.WriteLine($"FINAL-CACHE {name}: groups={cached}, nonzeroGeometry={nonzeroGeometry}, words={result.Words.Count}; top={string.Join(", ",result.Words.Take(5).Select(w=>$"{w.Text}:{w.Score.TotalCost}"))}. Explicit scope and original variable beam.");
 }
 Console.WriteLine("PASS actual raw180 -> final195 ->35category geometry -> native acoustic score -> variable dictionary/native lexical terminal pipeline. Native cache scope/flags remain explicit.");
}
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Malformed variable request accepted.");}catch(ArgumentException){}}
