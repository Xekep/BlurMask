using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json required");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
void Require(bool v,string message){if(!v)throw new InvalidOperationException(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){return;}throw new InvalidOperationException("Invalid WORD276 input accepted.");}
int cases=0,references=0,zeroEndpoints=0;
foreach(var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    var word=LegacyNativeWordBuilder.CreateEmpty(pool,Convert.FromBase64String(test.GetProperty("storage").GetString()!));
    ulong previous=ulong.MaxValue;
    foreach(var glyph in test.GetProperty("glyphs").EnumerateArray())
    {
        var result=word.AppendGlyph(glyph.GetProperty("key").GetUInt64(),glyph.GetProperty("descriptor").GetInt32(),false,new byte[40]);
        if(previous!=ulong.MaxValue)word.LinkGlyph(previous,result.Reference);
        word.SetLastOrdinarySymbol(0); //Exercises276.38 on the actual last linked reference.
        byte[] row=pool.CopyRecord(result.Reference);
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),glyph.GetProperty("kind").GetUInt16());
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(6),glyph.GetProperty("publication").GetUInt16());
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),glyph.GetProperty("symbol").GetUInt16());
        pool.ReplaceRecord(result.Reference,row);previous=result.Reference;
    }
    var scalar=test.GetProperty("scalars");
    word.SetRawScalars(BitConverter.UInt32BitsToSingle(scalar[0].GetUInt32()),BitConverter.UInt32BitsToSingle(scalar[1].GetUInt32()),BitConverter.UInt32BitsToSingle(scalar[2].GetUInt32()),BitConverter.UInt32BitsToSingle(scalar[3].GetUInt32()));
    ulong[] expected=test.GetProperty("reading").EnumerateArray().Select(x=>x.GetUInt64()).ToArray();
    Require(word.GetReferenceCount()==expected.Length&&word.GetSymbolSpan()==test.GetProperty("spanCount").GetInt32(),"Native reference count vs symbol-span count differs.");
    Require(Enumerable.Range(0,expected.Length).Select(i=>word.GetReadingOrderReference(i)).SequenceEqual(expected),"Native reverse-linked reading order lost zero-span references.");
    Require(word.CopyStorage().AsSpan().SequenceEqual(Convert.FromBase64String(test.GetProperty("raw").GetString()!)),"WORD276 constructor/opaque/rawscalar bytes differ.");
    var span=word.GetSourceSpan((n,a)=>n*100+a,(n,a)=>n*100+a+7);
    Require(span==new LegacyNativeWordSourceSpan(test.GetProperty("first").GetInt32(),test.GetProperty("last").GetInt32()),"Native first/last source-boundary/key policy differs.");
    var publisher=new LegacyNativePlainWordPublisher(pool,test.GetProperty("ownerc4").GetBoolean(),false,false);
    var node=publisher.Publish(word,(n,a)=>n*100+a,(n,a)=>n*100+a+7);
    Require(node.Word.CopyStorage().AsSpan().SequenceEqual(Convert.FromBase64String(test.GetProperty("published").GetString()!))&&node.Word.FlatReferences.SequenceEqual(expected),"275 plain flatten/flags/capacity/source owner differs.");
    Require(node.Word.GetReferenceCount()==expected.Length&&node.Word.GetSymbolSpan()==test.GetProperty("spanCount").GetInt32()&&
        Enumerable.Range(0,expected.Length).Select(i=>node.Word.GetReadingOrderReference(i)).SequenceEqual(expected),"Published275 word no longer implements276 count/span/nth-reference consumers.");
    word.SetRawScalars(999,998,997,996);
    var repeated=publisher.Publish(word,(n,a)=>n*100+a,(n,a)=>n*100+a+7);
    Require(repeated.WordIndex==0&&ReferenceEquals(repeated.Word,node.Word)&&ReferenceEquals(repeated.Next,node),"279 exacthead dedup/scalar preservation/prepend lifecycle differs.");
    var snapshot=node.Word.CopyStorage();snapshot.AsSpan().Clear();
    Require(node.Word.CopyStorage().AsSpan().SequenceEqual(Convert.FromBase64String(test.GetProperty("published").GetString()!)),"Published word exposes mutable header.");
    if(test.GetProperty("glyphs")[0].GetProperty("kind").GetUInt16()==1&&((test.GetProperty("glyphs")[0].GetProperty("symbol").GetUInt16()>>2)&0x1fff)==0)zeroEndpoints++;
    references+=expected.Length;cases++;
}
var guardPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var empty=LegacyNativeWordBuilder.CreateEmpty(guardPool,new byte[120]);
Require(empty.GetReferenceCount()==0&&empty.GetSymbolSpan()==0,"Empty builder fabricated glyphs.");
Reject(()=>empty.GetReadingOrderReference(0));Reject(()=>empty.GetSourceSpan((n,a)=>0,(n,a)=>0));
Reject(()=>LegacyNativeWordBuilder.CreateEmpty(guardPool,new byte[119]));
Reject(()=>new LegacyNativePlainWordPublisher(guardPool,false,true,false));
Reject(()=>new LegacyNativePlainWordPublisher(guardPool,false,false,true));
var inserted=empty.AppendGlyph(0,17,false,new byte[40]);empty.SetLastOrdinarySymbol(4);
Require(inserted.Reference==0&&empty.GetReferenceCount()==1,"Zero reference was mistaken for empty sentinel.");
var other=LegacyNativeWordBuilder.CreateEmpty(LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false),new byte[120]);other.AppendGlyph(0,17,false,new byte[40]);
var publisherGuard=new LegacyNativePlainWordPublisher(guardPool,false,false,false);
Reject(()=>publisherGuard.Publish(other,(n,a)=>0,(n,a)=>0));
using(var cancellation=new CancellationTokenSource())
{
    int lastCalls=0;
    Reject(()=>publisherGuard.Publish(empty,(n,a)=>{cancellation.Cancel();return 1;},(n,a)=>{lastCalls++;return 2;},cancellation.Token));
    Require(lastCalls==0&&publisherGuard.Head is null,"Boundary cancellation reached lastcallback or mutated output.");
}
empty.LinkGlyph(inserted.Reference,inserted.Reference);Reject(()=>empty.GetReferenceCount());
Require(zeroEndpoints>0,"Zero-span leading/trailing controls omitted.");
Console.WriteLine($"PASS {cases} independent WORD276/publisher controls / {references} glyph references / {zeroEndpoints} zero-span endpoint cases; descriptor-preserving links, readingorder, rawscalars, actualcallback boundaries, head dedup, ownerflags, snapshots and cancellation. Prepared references/explicit false geometry flags; not full273/rawRussian materialization.");
