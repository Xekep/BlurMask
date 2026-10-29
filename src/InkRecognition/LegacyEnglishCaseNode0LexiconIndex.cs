namespace InkRecognition;

// Pure immutable data preparation BEFORE the OLD execution root is claimed.
// No dictionary reload, full-resource copy/hash, recursion or growing collection.
internal sealed class LegacyEnglishCaseNode0LexiconIndex
{
    private readonly PreparedData data;
    private readonly LegacyEnglishCaseNode0Ledger ledger;
    internal LegacyPackedLexicon Lexicon{get;}
    internal LegacyEnglishLkFreshNumericBinding Numeric{get;}
    private LegacyEnglishCaseNode0LexiconIndex(LegacyEnglishLkFreshNumericBinding numeric,LegacyEnglishCaseNode0Ledger ledger,
        PreparedData data)
    {Numeric=numeric;Lexicon=numeric.DataOwner.Dictionary;this.ledger=ledger;this.data=data;}
    // Owner-private immutable value: no array/ROM/ref-return accessor exists.
    internal sealed class PreparedData
    {
        private readonly LegacyPackedLexicon owner;
        private readonly int[] words;
        private readonly ulong low,high;
        internal PreparedData(LegacyPackedLexicon owner,int[] words,ulong low,ulong high)
        {this.owner=owner;this.words=words;this.low=low;this.high=high;}
        internal bool IsFor(LegacyPackedLexicon value)=>ReferenceEquals(owner,value);
        internal int Word(int node)
        {
            if((uint)node>=(uint)words.Length||words[node]<0)
                throw new NotSupportedException("Actual terminal dictionary node required; nonterminal reads are separate.");
            return words[node];
        }
        internal bool HasTerminalLabel(int label)=>label<64?(low&(1UL<<label))!=0:(high&(1UL<<(label-64)))!=0;
    }
    internal static LegacyEnglishCaseNode0LexiconIndex Prepare(LegacyEnglishLkFreshNumericBinding numeric,
        LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.IndexKernel,128);
        var data=numeric.DataOwner;var lexicon=data.Dictionary;
        if(data.Fingerprint!="9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9"||numeric.SourceKind!=0x135||numeric.Profile!=2||
            numeric.LanguageOwner.Order!=3||numeric.Context00!=0x4000000||data.LexicalAlphabet.LabelCount!=104||
            lexicon.NodeCount!=666547||lexicon.StoredTerminalCount!=260074||lexicon.PackedLabelMask!=127||lexicon.PackedTerminalBit!=128||
            lexicon.MaximumWordLength is <1 or >256)
            throw new NotSupportedException("Exact immutable EN packed dictionary/numeric ownership required before root claim.");
        // Exact owner reference, never fingerprint-as-cache-key. A cold operation
        // does NOT silently warm the cache, raise16Mi visits or retain its ledger.
        var prepared=lexicon.RequirePreparedNode0Index(token);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        return new(numeric,ledger,prepared);
    }
    internal void RequireEmptyAuxiliary(ReadOnlySpan<ushort> map)
    {
        if(map.Length!=104)throw new NotSupportedException("Exact auxiliary104 map required.");
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMapProof,104*8);
        for(int label=0;label<104;label++)
        {
            // Includes lexical0, protocol/sentinel labels and every value other
            // thanFFFF. No English-letter assumption or special-label skip.
            // Fixed label0..103 guarantees shifts0..63/0..39, never masked128.
            bool terminal=data.HasTerminalLabel(label);
            if(terminal&&map[label]!=ushort.MaxValue)throw new NotSupportedException("Auxiliary map admits a terminal edge outside empty-output subset.");
        }
    }
    // Only the existing exact same-model index and authenticated EMPTY auxiliary callers use this work debit.
    // This reads immutable metadata, not a source query, and never manufactures an empty page.
    internal void ChargeEmptyAuxiliaryRead(ulong sourceTag)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,512); // BEFORE the new bounded packed metadata lookup.
        var range=Lexicon.GetChildRange(unchecked((int)(uint)sourceTag));
        int children=checked(range.End-range.Start);
        // Whole same-model index preparation already checked <=81 at every node before publishing PreparedData.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryOuter,EmptyAuxiliaryReadVisits(children));
    }
    // Pure scalar bound only. No caller count grants a query/owner/source capability.
    internal static long EmptyAuxiliaryReadVisits(int children)
    {
        if((uint)children>81)throw new InvalidDataException("Same prepared English index child-count premise changed.");
        return checked(16384L+256L*children);
    }
    internal void ChargeBindingChecks()=>ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMapProof,16);
    internal int ReadTerminalWordId(int node)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.TerminalIndex,32);
        return data.Word(node);
    }
}
