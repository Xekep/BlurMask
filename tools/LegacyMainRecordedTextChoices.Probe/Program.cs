using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;

bool reloadModel=args.Length==13 && args[12]=="--reload-model";
if(args.Length!=12 && !reloadModel)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 recorded-manifest capture [--reload-model]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var circlesFile=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(circlesFile);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
byte[] manifestBytes=File.ReadAllBytes(args[10]);using var manifest=JsonDocument.Parse(manifestBytes);
if(manifest.RootElement.GetProperty("commit").GetString()!="c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
    manifest.RootElement.GetProperty("penUpMetadata").GetString()!="absent")throw new InvalidDataException("Pinned unsegmented diagnostic corpus required.");
var samples=manifest.RootElement.GetProperty("samples").EnumerateArray().Where(s=>s.GetProperty("kind").GetString()=="word").ToArray();
if(samples.Length!=9)throw new InvalidDataException("Fixed nine-word diagnostic subset required.");
using var output=File.Create(args[11]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteString("manifestSha256",Convert.ToHexStringLower(SHA256.HashData(manifestBytes)));w.WriteString("inputAssumption","One continuous stroke: source has no pen-up events; expected labels never read by inference.");w.WriteStartArray("samples");
int complete=0,refused=0;
LegacyFreshMainRecognitionContext? shared = reloadModel ? null : LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);
if(shared is not null)
{
    //Exercise owned snapshots; no modified buffer re-enters shared inference.
    Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);Array.Fill(engine,(byte)0);
    Array.Fill(line30,(byte)0);Array.Fill(line50,(byte)0);
}
object? previousModel=null,previousContexts=null,previousDocument=null;
foreach(var sample in samples)
{
    string name=sample.GetProperty("file").GetString()!;
    if(name!=Path.GetFileName(name))throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[10]))!,name));
    string hash=Convert.ToHexStringLower(SHA256.HashData(bytes));
    if(bytes.Length>200000 || hash!=sample.GetProperty("trackSha256").GetString())throw new InvalidDataException("Track hash mismatch.");
    using var track=JsonDocument.Parse(bytes);
    var points=track.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
    if(points.Length is <2 or >4096)throw new InvalidDataException("Bounded original point vector required.");
    w.WriteStartObject();w.WriteString("file",name);w.WriteString("trackSha256",hash);w.WriteNumber("points",points.Length);
    string stage="raw first/main";
    try
    {
        var owner=shared is null
            ? LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,[new InkStroke(points)],0,table,cases,publication)
            : shared.Prepare([new InkStroke(points)],0,table,cases,publication);
        var raw=owner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
        var currentModel=raw.Session.Plan.SourcePolicy.Model;
        var currentContexts=raw.Session.Plan.SourcePolicy.DictionaryOwner.Contexts;
        if(previousModel is not null && (ReferenceEquals(previousModel,currentModel)!=!reloadModel ||
            ReferenceEquals(previousContexts,currentContexts) || ReferenceEquals(previousDocument,owner.SourceDocument)))
            throw new InvalidOperationException("Model reuse or per-invocation mutable owner isolation changed.");
        previousModel=currentModel;previousContexts=currentContexts;previousDocument=owner.SourceDocument;
        stage="first candidate completion";
        var first=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);stage="glyph choice publication";
        using var choices=shared is null
            ? LegacyFreshMainTextGlyphChoices.Prepare(ak,first,table,publication,normalization,circles)
            : shared.PrepareTextChoices(first,table,publication,normalization,circles);
        var words=choices.Publish(new byte[40],new byte[2040*4]);
        w.WriteString("status","complete");w.WriteStartArray("words");
        foreach(var word in words)
        {
            w.WriteStartObject();w.WriteStartArray("glyphs");
            foreach(var glyph in word.Glyphs)
            {
                w.WriteStartObject();w.WriteNumber("reference",glyph.SelectedReference);
                byte[]? text=glyph.CopySelectedUtf8();if(text is null)w.WriteNull("selected");else w.WriteString("selected",Convert.ToHexString(text));
                w.WriteNumber("marker",glyph.Published.AdjustedMarkerIndex);w.WriteStartArray("alternatives");
                foreach(var alternative in glyph.Published.Records)w.WriteStringValue(Convert.ToHexString(alternative.Text.ReadBytes()));w.WriteEndArray();
                w.WriteStartArray("positions");foreach(var range in glyph.SourcePositions.Ranges)
                {w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
            }
            w.WriteEndArray();w.WriteEndObject();
        }
        w.WriteEndArray();complete++;
    }
    catch(Exception error)when(error is NotSupportedException or InvalidDataException or ArgumentException)
    {w.WriteString("status","refused");w.WriteString("stage",stage);w.WriteString("reason",error.Message);refused++;}
    w.WriteEndObject();w.Flush();
}
w.WriteEndArray();w.WriteNumber("complete",complete);w.WriteNumber("refused",refused);w.WriteEndObject();w.Flush();
Console.WriteLine($"Recorded diagnostic: {complete} complete/{refused} explicit refusals; no expected labels read and no accepted OCR claim.");
