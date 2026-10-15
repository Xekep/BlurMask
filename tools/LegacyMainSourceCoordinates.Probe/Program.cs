using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json required");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,bytes=0,empty=0,frames=0;
foreach(var test in document.RootElement.EnumerateArray())
{
    if(test.TryGetProperty("frame",out var rawFrame))
    {
        byte[] frame=Convert.FromHexString(rawFrame.GetString()!);
        LegacyNativeMainSourceCoordinates.WriteCallbackCounts(test.GetProperty("glyph").GetByte(),frame);
        if(!frame.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("after").GetString()!)))
            throw new InvalidDataException("Whole944 counter frame differs.");
        frames++;continue;
    }
    byte[] storage=Convert.FromHexString(test.GetProperty("before").GetString()!);
    int result=LegacyNativeMainSourceCoordinates.WriteSingle(test.GetProperty("node").GetUInt32(),
        test.GetProperty("arc").GetUInt16(),storage);
    if(result!=1||!storage.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("after").GetString()!)))
        throw new InvalidDataException("Whole coordinate storage or required count differs.");
    count++;bytes+=storage.Length;if(storage.Length==0)empty++;
}
try{LegacyNativeMainSourceCoordinates.WriteSingle(0,0,new byte[17]);throw new InvalidDataException("Partial coordinate accepted.");}
catch(ArgumentException){}
foreach(int invalid in new[]{-1,21})
{
    byte[] bad=new byte[944];System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bad.AsSpan(0x25c),invalid);
    try{LegacyNativeMainSourceCoordinates.WriteCallbackCounts(0,bad);throw new InvalidDataException("Invalid callback count accepted.");}catch(ArgumentException){}
}
try{LegacyNativeMainSourceCoordinates.WriteCallbackCounts(0,new byte[943]);throw new InvalidDataException("Partial frame accepted.");}catch(ArgumentException){}
Console.WriteLine($"PASS {count} independent coordinate controls/{bytes} whole storage bytes/{empty} zero-capacity calls and {frames} whole944 counter frames; requiredCount1, untouched padding/tail and 4 extent guards. Packed source references, not XY.");
