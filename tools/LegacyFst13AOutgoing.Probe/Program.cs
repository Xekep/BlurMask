using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
var guards=new List<string>();var canceled=new CancellationToken(true);
void Require(bool ok,string name){if(!ok)throw new Exception(name);}
void Refuse<T>(string name,Action call)where T:Exception
{try{call();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal: "+name);}
byte[] lk=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-lk-text.lite.res"));
byte[] ak=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));
var root=LegacyLinguisticRoot13FWireLayout.Read(lk);var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(root);
var source=LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak,398);
Require(source.HasActualFreshMain135Authority&&source.ActualAcousticSourceKind==0x135&&ReferenceEquals(source.SourceRoot,root)&&ReferenceEquals(source.SourceChild,wire.SourceChild),"Actual identity lost");
Require(source.StateCount==98&&source.TransitionWordCount==299,"Actual shape changed");
var table=source.CopyCostTable()!;var tableBytes=new byte[1024];for(int i=0;i<256;i++)BinaryPrimitives.WriteInt32LittleEndian(tableBytes.AsSpan(i*4),table[i]);
Require(Convert.ToHexStringLower(SHA256.HashData(tableBytes))=="6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3","Full native table differs");
var tableCopy=source.CopyCostTable()!;tableCopy[0]^=int.MaxValue;Require(source.CopyCostTable()![0]==table[0],"Cost output aliases");guards.Add("actual-cost-output-mutation");
using var output=File.Create(args[1]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteNumber("schema",1);w.WriteStartArray("sources");
int sources=0,states=0,rows=0;
void Capture(string name,LegacyFst13AOutgoingSource s)
{
 w.WriteStartObject();w.WriteString("name",name);w.WriteBoolean("actualAuthority",s.HasActualFreshMain135Authority);w.WriteBoolean("rootReceipt",s.SourceRoot is not null);w.WriteBoolean("childReceipt",s.SourceChild is not null);w.WriteString("wireSha",s.WireFingerprint.ToLowerInvariant());
 if(s.ActualAcousticFingerprint is {} a)w.WriteString("acousticSha",a.ToLowerInvariant());
 var costs=s.CopyCostTable();w.WriteStartArray("costs");if(costs is not null)foreach(int x in costs)w.WriteNumberValue(x);w.WriteEndArray();w.WriteBoolean("hasCostTable",s.HasCostTable);
 w.WriteNumber("stateCount",s.StateCount);w.WriteNumber("wordCount",s.TransitionWordCount);w.WriteStartArray("states");
 for(uint state=0;state<s.StateCount;state++)
 {
  var result=s.Query(state);Require(result.State==state&&result.OutputFlag==0,"Wrong state/flag");
  var copy=result.CopyRows();w.WriteStartObject();w.WriteNumber("state",state);w.WriteNumber("outputFlag",result.OutputFlag);w.WriteStartArray("rows");
  for(int i=0;i<result.Count;i++){var r=result.GetRow(i);Require(copy[i]==r,"Output copy differs");w.WriteStartArray();w.WriteNumberValue(r.TargetState);w.WriteNumberValue(r.SourceLabel);w.WriteNumberValue(r.Cost);w.WriteNumberValue(r.TargetSignedByte6);w.WriteNumberValue(r.TargetByte7);w.WriteEndArray();}
  w.WriteEndArray();w.WriteEndObject();if(copy.Length!=0){var first=result.GetRow(0);copy[0]=default;Require(result.GetRow(0)==first,"Result copy aliases");}
  Refuse<ArgumentOutOfRangeException>(name+"-row-negative-"+state,()=>result.GetRow(-1));
  Refuse<ArgumentOutOfRangeException>(name+"-row-end-"+state,()=>result.GetRow(result.Count));
  Refuse<OperationCanceledException>(name+"-copy-canceled-"+state,()=>result.CopyRows(canceled));
  rows+=result.Count;states++;
 }
 w.WriteEndArray();w.WriteEndObject();sources++;
 Parallel.For(0,32,i=>{uint state=(uint)(i%s.StateCount);var a=s.Query(state).CopyRows();Require(a.SequenceEqual(s.Query(state).CopyRows()),"Concurrent query changed");});
}
Capture("actual-number",source);
var genericActual=LegacyFst13AOutgoingSource.CreateDiagnostic(wire,table);Capture("diagnostic-actual-table",genericActual);
Require(!genericActual.HasActualFreshMain135Authority&&genericActual.SourceRoot is null&&genericActual.SourceChild is null&&genericActual.ActualAcousticSourceKind is null,"Diagnostic acquired authority");guards.Add("diagnostic-no-actual-authority");
foreach(bool narrow in new[]{false,true})
{
 var raw=Build(narrow);var input=LegacyFst23CWireLayout.ReadInline(raw,3);var custom=Enumerable.Range(0,256).Select(i=>(i-128)*12345+77).ToArray();
 var owned=LegacyFst13AOutgoingSource.CreateDiagnostic(input,custom,13);Array.Fill(custom,int.MaxValue);Array.Fill(raw,(byte)0);
 Capture(narrow?"synthetic-narrow":"synthetic-wide",owned);
 Require(owned.Query(0).GetRow(0).Cost==-128*12345+77&&owned.Query(0).GetRow(1).Cost==-127*12345+77,"Signed table centering lost");guards.Add(narrow?"narrow-caller-mutation":"wide-caller-mutation");
 Capture(narrow?"synthetic-narrow-null":"synthetic-wide-null",LegacyFst13AOutgoingSource.CreateDiagnostic(input,[]));
}
w.WriteEndArray();
var foreign=LegacyLinguisticRoot13FWireLayout.Read(lk);
Refuse<ArgumentException>("foreign-root-identity",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(foreign,wire,ak));
Refuse<ArgumentException>("generic-wire-no-receipt",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,LegacyFst23CWireLayout.ReadInline(wire.CopyInlineBytes(),299576),ak));
Refuse<ArgumentException>("wrong-acoustic-length",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak.AsSpan(1)));
var wrong=ak.ToArray();wrong[5233]^=1;Refuse<ArgumentException>("wrong-acoustic-content",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,wrong));
Refuse<ArgumentNullException>("null-root",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(null!,wire,ak));
Refuse<ArgumentNullException>("null-wire",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(null!,[]));
Refuse<InvalidDataException>("actual-whole-budget",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak,397));
Refuse<ArgumentOutOfRangeException>("negative-budget",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(wire,[], -1));
Refuse<ArgumentOutOfRangeException>("excess-budget",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(wire,[],int.MaxValue));
Refuse<ArgumentException>("short-table",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(wire,new int[255]));
Refuse<ArgumentException>("long-table",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(wire,new int[257]));
Refuse<ArgumentOutOfRangeException>("query-sentinel",()=>source.Query(98));
Refuse<ArgumentOutOfRangeException>("query-uintmax",()=>source.Query(uint.MaxValue));
Refuse<ArgumentOutOfRangeException>("negative-row-budget",()=>source.Query(0,-1));
Refuse<ArgumentOutOfRangeException>("excess-row-budget",()=>source.Query(0,int.MaxValue));
Refuse<ArgumentOutOfRangeException>("negative-row-byte-budget",()=>source.Query(0,rowByteBudget:-1));
Refuse<ArgumentOutOfRangeException>("excess-row-byte-budget",()=>source.Query(0,rowByteBudget:int.MaxValue));
Refuse<InvalidDataException>("row-budget",()=>source.Query(0,0));
Refuse<InvalidDataException>("row-byte-budget",()=>source.Query(0,rowByteBudget:15));
Require(source.Query(0,1,16).Count==1,"Exact query budgets rejected");guards.Add("exact-row-budgets");
Refuse<OperationCanceledException>("cancel-actual",()=>LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak,token:canceled));
Refuse<OperationCanceledException>("cancel-diagnostic",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(wire,[],token:canceled));
Refuse<OperationCanceledException>("cancel-null-first",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(null!,[],token:canceled));
Refuse<OperationCanceledException>("cancel-query",()=>source.Query(0,token:canceled));
Refuse<OperationCanceledException>("cancel-cost-copy",()=>source.CopyCostTable(canceled));
void Malformed(string name,Action<byte[]> mutate,bool storage=false)
{var b=Build(false);mutate(b);Refuse<InvalidDataException>(name,()=>{var r=LegacyFst23CWireLayout.ReadInline(b,3);if(!storage)_=LegacyFst13AOutgoingSource.CreateDiagnostic(r,[]);});}
// origin3: 13-byte aligned header, six descriptors, field20 at61, words at65.
Malformed("offset-decrease",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(13+2*8),4));
Malformed("offset-out-of-range",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(13),8));
Malformed("bad-target",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(65+6*4),5));
Malformed("max-target",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(65),uint.MaxValue));
Malformed("sentinel-disagreement",b=>BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(13+5*8),6),true);
var zero=Build(false).AsSpan(0,25).ToArray();BinaryPrimitives.WriteUInt32LittleEndian(zero.AsSpan(4),0);BinaryPrimitives.WriteInt32LittleEndian(zero.AsSpan(8),0);BinaryPrimitives.WriteUInt32LittleEndian(zero.AsSpan(13),0);BinaryPrimitives.WriteUInt32LittleEndian(zero.AsSpan(21),uint.MaxValue);
var empty=LegacyFst13AOutgoingSource.CreateDiagnostic(LegacyFst23CWireLayout.ReadInline(zero,3),[]);
Require(empty.StateCount==0&&empty.TransitionWordCount==0&&empty.CopyCostTable() is null,"Empty graph storage differs");guards.Add("empty-graph-owned");
Refuse<ArgumentOutOfRangeException>("empty-graph-no-state",()=>empty.Query(0));
Refuse<OperationCanceledException>("cancel-null-cost-copy",()=>empty.CopyCostTable(canceled));
var prefix=Build(false);BinaryPrimitives.WriteUInt32LittleEndian(prefix.AsSpan(13),1);
Require(LegacyFst13AOutgoingSource.CreateDiagnostic(LegacyFst23CWireLayout.ReadInline(prefix,3),[]).Query(0).Count==4,"Invented first offset zero restriction");guards.Add("nonzero-first-offset-allowed");
BinaryPrimitives.WriteUInt32LittleEndian(prefix.AsSpan(65),uint.MaxValue);
Refuse<InvalidDataException>("unused-prefix-target-whole-validation",()=>LegacyFst13AOutgoingSource.CreateDiagnostic(LegacyFst23CWireLayout.ReadInline(prefix,3),[]));
var last=LegacyFst13AOutgoingSource.CreateDiagnostic(LegacyFst23CWireLayout.ReadInline(Build(false),3),[]).Query(4,0,0);
Require(last.Count==0&&last.OutputFlag==0,"Empty row budget/flag differs");guards.Add("zero-outgoing-zero-budgets");
Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);GC.Collect();GC.WaitForPendingFinalizers();
Require(source.Query(0).GetRow(0).TargetState==97&&source.Query(0).GetRow(0).Cost==3454&&source.CopyCostTable()!.SequenceEqual(table),"Retained source lifetime changed");guards.Add("retained-lifetime-after-caller-mutation-gc");
w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteNumber("sourceCount",sources);w.WriteNumber("stateCount",states);w.WriteNumber("rowCount",rows);w.WriteBoolean("query28CImplemented",false);w.WriteBoolean("field20IsInitial",false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS13A outgoing: {sources} sources/{states} states/{rows} rows/{guards.Count} guards; actual98/299/256costs, no28C or recognition.");

static byte[] Build(bool narrow)
{
 var b=new byte[93];BinaryPrimitives.WriteUInt16LittleEndian(b,0x109);BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(2),narrow?(ushort)254:ushort.MaxValue);BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(4),5);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(8),7);b[12]=0xe7;
 uint[] offsets=[0,5,5,6,7,7];ushort[] labels=[0,65535,255,1,65534,42];byte[] tags=[128,129,0,1,127,255];
 for(int i=0;i<6;i++){int at=13+i*8;BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(at),offsets[i]);if(narrow){b[at+4]=i==0?(byte)0:i==1?(byte)255:(byte)(i+10);b[at+5]=tags[i];b[at+6]=(byte)(0xa0+i);b[at+7]=0xfa;}else{BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at+4),labels[i]);b[at+6]=tags[i];b[at+7]=(byte)(0xa0+i);}}
 BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(61),0xdeadbeef);uint[] targets=[0,1,1,2,3,4,2];for(int i=0;i<7;i++)BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(65+i*4),targets[i]);return b;
}
