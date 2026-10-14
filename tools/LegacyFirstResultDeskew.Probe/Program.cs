using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("Pass own deskew controls and pinned RU acoustic resource.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0;
foreach(var c in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var strokes=c.GetProperty("physical").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var ranges=c.GetProperty("ranges").EnumerateArray().Select(r=>r.EnumerateArray().Select(p=>new LegacyFirstResultSourceRange(new(p[0][0].GetInt32(),p[0][1].GetInt32(),p[0][2].GetInt32()),new(p[1][0].GetInt32(),p[1][1].GetInt32(),p[1][2].GetInt32()))).ToArray()).ToArray();
    var raw=c.GetProperty("source").EnumerateArray().Select(v=>Convert.FromBase64String(v.GetString()!)).ToArray();var rows=raw.Select((r,i)=>new LegacyFirstResultLineRow(r,ranges[i])).ToArray();
    var guide=Convert.FromBase64String(c.GetProperty("guide").GetString()!);
    var result=LegacyFirstResultLineDeskew.Apply(strokes,c.GetProperty("first").GetInt32(),c.GetProperty("last").GetInt32(),rows,c.GetProperty("start").GetInt32(),c.GetProperty("end").GetInt32(),guide,c.GetProperty("minimum").GetSingle(),c.GetProperty("preserve").GetBoolean());
    if(!result.CopyGuide().Span.SequenceEqual(Convert.FromBase64String(c.GetProperty("expectedGuide").GetString()!)))throw new Exception($"Guide bytes differ{count}.");
    var points=result.CopyPhysicalPoints();var expected=c.GetProperty("expectedPoints").EnumerateArray().ToArray();
    for(int s=0;s<points.Length;s++){var e=expected[s].EnumerateArray().ToArray();for(int j=0;j<points[s].Length;j++){Equal(points[s][j].X,e[j][0].GetSingle());Equal(points[s][j].Y,e[j][1].GetSingle());}}
    var expectedRows=c.GetProperty("expectedRows").EnumerateArray().ToArray();
    for(int i=0;i<rows.Length;i++)
    {if(!result.Rows[i].CopyStorage().Span.SequenceEqual(Convert.FromBase64String(expectedRows[i].GetString()!)))throw new Exception($"720 row differs{count}/{i}.");if(!rows[i].CopyStorage().Span.SequenceEqual(raw[i])||!result.Rows[i].Ranges.SequenceEqual(ranges[i]))throw new Exception("Source mutation/membership change.");}
    var affine=c.GetProperty("affine");Equal(result.Cosine,affine[0].GetSingle());Equal(result.Sine,affine[1].GetSingle());Equal(result.TranslationX,affine[2].GetSingle());Equal(result.TranslationY,affine[3].GetSingle());
    byte[] snapshot=result.CopyGuide().ToArray();snapshot.AsSpan().Fill(255);if(!result.CopyGuide().Span.SequenceEqual(Convert.FromBase64String(c.GetProperty("expectedGuide").GetString()!)))throw new Exception("Owned guide leaked.");
    for(int s=0;s<points.Length;s++)if(points[s].Length!=0)points[s][0]=new(999,999);
    var fresh=result.CopyPhysicalPoints();for(int s=0;s<fresh.Length;s++)if(fresh[s].Length!=0&&fresh[s][0]==new InkPoint(999,999))throw new Exception("Owned points leaked.");count++;
}
byte[] data=new byte[720],g=new byte[40];BinaryPrimitives.WriteSingleLittleEndian(g.AsSpan(4),1);
var stroke=new InkStroke([new InkPoint(0,0),new InkPoint(20,30)]);var empty=new LegacyFirstResultLineRow(data,[]);
Reject(()=>new LegacyFirstResultLineRow(data,[new(new(0,0,0),new(0,0,1))]));
using var cancelled=new CancellationTokenSource();cancelled.Cancel();Reject(()=>LegacyFirstResultLineDeskew.Apply([stroke],0,0,[empty],0,1,g,0,false,cancelled.Token));
Reject(()=>LegacyFirstResultLineDeskew.Apply([stroke],0,0,[empty],1,0,g,0,false));
BinaryPrimitives.WriteSingleLittleEndian(g.AsSpan(8),float.MaxValue);Reject(()=>LegacyFirstResultLineDeskew.Apply([stroke],0,0,[empty],0,1,g,0,false));
BinaryPrimitives.WriteSingleLittleEndian(g.AsSpan(8),0);
var manyRanges=Enumerable.Repeat(new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,1)),4096).ToArray();var manyData=new byte[720];BinaryPrimitives.WriteInt64LittleEndian(manyData.AsSpan(8),4096);
var denseOwned=new LegacyFirstResultLineRow(manyData,manyRanges);
Reject(()=>LegacyFirstResultLineDeskew.Apply([stroke],0,0,Enumerable.Repeat(denseOwned,17).ToArray(),0,1,g,0,false));
byte[] resource=File.ReadAllBytes(args[1]);if(Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned RU resource required.");
var category=LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out _);var alphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),0x103);
var profile=LegacyFirstResultLineProfile.ReadV103Node(resource,187937).PrepareFitProfile(0,0,[]);int actualSources=0;
for(ushort label=0;label<category.StartGeometryMap.Length;label++)
{
    if(!alphabet.TryGetText(label,out _))continue;
    var original=new[]{new InkStroke([new InkPoint(-50,-50)]),new InkStroke([new InkPoint(5,-10),new InkPoint(20,20),new InkPoint(50,90),new InkPoint(60,110)])};
    var alt=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(alt.AsSpan(8),label);BinaryPrimitives.WriteSingleLittleEndian(alt.AsSpan(12),.75f);
    var source=LegacyFirstResultSourceRecord.Create(alt,0,[new(new(0,0,1),new(0,0,2))],[1],original,category,2);
    var markers=new LegacyFirstResultLineMarkers(65535,65535,65535,65535,65535,65535,65535);
    var prepared=LegacyFirstResultLinePreparation.Prepare([source],0,1,markers);var envelope=LegacyCharacterLineFitCompletion.ScanPhysicalInterval(original,1,1);
    var fit=LegacyCharacterLineFitCompletion.Estimate(prepared,profile,envelope);var guide=new byte[40];
    BinaryPrimitives.WriteSingleLittleEndian(guide,fit.Baseline);BinaryPrimitives.WriteSingleLittleEndian(guide.AsSpan(4),fit.Height);BinaryPrimitives.WriteSingleLittleEndian(guide.AsSpan(8),fit.Slope);
    BinaryPrimitives.WriteSingleLittleEndian(guide.AsSpan(12),fit.Baseline);BinaryPrimitives.WriteSingleLittleEndian(guide.AsSpan(16),fit.Height);
    var composed=LegacyFirstResultLineDeskew.Apply(original,1,1,[LegacyFirstResultLineRow.FromFirstResult(source)],0,1,guide,profile.MinimumHeight,false);
    if(!composed.CopyGuide().Span[8..].SequenceEqual(guide.AsSpan(8)))throw new Exception("Prepared pre-deskew guide8/C/10 tail changed.");
    var p=composed.CopyPhysicalPoints();if(!p[0].SequenceEqual(original[0].Points))throw new Exception("Outside descriptor physical buffer changed.");
    var row=composed.Rows[0].CopyStorage().Span;
    Equal(BinaryPrimitives.ReadSingleLittleEndian(row[24..]),Math.Min(p[1][1].X,p[1][2].X));Equal(BinaryPrimitives.ReadSingleLittleEndian(row[28..]),Math.Max(p[1][1].X,p[1][2].X));
    Equal(BinaryPrimitives.ReadSingleLittleEndian(row[32..]),Math.Min(p[1][1].Y,p[1][2].Y));Equal(BinaryPrimitives.ReadSingleLittleEndian(row[36..]),Math.Max(p[1][1].Y,p[1][2].Y));actualSources++;
}
Console.WriteLine($"PASS {count} binary32 affine/full720-byte/range-membership controls and{actualSources} actual RU category/cropped-source→C1A→deskew source compositions; zero-range opposite-corner branch, empty physical points, preserve guide flag, foreign mutation isolation and overflow/cancel guards. Selected glyph labels and missing marker lookup remain explicit prepared inputs; no full main-frame/OCR winner claim.");
static void Equal(float a,float b){if(!float.IsFinite(a)||BitConverter.SingleToInt32Bits(a)!=BitConverter.SingleToInt32Bits(b))throw new Exception($"F32 bits differ{a}/{b}: {BitConverter.SingleToInt32Bits(a):x}/{BitConverter.SingleToInt32Bits(b):x}.");}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Invalid input accepted.");}
