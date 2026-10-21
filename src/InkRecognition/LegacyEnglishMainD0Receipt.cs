namespace InkRecognition;

/// <summary>Actual immutable selected graph arc and current pooled220 costs.
///Readability survives later epochs; source-query authority does not.</summary>
public sealed class LegacyEnglishMainD0SelectedRow
{
    private readonly ReadOnlyMemory<int> costs;
    public int Anchor{get;}
    public int ArcIndex{get;}
    public int GroupId{get;}
    public byte Span{get;}
    public byte Flags{get;}
    public uint StartContextId{get;}
    public uint EndContextId{get;}
    internal LegacyEnglishMainD0SelectedRow(int anchor,int ordinal,LegacyOrdinaryCursiveCandidateArc actual,ReadOnlyMemory<int> ownedSnapshot)
    {Anchor=anchor;ArcIndex=ordinal;GroupId=actual.GroupId;Span=actual.Span;Flags=actual.Flags;StartContextId=actual.StartContextId;EndContextId=actual.EndContextId;costs=ownedSnapshot;}
    public int[] CopyCosts()=>costs.ToArray();
    internal ReadOnlySpan<int> Costs=>costs.Span;
}

/// <summary>Sealed real rebuilt D0 result. Not a request80, native pointer,
///configured full-root controller or a word-decoder result.</summary>
public sealed class LegacyEnglishMainD0Receipt
{
    private readonly LegacyEnglishMainD0SelectedRow[] rows;
    private readonly byte[] permissions,eligibility;
    private readonly LegacyPreparedDictionaryWorkingMap map;
    private readonly LegacyEnglishD0MapLease lease;
    internal LegacyEnglishMainSpanSession Owner=>lease.Owner;
    public LegacyEnglishFreshDictionaryQuerySession QuerySession=>lease.Binding;
    public LegacyEnglishMainOnlyAcousticScorer Scorer=>QuerySession.Scorer;
    public LegacyEnglishMainOnlySourceMasks Masks=>Scorer.Masks;
    public LegacySource247WireLayout Source=>Masks.Source;
    public int Anchor{get;}
    public byte Span{get;}
    public long Epoch=>lease.Version;
    public int ComputedRows{get;}
    public int ReusedRows{get;}
    public int SelectedRowCount=>rows.Length;
    public bool IsCurrent=>Owner.IsCurrent(this);
    internal LegacyEnglishMainD0Receipt(LegacyEnglishD0MapLease lease,int anchor,byte span,
        LegacyEnglishMainD0SelectedRow[] ownedRows,LegacyPreparedAcousticPermissionResult result,
        byte[] ownedEligibility,int computed,int reused)
    {
        this.lease=lease;Anchor=anchor;Span=span;rows=ownedRows;map=result.WorkingMap??throw new InvalidDataException("Successful actual D0 source refresh required.");
        permissions=result.Permissions.ToArray();eligibility=ownedEligibility;ComputedRows=computed;ReusedRows=reused;
    }
    public LegacyEnglishMainD0SelectedRow[] CopySelectedRows()=>rows.ToArray();
    public byte[] CopyPermissions()=>permissions.ToArray();
    public byte[] CopyEligibility()=>eligibility.ToArray();
    public ushort[] CopyWorkingMap()=>map.CopySelectedMap(false,0);
    // Observational validation only, NOT an execution lease. A future query
    // consumer must reserve the SAME binding gate atomically against BeginD0.
    internal void RequireCurrentBinding(LegacyEnglishFreshDictionaryQuerySession actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(QuerySession,actual)||!IsCurrent)throw new InvalidOperationException("Current same-binding D0 receipt required; historical or foreign snapshots have no query authority.");
    }
    internal LegacyPreparedDictionaryWorkingMap TakeMapForInstallation(LegacyEnglishD0MapLease actual)
    {if(!ReferenceEquals(actual,lease))throw new InvalidOperationException("Same actual D0 invocation lease required.");return map;}
}
