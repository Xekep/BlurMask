using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length is not (2 or 5))throw new ArgumentException("Usage: LegacyOrdinaryRankedGroup.Probe ranked-controls.json old-cache-controls.json [ru-ak.res engine-profile.json separation-profile.json]");
using var fixtures=JsonDocument.Parse(File.ReadAllBytes(args[0]));using var own=JsonDocument.Parse(File.ReadAllBytes(args[1]));
byte[] alphabetBytes=Convert.FromBase64String(own.RootElement.GetProperty("alphabet").GetString()!);var alphabet=LegacyInkAlphabet.Load(alphabetBytes,0x103);
int controls=0;var ownedCosts=Enumerable.Repeat(0x4000000,alphabet.LabelCount+3).ToArray();
foreach(var fixture in fixtures.RootElement.EnumerateArray())
{
 int[] costs=fixture.GetProperty("costs").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 var candidates=costs.Select((v,i)=>new LegacyAcousticCacheCandidate((ushort)i,v)).ToArray();
 var cache=new LegacyOrdinaryAcousticClassCacheResult(ownedCosts,candidates,1234);
 var group=new LegacyOrdinaryRankedAcousticGroup(cache,alphabet,alphabetBytes.AsSpan(0,16),new[]{0,123,456},fixture.GetProperty("scale").GetSingle());
 var expected=fixture.GetProperty("weights").EnumerateArray().Select(v=>v.GetSingle()).ToArray();var bytes=fixture.GetProperty("bytes").EnumerateArray().Select(v=>v.GetByte()).ToArray();
 for(int i=0;i<costs.Length;i++)
 {
  float actual=group.GetNormalizedLikelihood((ushort)i);if(Math.Abs(actual-expected[i])>2e-7f||group.GetFirstControllerLikelihoodByte((ushort)i)!=bytes[i])throw new Exception("Independent top20 normalized likelihood/byte differs.");
  if(group.Query((ushort)i,ushort.MaxValue,0,false,false)!=costs[i]||group.Query((ushort)i,(ushort)i,0,false,false)!=costs[i]||group.Query((ushort)i,(ushort)i,0,true,false)!=(i<20?costs[i]:0x4000000))throw new Exception("Native ranked access/restrict20 differs.");
 }
 if(group.Query((ushort)alphabet.LabelCount,ushort.MaxValue,1,true,false)!=1357||group.GetNormalizedLikelihood(191)!=0||group.Query(191,ushort.MaxValue,0,false,false)!=0x4000000)throw new Exception("Fallback/missing ranked lookup differs.");
 if(group.Query(0,0,2,false,true)!=(int)(fixture.GetProperty("scale").GetSingle()*40000f)+456)throw new Exception("Primitive request/category cost differs.");
 if(costs.Length>0&&group.Query(190,0,1,false,false)!=costs[0]+123)throw new Exception("Explicit ordinal incorrectly verified symbol identity.");
 controls++;
}
ExpectInvalid(()=>new LegacyOrdinaryRankedAcousticGroup(new(ownedCosts,new LegacyAcousticCacheCandidate[]{new(0,2),new(0,3)},0),alphabet,alphabetBytes.AsSpan(0,16),new[]{0},1));
ExpectInvalid(()=>new LegacyOrdinaryRankedAcousticGroup(new(ownedCosts,new LegacyAcousticCacheCandidate[]{new(0,3),new(1,2)},0),alphabet,alphabetBytes.AsSpan(0,16),new[]{0},1));
ExpectInvalid(()=>new LegacyOrdinaryRankedAcousticGroup(new(ownedCosts,new LegacyAcousticCacheCandidate[]{new(0,2)},0),alphabet,alphabetBytes.AsSpan(0,16),new[]{0},0));
var bypassHeader=alphabetBytes.AsSpan(0,16).ToArray();
for(int slot=4;slot<=6;slot++)BinaryPrimitives.WriteUInt16LittleEndian(bypassHeader.AsSpan(slot*2),(ushort)(slot-1));
var bypassGroup=new LegacyOrdinaryRankedAcousticGroup(new(ownedCosts,new LegacyAcousticCacheCandidate[]{new(0,-1)},37),alphabet,bypassHeader,new[]{456},1);
for(ushort symbol=3;symbol<=5;symbol++)
    if(bypassGroup.Query(symbol,0,-1,false,false)!=-1||bypassGroup.Query(symbol,0,-1,false,true)!=40000)
        throw new Exception("Native first ranked header bypass changed negative cost/required geometry.");
if(bypassGroup.Query((ushort)alphabet.LabelCount,ushort.MaxValue,0,true,false)!=493)
    throw new Exception("Fallback didn't reach its ordinary geometry addition.");
using(var cancel=new CancellationTokenSource()){cancel.Cancel();try{new LegacyOrdinaryRankedAcousticGroup(new(ownedCosts,ReadOnlyMemory<LegacyAcousticCacheCandidate>.Empty,0),alphabet,alphabetBytes.AsSpan(0,16),new[]{0},1,cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}}
if(args.Length==5)
{
 byte[] resource=File.ReadAllBytes(args[2]);using var json=JsonDocument.Parse(File.ReadAllBytes(args[3]));var a=json.RootElement.GetProperty("activations");
 var activation=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4]));var profile=LegacyCursiveSeparationProfile.ReadV106Node(resource,11506);
 var category=new LegacyCursiveCategoryMapper(LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out _));
 InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var session=LegacyFreshFirstGlyphSession.Prepare(resource,6120,mir,activation);var counts=session.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();var mask=Enumerable.Repeat((byte)1,session.Scorer.Alphabet.LabelCount).ToArray();
 int checkedGlyphs=0;
 foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
 {
  var ranges=ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray();var score=session.Score(ranges,0,15,mask,[]);
  int geom=LegacyCursiveGroupSeparationCost.Evaluate(session.Cuts.Cuts.PreparedInk,ranges,session.Row.Frame.Line.Height,profile.GapOffset,profile.Slope,table.Values.Span,profile.MaximumCost);
  var group=new LegacyOrdinaryRankedAcousticGroup(score.Cache,session.Scorer.Alphabet,resource.AsSpan(session.Scorer.AlphabetOffset,16),Enumerable.Repeat(geom,category.CompositeCategoryCount).ToArray(),1);
  var independent=LegacyPreparedAcousticCandidateScorer.ForAlphabet(new(0x4000000,0x4000000,0x4000000),session.Scorer.Alphabet);
  for(int i=0;i<group.Entries.Length;i++)
  {
   ushort symbol=group.Entries.Span[i].Symbol;int mapped=category.GetSymbolCompositeCategory(symbol);if(mapped<0)continue;
   int ranked=group.Query(symbol,(ushort)i,mapped,false,false);
   // First-query also bypasses source header4/5/6; own actual shortlist here
   // contains none of those extra templates, so the two ordinary APIs agree.
   int expected=independent.Evaluate(score.Cache.Costs.Span,symbol,Enumerable.Repeat(geom,category.CompositeCategoryCount).ToArray(),mapped,0,0,0x4000000).TotalCost;
   if(ranked!=expected)throw new Exception("Actual ranked first query disagrees with direct ordinary cost on common branch.");
  }
  string Text(ushort id)=>session.Scorer.Alphabet.TryGetText(id,out string t)?t:"?";
  Console.WriteLine($"first23f ranked physical[{string.Join(',',ids)}], geom={geom}: {string.Join(',',group.Entries.ToArray().Take(4).Select(v=>$"{Text(v.Symbol)}:{v.Cost}/p{v.NormalizedLikelihood:F4}/b{group.GetFirstControllerLikelihoodByte(v.Symbol)}"))}");checkedGlyphs++;
 }
 Console.WriteLine($"PASS actual ownedfirstcontext/NN/cache->geometry/rankedquery/posterior {checkedGlyphs} own glyphs; explicit masks/pairs/prepared ranges, no wholetext winner.");
}
Console.WriteLine($"PASS {controls} independent float top20 likelihood + native ranked lookup, primitive/fallback/restrict20/byte controls; not acceptance confidence.");
static InkStroke Stroke(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException){return;}throw new Exception("Invalid ranked group accepted.");}
