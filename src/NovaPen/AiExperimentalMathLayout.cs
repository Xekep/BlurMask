using InkRecognition;
namespace NovaPen;

internal static class AiExperimentalMathLayout
{
    // Detached DATA carrier only; prepared injected tests do not claim to have
    // executed the cold recognizer. Production owner derives all3 values from
    // genuine ExperimentalMathResult; confidence deliberately absent.
    internal sealed record Candidate(string? LaTex,string? UnsupportedReason,int SyntaxNodes);
    internal static bool Enabled(string mode,bool experimental)=>mode=="math"&&experimental;
    internal static bool CanApply(AiInkLayout.Result value,string mode,bool experimental,bool installedRawPack=false)=>
        value.Admission switch
        {
            AiInkLayout.ResultAdmission.UncalibratedMath=>Enabled(mode,experimental),
            AiInkLayout.ResultAdmission.ExperimentalClassifiedMath=>AiExperimentalAutoLayout.Enabled(mode,experimental,installedRawPack)&&value.ConfidenceForAdmission is null&&value.CategoryScore is >=.85f and <=1f,
            AiInkLayout.ResultAdmission.NativeSelectedText=>experimental&&(mode=="text"||mode=="auto")&&value.TextAdmitted&&value.ConfidenceForAdmission is null,
            _=>value.Admission==AiInkLayout.ResultAdmission.Scored
        };
    internal static IReadOnlyList<AiInkLayout.Result> Recognize(IReadOnlyList<InkMark> strokes,
        Func<IReadOnlyList<InkStroke>,CancellationToken,Candidate?>? recognize=null,CancellationToken token=default,
        bool calculate=true,Func<string,CancellationToken,InkMath.Solver.MathSolution?>? solve=null)
    {
        token.ThrowIfCancellationRequested();if(strokes.Count is<1 or>256)return [];
        long count=0;foreach(var stroke in strokes){token.ThrowIfCancellationRequested();count+=stroke.Points.Count;if(stroke.Points.Count<1||count>4096)return [];}
        recognize??=AiExperimentalMathModels.Recognize;var output=new List<AiInkLayout.Result>();
        foreach(var group in strokes.GroupBy(s=>(s.Color,s.FadeSeconds)))
        {
            token.ThrowIfCancellationRequested();var sources=group.ToArray();var bounds=sources.Select(s=>s.Bounds).Aggregate((a,b)=>a.Union(b));
            if(!double.IsFinite(bounds.Width)||!double.IsFinite(bounds.Height)||bounds.Width<2||bounds.Height<2||
                bounds.Left< -100000||bounds.Top< -100000||bounds.Right>100000||bounds.Bottom>100000)continue;
            var ink=new InkStroke[sources.Length];bool invalid=false;
            for(int i=0;i<sources.Length;i++)
            {
                token.ThrowIfCancellationRequested();var points=new InkPoint[sources[i].Points.Count];
                for(int j=0;j<points.Length;j++)
                {
                    var p=sources[i].Points[j];if(!double.IsFinite(p.X)||!double.IsFinite(p.Y)||Math.Abs(p.X)>100000||Math.Abs(p.Y)>100000){invalid=true;break;}
                    points[j]=new((float)p.X,(float)p.Y);
                }
                ink[i]=new(points);
            }
            if(invalid)continue;
            var candidate=recognize(ink,token);token.ThrowIfCancellationRequested();
            if(candidate is not {UnsupportedReason:null,LaTex:{Length:>0 and<=65536} latex,SyntaxNodes:>0}||!MathTypesetting.Supported(latex))continue;
            string text=calculate?AiMathLayout.WithSolvedResult(latex,token,solve):latex;token.ThrowIfCancellationRequested();
            var first=sources[0];var mark=new InkMark(InkTool.Text,first.Color,first.Width,bounds.TopLeft)
            {Id=first.Id,Text=text,Font="STIX Two Math",FontSize=Math.Clamp(bounds.Height,8,300),FadeSeconds=first.FadeSeconds};
            mark.AddPoint(bounds.BottomRight+new Avalonia.Vector(Math.Min(100000-bounds.Right,AiMathLayout.ResultWidth(latex,text,bounds.Height)),0));
            output.Add(new(sources,mark,float.NaN){Admission=AiInkLayout.ResultAdmission.UncalibratedMath,SizingText=latex,
                MathCalculationFailed=calculate&&text==latex&&AiMathLayout.HasCalculationRequest(latex)});
        }
        return output;
    }
}
