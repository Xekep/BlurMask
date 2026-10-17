using InkRecognition;
using System.Text;
using System.Text.Json;
if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));using var file=File.Create(args[11]);using var w=new Utf8JsonWriter(file);
w.WriteStartObject();w.WriteStartArray("prepared");int guards=0,values=0,collisions=0;
var retries=new List<(ulong Seed,byte[] Previous,byte[] Actual,ulong State)>();
foreach(var row in controls.RootElement.GetProperty("sequences").EnumerateArray())
{
 ulong seed=row.GetProperty("seed").GetUInt64();var owner=LegacyLibraryUuidGenerator.FromPreparedState(seed);
 w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteStartArray("values");
 foreach(var expected in row.GetProperty("values").EnumerateArray())
 {
  var id=owner.NextIdentifier();byte[] raw=id.CopyRawStorage16();byte[] text=id.CopyUtf8Storage36();
  if(!ReferenceEquals(id.Owner,owner)||!raw.AsSpan().SequenceEqual(expected.GetProperty("raw").GetBytesFromBase64())||
     Encoding.ASCII.GetString(text)!=expected.GetProperty("text").GetString())throw new InvalidDataException("Native model62 sequence/raw-order text differs.");
  WriteIdentifier(id);raw[0]^=255;text[0]^=255;if(id.CopyRawStorage16()[0]==raw[0]||id.CopyUtf8Storage36()[0]==text[0])throw new InvalidDataException("Identifier exposes mutable aliases.");values++;
 }
 w.WriteEndArray();w.WriteNumber("finalState",owner.CurrentStateForControls);w.WriteEndObject();
 if(owner.CurrentStateForControls!=row.GetProperty("finalState").GetUInt64())throw new InvalidDataException("Native64-bit state advance/wrap differs.");
 if(collisions<128)
 {
  var baseline=LegacyLibraryUuidGenerator.FromPreparedState(seed);var first=baseline.NextIdentifier();var second=baseline.NextIdentifier();
  var retry=LegacyLibraryUuidGenerator.FromPreparedState(seed);var actual=retry.NextDistinct(first.CopyRawStorage16());
  if(!actual.CopyRawStorage16().AsSpan().SequenceEqual(second.CopyRawStorage16())||retry.CurrentStateForControls!=baseline.CurrentStateForControls)
   throw new InvalidDataException("Native032550 equality retry did not consume another identifier.");
  retries.Add((seed,first.CopyRawStorage16(),actual.CopyRawStorage16(),retry.CurrentStateForControls));
  collisions++;
 }
}
w.WriteEndArray();
w.WriteStartArray("sameIdRetries");foreach(var r in retries){w.WriteStartObject();w.WriteNumber("seed",r.Seed);w.WriteBase64String("previous",r.Previous);w.WriteBase64String("actual",r.Actual);w.WriteNumber("stateAfter",r.State);w.WriteEndObject();}w.WriteEndArray();
ulong preserved=7;byte[] bad=Enumerable.Repeat((byte)0xa7,15).ToArray();Reject(()=>LegacyModel62Uuid.Generate(ref preserved,bad));if(preserved!=7||bad.Any(b=>b!=0xa7))throw new InvalidDataException("Invalid generation wrote state/destination.");
byte[] raw16=new byte[16],dest=Enumerable.Repeat((byte)0xb5,36).ToArray();Reject(()=>LegacyModel62Uuid.Format(raw16,dest));if(dest.Any(b=>b!=0xb5))throw new InvalidDataException("Invalid format wrote destination.");
byte[] tail=Enumerable.Repeat((byte)0xa6,40).ToArray();LegacyModel62Uuid.Format(Enumerable.Range(0,16).Select(i=>(byte)i).ToArray(),tail);
if(Encoding.ASCII.GetString(tail,0,36)!="00010203-0405-0607-0809-0a0b0c0d0e0f"||tail[36]!=0||tail.Skip(37).Any(b=>b!=0xa6))throw new InvalidDataException("Raw formatter changed byte order/NUL/trailing residue.");
var cancelled=LegacyLibraryUuidGenerator.FromPreparedState(9);Reject(()=>cancelled.NextIdentifier(new(true)));if(cancelled.CurrentStateForControls!=9)throw new InvalidDataException("Cancelled generator advanced.");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
w.WriteStartArray("actualSeeds");
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var actual=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]),raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,actual,norm,circles,new byte[4],new byte[4]);completion.Run();
 var selected=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(actual.SourceDocument)).CreateSelectedCopy();
 using var words=LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selected,table,publication,norm,circles);
 var seed=words.DocumentSeed!;var identifier=seed.LibraryIdentifier38;
 if(seed.StringPool.Count!=0)throw new InvalidDataException("Result identifier was not retained before WORD publication.");
 var expected=LegacyLibraryUuidGenerator.FromPreparedState(identifier.Owner.InitialState).NextDistinct(new byte[16]);
 if(!expected.CopyRawStorage16().AsSpan().SequenceEqual(identifier.CopyRawStorage16()))throw new InvalidDataException("Actual library seed is not feeding the restored algorithm.");
 var published=words.Publish(new byte[40],new byte[2040*4]);
 if(!ReferenceEquals(seed.LibraryIdentifier38,identifier)||!ReferenceEquals(words.DocumentSeed,seed)||published.Alternatives.Count!=2)
  throw new InvalidDataException("WORD publication lost the exact result-owned identifier.");
 w.WriteStartObject();w.WriteNumber("libraryInitialState",identifier.Owner.InitialState);w.WriteNumber("stateAfter",identifier.Owner.CurrentStateForControls);w.WriteNumber("alternatives",published.Alternatives.Count);w.WritePropertyName("identifier");WriteIdentifier(identifier);w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS1024 prepared native-seed sequences/{values} UUIDs/{collisions} forced same-ID retries/3 actual library result owners/{guards} guards; cryptographic library initial-state adaptation explicit.");
void WriteIdentifier(LegacyLibraryTextResultIdentifier id){w.WriteStartObject();w.WriteBase64String("raw",id.CopyRawStorage16());w.WriteString("text",id.ToString());w.WriteEndObject();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Required UUID guard missing.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
