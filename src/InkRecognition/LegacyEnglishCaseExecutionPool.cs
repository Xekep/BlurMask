namespace InkRecognition;

//Library-only first-query envelope, not the native controller lifetime limit.
internal sealed class LegacyEnglishCaseExecutionPoolReceipt
{
    internal const int Capacity=2048;
    internal const long CompleteLogicalCeiling=90987785;
    private readonly LegacyEnglish119MainKnowledge knowledge;
    private readonly LegacyEnglishLkFreshNumericBinding numeric;
    private LegacyEnglishCaseInitializationPlan? plan;
    private LegacyEnglishCaseExecutionPoolReceipt(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n){knowledge=k;numeric=n;}
    internal static LegacyEnglishCaseExecutionPoolReceipt Preflight(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,long maximum,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(k);ArgumentNullException.ThrowIfNull(n);token.ThrowIfCancellationRequested();
        if(maximum<CompleteLogicalCeiling||maximum>128L*1024*1024||!ReferenceEquals(k.Source,n.DataOwner.Source)||!k.Owner.IsComplete||k.Owner.IsFaulted)
            throw new ArgumentException("Complete same-source first-query logical budget required before entry.");
        _=LegacyEnglishMainInkSnapshot.Preflight(k.Owner.GroupCount,k.Owner.NodeCount,k.Owner.CandidateCount);
        _=LegacyEnglishMainSpanSession.Preflight(k.Owner.GroupCount,k.Owner.NodeCount,k.Owner.CandidateCount,new(1,k.Owner.CandidateCount));
        var dictionary=n.DataOwner.Dictionary;
        if(dictionary.NodeCount!=666547||dictionary.StoredTerminalCount!=260074||n.DataOwner.LexicalAlphabet.LabelCount!=104||k.Source.Alphabet.LabelCount!=217)
            throw new NotSupportedException("Actual checked English root grammar required.");
        //Data owner has already checked the complete immutable LK hash/tree.
        //Recheck the admitted root-specific no-control-edge bound before a copy.
        var range=dictionary.GetChildRange(0);if(range.End-range.Start-1!=80)throw new InvalidDataException("Actual eighty root edges required.");
        for(int i=range.Start+1;i<range.End;i++)
        {token.ThrowIfCancellationRequested();ushort label=dictionary.GetLabel(i);if(label>=104||label==90)throw new NotSupportedException("First query delimiter recursion is not admitted.");}
        return new(k,n);
    }
    internal void Claim(LegacyEnglishCaseInitializationPlan actual,LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n)
    {if(!ReferenceEquals(k,knowledge)||!ReferenceEquals(n,numeric)||Interlocked.CompareExchange(ref plan,actual,null)is not null)throw new InvalidOperationException("One exact source-bound execution plan required.");}
    internal int RequireCapacity(LegacyEnglishCaseInitializationPlan actual)
    {if(!ReferenceEquals(plan,actual))throw new InvalidOperationException("Exact consumed execution pool receipt required.");return Capacity;}
}
