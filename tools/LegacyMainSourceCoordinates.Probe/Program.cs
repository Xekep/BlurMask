using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json required");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,bytes=0,empty=0;
foreach(var test in document.RootElement.EnumerateArray())
{
    byte[] storage=Convert.FromHexString(test.GetProperty("before").GetString()!);
    int result=LegacyNativeMainSourceCoordinates.WriteSingle(test.GetProperty("node").GetUInt32(),
        test.GetProperty("arc").GetUInt16(),storage);
    if(result!=1||!storage.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("after").GetString()!)))
        throw new InvalidDataException("Whole coordinate storage or required count differs.");
    count++;bytes+=storage.Length;if(storage.Length==0)empty++;
}
try{LegacyNativeMainSourceCoordinates.WriteSingle(0,0,new byte[17]);throw new InvalidDataException("Partial coordinate accepted.");}
catch(ArgumentException){}
Console.WriteLine($"PASS {count} independent coordinate controls/{bytes} whole storage bytes/{empty} zero-capacity calls; requiredCount1, untouched padding/tail and invalid extent guard. Packed source references, not XY.");
