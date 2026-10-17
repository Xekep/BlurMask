using System.Runtime.CompilerServices;
using System.Security.Cryptography;
[assembly:InternalsVisibleTo("LegacyMainConfiguredSourceCompletion.Probe")]
namespace InkRecognition;

/// <summary>Bounded TOP.V68 for two actual configured alternatives selecting
/// one shared275 span, with no active bit8 compact candidate. Source export,
/// linked-WORD ranking and score publication run once for that actual span.</summary>
public sealed class LegacyFreshMainConfiguredNoCandidateSourceCompletion
{
    private sealed class Claim {internal int Entered;}
    private static readonly ConditionalWeakTable<LegacyFreshMainPlainResultOwnership,Claim> claims=new();
    private readonly LegacyFreshMainGlyphUtf8Emission emission;
    private readonly LegacyOwnedMainCandidateSource[] candidates;
    private readonly LegacyNativePublishedWordNode[] heads;
    private readonly byte[] leftScratch,rightScratch;
    private readonly object gate=new();
    private bool started;
    public LegacyOwnedMainWordSourceCollection Sources {get;}
    public bool SourcesMarkedComplete {get;private set;}
    public bool RankingApplied {get;private set;}
    public bool SpanScoresApplied {get;private set;}
    public bool CheckboxGuardVerified {get;private set;}
    public bool IsTopV68Completed {get;private set;}
    public bool IsFaulted {get;private set;}
    public int ComparisonCalls {get;private set;}
    public int InputWordCount {get;private set;}
    public int OutputWordCount {get;private set;}
    public int QuotaAfter {get;private set;}
    internal LegacyOwnedMainWordSourceCollection SourcesFor(LegacyOwnedMainCandidateSource candidate,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(candidate);token.ThrowIfCancellationRequested();
        if(!IsTopV68Completed||IsFaulted||!candidates.Any(c=>ReferenceEquals(c.CandidateIdentity,candidate.CandidateIdentity)&&
            ReferenceEquals(c.SourceIdentity,candidate.SourceIdentity)))
            throw new ArgumentException("A selected candidate of this successful actual configured source completion required.");
        return Sources.BindSameSpanCandidate(candidate,token);
    }
    private LegacyFreshMainConfiguredNoCandidateSourceCompletion(LegacyOwnedMainWordSourceCollection sources,
        LegacyOwnedMainCandidateSource[] candidates,LegacyNativePublishedWordNode[] heads,
        LegacyFreshMainGlyphUtf8Emission emission,ReadOnlySpan<byte> left,ReadOnlySpan<byte> right)
    {Sources=sources;this.candidates=candidates;this.heads=heads;this.emission=emission;leftScratch=left.ToArray();rightScratch=right.ToArray();}

    public static LegacyFreshMainConfiguredNoCandidateSourceCompletion Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actualResult,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        ReadOnlySpan<byte> leftCallerResidue4,ReadOnlySpan<byte> rightCallerResidue4,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualResult);ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles);token.ThrowIfCancellationRequested();
        if(leftCallerResidue4.Length!=4||rightCallerResidue4.Length!=4||leftCallerResidue4.Overlaps(rightCallerResidue4))
            throw new ArgumentException("Two separate complete scalar comparison residues required.");
        var alternatives=actualResult.Alternatives.CopyActiveAlternatives();
        if(!actualResult.Alternatives.IsComplete||actualResult.Alternatives.IsFaulted||alternatives.Length!=2)
            throw new NotSupportedException("Exactly two completed genuine configured alternatives required.");
        var candidates=alternatives.Select(a=>LegacyOwnedMainCandidateSource.FromConfigured(actualResult,a,token)).ToArray();
        var first=candidates[0];var policy=first.SourcePolicy;var controller=policy.Controller;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(
            controller.Owner.SelectedSourceOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The identical actual primary acoustic resource is required.");
        if(policy.HasAssociatedSource||policy.Model.SourceKind!=0x135||controller.SecondaryScaleEc!=0||
            controller.SecondaryCountProperty12d0003!=2||controller.Configuration.Trace2368!=0||
            controller.Configuration.Mode22f6!=0||controller.Refresh.SourceFormat22f9!=0)
            throw new NotSupportedException("Fresh configured property18=2 kind135 no-associated source required.");
        var sources=LegacyOwnedMainWordSourceCollection.PrepareConfigured(first,token);
        var heads=new LegacyNativePublishedWordNode[2];
        for(int i=0;i<candidates.Length;i++)
        {
            var c=candidates[i];c.Validate(token);
            if(!ReferenceEquals(c.TopOwner,first.TopOwner)||!ReferenceEquals(c.SourceIdentity,first.SourceIdentity)||
                c.WordHead18 is not{} head||head.Next is not null||head.Span!=sources.Word.Span)
                throw new NotSupportedException("Two actual single-WORD heads selecting one shared275 span required.");
            heads[i]=head;
        }
        var compact=actualResult.Compact;byte[] storage=compact.CopyCompactStorage();
        for(int i=0;i<compact.RecordCount;i++)
        {
            token.ThrowIfCancellationRequested();
            if((storage[i*40+32]&8)!=0)
                throw new NotSupportedException("Enabled219580 candidate augmentation cannot be omitted.");
        }
        foreach(string text in new[]{"\u2610","\u2611","\u2612"})
            if(policy.Model.AcousticAlphabet.TryGetLabel(text,out _))
                throw new NotSupportedException("Enabled native checkbox rewriting needs its separate owner.");
        var span=first.Ownership.WordPublisher.CaptureSpan(sources.Word,heads,token);
        if(span.SelectedNodes.Count!=2)throw new InvalidDataException("Both actual configured279 selections must be retained.");
        var emission=LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(first,normalization,circles,token);
        return new(sources,candidates,heads,emission,leftCallerResidue4,rightCallerResidue4);
    }

    public void Run(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(started)throw new InvalidOperationException("An entered configured source completion cannot be retried.");
            foreach(var candidate in candidates)candidate.Validate(token);
            var publisher=candidates[0].Ownership.WordPublisher;
            var span=publisher.CaptureSpan(Sources.Word,heads,token);
            if(Interlocked.CompareExchange(ref claims.GetValue(candidates[0].TopOwner,static _=>new()).Entered,1,0)!=0)
                throw new InvalidOperationException("This actual configured TOP already entered source completion.");
            started=true;
            try
            {
                SourcesMarkedComplete=true;InputWordCount=span.Words.Count;
                ushort ignored=emission.PrimaryAlphabet.TryGetLabel("\u200d",out ushort label)?label:ushort.MaxValue;
                bool Compare(ReadOnlySpan<byte> left,ReadOnlySpan<byte> right)
                {
                    ComparisonCalls++;
                    return LegacyNativeMainWordEquality.Compare(candidates[0].TopOwner.GlyphPool,left,right,leftScratch,rightScratch,
                        ignored,static symbol=>symbol,text=>emission.ReadControlLabel(text,token),token);
                }
                var ranked=LegacyPreparedMainWordRanking.Rank(span.Words.Select(w=>w.CopyStorage()).ToArray(),
                    span.SelectedOrdinals.ToArray(),LegacyPreparedMainWordRanking.InitialQuota(2,65535,false),false,Compare,token);
                publisher.ApplySpanRanking(span,ranked.Words,ranked.OutputSourceOrdinals,ranked.OldToNew,token);
                RankingApplied=true;QuotaAfter=ranked.QuotaAfter;OutputWordCount=ranked.Words.Length;
                var scored=publisher.CaptureSpan(Sources.Word,heads,token);
                var scores=LegacyPreparedMainSpanScorePublication.Normalize(scored.Words.Select(w=>w.CopyStorage()).ToArray(),token);
                publisher.ApplySpanScores(scored,scores.Words,token);SpanScoresApplied=true;
                CheckboxGuardVerified=true;token.ThrowIfCancellationRequested();IsTopV68Completed=true;
            }
            catch{IsFaulted=true;throw;}
        }
    }
}
