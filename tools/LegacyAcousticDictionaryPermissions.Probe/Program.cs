using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using InkRecognition;

if(args.Length is not (1 or 5)) throw new ArgumentException("controls.json [RU acoustic.res RU lexical.res engine-profile.json separation-profile.json]");
using var control=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0;
foreach(var c in control.RootElement.EnumerateArray())
{
 var source=new LegacyPreparedDictionaryWorkingMap(c.GetProperty("count").GetInt32(),c.GetProperty("flags").GetUInt32(),U16(c,"original"),[]);
 var rows=c.GetProperty("rows").EnumerateArray().Select(v=>new LegacyPreparedAcousticPermissionGroup(
   v.GetProperty("costs").EnumerateArray().Select(x=>x.GetInt32()).ToArray(),v.GetProperty("scale").GetSingle(),
   v.GetProperty("recomputed").GetBoolean(),v.GetProperty("kind").GetByte(),v.GetProperty("present").GetBoolean(),v.GetProperty("bits").GetByte(),true)).ToArray();
 var actual=LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(source,U16(c,"reserved"),rows);
 Require(actual.Succeeded&&actual.Permissions.Span.SequenceEqual(Bytes(c,"permission"))&&actual.WorkingMap!.CopySelectedMap(false,0).AsSpan().SequenceEqual(U16(c,"working")),$"Permission/control {cases} differs.");
 Require(source.CopySelectedMap(false,0).AsSpan().SequenceEqual(U16(c,"original")),"Producer mutated source snapshot.");cases++;
}
var own=LegacyPackedLexicon.Load(OwnLexicon());var emptyNode=LegacyPackedLexicon.Load(OwnLexicon(true));int descriptorCases=0;
foreach(string name in new[]{"subword","x-subword-tail","SUBWORD","sub-word","","ru_RU-lk-text.lite.lex"})
foreach(uint? order in new uint?[]{null,1,2,3})
foreach(bool bound in new[]{false,true})
foreach(bool leaf in new[]{false,true})
{
 if(order is null&&bound)continue;
 var profile=new LegacyDictionaryResourcePrefix(name,-65536,32768,7,1.25f,110);
 var d=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(profile,leaf?emptyNode:own,order,bound);
 uint expected=(name.Contains("subword",StringComparison.Ordinal)&&!leaf?0x800u:0xc00u)+(bound?2u:0)+((order??0)*4)+(!leaf?0x20u:0);
 Require(d.Flags==expected&&d.Weight24==-1&&d.Weight28==.5f&&d.RuntimeLanguageScale==1.25f&&d.LanguageOrder==(order??0),"Fresh node1/name/order/fixed-point descriptor differs.");descriptorCases++;
}
Require(LegacyPreparedDictionarySourceDescriptorFactory.FindIdentifierSubstring("pre\0subword"u8,"subword"u8)==-1&&
 LegacyPreparedDictionarySourceDescriptorFactory.FindIdentifierSubstring("preSUBword"u8,"subword"u8)==-1&&
 LegacyPreparedDictionarySourceDescriptorFactory.FindIdentifierSubstring("xsubwordy"u8,"subword"u8)==1&&
 LegacyPreparedDictionarySourceDescriptorFactory.FindIdentifierSubstring("x"u8,""u8)==0,"Builtin byte substring service differs.");
foreach(bool explicitWeight in new[]{false,true})
{
 var bytes=OwnResource(explicitWeight);var p=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(bytes);
 Require(p.Identifier=="own-subword-dictionary"&&p.Weight60==(explicitWeight?-65536:32768)&&p.Weight64==(explicitWeight?196608:65536)&&p.RuntimeLanguageScale==.75f&&p.AlphabetPayloadOffset==(explicitWeight?118:110),"Scalar prefix/decoded name/weight format differs.");
 foreach(int offset in new[]{0,4,8,12,16,18,20,80}){var bad=bytes.ToArray();bad[offset]^=0x10;Reject(()=>LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(bad));}
}
var lang=new byte[94];Put32(lang,0,0x0101fe90);Put32(lang,4,86);Put32(lang,8,0x2d1);Put32(lang,12,0x01000000);Put16(lang,16,0x100);
for(uint order=1;order<=3;order++){Put32(lang,90,order);Require(LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lang)==order,"Language order differs.");}
Put32(lang,90,0);Reject(()=>LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lang));
var map=new LegacyPreparedDictionaryWorkingMap(3,0xc00,new ushort[]{0,1,2},[]);ushort[] reserved=[0,65535,65535,65535,65535,65535];
var row=new LegacyPreparedAcousticPermissionGroup(new int[]{0,40000,0x4000000},1,true,0,false,0,true);
var failed=LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row,row with{Succeeded=false}]);
Require(!failed.Succeeded&&failed.Permissions.IsEmpty&&failed.WorkingMap is null,"Failed selected row forwarded partial source68 permissions.");
Reject(()=>LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row],new CancellationToken(true)));
Reject(()=>LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row with{CostScale=float.NaN}]));
Reject(()=>LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row with{CostScale=60000}]));
Reject(()=>LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row with{GraphKind=3}]));
Reject(()=>LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(map,reserved,[row with{FinalCosts=new int[2]}]));
Reject(()=>LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(new("x",0,0,0,1,0),own,null,true));
Console.WriteLine($"PASS {cases} independent binary32 permission/immutable working-map controls; {descriptorCases} fresh source descriptors; exact byte service, resource prefix, n-gram order, stale-state ordering, failure/no-forward and guards.");
if(args.Length==1)return;

byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);
var bundle=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));
uint actualOrder=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));
var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,bundle.Lexicon,actualOrder,true);
Require(prefix.Identifier=="ru_RU-lk-text.lite.lex"&&prefix.Weight60==0&&prefix.Weight64==65536&&prefix.RuntimeLanguageScale==1&&actualOrder==2&&
 (descriptor.Flags&0x1e00)==0xc00,"Pinned RU prefix/order/category differs.");
var sourceTokens=LegacyLinguisticSymbolMaskController.CreateDictionarySource(bundle.AcousticAlphabet,bundle.LexicalAlphabet);
var preparedBinding=new LegacyPreparedOrdinaryLinguisticMaskBinding(bundle.AcousticAlphabet,ak.AsSpan(99,16),[sourceTokens]);
var masks=preparedBinding.Build(false,true,[],[],[],[]).Masks;
var initial=new LegacyPreparedDictionaryWorkingMap(bundle.AcousticAlphabet.LabelCount,descriptor.Flags,sourceTokens.DictionaryMap.Span,[]);
ushort[] slots=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(bundle.AcousticAlphabet,ak.AsSpan(99,16));
float scale=LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(ak.AsSpan(189734));
Require(slots.AsSpan(0,3).SequenceEqual(new ushort[]{182,180,181})&&scale==1,"Actual RU retained header/disabled-power scale differs.");
var finalCache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(ak.AsSpan(189734)),bundle.AcousticAlphabet,ak.AsSpan(99,16));
var nativeBridge=new LegacyCursiveFinalCacheGroupScorer(finalCache,
 LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734),out _),bundle.AcousticAlphabet),
 bundle.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(ak,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
 bundle.AcousticAlphabet,bundle.LexicalAlphabet,masks.Primary.Span,[],relatedFormMode:null);
var ingress=LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak);
var recognizer=new LegacyRussianCursiveWordRecognizer(bundle,relatedFormMode:null);
var terminalScorer=new LegacyWordCandidateScorer(bundle.CreateResourceLanguageScorer(prefix.RuntimeLanguageScale));
InkStroke Stroke(params (float X,float Y)[] pts)=>new(pts.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var fixtures=new[]{("own printed",new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))}),
 ("own joined",new[]{Stroke((0,90),(10,20),(20,90),(30,20),(40,90),(50,90),(60,20),(70,90),(80,20),(90,90),(100,90),(110,20),(120,90),(130,20),(140,90))})};
foreach(var(name,raw)in fixtures)
{
 var timer=Stopwatch.StartNew();var caches=new Dictionary<LegacyCursiveCandidateGroup,int[]>();
 // Prepared ordinary kind0/partial scope and callback-selected rows are explicit.
 // The ingress endpoint derives per-physical enclosure/cut probabilities; scalar NaN is intentionally unused.
 var result=recognizer.RecognizeOrdinarySourceWithFinalCache(raw,ingress,
  new LegacyCursiveWordSearchOptions(90,80,float.NaN,prefix.RuntimeLanguageScale,BeamWidth:128,MaximumExpandedChoices:10_000_000),nativeBridge,
  (g,glyph)=>{var scope=new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
    caches[g]=finalCache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,masks.Primary.Span,scope,[]).Costs.ToArray();return scope;},false,false,false);
 var rows=caches.Values.Select(cost=>new LegacyPreparedAcousticPermissionGroup(cost,scale,true,0,false,0,true)).ToArray();
 var permissions=LegacyPreparedAcousticDictionaryPermissions.BuildAndRefresh(initial,slots,rows);
 var working=permissions.WorkingMap!;Require(permissions.Succeeded&&caches.Count==result.ClassifiedGroups&&caches.Count>0,"Actual NN/cache rows not processed.");
 int? Terminal(int state,int cost){Require(bundle.WordIndex.TryGetWordId(state,out int id),"Nonterminal actual dictionary state.");var score=terminalScorer.Score(id,[],0,false,true,false,cost,LegacyWordCandidateScorer.ImpossibleCost,false);return score.IsAccepted?(int)score.TotalCost:null;}
 var search=LegacyVariableWordSearch.Search(bundle.Lexicon,result.Lattice,
  g=>caches.TryGetValue(g,out var costs)?working.MapAcousticCosts(costs.AsSpan(0,bundle.AcousticAlphabet.LabelCount),false,0):ReadOnlyMemory<LegacyIntegerLexicalCandidate>.Empty,
  128,10_000_000,Terminal);
 var selected=working.CopySelectedMap(false,0);
 foreach(var word in search.Matches)
 {Require(bundle.WordIndex.TryGetWordId(word.TerminalState,out _),"Actual map yielded non-dictionary word.");foreach(ushort label in word.Labels.Span)Require(selected[label]!=ushort.MaxValue,"Disallowed map label entered real lexical inference.");}
 timer.Stop();
 Console.WriteLine($"ACTUAL {name}: descriptor=0x{descriptor.Flags:x},nativeFreshNode1Children={bundle.Lexicon.GetChildCount(1)},reserved={string.Join(',',slots)},mask={masks.Primary.Span.ToArray().Count(x=>x!=0)},cached={caches.Count},permissionFinite={permissions.Permissions.Span.ToArray().Count(x=>(x&1)!=0)},words={search.Matches.Count},ms={timer.Elapsed.TotalMilliseconds:F2};top={string.Join(',',search.Matches.Take(5).Select(w=>string.Join("",w.Labels.Span.ToArray().Select(l=>bundle.LexicalAlphabet.TryGetText(l,out var t)?t:"?"))+":"+w.Cost))}. Prepared kind0/scope and original beam; explicit guide frame; no native acceptance/word accuracy claim.");
}
Console.WriteLine("PASS pinned RU resource name/order -> source descriptor, prepared source controller -> actual NN/final195 -> permissions/source68 -> working map -> real packed dictionary/native lexical totals. No label aliases or handwritten answer corrections.");

static ushort[] U16(JsonElement v,string k)=>v.GetProperty(k).EnumerateArray().Select(x=>x.GetUInt16()).ToArray();
static byte[] Bytes(JsonElement v,string k)=>v.GetProperty(k).EnumerateArray().Select(x=>x.GetByte()).ToArray();
static void Require(bool condition,string text){if(!condition)throw new InvalidDataException(text);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new InvalidDataException("Malformed/canceled request accepted.");}
static void Put32(byte[] b,int p,uint v)=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(p),v);
static void Put16(byte[] b,int p,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(p),v);
static byte[] OwnResource(bool explicitWeight)
{
 byte[] b=new byte[explicitWeight?118:110];Put32(b,0,0x0101fe90);Put32(b,4,(uint)b.Length-8);Put32(b,8,0x13b);Put32(b,12,0x03000000);
 Put16(b,16,0x301);Put16(b,18,0x100);Put32(b,20,0xe17557);Put32(b,80,0xffffffff);Put32(b,84,0x07000000);
 byte[] name=new byte[56];Encoding.UTF8.GetBytes("own-subword-dictionary").CopyTo(name,0);byte[] key=[0x14,0x12,0x19,0x77];for(int i=0;i<56;i++)b[24+i]=(byte)(name[i]^key[i&3]);
 int p=92;Put32(b,p,explicitWeight?0xff000000:32768);p+=4;if(explicitWeight){Put32(b,p,unchecked((uint)-65536));Put32(b,p+4,196608);p+=8;}
 Put32(b,p,7);Put32(b,p+4,BitConverter.SingleToUInt32Bits(.75f));Put32(b,p+8,0x2b);Put16(b,p+12,0x103);return b;
}
static byte[] OwnLexicon(bool leaf=false)
{
 using var m=new MemoryStream();using var w=new BinaryWriter(m);w.Write(0x0101fe90u);w.Write(0u);w.Write(0x2b4u);w.Write(0x01000000u);w.Write(0u);w.Write(7u);w.Write(2u);
 Packed(w,[0,0,5,2,4,1,5],3);w.Write((byte)3);Packed(w,leaf?[3,0,2,1,0,0,0]:[3,2,0,1,0,0,0],2);w.Write(3u);w.Flush();var b=m.ToArray();Put32(b,4,(uint)b.Length-8);return b;
}
static void Packed(BinaryWriter w,uint[] v,int width)
{
 w.Write(0x01000000u);w.Write((uint)v.Length);w.Write((byte)width);byte[] b=new byte[(v.Length*width+31)/32*4];for(int i=0;i<v.Length;i++)for(int k=0;k<width;k++)if((v[i]&(1u<<k))!=0)b[(i*width+k)/8]|=(byte)(1<<((i*width+k)%8));w.Write(b);
}
