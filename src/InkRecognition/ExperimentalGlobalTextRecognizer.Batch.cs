using System.Buffers.Binary;
using System.Text;
namespace InkRecognition;

public sealed partial class ExperimentalGlobalTextRecognizer
{
    private TextRecognition ReadCompletedBatch(LegacyFreshMainGlobalWordTextPublication parent,
        LegacyFreshMainGlobalPlainRecognitionResult source,InkStroke[] input,ExperimentalCompletedTextDiagnostic? diagnostic,
        CancellationToken token)
    {
        var selected=LegacyCompletedBatchSelectedWords.Read(parent,source,token);
        if(diagnostic is not null)
        {
            diagnostic.Stage="coverage";diagnostic.FullCoverage=selected.FullCoverage;
            diagnostic.WordOriginalRanges=selected.OriginalCoverage;
            diagnostic.FullInputRanges=FullOriginalInput(parent.Scope.Document,token);
            diagnostic.SelectedTopCount=selected.Words.Length;
            diagnostic.SourceSelectedOrdinals=selected.Words.Select(w=>w.SourceSelectedIndex).ToArray();
            diagnostic.PayloadSelectedOrdinals=selected.Words.Select(w=>w.PayloadSelectedIndex).ToArray();
            diagnostic.Source275Counts=selected.Words.Select(w=>w.SourceChoiceCount).ToArray();
            diagnostic.ProcessedChoiceCounts=selected.Words.Select(w=>w.ChoiceCount).ToArray();
            diagnostic.Actual275PayloadCount=diagnostic.Source275Counts.Sum();
            diagnostic.AllWordChoicesPublished=parent.PublishesAllWordChoices;
            diagnostic.PublishedWordChoiceCount=diagnostic.ProcessedChoiceCounts.Sum();
            diagnostic.SelectedWordOrdinal=selected.Words.Length==1?selected.Words[0].SourceSelectedIndex:-1;
            diagnostic.AllWordEnumeratedArrayEnvelopeBytes=parent.AllWordEnumeratedArrayEnvelopeBytes;
        }
        if(!selected.FullCoverage)
        {if(diagnostic is not null)diagnostic.Reason="partial-original-WORD-coverage";return Empty();}
        int selectedCharacters=0;float score=1;
        foreach(var word in selected.Words)
        foreach(var glyph in word.Glyphs)
        {
            token.ThrowIfCancellationRequested();string value=Encoding.UTF8.GetString(glyph.Utf8);
            if(diagnostic is not null)
            {
                diagnostic.Stage="glyph-host-policy";byte[] captured=glyph.Utf8;
                if(captured.Length>1024-diagnostic.CapturedUtf8Bytes){captured=[];diagnostic.Utf8CaptureTruncated=true;}
                diagnostic.CapturedUtf8Bytes+=captured.Length;
                diagnostic.Glyphs.Add(new(glyph.Reference,glyph.Record40,captured));
            }
            if(!TryAdmitSelectedGlyph(glyph.Record40,value,out float candidate))
            {
                if(diagnostic is not null)
                {
                    float native=BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(glyph.Record40.AsSpan(16)));
                    diagnostic.Reason=!float.IsFinite(native)||native<0||native>1?"glyph-score-outside-native-domain":"glyph-kind-or-text-policy";
                }
                return Empty();
            }
            if(value.Length>1024-selectedCharacters){if(diagnostic is not null)diagnostic.Reason="output-length";return Empty();}
            selectedCharacters+=value.Length;score=Math.Min(score,candidate);
        }
        var structured=LegacyCompletedGlobalTextWords.FromDetached(selected,parent.CopySourceLines56());
        var runs=new PositionedWordRun[structured.Words.Count];long remainingPointReads=RecognizedWordInkBounds.MaximumWholeRunPointReads;
        for(int i=0;i<runs.Length;i++)
        {
            token.ThrowIfCancellationRequested();var word=structured.Words[i];var ranges=word.CopyOriginalRanges();
            var projection=RecognizedWordInkBounds.Project(input,ranges,remainingPointReads,token);remainingPointReads-=projection.PointReads;
            runs[i]=new(word.SelectedUtf8,word.CandidateScore,projection.Bounds,word.Line,ranges);
        }
        if(source.Selection.Publication.Policy.Owner.AcousticEvaluations!=source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("Batch TEXT must retain the same acoustic evaluations.");
        token.ThrowIfCancellationRequested();
        int nodes=source.Selection.Publication.CopyNodeStarts2478().Length-1;
        if(diagnostic is not null){diagnostic.Stage="admitted";diagnostic.Reason="accepted";}
        return new(runs.Length==1?runs[0].Text:"",score,nodes){ExperimentalCandidateScore=true,NativeSelectedText=true,WordRuns=Array.AsReadOnly(runs),WordRunsCoverFullInput=true};
    }
}
