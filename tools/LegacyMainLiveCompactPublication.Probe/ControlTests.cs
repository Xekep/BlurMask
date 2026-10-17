using InkRecognition;

internal static class ControlTests
{
    internal static int CheckNodeWriteback(LegacyOwnedFreshMainWordControllerPolicy policy,
        LegacyOwnedFreshMainWordControllerPolicy foreignPolicy, LegacyMainDirectWordNodeResult source)
    {
        int checks = 0;
        var buffer = new LegacyOwnedFreshMainWordNodeBuffer(policy, 0, source.Records.Span, source.PrimaryAlternatives.Span);
        buffer.BeginLanguage();
        buffer.FinishWithoutLanguage();
        var baseline = source with
        {
            EndingNode = 0, SourceEnd23f0 = buffer.SourceEnd23f0, RetainedEnd23f8 = buffer.RetainedEnd23f8,
            Records = buffer.CopyRecordStorage(), PrimaryAlternatives = buffer.CopyPrimaryStorage()
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

    internal static int CheckObserverFailures(byte[] ak, byte[] lk, LegacyRussianModelBundle model,
        MainLineWordFixture input, LegacyNativeCaseData cases, LegacyNativeTextPublicationData publication)
    {
        int checks = 0;
        foreach (int behavior in new[] { 0, 1, 2 })
        {
            var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, publication);
            var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, input.Table);
            var controller = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
            var policy = LegacyFreshCaseMainSourcePolicy.Prepare(ak, lk, controller);
            var confidence = LegacyOwnedRussianMainGlyphConfidence.Prepare(ak, dictionary, input.Context);
            ulong identity = 0x1000;
            var scheduler = new LegacyOwnedMainDirectWordPass(ak, controller, owner.MainLineAcoustic, input.Table, request =>
            {
                byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
                return new(bytes, ++identity);
            }, policy, default);
            byte[] residue = Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray();
            using var cancellation = new CancellationTokenSource();
            bool invoked = false, rejected = false;
            try
            {
                scheduler.RunOwnedWithPreparedPublicationAtNode(ak, confidence, residue, 2, _ =>
                {
                    invoked = true;
                    if (behavior == 0) throw new InvalidOperationException("Observer failure sentinel.");
                    if (behavior == 1) cancellation.Cancel();
                    if (behavior == 2) owner.AdvanceNodeEpoch(2); //External V80 invalidates the LIVE source.
                }, cancellation.Token);
            }
            catch (Exception error) when (error is InvalidOperationException or OperationCanceledException)
            { rejected = true; }
            Require(invoked && rejected, "Observer failure, cancellation or external V80 escaped publication validation.");
            checks++;
            rejected = false;
            try { scheduler.Run(); }
            catch (InvalidOperationException) { rejected = true; }
            Require(rejected, "A failed publication pass was reusable.");
            checks++;
        }
        return checks;
    }

    private static void Require(bool accepted, string message)
    { if (!accepted) throw new InvalidDataException(message); }
}
