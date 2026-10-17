using System.Text.Json;
using System.Buffers.Binary;
using InkRecognition;

if(args.Length!=3)throw new ArgumentException("raw controls, actual RU resource and audit output required.");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));byte[] resource=File.ReadAllBytes(args[1]);
using var stream=File.Create(args[2]);using var writer=new Utf8JsonWriter(stream);writer.WriteStartArray();
int cases=0,queries=0,actual=0;
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=c.GetProperty("raw").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var original=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0));
    // Explicit numeric resampling controls keep genuine original metadata keys.
    // Current physical order is deliberately different from the key owner.
    var order=Enumerable.Range(0,original.Strokes.Count).ToArray();if(cases%2==1)Array.Reverse(order);
    var points=new List<InkStroke>();var ticks=new List<uint>();int[] starts=new int[original.Strokes.Count];
    for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+original.Strokes[i-1].Points.Count;
    foreach(int physical in order)
    {
        var old=original.Strokes[physical].Points;var next=new List<InkPoint>();
        for(int p=0;p<old.Count-1;p++)for(int part=0;part<4;part++)
        {float t=part/4f;next.Add(new(old[p].X+(old[p+1].X-old[p].X)*t,old[p].Y+(old[p+1].Y-old[p].Y)*t));ticks.Add((uint)((starts[physical]+p)*200+part*50));}
        next.Add(old[^1]);ticks.Add((uint)((starts[physical]+old.Count-1)*200));points.Add(new(next.ToArray()));
    }
    var keyRows=original.Strokes.SelectMany((stroke,index)=>Enumerable.Range(0,stroke.Points.Count).Select(p=>original.Chronology.Resolve(index,p).Key)).ToArray();
    var mapped=LegacySourcePointChronology.CreatePrepared(points.Select(x=>x.Points.Count).ToArray(),keyRows,Enumerable.Range(0,keyRows.Length).ToArray(),ticks.ToArray(),200);
    var ink=LegacyCursivePreparedInk.Create(points);
    if(cases%3==0&&ink.StrokePointCounts.Span[0]>1)
    {var a=ink.Points.Span[0];var b=ink.Points.Span[1];ink=ink.Insert(0,1,new((a.X+b.X)/2,(a.Y+b.Y)/2),0);}
    var chronology=mapped.WithInsertedPreparedInk(ink);
    var counts=ink.StrokePointCounts.Span.ToArray();
    var markers=new List<LegacyCursiveCutMarker>();
    for(int s=0;s<counts.Length;s++){markers.Add(new(0,s,0));if(counts[s]>2)markers.Add(new(0,s,counts[s]/2));}
    var cuts=new LegacyCursiveCutResult(ink,markers.ToArray(),LegacyCursiveInkAtoms.Build(counts,markers.ToArray()));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(c.GetProperty("reorder").GetBoolean(),c.GetProperty("shift").GetByte(),.3f,.3f),new byte[counts.Length],Enumerable.Repeat(-1,counts.Length).ToArray());
    Inspect(seeds,chronology,original,3,raw.Select(s=>s.Points.Count).ToArray());cases++;
}
foreach(float scale in new[]{.5f,1f,2f})foreach(float shift in new[]{-70f,0f,130f})foreach(bool duplicates in new[]{false,true})
{
    InkStroke[] input=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
    var raw=input.Select(s=>new InkStroke(s.Points.SelectMany(p=>duplicates?new[]{p,p,p}:new[]{p}).Select(p=>new InkPoint(p.X*scale+shift,p.Y*scale-shift)).ToArray())).ToArray();
    var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
    Inspect(seeds,cuts.Chronology,row.OriginalSource,5,raw.Select(s=>s.Points.Count).ToArray());actual++;
}
writer.WriteEndArray();writer.Flush();
Console.WriteLine($"Prepared {cases} independent raw cases/{actual} actual RU frame/cut/graph cases/{queries} source callback audits. Run independent Python raw-map/residual oracle; old key-only API parity checked. No fullV140, OCR winner or wrapper claim.");

void Inspect(LegacyOrdinaryCursiveSeedResult seeds,LegacySourcePointChronology chronology,LegacyOrdinarySourceInk original,int atoms,int[] originalCounts)
{
    var owner=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,original,atoms);var graph=owner.Graph;
    var seedGraph=seeds.InsertionShift==0?LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds):LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
    writer.WriteStartObject();writer.WriteStartArray("keys");
    for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)WriteKey(original.Chronology.Resolve(s,p).Key);
    writer.WriteEndArray();writer.WriteStartArray("originalCounts");foreach(int count in originalCounts)writer.WriteNumberValue(count);writer.WriteEndArray();
    writer.WriteNumber("factor",chronology.SourceFactor);writer.WriteStartArray("map");foreach(uint tick in chronology.PreparedPointMap.Span)writer.WriteNumberValue(tick);writer.WriteEndArray();
    writer.WriteStartArray("counts");foreach(int count in chronology.StrokePointCounts.Span)writer.WriteNumberValue(count);
    writer.WriteEndArray();writer.WriteStartArray("seeds");
    foreach(var seed in seeds.Seeds){var r=seed.Atom.Range;writer.WriteStartArray();writer.WriteNumberValue(r.StrokeIndex);writer.WriteNumberValue(r.StartPoint);writer.WriteNumberValue(r.EndPointExclusive);writer.WriteEndArray();}writer.WriteEndArray();
    writer.WriteStartArray("nodes");for(int n=0;n<seedGraph.NodeCount;n++)
    {
        writer.WriteStartArray();foreach(var arc in seedGraph.GetNode(n))
        {writer.WriteStartArray();writer.WriteNumberValue(arc.SeedGroupIndex);writer.WriteNumberValue(arc.StartContextId);writer.WriteNumberValue(arc.EndContextId);writer.WriteStartArray();foreach(int child in arc.SuccessorArcIndices.Span)writer.WriteNumberValue(child);writer.WriteEndArray();writer.WriteEndArray();}writer.WriteEndArray();
    }writer.WriteEndArray();writer.WriteStartArray("queries");
    for(int n=0;n<graph.NodeCount;n++)for(int a=0;a<graph.GetNode(n).Count;a++)
    {
        var arc=graph.GetNode(n)[a];var ranges=owner.ResolveCandidateWithResiduals(n,a);var old=owner.ResolveCandidate(n,a);
        if(!old.SequenceEqual(ranges.Select(r=>new LegacyFirstResultSourceRange(r.First,r.Last))))throw new InvalidDataException("Old key-only API changed.");
        writer.WriteStartObject();writer.WriteNumber("first",arc.StartContextId);writer.WriteNumber("last",arc.EndContextId);writer.WriteStartArray("ranges");
        foreach(var r in ranges){writer.WriteStartObject();writer.WriteBase64String("reference24",r.CopyReferenceStorage24());writer.WriteBase64String("residual2",r.CopyResidualStorage2());writer.WriteEndObject();}writer.WriteEndArray();writer.WriteEndObject();queries++;
        if(ranges.Length==0)throw new Exception("Actual successful seed path lost source membership.");
    }
    writer.WriteEndArray();writer.WriteEndObject();
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();
    Reject(()=>owner.ResolveCandidateWithResiduals(0,0,cancelled.Token));Reject(()=>owner.ResolveForward(uint.MaxValue,0));Reject(()=>owner.ResolveCandidate(-1,0));
    var firstResult=owner.ResolveCandidateWithResiduals(0,0);var unchanged=firstResult[0];firstResult[0]=new(new(999,999,999),new(999,999,999),9,9);
    byte[] copy=unchanged.CopyReferenceStorage24();copy.AsSpan().Fill(0x77);byte[] rem=unchanged.CopyResidualStorage2();rem.AsSpan().Fill(0x78);
    if(owner.ResolveCandidateWithResiduals(0,0)[0]!=unchanged)throw new Exception("Source result array aliases owner.");
}
void WriteKey(LegacySourcePointKey key){writer.WriteStartArray();writer.WriteNumberValue(key.First);writer.WriteNumberValue(key.Second);writer.WriteNumberValue(key.Third);writer.WriteEndArray();}
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Invalid request accepted.");}
