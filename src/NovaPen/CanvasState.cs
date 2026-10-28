using System.Text.Json.Serialization;
using NovaPen.Shared;

namespace NovaPen;

internal sealed record CanvasState
{
    public BoardSnapshot Board { get; init; } = new();
    public BoardSnapshot Baseline { get; init; } = new();
    public BoardConnection? Connection { get; init; }
    public BoardPatch? Pending { get; init; }
    public BoardSnapshot? PendingBefore { get; init; }
    public string? ViewKey { get; init; }
}
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CanvasState))]
internal partial class CanvasJsonContext : JsonSerializerContext;

internal static class CanvasMerge
{
    public static BoardPatch Difference(BoardSnapshot before, BoardSnapshot after)
    {
        var previous = before.Marks.ToDictionary(m => m.Id);
        var current = after.Marks.Select(m => m.Id).ToHashSet();
        var lastSurvivor = after.Marks.FindLastIndex(m => previous.ContainsKey(m.Id));
        var placements = new List<InkPlacement>();
        for (var i = 0; i < lastSurvivor; i++)
            if (!previous.ContainsKey(after.Marks[i].Id)) placements.Add(new(after.Marks[i].Id, after.Marks[i + 1].Id));
        return new()
        {
            Add = after.Marks.Where(m => !previous.ContainsKey(m.Id)).ToList(),
            Update = after.Marks.Where(m => previous.TryGetValue(m.Id, out var old) && !BoardValidation.Same(old, m)).ToList(),
            Remove = before.Marks.Where(m => !current.Contains(m.Id)).Select(m => m.Id).ToList(),
            InsertBefore = placements,
            Title = before.Title == after.Title ? null : after.Title,
            Background = before.Background == after.Background ? null : after.Background
        };
    }
    public static bool HasChanges(BoardPatch patch) => patch.Add.Count > 0 || patch.Update.Count > 0 || patch.Remove.Count > 0 || patch.InsertBefore.Count > 0 || patch.Title is not null || patch.Background is not null;
    public static BoardSnapshot Rebase(BoardSnapshot remote, BoardSnapshot before, BoardSnapshot current)
    {
        var patch = Difference(before, current);
        var removed = patch.Remove.ToHashSet();
        var marks = remote.Marks.Where(m => !removed.Contains(m.Id)).ToList();
        var updated = patch.Update.ToDictionary(m => m.Id);
        for (var i = 0; i < marks.Count; i++) if (updated.TryGetValue(marks[i].Id, out var mark)) marks[i] = mark;
        var ids = marks.Select(m => m.Id).ToHashSet();
        foreach (var mark in patch.Add) if (ids.Add(mark.Id)) marks.Add(mark);
        BoardOrder.Apply(marks, patch.InsertBefore);
        return remote with { Marks = marks, Title = patch.Title ?? remote.Title, Background = patch.Background ?? remote.Background };
    }
}
