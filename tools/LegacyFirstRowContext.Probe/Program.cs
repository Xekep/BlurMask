using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length is not (1 or 3))throw new ArgumentException("Usage: LegacyFirstRowContext.Probe controls.json [ru-ak.res engine-profile.json]");
using(var reference=JsonDocument.Parse(File.ReadAllBytes(args[0])))
{
 int checkedCases=0,divergent=0;
 foreach(var c in reference.RootElement.EnumerateArray())
 {
  var strokes=c.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  float height=c.GetProperty("height").GetSingle(),xr=c.GetProperty("xReference").GetSingle(),yr=c.GetProperty("yReference").GetSingle();bool maximum=c.GetProperty("maximumX").GetBoolean(),pointmean=c.GetProperty("pointMeanY").GetBoolean();
  var output=LegacyFloatContextGeometry.Extract(strokes,height,xr,yr,maximum,pointmean);var expected=c.GetProperty("expected").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
  for(int i=0;i<30;i++)if(Math.Abs(output[i]-expected[i])>1.5e-6f)throw new Exception($"Float context case{checkedCases} channel{i}: {output[i]} vs{expected[i]}");
  var rounded=strokes.Select(s=>new LegacyInkStroke(s.Points.Select(p=>new LegacyInkPoint((short)p.X,(short)p.Y)).ToArray())).ToArray();
  var shortOutput=LegacyContextGeometry.Extract(rounded,height,xr,yr,maximum,pointmean);
  if(!output.AsSpan().SequenceEqual(shortOutput))divergent++;
  checkedCases++;
 }
 if(checkedCases!=512||divergent<300)throw new Exception("Missing fractional-vs-truncated coverage.");
 Console.WriteLine($"PASS{checkedCases} independent float226 geometry/raster controls;{divergent} differ from premature short truncation.");
}
byte[] resource=OwnSource();int offset=0;var activations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
if(args.Length==3){resource=File.ReadAllBytes(args[1]);offset=6120;using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=json.RootElement.GetProperty("activations");activations=new(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());}
var model=LegacyFirstAcousticModel.ReadSource(resource,offset);var profile=LegacyFirstRowContextProfile.ReadSource(resource,model);
int settings=model.OrdinaryNodeOffset+21+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(model.OrdinaryNodeOffset+25));
foreach(int field in new[]{13,14,20,22,23})
{
 byte[] damaged=resource.ToArray();damaged[settings+field]=0xff;
 ExpectInvalid(()=>LegacyFirstRowContextProfile.ReadSource(damaged,model));
}
ExpectInvalid(()=>LegacyFloatContextGeometry.Extract([Stroke((float.NaN,1),(1,1))],10,0,0));
ExpectInvalid(()=>LegacyFloatContextGeometry.Extract([new InkStroke([])],10,0,0));
ExpectInvalid(()=>LegacyFloatContextGeometry.Extract([new InkStroke(null!)],10,0,0));
var tiny=LegacyFloatContextGeometry.Extract([Stroke((0,0),(1e-10f,1e-10f))],10,0,0);
if(tiny[6]!=-.5f||tiny[7]!=-.5f||tiny[13]!=0||tiny[14]!=0)throw new Exception("Native tiny-point context stage differs.");
if(profile.MaximumWindowStrokes!=3||profile.LeftVariance!=1||profile.RightVariance!=1||profile.NonblankCutoff!=.1f||profile.Selector!=2||profile.NativeBlankClass!=28||profile.UsePointMeanY||profile.UseMaximumX)throw new Exception("First typed context configuration differs.");
InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
var row=LegacyOrdinaryRowAcousticInput.Prepare(mir,model.RowProfile);var cuts=row.GenerateRussianCuts();var context=LegacyFirstRowContext.Prepare(resource,offset,row,activations);
if((args.Length==1&&context.Groups.Count!=9)||context.Groups.Count is <1 or >9||context.Groups.Any(g=>g.EndPhysicalIndex-g.StartPhysicalIndex>=3))throw new Exception($"Invalid first-context accepted group count/bounds {context.Groups.Count}.");
var counts=cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();int scored=0;
foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
{
 var ranges=ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray();var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,counts,0,false,false);
 var fixedContext=context.GetContext(cuts,ranges);if(fixedContext.Length!=28||fixedContext.All(v=>v==-1024))throw new Exception("Actual context not populated.");
 var scores=context.Classify(cuts,ranges,signs.Start,signs.End,10,10);var costs=scores.IntegerCosts.ToArray();
 if(args.Length==1&&fixedContext.Any(v=>v!=fixedContext[0]))throw new Exception("Own equal-output context network lost equal class weighting.");
 Console.WriteLine($"Actual first-row float context groups{context.Groups.Count} physical[{string.Join(',',ids)}] context [{string.Join(',',fixedContext)}] top5 {string.Join(',',costs.Select((c,i)=>(c,i)).OrderBy(v=>v.c).Take(5).Select(v=>$"{v.i}:{v.c}"))}");scored++;
}
var foreign=LegacyOrdinaryRowAcousticInput.Prepare(mir,model.RowProfile).GenerateRussianCuts();ExpectInvalid(()=>context.GetContext(foreign,[new(0,0,counts[0])]));
using(var cts=new CancellationTokenSource()){cts.Cancel();try{context.GetContext(cuts,[new(0,0,counts[0])],cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}}
Console.WriteLine($"PASS typed first227 configuration, float source265 -> {context.Groups.Count} accepted context windows -> resource selector/weighting -> first247/229: {scored} glyphs. Already-selected ordinary row only; no finalcache/word-winner accuracy claim.");
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid context state accepted.");}
static byte[] Network(int width,ushort kind,int outputs=2)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(0u);w.Write((byte)0x12);w.Write(kind);w.Write(width);w.Write(2);w.Write(0u);w.Write(0u);w.Write(outputs+1);w.Write((ushort)10);
    w.Write(new byte[width]);w.Write(new byte[]{1,1});w.Write(new byte[outputs*2]);w.Write(Enumerable.Repeat((byte)1,outputs+1).ToArray());var bytes=s.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
}
static byte[] OwnSource()
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);
    Header(w,0x247,0x10d);w.Write((ushort)0x201);w.Write((ushort)0x100);w.Write(new byte[4+56]);w.Write(0);w.Write(0);w.Write((byte)0);w.Write(0x135);w.Write(new byte[4]);w.Write(0f);
    foreach(float f in new[]{.96f,.85f,3f,10f})w.Write(f);w.Write(new byte[4]);w.Write(.25f);w.Write((byte)0);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(2);
    int p=(int)s.Position;Header(w,0x24a,0x102);w.Write(0);w.Write(100f);w.Write((byte)1);w.Write(-.2f);w.Write(.2f);w.Write(new byte[]{4,6,0});Finish(s,p);
    p=(int)s.Position;Header(w,0x265,0x102);foreach(float f in new[]{.05f,.075f,.075f,.15f,.35f,.1f,.15f,.0001f})w.Write(f);Finish(s,p);
    w.Write((byte)1);p=(int)s.Position;Header(w,0x297,0x106);Finish(s,p);p=(int)s.Position;Header(w,0x21b,0x10a);Finish(s,p);
    p=(int)s.Position;Header(w,0x21d,0x110);w.Write((ushort)0xffff);w.Write((short)100);w.Write((byte)1);w.Write(Network(30,0x7001,28));w.Write(0f);w.Write(1f);w.Write(1f);w.Write((byte)0);w.Write((byte)3);w.Write((ushort)28);w.Write(.1f);w.Write((byte)1);w.Write((byte)2);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);
    w.Write(new byte[2+4+5*2+7*4]);w.Write(new byte[6]);w.Write(new byte[2+2+4+4+2+2]);w.Write(new byte[2]);
    for(int i=0;i<7;i++){w.Write((byte)0);w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?2:0));if(i>=2)w.Write(0f);w.Write((byte)0);w.Write((byte)(i<2?1:0));if(i<2)w.Write(Network(i==0?247:229,(ushort)(i==0?0x1f:0x2f)));w.Write((byte)0);}
    Finish(s,p);w.Write(new byte[190]);Finish(s,0);return s.ToArray();
}
static void Header(BinaryWriter w,uint kind,ushort version){w.Write(0x0101fe90u);w.Write(0u);w.Write(kind);w.Write((ushort)0);w.Write(version);}
static void Finish(MemoryStream s,int p){long save=s.Position;s.Position=p+4;using(var w=new BinaryWriter(s,System.Text.Encoding.UTF8,true))w.Write((uint)(save-p-8));s.Position=save;}
