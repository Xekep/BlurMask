namespace InkRecognition;

//Library-only first-query envelope, not the native controller lifetime limit.
internal class LegacyEnglishCaseExecutionPoolReceipt
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
    internal static LegacyEnglishCaseExecutionPoolReceipt PreflightOuterWide(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,long maximum,LegacyEnglishCaseNode0Ledger ledger,LegacyNativeTextPublicationData publication,LegacyEnglishCaseNode0Completion.GraphPeriodicPreparation periodic,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(ledger);ArgumentNullException.ThrowIfNull(publication);
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,512);_=periodic.Require(k,token);
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
        // Entire opt-in pool including retained metadata (no default instance fields change).
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,128);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,256);
        return new OuterWidePool(k,n,ledger,publication);
    }
    // Default base never owns a fact and has NO new instance fields.
    internal virtual bool HasOuterWidePreparation=>false;
    internal virtual LegacyNativeCasePreparation PrepareOuterWide(LegacyEnglishCaseInitializationPlan plan,LegacyNativeCaseAlphabetRanges ranges,LegacyNativeTextPublicationData publication,CancellationToken token)
        =>throw new InvalidOperationException("Actual owning outer-wide preparation pool required.");
    internal virtual void ObserveOuterWideText(ushort label,string? text,CancellationToken token)
        =>throw new InvalidOperationException("Only an active private outer-wide preparation may observe text.");
    internal virtual bool RequireOuterWide(LegacyEnglishCaseInitializationPlan plan,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,LegacyNativeCasePreparation preparation,LegacyInkAlphabet alphabet,LegacyNativeTextPublicationData publication,CancellationToken token)
        =>throw new InvalidOperationException("Outer-wide proof is uncomputed; no fallback authority.");
    private sealed class OuterWidePool:LegacyEnglishCaseExecutionPoolReceipt
    {
        private readonly LegacyEnglishCaseNode0Ledger ledger;
        private readonly LegacyInkAlphabet alphabet;
        private readonly LegacyNativeTextPublicationData publication;
        private readonly int thread,labelCount;
        private LegacyNativeCasePreparation? prepared;
        private int phase,nextLabel;
        private bool any;
        internal OuterWidePool(LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n,LegacyEnglishCaseNode0Ledger ledger,LegacyNativeTextPublicationData publication):base(k,n)
        {this.ledger=ledger;alphabet=k.Source.Alphabet;this.publication=publication;thread=Environment.CurrentManagedThreadId;labelCount=alphabet.LabelCount;}
        internal override bool HasOuterWidePreparation=>true;
        internal override LegacyNativeCasePreparation PrepareOuterWide(LegacyEnglishCaseInitializationPlan plan,LegacyNativeCaseAlphabetRanges ranges,LegacyNativeTextPublicationData actual,CancellationToken token)
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,512);
            _=RequireCapacity(plan);token.ThrowIfCancellationRequested();
            if(phase!=0||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(ranges.Alphabet,alphabet)||!ReferenceEquals(actual,publication))
                throw new InvalidOperationException("One actual same-thread outer alphabet preparation required.");
            phase=1;
            try
            {
                var value=LegacyNativeCasePreparation.PrepareOuterWide(ranges,actual,this,token);
                token.ThrowIfCancellationRequested();
                if(nextLabel!=alphabet.LabelCount||!ReferenceEquals(value.Alphabet,alphabet))throw new InvalidOperationException("Whole same outer alphabet script loop required.");
                prepared=value;phase=2;return value; // SOME is retained DATA, never an early refusal.
            }
            catch{phase=3;throw;}
        }
        internal override void ObserveOuterWideText(ushort label,string? text,CancellationToken token)
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,96);
            token.ThrowIfCancellationRequested();
            if(phase!=1||thread!=Environment.CurrentManagedThreadId||label!=nextLabel||label>=labelCount)
                throw new InvalidOperationException("Exact active ordered original alphabet text loop required.");
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);nextLabel=label+1;
            if(!LegacyWideSymbolPairs.TryGetSingleWideBase(text,out int scalar))return;
            // Actual base string + bounded same-alphabet lookup BEFORE allocation/access.
            // Wide substitutions all have BMP base scalars, at most one UTF16 char.
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(512L+160L*labelCount));
            if(alphabet.TryGetLabel(new System.Text.Rune(scalar).ToString(),out _))any=true;
        }
        internal override bool RequireOuterWide(LegacyEnglishCaseInitializationPlan plan,LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,LegacyNativeCasePreparation actual,LegacyInkAlphabet actualAlphabet,LegacyNativeTextPublicationData actualPublication,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();_=RequireCapacity(plan);
            if(phase!=2||!ReferenceEquals(prepared,actual)||!ReferenceEquals(actualAlphabet,alphabet)||!ReferenceEquals(actual.Alphabet,alphabet)||
                !ReferenceEquals(actualPublication,publication)||!ReferenceEquals(stage.Ledger,ledger)||!ReferenceEquals(stage.Lease.Root.Source.Alphabet,alphabet))
                throw new InvalidOperationException("Computed actual outer preparation/current owner ledger/alphabet required.");
            return !any;
        }
    }
    internal void Claim(LegacyEnglishCaseInitializationPlan actual,LegacyEnglish119MainKnowledge k,LegacyEnglishLkFreshNumericBinding n)
    {if(!ReferenceEquals(k,knowledge)||!ReferenceEquals(n,numeric)||Interlocked.CompareExchange(ref plan,actual,null)is not null)throw new InvalidOperationException("One exact source-bound execution plan required.");}
    internal int RequireCapacity(LegacyEnglishCaseInitializationPlan actual)
    {if(!ReferenceEquals(plan,actual))throw new InvalidOperationException("Exact consumed execution pool receipt required.");return Capacity;}
}
