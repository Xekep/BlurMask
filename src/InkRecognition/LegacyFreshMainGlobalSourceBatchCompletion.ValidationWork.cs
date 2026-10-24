namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalSourceBatchCompletion
{
    private bool validationMeterInitialized;
    private long validationVisitLimit,validationVisits;
    private long associationVisits,associationCopyReserve;
    public long EnumeratedAssociationLoopReserve=>associationVisits;
    public long EnumeratedAssociationCopyReserve=>associationCopyReserve;
    private long associatedResidualInputs,associatedRows,associationMergeRequests,newAssociatedExtraSources;
    public long AssociatedResidualInputs=>associatedResidualInputs;
    public long AssociatedRows=>associatedRows;
    public long AssociationMergeRequests=>associationMergeRequests;
    public long NewAssociatedExtraSources=>newAssociatedExtraSources;
    public long AssociationResolvedWithoutNewExtra=>associationMergeRequests-newAssociatedExtraSources;
    private void RecordAssociation(int residual,int rows,int merges,int newExtras)
    {
        if(residual<0||rows<0||merges<0||merges>rows||newExtras<0||newExtras>merges)
            throw new InvalidDataException("Actual residual/source cache accounting changed.");
        associatedResidualInputs+=residual;associatedRows+=rows;associationMergeRequests+=merges;newAssociatedExtraSources+=newExtras;
    }
    private int validationMeterThread;
    //DATA counters, not a receipt/issuer and not a helper/CPU/RSS measurement.
    public long EnumeratedValidationLoopBodies=>validationVisits;
    public long MaximumValidationLoopBodies=>validationVisitLimit;
    private void InitializeValidationMeter(long preflight)
    {
        int currentThread=Environment.CurrentManagedThreadId;
        int existing=Interlocked.CompareExchange(ref validationMeterThread,currentThread,0);
        if(existing!=0&&existing!=currentThread)throw new InvalidOperationException("Same batch validation-meter thread required before entry.");
        if(Volatile.Read(ref validationMeterInitialized))return; //No refund/reset on retry.
        if(preflight<0||preflight>maximumWork)throw new NotSupportedException("Batch validation work policy cannot initialize beyond preflight envelope.");
        validationVisitLimit=maximumWork-preflight;Volatile.Write(ref validationMeterInitialized,true);
    }
    private void ChargeAssociation(long work,long copied,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!Volatile.Read(ref validationMeterInitialized)||Volatile.Read(ref validationMeterThread)!=Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Initialized same-thread actual batch association budget required.");
        if(work<0||work>validationVisitLimit-validationVisits-associationVisits||copied<0||
            copied>maximumCopy-PreflightEnumeratedCopyBytes-earlyTextPreparationCopyBytes-associationCopyReserve)
            throw new NotSupportedException("Actual residual association exceeds remaining batch structural/copy policy before expansion.");
        associationVisits+=work;associationCopyReserve+=copied;
    }
    internal void RequireValidationBinding(LegacyNativePlainWordPublisher actual,LegacyNativePlainWordBatchBinding actualBinding)
    {if(!ReferenceEquals(actual,publisher)||!ReferenceEquals(actualBinding,binding))throw new InvalidOperationException("Same private batch validation budget/binding required.");}
    internal void DebitValidationLoops(long count,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //Preflight entry scans are separately bounded before initialization.
        int boundThread=Volatile.Read(ref validationMeterThread);
        if(boundThread!=0&&boundThread!=Environment.CurrentManagedThreadId)throw new InvalidOperationException("Same batch validation-meter thread required.");
        if(!Volatile.Read(ref validationMeterInitialized))return;
        if(count<0||count>validationVisitLimit-validationVisits-associationVisits)
            throw new NotSupportedException("Batch cumulative source-validation loop policy exhausted before traversal.");
        validationVisits+=count;
    }
}
