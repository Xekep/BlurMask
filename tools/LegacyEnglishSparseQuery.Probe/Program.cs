using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("models output-directory");
string m=args[0],output=args[1];Directory.CreateDirectory(output);
byte[] ak=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res")),lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var source=LegacySource247WireLayout.Read(ak);var firstModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(ak,source,source.Children.First(c=>c.Kind==0x249));
var data=LegacyEnglishLkPackedData.Read(source,lk);var language=LegacyEnglishLkSparseLanguageData.Read(data,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var line30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var line50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var separation=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cfile=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pfile=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cfile);var publication=LegacyNativeTextPublicationData.Load(pfile);
//An original diagnostic drawing creates the real completed MAIN owner. No
//expected handwriting text, terminal or class is inferred from this drawing.
InkStroke[] input=[new(new InkPoint[]{new(0,0),new(7,40),new(14,0)})];
var parent=LegacyEnglish119ParentSubmission.Prepare(firstModel,input,96,line30,line50);
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var candidates=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,separation);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,separation);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,separation).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
parent.Publish(parent.Children.Select(c=>Recognize(c.Submission)).ToArray());
var post=LegacyEnglish119ParentPostprocessing.Prepare(parent,caseData,publication).Run();var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,separation).Run();
var masks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);
var query=LegacyEnglishSparseTerminalQuery.Create(masks);int guards=0,cancels=0,queryCancels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected strict refusal.");}
Guard(()=>LegacyEnglishSparseTerminalQuery.Create(null!));Guard(()=>LegacyEnglishSparseTerminalQuery.Create(masks,token:new(true)));
long budget=query.AdditionalLogicalAllocationBudget;Guard(()=>LegacyEnglishSparseTerminalQuery.Create(masks,budget-1));Guard(()=>LegacyEnglishSparseTerminalQuery.Create(masks,-1));
if(LegacyEnglishSparseTerminalQuery.Preflight(data,budget)!=budget||budget>16L*1024*1024)throw new Exception("Whole map/table envelope changed.");
foreach(string stage in new[]{"query:preflight","query:index","query:wordCodes","query:tables","query:publish"})
{using var cts=new CancellationTokenSource();int seen=0;Guard(()=>LegacyEnglishSparseTerminalQuery.CreatePrepared(masks,budget,s=>{if(s==stage){seen++;cts.Cancel();}},cts.Token));if(seen!=1)throw new Exception("Creation cancellation missing.");cancels++;}
using(var cts=new CancellationTokenSource()){int seen=0;Guard(()=>LegacyEnglishSparseTerminalQuery.CreatePrepared(masks,budget,s=>{if(s=="query:tables"&&++seen==3)cts.Cancel();},cts.Token));if(seen!=3)throw new Exception("Late table cancellation missing.");cancels++;}
var nodes=new int[260074];Array.Fill(nodes,-1);int terminals=0,nonterminal=-1;
foreach(int node in Enumerable.Range(0,data.Dictionary.NodeCount))
{
 if(!data.Dictionary.IsTerminal(node)){if(nonterminal<0)nonterminal=node;continue;}
 var terminal=query.ReadOrdinaryTerminal(node);if((uint)terminal.WordId>=260074||nodes[terminal.WordId]!=-1)throw new Exception("Terminal ordinals not bijective.");nodes[terminal.WordId]=node;terminals++;
}
if(terminals!=260074||nodes.Any(n=>n<0))throw new Exception("Whole actual terminal map missing.");
Guard(()=>query.ReadOrdinaryTerminal(nonterminal));Guard(()=>query.ReadOrdinaryTerminal(-1));Guard(()=>query.ReadOrdinaryTerminal(666547));Guard(()=>query.ReadOrdinaryTerminal(nodes[0],token:new(true)));
var terminal0=query.ReadOrdinaryTerminal(nodes[0]);var first=query.First(terminal0)??throw new Exception("Actual first numerical row absent.");
var other=LegacyEnglishSparseTerminalQuery.Create(masks,budget);Guard(()=>query.First(other.ReadOrdinaryTerminal(nodes[0])));Guard(()=>other.Continue(first,other.ReadOrdinaryTerminal(nodes[0])));Guard(()=>query.Continue(other.First(other.ReadOrdinaryTerminal(nodes[0]))!,terminal0));
Guard(()=>query.First(null!));Guard(()=>query.Continue(null!,terminal0));Guard(()=>query.First(terminal0,new(true)));Guard(()=>query.Continue(first,terminal0,new(true)));Guard(()=>first.GetHistory(-1));Guard(()=>first.GetHistory(first.HistoryCount));Guard(()=>first.CopyRecordOver(new byte[39]));Guard(()=>first.WriteRecord(new byte[41]));
Guard(()=>query.EvaluatePrepared(new[]{0,1,2},0,0,false));Guard(()=>query.EvaluatePrepared(new[]{-1},0,0,false));Guard(()=>query.EvaluatePrepared(new[]{1024},0,0,false));Guard(()=>query.EvaluatePrepared(new[]{0},-1,0,false));Guard(()=>query.EvaluatePrepared(new[]{0},1024,0,false));Guard(()=>query.EvaluatePrepared(new[]{0},0,-1,false));Guard(()=>query.EvaluatePrepared(new[]{0},0,260074,false));Guard(()=>query.EvaluatePrepared(new[]{0},0,0,false,new(true)));
byte[] Pattern(long index){byte[] b=new byte[40];for(int j=0;j<40;j++)b[j]=(byte)(index*17+j*29+0x5b);return b;}
byte[] RowBytes(LegacyEnglishSparseQueryRow row,long index){byte[] b=Pattern(index),before=b.ToArray();row.Write(b);for(int j=0;j<40;j++)if((row.WrittenByteMask&(1UL<<j))==0&&b[j]!=before[j])throw new Exception("Unwritten allocator residue changed.");return b;}
var stable=first.CopyRecordOver(Pattern(123));var leak=first.CopyRecordOver(Pattern(123));leak[0]^=1;if(!stable.SequenceEqual(first.CopyRecordOver(Pattern(123))))throw new Exception("Published numerical row aliases copy.");
var tokenCopy=terminal0.CopySource70Token();tokenCopy[4]^=1;if(terminal0.CopySource70Token()[4]==tokenCopy[4])throw new Exception("Terminal metadata aliases copy.");
foreach(string stage in new[]{"query:evaluate","query:publish"})foreach(bool continuation in new[]{false,true})
{using var cts=new CancellationTokenSource();int seen=0;Guard(()=>query.QueryPrepared(terminal0,continuation?first:null,s=>{if(s==stage){seen++;cts.Cancel();}},cts.Token));if(seen!=1||!stable.SequenceEqual(first.CopyRecordOver(Pattern(123))))throw new Exception("Canceled query published or mutated prior history.");queryCancels++;}
using var rawDigest=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long controls=0;var kinds=new long[5];var orders=new long[3];
int preparedWord0Cost=numeric.CopyWordCosts()[language.CopyFieldPayload("98.wordCodes")[0]];
using(var file=File.Create(Path.Combine(output,"prepared-records.bin")))
{
 void Check(int kind,ReadOnlySpan<int> history,int current,int? word=null)
 {
  int chosenWord=word??0;var row=query.EvaluatePrepared(history,current,chosenWord,false);file.Write(RowBytes(row,controls));
  int baseValue=row.Cost-(word.HasValue?0:preparedWord0Cost);
  //The baseline class-only digest deliberately omits the extra prepared word0
  //term outside BOS cases. Full40-byte rows retain the actual compiled word0.
  Span<byte> item=stackalloc byte[32];int[] values=[history.Length,history.Length>0?history[0]:0,history.Length>1?history[1]:0,current,word??-1,baseValue,row.MatchedOrder,row.HistoryCount];for(int i=0;i<8;i++)BinaryPrimitives.WriteInt32LittleEndian(item[(i*4)..],values[i]);rawDigest.AppendData(item);kinds[kind]++;orders[row.MatchedOrder-1]++;controls++;
 }
 for(int word=0;word<260074;word++)Check(0,new[]{0},language.GetPackedValue(LegacyEnglishSparsePackedField.WordClasses,word),word);
 int Off(int level,int index)=>language.GetPackedValue(level==0?LegacyEnglishSparsePackedField.Order1Offsets:LegacyEnglishSparsePackedField.Order2Offsets,index);
 int Label(int level,int index)=>language.GetPackedValue(level==0?LegacyEnglishSparsePackedField.Order2Labels:LegacyEnglishSparsePackedField.Order3Labels,index);
 int Missing(int level,int lo,int hi){int expected=0;for(int i=lo;i<hi;i++){int value=Label(level,i);if(value!=expected)return expected;expected++;}if(expected>=1024)throw new Exception("Missing-class control not available.");return expected;}
 //Exactly the independently frozen raw reference order: all actual bigram
 //prefixes, each row's trigrams and its first missing trigram class.
 for(int h=0;h<1024;h++)for(int j=Off(0,h);j<Off(0,h+1);j++)
 {int v=Label(0,j);Check(1,new[]{h},v);int lo=Off(1,j),hi=Off(1,j+1);for(int k=lo;k<hi;k++)Check(2,new[]{h,v},Label(1,k));Check(3,new[]{h,v},Missing(1,lo,hi));}
 for(int h=0;h<1024;h++)Check(4,new[]{h},Missing(0,Off(0,h),Off(0,h+1)));
}
string digest=Convert.ToHexString(rawDigest.GetHashAndReset()).ToLowerInvariant();if(controls!=643868||digest!="1cad8793f3a43641a912f581be810347aa1b0bba066cc0f86fba7b820f223ebe")throw new Exception("Independent raw reference order/numerics changed.");
long publicFirst=0,publicContinuation=0;using(var file=File.Create(Path.Combine(output,"lexical-first-records.bin")))
{
 byte[] header=new byte[12];
 for(int word=0;word<260074;word++)
 {
  var t=query.ReadOrdinaryTerminal(nodes[word],(word&1)!=0);var row=query.First(t)??throw new Exception("Actual admitted first row missing.");BinaryPrimitives.WriteInt32LittleEndian(header,nodes[word]);BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4),word);BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(8),t.Boundary?1:0);file.Write(header);file.Write(t.CopySource70Token());file.Write(row.CopyRecordOver(Pattern(publicFirst)));publicFirst++;
 }
}
using(var file=File.Create(Path.Combine(output,"lexical-continuation-records.bin")))
{
 byte[] header=new byte[12];
 var previous=first;for(int i=0;i<4096;i++)
 {int word=(i*61+7)%260074;var t=query.ReadOrdinaryTerminal(nodes[word],(i&1)!=0);var result=query.Continue(previous,t)??throw new Exception("Actual continuation numerical row missing.");BinaryPrimitives.WriteInt32LittleEndian(header,nodes[word]);BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4),word);BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(8),t.Boundary?1:0);file.Write(header);file.Write(t.CopySource70Token());file.Write(result.CopyRecordOver(Pattern(i)));previous=result;publicContinuation++;}
}
var preparedEmpty=query.EvaluatePrepared([],0,0,false);if(preparedEmpty.HistoryCount!=1||preparedEmpty.MatchedOrder!=1)throw new Exception("Prepared empty-history unigram contract changed.");var emptyBytes=RowBytes(preparedEmpty,73);
Parallel.For(0,64,i=>{var previous=first;for(int j=0;j<64;j++){int word=(i*131+j*61+7)%260074;previous=query.Continue(previous,query.ReadOrdinaryTerminal(nodes[word],(j&1)!=0))??throw new Exception("Concurrent history result absent.");}if(first.HistoryCount!=2||first.GetHistory(0)!=0)throw new Exception("Parallel calls mutated previous history.");});
if(!stable.SequenceEqual(first.CopyRecordOver(Pattern(123))))throw new Exception("Canceled/concurrent query mutated completed history.");
var retry=LegacyEnglishSparseTerminalQuery.Create(masks,budget);if(!retry.First(retry.ReadOrdinaryTerminal(nodes[0]))!.CopyRecordOver(Pattern(123)).SequenceEqual(stable))throw new Exception("Canceled preparation poisoned retry.");
var files=new Dictionary<string,string>();foreach(string name in new[]{"prepared-records.bin","lexical-first-records.bin","lexical-continuation-records.bin"}){using var f=File.OpenRead(Path.Combine(output,name));files[name]=Convert.ToHexString(SHA256.HashData(f)).ToLowerInvariant();}
using(var file=File.Create(Path.Combine(output,"capture.json")))using(var w=new Utf8JsonWriter(file))
{
 w.WriteStartObject();w.WriteString("Scope",query.Policy);w.WriteNumber("AdditionalLogicalAllocationBytes",budget);w.WriteNumber("Terminals",terminals);w.WriteNumber("PreparedNumericControls",controls);w.WriteString("PreparedClassDigest",digest);w.WriteNumber("PublicLexicalFirst",publicFirst);w.WriteNumber("PublicLexicalContinuations",publicContinuation);w.WriteNumber("ParallelQueries",4096);w.WriteNumber("Guards",guards);w.WriteNumber("CanceledPreparations",cancels);w.WriteNumber("CanceledQueries",queryCancels);w.WriteString("PreparedEmptyHistoryRecord",Convert.ToHexString(emptyBytes));w.WriteNumber("PreparedEmptyHistoryMask",preparedEmpty.WrittenByteMask);w.WritePropertyName("ControlKinds");w.WriteStartArray();foreach(long n in kinds)w.WriteNumberValue(n);w.WriteEndArray();w.WritePropertyName("MatchedOrders");w.WriteStartArray();foreach(long n in orders)w.WriteNumberValue(n);w.WriteEndArray();w.WritePropertyName("Files");w.WriteStartObject();foreach(var pair in files)w.WriteString(pair.Key,pair.Value);w.WriteEndObject();w.WriteBoolean("TargetsRead",false);w.WriteBoolean("VendorExecuted",false);w.WriteEndObject();
}
Console.WriteLine($"PASS EN lexical V58 {controls}prepared, {publicFirst}first, {publicContinuation}continuations, {guards}guards, {budget}additional logical bytes");
