using InkRecognition;
using System.Text.Json;
var stroke = new LegacyInkStroke[]{new([new LegacyInkPoint(0,0),new(10,0)])};
var line=LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[0,10],[0,1],[0x41,0x82],[0,0]);
Require(line.SequenceEqual(new[]{1024,1024,1024,512,1,1,1024,0,0,1,1,0,0,0,0,0}),"Analytic spatial line statistics failed.");
var singleton=LegacySpatialWindowStatistics.Extract(stroke,11,1,-1,[0,10],[1],[0,0xc0],[0,1]);
Require(singleton[0]==0&&singleton[1]==0&&singleton[2]==512&&singleton[3]==1024&&singleton[4]==1&&singleton[5]==1&&singleton[15]==512,"Singleton spatial extent/density/physical position/flags failed.");
Require(LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[0,0],[1],[0,0],[0,0])[3]==0,"Zero total physical arc must preserve zero raw mean.");
Require(LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[0,10],[],[0,0],[0,0]).All(p=>p==0),"Empty spatial window must emit raw zeros.");
Throws<ArgumentException>(()=>LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[],[],[0,0],[0,0]));
Throws<ArgumentException>(()=>LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[1,0],[0],[0,0],[0,0]));
Throws<ArgumentException>(()=>LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[0,2097152],[0],[0,0],[0,0]));
Throws<OperationCanceledException>(()=>LegacySpatialWindowStatistics.Extract(stroke,11,1,0,[0,10],[],[0,0],[0,0],cancellationToken:new CancellationToken(true)));
var cells=Enumerable.Range(0,16).Select(_=>new int[16]).ToArray();cells[0]=Enumerable.Repeat(128,16).ToArray();
var minima=Enumerable.Repeat((short)-1024,274).ToArray();var maxima=Enumerable.Repeat((short)1024,274).ToArray();
var transformed=LegacySpatialFeatureAssembly.TransformGeometry(new int[17],cells,minima,maxima);
Require(transformed.Length==274&&transformed[18]==0&&transformed[19]==0&&transformed[20]==0&&transformed[21]==-768&&transformed[26]==256&&transformed[33]==0,"Spatial width/height/density/position/covariance/loop transform denominators failed.");
Throws<ArgumentException>(()=>LegacySpatialFeatureAssembly.TransformGeometry(new int[17],cells,minima.AsSpan(1),maxima));
Throws<OperationCanceledException>(()=>LegacySpatialFeatureAssembly.TransformGeometry(new int[17],cells,minima,maxima,new CancellationToken(true)));
int count=0;
if(args.Length>0)
{
    if(args.Length!=1||new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Usage: LegacySpatialStatistics.Probe [own-reference.json up to8MiB]");
    using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var item in document.RootElement.EnumerateArray())
    {
        if(item.TryGetProperty("global",out var global))
        {
            int[] Integers(JsonElement element)=>element.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
            short[] Shorts(string key)=>item.GetProperty(key).EnumerateArray().Select(p=>p.GetInt16()).ToArray();
            var geometry=LegacySpatialFeatureAssembly.TransformGeometry(Integers(global),item.GetProperty("cells").EnumerateArray().Select(Integers).ToArray(),Shorts("minima"),Shorts("maxima"));
            Require(geometry.SequenceEqual(Shorts("expected")),$"Independent spatial geometry case{count} failed.");count++;continue;
        }
        var strokes=item.GetProperty("strokes").EnumerateArray().Select(s=>new LegacyInkStroke(s.EnumerateArray().Select(p=>new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray())).ToArray();
        byte[] Bytes(string key)=>item.GetProperty(key).EnumerateArray().Select(p=>p.GetByte()).ToArray();
        var actual=LegacySpatialWindowStatistics.Extract(strokes,item.GetProperty("width").GetInt32(),item.GetProperty("height").GetInt32(),item.GetProperty("shift").GetInt32(),item.GetProperty("cumulative").EnumerateArray().Select(p=>p.GetInt32()).ToArray(),Bytes("members"),Bytes("flags"),Bytes("intersections"),item.GetProperty("unit").GetBoolean());
        Require(actual.SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(p=>p.GetInt32())), $"Independent spatial statistics case{count} failed.");count++;
    }
}
Console.WriteLine($"PASS spatial local16/geometry274: {count} independent controls, bbox/density/cumulative mean/covariance/flags, transform/calibration, empty and zeroarc, malformed/cancellation. Not handwriting accuracy.");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected{typeof(T).Name}.");}
