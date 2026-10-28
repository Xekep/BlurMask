namespace InkRecognition;
internal sealed class LegacyGlobalWordTextWorkBudget
{
    internal int Rows{get;private set;}
    internal int Ranges{get;private set;}
    internal int Utf8Bytes=>Rows*4;
    internal void AddSource(int existingRows,int compactRows,int sourceReferences,int physicalInputs)
    {
        if(existingRows<0||compactRows<0||sourceReferences<0||physicalInputs is<1 or>256)
            throw new ArgumentOutOfRangeException(nameof(existingRows));
        long addedRows=(long)existingRows+compactRows,addedRanges=(long)sourceReferences*physicalInputs;
        if(addedRows>65536-Rows||addedRanges>1_048_576-Ranges)
            throw new NotSupportedException("Whole WORD row/projected-range budgets exceeded before claim.");
        Rows+=(int)addedRows;Ranges+=(int)addedRanges;
    }
}
