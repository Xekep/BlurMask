using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMathCandidateRetention.Probe")]
namespace InkRecognition.Math;
internal sealed partial class Math2Mode0CompletedEventLease
{
    //Diagnostic weak handle only; it must never keep an old score tensor live.
    internal WeakReference CandidateReferenceForRetentionProbe()
    { candidates.TryGetTarget(out var candidate); return new(candidate); }
}
internal sealed partial class Math2Mode0PrefixStep
{
    //The context gate owns the pending step. Capture anchors this candidate
    //until consuming Apply, failure or disposal; historical leases do not.
    internal void ReleaseCompletedCandidateRetention() { completedCandidates = null; }
}
