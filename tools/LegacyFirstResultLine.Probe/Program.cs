using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=2)throw new ArgumentException("Pass own controls and pinned RU acoustic resource.");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));var root=document.RootElement;
byte[] resource=File.ReadAllBytes(args[1]);
if(Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned resource required.");
int transfers=0,fitCount=0,fallbacks=0;
foreach(var c in root.GetProperty("cases").EnumerateArray())
{
    var v=c.GetProperty("markers").EnumerateArray().Select(x=>x.GetUInt16()).ToArray();var markers=new LegacyFirstResultLineMarkers(v[0],v[1],v[2],v[3],v[4],v[5],v[6]);
    var item=LegacyFirstResultLinePreparation.ReadSourceRow(Convert.FromBase64String(c.GetProperty("source").GetString()!),markers);
    if(!Encode(item).AsSpan().SequenceEqual(Convert.FromBase64String(c.GetProperty("expected").GetString()!)))throw new Exception($"Binary transfer differs {transfers}.");transfers++;
}
foreach(var v in root.GetProperty("fits").EnumerateArray())
{
    var c=v.GetProperty("case");float[] p=ReadFloats(c.GetProperty("profile"));var profile=new LegacyCharacterLineFitProfile((byte)p[0],(byte)p[1],p[2],p[3],p[4],p[5],p[6]);
    var items=c.GetProperty("items").EnumerateArray().Select(i=>{var b=ReadFloats(i.GetProperty("bounds"));var candidates=i.GetProperty("candidates").EnumerateArray().Select(j=>{var a=ReadFloats(j);return new LegacyCharacterLineCandidate(a[0],a[1],(int)a[2],a[3],a[4],a[5],a[6]);}).ToArray();return new LegacyPreparedCharacterLineItem(i.GetProperty("known").GetBoolean(),i.GetProperty("clip").GetBoolean(),i.GetProperty("marker").GetBoolean(),b[0],b[1],b[2],b[3],candidates);}).ToArray();
    var b=ReadFloats(v.GetProperty("bounds"));var result=LegacyCharacterLineFitCompletion.Estimate(items,profile,new(b[0],b[1],b[2],b[3]),c.GetProperty("force").GetBoolean());
    Check(result,v.GetProperty("output"));if((int)result.Basis!=v.GetProperty("basis").GetInt32())throw new Exception("Completion basis differs.");fitCount++;
}
foreach(var v in root.GetProperty("fallbacks").EnumerateArray())
{
    float[] b=ReadFloats(v.GetProperty("bounds"));var profile=new LegacyCharacterLineFitProfile(2,20,-.4f,.4f,v.GetProperty("minimum").GetSingle(),float.MaxValue,0);
    var result=LegacyCharacterLineFitCompletion.Estimate([],profile,new(b[0],b[1],b[2],b[3]));Check(result,v.GetProperty("output"));
    if(result.Basis!=LegacyCharacterLineFitBasis.RejectedPrimaryBoundingBox||result.Primary is not null)throw new Exception("Fallback presented as primary fit.");fallbacks++;
}
var actualProfile=LegacyFirstResultLineProfile.ReadV103Node(resource,187937);
if(actualProfile.Option10!=0||actualProfile.Option14!=100||actualProfile.Option18!=0||actualProfile.MinimumItemCount!=2||actualProfile.MaximumItemCount!=20)throw new Exception("Actual24B profile differs.");
foreach(var v in root.GetProperty("profiles").EnumerateArray())
{
    var p=actualProfile.PrepareFitProfile(v.GetProperty("dpi").GetSingle(),v.GetProperty("count").GetInt32(),v.GetProperty("line").EnumerateArray().Select(x=>x.GetUInt32()).ToArray());
    Equal(p.MinimumHeight,v.GetProperty("minimum").GetSingle());Equal(p.MaximumBoxWidth,v.GetProperty("width").GetSingle());
    if(p.MinimumSlope!=actualProfile.MinimumSlope||p.MaximumSlope!=actualProfile.MaximumSlope||p.KnownGlyphWeightCap!=0)throw new Exception("Native config forwarding differs.");
}
//Actual resource categories and labels, actual cropped raw-point geometry, but prepared
//alternative labels/selection: this deliberately does not claim an inferred word winner.
var categories=LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out _);var alphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),0x103);
ushort Label(string text)=>alphabet.TryGetLabel(text,out var value)?value:ushort.MaxValue;
var actualMarkers=new LegacyFirstResultLineMarkers(Label("\U000f0002"),Label("\U000f0003"),Label("-"),Label("_"),Label("\u30fc"),Label("\u4e00"),Label("\ufffd"));
var records=new List<LegacyFirstResultSourceRecord>();int composed=0;int primaryCount=0;
for(ushort label=0;label<categories.StartGeometryMap.Length;label++)
{
    if(!alphabet.TryGetText(label,out _))continue;
    var strokes=new[]{new InkStroke([new InkPoint(9,1),new InkPoint(20,20),new InkPoint(30,90),new InkPoint(80,110)])};
    var row=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),label);BinaryPrimitives.WriteSingleLittleEndian(row.AsSpan(12),.75f);
    var result=LegacyFirstResultSourceRecord.Create(row,0,[new(new(0,0,1),new(0,0,2))],[0],strokes,categories,2);
    records.Clear();records.Add(result);var items=LegacyFirstResultLinePreparation.Prepare(records,0,1,actualMarkers);
    byte[] transfer=Encode(items[0]);if(BinaryPrimitives.ReadSingleLittleEndian(transfer.AsSpan(8))!=20||BinaryPrimitives.ReadSingleLittleEndian(transfer.AsSpan(12))!=30||items[0].CandidateCount!=1)throw new Exception("Actual cropped source geometry lost.");
    var bounds=LegacyCharacterLineFitCompletion.ScanPhysicalInterval(strokes,0,0);
    if(bounds!=new LegacyCharacterLineInputBounds(9,1,80,110))throw new Exception("Physical input envelope confused with cropped glyph geometry.");
    var complete=LegacyCharacterLineFitCompletion.Estimate(items,actualProfile.PrepareFitProfile(0,0,[]),bounds);
    if(!float.IsFinite(complete.Baseline)||!float.IsFinite(complete.Height)||!float.IsFinite(complete.Slope))throw new Exception("Actual source completion nonfinite.");
    if(complete.Basis==LegacyCharacterLineFitBasis.PrimaryProfileFit)primaryCount++;composed++;
}
var empty=new InkStroke([]);if(LegacyCharacterLineFitCompletion.ScanPhysicalInterval([empty],0,0)!=new LegacyCharacterLineInputBounds())throw new Exception("Native empty physical bounds differ.");
var raw=new byte[720];BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),65535);BinaryPrimitives.WriteSingleLittleEndian(raw.AsSpan(80),float.NaN);
if(LegacyFirstResultLinePreparation.ReadSourceRow(raw,default).CandidateCount!=0)throw new Exception("Inactive NaN should not be read through early sentinel.");
BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),0);Reject(()=>LegacyFirstResultLinePreparation.ReadSourceRow(raw,default));
Reject(()=>LegacyFirstResultLinePreparation.ReadSourceRow(new byte[719],default));Reject(()=>LegacyFirstResultLinePreparation.Prepare(records,1,0,actualMarkers));
Reject(()=>actualProfile.PrepareFitProfile(96,1,[]));Reject(()=>LegacyFirstResultLineProfile.ReadV103Node(resource,187938));
using var cancel=new CancellationTokenSource();cancel.Cancel();Reject(()=>LegacyFirstResultLinePreparation.Prepare(records,0,1,actualMarkers,cancel.Token));
Console.WriteLine($"PASS {transfers} independent720→584 transfers, {fitCount} NumPy full-fit completions, {fallbacks} native explicit fallback boundaries, {root.GetProperty("profiles").GetArrayLength()} profile-metadata variants, actual RU24B + {composed} category/cropped-point/source compositions ({primaryCount} primary fits). Prepared glyph selection/alphabet marker IDs; no main-frame orchestration or OCR winner claim.");
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Invalid input accepted.");}
static float[] ReadFloats(JsonElement values)=>values.EnumerateArray().Select(v=>v.GetSingle()).ToArray();
static void Equal(float a,float b){if(!float.IsFinite(a)||Math.Abs((double)a-b)>Math.Max(1e-5,Math.Abs((double)b)*2e-5))throw new Exception($"Float differs {a}/{b}.");}
static void Check(LegacyCharacterLineCompletedFit r,JsonElement v){var a=ReadFloats(v);Equal(r.Baseline,a[0]);Equal(r.Height,a[1]);Equal(r.Slope,a[2]);}
static byte[] Encode(LegacyPreparedCharacterLineItem item)
{
    var b=new byte[584];b[0]=(byte)(item.KnownGlyph?1:0);b[1]=(byte)(item.ClipVerticalToWidth?1:0);b[2]=(byte)(item.DownweightMarker?1:0);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(4),item.CandidateCount);
    void F(int o,float v)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(o),v);
    F(8,item.Left);F(12,item.Right);F(16,item.Top);F(20,item.Bottom);var slots=item.CopyPreparedSlots();
    for(int i=0;i<20;i++){var c=slots[i];F(24+i*8,c.BaseWeight);F(28+i*8,c.WeightCoefficient);BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(184+i*4),c.Kind);F(264+i*8,c.MainTop);F(268+i*8,c.AlternateTop);F(424+i*8,c.MainBottom);F(428+i*8,c.AlternateBottom);}
    return b;
}
