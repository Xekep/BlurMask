using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("Prepared feature output path required; no model/NN loads.");
short[] context=Enumerable.Range(0,28).Select(i=>(short)(-1000+37*i)).ToArray();
LegacyInkStroke[] Point(short y,int count=2)=>[new(Enumerable.Repeat(new LegacyInkPoint(7,y),count).ToArray())];
using var full=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);using var bases=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
byte[] bytes=new byte[988],baselineBytes=new byte[18];var controls=new List<object>();int cases=0;
for(int yi=short.MinValue;yi<=short.MaxValue;yi++)
{
 short y=(short)yi,start=(short)((yi&1)==0?-1024:1024),end=(short)((yi&2)==0?-1024:1024);
 if(!LegacyMainPointGlyphFeatures.TryCreate(Point(y),new(0,100),context,start,end,default,out var t,out var r))throw new Exception("Point declined");
 if(t.Length!=256||r.Length!=238||!t.AsSpan(226,28).SequenceEqual(context)||!r.AsSpan(208,28).SequenceEqual(context)||t[^2]!=start||t[^1]!=end||r[^2]!=start||r[^1]!=end)throw new Exception("Context/boundary mismatch");
 int at=0;foreach(short v in t){BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(at),v);at+=2;}foreach(short v in r){BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(at),v);at+=2;}
 full.AppendData(bytes);for(int i=0;i<9;i++)BinaryPrimitives.WriteInt16LittleEndian(baselineBytes.AsSpan(i*2),t[217+i]);bases.AppendData(baselineBytes);cases++;
 if(new[]{-32768,-12801,-12800,-401,-400,-1,0,1,99,100,101,12799,12800,32767}.Contains(yi))controls.Add(new{Y=yi,Start=start,End=end,Trajectory=t,Raster=r});
}
var guards=new List<string>();void Guard(string n,Action f,Type type){try{f();}catch(Exception e){if(e.GetType()==type){guards.Add(n);return;}throw;}throw new Exception("Missing refusal:"+n);}
Guard("generic-before-point",()=>LegacyStrokeFeatures.ExtractInputWithBaseline(Point(0),new(0,100),context,1024,1024),typeof(ArgumentException));
Guard("generic-raster-point",()=>LegacyRasterFeatures.ExtractInputWithBaseline(Point(0),new(0,100),context,1024,1024),typeof(ArgumentException));
Guard("wrong-context",()=>LegacyMainPointGlyphFeatures.TryCreate(Point(0),new(0,100),[],1024,1024,default,out _,out _),typeof(ArgumentException));
Guard("over4096",()=>LegacyMainPointGlyphFeatures.TryCreate(Point(0,4097),new(0,100),context,1024,1024,default,out _,out _),typeof(ArgumentException));
using(var cts=new CancellationTokenSource()){cts.Cancel();try{LegacyMainPointGlyphFeatures.TryCreate(Point(0),new(0,100),context,1024,1024,cts.Token,out _,out _);throw new Exception("Missing cancellation");}catch(OperationCanceledException e){if(e.CancellationToken!=cts.Token)throw;guards.Add("cancel-exact-token");}}
if(LegacyMainPointGlyphFeatures.TryCreate(Point(0),new(1,100),context,1024,1024,default,out _,out _))throw new Exception("Foreign baseline admitted");guards.Add("foreign-baseline-refused");
LegacyInkStroke[] nonpoint=[new([new(0,0),new(1,0)])];if(LegacyMainPointGlyphFeatures.TryCreate(nonpoint,new(0,100),context,1024,1024,default,out _,out _))throw new Exception("Nonpoint admitted");guards.Add("nonpoint-declines");
//Pure directional feature path only: no network classes or models instantiated.
var direction=LegacySaturatedDirectionalFeatures.Extract(Point(-1),new(0,100),LegacyDirectional2B1DefaultCalibration.CopyMinimum(false),LegacyDirectional2B1DefaultCalibration.CopyMaximum(false),LegacyDirectional2B1DefaultCalibration.CopyMinimum(true),LegacyDirectional2B1DefaultCalibration.CopyMaximum(true),context,-1024,1024);
if(direction.Temporal.Length!=313||direction.Spatial.Length!=313)throw new Exception("Existing directional point path failed");
File.WriteAllText(args[0],JsonSerializer.Serialize(new{Scope="PURE features only, no NN/model loading; all shortY prepared domain not actual failed candidate capture",Cases=cases,Context=context,FeatureSha256=Convert.ToHexStringLower(full.GetHashAndReset()),BaselineSha256=Convert.ToHexStringLower(bases.GetHashAndReset()),Controls=controls,Guards=guards,DirectionalTemporal=direction.Temporal,DirectionalSpatial=direction.Spatial},new JsonSerializerOptions{WriteIndented=true}));
