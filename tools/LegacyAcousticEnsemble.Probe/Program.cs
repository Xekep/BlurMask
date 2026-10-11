using InkRecognition;
using System.Text.Json;
LegacyAcousticFamilyScores?[] slots=new LegacyAcousticFamilyScores?[7];
slots[2]=new(new[]{8192},new[]{.25f},new[]{30f});slots[3]=new(new[]{0},new[]{0f},new[]{.575f});
Require(LegacyAcousticEnsemble.Combine(slots)[0]==.5f,"Per-family center equality or active count failed.");
slots[2]=new(new[]{32767},new[]{-100f},new[]{100f});slots[3]=null;
Require(LegacyAcousticEnsemble.Combine(slots)[0]==1,"High-confidence clamp failed.");
slots[2]=new(new[]{0},new[]{100f},new[]{100f});
Require(LegacyAcousticEnsemble.Combine(slots)[0]==0,"Native float-max exponent fallback failed.");
Throws<ArgumentException>(()=>LegacyAcousticEnsemble.Combine([]));
Throws<ArgumentException>(()=>LegacyAcousticEnsemble.Combine(new LegacyAcousticFamilyScores?[7]));
slots[2]=new(new[]{32768},new[]{0f},new[]{1f});Throws<ArgumentException>(()=>LegacyAcousticEnsemble.Combine(slots));
slots[2]=new(new[]{0},new[]{float.NaN},new[]{1f});Throws<ArgumentException>(()=>LegacyAcousticEnsemble.Combine(slots));
slots[2]=new(new[]{0},new[]{0f},new[]{0f});Throws<ArgumentException>(()=>LegacyAcousticEnsemble.Combine(slots));
Throws<OperationCanceledException>(()=>LegacyAcousticEnsemble.Combine(slots,new CancellationToken(true)));
int count=0;
if(args.Length>0)
{
    if(args.Length!=1||new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Usage: LegacyAcousticEnsemble.Probe [own-controls.json up to8MiB]");
    using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var item in document.RootElement.GetProperty("cases").EnumerateArray())
    {
        var families=item.GetProperty("families").EnumerateArray().Select(f=>f.ValueKind==JsonValueKind.Null?null:new LegacyAcousticFamilyScores(
            f.GetProperty("probabilities").EnumerateArray().Select(p=>p.GetInt32()).ToArray(),
            f.GetProperty("centers").EnumerateArray().Select(p=>p.GetSingle()).ToArray(),
            f.GetProperty("slopes").EnumerateArray().Select(p=>p.GetSingle()).ToArray())).ToArray();
        var actual=LegacyAcousticEnsemble.Combine(families);var expected=item.GetProperty("expected").EnumerateArray().Select(p=>p.GetSingle()).ToArray();
        Require(actual.Length==expected.Length&&actual.Zip(expected).All(p=>Math.Abs(p.First-p.Second)<=1e-7f),$"Independent ensemble case{count} failed.");count++;
    }
}
Console.WriteLine($"PASS calibrated acoustic ensemble: {count} independent mode1 controls, centers, active count, float summation, exponent fallback, malformed/cancellation. Not all-family recognition or SDK candidate policy.");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected{typeof(T).Name}.");}
