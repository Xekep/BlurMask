namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    internal int CompletedWordUseCount(CancellationToken token)
    {
        Scope.Validate();token.ThrowIfCancellationRequested();
        if(!completed||faulted||Wrapper.IsFaulted||Wrapper.Choices.IsFaulted||batchWords is null||
            publishedWords.Count!=batchWords.Count||publishedSelectedIndices.Count!=publishedWords.Count||
            publishedOriginalSources?.Length!=publishedWords.Count||publishedSourceSelectedIndices?.Length!=publishedWords.Count)
            throw new InvalidOperationException("Entire healthy same-owner batch WORD/TEXT publication required.");
        batchWords.Require(token);return publishedWords.Count;
    }
    internal CompletedWordUse ReadCompletedWordUse(int ordinal,CancellationToken token)
    {
        int count=CompletedWordUseCount(token);
        if((uint)ordinal>=count)throw new ArgumentOutOfRangeException(nameof(ordinal));
        var use=new CompletedWordUse(this,ordinal);use.Require(token);return use;
    }
    //No constructor/array/reference supplied by a caller is accepted as a
    //source issuer. Default struct is unusable. Every access binds the exact
    //genuine node, per-link source receipt and shared payload selected slot.
    internal readonly struct CompletedWordUse
    {
        private readonly LegacyFreshMainGlobalWordTextPublication parent;
        internal int Ordinal{get;}
        internal CompletedWordUse(LegacyFreshMainGlobalWordTextPublication parent,int ordinal)
        {this.parent=parent;Ordinal=ordinal;}
        internal void Require(CancellationToken token)
        {
            if(parent is null)throw new InvalidOperationException("Private completed WORD use required.");
            int count=parent.CompletedWordUseCount(token);
            if((uint)Ordinal>=count)throw new InvalidOperationException("Completed WORD use ordinal escaped its parent.");
            var node=parent.batchWords!.NodeAt(Ordinal,token);
            if(node.WordIndex!=parent.publishedSourceSelectedIndices![Ordinal]||
                !ReferenceEquals(node.Word.Pool,parent.Wrapper.ActualCompletion.Source.Session!.Ownership.GlyphPool)||
                !ReferenceEquals(parent.publishedOriginalSources![Ordinal].Owner,parent)||
                !ReferenceEquals(parent.publishedWords[Ordinal].OriginalSource.Owner,parent))
                throw new InvalidOperationException("Current TOP/source/payload owner binding changed.");
            _=parent.publishedWords[Ordinal].ChoiceAt(parent.publishedSelectedIndices[Ordinal]);
        }
        internal int SourceSelectedIndex(CancellationToken token)
        {Require(token);return parent.publishedSourceSelectedIndices![Ordinal];}
        internal int PayloadSelectedIndex(CancellationToken token)
        {Require(token);return parent.publishedSelectedIndices[Ordinal];}
        internal int SourceChoiceCount(CancellationToken token)
        {Require(token);return parent.batchWords!.SpanAt(Ordinal,token).Words.Count;}
        internal int PayloadChoiceCount(CancellationToken token)
        {Require(token);return parent.publishedWords[Ordinal].ChoiceCount;}
        internal LegacyFreshMainGlobalWordSourceChoice SelectedChoice(CancellationToken token)
        {Require(token);return parent.publishedWords[Ordinal].ChoiceAt(parent.publishedSelectedIndices[Ordinal]);}
        internal int OriginalRangeCount(CancellationToken token)
        {Require(token);return parent.publishedOriginalSources![Ordinal].OriginalRangeCount;}
        internal LegacyNativeInkPositionRange[] CopyOriginalRanges(CancellationToken token)
        {Require(token);return parent.publishedOriginalSources![Ordinal].CopyOriginalKey();}
        internal LegacyNativePlainWordRecord SourceWord(CancellationToken token)
        {Require(token);return parent.batchWords!.NodeAt(Ordinal,token).Word;}
    }
}
