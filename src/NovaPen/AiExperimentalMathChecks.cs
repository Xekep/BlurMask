using Avalonia;
using Avalonia.Media;
using InkRecognition;
namespace NovaPen;
// Prepared injected layout/route tests only; no model/user profile/GUI/solver.
internal static class AiExperimentalMathChecks
{
    internal static void Run()
    {
        int checks=0;void Need(bool yes){if(!yes)throw new InvalidOperationException("Experimental math layout guard failed: "+checks);checks++;}
        InkMark Stroke(int n=3)
        {var s=new InkMark(InkTool.Pen,Colors.Black,3,new(10,10));for(int i=1;i<n;i++)s.AddPoint(new(10+i%20,10+(i%2)*20));return s;}
        var ink=new[]{Stroke(),Stroke()};var before=ink.Select(InkDocument.ExportMark).ToArray();int calls=0;
        AiExperimentalMathLayout.Candidate? Decode(IReadOnlyList<InkStroke> input,CancellationToken token)
        {token.ThrowIfCancellationRequested();calls++;Need(input.Count==2&&input.Sum(s=>s.Points.Count)==6);return new("x^2",null,3);}
        var result=AiExperimentalMathLayout.Recognize(ink,Decode,calculate:false).Single();
        Need(calls==1&&float.IsNaN(result.Confidence)&&result.ConfidenceForAdmission is null&&
            result.Admission==AiInkLayout.ResultAdmission.UncalibratedMath&&float.IsPositiveInfinity(result.AcceptanceThreshold));
        Need(AiExperimentalMathLayout.CanApply(result,"math",true)&&!AiExperimentalMathLayout.CanApply(result,"auto",true)&&
            !AiExperimentalMathLayout.CanApply(result,"text",true)&&!AiExperimentalMathLayout.CanApply(result,"math",false));
        Need(!AiExperimentalMathLayout.Enabled("auto",true)&&!AiExperimentalMathLayout.Enabled("math",false));
        Need(ink.Select(InkDocument.ExportMark).Zip(before).All(p=>NovaPen.Shared.BoardValidation.Same(p.First,p.Second)));
        var originalPoint=ink[0].Points[0];
        _=AiExperimentalMathLayout.Recognize(ink,(input,_)=>{((InkPoint[])input[0].Points)[0]=new(999,999);return new("x",null,1);},calculate:false);
        Need(ink[0].Points[0]==originalPoint);
        // NaN is transient score-only: persistent/shared InkMark DTO contains only
        // finite geometry/style/text, never confidence/admission fields.
        var dto=InkDocument.ExportMark(result.Text);Need(NovaPen.Shared.BoardValidation.Valid(dto));
        var imported=new InkDocument();imported.Import([dto]);Need(imported.Marks.Single().Text=="x^2"&&NovaPen.Shared.BoardValidation.Valid(InkDocument.ExportMark(imported.Marks.Single())));
        var doc=new InkDocument();foreach(var s in ink)doc.Add(s);Need(doc.ReplaceRecognized(result.Strokes,result.Text));doc.Undo();Need(doc.Marks.SequenceEqual(ink));
        foreach(var bad in new AiExperimentalMathLayout.Candidate?[]{null,new(null,null,1),new("x",null,0),new("x","unsupported",1),new(@"\unsupported{x}",null,1)})
            Need(AiExperimentalMathLayout.Recognize(ink,(_,_)=>bad,calculate:false).Count==0);
        using(var c=new CancellationTokenSource())
        {
            c.Cancel();try{AiExperimentalMathLayout.Recognize(ink,Decode,c.Token);throw new Exception("Pre-cancel admitted.");}
            catch(OperationCanceledException e){Need(e.CancellationToken==c.Token&&c.IsCancellationRequested);}
        }
        using(var c=new CancellationTokenSource())
        {
            bool reached=false;try{AiExperimentalMathLayout.Recognize(ink,(_,_)=>{reached=true;c.Cancel();return new("x",null,1);},c.Token);throw new Exception("Post-cancel admitted.");}
            catch(OperationCanceledException e){Need(reached&&e.CancellationToken==c.Token&&c.IsCancellationRequested);}
        }
        int boundaryCalls=0;
        Need(AiExperimentalMathLayout.Recognize([Stroke(2048),Stroke(2048)],(_,_)=>{boundaryCalls++;return new("x",null,1);},calculate:false).Count==1&&boundaryCalls==1);
        Need(AiExperimentalMathLayout.Recognize([Stroke(2048),Stroke(2049)],(_,_)=>{boundaryCalls++;return new("x",null,1);},calculate:false).Count==0&&boundaryCalls==1);
        int solves=0;
        Need(AiExperimentalMathLayout.Recognize(ink,(_,_)=>new("x=2",null,1),solve:(_,_)=>{solves++;return null;}).Count==1&&solves==0);
        var request=AiExperimentalMathLayout.Recognize(ink,(_,_)=>new("x^2=",null,1),solve:(_,_)=>{solves++;return null;}).Single();Need(solves==1&&request.MathCalculationFailed);
        foreach(string language in new[]{"en","fr","de"})Need(UiText.Translate("Формулы · экспериментально",language)!="Формулы · экспериментально");
        // Auto never resolves the new bundled owner. Both decoders explicitly
        // injected here; no SettingsStore, installed assets or user models read.
        int mathCalls=0;var auto=AiAutomaticLayout.Recognize(ink,new Text(),(_,_)=>{mathCalls++;throw new Exception("Unnecessary math.");});
        Need(auto.Count>0&&mathCalls==0&&auto.All(r=>r.Admission==AiInkLayout.ResultAdmission.Scored));
        Console.WriteLine($"Experimental math prepared layout/route: {checks} checks; no NN/profile/solver/GUI.");
    }
    private sealed class Text:IStrokeTextRecognizer
    {public string Language=>"en";public TextRecognition Recognize(IReadOnlyList<InkStroke> ink,CancellationToken token=default){token.ThrowIfCancellationRequested();return new("hello",.99f,6);}}
    // Explicit actual ink probe: no expected label, calibration or solver output.
    internal static void RunActual(string path)
    {
        try
        {
            var file=new FileInfo(path);if(file.Length is <1 or >4*1024*1024)throw new InvalidDataException("Bounded raw ink fixture required.");
            var ink=AiPenProbe.ReadStrokes(path);var result=AiExperimentalMathLayout.Recognize(ink,calculate:false);
            if(result.Any(r=>r.Admission!=AiInkLayout.ResultAdmission.UncalibratedMath||!float.IsNaN(r.Confidence)||r.ConfidenceForAdmission is not null||!NovaPen.Shared.BoardValidation.Valid(InkDocument.ExportMark(r.Text))))
                throw new InvalidOperationException("Actual experimental math admission/export differs.");
            Console.WriteLine($"Actual experimental manual math: {result.Count} candidates; confidence=null; "+string.Join(" / ",result.Select(r=>r.Text.Text)));
        }
        finally{AiExperimentalMathModels.Release();}
    }
}
