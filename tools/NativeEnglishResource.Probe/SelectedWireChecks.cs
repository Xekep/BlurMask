using InkRecognition;
using System.Security.Cryptography;
using System.Text.Json;

internal static class SelectedWireChecks
{
    internal static void Run(string models, string referencePath, string outputPath)
    {
        byte[] raw = File.ReadAllBytes(Path.Combine(models, "en-US/en_US-ak-cur.res"));
        byte[] reference = File.ReadAllBytes(referencePath);
        const string packHash = "39418d35708502db6520fa22781614c30c7256efdb9981bf6bbf9b1b0c4f8d5b";
        int checks = 0;
        void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception e) when (e is ArgumentException or InvalidDataException or OperationCanceledException or NotSupportedException)
            { checks++; return; }
            throw new Exception("Required wire-layout refusal missing.");
        }
        Check(Convert.ToHexStringLower(SHA256.HashData(reference)) == packHash, "Independent reference pack differs.");
        var source = LegacySource247WireLayout.Read(raw);
        var child = source.Children.Single(c => c.Destination == "247.148");
        var wire = LegacySelectedEnglish106WireLayout.Read(source, child);
        Check(wire.BelongsTo(source, child), "Actual owner was not retained.");
        Check(wire.NetworkOffset == 1774299 && wire.NetworkEnd == 3782348 &&
            wire.PayloadOffset == 1774464 && wire.PayloadBytes == 2007776 &&
            wire.PaddingOffset == 1774455 && wire.PaddingBytes == 9 &&
            wire.TailOffset == 3782244 && wire.TailBytes == 104, "Absolute wire intervals differ.");
        Check(wire.Layers.Count == 4 && wire.Tensors.Count == 67 &&
            wire.Tensors.Count(t => t.Kind == 1) == 49 && wire.Tensors.Count(t => t.Kind == 2) == 18,
            "Tensor/layer partition differs.");
        int end = 0;
        foreach (var tensor in wire.Tensors)
        { Check(tensor.Offset == end && tensor.Bytes > 0, "Tensor ranges overlap or leave a hole."); end += tensor.Bytes; }
        Check(end == wire.PayloadBytes, "Tensor ranges do not cover the complete payload.");
        Check(wire.CopyPayload().AsSpan().SequenceEqual(raw.AsSpan(wire.PayloadOffset, wire.PayloadBytes)), "Payload bytes differ.");
        Check(wire.CopyAlignmentPadding().AsSpan().SequenceEqual(raw.AsSpan(wire.PaddingOffset, wire.PaddingBytes)), "Padding bytes differ.");
        Check(wire.CopyOpaqueTail().AsSpan().SequenceEqual(raw.AsSpan(wire.TailOffset, wire.TailBytes)), "Opaque tail bytes differ.");
        foreach (var copy in new Func<byte[]>[] { () => wire.CopyPayload(), () => wire.CopyAlignmentPadding(), () => wire.CopyOpaqueTail() })
        { var first = copy(); var expected = first.ToArray(); first.AsSpan().Fill(0); Check(copy().AsSpan().SequenceEqual(expected), "A defensive copy changed its wire owner."); }
        Reject(() => ((IList<LegacySelected106Tensor>)wire.Tensors)[0] = wire.Tensors[0] with { Offset = -1 });
        Reject(() => ((IList<LegacySelected106Layer>)wire.Layers).Clear());
        var other = LegacySource247WireLayout.Read(raw);
        var foreign = other.Children.Single(c => c.Destination == "247.148");
        Check(!wire.BelongsTo(other, foreign) && !wire.BelongsTo(source, foreign), "Equal bytes forged owner identity.");
        Reject(() => LegacySelectedEnglish106WireLayout.Read(source, foreign));
        Reject(() => LegacySelectedEnglish106WireLayout.Read(source, source.Children.First(c => !ReferenceEquals(c, child))));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        void Canceled(Action<CancellationToken> action)
        {
            try { action(canceled.Token); }
            catch (OperationCanceledException e) when (e.CancellationToken == canceled.Token) { checks++; return; }
            throw new Exception("Exact cancellation token was lost.");
        }
        Canceled(t => LegacySelectedEnglish106WireLayout.Read(source, child, t));
        Canceled(t => wire.CopyPayload(t)); Canceled(t => wire.CopyAlignmentPadding(t)); Canceled(t => wire.CopyOpaqueTail(t));
        Check(NativeEnglishTextResource.CreatePortableSnapshot(raw).AsSpan().SequenceEqual(reference), "Refactored portable writer changed the pack.");
        foreach (int at in new[] { 0, 14, 1774455, 1774464, 3782244, raw.Length - 1 })
        {
            var mutated = raw.ToArray(); mutated[at] ^= 1;
            Reject(() => { var owner = LegacySource247WireLayout.Read(mutated); LegacySelectedEnglish106WireLayout.Read(owner, owner.Children.Single(c => c.Destination == "247.148")); });
        }
        var retained = wire.CopyOpaqueTail(); raw.AsSpan().Fill(0);
        Check(wire.CopyOpaqueTail().AsSpan().SequenceEqual(retained), "Caller resource mutation changed the retained tail.");
        using var file = File.Create(outputPath); using var json = new Utf8JsonWriter(file, new() { Indented = true });
        json.WriteStartObject(); json.WriteString("scope", "Actual C# selected106 wire data/copy/owner/cancellation checks; no model inference or vendor SDK");
        json.WriteNumber("checks", checks); json.WriteString("portableSHA256", packHash);
        json.WriteNumber("tensors", wire.Tensors.Count); json.WriteNumber("payloadBytes", wire.PayloadBytes);
        json.WriteNumber("paddingBytes", wire.PaddingBytes); json.WriteNumber("tailBytes", wire.TailBytes);
        json.WriteEndObject(); Console.WriteLine($"PASS selected106 wire: {checks} checks, complete portable pack unchanged; no NN.");
    }
}
