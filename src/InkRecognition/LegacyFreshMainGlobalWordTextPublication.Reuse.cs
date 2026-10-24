namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    private int currentPayloadFirstOrdinal=-1,sourceSelectedWordIndex;
    private LegacyPreparedGlobalWordPayloadReuse.Plan currentReusePlan;
    private readonly int[]? publishedSourceSelectedIndices;
    private readonly LegacyFreshMainGlobalWordSourceReceipt[]? publishedOriginalSources;

    internal void RequireExistingWordIntern(LegacyOwnedMainTextSourceNode node,
        LegacyFreshMainGlobalWordSourceReceipt actual,CancellationToken token)
    {
        RequireWordIntern(actual,token);
        if(batchWords is null||currentPayloadFirstOrdinal<0||currentPayloadFirstOrdinal>=batchWordOrdinal||
            currentPayloadFirstOrdinal>=publishedWords.Count||
            !ReferenceEquals(publishedWords[currentPayloadFirstOrdinal].Node,node)||
            !ReferenceEquals(node.FirstGlobalWordSource,publishedOriginalSources![currentPayloadFirstOrdinal])||
            !ReferenceEquals(node.FirstGlobalWordSource?.Owner,this))
            throw new InvalidOperationException("Existing WORD must be the exact earlier payload of this private completed queue.");
    }
    private bool IsCurrentPayloadSource()
    {
        if(payload is null)return false;
        if(ReferenceEquals(payload.Node.FirstGlobalWordSource,OriginalWordSource))return true;
        return batchWords is not null&&currentPayloadFirstOrdinal>=0&&currentPayloadFirstOrdinal<batchWordOrdinal&&
            currentPayloadFirstOrdinal<publishedWords.Count&&ReferenceEquals(payload,publishedWords[currentPayloadFirstOrdinal])&&
            ReferenceEquals(payload.Node.FirstGlobalWordSource,publishedOriginalSources![currentPayloadFirstOrdinal]);
    }
    private void ReconcileCurrentWord(LegacyOwnedMainTextSourceNode node,ReadOnlySpan<byte> allocator40,
        ReadOnlySpan<byte> selection4,LegacyGlobalWordTextPreparedFault fault,CancellationToken token)
    {
        RequireExistingWordIntern(node,OriginalWordSource,token);
        if(allWords is null||currentReusePlan.ChoiceMap.Length!=allWords.Count||
            (uint)sourceSelectedWordIndex>=currentReusePlan.ChoiceMap.Length)
            throw new InvalidOperationException("Exact preflighted current275 reuse plan required.");
        batchWords!.RequireEqualityReadSet(token);
        payload=publishedWords[currentPayloadFirstOrdinal];Record("word-intern-reuse");
        payload.AppendChoiceSlots(currentReusePlan.ResultChoiceCount);
        stage="characters";int append=0;
        for(int ordinal=0;ordinal<allWords.Count;ordinal++)
        {
            var cursor=allWords.Open(this,token);
            if(append<currentReusePlan.AppendOrdinals.Length&&currentReusePlan.AppendOrdinals[append]==ordinal)
            {
                int mapped=currentReusePlan.ChoiceMap[ordinal];payload.BeginChoice(mapped);nextCharacter=-1;
                Wrapper.BeginSpanWord(this,cursor,token);Record("word-reuse-begin:"+ordinal);
                var output=Wrapper.PublishIntoWord(this,allocator40,selection4,fault.Character,token);
                if(output.SelectedFlags!=payload.CurrentChoiceFlags||
                    !LegacyOwnedRawInkRangeOperations.Equal(output.CopyOrdinaryRanges(token),payload.CopyCurrentOrdinaryRanges()))
                    throw new InvalidDataException("Reused WORD appended CHAR choice differs from actual current275 output.");
                payload.CompleteChoice(aggregateParentFlags:false);Wrapper.CloseSpanWord(this,cursor,token);append++;
                Record("word-reuse-complete:"+ordinal);
            }
            else Record("word-reuse-existing:"+ordinal);
            allWords.Close(this,cursor,token);
        }
        if(append!=currentReusePlan.AppendOrdinals.Length)throw new InvalidOperationException("Every preflighted reuse append must finish.");
        allWords.RequireComplete(this,token);
        selectedWordIndex=currentReusePlan.SelectedChoiceIndex;
        _=payload.ChoiceAt(selectedWordIndex);Record("word-reused");
        //Native existing-key branch does not initialize/register a second WORD
        //selection. The TEXT link retains the remapped selected choice index.
    }
    public LegacyFreshMainGlobalWordSourceChoice[] CopyPublishedSelectedWordChoices()
    {
        Scope.Validate();lock(gate)
        {
            if(!completed||allWords is null)throw new InvalidOperationException("Complete all-choice publication required.");
            return publishedWords.Select((word,index)=>word.ChoiceAt(publishedSelectedIndices[index])).ToArray();
        }
    }
    public int[] CopyPublishedSourceSelectedIndices()
    {Scope.Validate();lock(gate){if(!completed)throw new InvalidOperationException("Complete publication required.");return publishedSourceSelectedIndices?.ToArray()??publishedSelectedIndices.ToArray();}}
    public LegacyFreshMainGlobalWordSourceReceipt[] CopyPublishedOriginalWordSources()
    {Scope.Validate();lock(gate){if(!completed)throw new InvalidOperationException("Complete publication required.");return publishedOriginalSources?.ToArray()??publishedWords.Select(word=>word.OriginalSource).ToArray();}}
}
