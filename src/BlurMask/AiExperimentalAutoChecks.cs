using Avalonia.Media;
using InkRecognition;
using InkRecognition.RawContent;
namespace BlurMask;

// Injected routing tests: no model load, NN, settings install, GUI or CAS process.
internal static class AiExperimentalAutoChecks
{
    internal static void Run(string? packPath=null)
    {
        var names=new List<string>();void Need(bool value,string name){if(!value)throw new InvalidOperationException("Experimental AUTO: "+name);names.Add(name);}
        InkMark Stroke(int n=3,Color? color=null,int fade=0){var s=new InkMark(InkTool.Pen,color??Colors.Black,3,new(10,10)){FadeSeconds=fade};for(int i=1;i<n;i++)s.AddPoint(new(10+i%20,10+i%2*20));return s;}
        var ink=new[]{Stroke(),Stroke(4)};int calls=0,solves=0;
        RawContentResult Classification(InkCategory category=InkCategory.Math,float score=.9f,int[]? ids=null)=>
            new(Enumerable.Range(0,ink.Length).Select(i=>new InkClassification(i,category,score)).ToArray(),[new(category,ids??[1,0],score)]);
        AiExperimentalMathLayout.Candidate? Decode(IReadOnlyList<InkStroke> input,CancellationToken token)
        {token.ThrowIfCancellationRequested();calls++;Need(input.Count==2&&input[0].Points.Count==3&&input[1].Points.Count==4,"decoder-exact-members-"+calls);return new("x^2",null,3);}
        IReadOnlyList<AiInkLayout.Result> Route(RawContentResult? classification,AiExperimentalMathLayout.Candidate? candidate=null)=>
            AiExperimentalAutoLayout.Recognize(ink,(_,_)=>classification,(_,_)=>{calls++;return candidate??new("x^2",null,3);},solve:(_,_)=>{solves++;return null;});
        var before=ink.Select(InkDocument.ExportMark).ToArray();
        var result=AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(),Decode,solve:(_,_)=>{solves++;return null;}).Single();
        Need(result.Admission==AiInkLayout.ResultAdmission.ExperimentalClassifiedMath&&float.IsNaN(result.Confidence)&&result.ConfidenceForAdmission is null&&result.CategoryScore==.9f,"null-formula-distinct-category");
        Need(result.Strokes.SequenceEqual(ink)&&solves==0,"chronology-and-no-implicit-CAS");
        Need(AiExperimentalMathLayout.CanApply(result,"auto",true)&&!AiExperimentalMathLayout.CanApply(result,"auto",false)&&!AiExperimentalMathLayout.CanApply(result,"math",true)&&!AiExperimentalMathLayout.CanApply(result,"text",true),"mode-setting-publication-gate");
        Need(!AiExperimentalMathLayout.CanApply(result,"auto",true,true),"installed-override-before-publication");
        foreach(string mode in new[]{"auto","text","math"})foreach(bool experimental in new[]{false,true})foreach(bool installed in new[]{false,true})
            Need(AiExperimentalAutoLayout.Enabled(mode,experimental,installed)==(mode=="auto"&&experimental&&!installed),$"route-policy-{mode}-{experimental}-{installed}");
        var manual=result with{Admission=AiInkLayout.ResultAdmission.UncalibratedMath,CategoryScore=null};
        Need(AiExperimentalMathLayout.CanApply(manual,"math",true)&&!AiExperimentalMathLayout.CanApply(manual,"auto",true),"manual-admission-remains-separate");
        Need(ink.Select(InkDocument.ExportMark).Zip(before).All(p=>BlurMask.Shared.BoardValidation.Same(p.First,p.Second)),"original-ink-unchanged");
        var doc=new InkDocument();var batch=new AiInkBatch(doc);foreach(var s in ink){doc.Add(s);batch.Add(s);}
        Need(batch.Apply(result)&&batch.Snapshot().SequenceEqual(ink),"live-original-association");doc.Undo();Need(doc.Marks.SequenceEqual(ink),"undo-restores-original-members");
        var dto=InkDocument.ExportMark(result.Text);Need(BlurMask.Shared.BoardValidation.Valid(dto),"peer-DTO-finite");var imported=new InkDocument();imported.Import([dto]);Need(imported.Marks.Single().Text=="x^2","peer-DTO-roundtrip");
        int c=calls;Need(Route(null).Count==0&&calls==c,"classifier-null-keeps-ink");
        Need(Route(Classification(InkCategory.Shape)).Count==0&&calls==c,"drawing-keeps-ink");
        foreach(float score in new[]{.849f,float.NaN,float.PositiveInfinity,1.01f})Need(Route(Classification(score:score)).Count==0&&calls==c,"invalid-category-score-"+score.ToString(System.Globalization.CultureInfo.InvariantCulture));
        foreach(var ids in new[]{new[]{0,0},new[]{0,2},new[]{-1,1},Array.Empty<int>()})Need(Route(Classification(ids:ids)).Count==0&&calls==c,"invalid-members-"+string.Join(',',ids));
        var overlap=Classification() with{Regions=[new(InkCategory.Math,[0],.9f),new(InkCategory.Math,[0,1],.9f)]};
        Need(Route(overlap).Count==0&&calls==c,"all-overlapping-regions-refused");
        Need(Route(Classification() with{Strokes=[new(0,InkCategory.Math,.9f),new(0,InkCategory.Math,.9f)]}).Count==0&&calls==c,"duplicate-prediction-refused");
        Need(Route(Classification() with{Strokes=[new(0,InkCategory.Math,.9f),new(1,InkCategory.Text,.9f)]}).Count==0&&calls==c,"category-membership-refused");
        Need(Route(Classification() with{Regions=[new(InkCategory.Math,[0,1],.95f)]}).Count==0&&calls==c,"region-score-not-relabelled");
        var second=ink[1];ink[1]=Stroke(4,Colors.Red);Need(Route(Classification()).Count==0&&calls==c,"mixed-color-whole-formula-refused");ink[1]=second;
        ink[1]=Stroke(4,fade:3);Need(Route(Classification()).Count==0&&calls==c,"mixed-fade-whole-formula-refused");ink[1]=second;
        foreach(var candidate in new[]{new AiExperimentalMathLayout.Candidate("x","unsupported",1),new("x",null,0),new(@"\unsupported{x}",null,1)})Need(Route(Classification(),candidate).Count==0,"unsupported-syntax-keeps-ink-"+names.Count);
        Need(AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(),(_,_)=>throw new InvalidDataException("decoder unavailable")).Count==0,"failed-region-keeps-ink");
        _=AiExperimentalAutoLayout.Recognize(ink,(input,_)=>{((InkPoint[])input[0].Points)[0]=new(999,999);return Classification();},(_,_)=>new("x",null,1));
        Need(ink[0].Points[0].X==10,"classifier-owned-input-snapshot");
        var mixed=new RawContentResult([new(0,InkCategory.Math,.9f),new(1,InkCategory.Text,.9f)],[new(InkCategory.Text,[1],.9f),new(InkCategory.Math,[0],.9f)]);
        var mixedResults=AiExperimentalAutoLayout.Recognize(ink,(_,_)=>mixed,(_,_)=>new("x",null,1),new Text());
        Need(mixedResults.Count==2&&mixedResults.Single(r=>r.Admission==AiInkLayout.ResultAdmission.ExperimentalClassifiedMath).Strokes.SequenceEqual([ink[0]])&&mixedResults.Single(r=>r.Admission==AiInkLayout.ResultAdmission.Scored).Strokes.SequenceEqual([ink[1]]),"mixed-text-math-regions-exact-members");
        var bounded=new[]{Stroke(2048),Stroke(2048)};
        Need(AiExperimentalAutoLayout.Recognize(bounded,(_,_)=>Classification(),(_,_)=>new("x",null,1)).Count==1,"4096-point-region-admitted");
        bounded=[Stroke(2048),Stroke(2049)];Need(AiExperimentalAutoLayout.Recognize(bounded,(_,_)=>Classification(),(_,_)=>throw new Exception("Oversized decoder entry")).Count==0,"4097-point-region-refused-before-decode");
        Need(AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(),(_,_)=>new("x^2=",null,3),solve:(_,_)=>{solves++;return null;}).Single().MathCalculationFailed&&solves==1,"trailing-equals-only-after-admission");
        _=AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(),(_,_)=>new("x=2",null,3),solve:(_,_)=>{solves++;return null;});Need(solves==1,"written-RHS-not-calculated");
        Need(AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(InkCategory.Text),(_,_)=>throw new Exception("Text went to math"),new Text()).All(r=>r.Admission==AiInkLayout.ResultAdmission.Scored),"text-region-remains-text");
        var numericText=AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(InkCategory.Text),(_,_)=>throw new Exception("Numeric text went to Math2"),new Text("1+2="),solve:(source,_)=>source=="1+2"?new("3",false):throw new Exception("Unexpected CAS input")).Single();
        Need(numericText.Text.Text=="1+2=3"&&numericText.Admission==AiInkLayout.ResultAdmission.Scored,"scored-numeric-text-calculation-preserved");
        using(var cancel=new CancellationTokenSource())
        {cancel.Cancel();try{AiExperimentalAutoLayout.Recognize(ink,(_,_)=>throw new Exception("Cancelled classifier entry"),token:cancel.Token);throw new Exception("Cancellation ignored");}catch(OperationCanceledException e){Need(e.CancellationToken==cancel.Token,"pre-cancel-token");}}
        using(var cancel=new CancellationTokenSource())
        {bool reached=false;try{AiExperimentalAutoLayout.Recognize(ink,(_,_)=>Classification(),(_,_)=>{reached=true;cancel.Cancel();return new("x^2=",null,3);},token:cancel.Token,solve:(_,_)=>throw new Exception("Cancelled CAS entry"));throw new Exception("Cancellation ignored");}catch(OperationCanceledException e){Need(reached&&e.CancellationToken==cancel.Token,"post-decode-cancel-before-admission");}}
        using(var cancel=new CancellationTokenSource())
        {bool reached=false;try{AiExperimentalAutoLayout.Recognize(ink,(_,_)=>{reached=true;cancel.Cancel();return Classification();},(_,_)=>throw new Exception("Cancelled decoder entry"),token:cancel.Token);throw new Exception("Cancellation ignored");}catch(OperationCanceledException e){Need(reached&&e.CancellationToken==cancel.Token,"post-classifier-cancel-before-decode");}}
        if(packPath is not null)
        {
            var bytes=File.ReadAllBytes(packPath);AiExperimentalRawContentModels.ValidatePack(bytes);Need(bytes.Length==AiExperimentalRawContentModels.PackBytes,"exact-bundle-static-validation");
            bytes[72]^=1;try{AiExperimentalRawContentModels.ValidatePack(bytes);throw new Exception("Corrupt bundle admitted");}catch(InvalidDataException){Need(true,"corrupt-bundle-before-ORT");}
            try{AiExperimentalRawContentModels.ValidatePack(bytes.AsSpan(1));throw new Exception("Truncated bundle admitted");}catch(InvalidDataException){Need(true,"truncated-bundle-before-ORT");}
        }
        if(names.Distinct().Count()!=names.Count)throw new Exception("Duplicate test name");
        foreach(string name in names)Console.WriteLine("PASS "+name);
        Console.WriteLine($"Experimental AUTO prepared: {names.Count} guards; confidence=null; no NN/vendor/GUI/CAS process.");
    }
    private sealed class Text(string value="hello"):IStrokeTextRecognizer
    {public string Language=>"en";public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes,CancellationToken token=default)=>new(value,.99f,6);}

    // Actual diagnostic entry only; never called by prepared checks. Activation
    // requires separately reviewed bundled assets and inference authorization.
    internal static void RunActual(string path)
    {
        var previous=Program.Settings;
        try
        {
            Program.Settings=previous with{ExperimentalTextEngine=true,AiMode="auto",GiacEnabled=false};
            var file=new FileInfo(path);if(file.Length is <1 or >4*1024*1024)throw new InvalidDataException("Bounded ink fixture required.");
            var ink=AiPenProbe.ReadStrokes(path);RawContentResult? classification=null;int classifierCalls=0,solverRequests=0;
            var candidates=new List<AiExperimentalMathLayout.Candidate?>();
            var results=AiExperimentalAutoLayout.Recognize(ink,(input,token)=>
                {classifierCalls++;return classification=AiExperimentalRawContentModels.Analyze(input,token);},
                (input,token)=>{var actual=AiExperimentalMathModels.Recognize(input,token);candidates.Add(actual);return actual;},
                solve:(_,_)=>{solverRequests++;return null;});
            using var bytes=new MemoryStream();
            using(var w=new System.Text.Json.Utf8JsonWriter(bytes,new(){Indented=true}))
            {
                w.WriteStartObject();w.WriteString("scope","Actual experimental AUTO; no expected labels; CAS process disabled");
                w.WriteNumber("sourceStrokes",ink.Count);w.WriteNumber("classifierCalls",classifierCalls);w.WriteBoolean("classifierSucceeded",classification is not null);
                w.WriteStartArray("strokeClassifications");
                if(classification is not null)foreach(var p in classification.Strokes)
                {w.WriteStartObject();w.WriteNumber("index",p.StrokeIndex);w.WriteString("category",p.Category.ToString());if(float.IsFinite(p.Confidence))w.WriteNumber("categoryScore",p.Confidence);else w.WriteNull("categoryScore");w.WriteEndObject();}
                w.WriteEndArray();w.WriteStartArray("regions");
                if(classification is not null)foreach(var region in classification.Regions)
                {w.WriteStartObject();w.WriteString("category",region.Category.ToString());if(float.IsFinite(region.Confidence))w.WriteNumber("categoryScore",region.Confidence);else w.WriteNull("categoryScore");w.WriteStartArray("indices");foreach(int i in region.StrokeIndices)w.WriteNumberValue(i);w.WriteEndArray();w.WriteEndObject();}
                w.WriteEndArray();w.WriteStartArray("mathCandidates");
                foreach(var candidate in candidates)
                {w.WriteStartObject();w.WriteString("recognizedLaTex",candidate?.LaTex);w.WriteString("unsupportedReason",candidate?.UnsupportedReason);w.WriteNumber("syntaxNodes",candidate?.SyntaxNodes??0);w.WriteNull("formulaConfidence");w.WriteEndObject();}
                w.WriteEndArray();w.WriteStartArray("admittedResults");
                foreach(var result in results)
                {
                    w.WriteStartObject();w.WriteString("admission",result.Admission.ToString());w.WriteString("renderedText",result.Text.Text);w.WriteString("recognizedSizingText",result.SizingText);
                    if(result.CategoryScore is {} score)w.WriteNumber("categoryScore",score);else w.WriteNull("categoryScore");
                    if(result.ConfidenceForAdmission is {} confidence)w.WriteNumber("recognitionConfidence",confidence);else w.WriteNull("recognitionConfidence");
                    w.WriteBoolean("validPeerDTO",BlurMask.Shared.BoardValidation.Valid(InkDocument.ExportMark(result.Text)));
                    w.WriteStartArray("sourceIndices");foreach(var source in result.Strokes)w.WriteNumberValue(ink.IndexOf(source));w.WriteEndArray();w.WriteEndObject();
                }
                w.WriteEndArray();w.WriteNumber("suppressedCASRequests",solverRequests);w.WriteEndObject();
            }
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(bytes.ToArray()));
        }
        finally{Program.Settings=previous;AiExperimentalRawContentModels.Release();AiExperimentalMathModels.Release();}
    }
}
