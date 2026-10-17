using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;

internal static class RecordedInputs
{
    internal static (InkStroke[][] Samples,string[] Names) Load(string path)
    {
        using var manifest=JsonDocument.Parse(File.ReadAllBytes(path));
        if(manifest.RootElement.GetProperty("commit").GetString()!="c6629026b1fdef51fd08e3d2fddffd82f97e60a5"||
            manifest.RootElement.GetProperty("penUpMetadata").GetString()!="absent")
            throw new InvalidDataException("Pinned recorded coordinates with absent pen-up metadata required.");
        var inputs=manifest.RootElement.GetProperty("samples").EnumerateArray().Where(s=>s.GetProperty("kind").GetString()=="word").ToArray();
        if(inputs.Length!=9)throw new InvalidDataException("Fixed nine-word diagnostic subset required.");
        var samples=new List<InkStroke[]>();var names=new List<string>();
        foreach(var input in inputs)
        {
            string name=input.GetProperty("file").GetString()!;
            if(name!=Path.GetFileName(name))throw new InvalidDataException("Direct sibling track required.");
            byte[] data=File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,name));
            if(data.Length>200000||Convert.ToHexStringLower(SHA256.HashData(data))!=input.GetProperty("trackSha256").GetString())
                throw new InvalidDataException("Recorded track hash mismatch.");
            using var track=JsonDocument.Parse(data);
            var points=track.RootElement.GetProperty("points").EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),-p[1].GetSingle())).ToArray();
            if(points.Length is<2 or>4096)throw new InvalidDataException("Bounded original coordinate track required.");
            samples.Add([new InkStroke(points)]);names.Add(name);
        }
        return(samples.ToArray(),names.ToArray());
    }
}
