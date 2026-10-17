using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Bounded TOP.V68 for one actual fresh first-alternative WORD/span
/// with no active compact row carrying bit8. This closes the genuine empty
///219580 candidate branch,13D360,275.V08 and the absent-RU-checkbox guard.
/// It does not implement enabled WORD augmentation or0283B0 acceptance.</summary>
public sealed class LegacyFreshMainNoCandidateSourceCompletion
{
    private sealed class Claim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyFreshMainPlainResultOwnership,Claim> claims=new();
    public LegacyOwnedMainWordSourceCollection Sources {get;}
    public LegacyFreshMainCompletedFirstCandidate Candidate=>Sources.Candidate;
    public bool SourcesMarkedComplete {get;private set;}
    public bool RankingApplied {get;private set;}
    public bool SpanScoresApplied {get;private set;}
    public bool CheckboxGuardVerified {get;private set;}
    public bool IsTopV68Completed {get;private set;}
    public bool IsFaulted {get;private set;}
    private bool started;
    private readonly object gate=new();
    private LegacyFreshMainNoCandidateSourceCompletion(LegacyOwnedMainWordSourceCollection sources){Sources=sources;}

    public static LegacyFreshMainNoCandidateSourceCompletion Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyOwnedMainWordSourceCollection actualSources,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSources);token.ThrowIfCancellationRequested();
        var candidate=actualSources.Candidate;var raw=candidate.RawResult;var ownership=raw.Session.Ownership;
        var policy=candidate.Snapshot.SourcePolicy;
        if(acousticResource.Length!=572763||!Convert.ToHexString(SHA256.HashData(acousticResource)).Equals(
            policy.Controller.Owner.SelectedSourceOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The actual owning acoustic resource is required.");
        if(candidate.Snapshot.ActiveAlternativeCount60!=1||candidate.Snapshot.PublishedWordCount!=1||
            candidate.RawWordsInNativeHeadOrder.Count!=1||actualSources.Word.Next is not null||
            !ReferenceEquals(actualSources.Word,candidate.Snapshot.WordHead18)||
            !ReferenceEquals(candidate.Snapshot.WordHead18,ownership.WordPublisher.Head)||
            !ReferenceEquals(candidate.Snapshot.Session,raw.Session)||!raw.PublicationCompleted||
            !raw.Session.IsComplete||raw.Session.IsStopped||policy.HasAssociatedSource||
            policy.Controller.SecondaryScaleEc!=0||policy.Controller.SecondaryCountProperty12d0003 is not null||
            policy.Model.SourceKind!=0x135||policy.Controller.Configuration.Trace2368!=0)
            throw new NotSupportedException("One genuine completed fresh kind135 first alternative/WORD and its exact source prefix required.");
        var compact=ownership.Materialization.CopyCompactStorage();int count=ownership.Materialization.RecordCount;
        if(count<1||compact.Length<count*40)throw new InvalidDataException("Actual complete active compact extent required.");
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();
            if((compact[i*40+32]&8)!=0)
                throw new NotSupportedException("Enabled219580 candidate augmentation needs its actual218A30/218560/WORD owner; no candidate is discarded.");
        }
        //13D000 enters no rewrite when all three lookup results areFFFF:
        //its selected scalar must first be!=FFFF before any equality gate.
        //This is the actual primary AL of the same sealed raw main owner.
        var alphabet=policy.Model.AcousticAlphabet;
        foreach(string literal in new[]{"\u2610","\u2611","\u2612"})
            if(alphabet.TryGetLabel(literal,out _))throw new NotSupportedException("The enabled checkbox rewrite requires its actual owner.");
        var span=ownership.WordPublisher.CaptureSpan(actualSources.Word,[candidate.Snapshot.WordHead18],token);
        if(span.Words.Count!=1||span.SelectedNodes.Count!=1||span.SelectedOrdinals[0]!=0)
            throw new NotSupportedException("The current bounded empty-augmentation proof owns a singleton275 entry.");
        token.ThrowIfCancellationRequested();return new(actualSources);
    }

    public void Run(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(started)throw new InvalidOperationException("The source completion retains an entered prefix and cannot be retried.");
            var ownership=Candidate.RawResult.Session.Ownership;var publisher=ownership.WordPublisher;
            var head=Candidate.Snapshot.WordHead18;
            var span=publisher.CaptureSpan(Sources.Word,[head],token);
            if(span.Words.Count!=1||span.SelectedNodes.Count!=1||span.SelectedOrdinals[0]!=0)
                throw new InvalidOperationException("The prepared singleton source/span entry is stale.");
            if(Interlocked.CompareExchange(ref claims.GetValue(ownership.TopOwner,static _=>new()).Started,1,0)!=0)
                throw new InvalidOperationException("This actual TOP already entered its source completion.");
            started=true;
            try
            {
                //The genuine count/fill26N sidecar already owns the successful
                //219580 prefix. No bit8 row exists, so no218A30 call is skipped.
                //FrontendV228's fresh numerical invalidation is stateless here;
                //it does not clear or replace the actual higher-source overlay.
                SourcesMarkedComplete=true;
                var rank=LegacyPreparedMainWordRanking.Rank(span.Words.Select(w=>w.CopyStorage()).ToArray(),
                    span.SelectedOrdinals.ToArray(),LegacyPreparedMainWordRanking.InitialQuota(65535,65535,false),false,
                    static (left,right)=>throw new InvalidOperationException("A singleton native rank must not invoke WORD comparison."),token);
                publisher.ApplySpanRanking(span,rank.Words,rank.OutputSourceOrdinals,rank.OldToNew,token);RankingApplied=true;
                var scored=publisher.CaptureSpan(Sources.Word,[head],token);
                var scores=LegacyPreparedMainSpanScorePublication.Normalize(scored.Words.Select(w=>w.CopyStorage()).ToArray(),token);
                publisher.ApplySpanScores(scored,scores.Words,token);SpanScoresApplied=true;
                CheckboxGuardVerified=true;token.ThrowIfCancellationRequested();IsTopV68Completed=true;
            }
            catch{IsFaulted=true;throw;}
        }
    }
}
