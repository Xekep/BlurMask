namespace InkRecognition;

/// <summary>Value-only literal0283B0 existing-WORD choice reconciliation.
/// This helper never creates a WORD/source/queue issuer. Its only production
/// caller supplies equality from the same completed private275 owner.</summary>
internal static class LegacyPreparedGlobalWordPayloadReuse
{
    //Native13F970 consumes parent20, kind4&7, skip flag6&2 and symbol8.
    //Classifier scores0c/10/14 and publication flagbit8 may legitimately change
    //on same-descriptor refresh and are deliberately not equality inputs.
    internal static bool SameEqualityInputs(ReadOnlySpan<byte> before,ReadOnlySpan<byte> after)
    {
        if(before.Length!=40||after.Length!=40)throw new ArgumentException("Complete equality source records required.");
        return (before[4]&7)==(after[4]&7)&&(before[6]&2)==(after[6]&2)&&
            before.Slice(8,2).SequenceEqual(after.Slice(8,2))&&
            before.Slice(32,8).SequenceEqual(after.Slice(32,8));
    }
    internal readonly record struct Plan(int[] ChoiceMap,int[] AppendOrdinals,int ResultChoiceCount,int SelectedChoiceIndex);
    internal static Plan Build(int currentRows,int firstOwnerRows,int existingPayloadChoices,
        bool sameNativeSpan,int selectedCurrent,Func<int,int,bool> equal,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(equal);
        if(currentRows is<1 or>4095||firstOwnerRows is<1 or>4095||
            existingPayloadChoices<firstOwnerRows||existingPayloadChoices>4095||(uint)selectedCurrent>=currentRows)
            throw new ArgumentException("Bounded private WORD payload extents required.");
        if(sameNativeSpan&&currentRows!=firstOwnerRows)
            throw new InvalidOperationException("Identical native275 span must retain its original rows.");
        var map=new int[currentRows];var append=new List<int>(currentRows);int count=existingPayloadChoices;
        for(int current=0;current<currentRows;current++)
        {
            token.ThrowIfCancellationRequested();
            if(sameNativeSpan){map[current]=current;continue;}
            int first=0;
            //Native compares ONLY against the first source owner's original
            //rows, not choices appended by earlier reused owners, nor other
            //unmatched rows of the current source owner.
            while(first<firstOwnerRows&&!equal(current,first))first++;
            if(first<firstOwnerRows)map[current]=first;
            else
            {
                if(count==4095)throw new NotSupportedException("Reconciled WORD payload exceeds4095 before publication.");
                if(current==selectedCurrent)selectedCurrent=count;
                map[current]=count++;append.Add(current);
            }
        }
        if((uint)selectedCurrent>=count)throw new NotSupportedException("Native retained selected choice lies outside reconciled payload.");
        return new(map,append.ToArray(),count,selectedCurrent);
    }
}
