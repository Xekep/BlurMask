using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>Completed actual negative-span217670 refresh. Source records are
/// owned24-byte values; no native pointers or unallocated residual tail appear.</summary>
public sealed class LegacyOwnedMainRefreshedSpanGlyphOwner
{
    public LegacyOwnedMainSpanGlyphRefresh Owner {get;}
    public LegacyOwnedMainSpanGlyphSourceResult Source {get;}
    public uint SourceScoreBits=>0x3f800000;
    internal LegacyOwnedMainRefreshedSpanGlyphOwner(LegacyOwnedMainSpanGlyphRefresh owner,
        LegacyOwnedMainSpanGlyphSourceResult source){Owner=owner;Source=source;}
}

/// <summary>Fresh ordinary negative-span217670 branch. Existing actual pool
/// ownership is required; refresh exports source references and sets scalar1.
/// It never appends glyphs, resolves symbols or calls confidence/geometry costs.</summary>
public sealed class LegacyOwnedMainSpanGlyphRefresh
{
    private static readonly ConditionalWeakTable<LegacyFreshMainSingleLinePlainRecognitionResult,
        LegacyOwnedMainSpanGlyphRefresh> owners=new();
    private static readonly ConditionalWeakTable<LegacyOwnedMainConfiguredAlternative,
        LegacyOwnedMainSpanGlyphRefresh> configuredOwners=new();
    private readonly object gate=new();
    private readonly Dictionary<ulong,LegacyOwnedMainRefreshedSpanGlyphOwner?> progress=[];
    public LegacyOwnedMainSpanGlyphSource Source {get;}
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult=>Source.RawResult;
    public LegacyOwnedMainCandidateSource? CandidateSource=>Source.CandidateSource;
    private readonly LegacyNativeGlyphRecordPool pool;
    private LegacyOwnedMainSpanGlyphRefresh(LegacyOwnedMainSpanGlyphSource source)
    {Source=source;pool=source.CandidateSource?.Ownership.GlyphPool??source.RawResult.Session.Ownership.GlyphPool;}
    public static LegacyOwnedMainSpanGlyphRefresh Prepare(LegacyFreshMainSingleLinePlainRecognitionResult actualResult,
        CancellationToken token=default)
    {
        var source=LegacyOwnedMainSpanGlyphSource.Prepare(actualResult,token);
        return owners.GetValue(actualResult,_=>new(source));
    }

    /// <summary>Candidate-local refresh epoch for a genuine configured list.
    /// Shared TOP identity alone cannot substitute for its selected overlay.</summary>
    public static LegacyOwnedMainSpanGlyphRefresh PrepareConfigured(LegacyOwnedMainCandidateSource candidate,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(candidate);candidate.Validate(token);
        if(!candidate.IsConfigured)throw new ArgumentException("Actual configured candidate required.");
        if(configuredOwners.TryGetValue(candidate.ConfiguredAlternative,out var existing))
        {
            if(!ReferenceEquals(existing.CandidateSource!.SourceOverlay,candidate.SourceOverlay)||
                !ReferenceEquals(existing.CandidateSource.Ownership,candidate.Ownership))
                throw new ArgumentException("The same configured selected session/overlay required.");
            token.ThrowIfCancellationRequested();return existing;
        }
        var source=LegacyOwnedMainSpanGlyphSource.PrepareConfigured(candidate,token);
        return configuredOwners.GetValue(candidate.ConfiguredAlternative,_=>new(source));
    }

    public LegacyOwnedMainRefreshedSpanGlyphOwner RefreshOwner(ulong canonicalSpanKey,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(canonicalSpanKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalSpanKey)||
            unchecked((short)(canonicalSpanKey>>24))>=0)
            throw new ArgumentException("An actual canonical negative-span pool owner required.");
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            CandidateSource?.Validate(token);
            _=pool.CopyOwnerNode(canonicalSpanKey,token);
            if(progress.TryGetValue(canonicalSpanKey,out var completed))
                return completed??throw new InvalidOperationException("This owner retains an interrupted refresh; retry is unsupported.");
            progress.Add(canonicalSpanKey,null);
            var source=Source.Read(canonicalSpanKey,token);
            token.ThrowIfCancellationRequested();
            var result=new LegacyOwnedMainRefreshedSpanGlyphOwner(this,source);
            progress[canonicalSpanKey]=result;return result;
        }
    }
}
