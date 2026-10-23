namespace InkRecognition;

// Historical primary-only bytes. This type deliberately has no parent/history API.
internal sealed class LegacyEnglishCasePrimaryCompletionReceipt
{
    private readonly byte[] rows,alternatives,rowMasks,alternativeMasks;
    private readonly int[] ordinals;
    internal int Count{get;}
    internal int InitialCount{get;}
    internal int ContextNodes{get;}
    internal int ReservedContextResolutions{get;}
    internal long LogicalPayloadBytes{get;}
    internal long ReservedCopyWorkBytes{get;}
    internal LegacyEnglishCasePrimaryCompletionReceipt(byte[] rows,byte[] alternatives,byte[] rowMasks,byte[] alternativeMasks,
        int[] ordinals,int count,int initialCount,int contexts,int resolutions,long payload,long copyWork)
    {
        this.rows=rows;this.alternatives=alternatives;this.rowMasks=rowMasks;this.alternativeMasks=alternativeMasks;
        this.ordinals=ordinals;Count=count;InitialCount=initialCount;ContextNodes=contexts;
        ReservedContextResolutions=resolutions;LogicalPayloadBytes=payload;ReservedCopyWorkBytes=copyWork;
    }
    //Exactly one export, included in the execution ledger. Arrays are historical
    //copies; neither these bytes nor equal bytes can be attached to another owner.
    private int exported;
    internal LegacyEnglishCasePrimaryCompletionSnapshot Export(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref exported,1,0)!=0)throw new InvalidOperationException("One budgeted primary history export required.");
        var result=new LegacyEnglishCasePrimaryCompletionSnapshot(rows.ToArray(),alternatives.ToArray(),rowMasks.ToArray(),alternativeMasks.ToArray(),ordinals.ToArray(),Count);
        token.ThrowIfCancellationRequested();return result;
    }
}

internal sealed record LegacyEnglishCasePrimaryCompletionSnapshot(byte[] Rows88,byte[] PrimaryAlternatives80,
    byte[] RowDefinedBitMasks,byte[] AlternativeDefinedBitMasks,int[] SourceOrdinals,int Count);
