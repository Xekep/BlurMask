using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>One completed ordinary217670 source owner. Pointer-backed source
/// records are owned bytes; this is not a dump of the native AVL node.</summary>
public sealed class LegacyOwnedMainRefreshedGlyphOwner
{
    public LegacyOwnedMainOrdinaryGlyphRefresh Owner { get; }
    public LegacyOwnedMainGlyphSourceMetadataResult Source { get; }
    public uint SourceScoreBits { get; }
    public int VisitedCompactCount { get; }
    internal LegacyOwnedMainRefreshedGlyphOwner(LegacyOwnedMainOrdinaryGlyphRefresh owner,
        LegacyOwnedMainGlyphSourceMetadataResult source, uint scalar, int visited)
    { Owner = owner; Source = source; SourceScoreBits = scalar; VisitedCompactCount = visited; }
}

/// <summary>Completed actual refresh followed by13A830 selection. Allocation
/// refusal in selection remains distinct from an unavailable source owner.</summary>
public sealed class LegacyOwnedMainRefreshedGlyphAlternatives
{
    public LegacyOwnedMainRefreshedGlyphOwner RefreshedOwner { get; }
    public LegacyPreparedGlyphAlternativeSelection Selection { get; }
    internal LegacyOwnedMainRefreshedGlyphAlternatives(LegacyOwnedMainRefreshedGlyphOwner owner,
        LegacyPreparedGlyphAlternativeSelection selection) { RefreshedOwner = owner; Selection = selection; }
}

/// <summary>Bounded ordinary nonnegative CASE217670 refresh over the actual
/// published pool and compact2460/2478 owner. Span/protocol sources and complete
/// string-wrapper acceptance remain separate. A failed refresh retains its
/// mutation prefix and cannot be retried by this portable owning adapter.</summary>
public sealed class LegacyOwnedMainOrdinaryGlyphRefresh
{
    private sealed class Progress
    {
        internal LegacyOwnedMainGlyphSourceMetadataResult? Source;
        internal byte[]? SourceAllocation26;
        internal uint? SourceScoreBits;
        internal LegacyOwnedMainRefreshedGlyphOwner? Completed;
    }
    private static readonly ConditionalWeakTable<LegacyFreshMainSingleLinePlainRecognitionResult,
        LegacyOwnedMainOrdinaryGlyphRefresh> owners = new();
    private readonly Dictionary<ulong, Progress> progress = [];
    private readonly object gate = new();
    private readonly byte[] compact;
    private readonly int[] starts;
    private readonly LegacyOwnedRussianGlyphSymbolResolver resolver;
    private readonly LegacyOwnedRussianMainSourceSymbolCallbacks callbacks;
    private readonly ushort replacement;
    public LegacyFreshMainSingleLinePlainRecognitionResult RawResult { get; }
    public LegacyOwnedMainGlyphSourceMetadata SourceMetadata { get; }
    public LegacyOwnedRussianMainGlyphScoring Scoring { get; }

    private LegacyOwnedMainOrdinaryGlyphRefresh(ReadOnlySpan<byte> resource,
        LegacyFreshMainSingleLinePlainRecognitionResult raw, LegacyCursiveSeparationTable table,
        CancellationToken token)
    {
        RawResult = raw;
        ValidateResult(raw);
        var session = raw.Session;
        var materialization = session.Ownership.Materialization;
        var policy = session.Plan.SourcePolicy;
        SourceMetadata = LegacyOwnedMainGlyphSourceMetadata.Prepare(session.Ownership,
            session.SourceOverlay, raw.Sampled.SourceRanges, token);
        Scoring = LegacyOwnedRussianMainGlyphScoring.Prepare(resource, SourceMetadata, table, token);
        resolver = LegacyOwnedRussianGlyphSymbolResolver.Prepare(policy.DictionaryOwner,
            policy.DictionaryOwner.Symbols.Preparation, token);
        callbacks = LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(resource,
            materialization.ConfidenceOwner, raw.Sampled, token);
        if (!policy.Model.AcousticAlphabet.TryGetLabel("\ufffd", out replacement))
            throw new InvalidDataException("Actual source replacement label is required.");
        compact = materialization.CopyCompactStorage(); starts = materialization.CopyNodeStarts2478();
    }

    private static void ValidateResult(LegacyFreshMainSingleLinePlainRecognitionResult raw)
    {
        var session=raw.Session;var materialization=session.Ownership.Materialization;
        var policy=session.Plan.SourcePolicy;
        if (!session.IsComplete || session.IsStopped || !raw.PublicationCompleted ||
            !ReferenceEquals(materialization.CaseSourcePolicy, policy) ||
            !ReferenceEquals(raw.Terminal, materialization.TerminalResult) ||
            !ReferenceEquals(raw.Sampled.SourceRanges.Graph, policy.Graph) ||
            policy.Controller.Configuration.Trace2368 != 0)
            throw new ArgumentException("An actual completed fresh CASE publication and its unchanged source owner are required.");
    }

    public static LegacyOwnedMainOrdinaryGlyphRefresh Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainSingleLinePlainRecognitionResult actualResult, LegacyCursiveSeparationTable table,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); ArgumentNullException.ThrowIfNull(table);
        token.ThrowIfCancellationRequested();
        //Returning an existing epoch still validates the caller's resources.
        //Do not reconstruct source keys, geometry and compact buffers merely
        //to discard the newly allocated owner on a weak-table cache hit.
        ValidateResult(actualResult);
        if(owners.TryGetValue(actualResult,out var existing))
        {
            if(resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(
                Convert.ToHexString(SHA256.HashData(resource)),actualResult.Session.Plan.SourcePolicy.Controller.Owner.SelectedSourceOwner.SourceFingerprint))
                throw new InvalidDataException("Same pinned main acoustic source required.");
            var values=table.Values.Span;
            if(values.Length!=256)throw new InvalidDataException("Actual engine separation table required.");
            Span<byte> bytes=stackalloc byte[1024];
            for(int i=0;i<values.Length;i++)BinaryPrimitives.WriteSingleLittleEndian(bytes[(i*4)..],values[i]);
            if(!SHA256.HashData(bytes).AsSpan().SequenceEqual(Convert.FromHexString(
                "b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf")))
                throw new InvalidDataException("Actual engine separation table required.");
            token.ThrowIfCancellationRequested();return existing;
        }
        var prepared = new LegacyOwnedMainOrdinaryGlyphRefresh(resource, actualResult, table, token);
        token.ThrowIfCancellationRequested();
        return owners.GetValue(actualResult, _ => prepared);
    }

    public LegacyOwnedMainRefreshedGlyphOwner RefreshOwner(ulong canonicalOwnerKey,
        ReadOnlySpan<byte> allocatedRecordStorage40, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (canonicalOwnerKey != LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalOwnerKey) ||
            allocatedRecordStorage40.Length != 40)
            throw new ArgumentException("Canonical owner and explicit complete allocated-record storage required.");
        lock (gate)
        {
            token.ThrowIfCancellationRequested();
            var pool = RawResult.Session.Ownership.GlyphPool;
            _ = pool.CopyOwnerNode(canonicalOwnerKey, token);
            if (progress.TryGetValue(canonicalOwnerKey, out var previous))
                return previous.Completed ?? throw new InvalidOperationException("This owner retains a failed refresh prefix; retry is unsupported.");
            var state = new Progress(); progress.Add(canonicalOwnerKey, state);
            state.Source = SourceMetadata.Read(canonicalOwnerKey, token);
            state.SourceAllocation26 = state.Source.CopySourceAllocation26();
            state.SourceScoreBits = Scoring.ReadFirstCandidateScoreBits(state.Source, token);
            int first = starts[state.Source.BaseNode], end = starts[state.Source.BaseNode + 1], visited = 0;
            for (int index = first; index < end; index++)
            {
                token.ThrowIfCancellationRequested(); ReadOnlySpan<byte> row = compact.AsSpan(index * 40, 40);
                byte traits = row[0x20];
                if ((traits & 0x20) == 0 || (traits & 7) != 0 ||
                    BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) != state.Source.BaseSourceKey) continue;
                var inserted = pool.AppendDescriptor(canonicalOwnerKey, index - first, false, allocatedRecordStorage40, token);
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[0x1c..]);
                ushort emitted, normalized; byte variant = 0;
                if (symbol == replacement)
                {
                    var resolved = callbacks.ResolveReplacement(state.Source.BaseNode, state.Source.BaseArc, token);
                    emitted = resolved.EmittedSymbol; normalized = resolved.NormalizedOriginal; variant = resolved.ExternalVariant;
                }
                else
                {
                    var resolved = resolver.Resolve(symbol, 0, false, token);
                    emitted = resolved.EmittedSymbol; normalized = resolved.NormalizedOriginal;
                }
                if (!LegacyNativeGlyphSymbolUpdate.Apply(pool, inserted.Reference, emitted, 1, 0, variant != 0, token))
                    throw new NotSupportedException("Actual ordinary scalar symbol update did not complete.");
                byte[] record = pool.CopyRecord(inserted.Reference, token);
                uint symbolScore = Scoring.ReadSymbolScoreBits(state.Source, normalized, token);
                BinaryPrimitives.WriteUInt32LittleEndian(record.AsSpan(0x10), symbolScore);
                pool.ReplaceRecord(inserted.Reference, record, token);
                var scores = Scoring.ReadPreparedSymbol(state.Source, normalized, (traits & 0x40) != 0, token);
                BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(0x14), scores.Cost);
                BinaryPrimitives.WriteSingleLittleEndian(record.AsSpan(0x0c), row[0x21] / 255f);
                ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4));
                BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4), (ushort)((flags & 0xfff7) | ((traits & 0x40) != 0 ? 8 : 0)));
                pool.ReplaceRecord(inserted.Reference, record, token); visited++;
            }
            token.ThrowIfCancellationRequested();
            return state.Completed = new(this, state.Source, state.SourceScoreBits.Value, visited);
        }
    }

    public LegacyOwnedMainRefreshedGlyphAlternatives Select(ulong actualGlyphReference,
        ReadOnlySpan<byte> allocatedRecordStorage40, ReadOnlySpan<byte> selectionAllocation4,
        int initialSelectionCapacity = 0, CancellationToken token = default)
    {
        ulong key = LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(actualGlyphReference);
        uint marker = unchecked((uint)(int)(short)(actualGlyphReference >> 40));
        var refreshed = RefreshOwner(key, allocatedRecordStorage40, token);
        var node = RawResult.Session.Ownership.GlyphPool.CopyOwnerNode(key, token);
        var selection = LegacyPreparedGlyphAlternativeSelection.Select(node, marker,
            selectionAllocation4, initialSelectionCapacity, token);
        return new(refreshed, selection);
    }
}
