using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

if(args.Length!=1)throw new ArgumentException("capture.json");
var labels=new LegacyContextualQuoteLabels(10,11,20,21,22,23,24);
int checks=0;
//Exhaust every ushort to check the native wrapped four-label interval.
for(int n=0;n<=ushort.MaxValue;n++)
{
    ushort value=(ushort)n;bool control=n>=65529&&n<=65532;
    Check(LegacyPreparedContextualQuoteRules.Leading(11,value,0,0,labels)==(control?11:20));
    Check(LegacyPreparedContextualQuoteRules.Trailing(value,11,0,0,false,labels)==(control?11:21));
    Check(LegacyPreparedContextualQuoteRules.Trailing(value,11,0,0,true,labels)==(control?11:20));
}
using var output=File.Create(args[0]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("cases");
foreach(ushort current in new ushort[]{10,11,24,30})
foreach(ushort previous in new ushort[]{0,31,99,100,65528,65529,65530,65531,65532,65533,65535})
foreach(ushort next in new ushort[]{0,65528,65529,65530,65531,65532,65533,65535})
foreach(int category in new[]{0,1,2})
foreach(bool carry in new[]{false,true})
foreach(short arc in new short[]{-32768,-1,0,32767})
foreach(byte kind in new byte[]{0,1,255})
{
    var result=LegacyPreparedContextualQuoteRules.Interior(previous,current,next,arc,kind,100,category,carry,labels);
    w.WriteStartArray();w.WriteNumberValue(previous);w.WriteNumberValue(current);w.WriteNumberValue(next);
    w.WriteNumberValue(category);w.WriteBooleanValue(carry);w.WriteNumberValue(arc);w.WriteNumberValue(kind);
    w.WriteNumberValue(result.Label);w.WriteBooleanValue(result.Carry);w.WriteEndArray();
    //Every non-quote and every disallowed source must preserve its scalar.
    if(current is not(10 or 11)||arc<0||kind!=0)Check(result.Label==current);
}
w.WriteEndArray();
//OpenDouble alone does not pass the native three-field entry guard.
Check(!new LegacyContextualQuoteLabels(10,11,20,65535,65535,65535,24).HasEnabledReplacement);
Check(labels.HasEnabledReplacement);
Check(!new LegacyContextualQuoteLabels(10,11,65535,65535,65535,65535,24).HasEnabledReplacement);
w.WriteStartArray("cursorCases");
for(int test=0;test<64;test++)
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    var words=new List<LegacyNativePlainWordRecord>();
    var inputs=new List<ulong[]>();int identity=1;
    foreach(int count in new[]{0,test%5,0,1+(test/5)%4,0})
    {
        var refs=new List<ulong>();
        for(int index=0;index<count;index++)
        {
            var inserted=pool.AppendDescriptor((ulong)identity,identity,false,new byte[40]);identity++;
            byte[] row=pool.CopyRecord(inserted.Reference);
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),0);
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),(ushort)(test*8+index));
            pool.ReplaceRecord(inserted.Reference,row);refs.Add(inserted.Reference);
        }
        //Explicit prepared WORD/vector controls; no recognized word is claimed.
        ulong[] vector=refs.ToArray();words.Add(new(pool,new byte[120],vector));inputs.Add(vector);
    }
    var cursor=new LegacyPreparedPublishedGlyphCursor(pool,words);
    byte[] scratch=[0xc1,0x4e,0xa5,0x73];
    w.WriteStartObject();w.WriteStartArray("words");
    foreach(var vector in inputs){w.WriteStartArray();foreach(ulong reference in vector)w.WriteNumberValue(reference);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("reads");
    foreach(ulong expected in inputs.SelectMany(v=>v))
    {
        var read=cursor.Next(scratch);Check(read.HasReference&&read.Reference==expected);
        w.WriteStartArray();w.WriteNumberValue(read.Reference);w.WriteNumberValue(read.WordOrdinal);w.WriteNumberValue(read.ReferenceOrdinal);w.WriteNumberValue(read.Label);w.WriteEndArray();
    }
    w.WriteEndArray();byte[] before=scratch.ToArray();
    for(int retry=0;retry<2;retry++)
    {
        var end=cursor.Next(scratch);Check(!end.HasReference&&end.Reference==0&&end.Label==65535&&scratch.SequenceEqual(before));
    }
    w.WriteEndObject();
}
w.WriteEndArray();
//An unsupported scalar read consumes its reference before failure. The
//portable cursor retains that prefix and refuses an implicit retry.
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    var inserted=pool.AppendDescriptor(1,1,false,new byte[40]);byte[] row=pool.CopyRecord(inserted.Reference);
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),3);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),4);pool.ReplaceRecord(inserted.Reference,row);
    var cursor=new LegacyPreparedPublishedGlyphCursor(pool,[new(pool,new byte[120],[inserted.Reference])]);
    bool failed=false;try{cursor.Next(new byte[4]);}catch(ArgumentException){failed=true;}Check(failed);
    failed=false;try{cursor.Next(new byte[4]);}catch(InvalidOperationException){failed=true;}Check(failed);
}
w.WriteNumber("checks",checks);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {checks} contextual quote wrap/source/entry guards; enabled-body numeric decisions only, no app policy change.");
void Check(bool ok){if(!ok)throw new Exception("Contextual quote numeric rule changed.");checks++;}
