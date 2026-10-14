using InkRecognition;
using System.Text.Json;
internal static class GapControls
{
    public static void Run(string path,LegacyOrdinaryGapProfile profile)
    {
        using var json=JsonDocument.Parse(File.ReadAllBytes(path));int accepted=0,rejected=0;
        foreach(var item in json.RootElement.EnumerateArray())
        {
            var d=Memory(item.GetProperty("descriptors"));var g=Memory(item.GetProperty("guides"));
            var rows=item.GetProperty("rows").EnumerateArray().Select(v=>new LegacyFirstResultLineRow(Convert.FromBase64String(v.GetString()!),[])).ToArray();
            float incoming=item.GetProperty("incoming").GetSingle();var e=item.GetProperty("expected");
            if(e.ValueKind==JsonValueKind.Null)
            {Throws<NotSupportedException>(()=>LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture(profile,d,g,rows,incoming));rejected++;continue;}
            var result=LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture(profile,d,g,rows,incoming);
            Near(result.SourceGap,e.GetProperty("gap").GetDouble());Near(result.Mean,e.GetProperty("mean").GetDouble());Near(result.Variance,e.GetProperty("variance").GetDouble());
            if(result.Assigned!=e.GetProperty("assigned").GetBoolean()||result.NormalizedGaps.Count!=e.GetProperty("observations").GetArrayLength()||result.SortedRowIndices.Count!=e.GetProperty("sorted").GetArrayLength())
                throw new InvalidDataException("Native29B publication/count differs.");
            for(int i=0;i<result.NormalizedGaps.Count;i++)Near(result.NormalizedGaps[i],e.GetProperty("observations")[i].GetDouble());
            for(int i=0;i<result.SortedRowIndices.Count;i++)if(!result.SortedRowIndices[i].Span.SequenceEqual(e.GetProperty("sorted")[i].EnumerateArray().Select(v=>v.GetInt32()).ToArray()))
                throw new InvalidDataException("Exact175260 tie/order differs.");
            Throws<OperationCanceledException>(()=>LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture(profile,d,g,rows,incoming,new(true)));
            accepted++;
        }
        Console.WriteLine($"PASS {accepted} independent29B mean/variance/sort/frontier controls, {rejected} active-mixture refusals and conditional incoming9C preservation.");
    }
    private static ReadOnlyMemory<byte>[] Memory(JsonElement v)=>v.EnumerateArray().Select(x=>new ReadOnlyMemory<byte>(Convert.FromBase64String(x.GetString()!))).ToArray();
    private static void Near(double a,double b){if(!double.IsFinite(a)||Math.Abs(a-b)>2e-7*Math.Max(1,Math.Abs(b)))throw new InvalidDataException($"Native29B scalar differs {a} vs {b}.");}
    private static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
}
