using InkRecognition;
using System.Text.Json;
if(args.Length!=9)throw new ArgumentException("AK LK engine separation case-data publication-data line30 line50 capture.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),sep=File.ReadAllBytes(args[3]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(sep);
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var single=MainFixture.Create(ak,engine,table);
var line=MainLineWordFixture.Create(ak,engine,sep,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),cases,publication,2);
var a=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,single,cases,publication));
var b=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,single));
var c=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,cases,publication),line.Context);
var d=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,LegacyOwnedRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source),line.Context);
var sources=new[]{LegacyOwnedRussianMainSourceGeometry.Prepare(ak,a,single),LegacyOwnedRussianMainSourceGeometry.Prepare(ak,b,single),
    LegacyOwnedRussianMainSourceGeometry.Prepare(ak,c,line.Source),LegacyOwnedRussianMainSourceGeometry.Prepare(ak,d,line.Source)};
int guards=0,arcs=0;
using(var file=File.Create(args[8]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteStartArray("sources");
    for(int i=0;i<sources.Length;i++)
    {
        var s=sources[i];var ink=i<2?single.Cuts.PreparedInk:line.Source.Cuts.PreparedInk;
        w.WriteStartObject();w.WriteBoolean("ordered",s.CollectionC1);w.WriteStartArray("points");
        foreach(var p in ink.Points.Span){w.WriteStartArray();w.WriteNumberValue(B(p.X));w.WriteNumberValue(B(p.Y));w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("counts");foreach(int n in ink.StrokePointCounts.Span)w.WriteNumberValue(n);w.WriteEndArray();
        w.WriteStartArray("groups");foreach(var g in s.Graph.Groups)
        {w.WriteStartArray();foreach(var r in g.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
        int lineId=s.ReadLine(0,0);var guide=s.ReadGuide(lineId);
        float actualBaseline=i<2?single.SourceBaseline:line.Source.Owner.SourceBaseline;
        float actualHeight=i<2?single.SourceHeight:line.Source.Owner.SourceHeight;
        if(B(guide.Baseline)!=B(actualBaseline)||B(guide.Height)!=B(actualHeight))throw new Exception("Source guide identity lost");
        w.WriteStartArray("guide");w.WriteNumberValue(lineId);w.WriteNumberValue(B(guide.Baseline));w.WriteNumberValue(B(guide.Height));w.WriteEndArray();
        w.WriteStartArray("arcs");
        for(int node=0;node<s.Graph.NodeCount;node++)for(int arc=0;arc<s.Graph.GetNode(node).Count;arc++)
        {
            var box=s.ReadBounds(node,(ushort)arc);if(box!=s.ReadBounds(node,(ushort)arc))throw new Exception("Unstable group cache");
            w.WriteStartArray();foreach(int n in new[]{node,arc,s.Graph.GetNode(node)[arc].GroupId,s.ReadLine(node,(ushort)arc),s.ReadKind(node,(ushort)arc),
                s.ReadTopMark(node,(ushort)arc)?1:0,B(box.MinimumX),B(box.MinimumY),B(box.MaximumX),B(box.MaximumY)})w.WriteNumberValue(n);
            w.WriteEndArray();arcs++;
        }w.WriteEndArray();
        w.WriteStartArray("flags");for(ushort label=0;label<model.AcousticAlphabet.LabelCount;label++)w.WriteBooleanValue(s.ReadAlphabetFlag(label));w.WriteEndArray();
        Reject(()=>s.ReadBounds(-1,0));Reject(()=>s.ReadBounds(0,ushort.MaxValue));Reject(()=>s.ReadBounds(0,0,new(true)));
        Reject(()=>s.ReadGuide(-1));Reject(()=>s.ReadAlphabetFlag(ushort.MaxValue));w.WriteEndObject();
    }
    w.WriteEndArray();
    Reject(()=>LegacyOwnedRussianMainSourceGeometry.Prepare(ak,a,line.Source));
    Reject(()=>LegacyOwnedRussianMainSourceGeometry.Prepare(ak,c,single));
    Reject(()=>LegacyOwnedRussianMainSourceGeometry.Prepare(ak,a,single,new(true)));
    var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyOwnedRussianMainSourceGeometry.Prepare(bad,a,single));
    w.WriteNumber("arcs",arcs);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS{arcs} actual case/DIRECT graph arc bounds/kind/line/topmark/guide and alphabet flags;{guards} guards; higher arcs separate.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid source accepted");}
static int B(float x)=>BitConverter.SingleToInt32Bits(x);
