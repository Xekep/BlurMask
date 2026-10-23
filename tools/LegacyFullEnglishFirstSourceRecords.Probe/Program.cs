using InkRecognition;
using FullFirstSourceRecordsProbe;
using System.Text.Json;

if(args.Length==4&&args[0]=="--prepared"){PreparedControls.Run(args[1],args[2],args[3]);return;}
if(args.Length!=4||args[0] is not("--actual" or "--guards"))throw new ArgumentException("--prepared models controls output OR --actual/--guards models blind-raw output; execution requires review");
bool guards=args[0]=="--guards";
var knowledge=LegacyFullEnglishFirstKnowledge.Load(File.ReadAllBytes(Path.Combine(args[1],"en-US/en_US-ak-cur.res")));
var m30=File.ReadAllBytes(Path.Combine(args[1],"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(args[1],"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[1],"ru-RU/legacy-separation-profile.json")));
using var input=JsonDocument.Parse(File.ReadAllBytes(args[2]));var elements=input.RootElement.ValueKind==JsonValueKind.Array?input.RootElement.EnumerateArray().ToArray():input.RootElement.GetProperty("samples").EnumerateArray().ToArray();
if(guards&&elements.Length>3)throw new ArgumentException("Guard mode is only approved small fixtures, not a corpus NN rerun");
using var stream=File.Create(args[3]);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteBoolean("TargetsRead",false);w.WriteString("Scope","Actual typed full FIRST source720 only; no accepted text/final guide/accuracy");w.WriteBoolean("GuardMode",guards);w.WriteStartArray("Actual");int accepted=0,refused=0;
foreach(var item in elements)
{
    foreach(var property in item.EnumerateObject())if(property.Name is not("Name" or "Raw" or "Resolution" or "id" or "strokes"))throw new ArgumentException("Only blind coordinates/id/resolution accepted");
    string name=item.TryGetProperty("Name",out var n)?n.GetString()!:item.GetProperty("id").GetString()!;
    var rawJson=item.TryGetProperty("Raw",out var r)?r:item.GetProperty("strokes");
    var raw=rawJson.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    if(guards&&raw.Sum(s=>s.Points.Count)>64)throw new ArgumentException("Small64-point guard input only");
    float resolution=item.TryGetProperty("Resolution",out var resolutionJson)?resolutionJson.GetSingle():0;
    w.WriteStartObject();w.WriteString("Name",name);
    LegacyFullEnglishFirstParentInput a;LegacyFullEnglishFirstGlyphEmission[] enriched;LegacyFullEnglishFirstSourceRecords[] sources;
    try
    {
        a=LegacyFullEnglishFirstParentInput.Prepare(knowledge,raw,resolution,m30,m50);var b1=LegacyFullEnglishFirstController.Prepare(a);b1.Run(table);
        var b2=LegacyFullEnglishFirstControllerCosts.Prepare(b1,table);b2.Run();var b3=LegacyFullEnglishFirstPrimary.Prepare(b2);b3.Run();var b4=LegacyFullEnglishFirstContextPasses.Prepare(b3);b4.Run();
        var b5=LegacyFullEnglishFirstTerminal.Prepare(b4);b5.Run();var zero=LegacyFullEnglishFirstPlainText.Prepare(b5);zero.Run();
        byte[] word=Enumerable.Range(0,120).Select(i=>(byte)(i*37+11)).ToArray(),glyph=Enumerable.Range(0,40).Select(i=>(byte)(i*59+23)).ToArray();
        foreach(int at in new[]{32,88,104})word.AsSpan(at,8).Clear();foreach(int at in new[]{96,112})word.AsSpan(at,4).Clear();
        var residue=LegacyFullEnglishFirstPlainText.Prepare(b5);residue.RunPrepared(null,null,word,glyph);
        enriched=[LegacyFullEnglishFirstGlyphEmission.Prepare(zero),LegacyFullEnglishFirstGlyphEmission.Prepare(residue)];foreach(var b7 in enriched)b7.Run();
        sources=[LegacyFullEnglishFirstSourceRecords.Prepare(enriched[0]),LegacyFullEnglishFirstSourceRecords.Prepare(enriched[1])];
    }
    catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidDataException)
    {w.WriteString("Status","PreentryRefused");w.WriteString("ExceptionType",e.GetType().Name);w.WriteEndObject();refused++;continue;}
    var diagnostic=new SourceExports();w.WriteString("Status","Accepted");w.WriteNumber("NetworkBefore",a.NetworkEvaluationCount);w.WriteNumber("ExportPeak",diagnostic.Peak);w.WriteNumber("ExportCopy",diagnostic.Copy);
    string parentBefore=SourceExports.HashParent(a);w.WriteString("ParentBefore",parentBefore);SourceExports.Parent(w,a,sources[0]);w.WriteStartArray("Variants");
    for(int i=0;i<2;i++)
    {
        w.WriteStartObject();w.WriteNumber("Residue",i);string before=SourceExports.HashB7(enriched[i]);w.WriteString("UpstreamBefore",before);SourceExports.Children(w,enriched[i]);
        sources[i].Run();if(!sources[i].IsComplete||sources[i].IsFaulted)throw new InvalidDataException("B8 complete owned source required");SourceExports.Rows(w,sources[i]);
        string after=SourceExports.HashB7(enriched[i]);if(after!=before)throw new InvalidDataException("B8 mutated upstream rich pool/WORDS/ranges");w.WriteString("UpstreamAfter",after);w.WriteEndObject();
    }w.WriteEndArray();
    if(guards){w.WriteStartArray("GuardProofs");w.WriteStringValue(CategorySnapshotControls.Run(enriched[0]));foreach(string proof in LifecycleControls.Run(enriched[0]))w.WriteStringValue(proof);w.WriteEndArray();SourceExports.Prefixes(w,LifecycleControls.Prefixes);SourceExports.CategoryProof(w);}
    string parentAfter=SourceExports.HashParent(a);if(parentBefore!=parentAfter)throw new InvalidDataException("B8 mutated actual current parent/metadata/descriptors");w.WriteString("ParentAfter",parentAfter);
    w.WriteNumber("NetworkAfter",a.NetworkEvaluationCount);w.WriteEndObject();w.Flush();accepted++;
}
w.WriteEndArray();w.WriteNumber("Accepted",accepted);w.WriteNumber("Refused",refused);w.WriteEndObject();Console.WriteLine($"B8 source-only accepted{accepted}/refused{refused}; NO finalguide/acceptedtext/quality claim");
