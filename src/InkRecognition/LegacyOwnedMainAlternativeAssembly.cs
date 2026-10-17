using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>One actual post-callback279 list, quote-cleaned with its same TOP
/// pool and freshF4 policy. This does not select another terminal.</summary>
public sealed class LegacyOwnedMainCompletedAlternative
{
    private sealed class Claim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyFreshMainPlainResultOwnership, Claim> claims = new();
    public LegacyOwnedMainFirstAlternativeSnapshot Snapshot { get; }
    public LegacyFreshMainPlainResultOwnership Ownership => Snapshot.Session.Ownership;
    public LegacyFreshMainPlainResultOwnership TopOwner => Ownership.TopOwner;
    public LegacyFreshMainResultGlyphNormalizationNoOp Normalization { get; }
    public bool IsReleased { get; private set; }
    internal LegacyNativePlainWordRecord[] Words { get; }
    private LegacyOwnedMainCompletedAlternative(LegacyOwnedMainFirstAlternativeSnapshot snapshot,
        LegacyFreshMainResultGlyphNormalizationNoOp normalization)
    {
        Snapshot = snapshot; Normalization = normalization;
        var words = new List<LegacyNativePlainWordRecord>();
        for (var node = snapshot.WordHead18; node is not null; node = node.Next) words.Add(node.Word);
        Words = words.ToArray();
    }
    public static LegacyOwnedMainCompletedAlternative FromFirstCandidate(LegacyFreshMainCompletedFirstCandidate first,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(first); token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(first.GlyphNormalization.Snapshot, first.Snapshot))
            throw new ArgumentException("Exact already completed first-candidate proof required.");
        Begin(first.Snapshot.Session.Ownership);
        return new(first.Snapshot, first.GlyphNormalization);
    }
    public static LegacyOwnedMainCompletedAlternative CompleteActualCallbacks(LegacyOwnedMainTerminalResult terminal,
        LegacyOwnedMainGlyphFlushSession session, IReadOnlyList<LegacyOwnedMainWordPublicationResult> publications,
        ReadOnlySpan<byte> allocation400, Span<byte> leading4, Span<byte> trailing4, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (leading4.Length != 4 || trailing4.Length != 4 || leading4.Overlaps(trailing4))
            throw new ArgumentException("Two separate complete quote scratch slots required.");
        var snapshot = LegacyOwnedMainFirstAlternativeSnapshot.Capture(terminal, session, publications, allocation400, token);
        var noOp = LegacyFreshMainResultGlyphNormalization.Prepare(snapshot.SourcePolicy, token);
        Begin(snapshot.Session.Ownership); token.ThrowIfCancellationRequested();
        var words = new List<LegacyNativePlainWordRecord>();
        for (var node = snapshot.WordHead18; node is not null; node = node.Next) words.Add(node.Word);
        LegacyNativeWordQuoteNormalization.Prepare(snapshot.SourcePolicy.Model.AcousticAlphabet, token)
            .NormalizePublished(snapshot.Session.Ownership.GlyphPool, words, leading4, trailing4, null, token);
        var proof = noOp.Apply(snapshot, token); token.ThrowIfCancellationRequested();
        return new(snapshot, proof);
    }
    private static void Begin(LegacyFreshMainPlainResultOwnership owner)
    {
        if (Interlocked.CompareExchange(ref claims.GetValue(owner, static _ => new()).Started, 1, 0) != 0)
            throw new InvalidOperationException("This actual279 publisher has already entered candidate completion.");
    }
    internal void ReleasePlainList()
    {
        Ownership.WordPublisher.ReleaseAlternativeList();
        IsReleased = true;
    }
}

public readonly record struct LegacyMainAlternativeAllocationRequest(int PreviousCapacity, int RequiredCapacity,
    ReadOnlyMemory<byte> PreviousStorage)
{
    public int RequiredBytes => checked(RequiredCapacity * 40);
}
public delegate byte[] LegacyMainAlternativeStorageAllocator(LegacyMainAlternativeAllocationRequest request);
public readonly record struct LegacyMainAlternativeConsideration(bool Duplicate, int DuplicateOf, int Slot, int ActiveCount);

/// <summary>Prepared post-callback273 raw40 collection and native plain duplicate
/// gate. Owns the real TOP pool and independent279 heads. It does not fabricate
/// secondary-count settings, select terminals, maintain2257C0 exclusions or rescue.</summary>
public sealed class LegacyOwnedMainAlternativeAssembly
{
    private sealed class Claim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyFreshMainPlainResultOwnership, Claim> claims = new();
    private readonly object gate = new();
    private readonly LegacyMainAlternativeStorageAllocator allocator;
    private readonly List<LegacyOwnedMainCompletedAlternative> active = [];
    private readonly HashSet<LegacyOwnedMainCompletedAlternative> consumed = [];
    private byte[] storage = [];
    private bool busy, faulted, finalized;
    public LegacyFreshMainPlainResultOwnership TopOwner { get; }
    public int ActiveCount { get { lock (gate) return active.Count; } }
    public int Capacity { get { lock (gate) return storage.Length / 40; } }
    public bool IsFaulted { get { lock (gate) return faulted; } }
    public long Reference78 { get; private set; }
    public int NativeFreshSecondaryLimit => TopOwner.Controller.Configuration.SecondaryAlternativeCount2404;
    private LegacyOwnedMainAlternativeAssembly(LegacyFreshMainPlainResultOwnership top, LegacyMainAlternativeStorageAllocator allocator)
    { TopOwner = top; this.allocator = allocator; }
    public static LegacyOwnedMainAlternativeAssembly Prepare(LegacyFreshMainPlainResultOwnership actualTop,
        LegacyMainAlternativeStorageAllocator allocator, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualTop); ArgumentNullException.ThrowIfNull(allocator);
        token.ThrowIfCancellationRequested(); var top = actualTop.TopOwner;
        var policy = top.Materialization.CaseSourcePolicy ?? throw new ArgumentException("Actual fresh CASE main TOP required.");
        _ = LegacyFreshMainResultGlyphNormalization.Prepare(policy, token);
        if (Interlocked.CompareExchange(ref claims.GetValue(top, static _ => new()).Started, 1, 0) != 0)
            throw new InvalidOperationException("This actual TOP already owns an alternative assembly.");
        return new(top, allocator);
    }
    public LegacyMainAlternativeConsideration Consider(LegacyOwnedMainCompletedAlternative candidate,
        Span<byte> leftScalar4, Span<byte> rightScalar4, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidate); token.ThrowIfCancellationRequested();
        if (leftScalar4.Length != 4 || rightScalar4.Length != 4 || leftScalar4.Overlaps(rightScalar4))
            throw new ArgumentException("Two complete separate scalar scratch slots required.");
        lock (gate)
        {
            if (busy || faulted || finalized) throw new InvalidOperationException("An active healthy unfinalized assembly is required.");
            if (!ReferenceEquals(candidate.TopOwner, TopOwner) || !ReferenceEquals(candidate.Ownership.GlyphPool, TopOwner.GlyphPool) ||
                !ReferenceEquals(candidate.Ownership.Materialization, TopOwner.Materialization) || candidate.IsReleased || consumed.Contains(candidate))
                throw new ArgumentException("A new completed candidate from this identical TOP/materialization/pool is required.");
            if (!candidate.Snapshot.Session.IsComplete || !ReferenceEquals(candidate.Snapshot.WordHead18, candidate.Ownership.WordPublisher.Head))
                throw new InvalidOperationException("The candidate's completed actual WORD list changed.");
            if (active.Count == 4090) throw new NotSupportedException("Portable4090-active-alternative allocation budget exceeded.");
            busy = true;
            try
            {
                if (active.Count == 0) Reference78 = candidate.Snapshot.Terminal.Winner.TotalCost;
                int capacity = NextCapacity(active.Count);
                if (storage.Length / 40 < active.Count + 1)
                {
                    byte[] grown = allocator(new(storage.Length / 40, capacity, storage.ToArray()));
                    token.ThrowIfCancellationRequested();
                    if (grown is null || grown.Length != capacity * 40 || !grown.AsSpan(0, storage.Length).SequenceEqual(storage))
                        throw new ArgumentException("Explicit owned reallocation must preserve the entire previous allocation prefix.");
                    storage = grown.ToArray();
                }
                int slot = active.Count;
                LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(storage.AsSpan(slot * 40, 40), token);
                var winner = candidate.Snapshot.Terminal.Winner;
                var input = new LegacyMainResultScoreInput(winner.TotalCost, checked((int)Reference78),
                    candidate.Snapshot.SourceNodeCount2310, winner.RowCost14, candidate.Snapshot.SourcePolicy.Model.SourceKind);
                LegacyMainResultScore.WriteCostChannels(storage.AsSpan(slot * 40, 40), input, token).CopyTo(storage, slot * 40);
                active.Add(candidate); consumed.Add(candidate);
                for (int previous = 0; previous < slot; previous++)
                {
                    if (!LegacyNativeMainAlternativeEquality.Compare(TopOwner.GlyphPool, active[previous].Words, candidate.Words,
                        leftScalar4, rightScalar4, null, token)) continue;
                    //Native1415B0 releases279 nodes/vectors, not shared275 words
                    //or274 pool rows. Portable nodes remain historical managed
                    //values; IsReleased records the exact logical-list release.
                    candidate.ReleasePlainList(); active.RemoveAt(slot);
                    return new(true, previous, slot, active.Count);
                }
                token.ThrowIfCancellationRequested(); return new(false, -1, slot, active.Count);
            }
            catch { faulted = true; throw; }
            finally { busy = false; }
        }
    }
    public byte[] CopyRawStorage() { lock (gate) return storage.ToArray(); }
    public LegacyOwnedMainCompletedAlternative[] CopyActiveOwners() { lock (gate) return active.ToArray(); }
    public LegacyMainPublishedResultScores FinalizeScores(CancellationToken token = default)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested();
            if (busy || faulted || finalized) throw new InvalidOperationException("An unfinalized healthy assembly required.");
            var result = LegacyMainResultScorePublication.Normalize(storage, active.Count, -1f, token);
            storage = result.CopyStorage(); finalized = true; return result;
        }
    }
    internal static int NextCapacity(int activeCount)
    {
        if (activeCount is < 0 or > 4089) throw new ArgumentOutOfRangeException(nameof(activeCount));
        int needed = activeCount + 1;
        return ((needed + 9) / 10) * 10;
    }
}
