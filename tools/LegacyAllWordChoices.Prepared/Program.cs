using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("REPORT");
var checks=new List<string>();
void Check(bool v,string n){if(!v)throw new InvalidDataException(n);checks.Add(n);}
void Refuse(Action a,Type type,string n){try{a();}catch(Exception e){Check(e.GetType()==type,n);return;}throw new InvalidDataException(n);}
foreach(int n in new[]{0,1,2,3,4,127,4096})
{
 var b=LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(0xa51033,n);
 var expected=new byte[48];BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8),0xa51033);
 BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(24),n);BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(28),n);
 Check(b.AsSpan().SequenceEqual(expected),"choice48-exact-"+n);
}
Refuse(()=>LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(0,-1),typeof(ArgumentOutOfRangeException),"negative-linkcount");
Refuse(()=>LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(0,4097),typeof(ArgumentOutOfRangeException),"too-many-links");
uint[] values=[0,1,8,0x100000,0x200000,0x300000,0xffffffff];
foreach(uint prior in values)foreach(uint defaults in values)foreach(uint marker in values)
{
 //Independent literal instruction-register transcription: MOV EDX,R8; BTR R8D,20; OR/AND/XOR.
 uint r8=prior|defaults,edx=r8;r8&=0xffefffff;edx|=marker;edx&=0x100000;edx^=r8;
 Check(LegacyFreshMainGlobalWordSourceChoice.FoldFlags(prior,defaults,marker)==edx,"literal-default-tail-"+prior+"-"+defaults+"-"+marker);
}
foreach(uint prior in values)foreach(uint marker in values)
 Check(LegacyFreshMainGlobalWordSourceChoice.FoldFlags(prior,marker,marker)==(prior|marker),"admitted-marker-union-"+prior+"-"+marker);
Check(LegacyFreshMainGlobalWordSourceChoice.FoldFlags(0x100000,0,0)==0x100000,"prior-bit20-must-survive-zero-next");
var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
var words=new List<LegacyNativeWordBuilder>();var span=new LegacyNativeWordSourceSpan(0,9);
LegacyNativeWordBuilder Add(int i)
{
 var b=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);var g=b.AppendGlyph((ulong)i,i,true,new byte[40]);
 var bytes=pool.CopyRecord(g.Reference);BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4),0);pool.ReplaceRecord(g.Reference,bytes);
 b.SetRawScalars(i,0,0,0);publisher.InternPlainWord(b,span,out _);words.Add(b);return b;
}
Add(1);Add(2);var node=publisher.Publish(words[0],(_,_)=>0,(_,_)=>9);
var before=publisher.CaptureSpan(node,[node]);publisher.RequireLiveSpanRows(before,default);Check(node.WordIndex==0&&before.Words.Count==2,"genuine-a2-top1-readonly");
Refuse(()=>publisher.RequireLiveSpanRows(before,new CancellationToken(true)),typeof(OperationCanceledException),"precancel-readonly");
publisher.RequireLiveSpanRows(before,default);Check(node.WordIndex==0,"cancel-does-not-consume");
var otherPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var other=new LegacyNativePlainWordPublisher(otherPool,false,false,false);
Refuse(()=>other.RequireLiveSpanRows(before,default),typeof(InvalidOperationException),"foreign-publisher-list");
var scored=before.Words.Select(w=>w.CopyStorage()).ToArray();BinaryPrimitives.WriteSingleLittleEndian(scored[1].AsSpan(16),123);
publisher.ApplySpanScores(before,scored);
Refuse(()=>publisher.RequireLiveSpanRows(before,default),typeof(InvalidOperationException),"stale-scored-row-refused");
var after=publisher.CaptureSpan(node,[node]);publisher.RequireLiveSpanRows(after,default);
Add(3);Refuse(()=>publisher.RequireLiveSpanRows(after,default),typeof(InvalidOperationException),"stale-appended-count-refused");
Check(node.WordIndex==0,"no-argmax-no-top-mutation");
File.WriteAllText(args[0],JsonSerializer.Serialize(new{Scope="PURE prepared scalar/layout and real publisher row-binding controls; NO completedScope authority/NN/TEXT",Count=checks.Count,Checks=checks},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine("PASS "+checks.Count+" prepared controls; no NN");
