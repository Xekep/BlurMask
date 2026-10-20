using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;

internal static class PhysicalInputs
{
    internal static Dictionary<InkStroke[],LegacyPhysicalStrokeConstructor[]> Load(string path,
        (InkStroke[][] Samples,string[] Names) recorded)
    {
        using var manifest=JsonDocument.Parse(File.ReadAllBytes(path));
        var files=manifest.RootElement.GetProperty("samples").EnumerateArray()
            .Where(s=>s.GetProperty("kind").GetString()=="word").ToArray();
        if(files.Length!=9||recorded.Samples.Length!=9)throw new InvalidDataException("Fixed9 actual tracks required.");
        var result=new Dictionary<InkStroke[],LegacyPhysicalStrokeConstructor[]>();
        for(int i=0;i<files.Length;i++)
        {
            string name=files[i].GetProperty("file").GetString()!;
            if(name!=recorded.Names[i]||name!=Path.GetFileName(name))throw new InvalidDataException("Actual recorded source identity lost.");
            byte[] raw=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,name));
            if(Convert.ToHexStringLower(SHA256.HashData(raw))!=files[i].GetProperty("trackSha256").GetString())throw new InvalidDataException("Recorded timing/XY hash mismatch.");
            using var track=JsonDocument.Parse(raw);int[] intervals=track.RootElement.GetProperty("intervalsMs").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
            if(intervals.Length!=recorded.Samples[i][0].Points.Count||recorded.Samples[i].Length!=1)
                throw new InvalidDataException("Actual point/time pair count lost; no invented pen-up.");
            int elapsed=0;var times=new int[intervals.Length];
            for(int p=0;p<times.Length;p++){if(intervals[p]<0)throw new InvalidDataException("Negative physical interval.");times[p]=elapsed=checked(elapsed+intervals[p]);}
            long baseTag=checked((long)times[0]*1000);for(int p=times.Length-1;p>=0;p--)times[p]-=times[0];
            //1000Hz is an EXPLICIT host format parameter. Actual measured
            //channel samples control timing; this is not inferred hardware Hz.
            result.Add(recorded.Samples[i],[LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(1000,baseTag,times)]);
        }
        return result;
    }
    internal static LegacyPhysicalStrokeConstructor[] PreparedUniform(InkStroke[] strokes)=>strokes.Length switch
    {
        1=>[LegacyPhysicalStrokeConstructor.Uniform(1000,0)],
        2=>[LegacyPhysicalStrokeConstructor.Uniform(1000,0),LegacyPhysicalStrokeConstructor.Uniform(1000,1_000_000)],
        3=>[LegacyPhysicalStrokeConstructor.Uniform(1000,0),LegacyPhysicalStrokeConstructor.Uniform(1000,1_000_000),LegacyPhysicalStrokeConstructor.Uniform(1000,2_000_000)],
        _=>throw new InvalidDataException("Explicit prepared uniform case absent.")
    };
}
