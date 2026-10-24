using System.Runtime.CompilerServices;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Successful217670 ordinary source owner. Source-header1C and26N
/// bytes are typed sidecars, not invented native64-byte node storage.</summary>
public sealed class LegacyFreshMainGlobalRefreshedGlyphOwner
{
    public LegacyFreshMainGlobalOrdinaryGlyphRefresh Owner {get;}
    public LegacyFreshMainGlobalGlyphSourceMetadataResult Source {get;}
    public uint SourceScoreBits {get;}
    public int VisitedCompactCount {get;}
    public bool RefreshAttempted=>true;
    internal LegacyFreshMainGlobalRefreshedGlyphOwner(LegacyFreshMainGlobalOrdinaryGlyphRefresh owner,
        LegacyFreshMainGlobalGlyphSourceMetadataResult source,uint bits,int visited)
    {Owner=owner;Source=source;SourceScoreBits=bits;VisitedCompactCount=visited;}
}
public sealed class LegacyFreshMainGlobalRefreshedGlyphAlternatives
{
    public LegacyFreshMainGlobalRefreshedGlyphOwner RefreshedOwner {get;}
    public LegacyPreparedGlyphAlternativeSelection Selection {get;}
    internal LegacyFreshMainGlobalRefreshedGlyphAlternatives(LegacyFreshMainGlobalRefreshedGlyphOwner owner,
        LegacyPreparedGlyphAlternativeSelection selection){RefreshedOwner=owner;Selection=selection;}
}

/// <summary>Ordinary nonnegative217670 over the exact live global postV68
/// pool/session/overlay. Existing records survive and all matching compact
/// descriptors append. Failed epochs retain writes and poison that owner;
/// native failed4000 replay is deliberately outside this bounded facade.</summary>
public sealed class LegacyFreshMainGlobalOrdinaryGlyphRefresh
{
    private sealed class Progress
    {
        internal LegacyFreshMainGlobalGlyphSourceMetadataResult? Source;
        internal int Count,AllocationRequestedBytes;
        internal byte[]? Allocation;
        internal uint? FirstScoreBits;
        internal LegacyFreshMainGlobalRefreshedGlyphOwner? Completed;
    }
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalNoCandidateSourceCompletion,
        LegacyFreshMainGlobalOrdinaryGlyphRefresh> owners=new();
    private readonly object gate=new();
    private readonly Dictionary<ulong,Progress> progress=[];
    private readonly byte[] compact;
    private readonly int[] starts;
    private readonly LegacyNativeGlyphRecordPool pool;
    private readonly LegacyFreshMainGlobalOrdinaryGlyphProducer producer;
    private int retainedReferences;
    internal LegacyGlobalCompletedTextSource ActualCompletion{get;}
    public LegacyFreshMainGlobalNoCandidateSourceCompletion Completion=>ActualCompletion.LegacyCompletion??throw new InvalidOperationException("This owner uses completed augmentation; no NoCandidate authority exists.");
    public LegacyFreshMainGlobalGlyphSourceMetadata SourceMetadata {get;}
    public LegacyFreshMainGlobalGlyphScoring Scoring=>producer.Scoring;
    private LegacyFreshMainGlobalOrdinaryGlyphRefresh(LegacyGlobalCompletedTextSource completion)
    {
        ActualCompletion=completion;var source=completion.Source;var session=source.Session!;
        pool=session.Ownership.GlyphPool;producer=session.OrdinaryProducer;
        SourceMetadata=session.Associations.Metadata;
        if(!ReferenceEquals(SourceMetadata.SourceOverlay,session.Associations.SourceOverlay)||
            !ReferenceEquals(SourceMetadata.Graph,session.Ownership.Geometry.Graph)||
            !ReferenceEquals(producer.Geometry,session.Ownership.Geometry)||
            !ReferenceEquals(producer.Callbacks,session.Associations.Plan.BacktrackRows.SourceCallbacks)||
            !ReferenceEquals(producer.Geometry.Policy,completion.Snapshot.SourcePolicy))
            throw new ArgumentException("Identical actual global pool/source overlay/metadata/scoring/selection required.");
        compact=source.Selection.Publication.CopyCompactStorage();starts=source.Selection.Publication.CopyNodeStarts2478();
    }
    public static LegacyFreshMainGlobalOrdinaryGlyphRefresh Prepare(ReadOnlySpan<byte> acousticResource,
        LegacyFreshMainGlobalNoCandidateSourceCompletion actualCompletedSources,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCompletedSources);actualCompletedSources.Validate(token);
        if(!actualCompletedSources.IsTopV68Completed||actualCompletedSources.IsFaulted)
            throw new ArgumentException("Successful actual live global TOP.V68 required.");
        if(acousticResource.Length!=572763||!Convert.ToHexString(SHA256.HashData(acousticResource)).Equals(
            actualCompletedSources.Snapshot.SourcePolicy.Owner.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The same actual acoustic source resource required.");
        var binding=actualCompletedSources.TextScope.BindLegacyTextSource(actualCompletedSources,token);
        var result=owners.GetValue(actualCompletedSources,_=>PrepareBound(binding,token));result.Validate(token);return result;
    }
    public static LegacyFreshMainGlobalOrdinaryGlyphRefresh Prepare(ReadOnlySpan<byte> resource,LegacyLibraryGlobalTextPublicationScope scope,LegacyFreshMainGlobalWordAugmentation actual,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(resource.Length!=572763)throw new ArgumentException("Complete acoustic source resource required.");
        byte[] ownedResource=resource.ToArray();LegacyGlobalCompletedTextSource.ValidateAugmentationResource(scope,actual,ownedResource,token);
        return PrepareCompleted(ownedResource,scope.BindAugmentedTextSource(actual,token),token);
    }
    internal static LegacyFreshMainGlobalOrdinaryGlyphRefresh PrepareCompleted(ReadOnlySpan<byte> resource,LegacyGlobalCompletedTextSource binding,CancellationToken token)
    {
        binding.Validate(token);
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(binding.Snapshot.SourcePolicy.Owner.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Same actual acoustic resource required.");
        return PrepareBound(binding,token);
    }
    private static LegacyFreshMainGlobalOrdinaryGlyphRefresh PrepareBound(LegacyGlobalCompletedTextSource binding,CancellationToken token)
    {
        binding.Validate(token);lock(binding.Gate){var result=binding.RefreshOwner??=new(binding);result.Validate(token);return result;}
    }
    private void Validate(CancellationToken token)
    {
        ActualCompletion.Validate(token);
        if(!ActualCompletion.IsTopV68Completed||ActualCompletion.Source.Selection.Publication.Policy.Owner.AcousticEvaluations!=ActualCompletion.Source.CachedMainGroupEvaluations)
            throw new InvalidOperationException("Live completed global source and unchanged cached NN required.");
    }
    public LegacyFreshMainGlobalRefreshedGlyphOwner RefreshOwner(ulong canonicalOwnerKey,
        ReadOnlySpan<byte> allocatedRecordStorage40,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(canonicalOwnerKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalOwnerKey)||
            unchecked((short)(canonicalOwnerKey>>24))<0||allocatedRecordStorage40.Length!=40)
            throw new ArgumentException("Canonical nonnegative ordinary owner and complete allocated row40 required.");
        lock(gate)
        {
            Validate(token);_=pool.CopyOwnerNode(canonicalOwnerKey,token);
            if(progress.TryGetValue(canonicalOwnerKey,out var old))
                return old.Completed??throw new InvalidOperationException("Failed global refresh prefix cannot retry.");
            var state=new Progress();progress.Add(canonicalOwnerKey,state);
            //Count/fill use the SAME real session overlay. No surrogate graph,
            //new NN group or whole-WORD range substitutes for target membership.
            var count=SourceMetadata.Read(canonicalOwnerKey,token);state.Count=count.ReferenceCount;
            if(state.Count>65536-retainedReferences)
                throw new NotSupportedException("Portable aggregate65536 source-reference retention budget exceeded.");
            state.Source=count;retainedReferences=checked(retainedReferences+state.Count);
            state.AllocationRequestedBytes=checked(state.Count*26);
            var fill=SourceMetadata.Read(canonicalOwnerKey,token);
            if(fill.BaseSourceKey!=count.BaseSourceKey||fill.ReferenceCount!=count.ReferenceCount||!fill.References.SequenceEqual(count.References))
                throw new InvalidDataException("Actual global source changed between count and fill.");
            state.Allocation=fill.CopySourceAllocation26();
            state.FirstScoreBits=BitConverter.SingleToUInt32Bits(Scoring.ReadFirstCandidateScore(count.BaseNode,count.BaseArc,token));
            LegacyRussianMainReplacementResolution Resolve(ushort symbol,CancellationToken ct)
            {Validate(ct);var value=producer.ResolveForRefresh(count.BaseNode,count.BaseArc,symbol,ct);Validate(ct);return value;}
            uint ReadScore(ushort symbol,CancellationToken ct)
            {Validate(ct);var value=BitConverter.SingleToUInt32Bits(Scoring.ReadSymbolScore(count.BaseNode,count.BaseArc,symbol,ct));Validate(ct);return value;}
            LegacyOrdinaryGlyphRefreshCost ReadCost(ushort symbol,bool end,CancellationToken ct)
            {Validate(ct);byte[] request=Scoring.CreateRequest(count.BaseNode,count.BaseArc,symbol,end,ct);int cost=Scoring.ReadRequestCost(request,ct);Validate(ct);return new(cost);}
            var operations=new LegacyOrdinaryGlyphRefreshOperations(Resolve,ReadScore,ReadCost);
            int visited=LegacyNativeOrdinaryGlyphRefreshKernel.Apply(pool,canonicalOwnerKey,compact,
                starts[count.BaseNode],starts[count.BaseNode+1],count.BaseSourceKey,operations,allocatedRecordStorage40,token);
            Validate(token);return state.Completed=new(this,count,state.FirstScoreBits.Value,visited);
        }
    }
    public LegacyFreshMainGlobalRefreshedGlyphAlternatives Select(ulong actualGlyphReference,
        ReadOnlySpan<byte> allocatedRecordStorage40,ReadOnlySpan<byte> selectionAllocation4,
        int initialSelectionCapacity=0,CancellationToken token=default)
    {
        var refreshed=RefreshOwner(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(actualGlyphReference),allocatedRecordStorage40,token);
        uint marker=unchecked((uint)(int)(short)(actualGlyphReference>>40));
        var selected=LegacyPreparedGlyphAlternativeSelection.Select(pool.CopyOwnerNode(refreshed.Source.TargetSourceKey,token),
            marker,selectionAllocation4,initialSelectionCapacity,token);Validate(token);return new(refreshed,selected);
    }
    internal (int Count,byte[]? Allocation,uint? FirstBits,bool Completed) CopyPrefix(ulong key)
    {lock(gate){if(!progress.TryGetValue(key,out var p))throw new ArgumentException("Entered actual refresh prefix required.");return(p.Count,p.Allocation?.ToArray(),p.FirstScoreBits,p.Completed is not null);}}
}
