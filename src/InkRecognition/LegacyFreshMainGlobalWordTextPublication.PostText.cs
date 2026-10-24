using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    //Connected successful ordinary branch of0283B0 after TEXT registration:
    //025E40 ->027670 ->0248D0 -> per-choice01C740. This method does not
    //simulate a destructive branch with detached rows or silently prune it.
    internal void RequireCompletedOrdinaryPostText(CancellationToken token)
    {
        int count=CompletedWordUseCount(token);
        if(count is<1 or>128||AlternativeCount!=1||Text.Selection is null)
            throw new NotSupportedException("Bounded one-TEXT completed WORD vector required.");
        byte[][] descriptors=CopyDescriptors();byte[] sourceLines=CopySourceLines56();
        if(descriptors.Length!=1||descriptors[0].Length!=88||(descriptors[0][0]&15)!=0||
            BinaryPrimitives.ReadInt32LittleEndian(descriptors[0].AsSpan(16))!=0||
            LineCount!=1||sourceLines.Length!=56||WrapperLineOffset!=1)
            throw new NotSupportedException("Post-TEXT currently requires one actual ordinary line0.");
        //Preflight the actual private choices before any interval expansion.
        //All shared payload choices participate in025E40, not just TOP's use.
        long rows=0,characters=0;
        var unique=new List<LegacyFreshMainGlobalWordSourcePayload>(count);
        var work=new LegacyCompletedPostTextIntervalWork();
        foreach(var word in publishedWords)
        {
            token.ThrowIfCancellationRequested();
            if(unique.Contains(word))continue;unique.Add(word);
            if((word.Flags&0x201000)!=0||word.Field40!=-1)
                throw new NotSupportedException("Post-TEXT special WORD/override branch is not connected.");
            rows=checked(rows+word.OriginalSource.OriginalRangeCount);
            for(int c=0;c<word.ChoiceCount;c++)
            {
                var choice=word.ChoiceAt(c);
                if(choice.LinkCount is<1||choice.LinkCount>4096-characters)throw new NotSupportedException("Bounded post-TEXT links required before cloning.");
                var links=choice.CopyCharacterLinks();long startRows=rows;
                if(links.Count==0||links.Count>4096-characters)
                    throw new NotSupportedException("Bounded nonempty post-TEXT CHAR vector required.");
                characters+=links.Count;
                foreach(var link in links)
                {
                    var source=link.Source;
                    if(!source.IsComplete||!ReferenceEquals(source.Owner,Wrapper)||source.SourceLineId!=0||
                        source.PriorRangeCount!=0||!ReferenceEquals(source.Node.FirstGlobalSource,source.Source))
                        throw new NotSupportedException("Post-TEXT requires exact fresh same-wrapper CHAR line0.");
                    //Source.CopyRanges length is bounded before the later
                    //subtraction; no inferred raw-point index is substituted.
                    rows=checked(rows+source.Node.RangeCount);
                }
                if(choice.OrdinaryRangeCount>rows-startRows)
                    throw new InvalidOperationException("Private ordinary union exceeds its actual CHAR-key input extent.");
            }
        }
        RequireCompletedWordChain(unique);
        //027670 reads ordinal0/1 of the NEXT word's FIRST choice, not its
        //selected choice. Prove that consumed ordinal exists before the
        //ordinary-line0 no-positive-gap argument is used.
        for(int i=0;i<count-1;i++)
        {
            var current=publishedWords[i].ChoiceAt(publishedSelectedIndices[i]);
            if((current.Flags&0x2000)==0)continue;
            var first=publishedWords[i+1].ChoiceAt(0);int nextOrdinal=(int)((first.Flags>>20)&1);
            if(first.LinkCount<=nextOrdinal)
                throw new NotSupportedException("Native line-gap FIRST control ordinal unavailable.");
        }
        if(rows>4096)
            throw new NotSupportedException("Post-TEXT source row ceiling exceeded before interval operations.");
        //025E40: selected last line and every alternative last line are0;
        //no024A90/025940 call, maxReferencedLine+1==actual line count1.
        //027670: previousLine becomes0 at each WORD. Any0x2000 choice sees
        //next first CHAR line0, giving gap=-1; no duplicated CHAR refs.
        //Fresh source22A=false excludes026090 (same existing source factory).
        LegacyNativeInkPositionRange[] union=[];
        foreach(var word in unique)
        {
            work.ChargeUnion(union.Length,word.OriginalSource.OriginalRangeCount);
            union=RangeOperations.Union(union,word.OriginalSource.CopyOriginalKey(),token);
        }
        work.ChargeRows(checked(union.Length+Text.RangeCount));
        if(!LegacyOwnedRawInkRangeOperations.Equal(union,Text.CopyRanges()))
            throw new NotSupportedException("Post-TEXT unequal WORD/TEXT union requires native0238B0.");
        foreach(var word in unique)
        {
            work.ChargeRows(word.OriginalSource.OriginalRangeCount);
            var fallback=word.OriginalSource.CopyOriginalKey();
            for(int c=word.ChoiceCount-1;c>=0;c--)
            {
                token.ThrowIfCancellationRequested();var choice=word.ChoiceAt(c);
                work.ChargeRows(choice.OrdinaryRangeCount);
                var remaining=choice.CopyOrdinaryRanges();
                if(remaining.Length==0){work.ChargeRows(fallback.Length);remaining=fallback.ToArray();}
                foreach(var link in choice.CopyCharacterLinks())
                {
                    int cuts=link.Source.Node.RangeCount;
                    //Subtract may split one row for each cut. Charge that
                    //growth inside this actual call before cloning/traversal.
                    work.ChargeSubtract(remaining.Length,cuts);
                    var key=link.Source.Source.CopyRanges(token);
                    if(key.Length!=cuts)throw new InvalidOperationException("Private CHAR key extent changed.");
                    remaining=RangeOperations.Subtract(remaining,key,token);
                }
                //Native keeps the sole choice, even with a remainder. With
                //multiple choices, retaining this row would skip024A90 and
                //its CHAR unregister/TEXT-link fixups, so refuse instead.
                if(remaining.Length!=0&&word.ChoiceCount>1)
                    throw new NotSupportedException("Post-TEXT residual choice removal requires native024A90 registration handling.");
            }
        }
        CompletedWordUseCount(token);RequireCompletedWordChain(unique);
    }
    private void RequireCompletedWordChain(IReadOnlyList<LegacyFreshMainGlobalWordSourcePayload> actual)
    {
        var interner=Scope.SharedState.Words;
        if(interner.Count!=actual.Count)throw new InvalidOperationException("Native WORD interner changed outside this complete publication.");
        LegacyOwnedMainTextSourceNode? previous=null;
        foreach(var word in actual)
        {
            var node=word.Node;
            if(!ReferenceEquals(node.Owner,interner)||!ReferenceEquals(node.FirstGlobalWordSource,word.OriginalSource)||
                !ReferenceEquals(node.Previous,previous)||(previous is not null&&!ReferenceEquals(previous.Next,node)))
                throw new InvalidOperationException("Exact native-order completed WORD chain required.");
            previous=node;
        }
        if(previous?.Next is not null)throw new InvalidOperationException("Native WORD chain has an unconsumed tail.");
    }
}

//Operational interval/row work only, not unfolded framework/CPU/RSS. No
//refund/reset. Each debit precedes the actual clone/interval operation.
internal sealed class LegacyCompletedPostTextIntervalWork
{
    internal const long Maximum=16_777_216;
    internal long Used{get;private set;}
    internal void ChargeRows(int rows){Count(rows);Debit(rows);}
    internal void ChargeUnion(int target,int added)
    {Count(target);Count(added);long extent=(long)target+added;Debit(checked(extent+extent*added));}
    internal void ChargeSubtract(int target,int cuts)
    {Count(target);Count(cuts);long extent=(long)target+cuts;Debit(checked(extent+extent*cuts));}
    private static void Count(int value)
    {if(value is<0 or>65536)throw new NotSupportedException("Bounded actual interval operand required before work debit.");}
    private void Debit(long amount)
    {if(amount>Maximum-Used)throw new NotSupportedException("Whole post-TEXT interval-work ceiling exhausted before operation.");Used+=amount;}
}
