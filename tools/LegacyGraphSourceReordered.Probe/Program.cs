using System.Text.Json;
using InkRecognition;

if(args.Length!=4)throw new ArgumentException("AK.res line30.res line50.res output.json");
byte[] ak=File.ReadAllBytes(args[0]);
InkStroke Draw(float x)=>new(new InkPoint[]{new(x,90),new(x,10),new(x+25,60),new(x+50,10),new(x+50,90)});
InkStroke[] raw=[Draw(90),Draw(0)];
var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]),raw,0);
var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]));
if(!reordered.OriginalPhysicalStrokeIndices.Span.SequenceEqual(new[]{1,0}))throw new InvalidDataException("Fixture must reach genuine model-selected physical reversal.");
var controller=LegacyMainFirstResultLineController.PrepareOwnedCurrentSource(ak,reordered);
var source=controller.Source;var ink=LegacyCursivePreparedInk.Create(source.Strokes);var chronology=source.Chronology.WithInsertedPreparedInk(ink);
int[] counts=ink.StrokePointCounts.ToArray();var markers=counts.Select((_,s)=>new LegacyCursiveCutMarker(0,s,0)).ToArray();
var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(counts,markers));
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(false,0,.3f,.3f),new byte[counts.Length],Enumerable.Repeat(-1,counts.Length).ToArray());
var ranges=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,source,2);
var metadata=source.Strokes.SelectMany((stroke,s)=>Enumerable.Range(0,stroke.Points.Count).Select(p=>source.Chronology.Resolve(s,p))).ToArray();
var byOriginal=new LegacySourcePointKey[metadata.Length];var seen=new bool[metadata.Length];
foreach(var point in metadata)
{
    if((uint)point.KeyRecordIndex>=(uint)byOriginal.Length||seen[point.KeyRecordIndex])throw new InvalidDataException("Genuine one-to-one raw metadata ownership required.");
    seen[point.KeyRecordIndex]=true;byOriginal[point.KeyRecordIndex]=point.Key;
}
int queries=0;bool failed=false;
using(var stream=File.Create(args[3]))using(var output=new Utf8JsonWriter(stream))
{
    output.WriteStartObject();output.WriteStartArray("physicalOrder");foreach(int i in reordered.OriginalPhysicalStrokeIndices.Span)output.WriteNumberValue(i);output.WriteEndArray();
    output.WriteStartArray("currentKeys");foreach(var p in metadata)Key(p.Key);output.WriteEndArray();output.WriteStartArray("originalKeyTable");foreach(var key in byOriginal)Key(key);output.WriteEndArray();
    output.WriteStartArray("queries");
    for(int n=0;n<ranges.Graph.NodeCount;n++)for(int a=0;a<ranges.Graph.GetNode(n).Count;a++)
    {
        var arc=ranges.Graph.GetNode(n)[a];var physical=ranges.Graph.Groups[arc.GroupId].Ranges.ToArray();
        if(physical.Length!=2||physical.Select(r=>r.StrokeIndex).Distinct().Count()!=2||physical.Any(r=>r.StartPoint!=0||r.EndPointExclusive!=counts[r.StrokeIndex]))continue;
        var actual=ranges.ResolveCandidateWithResiduals(n,a);
        //Independent original-F8 table proves adjacency of the two complete
        //raw source intervals despite their reversed current physical order.
        var expected=new LegacyOrdinaryGraphSourceReference(byOriginal[0],byOriginal[^1],0,0);
        failed|=actual.Length!=1||actual[0]!=expected;queries++;
        output.WriteStartObject();output.WriteNumber("node",n);output.WriteNumber("arc",a);output.WriteStartArray("actual");foreach(var r in actual){output.WriteStartArray();Key(r.First);Key(r.Last);output.WriteNumberValue(r.FirstResidual);output.WriteNumberValue(r.LastResidual);output.WriteEndArray();}output.WriteEndArray();output.WriteStartArray("expectedFromOriginalF8");Key(expected.First);Key(expected.Last);output.WriteNumberValue(expected.FirstResidual);output.WriteNumberValue(expected.LastResidual);output.WriteEndArray();output.WriteEndObject();
    }
    output.WriteEndArray();output.WriteBoolean("failed",failed);output.WriteEndObject();output.Flush();
    void Key(LegacySourcePointKey key){output.WriteStartArray();output.WriteNumberValue(key.First);output.WriteNumberValue(key.Second);output.WriteNumberValue(key.Third);output.WriteEndArray();}
}
if(queries==0)throw new InvalidDataException("Fixture lacks its genuine two-physical ordinary candidate.");
if(failed)throw new InvalidDataException("Source adjacency used current physical order instead of original KeyRecordIndex/F8 owner.");
Console.WriteLine($"PASS actual model-selected physical reversal/current-source factory/{queries} complete two-physical source candidates; original F8 table retained.");
