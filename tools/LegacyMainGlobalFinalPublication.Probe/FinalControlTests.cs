using InkRecognition;
internal static class FinalControlTests
{
    internal static int Run(byte[] ak, Func<LegacyFreshMainGlobalWordControllerPolicy> fresh, byte[] residue, LegacyMainDirectStorageProvider allocator)
    {
        int guards = 0;
        void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException) { guards++; return; } throw new Exception("Expected controlled final failure."); }
        foreach (int fault in new[] { 0, 1, 2 })
        {
            var policy = fresh(); var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, policy.Owner);
            var pass = LegacyFreshMainGlobalWordPass.CreateWithPreparedPublicationAtNode(ak, policy, allocator, confidence, residue, 2).RunOwned();
            byte[] live = pass.CopyIntermediateCompactStorage(); int count = pass.IntermediateCompactRecordCount2470;
            LegacyFreshMainGlobalFinalPublication? numeric = null; using var canceled = new CancellationTokenSource();
            Reject(() => LegacyFreshMainGlobalFinalPublication.AppendWithObserver(pass, confidence, ak, value =>
            {
                numeric = value;
                if (fault == 0) throw new InvalidOperationException("Prepared postnumeric failure.");
                if (fault == 1) canceled.Cancel();
                else
                {
                    //An owned query changes the SAME slots and context state.
                    //Its lease revision advances legitimately; then block return.
                    _ = pass.ReadResultQuery(8UL << 40, new()
                    {
                        Capacity = 7,
                        ContextRoot = policy.Owner.Contexts.ContainerIdentity,
                        Parameter1c = (uint)(pass.NodeCount - 1),
                        Parameter20 = 0,
                        Flags24 = 0x100
                    }, false, default);
                    //Even an empty read advances the same owned request revision;
                    //no positive row or new permission map is manufactured.
                    throw new InvalidOperationException("Prepared exception after retained query side effects.");
                }
            }, canceled.Token));
            if (numeric is null || numeric.RecordCount2470 <= count || !numeric.CopyCompactStorage().AsSpan(0, count * 40).SequenceEqual(live.AsSpan(0, count * 40)) ||
                !live.AsSpan().SequenceEqual(pass.CopyIntermediateCompactStorage())) throw new Exception("Failed return lost an already-published LIVE prefix.");
            Reject(() => LegacyFreshMainGlobalFinalPublication.Append(pass, confidence, ak));
            if (fault == 1) Reject(() => LegacyFreshMainGlobalTerminalInvocation.Prepare(policy).SelectPublished(numeric, canceled.Token));
            else if (!LegacyFreshMainGlobalTerminalInvocation.Prepare(policy).SelectPublished(numeric).Winner.HasWinner)
                throw new Exception("Detached successful numeric copy changed despite only observer exception.");
        }
        //A final-only old pass has no retained LIVE producer/reader and must not be coerced.
        var old = fresh(); var oldConfidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, old.Owner);
        var oldPass = new LegacyFreshMainGlobalWordPass(ak, old, allocator).RunOwned();
        Reject(() => LegacyFreshMainGlobalFinalPublication.Append(oldPass, oldConfidence, ak));
        return guards;
    }
}
