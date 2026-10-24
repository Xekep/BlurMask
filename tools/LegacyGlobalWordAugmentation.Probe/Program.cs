using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("historical-walks.json prepared-parameters.json capture.json");
byte[] historical=File.ReadAllBytes(args[0]);using var parameters=JsonDocument.Parse(File.ReadAllBytes(args[1]));var p=parameters.RootElement;
if(Convert.ToHexStringLower(SHA256.HashData(historical))!=p.GetProperty("historicalSha256").GetString())throw new InvalidDataException("Pinned old numeric capture required.");
using var inputs=JsonDocument.Parse(historical);if(inputs.RootElement.GetArrayLength()!=7)throw new InvalidDataException("Exact seven historical captures required.");
ushort P(string name)=>p.GetProperty(name).GetUInt16();
var report=new List<object>();int walkCount=0,guards=0;
using var output=File.Create(args[2]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteString("Scope","PURE numeric historical replay and prepared275 append/update/rank; no actual owner/NN");w.WriteNumber("NNCalls",0);w.WriteStartArray("Historical");
foreach(var sample in inputs.RootElement.EnumerateArray())
{
 byte[] compact=sample.GetProperty("compact").GetBytesFromBase64(),residue=sample.GetProperty("residue").GetBytesFromBase64();int nodes=sample.GetProperty("nodes").GetInt32();
 var arcs=sample.GetProperty("arcs").EnumerateArray().Select(n=>n.EnumerateArray().Select(a=>a.EnumerateArray().Select(v=>unchecked((int)v.GetInt64())).ToArray()).ToArray()).ToArray();
 var kernel=new LegacyGlobalSpanAlternativeWalkKernel(compact,sample.GetProperty("count").GetInt32(),nodes,P("marker"),P("joiner"),P("closed"),P("unit"),P("softHyphen"),Enumerable.Repeat((byte)0xbd,64).ToArray(),
  (node,arc)=>arcs[node][arc][0],(node,arc)=>arcs[node][arc][1],static(_,cursor,_)=>new(cursor,0,0,0));
 w.WriteStartArray();
 foreach(var entry in sample.GetProperty("walks").EnumerateArray())
 {
  int index=entry.GetProperty("index").GetInt32(),min=entry.GetProperty("minimum").GetInt32();
  foreach(bool emit in new[]{false,true})
  {
   var r=kernel.Run(index,min,emit,emit?(ushort)65534:ushort.MaxValue,emit?residue:[],default);var e=entry.GetProperty(emit?"emit":"inspect");
   Check(r.Status==e.GetProperty("status").GetInt32()&&r.UpdatedMinimumNode==e.GetProperty("minimum").GetInt32()&&r.LastDirectParentIndex==e.GetProperty("parent").GetInt32()&&r.LastSourceKey==e.GetProperty("source").GetUInt64()&&r.WordFlags==e.GetProperty("wordFlags").GetUInt16());
   Check(r.Rows72.AsSpan().SequenceEqual(e.GetProperty("rows").GetBytesFromBase64())&&r.VisitedIndices.SequenceEqual(e.GetProperty("visited").EnumerateArray().Select(v=>v.GetInt32()))&&r.ResidualReferences.SequenceEqual(e.GetProperty("residuals").EnumerateArray().Select(v=>v.GetUInt64())));
   w.WriteStartObject();w.WriteNumber("Index",index);w.WriteBoolean("Emit",emit);w.WriteNumber("Status",r.Status);w.WriteNumber("Minimum",r.UpdatedMinimumNode);w.WriteNumber("Parent",r.LastDirectParentIndex);w.WriteNumber("Source",r.LastSourceKey);w.WriteNumber("Flags",r.WordFlags);w.WriteBase64String("Rows",r.Rows72);w.WriteEndObject();walkCount++;
  }
 }
 w.WriteEndArray();
 Reject(()=>kernel.Run(-1,-1,false,65535,[],default));Reject(()=>kernel.Run(0,-1,false,65535,[],new CancellationToken(true)));
}
w.WriteEndArray();
//Explicit synthetic eligibility, never patched into an observed input/actualowner.
byte[] toy=new byte[120];int[] toyStarts=[0,1,2,3];
for(int i=0;i<3;i++)
{
 var row=toy.AsSpan(i*40,40);row[4]=0;BinaryPrimitives.WriteUInt64LittleEndian(row[8..],(ulong)i);BinaryPrimitives.WriteInt32LittleEndian(row[16..],i-1);
 BinaryPrimitives.WriteInt32LittleEndian(row[20..],(i+1)*100);BinaryPrimitives.WriteInt32LittleEndian(row[24..],(i+1)*50);BinaryPrimitives.WriteUInt16LittleEndian(row[28..],(ushort)(i+1));
 row[32]=i==2?(byte)8:(byte)0;row[33]=255;BinaryPrimitives.WriteInt32LittleEndian(row[36..],-1);
}
int ToyFirst(int node,ushort arc){Check((uint)node<3&&arc==0);return node<<7;}
int ToyLast(int node,ushort arc)=>ToyFirst(node,arc);
var toyWalker=new LegacyGlobalSpanAlternativeWalkKernel(toy,3,3,P("marker"),P("joiner"),P("closed"),P("unit"),P("softHyphen"),new byte[64],ToyFirst,ToyLast,static(_,cursor,_)=>new(cursor,0,0,0));
int inspections=0;
var toyScan=LegacyPreparedMainWordAugmentationCandidates.Scan(toy,3,toyStarts,0,256,false,ToyFirst,ToyLast,(index,min)=>{inspections++;var dry=toyWalker.Run(index,min,false,65535,[],default);return new(dry.Status,dry.LastSourceKey);});
Check(toyScan.CompactIndices.SequenceEqual(new[]{2})&&inspections==1);
var emitted=toyWalker.Run(2,0,true,65534,new byte[72],default);Check(emitted.Status==4&&emitted.Rows72.Length==216&&emitted.ResidualReferences.Length==0);
var toyPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var toyWord=LegacyNativeWordBuilder.CreateEmpty(toyPool,new byte[120]);toyWord.ApplySpanAlternativeFlags30(emitted.WordFlags);
LegacyPreparedGlyphFinalizationInput ToyProduce(ReadOnlySpan<byte> row,LegacyOrdinaryGlyphProductionScratch scratch,ulong conversion,ReadOnlySpan<byte> compact,CancellationToken token)
{
 Check(conversion==1);ushort[] labels=new ushort[256];labels[255]=BinaryPrimitives.ReadUInt16LittleEndian(row[58..]);
 return new(true,255,labels,0,0,0x3f800000,BitConverter.SingleToUInt32Bits(.8f),0,0);
}
var toyChain=LegacyMainOrdinaryGlyphChainKernel.Apply(toyWord,emitted.Rows72,[],3,3,toyStarts,toy,static _=>1,ToyProduce,static(_,_,_,_,_,_,_)=>throw new InvalidDataException("No protocol substitution"),65535,uint.MaxValue,uint.MaxValue,false,new byte[40],new ushort[256],new byte[64],1,default);
Check(toyChain.Succeeded&&toyChain.CreatedCount==3&&toyWord.GetSymbolSpan()==3);
var toyCosts=LegacyPreparedMainAugmentedWordCosts.Calculate(toy,3,2,emitted.LastDirectParentIndex,0,2,0x135);toyWord.SetRawScalars(toyCosts.Score10,1,toyCosts.Cost18,toyCosts.Cost1C);
var toyPublisher=new LegacyNativePlainWordPublisher(toyPool,false,false,false);var toyNode=toyPublisher.Publish(toyWord,ToyFirst,ToyLast);Check(toyNode.Span==new LegacyNativeWordSourceSpan(0,256));
w.WriteStartObject("EligibleFlow");w.WriteBase64String("Compact",toy);w.WriteBase64String("Rows72",emitted.Rows72);w.WriteBase64String("Word120",toyNode.Word.CopyStorage());w.WriteNumber("SelectedCount",toyScan.CompactIndices.Length);w.WriteNumber("Inspections",inspections);w.WriteNumber("Created",toyChain.CreatedCount);w.WriteNumber("Head",toyNode.Word.HeadReference);w.WriteEndObject();
toy[2*40+32]=0;int noOpReads=0;var noOp=LegacyPreparedMainWordAugmentationCandidates.Scan(toy,3,toyStarts,0,256,false,ToyFirst,ToyLast,(_,_)=>{noOpReads++;throw new InvalidDataException("Ineligible inspect invoked");});Check(noOp.CompactIndices.Length==0&&noOpReads==0);w.WriteBoolean("IneligibleNoInspect",true);
w.WriteStartArray("Prepared");
for(int trial=0;trial<128;trial++)
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
 (LegacyNativeWordBuilder Word,ulong Reference) Word(int label,int descriptor,float cost)
 {
  var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);var add=word.AppendGlyph(0,descriptor,false,new byte[40]);byte[] row=pool.CopyRecord(add.Reference);
  BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),0);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(6),0);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),(ushort)label);BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(32),ulong.MaxValue);pool.ReplaceRecord(add.Reference,row);
  word.ApplySpanAlternativeFlags30(0);word.SetRawScalars(-5*cost,1,cost,trial/8192f);return(word,add.Reference);
 }
 var original=Word(1,trial*3,.1f+trial/1000f);var node=publisher.Publish(original.Word,static(_,_)=>0,static(_,_)=>0);byte[] originalBefore=node.Word.CopyStorage();
 var candidate=Word(2,trial*3+1,.4f+trial/1000f);_=publisher.InternPlainWord(candidate.Word,node.Span,out int ordinal);Check(ordinal==1&&ReferenceEquals(publisher.Head,node));
 var duplicate=Word(2,trial*3+2,.2f+trial/1000f);byte[] left=new byte[4],right=new byte[4];
 Check(LegacyNativeMainWordEquality.Compare(pool,candidate.Word.CopyStorage(),duplicate.Word.CopyStorage(),left,right,P("joiner"),static x=>x));
 Check(!LegacyNativeMainWordEquality.Compare(pool,original.Word.CopyStorage(),duplicate.Word.CopyStorage(),left,right,P("joiner"),static x=>x));
 var before=publisher.CaptureSpan(node,[node]);float cost=.2f+trial/1000f;publisher.UpdateAugmentedDuplicate(before,1,1,-5*cost-.00001f,cost,trial/8192f,0,default);
 Check(node.Word.CopyStorage().AsSpan().SequenceEqual(originalBefore));
 var cleaned=pool.RemoveCreatedChainSegment(duplicate.Reference,duplicate.Reference);Check(cleaned.RemovedReferences.SequenceEqual(new[]{duplicate.Reference}));Reject(()=>pool.CopyRecord(duplicate.Reference));
 var updated=publisher.CaptureSpan(node,[node]);byte[][] raw=updated.Words.Select(x=>x.CopyStorage()).ToArray();
 var rank=LegacyPreparedMainWordRanking.Rank(raw,updated.SelectedOrdinals.ToArray(),2,false,(a,b)=>LegacyNativeMainWordEquality.Compare(pool,a,b,left,right,P("joiner"),static x=>x));
 publisher.ApplySpanRanking(updated,rank.Words,rank.OutputSourceOrdinals,rank.OldToNew);var ranked=publisher.CaptureSpan(node,[node]);
 var mass=LegacyPreparedMainSpanScorePublication.Normalize(ranked.Words.Select(x=>x.CopyStorage()).ToArray());publisher.ApplySpanScores(ranked,mass.Words);var final=publisher.CaptureSpan(node,[node]);
 w.WriteStartObject();w.WriteNumber("Trial",trial);w.WriteNumber("SelectedOrdinal",node.WordIndex);w.WriteNumber("PayloadCount",final.Words.Count);w.WriteStartArray("BeforeMass");foreach(byte[] a in rank.Words)w.WriteBase64StringValue(a);w.WriteEndArray();w.WriteStartArray("AfterMass");foreach(var a in final.Words)w.WriteBase64StringValue(a.CopyStorage());w.WriteEndArray();w.WriteEndObject();
 Check(final.Words.Count==2&&node.WordIndex==0);
 //Actual publisher rejects a mutation of any original source payload.
 var invalid=publisher.CaptureSpan(node,[node]);Reject(()=>publisher.UpdateAugmentedDuplicate(invalid,0,1,0,0,0,0,default));Check(node.Word.CopyStorage().AsSpan().SequenceEqual(final.Words[0].CopyStorage()));
}
w.WriteEndArray();w.WriteStartArray("Source26");
foreach(int n in new[]{0,1,2,125,1024})
{
 var refs=Enumerable.Range(0,n).Select(i=>new LegacyOrdinaryGraphSourceReference(new(i+100,-i-3,i*7),new(i+200,-i-9,i*11),(byte)(i*3),(byte)(i*5))).ToArray();
 byte[] data=LegacyFreshMainGlobalWordAugmentation.EncodeSourceStorage26(refs,default);Check(data.Length==n*26);w.WriteStartObject();w.WriteNumber("Count",n);w.WriteBase64String("Bytes",data);w.WriteEndObject();
}
w.WriteEndArray();
byte[] reusableSeed=new byte[120];Array.Fill(reusableSeed,(byte)0xa5,44,4);reusableSeed[52]=0xa6;
var reusableWord=LegacyNativeWordBuilder.CreateEmpty(LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false),reusableSeed);
for(int i=0;i<125;i++){reusableWord.ApplyAugmentationFlag32(true,default);reusableWord.SetRawScalars(i,-i,i+2,i+3);reusableWord.ResetPlain();}
w.WriteBase64String("WorkingReuse120",reusableWord.CopyStorage());
w.WriteNumber("HistoricalResults",walkCount);w.WriteNumber("PreparedCases",128);w.WriteNumber("Checks",guards);w.WriteBoolean("ActualOwnerEntryExercised",false);w.WriteEndObject();Console.WriteLine($"PASS historical218A30results{walkCount}/prepared275cases128/checks{guards}; NN0; actual owner issuer unexercised.");
void Check(bool value){if(!value)throw new InvalidDataException("Prepared numeric control failed.");guards++;}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Refusal absent.");}
