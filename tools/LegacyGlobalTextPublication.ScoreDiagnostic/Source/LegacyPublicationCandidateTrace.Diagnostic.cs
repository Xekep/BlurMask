#if BLURMASK_PUBLICATION_DIAGNOSTICS
namespace InkRecognition;

internal sealed class LegacyPublicationCandidateResidues
{
    public byte[] Word120{get;init;}=[];public byte[] Glyph40{get;init;}=[];
    public ushort[] Labels256{get;init;}=[];public byte[] Request64{get;init;}=[];public byte[] Backtrack72{get;init;}=[];
}
internal sealed class LegacyPublicationCandidateBasis
{
    public int First{get;init;}public int Last{get;init;}public int OriginalCount{get;set;}
    public byte[] Compact40{get;init;}=[];public int[] Starts{get;init;}=[];
    public ushort[] SpecialLabels{get;init;}=[];
    public int[] ScanSelected{get;set;}=[];public int[] ScanAttempts4{get;set;}=[];
    public ulong[] ScanFirstReferences{get;set;}=[];public bool ScanApplied{get;set;}
}
internal sealed class LegacyPublicationCandidateBoundary
{
    public int WalkIndex{get;init;}public bool Emit{get;init;}public bool First{get;init;}
    public int Node{get;init;}public ushort Arc{get;init;}public int ResolvedNode{get;init;}public ushort ResolvedArc{get;init;}public int Value{get;init;}
}
internal sealed class LegacyPublicationCandidateDelimiter
{
    public int WalkIndex{get;init;}public bool Emit{get;init;}public byte[] Request64{get;init;}=[];
    public int Remaining{get;init;}public int Emitted{get;init;}public byte FlagC{get;init;}public byte FlagD{get;init;}
}
internal sealed class LegacyPublicationCandidateCleanup
{
    public ulong First{get;init;}public ulong Last{get;init;}
    public List<LegacyPublicationEqualityRead> PreflightRows{get;}=new(8);
    public ulong[] ReversalReferences{get;set;}=[];public ulong[] NewLinks{get;set;}=[];
    public ulong[] PopReferences{get;set;}=[];public int[] CountsBefore{get;set;}=[];public bool[] Removed{get;set;}=[];
    public bool Applied{get;set;}
}
internal sealed class LegacyPublicationCandidateOrdinaryRead
{
    public int WordOrdinal{get;init;}public int ReferenceOrdinal{get;init;}
    public ulong Reference{get;init;}public byte[] RawRecord40{get;init;}=[];
}
internal sealed class LegacyPublicationCandidateFrame
{
    public int Ordinal{get;init;}public int CompactIndex{get;init;}public int OriginalCount{get;init;}
    public int InputCount{get;set;}public int MatchObserved{get;set;}=-2;
    public bool Applied{get;set;}public string OutcomeObserved{get;set;}="Pending";
    public int EmissionStatus{get;set;}public int LastDirectParent{get;set;}
    public int UpdatedMinimum{get;set;}public ushort WordFlags{get;set;}public ulong LastSourceKey{get;set;}
    public byte[] Rows72{get;set;}=[];public byte[] ExecutionRows72{get;set;}=[];
    public int[] Visited{get;set;}=[];public ulong[] ResidualReferences{get;set;}=[];
    public bool ChainObserved{get;set;}public bool ChainSucceeded{get;set;}
    public ulong ChainFirstNewReference{get;set;}public ulong ChainLastProcessedReference{get;set;}
    public int ChainCreatedCount{get;set;}public int ChainReusedCount{get;set;}
    public byte[] PreCostWord120{get;set;}=[];public uint[] ObservedCostBits{get;set;}=[];
    public List<LegacyPublicationEqualityCall> Equalities{get;}=new(2);
    public List<LegacyPublicationCandidateOrdinaryRead> OrdinaryPreflightRows{get;}=new(8);
    public byte[] ExistingDuplicate120{get;set;}=[];
    public bool ClassHeaderObserved{get;set;}public ushort ClassFlags{get;set;}public uint ClassHeader{get;set;}
    public byte[] UpdateBefore120{get;set;}=[];public byte[] UpdateAfter120{get;set;}=[];public bool UpdateApplied{get;set;}
    public bool InternAttempted{get;set;}public bool InternHeadReuse{get;set;}public bool InternApplied{get;set;}
    public int InternOrdinal{get;set;}=-1;public bool InternOwnerC4{get;set;}
    public byte[] InternBefore120{get;set;}=[];public byte[] InternAfter120{get;set;}=[];public ulong[] InternChain{get;set;}=[];public ulong[] InternFlatReferences{get;set;}=[];
    public List<LegacyPublicationEqualityRead> InternGlyphRows{get;}=new(8);
    public LegacyPublicationCandidateCleanup? Cleanup{get;set;}
}
internal sealed class LegacyPublicationCandidateData
{
    public string Scope{get;init;}="AUTHENTIC219580_INPUTS_AND_FATES_NOT_FULL_POOL_ALLOCATOR";
    public bool Truncated{get;init;}public LegacyPublicationCandidateResidues? Residues{get;init;}
    public LegacyPublicationCandidateBasis? Basis{get;init;}
    public LegacyPublicationCandidateFrame[] Candidates{get;init;}=[];
    public LegacyPublicationCandidateBoundary[] Boundaries{get;init;}=[];
    public LegacyPublicationCandidateDelimiter[] Delimiters{get;init;}=[];
}
internal sealed partial class LegacyPublicationScoreTrace
{
    private LegacyPublicationCandidateResidues? candidateResidues;
    private LegacyPublicationCandidateBasis? candidateBasis;
    private readonly List<LegacyPublicationCandidateFrame> candidateFrames=new(4);
    private readonly List<LegacyPublicationCandidateBoundary> candidateBoundaries=new(8);
    private readonly List<LegacyPublicationCandidateDelimiter> candidateDelimiters=new(8);
    private LegacyPublicationCandidateFrame? currentCandidate,equalityCandidate;
    private bool candidateTruncated,candidateTicket;
    private int candidateRawReads;
    internal bool AdmitCandidate(LegacyFreshMainGlobalPlainRecognitionResult actual)
    {
        if(disposed||detached is not null||candidateTruncated)return false;
        if(candidateTicket){candidateTruncated=true;return false;}
        if(!budget.Reserve(64,0,64)){candidateTruncated=true;return false;}
        if(thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(source,actual)||
            !ReferenceEquals(actual.ScoreTrace,this)||!ReferenceEquals(actual.Session,session))
        {candidateTruncated=true;return false;}
        candidateTicket=true;return true;
    }
    private bool CandidateConsume(){if(!candidateTicket)return false;candidateTicket=false;return true;}
    private bool CandidateReserve(long bytes,long headers=512)
    {
        if(bytes<0||!budget.Reserve(bytes+headers,2*bytes,bytes+256)){candidateTruncated=true;return false;}
        return true;
    }
    internal void CaptureCandidateResidues(LegacyGlobalSourceCallerResidue residue)
    {
        // Begin prepays2048/2048/2048 before passing this owner reference.
        candidateResidues=new(){Word120=residue.Word.ToArray(),Glyph40=residue.Glyph.ToArray(),Labels256=residue.Labels.ToArray(),Request64=residue.Request.ToArray(),Backtrack72=residue.Backtrack.ToArray()};
    }
    internal void CandidateResiduesUnavailable(){candidateTruncated=true;}
    internal void CaptureCandidateBasis(int first,int last,byte[] compact,int[] starts,ushort marker,ushort joiner,ushort closed,ushort unit,ushort softHyphen)
    {
        if(!CandidateConsume())return;
        if(candidateBasis is not null||compact.Length%40!=0||compact.Length>131072*40||starts.Length is <2 or >4097)
        {candidateTruncated=true;return;}
        if(!CandidateReserve(compact.Length+4L*starts.Length+10))return;
        candidateBasis=new(){First=first,Last=last,Compact40=compact.ToArray(),Starts=starts.ToArray(),SpecialLabels=[marker,joiner,closed,unit,softHyphen]};
    }
    internal void CaptureCandidateScan(LegacyPreparedMainWordAugmentationCandidates.Result scan,int originalCount)
    {
        if(!CandidateConsume())return;
        int n=scan.Attempts.Length;
        if(candidateBasis is null||n>131072||scan.CompactIndices.Length>131072){candidateTruncated=true;return;}
        if(!CandidateReserve(24L*n+4L*scan.CompactIndices.Length))return;
        int[] details=new int[n*4];ulong[] refs=new ulong[n];
        for(int i=0;i<n;i++){var a=scan.Attempts[i];details[i*4]=a.CompactIndex;details[i*4+1]=a.MinimumNode;details[i*4+2]=a.Status;details[i*4+3]=a.Included?1:0;refs[i]=a.FirstReference;}
        candidateBasis.ScanAttempts4=details;candidateBasis.ScanFirstReferences=refs;candidateBasis.ScanSelected=scan.CompactIndices.ToArray();candidateBasis.OriginalCount=originalCount;candidateBasis.ScanApplied=true;
    }
    internal void CaptureCandidateBoundary(int walk,bool emit,bool first,int node,ushort arc,int resolved,ushort resolvedArc,int value)
    {
        if(!CandidateConsume())return;
        if(candidateBoundaries.Count>=8192||!CandidateReserve(0,128)){candidateTruncated=true;return;}
        candidateBoundaries.Add(new(){WalkIndex=walk,Emit=emit,First=first,Node=node,Arc=arc,ResolvedNode=resolved,ResolvedArc=resolvedArc,Value=value});
    }
    internal void CaptureCandidateDelimiter(int walk,bool emit,byte[] request,int remaining,int count,byte c,byte d)
    {
        if(!CandidateConsume())return;
        if(request.Length!=64||candidateDelimiters.Count>=8192||!CandidateReserve(64,192)){candidateTruncated=true;return;}
        candidateDelimiters.Add(new(){WalkIndex=walk,Emit=emit,Request64=request.ToArray(),Remaining=remaining,Emitted=count,FlagC=c,FlagD=d});
    }
    internal void BeginCandidate(int index,int originalCount)
    {
        if(!CandidateConsume())return;
        if(candidateBasis is null||currentCandidate is not null||candidateFrames.Count>=64||!CandidateReserve(0,2048)){candidateTruncated=true;return;}
        currentCandidate=new(){Ordinal=candidateFrames.Count,CompactIndex=index,OriginalCount=originalCount};candidateFrames.Add(currentCandidate);
    }
    internal void CaptureCandidateEmission(LegacyGlobalSpanAlternativeWalkKernel.Result emission,byte[] execution)
    {
        if(!CandidateConsume())return;
        int n=emission.Rows72.Length;
        if(currentCandidate is null||n%72!=0||n>4096*72||execution.Length!=n||emission.VisitedIndices.Length>131072||emission.ResidualReferences.Length>4096)
        {candidateTruncated=true;return;}
        if(!CandidateReserve(2L*n+4L*emission.VisitedIndices.Length+8L*emission.ResidualReferences.Length))return;
        currentCandidate.EmissionStatus=emission.Status;currentCandidate.LastDirectParent=emission.LastDirectParentIndex;currentCandidate.UpdatedMinimum=emission.UpdatedMinimumNode;
        currentCandidate.WordFlags=emission.WordFlags;currentCandidate.LastSourceKey=emission.LastSourceKey;
        currentCandidate.Rows72=emission.Rows72.ToArray();currentCandidate.ExecutionRows72=execution.ToArray();currentCandidate.Visited=emission.VisitedIndices.ToArray();currentCandidate.ResidualReferences=emission.ResidualReferences.ToArray();
    }
    internal void CaptureCandidateChain(LegacyPreparedGlyphChainResult chain)
    {
        if(!CandidateConsume())return;if(currentCandidate is null){candidateTruncated=true;return;}
        currentCandidate.ChainObserved=true;currentCandidate.ChainSucceeded=chain.Succeeded;
        currentCandidate.ChainFirstNewReference=chain.FirstNewReference;currentCandidate.ChainLastProcessedReference=chain.LastProcessedReference;
        currentCandidate.ChainCreatedCount=chain.CreatedCount;currentCandidate.ChainReusedCount=chain.ReusedCount;
    }
    internal void CaptureCandidateCosts(byte[] before,LegacyPreparedMainAugmentedWordCosts.Result costs)
    {
        if(!CandidateConsume())return;if(currentCandidate is null||before.Length!=120||!CandidateReserve(132)){candidateTruncated=true;return;}
        currentCandidate.PreCostWord120=before.ToArray();currentCandidate.ObservedCostBits=[BitConverter.SingleToUInt32Bits(costs.Score10),BitConverter.SingleToUInt32Bits(costs.Cost18),BitConverter.SingleToUInt32Bits(costs.Cost1C)];
    }
    internal void CaptureCandidateOrdinaryRow(int wordOrdinal,int referenceOrdinal,ulong reference,byte[] record)
    {
        if(!CandidateConsume())return;
        if(currentCandidate is null||record.Length!=40||candidateRawReads>=8192||!CandidateReserve(40,192)){candidateTruncated=true;return;}
        currentCandidate.OrdinaryPreflightRows.Add(new(){WordOrdinal=wordOrdinal,ReferenceOrdinal=referenceOrdinal,Reference=reference,RawRecord40=record.ToArray()});candidateRawReads++;
    }
    internal bool AdmitAugmentationEquality(LegacyFreshMainGlobalPlainRecognitionResult actual)
    {
        if(currentCandidate is null||candidateTruncated)return false;
        if(!AdmitEquality(actual)){candidateTruncated=true;return false;}
        equalityCandidate=currentCandidate;return true;
    }
    internal void UseRankEquality(){equalityCandidate=null;}
    internal void CaptureCandidateMatch(int match,int inputCount)
    {if(!CandidateConsume())return;if(currentCandidate is null){candidateTruncated=true;return;}currentCandidate.MatchObserved=match;currentCandidate.InputCount=inputCount;}
    internal void CaptureCandidateDuplicate(byte[] existing)
    {if(!CandidateConsume())return;if(currentCandidate is null||existing.Length!=120||!CandidateReserve(120)){candidateTruncated=true;return;}currentCandidate.ExistingDuplicate120=existing.ToArray();}
    internal void CaptureCandidateClassHeader(ushort flags,uint header)
    {if(!CandidateConsume())return;if(currentCandidate is null){candidateTruncated=true;return;}currentCandidate.ClassHeaderObserved=true;currentCandidate.ClassFlags=flags;currentCandidate.ClassHeader=header;}
    internal void EndCandidate(string outcome)
    {if(!CandidateConsume())return;if(currentCandidate is null){candidateTruncated=true;return;}currentCandidate.OutcomeObserved=outcome;currentCandidate.Applied=true;currentCandidate=null;}
    internal bool CandidateAdmitCurrent(LegacyNativeGlyphRecordPool actualPool)
    {
        if(disposed||detached is not null||candidateTruncated)return false;
        if(!budget.Reserve(0,0,64)){candidateTruncated=true;return false;}
        if(source is null||session is null||!ReferenceEquals(actualPool,session.Ownership.GlyphPool))
        {candidateTruncated=true;return false;}
        return AdmitCandidate(source);
    }
    internal void CaptureInternBefore(byte[] data,ulong[] chain,bool ownerC4)
    {
        if(!CandidateConsume())return;
        if(currentCandidate is null||data.Length!=120||chain.Length>4096||!CandidateReserve(120+8L*chain.Length)){candidateTruncated=true;return;}
        currentCandidate.InternAttempted=true;currentCandidate.InternBefore120=data.ToArray();currentCandidate.InternChain=chain.ToArray();currentCandidate.InternOwnerC4=ownerC4;
    }
    internal void CaptureInternGlyph(ulong reference,byte[] record)
    {
        if(!CandidateConsume())return;
        if(currentCandidate is null||record.Length!=40||candidateRawReads>=8192||!CandidateReserve(40,192)){candidateTruncated=true;return;}
        currentCandidate.InternGlyphRows.Add(new(){Reference=reference,RawRecord40=record.ToArray()});candidateRawReads++;
    }
    internal void CaptureInternApplied(byte[] data,ulong[] flat,int index)
    {
        if(!CandidateConsume())return;
        if(currentCandidate is null||data.Length!=120||flat.Length>4096||!CandidateReserve(120+8L*flat.Length)){candidateTruncated=true;return;}
        currentCandidate.InternAfter120=data.ToArray();currentCandidate.InternFlatReferences=flat.ToArray();currentCandidate.InternOrdinal=index;currentCandidate.InternApplied=true;
    }
    internal void CaptureInternReused(int index,ulong[] chain)
    {
        if(!CandidateConsume())return;
        if(currentCandidate is null||chain.Length>4096||!CandidateReserve(8L*chain.Length)){candidateTruncated=true;return;}
        currentCandidate.InternAttempted=true;currentCandidate.InternHeadReuse=true;currentCandidate.InternOrdinal=index;currentCandidate.InternChain=chain.ToArray();currentCandidate.InternApplied=true;
    }
    internal void CaptureUpdateBefore(byte[] data)
    {if(!CandidateConsume())return;if(currentCandidate is null||data.Length!=120||!CandidateReserve(120)){candidateTruncated=true;return;}currentCandidate.UpdateBefore120=data.ToArray();}
    internal void CaptureUpdateApplied(byte[] data)
    {if(!CandidateConsume())return;if(currentCandidate is null||data.Length!=120||!CandidateReserve(120)){candidateTruncated=true;return;}currentCandidate.UpdateAfter120=data.ToArray();currentCandidate.UpdateApplied=true;}
    internal void BeginCleanup(ulong first,ulong last)
    {if(!CandidateConsume())return;if(currentCandidate is null||!CandidateReserve(0,512)){candidateTruncated=true;return;}currentCandidate.Cleanup=new(){First=first,Last=last};}
    internal void CaptureCleanupRead(ulong reference,byte[] record)
    {
        if(!CandidateConsume())return;var cleanup=currentCandidate?.Cleanup;
        if(cleanup is null||record.Length!=40||candidateRawReads>=8192||!CandidateReserve(40,192)){candidateTruncated=true;return;}
        cleanup.PreflightRows.Add(new(){Reference=reference,RawRecord40=record.ToArray()});candidateRawReads++;
    }
    internal LegacyPublicationCandidateCleanup? PrepareCleanupMutation(int count)
    {
        // Called BEFORE the cleanup's final token check and ALL actual writes.
        if(!CandidateConsume())return null;var cleanup=currentCandidate?.Cleanup;
        if(cleanup is null||count is <1 or >4096||!CandidateReserve(29L*count,512)){candidateTruncated=true;return null;}
        cleanup.ReversalReferences=new ulong[count];cleanup.NewLinks=new ulong[count];cleanup.PopReferences=new ulong[count];cleanup.CountsBefore=new int[count];cleanup.Removed=new bool[count];return cleanup;
    }
    private LegacyPublicationCandidateData ExportCandidates()
    {
        // Root/list final array slots are charged per event and fixed Begin.
        return new(){Truncated=candidateTruncated||candidateTicket||currentCandidate is not null||candidateResidues is null||candidateBasis is null||!candidateBasis.ScanApplied,Residues=candidateResidues,Basis=candidateBasis,
            Candidates=candidateFrames.ToArray(),Boundaries=candidateBoundaries.ToArray(),Delimiters=candidateDelimiters.ToArray()};
    }
    private void ClearCandidateBindings(){if(currentCandidate is not null||candidateTicket)candidateTruncated=true;currentCandidate=null;equalityCandidate=null;candidateTicket=false;}
}
#endif
