using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;

if(args.Length!=3)throw new ArgumentException("models-directory prepared-activation-profile.json capture.json");
using var profile=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=profile.RootElement.GetProperty("activations");
var hiddenLut=a.GetProperty("hidden").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
var activations=new LegacyInkActivations(hiddenLut,a.GetProperty("output").EnumerateArray().Select(x=>x.GetInt32()).ToArray());
var canceled=new CancellationToken(true);int guards=0;var captures=new List<ModelCapture>();
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new Exception("Expected prepared/owner refusal.");}
LegacyFirstAcoustic119WireModel Read(byte[]data){var p=LegacySource247WireLayout.Read(data);return LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,p,p.Children.First(c=>c.Kind==0x249));}
VectorCapture Run(LegacyPreparedNetwork107Evaluator eval,short[] input,int ordinal)
{
    var raw=eval.EvaluatePreparedRaw(input);var exp=eval.EvaluatePreparedExp(input);var source=eval.EvaluatePreparedSourceUnmasked(input);
    if(!ReferenceEquals(source.Owner,eval)||!ReferenceEquals(eval.Network.Source,eval.Model.Source)||raw.HasClassProbabilities||
        !raw.CopyOutputDots().SequenceEqual(source.CopyOutputDots())||!raw.CopyOutputDots().SequenceEqual(exp.CopyOutputDots()))throw new Exception("Prepared evaluation ownership/dots differ.");
    Guard(()=>raw.CopyProbabilities());
    int[] copy=source.CopyOutputDots();int old=copy[0];copy[0]^=255;if(source.CopyOutputDots()[0]!=old)throw new Exception("Mutable raw result.");
    return new(ordinal,input,raw.CopyHiddenDots(),raw.CopyHiddenValues(),raw.CopyOutputDots(),exp.CopyProbabilities(),exp.StabilizationOffset,
        source.CopyProbabilities(),source.StabilizationOffset,source.SpecialClassIndex);
}
foreach(string name in new[]{"en-US/en_US-ak-cur.lite.res","en-US/en_US-ak-cur.res"})
{
    byte[] data=File.ReadAllBytes(Path.Combine(args[0],name));var model=Read(data);var foreign=Read(data);var nets=new List<NetworkCapture>();
    foreach(var slot in model.Slots.Where(s=>s.Networks.Count==1))
    {
        var net=slot.Networks[0];var eval=LegacyPreparedNetwork107Evaluator.Prepare(model,net,activations);var cases=new List<VectorCapture>();
        if(eval.Quantizer!=10||eval.SourceSpecialClassIndex!=180)throw new Exception("Actual119 quantizer/AL+E differs.");
        Guard(()=>LegacyPreparedNetwork107Evaluator.Prepare(foreign,net,activations));
        Guard(()=>LegacyPreparedNetwork107Evaluator.Prepare(model,slot.Index,activations,canceled));
        Guard(()=>eval.EvaluatePreparedSourceUnmasked(new short[net.InputWidth-1]));Guard(()=>eval.EvaluatePreparedRaw(new short[net.InputWidth],canceled));
        for(int n=0;n<32;n++)
        {
            short[] input=Enumerable.Range(0,net.InputWidth).Select(i=>n switch
            {0=>(short)0,1=>(short)1024,2=>short.MinValue,3=>short.MaxValue,4=>(i&1)==0?short.MinValue:short.MaxValue,
                _=>(short)unchecked((i*7919+n*104729)^(i*n*31337))}).ToArray();
            cases.Add(Run(eval,input,n));
        }
        nets.Add(new(slot.Index,net.Offset,net.End,net.Fingerprint,net.InputWidth,net.HiddenWidth,net.OutputWidth,cases));
    }
    Guard(()=>LegacyPreparedNetwork107Evaluator.Prepare(model,6,activations));Guard(()=>LegacyPreparedNetwork107Evaluator.Prepare(model,-1,activations));
    captures.Add(new(name,Convert.ToHexString(SHA256.HashData(data)),nets));
}
//Explicit prepared resource controls retain real source/network ownership but
//change only named raw data. They are not observed native configured resources.
byte[] original=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.lite.res"));var originalModel=Read(original);var network=originalModel.Slots[0].Networks[0];
int header=originalModel.Source.Fields.Single(f=>f.Owner=="2B"&&f.Destination=="payload").Offset;
var prepared=new List<PreparedCapture>();
foreach(ushort special in new ushort[]{0,180,181,ushort.MaxValue})
{
    byte[] data=original.ToArray();foreach(var m in network.Matrices)data.AsSpan(m.WeightsOffset,m.Rows*m.Stride).Clear();
    BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(header+6),special);var model=Read(data);var eval=LegacyPreparedNetwork107Evaluator.Prepare(model,0,activations);
    var result=Run(eval,new short[eval.Network.InputWidth],0);prepared.Add(new(special,"zeroWeights",result));
    if(result.OutputDots.Any(n=>n!=0)||special==180&&result.SourceProbabilities[180]!=16384||special==ushort.MaxValue&&result.SpecialClassIndex!=-1)throw new Exception("Zero-weight mixed/allEXP control differs.");
}
byte[] malformed=original.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(malformed.AsSpan(header+6),182);var outside=LegacyPreparedNetwork107Evaluator.Prepare(Read(malformed),0,activations);
Guard(()=>outside.EvaluatePreparedSourceUnmasked(new short[network.InputWidth]));outside.EvaluatePreparedExp(new short[network.InputWidth]);
malformed=original.ToArray();malformed[network.Matrices[0].WeightsOffset+network.Matrices[0].Columns]=1;Guard(()=>LegacyPreparedNetwork107Evaluator.Prepare(Read(malformed),0,activations));
malformed=original.ToArray();var first=network.Matrices[0];malformed.AsSpan(first.WeightsOffset,first.Stride).Clear();
malformed.AsSpan(first.WeightsOffset,4).Fill(128);malformed[first.ScalesOffset+1]=128;var minEval=LegacyPreparedNetwork107Evaluator.Prepare(Read(malformed),0,activations);
short[] minimum=new short[network.InputWidth];minimum.AsSpan(0,4).Fill(short.MinValue);Guard(()=>minEval.EvaluatePreparedRaw(minimum));
//Explicit numerical scalar controls around every half-integer probability
//boundary. These do not assert that real feature vectors produce these dots.
var scalarDots=new SortedSet<int>{int.MinValue,int.MaxValue,0};
for(int n=0;n<32768;n++)
{
    double probability=n+.5;double center=524288d*Math.Log(probability/(32768d-probability));int at=(int)Math.Floor(center);
    for(int delta=-2;delta<=2;delta++)scalarDots.Add(at+delta);
}
foreach(int center in new[]{-39321600,39321600})foreach(int delta in new[]{-8,-4,-3,-2,-1,0,1,2,3,4,8})scalarDots.Add(center+delta);
int[] scalar=scalarDots.ToArray();var scalarCapture=new ScalarCapture(scalar,scalar.Select(n=>BitConverter.SingleToInt32Bits(LegacyPreparedNetwork107Evaluator.ComputeSpecialArgument(n))).ToArray(),scalar.Select(LegacyPreparedNetwork107Evaluator.ComputeSpecialProbability).ToArray());
File.WriteAllText(args[2],JsonSerializer.Serialize(new Capture(hiddenLut,captures,prepared,scalarCapture,guards),ProbeJson.Default.Capture)+"\n");
Console.WriteLine($"PASS2 actual119 owners/6networks/192 prepared vectors/4explicit resource controls/{guards} guards; no feature/controller join");
Console.WriteLine($"PASS{scalar.Length} prepared special-dot rounding/threshold scalar controls; portable Math.Exp adaptation");

sealed record VectorCapture(int Ordinal,short[] Input,int[] HiddenDots,short[] HiddenValues,int[] OutputDots,int[] ExpProbabilities,int ExpOffset,int[] SourceProbabilities,int SourceOffset,int SpecialClassIndex);
sealed record NetworkCapture(int Slot,int Offset,int End,string Fingerprint,int InputWidth,int HiddenWidth,int OutputWidth,List<VectorCapture> Vectors);
sealed record ModelCapture(string Path,string Fingerprint,List<NetworkCapture> Networks);
sealed record PreparedCapture(ushort HeaderSpecial,string Mutation,VectorCapture Vector);
sealed record ScalarCapture(int[] Raw,int[] ArgumentBits,int[] Probabilities);
sealed record Capture(int[] PreparedHiddenLut,List<ModelCapture> Models,List<PreparedCapture> PreparedResourceControls,ScalarCapture PreparedSpecialScalarControls,int Guards);
[JsonSourceGenerationOptions(WriteIndented=true)]
[JsonSerializable(typeof(Capture))]
partial class ProbeJson:JsonSerializerContext;
