namespace BlurMask.Shared;

// Preview packets are ephemeral. Only the regular changes API commits a mark.
public sealed record LiveFrame
{
    public string Type { get; init; } = "preview";
    public Guid Peer { get; init; }
    public InkMarkDto? Mark { get; init; }
    public int Offset { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public Guid MarkId { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public List<InkPointDto>? Points { get; init; }
    public long Revision { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public long BaseRevision { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public BoardPatch? Patch { get; init; }
}

public static class LiveProtocol
{
    public const int MaxPacketBytes = 64 * 1024;
    public const int ChunkPoints = 512;
    public static bool TryApply(BoardSnapshot before, LiveFrame frame, out BoardSnapshot? after)
    {
        after = null;
        if (frame.Type != "changed" || frame.BaseRevision != before.Revision || frame.Revision != before.Revision + 1 ||
            frame.Patch is not { } patch || !BoardValidation.Valid(patch)) return false;
        var removed = patch.Remove.ToHashSet();
        var updated = patch.Update.ToDictionary(m => m.Id);
        var marks = before.Marks.Where(m => !removed.Contains(m.Id)).Select(m => updated.GetValueOrDefault(m.Id) ?? m).ToList();
        var ids = marks.Select(m => m.Id).ToHashSet();
        foreach (var mark in patch.Add) if (ids.Add(mark.Id)) marks.Add(mark);
        var candidate = before with { Revision = frame.Revision, Marks = marks,
            Title = patch.Title?.Trim() ?? before.Title, Background = patch.Background ?? before.Background };
        if (!BoardValidation.Valid(candidate)) return false;
        after = candidate; return true;
    }
    public static bool TryMerge(LiveFrame frame, InkMarkDto? previous, out InkMarkDto? merged)
    {
        merged = null;
        if (frame.Type == "points")
        {
            if (previous is null || previous.Id != frame.MarkId || previous.Tool is not ("Pen" or "Highlighter") ||
                frame.Offset <= 0 || frame.Offset != previous.Points.Count || frame.Mark is not null ||
                frame.Points is not { Count: > 0 and <= ChunkPoints } delta || frame.Offset + delta.Count > 20000 ||
                !delta.All(p => p is not null && double.IsFinite(p.X) && double.IsFinite(p.Y) && Math.Abs(p.X) <= 100000 && Math.Abs(p.Y) <= 100000)) return false;
            previous.Points.AddRange(delta); merged = previous; return true;
        }
        if (frame.Type != "preview" || frame.Mark is not { } mark ||
            !BoardValidation.Valid(mark) || mark.Tool is "Image" || mark.Points.Count > ChunkPoints || frame.Offset != 0) return false;
        // The accumulator owns its list; never mutate a received/queued packet.
        merged = mark with { Points = new(mark.Points) };
        return true;
    }
    public static IEnumerable<LiveFrame> Chunks(InkMarkDto mark, int offset = 0)
    {
        for (var i = offset; i < mark.Points.Count; i += ChunkPoints)
            yield return i == 0 ? new() { Mark = mark with { Points = mark.Points.Take(ChunkPoints).ToList() } } :
                new() { Type = "points", MarkId = mark.Id, Points = mark.Points.Skip(i).Take(ChunkPoints).ToList(), Offset = i };
    }
}
