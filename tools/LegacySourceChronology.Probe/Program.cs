using InkRecognition;
using System.Text.Json;
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));int kernels=0,sources=0,identities=0,orders=0;
foreach(var t in json.RootElement.GetProperty("kernels").EnumerateArray())
{
    uint[] incoming=U(t.GetProperty("incoming")),previous=U(t.GetProperty("previous"));
    uint[] result=LegacySourcePointChronology.ComposePointMap(incoming,t.GetProperty("factor").GetUInt32(),previous,t.GetProperty("ratio").GetUInt32());
    Require(result.AsSpan().SequenceEqual(U(t.GetProperty("expected"))),"Unsigned wrapped point-map composition differs");kernels++;
}
foreach(var t in json.RootElement.GetProperty("sources").EnumerateArray())
{
    int[] counts=I(t.GetProperty("counts")),keyMap=I(t.GetProperty("keyMap"));uint[] pointMap=U(t.GetProperty("pointMap"));
    var keys=t.GetProperty("keys").EnumerateArray().Select(k=>new LegacySourcePointKey(k[0].GetInt32(),k[1].GetInt32(),k[2].GetInt32())).ToArray();
    var source=LegacySourcePointChronology.CreatePrepared(counts,keys,keyMap,pointMap,t.GetProperty("ratio").GetUInt32());
    int global=0;for(int s=0;s<counts.Length;s++)for(int p=0;p<counts[s];p++)
    {Check(source.Resolve(s,p),t.GetProperty("identities")[global++]);identities++;}
    var ranges=t.GetProperty("ranges").EnumerateArray().Select(r=>new LegacyInkRange(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32())).ToArray();
    int[] expected=I(t.GetProperty("order"));
    Require(source.OrderRangeIndicesForNormalization(ranges).AsSpan().SequenceEqual(expected),"Signed key quicksort/tie encounter order differs");
    var atoms=ranges.Select(r=>new LegacyCursiveInkAtom(default,r)).ToArray();
    Require(source.OrderAtomIndicesForNormalization(atoms).AsSpan().SequenceEqual(expected),"Atom start-key order differs");orders++;
    uint[] incoming=U(t.GetProperty("incoming"));var remapped=source.RemapPreparedPoints(new[]{incoming.Length},incoming);
    Require(remapped.PreparedPointMap.Span.SequenceEqual(U(t.GetProperty("remapped"))),"Prepared source remap differs");
    for(int p=0;p<incoming.Length;p++){Check(remapped.Resolve(0,p),t.GetProperty("remappedIdentities")[p]);identities++;}
    var before=source.Resolve(0,0);keys[0]=default;keyMap[0]=0;if(pointMap.Length>0)pointMap[0]=0;counts[0]=1000;
    Require(source.Resolve(0,0)==before,"Source metadata inputs were not copied");sources++;
}
// Inserted cut-point repeats prior original chronology, including prior remapping.
var raw=new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(2,2),new InkPoint(4,0)}),new InkStroke(new[]{new InkPoint(7,0),new InkPoint(8,1)})};
var ink=LegacyCursivePreparedInk.Create(raw).Insert(0,1,new InkPoint(1,1),2).Insert(0,2,new InkPoint(1.5f,1.5f),2);
var ordinary=LegacySourcePointChronology.CreateOrdinary(new[]{3,2});var inserted=ordinary.WithInsertedPreparedInk(ink);
Require(inserted.Resolve(0,0).Key==new LegacySourcePointKey(0,0,0) && inserted.Resolve(0,1)==inserted.Resolve(0,0) && inserted.Resolve(0,2)==inserted.Resolve(0,0),"Synthetic cut-point chronology identity lost");
Require(inserted.Resolve(1,1).Key==new LegacySourcePointKey(0,1,1),"Later physical stroke chronology changed");
var shuffled=new[]{new LegacyInkRange(1,0,2),new LegacyInkRange(0,3,5),new LegacyInkRange(0,0,2)};
Require(inserted.OrderRangeIndicesForNormalization(shuffled).AsSpan().SequenceEqual(new[]{2,1,0}),"Ordinary prepared range chronology differs");
var tie=inserted.OrderRangeIndicesForNormalization(new[]{new LegacyInkRange(0,0,1),new LegacyInkRange(0,1,2),new LegacyInkRange(0,2,3)});
Require(tie.AsSpan().SequenceEqual(new[]{2,1,0}),"Equal-key native quicksort swaps were replaced with stable order");
// Native ceiling and low-byte remainder are observable even for factor512.
var fractional=LegacySourcePointChronology.CreatePrepared(new[]{3},new[]{new LegacySourcePointKey(0,0,0),new LegacySourcePointKey(0,0,1)},new[]{0,1},new uint[]{0,1,256},512);
Require(fractional.Resolve(0,1).MetadataPointIndex==1 && fractional.Resolve(0,1).RemainderByte==255 && fractional.Resolve(0,2).RemainderByte==0,"Ceiling or low-byte remainder differs");
// Keep explicit unit1 integer truncation independent of native ordinary200.
var explicitUnit=LegacySourcePointChronology.CreatePrepared(new[]{3,2},
    new[]{new LegacySourcePointKey(0,0,0),new LegacySourcePointKey(0,0,1),new LegacySourcePointKey(0,0,2),
        new LegacySourcePointKey(0,1,0),new LegacySourcePointKey(0,1,1)},new[]{0,1,2,3,4},sourceFactor:1);
var sample=explicitUnit.RemapPreparedPoints(new[]{3},new uint[]{0,1,4});
var second=sample.RemapPreparedPoints(new[]{3},new uint[]{0,1,3},2);
Require(second.PreparedPointMap.Span.SequenceEqual(new uint[]{0,0,2}),"Fractional explicit source remap integer truncation differs");
// Native16A130 source factor200;16AA00 composes in the retained unit.
var ordinarySample=ordinary.RemapPreparedPoints(new[]{3},new uint[]{0,1,4});
Require(ordinarySample.SourceFactor==200 && ordinarySample.PreparedPointMap.Span.SequenceEqual(new uint[]{0,200,800}),"Native ordinary200 initial map unit differs");
var ordinarySecond=ordinarySample.RemapPreparedPoints(new[]{3},new uint[]{0,1,3},2);
Require(ordinarySecond.PreparedPointMap.Span.SequenceEqual(new uint[]{0,100,500}),"Native ordinary200 fractional source remap differs");
Require(ordinarySecond.Resolve(0,1).MetadataPointIndex==1 && ordinarySecond.Resolve(0,1).RemainderByte==100 &&
    ordinarySecond.Resolve(0,2).MetadataPointIndex==3 && ordinarySecond.Resolve(0,2).RemainderByte==100 &&
    ordinarySecond.Resolve(0,2).Key==new LegacySourcePointKey(0,1,0),"Native ordinary200 ceiling/remainder/key ownership differs");
Reject(()=>LegacySourcePointChronology.ComposePointMap(new uint[]{1},0,[],1));
Reject(()=>LegacySourcePointChronology.ComposePointMap(new uint[]{3},2,new uint[]{0,1},1));
Reject(()=>ordinary.RemapPreparedPoints(new[]{1},new uint[]{0},0));
Reject(()=>ordinary.Resolve(0,3));Reject(()=>ordinary.OrderRangeIndicesForNormalization(new[]{new LegacyInkRange(0,0,4)}));
Reject(()=>LegacySourcePointChronology.CreatePrepared(new[]{1},new[]{default(LegacySourcePointKey)},new[]{1}));
Reject(()=>LegacySourcePointChronology.CreatePrepared(new[]{1},new[]{default(LegacySourcePointKey)},new[]{0},new uint[]{1}));
try{ordinary.WithInsertedPreparedInk(ink,new CancellationToken(true));throw new Exception("Cancellation ignored");}catch(OperationCanceledException){}
Console.WriteLine($"PASS source218 chronology: {kernels} unsigned-wrap map kernels,{sources} prepared sources,{identities} point identities,{orders} signed/tie range orders; native inserted-point bridge, ceiling/remainder, chained resampling,bounds and cancellation. Native line-DAG seed order remains separate.");
static uint[] U(JsonElement a)=>a.EnumerateArray().Select(v=>v.GetUInt32()).ToArray();
static int[] I(JsonElement a)=>a.EnumerateArray().Select(v=>v.GetInt32()).ToArray();
static void Check(LegacySourcePointIdentity actual,JsonElement e)
{Require(actual.MetadataPointIndex==e[0].GetInt32() && actual.KeyRecordIndex==e[1].GetInt32() && actual.RemainderByte==e[2].GetInt32() && actual.Key==new LegacySourcePointKey(e[3][0].GetInt32(),e[3][1].GetInt32(),e[3][2].GetInt32()),"Original point identity or metadata key differs");}
static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
static void Reject(Action action){try{action();throw new Exception("Unsupported source input accepted");}catch(ArgumentException){}}
