using Avalonia;
using InkRecognition;
using System.Text;
using System.Globalization;
namespace BlurMask;

internal static partial class AiInkLayout
{
    internal sealed record PositionedWord(InkMark Mark,Rect SourceBounds,LegacyCompletedGlobalTextWords.PreparedLine Line);
    private static Result? RecognizePositioned(InkMark[] sources,IStrokeTextRecognizer recognizer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //The exact transformed points sent to inference also index every WORD
        //original range. No camera transform/last glyph box is substituted.
        var ink=sources.Select(s=>new InkStroke(s.Points.Select(p=>p.Transform(s.Transform)).Select(p=>new InkPoint((float)p.X,(float)p.Y)).ToArray())).ToArray();
        var decoded=recognizer.Recognize(ink,token);token.ThrowIfCancellationRequested();
        if(decoded.WordRuns is not{Count:>0 and<=128} runs||!decoded.WordRunsCoverFullInput||
            !decoded.ExperimentalCandidateScore||!float.IsFinite(decoded.Confidence)||decoded.Confidence is<.5f or>1f)
            return null;
        if(runs.Count>1&&decoded.Text.Length!=0||decoded.Confidence!=runs.Min(r=>r.CandidateScore)||runs.Sum(r=>(long)r.OriginalRanges.Count)>4096)return null;
        var first=sources[0];var output=new PositionedWord[runs.Count];int characters=0;long remainingPointReads=RecognizedWordInkBounds.MaximumWholeRunPointReads;
        for(int i=0;i<runs.Count;i++)
        {
            token.ThrowIfCancellationRequested();var run=runs[i];characters=checked(characters+run.Text.Length);
            if(!float.IsFinite(run.CandidateScore)||run.CandidateScore is<.5f or>1f||string.IsNullOrWhiteSpace(run.Text)||
                run.Text.Any(char.IsWhiteSpace)||run.Text.EnumerateRunes().Any(r=>Rune.GetUnicodeCategory(r) is UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate)||characters>1024||!FiniteLine(run.Line))return null;
            var projection=RecognizedWordInkBounds.Project(ink,run.OriginalRanges,remainingPointReads,token);remainingPointReads-=projection.PointReads;
            var actual=projection.Bounds;
            if(actual!=run.InkBounds)return null;
            var b=new Rect(actual.Left,actual.Top,actual.Right-actual.Left,actual.Bottom-actual.Top);
            if(b.Width<=0||b.Height<=0||b.Left< -100000||b.Top< -100000||b.Right>100000||b.Bottom>100000)return null;
            double baseline=run.Line.OriginY+(b.Left-run.Line.OriginX)*Math.Tan(run.Line.Angle);
            if(!double.IsFinite(baseline))return null;
            var transform=Matrix.CreateTranslation(-b.Left,-baseline)*Matrix.CreateRotation(run.Line.Angle)*Matrix.CreateTranslation(b.Left,baseline);
            var mark=new InkMark(InkTool.Text,first.Color,first.Width,b.TopLeft)
            {Id=i==0?first.Id:Guid.NewGuid(),Text=run.Text,Font="Inter",FontSize=Math.Clamp(run.Line.XHeight,8,300),FadeSeconds=first.FadeSeconds,Transform=transform};
            mark.AddPoint(b.BottomRight);output[i]=new(mark,b,run.Line);
        }
        //Independently check the entire original input coverage before a mark
        //can replace ink. One unsupported WORD rejects this whole Result.
        var all=runs.SelectMany(r=>r.OriginalRanges).ToArray();
        if(!RecognizedWordInkBounds.CoversFullInput(ink,all,token))return null;
        return new(sources,output[0].Mark,decoded.Confidence){ExperimentalCandidateScore=true,PositionedWords=output};
    }
    private static bool FiniteLine(LegacyCompletedGlobalTextWords.PreparedLine line)=>line.Index>=0&&double.IsFinite(line.OriginX)&&
        double.IsFinite(line.OriginY)&&double.IsFinite(line.Angle)&&Math.Cos(line.Angle)!=0&&
        double.IsFinite(line.XHeight)&&line.XHeight>0&&double.IsFinite(line.UnadjustedBaseline);
}
