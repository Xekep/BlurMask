using InkRecognition;
using System.Text.Json;

if(args.Length!=1)throw new ArgumentException("Independent source ending metadata controls required.");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,keys=0,projected=0,differences=0;
foreach(var row in json.RootElement.GetProperty("cases").EnumerateArray())
{
    var input=row.GetProperty("raw").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var source=LegacyOrdinarySourceInk.Prepare(input,new(0x135,false,0,0));
    var expected=row.GetProperty("keys").EnumerateArray().Select(k=>k.EnumerateArray().Select(v=>v.GetInt32()).ToArray()).ToArray();
    var first=new List<LegacySourcePointKey>();int global=0;
    for(int physical=0;physical<source.Strokes.Count;physical++)for(int point=0;point<source.Strokes[physical].Points.Count;point++)
    {
        var identity=source.Chronology.Resolve(physical,point);var ending=source.ResolveEndingMetadata(identity);
        var e=expected[global];
        if(identity.Key!=new LegacySourcePointKey(e[0],e[1],e[2])||ending.Key!=new LegacySourcePointKey(e[0],e[1],e[3])||
            ending.MetadataPointIndex!=identity.MetadataPointIndex||ending.KeyRecordIndex!=identity.KeyRecordIndex||ending.RemainderByte!=identity.RemainderByte)
            throw new Exception("Native ordinary first/end key owner mismatch.");
        if(identity.Key.Third!=ending.Key.Third)differences++;
        first.Add(identity.Key);global++;keys++;
    }
    if(global!=expected.Length)throw new Exception("Independent retained metadata count differs.");
    var queries=row.GetProperty("queries").EnumerateArray().ToArray();
    uint[] ticks=queries.Select(q=>q[0].GetUInt32()).ToArray();
    var prepared=LegacySourcePointChronology.CreatePrepared([ticks.Length],first.ToArray(),Enumerable.Range(0,global).ToArray(),ticks,200);
    for(int point=0;point<ticks.Length;point++)
    {
        var identity=prepared.Resolve(0,point);var end=source.ResolveEndingMetadata(identity);var q=queries[point];
        if(end.Key!=new LegacySourcePointKey(q[1].GetInt32(),q[2].GetInt32(),q[3].GetInt32())||
            end.KeyRecordIndex!=q[4].GetInt32()||end.RemainderByte!=q[5].GetByte())
            throw new Exception("168e10 prepared ceil-map/key-index/end owner differs.");
        projected++;
    }
    //End counts are owned, including final dropped raw points.
    var last=source.Chronology.Resolve(source.Strokes.Count-1,source.Strokes[^1].Points.Count-1);
    input[0]=new InkStroke([new InkPoint(42,42)]);
    if(source.ResolveEndingMetadata(last).Key.Third!=expected[^1][3])throw new Exception("Original endpoint count aliases caller.");
    try{source.ResolveEndingMetadata(last with{KeyRecordIndex=-1});throw new Exception("Foreign key index accepted.");}catch(ArgumentException){ }
    using var canceled=new CancellationTokenSource();canceled.Cancel();
    try{source.ResolveEndingMetadata(last,canceled.Token);throw new Exception("Canceled ending getter returned.");}catch(OperationCanceledException){ }
    cases++;
}
if(cases!=512||differences==0)throw new Exception("Expected independent duplicate-run endpoint controls.");
Console.WriteLine($"PASS {cases} independent raw ending-owner controls / {keys} retained key records / {projected} fractional prepared lookups / {differences} distinct start/end ordinals; native108 owner, ceil/d8 identity, final duplicate runs, owned counts and guards. General source-range1a9ae0 dispatch remains separate.");
