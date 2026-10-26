using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=2 || args[1] is not("before" or "after"))throw new ArgumentException("FAKE-ADMISSION.json before|after");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));int checks=0;
foreach(var test in json.RootElement.EnumerateArray())
{
    byte[] row=new byte[test.GetProperty("length").GetInt32()];
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),test.GetProperty("kind").GetUInt16());
    int bits=test.GetProperty("scoreBits").GetInt32();BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(16),bits);
    bool actual=ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row,test.GetProperty("text").GetString()!,out float score);
    if(actual!=test.GetProperty(args[1]).GetBoolean() || actual&&BitConverter.SingleToInt32Bits(score)!=bits)
        throw new InvalidOperationException(test.GetProperty("name").GetString()+": actual admission/value differs.");
    checks++;
}
if(checks!=21)throw new InvalidOperationException("Incomplete fake admission cohort.");
Console.WriteLine($"PASS {checks} real-method fake admission/domain/token controls ({args[1]}); no model/NN/native.");
