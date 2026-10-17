using InkRecognition;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
int guards=0,prepared=0,actual=0,singleParity=0,multiple=0,inserts=0;
using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("prepared");
for(int seed=0;seed<192;seed++)
{
    int physical=2+seed%3;var raw=new InkStroke[physical];
    for(int s=0;s<physical;s++)
    {
        float x=s*130+seed%7*.125f,y=(s%2)*140;
        raw[s]=seed%3==1
            ?new(new InkPoint[]{new(x,y+15),new(x+20,y+15),new(x+40,y+15),new(x+60,y+15),new(x+80,y+15)})
            :new(new InkPoint[]{new(x,y+90),new(x+10,y+15),new(x+25,y+65),new(x+40,y+10),new(x+55,y+90)});
    }
    int[] counts=raw.Select(s=>s.Points.Count).ToArray();int total=counts.Sum();
    var keys=Enumerable.Range(0,total+1).Select(i=>new LegacySourcePointKey(i%5-2,i/5,i*3)).ToArray();
    var map=Enumerable.Range(0,total).Select(i=>(uint)(200*i+(i==total-1?0:seed%2*100))).ToArray();
    var chronology=LegacySourcePointChronology.CreatePrepared(counts,keys,Enumerable.Range(0,keys.Length).ToArray(),map,200);
    var descriptors=new List<byte[]>();var guides=new List<byte[]>();int first=0;
    if(seed%4==0)descriptors.Add(Separator());
    while(first<physical)
    {
        int last=seed%3==0?first:Math.Min(physical-1,first+seed%2);
        int line=guides.Count;guides.Add(Guide(90+(first%2)*140,40+seed%5));descriptors.Add(Descriptor(first,last,line));
        first=last+1;if(first<physical&&seed%2==0)descriptors.Add(Separator());
    }
    if(seed%7==0)descriptors.Add(Separator());
    float[] probabilities=Enumerable.Range(0,physical).Select(s=>new float[]{0,.49f,.5f,.75f,1}[(seed+s)%5]).ToArray();
    var result=LegacyMainDescriptorCutSequence.Execute(raw,chronology,probabilities,descriptors,guides);
    inserts+=result.CopyStrokes().Sum(s=>s.Points.Count)-total;
    WriteCase(raw,chronology,keys,probabilities,descriptors.ToArray(),guides.ToArray(),result.Steps,result.CopyStrokes(),result.Chronology,result.CopyTrailingPending());
    var repeat=LegacyMainDescriptorCutSequence.Execute(raw,chronology,probabilities,descriptors,guides);
    if(!Same(result.CopyStrokes(),repeat.CopyStrokes())||!result.Chronology.PreparedPointMap.Span.SequenceEqual(repeat.Chronology.PreparedPointMap.Span))
        throw new Exception("Repeat changed source input snapshots.");
    prepared++;
}
w.WriteEndArray();w.WriteStartArray("actual");
var samples=new[]{Word(0,0),Word(0,250).Concat(Word(0,0)).ToArray(),Word(0,0).Concat(Word(230,0)).ToArray(),
    Word(0,0).Concat(Word(300,0)).ToArray(),Word(0,0).Concat(Word(450,0)).ToArray(),Word(0,0).Concat(Word(700,0)).ToArray(),Hi(0).Concat(Hi(220)).ToArray()};
foreach(var raw in samples)
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,caseData,publication).Run();
    var result=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);
    if(!ReferenceEquals(result.Preparation,input.Smoothed)||result.GraphConfigurationByte3||result.HasCategoryLimitObject||
        result.CategoryOwner.SamplerNodeOffset!=188157||result.SpanProfile.MaximumAtoms!=8)
        throw new Exception("Actual fresh cut source/model policy ownership differs.");
    var descriptors=input.Lines.CopyDescriptors();var source=input.Lines.Owner.Controller.Source;
    if(descriptors.Length==1)
    {
        var old=LegacyFreshMainSingleLineSampler.Prepare(ak,input.Smoothed).Sample(table);
        var step=result.Steps[0];
        if(!old.Cuts.PreparedInk.Points.Span.SequenceEqual(step.CopySelectedPreparedPoints())||
            !old.Cuts.Markers.SequenceEqual(step.CopyMarkers())||!old.Cuts.Atoms.SequenceEqual(step.CopyAtoms())||
            !Same(old.Cuts.PreparedInk.ToStrokes(),result.CopyStrokes())||
            !old.Chronology.PreparedPointMap.Span.SequenceEqual(result.Chronology.PreparedPointMap.Span))
            throw new Exception("Existing single-descriptor cuts/records/markers/atoms/chronology changed.");
        singleParity++;
    }
    else multiple++;
    WriteCase(input.Smoothed.Strokes,input.Smoothed.Chronology,[],source.SourceStrokeProbabilities.ToArray(),descriptors,input.Lines.CopyGuides(),
        result.Steps,result.CopyStrokes(),result.Chronology,result.CopyTrailingPendingDescriptorOrdinals());
    var snapshot=result.CopyStrokes();if(!Same(snapshot,result.CopyStrokes()))throw new Exception("Owned coordinates changed.");
    var returned=result.CopyDescriptors();returned[0][0]^=255;if(result.CopyDescriptors()[0][0]!=descriptors[0][0])throw new Exception("Descriptor snapshot aliases owner.");
    var memory=result.Chronology.PreparedPointMap;
    if(MemoryMarshal.TryGetArray(memory,out ArraySegment<uint> recovered)&&recovered.Count>0)
    {uint old=recovered.Array![recovered.Offset];recovered.Array[recovered.Offset]=uint.MaxValue;if(result.Chronology.PreparedPointMap.Span[0]!=old)throw new Exception("Chronology snapshot aliases owner.");}
    var foreign=ak.ToArray();foreign[^1]^=1;Reject(()=>LegacyFreshMainDescriptorCutSource.Prepare(foreign,input.Smoothed));
    Reject(()=>LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed,new(true)));
    actual++;
}
w.WriteEndArray();
var two=new InkStroke[]{new(new InkPoint[]{new(0,100),new(10,0),new(20,100)}),new(new InkPoint[]{new(50,100),new(60,0),new(70,100)})};
var ordinary=LegacySourcePointChronology.CreateOrdinary([3,3]);
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(1,1,0)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,1,-1)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,1,1)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,1,0)],[Guide(100,0)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0],[Descriptor(0,1,0)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[float.NaN,0],[Descriptor(0,1,0)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[new byte[87]],[Guide(100,40)]));
var malformed=Separator();malformed[32]=0;Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[malformed,Descriptor(0,1,0)],[Guide(100,40)]));
var unsupported=Descriptor(0,1,0);unsupported[0]=6;Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[unsupported],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,0,0),Descriptor(0,1,0)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,0,0)],[Guide(100,40)]));
Reject(()=>LegacyMainDescriptorCutSequence.Execute(two,ordinary,[0,0],[Descriptor(0,1,0)],[Guide(100,40)],new(true)));
var horizontal=new InkStroke(new InkPoint[]{new(0,15),new(20,15),new(40,15),new(60,15),new(80,15)});
if(LegacyCursiveCutProducer.GenerateRussianVersion106([horizontal],90,40,new float[]{0}).PreparedInk.Points.Length<=5)
    throw new Exception("Shared point-budget fixture no longer inserts.");
var fullSource=new InkStroke[]{horizontal,new(Enumerable.Range(0,4091).Select(i=>new InkPoint(200,i*.125f)).ToArray())};
Reject(()=>LegacyMainDescriptorCutSequence.Execute(fullSource,LegacySourcePointChronology.CreateOrdinary([5,4091]),[0,1],
    [Descriptor(0,0,0),Descriptor(1,1,0)],[Guide(90,40)]));
if(singleParity==0||multiple==0||inserts==0)throw new Exception($"Required coverage missing: single{singleParity}/multiple{multiple}/inserts{inserts}.");
w.WriteNumber("preparedCount",prepared);w.WriteNumber("actualCount",actual);w.WriteNumber("singleParity",singleParity);w.WriteNumber("multiple",multiple);w.WriteNumber("inserted",inserts);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{prepared} prepared descriptor-map states/{inserts} inserted points;{actual} actual raw inputs/{multiple} multi-descriptor/{singleParity} old single-line byte parity/{guards} guards. Shared cut source only; no global graph or multiline text claim.");

void WriteCase(IReadOnlyList<InkStroke> raw,LegacySourcePointChronology before,LegacySourcePointKey[] keys,float[] probabilities,
    byte[][] descriptors,byte[][] guides,IReadOnlyList<LegacyMainDescriptorCutStep> steps,IReadOnlyList<InkStroke> final,LegacySourcePointChronology after,int[] trailing)
{
    w.WriteStartObject();Points("source",raw);w.WriteNumber("factor",before.SourceFactor);UInts("priorMap",before.PreparedPointMap.Span);
    w.WriteStartArray("keys");foreach(var k in keys){w.WriteStartArray();w.WriteNumberValue(k.First);w.WriteNumberValue(k.Second);w.WriteNumberValue(k.Third);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("beforeIdentities");Identities(before);w.WriteEndArray();
    Ints("probabilities",probabilities.Select(BitConverter.SingleToInt32Bits).ToArray());
    w.WriteStartArray("descriptors");foreach(var d in descriptors)w.WriteBase64StringValue(d);w.WriteEndArray();
    w.WriteStartArray("guides");foreach(var g in guides)w.WriteBase64StringValue(g);w.WriteEndArray();
    w.WriteStartArray("steps");foreach(var s in steps)
    {
        w.WriteStartObject();w.WriteNumber("ordinal",s.DescriptorOrdinal);w.WriteNumber("kind",s.Kind);w.WriteNumber("line",s.LineId);w.WriteNumber("first",s.FirstPhysical);w.WriteNumber("last",s.LastPhysical);
        w.WriteNumber("baselineBits",BitConverter.SingleToInt32Bits(s.Baseline));w.WriteNumber("heightBits",BitConverter.SingleToInt32Bits(s.Height));
        Ints("beforeCounts",s.CopyInputPointCounts());Ints("afterCounts",s.CopyOutputPointCounts());Ints("pending",s.CopyPendingDescriptorOrdinals());UInts("incoming",s.CopyIncomingPointMap());UInts("composed",s.CopyComposedPointMap());
        w.WriteStartArray("records");foreach(var p in s.CopySelectedPreparedPoints())Ints(null,[BitConverter.SingleToInt32Bits(p.X),BitConverter.SingleToInt32Bits(p.Y),BitConverter.SingleToInt32Bits(p.DeltaX),BitConverter.SingleToInt32Bits(p.DeltaY),BitConverter.SingleToInt32Bits(p.SegmentLength),p.Tag,p.StrokeIndex,p.OriginalPointIndex]);w.WriteEndArray();
        w.WriteStartArray("markers");foreach(var m in s.CopyMarkers())Ints(null,[m.Kind,m.StrokeIndex,m.PointIndex]);w.WriteEndArray();
        w.WriteStartArray("atoms");foreach(var r in s.CopyAtoms())Ints(null,[r.Marker.Kind,r.Range.StrokeIndex,r.Range.StartPoint,r.Range.EndPointExclusive]);w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();Points("final",final);UInts("finalMap",after.PreparedPointMap.Span);Ints("trailing",trailing);
    w.WriteStartArray("afterIdentities");Identities(after);w.WriteEndArray();w.WriteEndObject();
}
void Identities(LegacySourcePointChronology c){for(int s=0;s<c.StrokePointCounts.Length;s++)for(int p=0;p<c.StrokePointCounts.Span[s];p++){var v=c.Resolve(s,p);Ints(null,[v.Key.First,v.Key.Second,v.Key.Third,v.MetadataPointIndex,v.KeyRecordIndex,v.RemainderByte]);}}
void Points(string name,IReadOnlyList<InkStroke> source){w.WriteStartArray(name);foreach(var s in source){w.WriteStartArray();foreach(var p in s.Points)Ints(null,[BitConverter.SingleToInt32Bits(p.X),BitConverter.SingleToInt32Bits(p.Y)]);w.WriteEndArray();}w.WriteEndArray();}
void Ints(string? name,IEnumerable<int> values){if(name is null)w.WriteStartArray();else w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void UInts(string name,ReadOnlySpan<uint> values){w.WriteStartArray(name);foreach(uint v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid source scope accepted");}
static bool Same(IReadOnlyList<InkStroke> a,IReadOnlyList<InkStroke>b)=>a.Count==b.Count&&!a.Where((s,i)=>!s.Points.SequenceEqual(b[i].Points)).Any();
static byte[] Guide(float baseline,float height){var b=new byte[40];BinaryPrimitives.WriteSingleLittleEndian(b,baseline);BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(4),height);return b;}
static byte[] Descriptor(int first,int last,int line){var d=new byte[88];BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(16),line);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(36),first);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(40),last);return d;}
static byte[] Separator(){var b=new byte[88];b[0]=3;BinaryPrimitives.WriteInt64LittleEndian(b.AsSpan(32),-1);return b;}
static InkStroke[] Word(float x,float y)=>[
 new(new InkPoint[]{new(x,y+90),new(x,y+90),new(x,y+10),new(x+25,y+60),new(x+50,y+10),new(x+50,y+90)}),
 new(new InkPoint[]{new(x+70,y+10),new(x+70,y+90),new(x+120,y+10),new(x+120,y+90)}),
 new(new InkPoint[]{new(x+140,y+90),new(x+140,y+10),new(x+180,y+10),new(x+190,y+20),new(x+190,y+40),new(x+180,y+50),new(x+140,y+50)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
