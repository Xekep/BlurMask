using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;
using InkRecognition.Math;
if(args.Length!=5)throw new ArgumentException("pack rawMath2 officialORT manifest output");
CorpusChecks.Run(args[0],args[2],args[3],args[4]+".baseline.json");
using var pack=File.OpenRead(args[0]);var model=MathModel.Read(pack);
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));
using var models=new Math2Mode0TensorModels(runtime,model);
var gate=LegacyMath2ObservableTokens.BindBundledResource(File.ReadAllBytes(args[1]));
using var manifest=JsonDocument.Parse(File.ReadAllBytes(args[3]));
string directory=Path.GetDirectoryName(Path.GetFullPath(args[3]))!;
using var file=File.Create(args[4]);using var w=new Utf8JsonWriter(file,new(){Indented=true});
w.WriteStartObject();w.WriteString("policy","Diagnostic argmax control only, same128 cap with force off/on; not native search, acceptance or a proposed application route.");
w.WriteString("manifestSHA256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[3]))));
w.WriteStartArray("samples");
foreach(var sample in manifest.RootElement.GetProperty("samples").EnumerateArray())
{
 string name=sample.GetProperty("strokes").GetString()!;
 if(Path.GetFileName(name)!=name)throw new Exception("Invalid member name.");
 using var ink=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory,name)));
 var strokes=ink.RootElement.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(v=>new InkPoint(v[0].GetSingle(),v[1].GetSingle())).ToArray())).ToArray();
 var features=MathPreprocessor.Process(strokes);
 w.WriteStartObject();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteNumber("frames",features.Frames);w.WriteStartArray("controls");
 foreach(bool force in new[]{false,true})
 {
  var first=Decode(force);var repeat=Decode(force);
  if(first.Completed!=repeat.Completed||!first.Symbols.AsSpan().SequenceEqual(repeat.Symbols))throw new Exception("Diagnostic state run is not deterministic.");
  w.WriteStartObject();w.WriteBoolean("force",force);w.WriteBoolean("completed",first.Completed);w.WriteStartArray("symbols");foreach(var id in first.Symbols)w.WriteNumberValue(id);w.WriteEndArray();
  string[] labels=first.Symbols.Select(id=>model.Labels[id]).ToArray();w.WriteString("rawText",string.Concat(labels));w.WriteString("latex",first.Completed?MathLatexFormatter.Format(labels):null);w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();Console.WriteLine("Audited diagnostic force controls "+sample.GetProperty("id").GetString());
 (bool Completed,ushort[] Symbols) Decode(bool force)
 {
  using var run=runtime.CreateRun(CancellationToken.None);
  using var src=runtime.Tensor(features.Values,features.Frames,1,26);
  using var down=runtime.Tensor(Enumerable.Repeat(1L,features.Frames).ToArray(),features.Frames,1);
  using var ends=runtime.Tensor(features.StrokeEnds,1,features.StrokeEnds.Length);
  using var encoder=models.RunEncoder(run,src,down,ends);
  using var state=Math2Mode0TensorState.Prepare(models,encoder,new(gate,force));
  var ids=new List<ushort>();
  for(int i=0;i<128;i++)
  {
   var step=state.RunStep();var logits=step.CopyOutput(0).Values;int chosen=0;
   for(int j=1;j<logits.Length;j++)if(logits[j]>logits[chosen])chosen=j;
   if(chosen==1)return(true,ids.ToArray());if(chosen==0)return(false,ids.ToArray());
   ids.Add((ushort)chosen);byte[] row=new byte[48];BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8),(ulong)chosen);
   state.ApplySelected(step.BindPreparedRows(row));
  }
  return(false,ids.ToArray());
 }
}
w.WriteEndArray();w.WriteEndObject();
