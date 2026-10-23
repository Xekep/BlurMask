namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseInitializationNumericTables
{
    // Scalar result only. Neither the existing six code buffers nor cost
    // arrays leave this method, even as recoverable ReadOnlyMemory views.
    internal readonly record struct Node0FirstNumeric(int Word,int Category,int Class,int MatchedOrder,int Cost);

    // NEW immutable owner-local holder; OLD preparation never constructs it.
    // No source arrays/views escape. A default/other holder is not Stage authority.
    internal sealed class Node0Handles
    {
        private readonly LegacyEnglishCaseInitializationNumericTables owner;
        private readonly LegacyEnglishLkSparseLanguageData.Node0FirstPackedReader node0Packed;
        private readonly byte[] node0WordCode,node0OneCode,node0TwoCode,node0BackoffCode;
        private Node0Handles(LegacyEnglishCaseInitializationNumericTables owner)
        {
            this.owner=owner;
            node0Packed=LegacyEnglishLkSparseLanguageData.Node0FirstPackedReader.Prepare(owner.Numeric.LanguageOwner);
            node0WordCode=owner.codes["98.wordCodes"];node0OneCode=owner.codes["C0.0"];
            node0TwoCode=owner.codes["C0.1"];node0BackoffCode=owner.codes["C8.0"];
        }
        internal static Node0Handles Prepare(LegacyEnglishCaseInitializationNumericTables owner,
            LegacyEnglishCaseNode0Completion full,LegacyEnglishCasePrimaryCompletion primary)
        {
            full.RequireNumericPreparation(primary);
            return new(owner);
        }
        // SOURCE DRAFT: partial FIRST's private owned terminal callback issues
        // word/category data; this numerical method itself is not source70
        // authority. It does not manufacture a token or accept a
        // caller-supplied previous-word/history/cache object.
        internal Node0FirstNumeric ReadNode0First(LegacyEnglishCaseNode0Completion.Stage stage,
            int terminalWord,int category,CancellationToken token)
        {
            stage.Require();stage.RequireNumericHandles(this);token.ThrowIfCancellationRequested();stage.ChargeFirstKernel(2048);
            var initializer=stage.Lease.Owner.Initializer;
            if(!ReferenceEquals(initializer.NumericForAudit,owner)||!ReferenceEquals(stage.Lease.Root.NumericOwner,owner.Numeric)||
                !ReferenceEquals(initializer.PlanForAudit.NumericTables,owner))
                throw new InvalidOperationException("Exact same-live initializer numeric table owner required.");
            if((uint)terminalWord>=260074||(uint)category>12)
                throw new NotSupportedException("Actual terminal word and case category0..12 required.");
            var language=owner.Numeric.LanguageOwner;
            if(!node0Packed.IsFor(language)||language.ReservedWordIndex!=260074||language.Order!=3||language.ClassSlots!=1024||
                node0Packed.ReadWordClass(260074)!=0)
                throw new InvalidDataException("Pinned initial anchor resolves the owned reserved class0.");
            int currentClass=node0Packed.ReadWordClass(terminalWord);
            if((uint)currentClass>=1024)throw new InvalidDataException("Actual bounded word class required.");
            // Scope all views here. These arrays were created once by the OLD
            // initializer and have no post-Prepare mutators; do not clone or cache.
            ReadOnlySpan<byte> wordCode=node0WordCode,oneCode=node0OneCode,twoCode=node0TwoCode,backoffCode=node0BackoffCode;
            ReadOnlySpan<int> wordCost=owner.word,oneCost=owner.classes[0],twoCost=owner.classes[1],backoffCost=owner.backoff[0];
            if(wordCode.Length!=260075||oneCode.Length!=1024||twoCode.Length!=113643||backoffCode.Length!=1024||
                wordCost.Length!=256||oneCost.Length!=256||twoCost.Length!=256||backoffCost.Length!=256)
                throw new InvalidDataException("Complete existing private numeric arrays required.");
            int first=node0Packed.ReadOrder1Offset(0),end=node0Packed.ReadOrder1Offset(1);
            if(first<0||end<first||end>113643)throw new InvalidDataException("Bounded reserved-class sparse row required.");
            int low=first,high=end;
            while(low<high)
            {
                token.ThrowIfCancellationRequested();int middle=low+((high-low)>>1);
                int label=node0Packed.ReadOrder2Label(middle);
                if(label<currentClass)low=middle+1;else high=middle;
            }
            bool found=low<end&&node0Packed.ReadOrder2Label(low)==currentClass;
            int matched=found?2:1;
            int baseCost=found?twoCost[twoCode[low]]:checked(backoffCost[backoffCode[0]]+oneCost[oneCode[currentClass]]);
            int fallback=category switch{3=>1,5 or 6=>2,10 or 11 or 12=>3,_=>0};
            int result=checked(baseCost+wordCost[wordCode[terminalWord]]+fallback);
            token.ThrowIfCancellationRequested();stage.Require();
            return new(terminalWord,category,currentClass,matched,result);
        }
    }
}
