using InkRecognition;
using System.Text.Json;

var controls = args.Length == 0 ? null : JsonSerializer.Deserialize(File.ReadAllText(args[0]), ControlsContext.Default.Controls);
int checkedCases = 0;
if (controls != null)
    foreach (var item in controls.Cases)
    {
        InkStroke[] source = item.Strokes.Select(points => new InkStroke(points.Select(p=>new InkPoint(p[0],p[1])).ToArray())).ToArray();
        float[] values = LegacySourceStrokeEnclosure.Extract(source);
        if (values.Length != item.Expected.Length) throw new Exception("Output length mismatch.");
        for (int i = 0; i < values.Length; i++)
        {
            if (!float.IsFinite(values[i]) || Math.Abs(values[i]-item.Expected[i]) > 2e-4f)
                throw new Exception($"Reference{checkedCases} stroke{i}: expected{item.Expected[i]:R}, actual{values[i]:R}.");
            if (LegacySourceStrokeEnclosure.Evaluate(source,i) != values[i]) throw new Exception("Single-stroke result mismatch.");
        }
        checkedCases++;
    }
InkStroke S(params (float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
var square=S((0,0),(10,0),(10,10),(0,10),(0,0));
var interior=S((3,3),(4,4),(5,5));
void Eq(float expected,float actual,string name,float tolerance=1e-6f)
{ if(Math.Abs(expected-actual)>tolerance)throw new Exception($"{name}: expected{expected:R},actual{actual:R}."); }
Eq(0,LegacySourceStrokeEnclosure.Evaluate([square],0),"An isolated circle/contour is not enclosing ink");
Eq(12.566f/16,LegacySourceStrokeEnclosure.Evaluate([square,interior],0),"Analytic square area/perimeter²");
Eq(0,LegacySourceStrokeEnclosure.Evaluate([square,S((5,5),(30,30))],0),"Exactly half is not enough");
Eq(12.566f/16,LegacySourceStrokeEnclosure.Evaluate([square,S((5,5),(4,4),(30,30))],0),"Strict majority");
Eq(0,LegacySourceStrokeEnclosure.Evaluate([S((0,0),(10,0),(10,10)),interior],0),"Open stroke");
var many=new InkStroke(Enumerable.Range(0,501).Select(i=>new InkPoint((float)Math.Cos(i*Math.Tau/500)*10,(float)Math.Sin(i*Math.Tau/500)*10)).ToArray());
Eq(0,LegacySourceStrokeEnclosure.Evaluate([many,interior],0),"Native500-point limit");
Eq(0,LegacySourceStrokeEnclosure.Evaluate([S((2,2)),interior],0),"Single point");
Eq(0,LegacySourceStrokeEnclosure.Evaluate([S((2,2),(2,2)),interior],0),"Repeated points");
Eq(12.566f/16,LegacySourceStrokeEnclosure.Evaluate([interior,square],1),"Source ordering does not require contour first");
try{LegacySourceStrokeEnclosure.Extract([]);throw new Exception("Empty input accepted.");}catch(ArgumentException){}
try{LegacySourceStrokeEnclosure.Extract([S((float.NaN,1))]);throw new Exception("NaN accepted.");}catch(ArgumentException){}
try{LegacySourceStrokeEnclosure.Evaluate([square],1);throw new Exception("Invalid ordinal accepted.");}catch(ArgumentOutOfRangeException){}
using var canceled=new CancellationTokenSource();canceled.Cancel();
try{LegacySourceStrokeEnclosure.Extract([square],canceled.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {checkedCases} independent enclosure controls + analytic geometry/majority/limits/validation/cancellation.");

sealed record Controls(Case[] Cases);
sealed record Case(float[][][] Strokes,float[] Expected);
[System.Text.Json.Serialization.JsonSerializable(typeof(Controls))]
partial class ControlsContext: System.Text.Json.Serialization.JsonSerializerContext;
