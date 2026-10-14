using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;

if(args.Length!=3)throw new ArgumentException("AK resource, engine profile and independent audit output required.");
byte[] resource=File.ReadAllBytes(args[0]);
if(Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned source resource required.");
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var tables=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(tables.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),
    tables.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var producer=LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,6120);
var indexMap=LegacyAcousticConfidenceLabelMap.ReadFreshFirstSource(resource,6120);
var raw=new[]{Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((70,10),(70,90),(120,10),(120,90)),Draw((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var rows=new List<AuditRow>();int groups=0, values=0, refused=0;
for(int variant=0;variant<24;variant++)
{
    float scale=new[]{.5f,1f,2f}[variant%3],dx=(variant/3)*13.25f,dy=-(variant/3)*7.5f;
    var strokes=raw.Select(s=>new InkStroke(s.Points.Select((p,j)=>new InkPoint(
        p.X*scale+dx+((j*7+variant*3)%11-5)*scale*.2f,
        p.Y*scale+dy+((j*5+variant*7)%13-6)*scale*.15f)).ToArray())).ToArray();
    var session=LegacyFreshFirstGlyphSession.Prepare(resource,6120,strokes,activations);
    var counts=session.Cuts.Cuts.PreparedInk.StrokePointCounts.Span;byte[] mask=Enumerable.Repeat((byte)1,session.Scorer.Alphabet.LabelCount).ToArray();
    for(int physical=0;physical<strokes.Length;physical++)
    {
        var scored=session.Score([new(physical,0,counts[physical])],0,15,mask,[]);
        var published=new int[producer.NetworkClassCount];
        for(ushort symbol=0;symbol<published.Length;symbol++)
        {published[symbol]=BitConverter.SingleToInt32Bits(producer.ReadSymbolScore(scored.Glyph.Scores,scored.Cache,symbol));values++;}
        var aliases=new List<int[]>();
        foreach(var alias in indexMap.ExplicitMappings)
        {
            //Controlled retained-label input uses real NN channels but does
            //not claim that the classifier actually selected this primitive.
            var controlled=scored.Cache with{Candidates=new LegacyAcousticCacheCandidate[]{new(alias.Key,0)}};
            var nativeOutput=scored.Cache with{Candidates=new LegacyAcousticCacheCandidate[]{new(alias.Value,0)}};
            int actual=BitConverter.SingleToInt32Bits(producer.ReadSymbolScore(scored.Glyph.Scores,controlled,alias.Key));
            if(actual!=
                BitConverter.SingleToInt32Bits(producer.ReadSymbolScore(scored.Glyph.Scores,nativeOutput,alias.Value)))
                throw new Exception("Actual prototype confidence input index differs from its raw native output channel.");
            aliases.Add(new[]{(int)alias.Key,(int)alias.Value,actual});
        }
        if(variant==0&&physical==0)
        {
            var cutoff=scored.Cache with{Candidates=Enumerable.Range(0,20).Select(v=>new LegacyAcousticCacheCandidate((ushort)v,0)).Append(new(193,0)).ToArray()};
            if(producer.ReadSymbolScore(scored.Glyph.Scores,cutoff,193)!=0)throw new Exception("Alias remap incorrectly bypasses native rank20 cutoff.");
            if(!session.Scorer.Alphabet.TryGetLabel("\U000f0001",out ushort closedLabel))throw new Exception("Actual closed primitive missing.");
            var closed=scored.Cache with{Candidates=new LegacyAcousticCacheCandidate[]{new(closedLabel,0)}};
            try{producer.ReadSymbolScore(scored.Glyph.Scores,closed,closedLabel);throw new Exception("Unresolved closed probability acquired a guessed NN score.");}catch(NotSupportedException){refused++;}
        }
        rows.Add(new(scored.Glyph.Scores.TrajectoryProbability32768.ToArray(),scored.Glyph.Scores.RasterProbability32768.ToArray(),
            scored.Cache.Candidates.Span.ToArray().Select(v=>(int)v.Symbol).ToArray(),published,aliases.ToArray()));
        var empty=scored.Cache with{Candidates=ReadOnlyMemory<LegacyAcousticCacheCandidate>.Empty};
        if(producer.ReadSymbolScore(scored.Glyph.Scores,empty,0)!=0)throw new Exception("Empty candidates produced a score.");
        //Same dimensions and networks with changed, inactive metadata are still a foreign source.
        byte[] changed=resource.ToArray();changed[187747]^=1;
        var foreign=LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(changed,6120);
        try{foreign.ReadSymbolScore(scored.Glyph.Scores,scored.Cache,0);throw new Exception("Foreign source channels accepted.");}catch(ArgumentException){refused++;}
        using var canceled=new CancellationTokenSource();canceled.Cancel();
        try{producer.ReadSymbolScore(scored.Glyph.Scores,scored.Cache,0,canceled.Token);throw new Exception("Canceled score returned.");}catch(OperationCanceledException){refused++;}
        groups++;
    }
}
var wrongMode=resource.ToArray();wrongMode[187726]=1;
try{LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(wrongMode,6120);throw new Exception("Mode1 without its calibration data accepted.");}catch(NotSupportedException){refused++;}
using(var output=File.Create(args[2]))using(var writer=new Utf8JsonWriter(output))
{
    writer.WriteStartObject();writer.WriteNumber("groups",groups);writer.WriteNumber("values",values);writer.WriteStartArray("rows");
    foreach(var row in rows)
    {
        writer.WriteStartObject();WriteArray(writer,"a",row.A);WriteArray(writer,"b",row.B);
        WriteArray(writer,"order",row.Order);WriteArray(writer,"expected",row.Expected);
        writer.WriteStartArray("aliases");foreach(var alias in row.Aliases){writer.WriteStartArray();foreach(int value in alias)writer.WriteNumberValue(value);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteEndObject();
    }
    writer.WriteEndArray();writer.WriteEndObject();
}
Console.WriteLine($"PASS {groups} actual raw-source/frame/smoothing/context/NN/final-cache groups -> {values} fresh mode0 candidate scores plus {groups*indexMap.ExplicitMappings.Count} controlled prototype aliases; {refused} foreign-mode/cancellation guards. Independent scalar audit required; closed/wide probability services and accepted OCR remain separate.");
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void WriteArray(Utf8JsonWriter writer,string name,int[] values)
{writer.WriteStartArray(name);foreach(int value in values)writer.WriteNumberValue(value);writer.WriteEndArray();}
sealed record AuditRow(int[] A,int[] B,int[] Order,int[] Expected,int[][] Aliases);
