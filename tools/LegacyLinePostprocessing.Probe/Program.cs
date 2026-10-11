using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("Supply independent postcallback controls.");
if(new FileInfo(args[0]).Length>16*1024*1024)throw new ArgumentException("Controls exceed 16 MiB.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,points=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
    var strokes=c.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray()).ToArray();
    var original=strokes.Select(s=>(InkPoint[])s.Clone()).ToArray();
    var line=new LegacyLinePair(c.GetProperty("input_baseline").GetSingle(),c.GetProperty("input_height").GetSingle());float slope=c.GetProperty("slope").GetSingle();
    var a=LegacyLineDeskew.Apply(strokes,line,slope);var e=c.GetProperty("deskew");
    Equal(a.Line.Baseline,e.GetProperty("baseline").GetSingle());Equal(a.Line.Height,e.GetProperty("height").GetSingle());
    Equal(a.Cosine,e.GetProperty("cosine").GetSingle());Equal(a.Sine,e.GetProperty("sine").GetSingle());Equal(a.TranslationX,e.GetProperty("tx").GetSingle());Equal(a.TranslationY,e.GetProperty("ty").GetSingle());Require(a.Slope==slope,"Native slope preservation.");
    for(int i=0;i<strokes.Length;i++)for(int j=0;j<strokes[i].Length;j++)
    {
        var p=e.GetProperty("transformed")[i][j];Equal(a.Strokes[i][j].X,p[0].GetSingle());Equal(a.Strokes[i][j].Y,p[1].GetSingle());
        Require(strokes[i][j]==original[i][j],"Public input remains intact.");points++;
    }
    var g=c.GetProperty("guideline");LegacyLineCluster? guideline=g.ValueKind==JsonValueKind.Null?null:new(g[0].GetInt32(),g[1].GetSingle(),g[2].GetSingle(),g[3].GetSingle());
    var m=LegacyLineGuidelineAdjustment.Compute(strokes,line,slope,c.GetProperty("enabled").GetBoolean(),guideline,c.GetProperty("row").GetInt32());var v=c.GetProperty("metadata");
    Equal(m.AlignmentBaseline,v.GetProperty("alignment").GetSingle());Equal(m.AlignmentHeight,v.GetProperty("aligned_height").GetSingle());
    Require(m.GuidelineIndex==v.GetProperty("guideline_index").GetInt32()&&m.RowIndex==v.GetProperty("row_index").GetInt32(),"Native guideline row inference.");count++;
}
Throws<ArgumentException>(()=>LegacyLineDeskew.Apply([],new(0,1),0));
Throws<ArgumentException>(()=>LegacyLineDeskew.Apply([[new(float.NaN,0)]],new(0,1),0));
Throws<InvalidDataException>(()=>LegacyLineDeskew.Apply([[new(0,0)]],new(0,1),float.MaxValue));
Throws<ArgumentException>(()=>LegacyLineGuidelineAdjustment.Compute([[new(0,0)]],new(0,1),0,true,new(2,0,0,1)));
Throws<InvalidDataException>(()=>LegacyLineGuidelineAdjustment.Compute([[new(0,0)]],new(1e20f,1),0,true,new(2,0,1,1)));
Throws<OperationCanceledException>(()=>LegacyLineDeskew.Apply([[new(0,0)]],new(0,1),0,new(true)));
Throws<OperationCanceledException>(()=>LegacyLineGuidelineAdjustment.Compute([[new(0,0)]],new(0,1),0,true,cancellationToken:new(true)));
Console.WriteLine($"PASS {count} own postcallback affine + guideline controls ({points} transformed points); fallback, row ties, input ownership, bounds, cancellation.");
static void Equal(float a,float b)=>Require(float.IsFinite(a)&&MathF.Abs(a-b)<=2e-7f*MathF.Max(1,MathF.Abs(b)),$"Postcallback float {a} vs {b}");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}");}
