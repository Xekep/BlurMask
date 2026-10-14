using InkRecognition;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("Supply controls, actualAK, embedded30, embedded50.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));byte[] ak=File.ReadAllBytes(args[1]),m30=File.ReadAllBytes(args[2]),m50=File.ReadAllBytes(args[3]);
int cases=0,calls=0,keys=0,changed=0,scaleChanges=0;
foreach(var item in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var groups=LegacyFreshMainPhysicalGrouping.Prepare(ak,m30,m50,raw,96);var split=LegacyFreshMainDescriptorSplit.Prepare(groups);
    var result=LegacyFreshMainPhysicalReordering.Prepare(split,m30,m50);var expected=item.GetProperty("reorder");
    int[] order=expected.GetProperty("original").EnumerateArray().Select(i=>i.GetInt32()).ToArray();
    Require(ReferenceEquals(result.Input,split)&&!result.HasE8Callback&&!result.HasExistingGuides,"Actual fresh no-external-setter owner.");
    Require(result.OriginalPhysicalStrokeIndices.Span.SequenceEqual(order),$"Independent nested physical order{cases}.");
    if(!result.OriginalPhysicalStrokeIndices.Span.SequenceEqual(split.OriginalPhysicalStrokeIndices.Span))changed++;
    var expectedCalls=expected.GetProperty("calls").EnumerateArray().ToArray();Require(result.ReceiverCalls.Count==expectedCalls.Length,"Actual provider call count.");
    for(int i=0;i<expectedCalls.Length;i++)
    {
        var e=expectedCalls[i];var a=result.ReceiverCalls[i];Require(a.DescriptorOrdinal==e[0].GetInt32()&&a.StartingNeuralScale==e[1].GetSingle()&&a.EndingNeuralScale==e[2].GetSingle()&&a.LocalPhysicalCount==e[3].GetInt32()&&a.ProviderGroupCount==e[4].GetInt32()&&a.VerticalStripCount==e[5].GetInt32(),"Independent actual provider reset/carry/groups.");if(a.StartingNeuralScale!=a.EndingNeuralScale)scaleChanges++;calls++;
    }
    var records=result.CopyDescriptors();var expectedBytes=expected.GetProperty("descriptors").EnumerateArray().ToArray();
    Require(records.Length==expectedBytes.Length,"Descriptor count preserved.");for(int d=0;d<records.Length;d++)Require(records[d].SequenceEqual(Convert.FromHexString(expectedBytes[d].GetString()!)),"F15 descriptor bytes preserved.");
    Require(result.Chronology.PreparedPointMap.Span.SequenceEqual(expected.GetProperty("ticks").EnumerateArray().Select(v=>v.GetUInt32()).ToArray()),"Independent composed factor200 keys.");
    for(int s=0;s<order.Length;s++)for(int p=0;p<result.Strokes[s].Points.Count;p++)
    {
        var original=groups.Packet.Source.Strokes[order[s]];Require(result.Strokes[s].Points[p]==original.Points[p],"Actual retained coordinate source.");
        Require(result.Chronology.Resolve(s,p).Key==groups.Packet.Source.Chronology.Resolve(order[s],p).Key,"Actual original source identity after both reorders.");keys++;
    }
    Throws<OperationCanceledException>(()=>LegacyFreshMainPhysicalReordering.Prepare(split,m30,m50,new(true)));cases++;
}
Require(calls>0&&changed>0&&scaleChanges>0,"Independent controls must execute actual per-descriptorNNs and interval reorder.");
Console.WriteLine($"PASS {cases} actual fresh-source15F050 cases/{calls} owned receiver calls/{keys} original keys/{changed} interval-reorderedcases/{scaleChanges} actualNNscalechanges; externalE8/SDK/mainwinner notclaimed.");
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
