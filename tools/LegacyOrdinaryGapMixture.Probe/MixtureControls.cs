using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
internal static class MixtureControls
{
    public static void Run(string path,LegacyOrdinaryGapProfile profile,LegacyOrdinaryGapMixtureProfile mixture)
    {
        using var json=JsonDocument.Parse(File.ReadAllBytes(path));int fits=0,rejected=0,producers=0,attempts=0;
        foreach(var item in json.RootElement.GetProperty("fits").EnumerateArray())
        {
            float[] p=item.GetProperty("profile").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
            var own=new LegacyOrdinaryGapMixtureProfile(p[0],p[1],p[2],p[3],p[4],p[5]);
            float[] observations=item.GetProperty("observations").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
            var e=item.GetProperty("expected");
            if(e.ValueKind==JsonValueKind.Null){Throws<InvalidDataException>(()=>LegacyNativeOrdinaryGapMixture.Fit(own,observations));rejected++;continue;}
            var result=LegacyNativeOrdinaryGapMixture.Fit(own,observations);Check(result,e);fits++;attempts+=result.Attempts.Count;
            Throws<OperationCanceledException>(()=>LegacyNativeOrdinaryGapMixture.Fit(own,observations,new(true)));
        }
        foreach(var item in json.RootElement.GetProperty("cases").EnumerateArray())
        {
            var ds=Memory(item.GetProperty("descriptors"));var gs=Memory(item.GetProperty("guides"));
            var rows=item.GetProperty("rows").EnumerateArray().Select(v=>new LegacyFirstResultLineRow(Convert.FromBase64String(v.GetString()!),[])).ToArray();
            var result=LegacyNativeFreshGapPreparation.Apply(profile,mixture,ds,gs,rows,-1);var e=item.GetProperty("expected");
            Near(result.Blend,e.GetProperty("blend").GetDouble());Near(result.Output.SourceGap,e.GetProperty("gap").GetDouble());
            Near(result.Output.Mean,e.GetProperty("mean").GetDouble());Near(result.Output.Variance,e.GetProperty("variance").GetDouble());
            for(int i=0;i<result.QualifyingGaps.Count;i++)Near(result.QualifyingGaps[i],e.GetProperty("qualifying")[i].GetDouble());
            for(int i=0;i<result.Output.NormalizedGaps.Count;i++)Near(result.Output.NormalizedGaps[i],e.GetProperty("observations")[i].GetDouble());
            for(int i=0;i<result.Output.SortedRowIndices.Count;i++)if(!result.Output.SortedRowIndices[i].Span.SequenceEqual(e.GetProperty("sorted")[i].EnumerateArray().Select(v=>v.GetInt32()).ToArray()))
                throw new InvalidDataException("Native active29B sort differs.");
            Check(result.Mixture??throw new InvalidDataException("Native active mixture not executed."),e.GetProperty("mixture"));
            Throws<OperationCanceledException>(()=>LegacyNativeFreshGapPreparation.Apply(profile,mixture,ds,gs,rows,-1,new(true)));
            if(producers==0)
            {
                byte[] bad=rows[0].CopyStorage().ToArray();BinaryPrimitives.WriteInt32LittleEndian(bad.AsSpan(16),0);
                var invalid=rows.ToArray();invalid[0]=new(bad,[]);
                Throws<NotSupportedException>(()=>LegacyNativeFreshGapPreparation.Apply(profile,mixture,ds,gs,invalid,-1));
                Throws<ArgumentException>(()=>LegacyNativeOrdinaryGapMixture.Fit(mixture,[float.NaN]));
                Throws<ArgumentException>(()=>LegacyNativeOrdinaryGapMixture.Fit(mixture with{FirstVariance=0},[.5f]));
            }
            producers++;
        }
        Console.WriteLine($"PASS {fits} independent nativeEM/{attempts} retry attempts, {rejected} nonfinite intersection refusals +{producers} active native720 qualifying-gap/blend producers.");
    }
    private static void Check(LegacyOrdinaryGapMixtureResult result,JsonElement e)
    {
        Near(result.Intersection,e.GetProperty("intersection").GetDouble());
        if(result.Attempts.Count!=e.GetProperty("attempts").GetArrayLength())throw new InvalidDataException("Native variance retry count differs.");
        for(int i=0;i<result.Attempts.Count;i++)
        {
            var a=result.Attempts[i];var b=e.GetProperty("attempts")[i];
            if(a.Index!=b.GetProperty("index").GetInt32()||a.Iterations!=b.GetProperty("iterations").GetInt32())throw new InvalidDataException("Exactnative EM loop count differs.");
            string[] names=["firstMean","secondMean","firstVariance","secondVariance","firstPrior","secondPrior","intersection"];
            double[] values=[a.FirstMean,a.SecondMean,a.FirstVariance,a.SecondVariance,a.FirstPrior,a.SecondPrior,a.Intersection];
            for(int k=0;k<names.Length;k++)Near(values[k],Number(b.GetProperty(names[k])));
        }
    }
    private static ReadOnlyMemory<byte>[] Memory(JsonElement v)=>v.EnumerateArray().Select(x=>new ReadOnlyMemory<byte>(Convert.FromBase64String(x.GetString()!))).ToArray();
    private static double Number(JsonElement value)=>value.ValueKind==JsonValueKind.String?value.GetString() switch
    {"NaN"=>double.NaN,"Infinity"=>double.PositiveInfinity,"-Infinity"=>double.NegativeInfinity,_=>throw new InvalidDataException("Unexpected nonfinite trace value.")}:value.GetDouble();
    private static void Near(double a,double b){if(a.Equals(b))return;if(!double.IsFinite(a)||!double.IsFinite(b)||Math.Abs(a-b)>2e-6*Math.Max(1,Math.Abs(b)))throw new InvalidDataException($"Native active29B scalar differs {a} vs {b}.");}
    private static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
}
