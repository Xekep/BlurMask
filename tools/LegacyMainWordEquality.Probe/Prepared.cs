using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

static class Prepared
{
 internal static void Run(string path)
 {
  var random=new Random(0x13f970);int count=0,guards=0,equals=0;
  using var stream=File.Create(path);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();
  for(int trial=0;trial<4096;trial++)
  {
   var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);int n=random.Next(13),m=trial%3==0?n:random.Next(13);
   var rows=new List<byte[]>();byte[] wa=new byte[120],wb=new byte[120];random.NextBytes(wa);random.NextBytes(wb);
   ushort flags=(ushort)random.Next(65536);W16(wa,50,flags);W16(wb,50,(ushort)(trial%7==0?flags^0x100:flags));
   //These raw costs include NaNs/infinities;13F970 never reads any cost field.
   W32(wa,16,unchecked((int)0x7fc00001));W32(wb,16,unchecked((int)0xff800000));
   for(int i=0;i<n+m;i++)
   {
    byte[] row=new byte[40];random.NextBytes(row);W32(row,0,i);int kind=random.Next(8);
    W16(row,4,(ushort)((random.Next(8192)<<3)|kind));
    ushort[] labels=[0,1,2,3,4,5,6,7,8,11,12,184,65535,65534,65533,65532];W16(row,8,labels[random.Next(labels.Length)]);
    if(trial%3==0&&i>=n)rows[i-n].CopyTo(row,0);
    W32(row,0,i);W64(row,32,(i+1==n||i+1==n+m)?ulong.MaxValue:((ulong)(i+1)<<40));
    rows.Add(row);var insertion=pool.AppendDescriptor(0,i,true,row);pool.ReplaceRecord(insertion.Reference,row);
   }
   W64(wa,8,n==0?ulong.MaxValue:0);W64(wb,8,m==0?ulong.MaxValue:(ulong)n<<40);
   //Early WORD flag mismatch must not touch even unavailable glyph references.
   if(trial%7==0){W64(wa,8,0x123456);W64(wb,8,0xabcdef);}
   ushort ignored=(ushort)(trial%2==0?184:65535),mask=(ushort)(trial%5==0?0xff:0);bool mapped=trial%4!=0;
   ushort[] controls=[(ushort)random.Next(65536),(ushort)random.Next(65536),(ushort)random.Next(65536)];
   byte[] ls=new byte[4],rs=new byte[4];random.NextBytes(ls);random.NextBytes(rs);var li=ls.ToArray();var ri=rs.ToArray();
   int calls=0,lookups=0;ushort Map(ushort v){calls++;return(ushort)(v^mask);}ushort Lookup(string s){lookups++;return controls[s=="\n"?0:s==" "?1:2];}
   bool equal=LegacyNativeMainWordEquality.Compare(pool,wa,wb,ls,rs,ignored,mapped?Map:null,Lookup);if(equal)equals++;
   w.WriteStartObject();w.WriteBase64String("left",wa);w.WriteBase64String("right",wb);w.WriteNumber("ignored",ignored);w.WriteBoolean("map",mapped);w.WriteNumber("xor",mask);
   w.WriteStartArray("controls");foreach(var c in controls)w.WriteNumberValue(c);w.WriteEndArray();
   w.WriteBase64String("leftIn",li);w.WriteBase64String("rightIn",ri);w.WriteBase64String("leftOut",ls);w.WriteBase64String("rightOut",rs);w.WriteBoolean("equal",equal);w.WriteNumber("mapCalls",calls);w.WriteNumber("lookupCalls",lookups);
   w.WriteStartArray("rows");for(int i=0;i<rows.Count;i++){w.WriteStartObject();w.WriteNumber("reference",(ulong)i<<40);w.WriteBase64String("row",rows[i]);w.WriteEndObject();if(!rows[i].AsSpan().SequenceEqual(pool.CopyRecord((ulong)i<<40)))throw new InvalidDataException("Comparator mutated a pool record.");}w.WriteEndArray();w.WriteEndObject();count++;
  }
  w.WriteEndArray();w.Flush();
  var empty=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);byte[] word=new byte[120];W64(word,8,ulong.MaxValue);
  Reject(()=>LegacyNativeMainWordEquality.Compare(empty,new byte[119],word,new byte[4],new byte[4],0));
  byte[] shared=new byte[4];Reject(()=>LegacyNativeMainWordEquality.Compare(empty,word,word,shared,shared,0));
  Reject(()=>LegacyNativeMainWordEquality.Compare(empty,word,word,word.AsSpan(0,4),new byte[4],0));
  Reject(()=>LegacyNativeMainWordEquality.Compare(empty,word,word,new byte[4],new byte[4],0,token:new CancellationToken(true)));
  var cycle=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);byte[] cyclic=new byte[40];W16(cyclic,6,2);W64(cyclic,32,0);cycle.AppendDescriptor(0,0,false,cyclic);cycle.ReplaceRecord(0,cyclic);W64(word,8,0);
  Reject(()=>LegacyNativeMainWordEquality.Compare(cycle,word,word,new byte[4],new byte[4],0));
  Console.WriteLine($"PASS{count} prepared linked-WORD cases/{equals} equal/{guards} guards; raw float bits unused.");
  void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing comparison guard.");}
 }
 static void W16(byte[] b,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(o),v);
 static void W32(byte[] b,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(o),v);
 static void W64(byte[] b,int o,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(o),v);
}
