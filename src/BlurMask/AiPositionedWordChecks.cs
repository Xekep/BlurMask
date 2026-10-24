using Avalonia;
using Avalonia.Media;
using InkRecognition;
namespace BlurMask;

//Prepared/mock DATA checks only. No model/Scope/source issuer/GUI/profile.
internal static class AiPositionedWordChecks
{
    internal static void Run()
    {
        int checks=0;
        var ink=Stroke();var model=new Mock();
        var result=AiInkLayout.Recognize([ink],model);
        Need(model.Calls==1&&result.Count==1&&result[0].PositionedWords is{Length:2},"one whole input call/two positioned words");
        var words=result[0].PositionedWords!;
        Need(words.Select(w=>w.Mark.Text).SequenceEqual(["да","нет"]),"ordered words without generated spaces");
        Need(words[0].SourceBounds==new Rect(0,0,10,10)&&words[1].SourceBounds==new Rect(10,0,10,10),"actual per-range bounds, not last glyph or joined box");
        Need(words.All(w=>w.Line.XHeight==8&&w.Line.OriginY==15),"prepared line geometry retained");
        var document=new InkDocument();document.Add(ink);var batch=new AiInkBatch(document);batch.Add(ink);
        Need(batch.Apply(result[0])&&document.Marks.SequenceEqual(words.Select(w=>w.Mark)),"atomic multiword replacement of shared original stroke");
        Need(batch.Snapshot().SequenceEqual([ink]),"one source generation retained behind all words");
        document.Undo();Need(document.Marks.SequenceEqual([ink]),"single undo restores whole ink");
        document.Redo();Need(document.Marks.SequenceEqual(words.Select(w=>w.Mark)),"single redo restores whole word vector");
        words[1].Mark.Text="edited";Need(batch.Snapshot().Length==0&&!batch.Apply(result[0]),"one edited word revokes entire retained generation");
        var transformed=Stroke().WithTransform(Matrix.CreateScale(2,2)*Matrix.CreateTranslation(-50,25));
        var moved=AiInkLayout.Recognize([transformed],new Mock());
        Need(moved.Count==1&&moved[0].PositionedWords![0].SourceBounds==new Rect(-50,25,20,20)&&
             moved[0].PositionedWords![1].SourceBounds==new Rect(-30,25,20,20),"same transformed input coordinates used for inference and range placement");
        foreach(var failure in new[]{"low","whitespace","coverage","bounds","flat","flag","control","height","range"})
        {Need(AiInkLayout.Recognize([Stroke()],new Mock(failure)).Count==0,"whole expression refusal: "+failure);}
        using var cancel=new CancellationTokenSource();cancel.Cancel();
        try{AiInkLayout.Recognize([Stroke()],new Mock(),cancel.Token);throw new Exception("missing cancel");}
        catch(OperationCanceledException){checks++;}
        //Same input scalar producer for fractional boundaries, non-extreme tail
        //and explicit invalid extents. These are geometry DATA, not WORD proof.
        InkStroke[] input=[new InkStroke([new(0,0),new(10,20),new(20,0)])];
        var fractional=new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(0,.5f,3,3),LegacyNativeInkPosition.Pack(0,1.5f,5,3));
        Need(RecognizedWordInkBounds.FromOriginalRanges(input,[fractional])==new RecognizedInkBounds(5,10,15,20),"fractional source boundaries");
        var counted=new CountingPoints([new(0,0),new(10,20),new(20,0)]);
        InkStroke[] countedInput=[new InkStroke(counted)];
        try{RecognizedWordInkBounds.Project(countedInput,[fractional],4);throw new Exception("missing exhaustion");}
        catch(NotSupportedException){Need(counted.Reads==0,"known point work debited before first point read");}
        var projected=RecognizedWordInkBounds.Project(countedInput,[fractional],5);
        Need(projected.PointReads==5&&counted.Reads==5,"exact fractional point-indexer count");
        var fading=Stroke(10);var fadeResult=AiInkLayout.Recognize([fading],new Mock()).Single();
        var fadeDoc=new InkDocument();fadeDoc.Add(fading);var fadeBatch=new AiInkBatch(fadeDoc);fadeBatch.Add(fading);
        Need(fadeBatch.Apply(fadeResult)&&fadeDoc.FadingMarks.Count==2,"fading group replaces source as one generation");
        fadeDoc.Undo();Need(fadeDoc.FadingMarks.Count==0&&fadeBatch.Snapshot().Length==0,"one fading Undo removes all WORD peers");
        var expiry=Stroke(10);var expiryResult=AiInkLayout.Recognize([expiry],new Mock()).Single();
        var expiryDoc=new InkDocument();expiryDoc.Add(expiry);var expiryBatch=new AiInkBatch(expiryDoc);expiryBatch.Add(expiry);
        Need(expiryBatch.Apply(expiryResult),"fresh fading generation for expiry");
        expiryDoc.FadingMarks.Remove(expiryResult.PositionedWords![0].Mark);
        expiryDoc.ExpireFading(System.Diagnostics.Stopwatch.GetTimestamp());
        Need(expiryDoc.FadingMarks.Count==0&&expiryBatch.Snapshot().Length==0,"missing/expired peer revokes whole fading group");
        var oldFades=new InkDocument();oldFades.Add(Stroke(10));oldFades.Add(Stroke(10));oldFades.Undo();
        Need(oldFades.FadingMarks.Count==1,"unrelated old singleton fading Undo unchanged");
        var replacedInk=Stroke();var replaced=AiInkLayout.Recognize([replacedInk],new Mock()).Single();
        var replacedDoc=new InkDocument();replacedDoc.Add(replacedInk);var replacedBatch=new AiInkBatch(replacedDoc);replacedBatch.Add(replacedInk);
        Need(replacedBatch.Apply(replaced),"fresh generation for replaced-reference test");
        replacedDoc.Marks[1]=replacedDoc.Marks[1].WithTransform(Matrix.CreateTranslation(20,0));
        Need(replacedBatch.Snapshot().Length==0&&!replacedBatch.Apply(replaced),"transformed/import-style replacement revokes entire original group");
        AiInkBatchChecks.Run();
        Console.WriteLine($"PASS {checks} positioned WORD prepared/mock checks + existing AiInkBatchChecks; no NN/GUI/profile");
        void Need(bool pass,string message){if(!pass)throw new InvalidOperationException(message);checks++;}
    }
    private sealed class CountingPoints(InkPoint[] points):IReadOnlyList<InkPoint>
    {
        internal int Reads;public int Count=>points.Length;
        public InkPoint this[int index]{get{Reads++;return points[index];}}
        public IEnumerator<InkPoint> GetEnumerator()=>((IEnumerable<InkPoint>)points).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
    }
    private static InkMark Stroke(double fade=0){var m=new InkMark(InkTool.Pen,Colors.Black,1,new(0,0)){FadeSeconds=fade};m.AddPoint(new(10,10));m.AddPoint(new(20,0));return m;}
    private sealed class Mock(string? failure=null):IPositionedStrokeTextRecognizer
    {
        public string Language=>"prepared";public bool ProducesPositionedWordRuns=>true;internal int Calls;
        public TextRecognition Recognize(IReadOnlyList<InkStroke> input,CancellationToken token=default)
        {
            Calls++;token.ThrowIfCancellationRequested();var line=new LegacyCompletedGlobalTextWords.PreparedLine(0,0,15,0,8,-1,15);
            if(failure=="height")line=line with{XHeight=0};
            var ranges=new[]{new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(0,0,3,3),LegacyNativeInkPosition.Pack(0,1,5,3)),
                new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(0,1,3,3),LegacyNativeInkPosition.Pack(0,2,5,3))};
            if(failure=="coverage")ranges[1]=ranges[1] with{Last=LegacyNativeInkPosition.Pack(0,1.5f,5,3)};
            var runs=new PositionedWordRun[2];
            for(int i=0;i<2;i++)
            {
                var b=RecognizedWordInkBounds.FromOriginalRanges(input,[ranges[i]],token);
                if(failure=="bounds"&&i==1)b=b with{Left=b.Left+1};
                var r=ranges[i];if(failure=="range"&&i==1)r=r with{Last=new(9UL<<32|0x2ff05)};
                string text=i==0?"да":failure=="whitespace"?" ":failure=="control"?"\u200b":"нет";
                runs[i]=new(text,i==1&&failure=="low"?.49f:.8f,b,line,[r]);
            }
            return new(failure=="flat"?"да нет":"",runs.Min(r=>r.CandidateScore),2)
                {ExperimentalCandidateScore=true,WordRuns=Array.AsReadOnly(runs),WordRunsCoverFullInput=failure!="flag"};
        }
    }
}
