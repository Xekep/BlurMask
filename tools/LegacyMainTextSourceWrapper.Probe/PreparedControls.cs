using InkRecognition;
using System.Text.Json;

internal static class PreparedControls
{
    internal static void Run(string path)
    {
        using var json=JsonDocument.Parse(File.ReadAllBytes(path));int cases=0,outputs=0,guards=0;
        foreach(var item in json.RootElement.GetProperty("cases").EnumerateArray())
        {
            var counts=item.GetProperty("counts").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
            var strokes=counts.Select(n=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(i,0)).ToArray())).ToArray();
            var document=LegacyOwnedRawInkDocument.Create(strokes,default);
            var operations=LegacyOwnedRawInkRangeOperations.Prepare(document);
            var original=Ranges(item.GetProperty("original"));
            var ranges=item.GetProperty("ranges").EnumerateArray().Select(Ranges).ToArray();
            var skipped=Bools(item,"skip");var positive=Bools(item,"positive");var joiner=Bools(item,"joiner");
            var expected=item.GetProperty("expected").EnumerateArray().ToArray();int next=0;
            for(int i=0;i<ranges.Length;i++)
            {
                if(skipped[i])continue;
                (bool Ordinary,LegacyNativeInkPositionRange[] Key) result;
                try{result=LegacyPreparedMainTextSourceProcessing.Process(operations,original,ranges,skipped,positive,joiner,i,default);}
                catch(Exception error){throw new InvalidDataException($"Prepared case{cases}/source{i} failed.",error);}
                ranges[i]=result.Key;var e=expected[next++];
                if(e.GetProperty("index").GetInt32()!=i||e.GetProperty("positive").GetBoolean()!=result.Ordinary||
                    !result.Key.SequenceEqual(Ranges(e.GetProperty("key"))))throw new InvalidDataException($"Prepared source lifecycle mismatch{cases}/{i}.");
                outputs++;
            }
            if(next!=expected.Length)throw new InvalidDataException("Prepared output cardinality changed.");
            var after=item.GetProperty("after").EnumerateArray().Select(Ranges).ToArray();
            if(ranges.Where((r,i)=>!r.SequenceEqual(after[i])).Any())throw new InvalidDataException("Later-source mutation differs.");
            cases++;
        }
        //TEST clears CF; JBE after TEST is exactly ZF. The native skip is
        //kind1 AND masked run NONZERO AND low bits3, not zero-length NBSP.
        for(uint run=0;run<=ushort.MaxValue;run++)for(ushort kind=0;kind<8;kind++)
        {
            bool zero=(run&0x7ffc)==0;
            bool native=kind==1&&!zero&&(run&3)==3;
            if(LegacyFreshMainTextGlyphChoices.IsWrapperSkipped(new(kind,(ushort)run))!=native)
                throw new InvalidDataException("Native kind1 skip predicate changed.");
            guards++;
        }
        Console.WriteLine($"PASS{cases} prepared independent source-lifecycle cases/{outputs} emitted keys/{guards} completeushort skip predicate controls.");
    }
    private static bool[] Bools(JsonElement item,string name)=>item.GetProperty(name).EnumerateArray().Select(x=>x.GetBoolean()).ToArray();
    private static LegacyNativeInkPositionRange[] Ranges(JsonElement value)=>value.EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();
}
