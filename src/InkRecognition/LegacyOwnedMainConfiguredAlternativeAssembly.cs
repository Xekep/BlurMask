using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace InkRecognition;

/// <summary>Explicit caller-owned allocation/stack residues for the plain
/// configured result pass. They are copied before any callback is entered.</summary>
public sealed class LegacyMainConfiguredAlternativeResidue
{
    internal readonly byte[] Word, Backtrack, Glyph, Request, Leading, Trailing, Left, Right;
    internal readonly ushort[] Labels;

    public LegacyMainConfiguredAlternativeResidue(ReadOnlySpan<byte> emptyWord120,
        ReadOnlySpan<byte> backtrack72, ReadOnlySpan<byte> glyph40, ReadOnlySpan<ushort> labels256,
        ReadOnlySpan<byte> request64, ReadOnlySpan<byte> quoteLeading4, ReadOnlySpan<byte> quoteTrailing4,
        ReadOnlySpan<byte> equalityLeft4, ReadOnlySpan<byte> equalityRight4)
    {
        if (emptyWord120.Length != 120 || backtrack72.Length != 72 || glyph40.Length != 40 ||
            labels256.Length != 256 || request64.Length != 64 || quoteLeading4.Length != 4 ||
            quoteTrailing4.Length != 4 || equalityLeft4.Length != 4 || equalityRight4.Length != 4)
            throw new ArgumentException("Complete explicit WORD, row, glyph, label, request and scalar residues required.");
        if (BinaryPrimitives.ReadUInt64LittleEndian(emptyWord120[32..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(emptyWord120[88..]) != 0 ||
            BinaryPrimitives.ReadUInt64LittleEndian(emptyWord120[104..]) != 0 ||
            BinaryPrimitives.ReadInt32LittleEndian(emptyWord120[96..]) != 0 ||
            BinaryPrimitives.ReadInt32LittleEndian(emptyWord120[112..]) != 0)
            throw new NotSupportedException("Empty WORD without unowned auxiliary pointer vectors required.");
        Word = emptyWord120.ToArray(); Backtrack = backtrack72.ToArray(); Glyph = glyph40.ToArray();
        Labels = labels256.ToArray(); Request = request64.ToArray(); Leading = quoteLeading4.ToArray();
        Trailing = quoteTrailing4.ToArray(); Left = equalityLeft4.ToArray(); Right = equalityRight4.ToArray();
    }
}

/// <summary>An actual configured terminal and its successfully completed
/// callback list. WORDs retain native279.Head order, without invented spaces.</summary>
public sealed class LegacyOwnedMainConfiguredAlternative
{
    private readonly LegacyOwnedMainWordPublicationResult[] publications;
    private readonly LegacyNativePlainWordRecord[] words;
    public LegacyOwnedMainSecondaryCompactTerminal Terminal { get; }
    public LegacyOwnedMainGlyphFlushSession Session { get; }
    public LegacyFreshMainPlainResultOwnership Ownership => Session.Ownership;
    public LegacyNativePublishedWordNode? WordHead18 { get; }
    public bool IsReleased { get; internal set; }
    internal IReadOnlyList<LegacyNativePlainWordRecord> Words => words;
    internal LegacyOwnedMainConfiguredAlternative(LegacyOwnedMainSecondaryCompactTerminal terminal,
        LegacyOwnedMainGlyphFlushSession session, LegacyOwnedMainWordPublicationResult[] publications)
    {
        Terminal = terminal; Session = session; this.publications = publications;
        WordHead18 = session.Ownership.WordPublisher.Head;
        var list = new List<LegacyNativePlainWordRecord>();
        for (var node = WordHead18; node is not null; node = node.Next) list.Add(node.Word);
        words = list.ToArray();
    }
    public LegacyOwnedMainWordPublicationResult[] CopyPublications() => publications.ToArray();
    public byte[][] CopyRawUtf8Words(CancellationToken token = default)
    {
        if (IsReleased) throw new InvalidOperationException("The duplicate279 list was released.");
        var result = new List<byte[]>();
        //Publications run in callback order; the linked head prepends.
        for (int i = publications.Length - 1; i >= 0; i--)
        {
            token.ThrowIfCancellationRequested();
            if (publications[i].PublishedWord is not null)
                result.Add(LegacyOwnedMainWordUtf8Projection.Project(publications[i], token).CopyUtf8());
        }
        token.ThrowIfCancellationRequested(); return result.ToArray();
    }
}

public readonly record struct LegacyMainConfiguredAlternativeAttempt(int OutputIndex, int ExclusionCount,
    LegacyOwnedMainConfiguredAlternative? Candidate, bool Duplicate, int DuplicateOf);

/// <summary>One actual configured2, no-rescue result pass over an already
/// published compact pool. This is the plain terminal/callback/duplicate/score
/// branch of2271F0, not the result wrapper, accepted text or calibrated confidence.</summary>
public sealed class LegacyOwnedMainConfiguredAlternativeAssembly
{
    private sealed class Claim { internal int Started; }
    private static readonly ConditionalWeakTable<LegacyOwnedMainSecondaryCompactWordMaterialization, Claim> claims = new();
    private readonly object gate = new();
    private readonly byte[] resource;
    private readonly LegacyOwnedRussianMainSourceGeometry geometry;
    private readonly LegacyOwnedRussianMainSourceSymbolCallbacks callbacks;
    private readonly LegacyOwnedRussianMainInteriorDelimiter interior;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyMainAlternativeStorageAllocator allocator;
    private readonly LegacyMainConfiguredAlternativeResidue residue;
    private readonly List<LegacyOwnedMainConfiguredAlternative> active = [];
    private readonly List<LegacyMainConfiguredAlternativeAttempt> attempts = [];
    private readonly List<ulong> exclusions = [];
    private byte[] storage = [];
    private bool started, complete, faulted;
    public LegacyOwnedMainSecondaryCompactWordMaterialization Materialization { get; }
    public LegacyFreshCaseMainSourcePolicy SourcePolicy => Materialization.PassResult.SourcePolicy;
    public LegacyFreshMainPlainResultOwnership? TopOwner { get; private set; }
    public int OutputIndex { get; private set; }
    public long Reference78 { get; private set; }
    public bool IsComplete { get { lock (gate) return complete; } }
    public bool IsFaulted { get { lock (gate) return faulted; } }
    public LegacyMainPublishedResultScores? FinalScores { get; private set; }

    private LegacyOwnedMainConfiguredAlternativeAssembly(byte[] resource,
        LegacyOwnedMainSecondaryCompactWordMaterialization materialization,
        LegacyOwnedRussianMainSourceGeometry geometry, LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyOwnedRussianMainInteriorDelimiter interior, LegacyCursiveSeparationTable table,
        LegacyMainAlternativeStorageAllocator allocator, LegacyMainConfiguredAlternativeResidue residue)
    {
        this.resource = resource; Materialization = materialization; this.geometry = geometry;
        this.callbacks = callbacks; this.interior = interior; this.table = table;
        this.allocator = allocator; this.residue = residue;
    }

    public static LegacyOwnedMainConfiguredAlternativeAssembly Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainSecondaryCompactWordMaterialization actualMaterialization,
        LegacyOwnedRussianMainSourceGeometry geometry, LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,
        LegacyOwnedRussianMainInteriorDelimiter interior, LegacyCursiveSeparationTable table,
        LegacyMainAlternativeStorageAllocator allocator, LegacyMainConfiguredAlternativeResidue residue,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualMaterialization); ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(callbacks); ArgumentNullException.ThrowIfNull(interior);
        ArgumentNullException.ThrowIfNull(table); ArgumentNullException.ThrowIfNull(allocator);
        ArgumentNullException.ThrowIfNull(residue); token.ThrowIfCancellationRequested();
        var source = actualMaterialization.PassResult.SourcePolicy;
        LegacyOwnedMainCompactWordMaterialization.ValidatePublicationOwner(resource, source.Controller,
            source, actualMaterialization.ConfidenceOwner);
        _ = LegacyOwnedMainSecondaryTerminalCursor.Prepare(actualMaterialization.PassResult, token);
        if (!ReferenceEquals(geometry.Owner, actualMaterialization.ConfidenceOwner) ||
            !ReferenceEquals(callbacks.Owner, actualMaterialization.ConfidenceOwner) ||
            !ReferenceEquals(interior.Geometry, geometry) || !ReferenceEquals(geometry.Graph, source.Graph))
            throw new ArgumentException("Exact actual configured source, geometry, callback and compact owners required.");
        //The only extra native property accepted by this privately constructed
        //controller is00D800's short18. It cannot change fresh99/A8/F4/AA:
        //00C860 clears qword98/A4 and wordF4; property12D0003 writes only short18.
        if (source.Controller.SecondaryCountProperty12d0003 != 2 ||
            source.Controller.Configuration.SecondaryAlternativeCount2404 != 2 ||
            source.DictionaryOwner.ContainerCaseMode90 != uint.MaxValue ||
            source.AcousticFingerprint != "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978")
            throw new NotSupportedException("Only the actual fresh known-property18 configured2 source is supported.");
        //20C1C0/20C4B6 writes24FA from the actual U+202B lookup. For this
        //owned alphabet it is absent, so221945 skips224CD0; source2300 alone
        //would not prove that independent repair/embedding gate is inactive.
        if (source.Model.AcousticAlphabet.TryGetLabel("\u202b", out _))
            throw new NotSupportedException("The enabled24FA/224CD0 result-repair branch requires its actual owner.");
        //No claim is consumed by pure input validation or cancellation.
        token.ThrowIfCancellationRequested();
        return new(resource.ToArray(), actualMaterialization, geometry, callbacks, interior, table, allocator,
            new(residue.Word, residue.Backtrack, residue.Glyph, residue.Labels, residue.Request,
                residue.Leading, residue.Trailing, residue.Left, residue.Right));
    }

    public void Run(CancellationToken token = default)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested();
            if (started) throw new InvalidOperationException("An actual configured result pass is single-use, including failed prefixes.");
            if (Interlocked.CompareExchange(ref claims.GetValue(Materialization, static _ => new()).Started, 1, 0) != 0)
                throw new InvalidOperationException("This actual compact pool already entered configured result assembly.");
            started = true;
            try
            {
                byte[] word = residue.Word.ToArray();
                int capacity = Materialization.PassResult.Storage.FinalSourceEnd;
                while (OutputIndex < 2 && exclusions.Count != capacity)
                {
                    token.ThrowIfCancellationRequested();
                    var terminal = Materialization.SelectOwned(exclusions.ToArray(), token);
                    if (terminal is null)
                    {
                        attempts.Add(new(OutputIndex, exclusions.Count, null, false, -1));
                        OutputIndex++; continue;
                    }
                    exclusions.Add(terminal.Selection.ExclusionIdentity);
                    int total = terminal.Selection.Winner.TotalCost;
                    if (total == 0x4000000 || total == unchecked(Materialization.PassResult.Storage.FinalCumulativeCost + 0x4000000))
                    {
                        //2257C0 has already advanced the exclusion vector.
                        //227608..22762E still treats these totals as no output.
                        attempts.Add(new(OutputIndex, exclusions.Count, null, false, -1));
                        OutputIndex++; continue;
                    }
                    if (OutputIndex == 0) Reference78 = terminal.Selection.Winner.TotalCost;
                    int slot = active.Count;
                    if (slot == storage.Length / 40)
                    {
                        int required = LegacyOwnedMainAlternativeAssembly.NextCapacity(slot);
                        byte[] grown = allocator(new(storage.Length / 40, required, storage.ToArray()));
                        token.ThrowIfCancellationRequested();
                        Materialization.PassResult.Storage.ValidateResultQueryEpoch(token);
                        if (grown is null || grown.Length != required * 40 ||
                            !grown.AsSpan(0, storage.Length).SequenceEqual(storage))
                            throw new ArgumentException("Explicit native rounded allocation must retain its full prior prefix.");
                        storage = grown.ToArray();
                    }
                    LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(storage.AsSpan(slot * 40, 40), token);
                    var winner = terminal.Selection.Winner;
                    LegacyMainResultScore.WriteCostChannels(storage.AsSpan(slot * 40, 40),
                        new(winner.TotalCost, checked((int)Reference78), SourcePolicy.Graph.NodeCount,
                            winner.RowCost14, SourcePolicy.Model.SourceKind), token).CopyTo(storage, slot * 40);
                    var ownership = TopOwner is null
                        ? LegacyFreshMainPlainResultOwnership.CreateConfigured(terminal, geometry, token)
                        : TopOwner.ForkConfiguredAlternative(terminal, token);
                    TopOwner ??= ownership;
                    var rows = LegacyOwnedMainGlyphBacktrackRows.PrepareConfigured(resource, terminal, callbacks, residue.Backtrack, token);
                    var plan = LegacyOwnedMainGlyphFlushPlan.PrepareConfigured(rows, token);
                    var session = LegacyOwnedMainGlyphFlushSession.PrepareConfigured(ownership, plan, word, token);
                    var publisher = LegacyOwnedMainPlainWordPublication.PrepareConfigured(resource, session, table, callbacks, interior, token);
                    var publications = new List<LegacyOwnedMainWordPublicationResult>();
                    while (!session.IsComplete)
                    {
                        var association = session.PrepareNext(token);
                        var published = publisher.Publish(association, residue.Glyph, residue.Labels, residue.Request, token);
                        publications.Add(published); session.CompleteCurrent(published, token);
                        if (!published.Succeeded) throw new InvalidOperationException("An actual native WORD callback failed.");
                    }
                    //The native seed276 is shared across alternatives. Carry
                    //its reset storage, including untouched/published residue.
                    word = session.WorkingWord.CopyStorage(token);
                    var candidate = new LegacyOwnedMainConfiguredAlternative(terminal, session, publications.ToArray());
                    LegacyNativeWordQuoteNormalization.Prepare(SourcePolicy.Model.AcousticAlphabet, token)
                        .NormalizePublished(ownership.GlyphPool, candidate.Words, residue.Leading, residue.Trailing, null, token);
                    //Fresh configurationF4==0:21F320 returns before any glyph read/write.
                    token.ThrowIfCancellationRequested();
                    Materialization.PassResult.Storage.ValidateResultQueryEpoch(token);
                    int duplicateOf = -1;
                    for (int previous = 0; previous < active.Count; previous++)
                        if (LegacyNativeMainAlternativeEquality.Compare(ownership.GlyphPool, active[previous].Words,
                            candidate.Words, residue.Left, residue.Right, null, token))
                        { duplicateOf = previous; break; }
                    if (duplicateOf >= 0)
                    {
                        ownership.WordPublisher.ReleaseAlternativeList(); candidate.IsReleased = true;
                        attempts.Add(new(OutputIndex, exclusions.Count, candidate, true, duplicateOf));
                        //Native1415B0 decrements273.count but keeps the row and
                        //shared275/pool. It retries the SAME output index.
                        continue;
                    }
                    active.Add(candidate);
                    attempts.Add(new(OutputIndex, exclusions.Count, candidate, false, -1));
                    OutputIndex++;
                }
                Materialization.PassResult.Storage.ValidateResultQueryEpoch(token);
                FinalScores = LegacyMainResultScorePublication.Normalize(storage, active.Count, -1f, token);
                storage = FinalScores.CopyStorage(); token.ThrowIfCancellationRequested(); complete = true;
            }
            catch { faulted = true; throw; }
        }
    }

    public byte[] CopyStorage() { lock (gate) return storage.ToArray(); }
    public ulong[] CopyExcludedIdentities() { lock (gate) return exclusions.ToArray(); }
    public LegacyMainConfiguredAlternativeAttempt[] CopyAttempts() { lock (gate) return attempts.ToArray(); }
    public LegacyOwnedMainConfiguredAlternative[] CopyActiveAlternatives() { lock (gate) return active.ToArray(); }
}
