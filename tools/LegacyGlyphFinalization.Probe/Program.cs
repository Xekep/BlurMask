using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json");
using var file=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int controls=0,applied=0,refused=0,failedProducers=0,guards=0;
const ulong key=2UL|(1UL<<24);
foreach(var test in file.RootElement.EnumerateArray())
{
    byte[] before=Bytes(test,"before"),other=Bytes(test,"other"),source=Bytes(test,"source");
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    ulong reference=pool.AppendDescriptor(key,123,false,new byte[40]).Reference;
    ulong second=pool.AppendDescriptor(key,124,false,new byte[40]).Reference;
    pool.ReplaceRecord(reference,before);pool.ReplaceRecord(second,other);
    ushort[] scratch=test.GetProperty("labels").EnumerateArray().Select(v=>v.GetUInt16()).ToArray();
    var prepared=new LegacyPreparedGlyphFinalizationInput(test.GetProperty("success").GetBoolean(),test.GetProperty("cursor").GetInt32(),
        scratch,test.GetProperty("flag").GetByte(),test.GetProperty("external").GetByte(),test.GetProperty("score0C").GetUInt32(),
        test.GetProperty("score10").GetUInt32(),test.GetProperty("cost").GetInt32(),test.GetProperty("pub").GetUInt16());
    //Input owns its scratch copy. Mutating the caller cannot change symbol0
    //or the untouched string scratch; the native finalizer writes scratch0
    //only in its own temporary, not this external original array.
    Array.Fill(scratch,(ushort)0x7777);
    bool result=false,rejected=false;
    try{result=LegacyPreparedGlyphFinalization.Apply(pool,reference,source,prepared,false);}
    catch(NotSupportedException){rejected=true;refused++;}
    Require(result==test.GetProperty("result").GetBoolean()&&rejected==test.GetProperty("reject").GetBoolean(),"Independent status/cursor/macro branch differs.");
    Require(pool.CopyRecord(reference).AsSpan().SequenceEqual(Bytes(test,"expected"))&&pool.CopyRecord(second).AsSpan().SequenceEqual(other),
        "Complete glyph40 or neighbouring shared-pool record changed.");
    var published=new LegacyNativeGlyphRecordOwner(pool.CopyOwnerNodes()).Resolve(reference).ReadRecord();
    Require(published.AsSpan().SequenceEqual(Bytes(test,"expected")),"Published immutable owner differs from mutable pool.");
    if(result)applied++;if(!prepared.ProducerSucceeded)failedProducers++;
    //Before-entry cancellation never mutates either record. A cancellation
    //after symbol application is explicitly not promised transactional.
    byte[] snapshot=pool.CopyRecord(reference);
    Reject(()=>LegacyPreparedGlyphFinalization.Apply(pool,reference,source,prepared,false,new(true)));
    Require(pool.CopyRecord(reference).AsSpan().SequenceEqual(snapshot),"Canceled entry wrote a partial row.");
    controls++;
}
var guardPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
ulong guardReference=guardPool.AppendDescriptor(key,123,false,new byte[40]).Reference;
ushort[] labels=Enumerable.Repeat((ushort)0xfff9,256).ToArray();
var normal=new LegacyPreparedGlyphFinalizationInput(true,255,labels,0,0,0,0,0,0);
Reject(()=>new LegacyPreparedGlyphFinalizationInput(true,255,labels.AsSpan(1),0,0,0,0,0,0));
Reject(()=>LegacyPreparedGlyphFinalization.Apply(guardPool,guardReference,new byte[71],normal,false));
Reject(()=>LegacyPreparedGlyphFinalization.Apply(guardPool,guardReference,new byte[72],normal,true));
Reject(()=>LegacyPreparedGlyphFinalization.Apply(guardPool,guardReference|(5UL<<40),new byte[72],normal,false));
Reject(()=>LegacyPreparedGlyphFinalization.Apply(guardPool,999,new byte[72],normal,false));
//A failed216BC0 output consumes neither source72 nor pool-reference fields.
var failed=new LegacyPreparedGlyphFinalizationInput(false,int.MinValue,[],0,0,0,0,0,0);
Require(!LegacyPreparedGlyphFinalization.Apply(guardPool,999,[],failed,false),"Failed producer accessed an unused pool reference/source row.");
//Existing partial state is not rolled back by a later failed/canceled call.
//This is a deterministic retained-prefix check, not an injected claim that a
//native cancellation callback exists between the two stores.
Require(LegacyNativeGlyphSymbolUpdate.Apply(guardPool,guardReference,55,1,0,true),"Partial symbol update failed.");
byte[] partial=guardPool.CopyRecord(guardReference);
Require(!LegacyPreparedGlyphFinalization.Apply(guardPool,guardReference,[],failed,false),"Failed producer rewrote existing partial state.");
Reject(()=>LegacyPreparedGlyphFinalization.Apply(guardPool,guardReference,new byte[72],normal,false,new(true)));
Require(guardPool.CopyRecord(guardReference).AsSpan().SequenceEqual(partial)&&
    (BinaryPrimitives.ReadUInt16LittleEndian(partial.AsSpan(4))&7)==4,
    "Canceled/failed entry rolled back a previously committed partial symbol prefix.");
Console.WriteLine($"PASS {controls} independent full40/raw-float/symbol/macro/countwrap finalizations: {applied} applied/{refused} native-OOB scope refusals/{failedProducers} failed producers; shared-pool/publication/scratch ownership, {guards} cancellation/extent/reference/trace guards.216BC0 producer and full218560 remain explicit gaps.");

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid finalizer input accepted.");}
static byte[] Bytes(JsonElement row,string name)=>Convert.FromHexString(row.GetProperty(name).GetString()!);
static void Require(bool value,string text){if(!value)throw new Exception(text);}
