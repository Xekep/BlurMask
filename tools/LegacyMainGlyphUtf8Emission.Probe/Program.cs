using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication line30 line50 normalization circles capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
byte[] line30=File.ReadAllBytes(args[6]),line50=File.ReadAllBytes(args[7]);
using var nf=File.OpenRead(args[8]);using var circleFile=File.OpenRead(args[9]);
var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(circleFile);
InkStroke[][] inputs=[Word(0),Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
int actual=0,prepared=0,refused=0,guards=0;
using var output=File.Create(args[10]);using var writer=new Utf8JsonWriter(output);
writer.WriteStartObject();writer.WriteStartArray("inputs");
for(int input=0;input<inputs.Length;input++)
{
    var raw=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,inputs[input],0,table,cases,publication)
        .Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
    var emission=LegacyFreshMainGlyphUtf8Emission.Prepare(raw,normalization,circles);
    if(!ReferenceEquals(emission.TopOwner,raw.Session.Ownership.TopOwner)||
        !ReferenceEquals(emission.PrimaryAlphabet,raw.Session.Plan.SourcePolicy.Model.AcousticAlphabet)||
        emission.ControlSymbolCount!=0||emission.AlternateOffsetCount!=0)throw new InvalidDataException("Actual TOP owners were replaced.");
    foreach(string value in new[]{"\n"," ","\u00a0","A","\u0430","\ufffd",""})
        if(emission.ReadControlLabel(value)!=65535)throw new InvalidDataException("Fresh lookup inserted or reused acoustic symbols.");
    var pool=raw.Session.Ownership.GlyphPool;
    writer.WriteStartObject();writer.WriteNumber("input",input);writer.WriteStartArray("actual");
    foreach(var node in pool.CopyOwnerNodes())
    {
        var snapshot=new LegacyNativeGlyphRecordOwner([node]);int count=node.Records.Length/40;
        for(int i=0;i<count;i++)
        {
            var glyph=snapshot.Resolve(node.Key|((ulong)i<<40));writer.WriteStartObject();
            writer.WriteNumber("reference",glyph.Reference);writer.WriteBase64String("row40",glyph.ReadRecord());
            Write(()=>emission.Emit(glyph));writer.WriteEndObject();actual++;
        }
    }
    writer.WriteEndArray();
    if(input==0)
    {
        writer.WriteStartArray("preparedDescriptors");
        //Numeric descriptor tests retain their explicit prepared boundary;
        //they never mutate a decoded pool record or become inference labels.
        foreach(int kind in Enumerable.Range(0,8))
        foreach(ushort symbol in Enumerable.Range(0,emission.PrimaryAlphabet.LabelCount).Select(i=>(ushort)i)
            .Concat(new ushort[]{65535,65532,65531,65529,65526,65523,65522,65521,65520,65519,65518,65517,65516,65515,32764}))
        {
            var d=new LegacyNativeGlyphSymbolDescriptor((ushort)(kind|0xa500),symbol);
            writer.WriteStartObject();writer.WriteNumber("flags",d.KindFlags);writer.WriteNumber("symbol",symbol);
            Write(()=>emission.EmitPreparedDescriptor(d));writer.WriteEndObject();prepared++;
        }
        writer.WriteEndArray();
        var first=pool.CopyOwnerNodes().First(n=>n.Records.Length!=0);byte[] altered=first.Records.ToArray();altered[6]^=1;
        var foreign=new LegacyNativeGlyphRecordOwner([new(first.Key,first.PackedCount,altered)]).Resolve(first.Key);
        Reject(()=>emission.Emit(foreign));
        using var canceled=new CancellationTokenSource();canceled.Cancel();
        Reject(()=>emission.EmitPreparedDescriptor(new(0,0),canceled.Token));
        Reject(()=>emission.ReadControlLabel(" ",canceled.Token));
        Reject(()=>LegacyFreshMainGlyphUtf8Emission.Prepare(raw,normalization,circles,canceled.Token));
    }
    writer.WriteEndObject();
    void Write(Func<byte[]> operation)
    {
        try{writer.WriteBase64String("utf8",operation());}
        catch(Exception e)when(e is InvalidDataException or NotSupportedException){writer.WriteBoolean("refused",true);refused++;}
    }
}
writer.WriteEndArray();writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS {inputs.Length} actual raw sources, {actual} genuine pool records, {prepared} explicitly prepared descriptors, {refused} bounded native/undefined refusals, {guards} guards. No accepted OCR/circle-winner claim.");
void Reject(Action operation){try{operation();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Ownership/cancellation guard ignored.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
