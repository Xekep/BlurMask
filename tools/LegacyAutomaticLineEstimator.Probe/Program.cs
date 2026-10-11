using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length != 1) throw new ArgumentException("Supply independent class24a controls.");
if(new FileInfo(args[0]).Length > 16 * 1024 * 1024) throw new ArgumentException("Controls exceed 16 MiB.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int sparse=0, dense=0, blended=0, nodes=0;
foreach(var item in document.RootElement.EnumerateArray())
{
    byte[] node = Convert.FromHexString(item.GetProperty("node").GetString()!);
    var profile = LegacyAutomaticLineProfile.ReadResourceNode(node, 0); var p = item.GetProperty("profile");
    Require(profile.NativeFirstField == p[0].GetInt32() && profile.NativeSecondField == p[1].GetSingle() &&
        profile.IncludeEndpoints == (p[2].GetInt32() != 0) && profile.MinimumSlope == p[3].GetSingle() &&
        profile.MaximumSlope == p[4].GetSingle() && profile.SparseThreshold == p[5].GetByte() &&
        profile.DenseThreshold == p[6].GetByte() && profile.ReservedByte == p[7].GetByte(), "Exact original scalar fields."); nodes++;
    var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => s.EnumerateArray().Select(v => new InkPoint(v[0].GetSingle(),v[1].GetSingle())).ToArray()).ToArray();
    var a = LegacyAutomaticLineEstimator.Estimate(strokes, profile);
    Equal(a.Baseline, item.GetProperty("baseline").GetDouble()); Equal(a.UpperGuideline,item.GetProperty("upper").GetDouble()); Equal(a.Slope,item.GetProperty("slope").GetDouble());
    Require(a.VerticalRunCount == item.GetProperty("runs").GetInt32() && a.UsedDenseFit == item.GetProperty("dense").GetBoolean() && a.BlendedSparseStatistics == item.GetProperty("blended").GetBoolean(), "Class24a branch boundaries.");
    Require(a.UsedDenseFit ? a.FitIterations is >=1 and <=20 : a.FitIterations == 0, "Fit iteration limit.");
    if(a.UsedDenseFit) dense++; else sparse++; if(a.BlendedSparseStatistics) blended++;
}
byte[] valid = Convert.FromHexString(document.RootElement[0].GetProperty("node").GetString()!);
Throws<InvalidDataException>(() => LegacyAutomaticLineProfile.ReadResourceNode(valid, -1));
Throws<InvalidDataException>(() => LegacyAutomaticLineProfile.ReadResourceNode(valid.AsSpan(0,35), 0));
byte[] corrupt=(byte[])valid.Clone(); corrupt[14]=0;
Throws<InvalidDataException>(() => LegacyAutomaticLineProfile.ReadResourceNode(corrupt, 0));
corrupt=(byte[])valid.Clone(); BinaryPrimitives.WriteSingleLittleEndian(corrupt.AsSpan(25), float.NaN);
Throws<InvalidDataException>(() => LegacyAutomaticLineProfile.ReadResourceNode(corrupt, 0));
corrupt=(byte[])valid.Clone(); corrupt[33]=0;
Throws<InvalidDataException>(() => LegacyAutomaticLineProfile.ReadResourceNode(corrupt, 0));
var goodProfile=LegacyAutomaticLineProfile.ReadResourceNode(valid,0);
Throws<OperationCanceledException>(() => LegacyAutomaticLineEstimator.Estimate([[new(0,0)]], goodProfile, new(true)));
Throws<ArgumentException>(() => LegacyAutomaticLineEstimator.Estimate([[new(float.NaN,0)]], goodProfile));
Console.WriteLine($"PASS {nodes} exact scalar node reads, {sparse} sparse + {dense} dense estimates ({blended} blended); threshold neighbours, profile rejection, cancellation.");
static void Equal(double a,double b) => Require(double.IsFinite(a) && Math.Abs(a-b) <= 2e-7 * Math.Max(1,Math.Abs(b)), $"Class24a geometry {a} vs {b}");
static void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T:Exception { try { action(); } catch(T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}"); }
