using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=2)throw new ArgumentException("models-directory capture.json");
string[] paths=["en-US/en_US-ak-cur.lite.res","en-US/en_US-ak-cur.res","en-US/en_US-ak-superimposed.lite.res","en-US/en_US-ak-superimposed.res","ru-RU/ru_RU-ak-cur.lite.res"];
using var output=File.Create(args[1]);using var w=new Utf8JsonWriter(output);int cases=0,guards=0,changed=0,failed=0;
w.WriteStartObject();w.WriteStartArray("sources");
foreach(string path in paths)
{
 byte[] resource=File.ReadAllBytes(Path.Combine(args[0],path));var empty=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
 int sampler=path switch{"en-US/en_US-ak-cur.lite.res"=>182412,"en-US/en_US-ak-cur.res"=>374182,"ru-RU/ru_RU-ak-cur.lite.res"=>188157,_=>207622};
 var controls=Controls();var template=LegacyPreparedContextualQuoteSource.Prepare(resource,sampler,empty,controls,(_,_)=>0);
 w.WriteStartObject();w.WriteString("path",path);w.WriteString("sha256",template.ResourceFingerprint);w.WriteNumber("count",template.Alphabet.LabelCount);w.WriteNumber("profile",template.CategoryOwner.CategoryProfileOffset);
 w.WriteStartArray("labels");var labels=template.Labels;foreach(ushort v in new[]{labels.Apostrophe,labels.DoubleQuote,labels.OpenDouble,labels.CloseDouble,labels.OpenSingle,labels.CloseSingle,labels.LowDouble})w.WriteNumberValue(v);w.WriteEndArray();
 w.WriteStartArray("categories");for(ushort n=0;n<template.Alphabet.LabelCount;n++){try{w.WriteNumberValue(template.ReadPreviousCategory(n,default));}catch(NotSupportedException){w.WriteNullValue();}}w.WriteEndArray();
 w.WriteStartArray("cases");
 for(int seed=0;seed<256;seed++)RunCase(resource,template,seed,0,0,0,false);
 //Fault injection is explicitly prepared; no native callback success is supplied.
 foreach(int stop in new[]{1,2,3,4,5,8,12})RunCase(resource,template,4,stop,0,0,false);
 foreach(int stop in new[]{1,2,3,4})RunCase(resource,template,4,0,stop,0,false);
 foreach(int stop in new[]{1,2,3,8,12})RunCase(resource,template,4,0,0,stop,false);
 RunCase(resource,template,4,0,0,0,true);
 w.WriteEndArray();w.WriteStartArray("preparedCarryControls");
 foreach(int seed in new[]{6,7})
 {
  RunCase(resource,template,seed,0,0,0,false,0);
  for(int stop=1;stop<=18;stop++)RunCase(resource,template,seed,stop,0,0,false,0);
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();
//Foreign WORD and unsafe null-head are rejected only after both native guards.
{
 byte[] resource=File.ReadAllBytes(Path.Combine(args[0],paths[0]));var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var foreign=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
 var source=LegacyPreparedContextualQuoteSource.Prepare(resource,182412,pool,Controls(),(_,_)=>0);
 var node=new LegacyNativePublishedWordNode(default,0,new(foreign,new byte[120],[]),null);
 var bad=new LegacyPreparedPublishedGlyphRewrite(source,node,1);Reject(()=>bad.Run());if(!bad.IsStopped)throw new Exception("Foreign failure was not stopped.");Reject(()=>bad.Run());
 var noHead=new LegacyPreparedPublishedGlyphRewrite(source,null,1);Reject(()=>noHead.Run());Reject(()=>noHead.Run());
 var disabled=new LegacyPreparedPublishedGlyphRewrite(source,node,0);disabled.Run();if(!disabled.IsComplete||disabled.CopyEvents().Length!=0)throw new Exception("Disabled entry touched a WORD.");Reject(()=>disabled.Run());
 Reject(()=>LegacyPreparedContextualQuoteSource.Prepare([],0,pool,Controls(),(_,_)=>0));
 Reject(()=>LegacyPreparedContextualQuoteSource.Prepare(resource,182412,pool,Controls(),(_,_)=>0,new(true)));
 Reject(()=>LegacyPreparedContextualQuoteSource.Prepare(resource,0,pool,Controls(),(_,_)=>0));
 Reject(()=>LegacyPreparedContextualQuoteSource.Prepare(resource,resource.Length-15,pool,Controls(),(_,_)=>0));
 var badHeader=new byte[120];BinaryPrimitives.WriteInt32LittleEndian(badHeader.AsSpan(40),1);
 var mismatched=new LegacyPreparedPublishedGlyphRewrite(source,new(default,0,new(pool,badHeader,[]),null),1);Reject(()=>mismatched.Run());Reject(()=>mismatched.Run());
 var inserted=pool.AppendDescriptor(1,1,false,new byte[40]);
 var overRefs=new LegacyPreparedPublishedGlyphRewrite(source,new(default,0,Word(pool,Enumerable.Repeat(inserted.Reference,65537).ToArray()),null),1);Reject(()=>overRefs.Run());Reject(()=>overRefs.Run());
 LegacyNativePublishedWordNode? many=null;for(int i=0;i<4097;i++)many=new(default,i,Word(pool,[]),many);
 var overWords=new LegacyPreparedPublishedGlyphRewrite(source,many,1);Reject(()=>overWords.Run());Reject(()=>overWords.Run());
}
w.WriteNumber("cases",cases);w.WriteNumber("changed",changed);w.WriteNumber("failed",failed);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {cases} prepared linked WORD drivers/{changed} changed/{failed} stopped/{guards} guards; no actual enabled controller.");

void RunCase(byte[] resource,LegacyPreparedContextualQuoteSource template,int seed,int failEvent,int failQuery,int cancelEvent,bool preCanceled,ushort? preparedLowDouble=null)
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var controls=Controls();
 var labels=template.Labels;ushort letter=template.Alphabet.TryGetLabel("A",out ushort al)?al:(ushort)0;
 ushort q=labels.DoubleQuote,apostrophe=labels.Apostrophe;
 var descriptors=new List<(ushort Kind,ushort Label)>();
 var reading=new List<int[]>();
 if(preparedLowDouble is{} low)
 {
  descriptors.AddRange(seed==6?new[]{((ushort)0,low),((ushort)0,q),((ushort)0,letter),((ushort)0,q)}:new[]{((ushort)0,low),((ushort)0,q)});
  reading.Add([0]);reading.Add([]);reading.Add(Enumerable.Range(1,descriptors.Count-1).ToArray());
 }
 else if(seed==0){descriptors.Add((0,q));reading.Add([0]);}
 else if(seed==1){descriptors.AddRange(new[]{((ushort)0,q),((ushort)0,letter),((ushort)0,q)});reading.Add([0,1,2]);}
 else if(seed==2){descriptors.Add((0,q));reading.Add([0]);reading.Add([]);reading.Add([0,0]);}
 else if(seed==3){descriptors.AddRange(new[]{((ushort)0,q),((ushort)0,letter),((ushort)0,(ushort)65535),((ushort)0,q)});reading.Add([0,1,2,3]);}
 else if(seed==4){descriptors.AddRange(new[]{((ushort)0,q),((ushort)0,letter),((ushort)0,q),((ushort)0,q),((ushort)0,letter),((ushort)0,q)});reading.Add([0,1]);reading.Add([]);reading.Add([2,3]);reading.Add([4,5]);}
 else if(seed==5){descriptors.AddRange(new[]{((ushort)0,q),((ushort)0,letter),((ushort)0,q)});reading.Add([0,1]);reading.Add([2,1]);}
 else if(seed==8){descriptors.AddRange(new[]{((ushort)4,q),((ushort)0,letter),((ushort)4,q)});reading.Add([0,1,2]);}
 else if(seed==9){descriptors.AddRange(new[]{((ushort)3,(ushort)6),((ushort)0,q),((ushort)0,letter),((ushort)0,q)});reading.Add([0,1,2,3]);}
 else if(seed==10){reading.Add([]);reading.Add([]);reading.Add([]);}
 else
 {
  int count=1+seed%17;
  for(int n=0;n<count;n++)
  {
   int mode=(seed+n*7)%16;ushort value=mode switch{0=>q,1=>apostrophe,2=>letter,3=>65529,4=>65530,5=>65531,6=>65532,7=>65533,8=>65535,9=>labels.OpenDouble,10=>labels.CloseDouble,_=>(ushort)((seed*13+n*23)%template.Alphabet.LabelCount)};
   ushort kind=mode switch{3 or 4 or 5 or 6=>1,11=>4,12=>3,13=>2,14=>5,15=>6,_=>0};
   if(kind==1)value=(ushort)(4+(mode-3));
   if(kind==3)value=seed%3==0?(ushort)0:seed%3==1?(ushort)5:(ushort)(4+seed%4);
   descriptors.Add((kind,value));
  }
  var all=Enumerable.Range(0,count).Select(n=>seed%7==0?n%Math.Min(count,3):n).ToArray();
  reading.Add([]);reading.Add(all.Take(count/2).ToArray());reading.Add([]);reading.Add(all.Skip(count/2).ToArray());reading.Add([]);
 }
 var refs=new List<ulong>();var kinds=new Dictionary<ulong,byte>();
 for(int n=0;n<descriptors.Count;n++)
 {
  bool direct=seed<=5||seed is 8 or 9||preparedLowDouble is not null;
  ushort arc=direct?(ushort)n:((seed+n)%9==0?(ushort)0xffff:(seed+n)%11==0?(ushort)0x8000:(ushort)((seed+n)%32768));
  int node=seed>=5&&(seed+n)%8==0?-1:n+1;
  if(seed==5){node=5;arc=1;}
  ulong key=(uint)node&0xffffffUL;key|=(ulong)arc<<24;if(seed!=5&&(seed+n)%5==0)key|=1UL<<56;
  var inserted=pool.AppendDescriptor(key,n,false,new byte[40]);byte[] row=pool.CopyRecord(inserted.Reference);
  for(int k=4;k<40;k++)row[k]=(byte)(seed*17+n*13+k*7);
  BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),(ushort)((BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4))&0xfff8)|descriptors[n].Kind));
  BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),descriptors[n].Label);pool.ReplaceRecord(inserted.Reference,row);
  refs.Add(inserted.Reference);kinds[key]=direct?(byte)0:(byte)((seed+n)%6==0?2:0);
 }
 //End-of-list's native {FFFF,0} still asks kind(node0,arc0). This is an
 //explicit prepared source service, not a fabricated actual graph arc.
 kinds[0]=0;var queries=new List<(int Node,ushort Arc)>();
 using var cts=new CancellationTokenSource();if(preCanceled)cts.Cancel();
 var source=LegacyPreparedContextualQuoteSource.Prepare(resource,template.CategoryOwner.SamplerNodeOffset,pool,controls,(reference,token)=>
 {
  queries.Add((reference.Node,reference.Arc));if(failQuery!=0&&queries.Count==failQuery)throw new InvalidOperationException("Prepared source query fault.");
  ulong key=((uint)reference.Node&0xffffffUL)|((ulong)reference.Arc<<24);
  //Graph kind ignores record ordinal and the source-key bit56.
  foreach(var pair in kinds)if((pair.Key&0xffffffffffUL)==key)return pair.Value;
  throw new ArgumentException("Prepared source callback has no matching reference.");
 });
 if(preparedLowDouble is{} field)source=source.WithPreparedLowDoubleField(field);
 LegacyNativePublishedWordNode? head=null;for(int i=reading.Count-1;i>=0;i--)head=new(default,i,Word(pool,reading[i].Select(n=>refs[n]).ToArray()),head);
 var wordHeaders=new List<byte[]>();for(var node=head;node is not null;node=node.Next)wordHeaders.Add(node.Word.CopyStorage());
 var before=pool.CopyOwnerNodes();byte f4=seed%13==0?(byte)0:seed%3==0?(byte)2:seed%3==1?(byte)1:(byte)255;
 if(seed<=5||seed is 8 or 9||preparedLowDouble is not null)f4=1;
 int eventCount=0;var driver=new LegacyPreparedPublishedGlyphRewrite(source,head,f4,e=>
 {eventCount++;if(failEvent!=0&&eventCount==failEvent)throw new InvalidOperationException("Prepared observer fault after event.");if(cancelEvent!=0&&eventCount==cancelEvent)cts.Cancel();});
 string? error=null;try{driver.Run(cts.Token);}catch(Exception e)when(e is NotSupportedException or InvalidOperationException or ArgumentException or OperationCanceledException){error=e.GetType().Name;}
 if((error is null)!=driver.IsComplete||driver.IsStopped!=(error is not null))throw new Exception("Completion/failure state mismatch.");
 Reject(()=>driver.Run());var after=pool.CopyOwnerNodes();bool differs=!Flatten(before).AsSpan().SequenceEqual(Flatten(after));if(differs)changed++;if(error is not null)failed++;
 int wi=0;for(var node=head;node is not null;node=node.Next)if(!node.Word.CopyStorage().AsSpan().SequenceEqual(wordHeaders[wi++]))throw new Exception("Rewrite changed WORD storage.");
 w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteNumber("f4",f4);if(preparedLowDouble is{} field24be)w.WriteNumber("preparedField24be",field24be);w.WriteNumber("failEvent",failEvent);w.WriteNumber("failQuery",failQuery);w.WriteNumber("cancelEvent",cancelEvent);w.WriteBoolean("preCanceled",preCanceled);if(error is null)w.WriteNull("error");else w.WriteString("error",error);
 w.WriteStartArray("vectors");foreach(var vector in reading){w.WriteStartArray();foreach(int index in vector)w.WriteNumberValue(refs[index]);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("wordHeaders");foreach(var header in wordHeaders)w.WriteBase64StringValue(header);w.WriteEndArray();
 Nodes("before",before);Nodes("after",after);
 w.WriteStartArray("kindMap");foreach(var pair in kinds){w.WriteStartArray();w.WriteNumberValue(pair.Key);w.WriteNumberValue(pair.Value);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("queries");foreach(var query in queries){w.WriteStartArray();w.WriteNumberValue(query.Node);w.WriteNumberValue(query.Arc);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("events");foreach(var e in driver.CopyEvents()){w.WriteStartArray();w.WriteStringValue(e.Operation);w.WriteNumberValue(e.Word);w.WriteNumberValue(e.Ordinal);w.WriteNumberValue(e.Reference);w.WriteNumberValue(e.Value);w.WriteNumberValue(e.Extra);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();cases++;
}
LegacyNativeDynamicControlAlphabet Controls(){var controls=LegacyNativeDynamicControlAlphabet.CreateFresh();controls.InsertUtf8("\n"u8);controls.InsertUtf8(" "u8);controls.InsertUtf8("\u00a0"u8);return controls;}
LegacyNativePlainWordRecord Word(LegacyNativeGlyphRecordPool pool,ulong[] refs){var bytes=new byte[120];BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(40),refs.Length);BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(44),refs.Length);return new(pool,bytes,refs);}
void Nodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var n in nodes){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
byte[] Flatten(IReadOnlyList<LegacyNativeGlyphRecordNode> nodes)=>nodes.SelectMany(n=>n.Records.ToArray()).ToArray();
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Invalid prepared rewrite accepted.");}
