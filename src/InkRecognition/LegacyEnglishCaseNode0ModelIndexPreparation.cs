namespace InkRecognition;

// SOURCE SUCCESSOR ONLY. Explicit model preprocessing, not Node0 operation work.
// No caller arrays, callback, resource-fingerprint cache key or ledger enters.
internal static class LegacyEnglishCaseNode0ModelIndexPreparation
{
    internal const int Nodes=666547,Terminals=260074;
    internal const long PayloadReservation=4L*Nodes+2L*257*4+128;
    internal const long CopyReservation=32L*Nodes+4L*Terminals+4096;
    internal const long VisitReservation=52L*Nodes+12+256+1024;
    // Separate fixed MODEL-preparation limits. Original operation16Mi gate is
    // unchanged; cold Node0 Prepare refuses until this explicit step succeeds.
    internal static void Prepare(LegacyEnglishLkFreshNumericBinding numeric,
        long maximumPayload=4L*1024*1024,long maximumCopies=32L*1024*1024,
        long maximumVisits=40L*1024*1024,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(numeric);token.ThrowIfCancellationRequested();
        if(maximumPayload<PayloadReservation||maximumCopies<CopyReservation||maximumVisits<VisitReservation)
            throw new ArgumentException("Complete explicit model-index reservation unavailable.");
        var source=numeric.DataOwner;var lexicon=source.Dictionary;
        if(source.Fingerprint!="9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9"||
            numeric.SourceKind!=0x135||numeric.Profile!=2||numeric.LanguageOwner.Order!=3||
            numeric.Context00!=0x4000000||source.LexicalAlphabet.LabelCount!=104||
            lexicon.NodeCount!=Nodes||lexicon.StoredTerminalCount!=Terminals||
            lexicon.PackedLabelMask!=127||lexicon.PackedTerminalBit!=128||lexicon.MaximumWordLength is <1 or >256)
            throw new NotSupportedException("Exact existing immutable English model owner required.");
        // Claim is local to exact lexicon. No global model table, lock on public
        // object, retained invocation/token or object allocated at old Load.
        if(!lexicon.TryClaimNode0IndexPreparation(token))return;
        try
        {
            int n=Nodes;
            var reader=LegacyPackedLexicon.Node0IndexReader.Open(lexicon,numeric);
            if(!reader.IsFor(lexicon))throw new InvalidDataException("Private reader owner mismatch.");
            // Whole reservations above precede each of these fixed allocations.
            var words=new int[n];var first=new int[257];var end=new int[257];
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
            token.ThrowIfCancellationRequested();
            var result=new LegacyEnglishCaseNode0LexiconIndex.PreparedData(lexicon,words,low,high);
            // All successful writes finish BEFORE release publication; failure
            // keeps local arrays unpublished. Cancellation never poisons a prior
            // published index and never cancels another preparation thread.
            token.ThrowIfCancellationRequested();lexicon.PublishNode0Index(result);
        }
        finally{lexicon.ReleaseNode0IndexPreparation();}
    }
}
