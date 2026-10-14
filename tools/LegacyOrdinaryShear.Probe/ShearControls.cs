using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

internal static class ShearControls
{
    public static int Run(string path,LegacyOrdinaryShearProfile profile,LegacyNativeCaseAlphabetRanges ranges)
    {
        using var json=JsonDocument.Parse(File.ReadAllBytes(path));var root=json.RootElement;
        for(int i=0;i<ranges.Alphabet.LabelCount;i++)
        {
            bool cased=((ushort)i>=ranges.FirstStart&&(ushort)i<=ranges.FirstEnd)||((ushort)i>=ranges.SecondStart&&(ushort)i<=ranges.SecondEnd);
            bool first=cased&&ranges.Alphabet.TryGetRelatedForms((ushort)i,0,out var forms)&&forms.First==i;
            if(first!=root.GetProperty("firstForms")[i].GetBoolean())throw new InvalidDataException("Independent actual related-form metadata differs.");
        }
        int count=0;
        foreach(var item in root.GetProperty("cases").EnumerateArray())
        {
            var descriptors=Memory(item.GetProperty("descriptors"));var guides=Memory(item.GetProperty("guides"));
            var strokes=item.GetProperty("points").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
            var associations=item.GetProperty("associations").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            var rows=item.GetProperty("rows").EnumerateArray().Select(r=>new LegacyFirstResultLineRow(Convert.FromBase64String(r.GetProperty("storage").GetString()!),
                r.GetProperty("ranges").EnumerateArray().Select(v=>new LegacyFirstResultSourceRange(new(0,v[0].GetInt32(),v[1].GetInt32()),new(0,v[2].GetInt32(),v[3].GetInt32()))).ToArray())).ToArray();
            var result=LegacyNativeOrdinaryShearPreparation.Apply(profile,descriptors,guides,strokes,associations,rows,ranges);
            var expected=item.GetProperty("expected");string[] names=["blend","low","high","gap","direction","shear"];
            float[] values=[result.Blend,result.MinimumSearchShear,result.MaximumSearchShear,result.GapSearchShear,result.DirectionShear,result.Shear];
            for(int i=0;i<values.Length;i++)Near(values[i],expected.GetProperty(names[i]).GetSingle());
            if(result.GridIterations!=expected.GetProperty("iterations").GetInt32())throw new InvalidDataException("Native float grid count differs.");
            var after=result.CopyPhysicalPoints();for(int s=0;s<after.Length;s++)for(int p=0;p<after[s].Length;p++)
            {Near(after[s][p].X,expected.GetProperty("points")[s][p][0].GetSingle());Near(after[s][p].Y,expected.GetProperty("points")[s][p][1].GetSingle());}
            for(int r=0;r<rows.Length;r++)
            {
                var b=result.Rows[r].CopyStorage().Span;for(int c=0;c<4;c++)Near(BinaryPrimitives.ReadSingleLittleEndian(b[(24+c*4)..]),expected.GetProperty("boxes")[r][c].GetSingle());
                var original=rows[r].CopyStorage().Span;
                for(int k=0;k<720;k++)if((k<24||k>=40)&&b[k]!=original[k])throw new InvalidDataException("24D modified non-geometric source720 bytes.");
            }
            Throws<OperationCanceledException>(()=>LegacyNativeOrdinaryShearPreparation.Apply(profile,descriptors,guides,strokes,associations,rows,ranges,new(true)));
            if(count==0)
            {
                Throws<ArgumentException>(()=>LegacyNativeOrdinaryShearPreparation.Apply(profile,descriptors,guides,strokes,[],rows,ranges));
                byte[] broken=descriptors[0].ToArray();BinaryPrimitives.WriteInt32LittleEndian(broken.AsSpan(16),-1);
                Throws<ArgumentException>(()=>LegacyNativeOrdinaryShearPreparation.Apply(profile,[broken],guides,strokes,associations,rows,ranges));
                Throws<ArgumentException>(()=>LegacyNativeOrdinaryShearPreparation.Apply(profile with{SearchStep=0},descriptors,guides,strokes,associations,rows,ranges));
            }
            count++;
        }
        Console.WriteLine($"PASS {count} independent24D float controls: genuine AL related forms, nonzero grid/directions,720 ranges, unchanged metadata and cancellation.");return count;
    }
    private static ReadOnlyMemory<byte>[] Memory(JsonElement values)=>values.EnumerateArray().Select(v=>new ReadOnlyMemory<byte>(Convert.FromBase64String(v.GetString()!))).ToArray();
    private static void Near(float a,float b){if(!float.IsFinite(a)||Math.Abs(a-b)>4e-6f*Math.Max(1,Math.Abs(b)))throw new InvalidDataException($"Native24D scalar differs: {a} vs {b}.");}
    private static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
}
