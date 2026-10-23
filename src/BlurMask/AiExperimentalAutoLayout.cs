using InkRecognition;
using InkRecognition.RawContent;
namespace BlurMask;

// Explicit experimental classification + supported-syntax admission. Category
// softmax stays separate from null formula confidence; this is not calibration.
internal static class AiExperimentalAutoLayout
{
    internal static bool Enabled(string mode,bool experimental,bool installedRawPack)=>mode=="auto"&&experimental&&!installedRawPack;
    internal static IReadOnlyList<AiInkLayout.Result> Recognize(IReadOnlyList<InkMark> strokes,
        Func<IReadOnlyList<InkStroke>,CancellationToken,RawContentResult?>? classify=null,
        Func<IReadOnlyList<InkStroke>,CancellationToken,AiExperimentalMathLayout.Candidate?>? math=null,
        IStrokeTextRecognizer? text=null,CancellationToken token=default,
        Func<string,CancellationToken,InkMath.Solver.MathSolution?>? solve=null)
    {
        token.ThrowIfCancellationRequested();
        if(strokes.Count is <1 or >256||strokes.Sum(s=>(long)s.Points.Count)>20000||strokes.Distinct().Count()!=strokes.Count)return [];
        var input=new InkStroke[strokes.Count];
        for(int i=0;i<input.Length;i++)
        {
            token.ThrowIfCancellationRequested();var points=strokes[i].Points;
            if(points.Count<1||points.Any(p=>!double.IsFinite(p.X)||!double.IsFinite(p.Y)||Math.Abs(p.X)>1000000||Math.Abs(p.Y)>1000000))return [];
            input[i]=new(points.Select(p=>new InkPoint((float)p.X,(float)p.Y)).ToArray());
        }
        var classified=(classify??AiExperimentalRawContentModels.Analyze)(input,token);token.ThrowIfCancellationRequested();
        if(classified is null||classified.Strokes.Count!=strokes.Count||classified.Regions.Count>strokes.Count)return [];
        var byIndex=new InkClassification?[strokes.Count];
        foreach(var p in classified.Strokes)
        {
            if(p.StrokeIndex<0||p.StrokeIndex>=strokes.Count||byIndex[p.StrokeIndex] is not null||!float.IsFinite(p.Confidence)||p.Confidence is <0 or >1)return [];
            byIndex[p.StrokeIndex]=p;
        }
        var plans=new List<(InkCategory Category,int[] Indices,float Score)>();var uses=new int[strokes.Count];
        foreach(var region in classified.Regions)
        {
            token.ThrowIfCancellationRequested();
            if(region.Category is not (InkCategory.Math or InkCategory.Text)||!float.IsFinite(region.Confidence)||region.Confidence is <.85f or >1f||region.StrokeIndices.Count is <1 or >256)continue;
            var ids=region.StrokeIndices.ToArray();
            if(ids.Any(i=>i<0||i>=strokes.Count)||ids.Distinct().Count()!=ids.Length)continue;
            if(ids.Any(i=>byIndex[i] is not {} p||p.Category!=region.Category||p.Confidence<.85f)||ids.Min(i=>byIndex[i]!.Confidence)!=region.Confidence)continue;
            Array.Sort(ids);foreach(int i in ids)uses[i]++;plans.Add((region.Category,ids,region.Confidence));
        }
        var results=new List<AiInkLayout.Result>();
        foreach(var plan in plans)
        {
            token.ThrowIfCancellationRequested();if(plan.Indices.Any(i=>uses[i]!=1))continue;
            var members=plan.Indices.Select(i=>strokes[i]).ToArray();
            try
            {
            if(plan.Category==InkCategory.Math)
            {
                // One region is one formula; mixed color/fade stays ink instead
                // of splitting into independently admitted formula fragments.
                if(members.Select(s=>(s.Color,s.FadeSeconds)).Distinct().Count()!=1)continue;
                var decoded=AiExperimentalMathLayout.Recognize(members,math,token,calculate:false);
                if(decoded.Count!=1||!decoded[0].Strokes.SequenceEqual(members)||decoded[0].Admission!=AiInkLayout.ResultAdmission.UncalibratedMath)continue;
                var admitted=decoded[0] with {Admission=AiInkLayout.ResultAdmission.ExperimentalClassifiedMath,CategoryScore=plan.Score};
                AiMathLayout.ApplySolvedResult(admitted,token,solve);token.ThrowIfCancellationRequested();results.Add(admitted);
            }
            else
            {
                var decoded=AiInkLayout.Recognize(members,text,token);
                if(decoded.Count==0||decoded.Any(r=>r.Admission!=AiInkLayout.ResultAdmission.Scored||!float.IsFinite(r.Confidence)||r.Confidence<r.AcceptanceThreshold))continue;
                var actual=decoded.SelectMany(r=>r.Strokes).ToArray();
                if(actual.Length!=members.Length||actual.Distinct().Count()!=members.Length||actual.Any(s=>!members.Contains(s)))continue;
                foreach(var result in decoded)
                    if(AiAutomaticLayout.Mathematical(result.Text.Text)&&MathTypesetting.Supported(result.Text.Text))
                        AiMathLayout.ApplySolvedResult(result,token,solve);
                token.ThrowIfCancellationRequested();results.AddRange(decoded);
            }
            }
            catch(Exception e)when(e is InvalidDataException or IOException or ArgumentException or InvalidOperationException or NotSupportedException)
            {token.ThrowIfCancellationRequested();System.Diagnostics.Trace.WriteLine($"Experimental AUTO region: {e.Message}");}
        }
        return results;
    }
}
