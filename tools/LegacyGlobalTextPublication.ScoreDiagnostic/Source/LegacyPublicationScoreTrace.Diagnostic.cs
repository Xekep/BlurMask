#if BLURMASK_PUBLICATION_DIAGNOSTICS
using System.Buffers.Binary;
namespace InkRecognition;

// Diagnostic-only variant. No state here is a source/model/query issuer.
internal sealed class LegacyPublicationScoreBudget
{
    internal const long Limit=8L*1024*1024;
    internal long Payload{get;private set;}
    internal long Copies{get;private set;}
    internal long Reads{get;private set;}
    internal bool Reserve(long payload,long copies,long reads)
    {
        if(payload<0||copies<0||reads<0||payload>Limit-Payload||copies>2*Limit-Copies||reads>4*Limit-Reads)return false;
        Payload+=payload;Copies+=copies;Reads+=reads;return true;
    }
}
internal sealed class LegacyPublicationScorePhase
{
    public string Phase{get;init;}="";
    public int First{get;init;}
    public int Last{get;init;}
    public int CarriedQuota{get;init;}
    public int? QuotaAfter{get;init;}
    public byte[][] Words120{get;init;}=[];
    public ulong[][] FlatReferences{get;init;}=[];
    public uint[] FourChannelBits{get;init;}=[];
    public int[] SelectedOrdinals{get;init;}=[];
    public int[] OldToNew{get;init;}=[];
    public int[] OutputSourceOrdinals{get;init;}=[];
    public int[] Released{get;init;}=[];
    public int[] ReleasedSourceOrdinals{get;init;}=[];
    public byte[][] RankingByOriginalIndex20{get;init;}=[];
    public int[] ScoreOrder{get;init;}=[];
}
internal sealed class LegacyPublicationSelectedScore
{
    public string Phase{get;init;}="";
    public int SelectedOrdinal{get;init;}
    public byte[] Word120{get;init;}=[];
    public ulong[] FlatReferences{get;init;}=[];
}
internal sealed class LegacyPublicationEqualityRead
{
    public ulong Reference{get;init;}
    public byte[] RawRecord40{get;init;}=[];
}
internal sealed class LegacyPublicationEqualityCall
{
    public int Ordinal{get;init;}
    public byte[] LeftWord120{get;init;}=[];
    public byte[] RightWord120{get;init;}=[];
    public byte[] LeftBefore4{get;init;}=[];
    public byte[] RightBefore4{get;init;}=[];
    public ushort IgnoredPrimaryLabel{get;init;}
    public string Mapper{get;init;}="Identity2360";
    public string ControlLookup{get;init;}="Null";
    public bool Applied{get;set;}
    public bool? ObservedEqual{get;set;}
    public byte[] LeftAfter4{get;set;}=[];
    public byte[] RightAfter4{get;set;}=[];
    public LegacyPublicationEqualityRead[] ReadEvents{get;set;}=[];
}
internal sealed class LegacyPublicationScoreData
{
    public string Scope{get;init;}="SCORE_ONLY_NOT_AUGMENTATION_POOL_REPLAY";
    public bool Truncated{get;init;}
    public bool ParentObserved{get;init;}
    public string ParentFrontier{get;init;}="NotObserved: six-case source-only driver does not construct a WORD-text parent; batch parents omitted.";
    public LegacyPublicationCandidateData? CandidateTrace{get;init;}
    public bool EqualityTruncated{get;init;}
    public LegacyPublicationEqualityCall[] Equalities{get;init;}=[];
    public LegacyPublicationScorePhase[] Phases{get;init;}=[];
    public LegacyPublicationSelectedScore[] Selected{get;init;}=[];
    public long PayloadCharge{get;init;}
    public long CopyCharge{get;init;}
    public long ReadCharge{get;init;}
}
internal sealed partial class LegacyPublicationScoreTrace:IDisposable
{
    private LegacyFreshMainGlobalPlainRecognitionResult? source;
    private LegacyFreshMainGlobalGlyphExecutionSession? session;
    private readonly LegacyPublicationScoreBudget budget;
    private readonly int thread=Environment.CurrentManagedThreadId;
    private readonly List<LegacyPublicationScorePhase> phases=new(3);
    private readonly List<LegacyPublicationSelectedScore> selected=new(2);
    private bool disposed,truncated,parentObserved,ticket;
    private int admissionAttempts;
    private LegacyPublicationScoreData? detached;
    private readonly List<LegacyPublicationEqualityCall> equalities=new(8);
    private LegacyPublicationEqualityCall? activeEquality;
    private List<LegacyPublicationEqualityRead>? activeReads;
    private bool equalityTruncated,equalityTicket;
    private int equalityReadCount,equalityCalls;
    // No observer arguments/vector reads are evaluated unless this succeeds.
    internal bool AdmitEquality(LegacyFreshMainGlobalPlainRecognitionResult actual)
    {
        if(disposed||detached is not null)return false;
        if(equalityTruncated||activeEquality is not null||equalityTicket||equalityCalls>=256)
        {equalityTruncated=true;return false;}
        // Pays bound-guard metadata BEFORE source/session/thread field checks.
        if(!budget.Reserve(0,0,64)){equalityTruncated=true;return false;}
        if(thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(source,actual)||
            !ReferenceEquals(actual.ScoreTrace,this)||!ReferenceEquals(actual.Session,session))
        {equalityTruncated=true;return false;}
        // Fixed call inputs/outputs, DTO/list/delegate and call-array envelope.
        if(!budget.Reserve(4096,512,1024)){equalityTruncated=true;return false;}
        equalityTicket=true;return true;
    }
    internal void BeginEquality(ReadOnlySpan<byte> leftWord,ReadOnlySpan<byte> rightWord,
        ReadOnlySpan<byte> leftScratch,ReadOnlySpan<byte> rightScratch,ushort ignored)
    {
        if(!equalityTicket)return;equalityTicket=false;
        if(leftWord.Length!=120||rightWord.Length!=120||leftScratch.Length!=4||rightScratch.Length!=4)
        {equalityTruncated=true;return;}
        activeReads=new(16);
        equalityCalls++;
        activeEquality=new(){Ordinal=equalityCandidate?.Equalities.Count??equalities.Count,LeftWord120=leftWord.ToArray(),RightWord120=rightWord.ToArray(),
            LeftBefore4=leftScratch.ToArray(),RightBefore4=rightScratch.ToArray(),IgnoredPrimaryLabel=ignored};
    }
    // Existing CompareCore Read already materialized this same physical row.
    // Never queries/copies the pool a second time or consumes a prepared row.
    internal void CaptureEqualityRead(ulong reference,byte[] record)
    {
        if(activeEquality is null||activeReads is null||equalityTruncated)return;
        if(equalityReadCount>=8192||!budget.Reserve(256,128,128))
        {equalityTruncated=true;return;}
        if(record.Length!=40){equalityTruncated=true;return;}
        activeReads.Add(new(){Reference=reference,RawRecord40=record.ToArray()});equalityReadCount++;
    }
    internal void EndEquality(bool result,ReadOnlySpan<byte> leftScratch,ReadOnlySpan<byte> rightScratch)
    {
        if(activeEquality is null||activeReads is null)return;
        activeEquality.ObservedEqual=result;activeEquality.LeftAfter4=leftScratch.ToArray();activeEquality.RightAfter4=rightScratch.ToArray();
        activeEquality.ReadEvents=activeReads.ToArray();activeEquality.Applied=!equalityTruncated;
        if(equalityCandidate is not null)equalityCandidate.Equalities.Add(activeEquality);else equalities.Add(activeEquality);
        activeEquality=null;activeReads=null;equalityCandidate=null;
    }
    internal void AbortEquality()
    {
        equalityTruncated=true;equalityTicket=false;
        if(activeEquality is not null&&activeReads is not null)
        {activeEquality.ReadEvents=activeReads.ToArray();if(equalityCandidate is not null)equalityCandidate.Equalities.Add(activeEquality);else equalities.Add(activeEquality);}
        activeEquality=null;activeReads=null;equalityCandidate=null;
    }
    internal LegacyPublicationScoreTrace(LegacyFreshMainGlobalPlainRecognitionResult source,LegacyPublicationScoreBudget budget)
    {this.source=source;session=source.Session;this.budget=budget;}
    // Call BEFORE evaluating observer arguments or any vector/metadata getters.
    internal bool Admit(LegacyFreshMainGlobalPlainRecognitionResult actual)
    {
        // At most16 guard attempts, included in the fixed Begin read allowance.
        if(disposed||detached is not null||admissionAttempts++>=16)return false;
        if(thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(source,actual)||
            !ReferenceEquals(actual.ScoreTrace,this)||!ReferenceEquals(actual.Session,session))return false;
        if(ticket){truncated=true;return false;}
        if(truncated||!budget.Reserve(512,0,131072)){truncated=true;return false;}
        ticket=true;return true;
    }
    private bool Consume(){if(!ticket)return false;ticket=false;return true;}
    private void Stop(){truncated=true;}
    internal void Capture(string phase,LegacyNativePlainWordSpanSnapshot span,byte[][] rows,IReadOnlyList<int> ordinals,
        int quota,LegacyPreparedMainWordRanking.Result? rank,int[]? order)
    {
        if(!Consume())return;
        int n=rows.Length,s=ordinals.Count;
        if(phases.Count>=3||n is <1 or >4095||span.Words.Count!=n||s is <1 or >4096){Stop();return;}
        long refs=0;
        for(int i=0;i<n;i++)
        {
            if(rows[i].Length!=120){Stop();return;}
            refs+=span.Words[i].FlatReferences.Count;
            if(refs>65536){Stop();return;}
        }
        long maps=(rank is null?0L:rank.OldToNew.Length+rank.OutputSourceOrdinals.Length+rank.Released.Length+rank.ReleasedSourceOrdinals.Length)+(order?.Length??0);
        if(maps>6L*4095||rank is not null&&(rank.RankingByOriginalIndex.Length!=n||rank.RankingByOriginalIndex.Any(r=>r.Length!=20))){Stop();return;}
        long raw=136L*n+8*refs+4*(s+maps)+(rank is null?0:20L*n);
        long payload=raw+256+64*(3L*n+12);
        if(!budget.Reserve(payload,2*raw,raw+16L*n)){Stop();return;}
        byte[][] words=new byte[n][],ranking=rank is null?[]:new byte[n][];
        ulong[][] flat=new ulong[n][];uint[] channels=new uint[n*4];
        for(int i=0;i<n;i++)
        {
            words[i]=rows[i].ToArray();flat[i]=span.Words[i].FlatReferences.ToArray();
            for(int j=0;j<4;j++)channels[i*4+j]=BinaryPrimitives.ReadUInt32LittleEndian(rows[i].AsSpan(16+j*4));
            if(rank is not null)ranking[i]=rank.RankingByOriginalIndex[i].ToArray();
        }
        phases.Add(new(){Phase=phase,First=span.Span.First,Last=span.Span.Last,CarriedQuota=quota,QuotaAfter=rank?.QuotaAfter,
            Words120=words,FlatReferences=flat,FourChannelBits=channels,SelectedOrdinals=ordinals.ToArray(),
            OldToNew=rank?.OldToNew.ToArray()??[],OutputSourceOrdinals=rank?.OutputSourceOrdinals.ToArray()??[],
            Released=rank?.Released.ToArray()??[],ReleasedSourceOrdinals=rank?.ReleasedSourceOrdinals.ToArray()??[],
            RankingByOriginalIndex20=ranking,ScoreOrder=order?.ToArray()??[]});
    }
    internal void CaptureSelected(string phase,int ordinal,ReadOnlySpan<byte> word,IReadOnlyList<ulong> refs,bool isParent=false)
    {
        if(!Consume())return;
        int n=refs.Count;
        if(selected.Count>=2||word.Length!=120||n is <0 or >4096){Stop();return;}
        long raw=120L+8L*n;
        if(!budget.Reserve(raw+512,2*raw,raw+64)){Stop();return;}
        selected.Add(new(){Phase=phase,SelectedOrdinal=ordinal,Word120=word.ToArray(),FlatReferences=refs.ToArray()});
        parentObserved|=isParent;
    }
    internal LegacyPublicationScoreData Export()
    {
        // The bounded outer arrays/receipt are prepaid by Begin. Engine arrays were detached at capture.
        return detached??=new(){Truncated=truncated||ticket,ParentObserved=parentObserved,
            CandidateTrace=ExportCandidates(),EqualityTruncated=equalityTruncated||activeEquality is not null||equalityTicket,Equalities=equalities.ToArray(),
            Phases=phases.ToArray(),Selected=selected.ToArray(),PayloadCharge=budget.Payload,CopyCharge=budget.Copies,ReadCharge=budget.Reads};
    }
    public void Dispose()
    {
        if(disposed)return;disposed=true;
        if(source is not null&&ReferenceEquals(source.ScoreTrace,this))source.ScoreTrace=null;
        source=null;session=null;ticket=false;equalityTicket=false;activeEquality=null;activeReads=null;ClearCandidateBindings();
    }
}
public sealed partial class LegacyLibraryGlobalTextPublicationScope
{
    internal LegacyPublicationScoreTrace? DiagnosticScoreTrace=>result?.ScoreTrace;
    internal LegacyPublicationScoreTrace? BeginPublicationScoreTrace(LegacyPublicationScoreBudget budget)
    {
        lock(gate)
        {
            // Fixed admission precedes source metadata and collector/list allocation.
            // Includes at most16 guard attempts and bounded Export receipt/outer arrays.
            if(!budget.Reserve(16384,0,256))return null;
            Validate();
            if(result is null||!result.PublicationCompleted||enteredCompletion||result.Session is null||result.ScoreTrace is not null)return null;
            var capture=new LegacyPublicationScoreTrace(result,budget);result.ScoreTrace=capture;
            if(completionResidue is not null&&budget.Reserve(2048,2048,2048))capture.CaptureCandidateResidues(completionResidue);
            else capture.CandidateResiduesUnavailable();
            return capture;
        }
    }
}
#endif
