using InkRecognition;
using System.Text.Json;

internal static class Node0IndexChecks
{
    internal static void Run(string models, string output)
    {
        int checks = 0;
        void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException)
            { checks++; return; }
            throw new Exception("Expected model-index refusal missing.");
        }
        byte[] acoustic = File.ReadAllBytes(Path.Combine(models, "en-US/en_US-ak-cur.lite.res"));
        byte[] linguistic = File.ReadAllBytes(Path.Combine(models, "en-US/en_US-lk-text.lite.res"));
        var source = LegacySource247WireLayout.Read(acoustic);
        LegacyEnglishLkFreshNumericBinding Load()
        {
            var data = LegacyEnglishLkPackedData.Read(source, linguistic);
            return LegacyEnglishLkFreshNumericBinding.Create(LegacyEnglishLkSparseLanguageData.Read(data, linguistic));
        }
        var numeric = Load(); var lexicon = numeric.DataOwner.Dictionary;
        LegacyEnglishCaseNode0Ledger Ledger() => LegacyEnglishCaseNode0Ledger.Create(LegacyEnglishCaseNode0Ledger.AggregateCeiling);
        Reject(() => LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric, Ledger(), default));
        Reject(() => LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric, maximumPayload: LegacyEnglishCaseNode0ModelIndexPreparation.PayloadReservation - 1));
        Reject(() => LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric, maximumCopies: LegacyEnglishCaseNode0ModelIndexPreparation.CopyReservation - 1));
        Reject(() => LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric, maximumVisits: LegacyEnglishCaseNode0ModelIndexPreparation.VisitReservation - 1));
        Reject(() => lexicon.RequirePreparedNode0Index(default));
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel();
            try { LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric, token: canceled.Token); }
            catch (OperationCanceledException e) when (e.CancellationToken == canceled.Token) { checks++; }
            Reject(() => lexicon.RequirePreparedNode0Index(default));
        }
        LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric);
        var prepared = lexicon.RequirePreparedNode0Index(default);
        var ledger = Ledger(); var index = LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric, ledger, default);
        Check(ReferenceEquals(index.Numeric, numeric) && ReferenceEquals(index.Lexicon, lexicon), "Warm wrapper has foreign invocation/model.");
        // Independent public graph traversal: no private packed reader or cached
        // words/bitmaps are used to generate the expected preorder IDs.
        var pending = new Stack<int>(); pending.Push(0); int visited = 0, terminals = 0, firstTerminal = -1;
        while (pending.Count != 0)
        {
            int node = pending.Pop(); visited++;
            if (lexicon.IsTerminal(node))
            {
                Check(index.ReadTerminalWordId(node) == terminals, "Cached terminal preorder differs from public graph.");
                if (firstTerminal < 0) firstTerminal = node; terminals++;
            }
            var range = lexicon.GetChildRange(node);
            for (int child = range.End - 1; child >= range.Start; child--) pending.Push(child);
        }
        Check(visited == 666547 && terminals == 260074, "Independent traversal did not cover the entire model.");
        Reject(() => index.ReadTerminalWordId(0));
        ushort[] map = Enumerable.Repeat(ushort.MaxValue, 104).ToArray(); index.RequireEmptyAuxiliary(map);
        map[lexicon.GetLabel(firstTerminal)] = 0; Reject(() => index.RequireEmptyAuxiliary(map));
        Check(ledger.Visits < LegacyEnglishCaseNode0Ledger.VisitCeiling, "Warm complete terminal reads exceeded the unchanged operation gate.");
        LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(numeric);
        Check(ReferenceEquals(prepared, lexicon.RequirePreparedNode0Index(default)), "Repeated preparation rebuilt the published index.");
        var secondNumeric = LegacyEnglishLkFreshNumericBinding.Create(numeric.LanguageOwner);
        var secondLedger = Ledger(); var second = LegacyEnglishCaseNode0LexiconIndex.Prepare(secondNumeric, secondLedger, default);
        Check(ReferenceEquals(second.Numeric, secondNumeric) && ReferenceEquals(second.Lexicon, lexicon), "Shared immutable model captured the first numeric invocation.");
        Check(secondLedger.Visits == 128, "Warm preparation hid a full dictionary traversal.");
        Parallel.For(0, 8, _ =>
        {
            var local = LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric, Ledger(), default);
            if (local.ReadTerminalWordId(firstTerminal) != 0) throw new Exception("Concurrent immutable lookup changed.");
        }); checks++;
        var foreign = Load();
        Check(!prepared.IsFor(foreign.DataOwner.Dictionary), "Equal bytes forged model-owner identity.");
        Reject(() => LegacyEnglishCaseNode0LexiconIndex.Prepare(foreign, Ledger(), default));
        using (var stop = new CancellationTokenSource())
        {
            stop.CancelAfter(1); bool canceled = false;
            try { LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(foreign, token: stop.Token); }
            catch (OperationCanceledException e) when (e.CancellationToken == stop.Token) { canceled = true; checks++; }
            if (canceled) Reject(() => foreign.DataOwner.Dictionary.RequirePreparedNode0Index(default));
            // Cancellation after the final poll may validly publish. Both
            // outcomes must leave a retryable owner and only a complete cache.
            LegacyEnglishCaseNode0ModelIndexPreparation.Prepare(foreign);
        }
        Check(!ReferenceEquals(prepared, foreign.DataOwner.Dictionary.RequirePreparedNode0Index(default)), "Distinct models shared prepared storage.");
        using var file = File.Create(output); using var json = new Utf8JsonWriter(file, new() { Indented = true });
        json.WriteStartObject(); json.WriteString("scope", "Actual C# explicit model preparation and warm index checks; no recognition, native SDK or whole-work certificate");
        json.WriteNumber("checks", checks); json.WriteNumber("nodes", visited); json.WriteNumber("terminals", terminals);
        json.WriteNumber("warmTerminalReadVisits", ledger.Visits); json.WriteNumber("warmPrepareVisits", secondLedger.Visits);
        json.WriteNumber("operationVisitCeiling", LegacyEnglishCaseNode0Ledger.VisitCeiling); json.WriteEndObject();
        Console.WriteLine($"PASS model index: {checks} checks, {terminals} terminal IDs, warm-only ledger; no NN.");
    }
}
