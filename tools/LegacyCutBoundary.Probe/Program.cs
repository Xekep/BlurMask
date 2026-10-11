using InkRecognition;
using System.Text.Json;
var weak = LegacyCutBoundaryPolicy.Evaluate(.1f,.3f,false,false,1);
Require(!weak.Retained && weak.JoinCost == 0 && weak.CutCost > 0 && !weak.GeometryPromoted, "Below-threshold NN cut retained.");
var promoted = LegacyCutBoundaryPolicy.Evaluate(.1f,.3f,false,true,1);
Require(promoted.Retained && promoted.GeometryPromoted && promoted.RawProbability == .1f && promoted.EffectiveProbability == .3f, "Previous hint did not promote, or raw NN output lost.");
Require(LegacyCutBoundaryPolicy.Evaluate(.3f,.3f,false,false,1).Retained, "Equality boundary rejected.");
Require(!LegacyCutBoundaryPolicy.Evaluate(0,0,true,true,1).Retained, "Zero-probability cut retained.");
Require(LegacyCutBoundaryPolicy.Evaluate(0,.5f,true,false,1).Retained, "Zero NN probability did not promote.");
Require(LegacyCutBoundaryPolicy.Evaluate(1,.5f,false,false,1) is {CutCost:0,JoinCost:LegacyCutBoundaryPolicy.ImpossibleCost}, "Unit probability sentinels differ.");
Require(LegacyCutBoundaryPolicy.Evaluate(0,.5f,false,false,1) is {CutCost:LegacyCutBoundaryPolicy.ImpossibleCost,JoinCost:0}, "Zero probability sentinels differ.");
Require(LegacyCutBoundaryPolicy.AdjustThreshold(.3f,.2f,-1) == .3f, "Current-weight sentinel was averaged.");
Require(LegacyCutBoundaryPolicy.AdjustThreshold(.3f,.2f,.8f) == .15f, "Float threshold adjustment differs.");
foreach (float bad in new[] {float.NaN,float.PositiveInfinity,-.1f,1.1f})
    Reject(()=>LegacyCutBoundaryPolicy.Evaluate(bad,.5f,false,false,1));
Reject(()=>LegacyCutBoundaryPolicy.Evaluate(.5f,.5f,false,false,0));
Reject(()=>LegacyCutBoundaryPolicy.Evaluate(float.Epsilon,.5f,false,false,float.MaxValue));
Reject(()=>LegacyCutBoundaryPolicy.AdjustThreshold(.5f,-2,.5f));
int controls = 0;
if (args.Length != 0)
{
    using var json = JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach (var item in json.RootElement.EnumerateArray())
    {
        float threshold = LegacyCutBoundaryPolicy.AdjustThreshold(item.GetProperty("base").GetSingle(),item.GetProperty("previous").GetSingle(),item.GetProperty("current").GetSingle());
        Require(BitConverter.SingleToInt32Bits(threshold) == item.GetProperty("thresholdBits").GetInt32(), "Independent threshold float bits differ.");
        var d = LegacyCutBoundaryPolicy.Evaluate(item.GetProperty("probability").GetSingle(),threshold,item.GetProperty("hint").GetBoolean(),item.GetProperty("previousHint").GetBoolean(),item.GetProperty("scale").GetSingle());
        Require(BitConverter.SingleToInt32Bits(d.EffectiveProbability) == item.GetProperty("effectiveBits").GetInt32() &&
            d.GeometryPromoted == item.GetProperty("promoted").GetBoolean() && d.Retained == item.GetProperty("retained").GetBoolean() &&
            d.CutCost == item.GetProperty("cut").GetInt32() && d.JoinCost == item.GetProperty("join").GetInt32(), "Independent boundary decision differs.");
        controls++;
    }
}
Console.WriteLine($"PASS sampler252 cut policy: positive/equality/zero/one decisions, geometry promotion, separate raw scores, float threshold arithmetic, cut/join logarithmic costs and int32 bounds; {controls} independent controls. Prepared-position production is separate.");
static void Require(bool ok,string message) { if (!ok) throw new InvalidOperationException(message); }
static void Reject(Action action) { try { action(); throw new Exception("Invalid boundary accepted."); } catch (ArgumentOutOfRangeException) { } }
