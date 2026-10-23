namespace InkRecognition;

public sealed partial class LegacyNativeWordHeapDescriptor
{
    // Authorized live scalar endpoint only. Destination is privately allocated
    // by fullowner, never a historical descriptor or caller callback source.
    internal void CopyNode0CompletedBacking(LegacyEnglishCaseNode0Completion.Stage stage,
        LegacyEnglishCaseNode0Ledger ledger,Span<byte> destination,int n,CancellationToken token)
    {
        stage.RequireCompletedCapture(ledger,n);token.ThrowIfCancellationRequested();
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2L*32000+512); //byte read/store pairs +DRAFT helper/guard bound
        if(!ReferenceEquals(stage.Descriptor,this)||AllocatedCapacity!=250||backing.Length!=32000||
            Capacity is not (31 or 124)||(uint)Count>31||mutating||resizing||destination.Length!=32000)
            throw new InvalidOperationException("Exact live fullowner250-slot completion snapshot required.");
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);backing.AsSpan().CopyTo(destination);
        token.ThrowIfCancellationRequested();
    }
}
