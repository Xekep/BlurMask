using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
using Raw=InkRecognition.LegacyMathMixed107RawResource;

if(args.Length!=2)throw new ArgumentException("Actual math-ak.res and output.json required.");
byte[] resource=File.ReadAllBytes(args[0]);var checks=new List<string>();
void Check(bool value,string name){if(!value)throw new InvalidOperationException(name);checks.Add(name);}
void Expect<T>(Action body,string name)where T:Exception
{try{body();}catch(T){checks.Add(name+":"+typeof(T).Name);return;}throw new InvalidOperationException(name+" did not reject");}
var events=new List<(Raw.Phase Phase,int Model,int Offset)>();
using var actual=Raw.LoadWithDiagnosticObserver(resource,(p,m,n)=>events.Add((p,m,n)));
var snapshots=actual.Models.Select(m=>m.Capture()).ToArray();Check(snapshots.Length==6,"actual-six");
var expected=snapshots.Select(s=>SHA256.HashData(s.CopyEnvelope())).ToArray();
for(int i=0;i<snapshots.Length;i++)
{
    var s=snapshots[i];Check(s.NativeRawTypeId==0x215&&s.Quantizer==10&&s.HiddenRows==200&&s.OutputRows==257,"raw-domain-"+i);
    Check(s.CopyEnvelope().AsSpan().SequenceEqual(resource.AsSpan(s.OriginalStart,s.OriginalEnd-s.OriginalStart)),"complete-envelope-"+i);
    short[] first=s.CopyFirstWeights();sbyte[] second=s.CopySecondWeights();byte[] scales=s.CopyScales();
    first[0]^=32767;second[0]^=127;scales[0]=0;byte[] changed=s.CopyEnvelope();changed[0]=0;
    byte[] align=s.CopyFirstAlignment();if(align.Length>0)align[0]=0;
    Check(SHA256.HashData(s.CopyEnvelope()).AsSpan().SequenceEqual(expected[i]),"detached-mutations-"+i);
    using var canceled=new CancellationTokenSource();canceled.Cancel();
    Expect<OperationCanceledException>(()=>s.CopyFirstWeights(canceled.Token),"snapshot-first-cancel-"+i);
    Expect<OperationCanceledException>(()=>s.CopyEnvelope(canceled.Token),"snapshot-envelope-cancel-"+i);
}
Expect<ArgumentException>(()=>Raw.LoadActualResource([]),"empty");
Expect<ArgumentException>(()=>Raw.LoadActualResource(resource.AsSpan(1)),"short-source");
using(var canceled=new CancellationTokenSource()){canceled.Cancel();Expect<OperationCanceledException>(()=>Raw.LoadActualResource(resource,canceled.Token),"entry-cancel");}
void Change(int at,uint value,int bytes,Type error,string name)
{
    byte[] d=resource.ToArray();if(bytes==1)d[at]=(byte)value;else if(bytes==2)BinaryPrimitives.WriteUInt16LittleEndian(d.AsSpan(at,2),(ushort)value);else BinaryPrimitives.WriteUInt32LittleEndian(d.AsSpan(at,4),value);
    bool copied=false;
    try{using var unused=Raw.LoadWithDiagnosticObserver(d,(p,_,_)=>{if(p is Raw.Phase.FirstWeights or Raw.Phase.SecondWeights)copied=true;});}
    catch(Exception e)when(e.GetType()==error){Check(!copied,name+"-preflight-before-payload");checks.Add(name+":"+error.Name);return;}
    throw new InvalidOperationException("Unexpected malformed result: "+name);
}
Change(0,0,4,typeof(InvalidDataException),"outer-frame");Change(16,0,2,typeof(NotSupportedException),"base-version");
Change(7448,0,1,typeof(InvalidDataException),"missing-configured-model");
for(int i=0;i<snapshots.Length;i++)
{
    int a=snapshots[i].OriginalStart;
    Change(a,0,2,typeof(InvalidDataException),"magic-"+i);Change(a+2,0x106,2,typeof(InvalidDataException),"version-"+i);
    Change(a+4,0xffffffff,4,typeof(InvalidDataException),"envelope-end-"+i);Change(a+8,0x12,1,typeof(NotSupportedException),"old-architecture-"+i);
    Change(a+9,0,2,typeof(InvalidDataException),"foreign-kind-"+i);
    foreach(int field in new[]{11,15,27})foreach(uint value in new uint[]{0,1,4097,uint.MaxValue})
        Change(a+field,value,4,typeof(InvalidDataException),"width-"+i+"-"+field+"-"+value);
    foreach(int field in new[]{19,23})Change(a+field,1,4,typeof(NotSupportedException),"middle-"+i+"-"+field);
    Change(a+31,9,2,typeof(NotSupportedException),"quantizer-"+i);
    Change(snapshots[i].OriginalEnd-258,2,1,typeof(InvalidDataException),"leading-scale-"+i);
    Change(snapshots[i].OriginalEnd-1,0,1,typeof(InvalidDataException),"zero-final-scale-"+i);
    Change(snapshots[i].FirstWeightStart,(uint)(resource[snapshots[i].FirstWeightStart]^0x55),1,typeof(InvalidDataException),"foreign-weight-fingerprint-"+i);
}
//Every observed boundary is canceled and separately throws before publication.
for(int stop=1;stop<=events.Count;stop++)
{
    using var cts=new CancellationTokenSource();int count=0;
    Expect<OperationCanceledException>(()=>{using var unused=Raw.LoadWithDiagnosticObserver(resource,(_,_,_)=>{if(++count==stop)cts.Cancel();},cts.Token);},"cancel-prefix-"+stop);
    Check(count==stop,"cancel-stopped-"+stop);count=0;
    Expect<InjectedException>(()=>{using var unused=Raw.LoadWithDiagnosticObserver(resource,(_,_,_)=>{if(++count==stop)throw new InjectedException();});},"throw-prefix-"+stop);
    Check(count==stop,"throw-stopped-"+stop);
}
byte[] mutable=resource.ToArray();bool altered=false;
using(var detached=Raw.LoadWithDiagnosticObserver(mutable,(p,_,_)=>{if(p==Raw.Phase.Hash&&!altered){Array.Clear(mutable);altered=true;}}))
{Check(altered,"original-mutated-after-capture");Check(detached.Models[0].Capture().CopyEnvelope().AsSpan().SequenceEqual(snapshots[0].CopyEnvelope()),"captured-source-isolation");}
bool reentered=false;
using(var outer=Raw.LoadWithDiagnosticObserver(resource,(p,_,_)=>{if(p==Raw.Phase.Publish&&!reentered){reentered=true;using var inner=Raw.LoadActualResource(resource);if(inner.Models.Count!=6)throw new InvalidOperationException();}}))Check(reentered,"diagnostic-reentry");
for(int pass=0;pass<3;pass++)
{
    using var owner=Raw.LoadActualResource(resource);var models=owner.Models.ToArray();using var start=new ManualResetEventSlim();int bad=0;
    Task[] readers=Enumerable.Range(0,4).Select(worker=>Task.Run(()=>
    {start.Wait();for(int n=0;n<12;n++){int index=(worker+n)%6;try{var s=models[index].Capture();if(!SHA256.HashData(s.CopyEnvelope()).AsSpan().SequenceEqual(expected[index]))Interlocked.Increment(ref bad);}catch(ObjectDisposedException){}}})).ToArray();
    Task closer=Task.Run(()=>{start.Wait();owner.Dispose();});start.Set();Task.WaitAll(readers.Append(closer).ToArray());
    Check(bad==0&&owner.IsDisposed,"parallel-copy-dispose-"+pass);
    Expect<ObjectDisposedException>(()=>models[0].Capture(),"closed-model-"+pass);
    Expect<ObjectDisposedException>(()=>{_ = owner.Models;},"closed-root-"+pass);owner.Dispose();
}
var historical=MakeHistorical(resource);for(int n=0;n<3&&historical.Weak.IsAlive;n++){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
Check(!historical.Weak.IsAlive,"snapshot-does-not-retain-root");
Check(SHA256.HashData(historical.Snapshot.CopyEnvelope()).AsSpan().SequenceEqual(expected[0]),"snapshot-after-root-collection");
actual.Dispose();Check(snapshots[5].CopyEnvelope().Length>0,"historical-after-dispose");
using(var stream=File.Create(args[1]))using(var w=new Utf8JsonWriter(stream,new(){Indented=true}))
{
    w.WriteStartObject();w.WriteBoolean("vendorExecution",false);w.WriteBoolean("inference",false);w.WriteString("resource",Raw.ResourceFingerprint);
    w.WriteNumber("boundaryCount",events.Count);w.WriteStartArray("boundaries");foreach(var e in events)w.WriteStringValue($"{e.Phase}:{e.Model}:{e.Offset}");w.WriteEndArray();
    w.WriteStartArray("checks");foreach(string c in checks)w.WriteStringValue(c);w.WriteEndArray();w.WriteStartArray("models");
    using var whole=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    foreach(var s in snapshots)
    {
        short[] f=s.CopyFirstWeights();byte[] first=new byte[f.Length*2];for(int n=0;n<f.Length;n++)BinaryPrimitives.WriteInt16LittleEndian(first.AsSpan(n*2,2),f[n]);
        sbyte[] v=s.CopySecondWeights();byte[] second=new byte[v.Length];for(int n=0;n<v.Length;n++)second[n]=unchecked((byte)v[n]);byte[] scales=s.CopyScales(),envelope=s.CopyEnvelope();
        byte[] header=new byte[32];int[] fields=[s.OriginalStart,s.OriginalEnd,s.Kind,s.InputWidth,s.FirstStride,s.SecondStride,s.Containing13A5Start,s.Containing13A5End];for(int n=0;n<fields.Length;n++)BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(n*4,4),fields[n]);
        whole.AppendData(header);whole.AppendData(first);whole.AppendData(second);whole.AppendData(scales);
        w.WriteStartObject();w.WriteNumber("start",s.OriginalStart);w.WriteNumber("end",s.OriginalEnd);w.WriteNumber("kind",s.Kind);w.WriteNumber("inputs",s.InputWidth);w.WriteNumber("firstStride",s.FirstStride);w.WriteNumber("secondStride",s.SecondStride);w.WriteNumber("parentStart",s.Containing13A5Start);w.WriteNumber("parentEnd",s.Containing13A5End);
        w.WriteBase64String("first",first);w.WriteBase64String("second",second);w.WriteBase64String("scales",scales);w.WriteBase64String("firstAlignment",s.CopyFirstAlignment());w.WriteBase64String("secondAlignment",s.CopySecondAlignment());w.WriteBase64String("envelope",envelope);w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteString("completeDigest",Convert.ToHexString(whole.GetHashAndReset()).ToLowerInvariant());w.WriteEndObject();
}
Console.WriteLine($"PASS6 owned raw215 models/{events.Count} cancellation+throw prefixes/{checks.Count} checks; no inference");

[MethodImpl(MethodImplOptions.NoInlining)]
static (WeakReference Weak,Raw.Snapshot Snapshot) MakeHistorical(byte[] source)
{var owner=Raw.LoadActualResource(source);var snapshot=owner.Models[0].Capture();var weak=new WeakReference(owner);owner.Dispose();return(weak,snapshot);}
sealed class InjectedException:Exception;
