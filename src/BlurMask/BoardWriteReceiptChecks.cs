using System.Text.Json;
using BlurMask.Shared;

namespace BlurMask;

internal static class BoardWriteReceiptChecks
{
    internal static void Run()
    {
        var image = new InkMarkDto
        {
            Tool = "Image", Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR1cAAAAASUVORK5CYII=",
            Points = [new(0, 0), new(100, 100)]
        };
        var peer = new InkMarkDto { Points = [new(40, 40), new(60, 60)] };
        var before = new BoardSnapshot { Id = Guid.NewGuid(), Revision = 1, Marks = [image, peer] };
        var added = new InkMarkDto { Points = [new(1, 1), new(2, 2)] };
        var patch = new BoardPatch { Add = [added], Title = "Canonical" };
        var receipt = new BoardWriteReceipt
        {
            BoardId = before.Id, OperationId = patch.OperationId, BaseRevision = 1, Revision = 2,
            Title = "Canonical", Background = before.Background, Width = before.Width, Height = before.Height, Patch = patch
        };
        var wire = JsonSerializer.Serialize(receipt, BoardWriteReceiptJsonContext.Default.BoardWriteReceipt);
        var read = JsonSerializer.Deserialize(wire, BoardWriteReceiptJsonContext.Default.BoardWriteReceipt)!;
        Check(read.TryApply(before, patch.OperationId, out var after) && after!.Marks.Count == 3 && after.Title == "Canonical" &&
            before.Marks.Count == 2 && before.Revision == 1 && before.Title != "Canonical" &&
            ReferenceEquals(before.Marks[0], after.Marks[0]) && ReferenceEquals(before.Marks[1], after.Marks[1]),
            "Receipt reconstruction mutated its baseline, dropped peer ink or copied an unchanged PNG.");
        Check(!wire.Contains(image.Image!, StringComparison.Ordinal), "Unchanged PNG was serialized into the write receipt.");
        var noChange = receipt with { Revision = 1, Title = before.Title, Patch = null };
        Check(noChange.TryApply(before, patch.OperationId, out var unchanged) && ReferenceEquals(before, unchanged), "No-change receipt changed the snapshot.");
        var moved = peer with { Points = [new(90, 90), new(110, 110)] };
        var move = new BoardPatch { Update = [moved], Remove = [image.Id] };
        Check((receipt with { OperationId = move.OperationId, Title = before.Title, Patch = move }).TryApply(before, move.OperationId, out var transformed) &&
            transformed!.Marks.Count == 1 && BoardValidation.Same(transformed.Marks[0], moved) && before.Marks.Count == 2,
            "Canonical update/removal receipt reconstructed the wrong layers.");
        foreach (var bad in new[]
        {
            receipt with { BoardId = Guid.NewGuid() }, receipt with { BoardId = Guid.Empty }, receipt with { OperationId = Guid.NewGuid() },
            receipt with { BaseRevision = 0 }, receipt with { BaseRevision = -1 }, receipt with { Revision = 3 },
            receipt with { Title = "wrong" }, receipt with { Width = 123 }, receipt with { Background = "black" }, receipt with { Patch = null },
            receipt with { Patch = patch with { OperationId = Guid.NewGuid() } }, receipt with { Patch = patch with { Add = null! } },
            noChange with { Revision = 2 }
        }) Check(!bad.TryApply(before, patch.OperationId, out _), "An inconsistent write receipt was accepted.");
        Check(!receipt.TryApply(before with { ReadOnly = true }, patch.OperationId, out _) &&
            !receipt.TryApply(before with { Marks = null! }, patch.OperationId, out _) &&
            !receipt.TryApply(before with { Marks = [null!] }, patch.OperationId, out _) &&
            !noChange.TryApply(before, Guid.Empty, out _), "Invalid/read-only receipt baseline was accepted.");
        // A lost reply or another peer's revision must trigger a full read, never
        // pretend an old delta is current and silently discard the peer's ink.
        Check(!receipt.TryApply(before with { Revision = 2 }, patch.OperationId, out _) &&
            !noChange.TryApply(before with { Revision = 3, Marks = [image, peer, added] }, patch.OperationId, out _),
            "Stale/lost-reply acknowledgement overwrote a newer baseline.");
        var deleted = before with { Marks = [peer] };
        var restored = before with { Marks = [added, image, peer] };
        var restore = CanvasMerge.Difference(deleted, restored);
        Check(restore.InsertBefore.Count == 2 && BoardValidation.Valid(restore), "Restore did not generate sparse layer anchors.");
        var encodedRestore = LiveWire.EncodeChange(new() { Type = "changed", BaseRevision = 1, Revision = 2, Patch = restore });
        Check(LiveProtocol.TryApply(deleted, LiveWire.Decode(encodedRestore), out var ordered) &&
            ordered!.Marks.Select(m => m.Id).SequenceEqual(restored.Marks.Select(m => m.Id)) && ReferenceEquals(ordered.Marks[2], peer),
            "Binary restored group changed layer order or copied the surviving peer.");
        var restoreReceipt = receipt with { Title = before.Title, OperationId = restore.OperationId, Patch = restore };
        var restoredWire = JsonSerializer.Serialize(restoreReceipt, BoardWriteReceiptJsonContext.Default.BoardWriteReceipt);
        Check(JsonSerializer.Deserialize(restoredWire, BoardWriteReceiptJsonContext.Default.BoardWriteReceipt)!.TryApply(deleted, restore.OperationId, out ordered) &&
            ordered!.Marks.Select(m => m.Id).SequenceEqual(restored.Marks.Select(m => m.Id)), "Receipt dropped restored layer positions.");
        var concurrent = new InkMarkDto { Points = [new(100, 100), new(120, 120)] };
        var rebased = CanvasMerge.Rebase(deleted with { Marks = [peer, concurrent] }, deleted, restored);
        Check(rebased.Marks.Select(m => m.Id).SequenceEqual(new[] { added.Id, image.Id, peer.Id, concurrent.Id }), "Restoring a group lost or reordered concurrent peer ink.");
        Check(CanvasMerge.Difference(before, before with { Marks = [image, peer, added] }).InsertBefore.Count == 0, "Ordinary append sends unnecessary layer anchors.");
        foreach (var placement in new[] { new InkPlacement(added.Id, added.Id), new InkPlacement(Guid.Empty, peer.Id), new InkPlacement(peer.Id, image.Id) })
            Check(!BoardValidation.Valid(restore with { InsertBefore = [placement] }), "Invalid restored layer anchor accepted.");
        Console.WriteLine("Write receipt: generated JSON, canonical changes, unchanged PNG references, sparse restored layer order, binary/rebase paths, stale replies and guards passed.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
