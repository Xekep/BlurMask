using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
int offset=LegacyFreshMainGeometricLineFallback.ReadMainProcessor(ak);if(offset!=187976)throw new Exception("Wrong main24A owner.");
var profile=LegacyAutomaticLineProfile.ReadResourceNode(ak,offset);int guards=0,created=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteNumber("processor",offset);
w.WriteString("node",Convert.ToHexString(ak.AsSpan(offset,36)));w.WriteStartArray("prepared");
for(int sample=0;sample<128;sample++)
{
    var points=new[]{new InkPoint[]{new(-40,10),new(-20,30)},Shape(sample),Shape(sample+128)};
    var descriptors=new[]{Descriptor(0,0,0),Descriptor(-1,1,1),Descriptor(-1,0,0,3),Descriptor(-1,2,2)};
    byte[] prefix=Enumerable.Range(0,40).Select(i=>(byte)(i+17)).ToArray();byte[][] guides=[prefix];
    var before=points.Select(s=>s.ToArray()).ToArray();var originalDescriptors=descriptors.Select(d=>d.ToArray()).ToArray();
    var output=LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,descriptors,guides,points);
    if(output.Created.Length!=2||!output.Guides[0].AsSpan().SequenceEqual(prefix)||
        !output.Descriptors[0].AsSpan().SequenceEqual(descriptors[0])||!output.Descriptors[2].AsSpan().SequenceEqual(descriptors[2])||
        !output.Points[0].AsSpan().SequenceEqual(points[0])||
        !points.SelectMany(s=>s).SequenceEqual(before.SelectMany(s=>s))||!Same(descriptors,originalDescriptors))
        throw new Exception("Prepared fallback changed an assigned descriptor, separator, prefix or caller input.");
    created+=output.Created.Length;w.WriteStartObject();WritePoints("inputPoints",points);WriteBytes("inputDescriptors",descriptors);WriteBytes("inputGuides",guides);
    WritePoints("points",output.Points);WriteBytes("descriptors",output.Descriptors);WriteBytes("guides",output.Guides);w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("actual");
var samples=new[]{Word(0,0),Word(0,250).Concat(Word(0,0)).ToArray(),Word(0,0).Concat(Word(230,0)).ToArray(),Word(0,0).Concat(Word(300,0)).ToArray(),Word(0,0).Concat(Word(450,0)).ToArray(),Word(0,0).Concat(Word(700,0)).ToArray(),Hi(0).Concat(Hi(220)).ToArray()};
foreach(var raw in samples)
{
    var owner=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd);var actual=owner.Run();
    var fallback=LegacyFreshMainGeometricLineFallback.Apply(ak,actual.Lines);
    if(fallback.CreatedLines.Count!=0||!ReferenceEquals(fallback.Lines,actual.Lines))throw new Exception("Assigned actual main guide was refitted.");
    Reject(()=>LegacyFreshMainGeometricLineFallback.Apply(ak,actual.Lines,new(true)));
    byte[] foreign=ak.ToArray();foreign[^1]^=1;Reject(()=>LegacyFreshMainGeometricLineFallback.Apply(foreign,actual.Lines));
    w.WriteStartObject();w.WriteNumber("source720",actual.Lines.PublishedRows.Count);w.WriteNumber("guides",actual.Lines.CopyGuides().Length);
    w.WriteNumber("words",actual.FirstResults.Sum(r=>r.Text.Words.Count(x=>!x.IsDelimiter)));w.WriteNumber("created",fallback.CreatedLines.Count);w.WriteEndObject();
}
w.WriteEndArray();
Reject(()=>LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,[Descriptor(-1,0,1)],[],[[new(0,0)]]));
Reject(()=>LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,[Descriptor(-1,0,0,6)],[],[[new(0,0)]]));
Reject(()=>LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,[Descriptor(-1,0,0)],[],[[new(float.NaN,0)]]));
Reject(()=>LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,[Descriptor(-1,0,0)],[],[[new(0,0)]],new(true)));
var overlapping=Enumerable.Range(0,257).Select(_=>Descriptor(-1,0,0)).ToArray();
Reject(()=>LegacyFreshMainGeometricLineFallback.ApplyPreparedDescriptors(profile,overlapping,[],[Enumerable.Range(0,4096).Select(i=>new InkPoint(i,10)).ToArray()]));
w.WriteNumber("guards",guards);w.WriteNumber("preparedGuides",created);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS128 prepared descriptor states/{created} new main24A guides;7 actual firstTEXT owners retain all assigned guides;{guards} guards. No actual empty-NN fixture claim.");
void Reject(Action f){try{f();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid fallback scope accepted.");}
void WriteBytes(string name,byte[][] values){w.WriteStartArray(name);foreach(var v in values)w.WriteBase64StringValue(v);w.WriteEndArray();}
void WritePoints(string name,InkPoint[][] points){w.WriteStartArray(name);foreach(var s in points){w.WriteStartArray();foreach(var p in s){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
static bool Same(byte[][] a,byte[][] b)=>a.Length==b.Length&&a.Where((r,i)=>!r.AsSpan().SequenceEqual(b[i])).Any()==false;
static byte[] Descriptor(int line,int first,int last,int kind=0){var d=new byte[88];BinaryPrimitives.WriteUInt16LittleEndian(d,(ushort)kind);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(16),line);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(36),first);BinaryPrimitives.WriteInt32LittleEndian(d.AsSpan(40),last);return d;}
static InkPoint[] Shape(int index){int count=2+index%49;return Enumerable.Range(0,count).Select(i=>new InkPoint(i*2.5f+(index%5)*100,(i%2==0?0:20)+(index%7-3)*i*.3f+index)).ToArray();}
static InkStroke[] Word(float x,float y)=>[
 new(new InkPoint[]{new(x,y+90),new(x,y+90),new(x,y+10),new(x+25,y+60),new(x+50,y+10),new(x+50,y+90)}),
 new(new InkPoint[]{new(x+70,y+10),new(x+70,y+90),new(x+120,y+10),new(x+120,y+90)}),
 new(new InkPoint[]{new(x+140,y+90),new(x+140,y+10),new(x+180,y+10),new(x+190,y+20),new(x+190,y+40),new(x+180,y+50),new(x+140,y+50)})];
static InkStroke[] Hi(float x)=>[
 new(new InkPoint[]{new(x,10),new(x,90)}),new(new InkPoint[]{new(x+40,10),new(x+40,90)}),new(new InkPoint[]{new(x,50),new(x+40,50)}),new(new InkPoint[]{new(x+70,10),new(x+70,90)})];
