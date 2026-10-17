using InkRecognition;
using System.Buffers.Binary;
internal static class LiveControlTests
{
    internal static int CheckNodeWriteback(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalWordControllerPolicy foreignPolicy, LegacyMainDirectWordNodeResult source)
    {
        int checks = 0;
        //Prepared allocation-ownership control: retain nonzero residue outside
        //SourceEnd and throughout unused pool fields, not just zero-filled slots.
        byte[] rowResidue = source.Records.ToArray(), poolResidue = source.PrimaryAlternatives.ToArray();
        for (int i = policy.Configuration.PrimaryArcLimit230a * 88; i < rowResidue.Length; i++) rowResidue[i] = (byte)(i * 37 + 11);
        for (int i = 0; i < poolResidue.Length; i++) poolResidue[i] = (byte)(i * 19 + 7);
        var buffer = new LegacyOwnedFreshMainWordNodeBuffer(LegacyMainOwnedWordControllerBinding.FromGlobal(policy), 0, rowResidue, poolResidue, [], default);
        buffer.BeginLanguage();
        buffer.FinishWithoutLanguage();
        var baseline = source with
        {
            EndingNode = 0,
            SourceEnd23f0 = buffer.SourceEnd23f0,
            RetainedEnd23f8 = buffer.RetainedEnd23f8,
            Records = buffer.CopyRecordStorage(),
            PrimaryAlternatives = buffer.CopyPrimaryStorage()
        };
        byte[] rows = baseline.Records.ToArray(), pool = baseline.PrimaryAlternatives.ToArray();
        for (int ordinal = 0; ordinal < baseline.SourceEnd23f0; ordinal++)
        {
            rows[ordinal * 88 + 6] ^= 2;
            rows[ordinal * 88 + 0x22] ^= 0x5a;
            rows[ordinal * 88 + 0x23] ^= 0xa5;
        }
        for (int offset = 0; offset < pool.Length; offset += 40)
        {
            pool[offset + 0x1b] ^= 0x80;
            pool[offset + 0x1c] ^= 0x5a;
            pool[offset + 0x1d] ^= 0xa5;
        }
        var allowed = baseline with { Records = rows, PrimaryAlternatives = pool };
        buffer.ApplyCompactPublication(policy, allowed, default);
        Require(buffer.CopyRecordStorage().Span.SequenceEqual(rows) && buffer.CopyPrimaryStorage().Span.SequenceEqual(pool),
            "Native permitted publication fields were not transported exactly.");
        checks++;

        //Each rejected field is tested against the complete allocations. A
        //failure must leave BOTH arrays unchanged, including outsideSourceEnd.
        foreach (int field in new[] { 0, 5, 7, 16, 20, 0x1c, 0x1e, 0x20, 0x21, 0x24, 0x30, 0x50 })
        {
            byte[] invalid = rows.ToArray(); invalid[field] ^= 1;
            RejectAtomic(allowed with { Records = invalid });
        }
        byte[] badMark = rows.ToArray(); badMark[6] ^= 1;
        RejectAtomic(allowed with { Records = badMark });
        byte[] outside = rows.ToArray(); outside[baseline.SourceEnd23f0 * 88 + 0x22] ^= 1;
        RejectAtomic(allowed with { Records = outside });
        foreach (int field in new[] { 0, 4, 0x10, 0x18, 0x1a, 0x1e, 0x1f, 0x24 })
        {
            byte[] invalid = pool.ToArray(); invalid[field] ^= 1;
            RejectAtomic(allowed with { PrimaryAlternatives = invalid });
        }
        byte[] badAlternativeMark = pool.ToArray(); badAlternativeMark[0x1b] ^= 1;
        RejectAtomic(allowed with { PrimaryAlternatives = badAlternativeMark });
        RejectAtomic(allowed with { EndingNode = 1 });
        RejectAtomic(allowed with { SourceEnd23f0 = (ushort)(allowed.SourceEnd23f0 - 1) });
        RejectAtomic(allowed with { RetainedEnd23f8 = (ushort)(allowed.RetainedEnd23f8 - 1) });
        RejectAtomic(allowed with { Records = rows.AsMemory(1) });
        RejectAtomic(allowed with { PrimaryAlternatives = pool.AsMemory(1) });
        RejectAtomic(allowed, new(true));
        bool foreignRejected = false;
        try { buffer.ApplyCompactPublication(foreignPolicy, allowed, default); }
        catch (ArgumentException) { foreignRejected = true; }
        Require(foreignRejected, "A different controller with the same source was accepted.");
        checks++;
        return checks;

        void RejectAtomic(LegacyMainDirectWordNodeResult changed, CancellationToken token = default)
        {
            bool rejected = false;
            try { buffer.ApplyCompactPublication(policy, changed, token); }
            catch (Exception error) when (error is ArgumentException or InvalidDataException or OperationCanceledException)
            { rejected = true; }
            Require(rejected, "Nonpublication field mutation was accepted.");
            Require(buffer.CopyRecordStorage().Span.SequenceEqual(rows) && buffer.CopyPrimaryStorage().Span.SequenceEqual(pool),
                "Rejected publication partially changed the live allocations.");
            checks++;
        }
    }

    private static void Require(bool allowed, string message) { if (!allowed) throw new Exception(message); }
    internal static int Run(byte[] ak, Func<LegacyFreshMainGlobalWordControllerPolicy> fresh, byte[] residue,
        LegacyMainDirectStorageProvider allocator)
    {
        int guards = 0;
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
            { guards++; return; }
            throw new Exception("Expected global LIVE ownership/cancellation refusal.");
        }
        var p = fresh(); var owner = p.Owner; owner.AdvanceNodeEpoch(-1);
        var span = owner.PrepareSpan(0, 0);
        var first = owner.CaptureLiveQueryLease(span, default);
        var lease = owner.CaptureLiveQueryLease(span, default);
        Reject(() => owner.ValidateLiveQueryLease(first, default));
        owner.ValidateLiveQueryLease(lease, default);
        Reject(() => fresh().Owner.ValidateLiveQueryLease(lease, default));
        Reject(() => owner.ValidateLiveQueryLease(lease, new(true)));
        Reject(() => owner.EndLiveQueryLease(lease, new(true)));
        owner.ValidateLiveQueryLease(lease, default);
        Reject(() => owner.ReadLiveQuery(lease, 8UL << 40, new() { Capacity = 1, ContextRoot = 0 }, false, default));
        var foreign = fresh();
        ulong foreignParent = foreign.Owner.Contexts.ResolveInitial(1).Identity;
        Reject(() => owner.ReadLiveQuery(lease, 8UL << 40, new()
        {
            Capacity = 1,
            ContextRoot = owner.Contexts.ContainerIdentity,
            ParentContext = foreignParent
        }, false, default));
        var query = new LegacyPreparedDictionaryQuery { Capacity = 1, ContextRoot = owner.Contexts.ContainerIdentity };
        _ = owner.ReadLiveQuery(lease, 8UL << 40, query, false, default);
        owner.ValidateLiveQueryLease(lease, default);
        owner.EndLiveQueryLease(lease, default);
        Reject(() => owner.ValidateLiveQueryLease(lease, default));
        Reject(() => owner.EndLiveQueryLease(lease, default));
        owner.AdvanceNodeEpoch(0);
        Reject(() => owner.CaptureLiveQueryLease(span, default));
        var partial = owner.PrepareSpan(1, 0);
        if (owner.GetSpanExtent(1) > 1) Reject(() => owner.CaptureLiveQueryLease(partial, default));
        for (byte s = 1; s < owner.GetSpanExtent(1); s++) partial = owner.PrepareSpan(1, s);
        var next = owner.CaptureLiveQueryLease(partial, default);
        owner.AdvanceNodeEpoch(1);
        Reject(() => owner.ValidateLiveQueryLease(next, default));

        var external = fresh().Owner; external.AdvanceNodeEpoch(-1); var externalSpan = external.PrepareSpan(0, 0);
        var externalLease = external.CaptureLiveQueryLease(externalSpan, default);
        _ = external.Read(externalSpan, 8UL << 40, new() { Capacity = 1, ContextRoot = external.Contexts.ContainerIdentity });
        Reject(() => external.ValidateLiveQueryLease(externalLease, default));

        //Shape/resource errors and entry cancellation occur before the owning pass is consumed.
        var entry = fresh(); var entryConfidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, entry.Owner);
        Reject(() => LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(new byte[ak.Length], entry, allocator, entryConfidence, residue));
        Reject(() => LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, entry, allocator, entryConfidence, new byte[41]));
        Reject(() => LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, entry, allocator,
            LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, fresh().Owner), residue));
        var pass = LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, entry, allocator, entryConfidence, residue, 2);
        Reject(() => pass.RunOwned(new(true)));
        var successful = pass.RunOwned();
        if (successful.NextCompactPublicationNode2490 != 3) throw new Exception("Entry cancellation consumed the actual pass.");
        byte[] copied = successful.CopyIntermediateCompactStorage(); byte original = copied[0]; copied[0] ^= 255;
        if (successful.CopyIntermediateCompactStorage()[0] != original) throw new Exception("Mutable global pool escaped.");
        Reject(() => pass.RunOwned());

        //A short production run has no native intermediate gate. Its unvisited
        //2478/2488 entries must not be exposed as invented native allocator zeros.
        var shortPolicy = fresh();
        var shortConfidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, shortPolicy.Owner);
        var shortNative = LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(ak, shortPolicy, allocator,
            shortConfidence, residue).RunOwned();
        if (shortNative.IntermediatePublicationCount != 0 || shortNative.NextCompactPublicationNode2490 != 0 ||
            shortNative.CopyIntermediateNodeStarts2478().Length != 0 || shortNative.CopyIntermediatePrefixOwners2488().Length != 0)
            throw new Exception("Unvisited compact table entries were exposed as native values.");
        var shortTerminal = LegacyFreshMainGlobalTerminalInvocation.Prepare(shortPolicy).Select(shortNative);
        var shortCompact = LegacyFreshMainGlobalCompactWordMaterialization.MaterializeWithIntermediatePublication(ak,
            shortTerminal, shortConfidence);
        var defaultPolicy = fresh();
        var defaultConfidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, defaultPolicy.Owner);
        var defaultPass = new LegacyFreshMainGlobalWordPass(ak, defaultPolicy, allocator).RunOwned();
        var defaultTerminal = LegacyFreshMainGlobalTerminalInvocation.Prepare(defaultPolicy).Select(defaultPass);
        var defaultCompact = LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak, defaultTerminal,
            defaultConfidence, residue);
        if (!shortCompact.CopyCompactStorage().AsSpan().SequenceEqual(defaultCompact.CopyCompactStorage()) ||
            !shortCompact.CopyNodeStarts2478().AsSpan().SequenceEqual(defaultCompact.CopyNodeStarts2478()) ||
            shortCompact.SelectedRecordIndex != defaultCompact.SelectedRecordIndex)
            throw new Exception("The retained production reader changed old short final-only compact bytes.");

        foreach (int fault in new[] { 0, 1, 2 })
        {
            var f = fresh(); var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, f.Owner);
            using var cancel = new CancellationTokenSource(); LegacyFreshMainGlobalIntermediatePublication? observed = null;
            var failed = LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, f, allocator, confidence, residue, 2, publication =>
            {
                observed = publication;
                if (fault == 0) throw new InvalidOperationException("Prepared observer failure after numeric prefix.");
                if (fault == 1) cancel.Cancel(); else f.Owner.AdvanceNodeEpoch(2);
            });
            Reject(() => failed.RunOwned(cancel.Token));
            Reject(() => failed.RunOwned());
            if (observed is null || observed.RecordCount2470 == 0 || observed.CopyNodeStarts2478()[3] != observed.RecordCount2470)
                throw new Exception("Failure discarded the completed publication prefix or skipped callback.");
        }
        return guards;
    }
}
