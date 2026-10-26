using System.Text.Json;
using InkRecognition;
if(args.Length!=2||args[1] is not ("before" or "after"))throw new ArgumentException("Expected fake-maps.json before|after.");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases=0,stages=0,points=0;
foreach(var c in document.RootElement.GetProperty("Cases").EnumerateArray())
{
    string name=c.GetProperty("Name").GetString()!;
    int[] original=Ints(c.GetProperty("OriginalCounts"));
    var ordinary=LegacySourcePointChronology.CreateOrdinary(original);
    var raw=original.Select((count,stroke)=>new InkStroke(Enumerable.Range(0,count)
        .Select(point=>new InkPoint(stroke*10+point,point+stroke)).ToArray())).ToArray();
    var chronology=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0)).Chronology;
    uint factor=args[1]=="after"?200u:1u;
    if(chronology.SourceFactor!=factor||ordinary.SourceFactor!=factor)throw new InvalidDataException($"{name}: initialfactor.");
    var expected=c.GetProperty(args[1]=="after"?"ExpectedAfter":"ExpectedBefore");int step=0;
    foreach(var s in c.GetProperty("Steps").EnumerateArray())
    {
        int[] counts=Ints(s.GetProperty("Counts"));uint[] ticks=UInts(s.GetProperty("Ticks"));
        chronology=chronology.RemapPreparedPoints(counts,ticks,s.GetProperty("IncomingFactor").GetUInt32());
        var e=expected[step].GetProperty("Snapshot");
        if(chronology.SourceFactor!=e.GetProperty("SourceFactor").GetUInt32()||
            !chronology.PreparedPointMap.Span.SequenceEqual(UInts(e.GetProperty("Map"))))
            throw new InvalidDataException($"{name}: stage{step} map/factor.");
        foreach(var p in e.GetProperty("Points").EnumerateArray())
        {
            var actual=chronology.Resolve(p.GetProperty("Stroke").GetInt32(),p.GetProperty("Point").GetInt32());
            var key=p.GetProperty("Key");
            if(actual.MetadataPointIndex!=p.GetProperty("MetadataPointIndex").GetInt32()||
                actual.KeyRecordIndex!=p.GetProperty("KeyRecordIndex").GetInt32()||
                actual.RemainderByte!=p.GetProperty("RemainderByte").GetByte()||
                actual.Key.First!=key[0].GetInt32()||actual.Key.Second!=key[1].GetInt32()||actual.Key.Third!=key[2].GetInt32())
                throw new InvalidDataException($"{name}: stage{step} actual metadata.");
            points++;
        }
        step++;stages++;
    }
    if(step!=expected.GetArrayLength())throw new InvalidDataException("Exactstagecoverage.");cases++;
}
var neutral=LegacySourcePointChronology.CreatePrepared([2],[new(0,0,0),new(0,0,1)],[0,1]);
if(neutral.SourceFactor!=1)throw new InvalidDataException("Generic explicit metadata default must remain1.");
if(cases!=8||stages!=11||points==0)throw new InvalidDataException("Genuine exact8cases/11stages required.");
Console.WriteLine(JsonSerializer.Serialize(new{Mode=args[1],Cases=cases,Stages=stages,MetadataPoints=points,NeuralCalls=0}));
static int[] Ints(JsonElement x)=>x.EnumerateArray().Select(v=>v.GetInt32()).ToArray();
static uint[] UInts(JsonElement x)=>x.EnumerateArray().Select(v=>v.GetUInt32()).ToArray();
