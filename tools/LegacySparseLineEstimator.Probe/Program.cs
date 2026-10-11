using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("Supply independent sparse-line controls.");
if(new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Controls exceed 8 MiB.");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));int estimated=0,declined=0;
foreach(var item in document.RootElement.EnumerateArray())
{
    var strokes=item.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    bool success=LegacySparseLineEstimator.TryEstimate(strokes,out var actual,item.GetProperty("threshold").GetByte());
    Require(success==item.GetProperty("estimated").GetBoolean(),"Sparse branch predicate.");
    if(!success){declined++;continue;}
    Equal(actual.Line.Baseline,item.GetProperty("baseline").GetSingle());Equal(actual.Line.Height,item.GetProperty("height").GetSingle());
    Equal(actual.MeanY,item.GetProperty("mean").GetSingle());Equal(actual.StandardDeviationY,item.GetProperty("std").GetSingle());Equal(actual.HalfBand,item.GetProperty("half").GetSingle());
    Require(actual.VerticalRunCount==item.GetProperty("runs").GetInt32(),"Native vertical run count.");estimated++;
}
Throws<ArgumentException>(()=>LegacySparseLineEstimator.TryEstimate([],out _));
Throws<ArgumentException>(()=>LegacySparseLineEstimator.TryEstimate([[]],out _));
Throws<ArgumentException>(()=>LegacySparseLineEstimator.TryEstimate([[new(float.NaN,0)]],out _));
Throws<ArgumentException>(()=>LegacySparseLineEstimator.TryEstimate([[new(0,0)]],out _,0));
Throws<InvalidDataException>(()=>LegacySparseLineEstimator.TryEstimate([[new(float.MaxValue,0),new(-float.MaxValue,0)]],out _));
Throws<OperationCanceledException>(()=>LegacySparseLineEstimator.TryEstimate([[new(0,0)]],out _,cancellationToken:new(true)));
Console.WriteLine($"PASS {estimated} independently estimated sparse lines and {declined} explicit higher-complexity declines; bounds, overflow and cancellation.");
static void Equal(float a,float b){Require(float.IsFinite(a)&&MathF.Abs(a-b)<=2e-6f*MathF.Max(1,MathF.Abs(b)),$"Sparse geometry {a} vs {b}");}
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}");}
