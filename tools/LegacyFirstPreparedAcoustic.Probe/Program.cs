using System.Buffers.Binary;
using InkRecognition;
using System.Text.Json;
if(args.Length is not (0 or 2))throw new ArgumentException("Usage: LegacyFirstPreparedAcoustic.Probe [ru-ak.res engine-profile.json]");
byte[] resource=OwnSource();int offset=0;var activations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
if(args.Length==2){resource=File.ReadAllBytes(args[0]);offset=6120;using var json=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=json.RootElement.GetProperty("activations");activations=new(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());}
var classifier=LegacyFirstPreparedAcousticClassifier.ReadSource(resource,offset,activations);
InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
var original=new Dictionary<string,int[]>();int checks=0;
foreach(float scale in new[]{1f,1.5f})
{
 var row=LegacyOrdinaryRowAcousticInput.Prepare(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+30,p.Y*scale+20)).ToArray())).ToArray(),classifier.Model.RowProfile);
 var cuts=row.GenerateRussianCuts();var context=Enumerable.Repeat((short)-1024,28).ToArray();var counts=cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();
 foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
 {
  var ranges=ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray();
  var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,counts,0,false,false);
  var result=classifier.Classify(row,cuts,ranges,context,signs.Start,signs.End,10,10);var costs=result.IntegerCosts.ToArray();string key=string.Join(',',ids);
  if(scale==1)original.Add(key,costs);else if(!original[key].AsSpan().SequenceEqual(costs))throw new Exception("First-model scale/translation costs differ.");
  if(result.Geometry.Ranges.Length!=ids.Length||result.MergedGeometry.Strokes.Count!=ids.Length||costs.Length!=classifier.Model.TrajectoryNetwork.OutputWidth)throw new Exception("Geometry/NN count mismatch.");
  if(args.Length==0 && !result.Scores.TrajectoryProbability32768.Span.SequenceEqual(new[]{16384,16384}))throw new Exception("Synthetic zero-network probabilities differ.");
  if(ids.Length==3)
  {
   var reduced=classifier.Classify(row,cuts,ranges,context,signs.Start,signs.End,10,1);
   if(reduced.MergedGeometry.Strokes.Count!=1||reduced.MergedGeometry.Strokes[0].Points.Length!=result.Geometry.Strokes.Sum(s=>s.Points.Length)||
       !reduced.MergedGeometry.Strokes[0].Points.SequenceEqual(result.Geometry.Strokes.SelectMany(s=>s.Points.ToArray()).ToArray()))
       throw new Exception("Caller explicit pen-up threshold failed to preserve all chronologically normalized points.");
  }
  Console.WriteLine($"Prepared firstNN scale{scale} physical[{key}] top5 {string.Join(',',costs.Select((c,i)=>(c,i)).OrderBy(v=>v.c).Take(5).Select(v=>$"{v.i}:{v.c}"))}");checks++;
 }
 var sampleRanges=new LegacyInkRange[]{new(0,0,counts[0])};var foreign=LegacyOrdinaryRowAcousticInput.Prepare(mir,classifier.Model.RowProfile).GenerateRussianCuts();
 ExpectInvalid(()=>classifier.Classify(row,foreign,sampleRanges,context,1024,1024,10,10));
 ExpectInvalid(()=>classifier.Classify(row,cuts,sampleRanges,new short[27],1024,1024,10,10));
 ExpectInvalid(()=>classifier.Classify(row,cuts,sampleRanges,context,0,1024,10,10));
 ExpectInvalid(()=>classifier.Classify(row,cuts,sampleRanges,context,1024,1024,0,10));
 using var cts=new CancellationTokenSource();cts.Cancel();try{classifier.Classify(row,cuts,sampleRanges,context,1024,1024,10,10,cancellationToken:cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
}
byte[] badFlags=resource.ToArray();badFlags[classifier.Model.OrdinaryNodeOffset+16]=0;
ExpectInvalid(()=>LegacyFirstPreparedAcousticClassifier.ReadSource(badFlags,offset,activations));
Console.WriteLine($"PASS{checks} owned prepared first-model groups,scale/translation,chronology/context/sign/ownership/rejection. Caller context neutral and whole glyph ranges; no native word-winner/accuracy proof.");
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid first-model prepared state accepted.");}
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
    p=(int)s.Position;Header(w,0x21d,0x110);w.Write((ushort)0xffff);w.Write((short)100);w.Write((byte)1);w.Write(Network(30,0x7001));w.Write(new byte[3*4+2+2+4+1]);w.Write((byte)2);w.Write(new byte[2]);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);
    w.Write(new byte[2+4+5*2+7*4]);w.Write(new byte[6]);w.Write(new byte[2+2+4+4+2+2]);w.Write(new byte[2]);
    for(int i=0;i<7;i++){w.Write((byte)0);w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?2:0));if(i>=2)w.Write(0f);w.Write((byte)0);w.Write((byte)(i<2?1:0));if(i<2)w.Write(Network(i==0?247:229,(ushort)(i==0?0x1f:0x2f)));w.Write((byte)0);}
    Finish(s,p);w.Write(new byte[190]);Finish(s,0);return s.ToArray();
}
static void Header(BinaryWriter w,uint kind,ushort version){w.Write(0x0101fe90u);w.Write(0u);w.Write(kind);w.Write((ushort)0);w.Write(version);}
static void Finish(MemoryStream s,int p){long save=s.Position;s.Position=p+4;using(var w=new BinaryWriter(s,System.Text.Encoding.UTF8,true))w.Write((uint)(save-p-8));s.Position=save;}
