using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
var guards=new List<string>();int cases=0;long descriptorsChecked=0,wordsChecked=0;
using var stream=File.Create(args[1]);using var w=new Utf8JsonWriter(stream);
w.WriteStartObject();w.WriteNumber("schema",1);w.WriteStartArray("cases");
string Sha(ReadOnlySpan<byte> bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
void Require(bool value,string message){if(!value)throw new Exception(message);}
void Refuse<T>(string name,Action call)where T:Exception
{
 try{call();}catch(T){guards.Add(name);return;}
 throw new Exception("Expected "+typeof(T).Name+": "+name);
}
void Capture(string name,byte[] input,int origin,LegacyFst23CWireLayout? supplied=null)
{
 var source=supplied??LegacyFst23CWireLayout.ReadInline(input,origin);int start=(int)((((long)origin+15)&~3L)-origin);
 Require(source.CopyInlineBytes().AsSpan().SequenceEqual(input),"Original inline copy differs.");
 Require(source.SourceOffset==origin&&source.SourceEnd==origin+input.Length&&source.Version==0x109,"Origin/version changed.");
 Require(source.DescriptorCount==source.StateCount+1,"Sentinel descriptor missing.");
 bool actual=name=="actual-number";
 Require((source.SourceRoot is not null)==actual&&(source.SourceChild is not null)==actual,"Generic bytes acquired a source receipt.");
 if(actual)Require(ReferenceEquals(source.SourceChild,source.SourceRoot!.Children[5]),"Actual child identity changed.");
 var copy=source.CopyInlineBytes();string fingerprint=Sha(input);Require(source.Fingerprint.ToLowerInvariant()==fingerprint,"Fingerprint differs.");
 var padding=source.CopyAlignmentPadding();Require(padding.AsSpan().SequenceEqual(input.AsSpan(12,start-12)),"Absolute alignment differs.");
 int wordsAt=start+source.DescriptorCount*8+4;var wordBytes=source.CopyTransitionBytes();
 Require(wordBytes.AsSpan().SequenceEqual(input.AsSpan(wordsAt)),"Raw transition bytes differ.");
 bool small=input.Length<=4096;using var descriptorHash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
 var buffer=new byte[8192];int used=0;
 w.WriteStartObject();w.WriteString("name",name);w.WriteNumber("origin",origin);w.WriteNumber("end",source.SourceEnd);
 w.WriteNumber("version",source.Version);w.WriteNumber("labels",source.LabelCount);w.WriteNumber("states",source.StateCount);
 w.WriteNumber("words",source.TransitionWordCount);w.WriteNumber("field20",source.RawField20);w.WriteString("fingerprint",fingerprint);
 w.WriteNumber("length",input.Length);w.WriteString("padding",Convert.ToHexStringLower(padding));w.WriteString("rawWordSha",Sha(wordBytes));
 w.WriteBoolean("rootReceipt",source.SourceRoot is not null);w.WriteBoolean("childReceipt",source.SourceChild is not null);
 if(actual){w.WriteString("rootFingerprint",source.SourceRoot!.Fingerprint.ToLowerInvariant());w.WriteNumber("childOrdinal",source.SourceChild!.Ordinal);w.WriteNumber("childOffset",source.SourceChild.Offset);w.WriteNumber("childEnd",source.SourceChild.End);}
 if(small)w.WriteBase64String("inline",input);
 w.WriteStartArray("descriptors");
 for(int i=0;i<source.DescriptorCount;i++)
 {
  var d=source.GetDescriptor(i);var raw=source.CopyDescriptorBytes(i);
  Require(raw.AsSpan().SequenceEqual(input.AsSpan(start+i*8,8)),"Raw descriptor copy differs.");
  var b=buffer.AsSpan(used,8);BinaryPrimitives.WriteUInt32LittleEndian(b,d.RawField0);BinaryPrimitives.WriteUInt16LittleEndian(b[4..],d.RawLabel);b[6]=d.RawField6;b[7]=d.RawField7;used+=8;
  if(used==buffer.Length){descriptorHash.AppendData(buffer);used=0;}
  if(small){w.WriteStartArray();w.WriteNumberValue(d.RawField0);w.WriteNumberValue(d.RawLabel);w.WriteNumberValue(d.RawField6);w.WriteNumberValue(d.RawField7);w.WriteStringValue(Convert.ToHexStringLower(raw));w.WriteEndArray();}
 }
 w.WriteEndArray();descriptorHash.AppendData(buffer.AsSpan(0,used));w.WriteString("descriptorValueSha",Convert.ToHexStringLower(descriptorHash.GetHashAndReset()));
 using var wordHash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);used=0;w.WriteStartArray("wordValues");
 for(int i=0;i<source.TransitionWordCount;i++)
 {
  uint v=source.GetTransitionWord(i);BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(used,4),v);used+=4;
  if(used==buffer.Length){wordHash.AppendData(buffer);used=0;}if(small)w.WriteNumberValue(v);
 }
 w.WriteEndArray();wordHash.AppendData(buffer.AsSpan(0,used));w.WriteString("wordValueSha",Convert.ToHexStringLower(wordHash.GetHashAndReset()));
 // Every returned array is detached. This checks cached fingerprint plus fresh
 // bytes/getters, rather than relying on the cached fingerprint alone.
 var first=source.GetDescriptor(0);var firstRaw=source.CopyDescriptorBytes(0);firstRaw[0]^=255;
 Require(source.GetDescriptor(0)==first&&!firstRaw.AsSpan().SequenceEqual(source.CopyDescriptorBytes(0)),"Descriptor output aliases.");
 Array.Fill(input,(byte)0xa5);copy[0]^=255;
 Require(Sha(source.CopyInlineBytes())==fingerprint&&source.Fingerprint.ToLowerInvariant()==fingerprint,"Caller/output mutation changed owner.");
 if(padding.Length!=0){padding[0]^=255;Require(!padding.AsSpan().SequenceEqual(source.CopyAlignmentPadding()),"Padding output aliases.");}
 if(wordBytes.Length!=0){uint firstWord=source.GetTransitionWord(0);wordBytes[0]^=255;Require(source.GetTransitionWord(0)==firstWord&&Sha(source.CopyTransitionBytes())==Sha(source.CopyInlineBytes().AsSpan(wordsAt)),"Word output aliases.");}
 if(small)Parallel.For(0,32,_=>Require(source.GetDescriptor(0)==first&&Sha(source.CopyInlineBytes())==fingerprint,"Concurrent immutable read changed state."));
 w.WriteBoolean("callerAndOutputMutationSafe",true);w.WriteBoolean("allGettersChecked",true);w.WriteEndObject();
 descriptorsChecked+=source.DescriptorCount;wordsChecked+=source.TransitionWordCount;cases++;
}
byte[] rootBytes=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-lk-text.lite.res"));
var root=LegacyLinguisticRoot13FWireLayout.Read(rootBytes);var actual=LegacyFst23CWireLayout.ReadActualRussianNumber(root);
Require(ReferenceEquals(actual.SourceRoot,root),"Actual root owner replaced.");
var actualInline=actual.CopyInlineBytes();Capture("actual-number",actualInline.ToArray(),299576,actual);
Capture("generic-actual-bytes",actualInline.ToArray(),299576);
var cloneRoot=LegacyLinguisticRoot13FWireLayout.Read(rootBytes);var cloneActual=LegacyFst23CWireLayout.ReadActualRussianNumber(cloneRoot);
Require(ReferenceEquals(cloneActual.SourceRoot,cloneRoot)&&!ReferenceEquals(actual.SourceRoot,cloneActual.SourceRoot)&&!ReferenceEquals(actual.SourceChild,cloneActual.SourceChild),"Equivalent roots coalesced identity.");guards.Add("distinct-actual-root-receipts");
Refuse<ArgumentException>("foreign-root-child",()=>cloneRoot.CopyChildEnvelope(actual.SourceChild!));
var altered=rootBytes.ToArray();altered[20]^=1;var wrong=LegacyLinguisticRoot13FWireLayout.Read(altered);
Refuse<ArgumentException>("wrong-root-fingerprint",()=>LegacyFst23CWireLayout.ReadActualRussianNumber(wrong));
Array.Fill(rootBytes,(byte)0);Require(ReferenceEquals(LegacyFst23CWireLayout.ReadActualRussianNumber(root).SourceRoot,root),"Root caller buffer aliases retained owner.");guards.Add("root-caller-mutation");
foreach(ushort labels in new ushort[]{0,1,254,255,65535})foreach(int origin in new[]{0,1,2,3})Capture($"procedural-{labels}-{origin}",Build(labels,3,5,origin),origin);
foreach(int origin in new[]{0,1,2,3})foreach(int words in new[]{0,3})Capture($"zero-states-{origin}-{words}",Build(255,0,words,origin),origin);
int high=int.MaxValue-64;Capture("high-origin",Build(254,1,1,high),high);
Capture("max-states",Build(255,LegacyFst23CWireLayout.MaximumStates,0,0),0);
Capture("max-words",Build(255,0,LegacyFst23CWireLayout.MaximumTransitionWords,0),0);
w.WriteEndArray();

byte[] valid=Build(254,2,3,1);
void Change(string name,Action<byte[]> mutate){var b=valid.ToArray();mutate(b);Refuse<InvalidDataException>(name,()=>LegacyFst23CWireLayout.ReadInline(b,1));}
foreach(ushort version in new ushort[]{0,0x108,0x10a}){var b=valid.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(b,version);Refuse<NotSupportedException>("version-"+version,()=>LegacyFst23CWireLayout.ReadInline(b,1));}
foreach(int length in Enumerable.Range(0,valid.Length))Refuse<InvalidDataException>("truncated-"+length,()=>LegacyFst23CWireLayout.ReadInline(valid.AsSpan(0,length),1));
Refuse<InvalidDataException>("trailing",()=>LegacyFst23CWireLayout.ReadInline(valid.Concat(new byte[]{0}).ToArray(),1));
foreach(uint value in new uint[]{LegacyFst23CWireLayout.MaximumStates+1,uint.MaxValue})Change("states-"+value,b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(4),value));
foreach(int value in new[]{-1,int.MinValue,LegacyFst23CWireLayout.MaximumTransitionWords+1,int.MaxValue})Change("words-"+value,b=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8),value));
Change("states-incomplete-cap",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(4),LegacyFst23CWireLayout.MaximumStates));
Change("words-incomplete-cap",b=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8),LegacyFst23CWireLayout.MaximumTransitionWords));
int sentinel=15+2*8;foreach(uint value in new uint[]{2,4,uint.MaxValue})Change("sentinel-"+value,b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(sentinel),value));
Refuse<ArgumentOutOfRangeException>("negative-origin",()=>LegacyFst23CWireLayout.ReadInline(valid,-1));
Refuse<ArgumentOutOfRangeException>("minimum-origin",()=>LegacyFst23CWireLayout.ReadInline(valid,int.MinValue));
Refuse<ArgumentOutOfRangeException>("origin-end-overflow",()=>LegacyFst23CWireLayout.ReadInline(valid,int.MaxValue));
Refuse<InvalidDataException>("input-byte-budget",()=>LegacyFst23CWireLayout.ReadInline(new byte[LegacyFst23CWireLayout.MaximumInlineBytes+1],0));
Refuse<ArgumentNullException>("null-root",()=>LegacyFst23CWireLayout.ReadActualRussianNumber(null!));
var canceled=new CancellationToken(true);
Refuse<OperationCanceledException>("cancel-inline",()=>LegacyFst23CWireLayout.ReadInline(valid,1,canceled));
Refuse<OperationCanceledException>("cancel-malformed-preflight",()=>LegacyFst23CWireLayout.ReadInline([],0,canceled));
Refuse<OperationCanceledException>("cancel-actual",()=>LegacyFst23CWireLayout.ReadActualRussianNumber(root,canceled));
Refuse<OperationCanceledException>("cancel-null-root-first",()=>LegacyFst23CWireLayout.ReadActualRussianNumber(null!,canceled));
var read=LegacyFst23CWireLayout.ReadInline(valid,1);var empty=LegacyFst23CWireLayout.ReadInline(Build(0,0,0,0),0);
foreach(int index in new[]{-1,int.MinValue,int.MaxValue,read.DescriptorCount})
{Refuse<ArgumentOutOfRangeException>("descriptor-"+index,()=>read.GetDescriptor(index));Refuse<ArgumentOutOfRangeException>("descriptor-copy-"+index,()=>read.CopyDescriptorBytes(index));}
foreach(int index in new[]{-1,int.MinValue,int.MaxValue,read.TransitionWordCount})Refuse<ArgumentOutOfRangeException>("transition-"+index,()=>read.GetTransitionWord(index));
Refuse<ArgumentOutOfRangeException>("empty-transition",()=>empty.GetTransitionWord(0));
Refuse<OperationCanceledException>("cancel-inline-copy",()=>read.CopyInlineBytes(canceled));
Refuse<OperationCanceledException>("cancel-padding-copy",()=>read.CopyAlignmentPadding(canceled));
Refuse<OperationCanceledException>("cancel-empty-padding-copy",()=>empty.CopyAlignmentPadding(canceled));
Refuse<OperationCanceledException>("cancel-words-copy",()=>read.CopyTransitionBytes(canceled));
Refuse<OperationCanceledException>("cancel-empty-words-copy",()=>empty.CopyTransitionBytes(canceled));
w.WriteStartArray("guards");foreach(string name in guards)w.WriteStringValue(name);w.WriteEndArray();w.WriteNumber("caseCount",cases);w.WriteNumber("descriptorGetters",descriptorsChecked);w.WriteNumber("wordGetters",wordsChecked);w.WriteBoolean("queryImplemented",false);w.WriteBoolean("field20SemanticsAssigned",false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS raw23C/109 storage: {cases} layouts/{guards.Count} guards, {descriptorsChecked} descriptor and {wordsChecked} transition getters; owned bytes/receipts/absolute alignment; no query or field20 semantics.");

static byte[] Build(ushort labels,int states,int words,int origin)
{
 int start=(int)((((long)origin+15)&~3L)-origin);int at20=start+(states+1)*8;var b=new byte[at20+4+words*4];
 BinaryPrimitives.WriteUInt16LittleEndian(b,0x109);BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(2),labels);BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(4),(uint)states);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8),words);
 for(int i=12;i<start;i++)b[i]=(byte)(0xa0+i);
 for(int i=0;i<=states;i++)
 {
  int at=start+i*8;uint field=i==states?(uint)words:unchecked(0xf1230000u+(uint)i*31337u);BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(at),field);
  if(labels<255){b[at+4]=i%3==0?(byte)255:i%3==1?(byte)254:(byte)13;b[at+5]=(byte)(0x80+i%128);b[at+6]=(byte)(0x21+i%200);b[at+7]=(byte)(0xe0+i%32);}
  else{BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at+4),i%3==0?ushort.MaxValue:i%3==1?(ushort)255:(ushort)0x8001);b[at+6]=(byte)(0x80+i%128);b[at+7]=(byte)(0x21+i%200);}
 }
 BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(at20),0xc001d00d);
 for(int i=0;i<words;i++)BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(at20+4+i*4),unchecked(0xdeadbeefu+(uint)i*2654435761u));
 return b;
}
