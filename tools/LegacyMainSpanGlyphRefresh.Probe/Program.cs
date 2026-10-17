using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
byte[] l30=File.ReadAllBytes(args[6]),l50=File.ReadAllBytes(args[7]);
var samples=new[]{Word(0),Word(0).Concat(Word(230)).ToArray(),Word(0).Concat(Word(300)).ToArray(),
    Word(0).Concat(Word(450)).ToArray(),Word(0).Concat(Word(700)).ToArray(),Word(450).Concat(Word(0)).ToArray()};
int actual=0,prepared=0,refused=0,records=0,guards=0,two=0;
using var output=File.Create(args[8]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("inputs");
for(int input=0;input<samples.Length;input++)
{
    var raw=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,samples[input],0,table,cases,publication)
        .Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
    var refresh=LegacyOwnedMainSpanGlyphRefresh.Prepare(raw);var source=refresh.Source;
    if(!ReferenceEquals(refresh,LegacyOwnedMainSpanGlyphRefresh.Prepare(raw)))throw new InvalidDataException("Repeated Prepare reset the same raw-result epoch.");
    var pool=raw.Session.Ownership.GlyphPool;
    var chronology=raw.Sampled.Chronology;var original=raw.Sampled.SourceRanges.OriginalSource;
    int total=original.Strokes.Sum(s=>s.Points.Count);var keys=new LegacySourcePointKey[total];var ends=new LegacySourcePointKey[total];
    for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
    {var id=original.Chronology.Resolve(s,p);keys[id.KeyRecordIndex]=id.Key;ends[id.KeyRecordIndex]=original.ResolveEndingMetadata(id).Key;}
    for(int s=0;s<chronology.StrokePointCounts.Length;s++)for(int p=0;p<chronology.StrokePointCounts.Span[s];p++)
    {var id=chronology.Resolve(s,p);if(id.KeyRecordIndex!=id.MetadataPointIndex)throw new InvalidDataException("Fresh originalD8 identity differs from the independent raw-map auditor scope.");}
    w.WriteStartObject();w.WriteNumber("input",input);w.WriteNumber("factor",chronology.SourceFactor);
    w.WriteStartArray("rawCounts");foreach(var stroke in samples[input])w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
    w.WriteStartArray("counts");foreach(int n in chronology.StrokePointCounts.Span)w.WriteNumberValue(n);w.WriteEndArray();
    w.WriteStartArray("map");foreach(uint tick in chronology.PreparedPointMap.Span)w.WriteNumberValue(tick);w.WriteEndArray();
    w.WriteStartArray("keys");foreach(var key in keys)Key(key);w.WriteEndArray();
    w.WriteStartArray("endingKeys");foreach(var key in ends)Key(key);w.WriteEndArray();
    w.WriteStartArray("graph");for(int n=0;n<source.Graph.NodeCount;n++){w.WriteStartArray();foreach(var arc in source.Graph.GetNode(n))
    {w.WriteStartArray();w.WriteNumberValue(arc.GroupId);w.WriteNumberValue(arc.Span);w.WriteNumberValue(arc.StartContextId);w.WriteNumberValue(arc.EndContextId);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groupRanges");foreach(var g in source.Graph.Groups){w.WriteStartArray();foreach(var r in g.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("actual");
    foreach(var node in pool.CopyOwnerNodes())if(unchecked((short)(node.Key>>24))<0)
    {
        var result=refresh.RefreshOwner(node.Key);var after=pool.CopyOwnerNode(node.Key);
        if(!ReferenceEquals(result,refresh.RefreshOwner(node.Key))||result.SourceScoreBits!=0x3f800000||
            after.PackedCount!=node.PackedCount||!after.Records.Span.SequenceEqual(node.Records.Span))
            throw new InvalidDataException("Span refresh appended/rewrote a glyph or changed its scalar/lifetime.");
        w.WriteStartObject();WriteResult(result.Source);w.WriteNumber("scalar",result.SourceScoreBits);
        w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("before40",node.Records.Span);w.WriteBase64String("after40",after.Records.Span);w.WriteEndObject();actual++;
    }
    w.WriteEndArray();w.WriteStartArray("prepared");ulong? snapshotKey=null;
    var contexts=Enumerable.Range(0,source.Graph.NodeCount).SelectMany(n=>source.Graph.GetNode(n))
        .Where(a=>a.Span==0).SelectMany(a=>new[]{(int)a.StartContextId,(int)a.EndContextId}).Distinct().Order().ToArray();
    foreach(int last in contexts)foreach(int first in contexts.Prepend(-1))if(first<last&&last-first<=32768)
    for(int mode=0;mode<2;mode++)
    {
        ulong key=((uint)last&0xffffffUL)|((ulong)unchecked((ushort)(first-last))<<24)|((ulong)mode<<56);
        w.WriteStartObject();w.WriteNumber("requested",key);
        try{var result=source.Read(key);WriteResult(result);prepared++;snapshotKey??=key;}
        catch(NotSupportedException){w.WriteBoolean("refused",true);refused++;}
        w.WriteEndObject();
    }
    w.WriteEndArray();
    if(snapshotKey is null)throw new InvalidDataException("Snapshot lifetime control lacks an actual-source request.");
    byte[] snapshot=source.Read(snapshotKey.Value).CopySourceStorage24();
    var changed=new List<(ArraySegment<LegacyInkRange> Target,LegacyInkRange[] Original)>();
    try
    {
        foreach(var group in source.Graph.Groups)if(MemoryMarshal.TryGetArray(group.Ranges,out ArraySegment<LegacyInkRange> segment))
        {changed.Add((segment,segment.AsSpan().ToArray()));segment.AsSpan().Fill(new(0,0,0));}
        if(!source.Read(snapshotKey.Value).CopySourceStorage24().AsSpan().SequenceEqual(snapshot))
            throw new InvalidDataException("Caller-accessible graph memory escaped the owned source snapshot.");
        guards++;
    }
    finally{foreach(var value in changed)value.Original.CopyTo(value.Target.AsSpan());}
    if(!MemoryMarshal.TryGetArray(chronology.PreparedPointMap,out ArraySegment<uint> pointMap))
        throw new InvalidDataException("Actual source map was not available for the snapshot lifetime control.");
    uint[] oldMap=pointMap.AsSpan().ToArray();
    try
    {
        pointMap.AsSpan().Fill(uint.MaxValue);
        if(!source.Read(snapshotKey.Value).CopySourceStorage24().AsSpan().SequenceEqual(snapshot))
            throw new InvalidDataException("Caller-accessible point map escaped the owned source snapshot.");
        guards++;
    }
    finally{oldMap.CopyTo(pointMap.AsSpan());}
    Reject(()=>LegacyOwnedMainSpanGlyphSource.Prepare(null!));
    Reject(()=>LegacyOwnedMainSpanGlyphSource.Prepare(raw,new(true)));
    Reject(()=>source.Read(0));Reject(()=>source.Read(1UL<<40));
    Reject(()=>source.Read((0x8000UL<<24)|1));Reject(()=>source.Read((0xffffUL<<24)|0x7fffff));
    Reject(()=>source.Read((0xffffUL<<24)|1,new(true)));
    Reject(()=>refresh.RefreshOwner(0));Reject(()=>refresh.RefreshOwner((0xffffUL<<24)|1|(1UL<<40)));
    ulong absent=(0xffffUL<<24)|0x7fffff;Reject(()=>refresh.RefreshOwner(absent));
    w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("actual",actual);w.WriteNumber("prepared",prepared);w.WriteNumber("refused",refused);
w.WriteNumber("references",records);w.WriteNumber("twoReferenceOutputs",two);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
if(prepared==0||records==0||two==0)throw new InvalidDataException("Reverse endpoint/count controls were not exercised.");
Console.WriteLine($"PASS{samples.Length} actual raw sources/{actual} genuine negative owners/{prepared} prepared span exports/{refused} bounded refusals/{records} references/{two} two-record outputs/{guards} guards.");
void WriteResult(LegacyOwnedMainSpanGlyphSourceResult result)
{
    w.WriteNumber("key",result.SourceKey);w.WriteNumber("last",result.LastContext);w.WriteNumber("first",result.FirstContext);w.WriteNumber("mode",result.Mode);
    byte[] bytes=result.CopySourceStorage24();w.WriteBase64String("source24",bytes);
    if(bytes.Length!=result.ReferenceCount*24)throw new InvalidDataException("Span allocation has an invented residual tail.");
    bytes.AsSpan().Fill(0xa5);if(bytes.AsSpan().SequenceEqual(result.CopySourceStorage24()))throw new InvalidDataException("Source bytes alias caller storage.");
    records+=result.ReferenceCount;if(result.ReferenceCount==2)two++;
}
void Key(LegacySourcePointKey key){w.WriteStartArray();w.WriteNumberValue(key.First);w.WriteNumberValue(key.Second);w.WriteNumberValue(key.Third);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Guard ignored.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
