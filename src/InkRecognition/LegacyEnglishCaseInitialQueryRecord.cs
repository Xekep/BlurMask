namespace InkRecognition;

//Defensive historical component output. Never a word/history/terminal authority.
internal sealed class LegacyEnglishCaseInitialQueryRecord
{
    private readonly byte[] records;
    internal int Skip{get;}
    internal int Count{get;}
    internal int Accepted{get;}
    internal int Slot{get;}
    internal int BackendCalls{get;}
    internal long Epoch{get;}
    private LegacyEnglishCaseInitialQueryRecord(long epoch,int skip,LegacyNativeDelimiterQueryPage page)
    {Epoch=epoch;Skip=skip;Count=page.ReturnedCount;Accepted=page.AcceptedCount;Slot=page.SlotIndex;BackendCalls=page.BackendCalls;records=page.Records.ToArray();}
    internal static LegacyEnglishCaseInitialQueryRecord Issue(LegacyEnglishCaseInitialQueryPass owner,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,int skip,LegacyNativeDelimiterQueryPage page)
    {lease.Root.ValidateInitialQuery(lease);if(!ReferenceEquals(owner,lease.Owner))throw new InvalidOperationException("Exact active page issuer required.");return new(lease.Epoch,skip,page);}
    internal byte[] CopyRecords()=>records.ToArray();
}
