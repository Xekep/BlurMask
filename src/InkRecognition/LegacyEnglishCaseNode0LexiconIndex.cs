namespace InkRecognition;

// Pure immutable data preparation BEFORE the OLD execution root is claimed.
// No dictionary reload, full-resource copy/hash, recursion or growing collection.
internal sealed class LegacyEnglishCaseNode0LexiconIndex
{
    private readonly int[] words;
    private readonly ulong terminalLabelsLow,terminalLabelsHigh;
    private readonly LegacyEnglishCaseNode0Ledger ledger;
    internal LegacyPackedLexicon Lexicon{get;}
    internal LegacyEnglishLkFreshNumericBinding Numeric{get;}
    private LegacyEnglishCaseNode0LexiconIndex(LegacyEnglishLkFreshNumericBinding numeric,LegacyEnglishCaseNode0Ledger ledger,
        int[] words,ulong low,ulong high)
    {Numeric=numeric;Lexicon=numeric.DataOwner.Dictionary;this.ledger=ledger;this.words=words;terminalLabelsLow=low;terminalLabelsHigh=high;}
    internal static LegacyEnglishCaseNode0LexiconIndex Prepare(LegacyEnglishLkFreshNumericBinding numeric,
        LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,32);
        var data=numeric.DataOwner;var lexicon=data.Dictionary;
        if(data.Fingerprint!="9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9"||numeric.SourceKind!=0x135||numeric.Profile!=2||
            numeric.LanguageOwner.Order!=3||numeric.Context00!=0x4000000||data.LexicalAlphabet.LabelCount!=104||
            lexicon.NodeCount!=666547||lexicon.StoredTerminalCount!=260074||lexicon.PackedLabelMask!=127||lexicon.PackedTerminalBit!=128||
            lexicon.MaximumWordLength is <1 or >256)
            throw new NotSupportedException("Exact immutable EN packed dictionary/numeric ownership required before root claim.");
        int n=lexicon.NodeCount;
        var words=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.TerminalIndex,n);
        var first=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.IndexStackFirst,257);
        var end=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.IndexStackEnd,257);
        // No generic PackedArray getters occur in the per-node closure. This
        // SOURCE-only reviewed bulk replacement for the per-access prototype.
        // Exact immutable loader owns a tree: n node visits and<=n frame pops.
        // Reader28 and DFS12 give<=52*n+12 under the reviewed named helper/
        // field/array/out-store convention; arithmetic and BCL internals remain
        // explicit exclusions. This exceeds current16MiB: preflight REFUSES.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,256);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,56);
        var reader=LegacyPackedLexicon.Node0IndexReader.Open(lexicon,numeric);
        if(!reader.IsFor(lexicon))throw new InvalidDataException("Private index reader owner mismatch.");
        // Charge BEFORE Array.Fill/stack initialization/DFS, no refunds. The
        // fixed256 factory/setup charge above is separate from this kernel.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,checked(52L*n+12));
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.TerminalIndex,checked(4L*n+4L*260074));
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,checked(12L*n));
        Array.Fill(words,-1);first[0]=0;end[0]=1;int depth=0,visited=0,terminal=0;ulong low=0,high=0;
        while(depth>=0)
        {
            token.ThrowIfCancellationRequested();
            if(first[depth]==end[depth]){depth--;continue;}
            int node=first[depth]++;reader.Read(node,out uint packed,out int start,out int finish);int label=(int)(packed&127);
            if((packed&~255U)!=0||(uint)label>=104)throw new InvalidDataException("Packed label exceeds same-owned lexical104 before any bitmap shift.");
            visited++;
            if(node!=0&&(packed&128)!=0)
            {
                if(terminal>=260074)throw new InvalidDataException("Terminal preorder overflow.");
                words[node]=terminal++;if(label<64)low|=1UL<<label;else high|=1UL<<(label-64);
            }
            var range=(Start:start,End:finish);
            if(range.Start<node+1||range.End<range.Start||range.End>n||range.End-range.Start>81)
                throw new InvalidDataException("Native strictly increasing <=81 child range required.");
            if(range.Start!=range.End)
            {
                if(depth==256)throw new InvalidDataException("Bounded257-frame preorder stack required.");
                depth++;first[depth]=range.Start;end[depth]=range.End;
            }
        }
        if(!reader.IsFor(lexicon)||visited!=n||terminal!=260074)throw new InvalidDataException("Whole dictionary and terminal preorder coverage required.");
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        return new(numeric,ledger,words,low,high);
    }
    internal void RequireEmptyAuxiliary(ReadOnlySpan<ushort> map)
    {
        if(map.Length!=104)throw new NotSupportedException("Exact auxiliary104 map required.");
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMapProof,104*4);
        for(int label=0;label<104;label++)
        {
            // Includes lexical0, protocol/sentinel labels and every value other
            // thanFFFF. No English-letter assumption or special-label skip.
            // Fixed label0..103 guarantees shifts0..63/0..39, never masked128.
            bool terminal=label<64?(terminalLabelsLow&(1UL<<label))!=0:(terminalLabelsHigh&(1UL<<(label-64)))!=0;
            if(terminal&&map[label]!=ushort.MaxValue)throw new NotSupportedException("Auxiliary map admits a terminal edge outside empty-output subset.");
        }
    }
    internal void ChargeBindingChecks()=>ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMapProof,16);
    internal int ReadTerminalWordId(int node)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.TerminalIndex,4);
        if((uint)node>=(uint)words.Length||words[node]<0)throw new NotSupportedException("Actual terminal dictionary node required; nonterminal reads are separate.");
        return words[node];
    }
}
