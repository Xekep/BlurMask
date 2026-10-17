using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=1)throw new ArgumentException("capture.json");
var random=new Random(0x275);int guards=0,wordCount=0,removals=0,selectedCount=0;
using var stream=File.Create(args[0]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
for(int trial=0;trial<1024;trial++)
{
 int n=random.Next(1,13);var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
 var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);var span=new LegacyNativeWordSourceSpan(0,9);
 var builders=new List<LegacyNativeWordBuilder>();var original=new List<LegacyNativePlainWordRecord>();
 for(int i=0;i<n;i++)
 {
  byte[] residue=new byte[120];random.NextBytes(residue);foreach(int p in new[]{32,88,104})residue.AsSpan(p,8).Clear();foreach(int p in new[]{96,112})residue.AsSpan(p,4).Clear();
  var word=LegacyNativeWordBuilder.CreateEmpty(pool,residue);ulong? previous=null;
  for(int j=0;j<1+random.Next(3);j++)
  {
   byte[] glyph=new byte[40];random.NextBytes(glyph);var inserted=word.AppendGlyph((ulong)(i*4+j),i*4+j,true,glyph);
   var actual=pool.CopyRecord(inserted.Reference);BinaryPrimitives.WriteUInt16LittleEndian(actual.AsSpan(4),0);BinaryPrimitives.WriteUInt16LittleEndian(actual.AsSpan(6),(ushort)random.Next(4096));BinaryPrimitives.WriteUInt16LittleEndian(actual.AsSpan(8),(ushort)(i%3));pool.ReplaceRecord(inserted.Reference,actual);
   if(previous is {} p)word.LinkGlyph(p,inserted.Reference);previous=inserted.Reference;
  }
  word.SetRawScalars((i-4)*.25f,i*.125f,i,-i);builders.Add(word);
  var row=publisher.InternPlainWord(word,span,out int ordinal);Check(ordinal==i&&publisher.Head is null);original.Add(row);
  word.SetRawScalars(200,300,400,500);var duplicate=publisher.InternPlainWord(word,span,out int same);Check(same==i&&ReferenceEquals(row,duplicate));
 }
 int[] chosen=Shuffle(Enumerable.Range(0,n)).Take(Math.Min(n,3)).ToArray();
 var publishers=new List<LegacyNativePlainWordPublisher>{publisher};var nodes=new List<LegacyNativePublishedWordNode>();
 for(int i=0;i<chosen.Length;i++)
 {
  var p=i==0?publisher:publisher.ForkAlternative();if(i!=0)publishers.Add(p);
  nodes.Add(p.Publish(builders[chosen[i]],(_,_)=>0,(_,_)=>9));Check(nodes[^1].WordIndex==chosen[i]);
 }
 LegacyNativePublishedWordNode?[] heads=publishers.Select(p=>p.Head).ToArray();
 var snapshot=publisher.CaptureSpan(nodes[0],heads);Check(snapshot.Words.Count==n&&snapshot.SelectedOrdinals.SequenceEqual(chosen));
 var keep=new List<int>();for(int i=0;i<n;i++)if(chosen.Contains(i)||random.Next(2)==0)keep.Add(i);
 var kept=Shuffle(keep);
 var map=Enumerable.Repeat(-1,n).ToArray();for(int i=0;i<kept.Length;i++)map[kept[i]]=i;
 byte[][] ranked=kept.Select(i=>original[i].CopyStorage()).ToArray();
 if(trial%4==0)BinaryPrimitives.WriteInt32LittleEndian(ranked[0].AsSpan(16),unchecked((int)0xff7fffff));
 var stale=publisher.CaptureSpan(nodes[0],heads);var poison=publisher.CaptureSpan(nodes[0],heads);
 var bad=ranked.Select(b=>b.ToArray()).ToArray();bad[0][0]^=1;
 Reject(()=>publisher.ApplySpanRanking(poison,bad,kept,map));Reject(()=>publisher.ApplySpanRanking(poison,ranked,kept,map));
 publisher.ApplySpanRanking(snapshot,ranked,kept,map);Reject(()=>publisher.ApplySpanRanking(snapshot,ranked,kept,map));
 Reject(()=>publisher.ApplySpanRanking(stale,ranked,kept,map));Reject(()=>publisher.ApplySpanRanking(stale,ranked,kept,map));
 var after=publisher.CaptureSpan(nodes[0],heads);Check(after.Words.Count==kept.Length);
 for(int i=0;i<kept.Length;i++)Check(after.Words[i].CopyStorage().AsSpan().SequenceEqual(ranked[i])&&after.Words[i].FlatReferences.SequenceEqual(original[kept[i]].FlatReferences));
 for(int i=0;i<nodes.Count;i++)Check(nodes[i].WordIndex==map[chosen[i]]&&ReferenceEquals(nodes[i].Word,after.Words[map[chosen[i]]]));
 byte[][] scored=ranked.Select(b=>b.ToArray()).ToArray();foreach(var row in scored){byte[] score=new byte[8];random.NextBytes(score);score.CopyTo(row,16);}
 publisher.ApplySpanScores(after,scored);Reject(()=>publisher.ApplySpanScores(after,scored));
 var final=publisher.CaptureSpan(nodes[0],heads);for(int i=0;i<kept.Length;i++)Check(final.Words[i].CopyStorage().AsSpan().SequenceEqual(scored[i])&&final.Words[i].FlatReferences.SequenceEqual(original[kept[i]].FlatReferences));
 var canceled=publisher.CaptureSpan(nodes[0],heads);Reject(()=>publisher.ApplySpanScores(canceled,scored,new CancellationToken(true)));
 publisher.ApplySpanScores(canceled,scored); //Cancellation before entry does not consume the snapshot.
 Reject(()=>publisher.CaptureSpan(nodes[0],[heads[0],heads[0]]));
 var foreignPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var foreignPublisher=new LegacyNativePlainWordPublisher(foreignPool,false,false,false);
 Reject(()=>foreignPublisher.ApplySpanScores(final,scored));
 w.WriteStartObject();w.WriteStartArray("original");foreach(var row in original){w.WriteStartObject();w.WriteBase64String("row",row.CopyStorage());w.WriteStartArray("flat");foreach(ulong r in row.FlatReferences)w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
 WriteInts("selected",chosen);WriteInts("sources",kept);WriteInts("map",map);w.WriteStartArray("ranked");foreach(var row in ranked)w.WriteBase64StringValue(row);w.WriteEndArray();
 w.WriteStartArray("output");foreach(var row in publisher.CaptureSpan(nodes[0],heads).Words){w.WriteStartObject();w.WriteBase64String("row",row.CopyStorage());w.WriteStartArray("flat");foreach(ulong r in row.FlatReferences)w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("scores");foreach(var row in scored)w.WriteBase64StringValue(row);w.WriteEndArray();WriteInts("rebound",nodes.Select(p=>p.WordIndex));w.WriteEndObject();
 wordCount+=n;selectedCount+=chosen.Length;removals+=n-kept.Length;
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS1024 prepared span owners/{wordCount} WORDs/{selectedCount}279nodes/{removals} released payloads/{guards} assertions; no V68 completion.");
void WriteInts(string name,IEnumerable<int> values){w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
int[] Shuffle(IEnumerable<int> values){int[] a=values.ToArray();for(int i=a.Length-1;i>0;i--){int j=random.Next(i+1);(a[i],a[j])=(a[j],a[i]);}return a;}
void Check(bool value){guards++;if(!value)throw new InvalidDataException("Span storage invariant failed.");}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing span storage guard.");}
