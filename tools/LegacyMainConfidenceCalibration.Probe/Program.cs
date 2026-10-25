using InkRecognition;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("acoustic.res controls.json");
byte[] resource=File.ReadAllBytes(args[0]);var calibration=LegacyMainConfidenceCalibration.ReadRussianSource(resource);
Require(calibration.SymbolCount==192&&calibration.ActiveFamilyCount==4,"Actual main table dimensions differ.");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[1]));int count=0,extended=0,fallback=0;
foreach(var row in controls.RootElement.GetProperty("rows").EnumerateArray())
{
    ushort label=row[0].GetUInt16();uint family=row[1].GetUInt32();int probability=row[2].GetInt32();uint expected=row[3].GetUInt32();
    uint actual=BitConverter.SingleToUInt32Bits(calibration.Calibrate(label,family,probability));
    // libm exp can differ by one final binary32 ULP across OS/architectures.
    Require(Math.Abs((long)actual-expected)<=1,"Independent original-label calibration differs.");count++;
    if(label is >=180 and <192&&family<4)extended++;
    if(label>=192||family>=4)fallback++;
}
Require(count==16548&&extended==576&&fallback==7332,"Missing stored/extended-label or dimension fallback coverage.");
int endpointCount=0,endpointExtended=0,endpointFallback=0;
foreach(var row in controls.RootElement.GetProperty("endpointRows").EnumerateArray())
{
    ushort label=row[0].GetUInt16();uint family=row[1].GetUInt32();int probability=row[2].GetInt32();uint expected=row[3].GetUInt32();
    Require(probability==32768,"Separate endpoint controls must exercise32768.");
    uint actual=BitConverter.SingleToUInt32Bits(calibration.Calibrate(label,family,probability));
    Require(Math.Abs((long)actual-expected)<=1,"Independent32768 endpoint calibration differs.");endpointCount++;
    if(label is >=180 and <192&&family<4)endpointExtended++;
    if(label>=192||family>=4)endpointFallback++;
}
Require(endpointCount==1379&&endpointExtended==48&&endpointFallback==611,"Missing separate32768 endpoint coverage.");
Require(calibration.Calibrate(185,0,16384)!=calibration.Calibrate(0,0,16384),"Extended label incorrectly reused an alias calibration.");
Require(calibration.Calibrate(185,0,16384)!=calibration.Calibrate(193,0,16384),"Extended stored calibration incorrectly used default fallback.");
float owned=calibration.Calibrate(185,0,16384);Array.Clear(resource);
Require(calibration.Calibrate(185,0,16384)==owned,"Calibration retained caller resource memory.");
int guards=0;
foreach(int invalid in new[]{-1,32769,int.MinValue,int.MaxValue})
    try{calibration.Calibrate(0,0,invalid);throw new Exception("Invalid EXP accepted.");}catch(ArgumentOutOfRangeException){guards++;}
try{calibration.Calibrate(0,0,0,new(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){guards++;}
try{calibration.Calibrate(0,0,32768,new(true));throw new Exception("Endpoint cancellation ignored.");}catch(OperationCanceledException){guards++;}
try{LegacyMainConfidenceCalibration.ReadRussianSource(resource);throw new Exception("Corrupt resource accepted.");}catch(InvalidDataException){guards++;}
try{LegacyMainConfidenceCalibration.ReadRussianSource(resource.AsSpan(0,100));throw new Exception("Short resource accepted.");}catch(InvalidDataException){guards++;}
try{LegacyMainConfidenceCalibration.ReadRussianSource(resource,new(true));throw new Exception("Canceled load accepted.");}catch(OperationCanceledException){guards++;}
Console.WriteLine($"PASS {count} original-label main calibration references, {extended} extended labels, {fallback} fallbacks; separate {endpointCount} endpoint32768 references/{endpointExtended} extended/{endpointFallback} fallback; {guards} guards and owned coefficients. Full group confidence/216BC0 not implemented by this scalar helper.");
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
