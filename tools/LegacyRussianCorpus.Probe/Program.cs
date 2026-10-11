using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if (args.Length != 8) throw new ArgumentException("Usage: acoustic.res lexical.res engine-profile.json separation-profile.json manifest.json report.json baselineYUp height. Private unsegmented-track diagnostic, not accuracy benchmark.");
float baselineYUp = float.Parse(args[6], CultureInfo.InvariantCulture);
float height = float.Parse(args[7], CultureInfo.InvariantCulture);
if (!float.IsFinite(baselineYUp) || !float.IsFinite(height) || height <= 0) throw new ArgumentException("Explicit finite line frame required.");
var timer = Stopwatch.StartNew();
byte[] acoustic = File.ReadAllBytes(args[0]);
var bundle = LegacyRussianModelBundle.Load(acoustic, File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]));
var classifier = bundle.CreatePreparedGlyphClassifier();
var cache = new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)), bundle.AcousticAlphabet, acoustic.AsSpan(99,16));
var bridge = new LegacyCursiveFinalCacheGroupScorer(cache,
    LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734), out _), bundle.AcousticAlphabet),
    bundle.CreateCursiveCategoryMapper(1), LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),
    LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3])), bundle.AcousticAlphabet, bundle.LexicalAlphabet,
    Enumerable.Repeat((byte)1,bundle.AcousticAlphabet.LabelCount).ToArray(), [], relatedFormMode:0);
var scope = new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
var recognizer = new LegacyRussianCursiveWordRecognizer(bundle, relatedFormMode:0);
double modelLoadMs = timer.Elapsed.TotalMilliseconds;
using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[4]));
if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
    manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent") throw new InvalidDataException("Expected pinned, explicitly unsegmented corpus.");
var samples = manifest.RootElement.GetProperty("samples");
if (samples.GetArrayLength() > 100) throw new InvalidDataException("Bounded corpus required.");
string directory = Path.GetDirectoryName(Path.GetFullPath(args[4]))!;
var reports = new List<SampleReport>();
foreach (var sample in samples.EnumerateArray())
{
    string file = sample.GetProperty("file").GetString()!;
    if (file != Path.GetFileName(file)) throw new InvalidDataException("Track must be a direct sibling.");
    byte[] bytes = File.ReadAllBytes(Path.Combine(directory,file));
    if (bytes.Length > 200_000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != sample.GetProperty("trackSha256").GetString()) throw new InvalidDataException("Track integrity/size mismatch.");
    using var record = JsonDocument.Parse(bytes);
    if (record.RootElement.GetProperty("coordinateFrame").GetString() != "pyglet-y-up" || record.RootElement.GetProperty("penUpMetadata").GetString() != "absent") throw new InvalidDataException("Unexpected coordinate semantics.");
    var points = record.RootElement.GetProperty("points").EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
    var intervals = record.RootElement.GetProperty("intervalsMs");
    if (points.Length is < 2 or > 4096 || intervals.GetArrayLength() != points.Length || points.Any(p => !float.IsFinite(p.X) || !float.IsFinite(p.Y))) throw new InvalidDataException("Invalid recorded coordinates/timing.");
    // Absence of pen-up metadata is never replaced by inferred physical boundaries.
    // The recognizer receives one artificial unsegmented path with explicit scope.
    InkStroke[] ink = [new(points)];
    string kind = sample.GetProperty("kind").GetString()!;
    var output = new SampleReport { Id=sample.GetProperty("id").GetString()!, Kind=kind, PointCount=points.Length };
    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    timer.Restart();
    try
    {
        if (kind == "character")
        {
            var prepared = LegacyCursivePreparedInk.Create(ink, cancellation.Token);
            var glyph = classifier.Classify(prepared,[new LegacyInkRange(0,0,points.Length)], points.Min(p=>p.X), -baselineYUp,height,100,10,[],1024,1024,true,[1,1,1,1,0,0,0],1,cancellationToken:cancellation.Token);
            var final = cache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,Enumerable.Repeat((byte)1,bundle.AcousticAlphabet.LabelCount).ToArray(),scope,[],cancellation.Token);
            output.Ranked = final.Costs.ToArray().Take(bundle.AcousticAlphabet.LabelCount).Select((cost,index)=>new Rank { Text=bundle.AcousticAlphabet.TryGetText((ushort)index,out string text)?text:"", Cost=cost }).OrderBy(r=>r.Cost).ThenBy(r=>r.Text,StringComparer.Ordinal).ToArray();
            output.Status="scored-final195-whole-track";
        }
        else if (kind == "word")
        {
            var result = recognizer.RecognizeSourceOrderWithFinalCache(ink,
                new LegacyCursiveWordSearchOptions(-baselineYUp,height,0,1,BeamWidth:64,MaximumScoredGroups:512,MaximumExpandedChoices:2_000_000),
                bridge,(_,_)=>scope,false,false,false,cancellationToken:cancellation.Token);
            output.Ranked=result.Words.Select(w=>new Rank { Text=w.Text,Cost=(int)w.Score.TotalCost }).ToArray();
            output.Groups=result.ClassifiedGroups; output.ExpandedChoices=result.ExpandedChoices;
            output.Status=output.Ranked.Length==0?"no-dictionary-candidate":"scored-final195-variable-beam";
        }
        else throw new InvalidDataException("Unknown corpus kind.");
    }
    catch(OperationCanceledException) { output.Status="declined-time-budget"; }
    catch(Exception error) when(error is ArgumentException or InvalidDataException or NotSupportedException) { output.Status="declined"; output.Error=error.Message; }
    output.ElapsedMs=timer.Elapsed.TotalMilliseconds;
    // Expected transcription becomes visible only after inference completes.
    output.Expected=sample.GetProperty("expected").GetString()!;
    Rank? expected=output.Ranked.FirstOrDefault(r=>r.Text==output.Expected);
    if(expected is not null)
    {
        output.ExpectedRankBest=1+output.Ranked.Count(r=>r.Cost<expected.Cost);
        output.ExpectedRankWorst=output.Ranked.Count(r=>r.Cost<=expected.Cost);
    }
    output.TopExact=output.Ranked.Length>0&&output.Ranked[0].Text==output.Expected;
    output.Ranked=output.Ranked.Take(5).ToArray();
    reports.Add(output);
    Console.WriteLine($"{output.Id}: {output.Status}; expected={output.Expected}; top={string.Join(',',output.Ranked.Select(r=>$"{r.Text}:{r.Cost}"))}; {output.ElapsedMs:F1}ms");
}
var report = new CorpusReport { ModelLoadMs=modelLoadMs, BaselineYUpHypothesis=baselineYUp,HeightHypothesis=height,Samples=reports.ToArray() };
File.WriteAllText(args[5],JsonSerializer.Serialize(report,ReportJsonContext.Default.CorpusReport));
Console.WriteLine($"Diagnostic exact-top comparisons: {reports.Count(r=>r.TopExact)}/{reports.Count}; no pen-up or native line-frame accuracy claim.");

internal sealed class CorpusReport
{
    public string Scope { get; set; }="Unsegmented real coordinate tracks; missing pen-up, explicit unverified guide-frame hypothesis; vendor training overlap unknown; costs are not calibrated confidence.";
    public double ModelLoadMs { get; set; }
    public float BaselineYUpHypothesis { get; set; }
    public float HeightHypothesis { get; set; }
    public SampleReport[] Samples { get; set; }=[];
}
internal sealed class SampleReport
{
    public string Id { get; set; }=""; public string Kind { get; set; }=""; public string Expected { get; set; }="";
    public string Status { get; set; }=""; public string? Error { get; set; }
    public int PointCount { get; set; } public double ElapsedMs { get; set; }
    public int? ExpectedRankBest { get; set; } public int? ExpectedRankWorst { get; set; }
    public bool TopExact { get; set; } public int Groups { get; set; } public long ExpandedChoices { get; set; }
    public Rank[] Ranked { get; set; }=[];
}
internal sealed class Rank { public string Text { get; set; }=""; public int Cost { get; set; } }
[JsonSerializable(typeof(CorpusReport))]
[JsonSourceGenerationOptions(WriteIndented=true)]
internal partial class ReportJsonContext : JsonSerializerContext { }
