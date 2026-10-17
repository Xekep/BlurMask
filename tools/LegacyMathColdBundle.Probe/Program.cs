using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("pack raw-resource official-runtime capture");
byte[] pack = File.ReadAllBytes(args[0]), raw = File.ReadAllBytes(args[1]);
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException or EndOfStreamException)
    { guards++; return; }
    throw new Exception("Required cold bundle guard absent.");
}
Math2Mode0ColdBundle Read(byte[] bytes, byte[] resource, CancellationToken token = default)
{ using var stream = new MemoryStream(bytes, writable:false); return Math2Mode0ColdBundle.Read(runtime, stream, resource, token); }
Reject(() => Read(pack, raw, new(true)));
Reject(() => Read(pack, raw[..^1]));
byte[] badRaw = raw.ToArray(); badRaw[^1] ^= 1; Reject(() => Read(pack, badRaw));
Reject(() => Read(pack[..^1], raw));
//Well-formed pack with a self-consistent body hash and an altered ordinary
//label: do not mistake authenticated format for the same source vocabulary.
byte[] badLabel = pack.ToArray();
int ordinary = 48 + 20;
for (int i=0;i<2;i++) ordinary += 4 + BinaryPrimitives.ReadInt32LittleEndian(pack.AsSpan(ordinary));
badLabel[ordinary+4] ^= 1;
SHA256.HashData(badLabel.AsSpan(48)).CopyTo(badLabel.AsSpan(16));
Reject(() => Read(badLabel, raw));
byte[] badGraph = pack.ToArray(); badGraph[^1] ^= 1;
SHA256.HashData(badGraph.AsSpan(48)).CopyTo(badGraph.AsSpan(16));
Reject(() => Read(badGraph, raw));
using var bundle = Read(pack, raw);
if (!bundle.ForceNonobservablePositions) throw new Exception("Lost actual cold metadata force flag.");
using var source = runtime.Tensor(new float[26*3], 3,1,26);
using var downsampling = runtime.Tensor(new long[]{1,1,1}, 3,1);
using var ends = runtime.Tensor(new long[]{2}, 1,1);
using var state = bundle.PrepareState(run,source,downsampling,ends);
using var second = bundle.PrepareState(run,source,downsampling,ends);
if(state.Offset!=0||second.Offset!=0||state.BeamCount!=1||second.BeamCount!=1)throw new Exception("Cold state inherited prior state.");
Reject(bundle.Dispose); Reject(runtime.Dispose); Reject(run.Dispose);
var step = state.RunStep();
byte[] selected = new byte[48]; //Explicit diagnostic selection: nonobservable BOS0, parent0.
state.ApplySelected(step.BindPreparedRows(selected));
var attention = state.CopyInputs().Single(x=>x.Name=="prev_attn_seg");
if(attention.Floats!.Any(x=>BitConverter.SingleToInt32Bits(x)!=unchecked((int)0xff7fffff)))throw new Exception("Actual cold force policy was not applied.");
Reject(()=>step.BindPreparedRows(selected));
if(second.Offset!=0)throw new Exception("Sibling cold state mutated.");
using (var output = File.Create(args[3]))
using (var writer = new Utf8JsonWriter(output))
{
    writer.WriteStartObject(); writer.WriteNumber("Guards",guards);
    writer.WriteBoolean("Force",bundle.ForceNonobservablePositions);
    writer.WriteNumber("Offset",state.Offset); writer.WriteStartArray("AttentionShape");
    foreach(long dimension in attention.Shape)writer.WriteNumberValue(dimension);
    writer.WriteEndArray(); writer.WriteBase64String("Attention",ToBytes(attention.Floats!));
    writer.WriteNumber("SecondOffset",second.Offset); writer.WriteNumber("MaximumPredictionCount",state.MaximumPredictionCount);
    writer.WriteString("VocabularyHash","4366c61258cd74b6520f654898f94b846cf772b5fdcd975b6a95a1cdaf154dce");
    writer.WriteEndObject();
}
Console.WriteLine($"PASS cold model/metadata ownership, actual force-enabled state and {guards} guards; no search or acceptance");
static byte[] ToBytes(float[] values){byte[] b=new byte[values.Length*4];Buffer.BlockCopy(values,0,b,0,b.Length);return b;}
