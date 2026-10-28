using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InkRecognition;
using InkRecognition.Math;
using InkMath.Solver;
var giacPath=Environment.GetEnvironmentVariable("BLURMASK_TEST_GIAC");
var giac=string.IsNullOrEmpty(giacPath)?null:new SymbolicMathSolver(Path.GetFullPath(giacPath));
if(args.Length<5)throw new ArgumentException("pack rawMath2 officialORT output.json manifest.json [...]");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
using var file=File.Create(args[3]);using var w=new Utf8JsonWriter(file,new(){Indented=true});
w.WriteStartObject();w.WriteString("scope","Actual raw NN cold1 and diagnostic prepared3/16 only; captures failures without candidate fallback. Expected labels are read after inference. Not formula accuracy, solving, application acceptance or native SDK execution.");
w.WriteString("resourceSHA256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))));w.WriteStartArray("samples");
foreach(string manifestPath in args.Skip(4))
{
 using var manifest=JsonDocument.Parse(File.ReadAllBytes(manifestPath));string directory=Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
 foreach(var sample in manifest.RootElement.GetProperty("samples").EnumerateArray())
 {
  string name=sample.GetProperty("strokes").GetString()!;if(name!=Path.GetFileName(name))throw new InvalidDataException("Filename-only corpus member required.");
  byte[] raw=File.ReadAllBytes(Path.Combine(directory,name));if(raw.Length>8_000_000)throw new InvalidDataException("Bounded raw ink required.");
  using var ink=JsonDocument.Parse(raw);int points=0;
  var strokes=ink.RootElement.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>{points++;return new InkPoint(p[0].GetSingle(),p[1].GetSingle());}).ToArray())).ToArray();
  if(points>65536||strokes.Length>4096)throw new InvalidDataException("Bounded raw coordinates required.");
  w.WriteStartObject();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteString("manifestSHA256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(manifestPath))));
  w.WriteString("inkSHA256",Convert.ToHexStringLower(SHA256.HashData(raw)));w.WritePropertyName("ink");w.WriteRawValue(raw);w.WriteStartArray("runs");
  foreach(int width in new[]{1,3,16})
  {
   w.WriteStartObject();w.WriteNumber("width",width);w.WriteString("policy",width==1?"ColdDefault":"PreparedSearchDiagnostic");string stage="features";
   Math2Mode0RawOnlySource? source=null;Math2Mode0OriginalMathFieldSource? field=null;Math2Mode0SelectedMathFieldCopy? receipt=null;OrtRun? run=null;
   try
   {
    var features=Math2Mode0RawSourcePositions.PrepareFeatures(strokes,default);
    w.WriteNumber("frames",features.Features.Frames);w.WriteStartArray("featureBits");foreach(float v in features.Features.Values)w.WriteNumberValue(BitConverter.SingleToInt32Bits(v));w.WriteEndArray();
    var view=LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document);field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
    run=runtime.CreateRun(default);stage="NN initial order";
    source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSelectedMathField(runtime,bundle,run,field,width);
    source.RunToInitialOrder();w.WriteStartArray("rawFrameHashes");foreach(string hash in source.Loop.CopyRawAttentionStatistics().RawFrameHashes)w.WriteStringValue(hash);w.WriteEndArray();
    stage="EF4";source.ApplyEf4();stage="final order";var final=source.OrderFinal();stage="full EE4 formatter";var output=final.FormatFullCold();
    w.WriteStartArray("formattedRecords");foreach(var row in output.CopyRecords())
    {
     w.WriteStartObject();w.WriteNumber("scoreBits",BitConverter.SingleToInt32Bits(row.Score));w.WriteStartArray("units");foreach(uint unit in row.Units)w.WriteNumberValue(unit);w.WriteEndArray();w.WriteStartArray("utf8");foreach(var unit in row.UnitRows)w.WriteStringValue(Convert.ToBase64String(unit.Text.ReadBytes()));w.WriteEndArray();
     var tokens=row.UnitRows.Select(u=>Encoding.UTF8.GetString(u.Text.ReadBytes()).TrimEnd('\0')).ToArray();string? latex=MathLatexFormatter.Format(tokens);
     w.WriteString("diagnosticLatex",latex);bool requested=latex?.TrimEnd().EndsWith('=')==true;w.WriteBoolean("diagnosticTrailingEqualsRequested",requested);
     // Named library adapter diagnostic only, not the original native evaluator or app dispatch.
     string? expression=latex is null?null:requested?latex.TrimEnd()[..^1].TrimEnd():latex;
     string? answer=requested&&expression is not null&&giac is not null?giac.SolveAsync(expression).GetAwaiter().GetResult()?.LaTex:null;
     w.WriteString("diagnosticGiac",answer);w.WriteString("diagnosticEqualsGatedResult",answer);w.WriteEndObject();
    }w.WriteEndArray();stage="source attachment";var attached=output.AttachSourcePositions();stage="field copy";receipt=attached.CopySelectedMathField(field);
    stage="fresh publisher";using var publication=Math2Mode0FreshMathPublisher.Create(receipt);var published=publication.CopySnapshot();
    w.WriteStartArray("publishedNames");foreach(var node in published.Temporary)w.WriteStringValue(node.Name);w.WriteEndArray();w.WriteBoolean("publisherComplete",true);stage="completed bounded publisher";
   }
   catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException)
   {w.WriteString("refusalType",e.GetType().Name);w.WriteString("refusal",e.Message);}
   finally{receipt?.Dispose();field?.Dispose();source?.Dispose();run?.Dispose();}
   w.WriteString("lastStage",stage);w.WriteEndObject();
  }
  w.WriteEndArray(); // Independent labels are accessed only after every NN run.
  w.WriteString("expectedPostInference",sample.GetProperty("expected").GetString());
  if(sample.TryGetProperty("creationMethod",out var method))w.WriteString("creationMethod",method.GetString());
  w.WriteEndObject();w.Flush();Console.WriteLine("Captured actual raw "+sample.GetProperty("id").GetString());
 }
}
w.WriteEndArray();w.WriteEndObject();
