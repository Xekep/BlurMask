namespace InkRecognition;

/// <summary>Actual case MAIN pooled rebuildfalse D0 and one source68 refresh.
///No public query, startup/history, prepared flags or whole-word result.</summary>
public sealed class LegacyEnglishFreshCaseMainSpanSession
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private LegacyOrdinaryGraphAcousticPermissionSession preparation=null!;
    private LegacyPreparedDictionaryWorkingMap current=null!;
    private LegacyEnglishFreshCaseMainSource.D0Lease? activeLease;
    private Receipt? latest;
    private int active;
    public LegacyEnglishFreshCaseMainScorer Scorer{get;}
    public LegacyEnglishFreshCaseMainSource CaseSource=>Scorer.CaseSource;
    public LegacyEnglishMainD0Bounds Bounds{get;}
    public long LogicalAllocationEnvelopeBytes{get;}
    private LegacyEnglishFreshCaseMainSpanSession(LegacyEnglishFreshCaseMainScorer scorer,LegacyOrdinaryCursiveCandidateGraph actual,LegacyEnglishMainD0Bounds bounds,long envelope)
    {Scorer=scorer;graph=actual;Bounds=bounds;LogicalAllocationEnvelopeBytes=envelope;}
    public static LegacyEnglishFreshCaseMainSpanSession Prepare(LegacyEnglishFreshCaseMainScorer scorer,LegacyEnglishMainD0Bounds bounds,CancellationToken token=default)
        =>PrepareCore(scorer,bounds,null,token);
    internal static LegacyEnglishFreshCaseMainSpanSession PrepareCore(LegacyEnglishFreshCaseMainScorer scorer,LegacyEnglishMainD0Bounds bounds,Action<string>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(scorer);scorer.CaseSource.RequireCurrent();token.ThrowIfCancellationRequested();var local=scorer.CaseSource.Knowledge.Owner;
        long envelope=LegacyEnglishMainSpanSession.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount,bounds);
        if(checked(envelope+scorer.LogicalCopiedPayloadEnvelopeBytes+1048576)>LegacyEnglishFreshCaseMainSource.CompleteLogicalEnvelopeUpperBytes)throw new ArgumentException("Complete additive case envelope exceeded.");
        var graph=local.RetainCompletedD0Graph(token);if(graph.MaximumAtoms!=8)throw new NotSupportedException("Actual ordinary MAIN graph required.");
        long arcs=0,ranges=0;for(int g=0;g<graph.Groups.Count;g++){token.ThrowIfCancellationRequested();ranges=checked(ranges+graph.Groups[g].Ranges.Length);if(ranges>131072)throw new InvalidDataException("Whole actual range bound exceeded.");}
        for(int n=0;n<graph.NodeCount;n++)foreach(var arc in graph.GetNode(n)){token.ThrowIfCancellationRequested();if((uint)arc.GroupId>=(uint)graph.Groups.Count||arc.Span>=8||++arcs>4096)throw new InvalidDataException("Whole actual arc bound exceeded.");}
        var result=new LegacyEnglishFreshCaseMainSpanSession(scorer,graph,bounds,envelope);bool claimed=false;
        try
        {
            var binding=scorer.CaseSource.ClaimD0(result,bounds,token);claimed=true;result.current=binding.Item1;observer?.Invoke("owner:claimed");token.ThrowIfCancellationRequested();
            var reserved=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(scorer.CaseSource.Source.Alphabet,scorer.CaseSource.Source.Alphabet.CopySerializedHeader());
            result.preparation=new(LegacyOrdinaryRecognitionGraph.FromSameSource(graph),result.current,reserved,(group,cancel)=>
            {
                var lease=result.activeLease??throw new InvalidOperationException("Actual D0 computation lease required.");var row=scorer.ScoreForD0(group,lease,cancel);return new(row.CopyCosts(),row.PowerScale);
            },binding.Item2);
            observer?.Invoke("owner:ready");token.ThrowIfCancellationRequested();return result;
        }
        catch{if(claimed)scorer.CaseSource.PoisonD0(result);throw;}
    }
    public Receipt PrepareSpan(int anchor,byte span,CancellationToken token=default)=>PrepareSpanCore(anchor,span,false,null,token);
    internal Receipt PrepareSpanCore(int anchor,byte span,bool unsupportedReuse,Action<string>? observer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(unsupportedReuse)throw new NotSupportedException("Reused-record alias postpass is not admitted by this owner.");
        if((uint)anchor>=(uint)graph.NodeCount||span>=8)throw new ArgumentOutOfRangeException(nameof(anchor));
        int count=graph.GetNode(anchor).Count(a=>a.Span==span);if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("Serialized case D0 required.");
        bool entered=false;
        try
        {
            var lease=CaseSource.BeginD0(this,current,count,token,observer);activeLease=lease;entered=true;observer?.Invoke("span:entered");token.ThrowIfCancellationRequested();
            var pass=preparation.EvaluateForFreshCase(anchor,span,lease,token);observer?.Invoke("span:prepared");token.ThrowIfCancellationRequested();
            if(!pass.Succeeded||!pass.PermissionResult.Succeeded||pass.PermissionResult.WorkingMap is null||pass.SelectedRows.Count!=count||pass.SelectedPositions.Count!=count||lease.Pending is null||!ReferenceEquals(lease.Pending,pass.PermissionResult.WorkingMap))throw new InvalidDataException("Complete actual once-forwarded case D0 required.");
            var rows=new LegacyEnglishMainD0SelectedRow[count];var eligibility=new byte[220];int index=0;var arcs=graph.GetNode(anchor);
            for(int ordinal=0;ordinal<arcs.Count;ordinal++)
            {
                token.ThrowIfCancellationRequested();var arc=arcs[ordinal];if(arc.Span!=span)continue;var selected=pass.SelectedRows[index];
                if(selected.Position!=new LegacyGraphGlyphPosition(anchor,ordinal)||pass.SelectedPositions[index]!=selected.Position||selected.GroupId!=arc.GroupId||selected.Costs.Length!=220)throw new InvalidDataException("Actual pooled callback order required.");
                rows[index++]=new(anchor,ordinal,arc,selected.Costs);for(int symbol=0;symbol<220;symbol++)if(selected.Costs.Span[symbol]<0x4000000)eligibility[symbol]=1;
            }
            var receipt=Receipt.Create(this,lease,anchor,span,rows,pass.PermissionResult,eligibility,pass.ComputedRows,pass.ReusedRows);
            observer?.Invoke("span:snapshot");token.ThrowIfCancellationRequested();CaseSource.Install(lease,token);observer?.Invoke("span:installed");token.ThrowIfCancellationRequested();
            current=pass.PermissionResult.WorkingMap;CaseSource.Complete(lease);latest=receipt;return receipt;
        }
        catch{if(entered)CaseSource.PoisonD0(this);throw;}
        finally{activeLease=null;Volatile.Write(ref active,0);}
    }
    internal bool IsCurrent(Receipt receipt)=>Volatile.Read(ref active)==0&&ReferenceEquals(latest,receipt)&&CaseSource.IsCurrent(this,receipt.Epoch);
    /// <summary>Defensive historical snapshots, not an execution/query lease.</summary>
    public sealed class Receipt
    {
        private readonly LegacyEnglishFreshCaseMainSpanSession owner;
        private readonly LegacyEnglishMainD0SelectedRow[] rows;
        private readonly byte[] raw,expanded,eligibility;
        private readonly LegacyPreparedDictionaryWorkingMap map;
        public int Anchor{get;}
        public byte Span{get;}
        public long Epoch{get;}
        public int ComputedRows{get;}
        public int ReusedRows{get;}
        public bool IsCurrent=>owner.IsCurrent(this);
        public int SelectedRowCount=>rows.Length;
        private Receipt(LegacyEnglishFreshCaseMainSpanSession owner,LegacyEnglishFreshCaseMainSource.D0Lease lease,int anchor,byte span,LegacyEnglishMainD0SelectedRow[] rows,LegacyPreparedAcousticPermissionResult result,byte[] eligibility,int computed,int reused)
        {this.owner=owner;Anchor=anchor;Span=span;Epoch=lease.Version;this.rows=rows;raw=(lease.RawPermissions??throw new InvalidOperationException("Actual raw permission phase required.")).ToArray();expanded=result.Permissions.ToArray();map=result.WorkingMap!;this.eligibility=eligibility;ComputedRows=computed;ReusedRows=reused;}
        internal static Receipt Create(LegacyEnglishFreshCaseMainSpanSession owner,LegacyEnglishFreshCaseMainSource.D0Lease lease,int anchor,byte span,LegacyEnglishMainD0SelectedRow[] rows,LegacyPreparedAcousticPermissionResult result,byte[] eligibility,int computed,int reused)
        {owner.CaseSource.ValidateLease(lease);if(!ReferenceEquals(lease.Owner,owner))throw new InvalidOperationException("Exact actual invocation required.");return new(owner,lease,anchor,span,rows,result,eligibility,computed,reused);}
        public LegacyEnglishMainD0SelectedRow[] CopySelectedRows()=>rows.ToArray();
        public byte[] CopyRawPermissions()=>raw.ToArray();
        public byte[] CopyPermissions()=>expanded.ToArray();
        public byte[] CopyEligibility()=>eligibility.ToArray();
        public ushort[] CopyWorkingMap()=>map.CopySelectedMap(false,0);
    }
}
