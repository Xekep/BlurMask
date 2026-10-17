using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length==2&&args[0]=="--prepared"){Prepared.Run(args[1]);return;}
if(args.Length!=11)throw new ArgumentException("--prepared capture OR AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]));
var samples=new[]{Word(0),Word(0).Concat(Word(230)).ToArray(),Word(0).Concat(Word(300)).ToArray(),Word(0).Concat(Word(450)).ToArray(),Word(0).Concat(Word(700)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Word(0).Select(s=>new InkStroke(s.Points.Concat(Enumerable.Repeat(s.Points[^1],3)).ToArray())).ToArray()};
using var output=File.Create(args[10]);using var w=new Utf8JsonWriter(output);w.WriteStartArray();int pairs=0,refs=0,guards=0;
LegacyNativePlainWordRecord? previousWord=null;LegacyFreshMainGlyphUtf8Emission? previousEmission=null;
foreach(var sample in samples)
{
 var raw=context.Prepare(sample,0,table,cases,publication).Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
 var emitter=LegacyFreshMainGlyphUtf8Emission.Prepare(raw,norm,circles);var comparer=LegacyFreshMainWordEquality.Prepare(raw,emitter);
 var words=new List<LegacyNativePlainWordRecord>();for(var n=raw.Session.Ownership.WordPublisher.Head;n is not null;n=n.Next)words.Add(n.Word);
 if(previousWord is not null)Reject(()=>comparer.Compare(words[0],previousWord,new byte[4],new byte[4]));
 if(previousEmission is not null)Reject(()=>LegacyFreshMainWordEquality.Prepare(raw,previousEmission));
 foreach(var a in words)foreach(var b in words)
 {
  byte[] ls=[0xa5,0x5a,0x80,0xcc],rs=[0x5a,0xa5,0x40,0xdd];var li=ls.ToArray();var ri=rs.ToArray();
  byte[] wa=a.CopyStorage(),wb=b.CopyStorage();bool equal=comparer.Compare(a,b,ls,rs);if(!equal)throw new InvalidDataException("Actual self-WORD comparison unexpectedly unequal.");
  var rows=raw.Session.Ownership.GlyphPool.CopyOwnerNodes().SelectMany(o=>o.Records.ToArray().Chunk(40).Select((r,i)=>(Reference:o.Key|((ulong)i<<40),Row:r))).ToArray();
  w.WriteStartObject();w.WriteBase64String("left",wa);w.WriteBase64String("right",wb);w.WriteNumber("ignored",comparer.IgnoredPrimaryLabel);w.WriteBoolean("map",true);w.WriteNumber("xor",0);w.WriteStartArray("controls");for(int i=0;i<3;i++)w.WriteNumberValue(65535);w.WriteEndArray();
  w.WriteBase64String("leftIn",li);w.WriteBase64String("rightIn",ri);w.WriteBase64String("leftOut",ls);w.WriteBase64String("rightOut",rs);w.WriteBoolean("equal",equal);
  w.WriteStartArray("rows");foreach(var row in rows){w.WriteStartObject();w.WriteNumber("reference",row.Reference);w.WriteBase64String("row",row.Row);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();pairs++;refs+=rows.Length;
  Reject(()=>comparer.Compare(a,b,new byte[3],new byte[4]));Reject(()=>comparer.Compare(a,b,new byte[4],new byte[4],new CancellationToken(true)));
 }
 Reject(()=>LegacyFreshMainWordEquality.Prepare(raw,emitter,new CancellationToken(true)));
 previousWord=words[0];previousEmission=emitter;
}
w.WriteEndArray();w.Flush();Console.WriteLine($"PASS{samples.Length} actual raw sources/{pairs} WORD pairs/{refs} actual pool records/{guards} guards; not TOP.V68.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing owner guard.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
