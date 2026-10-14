using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=4)throw new ArgumentException("Supply prepared controls, actualAK, embedded30, embedded50.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] ak=File.ReadAllBytes(args[1]),m30=File.ReadAllBytes(args[2]),m50=File.ReadAllBytes(args[3]);
int cases=0,descriptorCount=0,keyCount=0,changes=0,splits=0;
foreach(var item in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,m30,m50,raw,96);
    var result=LegacyFreshMainDescriptorSplit.Prepare(grouping);var expected=item.GetProperty("split");
    Require(ReferenceEquals(result.Input,grouping),"Actual main source/model ownership.");
    int[] order=expected.GetProperty("order").EnumerateArray().Select(i=>i.GetInt32()).ToArray();
    Require(result.OriginalPhysicalStrokeIndices.Span.SequenceEqual(order),"Independent physical source order.");
    if(!order.SequenceEqual(Enumerable.Range(0,order.Length)))changes++;
    var bytes=result.CopyDescriptors();var records=expected.GetProperty("descriptors").EnumerateArray().ToArray();
    Require(bytes.Length==records.Length&&result.DescriptorCount==bytes.Length,"Independent descriptor count.");
    if(bytes.Length>1)splits++;
    for(int d=0;d<bytes.Length;d++)
    {
        Require(bytes[d].SequenceEqual(Convert.FromHexString(records[d].GetString()!)),$"All88 descriptor bytes{cases}:{d}.");descriptorCount++;
        if((d&1)==0)
        {
            Require(BinaryPrimitives.ReadInt32LittleEndian(bytes[d].AsSpan(16))==-1,"No fabricated line assignment.");
            int first=BinaryPrimitives.ReadInt32LittleEndian(bytes[d].AsSpan(36)),last=BinaryPrimitives.ReadInt32LittleEndian(bytes[d].AsSpan(40));
            for(int p=first;p<=last;p++)Require(result.GetDescriptorOrdinalForPhysicalStroke(p)==d,"Current physical descriptor ordinal.");
        }
        else Require((BinaryPrimitives.ReadUInt16LittleEndian(bytes[d])&15)==3,"Actual intermediate separator kind.");
    }
    Require(result.Chronology.SourceFactor==200&&result.Chronology.PreparedPointMap.Span.SequenceEqual(expected.GetProperty("ticks").EnumerateArray().Select(p=>p.GetUInt32()).ToArray()),"Independent actual factor200 physical point map.");
    for(int s=0;s<order.Length;s++)
    {
        int original=order[s];var originalPoints=grouping.Packet.Source.Strokes[original].Points;var current=result.Strokes[s].Points;
        Require(current.Count==originalPoints.Count,"Actual reordered point count.");
        Require(result.SourceStrokeProbabilities.Span[s]==grouping.Packet.Source.SourceStrokeProbabilities.Span[original],"Physical probability is reordered, not recomputed.");
        for(int p=0;p<current.Count;p++)
        {
            Require(current[p]==originalPoints[p],"Exact raw retained point byte values.");
            var actual=result.Chronology.Resolve(s,p);var old=grouping.Packet.Source.Chronology.Resolve(original,p);
            Require(actual.Key==old.Key&&actual.MetadataPointIndex==old.MetadataPointIndex&&actual.KeyRecordIndex==old.KeyRecordIndex&&actual.RemainderByte==0,"Original-key ownership after physical reorder.");keyCount++;
        }
    }
    //Copy methods cannot expose the stored88-byte rows for caller mutation.
    bytes[0][16]=123;Require(result.CopyDescriptors()[0][16]==255,"Descriptor storage ownership.");
    Throws<ArgumentOutOfRangeException>(()=>result.GetDescriptorOrdinalForPhysicalStroke(-1));
    Throws<OperationCanceledException>(()=>LegacyFreshMainDescriptorSplit.Prepare(grouping,new(true)));cases++;
}
Require(changes>0&&splits>0,"Independent controls must really reorder physical strokes and split descriptors.");
Console.WriteLine($"PASS {cases} source-owned15B260 controls/{descriptorCount} exact88-byte records/{keyCount} original keys/{changes} changed physical orders/{splits} descriptor splits;15F050 remains separate.");
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
