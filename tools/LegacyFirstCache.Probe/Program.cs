using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length is not (1 or 3))throw new ArgumentException("Usage: LegacyFirstCache.Probe old-cache-controls.json [ru-ak.res engine-profile.json]");
using var own=JsonDocument.Parse(File.ReadAllBytes(args[0]));byte[] ownAlphabet=Convert.FromBase64String(own.RootElement.GetProperty("alphabet").GetString()!);
byte[] resource=OwnSource(ownAlphabet);int offset=0;var activations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
if(args.Length==3){resource=File.ReadAllBytes(args[1]);offset=6120;using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=json.RootElement.GetProperty("activations");activations=new(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());}
var profile=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(resource,offset,out var model);
if(profile!=new LegacyOrdinaryAcousticCacheProfile(0,0,0,.9f,.9f,0,85173,1,10))throw new Exception("First cache resource profile mismatch.");
int sourceEnd=offset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(offset+4));
for(int i=0;i<sourceEnd;i+=17)ExpectInvalid(()=>LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(resource.AsSpan(0,i),offset,out _));
byte[] compressed=resource.ToArray();compressed[model.OrdinaryNodeOffset+12]=1;ExpectInvalid(()=>LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(compressed,offset,out _));
InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
var row=LegacyOrdinaryRowAcousticInput.Prepare(mir,model.RowProfile);var cuts=row.GenerateRussianCuts();var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,offset,row,activations);
if(scorer.CacheWidth!=195||scorer.Alphabet.LabelCount!=192)throw new Exception("Owned first cache/alphabet width differs.");
if(args.Length==3&&(scorer.AlphabetOffset!=6219||!resource.AsSpan(6219,scorer.Alphabet.BytesConsumed).SequenceEqual(resource.AsSpan(99,scorer.Alphabet.BytesConsumed))))throw new Exception("Actual own first alphabet identity differs.");
byte[] mask=Enumerable.Repeat((byte)1,192).ToArray();
var state=new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
var counts=cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();
foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
{
 var ranges=ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray();var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,counts,0,false,false);
 var scores=scorer.Score(cuts,ranges,signs.Start,signs.End,10,10,mask,state,[]);
 if(scores.Cache.Costs.Length!=195||scores.Glyph.IntegerCosts.Length!=model.TrajectoryNetwork.OutputWidth||scores.Cache.Candidates.Length is <1 or >10)throw new Exception("First cache width/shortlist differs.");
 if(args.Length==1&&(scores.Cache.Costs.Span[0]!=5678||scores.Cache.Costs.Span[1]!=5678||scores.Cache.Candidates.Length!=2))throw new Exception("Own equal-output analytic cache differs.");
 string Text(ushort i)=>scorer.Alphabet.TryGetText(i,out string text)?text:"?";
 Console.WriteLine($"First source alphabet{scorer.AlphabetOffset} raw -> final195 physical[{string.Join(',',ids)}]: {string.Join(',',scores.Cache.Candidates.ToArray().Take(5).Select(v=>$"{Text(v.Symbol)}:{v.Cost}"))}. Explicit masks/scope flags/pairs.");
}
ExpectInvalid(()=>scorer.Score(cuts,[new(0,0,counts[0])],1024,1024,10,10,new byte[191],state,[]));
Console.WriteLine("PASS first resource-owned cache profile/model/inline alphabet, raw180->final195 actual diagnostic; no default controller/cache scope or native word-winner claim.");
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid first cache accepted.");}
static byte[] Network(int width,ushort kind,int outputs=2)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(0u);w.Write((byte)0x12);w.Write(kind);w.Write(width);w.Write(2);w.Write(0u);w.Write(0u);w.Write(outputs+1);w.Write((ushort)10);
    w.Write(new byte[width]);w.Write(new byte[]{1,1});w.Write(new byte[outputs*2]);w.Write(Enumerable.Repeat((byte)1,outputs+1).ToArray());var bytes=s.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
}
static byte[] OwnSource(byte[] alphabet)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);
    Header(w,0x247,0x10d);w.Write((ushort)0x201);w.Write((ushort)0x100);w.Write(new byte[4+56]);w.Write(0);w.Write(0);w.Write((byte)1);w.Write(0x2bu);w.Write((ushort)0x103);w.Write(alphabet);w.Write(0x135);w.Write(new byte[4]);w.Write(0f);
    foreach(float f in new[]{.96f,.85f,3f,10f})w.Write(f);w.Write(new byte[4]);w.Write(.25f);w.Write((byte)0);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(2);
    int p=(int)s.Position;Header(w,0x24a,0x102);w.Write(0);w.Write(100f);w.Write((byte)1);w.Write(-.2f);w.Write(.2f);w.Write(new byte[]{4,6,0});Finish(s,p);
    p=(int)s.Position;Header(w,0x265,0x102);foreach(float f in new[]{.05f,.075f,.075f,.15f,.35f,.1f,.15f,.0001f})w.Write(f);Finish(s,p);
    w.Write((byte)1);p=(int)s.Position;Header(w,0x297,0x106);Finish(s,p);p=(int)s.Position;Header(w,0x21b,0x10a);Finish(s,p);
    p=(int)s.Position;Header(w,0x21d,0x110);w.Write((ushort)0xffff);w.Write((short)100);w.Write((byte)1);w.Write(Network(30,0x7001,28));w.Write(0f);w.Write(1f);w.Write(1f);w.Write((byte)0);w.Write((byte)3);w.Write((ushort)28);w.Write(.1f);w.Write((byte)1);w.Write((byte)2);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);
    w.Write(new byte[2+4+5*2+7*4]);w.Write(new byte[6]);w.Write(new byte[2+2+4+4+2+2]);w.Write(new byte[2]);
    for(int i=0;i<7;i++){w.Write((byte)0);w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?2:0));if(i>=2)w.Write(0f);w.Write((byte)0);w.Write((byte)(i<2?1:0));if(i<2)w.Write(Network(i==0?247:229,(ushort)(i==0?0x1f:0x2f)));w.Write((byte)0);}
    w.Write((byte)0);w.Write(0f);w.Write(new byte[9]);w.Write((ushort)0);w.Write(0f);w.Write(0f);w.Write(0f);w.Write(.9f);w.Write(.9f);w.Write(0f);w.Write((byte)0);w.Write(new byte[8]);w.Write((byte)0);w.Write((short)0);w.Write(85173);w.Write((ushort)1);w.Write((ushort)10);w.Write(new byte[13]);w.Write(0f);Finish(s,p);w.Write(new byte[190]);Finish(s,0);return s.ToArray();
}
static void Header(BinaryWriter w,uint kind,ushort version){w.Write(0x0101fe90u);w.Write(0u);w.Write(kind);w.Write((ushort)0);w.Write(version);}
static void Finish(MemoryStream s,int p){long save=s.Position;s.Position=p+4;using(var w=new BinaryWriter(s,System.Text.Encoding.UTF8,true))w.Write((uint)(save-p-8));s.Position=save;}
