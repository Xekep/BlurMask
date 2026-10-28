using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Security.Cryptography;

if(args.Length is not (0 or 2))throw new ArgumentException("Usage: LegacyFirstAcoustic.Probe [ru-ak.res engine-profile.json]");
byte[] fixture=OwnSource();var model=LegacyFirstAcousticModel.ReadSource(fixture,0);
if(model.ContextSelector!=2||model.NormalizationTarget!=100||model.TrajectoryNetwork.InputWidth!=247||model.RasterNetwork.InputWidth!=229||
    !model.GetFreshWeights().AsSpan().SequenceEqual(new float[]{1,1,0,0,0,0,0}))throw new Exception("Owned first profile differs.");
int rejected=0;
for(int length=0;length<fixture.Length;length++){ExpectInvalid(()=>LegacyFirstAcousticModel.ReadSource(fixture.AsSpan(0,length),0));rejected++;}
var activations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
for(int i=0;i<128;i++)
{
    short[] a=Enumerable.Range(0,247).Select(j=>(short)((i*31+j*7)%2049-1024)).ToArray();
    short[] b=Enumerable.Range(0,229).Select(j=>(short)((i*17+j*3)%2049-1024)).ToArray();
    var scores=model.EvaluatePreparedInputs(a,b,activations);
    //Own zero-weight networks: both classes have exactly equal raw dots,
    //therefore each integer probability is16384 irrespective of supplied input.
    if(!scores.TrajectoryProbability32768.Span.SequenceEqual(new[]{16384,16384})||
        !scores.RasterProbability32768.Span.SequenceEqual(new[]{16384,16384}))throw new Exception("Own analytic equal-output result differs.");
    int cost=LegacyAcousticFixedLog.ProbabilityCost(16384);
    if(!scores.GetFreshIntegerCosts().AsSpan().SequenceEqual(new[]{cost,cost}))throw new Exception("Fresh mode2 ensemble differs.");
    if(scores.GetFreshIntegerCosts(new byte[]{0,1})[0]!=LegacyAcousticFixedLog.ImpossibleCost)throw new Exception("Explicit class mask differs.");
}
ExpectInvalid(()=>model.EvaluatePreparedInputs(new short[246],new short[229],activations));
ExpectInvalid(()=>model.EvaluatePreparedInputs(new short[247],new short[228],activations));
byte[] bad=fixture.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bad.AsSpan(model.OrdinaryNodeOffset+8),0x21b);ExpectInvalid(()=>LegacyFirstAcousticModel.ReadSource(bad,0));
bad=fixture.ToArray();int dynamicFlag=model.OrdinaryNodeOffset+16+4+1+Network(30,0x7001).Length+3*4+2+2+4+1+1+2;bad[dynamicFlag]=1;ExpectInvalid(()=>LegacyFirstAcousticModel.ReadSource(bad,0));
using(var cts=new CancellationTokenSource()){cts.Cancel();try{LegacyFirstAcousticModel.ReadSource(fixture,0,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}}
//Owned snapshots must not retain caller resource storage.
Array.Fill(fixture,(byte)0);if(model.EvaluatePreparedInputs(new short[247],new short[229],activations).GetFreshIntegerCosts().Length!=2)throw new Exception("Resource snapshot lost ownership.");
Console.WriteLine($"PASS owned source/typed first21d, {rejected} truncation controls,128 analytic vector/mask/noDynamic ensembles,shape/mode/cancellation rejection.");
if(args.Length==2)
{
    byte[] resource=File.ReadAllBytes(args[0]);model=LegacyFirstAcousticModel.ReadSource(resource,6120);
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[1]));var tables=json.RootElement.GetProperty("activations");
    activations=new(tables.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),tables.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
    if(model.OrdinaryNodeOffset!=13082||model.PrefixBytesConsumed!=187648-13082||model.ContextSelector!=2||model.TrajectoryNetwork.HiddenWidth!=200||model.TrajectoryNetwork.OutputWidth!=180||model.RasterNetwork.HiddenWidth!=200)
        throw new Exception("Actual first model identity differs.");
    var one=Stroke((0,80),(0,0),(25,50),(50,0),(50,80));
    var a=LegacyStrokeFeatures.ExtractIsolatedInput(one);var b=LegacyRasterFeatures.ExtractIsolatedInput(one);
    var scores=model.EvaluatePreparedInputs(a,b,activations);int[] costs=scores.GetFreshIntegerCosts();
    //Independent byte-slice loader reproduces exactly the typed ownership route.
    var rawA=LegacyInkNetwork.Load(resource.AsSpan(19195,85995)).Evaluate(a,activations);
    var rawB=LegacyInkNetwork.Load(resource.AsSpan(105197,82395)).Evaluate(b,activations);
    var expectedA=NativeFirstMixedReference(resource,rawA);
    var expectedB=NativeFirstMixedReference(resource,rawB);
    if(!expectedA.AsSpan().SequenceEqual(scores.TrajectoryProbability32768.Span)||!expectedB.AsSpan().SequenceEqual(scores.RasterProbability32768.Span))throw new Exception("Typed/native mixed model outputs differ.");
    if(expectedA.AsSpan().SequenceEqual(LegacyClassScores.Convert(rawA).Probability32768.Span)||
        expectedB.AsSpan().SequenceEqual(LegacyClassScores.Convert(rawB).Probability32768.Span))throw new Exception("Actual first mixed/plain negative control lost sensitivity.");
    Console.WriteLine($"PASS actual source6120->ordinary13082->247/229 firstNN; first5 integer labels {string.Join(',',costs.Select((c,i)=>(c,i)).OrderBy(v=>v.c).Take(5).Select(v=>$"{v.i}:{v.c}"))}. Caller-isolated short ink only; no native normalization/context/word winner claim.");
}

static LegacyInkStroke[] Stroke(params(short X,short Y)[] points)=>[new(points.Select(p=>new LegacyInkPoint(p.X,p.Y)).ToArray())];
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid profile/input accepted.");}
static byte[] Network(int width,ushort kind)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(0u);w.Write((byte)0x12);w.Write(kind);w.Write(width);w.Write(2);w.Write(0u);w.Write(0u);w.Write(3);w.Write((ushort)10);
    w.Write(new byte[width]);w.Write(new byte[]{1,1});w.Write(new byte[4]);w.Write(new byte[]{1,1,1});var bytes=s.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
}
static byte[] OwnSource()
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);
    Header(w,0x247,0x10d);w.Write((ushort)0x201);w.Write((ushort)0x100);w.Write(new byte[4+56]);w.Write(0);w.Write(0);w.Write((byte)0);w.Write(0x135);w.Write(new byte[4]);w.Write(0f);
    foreach(float f in new[]{.96f,.85f,3f,10f})w.Write(f);w.Write(new byte[4]);w.Write(.25f);w.Write((byte)0);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(2);
    int p=(int)s.Position;Header(w,0x24a,0x102);w.Write(0);w.Write(100f);w.Write((byte)1);w.Write(-.2f);w.Write(.2f);w.Write(new byte[]{4,6,0});Finish(s,p);
    p=(int)s.Position;Header(w,0x265,0x102);foreach(float f in new[]{.05f,.075f,.075f,.15f,.35f,.1f,.15f,.0001f})w.Write(f);Finish(s,p);
    w.Write((byte)1);p=(int)s.Position;Header(w,0x297,0x106);Finish(s,p);p=(int)s.Position;Header(w,0x21b,0x10a);Finish(s,p);
    p=(int)s.Position;Header(w,0x21d,0x110);w.Write((ushort)0);w.Write((short)100);w.Write((byte)1);w.Write(Network(30,0x7001));w.Write(new byte[3*4+2+2+4+1]);w.Write((byte)2);w.Write(new byte[2]);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);
    w.Write(new byte[2+4+5*2+7*4]);w.Write(new byte[6]);w.Write(new byte[2+2+4+4+2+2]);w.Write(new byte[2]);
    for(int i=0;i<7;i++){w.Write((byte)0);w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?2:0));if(i>=2)w.Write(0f);w.Write((byte)0);w.Write((byte)(i<2?1:0));if(i<2)w.Write(Network(i==0?247:229,(ushort)(i==0?0x1f:0x2f)));w.Write((byte)0);}
    Finish(s,p);w.Write(new byte[190]);Finish(s,0);return s.ToArray();
}
static void Header(BinaryWriter w,uint kind,ushort version){w.Write(0x0101fe90u);w.Write(0u);w.Write(kind);w.Write((ushort)0);w.Write(version);}
static void Finish(MemoryStream s,int p){long save=s.Position;s.Position=p+4;using(var w=new BinaryWriter(s,System.Text.Encoding.UTF8,true))w.Write((uint)(save-p-8));s.Position=save;}

// Independent native decomposition: ordinary EXP excludes AL.runtimeE and the
// special sigmoid is evaluated separately. This never calls the production mixed helper.
static int[] NativeFirstMixedReference(ReadOnlySpan<byte> resource,ReadOnlySpan<int> raw)
{
    if(Convert.ToHexStringLower(SHA256.HashData(resource))!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978"||resource[187648]!=0)
        throw new Exception("Actual first resource/unmasked dispatch required.");
    int special=BinaryPrimitives.ReadUInt16LittleEndian(resource[(6219+6)..]);
    if(special>=raw.Length)throw new Exception("Owned AL special output differs.");
    var ordinary=new int[raw.Length-1];for(int i=0,j=0;i<raw.Length;i++)if(i!=special)ordinary[j++]=raw[i];
    var normalized=LegacyClassScores.Convert(ordinary).Probability32768.Span;var expected=new int[raw.Length];
    for(int i=0,j=0;i<expected.Length;i++)if(i!=special)expected[i]=normalized[j++];
    float exponent=((float)raw[special]*-2f)/1048576f;
    expected[special]=exponent>75f?0:(int)(32768.0/(Math.Exp((double)exponent)+1.0)+.5);
    return expected;
}
