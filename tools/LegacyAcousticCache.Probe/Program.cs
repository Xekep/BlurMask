using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 4))throw new ArgumentException("Usage: controls.json [acoustic.res lexical.res engine-profile.json]");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] ownAlphabet=Convert.FromBase64String(json.RootElement.GetProperty("alphabet").GetString()!);
var alphabet=LegacyInkAlphabet.Load(ownAlphabet,0x103);
var stroke=new LegacyInkStroke(new LegacyInkPoint[]{new(0,0),new(100,0)});
LegacyOrdinaryAcousticCacheContext Context(bool prune=false,bool whole=false,bool endpoints=false,float? probability=null,bool skip=true)=>
 new(prune,false,skip,whole,endpoints,probability,false,ReadOnlyMemory<ushort>.Empty);
var ordinary=new LegacyOrdinaryAcousticCacheProfile(1,0,0,.9f,.9f,0,85173,1,10);
int count=0;
foreach(var test in json.RootElement.GetProperty("cases").EnumerateArray())
{
 var profile=ordinary with{CandidateThreshold=test.GetProperty("threshold").GetInt32(),MinimumCandidates=test.GetProperty("minimum").GetInt32(),CandidateLimit=test.GetProperty("cap").GetInt32()};
 var cache=new LegacyOrdinaryAcousticClassCache(profile,alphabet,ownAlphabet);
 int[] raw=test.GetProperty("raw").EnumerateArray().Select(v=>v.GetInt32()).ToArray();byte[] mask=test.GetProperty("mask").EnumerateArray().Select(v=>v.GetByte()).ToArray();
 var result=cache.Build(raw,[stroke],mask,Context(test.GetProperty("prune").GetBoolean()),[]);
 Require(result.Costs.Span.SequenceEqual(test.GetProperty("costs").EnumerateArray().Select(v=>v.GetInt32()).ToArray()),$"Independent sorted-set cache differs at {count}.");
 Require(result.Candidates.Span.SequenceEqual(test.GetProperty("candidates").EnumerateArray().Select(v=>new LegacyAcousticCacheCandidate(v[0].GetUInt16(),v[1].GetInt32())).ToArray()),$"Independent shortlist differs at {count}.");count++;
}
int profiles=0;
foreach(var test in json.RootElement.GetProperty("profiles").EnumerateArray())
{
 byte[] bytes=Convert.FromBase64String(test.GetProperty("bytes").GetString()!);
 Require(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(bytes)==new LegacyOrdinaryAcousticCacheProfile(1,.25f,.5f,.9f,.8f,.75f,85173,1,10),"Own serialized profile differs.");
 int tail=test.GetProperty("tail").GetInt32();
 foreach(int cut in new[]{15,tail,tail+5,bytes.Length-20})Reject<InvalidDataException>(()=>LegacyOrdinaryAcousticCacheProfile.ReadV110Node(bytes[..cut]));
 if(profiles==0)
 {
  foreach(int relative in new[]{40,49,50,73})
  {
   byte[] unsupported=(byte[])bytes.Clone();
   if(relative==73)BinaryPrimitives.WriteSingleLittleEndian(unsupported.AsSpan(tail+relative),1);else unsupported[tail+relative]=1;
   Reject<InvalidDataException>(()=>LegacyOrdinaryAcousticCacheProfile.ReadV110Node(unsupported));
  }
 }
 profiles++;
}
const int impossible=0x4000000;
var actualCache=new LegacyOrdinaryAcousticClassCache(ordinary,alphabet,ownAlphabet);
int[] costs=Enumerable.Repeat(impossible,192).ToArray();byte[] enabled=new byte[192];
void Reset(){Array.Fill(costs,impossible);Array.Fill(enabled,(byte)0);}
int directions=0;
foreach(var test in json.RootElement.GetProperty("directions").EnumerateArray())
{
 Reset();costs[140]=test.GetProperty("base").GetInt32();foreach(int id in new[]{140,185,186})enabled[id]=1;
 var points=test.GetProperty("points").EnumerateArray().Select(p=>new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray();
 var direction=actualCache.Build(costs,[new LegacyInkStroke(points)],enabled,Context(),[]);
 Require(direction.Costs.Span[185]==test.GetProperty("expected")[0].GetInt32()&&direction.Costs.Span[186]==test.GetProperty("expected")[1].GetInt32(),$"Independent hypot/float/log direction differs at {directions}.");directions++;
}
Reset();costs[140]=1000;costs[178]=2000;foreach(int i in new[]{140,178,180,181,185,186})enabled[i]=1;
var synthetic=actualCache.Build(costs,[stroke],enabled,Context(),[]);
Require(synthetic.Costs.Span[140]==impossible&&synthetic.Costs.Span[178]==impossible&&synthetic.FallbackCost==10192,"Prototypes/fallback not moved.");
Require(synthetic.Costs.Span[180]==1000&&synthetic.Costs.Span[181]==1000&&synthetic.Costs.Span[185]==impossible&&synthetic.Costs.Span[186]==137,"Hypot direction correction differs (signed negative correction −863).");
Require(synthetic.Costs.Span[193]==impossible&&synthetic.Costs.Span[194]==impossible,"Extra cache entries must copy quotes, not dashes.");
Reset();costs[0]=20;enabled[0]=1;enabled[190]=1;
var dot=actualCache.Build(costs,[stroke],enabled,Context(whole:true,endpoints:true,skip:false),[]);
Require(dot.Costs.Span[190]==19,"Whole endpoint0 knowledge rule must target dot.");
Reset();costs[180]=30;costs[188]=40;costs[189]=50;foreach(int i in new[]{180,188,189,191})enabled[i]=1;
var quotes=actualCache.Build(costs,[stroke],enabled,Context(),[]);
Require(quotes.Costs.Span[193]==40&&quotes.Costs.Span[194]==50&&quotes.Costs.Span[191]==30,"Quote buffers or soft-hyphen tail copy differ.");
Reset();costs[0]=10;enabled[0]=1;var pairs=actualCache.Build(costs,[stroke],enabled,Context(),[new(0,1)]);
Require(pairs.Costs.Span[1]==11&&!pairs.Candidates.Span.ToArray().Any(v=>v.Symbol==1),"Explicit native pair fill must add1 without extending shortlist.");
Reset();enabled[184]=1;Reject<NotSupportedException>(()=>actualCache.Build(costs,[stroke],enabled,Context(whole:true),[]));
Require(actualCache.Build(costs,[stroke],enabled,Context(whole:true,probability:.5f),[]).Costs.Span[184]==0,"Explicit source probability/resource zero coefficient branch differs.");
Reject<ArgumentException>(()=>actualCache.Build(costs,[stroke],enabled,Context(whole:true,probability:float.NaN),[]));
Reject<NotSupportedException>(()=>actualCache.Build(costs,[stroke],enabled,Context() with{SkipOrdinaryKnowledgeRules=false,ShareRelatedFormMinimum=true},[]));
Reject<ArgumentException>(()=>actualCache.Build(costs,[stroke],enabled,Context(),[new(192,1)]));
byte[] badHeader=ownAlphabet[..16];BinaryPrimitives.WriteUInt16LittleEndian(badHeader,192);
Reject<InvalidDataException>(()=>new LegacyOrdinaryAcousticClassCache(ordinary,alphabet,badHeader));
Reject<OperationCanceledException>(()=>actualCache.Build(costs,[stroke],enabled,Context(),[],new CancellationToken(true)));
Reset();costs[0]=costs[1]=costs[2]=10;enabled[0]=enabled[1]=enabled[2]=1;
var tied=new LegacyOrdinaryAcousticClassCache(ordinary with{CandidateLimit=2},alphabet,ownAlphabet).Build(costs,[stroke],enabled,Context(prune:true),[]);
Require(tied.Candidates.Span.SequenceEqual(new LegacyAcousticCacheCandidate[]{new(1,10),new(0,10)})&&tied.Costs.Span[2]==impossible,"Native tie insertion/quick-sort identity differs.");
Console.WriteLine($"PASS {count} independent cache/shortlist + {directions} direction controls + {profiles} serialized profiles; primitive identities, signed corrections, fallback, dot, quotes, soft hyphen, explicit pair order, native tie identity, unsupported/malformed/missing-probability/cancellation checks.");
if(args.Length==1)return;
byte[] acoustic=File.ReadAllBytes(args[1]);var bundle=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var resourceProfile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734));
Require(resourceProfile==ordinary,"Actual Russian ordinary cache resource profile differs.");
var resourceCache=new LegacyOrdinaryAcousticClassCache(resourceProfile,bundle.AcousticAlphabet,acoustic.AsSpan(99));
byte[] actualMask=Enumerable.Repeat((byte)1,bundle.AcousticAlphabet.LabelCount).ToArray();
// This diagnostic explicitly disables the opaque-probability primitive. It is
// not a recovered default user scope or a claim that that primitive is absent.
if(bundle.AcousticAlphabet.TryGetLabel("\U000f0001",out ushort primitive))actualMask[primitive]=0;
var classifier=bundle.CreatePreparedGlyphClassifier();
InkStroke Ink(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var printed=new InkStroke[]{Ink((0,90),(0,10),(25,60),(50,10),(50,90)),Ink((70,10),(70,90),(120,10),(120,90)),Ink((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
foreach(var(scale,dx,dy)in new[]{(1f,0f,0f),(2f,-100f,300f)})
{
 var moved=printed.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray())).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(moved);
 for(int letter=0;letter<3;letter++)
  foreach(short boundary in new short[]{1024,-1024})
  {
   var ranges=new LegacyInkRange[]{new(letter,0,moved[letter].Points.Count)};float minX=moved[letter].Points.Min(p=>p.X);
   var scored=classifier.Classify(prepared,ranges,minX,90*scale+dy,80*scale,100,10,[],boundary,boundary,true,new byte[]{1,1,1,1,0,0,0},1);
   var final=resourceCache.Build(scored.IntegerCosts.Span,scored.Geometry.Strokes,actualMask,Context(prune:true),[]);
   string Text(int id)=>bundle.AcousticAlphabet.TryGetText((ushort)id,out string? text)?text:$"#{id}";
   string Tops(IEnumerable<(int Id,int Cost)> items)=>string.Join(", ",items.OrderBy(v=>v.Cost).Take(5).Select(v=>$"{Text(v.Id)}:{v.Cost}"));
   Console.WriteLine($"ACTUAL own printed {"МИР"[letter]} scale={scale} boundaries={boundary}/{boundary} weights={string.Join(';',scored.ResolvedWeights.ToArray().Take(4).Select(v=>v.ToString("R",System.Globalization.CultureInfo.InvariantCulture)))}: raw[{Tops(scored.IntegerCosts.ToArray().Select((v,i)=>(i,v)))}] cache[{Tops(final.Candidates.ToArray().Select(v=>((int)v.Symbol,v.Cost)))}]. Explicit neutral context; accuracy diagnostic.");
  }
}
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
static void Reject<T>(Action action)where T:Exception{try{action();throw new Exception($"Expected {typeof(T).Name}.");}catch(T){}}
