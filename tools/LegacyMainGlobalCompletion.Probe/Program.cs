using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length is not(11 or 13))throw new ArgumentException("AK engine separation case publication line30 line50 capture LK normalization circles [recorded-manifest word-index]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
var shared=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[5]),File.ReadAllBytes(args[6]));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
using var nf=File.OpenRead(args[9]);using var cpf=File.OpenRead(args[10]);var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cpf);
int guards=0;LegacyRussianModelBundle? model=null;
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
var rawInputs=new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()};

using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("owners");
string? trackHash=null;int? recordedIndex=null;
if(args.Length==13)
{
    using var manifest=JsonDocument.Parse(File.ReadAllBytes(args[11]));
    if(manifest.RootElement.GetProperty("commit").GetString()!="c6629026b1fdef51fd08e3d2fddffd82f97e60a5"||
        manifest.RootElement.GetProperty("penUpMetadata").GetString()!="absent")
        throw new InvalidDataException("Pinned explicitly unsegmented recorded input required.");
    int trackIndex=int.Parse(args[12]);
    var sample=manifest.RootElement.GetProperty("samples").EnumerateArray().Where(x=>x.GetProperty("kind").GetString()=="word").ElementAt(trackIndex);
    string filename=sample.GetProperty("file").GetString()!;
    if(filename!=Path.GetFileName(filename))throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[11]))!,filename));
    trackHash=Convert.ToHexStringLower(SHA256.HashData(bytes));
    if(bytes.Length>200000||trackHash!=sample.GetProperty("trackSha256").GetString())throw new InvalidDataException("Bounded unchanged coordinate track required.");
    using var track=JsonDocument.Parse(bytes);
    var points=track.RootElement.GetProperty("points").EnumerateArray().Select(x=>new InkPoint(x[0].GetSingle(),-x[1].GetSingle())).ToArray();
    if(points.Length is <2 or >4096)throw new InvalidDataException("Bounded actual coordinate vector required.");
    //The recording has no pen-up events. Do not infer them from jumps or read
    //the expected transcription to create a more favorable stroke sequence.
    rawInputs=[new[]{new InkStroke(points)}];recordedIndex=trackIndex;
}
int index=0;
foreach(var raw in rawInputs)
{
 w.WriteStartObject();w.WriteNumber("owner",index++);if(recordedIndex is int ri){w.WriteNumber("recordedIndex",ri);w.WriteString("trackSha256",trackHash);w.WriteString("penUpMetadata","absent");}w.WriteStartArray("variants");
 for(int variant=0;variant<4;variant++)
 {
  var source=Run(raw,variant);var owner=source.Selection.Publication.Policy.Owner;
  model??=owner.Model;if(!ReferenceEquals(model,owner.Model))throw new Exception("Shared model was replaced.");
  var completion=LegacyFreshMainGlobalPublishedCompletion.Prepare(source,normalization,circles);
  Reject(()=>completion.Run(new byte[399],new byte[4],new byte[4]));
  Reject(()=>completion.Run(new byte[400],new byte[4],new byte[4],new(true)));
  Reject(()=>{byte[] overlap=new byte[4];completion.Run(new byte[400],overlap,overlap);});
  if(completion.IsComplete||completion.Snapshot is not null||completion.Import is not null||completion.SourceNames is not null||completion.Words.Count!=0)throw new Exception("Preflight mutated completion.");
  byte[] alternative=Enumerable.Repeat((byte)(variant*43),400).ToArray();
  byte[] leading=Enumerable.Repeat((byte)0x73,4).ToArray(),trailing=Enumerable.Repeat((byte)0x91,4).ToArray();
  var words=completion.Run(alternative,leading,trailing);
  var snapshot=completion.Snapshot!;var import=completion.Import!;
  if(!completion.IsComplete||!completion.QuoteCleanupCompleted||completion.GlyphNormalization is null||completion.GlyphNormalization.ExecutedGlyphRewriteBody||!ReferenceEquals(snapshot.Terminal,source.Selection)||!ReferenceEquals(snapshot.Session,source.Session)||!ReferenceEquals(import.PublishedSource,snapshot)||!ReferenceEquals(completion.Words,words)||!import.Succeeded||words.Count!=import.Words.Count||completion.SourceNames is not {} names||!ReferenceEquals(names.Owner,import)||names.NativeSourceCount!=2)throw new Exception("Actual completion ownership/stage order lost.");
  Reject(()=>completion.Run(alternative,leading,trailing));
  var replay=LegacyFreshMainGlobalPublishedCompletion.Prepare(source,normalization,circles);
  Reject(()=>replay.Run(alternative,leading,trailing));
  if(replay.IsComplete||replay.QuoteCleanupCompleted||replay.Import is not null||replay.SourceNames is not null||replay.Words.Count!=0||replay.Snapshot is null)throw new Exception("New owner bypassed quote claim or lost failure prefix.");
  w.WriteStartObject();w.WriteNumber("variant",variant);w.WriteNumber("nn",source.CachedMainGroupEvaluations);
  w.WriteBase64String("alternative",snapshot.CopyScoredStorage400());
  Nodes("original",source.Session!.Ownership.GlyphPool.CopyOwnerNodes());Nodes("imported",import.ImportedPool.CopyNodes());
  w.WriteStartArray("symbols");for(ushort i=0;i<import.ImportedPool.ControlSymbolCount;i++)w.WriteBase64StringValue(import.ImportedPool.ReadControlSymbolBytes(i));w.WriteEndArray();
  w.WriteStartArray("words");
  foreach(var word in words)
  {
   if(!ReferenceEquals(word.Owner,import)||!ReferenceEquals(word.Normalization,normalization)||!ReferenceEquals(word.Payloads,circles))throw new Exception("WORD completion owner lost.");
   var importedWord=import.Words.Single(v=>v.WordIndex==word.Raw.WordIndex);
   if(!names.ReadWord(importedWord)!.AsSpan().SequenceEqual(System.Text.Encoding.UTF8.GetBytes(owner.Symbols.ResourcePrefix.Identifier)))throw new Exception("Detached source name lost its actual WORD/model owner.");
   w.WriteStartObject();w.WriteNumber("index",word.Raw.WordIndex);w.WriteBase64String("raw",word.Raw.CopyBytes());w.WriteBase64String("completed",word.CopyBytes());
   w.WriteStartArray("glyphs");foreach(var glyph in word.Glyphs){w.WriteStartObject();w.WriteNumber("reference",glyph.Raw.Reference);w.WriteBase64String("record",glyph.Raw.CopyRecord40());w.WriteBase64String("raw",glyph.Raw.CopyBytes());w.WriteBase64String("completed",glyph.CopyBytes());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
   byte[] copy=word.CopyBytes();if(copy.Length>0){copy[0]^=255;if(copy.AsSpan().SequenceEqual(word.CopyBytes()))throw new Exception("Output bytes escaped.");}
  }
  w.WriteEndArray();w.WriteEndObject();
  if(owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)throw new Exception("Completion reevaluated NN.");
 }
 w.WriteEndArray();w.WriteEndObject();w.Flush();
}
w.WriteEndArray();
if(recordedIndex is null)
{
 var longRaw=Enumerable.Range(0,31).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray();var none=Run(longRaw,0);
 if(none.HasWinner||none.Session is not null)throw new Exception("Long fixture did not preserve noWinner.");
 Reject(()=>LegacyFreshMainGlobalPublishedCompletion.Prepare(none,normalization,circles));w.WriteNumber("noWinnerNodes",none.Selection.Publication.NodeCount);
}
else w.WriteNull("noWinnerNodes");
w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{rawInputs.Length*4} actual raw->completed global invocations/{guards}preflight/replay guards; no accepted text.");
LegacyFreshMainGlobalPlainRecognitionResult Run(IReadOnlyList<InkStroke> raw,int variant)
{
 byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*37+variant)).ToArray();foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
 return shared.PrepareGlobal(raw,0,table,cd,pd).Run(Enumerable.Repeat((byte)0xa7,4096*40).ToArray(),residue,new byte[40],new ushort[256],new byte[64],new byte[72]);
}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected completion refusal.");}
void Nodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var node in nodes){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
