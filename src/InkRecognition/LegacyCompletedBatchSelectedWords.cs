namespace InkRecognition;

//Detached consumer DATA only. No record/range/string produced here can issue
//a source, WORD/TEXT context, query or replacement live owner.
internal sealed class LegacyCompletedBatchSelectedWords
{
    internal readonly record struct Glyph(ulong Reference,byte[] Record40,byte[] Utf8);
    internal readonly record struct Word(int SourceSelectedIndex,int PayloadSelectedIndex,int SourceChoiceCount,int ChoiceCount,
        LegacyNativeInkPositionRange[] OriginalRanges,Glyph[] Glyphs);
    internal Word[] Words{get;}
    internal LegacyNativeInkPositionRange[] OriginalCoverage{get;}
    internal bool FullCoverage{get;}
    private LegacyCompletedBatchSelectedWords(Word[] words,LegacyNativeInkPositionRange[] coverage,bool full)
    {Words=words;OriginalCoverage=coverage;FullCoverage=full;}

    internal static LegacyCompletedBatchSelectedWords Read(LegacyFreshMainGlobalWordTextPublication actual,
        LegacyFreshMainGlobalPlainRecognitionResult original,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actual);ArgumentNullException.ThrowIfNull(original);
        token.ThrowIfCancellationRequested();var binding=actual.Wrapper.ActualCompletion;binding.Validate(token);
        if(!ReferenceEquals(binding.Source,original)||!actual.IsComplete||actual.IsFaulted||
            !actual.PublishesAllWordChoices||actual.AlternativeCount!=1||original.Session is not{} session)
            throw new ArgumentException("Same completed batch all-WORD publication and original source required.");
        actual.RequireCompletedOrdinaryPostText(token);
        int n=actual.CompletedWordUseCount(token);if(n is<1 or>4096)throw new NotSupportedException("Bounded complete WORD use vector required.");
        int glyphs=0,rangeRows=0;
        for(int i=0;i<n;i++)
        {
            var use=actual.ReadCompletedWordUse(i,token);
            int count=use.SelectedChoice(token).LinkCount,ranges=use.OriginalRangeCount(token);
            if(count<1||count>128-glyphs||ranges<1||ranges>65536-rangeRows)
                throw new NotSupportedException("Whole selected WORD/glyph/range extent exceeds public consumer subset.");
            glyphs+=count;rangeRows+=ranges;
        }
        //The existing exact union/full-input predicate remains; do not equate
        //NULL-root TEXT coverage with actual WORD consumed-input coverage.
        LegacyFreshMainGlobalDocumentCoverage.PreflightUnionWork(rangeRows);
        var doc=actual.Scope.Document;var operations=LegacyOwnedRawInkRangeOperations.Prepare(doc);
        var words=new Word[n];LegacyNativeInkPositionRange[] coverage=[];int utf8Bytes=0;
        for(int i=0;i<n;i++)
        {
            token.ThrowIfCancellationRequested();var use=actual.ReadCompletedWordUse(i,token);
            var key=use.CopyOriginalRanges(token);coverage=operations.Union(coverage,key,token);
            var links=use.SelectedChoice(token).CopyCharacterLinks();
            var output=new Glyph[links.Count];
            for(int j=0;j<links.Count;j++)
            {
                token.ThrowIfCancellationRequested();var choice=links[j].Choice;var source=choice.ProcessedSource;
                if(!ReferenceEquals(source.Owner,actual.Wrapper)||!source.IsOrdinarySource||
                    !ReferenceEquals(source.Document,doc)||source.CopyRanges(token).Length==0||
                    !ReferenceEquals(source.OriginalPositions.Source.Owner,session.Associations.Metadata))
                    throw new NotSupportedException("Selected CHAR must retain the same source document/metadata/pool.");
                //A reused WORD choice may deliberately retain FIRST-owner
                //glyph references. They need not equal current TOP.FlatRefs;
                //the private per-use payload/source/index binding proves it.
                byte[] row=session.Ownership.GlyphPool.CopyRecord(source.Reference,token);
                byte[] bytes=choice.CopySelectedUtf8(token);
                if(bytes.Length>1024-utf8Bytes)throw new NotSupportedException("Whole selected UTF8 payload exceeds public consumer bound.");
                utf8Bytes+=bytes.Length;output[j]=new(source.Reference,row,bytes);
            }
            words[i]=new(use.SourceSelectedIndex(token),use.PayloadSelectedIndex(token),use.SourceChoiceCount(token),use.PayloadChoiceCount(token),key,output);
        }
        actual.CompletedWordUseCount(token); //Final same-owner/node/selected validation.
        bool full=ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,coverage,token);
        return new(words,coverage,full);
    }
}
